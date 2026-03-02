import os
import json
import torch
import argparse
from PIL import Image
from tqdm import tqdm
import numpy as np
import matplotlib.pyplot as plt

# 导入你的 config 和 utils
import config
import utils

import sys

import sys
import os

# 1. 打印一下当前在哪里，以及 Python 能看到哪些路径
print("当前工作目录:", os.getcwd())
# print("Python搜索路径:", sys.path) # 路径太多先注释掉，需要再开

# 2. 暴力添加路径（再次确保万无一失）
current_dir = os.path.dirname(os.path.abspath(__file__))
sys.path.append(current_dir)

# 3. 【关键】直接导入，不要 try-except！
print("正在尝试导入 sam3...")
from sam3.model_builder import build_sam3_image_model
from sam3.model.sam3_image_processor import Sam3Processor
print("✅ 导入成功！")


def save_mask_as_png(mask_array, save_path):
    """把 True/False 的矩阵保存为黑白 PNG 图片"""
    # True -> 255 (白), False -> 0 (黑)
    img = Image.fromarray((mask_array * 255).astype(np.uint8))
    # 使用 1位像素模式压缩体积
    img = img.convert("1")
    img.save(save_path)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", type=str, required=True, help="数据集名称")
    parser.add_argument("--limit", type=int, default=0, help="测试用：只跑前N张")
    args = parser.parse_args()

    dataset_name = args.dataset
    if dataset_name not in config.DATASETS:
        print(f"❌ 数据集 {dataset_name} 未在 config.py 定义")
        return

    img_root = config.DATASETS[dataset_name]

    # === 1. 初始化输出路径 ===
    # 结果 JSONL
    os.makedirs(config.OUTPUT_DIR, exist_ok=True)
    json_path = os.path.join(config.OUTPUT_DIR, f"{dataset_name}_sam3.jsonl")

    # Mask 图片保存目录
    mask_dir = os.path.join(config.OUTPUT_DIR, f"{dataset_name}_masks")
    os.makedirs(mask_dir, exist_ok=True)

    # === 2. 加载模型 (只加载一次！) ===
    print(f"🔄 正在加载 SAM3 模型: {config.CHECKPOINT_PATH} ...")
    try:
        model = build_sam3_image_model(checkpoint_path=config.CHECKPOINT_PATH)
        # 将模型转为 float16 或 bfloat16 以节省 4060 的显存
        model = model.cuda().to(dtype=torch.bfloat16)
        processor = Sam3Processor(model)
        print("✅ 模型加载成功！")
    except Exception as e:
        print(f"❌ 模型加载失败: {e}")
        print("请检查 config.py 里的路径是否正确，以及显存是否足够。")
        return

    # === 3. 扫描文件 ===
    all_images = utils.get_all_images(img_root)

    # 断点续传逻辑
    processed = set()
    if os.path.exists(json_path):
        with open(json_path, 'r', encoding='utf-8') as f:
            for line in f:
                try:
                    processed.add(json.loads(line)['image_path'])
                except:
                    pass

    todo_images = [img for img in all_images if img not in processed]

    # limit 逻辑
    if args.limit > 0:
        todo_images = todo_images[:args.limit]

    print(f"🚀 开始处理 {len(todo_images)} 张图片 (Total: {len(all_images)})")

    # === 4. 循环推理 ===
    # 打开文件句柄准备写入
    with open(json_path, 'a', encoding='utf-8') as f_out:

        # 开启 inference_mode 节省显存
        with torch.inference_mode():
            for image_path in tqdm(todo_images, desc="Running SAM3"):
                try:
                    # A. 加载图片
                    image_pil = Image.open(image_path).convert("RGB")
                    w, h = image_pil.size

                    # B. SAM3 推理
                    inference_state = processor.set_image(image_pil)

                    # 提示词：找 "person"
                    output = processor.set_text_prompt(
                        state=inference_state,
                        prompt=config.TEXT_PROMPT
                    )

                    # C. 筛选主要人物 (Utils 里的逻辑)
                    result = utils.select_main_person(
                        output["masks"],
                        output["boxes"],
                        output["scores"],
                        w, h
                    )

                    if result is None:
                        # 没找到人，记录空结果
                        record = {
                            "image_path": image_path,
                            "status": "no_person_detected",
                            "bbox": None,
                            "mask_path": None
                        }
                    else:
                        # 找到了，保存 Mask 图片
                        file_name = os.path.splitext(os.path.basename(image_path))[0]
                        mask_filename = f"{file_name}_mask.png"
                        save_mask_path = os.path.join(mask_dir, mask_filename)

                        save_mask_as_png(result['mask'], save_mask_path)

                        record = {
                            "image_path": image_path,
                            "status": "success",
                            "bbox": result['bbox'],  # [x1, y1, x2, y2]
                            "score": result['score'],
                            # 存相对路径，方便迁移
                            "mask_path": os.path.join(f"{dataset_name}_masks", mask_filename),
                            "width": w,
                            "height": h
                        }

                    # D. 写入结果
                    f_out.write(json.dumps(record, ensure_ascii=False) + "\n")
                    f_out.flush()

                except Exception as e:
                    print(f"⚠️ 处理失败 {os.path.basename(image_path)}: {e}")
                    # 可以在这里加个 continue，防止程序直接崩

    print(f"\n✅ 全部完成！结果保存在: {json_path}")
    print(f"✅ Mask 图片保存在: {mask_dir}")


if __name__ == "__main__":
    main()
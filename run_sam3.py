import os
import json
import argparse
from tqdm import tqdm
from PIL import Image

# 导入配置和工具库
import config
import utils


def main():
    # === 1. 命令行参数设置 ===
    parser = argparse.ArgumentParser(description="Qwen3-VL 目标检测脚本")

    # 必选参数：数据集名称
    parser.add_argument("--dataset", type=str, required=True,
                        help="config.py 里定义的数据集名字，例如 dataset_A")

    # 【新增参数】限制处理数量
    parser.add_argument("--limit", type=int, default=0,
                        help="本次只处理 N 张图片。如果不填或填0，则处理所有图片。")

    args = parser.parse_args()

    # === 2. 检查数据集配置 ===
    dataset_name = args.dataset
    if dataset_name not in config.DATASETS:
        print(f"❌ 错误：在 config.py 里找不到名为 '{dataset_name}' 的数据集！")
        print(f"可用数据集: {list(config.DATASETS.keys())}")
        return

    root_path = config.DATASETS[dataset_name]
    print(f"=== 开始任务: {dataset_name} ===")
    print(f"=== 数据路径: {root_path} ===")

    # === 3. 准备输出文件 ===
    os.makedirs(config.OUTPUT_DIR, exist_ok=True)
    save_file = os.path.join(config.OUTPUT_DIR, f"{dataset_name}_bboxes.jsonl")

    # === 4. 断点续传逻辑 ===
    processed_files = set()
    if os.path.exists(save_file):
        print("📂 发现历史记录，正在读取已完成的图片...")
        with open(save_file, 'r', encoding='utf-8') as f:
            for line in f:
                try:
                    # 只要不报错，就把图片路径加入“已完成”名单
                    data = json.loads(line)
                    processed_files.add(data['image_path'])
                except:
                    pass
        print(f"✅ 已跳过 {len(processed_files)} 张历史图片")

    # === 5. 扫描并筛选任务 ===
    print("🔍 正在扫描所有图片文件...")
    all_images = utils.get_all_images(root_path)

    # 找出还没处理的图片
    todo_images = [img for img in all_images if img not in processed_files]

    total_found = len(all_images)
    remaining = len(todo_images)
    print(f"📊 统计：总计 {total_found} 张，剩余 {remaining} 张待处理")

    if remaining == 0:
        print("🎉 所有图片都已处理完毕！无需运行。")
        return

    # === 6. 【核心修改】应用 limit 限制 ===
    # 如果用户指定了 limit，并且 limit 小于剩余的任务数，就进行切片
    if args.limit > 0:
        if args.limit < remaining:
            print(f"⚠️  【测试模式】根据设置，本次只处理前 {args.limit} 张图片")
            todo_images = todo_images[:args.limit]
        else:
            print(f"ℹ️  设置的 limit ({args.limit}) 大于剩余图片数，将处理剩余的所有图片。")

    # === 7. 开始循环处理 ===
    print(f"🚀 开始处理这 {len(todo_images)} 张图片...")

    # 使用 'a' (append) 模式追加写入
    with open(save_file, 'a', encoding='utf-8') as f_out:
        for image_path in tqdm(todo_images, desc="Detecting"):

            # 步骤 A: 获取图片尺寸 (用于坐标转换)
            try:
                with Image.open(image_path) as img:
                    w, h = img.size
            except Exception as e:
                print(f"⚠️  无法打开图片 {os.path.basename(image_path)}: {e}")
                continue

            # 步骤 B: 调 API (Qwen-VL)
            # 注意：这里调用的是 utils.py，请确保你的 utils.py 里的 Payload 已经修好了
            raw_text = utils.call_qwen_api(
                image_path,
                config.PROMPT,
                config.API_URL,
                config.API_TIMEOUT
            )

            # 步骤 C: 解析坐标
            bbox = utils.parse_and_select_box(raw_text, w, h)

            # 步骤 D: 构造结果
            result = {
                "image_path": image_path,
                "bbox": bbox,  # 格式 [x1, y1, x2, y2]
                "raw_text": raw_text,  # 原始文本
                "width": w,
                "height": h
            }

            # 步骤 E: 写入文件 (处理一张存一张)
            f_out.write(json.dumps(result, ensure_ascii=False) + "\n")
            f_out.flush()

    print("\n" + "=" * 30)
    print("✅ 任务完成！")
    print(f"📁 结果已保存至: {save_file}")
    if args.limit > 0:
        print(f"💡 提示：这只是部分数据 (Limit={args.limit})。再次运行该命令将继续处理剩下的图片。")
    print("=" * 30)


if __name__ == "__main__":
    main()
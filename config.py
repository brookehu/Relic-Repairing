# config.py
import os

# === 模型路径 ===
# 请确保这里指向你真实的 .pt 文件路径
CHECKPOINT_PATH = "/model/sam3.pt"

# === 数据集设置 ===
DATASETS = {
    "ICFG-PEDES": "/dataset/ICFG-PEDES",
    "RSTPReid": "/dataset/RSTPReid",
    "UFine6926": "dataset/UFine6926"
}

# === SAM3 参数 ===
# 核心提示词：决定了模型找什么。
# "person" 通常通用，如果不行可以试 "full body person" 或 "human"
TEXT_PROMPT = "person"

# 阈值：只有得分高于这个值的 Mask 才会被保留
SCORE_THRESHOLD = 0.4

# === 输出设置 ===
OUTPUT_DIR = "output_sam3_results"
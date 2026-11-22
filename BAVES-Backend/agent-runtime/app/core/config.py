import os
from pathlib import Path
from dotenv import load_dotenv

# Thư mục gốc của project
BASE_DIR = Path(__file__).resolve().parent.parent
AGENTS_DIR = BASE_DIR / "agents"

# Tải cấu hình từ .env
load_dotenv(BASE_DIR.parent / ".env")
load_dotenv(BASE_DIR / ".env")

# Internal token bảo mật giữa Backend A và AI Runtime
INTERNAL_TOKEN = os.getenv("BAVES_INTERNAL_TOKEN", os.getenv("AI_RUNTIME_TOKEN", "baves-internal-secret-token"))


"""
clean_benchmark_markdown.py — Remove all Benchmark-related content from markdown files
"""
import re
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent.parent
TAILIEU_DIR = ROOT_DIR / "tailieu"

FILES_TO_CLEAN = [
    TAILIEU_DIR / "BAO_CAO.md",
    TAILIEU_DIR / "parts" / "01-mo-dau-p1-p2.md",
    TAILIEU_DIR / "parts" / "02-phan3.md",
    TAILIEU_DIR / "parts" / "03-phan4.md",
    TAILIEU_DIR / "parts" / "04-phan5-p6.md",
    TAILIEU_DIR / "parts" / "05-phan7-pl.md",
]

def clean_file(path: Path):
    if not path.exists():
        return
    text = path.read_text(encoding="utf-8")

    # Table 2: So sánh hệ thống
    text = re.sub(r'\|\s*So sánh đo thật với lý thuyết\s*\|.*?\n', '', text)
    text = re.sub(r'\(5\)\s*Benchmark so sánh số liệu đo thật với lý thuyết;\s*', '', text)

    # Table 3: Phân loại mô phỏng
    text = re.sub(r'Danh mục mô phỏng, hiển thị 3 vùng đồng bộ, điều khiển, Benchmark Lab', 'Danh mục mô phỏng, hiển thị 3 vùng đồng bộ, điều khiển giải thuật', text)
    text = re.sub(r'FR-3\.1 đến FR-3\.20b', 'FR-3.1 đến FR-3.19', text)

    # Table 4: Sprint S9
    text = re.sub(r'\|\s*S9\s*\|\s*07/07→13/07\s*\|\s*Premium, Lớp học phần, Benchmark\s*\|\s*Gems Shop, Premium mô phỏng, lớp học phần, Benchmark Lab\s*\|',
                  r'| S9 | 07/07→13/07 | Premium, Lớp học phần | Gems Shop, Premium mô phỏng, lớp học phần |', text)
    text = re.sub(r'nhóm Premium \+ Class \+ Benchmark \(S9\)', 'nhóm Premium + Class (S9)', text)

    # Use Case UC-28
    text = re.sub(r'\s*AB\[UC-28 Chạy Benchmark Lab\]', '', text)
    text = re.sub(r'\|\s*UC-28\s*\|\s*Chạy Benchmark Lab\s*\|.*?\n', '', text)

    # Functional Requirements FR-3.20, FR-3.20b
    text = re.sub(r'\|\s*FR-3\.20\s*\|\s*Benchmark Lab\s*\|.*?\n', '', text)
    text = re.sub(r'\|\s*FR-3\.20b\s*\|\s*Benchmark đa kích thước\s*\|.*?\n', '', text)

    # Site map & screen catalog
    text = re.sub(r'\s*NODE --> BENCH\[/benchmark/:k1/:k2/\]', '', text)
    text = re.sub(r'\|\s*Mô phỏng\s*\|\s*05 Simulator, 17 Benchmark Lab, 33 Khám phá\s*\|\s*3\s*\|',
                  r'| Mô phỏng | 05 Simulator, 33 Khám phá | 2 |', text)

    # Screen 17 / Hình 4.10 Benchmark
    text = re.sub(r'#### Màn 17 — Benchmark Lab\s*\n\s*!\[Hình 4\.10 - Benchmark Lab\].*?\n\s*\*Hình 4\.10:.*?\*\s*\n', '', text)
    text = re.sub(r'!\[Hình 4\.10 - Benchmark Lab\].*?\n\s*\*Hình 4\.10:.*?\*\s*\n', '', text)

    # Hearts deduction exception
    text = re.sub(r', trừ Benchmark Lab và node đã pass', ', trừ node đã pass', text)

    path.write_text(text, encoding="utf-8")
    print(f"[Cleaned] {path.name}")

def main():
    for f in FILES_TO_CLEAN:
        clean_file(f)

if __name__ == "__main__":
    main()

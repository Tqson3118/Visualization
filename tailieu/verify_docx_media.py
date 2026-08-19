"""
verify_docx_media.py — Detailed classification and verification of embedded media in BaoCaoDoAn.docx
"""

import os
import sys
import re
import zipfile
from pathlib import Path
from docx import Document
from lxml import etree

if hasattr(sys.stdout, "reconfigure"):
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass

ROOT_DIR = Path(__file__).resolve().parent.parent
DOCX_PATH = ROOT_DIR / "tailieu" / "BaoCaoDoAn.docx"
SCREENSHOTS_DIR = ROOT_DIR / "tailieu" / "screenshots"

# 22 UI Target Screenshots in Chapter 4 (Skipping Hình 4.10 Benchmark)
EXPECTED_UI_SCREENSHOTS = [
    ("Hình 4.1:", "01_landing.png"),
    ("Hình 4.2:", "02_login.png"),
    ("Hình 4.3:", "14_lesson_detail.png"),
    ("Hình 4.4:", "09_mo_phong_detail.png"),
    ("Hình 4.5:", "15_exercise.png"),
    ("Hình 4.6:", "05_lo_trinh.png"),
    ("Hình 4.7:", "16_ladder.png"),
    ("Hình 4.8:", "17_lab.png"),
    ("Hình 4.9:", "18_code_runner.png"),
    ("Hình 4.11:", "12_bang_xep_hang.png"),
    ("Hình 4.12:", "13_ho_so.png"),
    ("Hình 4.14:", "03_register.png"),
    ("Hình 4.15:", "04_dashboard.png"),
    ("Hình 4.16:", "06_lo_trinh_detail.png"),
    ("Hình 4.17:", "07_node_hub.png"),
    ("Hình 4.18:", "08_mo_phong.png"),
    ("Hình 4.19:", "10_lop_hoc.png"),
    ("Hình 4.20:", "11_lop_hoc_detail.png"),
    ("Hình 4.21:", "20_admin_dashboard.png"),
    ("Hình 4.22:", "21_admin_users.png"),
    ("Hình 4.23:", "22_admin_content.png"),
    ("Hình 4.24:", "23_admin_settings.png"),
]

def verify():
    print(f"=== VERIFYING DOCX MEDIA AND SCREENSHOTS: {DOCX_PATH.name} ===")
    assert DOCX_PATH.exists(), f"File {DOCX_PATH} does not exist!"

    doc = Document(str(DOCX_PATH))

    # 1. Inspect Zip package media files
    with zipfile.ZipFile(DOCX_PATH) as z:
        media_files = [f for f in z.namelist() if f.startswith('word/media/')]
        total_media_in_zip = len(media_files)

    # 2. Inspect document.xml drawing elements
    paragraphs = doc.paragraphs
    drawing_paras = []
    caption_map = {}

    for i, p in enumerate(paragraphs):
        txt = p.text.strip()
        if txt.startswith("Hình "):
            caption_map[i] = txt
        if len(p._element.xpath('.//w:drawing')) > 0:
            drawing_paras.append((i, p))

    # 3. Check Figure 4.10 is NOT in captions
    bad_410 = [c for c in caption_map.values() if "Hình 4.10" in c or "Hình 4.10:" in c]
    print(f"1. Hình 4.10 check: {'NONE (PASS)' if not bad_410 else f'FOUND: {bad_410}'}")
    assert len(bad_410) == 0, f"Found unexpected Hình 4.10 caption: {bad_410}"

    # 4. Verify Chapter 4 UI Screenshots embedded
    matched_screenshots = []
    missing_screenshots = []

    for cap_prefix, filename in EXPECTED_UI_SCREENSHOTS:
        found_cap = False
        for p_idx, cap_text in caption_map.items():
            if cap_text.startswith(cap_prefix):
                found_cap = True
                # Check previous paragraph has image
                prev_p = paragraphs[p_idx - 1]
                has_drawing = len(prev_p._element.xpath('.//w:drawing')) > 0
                img_path = SCREENSHOTS_DIR / filename
                file_exists = img_path.exists()
                file_size_kb = (img_path.stat().st_size / 1024) if file_exists else 0
                
                matched_screenshots.append({
                    "caption": cap_prefix,
                    "filename": filename,
                    "paragraph": p_idx,
                    "has_drawing": has_drawing,
                    "file_exists": file_exists,
                    "file_size_kb": file_size_kb,
                    "pass": has_drawing and file_exists and file_size_kb >= 20
                })
                break
        if not found_cap:
            missing_screenshots.append((cap_prefix, filename))

    print(f"\n2. Chapter 4 Target UI Screenshots: {len(matched_screenshots)}/22 verified")
    assert len(matched_screenshots) == 22, f"Expected 22 matched screenshots, got {len(matched_screenshots)}"
    assert len(missing_screenshots) == 0, f"Missing captions: {missing_screenshots}"

    all_ui_pass = all(s["pass"] for s in matched_screenshots)
    for s in matched_screenshots:
        print(f"   [{'OK' if s['pass'] else 'FAIL'}] {s['caption']:<12} -> {s['filename']:<24} ({s['file_size_kb']:.1f} KB, Drawing: {s['has_drawing']})")
    assert all_ui_pass, "Some UI screenshots failed verification"

    # 5. Classify all captions in document
    all_captions = list(caption_map.values())
    ch1_3_diagrams = [c for c in all_captions if any(c.startswith(f"Hình {ch}.") for ch in [1, 2, 3])]
    ch4_ui_captions = [c for c in all_captions if re.match(r"^Hình 4\.(?:[1-9]|1[1-9]|2[0-4]):", c)]
    ch4_db_captions = [c for c in all_captions if c.startswith("Hình 4.3.2.")]
    ch4_erd_captions = [c for c in all_captions if "4.25" in c or "4.13" in c]

    print("\n3. Media & Figure Classification Summary:")
    print(f"   - Total media files in zip package: {total_media_in_zip}")
    print(f"   - Total figure captions in document: {len(all_captions)}")
    print(f"   - Chapter 4 UI Screenshots (Real Data 1440x900): {len(ch4_ui_captions)}")
    print(f"   - Chapter 4 Architecture/SiteMap/ERD Diagrams: {len(ch4_erd_captions)}")
    print(f"   - Chapter 4 Database Entity Detailed Diagrams: {len(ch4_db_captions)}")
    print(f"   - Chapter 1-3 Architecture & Use Case Diagrams: {len(ch1_3_diagrams)}")

    print("\n>>> DOCX MEDIA & SCREENSHOTS AUDIT: 100% PASS <<<")

if __name__ == "__main__":
    verify()

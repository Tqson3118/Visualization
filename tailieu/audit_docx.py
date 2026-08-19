"""
audit_docx.py — Deep XML audit of BaoCaoDoAn.docx
"""
import io
import json
import re
import sys
import zipfile
from pathlib import Path
from lxml import etree

if hasattr(sys.stdout, "reconfigure"):
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass

DOCX_PATH = Path(r"D:\FPT\neww\tailieu\BaoCaoDoAn.docx")
NS = {
    "w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main",
    "r": "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
    "a": "http://schemas.openxmlformats.org/drawingml/2006/main",
    "pic": "http://schemas.openxmlformats.org/drawingml/2006/picture",
}

print(f"=== AUDITING DOCX XML: {DOCX_PATH} ===")
if not DOCX_PATH.exists():
    print(f"ERROR: {DOCX_PATH} does not exist!")
    sys.exit(1)

with zipfile.ZipFile(DOCX_PATH, "r") as z:
    file_list = z.namelist()
    print(f"ZIP contents: {len(file_list)} files found.")

    doc_xml_bytes = z.read("word/document.xml")
    doc_tree = etree.fromstring(doc_xml_bytes)

    styles_xml_bytes = z.read("word/styles.xml") if "word/styles.xml" in file_list else None
    styles_tree = etree.fromstring(styles_xml_bytes) if styles_xml_bytes else None

    # --- 1. TOC & Bookmarks Audit ---
    print("\n--- 1. TOC & Bookmarks Audit ---")
    doc_text = etree.tostring(doc_tree, encoding="utf-8", method="text").decode("utf-8")
    toc_errors = re.findall(r"Error!\s*Bookmark\s*not\s*defined", doc_text, re.IGNORECASE)
    print(f"TOC 'Error! Bookmark not defined' count: {len(toc_errors)}")
    if toc_errors:
        print("  WARNING: Found bookmark errors in document text!")
    else:
        print("  PASS: 0 bookmark errors found.")

    # --- 2. Typography & Fonts Audit ---
    print("\n--- 2. Typography & Fonts Audit ---")
    rfonts_list = doc_tree.xpath("//w:rFonts", namespaces=NS)
    non_tnr_fonts = set()
    for rf in rfonts_list:
        for attr in ["{http://schemas.openxmlformats.org/wordprocessingml/2006/main}ascii",
                     "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}hAnsi",
                     "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}cs"]:
            val = rf.get(attr)
            if val and val != "Times New Roman":
                non_tnr_fonts.add(val)
    print(f"Non-TNR font families in document.xml: {non_tnr_fonts}")
    if not non_tnr_fonts:
        print("  PASS: 100% runs in document.xml explicitly specify Times New Roman.")
    else:
        print(f"  INFO: Non-TNR fonts present: {non_tnr_fonts}")

    # --- 3. Header & Footer Audit ---
    print("\n--- 3. Header & Footer Audit ---")
    header_files = [f for f in file_list if f.startswith("word/header")]
    footer_files = [f for f in file_list if f.startswith("word/footer")]
    print(f"Header files ({len(header_files)}): {header_files}")
    print(f"Footer files ({len(footer_files)}): {footer_files}")

    placeholder_hits = []
    has_image_in_header = False
    for hf in header_files:
        h_bytes = z.read(hf)
        h_tree = etree.fromstring(h_bytes)
        h_text = etree.tostring(h_tree, encoding="utf-8", method="text").decode("utf-8")
        if "TÊN ĐỀ TÀI" in h_text:
            placeholder_hits.append(f"{hf}: 'TÊN ĐỀ TÀI'")
        if h_tree.xpath("//w:drawing", namespaces=NS) or h_tree.xpath("//a:blip", namespaces=NS) or b"image" in h_bytes or b"graphic" in h_bytes:
            has_image_in_header = True
            print(f"  Found image drawing element in header: {hf}")
        print(f"  Header text ({hf}): {h_text.strip()[:100]}...")

    if placeholder_hits:
        print(f"  WARNING: Placeholder found: {placeholder_hits}")
    else:
        print("  PASS: 0 'TÊN ĐỀ TÀI' placeholder occurrences in headers.")
    print(f"  Header Logo embedded: {has_image_in_header}")

    # --- 4. Bilingual Titles Audit ---
    print("\n--- 4. Bilingual Titles Audit ---")
    bilingual_patterns = [r"KHẢO SÁT\s*[-–]\s*SURVEY", r"PHÂN TÍCH\s*[-–]\s*ANALYSIS",
                          r"THIẾT KẾ\s*[-–]\s*DESIGN", r"THỰC HIỆN\s*[-–]\s*IMPLEMENT",
                          r"KIỂM THỬ\s*[-–]\s*TESTING"]
    bilingual_found = []
    for pat in bilingual_patterns:
        matches = re.findall(pat, doc_text, re.IGNORECASE)
        if matches:
            bilingual_found.extend(matches)
    print(f"Bilingual title matches found: {len(bilingual_found)}")
    if not bilingual_found:
        print("  PASS: No bilingual English-Vietnamese chapter titles found.")
    else:
        print(f"  WARNING: Bilingual titles found: {bilingual_found}")

    # --- 5. Tables & Test Cases Table Audit ---
    print("\n--- 5. Tables & Test Cases Table Audit ---")
    tables = doc_tree.xpath("//w:tbl", namespaces=NS)
    print(f"Total tables found in document: {len(tables)}")

    tc_table = None
    tc_table_idx = -1
    for idx, tbl in enumerate(tables):
        tbl_text = etree.tostring(tbl, encoding="utf-8", method="text").decode("utf-8")
        if "TC-01" in tbl_text and ("TC-130" in tbl_text or "TC-133" in tbl_text):
            tc_table = tbl
            tc_table_idx = idx
            break

    if tc_table is not None:
        rows = tc_table.xpath("./w:tr", namespaces=NS)
        header_cols = rows[0].xpath("./w:tc", namespaces=NS) if rows else []
        print(f"  PASS: Test Cases table found at table index {tc_table_idx}!")
        print(f"  Row count: {len(rows)} (1 header + {len(rows) - 1} TC rows)")
        print(f"  Column count: {len(header_cols)}")
        col_names = [etree.tostring(c, encoding="utf-8", method="text").decode("utf-8").strip() for c in header_cols]
        print(f"  Column headers: {col_names}")

        # Check orientation of section containing this table
        # Find ancestor or next sectPr
        parent = tc_table.getparent()
        sect_prs = doc_tree.xpath("//w:sectPr", namespaces=NS)
        orientations = []
        for s in sect_prs:
            pgSz = s.xpath("./w:pgSz", namespaces=NS)
            if pgSz:
                orient = pgSz[0].get("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}orient", "portrait")
                orientations.append(orient)
        print(f"  Document Section Orientations: {orientations}")
    else:
        print("  WARNING: Test Cases table with TC-01..TC-133 was NOT found!")

    # --- 6. Table Borders & Autofit Audit ---
    print("\n--- 6. Table Borders & Properties Audit ---")
    tables_with_borders = 0
    for tbl in tables:
        tblPr = tbl.xpath("./w:tblPr", namespaces=NS)
        if tblPr:
            borders = tblPr[0].xpath("./w:tblBorders", namespaces=NS)
            if borders:
                tables_with_borders += 1
    print(f"Tables with explicit tblBorders: {tables_with_borders}/{len(tables)}")

"""
verify_consistency.py — Check consistency across JSON, MD, and DOCX test results
"""
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

# 1. JSON
json_path = Path("tailieu/test_results.json")
json_data = json.loads(json_path.read_text(encoding="utf-8-sig"))
tc_list_json = json_data["testCases"]
print(f"[JSON] Total: {len(tc_list_json)}, PASS: {json_data['summary']['pass']}, FAIL: {json_data['summary']['fail']}, SKIP: {json_data['summary']['skip']}")

# 2. Markdown
md_path = Path("tailieu/test_results.md")
md_text = md_path.read_text(encoding="utf-8-sig")
md_total_match = re.search(r"Tổng\*\*:\s*(\d+)\s*TC\s*\|\s*✅\s*PASS:\s*(\d+)\s*\|\s*❌\s*FAIL:\s*(\d+)", md_text)
if md_total_match:
    print(f"[MD]   Total: {md_total_match.group(1)}, PASS: {md_total_match.group(2)}, FAIL: {md_total_match.group(3)}")
else:
    print("[MD]   Could not parse header summary!")

# 3. DOCX
docx_path = Path("tailieu/BaoCaoDoAn.docx")
docx_tc_count = 0
with zipfile.ZipFile(docx_path) as z:
    doc = etree.fromstring(z.read("word/document.xml"))
    NS = {"w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main"}
    tables = doc.xpath("//w:tbl", namespaces=NS)
    for idx, tbl in enumerate(tables):
        t = etree.tostring(tbl, encoding="utf-8", method="text").decode("utf-8")
        if "TC-01" in t and "TC-130" in t:
            rows = tbl.xpath("./w:tr", namespaces=NS)
            docx_tc_count = len(rows) - 1
            print(f"[DOCX] Found test case table at index {idx} with {docx_tc_count} TC rows (+ 1 header row = {len(rows)} total rows).")
            first_tc = etree.tostring(rows[1], encoding="utf-8", method="text").decode("utf-8")
            last_tc = etree.tostring(rows[-1], encoding="utf-8", method="text").decode("utf-8")
            print(f"  First TC: {first_tc[:60]}...")
            print(f"  Last TC:  {last_tc[:60]}...")
            break

print("\n--- Summary Verification ---")
assert len(tc_list_json) == 130, f"Expected 130 TCs in JSON, got {len(tc_list_json)}"
assert json_data["summary"]["pass"] == 130, f"Expected 130 PASS in JSON, got {json_data['summary']['pass']}"
assert json_data["summary"]["fail"] == 0, f"Expected 0 FAIL in JSON, got {json_data['summary']['fail']}"
assert md_total_match is not None, "Markdown summary regex not matched"
assert md_total_match.group(1) == "130", f"Expected 130 in MD total, got {md_total_match.group(1)}"
assert md_total_match.group(2) == "130", f"Expected 130 in MD pass, got {md_total_match.group(2)}"
assert md_total_match.group(3) == "0", f"Expected 0 in MD fail, got {md_total_match.group(3)}"
assert docx_tc_count == 130, f"Expected 130 TC rows in DOCX table, got {docx_tc_count}"
print("All 3 data sources (JSON, MD, DOCX) are 100% synchronized and consistent!")

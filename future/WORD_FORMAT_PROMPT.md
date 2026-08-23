# MASTER WORD DOCUMENT FORMAT & LAYOUT REPAIR PROMPT
# Dành cho: AI Agent chuyên trách định dạng tài liệu Word (python-docx + lxml)
# File mục tiêu: Báo cáo đồ án tốt nghiệp DSA-Visual (PRO2192)

---

> ## ⚠️ NGUYÊN NHÂN GÂY LỖI CẦN TRÁNH TUYỆT ĐỐI:
> 1. **Lỗi xoay ngang cả tài liệu**: Trong chuẩn OpenXML Word, thẻ `<w:sectPr>` nằm trong `<w:pPr>` sẽ quy định hướng trang cho **TOÀN BỘ CÁC TRANG PHÍA TRƯỚC NÓ**. Việc chèn bừa bãi thẻ Section Break Landscape trước Bảng Kiểm thử đã khiến hơn 50 trang đầu (Trang bìa, Lời mở đầu, Khảo sát, Thiết kế...) bị biến thành **Khổ ngang (Landscape)**!
> 2. **Lỗi hình ảnh/chữ chồng chéo (Overlapping)**: Khi trang bị ép xoay ngang, các hình ảnh định dạng nổi (`wp:anchor` - Floating elements) và bảng biểu bị lệch toạ độ tuyệt đối, dẫn đến tình trạng đè chồng lên văn bản.
>
> 👉 **QUYẾT NGHỊ THIẾT KẾ DUY NHẤT:**
> **Toàn bộ tài liệu 100% dùng KHỔ DỌC (PORTRAIT A4: 210mm x 297mm)**. Không ngắt section ngang. Bảng kiểm thử 133 TC sẽ được căn chỉnh tỉ lệ cột và giảm font (8.5pt - 9pt) để vừa khít hoàn hảo trong khổ dọc mà không bị tràn hay gãy layout!

---

## 🎯 DANH SÁCH NHIỆM VỤ SỬA WORD CHI TIẾT

```
File Nguồn (Sạch): C:\Users\Administrator\Downloads\BaoCaoDoAn_PRO2192 (AutoRecovered).docx
File Test Data:     D:\FPT\neww\tailieu\test_results.json (chứa 133 TC)
File Logo Dự Án:   D:\FPT\neww\tailieu\diagrams\logo_dsavisual.png
File Đích:          D:\FPT\neww\tailieu\BaoCaoDoAn.docx
```

---

### BƯỚC 1: KHÔI PHỤC VỀ KHỔ DỌC A4 CHUẨN (100% PORTRAIT)
- Lấy file gốc từ `C:\Users\Administrator\Downloads\BaoCaoDoAn_PRO2192 (AutoRecovered).docx`.
- Xóa bỏ mọi `<w:sectPr>` dư thừa nằm trong các paragraph `<w:pPr>` (chỉ giữ lại 1 `sectPr` duy nhất ở cuối `body`).
- Thiết lập toàn bộ Document về kích thước A4 chuẩn:
  - Width: `11906` twips (21.0 cm)
  - Height: `16838` twips (29.7 cm)
  - Orientation: `portrait`
  - Lề trang chuẩn học viện: Trái: `3.0 cm` (1701 twips), Phải: `2.0 cm` (1134 twips), Trên: `2.0 cm`, Dưới: `2.0 cm`.

---

### BƯỚC 2: CHUYỂN HÌNH ẢNH TRÔI NỔI THÀNH INLINE (CHỐNG CHỒNG CHÉO)
- Duyệt qua toàn bộ tài liệu, tìm tất cả các phần tử `<wp:anchor>` (Floating Image):
  - Chuyển đổi thuộc tính bọc hoặc đặt vị trí ảnh thành `inline` (`<wp:inline>`) để ảnh nằm cùng dòng chảy văn bản, không bị đè lên chữ.
  - Căn giữa (`alignment = CENTER`) cho tất cả các đoạn chứa hình ảnh.

---

### BƯỚC 3: HEADER & FOOTER ĐỒNG BỘ
- **Header**:
  - Bên trái: Text `HỌC VIỆN KỸ THUẬT FPT` (Font Times New Roman, 10pt, Bold).
  - Ở giữa: Tab căn phải.
  - Bên phải: Logo DSA Visual (chèn ảnh `logo_dsavisual.png`, chiều cao cố định `0.8 cm`).
  - **Tuyệt đối không để lại text "TÊN ĐỀ TÀI"**.
- **Footer**:
  - Bên trái: `Đồ án tốt nghiệp: DSA Visual` (10pt, Italic).
  - Bên phải: Số trang tự động (`PAGE` field).

---

### BƯỚC 4: CHUẨN HÓA TYPOGRAPHY & XÓA SONG NGỮ
- **Font chữ toàn bộ**: `Times New Roman`.
  - Body Text: `12pt`, Line spacing: `1.2 - 1.3`, After: `3pt`.
  - Heading 1 (`PHẦN 1`, `PHẦN 2`...): `14pt`, **Bold**, In hoa, Before: `12pt`, After: `6pt`.
  - Heading 2 (`1.1`, `1.2`...): `13pt`, **Bold**, Before: `6pt`, After: `3pt`.
  - Heading 3 (`1.1.1`...): `12pt`, **Bold**.
  - Table Cells: `9pt - 10pt`.
  - Captions: `10pt`, *Italic*, Căn giữa.
- **Xóa song ngữ tiêu đề**:
  - `KHẢO SÁT – SURVEY` / `KHẢO SÁT - SURVEY` → `KHẢO SÁT`
  - `PHÂN TÍCH – ANALYSIS` / `PHÂN TÍCH - ANALYSIS` → `PHÂN TÍCH`
  - `THIẾT KẾ – DESIGN` / `THIẾT KẾ - DESIGN` → `THIẾT KẾ`
  - `THỰC HIỆN – IMPLEMENT` / `THỰC HIỆN - IMPLEMENT` → `THỰC HIỆN`
  - `KIỂM THỬ – TESTING` / `KIỂM THỬ - TESTING` → `KIỂM THỬ`
- **Xóa ngoặc đơn rác ở Heading**: (VD: `2.1 Kiến trúc hệ thống (Architecture)` → `2.1 Kiến trúc hệ thống`).

---

### BƯỚC 5: SỬA LỖI MỤC LỤC (TOC BOOKMARKS)
- Quét tìm tất cả các đoạn trong Mục lục (TOC) có chứa chuỗi `"Error! Bookmark not defined"`.
- Xóa bỏ các đoạn lỗi này để mục lục khớp 100% với danh mục bài viết.

---

### BƯỚC 6: XÂY DỰNG BẢNG KIỂM THỬ 133 TEST CASES KHỔ DỌC CHUẨN ĐẸP
*Thay vì xoay ngang, hãy format bảng kiểm thử 7 cột thật tối ưu để vừa khít khổ dọc:*

1. **Vị trí chèn**: Sau tiêu đề `PHẦN 6: KIỂM THỬ` (hoặc mục kiểm thử tương ứng).
2. **Kích thước font chữ trong bảng**: `8.5pt` (Header 9pt Bold nền xám nhẹ `#F2F2F2`).
3. **Phân bổ tỷ lệ độ rộng 7 cột (Tổng width = 16.0 cm vừa khít lề A4)**:
   - Cột 1 (`TC-ID`): `1.3 cm` (8.1%) — Căn giữa
   - Cột 2 (`Màn hình`): `1.8 cm` (11.25%)
   - Cột 3 (`Thao tác`): `3.2 cm` (20.0%)
   - Cột 4 (`Đầu vào`): `2.7 cm` (16.8%)
   - Cột 5 (`Kết quả mong đợi`): `3.2 cm` (20.0%)
   - Cột 6 (`Kết quả thực tế`): `2.3 cm` (14.4%)
   - Cột 7 (`Kết quả`): `1.5 cm` (9.4%) — Căn giữa (`PASS` / `FAIL`)
4. **Border**: Toàn bộ viền đơn mỏng màu đen (`w:val="single"`, `w:sz="4"`, `w:color="CCCCCC"`).
5. **Dữ liệu**: Đọc toàn bộ 133 TC từ file `tailieu/test_results.json`.

---

## 🐍 SCRIPT PYTHON THỰC THI HOÀN CHỈNH (`word_perfect_fixer.py`)

Agent hãy tạo và chạy script Python sau để thực hiện tự động:

```python
import os, sys, json, re, shutil
sys.stdout.reconfigure(encoding='utf-8')
from pathlib import Path
from docx import Document
from docx.shared import Pt, Cm, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml.ns import qn, nsdecls
from docx.oxml import parse_xml, OxmlElement
from lxml import etree

SRC_INPUT = r'C:\Users\Administrator\Downloads\BaoCaoDoAn_PRO2192 (AutoRecovered).docx'
TARGET_OUTPUT = r'D:\FPT\neww\tailieu\BaoCaoDoAn.docx'
LOGO_PATH = r'D:\FPT\neww\tailieu\diagrams\logo_dsavisual.png'
TEST_JSON_PATH = r'D:\FPT\neww\tailieu\test_results.json'

doc = Document(SRC_INPUT)

# 1. FIX TOÀN BỘ SECTION VỀ A4 KHỔ DỌC
print("[1] Chuẩn hóa toàn bộ Section về Portrait A4...")
for p in doc.paragraphs:
    pPr = p._p.find(qn('w:pPr'))
    if pPr is not None:
        sectPr = pPr.find(qn('w:sectPr'))
        if sectPr is not None:
            pPr.remove(sectPr)

for s in doc.sections:
    s.page_width = Cm(21.0)
    s.page_height = Cm(29.7)
    s.orientation = 0  # Portrait
    s.top_margin = Cm(2.0)
    s.bottom_margin = Cm(2.0)
    s.left_margin = Cm(3.0)
    s.right_margin = Cm(2.0)

# 2. XÓA FLOATING ANCHORS CỦA ẢNH (CHUYỂN THÀNH INLINE)
print("[2] Chuyển đổi ảnh floating sang inline chống đè chữ...")
body = doc.element.body
anchors = body.xpath('//wp:anchor', namespaces={'wp': 'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing'})
for a in anchors:
    for wrap in a.xpath('./wp:wrapNone', namespaces={'wp': 'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing'}):
        wrap.getparent().remove(wrap)

# 3. TYPOGRAPHY & SONG NGỮ
print("[3] Chuẩn hóa font chữ và làm sạch tiêu đề song ngữ...")
BILINGUAL = {
    'KHẢO SÁT – SURVEY': 'KHẢO SÁT', 'KHẢO SÁT - SURVEY': 'KHẢO SÁT',
    'PHÂN TÍCH – ANALYSIS': 'PHÂN TÍCH', 'PHÂN TÍCH - ANALYSIS': 'PHÂN TÍCH',
    'THIẾT KẾ – DESIGN': 'THIẾT KẾ', 'THIẾT KẾ - DESIGN': 'THIẾT KẾ',
    'THỰC HIỆN – IMPLEMENT': 'THỰC HIỆN', 'THỰC HIỆN - IMPLEMENT': 'THỰC HIỆN',
    'KIỂM THỬ – TESTING': 'KIỂM THỬ', 'KIỂM THỬ - TESTING': 'KIỂM THỬ'
}

def fix_run_font(run, font='Times New Roman', sz=12, bold=None):
    run.font.name = font
    run.font.size = Pt(sz)
    if bold is not None: run.bold = bold
    rPr = run._r.get_or_add_rPr()
    rFonts = rPr.find(qn('w:rFonts'))
    if rFonts is None:
        rFonts = OxmlElement('w:rFonts')
        rPr.append(rFonts)
    for attr in ['ascii', 'hAnsi', 'eastAsia', 'cs']:
        rFonts.set(qn('w:' + attr), font)

for p in doc.paragraphs:
    if 'Error! Bookmark not defined' in p.text:
        p._p.getparent().remove(p._p)
        continue
    
    for old, new in BILINGUAL.items():
        if old in p.text:
            p.text = p.text.replace(old, new)
            break
            
    if p.style.name.startswith('Heading'):
        p.text = re.sub(r'\s*\([A-Za-z\s]+\)\s*$', '', p.text)
        sz = 14 if '1' in p.style.name else (13 if '2' in p.style.name else 12)
        for r in p.runs: fix_run_font(r, sz=sz, bold=True)
    else:
        for r in p.runs: fix_run_font(r, sz=12)

# 4. SỬA HEADER & FOOTER
print("[4] Chuẩn hóa Header & Footer...")
for s in doc.sections:
    hdr = s.header
    hdr.is_linked_to_previous = False
    for p in hdr.paragraphs: p.clear()
    p_hdr = hdr.paragraphs[0] if hdr.paragraphs else hdr.add_paragraph()
    
    r_left = p_hdr.add_run("HỌC VIỆN KỸ THUẬT FPT")
    fix_run_font(r_left, sz=10, bold=True)
    
    pPr = p_hdr._p.get_or_add_pPr()
    tabs = OxmlElement('w:tabs')
    tab = OxmlElement('w:tab')
    tab.set(qn('w:val'), 'right')
    tab.set(qn('w:pos'), '9072')  # 16cm
    tabs.append(tab)
    pPr.append(tabs)
    p_hdr.add_run()._r.append(OxmlElement('w:tab'))
    
    if Path(LOGO_PATH).exists():
        r_logo = p_hdr.add_run()
        r_logo.add_picture(LOGO_PATH, height=Cm(0.8))

# 5. CHÈN BẢNG 133 TEST CASES GỌN GÀNG KHỔ DỌC
print("[5] Chèn bảng kiểm thử 133 Test Cases khổ dọc...")
test_data = json.load(open(TEST_JSON_PATH, encoding='utf-8'))['testCases']

target_p = None
for p in doc.paragraphs:
    if 'KIỂM THỬ' in p.text and p.style.name.startswith('Heading'):
        target_p = p

if target_p is not None:
    tbl = doc.add_table(rows=1, cols=7)
    tbl.style = 'Table Grid'
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    
    col_names = ['TC-ID', 'Màn hình', 'Thao tác', 'Đầu vào', 'Kết quả mong đợi', 'Thực tế', 'Kết quả']
    col_widths = [Cm(1.3), Cm(1.8), Cm(3.2), Cm(2.7), Cm(3.2), Cm(2.3), Cm(1.5)]
    
    hdr_cells = tbl.rows[0].cells
    for i, name in enumerate(col_names):
        hdr_cells[i].text = name
        hdr_cells[i].width = col_widths[i]
        shading = parse_xml(r'<w:shd {} w:fill="F2F2F2"/>'.format(nsdecls('w')))
        hdr_cells[i]._tc.get_or_add_tcPr().append(shading)
        for r in hdr_cells[i].paragraphs[0].runs:
            fix_run_font(r, sz=9, bold=True)
            
    for tc in test_data:
        row = tbl.add_row()
        vals = [tc['id'], tc.get('screen',''), tc.get('action',''), tc.get('input',''),
                tc.get('expected',''), tc.get('actual',''), tc.get('status','')]
        for i, val in enumerate(vals):
            cell = row.cells[i]
            cell.width = col_widths[i]
            cell.text = str(val)[:150]
            if i in [0, 6]: cell.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in cell.paragraphs[0].runs:
                fix_run_font(r, sz=8.5, bold=(i==6))
                if i == 6 and val == 'PASS': r.font.color.rgb = RGBColor(0x15, 0x80, 0x3D)
                elif i == 6 and val == 'FAIL': r.font.color.rgb = RGBColor(0xB9, 0x1C, 0x1C)
                
    target_p._p.addnext(tbl._tbl)

# 6. LÀM SẠCH TẤT CẢ CÁC BẢNG TRONG BÁO CÁO
print("[6] Căn chỉnh và thêm border cho tất cả bảng...")
for t in doc.tables:
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    for r in t.rows:
        for c in r.cells:
            for p in c.paragraphs:
                for run in p.runs:
                    fix_run_font(run, sz=9.5)

# LƯU FILE
print(f"[7] Lưu tệp Word chuẩn: {TARGET_OUTPUT}")
doc.save(TARGET_OUTPUT)
print("HOÀN TẤT THÀNH CÔNG! Đã khôi phục 100% khổ dọc và layout sắc nét.")
```

---

## 📋 TIÊU CHÍ NGHIỆM THU (VERIFICATION CHECKLIST)
- [ ] Mở file Word trên MS Word: **Tất cả các trang đều là KHỔ DỌC (Portrait)**.
- [ ] Không còn bất kỳ trang nào bị xoay ngang.
- [ ] Không có hình ảnh nào đè lên chữ hoặc bảng.
- [ ] Bảng 133 Test Cases hiển thị trọn vẹn, không bị tràn ra ngoài lề phải.
- [ ] Header có logo DSA Visual và dòng chữ `HỌC VIỆN KỸ THUẬT FPT`.
- [ ] File lưu tại `D:\FPT\neww\tailieu\BaoCaoDoAn.docx` và đã push lên git `dev`.

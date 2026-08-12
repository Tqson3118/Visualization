# HỢP ĐỒNG VIẾT BÁO CÁO — HẰNG SỐ & BÀI HỌC CHUNG (SESSION A)

> File này là nguồn chung cho MỌI task viết part của tailieu/BAO_CAO.md.
> Mọi agent con PHẢI đọc file này trước khi viết. Mọi con số/tên trong đây là BẮT BUỘC, không được đổi.
> Nguồn chính: docs/BAO_CAO_SPEC.md (đặc tả) + tailieu/NET202_Project document_6 (1).pdf (S-Clinic — chuẩn độ sâu).

---

## 1. BÀI HỌC TỪ BÁO CÁO CŨ BỊ CHÊ (KHÔNG được tái phạm)

Báo cáo cũ (tailieu/PRO2192_Report (2).docx và bản formatted) bị chê vì:
1. Chèn khung placeholder kiểu `[KHUNG CHÈN ẢNH THỰC TẾ: ...]` — CẤM. Không có ảnh thật → dùng ảnh placeholder PNG có nhãn (tailieu/placeholders/) hoặc wireframe ASCII.
2. Viết nội dung "tô đẹp" lạc đề: SOLID, OOP Sandbox, Glassmorphism, Clean Architecture, PostgreSQL, Event Sourcing, DI Container... — hệ thống mới KHÔNG có các thứ đó (đã cắt theo NFR-17 và các quyết định ở docs). MỌI nội dung phải truy ngược về FR/NFR/UC/file trong docs/; không có nguồn thì KHÔNG viết.
3. Số liệu bịa: "1550+ tests", "22MB sau 5 phút" — CẤM. Số nào chưa đo được → ghi "chờ hoàn tất kiểm thử (tuần 19-20)".
4. Danh sách thành viên sai: bản cũ dùng 6 người (Nguyễn Đình Thiên Long, PS12345...) — hệ mới có 4 thành viên thật (mục 3 dưới), mã SV phải đúng.
5. Mục lục đánh số nhảy (3→16→21→25), ký tự lạ (emoji 🕮, từ ngoại lai không nguồn) — mục lục do pandoc --toc tự sinh, cấm ký tự lạ.
6. Học thuật hóa: công thức toán (LCOM4 = ...), giải thích lý thuyết thuật toán, trace từng bước, bảng trạng thái 15 GT — CẤM. Chỉ bảng Big-O tóm tắt nếu cần (bảng, không công thức).

## 2. CHUẨN ĐỘ SÂU S-CLINIC (đã đọc PDF 48 trang — bám theo)

1. **Đặc tả chức năng (Phần 3.3)**: mỗi chức năng đúng 4 mục ngắn: `Mô tả chức năng` (2-4 câu nghiệp vụ) / `Dữ liệu liên quan` (tên bảng) / `Đối tượng sử dụng` / `Yêu cầu bảo mật` (1-2 câu). KHÔNG dùng khuôn 7 thuộc tính FR.
2. **CẤM học thuật hóa** (xem mục 1.6).
3. **Engine EDV (4.4.2)**: tối đa 2 đoạn: (a) ý tưởng kiến trúc 5-8 dòng ("code thật chạy qua StepExecutor ghi trace, hoạt ảnh = phát lại trace — không hardcode"), (b) 1 khối code trích 5-10 dòng.
4. **Data dictionary (4.3.2)**: bảng 5 cột (Tên cột / Kiểu dữ liệu / Khóa / Bắt buộc / Mô tả chi tiết), mô tả mỗi cột 1 câu đời thường. Đủ 32 bảng nhưng mỗi bảng gọn (5-15 dòng), chỉ liệt kê cột quan trọng.
5. **Thực hiện (Phần 5)**: ảnh UI + mô tả ngắn từng màn (như S-Clinic 4.1); DB: Code-First + 1 ví dụ entity trích + 2-4 bullet cấu hình Fluent API; Services: mô tả quy trình bằng lời + sơ đồ (như S-Clinic 4.3).
6. **Kiểm thử (Phần 6)**: kế hoạch + kịch bản bảng + kết quả THẬT; chưa chạy → "chờ hoàn tất kiểm thử (tuần 19-20)".
7. **Kết luận**: 2 mục `Kết quả đạt được` + `Khó khăn & Bài học kinh nghiệm` — viết thành thật.
8. **Tổng thể**: nhiều ảnh, câu văn nghiệp vụ đơn giản, mỗi mục 1-4 trang; mục nào quá 4 trang là đang "đào sâu" → cắt.

## 3. HẰNG SỐ BẮT BUỘC (không đổi)

- Tên dự án: **Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**
- GVHD: **Phạm Ngọc Ái Liên** · Lớp: **SD21361** · Ngành: **Ứng dụng phần mềm** · Trường: **Trường Cao Đẳng Thực Hành FPT**
- Bìa ghi: "TP.HCM, ngày 12 tháng 8 năm 2026" · Ngày bảo vệ: ĐỂ TRỐNG
- Bảng 4 thành viên:

| Mã SV | Họ và tên | Vai trò | Nhiệm vụ chính |
|---|---|---|---|
| TD01287 | Mai Tiểu Bảo | Trưởng nhóm | Backend ASP.NET Core + CSDL (task khó ưu tiên) |
| TD01282 | Thái Quang Sơn | Thành viên | Frontend Vue 3 + giao diện (task khó ưu tiên) |
| TD01131 | Huỳnh Lê Minh Thư | Thành viên | Simulation Engine (code) + Kiểm thử |
| TD01261 | Trần Viết Tâm Phúc | Thành viên | Code hỗ trợ (đơn giản) + Tài liệu + triển khai |

## 4. MỐC THỜI GIAN CHÍNH THỨC (nhất quán MỌI bảng)

- Khởi động: **12/05/2026** · Kết thúc phát triển: **~11/08/2026** (13 tuần) — KHÔNG ghi "20 tuần/16 tuần"
- 10 sprint × ~1 tuần: S1 12/05→18/05 · S2 19/05→25/05 · S3 26/05→01/06 · S4 02/06→08/06 · S5 09/06→15/06 · S6 16/06→22/06 · S7 23/06→29/06 · S8 30/06→06/07 · S9 07/07→13/07 · S10 14/07→20/07
- Hoàn thiện (kiểm thử toàn diện + 12 file tài liệu + deploy staging): 21/07→11/08
- Báo cáo Word + slide + chuẩn bị bảo vệ: 12/08→31/08/2026 (bảo vệ cuối tháng 8 — ngày cụ thể để trống)

## 5. QUY TẮC CỨNG (CẤM — mọi part)

- CẤM cụm "20 tuần" và "16 tuần" ở MỌI ngữ cảnh (trừ đúng cụm "chờ hoàn tất kiểm thử (tuần 19-20)" để đánh dấu kết quả chưa chạy — cụm này do spec quy định).
- CẤM placeholder chữ kiểu [KHUNG CHÈN ẢNH...], CẤM ô trống trong bảng — thiếu dữ liệu → ghi "chưa cập nhật (12/08/2026)" hoặc "chờ hoàn tất kiểm thử (tuần 19-20)".
- CẤM số liệu bịa, CẤM công thức toán, CẤM lý thuyết thuật toán dài dòng, CẤM tên SOLID/OOP Sandbox/Glassmorphism/Clean Architecture/PostgreSQL/Event Sourcing (feature đã cắt).
- Mỗi mục ghi nguồn dạng `(nguồn: SRS §3.9)` — chú thích nhỏ cuối mục.
- Sơ đồ Mermaid quan trọng: GIỮ dạng code block + 1-2 câu giải thích.
- Ảnh: dùng `![Hình X.Y - Tên](placeholders/TEN.png)` + dòng caption `*Hình X.Y: mô tả 1 câu. (ảnh placeholder — chụp thật thay sau)*`. Đường dẫn ảnh tương đối tính từ tailieu/ (BAO_CAO.md nằm trong tailieu/).
- Ngôn ngữ: tiếng Việt, câu nghiệp vụ đơn giản, không ký tự lạ/emoji.
- Định dạng heading: PHẦN dùng `# PHẦN N: TÊN` (đúng tên trong cấu trúc mục 6), mục con `## N.M`, mục cháu `### N.M.K`.

## 6. CẤU TRÚC BÁO CÁO + BẢN ĐỒ PART FILE + NGUỒN THEO CHƯƠNG

| Part file (tailieu/parts/) | Nội dung | Nguồn chính (docs/) |
|---|---|---|
| 00-bai-hoc-va-hang-so.md | File này | — |
| 01-mo-dau-p1-p2.md | BÌA + LỜI MỞ ĐẦU + PHẦN 1 + PHẦN 2 | PRODUCTION_PROMPT §0.5/§20.1, SRS §1.2-1.3/§1.9/§3.9 |
| 02-phan3.md | PHẦN 3 (3.1-3.3) | SDD §2, SRS §6, SRS §3.9 |
| 03-phan4.md | PHẦN 4 (4.1-4.4) | SDD §3/§4/§5/§7/§8, SCREEN_MAP |
| 04-phan5-p6.md | PHẦN 5 + PHẦN 6 | SDD §3/§4/§5/§7, SRS §6, API_REFERENCE, TEST_PLAN |
| 05-phan7-pl.md | PHẦN 7 + KẾT LUẬN + TLTK + PHỤ LỤC A-D | DEPLOY, USER_GUIDE, GLOSSARY, API_REFERENCE, shared/simulation-catalog.json, SDD §16 |

Cấu trúc đích (đúng khuôn trường cũ, giữ số PHẦN, đổi tiêu đề mục con cho khớp DSA-Visual):

```
BÌA (không dùng heading markdown — dùng dòng chữ + --- ; hết bìa chèn page break)
LỜI MỞ ĐẦU (1-2 trang: bối cảnh, bài học bản cũ §0.5, cách tổ chức báo cáo)
MỤC LỤC (pandoc --toc tự sinh — không viết tay)

# PHẦN 1: GIỚI THIỆU ĐỀ TÀI
## 1.1 Giới thiệu dự án      (vấn đề, mục tiêu KPI G1-G8 tóm tắt, 3 khó khăn của SV)
## 1.2 Ban dự án             (bảng 4 thành viên + vai trò chuyên môn)

# PHẦN 2: KHẢO SÁT – SURVEY
## 2.1 Yêu cầu của khách hàng  (khảo sát VisuAlgo/USFCA/Algorithm-Visualizer + bảng so sánh + kết luận; FR/NFR tóm tắt)
## 2.2 Kế hoạch dự án          (10 sprint + hoàn thiện + bảo vệ — đúng mốc mục 4)

# PHẦN 3: PHÂN TÍCH - ANALYSIS
## 3.1 Mô hình triển khai hệ thống   (Browser → API → SQL Server)
## 3.2 Sơ đồ Use Cases
### 3.2.1 Tổng quan (3 tác nhân)
### 3.2.2 Use Cases dành cho người học (UC-01..08, 14, 17..19, 21..32)
### 3.2.3 Use Cases dành cho giảng viên (UC-09..11, 20)
### 3.2.4 Use Cases dành cho quản trị viên (UC-12, 13)
## 3.3 Đặc tả yêu cầu hệ thống (SRS)   (master matrix FR + 2-3 UC đặc tả 4 mục chuẩn S-Clinic)

# PHẦN 4: THIẾT KẾ - DESIGN
## 4.1 Mô hình công nghệ   (Vue 3 + Pinia + Vite / ASP.NET Core + EF Core / SQL Server)
## 4.2 Thiết kế giao diện
### 4.2.1 Sitemap (~32 màn)
### 4.2.2 Layout (design system: màu, font, component; App shell + sidebar theo vai trò)
### 4.2.3 Giao diện chức năng (ảnh placeholder 12 màn + wireframe ASCII)
## 4.3 Thiết kế dữ liệu
### 4.3.1 Sơ đồ quan hệ thực thể (ERD) (2 sơ đồ: lõi học tập 24 bảng + gamification/code 8 bảng)
### 4.3.2 Chi tiết thực thể (32 bảng, bảng 5 cột chuẩn S-Clinic)
## 4.4 Thiết kế phần mềm
### 4.4.1 Kiến trúc backend 2 lớp (Controller → Service → DbContext — KHÔNG Repository)
### 4.4.2 Simulation Engine EDV (TRÁI TIM đồ án — tối đa 2 đoạn theo chuẩn S-Clinic)
### 4.4.3 Máy trạng thái mô phỏng (stateDiagram-v2)

# PHẦN 5: THỰC HIỆN – IMPLEMENT
## 5.1 Cơ sở dữ liệu (Code-First, 2-3 migration/seed quan trọng)
## 5.2 Simulation Engine & Sandbox (generator bubble sort trích mã, golden data, Code Runner Web Worker)
## 5.3 Sơ đồ kiến trúc công nghệ (cấu trúc thư mục frontend/backend)
## 5.4 Các loại sơ đồ tương tác
### 5.4.1 Sequence Diagram (UC-01 chạy mô phỏng, UC-25 trừ tim atomic)
### 5.4.2 Activity Diagram (state machine mô phỏng, luồng Practice Ladder)
## 5.5 API Endpoints
### 5.5.1 Controllers (bảng endpoint theo nhóm)
### 5.5.2 Services (Business Logic) (12 service)

# PHẦN 6: KIỂM THỬ - TESTING
## 6.1 Chiến lược kiểm thử (kim tự tháp)
## 6.2 Kết quả kiểm thử (bảng PASS/FAIL — số THẬT hoặc "chờ hoàn tất kiểm thử (tuần 19-20)")
## 6.3 Hiệu năng + bảo mật + UX (TEST-PERF-001..008, TEST-SEC, SUS)

# PHẦN 7: ĐÓNG GÓI & TRIỂN KHAI
## 7.1 Đóng gói frontend/backend (lệnh build, docker-compose)
## 7.2 Triển khai production (nginx, systemd, biến môi trường)
## 7.3 CI/CD + backup (GitHub Actions, backup 14 bản)
## 7.4 Runbook sự cố (8 sự cố)

# KẾT LUẬN & HƯỚNG PHÁT TRIỂN
## Kết quả đạt được (so KPI G1-G8, tự đánh giá trung thực)
## Khó khăn & Bài học kinh nghiệm (thành thật — chuẩn S-Clinic 7.2)
## Hướng phát triển (backlog: online judge, AI Assistant PoC...)

# TÀI LIỆU THAM KHẢO (chỉ tài liệu THỰC SỰ dùng: Vue 3, ASP.NET Core, EF Core, CLRS, VisuAlgo...)

# PHỤ LỤC A: Hướng dẫn cài đặt môi trường (DEPLOY §2-3)
# PHỤ LỤC B: Phím tắt + thuật ngữ (USER_GUIDE + GLOSSARY)
# PHỤ LỤC C: Thư viện bên thứ ba (THIRD_PARTY.md CHƯA TỒN TẠI → ghi "chưa cập nhật (12/08/2026)" + bảng lib trích từ SDD/DEPLOY)
# PHỤ LỤC D: Danh mục mô phỏng (shared/simulation-catalog.json — 44 mô phỏng, đọc utf-8-sig)
```

## 7. HÌNH/BẢNG — QUY ƯỚC ĐÁNH SỐ THEO PHẦN

- Hình: `Hình <phần>.<thứ tự trong phần>` — VD Hình 3.1, Hình 3.2, Hình 4.1... Caption chuẩn: `*Hình X.Y: <tên> — <mô tả 1 câu>*`.
- Bảng: `Bảng <phần>.<thứ tự>` — VD Bảng 2.1, Bảng 4.3. Tiêu đề bảng đặt TRƯỚC bảng, định dạng `**Bảng X.Y: <tên>**`.
- Số thứ tự đếm RIÊNG trong từng part (không cần phối hợp giữa các part).

## 8. DANH SÁCH ẢNH PLACEHOLDER (đã có sẵn — trỏ đúng tên)

12 màn (tailieu/placeholders/): 01-home.png, 02-login.png, 04-lesson-detail.png, 05-simulator.png, 06-exercise.png, 13-learning-path.png, 14-ladder.png, 15-lab.png, 16-code-runner.png, 17-benchmark.png, 24-leaderboard.png, 32-profile.png
6 sơ đồ (tailieu/placeholders/): 01-usecase-tong-quan.png, 02-usecase-hoc-vien.png, 03-usecase-giang-vien.png, 04-usecase-admin.png, 05-erd-tong-quan.png, 06-erd-chi-tiet.png

# ĐẶC TẢ BÁO CÁO DỰ ÁN WORD (BAO_CAO_SPEC)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Đặc tả đầu ra báo cáo đồ án (Word) |
| Phiên bản | 1.0 |
| Ngày cập nhật | 12/08/2026 |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md (Phần 0-22) + 12 file bàn giao (17.1) |

---

## 1. Mục đích

File này là nguồn duy nhất (single source of truth) để AI sinh **báo cáo đồ án dạng Word** hoàn chỉnh. Báo cáo được tổng hợp từ 12 file bàn giao trong `docs/`, có ảnh chụp màn hình thật, đóng gói bằng Pandoc ra `.docx` trong thư mục `tailieu/`.

## 2. Đầu ra

| Mục | Giá trị |
|---|---|
| File nguồn Markdown | `tailieu/BAO_CAO.md` (AI sinh, theo cấu trúc mục 4) |
| File Word cuối | `tailieu/BaoCaoDoAn.docx` |
| Độ dài | **Bản ĐẦY ĐỦ** (đủ phạm vi mục) nhưng **mức độ trình bày theo chuẩn S-Clinic** — tham chiếu `tailieu/NET202_Project document_6 (1).pdf` (~48 trang); báo cáo mới dự kiến 50-70 trang, nhiều ảnh, không phình học thuật |
| Ảnh màn hình | `tailieu/screenshots/NN-ten-man.png` (chụp bằng Playwright, xem mục 6) |
| Lệnh build | `pandoc tailieu/BAO_CAO.md -o tailieu/BaoCaoDoAn.docx --toc --reference-doc="tailieu/DSA_Visual_Template.docx"` |
| Template tham chiếu | `tailieu/DSA_Visual_Template.docx` (nếu có — dùng làm khuôn định dạng trường; không có thì bỏ `--reference-doc`) |

## 3. Nguồn dữ liệu (đọc trước khi sinh báo cáo)

| # | Nguồn | Dùng cho chương |
|---|---|---|
| 1 | `docs/PRODUCTION_PROMPT.md` | Toàn bộ (bối cảnh, KPI, quyết định) |
| 2 | `docs/SRS.md` | Chương 1-2 |
| 3 | `docs/SDD.md` | Chương 3 |
| 4 | `docs/API_REFERENCE.md` | Chương 3.5 (phụ lục A) |
| 5 | `docs/TEST_PLAN.md` | Chương 4 |
| 6 | `docs/DEPLOY.md` | Chương 5 |
| 7 | `docs/USER_GUIDE.md` | Chương 6 (tóm tắt) |
| 8 | `docs/GLOSSARY.md` | Phụ lục B |
| 9 | `docs/SCREEN_MAP.md` | Chương 3.4 (bản đồ màn hình + danh sách ảnh) |
| 10 | `THIRD_PARTY.md` | Phụ lục C |
| 11 | `shared/simulation-catalog.json` | Phụ lục D |
| 12 | `tailieu/screenshots/` | Ảnh minh họa màn hình |

## 4. Cấu trúc báo cáo — THEO ĐÚNG KHUÔN TRƯỜNG (bắt buộc)

> Khuôn: `tailieu/PRO2192_Report (2).docx` (mẫu báo cáo cũ của nhóm, theo format trường FPT — LỜI MỞ ĐẦU + PHẦN 1..7 + KẾT LUẬN + TÀI LIỆU THAM KHẢO + PHỤ LỤC).
> Quy tắc: GIỮ NGUYÊN số hiệu PHẦN và cấp mục con của mẫu; ĐỔI tiêu đề mục con cho khớp hệ thống DSA-Visual mới. Được phép thêm mục con mới (VD: 3.2.3, 3.2.4 cho vai trò mới) — không đổi số PHẦN.

```
BÌA (GVHD, lớp, bảng 4 thành viên, tên dự án, ngành, TP.HCM ngày, TRƯỜNG CAO ĐẲNG THỰC HÀNH FPT)
LỜI MỞ ĐẦU (1-2 trang: bối cảnh, bài học bản cũ §0.5, cách tổ chức báo cáo)
MỤC LỤC (--toc của pandoc)

PHẦN 1: GIỚI THIỆU ĐỀ TÀI
  1.1 Giới thiệu dự án         ← SRS §1.2-1.3 (vấn đề, mục tiêu KPI, 3 khó khăn của SV)
  1.2 Ban dự án                ← BẢNG 4 THÀNH VIÊN (mục 4.1) + vai trò chuyên môn

PHẦN 2: KHẢO SÁT – SURVEY
  2.1 Yêu cầu của khách hàng   ← SRS §1.9 (khảo sát VisuAlgo/USFCA/Algorithm-Visualizer,
                                   bảng so sánh + kết luận) + FR/NFR tóm tắt
  2.2 Kế hoạch dự án           ← PRODUCTION_PROMPT 20.1 (nội dung 10 sprint, nhưng THỜI GIAN nén
                                   theo thực tế bảo vệ cuối tháng 8 — xem mốc bên dưới)

  **MỐC THỜI GIAN CHÍNH THỨC (bắt buộc dùng — nhất quán mọi bảng):**
  - Khởi động: 12/05/2026 · Kết thúc phát triển: ~11/08/2026 (13 tuần)
  - 10 sprint × ~1 tuần: S1 12/05→18/05 · S2 19/05→25/05 · S3 26/05→01/06 · S4 02/06→08/06 ·
    S5 09/06→15/06 · S6 16/06→22/06 · S7 23/06→29/06 · S8 30/06→06/07 · S9 07/07→13/07 ·
    S10 14/07→20/07
  - Hoàn thiện (kiểm thử toàn diện + 12 file tài liệu + deploy staging): 21/07→11/08
  - Báo cáo Word + slide + chuẩn bị bảo vệ: 12/08→31/08/2026 (bảo vệ cuối tháng 8)
  - ⚠ KHÔNG ghi "20 tuần/16 tuần" trong báo cáo — ghi 13 tuần; sprint là "~1 tuần"

PHẦN 3: PHÂN TÍCH - ANALYSIS
  3.1 Mô hình triển khai hệ thống   ← SDD §2 (sơ đồ Mermaid: Browser → API → SQL Server)
  3.2 Sơ đồ Use Cases
    3.2.1 Tổng quan                    ← SRS §6 (sơ đồ UC tổng thể; 3 tác nhân:
                                          Người học / Giảng viên / Quản trị viên)
    3.2.2 Use Cases dành cho người học ← SRS §6 (UC-01..08, 14, 17..19, 21..32 — sơ đồ Mermaid)
    3.2.3 Use Cases dành cho giảng viên ← SRS §6 (UC-09..11, 20 — sơ đồ Mermaid)
    3.2.4 Use Cases dành cho quản trị viên ← SRS §6 (UC-12, 13 — sơ đồ Mermaid)
  3.3 Đặc tả yêu cầu hệ thống (SRS)   ← SRS §3.9 (master matrix FR đầy đủ) + 2-3 UC hạt
                                          nhân đặc tả khuôn 17.13 (VD: UC-01, UC-25, UC-26)

PHẦN 4: THIẾT KẾ - DESIGN
  4.1 Mô hình công nghệ            ← SDD §2/§3 (Vue 3 + Pinia + Vite / ASP.NET Core + EF Core
                                      / SQL Server; sơ đồ 3 lớp công nghệ)
  4.2 Thiết kế giao diện
    4.2.1 Sitemap                     ← SDD §8 + SCREEN_MAP (sơ đồ route ~32 màn)
    4.2.2 Layout                      ← SDD §7 (hệ thống thiết kế: màu, font, component;
                                          App shell + sidebar theo vai trò 20.5.2)
    4.2.3 Giao diện chức năng         ← Ảnh màn hình thật (mục 6) + wireframe ASCII
                                          (Màn 05 simulator, Màn 14 ladder, Màn 16 code runner...)
  4.3 Thiết kế dữ liệu
    4.3.1 Sơ đồ quan hệ thực thể (ERD)  ← SDD §7 (2 sơ đồ ERD Mermaid: lõi học tập 24 bảng
                                             + gamification/code 8 bảng)
    4.3.2 Chi tiết thực thể             ← SDD §7 (32 bảng tóm tắt — đủ cột khóa chính,
                                             bảng Users/Lessons/Exercises/NodeSessions mô tả đầy đủ)
  4.4 Thiết kế phần mềm (thay "Sơ đồ lớp DAO & Repositories" —
      hệ mới KHÔNG dùng Repository theo NFR-17)
    4.4.1 Kiến trúc backend 2 lớp       ← SDD §5 (Controller → Service → DbContext, classDiagram)
    4.4.2 Simulation Engine EDV         ← SDD §4 (StepExecutor, TraceEvent, Generator/Renderer,
                                             classDiagram — TRÁI TIM đồ án, giải thích kỹ)
    4.4.3 Máy trạng thái mô phỏng       ← SDD §3 (stateDiagram-v2)

PHẦN 5: THỰC HIỆN – IMPLEMENT
  5.1 Cơ sở dữ liệu (thay "Database DDL")
                                    ← SDD §7 tóm tắt + trích 2-3 migration/seed quan trọng
  5.2 Simulation Engine & Sandbox (thay "CSS Layout Glassmorphism")
                                    ← SDD §4 (EDV triển khai: generator bubble sort trích mã,
                                      golden data; Code Runner sandbox Web Worker)
  5.3 Sơ đồ kiến trúc công nghệ     ← SDD §3/§5 (cấu trúc thư mục frontend/backend)
  5.4 Các loại sơ đồ tương tác
    5.4.1 Sequence Diagram              ← SRS §6 (UC-01 chạy mô phỏng, UC-25 trừ tim atomic)
    5.4.2 Activity Diagram              ← SDD §3 (state machine mô phỏng, luồng Practice Ladder)
  5.5 API Endpoints
    5.5.1 Controllers                   ← API_REFERENCE (bảng endpoint chính theo nhóm)
    5.5.2 Services (Business Logic)     ← API_REFERENCE + SDD §5 (bảng 11.4 — 12 service)

PHẦN 6: KIỂM THỬ - TESTING
  6.1 Chiến lược kiểm thử          ← TEST_PLAN §2 (kim tự tháp)
  6.2 Kết quả kiểm thử             ← TEST_PLAN bảng 14.6 (PASS/FAIL số thật, KHÔNG bịa số;
                                      chưa chạy thì ghi "chờ tuần 19-20")
  6.3 Hiệu năng + bảo mật + UX     ← TEST_PLAN (TEST-PERF-001..008, TEST-SEC, SUS)

PHẦN 7: ĐÓNG GÓI & TRIỂN KHAI
  7.1 Đóng gói frontend/backend    ← DEPLOY (lệnh build, docker-compose)
  7.2 Triển khai production        ← DEPLOY (nginx, systemd, biến môi trường)
  7.3 CI/CD + backup               ← DEPLOY §6 (GitHub Actions, backup 14 bản)
  7.4 Runbook sự cố                ← DEPLOY bảng 15.6 (tóm tắt 8 sự cố)

KẾT LUẬN & HƯỚNG PHÁT TRIỂN
  Kết quả đạt được (so KPI G1-G8, tự đánh giá trung thực)   ← tổng hợp
  Hướng phát triển (backlog 16.2: online judge, AI Assistant PoC...) ← SDD §16

TÀI LIỆU THAM KHẢO
  (Vue 3, ASP.NET Core, EF Core, CLRS, VisuAlgo...) — chỉ liệt kê tài liệu THỰC SỰ dùng

PHỤ LỤC A: Hướng dẫn cài đặt môi trường    ← DEPLOY §2-3
PHỤ LỤC B: Phím tắt + thuật ngữ            ← USER_GUIDE + GLOSSARY
PHỤ LỤC C: Thư viện bên thứ ba (license)   ← THIRD_PARTY
PHỤ LỤC D: Danh mục mô phỏng (catalog)     ← shared/simulation-catalog.json
```

### 4.1 Bìa và Ban dự án (thông tin THẬT — bắt buộc, xác nhận 12/08/2026)

- Tên dự án: **Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**
- GVHD: **Phạm Ngọc Ái Liên** · Lớp: **SD21361** · Ngành: Ứng dụng phần mềm (theo mẫu cũ — chờ xác nhận)
- **Ngày khởi động dự án: 12/05/2026** (đã chốt — mọi bảng kế hoạch tính từ mốc này; phát triển kết thúc ~11/08/2026, báo cáo/bảo vệ cuối tháng 8 — xem mốc ở Phần 2.2)
- Ngày bảo vệ: **để trống** (chưa có — người dùng điền sau khi có lịch)

| Mã SV | Họ và tên | Vai trò trong dự án | Nhiệm vụ chính (theo SDD §1.5 + xác nhận 12/08/2026) |
|---|---|---|---|
| TD01287 | Mai Tiểu Bảo | Trưởng nhóm | Backend ASP.NET Core + CSDL (task khó ưu tiên) |
| TD01282 | Thái Quang Sơn | Thành viên | Frontend Vue 3 + giao diện (task khó ưu tiên) |
| TD01131 | Huỳnh Lê Minh Thư | Thành viên | Simulation Engine (code) + Kiểm thử |
| TD01261 | Trần Viết Tâm Phúc | Thành viên | Code hỗ trợ (đơn giản) + Tài liệu + triển khai |

## 5. Quy tắc sinh báo cáo (bắt buộc)

### 5.0A MỨC ĐỘ TRÌNH BÀY — THAM CHIẾU CHUẨN S-CLINIC (BẮT BUỘC đọc trước khi viết)

> Đọc `tailieu/NET202_Project document_6 (1).pdf` (đồ án S-Clinic cùng GVHD — được đánh giá đạt) làm CHUẨN MỰC về độ sâu. Báo cáo DSA-Visual bám theo phong cách đó:

1. **Đặc tả chức năng (Phần 3.3 SRS)**: mỗi chức năng đúng 4 mục ngắn như S-Clinic: `Mô tả chức năng` (2-4 câu nghiệp vụ) / `Dữ liệu liên quan` (tên bảng) / `Đối tượng sử dụng` / `Yêu cầu bảo mật` (1-2 câu). KHÔNG dùng khuôn 7 thuộc tính FR đầy đủ của PRODUCTION_PROMPT trong báo cáo (docs/SRS.md mới là chỗ đặc tả sâu).
2. **CẤM học thuật hóa**: không công thức toán (CẤM kiểu "LCOM4 = số thành phần liên thông trong đồ thị G=(V,E)..." như bản cũ bị chê), không giải thích lý thuyết thuật toán (KHÔNG mô tả bubble sort so sánh thế nào, không trace từng bước, không bảng trạng thái 15 GT), không độ phức tạp dạng công thức — chỉ bảng Big-O tóm tắt nếu cần (bảng, không công thức).
3. **Engine EDV (4.4.2)**: tối đa 2 đoạn: (a) ý tưởng kiến trúc 5-8 dòng ("code thật chạy qua StepExecutor ghi trace, hoạt ảnh = phát lại trace — không hardcode"), (b) 1 khối code trích 5-10 dòng (mẫu như S-Clinic trích 1 entity). KHÔNG liệt kê toàn bộ interface/hợp đồng.
4. **Data dictionary (4.3.2)**: bảng 5 cột đơn giản (Tên cột / Kiểu dữ liệu / Khóa / Bắt buộc / Mô tả chi tiết) — mô tả mỗi cột 1 câu đời thường như S-Clinic. Đủ 32 bảng nhưng mỗi bảng gọn (5-15 dòng); bảng chỉ liệt kê cột quan trọng nhất, không chép nguyên đặc tả SDD.
5. **Thực hiện (Phần 5)**: ảnh UI + mô tả ngắn chức năng từng màn (như S-Clinic 4.1); DB: phương pháp Code-First + 1 ví dụ entity trích + điểm cấu hình Fluent API quan trọng (2-4 bullet); Services: quy trình nghiệp vụ mô tả bằng lời + sơ đồ (như S-Clinic 4.3 "Quy trình đặt lịch...").
6. **Kiểm thử (Phần 6)**: kế hoạch + kịch bản bảng + kết quả THẬT (S-Clinic 5.1-5.4); chưa chạy → ghi "chờ hoàn tất (tuần 19-20)", KHÔNG bịa số.
7. **Kết luận**: 2 mục như S-Clinic: `Kết quả đạt được` + `Khó khăn & Bài học kinh nghiệm` (viết thành thật — bản cũ bị chê chính vì khoe số liệu ảo).
8. **Tổng thể**: nhiều ảnh (UI + sơ đồ), câu văn nghiệp vụ đơn giản, mỗi mục 1-4 trang — nếu 1 mục vượt 4 trang thì đang "đào sâu" → cắt.

0. **BÀI HỌC TỪ BÁO CÁO CŨ BỊ CHÊ (bắt buộc đọc `tailieu/PRO2192_Report (2).docx` trước khi viết — tránh tái phạm)**:
   - CẤM chèn placeholder ảnh kiểu `[KHUNG CHÈN ẢNH THỰC TẾ: ...]` — không có ảnh thì dùng wireframe ASCII, không để khung rỗng.
   - CẤM viết text "tô đẹp" không ăn nhập hệ thống (VD mẫu cũ lạc đề sang SOLID/OOP/Glassmorphism khi hệ không có) — MỌI nội dung phải truy ngược về FR/NFR/UC/file trong docs/, không có nguồn thì KHÔNG viết.
   - CẤM số liệu bịa (VD mẫu cũ "1550+ tests", "22MB RAM" không kiểm chứng) — số nào không đo được ghi "chờ cập nhật sau khi kiểm thử (tuần 19-20)".
   - CẤM đánh số mục lỗi (mẫu cũ list nhảy 3→16→21→25), cấm ký tự lạ (emoji 🕮, từ ngoại lai không nguồn) — mục lục do pandoc --toc tự sinh.
   - Mã SV/avatar/tên thành viên PHẢI đúng (4 thành viên thật, mục 4.1) — không dùng lại danh sách 6 người/PS12345 của bản cũ.

1. Tuyệt đối KHÔNG placeholder: mọi bảng số liệu phải có dữ liệu từ nguồn; thiếu → ghi rõ "chưa cập nhật" kèm thời điểm, không để trống.
2. Mỗi chương ghi nguồn trích xuất dạng chú thích nhỏ: `(nguồn: SRS §3.9)`.
3. Sơ đồ Mermaid giữ nguyên (hội đồng xem được khi render GitHub); Word sẽ hiện dạng code block — chấp nhận, hoặc chuyển thành mô tả + bảng (quyết định: GIỮ Mermaid cho các sơ đồ quan trọng, thêm 1-2 câu giải thích).
4. Số liệu kiểm thử (Chương 4) lấy từ báo cáo thực tế trong TEST_PLAN — nếu chưa chạy test, ghi "chờ hoàn tất kiểm thử (tuần 19-20)".
5. Ảnh: mỗi ảnh đều có caption `Hình X.Y — Tên màn hình (mô tả 1 câu)`; ảnh thiếu → thay bằng wireframe ASCII đã có trong SDD, ghi chú vị trí chụp.

## 6. QUY TRÌNH CHỤP ẢNH MÀN HÌNH (Playwright + Vision model)

### 6.1 Nguyên tắc
- **BẢN ĐẦU TIÊN (đêm nay)**: mọi ảnh dùng **placeholder PNG 1920×1080** xám nhạt có nhãn chữ ở giữa (sinh bằng PowerShell System.Drawing, lưu `tailieu/placeholders/`) — để docx hoàn chỉnh về bố cục, người dùng review được; ảnh thật sau này ghi đè đúng tên file → chạy lại pandoc.
- Chụp bằng Playwright MCP trên **app thật** (dev server chạy + seed data), độ phân giải 1366×768 — sau khi app chạy được.
- Ảnh lưu `tailieu/screenshots/`, tên chuẩn: `NN-ten-man.png` (VD: `05-simulator.png`, `01-home.png`) — theo số Màn trong SCREEN_MAP.
- Sau khi chụp, dùng **qwen2.5vl:3b (Ollama)** để mô tả ảnh (layout, màu, lỗi vỡ layout) → viết caption + kiểm tra nhanh (kết hợp DOM assertions: overflow, broken image, console error).
- Nếu màn hình chưa có (app chưa build xong) → dùng wireframe ASCII trong SDD thay thế + ghi chú "Ảnh chụp khi hoàn thiện UI (tuần X)".

### 6.2 Danh sách ảnh tối thiểu (12 màn — bắt buộc trong báo cáo)

| # | File | Màn | Nguồn đặc tả |
|---|---|---|---|
| 1 | `01-home.png` | Trang chủ (3 demo công khai) | SDD Màn 01 |
| 2 | `02-login.png` | Đăng nhập/Đăng ký | SDD Màn 02 |
| 3 | `04-lesson-detail.png` | Chi tiết bài học | SDD Màn 04 |
| 4 | `05-simulator.png` | **Mô phỏng 3 vùng (màn quan trọng nhất)** | SDD Màn 05 + wireframe 7.8 |
| 5 | `06-exercise.png` | Bài tập trắc nghiệm | SDD Màn 06 |
| 6 | `13-learning-path.png` | Bản đồ Learning Path | SDD Màn 13 |
| 7 | `14-ladder.png` | Practice Ladder (stepper 3 bậc) | SDD Màn 14 |
| 8 | `15-lab.png` | Interactive Lab | SDD Màn 15 |
| 9 | `16-code-runner.png` | Code Runner (Monaco + canvas) | SDD Màn 16 |
| 10 | `17-benchmark.png` | Benchmark Lab (biểu đồ overlay) | SDD Màn 17 |
| 11 | `32-profile.png` | Hồ sơ (tabs) | SDD Màn 32 |
| 12 | `24-leaderboard.png` | Leaderboard / (Shop Màn 22, Quest 23) | SDD Màn 22-24 |

### 6.3 Caption mẫu (vision model sinh, người kiểm duyệt)

```
![Hình 3.1 - Màn hình mô phỏng Bubble Sort](screenshots/05-simulator.png)
*Hình 3.1: Trang mô phỏng hiển thị đồng bộ 3 vùng — mã giả (trái), canvas
trực quan (giữa), giải thích từng bước (phải); thanh điều khiển phát/dừng/bước.
(caption do qwen2.5vl mô tả + rà soát DOM assertions: không overflow, 0 lỗi console)*
```

## 7. Checklist rà soát trước khi build docx

- [ ] Đủ 12 nguồn (mục 3) đã tồn tại và đạt độ dài tối thiểu (17.2).
- [ ] Chương 2 có master matrix đầy đủ; Chương 3 có ERD 2 sơ đồ + ảnh Màn 05.
- [ ] Mọi bảng số liệu có dữ liệu (không ô trống).
- [ ] Pandoc đã cài; `--toc` ra mục lục tự động.
- [ ] File docx mở được bằng Word, ảnh hiển thị đúng (kiểm tra 1 lần thủ công).

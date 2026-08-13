# PROMPT ĐỢT G-PHỤ — 4 điểm thiếu + rà NFR-5 (chạy SAU khi đợt G xong merge dev)

Dán vào `/pm "..." --auto`:

```
Đọc session/HANDOFF_2026-08-12.md + docs/pm-report-d.md (§6 tồn đọng) + docs/pm-report-g.md (nếu có — đợt G đã merge dev) + docs/SETUP_TODO.md + docs/pm-decision-log-d.md.
Kiểm tra trước: đợt G (UX) đã merge vào dev — mọi task dưới đây base từ dev mới nhất, KHÔNG chạy song song lúc G còn chạy.
Nguồn chuẩn: PRODUCTION_PROMPT → SDD → API_REFERENCE → SRS. Phân vai theo .opencode/agent/ (dev-backend/dev-frontend/dev-test/dev-review/dev-docs). Task NHỎ, ghi trạng thái docs/work/<task>.md.

Đợt G-PHỤ — 4 task đóng tồn đọng đã xác định (từng nhánh feature):

1. [dev-backend, feature/feedback-endpoint] POST /lessons/{id}/feedback (LessonDetail.vue:85 đang TODO — nút feedback phía UI không lưu được):
   - Theo API_REFERENCE §4.x (tìm endpoint feedback/ContentFeedback) + SDD §10.2 bảng ContentFeedback; entity đã có sẵn (AppDbContext 32 DbSet — kiểm tra ContentFeedback).
   - Service + controller + DTO + validator đúng chuẩn đợt D (Result/ErrorCodes, 2 project, CẤM Repository). Auth: học viên đã đăng nhập.
   - Verify: dotnet build 0 warning + dotnet test + smoke POST có token (200) / không token (401).
2. [dev-backend, feature/2fa-email] 2FA email (AuthController.cs:123 đang trả 501):
   - Đọc API_REFERENCE phần 2FA + SETUP_TODO §6.5 + SDD Màn N-1. Cần: sinh OTP 6 số, gửi qua SMTP cấu hình (DSA__Email__* — dev dùng MailHog đã có trong docker-compose), endpoint gửi mã + xác nhận mã, lưu OTP có expiry.
   - Nếu SMTP thật chưa có → dùng MailHog (localhost:1025), ghi rõ trong report; KHÔNG giấu.
   - Verify: build + test + smoke luồng gửi mã → nhập mã đúng/sai qua MailHog API.
3. [dev-frontend, feature/lesson-sim-keys] lessons.ts còn 1-2 bài chưa gắn simulation key (vd sliding-window — TODO dòng 240):
   - Rà frontend/src/data/lessons.ts + shared/simulation-catalog.json: bài nào chưa có key khớp → hoặc thêm key hợp lệ, hoặc ghi rõ lý do không có (catalog không có mô phỏng tương ứng — giữ simulations: [] + chú thích).
   - Verify: npm run build + npm test + catalog.spec.ts không vỡ (44 key không đổi).
4. [dev-frontend, feature/breakpoints] Breakpoint theo dòng template (stores/simulation.ts:186 TODO nâng cao):
   - Nếu task quá rộng → phạm vi tối thiểu: UI dừng tại bước có pseudocodeLine trùng dòng người dùng đánh dấu (chỉ toggle + auto-pause, KHÔNG làm trình soạn thảo phức tạp). Nếu không khả thi trong 1 task → SKIP + ghi lý do vào report (SDD cho phép trì hoãn).
5. [dev-test + dev-review] Verify + review toàn bộ: build/test 2 phía, grep cấm sạch, verdict APPROVE/CHANGES REQUESTED.
6. [dev-docs] Rà NFR-5 sau đợt G: npm run build → đo bundle thật → nếu > 500KB gốc (engine hiện ~476KB + shadcn/motion/echarts mới) → nới NFR-5 trong SRS + SDD + TEST_PLAN TEST-PERF-007 (vd ≤ 1.5MB gốc, FCP giữ ≤ 1.5s) + ghi decision log + THIRD_PARTY bổ sung lib mới đợt G (shadcn-vue/Tailwind4/motion-v/GSAP/vue-echarts/Lenis/vue-sonner/lucide...) bằng npm ls thật.
7. [dev-backend + dev-frontend, feature/premium-qr] Premium checkout hiện mã QR chuyển khoản MB Bank (theo yêu cầu USER 12/08 — đã ghi quyết định vào docs/pm-decision-log-gp.md):
   - QUYẾT ĐỊNH ĐÃ CHỐT (không hỏi lại): thông tin TK **NGUYEN THI NHU HOA · MB Bank · STK 83863112088386** (BIN 970422); giá gói giữ nguyên PremiumView.vue (49.000đ/1 tháng, 129.000đ/3 tháng, 399.000đ/12 tháng); **nội dung CK TỰ ĐỘNG** dạng `DSV<UserId>T<months>` (VD DSV1002T3 — không cho khách tự ghi) để map được ai chuyển; **kích hoạt tự động**.
   - Luồng mới: bấm "Thanh toán" (Màn 25) → hiện **mã QR VietQR** + số tiền cố định theo gói + nội dung CK tự sinh (tự copy) → người dùng quét QR bằng app ngân hàng chuyển tiền → bấm "Tôi đã chuyển khoản" (kèm đếm ngược 60s chống bấm nhầm) → **kích hoạt Premium NGAY + log giao dịch** (PremiumSubscriptions + GemTransactions? — dùng bảng Subscription theo SDD §10.2; nếu cần cột riêng cho mã CK/ảnh bằng chứng → migration mới, ghi rõ).
   - Tạo QR: ưu tiên thư viện `qrcode` (npm, MIT) tự sinh VietQR EMVCo (payload: 000201010211 + beneficiary MB 970422 + STK + amount + nội dung + CRC16) — KHÔNG dùng vietqr.io online (phụ thuộc mạng khi demo); kiểm tra QR quét được bằng dev-test.
   - Verify: build + test; smoke: mở /premium → chọn gói → QR xuất hiện → bấm "đã chuyển" → gói active (HeartsMax 30) + log có bản ghi.
8. [dev-docs] Đồng bộ docs theo task 7 (LỆCH CHỦ Ý — bắt buộc): SRS §5.33 UC-32 + FR-10.7 đang ghi "KHÔNG tích hợp cổng thanh toán thật (SePay/VietQR = backlog)" → sửa thành: "checkout hiện QR chuyển khoản MB Bank (NGUYEN THI NHU HOA 83863112088386) + kích hoạt tự động sau khi người dùng xác nhận đã chuyển — KHÔNG gọi API ngân hàng/webhook (vẫn là mô phỏng thanh toán, tăng tính thực tế demo)" + SDD Màn 25 + USER_GUIDE §3.10 + THIRD_PARTY (thêm qrcode) + bump version theo §17.12; ghi decision log với lý do user yêu cầu.
9. [dev-ux + dev-e2e, feature/ux-review2] UI REVIEW VÒNG 2 THEO OLLAMA (bắt buộc — user yêu cầu 12/08, kết quả đợt G mới chỉ "tham chiếu" chưa dùng để sửa):
   - dev-e2e chụp LẠI 12 màn chính (BAO_CAO_SPEC §6.2, light + dark) → gửi Ollama qwen2.5vl:3b với prompt yêu cầu liệt kê ĐIỂM XẤU CỤ THỂ (nền quá sáng/tối, chữ khó đọc, spacing lệch, chồng lấn, thiếu nhất quán) + gợi ý sửa → lưu docs/work/vision-r2-*.txt.
   - dev-ux nhận danh sách nhận xét xấu → SỬA HẾT nhận xét khả thi (tối đa 2 vòng lặp chụp→sửa→chụp) — đặc biệt ưu tiên: độ tương phản (contrast ≥ 4.5:1 cho text), nền quá chói, khoảng cách nhất quán, empty state đẹp.
   - Mọi nhận xét xấu phải có trạng thái cuối: ĐÃ SỬA (ghi file:dòng) hoặc TỪ CHỐI (ghi lý do — vd giữ đúng token SDD).
   - dev-e2e tổng hợp bảng: nhận xét → trạng thái → chứng cứ ảnh trước/sau (docs/work/r2-*.png).

Quy trình: feature/<tên> từ dev → verify → review → merge dev. Commit: backend → bao, frontend → son, engine/test → thu, docs → phuc. Ghi log: docs/pm-report-gp.md + docs/pm-decision-log-gp.md. Việc cần user → SETUP_TODO. --auto
```

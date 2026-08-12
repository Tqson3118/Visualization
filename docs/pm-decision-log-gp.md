# PM DECISION LOG — SESSION G-PHỤ (PROMPT_G_PHU — 4 điểm thiếu + QR MB + UI review v2)

> Ngày: 13/08/2026 · Chế độ: --auto · Trạng thái: docs/pm-report-gp.md · Việc cần user: docs/SETUP_TODO.md

## [2026-08-13] Khởi động SESSION G-PHỤ — phạm vi 9 task, thứ tự tuần tự theo vùng
- Quyết định: Chạy PROMPT_G_PHU sau khi đợt G đã merge dev (xác nhận dev HEAD b4618aa có đủ G). Thứ tự: L1 = T1 feedback (backend) + T3 lessons-keys (frontend) song song; L2 = T2 2FA email (backend, sau T1) + T4 breakpoints (frontend, sau T3) song song; L3 = T7a premium-qr backend; L4 = T7b premium-qr frontend; L5 = T8 docs sync + T9a e2e chụp 12 màn + Ollama (song song); L6 = T9b dev-ux sửa nhận xét; L7 = T5 verify (dev-test) + review (dev-review). Mỗi task = 1 nhánh feature từ dev, merge tuần tự sau verify.
- Ảnh hưởng: 7-8 nhánh feature mới (feedback-endpoint, 2fa-email, lesson-sim-keys, breakpoints, premium-qr, ux-review2 + docs).

## [2026-08-13] T1 — POST /lessons/{id}/feedback
- Quyết định: Entity ContentFeedback ĐÃ CÓ (AppDbContext.cs:33 + migration + config). Triển khai endpoint POST /lessons/{id}/feedback: controller mỏng + ILessonService.AddFeedbackAsync + DTO + validator (Result/ErrorCodes, CẤM Repository), upsert 1 bản ghi (User,Lesson) — lần 2 update. Auth: học viên đăng nhập. Frontend LessonDetail.vue:94 TODO gỡ và gọi endpoint.
- Ảnh hưởng: backend Controllers/Services/Dtos + tests; frontend api/lessons.ts + LessonDetail.vue.

## [2026-08-13] T2 — 2FA email (AuthController.cs:123 trả 501)
- Quyết định: Triển khai 2FA email: PUT /auth/2fa giữ contract API_REFERENCE §4.12. Sinh OTP 6 số, lưu bảng mới UserOtp (hoặc cột Users — ưu tiên migration bảng OtpCodes: UserId, CodeHash, ExpiresAt, Purpose), gửi qua SMTP DSA__Email__* — dev dùng MailHog (localhost:1025 đã trong docker-compose). Endpoint: PUT /auth/2fa (bật/tắt) + POST /auth/2fa/send + POST /auth/2fa/verify (xác nhận). Ghi rõ MailHog trong report nếu SMTP thật chưa cấu hình.
- Ảnh hưởng: backend AuthController + AuthService + migration mới + tests.

## [2026-08-13] T3 — lessons.ts simulation keys (sliding-window TODO:240)
- Quyết định: Rà lessons.ts vs shared/simulation-catalog.json 44 key: sliding-window KHÔNG có key phù hợp trong catalog (chỉ sort/search/tree/heap/hash/graph/linear) → GIỮ simulations: [] + chú thích rõ (đúng seedData.spec.ts:82 quy ước). Kiểm tra các bài khác đều khớp. KHÔNG thêm key không có trong catalog.
- Ảnh hưởng: frontend/src/data/lessons.ts (chú thích), test không đổi.

## [2026-08-13] T4 — Breakpoint theo dòng template (simulation.ts:186)
- Quyết định: Phạm vi tối thiểu theo PROMPT: UI toggle breakpoint trên pseudocode line + auto-pause khi stepExecutor bước có pseudocodeLine == dòng đánh dấu. KHÔNG làm trình soạn thảo phức tạp. Nếu không khả thi trong 1 task → SKIP + ghi lý do (SDD cho phép trì hoãn).
- Ảnh hưởng: frontend/src/stores/simulation.ts + PseudocodePanel.vue + SimulatorView (nút toggle) + test.

## [2026-08-13] T7 — Premium checkout QR MB Bank (theo yêu cầu USER 12/08)
- Quyết định (ĐÃ CHỐT — không hỏi lại): thông tin TK **NGUYEN THI NHU HOA · MB Bank · STK 83863112088386** (BIN 970422); giá giữ nguyên PremiumView.vue (49k/1t, 129k/3t, 399k/12t); nội dung CK TỰ ĐỘNG `DSV<UserId>T<months>` (VD DSV1002T3) để map người chuyển; kích hoạt tự động sau khi bấm "Tôi đã chuyển khoản" (đếm ngược 60s). QR tự sinh VietQR EMVCo bằng thư viện `qrcode` (npm MIT) — KHÔNG dùng vietqr.io online. Log giao dịch PremiumSubscriptions + nếu cần cột mã CK → migration mới ghi rõ.
- Ảnh hưởng: frontend PremiumView + api/premium + backend PremiumController/service (activate sau xác nhận) + migration (nếu cần) + package.json (qrcode).

## [2026-08-13] T8 — Đồng bộ docs theo T7 (LỆCH CHỦ Ý — bắt buộc)
- Quyết định: SRS §5.33 UC-32 + FR-10.7 sửa "KHÔNG tích hợp cổng thanh toán thật" → "checkout hiện QR chuyển khoản MB Bank (NGUYEN THI NHU HOA 83863112088386) + kích hoạt tự động sau xác nhận — KHÔNG gọi API ngân hàng/webhook (mô phỏng thanh toán, tăng tính thực tế demo)". SDD Màn 25 + USER_GUIDE §3.10 + THIRD_PARTY (qrcode) + bump version §17.12.
- Ảnh hưởng: SRS.md, SDD.md, USER_GUIDE.md, THIRD_PARTY.md.

## [2026-08-13] T9 — UI review vòng 2 theo Ollama (bắt buộc)
- Quyết định: dev-e2e chụp LẠI 12 màn light+dark → Ollama qwen2.5vl:3b prompt yêu cầu liệt kê ĐIỂM XẤU CỤ THỂ + gợi ý sửa (lưu docs/work/vision-r2-*.txt) → dev-ux SỬA HẾT nhận xét khả thi (tối đa 2 vòng chụp→sửa→chụp, ưu tiên contrast ≥4.5:1, nền chói, spacing, empty state) → bảng trạng thái ĐÃ SỬA (file:dòng)/TỪ CHỐI (lý do). Kết quả đợt G chỉ "tham chiếu" — vòng này phải dùng để sửa.
- Ảnh hưởnng: frontend views/components (theo nhận xét), docs/work/vision-r2-*.txt, r2-*.png.

## [2026-08-13] Phân công commit + log
- Quyết định: backend → bao, frontend → son, engine/test → thu, docs → phuc (commit-as.ps1). Log: docs/pm-report-gp.md + docs/pm-decision-log-gp.md; việc cần user → SETUP_TODO.
- Ảnh hưởng: git history.

## [2026-08-13] T8 — Đồng bộ docs GP-T7 + đo lại NFR-5 (DONE)
- Quyết định: dev-docs (phuc) trên nhánh `feature/docs-sync-gp` (từ `dev` @ `0543411` — sau khi GP-T7 merge): SRS §3.10B FR-10.7 + §5.33 UC-32 + §1.3.2 — bỏ câu cũ "KHÔNG tích hợp cổng thanh toán thật (SePay/VietQR = backlog)" → mô tả checkout QR chuyển khoản MB Bank (NGUYEN THI NHU HOA · 83863112088386, BIN 970422) + nội dung CK tự động `DSV{userId}T{months}` + kích hoạt tự động sau xác nhận (đếm ngược 60s) — KHÔNG gọi API ngân hàng/webhook (mô phỏng thanh toán, tăng tính thực tế demo); SDD Màn 25/26 + §7.3.28 (OrderRef DSV) + §3.9 bundle; USER_GUIDE §3.10 (quét QR bằng app ngân hàng → chờ 60s → "Tôi đã chuyển khoản"); API_REFERENCE §4.14 (contentRef + OrderRef DSV + ví dụ response) + §8; THIRD_PARTY +`qrcode`/`@types/qrcode` (MIT — npm ls + node_modules package.json đã xác nhận); TEST_PLAN §10 số thật (BE 81 unit + 31 integration, FE 89 unit + 13 e2e → tổng 214) + TEST-PERF-007; version bump từng file.
- **NFR-5 đo lại (npm run build frontend/ — 13/08/2026, sau khi thêm qrcode):** JS gốc tải lần đầu ≈ `852 KB` (KHÔNG đổi so với đợt G — `qrcode` nằm trong chunk lazy `PremiumView` 32 KB gốc / 12.5 KB gzip, không vào preload); engine `476 KB` gốc / `120 KB` gzip (≤ 500KB ✓); echarts `324 KB` gốc / `110 KB` gzip (lazy); tổng JS gốc toàn dist ≈ `2.08 MB` (trước 1.95MB — +qrcode ~32KB lazy). **KHÔNG vượt ngưỡng → KHÔNG nới NFR-5** (giữ: JS gốc tải lần đầu ≤ 1.5MB, engine ≤ 500KB gốc) — ghi số thật tại SRS §4.1 NFR-5 / SDD §3.9 / TEST_PLAN TEST-PERF-007.
- Ảnh hưởng: SRS.md, SDD.md, USER_GUIDE.md, THIRD_PARTY.md (root), API_REFERENCE.md, TEST_PLAN.md, pm-decision-log-gp.md, docs/work/gp-t8.md. Commit docs → phuc (commit-as.ps1).

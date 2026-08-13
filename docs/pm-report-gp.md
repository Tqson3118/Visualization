# PM REPORT — SESSION G-PHỤ (4 điểm thiếu + QR MB Bank + UI review v2 — PROMPT_G_PHU)

> Ngày: 13/08/2026 · Chế độ: --auto · Quyết định: docs/pm-decision-log-gp.md · Việc cần user: docs/SETUP_TODO.md

## 1. Mục tiêu
Đợt G-PHỤ theo session/PROMPT_G_PHU.md — chạy SAU đợt G (đã merge dev): (1) feedback endpoint, (2) 2FA email, (3) lesson simulation keys, (4) breakpoint theo dòng, (5) verify + review, (6) rà NFR-5, (7) Premium checkout QR MB Bank (yêu cầu USER 12/08), (8) đồng bộ docs, (9) UI review vòng 2 theo Ollama. Nguồn chuẩn: PRODUCTION_PROMPT → SDD → API_REFERENCE → SRS.

## 2. Trạng thái task

| Task | Nội dung | Nhánh | Agent | Kết quả | Verify |
|---|---|---|---|---|---|
| GP-T1 | POST /lessons/{id}/feedback: DTO + validator + upsert ContentFeedback (1 bản ghi, lần 2 update) + FE LessonDetail gọi + toast | feature/feedback-endpoint | dev-backend | **DONE** | BE 65 test · FE 78 · smoke 200/400/401/DB 1 bản ghi |
| GP-T2 | 2FA email: OtpCodes (migration) + PUT /auth/2fa + POST /auth/2fa/send + /verify (OTP 6 số SHA256, 5 phút, SmtpClient MailHog) — gỡ 501 | feature/2fa-email | dev-backend | **DONE** | BE 77 test · smoke MailHog nhận mã thật · verify đúng/sai 200/400 |
| GP-T3 | lessons.ts 13 bài rà với catalog 44 key: 100% khớp; sliding-window giữ [] + chú thích (catalog không có key phù hợp) | feature/lesson-sim-keys | dev-frontend | **DONE** | FE 78 · e2e 13/13 · MISSING=[] |
| GP-T4 | Breakpoint theo dòng pseudocode: toggle chấm tròn + auto-pause khi pseudocodeLine ∈ breakpoints (KHÔNG đụng engine) | feature/breakpoints | dev-frontend | **DONE** (khả thi — có bằng chứng interface) | FE 82 · e2e 13/13 · smoke dừng đúng dòng 5 |
| GP-T7 | Premium QR MB Bank: OrderRef DSV{userId}T{months} + contentRef (BE) · VietQR EMVCo tự sinh (lib qrcode, CRC16) + QR canvas + copy CK + countdown 60s + kích hoạt tự động (FE) — fix planId 1m/3m/12m | feature/premium-qr | dev-backend + dev-frontend | **DONE** | BE 112 (81+31) · FE 89 · e2e 13/13 · smoke QR 40% đậm + HeartsMax=30 + OrderRef DSV3T1 |
| GP-T8 | Docs sync: SRS UC-32/FR-10.7 QR MB Bank, SDD Màn 25/26, USER_GUIDE §3.10, API_REFERENCE §4.14, THIRD_PARTY +qrcode MIT, TEST_PLAN số thật 214; NFR-5 đo lại 852KB — KHÔNG nới | feature/docs-sync-gp | dev-docs | **DONE** | grep hết câu cũ · THIRD_PARTY 0 ô trống · bundle 852KB ≤ 1.5MB |
| GP-T9a | UI review v2: chụp 24 ảnh (12 light+12 dark) → Ollama liệt kê điểm xấu → **39 nhận xét (11 P1 + 28 P2)** | — (chỉ báo) | dev-e2e | **DONE** | 24/24 màn 0 console error · 0 overflow |
| GP-T9b | Sửa nhận xét: **23 ĐÃ SỬA + 16 TỪ CHỐI (đo thật: contrast 4.68–10.66:1, overflow 0, header 9.48–10.66:1)** | feature/ux-review2 | dev-ux | **DONE** | FE 89 · e2e 13/13 · smoke 24/24 sạch |
| GP-T5 | Verify độc lập toàn đợt + review | — | dev-test + dev-review | **PASS 9/9 · VERDICT APPROVE** | FE 89 + 13 e2e · BE 81 + 31 · grep cấm 0 · secret 0 · smoke 6 luồng mới OK |

**Tổng: 9/9 DONE. Verdict APPROVE (0 P1/P2; 5 P3 ghi chú).** 8 merge vào dev + push origin.

## 3. Tính năng mới (đợt G-PHỤ)
- **Feedback bài học**: POST /lessons/{id}/feedback {rating 1-5, comment ≤1000} — upsert 1 bản ghi (User,Lesson), validator FluentValidation, FE gọi + toast.
- **2FA email**: bảng OtpCodes (SHA256 hash, 5 phút, purpose, used); PUT /auth/2fa {enabled} (true → OTP_REQUIRED qua send+verify), POST /auth/2fa/send (SMTP — dev MailHog), POST /auth/2fa/verify {code} → 200 enabled:true. Error codes: OTP_REQUIRED/INVALID/EXPIRED/USED/TWO_FA_ALREADY_ENABLED. **SMTP thật khi deploy → SETUP_TODO §10.1; luồng chặn login bước 2 → SETUP_TODO §10.2.**
- **Breakpoint simulator**: chấm tròn toggle trên từng dòng pseudocode; auto-pause khi play/step chạm bước có pseudocodeLine trùng; badge "Đã dừng tại breakpoint dòng N"; reset/xóa hit đúng.
- **Premium QR MB Bank** (yêu cầu USER 12/08): TK **NGUYEN THI NHU HOA · MB Bank · 83863112088386** (BIN 970422); giá giữ nguyên (49k/129k/399k); nội dung CK tự động `DSV<UserId>T<months>` (copy 1 chạm); QR VietQR EMVCo tự sinh bằng `qrcode` (MIT) — payload `00020101021152069704...6304<CRC16>`; nút "Tôi đã chuyển khoản" sau 60s đếm ngược → kích hoạt NGAY (HeartsMax 30) + log PremiumSubscriptions OrderRef=DSV...; KHÔNG API ngân hàng/webhook (mô phỏng thực tế demo).
- **UI review v2**: 23 sửa (gradient tối đi đạt 4.68–8.23:1, primary #007E72 4.97:1, text-muted #56706D 5.12:1, editor theme-aware, empty state icon contrast, hero title to hơn, spacing đồng bộ...) + 16 từ chối có số đo.

## 4. Verify tổng thể (GP-T5 — lệnh thật)
| Lệnh | Kết quả |
|---|---|
| npm run build (frontend) | PASS 0 lỗi |
| npm test (frontend) | PASS — 89/89 |
| npx playwright test | PASS — 13/13 |
| dotnet build DsaVisual.sln | PASS 0 warning / 0 error |
| dotnet test backend | PASS — Unit 81/81 + Integration 31/31 (Testcontainers) |
| Grep cấm (PostgreSQL/MediatR/Repository/Judge0/secret) | 0 match production |
| Smoke thật | feedback 200/401/400 · 2FA send→MailHog→verify 200/400 · premium QR→60s→HeartsMax 30 · breakpoint dừng đúng · 24/24 màn light+dark sạch |
| Secret scan | sạch |

## 5. Quyết định / lệch chủ ý (chi tiết docs/pm-decision-log-gp.md)
- T7 QR MB Bank theo yêu cầu USER — quyết định đã chốt, không hỏi lại; OrderRef đổi MOCK-{guid} → DSV{userId}T{months}; planId FE gửi sai monthly→400 (fix 1m/3m/12m).
- T4 breakpoint KHẢ THI (engine Step có pseudocodeLine — không phải SKIP).
- T8: NFR-5 giữ nguyên (bundle thật 852KB JS gốc đầu, engine 476KB ≤ 500KB — qrcode vào chunk lazy 32KB).
- P3 ghi nhận (không chặn): log OTP gate theo environment khi SMTP thiếu; thiếu rate-limit /auth/2fa/verify; comment Status=2; TLV length <100 byte (vietqr.ts); setTimeout chưa clear khi unmount PremiumView.

## 6. Việc còn lại / cần user (docs/SETUP_TODO.md)
- SMTP thật cho 2FA khi deploy (SETUP_TODO §10.1) + luồng chặn login bước 2 khi thiếu mã (sai 3 lần khóa 10p, nhớ thiết bị 30 ngày — §10.2, task backend riêng).
- Rate-limit /auth/2fa/verify (P3).
- Các việc cũ từ đợt G: merge feature/final-review → main (chờ user), xoay key DEEPSEEK, đổi mật khẩu seed trước demo.
- Cài Monaco full editor (nếu cần) — hiện textarea + theme-aware.

## 7. Kết luận
Đợt G-PHỤ hoàn thành trọn vẹn 9/9 task: feedback + 2FA email + breakpoint + Premium QR MB Bank + docs đồng bộ + UI review v2 (39 nhận xét xử lý hết) + verify 9/9 PASS + verdict APPROVE. Tất cả merged dev + push origin (HEAD 285d894).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.

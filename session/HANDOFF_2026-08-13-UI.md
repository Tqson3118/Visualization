# HANDOFF — 13/08/2026 (Session UI/UX Review — bắt đầu từ đây)

> File này để phiên UI/UX REVIEW tiếp theo nắm ngữ cảnh. Đọc kỹ trước khi làm. Nguồn chuẩn: docs/PRODUCTION_PROMPT.md — KHÔNG đoán.

## 1. DỰ ÁN
- Đồ án tốt nghiệp FPT (GVHD Phạm Ngọc Ái Liên, Lớp SD21361, 4 thành viên): **DSA-Visual** — hệ thống hỗ trợ học tập + trực quan hóa CTDL & giải thuật.
- Nhóm: Mai Tiểu Bảo TD01287 (backend) · Thái Quang Sơn TD01282 (frontend) · Huỳnh Lê Minh Thư TD01131 (engine+test) · Trần Viết Tâm Phúc TD01261 (tài liệu).
- Mốc: khởi động 12/05/2026, phát triển hết 11/08/2026, bảo vệ cuối tháng 8. **CẤM ghi "20 tuần/16 tuần" — chỉ 13 tuần.**

## 2. TRẠNG THÁI ĐẾN 13/08 (kiểm chứng thật)
- ✅ Docs 12/12 + báo cáo Word (BAO_CAO.md + docx) + git: `main` (ổn định, đứng tại 62493bd — CHƯA merge dev→main) + `dev` (tích hợp đầy đủ D/E/F/G).
- ✅ Đợt D: backend services/controllers/DTO/migration 32 bảng + seed thật + 44 generator + 33 view. Verify: FE build + 72/72 test, BE build 0 warning + 44 unit + 27 integration, e2e 11/11.
- ✅ Đợt E: engine Web Worker (runMeasure) + renderer canvas thật (SDD §8.3) + Playwright e2e + integration tests (Testcontainers).
- ✅ Đợt F: THIRD_PARTY phiên bản thật + checklist §17.9 + TEST_PLAN số liệu thật.
- ✅ Đợt G (UX polish): stack mới **tailwindcss 4 + shadcn-vue + motion-v + GSAP + vue-echarts + Lenis + vue-sonner + lucide/phosphor + font Geist/JetBrains Mono** — đã merge dev. G-F3D: 12 màn chính e2e — **11 PASS / 1 CÓ LỖI** (leaderboard) → đã fix G-F3E (P1 crash tab Level + P2 classId) + G-F3E2 (phân trang classId).
- ✅ Bug canvas ResizeObserver phình 10.535px → đã fix (ba62a33, canvas ổn định ~418-548px).
- ✅ 9 PR ma base `main` trên GitHub → **đã đóng 8** (chỉ còn PR #1 `dev → main`).
- ⏳ Đang chạy: đợt G phụ (PROMPT_G_PHU.md — feedback endpoint, 2FA, lesson-sim-keys, breakpoints, NFR-5, **premium QR MB Bank**, **task 9: UI review vòng 2 theo Ollama**).

## 3. VIỆC CỦA SESSION NÀY (đợt H — REVIEW + NÂNG CẤP UX/UI TOÀN BỘ)
1. **Đợi/kiểm tra đợt G phụ xong merge dev** (feedback/2FA/QR/ux-review2) — base từ dev mới nhất.
2. **Review + nâng cấp TOÀN BỘ 48 màn** (32 màn chuẩn + 16 màn phụ N-1..N-16; 38 route/36 view thực tế): bố cục, spacing, contrast, empty state, loading, responsive, dark/light, nhất quán token (teal #0D9488, OKLCH), chuẩn shadcn + motion-v micro-interaction. Prompt: `session/PROMPT_H_UI_REVIEW.md`.
3. **Ưu tiên bắt buộc**: 12 màn báo cáo (BAO_CAO_SPEC §6.2: home, login, lesson, simulator, exercise, path, ladder, lab, code-runner, benchmark, leaderboard, profile) phải đẹp nhất — sau đó admin×5, classes×3, shop, quests, premium+QR, cheatsheet, help, privacy, 404.
4. Vòng lặp bắt buộc: dev-ux sửa → dev-e2e chụp ảnh (light+dark) → Ollama qwen2.5vl:3b nhận xét (điểm XẤU cụ thể) → sửa → tối đa 2 vòng. Mọi nhận xét xấu có trạng thái ĐÃ SỬA/TỪ CHỐI (file `docs/work/`).

## 4. QUY TRÌNH (bắt buộc)
- Đội hình agent: pm · dev · dev-backend · dev-frontend · dev-engine · dev-ux · dev-test · dev-e2e (Playwright + Ollama qwen2.5vl:3b @ localhost:11434 — **model chính KHÔNG đọc ảnh, dùng Ollama để mô tả**) · dev-review · dev-docs. Task NHỎ (1 module/lần, fresh context), trạng thái ghi `docs/work/<task>.md`.
- Git: feature/<tên> từ `dev` → verify → review → merge dev. **KHI MỞ PR: base `dev` (KHÔNG main)** — 8 PR ma đã đóng vì lỗi này. Commit-as: backend→bao, frontend/UX→son, engine/test→thu, docs→phuc.
- Verify: FE `npm run build` + `npm test`; BE `dotnet build` (0 warning) + `dotnet test`; e2e `npx playwright test`; smoke browser console 0 lỗi; grep cấm sạch (PostgreSQL/MediatR/Repository/secret).
- Môi trường: backend :5000 (SQL Server Docker, seed `student@demo.local`/`Student@123`), frontend :5174 (5173 bị relay Docker chiếm), pandoc tại `C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe`.

## 5. VIỆC CÒN LẠI SAU ĐỢT H
- Merge dev→main (PR #1) sau khi user duyệt.
- 18 ảnh báo cáo thật: 6 sơ đồ (prompt `session/PROMPT_USECASE_UPGRADE.md` — SVG tay + Playwright render) + 12 màn UI (chụp từ app thật) → build lại docx pandoc.
- Điền ngày bảo vệ/ngành bìa (SETUP_TODO mục 1-2 — cần user).
- 2FA cần SMTP thật (đang MailHog); premium QR đã ghi vào G phụ task 7-8.

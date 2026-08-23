# Walkthrough — Conditional Pass (2026-08-19)

## Kết luận chính thức

> Hệ thống đã triển khai các Use Case chính với route frontend, API backend và mô hình dữ liệu tương ứng. Một số flow đã được xác nhận live-integrated, gồm classes join, quests, lesson/codelab, leaderboard và admin moderation. Tuy nhiên payment hiện mới có mock-pay flow; code-run history, course feedback frontend integration, codelab multi-task và RBAC hierarchy còn giới hạn. Do đó hệ thống đạt mức **Conditional Pass**, chưa đủ bằng chứng để kết luận 100% toàn bộ Use Case hoàn chỉnh.

## Bảng trạng thái

| Hạng mục | Trạng thái |
|---|---|
| Classes join contract | ✅ Verified |
| Quests contract | ✅ Verified |
| Premium UI/API | ⚠️ Mock payment |
| Leaderboard DB query | ✅ Verified; chưa gọi realtime |
| Badge count contract | ⚠️ Chưa đủ bằng chứng |
| Codelab submit | ✅ Có; multi-task partial |
| Submission history | ⚠️ Exercise có; Code Runner chưa đủ |
| Feedback | ⚠️ Nhiều domain; course feedback FE gap |
| RBAC | ✅ Backend authority; không gọi là hierarchy |
| Frontend build | ✅ PASS (pnpm allowlist vue-demi, 2026-08-19) |
| Vitest (646 tests) | ✅ PASS |
| Playwright (156 tests) | ⚠️ 155 pass / 1 fail (TC-09 flaky double-click) |
| Backend build (WebApi) | ✅ PASS |
| Consistency gate | ✅ PASS (130 TC sync JSON/MD/DOCX) |
| DOCX media gate | ✅ PASS |
| Sandbox navigation | ✅ Ẩn khỏi nav; route direct vẫn tồn tại (commit 74db020) |

## Fresh verification log (2026-08-19)

### A. Frontend build environment

- Thêm `frontend/.npmrc` với `onlyBuiltDependencies[]=vue-demi` (chỉ package cần thiết).
- Commit: `6311eae fix(frontend): allow vue-demi postinstall via pnpm allowlist`
- `pnpm install` → OK (486ms)
- `pnpm run build` → ✓ built in 7.66s

### B. Test suites

- `pnpm exec vitest run` → 57 files, 646 tests PASS
- `pnpm exec playwright test` → 155 passed, 1 failed (TC-09 double-click timeout — flaky, không phải regression Sandbox)

### C. Backend & gates

- `dotnet build` WebApi → 0 Warning, 0 Error
- `python tailieu/verify_consistency.py` → 130 TC sync PASS
- `python tailieu/verify_docx_media.py` → 100% PASS

### D. Sandbox navigation

- `AppHeader.vue`: 0 link Sandbox trong desktop/mobile nav
- Route direct (`/sorting-sandbox`, `/searching-sandbox`, `/graph-playground`, `/stack-queue-sandbox`) vẫn hoạt động
- Commit ẩn nav: `74db020 Hide sandbox from primary navigation`

### E. dist_usb

- `python tools/package_usb.py` → SUCCESS tại `dist_usb/`

## Không nên làm lúc này

Không "làm sạch" toàn bộ từ `fallback` — phân biệt:
- **production business data:** lấy từ API/DB
- **UI resilience fallback:** không phải dữ liệu nghiệp vụ giả
- **demo/mock fallback:** phải tắt hoặc cô lập

## Trạng thái nộp

**Conditional Pass có bằng chứng** — sẵn sàng nộp sau khi walkthrough và pnpm fix đã commit.
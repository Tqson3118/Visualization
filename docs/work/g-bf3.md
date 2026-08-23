# G-BF3 — FIX TEST-INFRA E2E PLAYWRIGHT: MOCK `/auth/refresh` (dev-e2e, 12/08/2026)

- **Task**: G-BF3 — sửa test-infra e2e playwright sau khi G-BF2 thêm `auth.refresh()` lúc boot (src/main.ts)
- **Ngày sửa**: 12/08/2026
- **Người sửa**: dev-e2e (Thai Quang Son)
- **Nhánh**: feature/ux-e2e-fix (tạo từ dev)
- **Kết quả**: `npx playwright test` **11/11 PASS** · `npm run build` **0 lỗi** · `npm test` **72/72 PASS**

## Vấn đề

- G-BF2 (feature/ux-bugfix-frontend) đổi boot app: `await auth.refresh()` TRƯỚC khi router guard chạy
  (bug #8 — mất phiên khi reload, ADR-004 cookie HttpOnly).
- `helpers/mockApi.ts` xử lý `POST /api/v1/auth/refresh` **luôn trả 200** → mỗi `page.goto()` app boot coi
  như đã authenticated (status='authenticated' + fetchMe) → guard `guestOnly` đá `/login` → `/home`,
  guard `requiresAuth` KHÔNG redirect `/login` → 8 spec auth/ladder/code-runner fail (timeout `#email`).
- Chỉ 3 spec simulator (demo key công khai `sort.bubble`, không cần login) pass → tổng 11 spec, fail 8.

## File đã sửa

| # | File | Thay đổi |
|---|---|---|
| 1 | `frontend/tests/e2e/helpers/mockApi.ts` | Thêm state `isAuthenticated` (mặc định `false`, mỗi test page mới); `POST /auth/login` + `/auth/register` → set `true`; `POST /auth/logout` → set `false`. `POST /auth/refresh`: chưa login → **401** `ApiErrorBody` (`UNAUTHORIZED`), đã login → **200** `{ accessToken, refreshToken, expiresIn, user }` |

## Lý do fix đúng chỗ

- App chỉ gọi refresh ở **2 nơi**: `src/main.ts` boot (chính) và `src/api/client.ts` response interceptor
  khi gặp 401 (url `/auth/` bị loại trừ khỏi vòng lặp refresh-retry → 401 không gây loop).
- `AuthStore.refresh()` chỉ đọc `response.accessToken`; body 200 giữ shape `RefreshResponse` (+ user).
- State `isAuthenticated` nằm trong closure của `mockApi(page)` → độc lập giữa các test, không rò rỉ.
- Sửa mỗi `tests/` + helpers — KHÔNG đổi spec logic, KHÔNG đổi app code.

## Kết quả verify (chạy 12/08/2026)

### E2E
- `npx playwright test` → **11/11 PASS** (auth 3 · ladder 2 · code-runner 3 · simulator 3) ✔
  - auth: register→/path, login→/path, guard /profile→/login?redirect=→quay lại /profile
  - ladder: guard /ladder/1 + stepper 3 bậc; FR-10.1 trừ đúng 1 tim (hearts 10→9)
  - code-runner: guard → editor + code mẫu; chạy code → Thành công; FR-9.6 ghi nhận hiện tại

### Build + unit
- `npm run build` (vue-tsc -b && vite build) → **0 lỗi, exit 0** ✔
- `npm test` (vitest run) → **72/72 PASS** (8 files) ✔

## Ghi chú
- Root cause nằm ở mock (backend không có cookie refresh token → phải 401), không phải bug app —
  G-BF2 đã verify smoke thật với backend docker (refresh 200 khi có cookie hợp lệ).
- `simulator.spec` không bị ảnh hưởng: demo key công khai, refresh 401 → status='error' → vẫn render.
- Không merge nhánh này vào dev; commit bằng commit-as.ps1 (danh tính thành viên).

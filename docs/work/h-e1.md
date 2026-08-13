# H-E1 — Polish 3 màn Auth phụ (register / forgot-password / reset-password)

Ngày: 2026-08-13 · Nhánh: `feature/ux-h-e1` · Từ: `dev` (HEAD 10b841a) · Commit: `07e7bdf`

## Phạm vi

3 view auth phụ chưa polish của nhóm E1 (AUTH PHỤ) + i18n (chỉ THÊM chuỗi mới, không sửa/xóa cũ):

| File | Màn | Nâng cấp |
|---|---|---|
| `frontend/src/views/RegisterView.vue` | Đăng ký | **Split layout đồng bộ LoginView** (brand aside gradient Aurora + dark scrim + 3 feature points; form card transparent, title `text-gradient-aurora`); Input thêm icon (user/mail/lock/lock); checklist mật khẩu đổi **grid 2 cột** (gọn chiều cao form 1366×768) — dấu ✓ success + text `--color-primary` (≥4.5:1 cả 2 theme); checkbox dùng `accent-color` token + focus-visible; submit error boxed như LoginView; pending teacher: panel success icon shield (không còn emoji 🎉); motion-v `Motion` fade-up 0.32s. **Logic validate/submit/redirect GIỮ NGUYÊN. Selector e2e giữ: `form.register__card`, 4 `.ui-input input`, `label.register__row` (Đồng ý), nút "Đăng ký".** |
| `frontend/src/views/ForgotPasswordView.vue` | Quên mật khẩu | Split layout đồng bộ LoginView; Input email icon mail; **toast success (vue-sonner)** sau khi gửi; state sent: panel icon mail tròn success + title/desc/hint + link về login (thay chuỗi emoji 📧). Logic gọi API + state sent GIỮ NGUYÊN. |
| `frontend/src/views/ResetPasswordView.vue` | Đặt lại mật khẩu | Split layout đồng bộ LoginView; Input icon key/lock; checklist grid 2 cột; error boxed; **toast success**; state success: panel icon check-circle tròn (thay ✅). Logic validate/token/redirect 2s GIỮ NGUYÊN. |
| `frontend/src/i18n/vi.ts` | — | THÊM `register.*` (18 key + checklist 5), `forgot.*` (12 key), `reset.*` (13 key + checklist 4). KHÔNG xóa/sửa chuỗi cũ (auth.* giữ nguyên). |

## Verify (số thật)

- `npm run build` (vue-tsc + vite) → **0 lỗi** (1.33–1.53s).
- `npm test` → **89/89 PASS** (11 files, 2.3s).
- `npm run lint` — repo **không có** script lint (package.json: dev/build/preview/test/test:e2e).
- Playwright:
  - **auth.spec.ts → 3/3 PASS** (register → /path với selector cũ; login; guard redirect) — không phá e2e hiện có.
  - **Render check tạm (spec đã xóa sau chạy) → 15/15 PASS**: 3 màn × light+dark @1366×768 + @390×844 → `overflow 0`, `console error 0` (ngoại trừ 401 `/auth/refresh` lúc boot — hành vi **có chủ đích** của `mockApi` dòng 343-348, xuất hiện trên mọi trang guest, không phải lỗi view); visual check: aside `linear-gradient` present, title `background-clip:text`, shell nằm gọn viewport (top ≥ 0, bottom ≤ vh).
  - Ảnh: `docs/work/h-e1-{register,forgot,reset}-{light,mobile}.png`.

## Ghi chú / rủi ro

- **Contrast**: checklist ok dùng `--color-primary` (#007E72 light ≈5.2:1 / #2DD4BF dark ≈6.7:1) thay vì success raw (#0FA968 light ≈2.5:1 — fail AA); dấu ✓ giữ success làm điểm nhấn (icon không yêu cầu 4.5:1). Aside gradient + scrim dark giữ nguyên công thức LoginView (GP-T9b #8).
- **Motion-v**: dùng `Motion` component (fade-up 0.32s) — build type-check pass; element có mặt trong DOM ngay khi mount nên không ảnh hưởng click/selector Playwright.
- **Bug UI pre-existing** (ghi chú cuối auth.spec.ts, ngoài phạm vi): `validate()` dùng `Object.assign(fieldErrors, errors)` không xóa key cũ → stale error hiển thị tới khi có key mới ghi đè. KHÔNG sửa để giữ nguyên hành vi e2e đã probe — đề xuất task sau.
- Chưa push/merge — chờ vòng Ollama review của PM (dev-e2e chụp ảnh + qwen2.5vl theo khung 7 tiêu chí).

## Đề xuất bước sau (không thực hiện)

- LoginView hiện chưa dùng motion-v (3 view này đã dùng) — nếu muốn đồng bộ 100% có thể thêm fade-up cho LoginView đợt sau.
- Màn `NotFoundView`/`PlaceholderView` vẫn là ứng viên polish đợt sau (chưa thuộc nhóm E1).

---

# H-E1 Fix (vòng 1/2) — P1 primary-foreground contrast AA toàn app + P3 aside badge

Ngày: 2026-08-13 · Nhánh: `feature/ux-h-e1` · Commit: (xem git log)

## Root cause (KHÁC chẩn đoán ban đầu)

- Chẩn đoán cũ: "`@theme inline` ở `tailwind.css:137` không sinh được utility `text-primary-foreground` (0 rule trong CSS build)" → **SAI**. Build mới xác nhận: `.text-primary-foreground{color:var(--primary-foreground)}` CÓ trong `dist/assets/index-*.css`; token `--primary-foreground` đúng (light `oklch(0.985 0 0)` ≈ trắng / dark `oklch(0.22 0.06 186)` ≈ teal đen). Probe cũ đo trên dev server build cũ → 0 rule.
- **Root cause thật**: `frontend/src/styles/global.css:36-42` — rule reset **unlayered** `button, input, select, textarea { font: inherit; color: inherit; }` (tải SAU tailwind.css). Theo spec Cascade Layers, style **unlayered thắng MỌI @layer** của Tailwind (kể cả `@layer utilities`, bất kể specificity) → mọi nút shadcn (`bg-primary text-primary-foreground`, `bg-destructive text-destructive-foreground`, `bg-secondary text-secondary-foreground`…) bị `color: inherit` → chữ = `--foreground` (#134E4A light / #CCFBF1 dark) trên nền primary (#007E72 / #26BDAE) → **1.91:1 light / 2.09:1 dark** (P1). Legacy `.btn-primary` không bị (có `color: var(--color-on-primary)` unlayered specificity cao hơn).

## Fix

| File | Trước | Sau |
|---|---|---|
| `frontend/src/styles/global.css:36-42` | rule form-control reset **unlayered** | bọc trong `@layer base` — Tailwind preflight vốn đã có rule tương đương trong base layer nên element KHÔNG có color utility không đổi hành vi; utility `text-*-foreground` (utilities layer) giờ thắng đúng cascade → sửa **mọi variant** (primary/destructive/secondary) một gốc |
| `frontend/src/views/RegisterView.vue:312` | `background: rgba(255,255,255,0.22)` (aside-badge) | `0.35` (P3) |
| `frontend/src/views/ForgotPasswordView.vue:192` | ditto | `0.35` (P3) |
| `frontend/src/views/ResetPasswordView.vue:229` | ditto | `0.35` (P3) |

KHÔNG đổi token: `--primary-foreground` (tailwind.css:56/94) và `--color-on-primary` (tokens.css:13/90) đã đúng — không cần chỉnh.

## Contrast đo được (computed style thực tế, Playwright canvas-composite, WCAG)

| Element | Light | Dark | Yêu cầu |
|---|---|---|---|
| Nút primary (submit login/register/forgot/reset + /path + /learn) | **4.76:1** | **7.12:1** | ≥ 4.5:1 ✓ |
| Nút destructive (inject test, cùng cascade) | 4.57:1 | 3.65:1 | ≥ 4.5:1 — dark chưa đạt (token `--destructive` dark = #F87171 + chữ trắng, thiết kế có chủ đích của shadcn default, **ngoài phạm vi P1 primary** — đề xuất đợt sau) |
| Nút secondary | 5.75:1 | 11.3:1 | ✓ |
| Badge aside (0.35) | cải thiện so với 0.22 (3.93→4.13 trước) | ditto | P3 ✓ (chữ trắng trên nền trắng 35% + gradient + border 45%) |

Trước fix: 1.91:1 (light) / 2.09:1 (dark) → Sau fix: **4.76:1 / 7.12:1**.

## Verify (số thật)

- `npm run build` → **0 lỗi** (1.26s).
- `npm test` → **89/89 PASS** (11 files).
- `npx playwright test` → **13/13 PASS** (24.8s) — auth register/login/guard + code-runner + ladder + leaderboard + simulator không vỡ.
- Grep CSS build: `.text-primary-foreground`, `.text-destructive-foreground`, `.text-secondary-foreground` đều có 1 rule `color: var(--…)`.
- Render /register, /forgot-password, /reset-password, /login, /path, /learn × light+dark @1366×768: **overflow 0/0**, console error chỉ còn `401 /api/v1/auth/refresh` lúc boot guest (hành vi có chủ đích — mockApi/auth.refresh, backend không chạy trong env này; 404 lẻ trước đó là race HMR dev server, không tái lập, không có trong lần đo lại).
- Lint: repo không có script lint (như cũ).

## Ghi chú / rủi ro

- Fix ở global.css thay vì fallback `button.bg-primary` (đề xuất trong task) vì fallback chỉ vá primary — destructive/secondary vẫn vỡ. `@layer base` giữ nguyên hành vi cho element không có color utility (preflight tương đương) — verified bằng build + e2e + đo contrast.
- **LoginView badge vẫn 0.22** (ngoài phạm vi task chỉ định 3 view) — nếu muốn đồng bộ, đợt sau đổi 1 dòng `login__aside-badge`.
- **Destructive dark 3.65:1** — pre-existing (token `--destructive` #F87171 + `--destructive-foreground` trắng, đúng shadcn default); không thuộc P1 primary; đề xuất task riêng (tối `--destructive` dark hoặc đổi foreground).
- Không push/merge — chờ PM review vòng 2.

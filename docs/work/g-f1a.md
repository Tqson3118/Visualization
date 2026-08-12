# G-F1a — Phase 1a ux-foundation: Tailwind 4 + shadcn-vue + Fonts + OKLCH tokens + dark mode

> Ngày: 12/08/2026 · Nhánh: `feature/ux-foundation` (tạo từ `dev`) · Commit danh tính: **son**
> Phạm vi: stack nền tảng UI/UX — KHÔNG đụng canvas simulator, KHÔNG đổi view/business logic.

## Mục tiêu
Cài nền tảng để Phase 1b/2 có thể: dùng shadcn-vue components, Tailwind 4 utilities, font Geist/JetBrains Mono self-host, theme OKLCH teal `#0D9488` primary + dark mode `class="dark"`.

## Kết quả từng mục

| # | Mục | Kết quả | Ghi chú |
|---|-----|---------|---------|
| 1 | Cài dependencies | **DONE** | 17 gói mới vào `dependencies` (bảng bên dưới). Không xóa gói cũ. |
| 2 | shadcn-vue init | **DONE** | `components.json` viết tay (CLI init tạo preset khác + base style trùng lặp). Verified `npx shadcn-vue add button card` chạy được, **idempotent** (không sửa CSS). |
| 3 | Tailwind 4 | **DONE** | Plugin `@tailwindcss/vite` trong `vite.config.ts`; `src/styles/tailwind.css` (`@import "tailwindcss"` + `tw-animate-css` + `@custom-variant dark` + `@theme inline`). CSS layer đúng chuẩn shadcn — global.css giữ nguyên. |
| 4 | Font | **DONE** | Self-host variable woff2: `public/fonts/GeistVariable.woff2` + `JetBrainsMonoVariable.woff2` (fontsource latin). `@font-face` + map `--font-sans/--font-mono`. |
| 5 | Map tokens → shadcn OKLCH | **DONE** | `:root` + `.dark` OKLCH variables; primary teal `oklch(0.6 0.12 185)`; dark palette đảo tối + thêm `.dark` cho `--color-*` legacy (USER_GUIDE §3.15). `tokens.css` biến cũ giữ nguyên. |
| 6 | main.ts import order | **DONE** | `tokens.css → tailwind.css → global.css`. |

## Gói đã cài (dependencies)

| Gói | Phiên bản | Ghi chú |
|-----|-----------|---------|
| tailwindcss | 4.3.3 | Tailwind v4 |
| @tailwindcss/vite | 4.3.3 | Plugin Vite |
| tw-animate-css | 1.4.0 | shadcn v4 animations |
| shadcn-vue | 2.8.2 | CLI + base config |
| reka-ui | 2.10.3 | Base của component shadcn-vue v4 |
| class-variance-authority | 0.7.1 | `button` shadcn cần cva (CLI không tự cài) |
| clsx | 2.1.1 | `cn()` helper |
| tailwind-merge | 3.6.0 | `cn()` helper |
| @lucide/vue | 1.31.0 | Icon library shadcn-vue v4 (`lucide-vue-next` đã bị deprecate) |
| lucide-vue-next | 1.0.0 | Giữ theo quyết định log (dù deprecate → thay bằng @lucide/vue) |
| motion-v | 2.3.0 | Animation cho Phase 2 |
| gsap | 3.15.0 | Canvas simulator effects (Phase 2) |
| vue-echarts | 8.1.0 | Benchmark charts (cần echarts ^6) |
| echarts | 6.1.0 | Chart engine |
| lenis | 1.3.26 | Smooth scroll (Phase 2) |
| vue-sonner | 2.0.9 | Toast (Phase 2) |
| @phosphor-icons/vue | 2.2.1 | **Thay `phosphor-vue`** — gói `phosphor-vue` trên npm là Vue 2 (peer vue ^2.6.11). Package Vue 3 chính thức là `@phosphor-icons/vue`. |

> Lưu ý: `canvas-confetti` + `@monaco-editor/loader` đã có sẵn, không đụng.

## File tạo / sửa

### Tạo mới
- `frontend/components.json` — style=default, baseColor=none, **css=`src/styles/tailwind.css`** (xem lệch ở dưới), aliases `@/components`, `@/lib/utils`, iconLibrary=lucide, font=geist-sans.
- `frontend/src/lib/utils.ts` — `cn()` = clsx + tailwind-merge (chuẩn shadcn).
- `frontend/src/styles/tailwind.css` — Tailwind 4 + tw-animate-css + `@custom-variant dark (&:is(.dark *))` + `@font-face` Geist/JetBrains Mono + `:root`/`.dark` OKLCH variables + `@theme inline` map + `@layer base` shadcn.
- `frontend/public/fonts/GeistVariable.woff2` (29.4 KB) + `JetBrainsMonoVariable.woff2` (40.4 KB).
- `docs/work/g-f1a-home.png`, `docs/work/g-f1a-login-dark.png` — ảnh verify browser.

### Sửa
- `frontend/vite.config.ts` — thêm `tailwindcss()` plugin.
- `frontend/src/main.ts` — import CSS đúng thứ tự tokens → tailwind → global.
- `frontend/src/styles/tokens.css` — `--font-sans/--font-mono` trỏ Geist/JetBrains Mono (kèm fallback); thêm block `.dark` cho palette `--color-*` legacy.
- `frontend/tsconfig.json` — thêm `compilerOptions.paths` (`@/* → ./src/*`) để CLI shadcn-vue resolve alias (CLI đọc tsconfig.json, không đọc tsconfig.app.json). Không ảnh hưởng build (files:[] + project references).
- `frontend/package.json` + `package-lock.json` — 17 gói dependencies mới.

## Lệch nhỏ so với yêu cầu (có lý do)
1. **`components.json → tailwind.css` thay vì `global.css`**: khi để css=global.css, CLI `add` inject base style chứa `@apply border-border` vào global.css — file KHÔNG có `@theme` nên build fail `Cannot apply unknown utility class`. Trỏ về `tailwind.css` (nơi `@import "tailwindcss"` + `@theme inline` + `@layer base` đã tồn tại) → CLI phát hiện base style có sẵn và **bỏ qua inject hoàn toàn** (verified: `add button card` không sửa file CSS nào). Đúng yêu cầu #3 của task (tailwind.css là nơi đặt theme).
2. **`phosphor-vue` → `@phosphor-icons/vue`**: `phosphor-vue@1.4.2` peer `vue@^2.6.11` — không cài được với Vue 3.5. Dùng package chính thức Vue 3.
3. **Thêm `class-variance-authority` + `reka-ui` + `@lucide/vue`**: cần thiết cho component shadcn-vue v4 (button import cva; reka-ui là base; @lucide/vue là icon lib thay lucide-vue-next đã deprecate).

## Verify

### Build + Test
- `npm run build` (vue-tsc + vite) → **0 lỗi** ✓
- `npm test` → **72/72 PASS** ✓
- Bundle: engine 476 KB (không đổi), index 92 KB, vendor 109 KB.

### Browser (dev server, Playwright)
- **Home** `/` — render đủ header/hero/features/footer, không vỡ layout ✓
- **Login** `/login` — form email/mật khẩu hiển thị đúng ✓
- **Lesson** `/learn/1` — auth guard redirect `/login?redirect=/learn/1` (đúng — chưa có backend) ✓
- Console: chỉ 1 error `401 /api/v1/auth/refresh` (không có backend — dự kiến, main.ts đã catch) ✓
- **Font**: `body` font-family = `Geist, ...`, Geist loaded; JetBrains Mono có sẵn trong font list ✓
- **Dark mode** (thêm `class="dark"` vào `<html>`): body `#042F2E`, `--primary` `oklch(0.72 0.12 185)`, button login `#2DD4BF` (dark primary), không vỡ layout ✓
- **Tailwind utilities end-to-end**: build với `shadcn-vue add button` → `.bg-primary`, `.text-primary-foreground`, `.border-input` được generate đúng (xóa sau khi verify) ✓. `@layer base` emit `body{background-color:var(--background);color:var(--foreground)}` và `*{border-color:var(--border)}` ✓.

### shadcn CLI (Phase 1b readiness)
- `npx shadcn-vue@latest add button card -y --overwrite` → tạo 9 file `src/components/ui/{button,card}/` thành công, **không** sửa CSS, deps up-to-date. Idempotent. (Files xóa sau verify — Phase 1b sẽ add lại.)

## Commit
- `.\commit-as.ps1 son "feat(ux): G-F1a - setup tailwind4 + shadcn + fonts Geist/JetBrains + OKLCH tokens dark mode"`

## Việc kế tiếp (Phase 1b)
- `npx shadcn-vue@latest add button card badge ...` — CLI đã sẵn sàng, idempotent.
- Thay 13 component tự xây bằng shadcn giữ API tương đương; dùng `@lucide/vue` làm icon lib.
- CSS utilities Tailwind tự sinh khi dùng — không cần config content.

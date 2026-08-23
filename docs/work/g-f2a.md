# G-F2a — POLISH TOÀN CỤC (nền tảng cho 12 màn)

> Ngày: 12/08/2026 · Nhánh: `feature/ux-polish` (tạo từ `dev`, đã có ux-foundation G-F1a/b)
> Phạm vi: motion-v page transitions, Lenis, vue-sonner, canvas-confetti, 3 gradient OKLCH, hover micro-interaction.
> KHÔNG đụng `engines/*` + `components/simulator/*`.

## Tóm tắt 6 mục

| # | Mục | Trạng thái | Ghi chú |
|---|-----|-----------|---------|
| 1 | Page transitions motion-v | ✅ DONE | `App.vue` dùng `<RouterView v-slot>` + `<Transition name="page" mode="out-in">`, `key = route.fullPath`. Fade + slide nhẹ (220ms). Tôn trọng `prefers-reduced-motion` (global.css cắt transition-duration 0.01ms — verify qua Playwright). Scroll về đầu SAU transition (`@after-enter` + Lenis `scrollToTop(true)`), giữ vị trí khi back/forward (popstate flag). Router thêm `scrollBehavior` chỉ restore `savedPosition`. |
| 2 | Lenis smooth scroll | ✅ DONE | `composables/useLenis.ts` mới — singleton `new Lenis({ autoRaf, anchors, allowNestedScroll, autoToggle, stopInertiaOnNavigate, respectReducedMotion })`. Init 1 lần trong App.vue. Export `scrollTo/getLenis/scrollToTop`. Import `lenis/dist/lenis.css`. GSAP ScrollTrigger không dùng trong codebase hiện tại — để ngỏ khi Phase 3 cần. |
| 3 | vue-sonner hoàn chỉnh | ✅ DONE | `<Toaster position="top-right" rich-colors close-button :theme>` mount trong App.vue. `lib/toast.ts` giữ API cũ + thêm `loading`, `promise` (loading→success/error/finally), `custom`. Verify toast render thật qua Playwright (dynamic import cùng module Vite). |
| 4 | canvas-confetti helper | ✅ DONE | `composables/useConfetti.ts` nâng cấp: export `fireConfetti(type: 'success'|'levelup'|'achievement'|'node-pass')`, dùng `confetti.create()` trên 1 canvas FIXED (pointer-events none, resize:true), màu theo palette (gold/aurora/mint). Giữ API cũ `useConfetti()` (fireSuccess/fireQuizPass/firePremium) delegate sang fireConfetti. Skip khi prefers-reduced-motion. |
| 5 | 3 gradient palette OKLCH | ✅ DONE | `src/styles/palettes.css` mới: `--gradient-aurora` (teal→cyan→violet — Shop/gamification), `--gradient-sunset` (amber→rose — Learn streak/huy hiệu), `--gradient-mint` (mint→teal — canvas chrome) + màu nền `--*-soft`. Light `:root` + dark `.dark` riêng. `tailwind.css` thêm `@theme inline` (`--background-image-*-gradient` → `bg-aurora-gradient`, `bg-sunset-gradient`, `bg-mint-gradient`) + `@utility text-gradient-*`. Import trong main.ts. |
| 6 | Hover micro-interaction | ✅ DONE | `global.css` thêm `.hover-lift` (nâng + shadow) và `.hover-glow` (glow ring + scale) — class thuần CSS, không phá shadcn. Nâng cấp `.card--interactive` thêm scale. Áp dụng demo tại HomeView (card + CTA + title gradient). |

## File tạo / sửa

**Tạo mới**
- `frontend/src/composables/useLenis.ts` — smooth scroll singleton (Lenis 1.3.26).
- `frontend/src/styles/palettes.css` — 3 gradient OKLCH + soft colors (light/dark).

**Sửa**
- `frontend/src/App.vue` — page transition + Lenis init + scroll-after-enter + popstate handling.
- `frontend/src/composables/useConfetti.ts` — thêm `fireConfetti(type)` + canvas cố định, giữ API cũ.
- `frontend/src/lib/toast.ts` — thêm `loading` / `promise` / `custom`.
- `frontend/src/main.ts` — import `palettes.css` + `lenis/dist/lenis.css`.
- `frontend/src/router/index.ts` — `scrollBehavior` (restore savedPosition, không scroll giữa transition).
- `frontend/src/styles/global.css` — page transition CSS + `.hover-lift`/`.hover-glow` + `.card--interactive` scale.
- `frontend/src/styles/tailwind.css` — `@theme inline` background-image gradient + `@utility text-gradient-*`.
- `frontend/src/views/HomeView.vue` — demo hover + gradient title.

## Verify

- ✅ `npm run build` — 0 lỗi (vue-tsc + vite build).
- ✅ `npm test` — 72/72 PASS (8 files).
- ✅ `npx playwright test simulator` — 3/3 PASS (canvas không vỡ khi Lenis/motion bật).
- ✅ Verify thủ công bằng Playwright (spec tạm đã xoá sau khi chạy):
  - home/login/lesson: scroll mượt (html.lenis + scrollY tăng khi wheel), page transition chạy (MutationObserver bắt `.page-enter-active`), toast hiện được (`[data-sonner-toast]` render), 0 console error (nhiễu mạng 401 refresh boot — pre-existing, không có backend), light+dark không vỡ (`--gradient-*` + background đổi đúng).
  - Reduced motion: transition-duration bị cắt về ~0.01ms.

## Ghi chú / rủi ro

- ⚠️ E2E full suite (auth/ladder/code-runner — 8 tests) FAIL — **pre-existing trên `dev` baseline** (đã stash xác nhận cùng lỗi): mock `/auth/refresh` trả 200 → app boot "đã đăng nhập" → guard guestOnly đá `/login` về `/home`. Không liên quan G-F2a (simulator spec vẫn PASS). Ngoài phạm vi task — ghi lại cho Phase sau.
- GSAP ScrollTrigger chưa được dùng trong codebase → Lenis `autoRaf` đứng độc, không cần sync; khi Phase 3 thêm ScrollTrigger sẽ sync qua `lenis.on('scroll', ScrollTrigger.update)`.

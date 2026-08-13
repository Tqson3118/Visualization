# NOTES — Phase 1 nhóm A (dev-frontend: 10 view landing/auth + 404 + simulations + cheatsheet)

Ngày: 13/08/2026 · Worktree `D:\FPT\neww-qa` (nhánh `feature/view-quality-a`).

## 1. Hero mini-sim Home — đã làm mức nào, Phase 2 làm gì tiếp
- ĐÃ LÀM: mini-sim chạy **step THẬT từ engine** (`getSimulation()` từ `engines/registry` — catalog.ts tự đăng ký 44 generator khi import; `generate()` trả `Step[]` có `structure.elements` (label + status) + explanation tiếng Việt). Render bằng **DOM block** (không canvas): block-token + index mono + status → màu `data-core/resolved/conflict` + swap-pop 240ms. 3 demo đúng FR-7.6: `sort.bubble` (99 bước), `search.binary` (13), `graph.bfs` (29) — data 6 phần tử cố định, autoplay 380ms/bước, lặp nhẹ sau 1.4s, dừng nếu `prefers-reduced-motion`.
- CHƯA LÀM (Phase 2 — cần session/harness đầy đủ): render bằng `renderers/arrayRenderer.ts` + `painter/canvasPainter.ts` trên canvas thật (đúng quyết định xuyên-nhóm "hero chạy renderer canvas thật"); animation "nhịp thở" compare tần số tăng dần; swap bằng spring thật. DOM-block hiện tại là xấp xỉ trung thực (cùng nguồn trace), đủ cho Phase 1, ghi rõ để pm quyết định Phase 2.
- Lưu ý: engine chunk (478kB) đã được tải ở Home trước đó vì Home import CATALOG — không đổi.

## 2. Component dùng chung — đã sửa (tối thiểu, có decision log)
1. `src/styles/global.css`:
   - Reset `*{padding:0}` unlayered → `@layer base` — **HOTFIX toàn app**: trước đây mọi padding utility Tailwind (shadcn button/input) bị đè thành 0px (đo thật: input `px-3 pl-9` → computed 0px; chữ chạm viền). Ảnh hưởng mọi view theo chiều *khôi phục đúng thiết kế*.
   - Page transition easing: `220ms ease` → enter `cubic-bezier(0.16,1,0.3,1)` / exit `cubic-bezier(0.7,0,0.84,0)` (KILL-LIST V2 cấm easing mặc định >150ms).
2. `src/components/ui/Input.vue` (dùng chung 10 view): prop `icon` nhận thêm `Component` (lucide-vue-next); string cũ vẫn qua BaseIcon — backward-compatible, view khác không đổi hành vi.
3. KHÔNG sửa: `AppHeader.vue` (còn `<button>` raw `.app-header__user` + menu — thuộc phạm vi khác), `Button.vue`, `ui/card/*`, `ui/input/*`. AppHeader vẫn render brand + login/register cho khách trên mọi route — HomeView đã bỏ header nội bộ nên không còn trùng.

## 3. Quyết định ghi thêm trong `docs/pm-decision-log-viewquality.md`
- Bỏ header nội bộ HomeView (fix 2 header).
- Hero: bỏ gradient/blob → surface band + panel tối canvas-ink + mini-sim engine thật (DOM block, Phase 2 = canvas).
- Auth aside → panel tối Data Bench; trên panel luôn tối dùng `rgba(255,255,255,…)` / `white/*` (không phải hex, không theo theme) — tương đương engine canvasTheme fallback (#d9dde8 / #6b7385); `border-subtle` theo theme sai màu trên nền tối nên không dùng.
- HOTFIX reset padding (mục 2.1) + easing page transition (mục 2.1) + Input.vue additive.
- Segmented vai trò Register qua Button.vue ghost (giữ selector e2e `button.register__role-option`).

## 4. Verify
- `npm run build` (vue-tsc -b + vite build): **PASS** (chạy 3 lần sau mỗi đợt sửa).
- `npm run test` (vitest): **95/95 PASS** (gồm RegisterView.spec.ts — selector e2e giữ nguyên).
- Không có lint script trong package.json (chỉ dev/build/test/test:e2e) — ghi rõ, không tự đoán.
- Dev server `npm run dev -- --port 5175` từ worktree (proxy /api → :5000 sẵn trong vite.config). Đo thật bằng chrome-devtools MCP: 3 mốc breakpoint (1366×768 / 768 / 390×844) — không overflow, không đè chữ; computed padding button/input đúng chuẩn; console error = 0; demo tab switching OK; dark mode OK (panel dữ liệu vẫn tối).
- Ảnh: `docs/work/view-quality/shots/` (10 ảnh light/dark 1366×768).
- Ollama: chạy được (`qwen2.5vl:3b` @ :11434), 3 gate × 5 view ghi tại `docs/work/view-quality/ollama-log/`. **Lưu ý độ tin cậy**: model 3B VL chấm yếu (tự mâu thuẫn — Login gate1 "không glassmorphism" vs Reset gate1 "có glassmorphism"; Reset gate1 bịa "gradient + blob" dù DOM chứng minh `backgroundImage: none`). Gate 3 (bản sắc): model nhận diện đúng "DSA Visual — dạy cấu trúc dữ liệu/giải thuật" trên cả 5 view, không view nào kết luận "chung chung" → dùng làm tín hiệu phụ. Bằng chứng chính: DOM/computed assertions ở trên.

## 5. Vi phạm còn lại có chủ đích (đã ghi decision log)
- `rgba(255,255,255,…)` trên panel tối (không phải @theme token) — bắt buộc vì panel LUÔN tối không theo theme; tương đương engine fallback.
- Dot "live" pulse 2s (opacity, cubic-bezier chuẩn) ở hero bench — ambient, bị cắt khi prefers-reduced-motion.
- Motion shell auth views (280ms) chồng với page transition — chấp nhận (nhẹ), có thể bỏ sau khi thống nhất route transition toàn app.

## 6. Đợt 2 (4 view còn lại: NotFound/Privacy/Help/Simulations) — ghi chú bổ sung
- **HOTFIX 2 toàn app**: `global.css` `a { color }` unlayered đè `text-primary-foreground` trên RouterLink-as-button (đo thật CTA HomeView + 404: chữ #007E72 trên nền primary ~1.2:1) → đưa vào `@layer base` + `a.inline-flex:hover { text-decoration: none }`. Sau sửa CTA 404 trắng/teal contrast 21:1; HomeView CTA cũng được sửa (regression tốt). Xem decision log.
- **Badge.vue** (shared): thêm `min-h-6` (height badge ≥ 24px, trục 5f) — đo 25.6px. Decision log.
- **Card.vue** base còn `shadow-sm` — SimulationsView override `shadow-none` tại call site; bản thân Card.vue là shared cần nhóm khác/Phase 2 xử lý chung (ghi để pm biết, KHÔNG sửa trong task này).
- **Theme toggle chưa được wire** (pre-existing): `stores/ui.ts` có `theme` ref nhưng không ai apply `.dark` class lên `<html>` (chỉ truyền cho Toaster). Dark mode chỉ test được bằng class thủ công — đề xuất task khác wire theme (toggle + localStorage + prefers-color-scheme).
- **BenchmarkPanel** (shared, ngoài phạm vi): heading "⚖ Benchmark Lab" còn emoji icon + "▶ Chạy benchmark" glyph — vi phạm icon lucide nhưng nằm trong component khác view → ghi để nhóm sở hữu sửa.
- Ollama 3B VL chấm yếu trên trang text (privacy gate1 "chung chung" dù có index mono; help "CO DAU VET" sau khi thêm index) — dùng làm tín hiệu phụ; bằng chứng chính = DOM/computed + tiêu chí "chi tiết chỉ app này có" trong standard.md.
- Ảnh đợt 2: 8 ảnh light/dark (notfound/privacy/help/simulations) lưu `docs/work/view-quality/shots/` (bản gốc chụp bằng chrome-devtools MCP tại temp; copy 2 đại diện vào shots nếu cần — hiện log ollama đã đủ).

## 7. Đợt 3 (view cuối nhóm A: CheatSheetView `/cheatsheet` — 14/08/2026) — ghi chú bổ sung
- **Visual check không cần sửa repo**: `/cheatsheet` có `meta.requiresAuth` → route guard chặn khi BE không reachable qua vite proxy (`localhost:5000` là WSL/Docker relay — Node proxy không kết nối được dù PowerShell gọi thẳng OK). Giải pháp: mock BE tạm ở temp (`%TEMP%\opencode\mock-be\server.js`, port 5999, CORS cho origin 5175, trả refresh/me/hearts giả) + chạy dev với `VITE_API_BASE_URL=http://localhost:5999/api/v1` — **KHÔNG đụng vite.config/repo**. API client đọc env này sẵn (client.ts).
- **Dark mode test**: dùng `.dark` class thủ công trên `<html>` (theme toggle chưa wire — mục 6). LƯU Ý khi đo: sau khi mutate class, đọc getComputedStyle 1 thuộc tính đơn có thể trả giá trị stale (đo được chrome bg light dù html var = dark) — phải đọc cả cụm (html var + element var + bg) trong 1 script để force style recalc; nhóm B/C đo dark nên làm vậy.
- **Còn lại có chủ đích (đã ghi decision log)**: chip Big-O tối dùng `rgba(255,255,255,0.92)` (panel LUÔN tối — tiền lệ mục 3); strip Big-O banner (decorative aria-hidden); `.input` global `transition: border-color 200ms ease` (global.css:169) — file shared, dùng ở search của view nhưng ngoài phạm vi → đề xuất task chung đổi sang `150ms cubic-bezier(0.16,1,0.3,1)`.
- **Ollama**: chạy được (qwen2.5vl:3b @ :11434), 3 gate light + 1 gate dark đều PASS — Gate 1 "RO RANG APP HOC CTDL", Gate 2/3 + dark: không overflow/overlap, contrast rõ. Log: `docs/work/view-quality/ollama-log/cheatsheet.md`. Ảnh: `docs/work/view-quality/shots/cheatsheet-{light,dark}-1366.png`.
- Verify: `npm run build` PASS (vue-tsc -b + vite build); `npm run test` PASS (95/95). Console error = 0; a11y issue = 0 (đã thêm `name` cho search input). Đo 3 mốc: 1366 (table full), 768 (table khớp, hScroll 0), 390 (card-stack — thead ẩn, tr = card, td data-label, hScroll 0).

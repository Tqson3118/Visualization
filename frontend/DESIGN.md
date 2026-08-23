# DESIGN — Brand Contract cho toàn bộ 36 view

> Nguồn chuẩn: `DESIGN-IDENTITY.md` (Phase 0 – BƯỚC A, đã chốt) + `PROMPT_VIEW_QUALITY_MASTER_V2.md` mục 5 BƯỚC B. File này là **hợp đồng duy nhất** cho 4 nhóm Phase 1: mọi quyết định visual của 36 view phải trả lời được bằng 1 dòng trong file này. Token hiện trạng đối chiếu: `src/styles/tokens.css` (legacy HEX) + `src/styles/tailwind.css` (shadcn OKLCH `@theme inline`) + `src/components/ui/button/index.ts` (buttonVariants). Người soạn: dev-docs · 13/08/2026.

## Lịch sử thay đổi

| Phiên bản | Ngày | Người soạn | Thay đổi |
|---|---|---|---|
| 1.0.0 | 13/08/2026 | dev-docs | Tạo lần đầu: 10 phần theo BƯỚC B; token CẦN THÊM liệt kê ở §2. |

---

## §1 Visual Theme & Atmosphere

**Tham chiếu** `DESIGN-IDENTITY.md`: motif **"Phòng thí nghiệm dữ liệu" (Data Bench)** — người học xây mental model bằng cách thao tác trực quan với dữ liệu; quy ước 2 lớp xuyên suốt: **lớp dữ liệu** (canvas, code, giá trị đọc kỹ) luôn là *sân khấu tối* trên nền `canvas-ink`; **lớp giao diện** (nav, tiêu đề, CTA) là *giấy lặng* phân cấp bằng luminance stacking (§6). Signature: **"Block thở theo bước"** — mỗi giá trị dữ liệu là một block biết trạng thái thuật toán của chính nó (default/compare/swap/resolve), có **index mono** bên dưới; nguồn sự kiện duy nhất là trace thật từ `stepExecutor`.

**Bầu không khí**: app học tập nghiêm túc (nền tảng đại học) — sạch, tĩnh, đúng nhịp dữ liệu. Accent teal `#0D9488` **CHỈ interactive** (CTA, link, active, focus) — không neon, không lòe loẹt, không trang trí thụ động. Hỗ trợ dark + light toàn site, nhưng **vùng dữ liệu LUÔN tối bất kể theme** (quyết định xuyên-nhóm, mục 3).

### Quyết định xuyên-nhóm đã chốt (BƯỚC A Pha 3 — ghi vào đây để 4 nhóm dùng chung)

| Quyết định | Chốt | Áp dụng |
|---|---|---|
| **Banner/hero mỗi trang** | **BỎ gradient trang trí ngẫu nhiên** (teal→tím, cam→đỏ... như bằng chứng mục 4 PROMPT). Banner = surface band elevation level-2 (§6) + luminance stacking + tùy chọn strip mono dữ liệu (block-token dãy) — không đổi hue theo trang. **Hero trang chủ chạy mô phỏng THẬT** (mini, tối giản, tự động lặp nhẹ) lấy trực tiếp từ `src/engines/catalog.ts` (3 demo công khai) + renderer canvas thật — không dùng video/Lottie/minh họa trang trí rời rạc. | §1 + §6 |
| **Icon library** | **`lucide-vue-next` duy nhất** (chuẩn shadcn-vue, đã cài). Loại dần `@lucide/vue` + `@phosphor-icons/vue` — báo pm vì đụng dependency. | §4 |
| **Stat-card hierarchy** | Tối đa **1 stat hero/màn** (block-token lớn + elevation level-2); stat còn lại lùi level-1, không icon tròn đổi màu lung tung. | §6 |
| **Block-token tái sử dụng** | Nơi NÊN dùng (thay vì bịa card trắng mới): XP history, streak calendar, benchmark result, quiz result, leaderboard rank, mọi dữ liệu tuần tự/có chỉ số — 1 class chung + index mono, cùng ngôn ngữ với canvas. | §1 + §2 |
| **Motif nền tối ở light mode** | Vùng dữ liệu **LUÔN tối**: simulator, code-runner, block-token — bất kể theme toàn site (như editor/terminal vẫn tối dù OS light). Phần còn lại theo theme. | §1 + §6 |
| **Công cụ animation** | Route = View Transitions API (fallback motion-v); enter/exit = motion-v; list reorder = `@formkit/auto-animate`; scroll reveal = GSAP ScrollTrigger; ăn mừng = `canvas-confetti` (đã cài); lenis chọn lọc 1–2 trang; `@vue-flow/core` chỉ nhóm B, sau khi xác nhận session Diagrams. | §1 decision log |

---

## §2 Color Palette & Roles

### 2.1 Palette gốc (6 màu từ DESIGN-IDENTITY — nguồn `canvasTheme.ts`)

| Hex | Tên (token) | Vai trò | Phạm vi |
|---|---|---|---|
| `#0D9488` | `accent` | **CHỈ interactive**: CTA, link, active, focus | UI token (`--primary` family trong `tailwind.css`) |
| `#0D1020` | `canvas-ink` | Nền vùng dữ liệu (canvas, code-runner, block-token) — LUÔN tối bất kể theme | Engine token (`canvasTheme.ts --canvas-bg`) |
| `#4255FF` | `data-core` | Màu block mặc định / giá trị dữ liệu | Engine token |
| `#34D399` | `resolved` | Trạng thái "đã ổn định": sorted, tìm thấy, hoàn thành, đúng | Engine token (+ semantic success) |
| `#F87171` | `conflict` | Trạng thái "va chạm": swap, so sánh sai, lỗi, hết lượt | Engine token (+ semantic error) |
| `#6B7385` | `index-muted` | Text thứ cấp trên nền tối: mono index, caption | Engine token (+ text tertiary trên nền tối) |

4 màu engine **không khai báo lại trong component** — đọc qua engine/canvas. Khi view ngoài canvas cần tái dùng block-token (XP history, streak...), thêm token `@theme` ánh xạ cùng giá trị (bảng 2.4).

### 2.2 Bảng màu UI — vai trò + token `@theme` (CẤM hex rời trong component)

| Vai trò | Tầng | Token hiện có | Token CẦN THÊM | Ghi chú |
|---|---|---|---|---|
| **Surface** | nền app | `--background` | — | light `oklch(0.978 0.014 187)` / dark `oklch(0.185 0.028 190)` |
| **Surface** | card level-1 | `--card` | — | `bg-card` — card thường, elevation level-1 (§6) |
| **Surface** | card level-2 (hero-stat, banner band) | — | `--card-raised` + `--color-card-raised` | light: trắng sạch hơn `oklch(0.99 0.006 190)`; dark: sáng hơn card 1 bậc `oklch(0.245 0.03 190)` |
| **Surface** | popover/dropdown/modal | `--popover` | — | có shadow (là ngoại lệ §6) |
| **Text** | tier 1 primary | `--foreground` | — | `text-foreground` — heading, body chính |
| **Text** | tier 2 secondary | — | `--foreground-secondary` + `--color-foreground-secondary` | label, description quan trọng; light `oklch(0.45 0.03 190)` / dark `oklch(0.85 0.02 190)` |
| **Text** | tier 3 tertiary | `--muted-foreground` | `--foreground-tertiary` (alias cùng giá trị) | caption, meta, bảng phụ; giữ `--muted-foreground` cho component shadcn gốc |
| **Text** | tier 4 quaternary-muted | — | `--foreground-quaternary` + `--color-foreground-quaternary` | placeholder, disabled, ghi chú phụ; light `oklch(0.6 0.02 190)` / dark `oklch(0.65 0.02 190)` |
| **Border** | tier 1 standard | `--border` | — | `border-border` — viền card/input mặc định |
| **Border** | tier 2 subtle (card nổi) | — | `--border-subtle` + `--color-border-subtle` | light `rgba(0,0,0,0.06)` / dark `rgba(255,255,255,0.06)` |
| **Border** | tier 3 strong (hover/focus/emphasis) | — | `--border-strong` + `--color-border-strong` | light `rgba(0,0,0,0.14)` / dark `rgba(255,255,255,0.16)` |
| **Accent (interactive)** | primary | `--primary` / `--ring` | — | CHỈ CTA/link/active/focus — không trang trí |
| **Semantic** | success | — | `--success` + `--color-success` | quy đổi OKLCH từ `--color-success` legacy (tokens.css); dark = `resolved` (`#34D399`) |
| **Semantic** | warning | — | `--warning` + `--color-warning` | quy đổi OKLCH từ `--color-warning` legacy (tokens.css, cả 2 theme) |
| **Semantic** | error | `--destructive` | — | đã có `--color-destructive`; dark `#F87171` (= `conflict`) |
| **Semantic** | info | — | `--info` + `--color-info` | quy đổi OKLCH từ `--color-info` legacy (tokens.css, cả 2 theme) |

**Quy ước gọi tên**: `text-foreground` / `text-foreground-secondary` / `text-foreground-tertiary` / `text-foreground-quaternary` = 4 tầng text chuẩn. Không nhầm với `--secondary-foreground` (chữ trên nút secondary). Khi 2 nguồn song song (`--foreground`/`--muted-foreground` của shadcn vs tầng text mới): **component shadcn gốc giữ nguyên; view mới/sửa Phase 1 bắt buộc dùng 4 tầng chuẩn**.

### 2.3 Quy tắc dùng màu

- Tương phản: text thường ≥ 4.5:1, text lớn ≥ 3:1 — kiểm bằng token đã chốt, không tự đổi tông.
- 3 màu trạng thái thuật toán (data-core/resolved/conflict) là **ngôn ngữ dữ liệu** — ngoài canvas chỉ dùng qua block-token/semantic, không nhuộm UI lung tung.
- Accent teal không dùng làm nền banner/trang trí — banner dùng surface level-2 (§6).

### 2.4 Token CẦN THÊM vào `src/styles/tailwind.css` (danh sách gộp — nhiệm vụ của task token, KHÔNG làm trong task này)

Raw trong `:root` + `.dark` (OKLCH):

1. `--foreground-secondary` · `--foreground-tertiary` · `--foreground-quaternary` — 4 tầng text (§2.2)
2. `--border-subtle` · `--border-strong` — border tiers 2–3
3. `--card-raised` — surface level-2
4. `--success` · `--warning` · `--info` — semantic (error dùng `--destructive` có sẵn)
5. (Khuyến nghị) `--canvas-ink` · `--data-core` · `--resolved` · `--conflict` · `--index-muted` — để block-token tái dùng 1 nguồn ngoài engine

`@theme inline` — thêm map tương ứng: `--color-card-raised` · `--color-foreground-secondary/-tertiary/-quaternary` · `--color-border-subtle/-strong` · `--color-success/-warning/-info` · `--color-canvas-ink` · `--color-data-core` · `--color-resolved` · `--color-conflict` · `--color-index-muted`.

Radius (hiện `--radius: 0.625rem` → calc ra 6/8/10/14, LỆCH scale §5): **ghi đè 4 dòng** `--radius-sm: 0.25rem` · `--radius-md: 0.5rem` · `--radius-lg: 0.75rem` · `--radius-xl: 1rem` (bỏ calc).

---

## §3 Typography Rules

Font: **Geist** (sans — display + body/UI) · **JetBrains Mono** (mono — mọi giá trị "đo được": index, Big-O, bộ đếm bước, mã giả, code, phím tắt). Variable fonts đã self-host (`/fonts/GeistVariable.woff2`, `/fonts/JetBrainsMonoVariable.woff2`). **3 mức weight duy nhất: 400 / 500 / 600 — CẤM 700 lung tung** (grep cấm `font-bold`/`font-700` ở heading).

### Hierarchy chuẩn

| Cấp | Size | Weight | Line-height | Letter-spacing | Dùng cho |
|---|---|---|---|---|---|
| **H1** | 48px (`text-4xl`) | 600 | 1.1 | **-0.03em (-1.44px)** | Tiêu đề trang (duy nhất 1 H1/màn) |
| **H2** | 36px (`text-3xl`) | 600 | 1.15 | **-0.025em (-0.9px)** | Section chính |
| **H3** | 30px (`text-2xl`) | 600 | 1.2 | **-0.02em (-0.6px)** | Section phụ / card title lớn |
| **H4** | 24px (`text-xl`) | 600 | 1.25 | **-0.015em (-0.36px)** | Card title / group title |
| **Body** | 16px (`text-base`) | 400 | 1.6 | 0 | Đoạn văn, nội dung chính |
| **Body sm** | 14px (`text-sm`) | 400 | 1.55 | 0 | Mô tả phụ, meta |
| **Caption** | 12px (`text-xs`) | 400 | 1.5 | 0 | Ghi chú, meta phụ (không nhỏ hơn 12px) |
| **Label / Button** | 14px (`text-sm`) | 500 | 1.4 | 0 | Nhãn form, nút, tab, breadcrumb |
| **Mono index** | 12px | 400 | 1.4 | 0 | Index dưới block, streak ngày |
| **Mono data** | 14px (`font-mono text-sm`) | 400 (highlight 500) | 1.6 | 0 | Big-O chip, bộ đếm `step 12/47`, giá trị block, code |

### Quy tắc

- **Letter-spacing âm giảm dần theo size** (bảng trên): H1 -0.03em → H4 -0.015em; body/caption trở đi = 0. Cấm `tracking-[...]` dương rời cho heading.
- Heading dùng `--font-heading` (= Geist), không chữ in hoa dàn trải (trừ label mono ngắn như `NODE 04/08`).
- Số liệu thống kê (XP, streak, benchmark) = Geist 600 + mono cho đơn vị/index; bộ đếm bước = mono.
- Cấm font-size tự đặt ngoài scale §3/`--text-*` (grep cấm `text-[17px]` cho text nội dung).

---

## §4 Component Stylings

Chỉ dùng **variant shadcn-vue SẴN CÓ** (đã map dưới) — **cấm tự chế variant mới lặp logic**. Icon: **`lucide-vue-next` duy nhất**, cùng size/stroke-width trong cùng component (quy ước: icon trong button 16px `size-4`; icon standalone 16–20px; stroke-width mặc định 2 — cấm trộn stroke 1.5/2/2.5 lung tung).

### 4.1 Button — bảng padding BẮT BUỘC (khớp `buttonVariants` hiện có)

| Size | Class trong `buttonVariants` | Padding | Radius | Min-height | Ghi chú |
|---|---|---|---|---|---|
| **default (md)** | `h-10 px-4 py-2` | ngang 16px / dọc 8px | `rounded-md` (8px) | **40px** | Kích thước mặc định — nút chính |
| **sm** | `h-9 px-3` | ngang 12px / dọc 6px | `rounded-md` | 36px | Compact — chỉ dùng khi cần; hit target vẫn ≥ 24×24 (WCAG 2.5.8) |
| **lg** | `h-11 px-8` | ngang 32px / dọc 8px | `rounded-md` | **44px** | CTA chính (đủ 44px mobile) |
| **icon** | `h-10 w-10` | — | `rounded-md` | 40×40px | Icon 16px, `aria-label` bắt buộc |
| **icon-sm** | `size-9` | — | `rounded-md` | 36×36px | Hiếm dùng |
| **icon-lg** | `size-11` | — | `rounded-md` | 44×44px | Icon hero / điều khiển chính |

Variant: `default` (primary teal) · `destructive` · `outline` · `secondary` · `ghost` · `link` — đúng như `buttonVariants`.

**Luật bất biến (trục interactive sizing — user phàn nàn nhiều nhất):**
- Mọi button/clickable qua `Button.vue`/`buttonVariants()` — grep `<button` raw = 0 (trừ canvas/editor/table-cell đặc biệt → ghi decision log).
- Text cách viền **≥ 8px**: min `px-3` (12px) — cấm `px-0/px-1/px-2/p-0/py-0` trên nút chữ.
- Icon + text **gap ≥ 8px**: `gap-2` (đã có sẵn trong base class).
- Nút chính: min-height **40px desktop / 44px mobile**; hit target ≥ **24×24** (WCAG 2.5.8).
- Nút liền kề cách nhau **≥ 8px**.
- Hover/focus: `hover:bg-primary/90` (per variant), focus = `focus-visible:ring-2 ring-ring ring-offset-2` (đã có) — không tự chế.

### 4.2 Card (`ui/card`)

- Card chuẩn: `bg-card border border-border rounded-lg` — elevation level-1, **không shadow** (§6).
- Card nổi (hero-stat, banner band): `bg-card-raised border border-border-subtle rounded-lg` — level-2.
- CardHeader: `p-6` (24px); CardTitle: H4 (24px/600/tracking -0.015em) hoặc `text-lg font-semibold`; CardDescription: `text-foreground-secondary text-sm`; CardContent: `p-6`; gap giữa title/description/content: 8px (spacing §5).
- Hover card: chỉ đổi `border-border-subtle → border-border-strong`, không scale/shadow (trừ card clickable → thêm ring khi focus).

### 4.3 Badge (`ui/badge`)

- Variant: `default` (primary) · `secondary` · `destructive` · `outline`.
- Chuẩn: `rounded-md border px-2.5 py-0.5 text-xs font-medium` — **padding ngang ≥ 6px + height ≥ 24px** (thêm `min-h-6` nếu cần).
- Big-O chip / trạng thái bước: dùng `font-mono text-xs` + `variant="outline"` — trạng thái thuật toán (resolved/conflict) qua `text-resolved`/`text-conflict` (khi token thêm) hoặc semantic success/destructive.

### 4.4 Input (`ui/input` + Select family)

- `h-10 rounded-md border border-input bg-background px-3 py-2 text-sm` — text cách viền ≥ 8px (px-3 = 12px).
- Placeholder = `text-foreground-quaternary`; label = `text-foreground-secondary text-sm font-medium`; helper = `text-foreground-tertiary text-xs`.
- Focus: `focus-visible:ring-2 ring-ring ring-offset-2`; error: `border-destructive` + message `text-destructive text-sm`.
- Select dùng cùng chiều cao/padding; không tự chế dropdown riêng.

### 4.5 Modal (`ui/dialog`)

- Overlay: `bg-background/80 backdrop-blur-sm` — **glassmorphism chỉ hợp lệ ở đây** (1-2 điểm nhấn, KILL-LIST).
- Content: `bg-card border border-border rounded-lg shadow-lg p-6 max-w-lg` — **shadow chỉ dùng cho dropdown/modal/focus** (§6).
- Close: icon button `size-9` ghost, icon 16px, `aria-label="Đóng"`.
- Enter/exit: motion-v, 200–300ms, easing `cubic-bezier(0.16,1,0.3,1)` (enter) / `cubic-bezier(0.7,0,0.84,0)` (exit).

### 4.6 Table (chưa có trong `ui/` — khi thêm, theo chuẩn shadcn-vue + token §2, KHÔNG tự chế)

- thead: `text-sm font-medium text-foreground-tertiary h-10 text-left`; th không in hoa trừ cột mono ngắn.
- tbody: tr `border-b border-border hover:bg-muted/50`; td `p-3` (12px), row height ≥ 44px.
- Cột dữ liệu số (index, bộ đếm, Big-O, thời gian): `font-mono text-sm` — ngôn ngữ dữ liệu.
- 3 trạng thái bắt buộc: skeleton (khớp kích thước thật), empty (component `EmptyState` + copy §9), error (message + nút retry).

---

## §5 Layout Principles

- **Spacing scale** (bội số token): `4 / 8 / 12 / 16 / 24 / 32 / 48 / 64` px — cấm hardcode `p-[13px]`; mọi padding/margin/gap là 1 trong các giá trị này (Tailwind: `p-1..p-16` + `p-3`=12px).
- **Internal < External**: trong card/nhóm dùng spacing nhỏ hơn giữa các nhóm — gap nội dung 8–16px, giữa card 16–24px, giữa section 32–48px.
- **Radius scale**: `4 (rounded-sm) / 8 (rounded-md) / 12 (rounded-lg) / 16 (rounded-xl) / full (rounded-full)` — card 12px, button/input/badge 8px, chip 8px, avatar/icon tròn full.
- **Grid 12 cột** responsive: `1366 → 12 cột · 768 → 8 cột · 390 → 4 cột` (span đầy đủ = 12/8/4); gutter 24px desktop / 16px mobile; container `max-w-7xl` (80rem), căn giữa.
- Bố cục màn theo 4 wireframe `DESIGN-IDENTITY.md` §1.4: dashboard / luồng học bài / editor-mô phỏng / danh sách — không bịa layout mới.
- Header: chiều cao 64px, border-bottom `border-border`, logo trái + nav giữa/phải; breadcrumb mono (`NODE 04/08`) khi trong luồng học.

---

## §6 Depth & Elevation

**Luminance stacking — nổi bậc bằng surface + border semi-transparent, KHÔNG bằng shadow dày.**

| Level | Light | Dark | Dùng cho |
|---|---|---|---|
| **0** | `bg-background` | `bg-background` | Nền app |
| **1** | `bg-card` + `border-border` | `bg-card` + `border-border` | Card thường, stat phụ |
| **2** | `bg-card-raised` + `border-border-subtle` (≈ trắng sạch + `rgba(0,0,0,0.06)`) | `bg-card-raised` + `border-border-subtle` (`rgba(255,255,255,0.05–0.08)`) | Hero-stat, banner band, block-token container |
| **3** | `bg-popover` + `shadow-md` | `bg-popover` + `shadow-md` | Dropdown, modal, tooltip — **ngoại lệ duy nhất có shadow** |

- Shadow **chỉ** cho dropdown/modal/focus (ring): `shadow-sm` popover/tooltip, `shadow-md` modal; card **cấm** shadow (grep `shadow-` trong card = vi phạm).
- Focus dùng `ring-ring` (đã là token), không bóng đổ chồng.
- **Stat-card hierarchy**: tối đa **1 hero-stat/màn** — hero = level-2 + block-token lớn (nền `canvas-ink`, block `data-core` + index mono) + số Geist 600; các stat còn lại level-1, số Geist 600 `text-2xl`, không icon tròn đổi màu lung tung, không `% xanh` vô nghĩa.
- **Dark-motif**: vùng dữ liệu (simulator canvas, code-runner, block-token, hero demo) **LUÔN nền `canvas-ink`** bất kể theme — như editor/terminal; block = `data-core`, resolve = `resolved`, conflict = `conflict`, index = `index-muted` (giá trị từ engine `canvasTheme.ts`).
- Gradient hợp lệ **chỉ khi mã hoá trạng thái** (VD dải `data-core → resolved` cho tiến trình sắp xếp) — không trang trí vô nghĩa.

---

## §7 Do's and Don'ts

### Do (làm)

1. Accent teal `#0D9488` **chỉ interactive** (CTA/link/active/focus) — không dùng trang trí thụ động.
2. Vùng dữ liệu **luôn tối** (`canvas-ink`) bất kể theme; block-token + index mono cho mọi dữ liệu tuần tự/có chỉ số.
3. Dùng token `@theme` §2 — **0 hex rời trong component**; nếu thiếu token → thêm vào CSS, không bịa.
4. Phân cấp card theo §6: tối đa 1 hero-stat/màn, card còn lại level-1; nổi bậc bằng surface + border, không shadow.
5. Text dùng đúng 4 tầng §2 + scale §3 (weight 400/500/600, tracking âm giảm dần theo size).
6. Mono cho mọi thứ "đo được": index, Big-O chip, bộ đếm bước, giá trị block, phím tắt.
7. Empty state = lời mời hành động (component `EmptyState` chung, copy §9) — không "No data" trơ trọi.
8. Nút + toast **cùng động từ**; label nút nói đúng hành động người dùng thấy.
9. Spacing/radius theo scale §5 (4/8/12/16/24/32/48/64 · 4/8/12/16/full); internal < external; grid 12 cột.
10. Animation: chỉ `transform` + `opacity`, easing chủ đích (`cubic-bezier(0.16,1,0.3,1)` enter / `cubic-bezier(0.7,0,0.84,0)` exit), 1–2 khoảnh khắc đáng đầu tư/view, tôn trọng `prefers-reduced-motion`.

### Don't (cấm)

1. Gradient 2 màu trang trí trên banner/hero/CTA (teal→tím, cam→đỏ...) — banner = surface band level-2 + mono strip.
2. Emoji làm icon chức năng (🎯🚀✨) hoặc trộn nhiều bộ icon — `lucide-vue-next` duy nhất, cùng size/stroke.
3. Mọi card nổi bằng nhau (`rounded-2xl` + shadow mềm đồng loạt) — không phân cấp = không có gì nổi.
4. Hero công thức: heading căn giữa + mô tả + 2 CTA + blob gradient phía sau.
5. Glassmorphism phủ khắp (`backdrop-blur` tràn lan) — chỉ overlay modal.
6. Fade+slide-up giống hệt trên >80% phần tử/view — animation cơ giới.
7. Stat-card icon tròn + số to + % xanh khi không có tường thuật thật.
8. Weight 700 lung tung, tracking dương cho heading, font-size ngoài scale §3.
9. Hex rời / `px-[13px]` / padding button tự đè (cấm `px-0/px-1/px-2/p-0/py-0` trên nút chữ); easing mặc định trình duyệt cho chuyển động >150ms.
10. Microcopy dịch máy: "Submit" thay tên hành động, nút "Lưu" nhưng toast "Cập nhật thành công", empty/error giọng hệ thống trơ trọi.

---

## §8 Responsive Behavior

3 mốc bắt buộc test: **1366×768 (laptop) · 768px (tablet) · 390px (mobile)** — không lệch/tràn/đè chữ ở mốc nào (trục breakpoint).

| Thành phần | 1366 | 768 | 390 |
|---|---|---|---|
| **Nav/header** | Full: logo + nav ngang + CTA | Thu gọn: ẩn label nav phụ, giữ logo + CTA chính | Drawer (motion-v enter) hoặc bottom-sheet; nút CTA chính `size="lg"` (44px) |
| **Breadcrumb mono** | Đầy đủ `NODE 04/08` | Giữ, rút gọn cấp cuối | Ẩn phần phụ, chỉ giữ vị trí hiện tại |
| **Table** | Full cột | Ẩn cột phụ (theo breakpoint), giữ cột số liệu | Chuyển card-stack (1 card = 1 hàng), cấm scroll ngang bảng chính |
| **Stat-card** | Grid 4 cột (12/3) | Grid 2 cột | 1 cột: hero-stat full width, stat phụ xếp dọc |
| **Simulator / code-runner** | Canvas + panel điều khiển cạnh nhau | Chồng dọc: canvas trên, toolbar + mã giả dưới | Canvas full width, toolbar icon-only (icon-sm), counter mono nhỏ hơn nhưng ≥ 12px |
| **Card grid (khám phá, BXH)** | Grid 12 cột (span 3–4) | Grid 8 cột (span 4) | Grid 4 cột (span 4 = 1 cột) |
| **Spacing** | gutter 24px, section gap 48px | gutter 24px | gutter 16px, section gap 32px; nút chính đạt ≥ 44px min-height |

Quy tắc chung: mobile giảm 1 bậc spacing (§5), giữ nguyên token màu/typography (không "mobile font riêng"); hit target ≥ 24×24 (WCAG 2.5.8) ở mọi mốc.

---

## §9 Content Voice

Giọng sản phẩm: người dùng là người đang *thao tác dữ liệu* — mọi câu chữ gọi đúng việc họ đang làm, đúng động từ họ vừa bấm. Nút và toast xác nhận **cùng động từ** (bấm "Lưu" → toast "Đã lưu", không phải "Cập nhật thành công"); nút đặt tên theo hành động người dùng thấy, không theo tên hệ thống ("Submit" → "Nộp bài" / "Chạy mô phỏng"). Empty state là **lời mời hành động**: "Chưa có bài benchmark nào — chạy thử bài đầu tiên" thay vì "No data". Error nói rõ **chuyện gì xảy ra + cách xử lý**: "Không tải được lộ trình, máy chủ không phản hồi — thử lại sau 30 giây" kèm nút retry. Mọi giá trị số (điểm, bước, Big-O) hiển thị mono và viết số thật (12/47), không làm tròn mơ hồ.

---

## §10 Agent Prompt Guide

### Quy ước gọi tên token (copy-paste chuẩn)

| Nhóm | Cú pháp dùng |
|---|---|
| Surface | `bg-background` · `bg-card` · `bg-card-raised` · `bg-popover` |
| Text 4 tầng | `text-foreground` (1) · `text-foreground-secondary` (2) · `text-foreground-tertiary` (3) · `text-foreground-quaternary` (4) |
| Border 3 tầng | `border-border` · `border-border-subtle` · `border-border-strong` |
| Interactive | `bg-primary text-primary-foreground` · `ring-ring` · `border-input` |
| Semantic | `text-success` · `text-warning` · `text-destructive` · `text-info` |
| Radius | `rounded-sm`(4) · `rounded-md`(8) · `rounded-lg`(12) · `rounded-xl`(16) · `rounded-full` |
| Typography | `font-sans`/`font-mono` · weight `font-normal`(400)/`font-medium`(500)/`font-semibold`(600) · tracking heading: H1 `tracking-[-0.03em]` H2 `[-0.025em]` H3 `[-0.02em]` H4 `[-0.015em]` |
| Dữ liệu | `font-mono text-sm` (giá trị/bộ đếm/Big-O) · `font-mono text-xs` (index) · nền tối `bg-canvas-ink` + block `bg-data-core`/`text-resolved`/`text-conflict` (khi token thêm §2.4) |

### Prompt mẫu (5–8 — sub-agent Phase 1 copy-paste)

1. **Card nội dung chuẩn**: "Create a card: surface `bg-card`, border standard `border border-border`, radius `rounded-lg`, no shadow. Header padding `p-6`, title `text-foreground text-lg font-semibold tracking-tight` (H4), description `text-foreground-secondary text-sm`, content `p-6` with internal gap `gap-3` (12px)."

2. **Hero-stat (tối đa 1/màn)**: "Create the single hero stat: surface level-2 `bg-card-raised border border-border-subtle rounded-lg`, value as a large block-token — dark panel `bg-canvas-ink` with block `bg-data-core` + index `font-mono text-xs text-index-muted` below, value `text-foreground text-2xl font-semibold tracking-[-0.015em]`, unit/caption `text-foreground-tertiary text-xs font-mono`."

3. **Banner/hero không gradient**: "Build page banner: surface band level-2 `bg-card-raised border-b border-border-subtle`, no gradient, no blob. Left: page title H1 `text-4xl font-semibold tracking-[-0.03em]` + description `text-foreground-secondary text-sm`; optional right: mono strip of block-tokens (`bg-canvas-ink`, blocks `bg-data-core`, index `font-mono text-xs`) showing real data. Homepage hero runs a real mini simulation from `src/engines/catalog.ts` on `bg-canvas-ink`."

4. **Button chuẩn**: "Use `Button` from `ui/button` with `buttonVariants` only — default CTA `variant="default" size="lg"` (h-11 px-8, 44px), secondary actions `variant="outline" size="default"` (h-10 px-4), icon-only `size="icon"` (h-10 w-10) with `aria-label`, icon `lucide-vue-next` 16px `size-4` stroke-width 2, icon+text gap `gap-2` (8px). Never override padding or add shadow."

5. **Bảng dữ liệu**: "Create a data table: thead `text-sm font-medium text-foreground-tertiary h-10 text-left`, rows `border-b border-border hover:bg-muted/50`, cells `p-3` (12px), numeric columns (index, steps, Big-O, time) in `font-mono text-sm`, values in `text-foreground`; include skeleton, EmptyState (invitation copy per §9), and error row with retry button."

6. **Empty state**: "Use the shared `EmptyState` component: motif array `[ ]` (dark panel `bg-canvas-ink` with empty block outline), contextual `lucide-vue-next` icon, title `text-foreground font-medium`, copy = invitation with concrete action (§9: 'Chưa có bài benchmark nào — chạy thử bài đầu tiên'), one primary CTA button `size="default"`."

7. **Modal (Dialog)**: "Use `ui/dialog`: overlay `bg-background/80 backdrop-blur-sm`, content `bg-card border border-border rounded-lg shadow-lg p-6 max-w-lg` (shadow allowed only here), title H4, close icon button `size-9` ghost with `aria-label="Đóng"`, enter/exit via motion-v 200–300ms `cubic-bezier(0.16,1,0.3,1)` / `cubic-bezier(0.7,0,0.84,0)`."

8. **Stat-card phụ (không phải hero)**: "Create secondary stat cards (max 1 hero per screen): `bg-card border border-border rounded-lg`, no icon circle, no shadow; label `text-foreground-tertiary text-xs`, value `text-foreground text-2xl font-semibold tracking-[-0.015em]`, delta/unit `font-mono text-xs`; keep all cards on same elevation level-1."

**Quy tắc cuối**: không có token/scale trong file này → không tự chế — báo lại pm/ghi `docs/work/view-quality/notes.md`; mọi đổi mới token/màu/animation phải vào `docs/pm-decision-log-viewquality.md` TRƯỚC khi làm.

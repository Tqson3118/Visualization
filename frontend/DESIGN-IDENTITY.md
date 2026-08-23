# DESIGN IDENTITY — DSA Visual

> Nguồn: Phase 0 – BƯỚC A, `PROMPT_VIEW_QUALITY_MASTER_V2.md` (mục 2/3/4/5). Chất liệu kỹ thuật: `frontend/src/engines/` — catalog 44 mô phỏng (`catalog.ts`), trace bước thật (`core/stepExecutor.ts`), canvas bar-mode V3 (`renderers/arrayRenderer.ts` + `painter/canvasPainter.ts`), màu canvas (`renderers/canvasTheme.ts`), benchmark Web Worker (`worker/compileWorker.ts`). Người soạn: dev-docs · 13/08/2026.

## Pha 1 — Brainstorm

### 1.1 Motif / Concept

**"Phòng thí nghiệm dữ liệu" (Data Bench)** — app là nơi người học xây mental model về cây/đồ thị/stack bằng cách *thao tác trực quan với chính dữ liệu*, không phải đọc trang giáo trình. Quy ước 2 lớp xuyên suốt: **lớp dữ liệu** (canvas, code, giá trị cần đọc kỹ) luôn là *sân khấu tối* — block số phát sáng trên nền `canvas-ink`, đúng thứ app đã làm tốt nhất (bằng chứng 4.6); **lớp giao diện** (điều hướng, tiêu đề, CTA) là *giấy lặng* — phân cấp bằng luminance stacking, không gradient. Mọi thứ "đo được" của thuật toán (index, bộ đếm bước, Big-O) sống bằng mono.

### 1.2 Bảng màu (6 mã, mỗi màu có vai trò + lý do chủ đề)

| Hex | Tên (token) | Vai trò | Lý do gắn chủ đề |
|---|---|---|---|
| `#0D9488` | `accent` | **CHỈ interactive**: CTA, link, active, focus | Đã chốt từ trước; teal là "điểm chạm" — đủ sáng trên nền tối để kéo mắt nhưng không phải neon, đủ tương phản trên nền light. Cấm trang trí thụ động. |
| `#0D1020` | `canvas-ink` | Nền vùng dữ liệu (canvas, block-token, code-runner) | Lấy nguyên từ engine thật (`canvasTheme.ts` — `--canvas-bg`). Là "sân khấu tối" để block phát sáng; LUÔN tối bất kể theme toàn site (như editor/terminal), giữ đúng bản sắc đang có. |
| `#4255FF` | `data-core` | Màu block mặc định / giá trị dữ liệu | Chính là màu block số có bản sắc nhất hiện tại (`canvasTheme.ts` — block default). Xanh dương "điện tử" phân biệt rõ với teal interactive — dữ liệu ≠ nút bấm, không nhầm chức năng. |
| `#34D399` | `resolved` | Trạng thái "đã ổn định": sorted, tìm thấy, hoàn thành, đúng đáp án | Từ `canvasTheme.ts` (`--color-accent-green`). Trong thuật toán, xanh lá là trạng thái kết thúc của bước — tái dùng cho thành công ngoài canvas (hoàn thành bài, streak) để cả app nói 1 ngôn ngữ. |
| `#F87171` | `conflict` | Trạng thái "va chạm": swap, so sánh sai, lỗi, hết lượt | Từ `canvasTheme.ts` (`--color-accent-red`). Swap là lúc dữ liệu va chạm — đỏ gắn với xung đột/lỗi, khớp ngữ nghĩa thuật toán. |
| `#6B7385` | `index-muted` | Text thứ cấp trên nền tối: mono index, caption, giá trị phụ | Từ `canvasTheme.ts` (`--color-text-muted`). Chỉ số dưới block cần lặng lẽ, đủ đọc nhưng không tranh chấp với block — đồng thời là tầng text tertiary trên nền tối toàn app. |

**Luminance stacking**: nổi bậc bằng surface sáng dần (light) / tối dần + border semi-transparent (dark), không shadow dày; gradient chỉ hợp lệ khi mã hoá trạng thái (VD dải `data-core → resolved` thể hiện tiến trình sắp xếp), không trang trí vô nghĩa.

### 1.3 Typography

| Vai trò | Font | Quy tắc |
|---|---|---|
| Display (H1–H4, số hero) | Geist | Tiết chế: weight ≤600, tracking âm giảm dần theo size, không chữ in hoa dàn trải |
| Body / UI (đoạn, label, nút) | Geist | Trung tính: 400/500, line-height 1.5–1.6, không dùng 700 lung tung |
| Data (index, Big-O, bộ đếm bước, mã giả, code, giá trị trong block) | JetBrains Mono | Mọi thứ "đo được" đều mono — index dưới block, chip `O(n log n)`, bộ đếm `step 12/47`, phím tắt |

### 1.4 Layout concept

**Một câu**: "Dữ liệu lên sân khấu tối, giao diện ở lại giấy trắng — màn nào cũng là nơi *nhìn vào dữ liệu*, không phải nhìn vào trang trí."

```
DASHBOARD (profile/overview)          LUỒNG HỌC BÀI (lesson / node hub)
┌──────────────────────────────┐      ┌──────────────────────────────┐
│ header: hearts/gems · mono   │      │ breadcrumb mono: node 04/08  │
├──────────────────────────────┤      │ H1 bài học (Geist)           │
│ H1 Geist · chip mono streak  │      │ ┌───────────┐ ┌────────────┐ │
│ ┌────────┐ ┌───────────────┐ │      │ │ lý thuyết │ │ canvas tối │ │
│ │1 stat  │ │ mini canvas   │ │      │ │ + Big-O   │ │ block demo │ │
│ │hero    │ │ tối — block   │ │      │ │ chip mono │ │ thở theo   │ │
│ │/màn    │ │ thở theo bước │ │      │ └───────────┘ │ bước thật  │ │
│ └────────┘ └───────────────┘ │      │               └────────────┘ │
│ XP history: block-token+idx  │      │ stepper: Lý thuyết→Luyện→KT │
└──────────────────────────────┘      └──────────────────────────────┘

EDITOR / MÔ PHỎNG (simulator, code)   DANH SÁCH (khám phá, path, BXH)
┌──────────────────────────────┐      ┌──────────────────────────────┐
│ toolbar: play/step/step-back │      │ header + tìm kiếm · count    │
│          counter mono        │      │ (mono)                       │
│ ┌──────────────────────────┐ │      │ ┌──────┐ ┌──────┐ ┌──────┐   │
│ │ CANVAS TỐI — block[0..n] │ │      │ │card  │ │card  │ │card  │   │
│ │        index mono        │ │      │ │chip  │ │chip  │ │chip  │   │
│ └──────────────────────────┘ │      │ │Big-O │ │Big-O │ │Big-O │   │
│ ┌─ mã giả mono ─┐ ┌─stack──┐ │      │ └──────┘ └──────┘ └──────┘   │
│ │ dòng HL theo │ │ mono   │ │      │ skeleton block · empty [ ]    │
│ │ bước thật    │ └────────┘ │      └──────────────────────────────┘
└──────────────────────────────┘
```

### 1.5 Signature — "Block thở theo bước" (Step-Reactive Block)

Một chi tiết duy nhất, tái dùng ở mọi nơi: **mỗi giá trị dữ liệu trên app là một block biết trạng thái thuật toán của chính nó** — block chỉ "diễn" khi thuật toán thực sự chạm nó.

- **Nguồn sự kiện duy nhất**: trace thật từ `stepExecutor` (Babel-instrumented) — mỗi bước phát `{target, action: compare|swap|write|resolve}`; UI bên ngoài canvas (XP history, benchmark, streak, leaderboard) đăng ký cùng nguồn thay vì tự bịa hiệu ứng.
- **Nhịp thở**: block đang compare — viền `data-core` nhấp nháy, tần số tăng dần khi tiến gần kết quả; block swap — 2 block đổi chỗ thật bằng spring ngắn (transform/opacity, 150–250ms, `cubic-bezier(0.16,1,0.3,1)` hoặc spring stiffness≈170/damping≈26); block resolve — chuyển `resolved` và "chốt" 1 nhịp settle.
- **Điểm nhớ**: mọi block đều có **chỉ số index mono** bên dưới — "dữ liệu luôn được đánh số" — lặp lại ngoài canvas (VD streak ngày 12·13·14, dòng XP #03), là dấu vân tay chỉ app này mới có (bằng chứng 4.6).
- **Kỷ luật**: không animation rảnh rỗi — block im lặng khi không bị thuật toán chạm; cả app chỉ có đúng 1 ngôn ngữ trạng thái (default/compare/swap/resolve) dùng cho cả canvas lẫn card.

## Pha 2 — Tự phản biện (3 lần sửa vì "quá chung chung")

1. **Bảng màu nháp đầu** ghi "teal accent + surface trắng + text xám" → hỏi "AI khác build dashboard bất kỳ có ra y hệt không?" — **CÓ** → bỏ, thay bằng palette bám engine thật (6 màu trên, 4 màu có nguồn file `canvasTheme.ts`, mỗi màu trả lời được "trạng thái thuật toán nào?").
2. **Signature nháp đầu** là "hover mượt + stagger reveal khi cuộn" → chính là fade+slide cơ giới trong KILL-LIST → bỏ, thay bằng block phản ứng *bước thật* — thứ không thể ghép vào dashboard bất kỳ vì nó đòi trace thuật toán thật.
3. **Layout nháp đầu** viết "sidebar trái + content phải" chung chung → cụ thể hoá thành quy tắc 2 lớp (sân khấu tối vs giấy trắng) + wireframe 4 loại màn. *Ghi chú chống nhầm KILL-LIST*: palette tối + xanh/đỏ/xanh lá KHÔNG phải cụm "nền đen + 1 accent chói" — 3 màu trạng thái (data-core/resolved/conflict) là *ngôn ngữ thuật toán*, accent UI duy nhất là teal (chỉ interactive), nền tối chỉ ở vùng dữ liệu, và toàn bộ là chất liệu đang chạy thật trong engine.

## Pha 3 — Checklist quyết định xuyên-nhóm (chốt 6/6)

| Quyết định | Chốt | Ghi vào |
|---|---|---|
| Banner/hero mỗi trang | **Bỏ gradient trang trí ngẫu nhiên**; banner = surface band + luminance stacking + strip mono dữ liệu; hero trang chủ chạy mô phỏng THẬT mini từ `catalog.ts` (3 demo công khai) | DESIGN.md §1 |
| Icon library | **`lucide-vue-next` duy nhất** (chuẩn shadcn-vue, đã cài ^1.0.0); loại dần `@lucide/vue` + `@phosphor-icons/vue` — báo pm vì đụng dependency | DESIGN.md §4 |
| Stat-card hierarchy | Tối đa **1 stat hero/màn** (block-token lớn + elevation cao); còn lại lùi 1 bậc, không icon tròn đổi màu lung tung | DESIGN.md §6 |
| Block-token tái sử dụng | Nơi NÊN dùng: XP history, streak calendar, benchmark result, quiz result, leaderboard rank, mọi dữ liệu tuần tự/có chỉ số — 1 class chung, kèm index mono | DESIGN.md §1 |
| Motif nền tối ở light mode | **Vùng dữ liệu LUÔN tối** bất kể theme (simulator, code-runner, block-token); phần còn lại theo theme — như editor/terminal | DESIGN.md §1 + §6 |
| Công cụ animation | Theo bảng phân công BƯỚC E: route = View Transitions API (fallback motion-v); enter/exit = motion-v; list reorder = `@formkit/auto-animate`; scroll reveal = GSAP ScrollTrigger; ăn mừng = `canvas-confetti` (đã cài); node-edge lộ trình = `@vue-flow/core` (chờ xác nhận session Diagrams — 0.5.1); lenis chọn lọc 1–2 trang | DESIGN.md §1 + decision log |

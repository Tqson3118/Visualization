# FIX-LOG — View quality Phase 1 (Nhóm B: 4 view học tập)

> Ghi trước/sau cho từng view (13/08/2026 · dev-engine). Chi tiết lỗi: `audit-<view>.md`. Quyết định: `docs/pm-decision-log-viewquality.md`.

## PathView `/path/:topicId`

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 59.5/100 | 86/100 |
| Đặc trưng | 2/10 | 8.5/10 |
| Trục dưới sàn | animation 7 < 8.4; thị-giác 5 < 8.4 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7) |

Đã sửa:
1. **Node-edge graph thật**: chuyển pill Duolingo → VueFlow (`PathGraph.vue` lazy `defineAsyncComponent` + `PathFlowNode.vue` block-token). Node = canvas-ink + index mono `NODE 01/08` + chip trạng thái (ĐANG HỌC/ĐÃ QUA/KHÓA) + icon lucide (Play/CheckCircle2/Lock/Flag); edge smoothstep, màu `resolved` khi node nguồn đã qua; snake 2 cột desktop / cột đơn <640px (không tràn 390). Bundle entry 106.44 → 106.59 kB (delta +0.15 kB); vue-flow 154.55 kB nằm trong lazy chunk PathGraph (số đầy đủ ở decision log).
2. **Banner**: bỏ gradient aurora + shadow-md → surface band level-2 (`bg-card-raised` + border-subtle); kicker mono `LEARNING PATH · TOPIC 01`; H1 + icon `Route` (bỏ 🎯); progress panel level-1 + bộ đếm mono `00/04`.
3. **Emoji → lucide**: 🎯🔒⭐▶🏁❤ → Route/Lock/Play/CheckCircle2/Flag/Heart (5 chỗ).
4. **Raw `<button>` (2) → role="button" div trong custom node** (canvas exception, decision log mục 2) + keyboard Enter/Space kích hoạt node (tested).
5. **Weight 700 → 600/500** (kicker, label, node); tracking dương chỉ còn trên label mono ngắn.
6. **Easing popover**: `ease 200ms` → enter `cubic-bezier(0.16,1,0.3,1)` 200ms / exit `cubic-bezier(0.7,0,0.84,0)` 150ms + prefers-reduced-motion.
7. **Hover node**: bỏ `hover-lift` (lift+shadow) → border-color 150ms chuẩn.

Kiểm chứng runtime (playwright, BE :5000 thật, /path/1): graph 4 node + 4 edge + final node render; click + Enter mở popover đúng; 0 console error/warn; 0 overflow 390px; light+dark đúng token.

## LessonView `/learn/:lessonId`

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 67/100 | 87.5/100 |
| Đặc trưng | 3/10 | 8/10 |
| Trục dưới sàn | animation 6 < 8.4; thị-giác 6 < 8.4 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Banner**: bỏ gradient sunset + text trắng + text-shadow + shadow-lg → surface band level-2; bỏ lớp `.dark` phủ (vá gradient) và `::after` overlay.
2. **Nút "Học tiếp"**: hành động giả (toast placeholder) → mở mô phỏng đầu tiên của bài thật (tested: /learn/1 → /simulator/sort.bubble); không có simulation → chuyển tab Lý thuyết.
3. **"← Về lộ trình"** → lucide `ArrowLeft`.
4. **Weight 700 → 600/500** (theory-meta dd, quiz-title); quiz-icon tint primary → neutral `bg-muted` (accent chỉ interactive).
5. **Hover card quiz**: bỏ `hover-lift` → border-color transition chuẩn.
6. **Breadcrumb mono**; theory card bỏ shadow-sm; pre/code giữ token.

Ghi nhận (ngoài phạm vi view): content CMS còn emoji (🎯 "Sắp xếp cơ bản", 👉, 📚 trong `contentHtml` của bài 1) — là dữ liệu nội dung qua `v-html`, không phải code view → Phase 2 (biên soạn nội dung / sanitize).

## SimulatorView `/simulator/:key`

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 61.5/100 | 84.5/100 |
| Đặc trưng | 6/10 | 8.5/10 |
| Trục dưới sàn | thị-giác 4 < 8.4; interactive 8 < 9.6 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Chrome header**: bỏ gradient-mint + blob blur + shadow + text-gradient title → surface band level-2; H1 text-3xl; subtitle có chip Big-O mono.
2. **4 raw `<button>` → Button.vue**: favorite/share = `size="icon"` 40×40 (Button.vue thêm size icon — decision log mục 3) + aria-pressed; toggle Call stack/Legend = ghost sm + lucide ChevronDown/ChevronRight (bỏ ▾▸).
3. **Canvas frame = motif tối lan tỏa**: khung canvas-wrap `bg-canvas-ink` + border index-muted 45%; meta bar mono index-muted (bỏ 6px/10px/2px hardcode → token).
4. **ControlBar/StatsBar (toolbar)**: indicator + step chip bỏ gradient-mint → `bg-primary` + mono; bỏ shadow-sm (control-bar, play, bp-badge); weight 700 → 500/600; `#fff` → token.
5. **States**: `__error/__empty/__loading` bỏ class `.card` (shadow-md) → `simulator__panel` (bg-card, border, rounded-lg).
6. **Footer phím tắt → mono** (thứ "đo được").

Giữ NGUYÊN: CanvasArea + engine + renderer (không đụng). Ghi nhận Phase 2: PseudocodePanel/LegendPanel/ExplainPanel/ManualPracticePanel/DemoBanner còn gradient/hex/700 (ngoài phạm vi khung ngoài).

## ExerciseView `/exercise/:id`

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 78/100 | 86.5/100 |
| Đặc trưng | 3/10 | 7.5/10 |
| Trục dưới sàn | đặc trưng < 7 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Toast**: bỏ emoji 🎉 → "Hoàn thành bài tập!".
2. **Toolbar**: bỏ class `.card` (shadow-md) → surface band level-2; kicker mono (bỏ 700 + tracking dương rời); H1 text-2xl + tracking âm.
3. **Nút toggle** thêm `aria-pressed` (trạng thái bật/tắt cho SR).
4. **Breadcrumb mono**.
5. Thêm skeleton/empty/error đã có sẵn — giữ.

Ghi nhận: QuizStage (component dùng chung) là phần lớn UI — nâng đặc trưng tiếp ở Phase 2 nếu cần ≥8.

## Kết quả verify
- `npm run build` (vue-tsc + vite): **PASS** — entry index 106.59 kB (trước 106.44, delta +0.15 kB); PathGraph lazy 154.55 kB.
- `npm test`: **95/95 PASS**.
- Smoke runtime (dev :5176 + BE :5000, đăng ký user thật): 4 view render đúng light+dark; PathView graph node-edge hoạt động; 0 console error/warn (trừ 401 refresh khi chưa đăng nhập — hành vi cũ của app); 0 overflow 390px; button 40×40 icon, node keyboard Enter OK.
- Ollama 3-gate (qwen2.5vl:3b): 5/6 ảnh Gate 1 = CÓ (nhận diện app DSA); Gate 2 = không lỗi UI rõ. Gate 3 model không ra điểm số sạch (rambling) — ghi log thô, dựa vào audit chủ quan cho đặc trưng.

## LabView /ladder/:nodeId/lab

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 67/100 | 86/100 |
| Đặc trưng | 5/10 | 8/10 |
| Trục dưới sàn | thị-giác 7 < 8.4 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Banner surface band level-2**: bỏ icon tile gradient-mint + shadow; thêm kicker mono INTERACTIVE LAB · NODE 01 + H1 text-3xl tracking âm + sub text-sm; breadcrumb mono.
2. **Info cards**: bỏ hover shadow-md + translateY → hover border-color 150ms easing chuẩn; icon tile gradient → g-muted + icon foreground-secondary 16px; title text-lg/600.
3. **LabStage canvas = sân khấu tối** (quyết định #5): nền canvas-ink, block data-core 18% + border data-core, index mono 12px index-muted, done → esolved; bỏ gradient cell--done + shadow.
4. **Bỏ emoji 🎉** (toast + win message) → text thuần; bỏ weight 800/700 → 600/500.
5. **"← Về Ladder"** → lucide ArrowLeft + Button ghost (bỏ ký tự ←).
6. **Cell raw button** (canvas exception — decision log): thêm ria-pressed + ria-label "Ô x: giá trị y"; bỏ shadow hover → border-color + translateY(-1px); easing chuẩn + prefers-reduced-motion.
7. **EmptyState component chung** cho LabStage khi không có dữ liệu (motif [ ] + icon database + copy §9).

Kiểm chứng runtime (playwright, BE :5000 + dev :5176, /ladder/1/lab): banner + 3 info cards + 6 cell block render; computed style xác nhận canvasBg rgb(13,16,32) = canvas-ink, cellBg data-core 18%, idx JetBrains Mono #6B7385, cellWeight 600; 0 console error/warn; 0 overflow 390px (375 < 390); light + dark screenshot lưu ollama-log/lab-{light,dark}.png.

## CodeRunnerView /code/:key

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 71/100 | 88.5/100 |
| Đặc trưng | 6/10 | 8.5/10 |
| Trục dưới sàn | thị-giác 6.5 < 8.4 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Bỏ ghi chú dev lộ UI** (r2-fixed-09): "* Monaco editor sẽ được bật khi cài gói monaco-editor (SDD Màn 16...)" → thay bằng caption phím tắt mono hữu ích: "Ctrl+Enter chạy code · Ctrl+Z hoàn tác · Ctrl+Shift+Z làm lại" (kbd chuẩn).
2. **Chrome header**: bỏ gradient-mint + border tint primary + shadow-md + ::after overlay → surface band level-2 (bg-card-raised + border-border-subtle); kicker mono CODE CHALLENGE · key; H1 text-3xl text-foreground (bỏ 💻 + text-gradient); sub text-sm text-secondary.
3. **Vùng code LUÔN tối** (quyết định #5): editor-wrap canvas-ink + gutter tối + textarea text color-mix(white 85%, index-muted) mono 14px (bỏ 13px + nền theo theme); output-box cũng canvas-ink.
4. **Panel bỏ shadow-sm** → border chuẩn (elevation level-1); panel-title weight 700→600, icon 15→16px, bỏ tint primary trên svg.
5. **◀ ▶ → lucide StepBack/StepForward** + Button size icon + aria-label; "▶ Chạy" → lucide Play; icon History 16px.
6. **Empty state → EmptyState component chung** (motif [ ] + icon database + copy §9 "Không tìm thấy bài thử thách" + action "Về danh mục").
7. Spacing: gap 6px→8px (gap-2), margin 4px→space-xs, textarea padding 14px→16px, gutter 48px + padding token.
8. Breadcrumb mono; lịch sử nộp bỏ class .card (shadow) → panel border chuẩn + date mono.

Kiểm chứng runtime (playwright, /code/sort.bubble + /code/not-a-real-key): editor-wrap computed bg rgb(13,16,32), taColor sáng, chromeBg card-raised + boxShadow none + backgroundImage none; noteText = phím tắt (không còn Monaco); empty state render motif [0 1 2] + action; 0 console error; 0 overflow 390px; light+dark screenshot ollama-log/code-{light,dark}.png. Giữ nguyên: textarea aria-label + "Thành công · Xms" (e2e).

## BenchmarkView /benchmark/:k1/:k2 (+ BenchmarkPanel — nội dung view, decision log P1-B3 mục 1)

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 65/100 | 87.5/100 |
| Đặc trưng | 5/10 | 8/10 |
| Trục dưới sàn | thị-giác 5 < 8.4; depth 4.5 < 4.8; interactive 9.5 < 9.6 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Chrome header**: bỏ gradient-mint + border tint + shadow-md + ::after overlay → surface band level-2 (bg-card-raised + border-subtle) + kicker mono `BENCHMARK · sort.bubble vs sort.merge` + H1 text-3xl/600/-0.02em (bỏ text-gradient).
2. **Label "Miễn phí tim (20.4)"** (BenchmarkPanel.vue:282 + BenchmarkView.vue:50): KHÔNG có trong i18n — hardcode; "(20.4)" là số mục SDD lộ UI → **"Không tốn tim"** (rõ nghĩa: benchmark không trừ tim người học; VD "Tìm kiếm tuyến tính — 20.4ms" trong prompt không đúng ngữ nghĩa màn hình — decision log mục 2).
3. **Empty state**: icon "scissors" (cây kéo vô nghĩa) → EmptyState chung icon "hourglass" (đo thời gian) + copy §9 "Chưa có số liệu đo — chọn 2+ giải thuật phía trên rồi bấm Chạy benchmark…".
4. **Chips raw `<button>` (5-17)** → Button.vue sm variant primary/secondary + aria-pressed; bỏ gradient chip + shadow-sm + translateY hover; easing --transition-fast → border-color 150ms cubic-bezier chuẩn.
5. **Kết quả benchmark = vùng dữ liệu LUÔN tối**: bảng + chart + kết luận nền canvas-ink; n = index mono, duration = block-token data-core (border/14%), so sánh = mono index-muted; palette ECharts đọc CSS var canvas palette (data-core/resolved/conflict/warning/info), fallback hex chỉ phòng SSR (pattern cssVar CodeRunnerView).
6. **Mobile ≤640px**: bảng → card-stack (td::before data-label mono), bỏ scroll ngang bảng chính (§8).
7. ⚖/▶ → lucide Scale/Play; weight 800 → 600/500; select chuẩn hóa 40px; breadcrumb mono; kết quả region enter 250ms easing chuẩn + prefers-reduced-motion.

Kiểm chứng runtime (chrome-devtools, /benchmark/sort.bubble/sort.merge — đăng ký user QA mới): banner surface band (dark #134E4A / light #F7FDFD, boxShadow none, backgroundImage none); chips Button sm + aria-pressed; chạy benchmark thật → bảng tối rgb(13,16,32) + block data-core (bg 14%/border 45%/mono) + n index mono 12px + chart + conclusion tối; console 0 error/warn/issue (đã fix ECharts containLabel → LegacyGridContainLabel + select name); 390px card-stack (thead none, tr grid, td::before data-label, wrap overflow visible, scrollW=clientW=390). Screenshot ollama-log/benchmark-{light,dark,mobile-light}.png — Gate1 CÓ, Gate2 sạch, Gate3 8/10.

## PathRedirectView /path (redirect từ /learn — topic selector 5 chủ đề)

| Hạng mục | Trước | Sau |
|---|---|---|
| Hygiene | 72.5/100 | 86/100 |
| Đặc trưng | 4/10 | 8/10 |
| Trục dưới sàn | thị-giác 6.5 < 8.4; đặc trưng 4 < 7 | KHÔNG còn |
| Đạt/không | KHÔNG ĐẠT | **ĐẠT** |

Đã sửa:
1. **Banner**: header phẳng + 🎯 → surface band level-2 + kicker mono "LEARNING PATH · CHỌN CHỦ ĐỀ" + H1 text-3xl/600/-0.02em + Route icon lucide.
2. **Card topic**: `<article role="button" tabindex="0">` (thiếu Space) → RouterLink (Enter/Space/focus-visible native); bỏ openTopic() (logic qua router).
3. **Bỏ .card/.card--interactive** (shadow-md + hover shadow-lg/translateY/scale) → card level-1, hover border-color 150ms chuẩn.
4. **Index vòng tròn primary + weight 800** → kicker mono index-muted `TOPIC 01/05` trong card (bỏ accent trang trí).
5. **EmptyState icon "map"** (KHÔNG tồn tại trong SVG_PATHS → fallback x-circle âm thầm) → "book".
6. Skeleton 88px → 150px khớp card thật; card grid stagger enter 240ms (max 8×40ms) + prefers-reduced-motion; card title text-lg/600; note fallback text-tertiary xs.

Kiểm chứng runtime (chrome-devtools, /path): banner band + 5 card RouterLink (href /path/1..5, focus-visible ring, Enter/Space native); computed: chrome bg card-raised (light #F7FDFD / dark #134E4A), shadow none, card shadow none, kicker JetBrains Mono 12px tertiary, H1 36px/600/tracking -0.72px; 0 console error; 0 overflow 390px (scrollW=clientW=390); 5/6 ảnh Ollama Gate1 CÓ (model nhiễu "emoji" trên 2 ảnh — notes.md mục P1-B3.2). Screenshot ollama-log/path-redirect-{light,dark,mobile-dark}.png.

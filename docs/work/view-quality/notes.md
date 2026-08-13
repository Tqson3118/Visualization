# NOTES — View quality Phase 1 (Nhóm B) · ghi chú cho Phase 2

> 13/08/2026 · dev-engine. Ghi chú ngoài phạm vi đã gặp, không tự sửa.

## Component dùng chung bị đụng tối thiểu
1. `frontend/src/components/ui/Button.vue` — thêm `size: 'icon' | 'icon-sm' | 'icon-lg'` (map thẳng `buttonVariants` §4.1, backward compatible). Lý do: SimulatorView favorite/share cần nút icon chuẩn 40×40; tránh 4 raw `<button>`.
2. `frontend/src/components/simulator/ControlBar.vue` + `StatsBar.vue` — bỏ gradient chip → primary solid + mono; bỏ shadow. Đây là "toolbar/panel thông tin" của SimulatorView (trong phạm vi khung ngoài).
3. `frontend/src/components/ui/Card.vue` (wrapper) + shadcn `card/Card.vue` vẫn có `shadow-sm` mặc định + `hover:-translate-y-0.5 hover:shadow-lg` cho interactive — LỆCH DESIGN.md §4.2/§6 (card cấm shadow, hover chỉ đổi border). KHÔNG sửa global vì ảnh hưởng 36 view — để Phase 2 đổi 1 nơi.

## Nội dung CMS còn emoji (không phải code view)
- Bài học 1 `contentHtml` chứa 🎯 ("Sắp xếp cơ bản"), 👉, 📚 — render qua `v-html` (LessonDetail). Emoji trong nội dung bài giảng = dữ liệu, không phải icon chức năng UI. Phase 2: quy ước biên soạn nội dung / sanitize, KHÔNG sửa trong view.

## Component simulator còn vi phạm nhẹ (ngoài khung ngoài)
- `PseudocodePanel.vue`: gradient (1), shadow (4), weight 700 (2), 2 raw `<button>` (breakpoint toggle — dạng editor/table-cell, cần decision log).
- `LegendPanel.vue`: 6 hex rời (màu legend canvas — nên đọc từ `canvasTheme.ts`).
- `ManualPracticePanel.vue` / `DemoBanner.vue`: weight 700.
→ Phase 2 khi có task riêng cho simulator panels.

## Đặc trưng ExerciseView
- Đạt 7.5 nhờ toolbar Data Bench (kicker mono + surface band) nhưng QuizStage (component chung) vẫn generic quiz. Muốn ≥8: Phase 2 sửa QuizStage (block-token cho câu hỏi/đáp án, mono cho số câu).

## Ollama 3-gate
- qwen2.5vl:3b context 4096 — ảnh fullpage > ~120KB fail; phải resize 1024px cho LessonView. Gate 3 (điểm đặc trưng) model trả lời lan man không ra số sạch — log thô giữ nguyên tại `ollama-log/`, điểm đặc trưng chính thức lấy từ audit chủ quan (đã ghi).
- Task P1-B3 (BenchmarkView + PathRedirectView): benchmark-{light,dark,mobile-light} + path-redirect-{light,dark,mobile-dark} → Gate 1 = CÓ (nhận diện app DSA Visual) 5/6 ảnh; 1 ảnh (benchmark-mobile-light) model trả lời lưỡng lự "không phải app học DSA" dù tự nhận diện "app DSA Visual" (màn mobile chỉ thấy card-stack + không thấy canvas). Gate 2: benchmark sạch; path-redirect model báo "có emoji icon (icon của các topic)" + "layout lệch" — **nhiễu model** (grep + DOM snapshot xác nhận view không còn emoji, không overflow 390px; model có thể nhầm icon Route lucide/progress thành emoji — đúng hạn chế model đã ghi). Gate 3: 7–8/10 (khớp audit chủ quan 8/10).

## Tooling
- Playwright MCP ghi screenshot vào cwd server = `D:\FPT\neww` (main worktree) — đã dọn sạch artifacts sau khi chạy; nếu chạy lại, đổi output dir.
- Dev server đã chạy ở :5176 (proxy /api → :5000, BE đang chạy).

## Task P1-B2 (LabView + CodeRunnerView) — ngoài phạm vi đã gặp
1. **Icon "🪜 bậc thang vỡ glyph" (r2-fixed-07)** nằm ở LadderView.vue:85 (🪜 Practice Ladder) — LadderView là view KHÁC, ngoài phạm vi task 2-view; empty state icon puzzle (icon="puzzle") ở QuizStage.vue:157 (Bậc 1). → Task LadderView (ưu tiên CAO, scorecard còn trống) xử lý: 🪜 → lucide, quiz empty state dùng EmptyState chung với icon theo ngữ cảnh.
2. **BenchmarkPanel.vue:289** còn <button> raw chip chọn thuật toán (BenchmarkView) — task BenchmarkView xử lý.
3. LabStage.vue cell là <button> raw nhưng nằm trong vùng canvas dữ liệu (decision log mục 2) — hợp lệ theo standard.md trục 5, đã thêm aria-pressed + aria-label.
4. --color-text-primary light theme = ar(--color-foreground) (#134E4A) — trên nền canvas-ink là quá tối; các view mới dùng color-mix(white 85%, index-muted) cho text vùng dữ liệu. Phase 2: thêm token --canvas-text chính thức.

## Task P1-B3 (BenchmarkView + PathRedirectView) — ngoài phạm vi / cho Phase 2
1. **ECharts 6 deprecation**: `grid.containLabel` đã bị thay bằng `LegacyGridContainLabel` (import `echarts/features`) — đã sửa trong BenchmarkPanel để hết warning console. View khác dùng ECharts với containLabel → áp dụng tương tự.
2. **Ollama Gate 2 nhiễu trên path-redirect**: model báo "emoji icon (icon của các topic)" + "layout lệch" dù grep + DOM snapshot sạch (view không còn emoji, không overflow 390px) — thêm 1 bằng chứng cho hạn chế qwen2.5vl:3b đã ghi; không hành động theo.
3. **`--color-card-raised` + theme khi test**: app có watcher tự bật/tắt class `dark` theo uiStore (localStorage) — thao tác class tay bị đè; đồng thời getComputedStyle đọc trong CÙNG evaluate với toggle class có thể trả giá trị stale (đọc lại ở evaluate kế tiếp là chính xác). Không phải bug view.
4. **Card `minmax(300px,1fr)`** ở /path cho 5 card → 3+2 cột tại 1200px container — grid auto-fill chuẩn, chấp nhận (model gọi "layout lệch" là nhầm).

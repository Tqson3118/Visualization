# G-F2c — POLISH 5 MÀN CHÍNH (nhóm 2)

> Ngày: 12/08/2026 · Nhánh: `feature/ux-polish` (base e5cebad = G-F2a + G-F2b)
> Phạm vi: Simulator chrome `/simulator/:key` · Ladder `/ladder/:nodeId` · Lab `/ladder/:nodeId/lab` · Code Runner `/code/:key` · Benchmark `/benchmark/:k1/:k2` (BAO_CAO_SPEC §6.2 — 05/14/15/16/17).
> Nguồn đặc tả: SDD Màn 05 (wireframe 7.8) + Màn 14/15/16/17 (§20.2.2) + pm-decision-log-g.md Phase 2.
> KHÔNG đụng `engines/*` + `renderers/*` + `components/simulator/CanvasArea.vue` + stepExecutor + worker.

## Tóm tắt kết quả

| # | Màn | Route | Trạng thái | Điểm nâng cấp chính |
|---|-----|-------|-----------|---------------------|
| 1 | 05 — Simulator chrome | `/simulator/:key` | ✅ DONE | **Chrome header Cyber Mint** (gradient mint → teal + overlay đọc được cả light/dark + đốm sáng) — chrome UI, **KHÔNG đè canvas**. Nút favorite/share → icon lucide `Star`/`Share2` (giữ aria-label). **ControlBar**: shadcn Button + lucide `SkipBack/SkipForward/Play/Pause/RotateCcw/Gauge`, indicator bước chạy gradient mint chip, select tốc độ style đẹp. **StatsBar**: chip icon lucide (`ListOrdered`/`ArrowRightLeft`/`Repeat`/`PenLine`), chip "Bước" nền mint. **PseudocodePanel**: gutter số dòng + **syntax highlight nhẹ** (keyword/number/string/comment) + active line viền mint + chip biến mint. Vùng vẽ bọc backdrop/canvas-wrap (padding/border/backdrop) — **CanvasArea NGUYÊN VẸN**, ResizeObserver fix giữ nguyên. |
| 2 | 14 — Ladder | `/ladder/:nodeId` | ✅ DONE | **Hero gradient Sunset** (palette 2) + breadcrumb + badge "Đang học · Node N". **Stepper 3 bậc**: số tròn gradient + **Badge trạng thái** (Đang học/Đã qua/Khóa) + đường nối giữa các bậc + hover. **GIỮ** `<ol aria-label="Các bậc luyện tập">` + nút Quiz/Lab/Code + disabled khi locked (e2e). Stage bọc trong **Card shadcn + hover-lift nhẹ**. Logic stage G-BF2 (tải exercise qua nodeId&stage) giữ nguyên. |
| 3 | 15 — Lab | `/ladder/:nodeId/lab` | ✅ DONE | Hero + **3 thẻ Mô tả bài / Mục tiêu / Hướng dẫn** (Card shadcn + icon lucide `BookOpen`/`Target`/`Lightbulb` + hover). **LabStage**: cell đẹp hơn (selected viền warning, **done = gradient mint** + shadow), canvas dashed border đổi màu khi sắp xong, feedback/win/fail là pill màu; **confetti `node-pass`** khi qua Bậc 2 (tôn trọng prefers-reduced-motion). Logic chấm trạng thái cuối + giới hạn bước giữ nguyên. |
| 4 | 16 — Code Runner | `/code/:key` | ✅ DONE | **Layout 2 cột** (Editor trái / Output phải). **Editor**: gutter số dòng đồng bộ scroll + textarea font JetBrains Mono + nền dark slate + caret mint (giữ textarea thật — Monaco chưa cài full). Toolbar **shadcn Button** Chạy/Khôi phục. **Output panel**: EmptyState idle ("Chưa có kết quả") + error pill đỏ + success pill xanh (GIỮ text `Thành công · Xms` — e2e) + pre output JSON + canvas 2 chiều + StatsBar + sim controls. GIỮ textarea `aria-label="Trình soạn mã {key}"`. |
| 5 | 17 — Benchmark | `/benchmark/:k1/:k2` | ✅ DONE | **Đã dùng ECharts** (vue-echarts 8.1 + echarts 6.1, tree-shaking `use([CanvasRenderer, LineChart, GridComponent, LegendComponent, TooltipComponent])`): line chart **DurationMs theo n, overlay nhiều thuật toán**, tooltip axis + legend + màu đọc từ CSS variables (đổi theme không cần reload, phụ thuộc `ui.theme`), `animation: !prefersReducedMotion`. **GIỮ bảng dữ liệu + conclusion + CSV export + fit lý thuyết (complexityClass) + EmptyState**. Hero chrome mint + badge "Miễn phí tim" + chips chọn thuật toán gradient mint. |

## Quyết định ECharts vs SVG (benchmark)

**ĐÃ DÙNG vue-echarts** — khả thi vì:
- `vue-echarts@8.1.0` + `echarts@6.1.0` đã có sẵn trong `dependencies` (chỉ chưa được dùng).
- BenchmarkPanel **không có unit test / e2e** → tự do thay SVG.
- ECharts sinh canvas — không bị style CSS can thiệp; màu giải bằng `getComputedStyle` đọc CSS var (cùng pattern `engines/renderers/canvasTheme.ts` — KHÔNG sửa file engines).
- Chunk BenchmarkPanel ~531 kB (gzip 180 kB) là **route lazy-load** riêng → không ảnh hưởng khởi động trang khác; chấp nhận (chỉ cảnh báo build, không fail).

## File tạo / sửa

**Sửa (13 file):**
- `frontend/src/views/SimulatorView.vue` — chrome Cyber Mint + subtitle + canvas-wrap + lucide icons (giữ `.simulator__title` + ControlBar indicator).
- `frontend/src/components/simulator/ControlBar.vue` — shadcn Button + lucide; GIỮ accessible name "Bước tới/Bước lùi/Chạy/Tạm dừng/Đặt lại" + `.control-bar__indicator`.
- `frontend/src/components/simulator/StatsBar.vue` — chip icon lucide + chip bước mint (giữ aria-label "Thống kê").
- `frontend/src/components/simulator/PseudocodePanel.vue` — gutter + syntax highlight nhẹ + active line mint (giữ aria-label "Mã giả").
- `frontend/src/views/LadderView.vue` — hero Sunset + badge (giữ heading /Practice Ladder/).
- `frontend/src/components/ladder/LadderStepper.vue` — badge trạng thái + đường nối + số tròn (giữ ol aria-label + disabled).
- `frontend/src/components/ladder/LadderShell.vue` — Card shadcn bọc stage.
- `frontend/src/views/LabView.vue` — 3 thẻ mô tả/mục tiêu/hướng dẫn.
- `frontend/src/components/ladder/LabStage.vue` — cell/style + confetti 'node-pass'.
- `frontend/src/views/CodeRunnerView.vue` — layout 2 cột editor/output + gutter số dòng + EmptyState/error style (giữ textarea aria-label + text "Thành công · Xms").
- `frontend/src/components/benchmark/BenchmarkPanel.vue` — **SVG → vue-echarts** line chart + header/chips mint.
- `frontend/src/views/BenchmarkView.vue` — chrome hero mint + breadcrumb + badge.
- `frontend/src/i18n/vi.ts` — thêm `simulator.subtitle`.

**Sửa (test-infra — KHÔNG đổi production):**
- `frontend/tests/e2e/helpers/mockApi.ts` — bổ sung mock `GET /exercises?nodeId&stage` (LadderView Bậc 1/3) + `POST /benchmarks/run` (lưu kết quả đo) → loại bỏ console 404 khi verify browser; 11 e2e hiện có vẫn PASS.

**Tạo mới (ảnh verify §6.2 — docs/work/):**
- `g-f2c-01-simulator.png`(+`-dark`), `g-f2c-02-ladder.png`(+`-dark`), `g-f2c-03-lab.png`(+`-dark`), `g-f2c-04-code-runner.png`(+`-dark`), `g-f2c-05-benchmark.png`(+`-dark`), `g-f2c.md`.

## Verify

- ✅ `npm run build` — 0 lỗi (vue-tsc + vite; chỉ cảnh báo chunk BenchmarkPanel > 500 kB).
- ✅ `npm test` — **72/72 PASS** (8 files).
- ✅ `npx playwright test` — **11/11 PASS** (auth 3, code-runner 3, ladder 2, simulator 3).
- ✅ Verify browser bằng Playwright (spec tạm đã xoá sau khi chạy): **5/5 màn render light + dark, 0 JS console error / 0 pageerror, không overflow ngang**, **canvas simulator vẫn hoạt động** (chạy bubble sort → bước tăng), **benchmark vẽ được ECharts** (canvas chart xuất hiện sau khi chạy). Ghi chú: 401 `/auth/refresh` lúc boot demo (không đăng nhập) là hành vi app bình thường — network notification, không phải JS error.

## Ghi chú / rủi ro

- **Syntax highlight code runner**: textarea thật (e2e `editor.fill()` phụ thuộc) không thể highlight theo token → chọn **style tối giản đẹp + gutter số dòng** (JetBrains Mono, dark slate, caret mint) thay vì overlay pre/transparent phức tạp. Đúng phương án "nếu không khả thi thì style tối giản đẹp" của task.
- **Canvas simulator**: chỉ thêm backdrop `simulator__canvas-wrap` bên ngoài `CanvasArea` (padding/border/backdrop) — component + ResizeObserver fix **KHÔNG đổi**.
- Không chạm `engines/*`, `renderers/*`, `stepExecutor`, worker. Các file `.opencode/`, `docs/pm-decision-log-*.md`, ảnh e2e/f3 cũ, `session/*`, `tailieu/*` là thay đổi của agent khác — KHÔNG nằm trong commit này.

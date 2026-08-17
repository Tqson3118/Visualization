# VISUAL + ROUTE UNIFICATION — Lộ trình & Shared Visual System

> Branch: feat/learning-path-visual-unification (từ dev). Không đụng source/VisualizationDSA (read-only reference),
> không xóa PathView/NodeHubView/legacy simulator, không đổi entity/DB naming.

## Workstream A — Lộ trình (routes + terminology)

| Route | Trạng thái | Ghi chú |
| --- | --- | --- |
| /path | canonical list — đã có (PathRedirectView) | Card chủ đề → /path/:topicId |
| /path/:topicId | detail/roadmap (PathView + PathGraph) | giữ nguyên |
| /path/:topicId/node/:nodeId, /final-test | legacy deep link | giữ nguyên |
| /lessons/:lessonId | canonical lesson (đổi từ /learn/:lessonId) | word "lesson" giữ |
| /learn, /learn/:lessonId | compatibility → redirect /path và /lessons/:id | không mất link cũ |
| /courses, /courses/:id | compatibility → redirect /path, /path/:id | alias mới (B2/A2) |

Terminology: public UI dùng "Lộ trình". Sửa roadmapFeature3 ("khóa học." → "lộ trình.") +
kicker PathRedirectView ("Learning Path" → "Lộ trình"). ID: topicId == pathId (cùng không gian Topic id) —
redirect /courses/:id → topicId an toàn. Progress: Lesson completion giữ canonical (UserProgress/markViewed);
visualizer/quiz/codelab chỉ phát event → lesson state (KHÔNG đổi localStorage key hiện có).

## Workstream B — Shared Visual System

### Contract (frontend/src/visualizer)
- types.ts: SharedVisualFrame {algorithmKey, stepIndex, totalSteps, description, highlights, data, pseudocodeLine, status, annotations, variables} + VisualRendererAdapter.
- stepToFrames.ts: Adapter legacy engine Step[] → SharedVisualFrame[] (dominantStatus: error>swap>active>highlight>done>idle).
- dslToFrames.ts: Adapter DSL TraceEvent → eventsToSteps → Step[] → frames.
- SortFrame/VCR (reference sandbox) → boundary: chỉ đọc thiết kế từ source/VisualizationDSA, không import vào build.

### Shell (frontend/src/components/visualizer)
- SharedVisualizerShell.vue: HUD header · stage (renderer adapter: ArrayBarsRenderer / fallback) ·
  playback controls (ControlBar chung) · progress · trace drawer · mã giả + explanation.
- ArrayBarsRenderer.vue: cột bar thuần DOM/CSS, tokens canvas (data-core/resolved/conflict/accent…).
- VisualizerTraceDrawer.vue: danh sách bước, nhảy tới bước.
- EmbeddedVisualizer.vue: nạp generator thật (getSimulation → Step[] → frames) cho lesson.

### Lesson integration (LessonView.vue)
- Tab Lý thuyết: nút "Chạy thử thuật toán" chỉ hiện khi lesson có simulationKey.
- Bấm nút → mở EmbeddedVisualizer INLINE trong theory (KHÔNG có tab Visualizer riêng).
- "Học tiếp" → về theory + mở visualizer inline (không rời lesson). Đóng → quay lại lý thuyết.
- Slice 1: Bubble Sort — lesson → theory → Chạy thử → shell (bubble generator thật) → Play/Next/Reset → đóng → Quiz → progress.

## Tests
- visualizer/stepToFrames.spec.ts (7) · dslToFrames.spec.ts (3) · SharedVisualizerShell.spec.ts (6)
- EmbeddedVisualizer.spec.ts (4) · views/__tests__/lessonViewVisualizer.spec.ts (3)
- router/aliases.spec.ts (acceptance routes)
- FE build (vue-tsc -b && vite build) PASS · full vitest PASS · BE build/test không đổi (0 warn/0 err).

## Backlog / cleanup candidates (KHÔNG xóa)
- Legacy full-page SimulatorView vẫn là integration riêng (Pixi/WebGL stage) — hợp nhất sâu vào
  SharedVisualizerShell là backlog; frame contract đã sẵn sàng.
- Graph/Stack/Queue/Tree renderers cho shell (hiện fallback renderer an toàn) — theo slice tiếp theo.
- SortFrame/VCR của sandbox: chỉ đến khi đồng bộ được giữa hai repo.
- source/VisualizationDSA33 (bản chưa track) — không chạm.

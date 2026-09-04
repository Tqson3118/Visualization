# 11. Simulator visual pipeline

## Flow
URL key → registry/catalog → chạy algorithm → tạo danh sách steps → store chọn currentStep → truyền structure qua props → chọn renderer theo kind → mount canvas → render structure.

## Chi tiết renderer
`ensureRenderer()` thường:
- kiểm tra canvas/structure;
- gọi `getRendererForKind(kind)`;
- dispose renderer cũ nếu đổi kind;
- mount renderer mới;
- resize;
- trả renderer hiện tại.

Khi structure đổi, watcher gọi `renderer.render(newStructure)`. Khi Play, currentIndex tăng nên currentStep và canvas thay đổi.

## Code cần tra
- `frontend/src/engines/registry.ts`
- catalog/algorithm definitions
- `frontend/src/core/CoreAnimationEngine.ts`
- `frontend/src/core/CompilerStepExecutor.ts`
- `frontend/src/components/visualizer/`
- renderer interface và `ArrayRenderer`.

## Câu hỏi sâu
Nếu không có renderer phải hiển thị fallback; khi unmount phải dispose để tránh memory leak; code highlight chỉ đổi nếu step có code line/trace metadata.

## Checklist phải học thuộc
Route key → registry → algorithm → steps → currentIndex → currentStep.structure → renderer registry → mount → render; thêm cleanup/dispose và code trace.

## Cách tra code
Tìm route params, registry lookup, step type, watch structure, getRendererForKind và renderer render. Đọc cả onMounted/onUnmounted.

## Câu hỏi khó
Nếu cùng kind nhưng structure đổi sâu thì watcher có bắt không? Nếu seek ngược thì canvas render snapshot nào? Nếu không có renderer thì fallback gì?

## 8. Flow simulator theo file/dòng

1. Bắt đầu tại route và View simulator; tìm route param bằng Ctrl+F `visual`, `simulation`, `key`, `route.params`.
2. Tra `frontend/src/engines/registry.ts` và catalog để thấy key được map thành algorithm/config nào.
3. Execution đi qua `CoreAnimationEngine.ts`/`CompilerStepExecutor.ts`; tìm `steps.push`, collector, trace và current index.
4. UI playback ở `frontend/src/features/visual-shell/components/SharedVisualizerShell.vue:154` và các component liên quan; watcher phản ứng khi step/index đổi.
5. Với CodeRunner, View truyền step ở `frontend/src/views/CodeRunnerView.vue:50` và hiển thị vị trí bước ở dòng 440.
6. CanvasArea gọi `ensureRenderer`, registry renderer, mount/resize; watcher structure gọi `renderer.render(newStructure)`. Tìm exact component bằng Ctrl+F `ensureRenderer`, `getRendererForKind`, `ArrayRenderer`.
7. Kết thúc UI: canvas vẽ array/tree/graph, code panel highlight trace line, controls cập nhật play/pause/seek.

**Cần kiểm tra:** có dispose ở onBeforeUnmount không, watcher có deep không, seek ngược có render snapshot không, kind không hỗ trợ hiển thị fallback gì.

## Flow diễn giải bằng lời
Khi vào `/simulator/sort.bubble`, View lấy key và tra registry/catalog. Engine chạy algorithm, step collector tạo các snapshot có structure, animation và code line. Store giữ steps/currentIndex; shell watcher ở `SharedVisualizerShell.vue:154` phản ứng khi index đổi. CanvasArea gọi ensureRenderer, registry trả ArrayRenderer, renderer mount/resize rồi render structure. Bấm Play làm timer tăng currentIndex; currentStep và props đổi, watcher gọi render lại, cell active đổi màu và code panel highlight line. Kết quả cuối cùng hiển thị qua canvas và playback controls.

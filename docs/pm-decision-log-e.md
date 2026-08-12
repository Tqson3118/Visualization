# PM DECISION LOG — SESSION E (Engine worker + renderers + integration tests + Playwright e2e)

> Ngày: 12/08/2026 · Chế độ: --auto · Trạng thái: docs/pm-report-e.md

## [2026-08-12 08:05] Khởi động SESSION E — 6 luồng theo session/PROMPT_E.md
- Quyết định: Triển khai Đợt E đúng 4 luồng + 2 bước chốt trong PROMPT_E: (1a) runMeasure → Web Worker, (1b) renderer canvas thật, (2) integration tests Testcontainers, (3) Playwright e2e config, (4) dev-e2e thực tế + review UI, (5) dev-test verify độc lập, (6) dev-review chốt merge. Chạy TUẦN TỰ từng task (1 nhánh feature mỗi task từ dev; không chạy song song vì cùng working tree git).
- Ảnh hưởng: 4 nhánh feature mới: feature/engine-worker, feature/engine-renderers, feature/integration-tests, feature/e2e-playwright.

## [2026-08-12 08:20] Task E1a hoàn thành — runMeasure sang Web Worker (thu)
- Quyết định: runMeasureInWorker trong compileWorker.ts (kind:'measure', timeout 5s → null, kill-switch terminate → null, không bao giờ reject); runMeasure cũ giữ + JSDoc DEPRECATED; BenchmarkPanel.vue await bất đồng bộ; 7 test mới mock Worker.
- Verify: npm test 57/57, build PASS, smoke Benchmark UI không bị chặn, console 0 lỗi engine. Ghi nhận bug backend sẵn có: /benchmarks/run + /me/hearts trả 500 (đã có trước đợt E — panel nuốt lỗi đúng thiết kế).
- Đề xuất mở rộng (CHƯA làm): nâng MAX_STEPS riêng cho measure mode (VD 1M) để n=500 đo thật thay vì N/A — chạm SDD §4.0.3, cần user duyệt → ghi SETUP_TODO §3.
- Ảnh hưởng: frontend/src/engines/worker/*, stepExecutor.ts (JSDoc), BenchmarkPanel.vue, compileWorker.spec.ts.

## [2026-08-12 08:30] Ràng buộc môi trường: Docker KHÔNG được reset (cảnh báo user)
- Quyết định: User cảnh báo — nếu Docker bị reset (restart/stop/down/pull thất bại giữa chừng) thì KHÔNG bật lại được, phải reset máy. Vì vậy: CẤM mọi task chạy docker restart/stop/down/up với container đang chạy; CẤM pull image không cần thiết (network đang chập chờn — git pull origin đã fail). Chỉ dùng 
eww-sqlserver-1 (Up healthy) cho integration test; nếu Testcontainers cần image chưa có local → dùng SQL Server đang chạy (port 1433) làm fallback, ghi rõ, KHÔNG báo PASS giả.
- Ảnh hưởng: task E2 (integration tests), E4 (e2e thực tế), mọi smoke browser.

## [2026-08-12 08:32] Khôi phục neww-sqlserver-1 (E1a smoke làm dừng container)
- Quyết định: phát hiện neww-sqlserver-1 Exited (255) sau task E1a (smoke của dev-engine có thể dừng nhầm). docker start thành công → Up healthy. Từ nay dặn mọi task: CẤM docker stop/restart/down; chỉ dùng container sẵn có.
- Ảnh hưởng: E1b, E2, E4.

## [2026-08-12 08:55] Task E1b hoàn thành — renderer canvas thật (thu)
- Quyết định: 8 file renderers mới (canvasPainter + array/stackQueue/list/tree/hashTable/graph + registry + index), CanvasArea.vue wire getRendererForKind (giữ fallback cũ + interactive), 14 test mới renderers.spec.ts.
- Verify: npm test 71/71, build PASS, smoke /simulator/sort.bubble + /simulator/tree.bst-insert render đúng màu CANVAS_COLORS, console 0 lỗi renderer.
- Ảnh hưởng: frontend/src/engines/renderers/*, components/simulator/CanvasArea.vue, __tests__/renderers.spec.ts.

## [2026-08-12 09:10] Task E2 hoàn thành — IntegrationTests thật + fix bug JWT (bao)
- Quyết định: 27 test integration (Auth 8/Lessons 10/Topics 7/Public 2) dùng Testcontainers.MsSql (image local, không pull) + WebApplicationFactory; JWT test helper HS256 đúng issuer/audience; seed unique Guid mỗi test.
- BUG PRODUCTION phát hiện + đã fix: JwtBearer MapInboundClaims mặc định true → claim sub thành NameIdentifier → NRE 500 mọi endpoint [Authorize]. Fix: MapInboundClaims=false + RoleClaimType tường minh; xóa workaround test-side; thêm test end-to-end login thật → GET /lessons 200.
- Verify: build 0 warning; dotnet test 71/71 (44 Unit + 27 Integration). Docker KHÔNG bị đụng (Testcontainers container riêng).
- Ảnh hưởng: backend Program.cs + tests/DsaVisual.IntegrationTests/*.

## [2026-08-12 09:30] Task E3 hoàn thành — Playwright e2e (thu)
- Quyết định: playwright.config.ts (baseURL 5173, webServer npm run dev, port E2E 5174 — 5173 bị relay Docker phục vụ bản cũ, KHÔNG đụng docker); 4 spec route-mock (auth/simulator/ladder/code-runner) + helpers/mockApi.ts (17 endpoint mock); vitest exclude tests/e2e.
- Verify: npm i -D @playwright/test OK (1.62.1), chromium có sẵn cache; build PASS; npm test 71/71; npx playwright test 11/11 PASS (2 lần).
- Ghi nhận (KHÔNG sửa view — task sau): SimulatorView chưa đọc query.step (FR-2.11), CodeRunnerView chưa giới hạn 200 dòng (FR-9.6), ResizeObserver loop, stale fieldErrors RegisterView.
- Ảnh hưởng: frontend/tests/e2e/*, playwright.config.ts, package.json, vite.config.ts.

## [2026-08-12 09:45] Chuẩn bị E4 — backend local + Ollama
- Quyết định: Backend chạy local (dotnet run, port 5000, env dev secret + connection localhost:1433 DsaVisual@Dev123 — KHÔNG docker up). SQL Server neww-sqlserver-1 Up healthy. Ollama qwen2.5vl:3b sẵn sàng (localhost:11434). Frontend dev port E2E 5174 (5173 bị relay Docker chiếm).
- Ảnh hưởng: E4.

## [2026-08-12 10:15] Task E6 hoàn thành — Review + fix (thu)
- Quyết định: dev-review verdict CHANGES REQUESTED (1 Major: hashTableRenderer width âm → IndexSizeError; 1 Minor: README sai số 26→27). Sửa qua feature/e2-fixes: clamp hw ≥ 12 + test m=11/w=200 + README đúng 27. Verify sau fix: FE 72/72 + build PASS, BE 44+27. Merge vào dev → chốt APPROVE.
- Ảnh hưởng: hashTableRenderer.ts, renderers.spec.ts, IntegrationTests README.

## [2026-08-12 10:40] Fix canvas phình 10.535px — ResizeObserver loop (son)
- Quyết định: chọn phương án A (user duyệt): .canvas-area__viewport height:420px cố định (bỏ min-height, giữ overflow:hidden) + guard delta <2px trong resize() (lastView) — 2 lớp chặn vòng lặp. Root cause: viewport flex:1 + cha height auto → canvas.style.height = parent.clientHeight → observer fire → phình.
- Verify: 4 màn (sort.heap/sort.bubble/structure.avl/graph.bfs) canvas 417.6px bất biến sau 2s, console 0 lỗi; scrollbar trang còn lại do layout app-shell (footer/main) — KHÔNG phải canvas (canvas-area bottom 574 < viewport 674). npm run build PASS + npm test 72/72. Account test: univ123@university.edu.vn / MatKhau@123 (register mới).
- Commit ba62a33 (son) → merge dev 090accf → push origin OK.
- Ảnh hưởng: frontend/src/components/simulator/CanvasArea.vue (6 insertions/2 deletions).

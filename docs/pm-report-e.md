# PM REPORT — SESSION E (Engine worker + renderers + IntegrationTests + Playwright e2e + E2E thực tế)

> Ngày: 12/08/2026 · Chế độ: --auto · Quyết định: docs/pm-decision-log-e.md · Việc cần user: docs/SETUP_TODO.md §6

## 1. Mục tiêu
Theo session/PROMPT_E.md — 4 luồng + 2 bước chốt: (1) engine worker + renderer thật, (2) integration tests thật, (3) Playwright e2e, (4) E2E thực tế + review UI, (5) verify độc lập, (6) review chốt merge. Nguồn chuẩn: SDD §4.0.3/§8.3, TEST_PLAN, ADR-012.

## 2. Trạng thái task

| Task | Nội dung | Nhánh | Agent | Kết quả | Verify (độc lập) |
|---|---|---|---|---|---|
| E1a | runMeasure → Web Worker (ADR-012, SDD §4.0.3) | feature/engine-worker | dev-engine | **DONE** | npm test 57/57 · build PASS · smoke Benchmark UI không bị chặn |
| E1b | Renderer canvas thật 6 loại CTDL (SDD §8.3) + registry + wire CanvasArea | feature/engine-renderers | dev-engine | **DONE** | npm test 71/71 · build PASS · smoke sort.bubble + tree.bst-insert render đúng |
| E2 | IntegrationTests THẬT: Testcontainers MsSql + WebApplicationFactory (Auth/Lessons/Topics/Public) + **fix bug JWT production** | feature/integration-tests | dev-backend | **DONE** | dotnet build 0 warning · test 44 Unit + 27 Integration PASS |
| E3 | Playwright e2e: config + 4 spec (auth/simulator/ladder/code-runner) route-mock | feature/e2e-playwright | dev-frontend | **DONE** | build PASS · npm test 71/71 · playwright 11/11 PASS |
| E4 | E2E thực tế + review UI (Ollama qwen2.5vl:3b) — 8 màn | — (chỉ báo) | dev-e2e | **DONE** | 3 PASS / 5 CÓ LỖI — 8 bug phát hiện (4 P1 + 4 P2) |
| E5 | Verify độc lập toàn bộ đợt | — | dev-test | **PASS** | 7/7 vùng PASS (build FE/BE, 71 FE, 44+27 BE, 11/11 e2e, grep cấm sạch, catalog 44/44, secret sạch) |
| E6 | Review diff toàn đợt + fix | feature/e2-fixes | dev-review + dev-engine | **DONE → APPROVE** | fix Major (hashTable width âm) + Minor (README) — FE 72/72, BE 44+27 |

**Tổng: 7/7 DONE. Không task FAIL/SKIP.** Tất cả merge vào dev (commit: 5a793e7, 0df9fc9, 3d40cbe, be56db9, 6fa1f1d; merge 98851ca, ac63799, 509008a, 63f28ad, bafbfb0).

## 3. File thay đổi chính (đã merge)
- **E1a**: frontend/src/engines/worker/{compileWorker,compiler.worker}.ts (runMeasureInWorker kind:'measure', timeout 5s → null, kill-switch), core/stepExecutor.ts (JSDoc deprecated), components/benchmark/BenchmarkPanel.vue (async), __tests__/compileWorker.spec.ts (7 test).
- **E1b**: frontend/src/engines/renderers/{canvasPainter,arrayRenderer,stackQueueRenderer,listRenderer,treeRenderer,hashTableRenderer,graphRenderer,rendererRegistry,index}.ts, components/simulator/CanvasArea.vue, __tests__/renderers.spec.ts (14 test).
- **E2**: backend/tests/DsaVisual.IntegrationTests/* (ApiFactory, ApiTestFixture, MssqlFixture, IntegrationTestBase, Auth/Lessons/Topics/Public tests — 27 test), **backend Program.cs: MapInboundClaims=false + RoleClaimType** (fix 500 toàn bộ authenticated endpoints).
- **E3**: frontend/playwright.config.ts, tests/e2e/{auth,simulator,ladder,code-runner}.spec.ts + helpers/{mockApi,auth}.ts, package.json (test:e2e), vite.config.ts (vitest exclude), tests/e2e/README.md.
- **E6**: hashTableRenderer clamp, renderers.spec.ts +1 test, IntegrationTests README.

## 4. Kết quả verify tổng thể (dev-test — E5)
| Lệnh | Kết quả |
|---|---|
| npm run build (frontend) | PASS 0 lỗi |
| npm test (frontend) | PASS — 71/71 (sau E6: 72/72) |
| dotnet build DsaVisual.sln | PASS 0 warning / 0 error |
| dotnet test DsaVisual.sln | PASS — Unit 44/44 + Integration 27/27 (Testcontainers thật) |
| npx playwright test | PASS — 11/11 (10.3s) |
| Grep cấm (PostgreSQL/MediatR/Repository/secret) | KHÔNG vi phạm |
| Catalog sync | 44/44 khớp |

## 5. Bug phát hiện trong đợt (trạng thái)
- ✅ **FIXED — Bug JWT production** (E2): MapInboundClaims=true → claim sub thành NameIdentifier → 500 mọi endpoint [Authorize]. Fix MapInboundClaims=false + RoleClaimType. Test e2e login thật → GET /lessons 200 chứng minh.
- ⚠ **CHƯA SỬA — 8 bug UI từ E2E thực tế** (4 P1 + 4 P2): mất phiên reload (ADR-004), mark-viewed 404, nút "Làm bài" chết, canvas phình ResizeObserver, submit exercise 400 UX tệ, Ladder stage rỗng (hardcode null), code-runs contract lệch, benchmarks/run 400. → ghi SETUP_TODO §6, đề xuất đợt sau.

## 6. Việc cần user (docs/SETUP_TODO.md §6)
- Quyết định contract FE/BE cho /code-runs (P2 #7) và /benchmarks/run (P2 #8) khi sửa đợt sau.
- (Đã ghi từ đợt D) đổi mật khẩu seed dev trước demo, cài monaco/chart.js tùy chọn.

## 7. Tồn đọng (đề xuất đợt sau — PROMPT_F)
1. Sửa 8 bug UI từ E4 (P1 trước: refresh phiên, mark-viewed, nút Làm bài, ResizeObserver, Ladder stage).
2. Triển khai FR-2.11 (?step=N trong SimulatorView), FR-9.6 (giới hạn 200 dòng CodeRunner), TEST-UI-007 (gắn data Ladder).
3. Push các nhánh lên origin khi mạng khôi phục (đang offline GitHub).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.

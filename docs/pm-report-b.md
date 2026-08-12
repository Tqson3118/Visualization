# PM REPORT — SESSION B (Bê code từ 3 source)

> Ngày: 12/08/2026 · Chế độ: --auto · Quyết định: `docs/pm-decision-log-b.md` · Chi tiết bê: `docs/REUSE_REPORT.md`

## 1. Mục tiêu
Scan 3 source (VisualizationDSA1 — ưu tiên, VisualizationDSA3 — curriculum, VisualizationDSA — nền), bê có chọn lọc code vào skeleton `frontend/` + `backend/` theo SDD (khớp §3.1 frontend / §5.1 backend 2 project / NFR-17 cấm Repository), ghi REUSE_REPORT.md, build thử, compliance check.

## 2. Trạng thái task

| Task | Nội dung | Agent | Kết quả | Verify |
|---|---|---|---|---|
| task-1 | Skeleton frontend + styles/ui/utils/api/stores/composables/router | dev-frontend | **DONE** | `npm run build` PASS (63KB+101KB gzip — NFR-5); `npm test` 5/5 |
| task-2 | Engine EDV (stepExecutor adapter + CoreAnimationEngine + canvasTheme + WebGPU optional + Web Worker) + catalog 44 key + components/simulator + SimulatorView | dev-frontend | **DONE** | build PASS; `npm test` **39/39**; catalog 44/44 khớp shared/simulation-catalog.json; browser smoke 0 lỗi |
| task-3 | Seed/curriculum: courses (V0), lessons (V3), shop_items (V1), seed plan backend + SeedData.cs | dev-frontend | **DONE** | build PASS; `npm test` 12/12; `dotnet build` PASS |
| task-4 | Backend skeleton 2 project theo SDD §5.1 + middleware + Result/ErrorCodes + docker-compose (SQL Server+MailHog) + Dockerfile/nginx | dev-backend | **DONE** | `dotnet build` PASS 0 warning; `dotnet test` 8/8; smoke /health 200 + /swagger 200 + 401 không token |
| task-5 | Build thử tổng thể + compliance check + check cấm + so catalog | dev | **DONE** | frontend build PASS + 39/39; backend build PASS + 8/8; cấm: KHÔNG vi phạm; catalog khớp 44/44 |

**Tổng: 5/5 DONE. Không task FAIL/SKIP.**

## 3. File thay đổi (tạo mới)
- **frontend/** (≈55+ file): config (package.json, vite.config.ts theo §3.9, tsconfig*, vitest.setup.ts, .env.development/.production/.example, Dockerfile, nginx.conf) · src/{main.ts, App.vue, router/ (13 route + guards), api/ (client.ts + 9 module), stores/ (9 theo §3.2 + auth.spec.ts), composables/ (8), i18n/vi.ts, styles/ (tokens.css + global.css), utils/ (format, validators, emojiParser), engines/ (core/types + stepExecutor + registry + webGpuPipeline, renderers/interface + coreAnimationEngine + canvasTheme, worker/ 2 file, catalog.ts 44 key + 3 spec), data/ (courses, lessons, shop_items.json + spec), views/ (Home, Login, Simulator, Placeholder, NotFound), components/ (ui/ 5, simulator/ 2), tests/e2e/README.md}
- **backend/** (87 file): DsaVisual.sln · src/DsaVisual.Api/ (Program.cs pipeline §5.8, Controllers/LessonsController §5.7.1, Dtos, Middlewares 2, appsettings*.json) · src/DsaVisual.Application/ (Common: Result/ErrorCodes/Pagination/DateTimeProvider; Dtos; Persistence: AppDbContext 32 DbSet + Configurations + Entities 35 + Migrations/README + Seed/README + SeedData.cs; Services: 13 (1 thật + 12 TODO); Validators: LessonValidator) · tests/ (UnitTests 8 test + IntegrationTests + Api.Tests) · .env.example, Dockerfile, .dockerignore, README
- **Root**: `docker-compose.yml` (sqlserver + mailhog + backend + frontend), `frontend/Dockerfile`, `frontend/nginx.conf`
- **Docs**: `docs/REUSE_REPORT.md` (mới — bảng bê/không bê 24+15 dòng + ghi chú chỉnh), `docs/pm-decision-log-b.md` (append 5 mục)

## 4. Kết quả verify (lệnh thật đã chạy)
| Lệnh | Kết quả |
|---|---|
| `npm run build` (frontend) | PASS — vue-tsc + vite, 0 lỗi |
| `npm test` (frontend) | PASS — 39/39 (5 files) |
| `dotnet build DsaVisual.sln` | PASS — 0 warning / 0 error (net10.0) |
| `dotnet test DsaVisual.sln` | PASS 8/8 (Api.Tests + IntegrationTests: "No test available" — chưa có test code, đúng dự kiến) |
| Smoke backend | /health 200, /swagger 200, GET /api/v1/lessons không token → 401 ✓ |
| Check cấm (Npgsql/PostgreSQL/Supabase/MediatR/Repository/Judge0/.env thật) | **KHÔNG vi phạm** — "Repository" chỉ trong comment giải thích |
| Catalog `shared/simulation-catalog.json` ↔ `engines/catalog.ts` | 44 = 44 — KHỚP (test catalog.spec.ts giữ đồng bộ) |

## 5. LỆCH so với SDD (compliance — phần còn TODO placeholder, không phải sai kiến trúc)
- **Frontend §3.1**: api thiếu 3 module (favorites/classes/benchmark) · views 5/32 (27 route trỏ PlaceholderView) · components/ui 5/13 · simulator 2/12 · ladder/gamification/lesson chưa có · e2e playwright chưa cấu hình.
- **Backend §5.1**: Controllers 1/15 · Dtos còn thiếu · Migrations chưa tạo (cần CSDL) · Api.Tests/IntegrationTests chưa có test thật.
- **Lệch chủ ý (đã ghi decision log + chú thích code)**: DTO dùng chung ở Application/Dtos (SDD tự mâu thuẫn §5.1 — tránh reference vòng); Asp.Versioning không dùng versioned ApiExplorer (mất controller trong OpenAPI .NET 10 — giữ v1 + suppress AV0021/0029); runMeasure main-thread (TODO Web Worker ADR-012); TraceKind suy diễn heuristic; font hệ thống thay Baloo 2/Comic Neue; PlaceholderView cho route chưa có view.
- **Engine**: generators là STUB (generate() throw "implement in task tiếp") — đúng phạm vi skeleton; inputSchema/pseudocode rỗng.

## 6. Quyết định đã ghi (xem `docs/pm-decision-log-b.md` — 13 mục)
Thứ tự nguồn ưu tiên · backend v2 KHÔNG bê code C# cũ · feature đã cắt không bê · secret chỉ .env.example · WebGpuPipeline optional · build thử được (Node 24 + .NET 10 sẵn) · phân công 5 task · RESUME phiên (phiên trước gián đoạn trước dispatch) · shared/simulation-catalog.json là nguồn catalog · .env.example viết mới · package.json rút gọn · hoàn thành + lệch chủ ý.

## 7. Việc còn tồn đọng (đề xuất task sau)
1. Generator EDV thật cho 44 mô phỏng (theo SDD §4.3/§4.7) — viết từng nhóm sort/search/structure/tree/graph.
2. Hoàn thiện views 32 màn theo SCREEN_MAP + 8 component UI + 10 simulator component.
3. Backend: 14 controller + DTO đầy đủ theo API_REFERENCE; AuthService JWT thật; Migration InitialCreate khi có SQL Server; test thật cho IntegrationTests (Testcontainers).
4. Seed thật backend (Topics/Lessons/Questions từ content-drafts 40 bài — nguồn đã liệt kê trong Seed README).
5. Chuyển runMeasure sang Web Worker (ADR-012); renderer canvas thật cho từng CTDL (SDD §8.3).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.

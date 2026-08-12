# REUSE REPORT — BÊ CODE TỪ 3 SOURCE (SESSION B)

> Ngày: 12/08/2026 · Chế độ: --auto · Quyết định chi tiết: `docs/pm-decision-log-b.md` · Trạng thái: `docs/pm-report-b.md`
> Mục đích: bảng quyết định BÊ/KHÔNG BÊ từng nhóm file của 3 source → skeleton v2 theo SDD.

## 1. Tổng quan 3 nguồn

| Nguồn | Vai trò | Kết luận scan |
|---|---|---|
| `source/VisualizationDSA1` | **Ưu tiên** (mới nhất, design-system, engine) | Nguồn chính cho engine/component/UI/utils/styles/composables/docker pattern |
| `source/VisualizationDSA3` | Curriculum | Nguồn cho engine tốt hơn (Babel AST interpreter, Web Worker), curriculum lessons, canvas theme |
| `source/VisualizationDSA` (gốc) | Nền | Chỉ làm nền so sánh; data courses.ts (4 course DSA — bản đầy đủ nhất, có isPremium) |

## 2. BẢNG BÊ (bê có chọn lọc — mọi file đều ĐÃ CHỈNH cho khớp v2)

| # | File nguồn | Chức năng | Bê? | Lý do | Nơi đích | Khớp SDD |
|---|---|---|---|---|---|---|
| 1 | V1 `design-system/visualizationdsa/MASTER.md` | Design tokens (màu/spacing/shadow/radius) | ✔ | Design system chuẩn, mới nhất | `frontend/src/styles/tokens.css` | §8.1, §3.1 |
| 2 | V1 `frontend/src/styles/theme.css` + `cinematic.css` | Phong cách nền | ✔ (viết mới) | Chỉ tham khảo phong cách, không chép nguyên trạng | `frontend/src/styles/global.css` | §3.1 |
| 3 | V1 `frontend/src/utils/emojiParser.ts` | Parse emoji | ✔ | Bê nguyên + thêm icon | `frontend/src/utils/emojiParser.ts` | §3.1 |
| 4 | V1 `frontend/package.json` | Dependency | ✔ (rút gọn) | Giữ vue/pinia/router/axios/monaco; cắt ~25 gói feature đã cắt | `frontend/package.json` | §3.1 |
| 5 | V1 `frontend/vite.config.ts` | Build config | ✔ (theo §3.9) | manualChunks chuyển dạng hàm cho Vite 8/Rolldown | `frontend/vite.config.ts` | §3.9 |
| 6 | V1 `frontend/vitest.setup.ts` | Test setup | ✔ | Bê nguyên (MemoryStorage) | `frontend/vitest.setup.ts` | §3.7 |
| 7 | **V3** `frontend/src/core/CompilerStepExecutor.ts` | Interpreter thuật toán (Babel AST) | ✔ (adapter) | V3 tốt hơn V1 (scope/đệ quy đúng; V1 chỉ regex hoist); thêm adapter → TraceEvent EDV | `frontend/src/engines/core/stepExecutor.ts` | §4.0.3 |
| 8 | **V3** `frontend/src/core/CoreAnimationEngine.ts` | Animation engine (Lerp 60FPS) | ✔ | V3 tốt hơn (clamp 32ms + cô lập lỗi; V1 kém) | `frontend/src/engines/renderers/coreAnimationEngine.ts` | §4.4 |
| 9 | **V3** `frontend/src/core/renderers/canvasTheme.ts` | Theme canvas (đọc CSS var) | ✔ | V3 đọc CSS var; V1 hardcode | `frontend/src/engines/renderers/canvasTheme.ts` | §4.4 |
| 10 | **V3** `frontend/src/core/WebGpuPipeline.ts` | Pipeline WebGPU (tùy chọn) | ✔ (OPTIONAL) | Ngoài luồng EDV chính — header ghi rõ cần rà WebGPU | `frontend/src/engines/core/webGpuPipeline.ts` | §4.2 |
| 11 | **V3** `frontend/src/core/compiler.worker.ts` + `compileWorker.ts` | Chạy code trong Web Worker | ✔ | ADR-012 sandbox client; chỉ sửa import path | `frontend/src/engines/worker/` | §4.0.3, ADR-012 |
| 12 | V1 `components/VcrControls.vue` | Control bar player | ✔ (chỉnh) | Props/emit theo simulationStore §3.2; bỏ Exit→Sandbox; thêm tốc độ 0.25–4x; i18n | `frontend/src/components/simulator/ControlBar.vue` | §3.1, §3.3 |
| 13 | V1 `components/VcrExplanationBanner.vue` | Panel giải thích bước | ✔ (chỉnh) | actionType→kind (TraceKind); i18n | `frontend/src/components/simulator/ExplainPanel.vue` | §3.1 |
| 14 | V1 `components/ToastContainer.vue(.css)` | Toast | ✔ (chỉnh) | Dùng uiStore mới; i18n; tokens light | `frontend/src/components/ui/ToastContainer.vue` | §3.1, §3.6 |
| 15 | V1 `components/ui/BottomSheet.vue` + `ConfirmModal.vue(.css)` | UI base | ✔ | Bê, chỉnh i18n/tokens | `frontend/src/components/ui/` | §3.1 |
| 16 | V1 `components/icons/SvgIcon.vue` | Icon | ✔ (đổi tên) | → BaseIcon.vue + thêm icon thiếu | `frontend/src/components/ui/BaseIcon.vue` | §3.1 |
| 17 | V1 `composables/useConfetti.ts` | Confetti | ✔ | Bê nguyên | `frontend/src/composables/useConfetti.ts` | §3.6 |
| 18 | **V0** `frontend/src/data/courses.ts` | 4 course DSA | ✔ (chỉnh) | V0 đầy đủ nhất (isPremium); thêm topicId map SDD §7.5; bỏ oop/solid | `frontend/src/data/courses.ts` | §7.5 |
| 19 | **V3** `frontend/src/data/lessons.ts` | Bài học DSA (quick-sort chuẩn) | ✔ (chỉnh) | Bản đầy đủ (entryFunction, testCases); simulations chỉ giữ key có trong catalog | `frontend/src/data/lessons.ts` | §7.5 |
| 20 | V1 `frontend/src/data/shop_items.json` | Cửa hàng gems | ✔ | Khớp Màn 22 | `frontend/src/data/shop_items.json` | §8.4 Màn 22 |
| 21 | V1 `backend/src/WebApi/Middlewares/ErrorHandlingMiddleware.cs` | Bắt exception → JSON | ✔ (ý tưởng) | VIẾT LẠI: namespace mới, envelope {error:{code,message,field,details}} theo API_REF §2.1 | `backend/src/DsaVisual.Api/Middlewares/ErrorHandlingMiddleware.cs` | §5.8 |
| 22 | V1 `backend/Dockerfile` | Docker backend | ✔ (pattern) | Multi-stage .NET 10, 2 project (V1 dùng 4 project + .NET 9) | `backend/Dockerfile` | §10, DEPLOY §4 |
| 23 | V1 `frontend/Dockerfile` + `nginx.conf` | Docker + nginx frontend | ✔ (pattern) | Node 22; bật proxy /api→backend:8080 (V1 để tắt); SPA fallback | `frontend/Dockerfile`, `frontend/nginx.conf` | §10, DEPLOY §4.3 |
| 24 | V1 `backend/seed-demo-course.sql` | Seed mẫu | ✔ (ý tưởng) | KHÔNG dùng file — schema v1 lệch v2 (AspNetUsers Guid vs Users int; bảng CourseModules/Codelabs không có trong v2); ghi seed plan + SeedData.cs mới | `backend/src/DsaVisual.Application/Persistence/Seed/README.md` + `SeedData.cs` | §7.5 |

## 3. BẢNG KHÔNG BÊ (+ lý do)

| # | File/Feature nguồn | Lý do không bê |
|---|---|---|
| 1 | V1 `backend/src/Domain/`, `Infrastructure/` | SDD §5.1 cấm — v2 chỉ 2 project (Api + Application), KHÔNG Repository pattern |
| 2 | Mọi `Repository*`, `MediatR`, CQRS handlers | Cấm theo SDD §5.1 + NFR-17 (Service dùng DbContext trực tiếp qua DbSet) |
| 3 | PostgreSQL/Npgsql/Supabase connection, `docker-compose` cũ (Postgres/Redis/Judge0) | Cấm — v2 dùng SQL Server 2019+ (SDD §2.1); viết docker-compose MỚI: sqlserver + mailhog (SDD §5.6, DEPLOY §3.3) |
| 4 | V1 `.env` / `.env.production` (Supabase + Cloudinary thật) | Secret — cấm; chỉ viết `.env.example` mới (JWT/SQL Server/MailHog placeholder) |
| 5 | V3 `features/archived/*` (compare-algorithms, concurrency-viz, debug-mode, learning-path, multi-view, oop-sandbox, solid-sandbox, state-inspector, state-sandbox, system-sandbox, timeline-playback) | Feature đã cắt (SRS §1.3.2 / SCREEN_MAP §2) |
| 6 | V3/V1 `embed-widget`, `export-share` | Feature đã cắt |
| 7 | V3 `features/design-patterns`, `di-sandbox`, `smart-quiz`, `system-design-viz`, `flowchart-playground`, `oop-visualization`, `solid-visualization` | Ngoài danh mục v2 (chỉ tham khảo, không copy) |
| 8 | V1 `frontend/src/core/CompilerStepExecutor.ts` | Thua V3 (regex hoist vs Babel AST — scope/đệ quy sai) |
| 9 | V1 `CoreAnimationEngine.ts`, `canvasTheme.ts` | V3 bản tốt hơn (clamp 32ms, CSS var) |
| 10 | V1 `composables/useTheoryProgress.ts` | Phụ thuộc API lesson-player cũ (đã cắt) + trùng useInterval §3.6 |
| 11 | V0/V1 `courses.ts` (oop-101, solid-101) | Ngoài curriculum DSA SDD §7.5 |
| 12 | 40 bài `content-drafts/v2/lesson-01..40` | Chưa bê — chỉ LIỆT KÊ nguồn trong Seed README (content.md + quiz.json); task triển khai nội dung sau |
| 13 | V1/V3 `stores/` cũ (classroomCurriculum.ts...) | SDD §3.2 cấm — viết mới 9 store theo bảng |
| 14 | V1/V3 `Program.cs`, Controllers, Validators C# | Kiến trúc cũ (4 tầng + Repository) — viết mới theo SDD §5.7 |
| 15 | Font Baloo 2 / Comic Neue (MASTER.md) | Font trẻ em, không hợp đối tượng đại học → font hệ thống (đợt G: thay tiếp bằng Geist + JetBrains Mono — xem §6) |

## 4. GHI CHÚ ĐIỂM CẦN CHỈNH (code cũ ≠ kiến trúc v2 — SDD ghi "đặc tả dự kiến")

1. **StepExecutor**: interpreter V3 giữ nguyên logic; TraceEvent suy diễn kind theo heuristic (interpreter cũ không gắn kind) — so sánh/hoán đổi chuẩn, loop/call/return gần đúng; `runMeasure` đang chạy main-thread → TODO chuyển Web Worker (ADR-012).
2. **Catalog engine**: 44 key KHỚP 100% `shared/simulation-catalog.json` (test `catalog.spec.ts` giữ đồng bộ — CI so khóa theo SDD §9.9); `generate()` là STUB (throw "implement in task tiếp") — generator thật là task triển khai.
3. **ControlBar/ExplainPanel**: chỉnh từ VcrControls/VcrExplanationBanner — phải tái kiểm khi có generator thật (props từ simulationStore).
4. **Seed**: `seed-demo-course.sql` KHÔNG dùng (schema v1 lệch); seed thật backend (Topics/Lessons/Questions/LearningPaths/Quests) là TODO GĐ2; `sliding-window` chưa có key simulation → `simulations: []` + TODO.
5. **Docker**: compose MỚI chỉ SQL Server + MailHog + backend + frontend; nginx proxy /api → backend:8080; .env.example placeholder.
6. **Compliance**: skeleton còn placeholder theo thiết kế (xem pm-report-b.md mục LỆCH): 27 view, 8 component UI, 10 simulator, 3 api module, 14 controller, Migrations, e2e — các phần này do task triển khai module, KHÔNG phải sai lệch kiến trúc.

## 5. Compliance check tổng hợp (task-5 verify)

- Build/test: frontend build PASS + 39/39 test; backend build PASS 0 warning + 8/8 test; smoke /health + /swagger OK.
- Check cấm: **KHÔNG vi phạm** — không Npgsql/PostgreSQL/Supabase/MediatR/AspNetUsers/Judge0; "Repository" chỉ xuất hiện trong comment "KHÔNG dùng Repository"; không có file .env thật (chỉ .env.example + .env.development/.production đúng SDD §3.1).
- Catalog: `shared/simulation-catalog.json` = 44 key ↔ `frontend/src/engines/catalog.ts` = 44 key — KHỚP.

## 6. GHI CHÚ ĐỢT G (ux-finalize — 12/08/2026): thay component tự xây bằng stack UI/UX mới

> Quyết định G (pm-decision-log-g.md): cài tailwindcss 4 + shadcn-vue + motion-v + gsap + vue-echarts + lenis + vue-sonner + phosphor/lucide; font Geist + JetBrains Mono self-host; tokens OKLCH + dark mode `class="dark"`. Chỉ đụng UI layer — **KHÔNG đụng canvas simulator/engine** (giữ nguyên §2 bảng 8-11, 12-13).

| Component/trước (tự xây — §2, SDD §3.1 cũ) | Thay bằng | Gói | File mới |
|---|---|---|---|
| `ToastContainer.vue` (§2 #14) | `<Toaster>` (vue-sonner, mount tại App.vue) | vue-sonner@2.0.9 | `src/lib/toast.ts` + `src/composables/useToast.ts` (giữ API cũ) |
| `BaseButton`/`Button.vue` | `ui/button` | shadcn-vue (reka-ui, class-variance-authority) | `src/components/ui/button/` |
| `BaseInput`/`Input.vue` | `ui/input` | shadcn-vue | `src/components/ui/input/` |
| `BaseModal`/`Modal.vue` | `ui/dialog` | shadcn-vue (reka-ui) | `src/components/ui/dialog/` |
| `BottomSheet.vue`/`ConfirmModal.vue` (§2 #15) | `ui/drawer` + `ui/dialog` | shadcn-vue (vaul-vue) | `src/components/ui/drawer/`, `dialog/` |
| `BaseCard`/`Card.vue` | `ui/card` | shadcn-vue | `src/components/ui/card/` |
| `BaseTabs`/`Tabs.vue` | `ui/tabs` | shadcn-vue (reka-ui) | `src/components/ui/tabs/` |
| `BaseTooltip`/`Tooltip.vue` | `ui/tooltip` | shadcn-vue (reka-ui) | `src/components/ui/tooltip/` |
| `BaseSelect`/`Select.vue` | `ui/select` | shadcn-vue (reka-ui) | `src/components/ui/select/` |
| `BaseBadge`/`Badge.vue` | `ui/badge` | shadcn-vue (class-variance-authority) | `src/components/ui/badge/` |
| `ProgressBar.vue` | `ui/progress` | shadcn-vue (reka-ui) | `src/components/ui/progress/` |
| `Skeleton.vue` | `ui/skeleton` | shadcn-vue | `src/components/ui/skeleton/` |
| `BaseIcon.vue` (§2 #16) | giữ tự xây + thêm icon từ | @lucide/vue + lucide-vue-next + @phosphor-icons/vue | `src/components/ui/BaseIcon.vue` |
| Chart (SVG tự vẽ — trước không có gói) | `VChartLazy.vue` (lazy vue-echarts) | vue-echarts@8.1.0 + echarts@6.1.0 | `src/components/ui/VChartLazy.vue` |
| Font hệ thống (§3 #15) | Geist (UI) + JetBrains Mono (mã giả) self-host | — (file woff2) | `public/fonts/GeistVariable.woff2`, `JetBrainsMonoVariable.woff2` |
| CSS global/tokens (đợt B §2 #1-2) | tokens OKLCH + Tailwind 4 CSS-first + dark mode | tailwindcss@4.3.3 + @tailwindcss/vite + tw-animate-css | `src/styles/tokens.css`, `tailwind.css`, `palettes.css` |
| Smooth scroll / animation | Lenis + motion-v + GSAP | lenis@1.3.26, motion-v@2.3.0, gsap@3.15.0 | composables + directive |

- **Không thay đổi**: engine EDV (stepExecutor, renderer canvas — §2 #7-11), 9 store Pinia, api modules, router, i18n — chỉ sửa call site UI theo wrapper shadcn-vue.
- Bundle thật sau đợt G (build 12/08/2026): engine 476KB gốc / 120KB gzip; echarts 324KB / 110KB (lazy); vendor 143KB / 54KB; JS tải lần đầu ≈ 852KB — NFR-5 đã nới (xem SRS §4.1, TEST_PLAN TEST-PERF-007).

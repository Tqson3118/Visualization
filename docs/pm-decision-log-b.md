# PM DECISION LOG — SESSION B (Bê code từ 3 source)

## [2026-08-12] Khởi động SESSION B (chế độ --auto, plan đã duyệt trong PM_MASTER_PLAN §SESSION B)
- Quyết định: Chạy SESSION B đầy đủ: scan 3 source → REUSE_REPORT.md → copy có chọn lọc → build thử. Chế độ --auto: không hỏi người dùng, mọi quyết định ghi tại đây.
- Ảnh hưởng: toàn bộ phiên; file đích frontend/, backend/ tại root D:\FPT\neww (CHƯA tồn tại → tạo skeleton mới theo SDD §3.1/§5.1)

## [2026-08-12] Thứ tự nguồn ưu tiên và vùng bê
- Quyết định: V1 (VisualizationDSA1) làm nguồn CHÍNH cho engine/component/UI/utils/styles/composables (bản mới nhất, .env.example sạch); V3 (VisualizationDSA3) làm nguồn cho design-tokens, nội dung học (course-seed, content-drafts v2, docs/content), docker/nginx pattern; V (VisualizationDSA gốc) chỉ làm nền so sánh — không có gì độc quyền cần bê thêm ngoài V1/V3 (backend C# Strategies + Repository = CẤM).
- Ảnh hưởng: danh sách copy trong REUSE_REPORT.md

## [2026-08-12] Backend v2: KHÔNG bê code C# cũ trực tiếp
- Quyết định: Backend 3 source đều là Clean Architecture 4 tầng (Domain/Application/Infrastructure/WebApi) + Repository + PostgreSQL + MediatR CQRS — trái SDD v2 (§5.1: 2 project, KHÔNG Repository, Service dùng DbContext trực tiếp, SQL Server). Chỉ bê: pattern Dockerfile (chỉnh), middleware ErrorHandling (chỉnh tên namespace), ý tưởng validator. Phần còn lại tạo skeleton rỗng theo SDD, ghi rõ "viết mới theo SDD".
- Ảnh hưởnge: backend/ không chứa code Domain/Strategies/Repositories từ source

## [2026-08-12] Feature đã cắt — KHÔNG bê
- Quyết định: Không bê toàn bộ features/archived/* (V3): compare-algorithms (= side-by-side đã cắt), concurrency-viz, debug-mode, learning-path, multi-view, oop-sandbox, solid-sandbox, state-inspector, state-sandbox, system-sandbox, timeline-playback. Không bê embed-widget (đã cắt), export-share (ngoài phạm vi SDD v2), design-patterns/di-sandbox/smart-quiz/system-design-viz/flowchart-playground/oop-visualization/solid-visualization (feature ngoài danh mục v2 — ghi chú tham khảo, không copy).
- Ảnh hưởng: danh sách "KHÔNG bê" trong REUSE_REPORT.md

## [2026-08-12] Secret — chỉ bê .env.example
- Quyết định: Chỉ copy .env.example (V1 — placeholder, không có secret thật). KHÔNG copy .env (V1 có file .env thật chứa Supabase connection string + Cloudinary key — bỏ qua), không copy docker-compose có credential thật (sẽ viết mới pattern).
- Ảnh hưởng: frontend/.env.example, backend/.env.example; không có secret nào vào repo

## [2026-08-12] WebGpuPipeline.ts và services API
- Quyết định: WebGpuPipeline.ts bê vào engines/core với ghi chú "optional — cần rà phụ thuộc WebGPU, không nằm trong luồng EDV chính". Các services/apiClient.ts bê làm tham chiếu vào src/api/ kèm ghi chú "phải chỉnh theo API_REFERENCE v2 (baseURL /api/v1, interceptor refresh)".
- Ảnh hưởng: engines/core/WebGpuPipeline.ts, src/api/*.ts

## [2026-08-12] Build thử — môi trường cho phép
- Quyết định: Node v24.15.0 + npm 11.12.1 + .NET SDK 10.0.300 có sẵn → build thử frontend (npm install hoặc tận dụng node_modules V1 nếu package.json khớp) + dotnet build backend skeleton. Docker có sẵn nhưng KHÔNG chạy docker-compose (tránh Postgres/Redis/Judge0 + mất thời gian).
- Ảnh hưởng: kết quả verify trong pm-report-b.md

## [2026-08-12] Phân công 5 task (dispatch celscin)
- Quyết định: task-1 skeleton frontend + styles/ui/shared/utils/core (coder); task-2 engines generators/renderers/catalog + components/simulator + composables (coder, sau task-1); task-3 seed + curriculum data (coder, sau task-1); task-4 backend skeleton + docker/nginx/env (coder, song song task-1); task-5 build thử frontend+backend (tester, sau 1-4). REUSE_REPORT.md do PM tổng hợp cuối phiên từ báo cáo các task.
- Ảnh hưởng: docs/REUSE_REPORT.md, docs/pm-report-b.md

## [2026-08-12] RESUME phiên (phiên trước gián đoạn — chưa tạo file nào)
- Quyết định: Kiểm tra hiện trạng: frontend/ backend/ docs/REUSE_REPORT.md CHƯA tồn tại → phiên trước chỉ ghi decision log rồi dừng trước khi dispatch. Tiếp tục thực hiện toàn bộ SESSION B theo plan đã ghi (5 task). Dispatch qua agent chuyên biệt: dev-frontend (task 1,2,3), dev-backend (task 4), dev (task 5 verify).
- Ảnh hưởng: toàn bộ phiên — các mục decision phía dưới (1-8) giữ nguyên hiệu lực.

## [2026-08-12] shared/simulation-catalog.json = nguồn catalog chính
- Quyết định: shared/simulation-catalog.json đã tồn tại tại root (SDD §4.5, §6.1) → engine frontend phải sinh engines/catalog.ts KHỚP danh sách key trong file này (đồng bộ backend §9.9); không bê catalog cũ của source.
- Ảnh hưởng: frontend/src/engines/catalog.ts

## [2026-08-12] .env.example V1 chứa secret — KHÔNG bê nguyên
- Quyết định: .env.example của V1 chứa connection string Supabase thật + Cloudinary key → KHÔNG bê nguyên. Viết .env.example MỚI cho frontend (VITE_API_BASE_URL) và backend (DSA__Jwt__Secret, ConnectionStrings SQL Server, SMTP MailHog) theo DEPLOY.md §2; chỉ giữ pattern đặt tên biến JWT.
- Ảnh hưởng: frontend/.env.example, backend/.env.example

## [2026-08-12] package.json frontend: chọn dependency theo SDD, cắt thừa
- Quyết định: Bê package.json V1 làm nền nhưng RÚT GỌN theo SDD §3.1/§3.9: giữ vue/pinia/vue-router/axios + monaco (Màn 16 Code Runner theo SDD §2.1) + dev deps (vite, plugin-vue, vitest, vue-tsc, @vue/test-utils, typescript). Cắt gói feature đã cắt/ngoài phạm vi (signalr, tsparticles, aos, chart.js, qrcode, yjs, xlsx, lottie, gsap... chỉ thêm khi task engine cần — ghi lý do).
- Ảnh hưởng: frontend/package.json

## [2026-08-12] Hoàn thành 5 task — build pass, không vi phạm cấm
- Quyết định: Tất cả task hoàn thành: task-1 skeleton frontend (build pass, 5/5 test), task-2 engine EDV + simulator (build pass, 39/39 test, catalog 44/44 khớp shared/simulation-catalog.json), task-3 seed/curriculum (build pass, 12/12 test, dotnet build pass), task-4 backend skeleton + docker (dotnet build pass 0 warning, 8/8 test, smoke /health + /swagger OK), task-5 verify tổng thể (PASS; check cấm KHÔNG vi phạm: không Npgsql/PostgreSQL/Supabase/MediatR/Repository code, không .env thật; compliance liệt kê lệch — chủ yếu là phần còn TODO placeholder). Đánh giá: skeleton milestone ĐẠT, coi là nền cho các task triển khai module sau.
- Ảnh hưởng: docs/REUSE_REPORT.md (tổng hợp bảng bê/không bê), docs/pm-report-b.md (trạng thái).

## [2026-08-12] Lệch chủ ý khi triển khai (đã xác nhận hợp lệ)
- Quyết định: (1) DTO dùng chung đặt Application/Dtos (SDD §5.1 tự mâu thuẫn Api/Dtos vs Validators ở Application — chọn Application để tránh reference vòng); (2) Asp.Versioning bỏ versioned ApiExplorer (mất controller trong OpenAPI .NET 10 — giữ 1 version v1 + suppress AV0021/AV0029); (3) runMeasure chạy main-thread (TODO chuyển Web Worker theo ADR-012); (4) TraceKind suy diễn heuristic từ interpreter cũ (chưa gắn kind trực tiếp); (5) font thiếu niên (Baloo 2/Comic Neue) KHÔNG bê — thay font hệ thống cho đối tượng đại học; (6) PlaceholderView thêm để route chưa có view thật. Các lệch này đều có chú thích trong code/README.
- Ảnh hưởng: pm-report-b.md mục LỆCH.

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

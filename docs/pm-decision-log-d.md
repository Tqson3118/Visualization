# PM DECISION LOG — SESSION D (Code thật thay stub — Đợt D)

> Ngày: 12/08/2026 · Chế độ: --auto · Trạng thái: docs/pm-report-d.md

## [2026-08-12] Khởi động SESSION D — phạm vi 4 task, 2 luồng
- Quyết định: Triển khai Đợt D theo lệnh user: (1) backend-services, (2) backend-seed, (3) engine-generators, (4) views. Backend 2 task TUẦN TỰ (cùng khu vực); frontend 2 task TUẦN TỰ (cùng khu vực); 2 luồng backend/frontend chạy SONG SONG (khác khu vực, khác nhánh — an toàn git).
- Ảnh hưởng: 4 nhánh feature mới từ dev: feature/backend-services, feature/backend-seed, feature/engine-generators, feature/views.

## [2026-08-12] Nguồn chuẩn & cấm đoán
- Quyết định: Mọi code theo PRODUCTION_PROMPT.md → SDD → API_REFERENCE.md → SCREEN_MAP.md. KHÔNG đoán, KHÔNG bê code C# cũ từ source/ (REUSE_REPORT §3 mục 1-2, 14). Cấm PostgreSQL/MediatR/Repository/secret thật — verify bằng grep trước khi báo xong.
- Ảnh hưởng: toàn bộ code sinh ra trong 4 task.

## [2026-08-12] Migration InitialCreate — chiến lược DB
- Quyết định: SQL Server qua docker-compose (container mssql đã có image; docker đang chạy). Nếu không nối được DB trong phiên (SA password/port/khởi động chậm) → dùng dotnet ef migrations add InitialCreate với connection string local (Server=localhost) — dotnet ef migrations add KHÔNG cần DB tồn tại (chỉ database update mới cần). Ghi rõ trạng thái vào report. KHÔNG hardcode secret: dùng biến môi trường/appsettings.Development.json.
- Ảnh hưởng: backend/src/DsaVisual.Application/Persistence/Migrations/*, appsettings*, .env.example.

## [2026-08-12] Phân công commit theo commit-as.ps1
- Quyết định: backend → bao, frontend → son, engine → thu, seed/tài liệu → phuc (đúng lệnh user). Mỗi task = 1 nhánh feature từ dev; KHÔNG commit thẳng main. Push nhánh lên origin; PR vào dev thực hiện bằng merge cục bộ khi không có gh CLI (ghi rõ trong report).
- Ảnh hưởnng: git history, docs/SETUP_TODO.md.

## [2026-08-12 06:40] Task 1 hoàn thành — backend-services (bao)
- Quyết định: 11 service thật + TokenService/JWT HS256 + PasswordHasher PBKDF2 + 17 controller (14 mới) + ~55 DTO + 7 validator + Migration InitialCreate. dotnet ef database update CHẠY THÀNH CÔNG lên SQL Server docker (32 bảng). Settings tách controller riêng (khớp route /settings); reorder dùng body {ids}; benchmark client gửi kèm results (ADR-012); 2FA 501 TODO.
- Verify: build 0 warning, test 44/44, smoke /health 200, grep cấm sạch. Đã merge vào dev + push.
- Ảnh hưởng: backend/ toàn bộ; SETUP_TODO mục 1.

## [2026-08-12 06:45] Task 3 hoàn thành — engine-generators (thu)
- Quyết định: 22 file generator thật (helpers + sort/search/linear/tree/heap/hash/graph/structure), 44/44 key đăng ký, inputSchema/pseudocode đầy đủ; catalog.spec.ts giữ 4 test đồng bộ + 6 test generator thật. GOLDEN bubble [3,1,2]: 21 bước; swaps=2 (mâu thuẫn nội tại bảng mốc vàng SDD ghi 3 — trace trung thực theo mã thật, ghi chú trong code).
- Verify: build PASS, test 44/44. Đã merge vào dev + push.
- Ảnh hưởng: frontend/src/engines/*.

## [2026-08-12 07:20] Task 2 hoàn thành — backend-seed (phuc)
- Quyết định: SeedRunner.cs idempotent (Users 3 + Topics 5 + Lessons 8 + LessonSimulations 14 + Exercises 29 + Questions 76 + Paths 5 + Nodes 18 + Quests 8 + Shop 8 + Settings 9); markdown→HTML + sanitize; hook --seed trong Program.cs; Seed README liệt kê 40 bài nguồn. AVL không có bài nguồn → content + 7 quiz tự soạn (ghi rõ). Seed chạy THẬT 2 lần (lần 2 bỏ qua — idempotent ✓).
- Verify: build 0 warning, test 44/44. Đã merge vào dev + push.
- Ảnh hưởng: backend Seed/*, Program.cs; Users chưa có cột MustChangePassword (SETUP_TODO mục 5.3).

## [2026-08-12 07:45] Task 4 hoàn thành — views (son)
- Quyết định: 33 view mới (0 route Placeholder), 13 UI component + 10 simulator + 5 ladder + AppHeader/AdminNav/BenchmarkPanel; 8 api module hoàn thiện + 3 mới (favorites/classes/benchmark); 7 store triển khai thật (simulation dùng generator thật); router 33 route + redirect /learn→/path, /dashboard→/profile. Không có monaco/chart.js → textarea + SVG (ghi chú); tokens teal #0D9488 bám repo.
- Verify: build PASS, test 50/50 (6 mới), smoke UI chrome-devtools 0 lỗi console. Đã merge vào dev + push.
- Ảnh hưởng: frontend/src toàn bộ; SETUP_TODO mục 5.

# PM REPORT — SESSION D (Code thật thay stub — Đợt D)

> Ngày: 12/08/2026 · Chế độ: --auto · Quyết định: docs/pm-decision-log-d.md · Việc cần user: docs/SETUP_TODO.md §5

## 1. Mục tiêu
Triển khai code THẬT thay stub theo nguồn chuẩn (PRODUCTION_PROMPT → SDD → API_REFERENCE → SCREEN_MAP): (1) backend services/controllers/DTO/migration, (2) seed thật, (3) engine generators 44 key, (4) views thật. Mỗi task = nhánh feature từ dev → dev tự verify → PM review → merge vào dev. KHÔNG bê code C# cũ.

## 2. Trạng thái task

| Task | Nội dung | Nhánh | Agent | Kết quả | Verify (PM độc lập) |
|---|---|---|---|---|---|
| task-1 | 11 service thật + TokenService + 14 controller mới + ~55 DTO + 7 validator + Migration InitialCreate (đã update DB 32 bảng) | feature/backend-services | dev-backend | **DONE** | build 0 warning · test 44/44 · smoke /health 200 · grep cấm sạch |
| task-2 | Seed thật idempotent: Users 3 + Topics 5 + Lessons 8 + Exercises 29 + Questions 76 + Paths 5 + Nodes 18 + Quests 8 + Shop 8 + Settings 9; README 40 bài nguồn; hook --seed | feature/backend-seed | dev-backend | **DONE** | build 0 warning · test 44/44 · seed chạy 2 lần idempotent · grep sạch |
| task-3 | Generator THẬT 44/44 key (22 file: sort/search/linear/tree/heap/hash/graph/structure) + inputSchema/pseudocode + catalog.spec.ts sửa (giữ 4 test đồng bộ + 6 test generator) | feature/engine-generators | dev-frontend | **DONE** | build PASS · test 44/44 · golden bubble 21 bước · grep stub = 0 |
| task-4 | 33 view thật (0 route Placeholder) + 13 UI + 10 simulator + 5 ladder + AppHeader + 3 api module mới + 7 store thật + router 33 route + redirect | feature/views | dev-frontend | **DONE** | build PASS · test 50/50 · smoke UI 0 console error |

**Tổng: 4/4 DONE. Không task FAIL/SKIP.** Tất cả đã merge vào dev + push origin (commit 3426fd0, c8f32be+a1f32e9, f975129/515706a/ae19db8, 2fb86d3; merge d1ab265, 5cbf9eb).

## 3. File thay đổi (đã merge vào dev)
- **backend/**: Services/ (AuthService 404 dòng JWT thật, User, Topic, Exercise, Progress, SimulationCatalog, Favorite, Setting, Class, CodeRunner, Gamification + TokenService, SettingsCache, LoginAttemptTracker, SubmissionLockRegistry, PasswordHasher, PasswordPolicy) · Controllers/ (Auth, Public, Topics, Simulations, Exercises, Progress, Users, Favorites, Admin, Settings, Classes, Me, CodeRuns, Gamification, Feedback + ApiControllerBase) · Dtos/ ~55 file · Validators/ 7 file · Persistence/Migrations/ InitialCreate (32 bảng) · Persistence/Seed/ (SeedRunner.cs + SeedData.cs đủ + README 40 bài) · Program.cs (--seed hook) · Configurations/ EF đầy đủ · tests/UnitTests 44 test (Auth/Exercise/Gamification/Topic/PasswordPolicy).
- **frontend/**: engines/generators/ 22 file + catalog.ts (generator thật) + catalog.spec.ts (10 test) · views/ 33 file (Register, ForgotPassword, ResetPassword, Lesson, Simulations, Path, NodeHub, FinalTest, Ladder, Lab, CodeRunner, Benchmark, Leaderboard, Profile, Admin×5, Classes×3, Shop, Quests, Premium, Subscription, CheatSheet, Help, Privacy + upgrade Simulator) · components/ui 13 + simulator 10 + ladder 5 + AppHeader/AdminNav/BenchmarkPanel · api/ 11 module (thêm favorites/classes/benchmark, hoàn thiện 8 module) · stores/ 7 store thật · router 33 route + redirect.

## 4. Kết quả verify tổng thể (đã chạy lại trên dev)
| Lệnh | Kết quả |
|---|---|
| dotnet build DsaVisual.sln | PASS — 0 warning / 0 error |
| dotnet test DsaVisual.sln | PASS — 44/44 |
| npm run build (frontend) | PASS — 0 lỗi (engine chunk 459KB/115KB gzip) |
| npm test (frontend) | PASS — 50/50 |
| Smoke API | GET /health → 200 {"status":"ok"} |
| Smoke UI (chrome-devtools) | /login, /register, /simulator/sort.bubble, /help, guard /path — 0 console error |
| Seed DB | SQL Server docker healthy; 5/8/29/76/5/18 bản ghi; chạy lần 2 bỏ qua hết (idempotent) |
| Grep cấm (backend/src + frontend/src) | KHÔNG vi phạm — không Npgsql/PostgreSQL/MediatR/Judge0/Repository/secret thật; không còn "implement in task tiếp" |

## 5. Quyết định / lệch chủ ý (chi tiết docs/pm-decision-log-d.md)
- Settings tách controller riêng (khớp route /settings); reorder topics body {ids}; benchmark client gửi kèm results (ADR-012); 2FA → 501 TODO.
- Golden bubble: swaps=2 theo trace thật (SDD §4.9A bảng ghi 3 — mâu thuẫn nội tại, ghi chú code).
- AVL không có bài nguồn trong 40 drafts → content + 7 quiz tự soạn (3 mục chuẩn).
- Users chưa có cột MustChangePassword (cần migration nếu giữ tính năng — SETUP_TODO §5.3).
- Không có monaco-editor/chart.js trong package.json → textarea + SVG (ghi chú code, SETUP_TODO §5.4).
- tokens.css Primary teal #0D9488 (bám repo — không đổi sang SDD #2563EB).

## 6. Việc còn tồn đọng (đề xuất task sau)
1. Chạy toàn bộ app (backend + seed + frontend) cho user xem thật; chụp 12 màn Playwright cho báo cáo (mục 4 HANDOFF).
2. Migration bổ sung cột MustChangePassword (nếu giữ FR "ép đổi mật khẩu lần đầu").
3. 2FA qua email (Màn N-1) — hiện 501 TODO.
4. Cài monaco-editor/chart.js khi cần Code Runner + benchmark đồ thị đầy đủ.
5. IntegrationTests thật với Testcontainers (test contract API — API_REFERENCE §7) — hiện mới có UnitTests.
6. runMeasure chuyển Web Worker (ADR-012 — TODO cũ từ Session B); renderer canvas thật cho từng CTDL (SDD §8.3 — hiện SimulatorView dùng CoreAnimationEngine cơ bản).
7. e2e playwright (frontend/tests/e2e README đã có hướng dẫn).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.

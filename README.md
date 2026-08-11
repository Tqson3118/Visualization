# DSA-Visual — Hệ thống hỗ trợ học tập và trực quan hóa CTDL & Giải thuật

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật** — ứng dụng web giúp sinh viên hiểu sâu CTDL/GT bằng mô phỏng từng bước (EDV), Practice Ladder 3 bậc và theo dõi tiến độ cá nhân.

| | |
|---|---|
| Phiên bản | 0.1 (pre-alpha — tài liệu v1.0) |
| Năm | 2026 |
| Công nghệ | Vue 3 + Pinia + Vite + TypeScript · C# ASP.NET Core 8 · SQL Server + EF Core 8 · JWT |
| Trạng thái | Khởi tạo — xem [docs/README.md](./docs/README.md) cho trạng thái tài liệu |

---

## 1. Giới thiệu nhanh

- **Mô phỏng EDV**: mọi giải thuật là mã TypeScript thật chạy qua `StepExecutor`; hoạt ảnh = phát lại trace — "code đến đâu, chạy visual đến đó".
- **Practice Ladder**: mỗi bài học = 3 bậc luyện tập (Trắc nghiệm → Kéo thả → Lập trình) chấm tự động.
- **Gamification**: Tim/Gems/Quest/Streak/XP/Leaderboard + Premium (checkout mô phỏng).
- **Hồ sơ tài liệu đầy đủ**: SRS, SDD, API, TEST_PLAN, USER_GUIDE, DEPLOY — xem [docs/](./docs/).

## 2. Cấu trúc repo (dự kiến)

```
.
├── docs/                    # Bộ tài liệu 12 file (mục lục: docs/README.md)
├── shared/
│   └── simulation-catalog.json   # Danh mục mô phỏng FE/BE (nguồn duy nhất khóa key)
├── frontend/                # Vue 3 + Vite + TS (kể cả engines/ — Simulation Engine)
├── backend/
│   ├── src/
│   │   ├── DsaVisual.Api/         # Web API (controllers, DTOs, middleware)
│   │   └── DsaVisual.Application/ # Services + AppDbContext + Migrations
│   └── tests/               # Unit + Integration
├── scripts/                 # sync-catalog, check-catalog-sync, deploy scripts
└── THIRD_PARTY.md           # Thư viện mã nguồn mở + license
```

## 3. Cài đặt môi trường dev

### Yêu cầu
- .NET SDK 8.0+, Node.js 20+, SQL Server 2019+ (hoặc Docker — `docker-compose.dev.yml` trong DEPLOY §3.3).
- PowerShell (Windows) / bash (Linux/macOS).

### Bước 1 — Backend

```powershell
cd backend
dotnet restore
# Dev không có SQL Server: dùng SQLite
$env:ConnectionStrings__Default = "Data Source=dsavisual-dev.db"
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet run --project src/DsaVisual.Api --seed
# Swagger: http://localhost:5000/swagger
```

### Bước 2 — Frontend

```powershell
cd frontend
npm install
npm run dev
# http://localhost:5173 (proxy /api → localhost:5000)
```

### Bước 3 — Kiểm tra nhanh

1. Mở `http://localhost:5173` → đăng ký tài khoản.
2. Vào "Học tập" → mở node "Bubble Sort" → chạy mô phỏng.
3. Chạy test: `cd frontend && npm run test:unit` · `dotnet test backend/DsaVisual.sln`.

> Chi tiết triển khai production (nginx/systemd/backup/runbook): [docs/DEPLOY.md](./docs/DEPLOY.md).

## 4. Lệnh hằng ngày

| Việc | Lệnh |
|---|---|
| Chạy frontend dev | `cd frontend && npm run dev` |
| Chạy backend dev | `cd backend && dotnet run --project src/DsaVisual.Api` |
| Unit test FE | `cd frontend && npm run test:unit` |
| Lint FE | `cd frontend && npm run lint` |
| Build FE | `cd frontend && npm run build` |
| Test BE | `dotnet test backend/DsaVisual.sln` |
| Integration test (cần Docker) | `dotnet test --filter "Category=Integration"` |
| E2E | `cd frontend && npm run test:e2e` |
| Seed lại dữ liệu mẫu | `dotnet run --project src/DsaVisual.Api --seed` |

## 5. Quy tắc làm việc nhóm (bắt buộc — nguồn prompt §2.7)

1. **Git workflow**: nhánh `main` (ổn định) + `develop` + feature branch `feat/<tên>`; merge qua Pull Request có **ít nhất 1 review**; không commit thẳng vào `main`/`develop`.
2. **Commit message** theo chuẩn Conventional Commits:
   - `feat:` tính năng mới · `fix:` sửa lỗi · `docs:` tài liệu · `test:` kiểm thử · `refactor:` tái cấu trúc.
   - VD: `feat(engine): add StepExecutor trace hook`, `fix(auth): return 401 instead of 500 on bad refresh`.
3. **Cập nhật tài liệu ngay khi code thay đổi** — không để SRS/SDD lệch với code (quy tắc 2.7 mục 3).
4. **Họp đứng 2 lần/tuần**; cập nhật bảng tiến độ dùng chung.
5. **CẤM commit**: `.env`, `appsettings.*.json` chứa secret, `node_modules/`, `bin/`, `obj/`, `dist/`, file backup DB (bài học từ bản cũ — secret lộ git).
6. **CẤM sửa DB trực tiếp** — mọi thay đổi schema qua EF Core Migrations.

## 6. Quy tắc code (tóm tắt — chi tiết SDD §3.8/§5.3)

| Phía | Quy tắc chính |
|---|---|
| Frontend | TypeScript strict; `<script setup>`; không `any`; chuỗi UI trong `src/i18n/vi.ts`; hàm ≤ 40 dòng; class ≤ 400 dòng |
| Backend | 2 project (Api + Application); Service dùng DbContext qua DbSet (không Repository); trả `Result<T>`; FluentValidation; error code trong bảng API_REFERENCE §2.2; thời gian UTC |
| Engine | generator thuần túy, không đụng DOM; `engines/catalog.ts` nguồn duy nhất key; CẤM hardcode chuỗi bước |
| Chung | Không static state; DI constructor; log bằng `ILogger<T>` (Serilog) — không `Console.WriteLine` |

## 7. Liên kết tài liệu

| Tài liệu | Nội dung |
|---|---|
| [docs/README.md](./docs/README.md) | Mục lục + ma trận ánh xạ yêu cầu → tài liệu |
| [docs/SRS.md](./docs/SRS.md) | Đặc tả yêu cầu phần mềm |
| [docs/SDD.md](./docs/SDD.md) | Thiết kế hệ thống (EDV, DB 32 bảng, 32 màn) |
| [docs/API_REFERENCE.md](./docs/API_REFERENCE.md) | Tham chiếu API |
| [docs/TEST_PLAN.md](./docs/TEST_PLAN.md) | Kế hoạch kiểm thử |
| [docs/USER_GUIDE.md](./docs/USER_GUIDE.md) | Hướng dẫn sử dụng |
| [docs/DEPLOY.md](./docs/DEPLOY.md) | Triển khai & vận hành |
| [docs/GLOSSARY.md](./docs/GLOSSARY.md) | Thuật ngữ |
| [docs/SCREEN_MAP.md](./docs/SCREEN_MAP.md) | Bản đồ màn hình |
| [docs/PRODUCTION_PROMPT.md](./docs/PRODUCTION_PROMPT.md) | Nguồn yêu cầu gốc |
| [THIRD_PARTY.md](./THIRD_PARTY.md) | Thư viện mã nguồn mở + license |
| [docs/BAO_CAO_SPEC.md](./docs/BAO_CAO_SPEC.md) | Đặc tả báo cáo Word |

## 8. Kiến trúc tổng quan (tóm tắt — chi tiết SDD §2)

```
Trình duyệt (SPA Vue 3)                        Máy chủ (ASP.NET Core)
┌──────────────────────────────┐              ┌─────────────────────────────┐
│ Giao diện + Pinia stores     │  REST+JWT   │ Controllers /api/v1/*       │
│ Simulation Engine (EDV):     │ ───────────▶ │ Services (nghiệp vụ)        │
│  StepExecutor → TraceEvent[] │              │ AppDbContext (EF Core)     │
│  Generator + Renderer        │              └──────────────┬──────────────┘
│ Code Runner (Web Worker)     │                             │
└──────────────────────────────┘              ┌──────────────▼──────────────┐
                                              │ SQL Server 2019+ (32 bảng)  │
                                              └─────────────────────────────┘
```

- **EDV**: mọi GT = mã TypeScript thật chạy qua StepExecutor; hoạt ảnh = phát lại trace (SDD §4.0).
- **Chấm bài code**: chạy sandbox Web Worker phía client — không Judge0 server (ADR-012).
- **Sinh bước**: frontend, batch (ADR-001) — bước lùi miễn phí, test dễ.
- **Trừ tim**: transaction atomic + `NodeSessions` UNIQUE (UserId, NodeId) chống double-spend multi-tab (SDD §7.3.29).

## 9. Bối cảnh và bài học từ bản cũ (bắt buộc đọc trước khi code)

Bản cũ (VisualizationDSA) bị hội đồng chấm phản hồi 3 lỗi gốc — thiết kế mới khắc phục triệt để:

| # | Phản hồi hội đồng | Cách khắc phục |
|---|---|---|
| 1 | "Cho code đến đâu, chạy visual đến đó" — bản cũ hardcode hoạt ảnh | Kiến trúc **EDV**: mọi GT là mã thật chạy qua StepExecutor, hoạt ảnh = trace thật |
| 2 | 1 màn gộp 4 chức năng (học + visual + code + quiz) | Nguyên tắc **"1 màn = 1 việc"**: mỗi route 1 nhiệm vụ; cấm nhúng chức năng chéo màn |
| 3 | Scope trôi dạt (payment thật, realtime...) | Loại trừ rõ (Premium = checkout MÔ PHỎNG; không realtime); 12 FR đã duyệt cắt; 20 tuần 10 sprint |

## 10. Roadmap (20 tuần / 10 sprint — nguồn prompt §20.1)

| Sprint | Tuần | Mục tiêu |
|---|---|---|
| S1 | 1-2 | Khởi động: repo, SRS v1, scaffold FE/BE |
| S2 | 3-4 | Lõi backend: Auth + Topics/Lessons CRUD + RBAC |
| S3 | 5-6 | Engine EDV cơ bản: StepExecutor + 3 GT đầu |
| S4 | 7-8 | Giao diện học tập: LearnView, SimulatorView, Learning Path (demo giữa kỳ) |
| S5 | 9-10 | Mở rộng engine: đủ 15 GT |
| S6 | 11-12 | Practice Ladder bậc 1-2 + Code Runner phần 1 |
| S7 | 13-14 | Code Runner phần 2: bài tập code + test ẩn (⚠ rủi ro cao) |
| S8 | 15-16 | Gamification lõi: Tim/Streak/XP/Quest/Leaderboard |
| S9 | 17-18 | Premium + Class + Benchmark Lab (⚠ rủi ro cao) |
| S10 | 19-20 | Hoàn thiện: test toàn diện, tài liệu 12 file, demo cuối kỳ |

> Chi tiết: docs/PM_MASTER_PLAN.md (kế hoạch sản xuất), docs/BAO_CAO_SPEC.md (báo cáo Word).

## 11. Trạng thái hiện tại

- **Tài liệu**: 12/12 file bàn giao đã sinh (SRS 1771 dòng, SDD 3725 dòng, API 735, TEST 780, USER_GUIDE 507, DEPLOY 404, GLOSSARY 105, catalog 44 entries, THIRD_PARTY 83, docs/README 209, README root 194, SCREEN_MAP 326) — xem docs/README §1.1.
- **Code v2**: CHƯA khởi tạo — toàn bộ cấu trúc `frontend/`, `backend/src/DsaVisual.Api`... trong tài liệu là **đặc tả dự kiến**, chưa phải code thật. Code hiện có nằm ở `VisualizationDSA/` (bản v1 cũ — PostgreSQL + Clean Architecture, KHÔNG dùng làm chuẩn cho v2).
- **Bước tiếp theo**: (1) giảng viên phê duyệt SRS/SDD; (2) bê code tái dùng từ `VisualizationDSA/` theo PM_MASTER_PLAN task B1; (3) khởi tạo repo v2 + cập nhật tài liệu cho khớp code thật.

## 12. Liên hệ

| Vai trò | Thành viên | Mã SV |
|---|---|---|
| Trưởng nhóm (Backend) | Mai Tiểu Bảo | TD01287 |
| Frontend | Thái Quang Sơn | TD01282 |
| Simulation Engine + Kiểm thử | Huỳnh Lê Minh Thư | TD01131 |
| Tài liệu + Triển khai | Trần Viết Tâm Phúc | TD01261 |
| Giảng viên hướng dẫn | Phạm Ngọc Ái Liên | — |

> Tài liệu này là tài liệu sống — cập nhật mỗi khi code thay đổi (quy tắc 2.7 mục 3).

## 13. Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Sinh mới (setup, lệnh chạy, quy tắc nhóm, roadmap 20 tuần) |
| 1.1 | 12/08/2026 | Trần Viết Tâm Phúc | Vá review: xóa bảng "Liên kết tài liệu" trùng lặp cuối file (trùng Mục 7); cập nhật "31 bảng" → "32 bảng" (NodeSessions + UserNodeProgress); thêm mục Lịch sử thay đổi |

# 🚀 DEV_SETUP — Môi trường & Quy ước (cho mọi agent/dev)

> **Ngày:** 2026-08-05 · **Áp dụng:** mọi session code VisualizationDSA
> **Nguồn plan:** `docs/update85/implementation_plan_detail.md` (đọc trước khi code)

---

## 1. Stack & Môi trường

| Thành phần | Version | Cổng |
|---|---|---|
| .NET SDK | 9.0 / 10.0 (đã cài) | — |
| Node | v24.15.0 · npm 11.12.1 | — |
| Docker Compose | backend + frontend + postgres + judge0 | 5055 / 5173 / 5433 / 2358 |

### Khởi chạy (Docker)
```bash
# từ thư mục VisualizationDSA
docker compose up -d        # backend 5055, frontend 5173, postgres 5433, judge0 2358
```
> Container đang chạy: `vdsa-backend`, `vdsa-frontend`, `vdsa-database`, `vdsa-judge0-api`.

### Chạy dev trực tiếp (không Docker)
```bash
cd backend/src/WebApi && dotnet run          # cần ConnectionStrings__DefaultConnection
cd frontend && npm run dev                    # Vite tại 5173
```

## 2. Lệnh kiểm chứng (cổng vào — chạy SAU MỖI task)

```bash
cd frontend && npm run build                          # gồm vue-tsc -b, phải xanh
cd frontend && npx vitest run                         # 822 test phải pass
cd backend  && dotnet build src/WebApi/WebApi.csproj  # phải xanh (KHÔNG có .sln ở backend root)
cd backend  && dotnet test tests/VisualizationDSA.UnitTests  # 50 test phải pass
```

## 3. Tài khoản test (3 vai)

| Vai | Email | Mật khẩu | Ghi chú |
|---|---|---|---|
| Admin | `admin@visualizationdsa.dev` | `Admin@2024` | Seed G4.1 — role Admin |
| Teacher | `demo@visualizationdsa.dev` | `Demo@2024` | Seed G4.1 — role Teacher, Premium |
| Student | `nguyenvana@visualizationdsa.dev` | `User@2024` | Seed G4.1 — role Student |

> Tài khoản 3 vai đã seed sẵn (G4.1.4) qua `DbSeeder.SeedLeaderboardUsersAsync`. Trước khi seed, có thể tạo thủ công qua `/concepts/auth/register`.

## 3b. Biến môi trường bắt buộc

| Biến | Bắt buộc | Ghi chú |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | ✅ | Postgres dev (docker: `Host=database;...`) |
| `Jwt__Key` | ✅ | Secret JWT, fail-fast nếu thiếu |
| `DEEPSEEK__APIKEY` | ✅ | Key DeepSeek thật (AI chat) |
| `SEPAY__WEBHOOKSECRET` | ✅ | HMAC verify webhook SePay |
| `Cloudinary__ApiSecret` | ⚠️ | Cần khi upload thumbnail (G3.5.5) |

## 4. Database

- **Postgres** Docker: cổng 5433, database `visualization_dsa_dev` (xem `.env`/docker-compose).
- **Seeder hiện tại:** `backend/src/Infrastructure/Data/DbSeeder.cs` (427 dòng) + `Data/SeedContent/` (courses.json 18KB, 3 lesson md, teacher_roadmaps.json).
- ⚠️ **Nội dung seed còn sơ sài** — task G4.1 sẽ làm seed chất lượng (≥3 roadmap × 5-8 bài × quiz 10-15 câu).
- Seeder tự chạy khi app khởi động (mỗi phần có guard `if Any() return`).

## 5. Quy ước code (bắt buộc)

- Không `any`/`dynamic` (trừ tạm + TODO).
- Không `alert()/confirm()` — dùng `ToastContainer` (đã có sẵn trong App.vue).
- Màu qua token (`bg-bg-surface`, `text-text-primary`...) — cấm hardcode.
- Build/test xanh trước khi kết thúc mỗi task.
- Sau mỗi 2-3 task: screenshot dark+light → lưu `docs/screenshots/`.

## 6. Tham chiếu UI (dev không xem ảnh — dùng LINK làm chuẩn)

> Mở các link sau khi cần biết layout/UX chuẩn. Đây là "đích đến" thay vì tự bịa.

| Màn cần làm | Tham chiếu |
|---|---|
| Journey path (roadmap) | https://coddy.tech/journeys/cpp/fundamentals · /cpp/sections |
| Profile | https://coddy.tech/profile |
| Shop / gems | https://coddy.tech/store |
| Nhiệm vụ / quest | https://coddy.tech/missions |
| Docs | https://coddy.tech/docs |
| Landing | https://coddy.tech (chính) |
| Visualizer thuần | https://clementmihailescu.github.io/Pathfinding-Visualizer/ |
| Code editor 3-pane | https://leetcode.com/problems/two-sum/ |
| Gamified learning | https://coddy.tech + https://www.duolingo.com |

## 7. Trạng thái Docker hiện tại (2026-08-05)

```
CONTAINER         IMAGE                       PORTS                STATUS
vdsa-frontend     visualizationdsa-frontend   5173->80             Up
vdsa-backend      visualizationdsa-backend    5055->5055           Up
vdsa-database     postgres:15-alpine          5433->5432           Up (healthy)
vdsa-judge0-api   judge0/judge0:1.13.0        2358->2358           Up
```

## 8. Quy trình làm việc chuẩn

```
1. Đọc implementation_plan_detail.md → chọn task theo G0→G4
2. Mở link tham chiếu UI phù hợp (mục 6)
3. Code theo DoD của task
4. Chạy build + test (mục 2)
5. Screenshot dark+light nếu là màn UI
6. Tóm tắt cho user duyệt trước khi sang task mới
```

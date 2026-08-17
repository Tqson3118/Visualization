# HƯỚNG DẪN TRIỂN KHAI VÀ VẬN HÀNH (DEPLOY)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Hướng dẫn triển khai & vận hành |
| Phiên bản | 1.0 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Trần Viết Tâm Phúc |
| Người duyệt | Phạm Ngọc Ái Liên |
| Độc giả | DevOps, admin, thành viên phụ trách triển khai |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md Phần 15 (triển khai), 10.6 (backup), 11.6 (email), 14.4 (quy trình chạy) |

> ⚠ **TRẠNG THÁI**: tài liệu hướng dẫn triển khai cho **hệ thống v2 (dự kiến)** — các lệnh/cấu trúc mô tả theo đặc tả SDD (project `DsaVisual.Api`/`DsaVisual.Application` chưa tồn tại trong code). Khi code v2 được khởi tạo, xác nhận lại đường dẫn project trước khi chạy lệnh.

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Sinh mới từ PRODUCTION_PROMPT.md v2.5 |
| 1.1 | 12/08/2026 | Trần Viết Tâm Phúc | Vá review: thêm sơ đồ Mermaid kiến trúc triển khai (§1.3), bảng môi trường (§2.3, renumber 2.3→2.4), tách mục 8 Kế hoạch rollback theo cấu trúc §17.3.6 |

---

# 1. YÊU CẦU HẠ TẦNG

## 1.1 Máy chủ thử nghiệm (tối thiểu)

| Tài nguyên | Tối thiểu | Khuyến nghị |
|---|---|---|
| CPU | 2 lõi | 4 lõi |
| RAM | 4 GB | 8 GB |
| Ổ cứng | 50 GB SSD | 100 GB SSD |
| Hệ điều hành | Windows Server 2019+ / Ubuntu 22.04+ | — |

## 1.2 Phần mềm

| Thành phần | Phiên bản | Ghi chú |
|---|---|---|
| .NET SDK | 8.0+ | build backend |
| ASP.NET Core Runtime | 8.0+ | chạy API |
| Node.js | 20+ | build frontend |
| SQL Server | 2019+ | production; dev có thể dùng SQLite/LocalDB (khác biệt migration ghi rõ §5.4) |
| Nginx | 1.24+ | reverse proxy + static files (Linux) |
| Docker (tùy chọn) | 24+ | dev môi trường chuẩn (SQL Server + MailHog) |

## 1.3 Kiến trúc triển khai (bắt buộc — nguồn PRODUCTION_PROMPT §15.1)

```mermaid
graph LR
    User((Người dùng)) --> LB[Nginx/Reverse Proxy<br/>443 TLS + static files]
    LB --> FE[Frontend static<br/>dist/]
    LB --> API[ASP.NET Core API<br/>Kestrel :5000]
    API --> DB[(SQL Server)]
    API --> SMTP[SMTP server (tùy chọn)]
```

**Giải thích**: Nginx đảm nhận 2 vai trò — phục vụ file tĩnh frontend (`dist/`) và reverse proxy `https://api.*/api/v1/*` sang Kestrel :5000; API truy cập SQL Server (production) và SMTP (tùy chọn, dev/staging dùng MailHog — §11.6 prompt, §3.3 tài liệu này). Mọi request vào production qua TLS 1.2+ (HSTS, NFR-13).

---

# 2. CHUẨN BỊ VÀ BIẾN MÔI TRƯỜNG

## 2.1 Biến môi trường Backend (đầy đủ)

```
# Bắt buộc
DSA__Jwt__Secret=<chuỗi ngẫu nhiên ≥ 32 ký tự — tuyệt đối không commit>
ConnectionStrings__Default=Server=localhost;Database=DsaVisual;User Id=sa;Password=<...>;TrustServerCertificate=True

# Tùy chọn (mặc định hợp lý)
DSA__Jwt__AccessTokenMinutes=60
DSA__Jwt__RefreshTokenDays=7
DSA__Cors__AllowedOrigins=https://dsa-visual.example.edu.vn
DSA__Email__SmtpHost / DSA__Email__SmtpPort / DSA__Email__From   (bỏ trống = chế độ log/MailHog)
DSA__Storage__Path=/var/lib/dsavisual/uploads
DSA__Simulation__MaxArraySize=100
DSA__Simulation__MaxGraphVertices=50
DSA__Auth__MaxLoginAttempts=5
DSA__Auth__LockoutMinutes=15
```

> Quy ước: biến dùng tiền tố `DSA__` (ánh xạ `appsettings.json` theo chuẩn ASP.NET Core); secret KHÔNG bao giờ trong `appsettings.Production.json` commit.

## 2.2 Biến môi trường Frontend

```
# .env.production
VITE_API_BASE_URL=https://api.dsa-visual.example.edu.vn/api/v1
```

> Không đặt secret nào ở frontend (mọi secret nằm phía backend).

## 2.3 Môi trường triển khai (nguồn PRODUCTION_PROMPT §15.2)

| Môi trường | URL (ví dụ) | Mục đích |
|---|---|---|
| Development | `localhost:5173` (Vite) + `localhost:5000` (API) | lập trình hằng ngày |
| Staging | `staging.dsa-visual.example.edu.vn` | kiểm thử trước khi lên prod |
| Production | `dsa-visual.example.edu.vn` | người dùng thật |

## 2.4 Tạo secret JWT (ví dụ)

```powershell
# PowerShell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

---

# 3. BUILD & CHẠY DEVELOPMENT

## 3.1 Backend

```powershell
# 1. Khôi phục + build
cd backend
dotnet restore
dotnet build

# 2. Cấu hình connection string (appsettings.Development.json hoặc env)
#    Dev dùng SQL Server local hoặc SQLite: ConnectionStrings__Default=Data Source=dsavisual-dev.db

# 3. Migration + seed
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet run --project src/DsaVisual.Api --seed    # seed idempotent (chạy lại không nhân đôi)

# 4. Kiểm tra
#    Swagger: http://localhost:5000/swagger
```

## 3.2 Frontend

```powershell
cd frontend
npm install
npm run dev        # Vite dev server :5173, proxy /api → localhost:5000
```

## 3.3 Docker dev (tùy chọn — SQL Server + MailHog)

```yaml
# docker-compose.dev.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "DsaVisual@Dev123"
    ports: ["1433:1433"]
  mailhog:
    image: mailhog/mailhog
    ports: ["1025:1025", "8025:8025"]   # SMTP 1025, UI 8025
```

- Dev/staging mặc định dùng **MailHog** (11.6): email không gửi thật, xem tại `http://localhost:8025`.
- Nếu chưa cấu hình SMTP: hệ thống ghi log + hiển thị link/mã trong log dev — KHÔNG block đăng ký/đăng nhập.

---

# 4. BUILD & TRIỂN KHAI PRODUCTION

> **Hệ điều hành máy chủ**: nhóm phát triển dùng Windows (môi trường dev), nhưng production đề xuất **Linux (Ubuntu 22.04)** — phần 4.1-4.4 là chuẩn Linux; **phần 4.5 là riêng cho Windows** (chọn 1 trong 2, không trộn lẫn).

## 4.1 Build frontend

```bash
cd frontend
npm ci
npm run build          # output dist/
```

## 4.2 Publish backend

```bash
cd backend
dotnet publish src/DsaVisual.Api -c Release -o /opt/dsavisual/api
```

## 4.3 Nginx (reverse proxy + static files)

```nginx
# /etc/nginx/sites-available/dsa-visual
server {
    listen 80;
    server_name dsa-visual.example.edu.vn;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name dsa-visual.example.edu.vn;

    ssl_certificate     /etc/ssl/certs/dsa-visual.crt;
    ssl_certificate_key /etc/ssl/private/dsa-visual.key;
    add_header Strict-Transport-Security "max-age=31536000" always;

    root /var/www/dsavisual;
    index index.html;

    location / { try_files $uri $uri/ /index.html; }        # SPA fallback

    location /api/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    client_max_body_size 10m;                                 # upload ảnh ≤ 5MB + lề
}
```

**Lưu ý X-Forwarded (Review E — chống XFF spoof)**: API chỉ tin `X-Forwarded-For/Proto` từ proxy ĐÃ KHAI BÁO.
- Nginx cùng máy chủ (`proxy_pass http://127.0.0.1:5000` ở trên) → tự tin (loopback luôn được phép).
- Nginx/load balancer ở MÁY KHÁC → khai báo IP qua biến môi trường:
  ```bash
  DSA__Proxy__KnownProxies__0=192.168.1.10     # IPv4
  DSA__Proxy__KnownProxies__1=2001:db8::10      # IPv6
  ```
- Không cấu hình → XFF từ internet BỊ BỎ QUA (fail-closed): rate limiter không bị spoof IP; nếu cookie refresh thiếu Secure khi chạy sau proxy ngoài, kiểm tra mục này trước.

## 4.4 Chạy API như service (systemd — Linux)

```ini
# /etc/systemd/system/dsavisual-api.service
[Unit]
Description=DSA-Visual API
After=network.target

[Service]
WorkingDirectory=/opt/dsavisual/api
ExecStart=/usr/bin/dotnet DsaVisual.Api.dll
Restart=always
EnvironmentFile=/etc/dsavisual/env      # chứa các biến DSA__*
User=www-data

[Install]
WantedBy=multi-user.target
```

```bash
systemctl daemon-reload
systemctl enable --now dsavisual-api
systemctl status dsavisual-api
```

## 4.5 Windows (IIS / Kestrel)

- IIS: cài ASP.NET Core Hosting Bundle, tạo site trỏ `DsaVisual.Api.exe` (in-process), HTTPS binding, upload limit `maxAllowedContentLength`.
- Hoặc chạy Kestrel đơn giản: `dotnet DsaVisual.Api.dll --urls http://0.0.0.0:5000` phía sau proxy.

---

# 5. CƠ SỞ DỮ LIỆU

## 5.1 Migration

```bash
# Sinh migration mới (khi đổi model)
dotnet ef migrations add AddLessonStatusColumn --project src/DsaVisual.Application --startup-project src/DsaVisual.Api

# Áp dụng
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

- EF Core Migrations là CÁCH DUY NHẤT thay đổi schema — không sửa DB trực tiếp (10.5).
- Migration có tên mô tả (`AddLessonStatusColumn`); review trước khi áp dụng production.

## 5.2 Seed (idempotent)

```bash
dotnet run --project src/DsaVisual.Api --seed
# Seed 8 bài học mẫu + 5 path + quiz/lab/code (~90 test ẩn) + settings
# Chạy lại KHÔNG nhân đôi dữ liệu (kiểm tra tồn tại trước khi chèn)
```

## 5.3 Backup & khôi phục (SQL Server)

> Đường dẫn backup tùy hệ điều hành: **Windows** dùng `D:\backups\...`; **Linux** dùng `/var/backups/dsavisual/...` (không trộn lẫn). Lịch chạy bằng SQL Agent (Windows) hoặc cron + sqlcmd (Linux).

```sql
-- Backup full hàng ngày 02:00 (giữ 14 bản — script job lịch)
BACKUP DATABASE DsaVisual TO DISK = 'D:\backups\DsaVisual_20260812.bak' WITH COMPRESSION;
-- Linux: '/var/backups/dsavisual/DsaVisual_20260812.bak'

-- Backup log mỗi 4 giờ
BACKUP LOG DsaVisual TO DISK = 'D:\backups\DsaVisual_20260812_0600.trn';

-- Restore (test restore 1 lần/tháng, ghi biên bản)
RESTORE DATABASE DsaVisual FROM DISK = 'D:\backups\DsaVisual_20260812.bak'
  WITH REPLACE, RECOVERY;
```

| Mục | Chính sách |
|---|---|
| Backup full | Hàng ngày 02:00, giữ 14 bản |
| Backup log | Mỗi 4 giờ |
| Test restore | 1 lần/tháng, ghi biên bản |
| Lưu trữ | Ổ KHÁC máy chủ (network share / object storage) |

## 5.4 SQLite/LocalDB ở dev

- Dev thiếu SQL Server → dùng SQLite (`ConnectionStrings__Default=Data Source=dsavisual-dev.db`).
- Khác biệt cần lưu ý: kiểu cột (nvarchar(max) → TEXT), ngày giờ, transaction — test integration LUÔN chạy trên SQL Server thật (Testcontainers) để tránh lệch hành vi.

---

# 6. CI/CD (GITHUB ACTIONS)

```yaml
# .github/workflows/ci.yml (mẫu rút gọn — bổ sung theo nhu cầu)
name: CI
on: [push, pull_request]

jobs:
  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: 20 }
      - run: cd frontend && npm ci
      - run: cd frontend && npm run lint
      - run: cd frontend && npx vue-tsc --noEmit
      - run: cd frontend && npm run test:unit
      - run: cd frontend && npm run build
      - run: npm audit --audit-level=high          # bảo mật dependency

  backend:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env: { ACCEPT_EULA: "Y", MSSQL_SA_PASSWORD: "DsaVisual@Ci123" }
        ports: ["1433:1433"]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet build backend/DsaVisual.sln
      - run: dotnet test backend/DsaVisual.sln
      - run: dotnet list backend/DsaVisual.sln package --vulnerable --include-transitive   # bảo mật

  catalog-sync:                                    # 9.9: FE/BE khóa key phải khớp
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: node scripts/check-catalog-sync.js    # so sánh shared/simulation-catalog.json vs engines/catalog.ts
```

- Deploy staging: tự động sau CI pass (script `scripts/deploy-staging.sh`).
- Deploy production: thủ công qua tag `release/*` (workflow deploy-prod.yml) — SSH + script.
- Trước deploy: backup DB (§5.3); migrate DB TRƯỚC khi deploy code.

---

# 7. GIÁM SÁT, LOG, RUNBOOK

## 7.1 Giám sát

| Mục | Chính sách |
|---|---|
| Log lỗi | Serilog rolling file `logs/` — giữ 90 ngày trực tuyến, sau đó nén + lưu trữ |
| Kiểm tra định kỳ | Hằng ngày: xem log 5xx, dung lượng ổ, service status |
| Cảnh báo | Email khi 5xx > 1% trong 5 phút (script đơn giản đọc log) |
| Cửa sổ bảo trì | 01:00-03:00 CN (nếu cần); thông báo trước 48h |

## 7.2 Runbook — Sự cố thường gặp (bảng bắt buộc)

| # | Triệu chứng | Nguyên nhân có thể | Các bước xử lý | Thời gian mục tiêu |
|---|---|---|---|---|
| 1 | API trả 503 liên tục | SQL Server ngừng | 1. Kiểm tra service SQL (services.msc / systemctl) 2. Khởi động lại 3. Kiểm tra log lỗi SQL 4. Restore backup nếu lỗi dữ liệu | 30 phút |
| 2 | 500 liên tục ở API | Lỗi code / config sai | 1. Xem log file gần nhất 2. Kiểm tra biến môi trường 3. Rollback phiên bản trước | 1 giờ |
| 3 | Đăng nhập chậm | Quá nhiều request hash mật khẩu | 1. Kiểm tra rate limit hoạt động 2. Tăng resources 3. Kiểm tra lockout DB | 1 giờ |
| 4 | Upload ảnh lỗi | Hết dung lượng ổ | 1. Kiểm tra `df -h` 2. Dọn upload tạm (job đêm) 3. Mở rộng ổ | 30 phút |
| 5 | Mô phỏng chậm phía client | Dữ liệu quá giới hạn / máy yếu | 1. Xác nhận giới hạn NFR-2 2. Gợi ý giảm kích thước dữ liệu 3. Kiểm tra phiên bản trình duyệt | 2 giờ |
| 6 | Token lỗi hàng loạt | Secret JWT bị thay đổi | 1. Kiểm tra `DSA__Jwt__Secret` 2. Khôi phục giá trị cũ 3. Người dùng đăng nhập lại | 30 phút |
| 7 | Email không gửi | SMTP lỗi | 1. Kiểm tra queue email trong DB 2. Kiểm tra kết nối SMTP 3. Bật lại service (hoặc dùng MailHog dev) | 1 giờ |
| 8 | Backup thất bại | Hết dung lượng / quyền | 1. Xem log job backup 2. Giải phóng dung lượng 3. Chạy lại thủ công | 1 giờ |
| 9 | Tim không hồi / trừ sai | Lỗi logic server timestamp | 1. Xem log thao tác `enter` 2. Kiểm tra đồng hồ máy chủ (NTP) 3. Kiểm tra NodeSessions bị lỗi | 1 giờ |
| 10 | Quest không reset 00:00 | Job đêm chết | 1. Kiểm tra job schedule (hosted service) 2. Chạy lại job thủ công 3. Xem log job | 30 phút |

## 7.3 Kế hoạch rollback

> Nội dung đã chuyển sang **mục 8 — KẾ HOẠCH ROLLBACK** (cấu trúc chuẩn §17.3.6 mục 8). Giữ tham chiếu tại đây để không hỏng liên kết nội bộ.

---

# 8. KẾ HOẠCH ROLLBACK (nguồn PRODUCTION_PROMPT §15.7)

1. **Rollback code**: giữ 2 bản deploy gần nhất; `systemctl stop` → restore bản cũ → `start` (thời gian mục tiêu ≤ 15 phút).
2. **Rollback DB**: chỉ dùng khi migration gây lỗi — restore backup TRƯỚC migration; KHÔNG chạy migration ngược tự động nếu chưa kiểm thử.
3. **Rollback dữ liệu**: bài học/bài tập bị xóa nhầm → khôi phục từ backup; dữ liệu mới sau mốc restore sẽ bị mất (cảnh báo rõ trước khi thực hiện).
4. Mọi rollback phải có nhật ký: ai, khi nào, lý do, kết quả — ghi vào log vận hành (Serilog + biên bản thủ công).

---

# 9. QUY TRÌNH CHẠY KIỂM THỬ (tham khảo — điều chỉnh theo thực tế)

| Bước | Lệnh |
|---|---|
| Unit test frontend | `cd frontend && npm run test:unit` |
| Lint frontend | `cd frontend && npm run lint` |
| Build frontend | `cd frontend && npm run build` |
| Test backend | `dotnet test backend/DsaVisual.sln` |
| Integration (cần Docker) | `dotnet test --filter "Category=Integration"` |
| E2E | `cd frontend && npm run test:e2e` (cần backend dev + build) |
| Load test | `k6 run frontend/tests/load/login.js` |

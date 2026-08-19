# PHẦN 7: ĐÓNG GÓI & TRIỂN KHAI

## 7.1 Đóng gói frontend/backend

Hệ thống được đóng gói thành hai phần độc lập: frontend Vue 3 build ra thư mục tĩnh `dist/`, backend ASP.NET Core publish ra bộ file chạy được. Lệnh build frontend:

```bash
cd frontend
npm ci
npm run build          # output dist/
```

`npm ci` cài đúng phiên bản dependency theo lockfile (dùng cho môi trường tự động), `npm run build` gọi Vite biên dịch ra thư mục `dist/` phục vụ tĩnh được qua nginx. Lệnh publish backend:

```bash
cd backend
dotnet publish src/DsaVisual.Api -c Release -o /opt/dsavisual/api
```

`dotnet publish` biên dịch project API theo cấu hình Release và gom toàn bộ file cần thiết vào thư mục đích (kèm runtime, không cần cài .NET SDK trên máy chạy).

Đối với môi trường phát triển chuẩn hóa, nhóm cung cấp `docker-compose.dev.yml` khởi động SQL Server và MailHog (bắt chước SMTP):

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

**Bảng 7.1: Các service trong docker-compose (môi trường dev)**

| Service | Hình ảnh | Công dụng |
|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | Cơ sở dữ liệu SQL Server, mở cổng 1433 |
| `mailhog` | `mailhog/mailhog` | Máy chủ SMTP giả để xem email gửi ra tại `http://localhost:8025` |

Nếu chưa cấu hình SMTP thật, hệ thống ghi log và hiển thị link/mã trong log dev — không chặn đăng ký/đăng nhập.

(nguồn: DEPLOY §3, §4.1-4.2)

## 7.2 Triển khai production

Kiến trúc triển khai production được mô tả bằng sơ đồ dưới: nginx làm điểm vào duy nhất, vừa phục vụ file tĩnh frontend vừa chuyển tiếp request API sang Kestrel; API kết nối SQL Server và SMTP (tùy chọn):

```mermaid
graph LR
    User((Người dùng)) --> LB[Nginx/Reverse Proxy<br/>443 TLS + static files]
    LB --> FE[Frontend static<br/>dist/]
    LB --> API[ASP.NET Core API<br/>Kestrel :5000]
    API --> DB[(SQL Server)]
    API --> SMTP[SMTP server (tùy chọn)]
```

Nginx đảm nhận 2 vai trò: phục vụ file tĩnh frontend (`dist/`) và reverse proxy sang Kestrel :5000. Mọi request vào production đều qua TLS 1.2+ (HSTS). Cấu hình nginx:

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

API chạy như service hệ thống bằng systemd (Linux), tự khởi động lại khi lỗi:

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

Nếu triển khai trên Windows, nhóm dùng IIS với ASP.NET Core Hosting Bundle (site trỏ `DsaVisual.Api.exe`, in-process) hoặc chạy Kestrel đơn giản phía sau proxy. Các biến môi trường quan trọng (giá trị cụ thể không ghi trong báo cáo):

**Bảng 7.2: Biến môi trường quan trọng**

| Biến | Mô tả |
|---|---|
| `DSA__Jwt__Secret` | Khóa ký token JWT — chuỗi ngẫu nhiên từ 32 ký tự trở lên, tuyệt đối không commit |
| `ConnectionStrings__Default` | Chuỗi kết nối SQL Server |
| `DSA__Jwt__AccessTokenMinutes` | Thời hạn access token (mặc định 60 phút) |
| `DSA__Jwt__RefreshTokenDays` | Thời hạn refresh token (mặc định 7 ngày) |
| `DSA__Cors__AllowedOrigins` | Danh sách origin được phép gọi API |
| `DSA__Email__SmtpHost` / `SmtpPort` / `From` | Cấu hình SMTP; để trống = chế độ log/MailHog |
| `DSA__Storage__Path` | Thư mục lưu file upload |
| `DSA__Simulation__MaxArraySize` | Giới hạn mảng mô phỏng (100 phần tử) |
| `DSA__Simulation__MaxGraphVertices` | Giới hạn số đỉnh đồ thị (50) |
| `DSA__Auth__MaxLoginAttempts` / `LockoutMinutes` | Khóa đăng nhập sau 5 lần sai 15 phút |
| `VITE_API_BASE_URL` | Base URL API cho frontend (khai báo trong `.env.production`) |

Quy ước chung: biến backend dùng tiền tố `DSA__` (ánh xạ `appsettings.json` theo chuẩn ASP.NET Core); secret không bao giờ nằm trong `appsettings.Production.json` đã commit; frontend không chứa secret nào.

(nguồn: DEPLOY §1.3, §2, §4.3-4.5)

## 7.3 CI/CD + backup

Pipeline CI/CD dùng GitHub Actions với 3 job chạy song song mỗi lần push hoặc tạo pull request:

**Bảng 7.3: Các job chính của GitHub Actions (ci.yml)**

| Job | Nội dung chính |
|---|---|
| `frontend` | Cài Node 20 → `npm ci` → lint (ESLint) → typecheck (`vue-tsc --noEmit`) → unit test (Vitest) → build → quét bảo mật dependency (`npm audit`) |
| `backend` | Cài .NET 8 → build solution → chạy test với SQL Server chạy trong service container → quét lỗ hổng package (`dotnet list package --vulnerable`) |
| `catalog-sync` | Chạy `scripts/check-catalog-sync.js` so sánh `shared/simulation-catalog.json` với danh sách key phía frontend — khác là fail build |

Sau khi CI pass, staging được deploy tự động qua script `scripts/deploy-staging.sh`. Deploy production làm thủ công qua tag `release/*` (workflow `deploy-prod.yml`, SSH + script). Trước khi deploy: backup DB trước, chạy migration DB trước khi deploy code.

Chính sách backup cơ sở dữ liệu:

```sql
-- Backup full hàng ngày 02:00 (giữ 14 bản — script job lịch)
BACKUP DATABASE DsaVisual TO DISK = 'D:\backups\DsaVisual_20260812.bak' WITH COMPRESSION;

-- Restore (test restore 1 lần/tháng, ghi biên bản)
RESTORE DATABASE DsaVisual FROM DISK = 'D:\backups\DsaVisual_20260812.bak'
  WITH REPLACE, RECOVERY;
```

**Bảng 7.4: Chính sách backup**

| Mục | Chính sách |
|---|---|
| Backup full | Hàng ngày lúc 02:00, giữ 14 bản |
| Backup log | Mỗi 4 giờ |
| Test restore | 1 lần/tháng, ghi biên bản |
| Lưu trữ | Ổ khác máy chủ (network share / object storage) |

(nguồn: DEPLOY §5.3, §6)

## 7.4 Runbook sự cố

Bảng dưới tóm tắt 8 sự cố thường gặp nhất khi vận hành (rút gọn từ danh sách 10 sự cố của DEPLOY):

**Bảng 7.5: Runbook sự cố thường gặp**

| Sự cố | Nguyên nhân | Xử lý | Thời gian mục tiêu |
|---|---|---|---|
| API trả 503 liên tục | SQL Server ngừng | Kiểm tra service SQL, khởi động lại, xem log lỗi SQL, restore backup nếu lỗi dữ liệu | 30 phút |
| 500 liên tục ở API | Lỗi code / config sai | Xem log file gần nhất, kiểm tra biến môi trường, rollback phiên bản trước | 1 giờ |
| Đăng nhập chậm | Quá nhiều request hash mật khẩu | Kiểm tra rate limit hoạt động, tăng tài nguyên, kiểm tra lockout DB | 1 giờ |
| Upload ảnh lỗi | Hết dung lượng ổ | Kiểm tra `df -h`, dọn upload tạm (job đêm), mở rộng ổ | 30 phút |
| Mô phỏng chậm phía client | Dữ liệu quá giới hạn / máy yếu | Xác nhận giới hạn hệ thống, gợi ý giảm kích thước dữ liệu, kiểm tra phiên bản trình duyệt | 2 giờ |
| Token lỗi hàng loạt | Secret JWT bị thay đổi | Kiểm tra `DSA__Jwt__Secret`, khôi phục giá trị cũ, người dùng đăng nhập lại | 30 phút |
| Email không gửi | SMTP lỗi | Kiểm tra queue email trong DB, kiểm tra kết nối SMTP, bật lại service | 1 giờ |
| Backup thất bại | Hết dung lượng / quyền | Xem log job backup, giải phóng dung lượng, chạy lại thủ công | 1 giờ |

Kèm theo đó, nhóm duy trì kế hoạch rollback: giữ 2 bản deploy gần nhất (rollback code trong 15 phút); rollback DB chỉ khi migration gây lỗi, restore backup trước mốc migration; mọi rollback phải ghi nhật ký (ai, khi nào, lý do, kết quả).

(nguồn: DEPLOY §7.2, §8)

# KẾT LUẬN & HƯỚNG PHÁT TRIỂN

## Kết quả đạt được

Đối chiếu 8 KPI mục tiêu (SRS §2.2) với kết quả thực tế:

**Bảng 7.6: Đánh giá KPI G1-G8**

| KPI | Mô tả (giá trị mục tiêu) | Đánh giá |
|---|---|---|
| G1 | Phủ nội dung học tập — số CTDL có mô phỏng (≥ 10) | Đạt — catalog có đủ 10 CTDL |
| G2 | Phủ giải thuật — số GT có mô phỏng (≥ 14, thiết kế 15) | Đạt — catalog có 34 thao tác giải thuật |
| G3 | Mức độ sử dụng — tỷ lệ sinh viên truy cập ≥ 1 lần/tuần (≥ 80%) | Chờ hoàn tất kiểm thử (tuần 19-20) — cần thí điểm lớp thật |
| G4 | Hiệu quả học tập — điểm kiểm tra chương (≥ 7.0/10) | Chờ hoàn tất kiểm thử (tuần 19-20) — đo ngoài hệ thống, do giảng viên chấm |
| G5 | Sự hài lòng — khảo sát UX thang 5 (≥ 4.0/5) | Chờ hoàn tất kiểm thử (tuần 19-20) — khảo sát SUS trong TEST_PLAN |
| G6 | Độ ổn định — uptime thí điểm 4 tuần (≥ 99.5%) | Chờ hoàn tất kiểm thử (tuần 19-20) — đo khi chạy staging/production |
| G7 | Hiệu năng — phản hồi API p95 (≤ 800ms) | Chờ hoàn tất kiểm thử (tuần 19-20) — k6 load test trong TEST_PLAN |
| G8 | Độ mượt — FPS khi mô phỏng (≥ 55) | Chờ hoàn tất kiểm thử (tuần 19-20) — TEST-PERF trong TEST_PLAN |

Về chức năng, hệ thống đã bao phủ các luồng chính: học theo lộ trình 5 path với mô phỏng từng bước, luyện tập 3 bậc (trắc nghiệm, thực hành kéo thả, lập trình với test ẩn), gamification (tim, gems, quest, streak, leaderboard), lớp học và báo cáo cho giảng viên, quản trị người dùng cho admin. Về kỹ thuật, nhóm triển khai được engine EDV (mã thật chạy qua StepExecutor, hoạt ảnh phát lại trace — không hardcode), sandbox code chạy trong Web Worker phía client (không dùng máy chủ container), xác thực JWT có cơ chế rotate-invalidate. Về giao diện, màn mô phỏng đồng bộ 3 vùng (mã giả, canvas, giải thích) với bảng màu legend, có chế độ tối và bố cục đáp ứng từ 1024px. Các con số hiệu năng, bảo mật và mức độ hài lòng chưa được khẳng định cho tới khi hoàn tất kiểm thử — nhóm không đưa ra số liệu chưa đo.

## Khó khăn & Bài học kinh nghiệm

**Khó khăn gặp phải:**

1. **Đồng bộ 3 vùng màn mô phỏng**: mỗi bước phải thống nhất giữa dòng mã giả tô sáng, hình vẽ canvas và lời giải thích. Lệch một bước là hoạt ảnh sai ngay; phải đưa toàn bộ thông tin vào TraceEvent ngay từ khi thiết kế engine.
2. **Khối lượng công việc lớn trong thời gian 13 tuần**: vừa code vừa viết 12 file tài liệu, buộc nhóm ưu tiên task mức Cao và dùng template chung để giảm tải (rủi ro R3 trong SDD).
3. **Phạm vi dự án trôi dạt**: nhiều tính năng hấp dẫn nhưng ngoài tầm (online judge, AI, thanh toán thật). Nhóm phải chốt các quyết định cắt giảm G-1..G-8: seed giảm từ 18 về 8 bài chất lượng cao, AI chỉ dừng ở PoC.
4. **Xung đột Git khi làm chung**: nhiều thành viên sửa cùng khu vực frontend/backend, gây merge conflict; nhóm khắc phục bằng cách tách nhánh theo module và rà soát trước khi merge.
5. **Môi trường dev thiếu SQL Server và SMTP thật**: nhóm dùng SQLite/LocalDB và MailHog ở dev, nhưng test tích hợp luôn chạy trên SQL Server thật để tránh lệch hành vi.

**Bài học kinh nghiệm:**

1. **Golden data và test từng bước cho mọi generator**: mỗi thuật toán phải có bộ dữ liệu chuẩn với kết quả mong đợi tính trước, giúp bắt sai logic từ sớm thay vì phát hiện khi demo.
2. **Cắt phạm vi sớm và dứt khoát**: 8 bài học chất lượng cao hoàn chỉnh tốt hơn 18 bài dở dang; quyết định cắt phải ghi lại lý do trong tài liệu để không tái tranh luận.
3. **Tài liệu đi song song với code**: cập nhật tài liệu theo từng sprint, không dồn cuối kỳ; mọi nội dung báo cáo truy ngược được về SRS/SDD/API.
4. **Đầu tư đúng chỗ vào engine lõi**: kiến trúc EDV (mã thật chạy, phát lại trace) giúp thêm mô phỏng mới không phải viết lại hoạt ảnh — chi phí ban đầu cao nhưng tiết kiệm về sau.
5. **Dự trù thời gian cho sprint rủi ro cao**: chấm điểm code (S7) và nhóm Premium + Class (S9) là hai sprint nặng nhất; cần buffer hoặc sẵn sàng hoãn tính năng không thiết yếu.

## Hướng phát triển

Backlog mở rộng đã ghi trong SDD cho các giai đoạn sau:

1. **Online judge chấm mã** — nâng cấp từ FR-9.3, chấm code do người học viết tự do thay vì hoàn thiện hàm theo khuôn.
2. **Mô phỏng thêm**: cây đỏ-đen, B/B+, trie, Prim/Kruskal, Floyd-Warshall, Topological sort, KMP.
3. **AI Assistant (PoC GĐ3)** — 1 endpoint `/ai/ask`, 3 chế độ: giải thích bước mở rộng, giải thích lỗi code, hỏi lý thuyết (RAG mini); tốn Hint token/Gems, có fallback offline, không chấm điểm.
4. **Di động responsive đầy đủ**; đa ngôn ngữ (i18n EN); import/export bài học JSON.
5. **10 bài seed còn lại** (Selection, Insertion, Merge, Quick, Heap Sort, Linear Search, Queue, BST Xóa & Duyệt, DFS, Dijkstra) kèm test ẩn.
6. **Tích hợp thanh toán thật** (SePay/VietQR) cho gói Premium — hiện chỉ thanh toán mô phỏng.

(nguồn: SDD §11.2)

# TÀI LIỆU THAM KHẢO

1. Vue.js — Tài liệu Vue 3. https://vuejs.org
2. Pinia — The intuitive Vue.js store. https://pinia.vuejs.org
3. Vite — Next Generation Frontend Tooling. https://vitejs.dev
4. Microsoft — Tài liệu ASP.NET Core. https://learn.microsoft.com/aspnet/core
5. Microsoft — Tài liệu Entity Framework Core. https://learn.microsoft.com/ef/core
6. Microsoft — Tài liệu SQL Server. https://learn.microsoft.com/sql
7. Thomas H. Cormen và cộng sự — Introduction to Algorithms (CLRS), MIT Press.
8. VisuAlgo — trực quan hóa giải thuật. https://visualgo.net
9. David Galles — Data Structure Visualizations, University of San Francisco. https://www.cs.usfca.edu/~galles/visualization
10. Algorithm Visualizer. https://algorithm-visualizer.org
11. Mermaid — Diagramming and charting tool. https://mermaid.js.org

# PHỤ LỤC A: Hướng dẫn cài đặt môi trường

**Yêu cầu phần mềm tối thiểu:**

**Bảng A.1: Yêu cầu phần mềm**

| Thành phần | Phiên bản | Mục đích |
|---|---|---|
| .NET SDK | 8.0+ | Build backend |
| ASP.NET Core Runtime | 8.0+ | Chạy API |
| Node.js | 20+ | Build frontend |
| SQL Server | 2019+ | Production; dev có thể dùng SQLite/LocalDB |
| Nginx | 1.24+ | Reverse proxy + static files (Linux) |
| Docker | 24+ | Tùy chọn — dev chuẩn hóa (SQL Server + MailHog) |

**Bước 1 — Cài frontend:** chạy `npm install` để cài dependency, sau đó `npm run dev` khởi động Vite dev server tại cổng 5173 (proxy `/api` sang localhost:5000). Xem kết quả tại `http://localhost:5173`.

```bash
cd frontend
npm install
npm run dev        # Vite dev server :5173, proxy /api → localhost:5000
```

**Bước 2 — Cài backend:** khôi phục và build solution bằng `dotnet restore` + `dotnet build`, chạy migration tạo CSDL, rồi khởi động API kèm seed dữ liệu mẫu. Kiểm tra tại Swagger `http://localhost:5000/swagger`.

```powershell
cd backend
dotnet restore
dotnet build

# Migration + seed
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet run --project src/DsaVisual.Api --seed    # seed idempotent (chạy lại không nhân đôi)
```

**Bước 3 — Cấu hình cơ sở dữ liệu:** dev dùng SQL Server local hoặc SQLite qua biến `ConnectionStrings__Default=Data Source=dsavisual-dev.db` (đặt trong `appsettings.Development.json` hoặc biến môi trường). Muốn môi trường chuẩn, khởi động SQL Server + MailHog bằng `docker compose -f docker-compose.dev.yml up -d`.

**Bước 4 — Chạy production (Linux):** build frontend bằng `npm ci` + `npm run build`, publish backend bằng `dotnet publish src/DsaVisual.Api -c Release -o /opt/dsavisual/api`, cấu hình nginx và systemd theo PHẦN 7.2.

(nguồn: DEPLOY §1.2, §2-3, §4.1-4.2)

# PHỤ LỤC B: Phím tắt + thuật ngữ

**Bảng B.1: Phím tắt trang mô phỏng**

| Phím | Chức năng |
|---|---|
| `Space` | Phát / Tạm dừng mô phỏng |
| `→` / `←` | Bước tiếp theo / bước trước đó |
| `Home` / `End` | Về bước đầu tiên / nhảy tới bước cuối |
| `[` / `]` | Giảm / tăng tốc độ chạy |
| `C` | Mở hộp thoại cấu hình dữ liệu |
| `F` | Lưu vào mục yêu thích |

(nguồn: USER_GUIDE §7.1)

**Bảng B.2: Thuật ngữ viết tắt**

| Thuật ngữ | Giải thích |
|---|---|
| DSA | Cấu trúc dữ liệu và giải thuật — lĩnh vực của hệ thống |
| CTDL / GT | Cấu trúc dữ liệu / Giải thuật (thuật toán) |
| EDV (Execution-Driven Visualization) | Kiến trúc lõi: mã thật chạy qua StepExecutor, hoạt ảnh = phát lại trace thực thi |
| StepExecutor | Bộ thực thi gắn thiết bị đo, chạy code mẫu và ghi TraceEvent[] |
| TraceEvent | Bản ghi một câu lệnh quan trọng khi thực thi: dòng code, snapshot biến, phần tử highlight, giải thích |
| SPA | Single Page Application — ứng dụng web tải một lần, chuyển nội dung không tải lại trang |
| JWT | JSON Web Token — chuỗi mã hóa xác thực; access token 60 phút, refresh token 7 ngày trong cookie an toàn |
| EF Core | Thư viện C# (ORM) truy vấn và lưu dữ liệu vào cơ sở dữ liệu |
| Migration | Cơ chế EF Core thay đổi cấu trúc bảng theo phiên bản |
| Sandbox | Môi trường chạy code cách ly phía client (Web Worker), giới hạn 10 giây / 64MB / 200 dòng |
| Big-O | Ký hiệu mô tả độ phức tạp thời gian/không gian của giải thuật |
| BFS / DFS | Duyệt đồ thị theo chiều rộng (hàng đợi) / theo chiều sâu (ngăn xếp) |
| Practice Ladder | Chuỗi luyện tập 3 bậc: Quiz → Interactive Lab → Code Challenge |
| NodeSession | Bản ghi phiên học 30 phút của một người học tại một node |
| KPI | Chỉ số đo lường mục tiêu dự án (G1-G8) |
| UC / FR / NFR | Use case / Yêu cầu chức năng / Yêu cầu phi chức năng |
| SUS | Bảng khảo sát đánh giá mức độ dùng được của giao diện |

(nguồn: GLOSSARY; SRS §2.2, §6; TEST_PLAN §2)

# PHỤ LỤC C: Thư viện bên thứ ba (license)

Chưa cập nhật đầy đủ (12/08/2026) — bảng dưới liệt kê thư viện chính theo nguồn SDD/DEPLOY, giấy phép sẽ bổ sung khi có THIRD_PARTY.md.

**Bảng C.1: Thư viện bên thứ ba chính**

| Thư viện | Công dụng | Ghi chú |
|---|---|---|
| Vue 3 | Framework frontend | Dùng toàn bộ giao diện (SDD §3) |
| Pinia | Quản lý trạng thái frontend | Store auth/lesson/simulation/progress/gamification (SDD §3.2) |
| Vite | Build tool frontend | Dev server + build production (SDD §3.9) |
| Axios | HTTP client | Gọi API, interceptor token (SDD §3.4) |
| Monaco Editor | Trình soạn mã | Code Runner Màn 16 (SDD §8) |
| Mermaid | Vẽ sơ đồ trong tài liệu | Sơ đồ kiến trúc, state machine (DEPLOY §1.3) |
| ASP.NET Core | Backend framework | API 2 lớp Controller → Service → DbContext (SDD §5) |
| Entity Framework Core | ORM | Code-First + Migrations (SDD §7) |
| Serilog | Ghi log có cấu trúc | Rolling file 90 ngày (DEPLOY §7.1) |
| xUnit | Unit test backend | DsaVisual.UnitTests (SDD §5) |
| Vitest | Unit test frontend | Test store và component (SDD §3.7) |
| Testcontainers | Chạy container trong test | Integration test trên SQL Server thật (DEPLOY §5.4) |
| k6 | Load test | Kịch bản login (DEPLOY §9) |

# PHỤ LỤC D: Danh mục mô phỏng (catalog)

Danh mục mô phỏng được đồng bộ từ file `shared/simulation-catalog.json` — 44 mô phỏng chia 2 nhóm: 34 thao tác giải thuật (algorithm) và 10 cấu trúc dữ liệu (structure). Trong đó 3 mô phỏng được mở xem công khai tại trang chủ không cần đăng nhập: Bubble Sort, Binary Search, BFS.

**Bảng D.1: Danh mục 44 mô phỏng**

| Nhóm | Tên mô phỏng | Mô tả |
|---|---|---|
| Sắp xếp | Sắp xếp nổi bọt (Bubble Sort) | So sánh và hoán đổi cặp phần tử liền kề, đưa phần tử lớn về cuối |
| Sắp xếp | Sắp xếp chọn (Selection Sort) | Chọn phần tử nhỏ nhất còn lại đưa về đầu mảng |
| Sắp xếp | Sắp xếp chèn (Insertion Sort) | Chèn từng phần tử vào đúng vị trí trong đoạn đã sắp xếp |
| Sắp xếp | Sắp xếp trộn (Merge Sort) | Chia mảng làm đôi rồi trộn các nửa đã sắp xếp (chia để trị) |
| Sắp xếp | Sắp xếp nhanh (Quick Sort — Lomuto) | Chọn chốt (pivot) và phân chia mảng quanh chốt |
| Sắp xếp | Sắp xếp vun đống (Heap Sort) | Vun mảng thành đống rồi trích phần tử lớn nhất về cuối |
| Tìm kiếm | Tìm kiếm tuyến tính (Linear Search) | Duyệt từng phần tử cho tới khi gặp giá trị cần tìm |
| Tìm kiếm | Tìm kiếm nhị phân (Binary Search) | Chia đôi đoạn tìm kiếm trên mảng đã sắp xếp |
| Ngăn xếp | Ngăn xếp — Push | Đẩy phần tử vào đỉnh ngăn xếp (LIFO) |
| Ngăn xếp | Ngăn xếp — Pop | Lấy phần tử trên đỉnh ngăn xếp ra |
| Ngăn xếp | Ngăn xếp — Peek | Xem phần tử trên đỉnh ngăn xếp mà không lấy ra |
| Hàng đợi | Hàng đợi — Enqueue | Thêm phần tử vào cuối hàng đợi (FIFO) |
| Hàng đợi | Hàng đợi — Dequeue | Lấy phần tử đầu hàng đợi ra |
| Danh sách liên kết | Danh sách liên kết — Chèn | Chèn nút vào đầu, cuối hoặc vị trí k |
| Danh sách liên kết | Danh sách liên kết — Xóa | Xóa nút theo vị trí hoặc giá trị |
| Danh sách liên kết | Danh sách liên kết — Tìm kiếm | Duyệt tìm nút chứa giá trị cần tìm |
| Danh sách liên kết | Danh sách liên kết — Duyệt | Đi qua toàn bộ nút theo thứ tự liên kết |
| Cây | BST — Chèn | Chèn khóa vào đúng vị trí trên cây nhị phân tìm kiếm |
| Cây | BST — Xóa | Xóa khóa và nối lại cây cho đúng tính chất BST |
| Cây | BST — Tìm kiếm | Dò tìm khóa theo quan hệ lớn hơn/nhỏ hơn của BST |
| Cây | BST — Duyệt Preorder | Duyệt theo thứ tự gốc – trái – phải |
| Cây | BST — Duyệt Inorder | Duyệt theo thứ tự trái – gốc – phải |
| Cây | BST — Duyệt Postorder | Duyệt theo thứ tự trái – phải – gốc |
| Cây | BST — Duyệt Level-order | Duyệt theo từng tầng từ trên xuống (BFS) |
| Cây | Cây AVL — Chèn kèm xoay (LL/RR/LR/RL) | Chèn và tự xoay để cây luôn cân bằng |
| Đống | Đống nhị phân — Chèn (bubble up) | Thêm phần tử và đẩy lên đúng vị trí |
| Đống | Đống nhị phân — Trích xuất max (sift down) | Lấy phần tử lớn nhất và sắp lại cho đúng tính chất đống |
| Đống | Đống nhị phân — Heapify | Vun mảng thành đống nhị phân |
| Bảng băm | Bảng băm — Chèn (chuỗi nối kết) | Tính hàm băm rồi chèn vào bucket tương ứng |
| Bảng băm | Bảng băm — Tìm kiếm | Tính hàm băm rồi tìm trong bucket tương ứng |
| Bảng băm | Bảng băm — Xóa | Xóa khóa khỏi bucket tương ứng |
| Đồ thị | Đồ thị — Duyệt BFS | Duyệt theo chiều rộng dùng hàng đợi |
| Đồ thị | Đồ thị — Duyệt DFS | Duyệt theo chiều sâu dùng ngăn xếp |
| Đồ thị | Đồ thị — Dijkstra (đường đi ngắn nhất) | Tìm đường đi ngắn nhất từ đỉnh nguồn |
| CTDL | Mảng (Array) | Cấu trúc lưu trữ tuần tự, truy cập theo chỉ số |
| CTDL | Danh sách liên kết đơn (Singly Linked List) | Các nút nối nhau bằng con trỏ |
| CTDL | Ngăn xếp (Stack — LIFO) | Cấu trúc vào sau ra trước |
| CTDL | Hàng đợi (Queue — FIFO) | Cấu trúc vào trước ra trước |
| CTDL | Cây nhị phân (Binary Tree) | Cây mỗi nút tối đa hai con |
| CTDL | Cây nhị phân tìm kiếm (BST) | Cây thỏa quan hệ khóa trái/phải |
| CTDL | Cây AVL (cân bằng) | Cây tự cân bằng theo độ chênh lệch chiều cao |
| CTDL | Đống nhị phân (Binary Heap — max-heap) | Cây đầy đủ thỏa tính chất đống |
| CTDL | Bảng băm (Hash Table — chuỗi nối kết) | Bảng ánh xạ khóa qua hàm băm |
| CTDL | Đồ thị (Graph — có hướng/vô hướng, trọng số) | Cấu trúc đỉnh và cạnh, có thể có trọng số |

(nguồn: shared/simulation-catalog.json)

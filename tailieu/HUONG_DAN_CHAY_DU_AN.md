# 🚀 HƯỚNG DẪN CÀI ĐẶT VÀ KHỞI CHẠY DỰ ÁN — VisualizationDSA

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**  
**Đồ án Tốt nghiệp ngành Công nghệ Thông tin — FPT Polytechnic**

---

## 1. Yêu cầu môi trường (Prerequisites)
- **Hệ điều hành:** Windows 10/11, macOS, hoặc Ubuntu Linux.
- **.NET SDK:** .NET 10.0 (hoặc 9.0/10.0 runtime).
- **Node.js:** Node.js v20.x hoặc v22.x LTS (kèm `npm` hoặc `pnpm`).
- **Docker Desktop:** Khởi chạy container SQL Server 2022.
- **Trình duyệt:** Google Chrome, Microsoft Edge, hoặc Mozilla Firefox (khuyến nghị màn hình 1440×900 trở lên).

---

## 2. Các bước khởi chạy nhanh (Quick Start)

### Bước 1: Khởi động Cơ sở dữ liệu (SQL Server 2022)
Khởi động container Docker đã cấu hình sẵn trong dự án:
```powershell
docker-compose up -d
```
> Hoặc khôi phục tệp `VisualizationDSA_Backup.bak` trong thư mục `04_Database/` theo hướng dẫn tại `04_Database/Huong_Dan_Restore_DB.md`.

---

### Bước 2: Khởi chạy Backend Web API (.NET 10)
Mở cửa sổ dòng lệnh Terminal thứ nhất:
```powershell
cd source\VisualizationDSA
dotnet run --project backend\src\WebApi\WebApi.csproj --urls http://127.0.0.1:5055
```
- API Swagger / Base URL: `http://localhost:5055`
- Database kết nối: `Server=localhost,1433;Database=VisualizationDSA;User Id=sa;Password=Dsa!2026Pass;`

---

### Bước 3: Khởi chạy Frontend Web App (Vue 3 + Vite)
Mở cửa sổ dòng lệnh Terminal thứ hai:
```powershell
cd frontend
npm install
npm run dev -- --port 5174 --config vite.real.config.ts
```
- Truy cập ứng dụng tại: `http://localhost:5174`

---

## 3. Danh sách tài khoản thử nghiệm hệ thống (Test Accounts)

Tất cả các tài khoản đều sử dụng chung mật khẩu: **`RealData@2024`**

| Vai trò | Email đăng nhập | Mật khẩu | Mô tả & Chức năng kiểm thử |
|---|---|---|---|
| **Sinh viên Top 1 (Anchor)** | `baolqse1801@fpt.edu.vn` | `RealData@2024` | Tài khoản Lê Quốc Bảo (Lớp SE1801, Level 10, 3800 XP, Streak 21, Premium: True, Đạt 14 huy hiệu, full chức năng học tập) |
| **Sinh viên Top 2** | `nhungtthse1802@fpt.edu.vn` | `RealData@2024` | Trần Thị Hồng Nhung (SE1802, Level 9, 3550 XP, Streak 28) |
| **Giảng viên (Teacher)** | `teacher1@fpt.edu.vn` | `RealData@2024` | Giảng viên chính thức (Quản lý lớp học, duyệt bài, tạo khóa học) |
| **Quản trị viên (Admin)** | `hungnv@fpt.edu.vn` | `RealData@2024` | Admin Nguyễn Văn Hùng (Dashboard thống kê tài chính, duyệt giảng viên, quản lý người dùng, cài đặt hệ thống) |

---

## 4. Kiểm tra các chức năng chính

1. **Khám phá & Mô phỏng thuật toán:** Truy cập `/simulations` và `/simulator/sort.bubble` (điều khiển từng bước, phát lại, đổi tốc độ 60 FPS).
2. **Lộ trình học tập & Bài giảng:** Truy cập `/path` (20 khóa học, 100 bài học Markdown đầy đủ, bài tập trắc nghiệm tự chấm).
3. **Lớp học & Tiến độ:** Truy cập `/classes` (12 lớp học SE1801..SE1812, 120 sinh viên ghi danh).
4. **Bảng xếp hạng & Gamification:** Truy cập `/leaderboard`, `/quests` (nhiệm vụ ngày/tuần/tháng), `/shop` (cửa hàng đổi vật phẩm bằng Gems).
5. **Trang quản trị thống kê:** Đăng nhập bằng `hungnv@fpt.edu.vn` $\rightarrow$ Truy cập `/admin/stats` (Biểu đồ doanh thu 7 ngày, biểu đồ phân bố vai trò người dùng, quản lý tài khoản).

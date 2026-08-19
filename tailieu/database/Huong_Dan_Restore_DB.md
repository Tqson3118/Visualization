# 🗄️ HƯỚNG DẪN RESTORE CƠ SỞ DỮ LIỆU — VisualizationDSA

Tài liệu hướng dẫn khôi phục cơ sở dữ liệu Microsoft SQL Server 2022 cho hệ thống VisualizationDSA.

---

## 1. Thông tin bản sao lưu (Backup Info)
- **Tên cơ sở dữ liệu:** `VisualizationDSA`
- **Hệ quản trị CSDL:** Microsoft SQL Server 2022 (v16.x)
- **Tệp sao lưu:** `VisualizationDSA_Backup.bak` (Dung lượng ~13.7 MB)
- **Quy mô dữ liệu:** 55 bảng, 137 người dùng (120 sinh viên, 8 giảng viên, 8 giảng viên chờ duyệt, 1 quản trị viên), 20 khóa học, 60 modules, 180 module items, 100 bài học, 52 quizzes, 418 câu hỏi trắc nghiệm, 12 lớp học, 30 đơn hàng, 22 huy hiệu, 7 nhiệm vụ, 10 vật phẩm shop.

---

## 2. Cách 1: Khôi phục bằng Docker SQL Server (Khuyên dùng)

### Bước 1: Sao chép tệp backup vào container Docker
```bash
docker cp VisualizationDSA_Backup.bak neww-sqlserver-1:/var/opt/mssql/data/VisualizationDSA_Backup.bak
```

### Bước 2: Thực thi lệnh Restore
```bash
docker exec neww-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Dsa!2026Pass" -C -Q "
RESTORE DATABASE VisualizationDSA 
FROM DISK = '/var/opt/mssql/data/VisualizationDSA_Backup.bak' 
WITH REPLACE;
"
```

---

## 3. Cách 2: Khôi phục bằng SQL Server Management Studio (SSMS)

1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới SQL Server instance.
2. Nhấp chuột phải vào thư mục **Databases** $\rightarrow$ Chọn **Restore Database...**
3. Tại mục **Source**, chọn **Device** $\rightarrow$ Nhấp nút `...` $\rightarrow$ Nhấp **Add** và trỏ đến tệp `VisualizationDSA_Backup.bak`.
4. Tại mục **Destination**, chọn Database là `VisualizationDSA`.
5. Chuyển sang tab **Options**:
   - Tích chọn **Overwrite the existing database (WITH REPLACE)**.
   - Tích chọn **Close existing connections to destination database**.
6. Nhấp **OK** để bắt đầu khôi phục.

---

## 4. Kiểm tra sau khi khôi phục

Chạy truy vấn SQL kiểm tra số lượng bản ghi:
```sql
USE VisualizationDSA;
SELECT 'Users' AS TableName, COUNT(*) AS TotalRows FROM Users
UNION ALL
SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL
SELECT 'Lessons', COUNT(*) FROM Lessons
UNION ALL
SELECT 'QuizQuestions', COUNT(*) FROM QuizQuestions
UNION ALL
SELECT 'Classrooms', COUNT(*) FROM Classrooms;
```
**Kết quả mong đợi:**
- Users: 137
- Courses: 20
- Lessons: 100
- QuizQuestions: 418
- Classrooms: 12

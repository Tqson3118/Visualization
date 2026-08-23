# Seed V2 — Danh sách user (users-v2.md)

Danh sách CHÍNH THỨC 69 user V2 (PROMPT_K_SEED_PROD_V2 — Task 1): 68 student `@university.edu.vn` theo 4 persona (Hardworking 13 / Average 32 / Slacker 13 / New 10) + 1 showcase account.

- Nguồn code: `backend/src/DsaVisual.Application/Persistence/Seed/SeedDemoActivity.V2.Data.cs` (`SeedDemoActivity.V2Users`).
- Ràng buộc: mỗi Email unique; KHÔNG trùng 26 email đã tồn tại trong DB; KHÔNG dùng lại 8 tên student V1 (doanminhduc, huynhthuy, lethikimngan, nguyenminhanh, nguyentrang, phamhoanglong, tranquocbao, vuthanhtung).
- Password chung (dev local): `Student@123` (hash bằng `PasswordHasher` — PBKDF2, pattern V1).
- Index deterministic 0..68: student 0-67 theo thứ tự nhóm; showcase = 68 (CreatedAt = 30 ngày trước).
- CreatedAt student = `now.AddDays(-(Index * 29 / 68)).AddHours(-(Index % 3) * 2).AddMinutes(-(Index * 5) % 60)` (rải 30 ngày, deterministic).

## Bảng email

| STT | Email | FullName | Persona | Index |
|----:|-------|----------|---------|------:|
| 1 | nguyenthanhhai@university.edu.vn | Nguyễn Thanh Hải | Hardworking | 0 |
| 2 | tranthuylinh@university.edu.vn | Trần Thùy Linh | Hardworking | 1 |
| 3 | lequangvinh@university.edu.vn | Lê Quang Vinh | Hardworking | 2 |
| 4 | phamthuha@university.edu.vn | Phạm Thu Hà | Hardworking | 3 |
| 5 | hoangminhtri@university.edu.vn | Hoàng Minh Trí | Hardworking | 4 |
| 6 | phanthanhson@university.edu.vn | Phan Thanh Sơn | Hardworking | 5 |
| 7 | vuongthihuong@university.edu.vn | Vương Thị Hương | Hardworking | 6 |
| 8 | dangquockhoa@university.edu.vn | Đặng Quốc Khoa | Hardworking | 7 |
| 9 | buithanhthao@university.edu.vn | Bùi Thanh Thảo | Hardworking | 8 |
| 10 | duongquanghuy@university.edu.vn | Dương Quang Huy | Hardworking | 9 |
| 11 | lythuylinh@university.edu.vn | Lý Thùy Linh | Hardworking | 10 |
| 12 | ngominhhieu@university.edu.vn | Ngô Minh Hiếu | Hardworking | 11 |
| 13 | dinhcongminh@university.edu.vn | Đinh Công Minh | Hardworking | 12 |
| 14 | nguyenvantuan@university.edu.vn | Nguyễn Văn Tuấn | Average | 13 |
| 15 | tranminhduc@university.edu.vn | Trần Minh Đức | Average | 14 |
| 16 | lethithanhvan@university.edu.vn | Lê Thị Thanh Vân | Average | 15 |
| 17 | phamngocanh@university.edu.vn | Phạm Ngọc Anh | Average | 16 |
| 18 | hoangthilan@university.edu.vn | Hoàng Thị Lan | Average | 17 |
| 19 | phanvanhung@university.edu.vn | Phan Văn Hùng | Average | 18 |
| 20 | vuhoangnam@university.edu.vn | Vũ Hoàng Nam | Average | 19 |
| 21 | trinhthimai@university.edu.vn | Trịnh Thị Mai | Average | 20 |
| 22 | donguyenkhang@university.edu.vn | Đỗ Nguyên Khang | Average | 21 |
| 23 | buihongnhung@university.edu.vn | Bùi Hồng Nhung | Average | 22 |
| 24 | duongthutrang@university.edu.vn | Dương Thu Trang | Average | 23 |
| 25 | lyquocbao@university.edu.vn | Lý Quốc Bảo | Average | 24 |
| 26 | ngothihong@university.edu.vn | Ngô Thị Hồng | Average | 25 |
| 27 | dinhvanphuc@university.edu.vn | Đinh Văn Phúc | Average | 26 |
| 28 | tominhchau@university.edu.vn | Tô Minh Châu | Average | 27 |
| 29 | hathithuhang@university.edu.vn | Hà Thị Thu Hằng | Average | 28 |
| 30 | caoxuandung@university.edu.vn | Cao Xuân Dũng | Average | 29 |
| 31 | luongdinhkhoi@university.edu.vn | Lương Đình Khôi | Average | 30 |
| 32 | doanngoctu@university.edu.vn | Đoàn Ngọc Tú | Average | 31 |
| 33 | truongvandat@university.edu.vn | Trương Văn Đạt | Average | 32 |
| 34 | maithihoa@university.edu.vn | Mai Thị Hoa | Average | 33 |
| 35 | lamquocthang@university.edu.vn | Lâm Quốc Thắng | Average | 34 |
| 36 | hovanlong@university.edu.vn | Hồ Văn Long | Average | 35 |
| 37 | vothikimchi@university.edu.vn | Võ Thị Kim Chi | Average | 36 |
| 38 | phungquanghuy@university.edu.vn | Phùng Quang Huy | Average | 37 |
| 39 | daoduyan@university.edu.vn | Đào Duy An | Average | 38 |
| 40 | tranhoangnam@university.edu.vn | Trần Hoàng Nam | Average | 39 |
| 41 | lethimyduyen@university.edu.vn | Lê Thị Mỹ Duyên | Average | 40 |
| 42 | nguyenduchuy@university.edu.vn | Nguyễn Đức Huy | Average | 41 |
| 43 | phamthingoc@university.edu.vn | Phạm Thị Ngọc | Average | 42 |
| 44 | vuminhkhang@university.edu.vn | Vũ Minh Khang | Average | 43 |
| 45 | nguyenhongquan@university.edu.vn | Nguyễn Hồng Quân | Average | 44 |
| 46 | tranquoctuan@university.edu.vn | Trần Quốc Tuấn | Slacker | 45 |
| 47 | levanhoa@university.edu.vn | Lê Văn Hòa | Slacker | 46 |
| 48 | phamdinhquang@university.edu.vn | Phạm Đình Quang | Slacker | 47 |
| 49 | hoangvantai@university.edu.vn | Hoàng Văn Tài | Slacker | 48 |
| 50 | nguyenxuanphong@university.edu.vn | Nguyễn Xuân Phong | Slacker | 49 |
| 51 | vuvanthang@university.edu.vn | Vũ Văn Thắng | Slacker | 50 |
| 52 | dangminhnhat@university.edu.vn | Đặng Minh Nhật | Slacker | 51 |
| 53 | buiminhquan@university.edu.vn | Bùi Minh Quân | Slacker | 52 |
| 54 | duongvandong@university.edu.vn | Dương Văn Đông | Slacker | 53 |
| 55 | phanducthien@university.edu.vn | Phan Đức Thiện | Slacker | 54 |
| 56 | trinhvanbinh@university.edu.vn | Trịnh Văn Bình | Slacker | 55 |
| 57 | doquangvinh@university.edu.vn | Đỗ Quang Vinh | Slacker | 56 |
| 58 | lyvankhanh@university.edu.vn | Lý Văn Khánh | Slacker | 57 |
| 59 | nguyengiabao@university.edu.vn | Nguyễn Gia Bảo | New | 58 |
| 60 | trankhanhlinh@university.edu.vn | Trần Khánh Linh | New | 59 |
| 61 | leminhkhoa@university.edu.vn | Lê Minh Khoa | New | 60 |
| 62 | phamanhtuan@university.edu.vn | Phạm Anh Tuấn | New | 61 |
| 63 | hoangmaiphuong@university.edu.vn | Hoàng Mai Phương | New | 62 |
| 64 | vungocanh@university.edu.vn | Vũ Ngọc Ánh | New | 63 |
| 65 | dangthaonguyen@university.edu.vn | Đặng Thảo Nguyên | New | 64 |
| 66 | buihuyhoang@university.edu.vn | Bùi Huy Hoàng | New | 65 |
| 67 | nguyencamtu@university.edu.vn | Nguyễn Cẩm Tú | New | 66 |
| 68 | trannhatminh@university.edu.vn | Trần Nhật Minh | New | 67 |
| 69 | showcase@demo.local | Sinh viên tiêu biểu | Showcase | 68 |

## Tóm tắt

- Tổng: 69 user mới (68 student + 1 showcase) — sau seed Users = 26 cũ + 69 mới = 95 (ngưỡng ≥ 85, xem decision log).
- Phân bổ: Hardworking 13 / Average 32 / Slacker 13 / New 10 / Showcase 1.
- Idempotent: guard theo `Users.Email` (lowercase) — chạy lại lần 2 → 0 thêm, 69 bỏ qua.

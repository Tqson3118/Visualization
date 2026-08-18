# KẾT QUẢ KIỂM THỬ — DSA-Visual
**Ngày chạy**: 18/08/2026 10:52
**Tổng**: 133 TC | ✅ PASS: 130 | ❌ FAIL: 3 | ⚠️ SKIP: 0

## Tóm tắt theo nhóm kiểm thử
| Nhóm chức năng | Số lượng TC | Pass | Fail |
|---|---|---|---|
| Đăng nhập | 13 | 10 | 3 |
| Đăng ký | 8 | 8 | 0 |
| Xác thực 2FA | 7 | 7 | 0 |
| Trang chủ | 6 | 6 | 0 |
| Mô phỏng | 14 | 14 | 0 |
| Bài học | 9 | 9 | 0 |
| Bài tập | 8 | 8 | 0 |
| Lộ trình & Tim | 8 | 8 | 0 |
| Practice Ladder | 9 | 9 | 0 |
| Cửa hàng & Kho | 7 | 7 | 0 |
| Hồ sơ cá nhân | 7 | 7 | 0 |
| Giảng viên | 9 | 9 | 0 |
| Quản trị | 7 | 7 | 0 |
| Bảo mật RBAC | 7 | 7 | 0 |
| Code Runner | 5 | 5 | 0 |
| Benchmark Lab | 3 | 3 | 0 |
| Bảng xếp hạng | 3 | 3 | 0 |
| Nhiệm vụ & Chuỗi | 3 | 3 | 0 |

## Chi tiết các Test Case FAIL
- **TC-05** (Màn 02 — Đăng nhập): Đăng nhập tài khoản đã bật xác thực 2FA — *Lỗi: Error: expect(received).toBeTruthy()

Received: false*
- **TC-07** (Màn 02 — Đăng nhập): Để trống trường mật khẩu, bấm Đăng nhập — *Lỗi: Error: expect(received).toBeFalsy()

Received: true*
- **TC-12** (Màn 02 — Đăng nhập): Nhập chuỗi XSS / SQL Injection vào trường input — *Lỗi: Error: expect(page).toHaveURL(expected) failed

Expected pattern: /\/login/
Received string:  "http://localhost:5174/pat*
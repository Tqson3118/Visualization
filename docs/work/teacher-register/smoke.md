# Smoke E2E — Màn Register (Task L) — teacher-register

- **Ngày**: 13/08/2026 · **Nhánh**: feature/teacher-register · **Worktree**: D:\FPT\neww-teacher
- **Môi trường**: Backend LOCAL :5001 (DsaVisual.Api, env Development, DB docker chung localhost:1433 — migration đã apply, 3 cột GV verified khi chạy) · Frontend Vite :5180 (proxy tạm → :5001, đã revert sau test)
- **Công cụ**: Playwright (script `smoke.mjs` — chạy UI THẬT, không mock) + DOM probe + backend log
- **Dữ liệu test**: email `gv.smoke.20260813.run5@university.edu.vn` · Mã GV `GV12345` · Khoa `Khoa CNTT` · Bio `5 nam giang day CTDL & GT` · MK `Abc@123456` · Admin `admin@system.local / Admin@123`

## Kết quả từng bước

| # | Bước | Kết quả | Chi tiết |
|---|------|---------|----------|
| 1 | Mở /register (mặc định Sinh viên) | **PASS** | Form gốc hiện đủ (Họ tên/Email/MK/checklist/Nhập lại MK/segmented/checkbox/nút Đăng ký); form GV ẩn. Không overflow ngang, ảnh không hỏng. |
| 2 | Chọn "Giảng viên" | **PASS** | Form con hiện đủ 3 field: Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy (textarea + đếm ký tự) + ghi chú "Thông tin sẽ được Admin xét duyệt". |
| 3 | Điền đủ + submit GV | **PASS** | Thông báo "Đăng ký thành công! Tài khoản giảng viên đang chờ duyệt…" hiện, KHÔNG chuyển trang login (vẫn /register). API /auth/register thật thành công → user TEACHER_PENDING id 2017. |
| 4 | Case thiếu Khoa/Bộ môn + Mã GV | **PASS (có ghi chú UX)** | Không submit khi thiếu. Lỗi inline hiện sau blur field: "Vui lòng nhập Khoa/Bộ môn", "Vui lòng nhập Mã giảng viên". **GAP-UX**: submit ngay (chưa blur) → KHÔNG hiện lỗi nào (lỗi chỉ hiện khi field touched — RegisterView.vue gating `touched.x ? fieldErrors.x : ''`). |
| 5 | Admin duyệt | **PASS** | Login admin → /admin/users → tab "Chờ duyệt Teacher" → tìm thấy user smoke → modal duyệt hiển thị ĐỦ: Khoa/Bộ môn = "Khoa CNTT", Mã giảng viên = "GV12345", Kinh nghiệm giảng dạy = "5 nam giang day CTDL & GT" → Duyệt → toast "Đã duyệt giảng viên!" (API /users/2017/approve-teacher 204). |
| 6 | Login bằng GV vừa duyệt | **PASS** | Logout admin → login GV thành công (redirect /path); role = **TEACHER** (response /auth/login, user id 2017); header hiện tên "GV Smoke Test" + link Quản trị (đúng quyền). |

## Kiểm tra bổ sung (DOM / HTTP / console)

- **Overflow ngang**: `body.scrollWidth == clientWidth` trên mọi màn chụp (register student/teacher/pending/admin modal/logged-in) → không overflow.
- **DOM probe cắt chữ/chồng lấn** (register student + teacher): KHÔNG phát hiện phần tử tràn ngang hay cắt chữ (scrollWidth ≤ clientWidth với mọi input/textarea/button/label/p).
- **Console**: không có JS error. Có log "Failed to load resource 401" từ /auth/refresh khi chưa đăng nhập (bootstrap auth — hành vi chung toàn app, không phải lỗi task L).
- **HTTP lỗi thật**: `GET /api/v1/progress/me → 500` (sau login, trang /path gọi) — backend stack: `ProgressService.LoadCountsAsync:223` `ArgumentException: An item with the same key has already been added. Key: 1` (`ToDictionary` trùng key). **Ngoài phạm vi task L** (không liên quan register) nhưng ảnh hưởng trải nghiệm sau login: trang Lộ trình hiện banner lỗi "Đã có lỗi xảy ra, vui lòng thử lại" (vision model cũng ghi nhận banner này ở smoke-06).
- **Email duyệt**: code approve KHÔNG gửi email (UserService.cs:129-151 không gọi email service) → MailHog không có email duyệt. **Mismatch copy**: form GV hứa "bạn sẽ nhận email khi được duyệt" (i18n vi.ts:116-118) nhưng backend không gửi → cần quyết định: gửi email khi duyệt hoặc sửa copy.
- **Dark mode**: không có toggle dark trên UI (theme store tồn tại nhưng chưa có nút bật — ngoài phạm vi) → chỉ chụp light.

## Ảnh đã lưu (docs/work/teacher-register/)

| Ảnh | Nội dung |
|-----|----------|
| smoke-01-register-student.png | /register chế độ Sinh viên (light) |
| smoke-02-register-teacher.png | /register chế độ Giảng viên — form con 3 field |
| smoke-03-register-pending.png | Thông báo chờ duyệt sau submit |
| smoke-04-register-missing-department.png | Lỗi validation thiếu Khoa/Bộ môn + Mã GV |
| smoke-05-admin-review-modal.png | Modal admin duyệt GV (Department/StaffCode/TeacherBio) |
| smoke-06-teacher-logged-in.png | Sau login GV (header + trang Lộ trình) |

Kèm: `smoke-raw.log` (log chi tiết), `smoke.mjs` (script), `dom-probe.mjs`.

## Kết luận

**6/6 bước PASS** — luồng đăng ký giảng viên → chờ duyệt → admin duyệt → login TEACHER hoạt động đúng end-to-end trên backend + DB thật. Không có lỗi chặn luồng. 2 điểm cần pm quyết: (1) GAP-UX lỗi inline chỉ hiện khi touched (RegisterView.vue), (2) copy hứa email duyệt nhưng backend chưa gửi + 500 /progress/me (pre-existing, ngoài scope L).

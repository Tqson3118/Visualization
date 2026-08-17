# 📋 Audit Nghiệp Vụ DSA-Visual — Báo cáo phát hiện 14/08/2026

> Audit dựa trên đọc code thật (backend services + controllers + entities + frontend views). Không đoán.

---

## Tổng quan: Nghiệp vụ LỎNG LẺO thật

Cảm nhận của bạn đúng. Hệ thống hiện tại thiên về **kỹ thuật** (CRUD, auth, gamification) nhưng thiếu **quy trình nghiệp vụ** mà một hệ thống giáo dục cần. Dưới đây là 14 phát hiện cụ thể, xếp theo mức độ nghiêm trọng.

---

## 🔴 MỨC CAO — Ảnh hưởng trực tiếp đến demo bảo vệ

### 1. KHÔNG CÓ QUY TRÌNH DUYỆT NỘI DUNG (Content Approval)
- **Hiện trạng**: GV tạo lesson với `Status = Active` → **sinh viên thấy NGAY**. Không có hàng chờ duyệt, không có Admin review.
- **File**: [`LessonService.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/LessonService.cs#L115-L152) — `CreateAsync` nhận `request.Status` trực tiếp, không kiểm tra.
- **Rủi ro**: GV tạo nội dung sai/18+/spam → SV thấy ngay. Đề thi bảo vệ hỏi "kiểm soát chất lượng nội dung thế nào?" → không trả lời được.
- **Tương tự**: Exercise cũng vậy (`ExerciseStatus.Active` → SV thấy ngay).

### 2. ADMIN DUYỆT GIÁO VIÊN KHÔNG CÓ HỒ SƠ ĐẦY ĐỦ
- **Hiện trạng**: Admin nhận được `ApproveTeacherRequest` nhưng chỉ thấy 3 field: `Department`, `StaffCode`, `TeacherBio` (max 500 ký tự).
- **File**: [`UserService.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/UserService.cs#L148-L182) — `ApproveTeacherAsync`
- **Thiếu**: Không có upload bằng cấp/chứng chỉ, không có link profile, không có verify mã giảng viên, không có lịch sử dạy.
- **Frontend**: [`AdminUsersView.vue`](file:///d:/FPT/neww/frontend/src/views/AdminUsersView.vue) — modal duyệt teacher chỉ hiện Department/StaffCode/TeacherBio.
- **Kết quả**: Admin bấm "Duyệt" dựa trên... 1 đoạn text 500 ký tự? Không có cơ sở xác minh.

### 3. GV SỬA/XÓA TOPIC CỦA GV KHÁC
- **Hiện trạng**: `TopicService.UpdateAsync` và `DeleteAsync` **KHÔNG kiểm tra ownership** (không có `CreatedBy` check).
- **File**: [`TopicService.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/TopicService.cs) — Update/Delete không có `CanManage()`.
- **So sánh**: `LessonService` CÓ check `CanManage(userId, role, lesson)` → Teacher chỉ sửa lesson mình tạo.
- **Rủi ro**: GV A tạo topic "Cấu trúc dữ liệu cơ bản" → GV B xóa được.

### 4. KHÔNG CÓ KHÁI NIỆM "KHÓA HỌC" (Course)
- **Hiện trạng**: Chỉ có Topic → Lesson (2 cấp). Không có Course entity, không có curriculum flow.
- **So sánh với ngoài kia**: Udemy/Coursera có Course → Section → Lesson → Quiz. Moodle có Course → Module → Activity.
- **Ảnh hưởng**: GV không thể "biên soạn 1 khóa học hoàn chỉnh" — chỉ tạo lesson rời rạc gắn vào topic.

### 5. PREMIUM HẾT HẠN KHÔNG CLAMP HEARTSMAX
- **Hiện trạng**: Khi premium hết hạn, `HeartsMax` vẫn giữ 30 (không giảm về 10 mặc định). 
- **File**: `GamificationService.cs:840-844` — đã ghi trong HANDOFF nhưng **chưa fix**.
- **SRS**: FR-10.7 yêu cầu clamp về 10 khi hết premium.

---

## 🟡 MỨC TRUNG BÌNH — Thiếu sót đáng kể nhưng có workaround

### 6. KHÔNG CÓ CONTENT MODERATION / FLAGGING
- **Hiện trạng**: Sinh viên không có cách report nội dung không phù hợp (ngoài Bug Report chung). Admin không có queue kiểm duyệt nội dung mới.
- **Thiếu**: Nút "Báo cáo nội dung", content review queue, auto-flag keywords.

### 7. BUG REPORT: ADMIN KHÔNG THỂ PHẢN HỒI
- **Hiện trạng**: Admin chỉ đổi status (New → Processing → Resolved → Closed). Không có field response/reply.
- **File**: [`FeedbackController.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Api/Controllers/FeedbackController.cs#L195-L224) — chỉ update `Status`, `AssigneeId`, `ResolvedAt`.
- **Entity `BugReport`**: Không có `ResponseText` / `AdminNote`.
- **Kết quả**: SV report bug → Admin đổi status "Đã xử lý" → SV không biết admin nói gì, sửa chưa.

### 8. ADMIN KHÔNG XEM ĐƯỢC CHI TIẾT TIẾN ĐỘ USER
- **Hiện trạng**: `AdminUserDto` trả về info cơ bản (tên, email, role, Department, StaffCode). Không có XP, Level, Streak, Gems, lessons completed, exercises passed.
- **File**: [`UserService.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/UserService.cs#L50-L63) — `GetByIdAsync` → `ToDto(user)` chỉ map basic fields.
- **Rủi ro**: Admin quản lý user nhưng không biết user đó học đến đâu.

### 9. GV CHƯA DUYỆT (TeacherPending) VẪN LOGIN ĐƯỢC
- **Hiện trạng**: Đăng ký teacher → `Role = TeacherPending` → vẫn login, nhận JWT, truy cập app như Student.
- **File**: [`AuthService.cs`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/AuthService.cs) — Register → `IsActive = true` → JWT issued.
- **Không sai logic** nhưng UX lỏng: user đăng ký teacher, login vào, thấy mình là Student, bấm "Quản trị" → redirect profile. Không hiểu chuyện gì xảy ra.

### 10. ACHIEVEMENT HARDCODE TRONG FRONTEND
- **Hiện trạng**: `ProfileView.vue` dòng 120-127 hardcode `first-sim: true`, `pass-node: false`, v.v. thay vì lấy từ backend.
- **Backend**: Có `UserAchievements` table, có `Achievements` table (17 records sau seed V2). Nhưng FE không gọi API.
- **Kết quả**: Thành tích hiện trên profile không phản ánh thực tế.

---

## 🟢 MỨC THẤP — Cải thiện chất lượng, không khẩn cấp

### 11. KHÔNG CÓ PREVIEW TRƯỚC KHI PUBLISH
- **Hiện trạng**: Admin/GV tạo lesson bằng HTML textarea raw. Không có preview "student sẽ thấy thế nào".
- **Frontend**: [`AdminContentView.vue`](file:///d:/FPT/neww/frontend/src/views/AdminContentView.vue) — modal tạo lesson dùng `<textarea>` cho `contentHtml`.

### 12. EMAIL DOMAINS SETTING CÒN TỒN TẠI NHƯNG LOGIC ĐÃ XÓA
- **Hiện trạng**: Prompt K đã bỏ chặn domain đăng ký. Nhưng `allowed.email.domains` setting vẫn tồn tại trong DB và Settings UI.
- **Có thể gây nhầm lẫn**: Admin thay đổi setting → tưởng có hiệu lực → thực tế không.

### 13. LESSON KHÔNG GẮN ĐƯỢC NHIỀU SIMULATION TRỰC TIẾP TỪ UI
- **Hiện trạng**: Backend hỗ trợ `LessonSimulations` (many-to-many). Nhưng AdminContentView chỉ có dropdown chọn 1 simulation.

### 14. CLASS ASSIGNMENT KHÔNG CÓ DEADLINE ENFORCEMENT
- **Hiện trạng**: `ClassAssignment` có deadline nhưng submit sau deadline vẫn được chấp nhận (chỉ đánh dấu "Late" trong report).
- **File**: [`ClassService.cs:418-507`](file:///d:/FPT/neww/backend/src/DsaVisual.Application/Services/ClassService.cs) — report phân loại OnTime/Late/NotSubmitted nhưng không block late submission.

---

## Ma trận tổng hợp

| # | Vấn đề | Mức | Cần BE? | Cần FE? | Ước lượng |
|---|--------|-----|---------|---------|-----------|
| 1 | Content approval workflow | 🔴 | ✅ | ✅ | 3-4h |
| 2 | Teacher profile thiếu hồ sơ | 🔴 | ✅ | ✅ | 2-3h |
| 3 | Topic ownership check | 🔴 | ✅ | — | 30min |
| 4 | Khái niệm khóa học | 🔴 | ⚠️ | ⚠️ | Scope lớn |
| 5 | Premium clamp HeartsMax | 🔴 | ✅ | — | 1h |
| 6 | Content moderation | 🟡 | ✅ | ✅ | 2-3h |
| 7 | Bug report response | 🟡 | ✅ | ✅ | 1-2h |
| 8 | Admin xem tiến độ user | 🟡 | ✅ | ✅ | 2h |
| 9 | TeacherPending UX | 🟡 | — | ✅ | 1h |
| 10 | Achievement hardcode | 🟡 | — | ✅ | 1-2h |
| 11 | Content preview | 🟢 | — | ✅ | 1h |
| 12 | Email domain setting | 🟢 | ✅ | — | 30min |
| 13 | Multi-simulation attach | 🟢 | — | ✅ | 1h |
| 14 | Deadline enforcement | 🟢 | ✅ | — | 1h |

---

## Khuyến nghị: Fix gì trước bảo vệ?

Với **~2 tuần** còn lại, **KHÔNG nên** làm #4 (khái niệm khóa học — scope quá lớn, đổi schema).

**Nên fix (tạo prompt)**:
1. **#1 Content approval** — thêm trạng thái `PendingReview` cho lesson, GV tạo → PendingReview → Admin duyệt → Active
2. **#2 Teacher profile** — thêm field `qualifications`/`certifications` text + hiện đầy đủ trong modal duyệt
3. **#3 Topic ownership** — fix nhanh nhất, thêm `CreatedBy` check
4. **#5 Premium clamp** — đã biết từ backend audit, fix nhanh
5. **#7 Bug report response** — thêm `AdminNote` field
6. **#8 Admin xem tiến độ user** — bổ sung vào `AdminUserDto`
7. **#9 TeacherPending UX** — hiện banner "Đang chờ duyệt" + ẩn nút không liên quan
8. **#10 Achievement dynamic** — gọi API thay hardcode

> [!IMPORTANT]
> Prompt `PROMPT_BUSINESS_LOGIC_AUDIT.md` đã được tạo tại `session/` — sẵn sàng giao cho session khác chạy trên worktree riêng.

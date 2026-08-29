# Đề 12 — Classes & Studio (Teacher Content)

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm  
**Bao phủ:** ClassesView, ClassDetailView (4 tab), ClassReportView, AdminContentView (3 tab), AdminLessonEditorView

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** Trong ClassesView.vue, hàm `onInviteInput` thực hiện validation mã mời như thế nào?

- A. Cho phép chữ thường, hoa, số; tối đa 8 ký tự
- B. Chỉ cho phép A-Z và số 0-9; tối đa 6 ký tự; tự động viết hoa
- C. Cho phép A-Z, 0-9 và dấu gạch ngang; tối đa 6 ký tự
- D. Regex `/^[a-zA-Z0-9]{4,8}$/` — không auto-uppercase

---

**Câu 2.** Nút "Tạo lớp" trong ClassesView hiển thị với điều kiện gì?

- A. `auth.role === 'TEACHER'`
- B. `auth.isAdmin`
- C. `auth.role === 'TEACHER' || auth.role === 'ADMIN'` (computed: `isTeacher`)
- D. Hiển thị với mọi user đã đăng nhập

---

**Câu 3.** ClassDetailView.vue có bao nhiêu tab và tên các tab?

- A. 3 tab: members / assignments / settings
- B. 3 tab: members / curriculum / settings
- C. 4 tab: members / assignments / curriculum / settings
- D. 5 tab: members / assignments / curriculum / report / settings

---

**Câu 4.** API endpoint nào được gọi để xuất báo cáo Excel từ ClassDetailView?

- A. GET /classes/{id}/export
- B. POST /classes/{id}/report
- C. GET /classes/{id}/report/excel (ClassesController)
- D. GET /classes/{id}/report/csv

---

**Câu 5.** AdminContentView.vue (route `/studio`) có 3 tab. Tab thứ 3 "Ý kiến học viên" kết nối với controller nào ở backend?

- A. FeedbackController
- B. AdminUsersController
- C. CourseFeedbackController
- D. ReviewController

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

### Câu TL-1 (2.5 điểm)

**Đề bài:** Trace luồng **Teacher tạo lớp và gán bài tập cho học sinh**:

1. Điều kiện nào để nút "Tạo lớp" hiển thị? (role check)
2. Sau khi tạo lớp, mã mời được tạo thế nào? (format, độ dài, ký tự hợp lệ)
3. Học sinh nhập mã → `joinOpen = true` → modal → API nào được gọi?
4. Trong ClassDetailView, Teacher gán bài tập (Assignment) qua tab nào, chọn loại gì (`assignType`)?
5. `assignType` có thể nhận giá trị gì? API nào được gọi khi submit?

*(Gợi ý: ClassesView.vue + ClassDetailView.vue)*

---

### Câu TL-2 (2.5 điểm)

**Đề bài:** Trace luồng **Teacher soạn và publish bài học** trên AdminContentView / AdminLessonEditorView:

1. Route của AdminContentView là gì? Roles nào được phép truy cập?
2. Tab 1 của AdminContentView gồm những modal builder nào?
3. Khi Teacher vào AdminLessonEditorView (`/studio/lessons/new`), họ có thể làm gì?
4. Bài học được soạn bằng format nào?
5. Sau khi lưu, bài học có thể được gán vào lớp qua flow nào trong ClassDetailView?

*(Gợi ý: AdminContentView tab 1, AdminLessonEditorView, ClassDetailView tab curriculum/assignments)*

# 10. Feedback teacher/admin

## Flow
Student gửi feedback → trạng thái New → Teacher xem và soạn trả lời → gửi Admin duyệt → Admin approve/close → Student thấy câu trả lời và feedback bị khóa chỉnh sửa.

## Vì sao nhiều tầng?
- Đảm bảo câu trả lời chuyên môn được kiểm duyệt.
- Phân tách trách nhiệm teacher và admin.
- Giữ audit trail minh bạch.

## Code cần tra
- `frontend/src/views/admin/sections/StudioFeedbackTab.vue`
- `backend/src/DsaVisual.Api/Controllers/FeedbackController.cs`
- `backend/src/DsaVisual.Application/Services/LessonService.cs`
- Entity `ContentFeedback`, `CourseFeedback`.

## Câu hỏi sâu
Mỗi trạng thái phải có actor được phép chuyển trạng thái. Không nên cho mọi role sửa feedback đã đóng.

## Checklist phải học thuộc
Vẽ state machine và ghi actor được phép chuyển từng trạng thái; xác định reply/approval lưu ở column nào và khi nào khóa sửa.

## Cách tra code
Tìm enum status, controller actions, service transition và query filter theo current user. Đối chiếu UI badge với status server.

## Câu hỏi khó
Nếu teacher trả lời nội dung độc hại thì admin duyệt ở đâu? Nếu feedback đã close cần sửa thì có reopen workflow/audit không?

## 7. Flow feedback từ UI đến UI

1. Student tạo feedback ở lesson/course component; tìm trong `frontend/src/api/lessons.ts` hàm feedback quanh dòng 112 và component gọi hàm đó.
2. Request đi tới LessonsController/ConceptsController hoặc feedback endpoint; xác định route thật bằng API module.
3. Teacher Studio bắt đầu tại `frontend/src/views/admin/sections/StudioFeedbackTab.vue:26-61`: state danh sách ở dòng 26, load API ở dòng 50, watcher filter ở dòng 61.
4. Filter/search chỉ lọc state ở client quanh dòng 74-93; phân biệt với filter server ở dòng 50-53.
5. Nút trả lời nằm template quanh dòng 206; tìm handler save/reply tiếp trong phần script và API `frontend/src/services/courseApi.ts:184-185`.
6. Response quay về StudioFeedbackTab: cập nhật feedbackItems, xóa loading, hiện toast hoặc reload. Student sau đó gọi API danh sách của mình và thấy reply/status mới.

**Cần xác minh:** status thật, role được reply/approve và endpoint admin; đọc enum/backend thay vì dùng trạng thái trong tài liệu cũ.

## Flow diễn giải bằng lời
Student nhập feedback và gửi; backend lấy current user, validate, lưu status ban đầu rồi trả response để UI báo thành công. Studio load danh sách tại `StudioFeedbackTab.vue:26-61`; filter/search xử lý quanh dòng 74-93. Teacher nhập reply ở template quanh dòng 206, gọi API `courseApi.ts:184-185`; backend kiểm tra role và trạng thái, lưu reply rồi trả DTO. Studio reload item, đổi badge và hiện toast; student mở lại sẽ thấy reply/status mới.

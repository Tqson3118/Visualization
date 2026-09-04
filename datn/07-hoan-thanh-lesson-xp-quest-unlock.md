# 07. Hoàn thành lesson, XP, quest, unlock

## Flow
1. User đọc lesson hoặc hoàn thành exercise.
2. Frontend gọi API complete.
3. Backend kiểm tra lesson, quyền truy cập và điều kiện hoàn thành.
4. Cập nhật progress.
5. Cộng XP và cập nhật quest.
6. Tìm lesson/node tiếp theo.
7. Trả về `nextLessonId`, XP đã nhận và trạng thái.
8. Frontend hiển thị modal, confetti hoặc điều hướng.

## Code cần tra
- `frontend/src/views/lesson/LessonStudyView.vue`
- `backend/src/DsaVisual.Api/Controllers/LessonsController.cs`
- `backend/src/DsaVisual.Application/Services/LessonService.cs`
- `backend/src/DsaVisual.Application/Services/ProgressService.cs`
- `backend/src/DsaVisual.Application/Services/GamificationService.cs`

## Câu hỏi sâu
Nếu gọi complete hai lần, XP không được cộng hai lần. Backend phải kiểm tra trạng thái hiện tại/idempotency và chỉ ghi nhận transition hợp lệ.

## Checklist phải học thuộc
Xác định điều kiện complete, bảng progress được update, XP transaction nào, quest event nào, thuật toán chọn next node và rule unlock.

## Cách tra code
Tìm endpoint complete trong LessonsController, đọc LessonService, rồi lần sang GamificationService và ProgressService. Ghi rõ response DTO.

## Câu hỏi khó
Nếu progress đã complete nhưng XP chưa cộng thì consistency được bảo đảm bằng transaction/retry nào? Nếu node sau bị hidden thì nextLesson xử lý ra sao?

## 7. Flow hoàn thành lesson đến UI

1. Bắt đầu ở `frontend/src/views/lesson/LessonStudyView.vue`; tìm Ctrl+F `complete`, `markViewed`, `progress`, `next`.
2. API progress nằm trong `frontend/src/api/progress.ts:57-58` cho đọc trạng thái; các hàm ghi progress cần tìm tiếp trong cùng file và lessonApi.
3. Request đi tới LessonsController rồi LessonService; sau đó tra ProgressService và GamificationService để biết write progress, XP, quest.
4. Response quay về View/store; state `completed`, `bestScore`, XP và next node được cập nhật.
5. UI kết thúc bằng badge completed, progress bar/modal chúc mừng hoặc điều hướng lesson kế tiếp. Tìm template trong phần cuối của LessonStudyView.

**Cần xác minh bằng code:** project có thể hoàn thành lesson thông qua mark-viewed hoặc thông qua pass exercise; không được kể một endpoint nếu chưa tìm thấy tên method thật.

## Flow diễn giải bằng lời
Student bấm hoàn thành hoặc pass exercise; `LessonStudyView.vue`/progress API gửi request. Backend kiểm tra lesson, quyền và điều kiện, rồi ghi progress completed. Tiếp đó service gamification cộng XP/cập nhật quest và learning-path tính node kế tiếp/unlock. Response trả completed, reward và next lesson. View/store nhận response, đổi badge, progress bar, XP và hiện modal/nút đi tiếp. Request lỗi thì UI không được tự đánh dấu hoàn thành.

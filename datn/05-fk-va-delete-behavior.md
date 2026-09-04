# 05. FK và delete behavior

## Foreign key
FK biểu diễn quan hệ giữa bảng con và bảng cha, đồng thời ngăn dữ liệu mồ côi.

Ví dụ:
- `Exercise.LessonId → Lessons.Id`
- `CodeSubmission.ExerciseId → Exercises.Id`
- `ClassAssignment.ClassId → Classes.Id`

## Delete behavior
- **Cascade**: xóa cha thì xóa con; chỉ dùng khi con phụ thuộc hoàn toàn.
- **Restrict/NoAction**: database chặn xóa cha nếu còn con.
- **SetNull**: xóa cha thì FK con thành null nếu quan hệ optional.

## Vì sao submission thường NoAction?
Lịch sử nộp bài là dữ liệu học tập/audit. Không nên xóa nó chỉ vì lesson hoặc exercise bị xóa.

## Code cần tra
- `backend/src/DsaVisual.Application/Persistence/Configurations/ExerciseConfiguration.cs`
- `backend/src/DsaVisual.Application/Persistence/Configurations/ClassConfiguration.cs`
- `backend/src/DsaVisual.Application/Persistence/Configurations/LearningPathConfiguration.cs`
- `backend/src/DsaVisual.Application/Persistence/Migrations/`

## Câu hỏi sâu
Khi xóa dữ liệu có nhiều FK NoAction, phải xóa bản ghi con trước, hoặc dùng soft delete thay vì hard delete.

## Checklist phải học thuộc
Với mỗi FK hãy ghi parent, child, required/optional và onDelete. Sau đó suy ra thứ tự xóa và nguy cơ orphan/loss history.

## Cách tra code
Đọc HasOne/WithMany/HasForeignKey trong Configuration rồi đối chiếu migration onDelete. Không suy luận chỉ từ tên property.

## Câu hỏi khó
Tại sao không cascade mọi thứ? Vì lịch sử nộp bài/progress cần giữ. Tại sao vẫn cần service check tồn tại? FK chỉ bắt lúc ghi DB, không trả lỗi nghiệp vụ dễ hiểu.

## Giải thích quan hệ bảng
User có nhiều CodeSubmission/Progress; Lesson có Exercise; LearningPath có nhiều LearningPathNode; Class có ClassMember và ClassAssignment; Assignment tham chiếu Exercise/Lesson; Achievement và DailyQuest là bảng định nghĩa, còn UserAchievement/UserQuest là tiến độ theo user. Khi bị hỏi một FK, hãy nói: bảng con nào, bảng cha nào, FK giúp truy ngược nghiệp vụ gì và delete behavior bảo vệ dữ liệu ra sao.

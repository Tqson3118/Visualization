# Đề 07 — Practice Ladder, Lab & Exercise
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** LadderView (`/ladder/:nodeId`), LabView (`/ladder/:nodeId/lab`), ExerciseView (`/exercise/:id`)

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** Trong `LadderView.vue`, mảng `STAGE_WEIGHTS` khai báo trọng số 3 bậc theo thứ tự nào?

A. Quiz 30% — Lab 20% — Code 50%
B. Quiz 20% — Lab 30% — Code 50%
C. Quiz 50% — Lab 30% — Code 20%
D. Quiz 20% — Lab 50% — Code 30%

---

**Câu 2.** Hàm `loadLadderExercises()` trong `LadderView.vue` gọi API theo cách nào?

A. Gọi tuần tự: `fetchExercises(stage:1)` xong mới gọi `fetchExercises(stage:3)`
B. Gọi `fetchExercises(stage:1)` trước, rồi `fetchExercises(stage:2)` song song
C. Dùng `Promise.all` gọi song song `fetchExercises(stage:1)` và `fetchExercises(stage:3)`
D. Dùng `Promise.race` gọi `fetchExercises(stage:1)` và `fetchExercises(stage:2)`

---

**Câu 3.** Trong `LadderView.vue`, khi API load bài tập bị lỗi (catch block), hành vi của ứng dụng là gì?

A. Throw lại lỗi, ứng dụng crash toàn trang
B. Hiện thông báo lỗi toast, chuyển hướng về trang chủ
C. Giữ `quizExercise` và `codeExerciseId` là `null`, LadderShell hiện EmptyState
D. Reload lại trang sau 3 giây

---

**Câu 4.** Trong `ExerciseView.vue`, khi user click nút **"Luyện tập (không chấm điểm)"**, điều gì xảy ra?

A. Gọi API tạo session luyện tập riêng biệt trên server
B. Toggle biến `practiceMode` local, truyền prop xuống `QuizStage`
C. Xóa toàn bộ lịch sử làm bài cũ rồi bắt đầu bài mới
D. Chuyển hướng tới route `/exercise/:id/practice`

---

**Câu 5.** Trong `LabView.vue`, tiêu chí **đạt** của LabStage là gì (theo `INFO_CARDS`)?

A. Viết đúng code sắp xếp và chạy thành công trong sandbox
B. Trả lời đúng tất cả câu quiz trắc nghiệm
C. Đưa dãy về đúng trạng thái cuối (tăng dần) trong số bước không vượt quá chuẩn × 1.5
D. Hoàn thành tất cả bước trong bài giảng lý thuyết

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

**Câu 6.** Trace luồng khởi tạo của `LadderView.vue` khi user vào route `/ladder/3`.
Ghi đủ **4 chặng** tên hàm/hook thật từ source, mô tả ngắn mỗi chặng.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```

---

**Câu 7.** Trace luồng khi user click **"Lịch sử làm bài"** trong `ExerciseView.vue`.
Ghi đủ **4 chặng** tên hàm/hook/state thật từ source, mô tả ngắn mỗi chặng.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```

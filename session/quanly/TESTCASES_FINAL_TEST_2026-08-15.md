# TEST CASES — Kiểm tra cuối lộ trình (khóa Grokking Data Structures)

Ngày chạy: 15/08/2026 · Môi trường: backend thật (localhost:5000) + dữ liệu seed thật
Kết quả: **29/29 PASS** (0 FAIL)

## A. Final-Quizz — 20 câu trắc nghiệm (exercise 88, node 53)

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| A1 | GET `/api/v1/concepts/quiz/88` | 20 câu hỏi | PASS (20) |
| A2 | Mỗi câu hợp lệ (options ≥ 2, correctIndex trong khoảng, có nội dung) | 0 câu hỏng | PASS |
| A3 | Phân bố 5 câu/module: 1–5 Array&Linked List · 6–10 Stack&Queue · 11–15 Hash · 16–20 Tree | đúng thứ tự module | PASS |
| A4 | Nộp 20/20 đúng | score=20, passed=true, xp=20 | PASS |
| A5 | Nộp 14/20 đúng (đúng 70%) | passed=true (ngưỡng ≥ 70%) | PASS |
| A6 | Nộp 13/20 đúng (65%) | passed=false | PASS |
| A7 | Nộp 0/20 đúng | score=0, passed=false, xp=0 | PASS |
| A8 | QuizId không tồn tại | 404 | PASS |
| A9 | Mảng answers thiếu câu → câu thiếu tính sai | score đếm đúng phần có | PASS |
| A10 | Sau khi pass → progress node 53 Status=2 | codelabCompleted=true | PASS |

## B. Kiểm tra cuối lộ trình — 3 bài code Assignment-style (exercise 79, node 54)

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| B1 | GET lesson 54 | sandboxType=codelab | PASS |
| B2 | ConfigJson parse thành 3 bài code | 3 tasks | PASS |
| B3 | Mỗi bài 5 testcase: 3 public + 2 hidden | đúng 3+2 | PASS |
| B4 | Toàn bộ input/expectedOutput là JSON hợp lệ | parse OK | PASS |
| B5 | entryFunction tồn tại trong initialCode | 3/3 | PASS |
| B6-1 | Bài 1 (Đảo ngược Linked List): solution đúng | 5/5 pass | PASS |
| B6-2 | Bài 2 (Dấu ngoặc Stack): solution đúng | 5/5 pass | PASS |
| B6-3 | Bài 3 (Tổng K trong BST + Hash): solution đúng | 5/5 pass | PASS |
| B7-1 | Bài 1: thiếu bước đảo ngược | fail 3/5 (case [] và [42] đối xứng nên đúng) | PASS |
| B7-2 | Bài 2: chỉ đếm ngoặc, không check thứ tự | fail (4/5) | PASS |
| B7-3 | Bài 3: dùng lại chính node | fail hidden case "không trùng node" | PASS |
| B8 | Bài 1: code lỗi biên dịch | báo compile error, 0 pass | PASS |
| B9 | Bài 1: vòng lặp vô hạn | timeout 1500ms (worker kill) | PASS |
| B10 | FE chỉ hiển thị 3 testcase công khai | 3 public | PASS |

## C. Tích hợp trang khóa (/courses/7)

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| C1 | Lesson 17 = Final-Quizz | sb=quiz, module="Kiểm tra cuối lộ trình", quizId=88 | PASS |
| C2 | Lesson 18 = Kiểm tra cuối lộ trình | sb=codelab, module="Kiểm tra cuối lộ trình" | PASS |
| C3 | Khóa có đúng 5 nhóm module | Module 1–4 + "Kiểm tra cuối lộ trình" | PASS |
| C4 | Thứ tự Final-Quizz (17) trước Kiểm tra cuối (18) | đúng | PASS |

## D. Hồi quy (đã chạy trong phiên)

- BE: `dotnet test` = 153 PASS (seed idempotent: chạy lại 0 thêm)
- FE: `vue-tsc --noEmit` sạch · `vitest run` = 164 PASS
- Seed 3 lần trên DB thật: node đổi tên 1 lần, Final-Quizz thêm 1 lần, kiểm tra cuối chuyển MCQ→CODE 1 lần, các lần sau 0 thêm (idempotent + tự cập nhật ConfigJson nếu lệch)

## E. Nghiệp vụ (business rules) — 7/7 PASS

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| P1 | POST `auth/progress/54` codelabCompleted=true | success | PASS |
| P2 | GET `auth/progress/54` → codelabCompleted=true (pass bền, không mất sau reload) | true | PASS |
| P3 | Course detail: lesson "Kiểm tra cuối lộ trình" status=Completed | Completed | PASS |
| P4 | Node kiểm tra cuối được tính vào completedLessons (tăng 1 node) | 3/18 | PASS |
| P5 | progressPercent đúng phép tính backend (3/18 → 17%) | 17% | PASS |
| P6 | Re-submit Final-Quizz 20/20 → vẫn pass, không lỗi (idempotent) | passed=true | PASS |
| P7 | Final-Quizz status=Completed qua quiz submit | Completed | PASS |

### Lỗi nghiệp vụ đã phát hiện & sửa trong đợt này
- **Bug (có sẵn, ảnh hưởng cả 4 Assignment):** `POST /concepts/auth/progress/{id}` (FE gọi khi hoàn thành bài code) KHÔNG ghi `UserNodeProgress` → hoàn thành bài code xong: tick ✓ mất sau reload, `progressPercent` khóa không bao giờ đạt 100%, box "Góp ý cho giảng viên" (điều kiện 100%) không bao giờ hiện. **Đã fix** tại `ConceptsController.SaveProgress`: `codelabCompleted=true` → upsert `UserNodeProgress` Status=2 (Stars=3, NodeScore=100, PassedAt). Verify P1–P5.

### Ghi chú thiết kế (không phải bug, cần chốt nghiệp vụ nếu muốn đổi)
- XP của bài code nhiều task: `LessonStepCodeLab` gọi `syncXP(50)`/task nhưng store đó chỉ cộng biến local (không persist, không gọi API) → XP thật chỉ tính qua `completeCodelab` (+100 XP). Không có XP kép.
- Khóa không có cơ chế khoá node: học viên vào thẳng "Kiểm tra cuối lộ trình" từ curriculum mà không cần hoàn thành trước (hành vi chung của cả khóa hiện tại).
- Thống kê khóa đổi theo dữ liệu thật: Quiz 13 → 14 (thêm Final-Quizz), Bài tập 4 → 5 (thêm Kiểm tra cuối code).

## F. Khoá node tuần tự (nghiệp vụ lộ trình) — 15/15 PASS (user mới)

Nguyên tắc: node MỞ khi node ngay trước (SortOrder) đã hoàn thành (Status=2) · node đầu luôn mở ·
node đã pass luôn mở (được xem lại) · chặn API 403.

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| L1 | User mới: Bài 1 không khoá | unlocked | PASS |
| L2 | User mới: node 2–18 đều KHOÁ (17/17) | locked | PASS |
| L3 | GET lesson bị khoá → 403 + "Bài học chưa được mở khóa" | 403 | PASS |
| L4 | Quiz submit cho quiz node bị khoá → 403 (chống nộp thẳng API) | 403 | PASS |
| L5 | Hoàn thành Bài 1 (SaveProgress completed=true) | success | PASS |
| L6 | Xong Bài 1 → Bài 2 MỞ, Bài 1 Completed | mở khoá đúng | PASS |
| L7 | Xong Bài 1 → Bài 3–18 vẫn khoá (16/16) | locked | PASS |
| L8 | GET Bài 2 sau khi mở → 200 | 200 | PASS |
| L9 | GET Bài 3 (mini-quizz) vẫn 403 | 403 | PASS |
| L10 | Xong Bài 2 → Mini-Quizz MỞ (có quizId) | 200 | PASS |
| L11 | Mini-Quizz đạt 70% → passed | passed=true | PASS |
| L12 | Pass Mini-Quizz → Assignment 1 MỞ | unlocked | PASS |
| L13 | Node 5 (Bài 3 module 2) vẫn khoá | locked | PASS |
| L14 | Xong Assignment (codelab) → Bài 3 module 2 MỞ | unlocked | PASS |
| L15 | Node đã pass luôn mở lại được (Bài 1, Bài 2 → 200) | 200/200 | PASS |

Thay đổi code (phiên này): `ConceptsController` (Locked DTO + IsNodeLockedAsync + 403 GET lesson/
quiz submit + SaveProgress nhận cờ `completed`) · `lessonApi.ts` (payload completed + message 403) ·
`useLessonStore` (lessonFinished) · `LessonStudyView` (sync sau khi hoàn thành + sidebar khoá) ·
`CourseDetailView` (UI khoá curriculum + chặn click).

## H. Full-walk cả 4 Assignment + fix ASM 3/4 — 24/24 PASS (user mới)

Đi hết khóa theo chuỗi khoá: theory → quiz → ASM → module sau. Tại MỖI Assignment: code SAI → không
pass; code ĐÚNG (đủ mọi task) → server chấm full → node Completed + mở node sau.

| Assignment | Task (server chấm) | SAI → không pass | ĐÚNG → pass + mở node sau |
|---|---|---|---|
| ASM 1 — Quản lý sinh viên | 3 task: insertAtHead(2), insertArray(2), arrayToLinkedList(2) | PASS (0/2) | PASS |
| ASM 2 — Điều phối vé xem phim | 2 task: isValid(5), processTickets(4) | PASS (0/5) | PASS |
| ASM 3 — Giỏ hàng tốc độ cao | 1 task: getUserInfo(3) — Hash Map | PASS (0/3) | PASS |
| ASM 4 — Product Catalog | 1 task: inorderTraversal(5) — BST | PASS (0/5) | PASS |

### Bug phát hiện & đã sửa trong đợt này
- **Bug seed ASM 3 + 4 (nghiêm trọng):** sandboxConfig nguồn của Assignment 3/4 là **object đơn**
  (không phải array) → `ConvertTasks` rơi vào fallback rỗng `{signature, testCases:[]}` (ConfigJson
  chỉ 105–134 ký tự) → FE không hiện đề thật, máy chủ không chấm được. **Đã fix** `BuildAssignmentConfig`
  (bọc task đơn thành mảng 1 phần tử) + thêm **tự chữa ConfigJson** cho exercise assignment (DB cũ
  86/87 đã được sửa: 134→2008, 105→4033 ký tự; seed chạy lại idempotent).
- **Bug khoá (bỏ qua kiểm tra):** `SaveProgress` không kiểm tra node khoá → ghi tiến độ/pass node
  chưa mở (bypass chuỗi). **Đã fix**: SaveProgress trả 403 nếu node bị khoá (node đã pass vẫn ghi được).

## G. Chấm code PHÍA MÁY CHỦ — bài ASM chỉ pass khi code ĐÚNG (Jint) — 14/14 PASS (user mới)

Nguyên tắc mới (15/08): nộp qua `/exercises/{id}/code-submit` kèm `taskId` → **máy chủ tự chạy code
(Jint sandbox: timeout 1.5s / 200k lệnh / 32MB / stack guard)** so testcase của task — điểm/Passed/Total
client khai bị BỎ QUA. Node ASM pass khi **tất cả task con** có bài nộp full-pass. `SaveProgress
completed=true` KHÔNG pass được node có bài code (chống bypass).

| # | Test case | Kỳ vọng | Kết quả |
|---|---|---|---|
| A1 | Lesson ASM có exerciseId (để nộp server) | có | PASS |
| A2 | ConfigJson có task (entryFunction + testcases) | có | PASS |
| A3 | Code SAI + client khai 100/100 → server chấm lại 0/2 | passed < total | PASS |
| A4 | Code SAI → node ASM không pass | NotStarted | PASS |
| A5 | completed=true qua SaveProgress không bypass node ASM | không pass | PASS |
| A6 | Lỗi biên dịch → báo lỗi rõ, passed=0 | error ≠ null | PASS |
| A7 | Vòng lặp vô hạn → bị chặn (40ms) + không pass | timeout | PASS |
| A8 | Compile error/timeout → node không pass | NotStarted | PASS |
| A9 | Code ĐÚNG 1 task → server chấm full 2/2 | full | PASS |
| A10 | Pass 1/3 task → node chưa pass (chờ đủ task) | NotStarted | PASS |
| A12 | Pass ĐỦ 3 task con → server chấm full từng task | 3/3 | PASS |
| A13 | Node Assignment 1 status=Completed | Completed | PASS |
| A14 | Pass Assignment → Bài 3 module 2 MỞ khoá | unlocked | PASS |
| A11 | Kiểm tra cuối lộ trình vẫn khoá khi chưa pass đủ chuỗi | locked | PASS |

Bổ sung:
- **P1–P8 (nghiệp vụ, 8/8 PASS)**: SaveProgress không pass node code; code sai task 2 → 2/5 (không full);
  pass đủ 3 task kiểm tra cuối → node pass bền; Final-Quizz vẫn pass qua quiz submit.
- **Unit tests mới**: `CodelabJudgeTests` 13 test (judge đúng/sai/compile/timeout/stack-guard/parse
  config; SubmitCode server-judge: đúng → pass node, sai → không pass, khai cao → bị bỏ qua,
  thiếu/sai TaskId → 400). BE tổng **166 PASS**.
- **Thay đổi code**: `CodelabJudgeService` (Jint) · `ExerciseService.SubmitCodeAsync` (judge + pass đủ
  task) · `CodeSubmitRequest.TaskId` · `LessonDetailResponse.ExerciseId` · `SaveProgress` (chặn bypass)
  · FE: `lessonApi.submitCodelab` + `LessonStepCodeLab` (server chấm trước khi pass) + store meta.

Ghi chú: A4/A5/A10/P1/P6 làm student@demo thật sự pass node 53 + 54 (progress thật — dữ liệu demo hợp lệ).

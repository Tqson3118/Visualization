# G-BF2 — STATUS SỬA 8 BUG FRONTEND (dev-frontend, 12/08/2026)

- **Task**: G-BF2 — sửa 8 bug frontend theo SETUP_TODO §6 + §8, contract đã chốt ở docs/pm-decision-log-g.md
- **Ngày sửa**: 12/08/2026
- **Người sửa**: dev-frontend (Thai Quang Son)
- **Nhánh**: feature/ux-bugfix-frontend (tạo từ origin/dev)
- **Kết quả**: 8/8 DONE — build 0 lỗi (vue-tsc + vite) · test 72/72 PASS · smoke thật (backend docker + dev server) tất cả contract 200

## File đã sửa

| # | Mục | File | Thay đổi |
|---|---|---|---|
| 1 | Leaderboard crash (P1 F3-NEW-1, §8.1) | `frontend/src/api/gamification.ts` | `fetchLeaderboard` map `PagedResponse.items` → `rows`; `myRank` = item có `userId === user` hiện tại (lấy từ auth store, nếu có) hoặc `null`; `LeaderboardDto` thêm `total`. `rows` luôn array → LeaderboardView không crash |
| 2 | code-runs contract (P2 #7, §6.7) | `frontend/src/stores/codeRunner.ts`, `frontend/src/api/codeRunner.ts` | `run()` gửi `{key: key.value, code, input: JSON.stringify(defaultArray), status:'Success', durationMs, stats}` thay vì `{code, input: array}`; `SaveCodeRunPayload` type mới (key bắt buộc string, input string) |
| 3 | benchmarks/run contract (P2 #8, §6.8) | `frontend/src/components/benchmark/BenchmarkPanel.vue`, `frontend/src/api/types.ts` | `buildResults()` map rows (size×key) → `[{key, measurements:[{n,durationMs,comparisons,swaps}]}]` gửi kèm trong POST; type `BenchmarkRequest.results` |
| 4 | Nút "Làm bài" chết (P1 #3, §6.3) | `frontend/src/views/NodeHubView.vue` | Thêm `@open-exercise="openExercise"` lắng nghe emit từ LessonDetail.vue:154 → điều hướng `/exercise/{id}` |
| 5 | Submit thiếu câu → 400 (P2 #5, §6.5) | `frontend/src/components/ladder/QuizStage.vue` | Trước khi nộp: liệt kê câu chưa trả lời (`Bạn còn X/Y câu chưa trả lời: 1, 3...`) + nhảy tới câu thiếu đầu tiên — không gửi request 400 |
| 6 | Ladder stage rỗng (P1 #6, §6.6) | `frontend/src/views/LadderView.vue`, `frontend/src/views/NodeHubView.vue`, `frontend/src/api/exercises.ts`, `frontend/src/views/AdminLadderView.vue`, `frontend/src/views/FinalTestView.vue` | Tải exercise qua `GET /exercises?nodeId&stage` (stage 1=QUIZ, 3=CODE); quiz lấy id rồi `fetchExercise(id)`; trống → EmptyState. Kèm fix contract `fetchExercises` unwrap `PagedResponse.items` (trước đọc nhầm mảng) + `ExerciseSummaryDto` |
| 7 | Router TEACHER vào /admin (P2 #5, §8.5) | `frontend/src/router/index.ts` | `/admin/users` + `/admin/settings` roles `['ADMIN']`; guard unauthorized → `/profile` nếu đã đăng nhập (trước về home) |
| 8 | Mất phiên khi reload (P1 #1, §6.1, ADR-004) | `frontend/src/main.ts` | Boot async: `await auth.refresh()` (cookie HttpOnly) → thành công thì `fetchMe()`; TRƯỚC khi router guard chạy. Refresh lỗi → không chặn trang công khai |

## Kết quả verify (chạy 12/08/2026)

### Build + test
- `npm run build` (vue-tsc -b && vite build) → **0 lỗi, exit 0** ✔
- `npm test` (vitest run) → **72/72 PASS** (8 files: engines 54 + api 6 + store 5 + data 7) ✔

### Smoke thật (backend localhost:5000 + dev localhost:5173, student@demo.local)
- **Mục 1**: `/leaderboard` không crash — render EmptyState (BE trả items=0 tuần hiện tại); console 0 lỗi ✔
- **Mục 2**: POST `/code-runs` → **201** (trước 400). Payload đúng: `{"key":"sort.bubble","code":"...","input":"[5,3,8,1,9,2,7]","status":"Success","durationMs":11,"stats":{...}}` ✔
- **Mục 3**: POST `/benchmarks/run` → **200** (trước 400). Payload có `results:[{key,measurements:[{n,durationMs,comparisons,swaps}]}]`; UI hiện "Đã lưu kết quả lên server." + bảng/biểu đồ ✔
- **Mục 4**: click "Làm bài" (Quiz: Bubble Sort) → điều hướng `/exercise/1` ✔
- **Mục 5**: "Nộp bài" khi 0/5 → alert `"Bạn còn 5/5 câu chưa trả lời: 1, 2, 3, 4, 5. Hãy trả lời đủ trước khi nộp bài."`, KHÔNG có request `/submit` ✔
- **Mục 6**: NodeHub + /ladder/1 gọi `GET /exercises?nodeId=1&stage=1` + `stage=3`; trống → EmptyState thay vì crash; stepper hiển thị đúng ✔
- **Mục 7**: teacher@demo.local vào `/admin/users` + `/admin/settings` → **redirect /profile**; `/admin/stats` vẫn mở được (không đổi) ✔
- **Mục 8**: reload trang → boot gọi `POST /auth/refresh [200]` + `GET /auth/me [200]`; trang protected `/ladder/1` giữ phiên (không bị đá về /login); header hiện đúng user ✔

## Ghi chú
- Canvas ResizeObserver (SETUP_TODO §6.4) đã fix từ ba62a33 — nằm ngoài phạm vi.
- Backend (G-BF1) xử lý song song: mark-viewed, heart regen persist, duplicate QuestionId 400, SubmissionLockRegistry, cookie Secure — không đụng FE.
- `fetchExercises` đổi contract từ mảng → `PagedResponse.items` (khớp BE thật) — buộc sửa kèm AdminLadderView (type `ExerciseSummaryDto`) + FinalTestView (fetch chi tiết theo id) để build xanh.
- Test hiện có 72 (không thêm mới trong task này — các fix đã verify bằng smoke thật).

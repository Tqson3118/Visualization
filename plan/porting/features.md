# FEATURES IMPLEMENTED — DSA-VISUAL Web (D:\FPT\neww)

> Hoàn tất port có chọn lọc 3 feature từ source/VisualizationDSA (repo tham khảo, CHỈ ĐỌC) vào web hiện tại.
> Branch: dev. 3 commit riêng (1 feature / 1 commit). Không đụng file dirty của team (source/VisualizationDSA, docker-compose.yml), không reset DB, chưa push (origin/dev vẫn 309e5b7).

## F1 — Teacher Learning Path / Curriculum per-class  ✅ commit d77a61c
- Mục tiêu: giáo viên tạo lộ trình học cho từng lớp (chọn bài học/bài tập có sẵn, sắp xếp, save draft, publish); học viên xem path + trạng thái tiến độ thật.
- Backend: Class + CurriculumTitle/Description/Published (default TRUE giữ hành vi cũ), ClassAssignment.SortOrder, migration AddClassCurriculum; 3 endpoint mới trong ClassesController (GET/PUT /classes/{id}/curriculum, PUT /classes/{id}/curriculum/reorder); AddAssignment tự append sortOrder; học viên không thấy draft; status từ UserProgress/ExerciseSubmission (completed/in_progress/not_started + progressPct); permission CanManage (owner/ADMIN) ở service.
- Frontend: api/types + api/classes (fetch/update/reorder), stores/classStore (curriculum state/actions), i18n vi.ts (classes.curriculum*), ClassDetailView.vue tab "Lộ trình học" (teacher: tên/mô tả + Save draft/Publish + move up/down + xóa + thêm; student: path + status + progress bar), utils/curriculumOrder.ts (buildReorderItems).
- Tests: BE 8 unit + 4 integration; FE 9. Regression sau F1: FE 24 files/216 tests; BE 167 unit + 82 integration = 249.

## F2 — Gamification UI (level, XP→next, quests, badges, streak)  ✅ commit 1c9f669
- Mục tiêu: hiển thị level/XP tiến tới level kế, quests todo/in-progress/completed, badges locked/unlocked, streak — KHÔNG đổi gamification engine.
- Backend: GET /me/gamification -> GamificationSummaryDto {xp, level, xpIntoLevel, xpForNextLevel, levelProgressPct}; công thức level = 1 + floor(sqrt(xp/100)); floorXP=(level-1)^2*100; span=100*(2*level-1). Files: Dtos/GamificationSummaryDto.cs, Services/IGamificationService.cs + GamificationService.cs (GetGamificationSummaryAsync), Api/Controllers/GamificationController.cs, tests/DsaVisual.UnitTests/GamificationSummaryTests.cs (5 test).
- Frontend: api/gamification.ts (fetchGamificationSummary), stores/gamification.ts (state summary + computed xpIntoLevel/xpForNextLevel/levelProgressPct + fetchSummary(); fetchAll() gọi fetchSummary — sửa luôn XP/level hero ProfileView), i18n vi.ts (section gamification), 4 component prop-driven mới: components/gamification/{XpProgressCard,StreakCard,QuestProgressCard,BadgeGrid}.vue, ProfileView.vue (import 4 card + fetchQuests trong onMounted + section .profile__gamification trong tab overview sau radar card + CSS).
- Tests: FE 13 (gamificationCards.spec.ts — fix lỗi "no tests": spec nằm trong __tests__/ nên import phải ../Xx.vue; probe.spec.ts đã xóa trước commit). Regression: FE 25 files/229 tests; BE 172 unit + 82 integration = 254.

## F3 — Code-to-Visual MVP (constrained DSL → trace → playback)  ✅ commit 7640550
- Mục tiêu: nâng Playground thành Code-to-Visual: DSL giới hạn (array/stack/queue) -> trace events -> playback trên canvas — KHÔNG chạy arbitrary code, KHÔNG AST/new Function/worker.
- Module thuần mới: frontend/src/features/code-to-visual/dsl/ {types.ts, parser.ts, trace.ts, toSimSteps.ts} + spec.
  - DSL: array.push(n) / array.set(i,v) / array.swap(i,j) / array.pop(); stack.push/pop/peek; queue.enqueue/dequeue/front. Parser từ chối lệnh lạ kèm line number (DslError{line,message}); comment \/\/, #, -- và dòng trống được bỏ qua; hỗ trợ số âm.
  - Interpreter đồng bộ (in-process): runTrace(ParsedOp[], initial{array,stack,queue}) -> TraceEvent[] {step, line, structure, operation, state, highlightedIndices?, explanation} + logs console + error runtime (pop rỗng, set/swap ngoài phạm vi) có line.
  - eventsToSteps: TraceEvent[] -> engine Step[] (Structure qua arrayStructure / linearStructure; pseudocodeLine carry-through; highlights channel swap/active/highlight; stats writes/swaps tích lũy; variables cho PseudocodePanel).
- View: frontend/src/views/CodeToVisualView.vue + route /playground/code-to-visual (name 'code-to-visual', requiresAuth). Layout 2 cột: trái = editor DSL + input mảng khởi tạo (2–50 số) + console log (info/success/error, timestamp) + docs cú pháp; phải = CanvasArea (DataStructureStage) + ControlBar (play/pause/step-back/step-forward/reset/speed) + PseudocodePanel (source lines, activeLine = dòng DSL, breakpoint vẫn hoạt động) + explanation + error badge dòng lỗi. Empty-state overlay khi chưa chạy.
- Store: stores/simulation.ts — thêm loadSteps(title, steps)/clearSteps (nạp steps trực tiếp từ DSL, không qua registry generator; LoadedSimulation.generator nullable; configureInput guard null).
- i18n: section codeToVisual (editorPlaceholder dùng \n escape — runtime thành newline).
- Tests: FE 14 DSL + 5 view (mount với test Pinia, stub ControlBar/CanvasArea/PseudocodePanel; assert empty-state, input validation → RUN disabled, run sinh steps + console success, lỗi cú pháp → data-testid dsl-error đúng line + store thải steps, Xóa → clear). Regression: FE 25 files/248 tests, build OK (vue-tsc + vite); BE không đổi.

## VERIFICATION CUỐI (Phase 4)
- FE: npm run build OK (vue-tsc -b + vite build, 0 error); npx vitest run -> 25 files / 248 tests PASS.
- BE: dotnet build DsaVisual.sln OK 0 warn/0 err; dotnet test -> 172 unit + 82 integration = 254 PASS.
- Docker (docker compose ps): neww-backend (Up healthy, :5000), neww-mailhog (Up), neww-sqlserver (Up healthy). vdsa-database (postgres) + vdsa-redis là của repo tham khảo (không phải app này). Testcontainers SQL Server dùng riêng cho integration test, tự dọn sau khi chạy.
- Giữ nguyên: file dirty của team (source/VisualizationDSA/** + docker-compose.yml) chưa stage/commit.
- HEAD dev = 7640550; origin/dev = 309e5b7 (chưa push — cần xác nhận user trước khi push).

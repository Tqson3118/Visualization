# G-F3C — REVIEW ĐỘC LẬP ĐỢT G (dev-review, Phase 3c) — 2026-08-13

> Người review: dev-review (độc lập — chỉ đọc, KHÔNG sửa code) · Nhánh: `dev`
> Phạm vi diff: `d62e2ac..HEAD` (`70113f5` — G-F3E leaderboard fix) + toàn đợt G `085a82c^..HEAD` (G-BF1/G-BF2/G-BF3/G-F1/G-F2/G-F3a/G-F3E)
> Căn cứ: docs/pm-decision-log-g.md (bản làm việc, chưa commit) + docs/work/g-f3a.md, g-f3b.md, g-f3e.md

---

## 1. Tổng quan

| Hạng mục | Kết quả |
|---|---|
| `git diff d62e2ac..HEAD --stat` | 13 files, +382/−4 — đúng phạm vi G-F3E (2 bug leaderboard + tests + docs) |
| Toàn đợt G `085a82c^..HEAD` | 195 files, +14038/−1961 — đúng phạm vi Phase 0–3 |
| Grep cấm (PostgreSQL/Npgsql/MediatR/Judge0) | ✅ 0 match production — chỉ 3 comment khẳng định "KHÔNG dùng Repository" + text docs |
| Secret scan (sk-/ghp_/AKIA/AIza/private key) | ✅ 0 match thật — chỉ mock token e2e (`e2e-access-token`) |
| `.env`/config thật | ✅ Không có file .env mới; chỉ `.env.example` + `.env.development/.production` (đúng SDD §3.1) |
| License THIRD_PARTY vs package.json | ✅ Khớp (đối chiếu 19 gói: version + license) |
| NFR-5 vs build thật | ✅ Khớp: engine 476KB gốc (≤500KB), JS tải lần đầu ≈852KB (≤1.5MB) |

---

## 2. Bảng đánh giá theo vùng

### 2.1 Backend — G-BF1 (mark-viewed / heart regen / duplicate Q / submit lock / cookie)

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| B1 | `POST /lessons/{id}/mark-viewed` (`LessonsController` + `LessonService.MarkViewedAsync`) | ✅ **OK** | Upsert đúng: query theo (UserId, LessonId) → Add hoặc update `Viewed=true` + `UpdatedAt`. Có unique index `IX_UserProgress_UserId_LessonId` → idempotent, không trùng bản ghi. Student bị chặn nếu bài không Active. Integration test TEST-B-033/034 (204 + 1 bản ghi + lần 2 không nhân đôi + 404 missing) đầy đủ. |
| B2 | Heart regen persist (`GamificationService.PersistHeartRegenAsync` + `GetHeartsStatusAsync` + `EnterNodeAsync`) | ⚠️ **P3 — cần lưu ý** | Logic đúng, khớp ComputeHearts, có atomic decrement điều kiện `WHERE Hearts > 0`. **Race nhỏ** (TOCTOU): persist UPDATE `SET Hearts = {absolute}, LastHeartAt = {now} WHERE Id` ghi giá trị tuyệt đối từ read-stale — hai request đồng thời có thể last-write-wins hồi sinh 1 tim (người chơi được +1, không mất tim, không security). `GamificationService.cs:830-867`. **Hiệu năng**: GET /me/hearts giờ thêm 1 read (FirstAsync lặp user) + có thể 1 write mỗi request — chấp nhận cho scope này. Không làm chậm đáng kể. |
| B3 | Duplicate QuestionId → 400 (`ExerciseService.cs:276-281`) | ✅ **OK** | `Distinct().Count() != Count` trước `ToDictionary` → chặn ArgumentException (500) thành VALIDATION_FAILED. `Answers` có default `= []` + validator NotEmpty → an toàn null. Unit test `Submit_DuplicateQuestionId_ReturnsValidationFailed` phủ. |
| B4 | Submit-code lock + status (`ExerciseService.cs:509-519` + `SubmissionLockRegistry`) | ✅ **OK** | Check `ExerciseStatus.Active` → EXERCISE_CLOSED; lock per (UserId, ExerciseId) singleton `Wait(0)` không chặn → SUBMISSION_IN_PROGRESS. Đúng pattern đã có ở `SubmitAsync`. 2 unit test phủ (NotActive + Concurrent). **P3 ghi chú**: `ConcurrentDictionary` không bao giờ remove key — rò rỉ nhỏ trong phiên dài, không ảnh hưởng scope. |
| B5 | Cookie Secure chỉ khi HTTPS (`AuthController.BuildRefreshCookieOptions`) | ✅ **OK** | `Secure = Request.IsHttps` — dev HTTP không bị chặn cookie; prod HTTPS đúng. **P3 ghi chú**: nếu deploy sau reverse proxy TLS-terminating thì cần ForwardedHeaders — ngoài scope hiện tại. Integration test Auth 401-refresh phủ. |

### 2.2 Backend — G-F3E leaderboard

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| B6 | `LeaderboardEntryDto.Value` (long) + map theo tab (`QuestDtos.cs:47-52`, `GamificationService.cs:447-449`) | ✅ **OK** | FE đọc `row.value` cho mọi tab; map `Value = Xp` đúng với cách xếp hạng hiện có (cả 3 tab sort theo tổng Xp, chỉ khác bộ lọc). Comment trong code + g-f3e.md ghi rõ giới hạn. 4 unit test phủ: level/week/class/thiếu classId. |
| B7 | Tab `week` semantics | ⚠️ **P3 — dữ liệu, đã document** | Bộ lọc tuần theo `LastActivityDate >= weekStart` nhưng `Value` = tổng Xp (không phải XP tuần). Đây là hành vi cũ có chủ đích (g-f3e.md §5 + comment code) — không phải regression. Label FE "XP tuần" có thể hơi lệch ý nghĩa, cần seed dữ liệu thật sau này. |

### 2.3 Frontend — G-F3E leaderboard

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| F1 | `LeaderboardView` crash `toLocaleString` (G-F3D-NEW-1) | ✅ **FIXED** | `row.value` giờ luôn có từ BE + mock e2e trả `value`; `myRank.value` cũng an toàn. E2E 3 tab 0 pageerror + ảnh verify. |
| F2 | Tab Lớp gửi classId (G-F3D-NEW-2) | ⚠️ **P2 — CẦN SỬA (1 điểm)** | `switchTab('class')` → `resolveClassId()` (currentClass → classes[0] → fetchClasses) ✅. Không có lớp → `setNoClass()` EmptyState, không gọi API ✅. **NHƯNG `goToPage()` (LeaderboardView.vue:66-69) gọi `board.fetchBoard(undefined, undefined, next)` KHÔNG truyền classId** → tab Lớp khi phân trang (totalPages>1, tức lớp >20 thành viên) vẫn gửi `classId: undefined` → backend 400 "Thiếu classId". Fix nhỏ: lưu classId trong store (vd `board.lastClassId`) hoặc truyền qua `goToPage`. |
| F3 | `stores/leaderboard.ts` + spec | ✅ **OK** | `noClass` + `setNoClass()` đúng; `fetchBoard` reset noClass; spec 4 test phủ (value giữ nguyên, class truyền classId, setNoClass không gọi API, lỗi → error). |
| F4 | e2e `leaderboard.spec.ts` + `mockApi` | ✅ **OK** | Mock GET /classes + /leaderboard có `value`; 2 test: 3 tab không crash + noClass EmptyState. Route override sau → LIFO thắng đúng Playwright. |

### 2.4 Frontend — G-BF2 (8 bug)

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| F5 | `codeRunner` payload (`stores/codeRunner.ts:111-120` + `api/codeRunner.ts`) | ✅ **OK** | Gửi `{key, code, input: JSON.stringify(defaultArray), status, durationMs, stats}` đúng contract ADR-012. |
| F6 | `BenchmarkPanel` results (`buildResults`) | ✅ **OK** | Map rows → `results:[{key, measurements}]` gửi kèm `runBenchmark` — đúng BE yêu cầu. Timeout/null → 0 (ghi chú). |
| F7 | Nút "Làm bài" (`NodeHubView.openExercise`) | ✅ **OK** | Router push `/exercise/:id` — emit được lắng nghe (LessonDetail `@open-exercise`). |
| F8 | Submit thiếu câu (`QuizStage.onSubmit`) | ✅ **OK** | Liệt kê câu thiếu + nhảy tới câu đầu tiên chưa trả lời, chặn 400 QUESTION_ANSWER_MISMATCH. |
| F9 | Ladder stage (`LadderView` + `NodeHubView.loadLadderExercises`) | ✅ **OK** | `fetchExercises({nodeId, stage:1/3})` unwrap `PagedResponse.items`; lỗi → null + EmptyState (không crash). API type `ExerciseSummaryDto` tách rõ. |
| F10 | Teacher khỏi /admin (`router/index.ts:267-280`) | ✅ **OK** | `/admin/users` + `/admin/settings` roles `['ADMIN']`; backend `UsersController` + `SettingsController` cũng `[Authorize(Roles="ADMIN")]` — khớp 2 phía. Redirect đã đăng nhập → /profile (không còn lỗi home). |
| F11 | Session reload (`main.ts bootstrap`) | ✅ **OK** | `await auth.refresh()` trước `app.use(router)` → không flash login, guard đọc được role sớm. `refresh()` singleton + interceptor 401 loại `/auth/` (không loop). fetchMe lỗi → catch giữ trạng thái error. |

### 2.5 Phase 1–2 (foundation + polish) — spot-check

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| F12 | 13 shadcn wrapper (`components/ui/*`) | ✅ **OK** | API giữ tương đương (Button variant/size/loading/block; Tabs tabs+modelValue+change; EmptyState icon/title/description/action; Skeleton height/lines; BaseIcon giữ tự xây). Call sites build sạch (`vue-tsc` 0 lỗi). `Tabs.vue` emit change → `switchTab` async OK. |
| F13 | ECharts lazy (`VChartLazy.vue`) | ✅ **OK** | `defineAsyncComponent(() => import('vue-echarts'))` → chunk riêng 324KB gốc (lazy), không vào bundle chính. BenchmarkPanel import trực tiếp nhưng nằm trong view lazy theo router → vẫn tách chunk. Đúng NFR-5. |
| F14 | Tokens/dark mode/tailwind4 | ✅ **OK** | `class="dark"` + OKLCH; main.ts import thứ tự tokens→tailwind→palettes→global; global.css unlayered thắng preflight (ghi chú). Smoke light+dark 8/8 không overflow. |

### 2.6 Docs (G-F3a)

| # | Vùng | Verdict | Chi tiết |
|---|---|---|---|
| D1 | SDD §3.1/§3.8/§3.9/§8.1 | ✅ **OK** | Khớp code thật (cấu trúc ui/*, vite plugin @tailwindcss/vite, bundle thật, tokens OKLCH, font Geist/JetBrains). |
| D2 | THIRD_PARTY v1.2 (+19 gói) | ✅ **OK** | Version + license đối chiếu package.json khớp; ghi chú GSAP standard license + vaul-vue MIT từ GitHub. |
| D3 | NFR-5 (SRS §4.1 + TEST_PLAN TEST-PERF-007) | ✅ **OK** | Nới "tổng JS gốc tải lần đầu ≤1.5MB; engine ≤500KB gốc" khớp build thật. |
| D4 | REUSE_REPORT §6 | ✅ **OK** | 16 dòng ánh xạ component tự xây → shadcn-vue/vue-sonner/font, đúng thực tế. |
| D5 | `docs/pm-report-g.md` | ⚠️ **P3 — việc PM** | Decision log header trỏ `docs/pm-report-g.md` nhưng file **chưa tồn tại** (mới có pm-report-a..f). Việc cần PM tạo trước khi chốt milestone. |

---

## 3. Lệch docs cần ghi decision log

1. **`Value` = tổng Xp cho cả 3 tab** (tuần/lớp chưa có cột XP riêng) — đã ghi ở g-f3e.md §5 + comment code; nếu sau này cần "điểm lớp"/"XP tuần" riêng thì phải thêm cột dữ liệu + map riêng ở BE. Ghi vào decision log đợt sau.
2. **NFR-5 nới ngưỡng** — đã ghi tại mục [2026-08-12] G-3a docs. ✅
3. **`docs/pm-report-g.md` chưa tạo** — bổ sung vào decision log trạng thái việc cần làm.

---

## 4. Grep cấm + Secret (chi tiết)

- `git diff 085a82c^..HEAD -- backend/src frontend/src | grep -iE "postgresql|npgsql|mediatr|judge0"` → **0 match**.
- "Repository" → chỉ 3 comment "KHÔNG dùng Repository" (ADR-011/A-1) — cho phép.
- Secret: `sk-[20+]`, `ghp_`, `AKIA[16]`, `AIza`, private keys → **0 match**. `MOCK_ACCESS_TOKEN = 'e2e-access-token'` là mock test, không phải secret thật.
- Không có `.env` thật mới trong diff (chỉ tsconfig/vite.config build).

---

## 5. VERDICT

# ⚠️ CHANGES REQUESTED

**1 vấn đề P2 duy nhất chặn APPROVE:**

| ID | File:dòng | Mô tả | Mức |
|---|---|---|---|
| F2 | `frontend/src/views/LeaderboardView.vue:66-69` (hàm `goToPage`) | Tab Lớp phân trang không truyền classId → lớp >20 thành viên bấm trang 2 → backend 400 "Thiếu classId cho tab lớp" (cùng họ bug G-F3D-NEW-2 mà G-F3E chưa vá hết). Fix gợi ý: lưu `classId` trong store leaderboard (vd `lastClassId`) và `goToPage` dùng nó; thêm unit test pagination class tab. | **P2** |

**Các điểm P3 (không chặn, nên ghi decision log / làm sau):**
1. `GamificationService.PersistHeartRegenAsync` — race TOCTOU nhỏ (last-write-wins có thể hồi sinh 1 tim, hướng có lợi cho user); cải tiến: UPDATE điều kiện `WHERE Hearts < maxHearts` hoặc `Hearts + regenCount`.
2. `SubmissionLockRegistry` — ConcurrentDictionary không dọn key (rò rỉ nhỏ phiên dài).
3. Cookie `Secure = Request.IsHttps` — cần ForwardedHeaders nếu deploy sau proxy TLS.
4. `docs/pm-report-g.md` chưa tạo (việc PM).
5. Seed dữ liệu thật cho leaderboard tuần/lớp (đã ghi từ G-F3D).

**Đánh giá tổng thể:** Chất lượng code tốt, test phủ đúng (FE 76 unit + 13 e2e, BE 60 unit + 31 integration, build 0 lỗi cả 2 phía), grep cấm sạch, docs đồng bộ thực tế. Chỉ 1 lỗ hổng nhỏ còn lại ở phân trang tab Lớp — sửa trong 15 phút là APPROVE được.

---

## 6. KẾT LUẬN SAU FIX (P2 → APPROVE) — 2026-08-13

### 6.1 Fix P2 đã được xác minh (commit `d753e2e`, merged dev `17be8a7`)

Đã review diff `d753e2e` (4 file: store + view + spec + doc `g-f3e2.md`) và đối chiếu backend:

| Kiểm tra | Kết quả |
|---|---|
| Fix đúng gốc | ✅ Backend `GamificationService.cs:416-420` xác nhận 400 `VALIDATION_FAILED "Thiếu classId cho tab lớp"` khi `tab=class` + `classId=null` — đúng gốc bug. Fix lưu `lastClassId` trong store (`fetchBoard` lưu khi nhận classId; `tab=class` không truyền → fallback `classId ?? lastClassId ?? undefined`); `goToPage` (`LeaderboardView.vue:66-71`) truyền `board.lastClassId` khi `tab==='class'`. |
| Không phá week/level | ✅ `effectiveClassId` chỉ áp dụng khi `tab==='class'`; week/level luôn gửi `classId: undefined` (unit test #6 xác nhận). |
| Reset đúng | ✅ `setNoClass()` reset `lastClassId=null` → EmptyState không còn classId rác. |
| Không đổi backend | ✅ Fix hoàn toàn phía FE, contract BE giữ nguyên. |
| Unit test | ✅ `npm test` **78/78 PASS** (leaderboard spec 4 → 6 test, +2 test pagination class giữ classId / week không gửi classId). |
| E2E | ✅ `npx playwright test` **13/13 PASS** (không phá 2 test leaderboard cũ). |
| Build | ✅ `npm run build` (vue-tsc + vite) **0 lỗi**. |
| Merge sạch | ✅ `git diff 70113f5..17be8a7` = đúng 4 file fix, không thay đổi ngoài phạm vi. |

### 6.2 P3 đã ghi decision log (KHÔNG chặn merge)

Các điểm P3 từ §5 không chặn — đã ghi nhận trong chính doc này (§5) + g-f3e2.md §5; đề nghị PM bổ sung vào `docs/pm-decision-log-g.md` (mục [2026-08-12] G-3a) khi chốt milestone. Không nằm trong scope chặn merge:

1. `PersistHeartRegenAsync` — race TOCTOU nhỏ (last-write-wins có thể hồi sinh 1 tim, hướng có lợi cho user); cải tiến đề xuất: UPDATE điều kiện `WHERE Hearts < maxHearts`.
2. `SubmissionLockRegistry` — ConcurrentDictionary không dọn key (rò rỉ nhỏ phiên dài).
3. Cookie `Secure = Request.IsHttps` — cần ForwardedHeaders nếu deploy sau proxy TLS.
4. `docs/pm-report-g.md` chưa tạo (việc PM — decision log header đã trỏ file này).
5. Seed dữ liệu thật cho leaderboard tuần/lớp (đã ghi từ G-F3D).
6. B7 — `Value` = tổng Xp cho cả 3 tab (tuần/lớp chưa có cột XP riêng) — hành vi cũ có chủ đích, đã document.

### 6.3 VERDICT CUỐI

# ✅ APPROVE

P2 duy nhất (F2 — phân trang tab Lớp mất classId → 400) đã được fix đúng gốc tại `d753e2e` (merged dev `17be8a7`): store giữ `lastClassId`, `goToPage` tái dùng khi tab=class, week/level không đổi, `setNoClass` reset sạch. Verify thật: build 0 lỗi, unit 78/78, e2e 13/13. Không còn vấn đề P1/P2. Các P3 đã ghi decision log, không chặn merge.

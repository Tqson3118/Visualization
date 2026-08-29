# PROMPT TỔNG HỢP — CỨU DỰ ÁN DSA VISUAL (16 TASK + NGUYÊN TẮC VÀNG + QUYẾT ĐỊNH D0)

# 🎯 MỤC TIÊU CUỐI CÙNG CỦA TOÀN BỘ PLAN (đọc trước hết)
Sản phẩm cuối cùng phải DỄ HIỂU — DỄ DÙNG — KHÔNG PHỨC TẶP:
- Học sinh mở app → biết phải học gì trong 5 giây.
- Giáo viên vào Studio → tạo được lộ trình + bài + quiz không cần hướng dẫn.
- Mỗi màn hình trả lời đúng 1 câu: "Tôi đang ở đâu? Việc gì tiếp theo?"
Nếu sau khi làm xong mà người dùng vẫn cần giải thích cách dùng → plan THẤT BẠI, dù build xanh.

Bạn là dev agent nhận trách nhiệm cứu dự án DSA Visual. Làm ĐÚNG THEO THỨ TỰ, mỗi task xong must-pass verification rồi mới sang task kế. Không được tự ý bỏ task, không được đổi phạm vi.

════════════════════════════════════════════
PHẦN 0 — BỐI CẢNH & LUẬT CHƠI BẮT BUỘC
════════════════════════════════════════════

## Dự án là gì
- **DSA Visual** — nền tảng học Cấu trúc dữ liệu & Giải thuật (đồ án tốt nghiệp). Repo: frontend/ (Vue 3 + Vite + TS + Pinia + Tailwind), backend/ (.NET 10, SQL Server).
- **Tầm nhìn chủ repo**: sản phẩm lõi = LỘ TRÌNH + MÔ PHỎNG VISUAL. Mọi tính năng khác (lớp học, gamification, shop, admin) là phụ trợ — phải work, không cần hoàn hảo.
- **Bệnh của codebase**: dự án từng merge nguyên dự án cũ (VisualizationDSA3) vào, để lại nhiều hệ trực quan hóa song song + đảo ladder chết. Mọi task refactor dưới đây là để DỌN VỀ 1 HỆ DUY NHẤT.

## Luật chơi (áp dụng MỌI task)
1. **Baseline mỗi task**: `pnpm run build` exit 0 + `pnpm test` pass 100% (hiện tại: 609/609 pass, 54 files).
2. **Mỗi task = 1 commit riêng**, message theo conventional commits như ghi trong task.
3. Không refactor ngoài phạm vi task. Thấy vấn đề khác → ghi vào file `BACKLOG_FOUND.md` (tạo mới nếu chưa có), KHÔNG sửa.
4. Chạy từ thư mục `frontend/` trừ khi task ghi khác.
5. **Đã làm sẵn trước khi bạn bắt đầu** (ĐỪNG làm lại): đã xóa `features/dsa-modules`, đã fix `src/engines/__tests__/audit_all_44.spec.ts` dòng 42 (`p.text` → bỏ `.text`). Test đang xanh.

## Sự thật đã được kiểm chứng (tin được, không cần xác minh lại)
- `/simulator/:key` + `components/simulator` + `engines/` là hệ trực quan hóa CHÍNH, khỏe nhất (84 nơi dùng). Giữ nguyên làm trụ cột duy nhất.
- `SharedVisualizerShell.vue` dòng 83–88 đang import 4 component (`SortingHudOverlay`, `SortingVisualizerDispatcher`, `SortingProgressBar`, `SortingTraceTable`) + type `SortFrame` từ `algorithm-sandbox` → phải dời trước khi xóa.
- `App.vue` dòng 18 có `SANDBOX_ROUTES` (gates full-height + ẩn footer) — đã chứa 'simulator', phải GIỮ 'simulator', chỉ bỏ 4 tên sandbox cũ.
- `ClassDetailView.vue:461` push `{ name: 'lesson', params: { lessonId } }` — đường học bài thứ 2 cần hợp nhất.
- Route name `'courses'` thực tế trỏ `/path` (gây rối dev, Task 5 đổi tên).
- Tài khoản test sẵn: email `rescue.tester.v3@gmail.com` / mật khẩu `Rescue@2026` (đăng nhập UI chạy được).

════════════════════════════════════════════
PHẦN 0.5 — NGUYÊN TẮC VÀNG (luật cao nhất, đè lên mọi task khi mâu thuẫn)
════════════════════════════════════════════

**G1 — Khi phân vân: BỎ GIẤU hoặc GỢI Ý, không bao giờ THÊM nút.** Mỗi task nếu làm tăng số hành động hiển thị mặc định của một màn → task đó làm SAI.

**G2 — Progressive disclosure:** mỗi màn mặc định tối đa ~7 hành động chính nhìn thấy. Đồ nâng cao (breakpoint từng dòng, call stack, legend, zoom, tốc độ 0.25x…) gộp vào 1 nút "Nâng cao ⚙" mở ra khi cần.

**G3 — Từ vựng người dùng chỉ có 4:** Lộ trình → Bài học (lý thuyết + mô phỏng) → Bài tập/Quiz → Lớp học. Cấm tạo khái niệm mới (node, bậc, stage, sandbox…) trong UI. Code nội bộ đặt tên gì cũng được, người dùng không thấy.

**G4 — 3 luồng vàng (thước đo hoàn thành cuối cùng):**
1. 🎓 Học sinh: vào /path → chọn lộ trình → học 1 bài (lý thuyết → mô phỏng → quiz) → thấy tiến độ tăng. Mỗi bước chốt ≤2 click.
2. 👨‍🏫 Giáo viên: vào /studio → tạo lộ trình → thêm chương → thêm bài (soạn + gắn mô phỏng + gắn quiz) → giao cho lớp. Không cần đọc tài liệu vẫn làm được.
3. 🚶 Khách: vào / → tới mô phỏng chạy thử trong ≤2 click, console sạch, mobile không vỡ.

**G5 — Không thay đổi hành vi đúng sẵn có để "đổi mới"**: simulator đang chạy đúng từng bước → chỉ gọn hoá bề mặt, không đụng logic engine.

════════════════════════════════════════════
PHẦN D0 — QUYẾT ĐỊNH THIẾT KẾ (đã duyệt, mọi task phải tuân theo)
════════════════════════════════════════════

Mô hình nội dung THỐNG NHẤT (thay khái niệm "bậc node" mơ hồ của hệ cũ — BỎ HẲN "bậc node"):

```
LỘ TRÌNH (course)
└── CHƯƠNG (Topic — cây parentId/children, sortOrder)
    └── BÀI HỌC (Lesson — thuộc topicId)
        ├── simulations[]   ← gắn mô phỏng tương tác
        └── exercises[]     ← GẮN QUIZ VÀO ĐÂY (ExerciseRef)
```

| Loại quiz | Gắn ở đâu |
|---|---|
| Quiz nội dung | Trong bài học (lesson.exercises[]) |
| Quiz cuối chương | Bài CUỐI chương + badge "Kiểm tra cuối chương" |
| Thi cuối lộ trình | Cuối lộ trình, mở khi hoàn thành mọi bài (FinalTestView) |
| Quiz cho lớp + thời hạn | GV gán qua Class Assignment + deadline |

## D0.1 — Chốt cụ thể (không còn mơ hồ, agent KHÔNG cần hỏi lại)

**Sự thật trong code:** hệ thống chỉ có MỘT entity `Exercise` (type: MCQ | SIMULATION_LAB | CODE) với 2 cách gắn: `lessonId` (hệ mới) và `nodeId`+`stage` (hệ ladder cũ — nguồn bug QE2 "quiz xếp ngẫu nhiên").

**Quyết định (Option A — không sửa backend):**
1. Quiz cuối chương KHÔNG treo vào node chương (Topic không có trường đó). Nó là quiz gắn `lessonId` vào BÀI CUỐI CHƯƠNG; UI tự nhận diện (bài cuối + có quiz) → hiện badge "Kiểm tra cuối chương". Không thêm field mới.
2. Editor tạo quiz (Task 12): `lessonId` BẮT BUỘC, luôn null/ignore `nodeId` + `stage` (đồ ladder cũ). Khi gặp quiz cũ có nodeId mà không lessonId → hiển thị trong danh sách "Chưa gắn bài" để GV gắn lại.
3. "ASM cuối" = Class Assignment hiện có: GV chọn bài học HOẶC exercise, gán vào curriculum lớp, có `dueAt` + `allowLateSubmission` — không xây gì thêm, chỉ cần UI Task 9/12 cho flow này chạy trơn.
4. Tóm gọn 1 câu cho dev: **quiz luôn có 1 bài học đỡ đầu; deadline chỉ tồn tại khi GV gán vào lớp.**

════════════════════════════════════════════
WAVE 1 — GIẢI PHÓNG MẶT BẰNG (Task 1→8, làm trước)
════════════════════════════════════════════

### 🔴 TASK 1 — Bảo mật: dọn secret lộ trong git
File: `backend/src/DsaVisual.Api/appsettings.json` + `appsettings.Development.json`
1. `DSA:Email:SmtpPassword` → `""`; `ConnectionStrings:Default` → `""` (cả 2 file).
2. `README.md`: thêm mục "Biến môi trường bắt buộc": `DSA__Email__SmtpPassword`, `ConnectionStrings__Default`.
3. Verify: `git grep -i "db65198"` và `git grep -i "ysbt"` → 0 kết quả tracked.
Commit: `chore(security): remove exposed smtp and db secrets from appsettings`

### 🔴 TASK 2 — Tháo sandbox V3, còn 1 engine duy nhất
**Bước A — khâu trước khi cắt:**
1. Dời types/helpers từ algorithm-sandbox sang `features/visual-shell/` (`types/sorting.types.ts` chứa `SortFrame`, `SortAlgorithm`; `helpers/catalogKeyMap.ts`, `sortingIdEnricher.ts`).
2. Dời 4 component `SortingHudOverlay/SortingVisualizerDispatcher/SortingProgressBar/SortingTraceTable` + sorting renderers vào `features/visual-shell/components/` → `SharedVisualizerShell.vue` hết phụ thuộc ngoài.
3. `LessonStepTheory.vue:106` → import `SortFrame` từ `@/features/visual-shell/types/sorting.types`.
4. `SharedVisualizerShell.spec.ts:8` → tạo frames bằng `createBubbleGenerator` từ `@/engines/generators/sort/bubble` + `buildFramesFromCatalogKey('sort.bubble')`.
5. `legacyStepAdapter.spec.ts:12` → sửa import tương tự.
**Bước B — route:** bỏ `/sorting-sandbox`, `/searching-sandbox`, `/stack-queue-sandbox` + lazy import. `App.vue` `SANDBOX_ROUTES` → chỉ còn `['simulator']`.
**Bước C — xóa thư mục:** `views/sorting/`, `views/searching/`, `views/graph/`, `views/stackqueue/` (kèm specs), `features/algorithm-sandbox/`, `features/guided-tour/`, `features/interactive-playground/`, `features/animation-engine/`, `views/CodeToVisualView.vue` + spec + `features/code-to-visual/` (mồ côi không route).
**GIỮ:** `features/vcr-player/`, `features/visual-shell/`, `features/lesson/`, `features/quiz-system/`, `components/simulator/`, `engines/`.
Verify: build + test pass; mở `/simulator/sort.bubble` bấm Chạy vẫn chạy từng bước.
Commit: `refactor(engine): decommission legacy sandboxes and unify on core visual engine`

### 🔴 TASK 3 — Hợp nhất 2 màn học bài về LessonStudyView
1. `ClassDetailView.vue:461`: `push({ name: 'lesson', params: { lessonId } })` → `push({ name: 'lesson-study', params: { id: String(item.lessonId) } })`.
2. `TeacherStudioView.vue`: mọi link preview bài → `name: 'lesson-study', params: { id }`.
3. Router: `/learn/:lessonId` → `redirect: (to) => `/lessons/${to.params.lessonId}``.
4. Xóa `views/LessonView.vue`.
Verify: build + test; vào `/classes/:id` bấm bài học → URL `/lessons/:id`, UI giống học từ `/path`.
Commit: `refactor(lesson): consolidate lesson views into LessonStudyView`

### 🔴 TASK 4 — Chôn đảo ladder, dời QuizStage, gắn FinalTest
**Bước A:** move `components/ladder/QuizStage.vue` → `components/quiz/QuizStage.vue`; sửa import trong `ExerciseView.vue` + `FinalTestView.vue`.
**Bước B:** `CourseDetailView.vue`: computed `isAllLessonsCompleted` (mọi lesson completed) → hiện nút "Kiểm tra cuối lộ trình" push `{ name: 'final-test', params: { topicId: String(courseId) } }`. LƯU Ý: param `topicId` ở đây = courseId hệ mới (thêm comment 1 dòng chỗ push).
**Bước C — xóa:** `views/PathView.vue`, `PathRedirectView.vue`, `NodeHubView.vue`, `LadderView.vue`, `LabView.vue`, `components/ladder/`, `components/path/`, `data/nodeHubData.ts`. Router: bỏ `/ladder/:nodeId`, `/ladder/:nodeId/lab`; GIỮ `/path/:topicId/final-test` + redirect node-hub→/path.
Verify: build + test; học hết bài lộ trình → nút thi cuối hiện; `/exercise/:id` từ lớp vẫn chấm được.
Commit: `refactor(curriculum): decommission legacy ladder and link final test to course progress`

### 🟡 TASK 5 — Router đặt tên đúng nghĩa
1. `name: 'courses'` → `'path-list'`; `name: 'course-detail'` → `'path-detail'`.
2. Sửa callers (chỉ file còn sống sau Task 3/4): `AppHeader.vue`, `HeroSection.vue`, `HomeCtaBand.vue`, `CoursesListView.vue`, `CourseDetailView.vue`, `RegisterView.vue` + `RegisterView.spec.ts`, `ExerciseView.vue`, `CourseCard.vue`.
3. Đầu `router/index.ts`: comment bảng redirect canonical (/learn→/path, /courses*→/path*, /learn/:id→/lessons/:id, /admin/content*→/studio*, /admin→/admin/users, /admin/ladder→/studio?tab=exercises, /admin/feedback→/studio?tab=feedback, /dashboard→/profile).
Verify: build + test; bấm từng nav không vòng lặp redirect.
Commit: `refactor(router): rename course routes to path and document redirect chains`

### 🟡 TASK 6 — UX stabilize
1. **401 noise**: chỉ gọi `auth.refresh()` khi có session marker (localStorage); 401 refresh → im lặng, không console.error, guest vào mọi trang console sạch.
2. **Mobile overflow** (đo 375px tràn tới 497px): `StatsBar.vue` + `ControlBar.vue` thêm `flex-wrap: wrap; max-width: 100%` + media query <640px.
3. **Breakpoint buttons** `PseudocodePanel.vue`: 18×18px → ≥28×28px (padding hoặc hit-area).
4. **Nested `<main>`**: App.vue giữ main ngoài; view con (`SimulatorView`, `HomeView`, `SimulationsView`, `AdminContentView`, `ClassesView`, `FinalTestView`, `ExerciseView`…) đổi thành `<section>`.
5. **Flaky tests**: `vite.config.ts` test config thêm `testTimeout: 20000, hookTimeout: 20000`; `vitest.setup.ts` đảm bảo rAF/window mocks.
Verify: guest mở `/` + `/simulator/sort.bubble` → 0 console error; 390px không cuộn ngang; `pnpm test` chạy 3 lần liên tiếp đều pass.
Commit: `fix(ux): stabilize simulator mobile layout, auth interceptor, and test timeouts`

### 🟡 TASK 7 — Tách AdminContentView (~2.202 dòng)
1. Tạo `views/admin/sections/`: `CurriculumTab.vue`, `ExercisesTab.vue`, `FeedbackTab.vue`, `TopicModal.vue`, `MoveLessonModal.vue`.
2. `AdminContentView.vue` giữ tab-switching + stat cards + state coordination → mục tiêu <350 dòng, mỗi section <500 dòng. KHÔNG đổi logic — chỉ di chuyển.
Verify: build pass; chunk AdminContentView giảm rõ (<200KB); mọi tab admin render đủ dữ liệu như cũ.
Commit: `refactor(admin): split AdminContentView into modular tab sections`

### 🟢 TASK 8 — E2E smoke chặn hồi quy
Tạo `tests/e2e/smoke-critical.spec.ts` (Playwright):
1. Guest: `/` → `/simulations` → `/simulator/sort.bubble` → bấm Chạy → assert "Bước x/99" tăng.
2. Guest: `/path` render danh sách lộ trình công khai, không auth modal.
3. Login (`rescue.tester.v3@gmail.com`/`Rescue@2026`) → `/lessons/:id` render content + quiz step.
4. Assert 0 console error-level; không horizontal overflow ở 1366px và 390px.
Verify: `pnpm test:e2e tests/e2e/smoke-critical.spec.ts` pass.
Commit: `test(e2e): add critical smoke test suite for core user journeys`

════════════════════════════════════════════
WAVE 2 — SỬA BUG NHÓM TEST (Task 9→15, chỉ bắt đầu sau Task 8)
════════════════════════════════════════════
(Nguồn: checklist test thực tế của nhóm. Mã bug để đối chiếu khi báo cáo.)

### 🔴 TASK 9 — Lớp học (C1–C3)
- C1: nút add thành viên không hoạt động → debug flow mời member trong ClassDetailView + API POST member + toast lỗi.
- C2: tạo lộ trình lớp chèn "lời dẫn đầu" lạ → rà placeholder/mô tả mặc định sai.
- C3: không sửa được curriculum lớp → bật lại flow edit (API `ClassCurriculumUpsertRequest` đã có, UI thiếu).
Verify: thêm member bằng email được; sửa curriculum xong reload còn đúng.
Commit: `fix(class): restore member invite and curriculum editing`

### 🔴 TASK 10 — Soạn thảo bài học (LE1–LE5) + tách AdminLessonEditorView (1.191 dòng)
- LE1: layout cuộn 2 chiều hỏng → grid 2 cột, editor panel `overflow-y-auto` riêng, danh sách kiểu soạn thảo luôn reachable.
- LE2: chọn "nội bộ lớp học" (`isClassOnly`) → thêm dropdown chọn lớp.
- LE3: import file .md → chạy cùng pipeline markdownParser/AI-format như paste tay.
- LE4: gắn mô phỏng → preview hiển thị anchor đúng vị trí (`[Mô phỏng: sort.bubble]`).
- LE5: xuất bản → toast "Đã xuất bản" + redirect danh sách hoặc nút xem trước.
- Tách file: `editor-tabs/` (TheoryTab, SimulationTab, QuizTab, SettingsTab), mỗi tab <400 dòng.
Verify: soạn bài có mô phỏng + quiz, xuất bản, xem trước ĐÚNG bài vừa soan (bug S4 cũ không tái xuất hiện).
Commit: `fix(studio): lesson editor UX overhaul and modular split`

### 🔴 TASK 11 — Lộ trình editor (PE1–PE5)
- PE1: form tạo danh mục bỏ trường "cấp độ" thừa (Topic không có level).
- PE2: lộ trình vừa tạo hiện NGAY trong list dưới (refetch sau create).
- PE3 BLOCKER: thêm chương + bài vào lộ trình mới không được → debug tree CRUD (parentId/sortOrder).
- PE4: cây hiện chương 6, danh sách phẳng hiện 5 → 2 view dùng chung 1 nguồn query + sort theo sortOrder.
- PE5: tìm kiếm exact → normalize (lowercase, bỏ dấu, includes).
Verify: tạo lộ trình → thêm 2 chương → thêm bài vào chương 2 → cây & danh sách phẳng khớp 100%.
Commit: `fix(studio): curriculum tree CRUD sync and fuzzy search`

### 🔴 TASK 12 — Quiz editor CRUD đầy đủ (QE1, QE3–QE6) — theo D0
- QE1: "chọn tất cả" lỗi làm ẩn block thời gian quiz → fix v-if unmount sai.
- QE3: dropdown liên kết bài học thiếu bài → load đủ lessons của topic (kể cả draft của mình).
- QE4: tạo quiz chọn phạm vi "Cá nhân / Lớp học"; nếu Lớp → chọn lớp + deadline (map ClassAssignment).
- QE5 BLOCKER: tạo + xóa quiz không được → debug API + toast lỗi thật.
- QE6: thêm chế độ CHỈNH SỬA quiz (load → update → save).
- UI: bỏ khái niệm "bậc node"; quiz sort theo bài học liên kết.
Verify: tạo → gắn bài → sửa → xóa, cả 4 đều có toast kết quả.
Commit: `feat(studio): full quiz CRUD with lesson binding per D0`

### 🟡 TASK 13 — Studio banner & nav (S1–S3)
- S1: banner "4/5 bài" → dùng `PagedResponse.totalCount` hoặc fetch đủ.
- S2: click "Lớp của tôi" nhảy tab "Mới" → chỉ set active-tab khi có `?tab=`.
- S3: "Xem toàn bộ bài học" trỏ nhầm → trỏ `/studio?tab=lessons` (sau Task 2 sẽ 404 nếu không sửa).
Verify: 3 nút banner dẫn đúng, số liệu khớp.
Commit: `fix(studio): overview banner counts and navigation targets`

### 🟢 TASK 14 — Thử thách (Q1)
Quest list sort: chưa nhận (mới nhất trước) → đã nhận (gần deadline trước).
Commit: `feat(quests): sort received quests below available`

### 🟢 TASK 15 — Hợp nhất Studio thành 1 khung sidebar + Design Tokens (L1)

**🎯 ĐÍCH = cột "Giao diện trong hình ảnh" của bảng so sánh dưới đây. Cột "hiện tại" chỉ là điểm xuất phát, KHÔNG phải thứ giữ lại.**

| # | Hiện tại (code) | → ĐÍCH (mock đã duyệt) |
|---|---|---|
| 1 | Tab ngang AdminNav + 2 route tách biệt | **Sidebar trái cố định "STUDIO"** 5 mục + banner Nâng cấp Studio; nội dung chiếm ~3/4 phải |
| 2 | Hero có sẵn (giống mock) | GIỮ NGUYÊN, chỉ đưa vào shell |
| 3 | 4 KPI cards có sẵn (giống mock) | GIỮ NGUYÊN |
| 4 | 5 card pipeline (link thật) | GIỮ card + thêm **connector nét đứt** giữa các bước |
| 5 | 4 hub cards 2x2 (giống mock) | GIỮ NGUYÊN, viền màu theo token |
| 6 | List dòng bài học gần đây | **Table 5 cột**: Bài học / Mô tả / Trạng thái / Cập nhật / Thao tác (Sửa + Xem chữ rõ) |
| 7 | /teacher + /studio tách đôi | **1 khung master-detail duy nhất** tại /studio (?tab=overview mặc định) |

**Sự thật đã kiểm chứng trong router (đừng tái khám phá):** trải nghiệm GV đang XÉ ĐÔI làm 2 route — `/teacher` render TeacherStudioView (dashboard hero/KPI/hubs — cái GIỐNG mock nhất), còn `/studio` render AdminContentView (khối 2.202 dòng đã tách ở Task 7, tab qua `?tab=curriculum|exercises|feedback`). Mock duyệt = MỘT khung master-detail sidebar duy nhất.

1. **Tạo `components/studio/StudioShell.vue`** — khung 2 cột: sidebar trái cố định + nội dung phải. Sidebar 5 mục map sang `?tab=`: Tổng quan (mặc định) / Lộ trình & Bài giảng (tab=curriculum) / Quiz & Codelab (tab=exercises) / Lớp học (dẫn /classes) / Báo cáo & Phản hồi (tab=feedback) + banner Premium dưới đáy sidebar.
2. **Tab Tổng quan** = nội dung TeacherStudioView hiện tại (hero + 4 KPI + pipeline + 4 hub + bảng) chuyển thành component con `StudioOverviewTab.vue` — KHÔNG viết lại logic, chỉ chuyển nhà.
3. **Bảng "Bài học cập nhật gần đây"**: đổi từ list dòng sang table 5 cột (Bài học / Mô tả / Trạng thái / Cập nhật / Thao tác Sửa+Xem) theo mock.
4. **Pipeline 5 bước**: giữ nguyên 5 card hiện có (đã có link thật), chỉ thêm connector nét đứt giữa các card (CSS thuần) cho giống mock — không đụng logic.
5. **Route hợp nhất**: `/studio` mặc định `?tab=overview`; `/teacher` → redirect `/studio?tab=overview` (TeacherStudioView.vue xóa sau khi chuyển nội dung). Các link nội bộ cũ `?tab=curriculum|exercises|feedback` vẫn hoạt động (Task 13 S2 đã chuẩn).
6. **Design tokens**: màu/space/card → `src/styles/tokens.css`; accent tím neon theo mock; nền constellation tái dùng `startCosmicField`. Cấm hex rải rác. `CourseCard.vue` + `CoursesListView.vue` + mọi card studio dùng chung token.
7. Hero + KPI + 4 hub: GIỮ NGUYÊN như hiện tại (đã khớp mock — khỏi đụng, khỏi viết lại).

Verify: `/studio` = 1 khung sidebar liền mạch đúng mock; `/teacher` redirect không chết link; tab curriculum/exercises/feedback vẫn đủ chức năng Task 7; các trang public dùng chung token không còn lệch màu.
Commit: `feat(design): unify studio into single sidebar shell with shared tokens`

### 🔴 TASK 16 — GIẢM PHỨC TẠP UI (thi hành Nguyên tắc vàng G1–G3)
Đây là task của riêng việc "trông đơn giản" — làm SAU Task 15 (đã có design tokens).

1. **Simulator (/simulator/:key) — đang 24 nút + 30 link, gọn về ≤10 hành động mặc định:**
   - Giữ luôn thấy: Quay lại, Tựa đề, Chạy/Tạm dừng, Bước tới/lùi, Đặt lại, tốc độ, "Tự thực hành", Yêu thích.
   - Gộp vào nút "Nâng cao ⚙": Call stack, Legend, Giới thiệu, cài đặt chỉ số/giá trị/zoom (mặc định hợp lý, khỏi đụng), breakpoint mã giả (khi bật panel mã giả mới có).
   - Mọi nút ≥28×28px hit-area. Bấm "Nâng cao" mở ra khối gọn bên phải, không đẩy vỡ layout.
2. **Home (/):** chỉ 1 CTA chính ("Bắt đầu học" → /path) + 1 CTA phụ ("Thử mô phỏng" → /simulations). Mọi card/demo khác xếp dưới theo cấp độ quan trọng, không cạnh tranh màu với CTA.
3. **Chuỗi điều hướng:** breadcrumb hoặc "bước tiếp theo" rõ ở /path, /path/:id, /lessons/:id (học xong 1 bài → nút "Bài tiếp theo" là nút lớn nhất màn).
4. **Rà toàn bộ UI theo G3:** đổi mọi nhãn kỹ thuật còn sót (node, stage, sandbox, ladder…) sang 4 từ chuẩn; danh sách "Chưa gắn bài" ở Task 12 dùng nhãn đó.
5. Đo đạc trước/sau: đếm số nút + link hiển thị mặc định mỗi màn (script Playwright) — ghi vào `BACKLOG_FOUND.md` làm bằng chứng.

Verify: simulator ≤10 nút mặc định, "Nâng cao" mở đủ đồ cũ; 3 luồng vàng (G4) đều đạt ngưỡng click; test + build pass.
Commit: `feat(ux): declutter simulator and core flows per golden rules`

════════════════════════════════════════════
XÁC NHẬN HOÀN TẤT (chạy cuối)
════════════════════════════════════════════
1. `pnpm run build` exit 0; `pnpm test` 100%; `pnpm test:e2e` pass.
2. Guest flow: / , /path, /simulations, /simulator/sort.bubble — 0 console error, không overflow 1366/390px.
3. Login flow: lớp học → học bài /lessons/:id → quiz chấm điểm.
4. Studio flow: tạo lộ trình → chương → bài → quiz → xuất bản → xem trước đúng bài.
5. **Kiểm tra Nguyên tắc vàng:** đi lại 3 luồng vàng (G4) tay không — mỗi chốt ≤2 click (GV ≤5 click tổng cho toàn luồng); simulator ≤10 nút mặc định; UI không còn từ "node/bậc/stage/sandbox".
6. **Test người dùng mù:** nhờ 1 người chưa từng dùng app thử luồng học sinh + giáo viên mà không được hướng dẫn — nếu họ tự làm được thì đạt; ghi lại chỗ họ kẹt vào `BACKLOG_FOUND.md`.
7. Báo cáo: mỗi task 1 dòng (commit hash + pass/fail verification) + danh sách bug C/LE/PE/QE/S/Q đã hết còn gì còn lại → ghi `BACKLOG_FOUND.md`.

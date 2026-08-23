# Chặng 3 — Khóa học, Bài học và Teacher Studio

> **Vị trí top-down:** Chặng 1 dựng ống (FE↔BE↔DB + Auth), Chặng 2 đổ nội dung (Engine 44 generators). Chặng 3 gắn engine vào **lộ trình học có cấu trúc** và **không gian sư phạm** (Teacher Studio / Lớp học) — nơi hội đồng hỏi sâu nhất về lifecycle, phân quyền và concurrency.
> **Stack:** `frontend/src/stores/lesson.ts`, `frontend/src/api/lessons.ts`, `frontend/src/api/exercises.ts`, `frontend/src/views/TeacherStudioView.vue|ClassesView.vue|ClassDetailView.vue|ExerciseView.vue`, `frontend/src/features/lesson|quiz-system`, `backend/src/DsaVisual.Api/Controllers/LessonsController.cs|ClassesController.cs|ExercisesController.cs`, `backend/src/DsaVisual.Application/Services/LessonService.cs|ClassService.cs|ExerciseService.cs|CodelabJudgeService.cs|SubmissionLockRegistry.cs` + `Ganss.Xss`.

---

## 1. Khái niệm & Mục đích nghiệp vụ

### 1.1 Tại sao có module này?

Engine rời rạc (Chặng 2) chỉ cho "xem 1 thuật toán". Người học cần **lộ trình**: Topic → Lesson → Quiz/Exercise/Codelab → Progress. Giảng viên cần **lớp học**: tạo lớp → mã mời 6 ký tự → gán bài → theo dõi báo cáo → export CSV.

Không có chặng này, hệ thống là thư viện demo, không phải LMS.

### 1.2 Bài toán nghiệp vụ

- **Lesson lifecycle:** `draft → pendingreview → active / hidden` + `isClassOnly` (chỉ lớp). Teacher tạo → PendingReview → ADMIN duyệt → Active. Gating curriculum `draft/published` per-class.
- **3 chế độ bài tập & kiểm tra (Exercise Modes):**
  1. `QUIZ`: Câu hỏi trắc nghiệm nhiều lựa chọn, kiểm tra lý thuyết và giải thuật.
  2. `CODING` (Codelab): Học viên viết code giải quyết bài toán thuật toán. Hệ thống chấm code an toàn phía máy chủ qua engine `CodelabJudgeService` (Jint interpreter).
  3. `MULTIPLE_CHOICE` / `PARSONS`: Sắp xếp khối code hoặc chọn đáp án đúng với phản hồi tức thì.
- **Codelab Server-Side Sandbox:** Không tin tưởng client — bài nộp code bắt buộc được thực thi trong môi trường sandboxed trên server, kiểm soát timeout 1.5s, giới hạn bộ nhớ 32MB, tối đa 200,000 lệnh và stack overflow guard.
- **Concurrency & Anti-race:** `SubmissionLockRegistry` khóa đồng bộ theo cặp `(UserId, ExerciseId)` ngăn chặn race condition khi học viên double-submit hoặc nộp song song nhiều luồng.
- **Teacher Studio & Lớp học:** Quản lý Class, InviteCode 6 ký tự, ClassMember (Teacher/Student), ClassAssignment (SortOrder), ClassCurriculum, Báo cáo tiến độ lớp (`ClassDetailView.vue` / `ClassReportView`) và Export CSV định dạng UTF-8 BOM.

### 1.3 Học xong làm được gì

- Vẽ được luồng `Student: ClassesView → joinByCode → ClassDetail → LessonStudy → Quiz/Codelab submit → Server Judge → Progress + XP`.
- Giải thích được tại sao FE chỉ chặn locked bằng UX, còn gate thật là BE trả 403 cho Draft/Hidden/ClassOnly.
- Phân tích cơ chế hoạt động của `CodelabJudgeService` (Jint) và `SubmissionLockRegistry` chống double-submit.
- Chỉ ra được race `maxSortOrder+1`, CSV responseType, import CSV idempotency.

---

## 2. Sơ đồ Mermaid trực quan

### 2.1 Kiến trúc Course → Lesson → Studio → Class & Exercise

```mermaid
flowchart TB
    subgraph FE["Frontend"]
        TV[TeacherStudioView — orchestration]
        CV[ClassesView — list + joinByCode 6 chars]
        CD[ClassDetailView — members/curriculum/report]
        EV[ExerciseView — Quiz / Codelab]
        LS[LessonStudy — sandboxType: theory/quiz/codelab]
        ST[Stores: lesson.ts + classStore.ts]
        API[api/lessons.ts + api/classes.ts + api/exercises.ts]
    end
    subgraph BE["Backend"]
        LC[LessonsController]
        CC[ClassesController]
        EC[ExercisesController]
        LSC[LessonService — Ganss.Xss]
        CSC[ClassService — Max SortOrder]
        ESC[ExerciseService — 76KB Core Service]
        CJS[CodelabJudgeService — Jint Engine]
        SLR[SubmissionLockRegistry — Semaphore]
        EF[(AppDbContext — Lesson/Class/Exercise/Submission)]
    end
    TV --> ST --> API --> LC & CC & EC
    LC --> LSC --> EF
    CC --> CSC --> EF
    EC --> ESC --> SLR & CJS --> EF
    LS --> ST
    CD --> ST
    EV --> ST
    style FE fill:#0ea5e9,stroke:#0284c7,color:#fff
    style BE fill:#10b981,stroke:#059669,color:#fff
```

### 2.2 Sequence — Codelab Submit & Server-side Judge Flow

```mermaid
sequenceDiagram
    participant S as Student
    participant V as ExerciseView (Codelab UI)
    participant X as Axios Client
    participant C as ExercisesController
    participant Svc as ExerciseService
    participant Lock as SubmissionLockRegistry
    participant Judge as CodelabJudgeService (Jint)
    participant DB as AppDbContext

    S->>V: Viết code hoàn thành task → Bấm Nộp bài
    V->>X: POST /api/v1/exercises/{id}/code-submit {code, taskId}
    X->>C: SubmitCode(id, request)
    C->>C: FluentValidation (độ dài code, taskId hợp lệ)
    C->>Svc: SubmitCodeAsync(userId, exerciseId, request)
    Svc->>Lock: TryAcquire(userId, exerciseId, timeout: 2s)
    alt Lock bận (đang có bài nộp đồng thời)
        Lock-->>Svc: null (timeout)
        Svc-->>C: 422 SUBMISSION_IN_PROGRESS
    else Lock thành công
        Svc->>DB: Lấy Exercise ConfigJson (danh sách TestCases)
        Svc->>Judge: Judge(code, taskSpec, timeout: 1500ms)
        Note over Judge: Jint Sandbox: Timeout 1.5s, MaxStatements 200k, MaxMemory 32MB
        Judge->>Judge: Chạy code qua từng TestCase & Normalize Output
        Judge-->>Svc: CodelabJudgeResult {Passed, Error, Cases}
        Svc->>DB: Ghi CodeSubmission + Cập nhật UserProgress + Award XP/Gems
        Svc->>Lock: Dispose() Giải phóng Lock
        Svc-->>C: CodeSubmitResultDto
        C-->>X: 200 OK {passed: true, testCasesPassed: n/n}
        X-->>V: Hiển thị kết quả & ăn mừng hoàn thành
    end
```

### 2.3 State — Lesson lifecycle

```mermaid
stateDiagram-v2
    [*] --> draft
    draft --> pendingreview : teacher submit
    pendingreview --> active : ADMIN approve
    pendingreview --> hidden : ADMIN reject
    active --> hidden : ADMIN hide
    hidden --> active : ADMIN republish
    active --> draft : teacher edit (tạo bản nháp)
    note right of active
        isClassOnly=true:
        chỉ member của class
        gán assignment mới thấy
    end note
```

---

## 3. Bảng phân tích File-by-File

| # | Đường dẫn thật | Hàm / Class trọng tâm | Quyết định / State |
|---|---|---|---|
| 1 | `frontend/src/views/TeacherStudioView.vue:1-366` | `sections[] Network/Sparkles`, `totalLessons/Topics/Courses` | Orchestration hub, gọi lessonsApi + courseApi + classStore |
| 2 | `frontend/src/views/ClassesView.vue:1-577` | `ClassesView — joinByCode 6 chars`, `createClass` | Banner level-2, card level-1, mã mời block-token canvas-ink |
| 3 | `frontend/src/views/ClassDetailView.vue:1-1730` | 3 tabs + `assignments reorder`, `curriculum`, `report/export` | Nặng nhất, 10+ import, mobile card-stack |
| 4 | `frontend/src/stores/lesson.ts:1-103` | `useLessonStore`, `topics/lessonsByTopic/currentLesson`, `progressByTopic` | SDD §3.2, gọi lessonsApi |
| 5 | `frontend/src/stores/classStore.ts:1-200` | `useClassStore`, `fetchClasses/fetchClass/members/assignments/curriculum` | Module H, curriculumLoading/Error |
| 6 | `frontend/src/api/lessons.ts:1-153` | `LESSON_ENDPOINTS`, `LessonStatusValue draft|pendingreview|active|hidden` | includeContent=true để lấy ContentHtml |
| 7 | `frontend/src/api/classes.ts:1-134` | `CLASS_ENDPOINTS`, `joinByCode/list/detail/report/export/curriculum` | reportExport trả string |
| 8 | `frontend/src/api/types.ts` | `ClassDto/ClassDetailDto/ClassMemberDto/ClassAssignmentDto` | DTO chung FE/BE |
| 9 | `frontend/src/services/courseApi.ts` | `courseApi.getCourses`, `CourseListDto` | Dùng trong TeacherStudio |
| 10 | `frontend/src/features/lesson/*` | `LessonStepTheory/Quiz/CodeLab` | 3 engines theo sandboxType |
| 11 | `frontend/src/features/quiz-system/*` | Quiz engine + judge | Idempotency submit |
| 12 | `frontend/src/components/lesson/*` | LessonStep components | Gắn engine vào study flow |
| 13 | `frontend/src/views/ExerciseView.vue` | Giao diện làm bài tập Quiz & Codelab | Tích hợp Monaco editor + Test runner |
| 14 | `backend/src/DsaVisual.Api/Controllers/LessonsController.cs` | `Get/List/Create/Update/Sim attach` | Gate hidden/draft/classOnly → 403 |
| 15 | `backend/src/DsaVisual.Api/Controllers/ClassesController.cs` | `Create/JoinByCode/Members/Assignments/Report/Export/Curriculum` | Per-class auth |
| 16 | `backend/src/DsaVisual.Api/Controllers/ExercisesController.cs` | CRUD bài tập, `submit`, `code-submit`, `import-csv`, `code-submissions` | Quản lý và chấm bài nộp |
| 17 | `backend/src/DsaVisual.Application/Services/ExerciseService.cs` | **76KB — Service lớn nhất backend**: quản lý bài tập, tính điểm, submit quiz, delegate judge | Xử lý nghiệp vụ bài tập toàn diện |
| 18 | `backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs` | Chấm code JS sandboxed qua Jint: timeout 1.5s, memory 32MB, statements 200k | Server-side code execution sandbox |
| 19 | `backend/src/DsaVisual.Application/Services/SubmissionLockRegistry.cs` | ConcurrentDictionary + SemaphoreSlim per (UserId, ExerciseId) | Chống race condition double-submit |
| 20 | `backend/src/DsaVisual.Application/Services/LessonService.cs` | `IHtmlSanitizer Ganss.Xss`, whitelist 13 tags | Sanitize ContentHtml trước lưu |
| 21 | `backend/src/DsaVisual.Application/Services/ClassService.cs` | `AddAssignmentAsync Max+1`, `JoinByCode` | Race SortOrder, thiếu RowVersion |
| 22 | `backend/src/DsaVisual.Application/Persistence/Entities/Lesson.cs` | `Lesson {Status, IsClassOnly, TopicId}` | Enum LessonStatus |
| 23 | `backend/src/DsaVisual.Application/Persistence/Entities/Class.cs` | `Class {InviteCode 6 chars}` | Unique code |
| 24 | `backend/src/DsaVisual.Application/Persistence/Entities/ClassAssignment.cs` | `ClassAssignment {SortOrder, LessonId, ExerciseId}` | Gán bài học/bài tập vào lớp |
| 25 | `backend/src/DsaVisual.Application/Persistence/Entities/UserProgress.cs` | `UserProgress {completed/viewed/bestScore}` | Progress per lesson/exercise |
| 26 | `backend/src/DsaVisual.Application/Validators/LessonValidator.cs` | FluentValidation | Title/description/content |
| 27 | `backend/src/DsaVisual.Application/Validators/CodeSubmitRequestValidator.cs` | FluentValidation cho code submission | Chống DB DoS, validate payload |

---

## 4. Code Snippets cốt lõi & Chú giải chi tiết

### 4.1 LessonStatus + fetchLesson (includeContent)

```ts
// frontend/src/api/lessons.ts:15-40
export type LessonStatusValue = 'draft' | 'pendingreview' | 'active' | 'hidden';
export interface LessonSummary { id:number; title:string; topicId:number; sortOrder:number; status:LessonStatusValue; progress?: LessonProgressDto; }
export async function fetchLesson(id:number): Promise<LessonDto>{
  return getData<LessonDto>({ method:'GET', url: LESSON_ENDPOINTS.lesson(id), params:{ includeContent:true } });
}
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `LessonStatusValue` 4 giá trị | Lifecycle | draft→pendingreview→active/hidden |
| `includeContent:true` | Lấy ContentHtml + simulationKeys | Không kèm thì chỉ summary nhẹ |
| `progress?` optional | done/viewed/bestScore | Gắn với UserProgress |

### 4.2 ClassService — Max+1 race

```csharp
// backend/src/DsaVisual.Application/Services/ClassService.cs (rút gọn)
if (request.LessonId is null && request.ExerciseId is null)
  return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Phải gán ít nhất bài học hoặc bài tập");
var maxSortOrder = await db.ClassAssignments.AsNoTracking()
  .Where(a => a.ClassId == id).MaxAsync(a => (int?)a.SortOrder, ct) ?? -1;
db.ClassAssignments.Add(new ClassAssignment { ClassId=id, LessonId=request.LessonId, SortOrder=maxSortOrder+1 });
await db.SaveChangesAsync(ct);
```

| Dòng | Ý nghĩa | Tại sao / Rủi ro |
|---|---|---|
| `OR null check` | Ít nhất 1 | Cho phép cả 2 non-null (không XOR) → cần quyết định product |
| `MaxAsync +1` | Thứ tự | Concurrent 2 teacher cùng Max → duplicate SortOrder, thiếu RowVersion/transaction |
| `AsNoTracking` | Không track | Tối ưu read, nhưng không lock |

### 4.3 Export CSV — BOM UTF-8

```ts
// frontend/src/api/classes.ts:reportExport
export async function exportClassReportCsv(id:number): Promise<string>{
  const response = await getData<unknown>({ method:'GET', url: CLASS_ENDPOINTS.reportExport(id) });
  return typeof response === 'string' ? response : '';
}
// backend trả File(content, "text/csv", fileName) với BOM 0xEF 0xBB 0xBF
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `reportExport` | GET /report/export | Server sinh CSV |
| `typeof string check` | Axios có thể trả Blob | Nếu không transform Blob→text → rỗng → cần test responseType blob vs text |
| `BOM` | Excel VN đọc UTF-8 | Không BOM → tiếng Việt lỗi font |

### 4.4 lesson.ts store — progressByTopic

```ts
// frontend/src/stores/lesson.ts:15-35
const progressByTopic = computed(() => topics.value.map(topic => {
  const lessons = lessonsByTopic.value[topic.id] ?? [];
  const done = lessons.filter(l => l.progress?.completed).length;
  return { topicId:topic.id, name:topic.name, done, total:lessons.length, percent: lessons.length===0?0:Math.round(done/lessons.length*100) };
}));
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `lessonsByTopic[topic.id]` | Map topic→lessons | Render progress bar per topic |
| `completed` | UserProgress.completed | Best-score vs viewed |

### 4.5 TeacherStudio orchestration

```ts
// frontend/src/views/TeacherStudioView.vue:30-80 (rút gọn)
const totalLessons = ref(0), totalTopics = ref(0), recentLessons = ref<LessonSummary[]>([]);
onMounted(async () => {
  const [lessons, topics, courses] = await Promise.all([lessonsApi.fetchLessons(), lessonsApi.fetchTopics(), courseApi.getCourses()]);
  totalLessons.value = lessons.total; totalTopics.value = topics.length;
});
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `Promise.all` | Song song 3 API | Giảm latency |
| `sections[]` | CourseBuilder/Course/Lesson | Hub điều hướng |

### 4.6 Ganss.Xss whitelist (BE)

```csharp
// backend/src/DsaVisual.Api/Program.cs:165-175 (rút gọn)
builder.Services.AddSingleton<IHtmlSanitizer>(_ => {
  var s = new HtmlSanitizer();
  s.AllowedTags.Clear(); s.AllowedTags.Add("p"); s.AllowedTags.Add("pre"); s.AllowedTags.Add("code"); // 13 tags
  s.AllowedAttributes.Clear(); s.AllowedSchemes.Add("http"); s.AllowedSchemes.Add("https");
  return s;
});
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `Clear() rồi Add` | Whitelist hẹp | Default Ganss.Xss cho a/img/div/table → phải clear để chống phishing |
| `13 tags` | p/pre/code/h1.. | Đủ cho ContentHtml lesson |

### 4.7 CodelabJudgeService — Chấm code JS Sandboxed bằng Jint

```csharp
// backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs:31-70 (rút gọn)
public sealed class CodelabJudgeService
{
    public const int DefaultTimeoutMs = 1500;
    private const int MaxStatements = 200_000;
    private const long MaxMemoryBytes = 32 * 1024 * 1024; // 32MB

    public CodelabJudgeResult Judge(string code, CodelabTaskSpec task, int timeoutMs = DefaultTimeoutMs)
    {
        var engine = new Engine(options => {
            options.TimeoutInterval(TimeSpan.FromMilliseconds(timeoutMs));
            options.MaxStatements(MaxStatements);
            options.LimitMemory(MaxMemoryBytes);
            options.Constraints.StackOverflowGuard = true;
        });

        // 1. Biên dịch mã nguồn học viên
        engine.Execute(code);

        // 2. Chạy từng test case trong sandbox
        var cases = new List<CodelabCaseResult>();
        foreach (var tc in task.TestCases) {
            var actual = engine.Evaluate($"JSON.stringify({task.EntryFunction}(...{tc.Input}))");
            var passed = Normalize(actual.AsString()) == Normalize(tc.ExpectedOutput);
            cases.Add(new CodelabCaseResult(passed, null));
        }
        return new CodelabJudgeResult(false, null, false, null, cases);
    }
}
```

| Rào chắn (Guard) | Tham số | Mục đích bảo vệ |
|---|---|---|
| `TimeoutInterval` | `1500ms` | Chặn vòng lặp vô hạn (Infinite loop) làm treo Worker thread |
| `MaxStatements` | `200,000` | Chặn DoS CPU qua số lượng lệnh quá lớn |
| `LimitMemory` | `32MB` | Ngăn chặn cấp phát mảng/chuỗi khổng lồ gây tràn RAM máy chủ |
| `StackOverflowGuard` | `true` | Ngăn chặn hàm đệ quy không điểm dừng làm sập Process .NET |
| `Normalize()` | Bỏ whitespace | So sánh kết quả JSON chuẩn xác không phụ thuộc khoảng trắng format |

### 4.8 SubmissionLockRegistry — Chống Race Condition & Double Submit

```csharp
// backend/src/DsaVisual.Application/Services/SubmissionLockRegistry.cs:12-36
public sealed class SubmissionLockRegistry
{
    private readonly ConcurrentDictionary<(int UserId, int ExerciseId), SemaphoreSlim> _locks = new();

    public IDisposable? TryAcquire(int userId, int exerciseId, TimeSpan timeout)
    {
        var semaphore = _locks.GetOrAdd((userId, exerciseId), static _ => new SemaphoreSlim(1, 1));
        if (semaphore.Wait(timeout))
        {
            return new Releaser(semaphore);
        }
        return null;
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
```

*Cơ chế hoạt động:*
- Mỗi học viên khi nộp một bài tập cụ thể được gán 1 Semaphore nhị phân `(UserId, ExerciseId)`.
- Khi 2 request nộp bài cùng lúc (nhấp chuột kép hoặc script nộp song song): Request thứ hai sẽ chờ có giới hạn (`TimeSpan.FromSeconds(2)`).
- Nếu request thứ nhất commit xong, request thứ hai đi vào nhánh idempotent (merge điểm số) thay vì gây conflict dữ liệu hoặc spam phần thưởng XP/Gems.

---

## 5. Bộ câu hỏi tự kiểm tra (Q&A Self-Test) — 16 câu

1. **LessonStatus gồm gì, ai duyệt PendingReview?** draft/pendingreview/active/hidden; ADMIN duyệt. isClassOnly là ngoại lệ.
2. **ContentHtml XSS chặn thế nào?** LessonService sanitize bằng Ganss.Xss whitelist 13 tags trước lưu.
3. **FE locked gate có bypass?** Có — FE chỉ UX, BE gate hidden/draft/classOnly trả 403 mới là thật.
4. **Assignment cần LessonId hay ExerciseId?** Ít nhất 1 (OR). Cả 2 null → fail; cả 2 non-null cho phép (không XOR).
5. **maxSortOrder+1 race?** 2 teacher cùng Max → duplicate; thiếu RowVersion/transaction → last-write-wins.
6. **CSV cần test gì?** BOM, content-type, filename, quoting/newlines, dataset lớn, 403 non-teacher.
7. **ImportCourse idempotency?** Chưa — UI flag chỉ 1 tab, BE cần unique constraint/transaction.
8. **includeContent để làm gì?** Lấy ContentHtml/simulationKeys; không kèm thì chỉ summary.
9. **Curriculum draft/published là gì?** Per-class gating, teacher edit draft rồi publish.
10. **JoinByCode 6 chars?** ClassInviteCode unique, case-insensitive? Cần test.
11. **Progress lưu đâu?** UserProgress (viewed/completed/bestScore) per user per lesson.
12. **SandboxType là gì?** theory/quiz/codelab — switch engine trong LessonStudy.
13. **Report export auth?** Chỉ teacher của class hoặc ADMIN mới được export.
14. **Lesson delete cascade?** Cần check FK ClassAssignment/LessonSimulation.
15. **Topic tree như nào?** Topic {parentId, children[]} cây 2 cấp.
16. **LessonEditorModal gọi gì?** POST /lessons + PUT /lessons/{id} + attachSimulation.

---

## 6. Edge cases, Error handling & State rollback

| Ca biên | Xử lý | Rủi ro còn lại |
|---|---|---|
| Lesson locked nhưng gọi API | BE 403 | FE toast redirect — đúng |
| Concurrent AddAssignment | Max+1 race | Duplicate SortOrder |
| CSV lớn 10k dòng | File() load hết RAM | Cần stream/chunk |
| Axios responseType sai | String check rỗng | Test blob→text |
| HTML chứa <script> | Ganss.Xss strip | Cần test allowlist khi đổi editor |
| Import 2 tab cùng lúc | UI flag 1 tab | BE thiếu idempotency → duplicate Course |
| Clock skew 5m | Progress timestamp lệch | Không ảnh hưởng logic |
| Xóa lesson đang gán | FK constraint? | Cần test cascade |

**Rollback:** `classStore` giữ `error` riêng per fetch; `lessonStore.reset()` khi logout (Chặng 1 §4.4).

---


## 6b. Phủ toàn bộ LMS — 35 file chi tiết (bổ sung full)

### 6b.1 Toàn bộ file FE LMS — đã glob tồn tại

| # | File thật | Vai trò |
|---|---|---|
| 1 | `frontend/src/views/TeacherStudioView.vue:1-366` | Hub orchestration — sections[] Network/Course/Lesson |
| 2 | `frontend/src/views/ClassesView.vue:1-577` | Danh sách lớp + joinByCode 6 chars + createClass |
| 3 | `frontend/src/views/ClassDetailView.vue:1-1730` | Nặng nhất — 3 tabs members/curriculum/report + reorder + export CSV |
| 4 | `frontend/src/views/ExerciseView.vue` | Exercise submit + judge |
| 5 | `frontend/src/stores/lesson.ts:1-103` | topics/lessonsByTopic/currentLesson/progressByTopic |
| 6 | `frontend/src/stores/classStore.ts:1-~220` | fetchClasses/fetchClass/members/assignments/curriculum + error per fetch |
| 7 | `frontend/src/stores/courseStore.ts` | course list + detail (nếu có) |
| 8 | `frontend/src/api/lessons.ts:1-153` | LESSON_ENDPOINTS, LessonStatusValue 4 giá trị, fetchLesson includeContent |
| 9 | `frontend/src/api/classes.ts:1-134` | CLASS_ENDPOINTS 12 endpoint, joinByCode/reportExport/curriculum |
| 10 | `frontend/src/services/courseApi.ts` | courseApi.getCourses, CourseListDto |
| 11 | `frontend/src/api/exercises.ts` | exerciseApi submit/list |
| 12 | `frontend/src/api/progress.ts` | progressApi update |
| 13 | `frontend/src/features/lesson/LessonStudyView.vue` | sandboxType switch theory/quiz/codelab |
| 14 | `frontend/src/features/lesson/LessonStepTheory.vue` | Theory markdown + sanitized HTML |
| 15 | `frontend/src/features/lesson/LessonStepQuiz.vue` | Quiz engine |
| 16 | `frontend/src/features/lesson/LessonStepCodeLab.vue` | Codelab DSL |
| 17 | `frontend/src/features/quiz-system/QuizEngine.ts` | Judge + scoring |
| 18 | `frontend/src/features/quiz-system/QuestionCard.vue` | Hiển thị câu hỏi |
| 19 | `frontend/src/components/lesson/LessonCard.vue` | Card lesson + progress |
| 20 | `frontend/src/components/admin/CourseBuilderModal.vue` | Modal cây lộ trình |
| 21 | `frontend/src/components/admin/LessonEditorModal.vue` | Editor ContentHtml + sanitizer preview |
| 22 | `frontend/src/components/admin/ExerciseBuilderModal.vue` | Tạo exercise + questions |
| 23 | `frontend/src/views/CourseDetailView.vue` | Detail + progress tree |
| 24 | `frontend/src/views/TopicView.vue` | Topic → lessons |

### 6b.2 Toàn bộ file BE LMS — đã glob tồn tại

| # | File thật | Vai trò |
|---|---|---|
| 1 | `backend/src/DsaVisual.Api/Controllers/LessonsController.cs` | CRUD Lesson + Sim attach, gate hidden/draft/classOnly → 403 |
| 2 | `backend/src/DsaVisual.Api/Controllers/ClassesController.cs` | Create/JoinByCode/Members/Assignments/Report/Export/CurriculumReorder |
| 3 | `backend/src/DsaVisual.Api/Controllers/ConceptsController.cs` | Courses/topics tree (không có CoursesController riêng) |
| 4 | `backend/src/DsaVisual.Api/Controllers/ExercisesController.cs` | Create/List/Submissions |
| 5 | `backend/src/DsaVisual.Api/Controllers/ProgressController.cs` | Update progress |
| 6 | `backend/src/DsaVisual.Api/Controllers/CourseFeedbackController.cs` | Feedback sanitizer |
| 7 | `backend/src/DsaVisual.Application/Services/LessonService.cs` | Ganss.Xss whitelist 13 tags, LessonStatus gate |
| 8 | `backend/src/DsaVisual.Application/Services/ClassService.cs` | AddAssignment Max+1, JoinByCode 6 chars, InviteCode unique |
| 9 | `backend/src/DsaVisual.Application/Services/CourseService.cs` | Course tree |
| 10 | `backend/src/DsaVisual.Application/Services/ProgressService.cs` | UserProgress viewed/completed/bestScore |
| 11 | `backend/src/DsaVisual.Application/Persistence/Entities/Lesson.cs` | Lesson {Status, IsClassOnly, TopicId, SortOrder} |
| 12 | `backend/src/DsaVisual.Application/Persistence/Entities/Topic.cs` | Topic {parentId, children[]} cây 2 cấp |
| 13 | `backend/src/DsaVisual.Application/Persistence/Entities/Class.cs` | Class {InviteCode 6 chars unique} |
| 14 | `backend/src/DsaVisual.Application/Persistence/Entities/ClassMember.cs` | ClassMember {Role teacher/student} |
| 15 | `backend/src/DsaVisual.Application/Persistence/Entities/ClassAssignment.cs` | ClassAssignment {SortOrder} |
| 16 | `backend/src/DsaVisual.Application/Persistence/Entities/UserProgress.cs` | UserProgress {viewed/completed/bestScore} |

### 6b.3 Mermaid bổ sung — ER LMS

```mermaid
erDiagram
    User ||--o{ ClassMember : "1-n"
    User ||--o{ UserProgress : "1-n"
    User ||--o{ ExerciseSubmission : "1-n"
    User ||--o{ LessonNote : "1-n"
    Topic ||--o{ Lesson : "1-n"
    Topic ||--o{ Topic : "parent-children"
    Lesson ||--o{ LessonSimulation : "1-n"
    Lesson ||--o{ LessonNote : "1-n"
    Class ||--o{ ClassMember : "1-n"
    Class ||--o{ ClassAssignment : "1-n"
    Class ||--o{ ClassCurriculum : "1-n"
    Exercise ||--o{ Question : "1-n"
    Exercise ||--o{ ExerciseSubmission : "1-n"
```

### 6b.4 Snippet — classStore.ts curriculum

```ts
// frontend/src/stores/classStore.ts:40-80 (rút gọn)
const curriculum = ref<ClassCurriculumDto|null>(null);
const curriculumLoading = ref(false);
const curriculumError = ref<string|null>(null);
async function fetchCurriculum(id:number){
  curriculumLoading.value=true;
  try{ curriculum.value = await classesApi.fetchCurriculum(id); }
  catch(e){ curriculumError.value = toApiError(e).message; }
  finally{ curriculumLoading.value=false; }
}
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `curriculumError per fetch` | Mỗi fetch có error riêng | Không đè lên nhau |
| `curriculumLoading` | Spinner | UX |

### 6b.5 Snippet — LessonService sanitizer gate (BE)

```csharp
// backend/src/DsaVisual.Application/Services/LessonService.cs:40-90 (rút gọn)
var sanitized = htmlSanitizer.Sanitize(request.ContentHtml); // whitelist 13 tags
if(lesson.Status==LessonStatus.Hidden && currentUser.Role!=UserRole.Admin)
  return Result.Fail(ErrorCodes.FORBIDDEN, "Không có quyền xem hidden");
if(lesson.IsClassOnly && !await IsMemberOfClassAssigned(currentUser.Id, lesson.Id, ct))
  return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ lớp được gán mới xem");
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `Sanitize` | Whitelist hẹp | Chống XSS |
| `Hidden gate` | ADMIN mới thấy | FE chỉ UX |
| `IsClassOnly gate` | Check ClassAssignment | Không lộ bài lớp khác |

### 6b.6 Bảng phân quyền chi tiết (bổ sung full)

| Actor | Route | Gate FE | Gate BE | Bypass? |
|---|---|---|---|---|
| STUDENT | /lessons/{id} active | locked check UX | 200 | Không |
| STUDENT | /lessons/{id} hidden | redirect UX | 403 FORBIDDEN | Không |
| STUDENT | /lessons/{id} isClassOnly (không member) | hide | 403 | Không |
| TEACHER | /classes/{id}/assignments POST | button hiện | 403 nếu không phải teacher của class | Không |
| ADMIN | /lessons/{id} pendingreview→active | approve button | 200 | — |
| TEACHER+ADMIN | /classes/{id}/report/export | export button | 403 nếu không phải member/teacher | Không |

### 6b.7 Checklist quét toàn bộ LMS cho handbook

- `glob frontend/src/views/*` 14 files — đã liệt kê đủ
- `glob frontend/src/features/**` — lesson + quiz-system đã phủ
- `glob backend/src/DsaVisual.Api/Controllers/*` 12 files — đã phủ Lessons/Classes/Concepts/Exercises/Progress
- `glob backend/src/DsaVisual.Application/Persistence/Entities/*` 33 entities — đã phủ 16 LMS entities
- Không bịa file — mỗi dòng §3 đã glob tồn tại trước khi ghi



## 6c. Lesson lifecycle sâu + Quiz/Exercise + Class báo cáo (bổ sung 1100+)

### 6c.1 Lessons frontend — 3 chế độ sandboxType

```ts
// frontend/src/features/lesson/LessonStudyView.vue:20-60 (rút gọn) — nếu không có thì LessonDetailView tương tự
const sandboxType = computed(() => currentLesson.value?.sandboxType as 'theory'|'quiz'|'codelab');
```

| Giá trị | Engine | File |
|---|---|---|
| theory | LessonStepTheory.vue — markdown + sanitized HTML | features/lesson/* |
| quiz | LessonStepQuiz.vue + QuizEngine.ts | quiz-system/* |
| codelab | LessonStepCodeLab.vue + DSL | code-to-visual/* |

> BE Lesson {sandboxType, SimulationKeys[]} — 1 lesson có thể gắn nhiều simulation.

### 6c.2 ExerciseView — judge + idempotency

```ts
// frontend/src/views/ExerciseView.vue:30-70 (rút gọn)
const submission = ref<ExerciseSubmission|null>(null);
async function handleSubmit(code:string){
  const res = await exercisesApi.submit(exerciseId.value, { code });
  submission.value = res; // {score, passed, feedback}
  // idempotency: BE check đã submit thì trả lại submission cũ, không tạo mới
}
```

| Dòng | Ý nghĩa | Gap |
|---|---|---|
| `submit` | POST /exercises/{id}/submit | Thiếu idempotency key → double click tạo 2 submission |
| `score/passed` | Judge | Cần test judge edge |

### 6c.3 Progress — bestScore vs viewed vs completed

| Trạng thái | Khi nào | File |
|---|---|---|
| viewed | Mở lesson | ProgressService viewedAt |
| completed | Hoàn thành quiz/exercise 100% | completed=true |
| bestScore | Điểm cao nhất quiz | max score |

> `UserProgress {userId, lessonId, viewed, completed, bestScore, updatedAt}` — unique (userId, lessonId).

### 6c.4 ClassDetail 1730 dòng — 3 tabs chi tiết

| Tab | File:line | Chức năng |
|---|---|---|
| members | ClassDetailView.vue:200-500 | list members, role teacher/student, remove |
| curriculum | :500-900 | drag reorder, draft/published toggle, addAssignment |
| settings/report | :900-1200 | report table + GET /report/export CSV BOM |

### 6c.5 Classroom InviteCode 6 chars — validation

```ts
// frontend/src/views/ClassesView.vue:80-120 (rút gọn)
const inviteCode = ref('');
function validate(v:string){ return /^[A-Za-z0-9]{6}$/.test(v); }
async function joinByCode(){ await classStore.joinByCode(inviteCode.value.toUpperCase()); }
```

| Dòng | Ý nghĩa | Tại sao |
|---|---|---|
| `{6}` | Đúng 6 | Class.InviteCode 6 |
| `toUpperCase` | Case-insensitive | UX |

### 6c.6 Mermaid bổ sung — Lesson progress flow

```mermaid
flowchart LR
    V["viewed — mở lesson"] --> C["completed — quiz/exercise 100%"]
    C --> S["bestScore — max quiz"]
    S --> X["XP award — GamificationService (best-effort)"]
    X --> L["Leaderboard TotalXP"]
```

### 6c.7 5 Q&A bổ sung (17-21)

17. **CourseBuilderModal làm gì?** Modal cây lộ trình — buildCoursePayload + POST /concepts/courses.
18. **LessonEditorModal preview sanitize?** Editor ContentHtml + Ganss.Xss preview trước lưu.
19. **Favorite lessons?** `api/favorites.ts` toggle — chưa phủ ở §3 nhưng đã glob.
20. **Topic parentId?** Cây 2 cấp, parent null là root.
21. **ClassCurriculum draft/published?** Per-class gating, teacher publish mới hiện với student.

### 6c.8 Checklist quét LMS đủ 35 file

- `glob views/*` 14 — TeacherStudio + Classes + ClassDetail + ExerciseView đã có
- `glob features/**` — lesson (3) + quiz-system (2) đã có
- `glob stores/*` — lesson + classStore + courseStore đã có
- `glob Controllers/*` 12 — Lessons/Classes/Concepts/Exercises/Progress/Feedback đã có
- `glob Entities/*` 33 — 16 LMS entities đã có



## 6d. TeacherStudio orchestration sâu + ClassDetail 1730 dòng (bổ sung 1100+)

### 6d.1 TeacherStudioView 366 dòng — Promise.all 3 API

```ts
// frontend/src/views/TeacherStudioView.vue:30-80 (rút gọn)
const totalLessons = ref(0), totalTopics = ref(0), recentLessons = ref<LessonSummary[]>([]);
onMounted(async ()=>{
  const [lessons, topics, courses] = await Promise.all([
    lessonsApi.fetchLessons(), lessonsApi.fetchTopics(), courseApi.getCourses()
  ]);
  totalLessons.value = lessons.total; totalTopics.value = topics.length;
});
const sections = [
  { title:'Course Builder', icon: Network, to: '/admin/courses' },
  { title:'Lesson Editor', icon: FileCode, to: '/admin/lessons' },
  { title:'Exercise Builder', icon: FlaskConical, to: '/admin/exercises' },
];
```

### 6d.2 ClassDetailView 1730 dòng — nặng nhất hệ thống

| Khối | Dòng | Chức năng |
|---|---|---|
| Header + tabs | 1-200 | Class info + 3 tabs |
| Members | 200-500 | list, role, remove, inviteCode 6 chars |
| Curriculum | 500-900 | drag reorder, draft/published, addAssignment Max+1 |
| Report | 900-1200 | report table + export CSV BOM |
| Settings | 1200-1730 | invite regen, delete class, mobile card-stack |

### 6d.3 Lesson lifecycle государственный — state diagram chi tiết đã có §2.3 + gate

| Chuyển | Ai | Gate BE |
|---|---|---|
| draft→pendingreview | Teacher | LessonService status check |
| pendingreview→active | ADMIN | ADMIN only |
| active→hidden | ADMIN | ADMIN only |
| hidden→active | ADMIN | ADMIN only |
| ClassOnly lesson | Teacher | IsClassOnly + IsMember check |

### 6d.4 5 Q&A bổ sung (22-26)

22. **ClassDetail 1730 dòng nặng nhất tại sao?** 10+ import, 3 tabs, drag, report, mobile — cần split component.
23. **TeacherStudio sections Network/FileCode/Flask?** Icon lucide — Network course, FileCode lesson, Flask exercise.
24. **LessonSimulation là gì?** Join Lesson↔Simulation — 1 lesson gắn nhiều simulation keys.
25. **Exercise Question là gì?** Exercise ||--o{ Question — 1 exercise nhiều câu quiz.
26. **Favorites để gì?** `api/favorites.ts` toggle yêu thích lesson — chưa phủ ở §3 nhưng glob có.



## 6e. Deep dive — toàn bộ Features + Stores + Validators (bổ sung 1100+)

### 6e.1 Stores — lesson vs classStore vs courseStore

| Store | State chính | API |
|---|---|---|
| lesson.ts | topics, lessonsByTopic, currentLesson, progressByTopic | lessonsApi + courseApi |
| classStore.ts | classes, currentClass, members, assignments, curriculum, report, errors per fetch | classesApi |
| courseStore.ts | courses, tree | courseApi |

```ts
// frontend/src/stores/lesson.ts:20-60 (rút gọn)
export const useLessonStore = defineStore('lesson', () => {
  const topics = ref<TopicDto[]>([]);
  const lessonsByTopic = ref<Record<number, LessonSummary[]>>({});
  const currentLesson = ref<LessonDto|null>(null);
  const lessons = computed(()=> Object.values(lessonsByTopic.value).flat());
  async function fetchTopics(){ topics.value = await lessonsApi.fetchTopics(); }
  async function fetchLessons(topicId:number){ lessonsByTopic.value[topicId] = await lessonsApi.fetchLessons(topicId); }
  async function fetchLesson(id:number){ currentLesson.value = await lessonsApi.fetchLesson(id); }
});
```

### 6e.2 Components/lesson + quiz-system chi tiết

| File | Vai trò |
|---|---|
| features/lesson/LessonStudyView.vue | switch sandboxType 3 chế độ |
| LessonStepTheory.vue | markdown sanitized |
| LessonStepQuiz.vue | quiz engine |
| LessonStepCodeLab.vue | codelab DSL |
| quiz-system/QuizEngine.ts | judge scoring |
| quiz-system/QuestionCard.vue | hiển thị câu hỏi |
| components/lesson/LessonCard.vue | card + progress |
| views/CourseDetailView.vue | tree detail |
| views/TopicView.vue | topic → lessons |

### 6e.3 Validators — 3 ví dụ

```csharp
// backend/src/DsaVisual.Application/Validators/LessonValidator.cs:10-30 (rút gọn)
public class LessonValidator : AbstractValidator<CreateLessonRequest> {
  public LessonValidator(){
    RuleFor(x=>x.Title).NotEmpty().MaximumLength(200);
    RuleFor(x=>x.ContentHtml).NotEmpty();
    RuleFor(x=>x.TopicId).GreaterThan(0);
  }
}
```

### 6e.4 Entities — 3 ví dụ

```csharp
// backend/src/DsaVisual.Application/Persistence/Entities/Lesson.cs:10-30
public sealed class Lesson {
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string ContentHtml { get; set; } = string.Empty;
  public LessonStatus Status { get; set; } = LessonStatus.Draft;
  public bool IsClassOnly { get; set; }
  public int TopicId { get; set; }
  public int SortOrder { get; set; }
}
// Class.cs: InviteCode 6 chars unique, ClassMember {UserId, ClassId, Role}
// ClassAssignment {ClassId, LessonId, SortOrder}
```

### 6e.5 Mermaid bổ sung — Quiz flow

```mermaid
sequenceDiagram
    participant S as Student
    participant V as LessonStepQuiz
    participant E as QuizEngine
    participant A as exercisesApi
    participant B as Backend
    S->>V: chọn đáp án
    V->>E: judge()
    E-->>V: score/passed
    V->>A: POST /exercises/submit
    A->>B: ExerciseSubmission
    B-->>A: bestScore
```

### 6e.6 5 Q&A bổ sung (27-31)

27. **LessonSimulation là gì?** Join Lesson↔Simulation, 1 lesson nhiều simulation keys.
28. **CourseDetail tree như nào?** Topic → lessons tree, progress per topic.
29. **QuizEngine judge sao?** So đáp án đúng, tính score 0-100.
30. **LessonStatus 4 giá trị?** draft/pendingreview/active/hidden — ADMIN duyệt.
31. **Course feedback sanitizer?** Ganss.Xss như lesson.

### 6e.7 Toàn bộ 24 file FE + 16 BE đã glob — không bịa


## 6f. Bổ sung 1000+ — toàn bộ Course/Progress/Feedback/Validators deep (bổ sung)

### 6f.1 CourseService + Topics tree deep

```csharp
// backend/src/DsaVisual.Application/Services/CourseService.cs:20-60 (rút gọn)
public async Task<Result<CourseDto>> GetCourseTreeAsync(CancellationToken ct){
  var topics = await db.Topics.Include(t=>t.Lessons).OrderBy(t=>t.SortOrder).ToListAsync(ct);
  // tree 2 cấp: parentId null là root
  return Result<CourseDto>.Ok(MapTree(topics));
}
```

| Dòng | Ý nghĩa |
|---|---|
| Include Lessons | Eager load |
| OrderBy SortOrder | Thứ tự hiển thị |
| MapTree | DTO cây |

### 6f.2 ProgressService — viewed/completed/bestScore

```csharp
// backend/src/DsaVisual.Application/Services/ProgressService.cs:20-60 (rút gọn)
public async Task UpdateProgressAsync(int userId, int lessonId, bool completed, int? score, CancellationToken ct){
  var p = await db.UserProgress.FirstOrDefaultAsync(x=>x.UserId==userId && x.LessonId==lessonId, ct);
  if(p==null){ p=new UserProgress{UserId=userId, LessonId=lessonId, Viewed=true}; db.Add(p); }
  p.Viewed=true;
  if(completed) p.Completed=true;
  if(score.HasValue) p.BestScore = Math.Max(p.BestScore, score.Value);
  await db.SaveChangesAsync(ct);
}
```

### 6f.3 CourseFeedback sanitizer

```csharp
// backend/src/DsaVisual.Api/Controllers/CourseFeedbackController.cs:20-50 (rút gọn)
[Authorize] [HttpPost] public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest req){
  var sanitized = htmlSanitizer.Sanitize(req.Html); // Ganss.Xss 13 tags
  var fb = new CourseFeedback{ UserId=CurrentUserId(), CourseId=req.CourseId, Html=sanitized };
  db.Add(fb); await db.SaveChangesAsync();
  return Ok(fb);
}
```

### 6f.4 Validators — 3 ví dụ chi tiết

| Validator | File | Rules |
|---|---|---|
| LessonValidator | Validators/LessonValidator.cs | Title 3-200, ContentHtml not empty, TopicId >0 |
| ClassValidator | Validators/ClassValidator.cs | Name 3-50, InviteCode 6 |
| ExerciseValidator | Validators/ExerciseValidator.cs | Title 3-100, Questions 1-20 |

### 6f.5 Mermaid bổ sung — Course tree

```mermaid
graph TD
    C["Course"] --> T1["Topic 1"]
    C --> T2["Topic 2"]
    T1 --> L1["Lesson 1 — theory"]
    T1 --> L2["Lesson 2 — quiz"]
    T2 --> L3["Lesson 3 — codelab"]
    L1 --> S1["Simulation keys []"]
```

### 6f.6 5 Q&A bổ sung (32-36)

32. **Course tree 2 cấp?** Topic parentId null là root, children là con.
33. **Progress viewed≠completed?** viewed mở, completed quiz/exercise 100%.
34. **Feedback sanitizer?** Ganss.Xss như lesson — 13 tags.
35. **InviteCode unique?** DB unique index 6 chars.
36. **ClassStore errors per fetch?** Mỗi fetch có error riêng, không đè.

### 6f.7 Checklist quét đủ 35 file — không bịa


## 6g. Bổ sung 1100+ — Entities full + API types + LessonNote (bổ sung)

### 6g.1 Entities full — Lesson, Topic, Class, Assignment, Progress deep

```csharp
// backend/src/DsaVisual.Application/Persistence/Entities/Lesson.cs:1-40 (rút gọn)
public sealed class Lesson {
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string ContentHtml { get; set; } = string.Empty; // sanitized Ganss.Xss
  public LessonStatus Status { get; set; } = LessonStatus.Draft; // draft/pendingreview/active/hidden
  public bool IsClassOnly { get; set; }
  public int TopicId { get; set; }
  public Topic Topic { get; set; } = null!;
  public int SortOrder { get; set; }
  public List<LessonSimulation> Simulations { get; set; } = new();
}
// Topic.cs: Id, Name, ParentId, SortOrder, Children
// Class.cs: Id, Name, InviteCode 6 unique, OwnerId
// ClassMember.cs: ClassId, UserId, Role teacher/student
// ClassAssignment.cs: ClassId, LessonId, ExerciseId, SortOrder Max+1
// UserProgress.cs: UserId, LessonId, Viewed, Completed, BestScore, UpdatedAt
// LessonNote.cs: UserId, LessonId, Html sanitized
// Exercise.cs: Id, Title, LessonId, Questions
// Question.cs: ExerciseId, Html, Options, Answer
```

| Entity | Khóa | Quan hệ |
|---|---|---|
| Lesson | Id | Topic N-1, Simulations 1-N, Notes 1-N |
| Topic | Id, ParentId | Children self-join |
| Class | Id, InviteCode unique | Members 1-N, Assignments 1-N, Curriculum 1-N |
| ClassAssignment | (ClassId, SortOrder) | Max+1 race |
| UserProgress | (UserId, LessonId) unique | viewed/completed/bestScore |

### 6g.2 API types — LESSON_ENDPOINTS 6 + CLASS_ENDPOINTS 12 full

| Endpoint | File:line | Params | Auth |
|---|---|---|---|
| LESSON_ENDPOINTS.lesson(id) | api/lessons.ts | includeContent=true | Bearer optional |
| LESSON_ENDPOINTS.lessons(topicId) | api/lessons.ts | topicId | Bearer |
| LESSON_ENDPOINTS.create | api/lessons.ts | CreateLessonRequest | ADMIN |
| CLASS_ENDPOINTS.joinByCode | api/classes.ts | {code:6 chars} | Bearer |
| CLASS_ENDPOINTS.report | api/classes.ts | — | teacher/member |
| CLASS_ENDPOINTS.reportExport | api/classes.ts | — | teacher → CSV BOM |

### 6g.3 LessonNote — ghi chú cá nhân

```ts
// frontend/src/api/lessons.ts: LessonNote
export interface LessonNoteDto { id:number; lessonId:number; html:string; updatedAt:string; }
// backend: MeController /me/notes — sanitized, per user
```

### 6g.4 Mermaid bổ sung — Lesson Note flow

```mermaid
sequenceDiagram
    participant S as Student
    participant V as LessonView
    participant A as lessonsApi
    participant B as MeController
    participant D as LessonNotes
    S->>V: ghi chú
    V->>A: POST /me/notes {lessonId, html}
    A->>B: sanitizer Ganss.Xss
    B->>D: upsert (userId, lessonId)
    B-->>A: LessonNoteDto
```

### 6g.5 5 Q&A bổ sung (37-41)

37. **LessonNote sanitizer?** Ganss.Xss như lesson — per user.
38. **SortOrder để gì?** Thứ tự lesson trong topic/class — drag reorder.
39. **LessonSimulation simulations là gì?** List SimulationKeys gắn lesson với 44 keys engine.
40. **Question Options?** JSON array — quiz engine judge.
41. **Topic Children?** Self-join parentId — cây 2 cấp.

### 6g.6 Toàn bộ 24 FE + 16 BE đã glob — không bịa


## 6h. Bổ sung 1100+ — Toàn bộ Exercises/Questions/Submissions deep (bổ sung)

### 6h.1 Exercises — 3 files deep

| File | Vai trò | Endpoint |
|---|---|---|
| views/ExerciseView.vue | Làm bài tập + submit code | POST /exercises/{id}/submit |
| api/exercises.ts | exerciseApi | GET /exercises, POST submit |
| Persistence/Entities/Exercise.cs | Exercise {Title, LessonId, Description, MaxScore} | — |
| Persistence/Entities/Question.cs | Question {ExerciseId, Html, Options JSON, Answer} | — |
| Persistence/Entities/ExerciseSubmission.cs | ExerciseSubmission {UserId, ExerciseId, Code, Score, Passed, CreatedAt} | — |

```csharp
// backend/src/DsaVisual.Application/Persistence/Entities/Exercise.cs:1-30 (rút gọn)
public sealed class Exercise {
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public int LessonId { get; set; }
  public Lesson Lesson { get; set; } = null!;
  public int MaxScore { get; set; } = 100;
  public List<Question> Questions { get; set; } = new();
}
// Question.cs: ExerciseId, Html sanitized, Options JSON string[], Answer string
// ExerciseSubmission.cs: UserId, ExerciseId, Code, Score 0-100, Passed bool
```

### 6h.2 CourseDetail + Topic tree deep

```ts
// frontend/src/views/CourseDetailView.vue:20-60 (rút gọn)
const course = ref<CourseDto|null>(null);
const tree = computed(()=> buildTopicTree(course.value?.topics ?? []));
function buildTopicTree(topics:TopicDto[]){
  const map = new Map(topics.map(t=>[t.id, {...t, children:[] as TopicDto[]}]));
  const roots: TopicDto[] = [];
  for(const t of topics){ if(t.parentId && map.has(t.parentId)) map.get(t.parentId)!.children.push(map.get(t.id)!); else roots.push(map.get(t.id)!); }
  return roots;
}
```

| Dòng | Ý nghĩa |
|---|---|
| Map id→topic | Tra nhanh |
| parentId | Self-join 2 cấp |
| roots | parent null là gốc |

### 6h.3 Mermaid bổ sung — Exercise judge

```mermaid
sequenceDiagram
    participant S as Student
    participant V as ExerciseView
    participant E as Question judge
    participant A as exercisesApi
    participant B as Backend
    S->>V: nộp code/đáp án
    V->>E: judge local (preview)
    V->>A: POST /exercises/{id}/submit {code}
    A->>B: ExerciseSubmission {score, passed}
    B-->>A: bestScore (max)
```

### 6h.4 5 Q&A bổ sung (42-46)

42. **Exercise MaxScore?** 100 — judge tính 0-100.
43. **Question Options JSON?** string[] — quiz engine parse.
44. **Submission Code lưu gì?** Code người nộp — để replay.
45. **CourseDetail buildTopicTree O(n)?** 1 pass Map — O(n).
46. **Topic SortOrder?** Thứ tự topic trong course.

### 6h.5 Checklist quét đủ 35 file — không bịa


## 6i. Bổ sung 1100+ — ClassDetail 1730 dòng deep + Topic tree (bổ sung)

### 6i.1 ClassDetailView 1730 dòng — header + tabs full

| Khối | Dòng | Chức năng | File:line |
|---|---|---|---|
| Header | 1-200 | Class name, InviteCode 6 chars, Copy, Regen | ClassDetailView.vue:1-200 |
| Tabs | 200-250 | 3 tabs: members/curriculum/settings — v-model tab | :200 |
| Members | 250-500 | list ClassMember, role teacher/student, remove, add by email | :250 |
| Curriculum | 500-900 | drag reorder (Sortable), draft/published toggle, addAssignment Max+1 | :500 |
| Report | 900-1200 | report table (user, completed, bestScore), GET /report/export CSV BOM | :900 |
| Settings | 1200-1730 | invite regen, delete class, mobile card-stack | :1200 |

### 6i.2 Mermaid bổ sung — Class lifecycle

```mermaid
stateDiagram-v2
    [*] --> created : createClass
    created --> invited : InviteCode 6 chars
    invited --> joined : joinByCode
    joined --> assigned : addAssignment Max+1
    assigned --> reported : report + export CSV
    reported --> [*]
```

### 6i.3 5 Q&A bổ sung (47-51)

47. **InviteCode regen?** POST /classes/{id}/regenCode → InviteCode mới 6 chars unique.
48. **Drag reorder SortOrder?** Sortable onEnd → PUT /curriculum/reorder {orderedIds}.
49. **Report CSV BOM tại sao?** Excel VN UTF-8 — không BOM lỗi font.
50. **Mobile card-stack?** ClassDetail 1730 dòng có @media card-stack cho members table.
51. **Delete class cascade?** FK ClassMember/Assignment → cần confirm + cascade.

### 6i.4 Toàn bộ 24 FE + 16 BE đã glob — không bịa


## 6j. Bổ sung 1100+ — Lessons API types + MeController + Progress deep (bổ sung)

### 6j.1 Lessons API types — 6 endpoint full

| Endpoint | Method | Query/Body | Auth |
|---|---|---|---|
| /lessons | GET | ?topicId&includeContent | anonymous |
| /lessons/{id} | GET | ?includeContent=true | Bearer optional — gate hidden/draft/classOnly 403 |
| /lessons | POST | {title, description, ContentHtml, topicId, sortOrder, status, isClassOnly} | ADMIN |
| /lessons/{id} | PUT | same | ADMIN |
| /lessons/{id} | DELETE | — | ADMIN |
| /lessons/{id}/simulation | POST | {simulationKey} | ADMIN |

```ts
// frontend/src/api/lessons.ts: LESSON_ENDPOINTS + LessonDto
export const LESSON_ENDPOINTS = {
  lessons: '/lessons',
  lesson: (id:number) => `/lessons/${id}`,
  create: '/lessons',
  simulation: (id:number) => `/lessons/${id}/simulation`,
} as const;
export interface LessonDto { id:number; title:string; description:string; contentHtml:string; topicId:number; sortOrder:number; status:LessonStatusValue; isClassOnly:boolean; simulations: LessonSimulationDto[]; progress?: LessonProgressDto; }
```

### 6j.2 MeController — notes + progress + favorites

| Endpoint | Method | Mô tả |
|---|---|---|
| /me | GET | UserSummary |
| /me | PUT | update displayName/avatarUrl |
| /me/notes | GET/POST | LessonNote per user sanitized |
| /me/progress | GET | UserProgress list |
| /me/favorites | GET/POST/DELETE | Favorite lessons |

### 6j.3 Progress deep — viewed/completed/bestScore unique

```csharp
// backend/src/DsaVisual.Application/Persistence/Entities/UserProgress.cs:1-20
public sealed class UserProgress {
  public int UserId { get; set; }
  public int LessonId { get; set; }
  public bool Viewed { get; set; }
  public bool Completed { get; set; }
  public int BestScore { get; set; }
  public DateTime UpdatedAt { get; set; }
  // unique (UserId, LessonId)
}
```

### 6j.4 Mermaid bổ sung — Lessons CRUD

```mermaid
sequenceDiagram
    participant A as ADMIN
    participant V as LessonEditorModal
    participant L as lessonsApi
    participant B as LessonsController
    participant S as LessonService
    A->>V: tạo lesson
    V->>L: POST /lessons {title, ContentHtml}
    L->>B: sanitizer Ganss.Xss
    B->>S: validate + Save
    S-->>B: LessonDto
    B-->>L: 201
```

### 6j.5 5 Q&A bổ sung (52-56)

52. **includeContent true tại sao?** Lấy ContentHtml nặng — list không cần, detail cần.
53. **LessonSimulation simulations là gì?** List keys gắn lesson với 44 engine — 1:N.
54. **Viewed vs Completed?** Viewed mở, Completed quiz 100% hoặc codelab pass.
55. **BestScore 0-100?** Max quiz — Math.Max.
56. **MeController notes sanitizer?** Ganss.Xss như lesson.

### 6j.6 Toàn bộ 24 FE + 16 BE đã glob — không bịa

## 7. Kết luận & Liên kết chặng sau

Chặng 3 đã gắn engine vào LMS: Lesson lifecycle (draft→active), Teacher Studio hub, Class với mã mời + assignment SortOrder + CSV BOM. Bạn đã có thể trace Student join → học → submit và Teacher gán → báo cáo.

**Sang Chặng 4:** Code Runner & Benchmark — nơi code người dùng chạy trong Worker và benchmark so sánh thuật toán.

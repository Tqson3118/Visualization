# Đáp Án — Đề 07: Practice Ladder, Lab & Exercise

---

## PHẦN I — TRẮC NGHIỆM

### Câu 1 — Đáp án: **B**
**Lý do:** Source `LadderView.vue` dòng 41–45 khai báo tường minh:
```ts
const STAGE_WEIGHTS = [
  { stage: 'Quiz',  weight: '20%', index: '01' },
  { stage: 'Lab',   weight: '30%', index: '02' },
  { stage: 'Code',  weight: '50%', index: '03' },
] as const;
```
Thứ tự Quiz 20% → Lab 30% → Code 50%.

---

### Câu 2 — Đáp án: **C**
**Lý do:** `loadLadderExercises()` dòng 55–58:
```ts
const [quizList, codeList] = await Promise.all([
  exercisesApi.fetchExercises({ nodeId: node, stage: 1 }),
  exercisesApi.fetchExercises({ nodeId: node, stage: 3 }),
]);
```
Hai lời gọi `stage:1` và `stage:3` chạy **song song** bằng `Promise.all`. Không có `stage:2` trong `loadLadderExercises` (stage 2 = Lab, có route riêng `/ladder/:nodeId/lab`).

---

### Câu 3 — Đáp án: **C**
**Lý do:** Catch block dòng 63–66:
```ts
} catch {
  // API lỗi → giữ null; LadderShell hiện EmptyState thay vì crash
  quizExercise.value = null;
  codeExerciseId.value = null;
}
```
Ứng dụng **không crash**, không chuyển hướng — giữ `null` và để `LadderShell` tự xử lý EmptyState.

---

### Câu 4 — Đáp án: **B**
**Lý do:** `ExerciseView.vue` dòng 29 & 123–124:
```ts
const practiceMode = ref(false);
// ...
@click="practiceMode = !practiceMode"
```
Click toggle trực tiếp biến `practiceMode` (ref local). Prop `practiceMode` được truyền xuống component `<QuizStage>` — không có API call nào.

---

### Câu 5 — Đáp án: **C**
**Lý do:** `LabView.vue` dòng 37–38, `INFO_CARDS[1].text`:
> "Đưa dãy về đúng trạng thái cuối theo chuẩn (sắp xếp tăng dần). Số bước dùng không vượt quá chuẩn × 1.5."

`LabStage` truyền prop `:standard-steps="8"`, tức giới hạn = 8 × 1.5 = 12 bước.

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG

### Câu 6 — Trace khởi tạo LadderView khi vào `/ladder/3`

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `onMounted` (hook) | Hook mount kích hoạt, `nodeId` = computed `'3'` từ `route.params.nodeId` |
| **2** | `lessonStore.fetchTopics()` | Gọi API lấy danh sách topics; lấy `first.id` gán vào `topicId` để breadcrumb link đúng |
| **3** | `loadLadderExercises()` | `Promise.all([fetchExercises({nodeId:3, stage:1}), fetchExercises({nodeId:3, stage:3})])` — fetch song song quiz + code exercises |
| **4** | `quizLoading.value = false` (finally) | Kết thúc loading → template render `<LadderShell>` với `quizExercise` và `codeExerciseId` đã điền |

**Lưu ý bổ sung:** Nếu `quizList` rỗng → `quizExercise = null`; `codeExerciseId = codeList[0]?.id ?? null`.

---

### Câu 7 — Trace khi click "Lịch sử làm bài" trong ExerciseView

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `loadHistory()` (hàm) | Hàm được gọi khi click Button "Lịch sử làm bài"; lấy `id = Number(route.params.id)` |
| **2** | `historyOpen.value = true` + `historyLoading.value = true` | Mở Drawer bên phải (`side="right"`, `size="md"`); bật spinner loading |
| **3** | `exercisesApi.fetchMySubmissions(id)` | Gọi API GET lịch sử nộp bài của user cho bài tập có `id`; kết quả gán vào `historyItems` |
| **4** | `historyLoading.value = false` (finally) | Kết thúc loading → Drawer render danh sách `historyItems`; mỗi item hiển thị `formatDateTime(submittedAt)` + Badge "Đạt/Chưa đạt" theo `score >= 70` |

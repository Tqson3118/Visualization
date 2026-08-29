# ĐÁP ÁN — Đề 12: Classes & Studio

---

## PHẦN I — TRẮC NGHIỆM

| Câu | Đáp án | Giải thích |
|-----|--------|-----------|
| 1 | **B** | `onInviteInput`: `.toUpperCase()` + replace `/[^A-Z0-9]/g, ''` + `.slice(0, 6)`. Chỉ A-Z và 0-9, tối đa 6 ký tự, tự viết hoa. VD hợp lệ: `DSA999`. |
| 2 | **C** | `const isTeacher = computed(() => auth.role === 'TEACHER' \|\| auth.role === 'ADMIN')`. Cả TEACHER và ADMIN đều thấy nút "Tạo lớp". |
| 3 | **C** | ClassDetailView có 4 tab: `members / assignments / curriculum / settings`. Xem `tab` ref khai báo: `ref<'members' \| 'assignments' \| 'curriculum' \| 'settings'>('members')`. |
| 4 | **C** | `GET /classes/{id}/report/excel` — handler trong `ClassesController`. Nút Download ở ClassDetailView (icon `Download` từ lucide). |
| 5 | **C** | Tab 3 "Ý kiến học viên" import `courseApi` và type `CourseFeedbackDto` → kết nối với `CourseFeedbackController`. |

---

## PHẦN II — TỰ LUẬN

### TL-1: Teacher tạo lớp & gán bài tập *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi ý.

1. **Điều kiện hiển thị nút "Tạo lớp":**
   ```typescript
   const isTeacher = computed(() => 
     auth.role === 'TEACHER' || auth.role === 'ADMIN'
   )
   // v-if="isTeacher" trên nút Tạo lớp
   ```

2. **Mã mời (invite code):**
   - Format: 6 ký tự, chỉ `A-Z` và `0-9` (ví dụ: `DSA999`, `ABC123`)
   - Được server sinh tự động khi tạo lớp
   - `onInviteInput` tự động: uppercase + filter ký tự lạ + giới hạn 6 ký tự

3. **Học sinh nhập mã → join:**
   - Nút "Nhập mã lớp" → `joinOpen = true` → Modal nhập mã
   - Học sinh nhập 6 ký tự mã → Submit → gọi `classStore.joinClass(code)`
   - API: `POST /classes/join` với body `{ code: "DSA999" }`
   - `onMounted`: `classStore.fetchClasses()` → `GET /classes`

4. **Gán bài (Assignment):**
   - Tab **`assignments`** trong ClassDetailView
   - Nút "Gán bài" → modal `assignOpen = true`
   - `assignType` ref quyết định loại bài gán

5. **`assignType` nhận giá trị:**
   - `'lesson'` — gán bài học
   - `'exercise'` — gán bài tập quiz
   - Khi submit: `POST /classes/{id}/assignments` với body:
     ```json
     { "type": "lesson|exercise", "itemId": ..., "dueDate": "...", "allowLate": true }
     ```

---

### TL-2: Teacher soạn bài & publish *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi ý.

1. **Route & Roles:**
   - Route: `/studio`
   - Roles: `['TEACHER', 'ADMIN']` — TEACHER_PENDING và STUDENT bị redirect

2. **Tab 1 — Modal builders:**
   - `CourseBuilderModal` — tạo/sửa Lộ trình (Course)
   - `ExerciseBuilderModal` — tạo/sửa Bài tập Quiz

3. **AdminLessonEditorView:**
   - Route: `/studio/lessons/new` (tạo mới) hoặc `/studio/lessons/:id/edit` (sửa)
   - Roles: `['TEACHER', 'ADMIN']`
   - Teacher soạn nội dung bài học dưới dạng **Markdown**

4. **Format soạn thảo:**
   - **Markdown** — có preview realtime
   - Hỗ trợ code block, heading, list, image embed

5. **Gán bài vào lớp sau khi lưu:**
   ```
   AdminLessonEditorView → lưu bài → bài có ID
       ↓
   ClassDetailView → tab "curriculum" → "Nhập từ Lộ trình"
       → openImportCourseModal() → chọn Course → importCourseToClass()
   HOẶC
   ClassDetailView → tab "assignments" → Gán bài
       → assignType = 'lesson' → chọn lessonId → submit
   ```

**Lưu ý nâng cao:** ClassDetailView import `courseApi.getCourses()` trong `openImportCourseModal()` để lấy danh sách lộ trình có sẵn → Teacher chọn → `classesApi.importCourseToClass(classId, courseId)`.

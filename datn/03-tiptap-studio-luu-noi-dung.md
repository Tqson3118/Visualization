# 03. Tiptap/Studio lưu nội dung

## Flow
1. Teacher/Admin vào `/studio`; router kiểm tra role.
2. Chọn lesson trong cây giáo trình.
3. Editor load nội dung từ API.
4. Tiptap giữ trạng thái editor ở frontend.
5. Teacher sửa theory, simulation, CodeLab, quiz hoặc settings.
6. Bấm lưu; frontend serialize dữ liệu rồi gửi API.
7. Backend kiểm tra ownership, validate và lưu database.
8. Nội dung Draft/Active được xử lý theo trạng thái xuất bản.

## Các tab
- Theory: rich text, Markdown, LaTeX, preview.
- Simulation: chọn `SimulationKey`.
- CodeLab: starter code, solution, public/hidden tests.
- Quiz: câu hỏi, options, đáp án, explanation.
- Settings: Draft/Active.

## Code cần tra
- `frontend/src/views/AdminContentView.vue`
- `frontend/src/views/admin/editor-tabs/TheoryTab.vue`
- `frontend/src/views/admin/editor-tabs/SimulationTab.vue`
- `frontend/src/views/admin/editor-tabs/CodeLabTab.vue`
- `frontend/src/views/admin/editor-tabs/QuizTab.vue`
- `backend/src/DsaVisual.Api/Controllers/LessonsController.cs`
- `backend/src/DsaVisual.Application/Services/LessonService.cs`

## Câu hỏi sâu
Tiptap lưu JSON, HTML hay Markdown? Hãy kiểm tra editor dùng `getJSON()`, `getHTML()` hay Markdown serializer. Phân biệt format nội bộ với field database như `ContentHtml`/`contentMd`.

## Checklist phải học thuộc
Load editor → sửa state → serialize → gửi DTO → validate ownership → lưu lesson/exercise/question → publish status → student đọc.

## Cách tra code
Tìm component chứa useEditor/EditorContent, tìm hàm save và API gọi; đối chiếu field gửi đi với DTO và property entity. Sau đó tìm nơi render ContentHtml/contentMd.

## Câu hỏi khó
Nếu save giữa chừng thất bại thì UI có đánh dấu đã lưu không? Nếu teacher sửa lesson khác thì backend chặn ở đâu? Nếu hai tab cùng sửa thì last-write hay concurrency token? Hãy tìm code thay vì đoán.

## 8. Flow Studio từ UI đến API

1. Bắt đầu tại `frontend/src/views/admin/AdminLessonEditorView.vue:300-370`: đọc form, watcher và draft auto-save. Dòng 327 là `saveDraftDebounced`; dòng 352 trở đi là watcher gọi autosave local. Đây chưa chắc là save database.
2. Tìm hàm save thật trong cùng file quanh `500-535`: dòng 519 tạo payload, dòng 529 gọi `lessonsApi.updateLesson(...)`.
3. Sang `frontend/src/api/lessons.ts:120`: đây là hàm HTTP update lesson; đọc URL/method/body ở các dòng kế tiếp.
4. Backend lần lượt tra LessonsController action update → LessonService method update → DTO/Validator. Dùng Ctrl+F `Update`, `PUT`, `PATCH`, `ContentHtml`, `contentMd`.
5. Response lesson trả về API → Promise ở `lessons.ts` resolve → AdminLessonEditorView cập nhật saving/save status/toast và dữ liệu form.
6. Với curriculum, bắt đầu `frontend/src/views/admin/sections/StudioCurriculumTab.vue:704-706` (nút save), sang `handleSavePath` quanh dòng 472 và API tương ứng.

**Điểm cần phân biệt:** autosave draft ở frontend không đồng nghĩa đã lưu DB. Hãy ghi riêng flow localStorage và flow nút Save gọi backend.

**Tiptap:** tìm trong editor component bằng `useEditor`, `EditorContent`, `getHTML`, `getJSON`; nếu không nằm ở file View thì lần theo component con được import.

## Flow diễn giải bằng lời
Teacher mở editor; `AdminLessonEditorView.vue:300-370` theo dõi form và autosave draft local. Khi bấm lưu thật, payload được tạo quanh dòng 519 và gọi `lessonsApi.updateLesson` tại dòng 529. API wrapper nằm ở `frontend/src/api/lessons.ts:120`; request tiếp tục vào LessonsController rồi LessonService để kiểm tra quyền, validate và ghi database. Response quay lại View, UI cập nhật save status/toast. Nếu lỗi, nội dung local vẫn giữ để thử lại. Khi học Tiptap, tìm `useEditor`, `EditorContent`, `getHTML`, `getJSON` để biết chính xác dữ liệu được serialize thế nào.

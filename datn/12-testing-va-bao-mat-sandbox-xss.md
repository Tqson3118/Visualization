# 12. Testing và bảo mật sandbox/XSS

## Testing cần nhớ
- Unit test: kiểm tra service/judge độc lập.
- Integration test: API + database.
- E2E test: thao tác thật từ UI.
- Regression test: bảo vệ bug đã sửa.

## Sandbox/code execution
Code người dùng là untrusted input. Cần:
- timeout;
- giới hạn CPU/memory/output;
- giới hạn số dòng hoặc kích thước source;
- không cho truy cập filesystem/network/secret;
- tách process/worker phù hợp;
- không trả hidden expected output ra client.

## XSS trong lesson/Tiptap
Không render HTML chưa sanitize bằng `v-html`. Sanitize khi lưu hoặc trước khi render; validate URL ảnh/link; không cho script/event handler lọt vào nội dung.

## Code cần tra
- `frontend/tests/e2e/security.spec.ts`
- `frontend/src/core/__tests__/`
- `backend/tests/DsaVisual.UnitTests/CodelabJudgeTests.cs`
- `backend/tests/DsaVisual.IntegrationTests/`
- `backend/src/DsaVisual.Application/Validators/`
- `frontend/src/views/lesson/` nơi render content.

## Câu hỏi bảo vệ
Frontend test không thay thế backend security. Hidden test, quyền truy cập, timeout và sanitize phải được kiểm soát ở server/tầng tin cậy.

## Checklist phải học thuộc
Mỗi security feature phải có threat, boundary, biện pháp chặn và test chứng minh. Tập trung timeout, resource limit, hidden tests, auth, input validation và sanitize HTML.

## Cách tra code
Đọc CodelabJudgeTests rồi integration test endpoint; đọc security E2E cho XSS; lần tới validator và nơi render lesson.

## Câu hỏi khó
Frontend sanitize có đủ không? Không; backend/render boundary cũng phải sanitize. Timeout của browser có giết được process server không? Không nhất thiết; execution boundary phải tự quản lý.

## 8. Flow test từ thao tác đến UI

**Unit/backend:** bắt đầu `backend/tests/DsaVisual.UnitTests/CodelabJudgeTests.cs`; đọc Arrange input → gọi judge/service → Assert status/output. Không có UI trong flow này.

**Integration:** bắt đầu `backend/tests/DsaVisual.IntegrationTests/`; lần request tới controller, database assertion và response status/body.

**E2E/UI:** bắt đầu `frontend/tests/e2e/security.spec.ts` hoặc code-runner spec; đọc page.goto → click/fill → wait response → expect text/DOM.

**Security flow CodeLab:** UI submit → API → judge sandbox timeout/resource limit → status response → UI hiển thị lỗi, không treo.

**Security flow XSS:** teacher/student nhập content → API lưu/validate hoặc render sanitize → browser nhận HTML an toàn → E2E assert script không execute. Tra component render lesson và mọi chỗ dùng `v-html`.

**Cách học mỗi test:** ghi input độc hại/biên, lớp bảo vệ, HTTP status hoặc UI assertion. Một test tốt phải chứng minh cả “bị chặn” và không tạo side effect nguy hiểm.

## Flow diễn giải bằng lời
Unit test gọi trực tiếp judge/service và assert output/status. Integration test gửi request qua controller, chạy service/database rồi assert response và side effect. E2E thao tác browser: mở trang, nhập, bấm nút, chờ API và assert DOM. Với CodeLab, source user đi từ UI tới sandbox có timeout/resource limit rồi mới trả status; frontend chỉ hiển thị kết quả. Với Tiptap/feedback, content đi qua validation/sanitize trước khi render; E2E kiểm tra script không chạy.

# Kế hoạch triển khai — Xử lý list test của tester + các mục đã chốt

> Ngày lập: 01/09 · Trạng thái: **chờ duyệt** · Ảnh chụp minh chứng: `test-results/ux-audit/`
> App dev đang chạy tại http://localhost:5173 (backend :5000, DB cloud) — bạn có thể tự kiểm tra.

---

## 0. Kết quả chạy thử trực tiếp (đã khởi động app + đo bằng DOM)

| # | Vấn đề tester | Kết quả kiểm chứng |
|---|---|---|
| L-9 | "khối bị làm hẹp vị trí khi chạy thử mô phỏng trực quan" | ✅ **Tái hiện được nguyên nhân**: player mô phỏng render **inline trong panel slide-over hẹp (~800px)** — player chỉ được 700px, chèn ép nội dung editor (ảnh `sim-player-state.png`) |
| L-3 | Nút "Chạy thử thuật toán" không dùng được | ❌ **Không tái hiện được**: bài Binary Search bấm chạy bình thường, canvas render có nội dung, console sạch. Cần tester chỉ rõ bài/key mô phỏng nào lỗi |
| L-10 | Bỏ chọn mô phỏng nhưng vẫn hiển thị trong bài | ✅ **Tìm thấy gốc rễ trong code**: khi lưu, key mô phỏng được lấy từ **hợp của list đính kèm + các thẻ `[Mô phỏng: key]` còn sót trong nội dung** (`ItemEditorSlideOver.vue:596`). Gỡ chip chỉ xóa trong list, **không xóa thẻ trong bài giảng** → lưu lại là tự gắn vào |
| S-5 | "Có 2 nút lưu trong phần chỉnh sửa nội dung" | ✅ Xác nhận: slide-over có nút **"Lưu"** (lưu mục) + thanh công cụ có **"Lưu lộ trình"** (lưu cây) — 2 khái niệm dễ gây nhầm |
| S-6 | Nút "Lưu lộ trình" không hoạt động, chọn Nháp tự lưu | ⏳ Chưa kịp tái hiện — sẽ điều tra khi fix (nghi vấn: autosave nháp ghi đè trạng thái xuất bản) |
| — | Form `/help` | ⚠️ **Form liên hệ hiện là GIẢ** — không gọi API nào, chỉ validate client rồi hiện "đã gửi" (`HelpView.vue:33`). Không lưu dữ liệu |

---

## A. Bug ưu tiên CAO (dễ mất dữ liệu / sai trạng thái)

### A1. Nút "Lưu lộ trình" + tự lưu nháp (S-6)
- Điều tra handler của nút **Lưu lộ trình** và cơ chế **autosave nháp**; đảm bảo: bấm Lưu mới ghi thay đổi trạng thái (Nháp ↔ Công khai), autosave chỉ lưu nội dung nháp và **không tự đổi trạng thái**.
- Thêm toast phản hồi khi lưu thành công/thất bại (nghi tester không thấy gì nên tưởng nút hỏng).

### A2. Bỏ chọn mô phỏng không có hiệu lực (L-10)
- `removeSimulation()` phải **xóa cả thẻ `[Mô phỏng: key]`** (và tiêu đề 🎮 đi kèm) khỏi nội dung bài giảng.
- Khi lưu: chỉ lưu `attachedSimulations`, không hợp lại với thẻ cũ (hoặc hợp nhưng đồng bộ 2 chiều khi gỡ).

### A3. Đồng bộ trạng thái unlock lộ trình (L-1)
- Học xong bài/quiz nhưng ngoài trang lộ trình + chi tiết khóa không mở mục tiếp theo.
- Kiểm tra: API tiến độ trả về gì, store khóa học có re-fetch sau khi hoàn thành bài không, điều kiện unlock tính ở đâu (frontend hay backend).
- Fix: sau `completeLesson` → invalidate/refetch tiến độ; điều kiện `isUnlocked` dùng dữ liệu tiến độ mới nhất.

## B. Bug ưu tiên TRUNG BÌNH

### B1. "Bài học cập nhật gần đây" — stale filter + không refresh (S-3, S-4)
- Reset query/state danh sách khi rời màn hình sửa bài (quay lại phải hiện "xem toàn bộ").
- Refetch danh sách sau khi lưu bài.

### B2. Hai nút lưu gây nhầm (S-5)
- Đổi nhãn cho phân biệt: nút trong slide-over → **"Lưu bài học"**, thanh công cụ → **"Lưu lộ trình"**; cân nhắc gộp: lưu bài học xong tự cập nhật cây.

### B3. Chạy thử mô phỏng bị chèn ép (L-9)
- Thay vì render player inline trong panel ~800px → mở **modal/overlay rộng** (dùng lại `InlineSimulationPlayer`, khung ~90% màn hình). Ảnh chứng: `sim-player-state.png`.

### B4. Nút "Chạy thử thuật toán" phía học viên (L-3)
- Không tái hiện được. Việc cần làm: (1) thêm **fallback** — nếu embedded player lỗi/WebGL fail thì hiện link "Mở trong tab mới"; (2) nhờ tester chỉ rõ bài + trình duyệt. Không sửa mù.

### B5. i18n "chờ duyệt teacher" (S-8)
- Sửa chuỗi thành "Chờ duyệt giáo viên".

### B6. Tìm kiếm người dùng quá khắt khe (S-7)
- Áp dụng `normalizeVi` (đã có sẵn) cho ô tìm user: khớp chứa chuỗi + bỏ dấu.

## C. Tính năng / thay đổi ĐÃ DUYỆT

### C1. Lộ trình: phân nhóm theo chủ đề — **phương án B** ✅
- Thêm **tiêu đề nhóm theo chủ đề** (group header theo `topicId`) trên lưới khóa học; giữ nguyên bộ lọc + tìm kiếm.
- File: `frontend/src/views/courses/CoursesListView.vue` + store. Ước lượng ~1–2h.
- Trả lời tester: phân chia theo chủ đề bị thay bằng bộ lọc từ 18/08 (commit `03545ef`), nay trả lại dạng tiêu đề nhóm.

### C2. "Xem & Chọn Mô phỏng" trong Studio ✅
- Thực tế modal **đã có 2 nút "Đính kèm"/"Chèn"** nhưng bị khuất + click card chỉ xem trước → tester tưởng không chọn được.
- Fix: (1) đổi nhãn nút mở modal thành **"Chọn Mô phỏng"**; (2) click card = **chọn ngay** (checkmark), giữ nút xem trước riêng; (3) đưa nhóm nút hành động lên vị trí luôn thấy được.
- File: `components/studio/SimulationPickerModal.vue`, `ItemEditorSlideOver.vue:1010`.

### C3. "Bắt đầu học" → vào bài đang học dở ✅
- Course detail: nếu có tiến độ, nút "Bắt đầu học" đổi thành **"Học tiếp: <tên bài>"** → `/lessons/:id` của bài chưa hoàn thành đầu tiên; mới hoàn toàn thì vào bài 1.

### C4. Admin đặt lại mật khẩu — chỉ TRẢ LỜI tester, không code mới ✅
- Xác nhận với bạn: **đúng — admin chỉ ĐẶT MẬT KHẨU MỚI cho user, không ai xem được mật khẩu cũ** (BCrypt băm 1 chiều). Đặt mật khẩu mới xong hệ thống tự đăng xuất mọi thiết bị của user đó.
- Tính năng đã có sẵn (Quản lý người dùng → Đặt lại mật khẩu). UI hiện chặn reset pass của **admin khác** — giữ nguyên quy tắc này (an toàn), sẽ nêu rõ trong câu trả lời cho tester.

### C5. Phản hồi học viên: 2 trạng thái ✅
- Gộp hiển thị thành **Chưa xử lý** (New/Read) và **Đã xử lý** (Resolved) — ở cả Studio feedback tab và cài đặt nền tảng.

### C6. Vai trò "chờ duyệt GV" — không thêm ✅
- Người chờ duyệt chỉ nằm ở **tab duyệt giáo viên**. Trả lời tester theo hướng này.

### C7. Luồng phản hồi: **GV xử lý feedback lộ trình — Admin xử lý feedback /help** ✅
- **Feedback lộ trình (ý kiến HV cho GV):** giữ nguyên luồng hiện tại — GV trả lời trực tiếp trong Studio, **bỏ vai trò admin chỉnh sửa** phần này. Vì chỉ GV edit được nên không còn vấn đề "admin duyệt ghi đè câu trả lời của GV" (câu trả lời của GV luôn nguyên vẹn, nằm ngay thread đó, GV tự quản).
- **Feedback /help:** kết nối form `/help` (hiện đang là form giả) tới backend:
  - Backend: đề xuất **tận dụng BugReport** (`POST /api/v1/bug-reports`) — đã có danh sách admin + sanitize XSS + trạng thái; chỉ cần cho phép gửi **kể cả khách** (UserId nullable + lưu Name/Email vào Context).
  - **Cài đặt nền tảng → "Báo cáo & Ý kiến"** (`AdminSettingsView.vue`): chuyển nguồn dữ liệu từ course-feedback (hiện đang **trùng dữ liệu** với tab của GV) sang kênh /help.
  - Áp rule đã chốt (mục 8): chưa xử lý → luôn "Đang xử lý"; xử lý xong → **khóa chỉnh sửa** (UI ẩn ô trả lời + backend từ chối PUT); giữ nút **"Mở lại"** cho admin (bạn có thể bỏ nếu muốn khóa tuyệt đối).
- ⚠️ Điểm cần bạn gật đầu: cho khách (chưa đăng nhập) gửi được form /help không, hay bắt buộc đăng nhập?

---

## D. Thứ tự thực hiện đề xuất

| Lượt | Việc | Ước lượng |
|---|---|---|
| 1 | A1 + A2 + B2 (cụm Studio lưu/autosave/mô phỏng — cùng 1 vùng code) | ~1 buổi |
| 2 | A3 (unlock lộ trình) | ~2–3h |
| 3 | B1 + B5 + B6 (nhanh) | ~2h |
| 4 | C1 + C3 (lộ trình phía học viên) | ~2–3h |
| 5 | C2 + B3 (Studio UX mô phỏng) | ~2–3h |
| 6 | C7 (kênh /help + khóa chỉnh sửa) | ~1 buổi |
| 7 | C4, C5, C6, soạn câu trả lời tester | 30' |

## E. Việc cần tester bổ sung
1. **L-3**: bài nào / nút nào / trình duyệt gì khi nút "Chạy thử thuật toán" không chạy (kèm screenshot nếu được).
2. **L-9**: screenshot màn hình bị hẹp nếu khác với vị trí tôi tái hiện (slide-over soạn bài → mục "Mô phỏng Trực quan đính kèm" → bấm "Chạy thử tại đây").
3. **S-6**: bước thao tác cụ thể khiến "chọn nháp chưa bấm lưu đã tự lưu".

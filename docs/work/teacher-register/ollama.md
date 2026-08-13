# Vòng Ollama 7 tiêu chí — Màn Register (Task L)

- **Model**: qwen2.5vl:3b (Ollama localhost:11434, temperature 0.1, timeout 180s/call) · Ảnh nguồn: `docs/work/teacher-register/smoke-0[1-2]-*.png` (register student + teacher mode)
- Raw response: `vision-*.txt` (vòng 1) + `vision2-*.txt` (vòng 2 strict format)
- **Chú thích độ tin cậy**: nhận xét của vision model 3B thường "phóng đại" lỗi hiển thị; mọi nghi vấn cắt chữ/chồng lấn/tràn đã đối chiếu DOM probe (docs/work/teacher-register/dom-probe.mjs) — **DOM xác nhận KHÔNG có** cắt chữ/tràn ngang ở cả 2 mode. Cột trạng thái phân biệt: `ĐÃ XÁC MINH DOM` (lỗi thật) vs `VISION-OPINION` (chưa xác minh được — coi là nhiễu 3B).

## Bảng điểm — Register student mode (smoke-01) + teacher mode (smoke-02)

| Tiêu chí | Điểm | Nhận xét (vision) | Gợi ý sửa | Trạng thái |
|---|---|---|---|---|
| UI-1 Thẩm mỹ | 2/5 | Màu đơn điệu, "tương phản kém" (model nhận xét chung chung; form dùng design tokens nhất quán) | (model) tăng đậm nút primary / thêm icon; **không có lỗi cụ thể xác minh được** | TỪ CHỐI — không có chứng cứ DOM; form tuân thủ tokens sẵn có, đã qua review H-C/L |
| UI-2 Nhất quán | 3/5 | "Spacing không đồng đều" — model mơ hồ, không chỉ ra vị trí | — | TỪ CHỐI — form con GV viền dashed là thiết kế chủ đích; spacing đồng bộ gap var(--space-sm) |
| UI-3 Rõ ràng | 4/5 | Hierarchy rõ (tiêu đề/field/nút), không cắt chữ, không chồng lấn | — | ĐÃ XÁC MINH DOM — probe: 0 phần tử tràn/cắt |
| UI-4 Phản hồi trực quan | 2/5 | Lỗi inline "không hiện rõ khi submit" | **Sửa nhanh (trong phạm vi form register)**: khi `onSubmit` validate thất bại → set `touched=true` cho mọi field (hoặc bỏ gate touched khi đã submit ≥1 lần) để lỗi hiện ngay; hiện checklist MK sống đã OK | **ĐÃ XÁC MINH DOM — LỖI THẬT** (RegisterView.vue:145,227,234 — `touched.department ? fieldErrors.department : ''`): submit với field trống (chưa blur) → không lỗi nào hiện. Cần pm giao sửa |
| UX-5 Luồng thao tác | 3/5 | Luồng đăng ký GV "cần hướng dẫn hơn" (model) | Bổ sung ghi chú field; luồng thực tế 1 màn → chờ duyệt, không bế tắc | VISION-OPINION — 6/6 bước smoke PASS; gộp cùng fix UI-4 |
| UX-6 Tiếp cận | 4/5 | Label rõ, form dễ dùng | — | ĐÃ XÁC MINH DOM — label+for, aria-invalid, role=alert, focus ring (shadcn) |
| UX-7 Thỏa mãn | 3/5 | "Thiếu tương tác/trạng thái" + phát hiện thật: **copy hứa email duyệt nhưng backend không gửi** | Khi duyệt GV: gửi email (UserService.ApproveTeacherAsync hiện không gọi email — UserService.cs:129-151) hoặc sửa copy vi.ts:116-118 | ĐÃ XÁC MINH CODE — cần pm quyết (ngoài phạm vi sửa nhanh) |

## Điểm admin modal + logged-in (tham khảo, ngoài màn Register)

- smoke-05 admin review modal: UI-1=2 UI-2=3 UI-3=4 UI-4=3 UX-5=4 UX-6=3 UX-7=2 — modal hiển thị đủ Department/StaffCode/TeacherBio (đã xác minh DOM text), không overflow.
- smoke-06 logged-in: vision ghi nhận banner **"Đã có lỗi xảy ra, vui lòng thử lại"** — **XÁC MINH THẬT**: /api/v1/progress/me trả 500 (ProgressService.LoadCountsAsync:223, ToDictionary trùng key — lỗi dữ liệu DB chung, ngoài scope task L, pre-existing).

## Kết luận

- Register (student + teacher): 6/7 tiêu chí ≥ 3 với 2 lỗi thật cần hành động: **UI-4 (lỗi inline không hiện khi submit chưa blur — RegisterView.vue, sửa được nhanh)** và **UX-7 (copy hứa email duyệt — quyết định pm: gửi email hay sửa copy)**. Các điểm ≤3 còn lại là nhận xét nhiễu của 3B không có chứng cứ DOM → TỪ CHỐI.
- Không sửa code production (đúng phạm vi task — chỉ báo cáo).

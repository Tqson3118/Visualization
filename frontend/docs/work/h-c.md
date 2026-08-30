# H-C — Polish 3 màn lớp học (Classes / ClassDetail / ClassReport)

Nhánh: `feature/ux-h-c` (từ `dev`) · Subagent: Dev UX · Ngày: 13/08/2026

## Phạm vi
- `frontend/src/views/ClassesView.vue` (Màn 19)
- `frontend/src/views/ClassDetailView.vue` (Màn 20)
- `frontend/src/views/ClassReportView.vue` (Màn 21)
- `frontend/src/i18n/vi.ts` — chỉ THÊM khối `classes.*` (96 chuỗi), không sửa/xóa chuỗi cũ.

KHÔNG đổi contract API, KHÔNG đổi logic store, KHÔNG đụng engines.

## Điểm nâng cấp chính

| View | Nâng cấp |
|---|---|
| ClassesView | Hero gradient Sunset (pattern LessonView + dark overlay GP-T9b), Card shadcn + hover-lift thay card cũ, avatar icon gradient, desc line-clamp-2, invite-code chip dashed mono (chỉ manager), skeleton theo grid, EmptyState giữ action, modal Join/Create giữ logic. **Fix UX thật**: input mã mời giờ tự viết hoa + lọc ký tự + chặn >6 (hàm normalize trước đây không được nối vào input). |
| ClassDetailView | Hero Sunset + chip trắng trong suốt: mã mời (KeyRound + code + nút Sao chép với phản hồi Check 1.6s), chip số thành viên, nút Báo cáo lớp. Tabs shadcn (Tabs wrapper) thay tab tự chế. Bảng thành viên chuẩn AdminUsers (avatar gradient-mint, hover row, ellipsis, scroll-x). Lộ trình đã gán: card row icon tint theo loại (lesson=mint/exercise=sunset/chung=aurora) + CalendarClock hạn nộp + Badge trạng thái. Cài đặt: danger zone card + **Modal xác nhận xóa thay window.confirm**. Giữ 3 modal cũ + thêm id label cho datetime-local. |
| ClassReportView | Hero Sunset compact (icon kính + tiêu đề + tên lớp/ID + Xuất CSV/In). 4 KPI Card shadcn + hover-lift + icon gradient tint (chuẩn AdminStats) + tabular-nums. Summary ProgressBar. Bảng sinh viên chuẩn AdminUsers (avatar, status Badge success/warning/muted, scroll-x, số căn giữa). Skeleton KPI + bảng. |

## Verify (số thật)
- `npm run build` (vue-tsc -b + vite build) → 0 lỗi ✓
- `npm test` (vitest run) → 11 files / **89/89 PASS** ✓
- Render check Playwright (spec tạm — đã xóa sau khi chạy): 13/13 PASS
  - 3 màn × light/dark × 1366×768 + 390×844 = 12 tổ hợp: **0 console error, 0 overflow ngang** (scrollWidth - clientWidth ≤ 1)
  - 1 test tương tác: tabs (Lộ trình đã gán / Cài đặt / Thành viên), modal tạo lớp, modal thêm thành viên, modal xóa lớp, sao chép mã mời + toast — 0 lỗi console
  - Screenshots: `docs/work/h-c-*.png` (12 ảnh)

## Ghi chú kỹ thuật
- Tất cả chuỗi UI mới chuyển vào `messages.classes.*` (SDD §3.8.5); toast lỗi từ backend giữ nguyên `err.message`.
- Lỗi vue-tsc gặp phải: computed `detailTabs` cần khai báo kiểu tường minh (push tab có key string không gán được vào union literal) — đã fix.
- Copy mã mời dùng `navigator.clipboard` + fallback toast (như bản cũ); Playwright cần grant clipboard permission khi test.
- Mã mời chỉ hiện cho manager (OWNER/TEACHER) — giữ hành vi gốc.

## Rủi ro / chưa làm
- Chưa chạy vòng Ollama vision review (PM chạy dev-e2e + qwen2.5vl theo quy trình).
- Đề xuất sau: `ClassReportView` có thể thêm donut phân bố trạng thái (SVG như AdminStats) nếu cần đủ "wow" cho demo; màn 20 backlog "chọn lesson/exercise khi gán" giữ nguyên ghi chú backlog.

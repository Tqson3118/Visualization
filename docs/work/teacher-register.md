# Trạng thái task L: Form đăng ký giảng viên

- [13/08 12:35] BE code xong (User+3 cột, RegisterAsync, AdminUserDto, migration 20260813052933_AddTeacherProfileFields) - build PASS, 36/36 test PASS
- [13/08 12:35] FE code xong (RegisterView segmented+form con, AdminUsersView modal, i18n +35 msg) - build PASS, 89/89 test PASS
- [13/08 12:52] REVIEW ĐỘC LẬP (verify lại thật) — VERDICT: ✅ APPROVE (có điều kiện)

## Kết quả verify độc lập (13/08 ~12:52)

| Hạng mục | Kết quả |
|---|---|
| Backend build | PASS, 0 lỗi (2 warning — NU1903 SSH.NET pre-existing, không phải task L) |
| Backend test | 98/98 unit (gồm 13 test mới Auth) + 31/31 integration PASS |
| Frontend build | PASS (bundle không nới: engine 476KB / echarts 323KB) |
| FE spec RegisterView.spec.ts | 5/5 PASS |
| Migration | 3 cột nullable Department(100)/StaffCode(50)/TeacherBio(500) + snapshot đồng bộ ✅ |
| Docs | SRS/SDD/API_REFERENCE/USER_GUIDE/PRODUCTION_PROMPT đã đồng bộ (changelog + cột + payload) ✅ |

## Nhận xét code (ngắn)

- BE: validate đúng trọng tâm (bắt buộc khi IsTeacher=true, trim, ≤100/50/500, bio rỗng→null, student bỏ qua field), dùng ErrorCodes.VALIDATION_FAILED theo pattern repo. ✅
- FE: segmented + form con có counter 500, aria-pressed/aria-invalid, toàn bộ text vào i18n; modal duyệt hiển thị info GV (chỉ khi có). ✅
- Test: 13 test BE phủ biên (thiếu field/whitespace/đúng 100-50-500/student bỏ qua) — không test trùng. ✅

## ⚠ VIỆC CÒN LẠI — cần session L làm nốt (4 mục)

1. **COMMIT + PR base `dev`** (HEAD đang ở base e707b01, chưa commit): commit-as FE→son, BE→bao. Sau khi merge → báo lại để chạy tiếp M.
2. **E2E smoke thật** (chưa chạy — DB có thể bận bởi K/H): apply migration lên DB docker → register GV qua UI/API (VD: `gv.nguyenvana@gmail.com` / Khoa CNTT / GV12345 / bio) → login admin@system.local/Admin@123 → tab "Chờ duyệt Teacher" thấy đủ Department/StaffCode/TeacherBio → duyệt → user login với role TEACHER. Chụp ảnh dark+light lưu `docs/work/teacher-register/` (hoặc ghi kết quả text nếu không chụp được).
3. **Vòng Ollama 7 tiêu chí** cho màn Register (thẩm mỹ/nhất quán/rõ ràng/phản hồi trực quan · luồng/tiếp cận/thỏa mãn — 1-5, ≤3 phải sửa ≤2 vòng): chụp Register (student + teacher mode) → chấm → ghi `docs/work/teacher-register/ollama.md`.
4. **NU1903 SSH.NET (pre-existing, KHÔNG sửa trong task L)** — ghi vào backlog review M (đợt J/M xử lý chung: `docs/work/final-review-2/backlog.md` sau này).

## ⚠ LƯU Ý MÔI TRƯỜNG KHI CHẠY SONG SONG (13/08 — quan trọng cho smoke E2E)

- **Port Vite bị dời tự động**: đang có session khác chạy frontend (5173/5174 bận) → Vite sẽ tự dời port (5175+). **BẮT BUỘC xác định port thật của mình** từ log `Local: http://localhost:XXXX/` và dùng ĐÚNG port đó cho mọi Playwright/screenshot — tránh chụp nhầm app của session khác. Nếu cần port cố định: `npm run dev -- --port 5180 --strictPort`.
- **Backend docker :5000 đang chạy image CŨ (chưa có migration L)** — frontend trỏ `:5000` sẽ không thấy cột Department/StaffCode/TeacherBio → register GV có thể lỗi/thiếu field. **Giải pháp: chạy backend LOCAL từ worktree L ở port khác** (VD `$env:ASPNETCORE_URLS="http://localhost:5001"` + sửa proxy `frontend/vite.config.ts` trỏ `http://localhost:5001` tạm thời cho smoke — nhớ revert trước khi commit) **hoặc** rebuild docker backend (phải chờ K seed xong, DB dùng chung). Khuyến nghị: backend local 5001, không đụng ai.
- Ollama 3.5GB chạy chung 1 server localhost:11434 — 2 session gửi ảnh cùng lúc chỉ bị xếp hàng chậm hơn, **timeout mỗi call ≥ 90-120s**, không sai kết quả.

Sau khi xong 4 mục → cập nhật file này + báo lại (verdict APPROVE, chỉ còn chờ commit/PR).

# VIỆC CẦN NGƯỜI DÙNG LÀM (SETUP_TODO)

> File này do các task trong PM_MASTER_PLAN tự cập nhật — gom MỌI việc cần người dùng xử lý:
> điền API key, xác nhận thông tin, quyết định. Sáng dậy mở file này trước, làm xong đánh dấu [x].

## 1. Điền API key / secret (thay `CHANGE_ME_...`)

| # | Key/Biến | File cần sửa | Vị trí | Lấy ở đâu | Trạng thái |
|---|---|---|---|---|---|
| 0 | **Email GitHub 4 thành viên** (son/bao/thu/phuc) | `commit-as.ps1` | 4 dòng Email=... | ✅ ĐÃ ĐIỀN ĐỦ (12/08): son=thaiquangson@gmail.com · bao=maitieubao@gmail.com · thu=thuhlmtd01131@gmail.com · phuc=robintran51128@gmail.com. ⚠ Email trước đó (tuananhmaiv2006@gmail.com) đã thay bằng maitieubao@gmail.com — nếu đó là account GitHub khác bạn muốn dùng thì báo. Chưa xác nhận email thu/phuc đúng chưa (đoán theo pattern username@gmail.com) | [ ] |
| 1 | `DSA__Jwt__Secret` | `backend/.../appsettings.Production.json` hoặc `.env` | dòng ... | tự sinh 32+ ký tự | [ ] |
| 2 | `ConnectionStrings__Default` | `.env` | dòng ... | SQL Server của nhóm | [ ] |
| 3 | `DSA__Email__*` (SMTP thật) | `.env` | dòng ... | Gmail App Password / SMTP trường — **chỉ cần khi deploy production; dev dùng MailHog** | [ ] |
| 4 | ... (task B1/B3 điền thêm theo code thực tế) | | | | [ ] |

## 2. Xác nhận thông tin (chưa quyết định được lúc chạy tự động)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | Ngày bảo vệ | báo cáo để trống | [ ] |
| 2 | Ngành trên bìa (đang ghi "Ứng dụng phần mềm" theo mẫu cũ) | xác nhận | [ ] |
| 3 | ... | | [ ] |

## 3. Quyết định kỹ thuật cần người dùng duyệt

| # | Quyết định | Nội dung (task đã ghi trong pm-decision-log) | Trạng thái |
|---|---|---|---|
| 1 | ... | ... | [ ] |

> Hướng dẫn: mỗi mục mới do agent thêm kèm ngày giờ `[YYYY-MM-DD HH:MM]` và tên task. Đừng xóa mục cũ — đánh dấu [x] khi xong.

## 4. Session C — Git + Push (2026-08-12, đã init + push xong)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | Xác nhận email GitHub THẬT của **Thu** (thuhlmtd01131@gmail.com) và **Phuc** (robintran51128@gmail.com) trong `commit-as.ps1` | Commit đã hiện đúng TÊN (đã push), nhưng avatar/liên kết GitHub chỉ khớp nếu email đúng là email GitHub của người đó. Son (thaiquangson@gmail.com) và Bao (maitieubao@gmail.com) cũng nên xác nhận lại. | [ ] |
| 2 | Verify repo GitHub: https://github.com/Tqson3118/Visualization — nhánh `main` + `dev` đã push (8 commit) | Push đã thành công bằng credential có sẵn trên máy (không cần nhập lại). Nếu muốn đổi repo public/private → Settings trên GitHub. | [ ] |
| 3 | (Tùy chọn) Xóa backup 3 .git cũ (~115MB) sau khi xác nhận repo mới ổn | `C:\Users\ADMINI~1\AppData\Local\Temp\opencode\git-backup-20260812\` — giữ lịch sử gốc của 3 source repo | [ ] |
| 4 | Khi Session B xong: commit `backend/` + `frontend/` root (skeleton v2) theo quy trình feature branch → PR → dev | Hiện UNTRACKED do Session B đang chạy song song | [ ] |
| 5 | (Tùy chọn) Đổi branch mặc định của GitHub repo thành `main` nếu GitHub tạo mặc định khác | GitHub tự nhận `main` khi push — thường không cần | [ ] |

> Cập nhật bởi Session C (PM --auto). Đừng xóa mục cũ — đánh dấu [x] khi xong.

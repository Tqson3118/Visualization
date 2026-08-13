# PM Report — Task L: Form đăng ký giảng viên (PROMPT_L_TEACHER_REGISTER)

## Mục tiêu
Bỏ checkbox "Tôi là giảng viên", thay bằng form đăng ký GV thật (Khoa/Bộ môn, Mã GV, Kinh nghiệm giảng dạy) trong app; BE lưu 3 cột mới; Admin thấy đủ thông tin khi duyệt; giữ luồng TeacherPending → duyệt → email.

## Trạng thái task
| Task | Kết quả |
|---|---|
| BE: 3 cột + validation + migration | ✅ DONE (User+3 cột nullable, RegisterRequest+3, AuthService validate+bắt buộc khi IsTeacher, AdminUserDto+3, migration AddTeacherProfileFields) |
| FE: RegisterView + AdminUsersView + i18n | ✅ DONE (segmented Sinh viên|Giảng viên, form con, modal duyệt hiển thị info GV, +35 msg i18n) |
| Test BE + FE | ✅ DONE (13 test BE + 6 boundary; RegisterView.spec.ts 6 case; verify độc lập PASS) |
| Review độc lập | ✅ APPROVE (minor đã fix: maxlength, validate độ dài, bio null) |
| Email duyệt GV | ✅ DONE (thêm gửi email approve/reject, verified qua MailHog thật) |
| Smoke E2E | ✅ DONE 6/6 PASS (register GV → admin duyệt → login TEACHER) |
| Ollama 7 tiêu chí | ✅ DONE (UI-4 + UX-7 đã sửa; còn lại từ chối kèm lý do nhiễu model) |
| Đồng bộ docs | ✅ DONE (SRS/SDD/API_REFERENCE/USER_GUIDE/PRODUCTION_PROMPT) |

## File thay đổi (chính)
- **BE**: `backend/src/DsaVisual.Application/Persistence/Entities/User.cs`, `Configurations/UserConfiguration.cs`, `Dtos/RegisterRequest.cs`, `Dtos/AdminUserDto.cs`, `Services/AuthService.cs`, `Services/UserService.cs`, `Persistence/Migrations/20260813052933_AddTeacherProfileFields.cs` (+Designer), `tests/.../AuthServiceTests.cs`, `UserServiceTests.cs` (mới), `TestServices.cs`
- **FE**: `frontend/src/views/RegisterView.vue`, `views/AdminUsersView.vue`, `api/auth.ts`, `api/admin.ts`, `i18n/vi.ts`, `views/RegisterView.spec.ts` (mới)
- **Docs**: SRS.md, SDD.md, API_REFERENCE.md, USER_GUIDE.md, PRODUCTION_PROMPT.md

## Kết quả verify (sau cùng, trên branch đã rebase origin/dev)
- `dotnet build DsaVisual.sln`: PASS (0 error; NU1903 SSH.NET pre-existing)
- `dotnet test DsaVisual.sln`: 101/101 unit + 31/31 integration PASS
- `npm run build`: PASS (vue-tsc + vite)
- `npm run test`: 95/95 PASS
- Migration applied lên DB docker chung: 3 cột verified trong INFORMATION_SCHEMA
- Email duyệt: verified thật qua MailHog (2FA pattern, không block luồng)
- Smoke E2E: 6/6 PASS (ảnh + md tại `docs/work/teacher-register/`)

## Quyết định đã ghi
`docs/pm-decision-log-teacher-register.md` — 8 mục: khởi tạo worktree, phân vai, setting gmail (thuộc prompt K), chốt thiết kế, lệch phạm vi (api/admin.ts + stale-error + test cũ), xử lý review minor + rebase, PR thủ công + E2E plan, xử lý vòng Ollama (UI-4/UX-7) + backlog.

## Git
- Nhánh: `feature/teacher-register` (7 commits, base `dev` @ 604c11f, đã rebase + push)
- Commit-as: bao (BE) / son (FE) / phuc (docs)
- **PR: CHƯA tạo tự động — máy không có `gh` CLI và không có GITHUB_TOKEN. Tạo thủ công:**
  https://github.com/Tqson3118/Visualization/pull/new/feature/teacher-register (base: dev)

## Tồn đọng (ngoài scope, ghi backlog)
- NU1903: SSH.NET 2025.1.0 high severity (pre-existing — đợt J/M xử lý)
- `/progress/me` 500 — ProgressService.cs:223 trùng key (pre-existing, ảnh hưởng /path sau login)
- Setting `allowed.email.domains` vẫn chặn @gmail.com → gỡ chặn là việc prompt K (chưa làm)
- Worktree `D:\FPT\neww-teacher` giữ lại đến khi PR merge xong → `git worktree remove`

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu 'làm lại <task/mục>' kèm ghi chú, PM chạy lại phần đó.

# PM Report — Seed dữ liệu demo production-like (PROMPT_K_SEED_PROD)

- **Ngày**: 13/08/2026 (--auto) | **Nhánh**: feature/seed-prod (worktree D:\FPT\neww-seed) | **PR #10** → base `dev`

## Mục tiêu
Seed dữ liệu hoạt động người dùng (16 bảng = 0 dòng) — "app phát hành 1 tháng": 8 student mới + lịch sử học + 2 lớp học. Kèm fix domain đăng ký (bỏ chặn — mọi email đăng ký được, quyết định user 13/08).

## Trạng thái task
| Task | Kết quả |
|---|---|
| SEED-1 SeedData records + xóa setting | DONE (build PASS) |
| SEED-2a..2d/2s SeedDemoActivity (5 file partial) | DONE (SEED-2 fail rỗng lần 1 → tách nhỏ + resume — đúng bài học) |
| SEED-2a-fix khôi phục 4 method | DONE |
| SEED-3 Classes partial | DONE |
| SEED-4 SeedRunner call + README | DONE |
| SEED-5 unit tests (12 test) | DONE — phát hiện 2 bug seed → fix (lần 1) → re-verify 97/97 + 31/31 PASS |
| SEED-6 --seed thật 2 lần + SQL + API smoke | DONE — lần 2 = 0 thêm; đạt/vượt mọi ngưỡng; register @gmail.com 201 |
| SEED-7 đồng bộ docs | DONE |
| SEED-8 dev-review | APPROVE (1 major follow-up + 2 docs nhỏ đã sửa) |
| PR #10 | CREATED (base dev) |

## File thay đổi
- Code seed: `SeedData.cs`, `SeedRunner.cs` (+1 dòng), `SeedDemoActivity.{cs,Students,Progress,Activity,Misc,Class}.cs` (mới ~2500 dòng), `README.md`
- Test: `SeedDemoActivityTests.cs` (mới, 9 test), `AuthServiceTests.cs` (+3 domain)
- Docs: `SDD.md`, `SRS.md`, `API_REFERENCE.md`, `docs/work/seed-prod.md` (mới), `docs/pm-decision-log-seed.md` (mới)

## Verify
- Build PASS, 0 warning mới; UnitTests 97/97 + Integration 31/31
- Seed thật DB docker: lần 1 thêm 8 students/10 achievements/33 UA/30 UP/33 UNP/50 sub/168 quests/139 gems/7 inv/27 fav/9 feedback/5 code/3 bug/3 notes/2 classes/12 members/7 assignments; lần 2 = 0 thêm
- SQL đếm: vượt toàn bộ ngưỡng; setting allowed.email.domains = 0; user rác smoke giữ nguyên; premium student@demo.local giữ nguyên
- API smoke: leaderboard level 13/week 9/class 7-5 rows; classes = 2; report OnTime=3 Late=2 NotSubmitted=2; achievements 10; register test-gmail@gmail.com → 201 (user id 2012, +2 gv.smoke TeacherPending — Users 17)

## Quyết định đã ghi
Docs/pm-decision-log-seed.md: worktree riêng; fix domain cách 1; cơ chế seed (a)/(b); tách 8+ task; bảng runtime không seed; tách SEED-2 sau lần trả rỗng; wiring + kỳ vọng UserInventory 7; 2 bug seed → fix; PR #10.

## Tồn đọng (ngoài phạm vi — đề xuất task riêng)
1. `GET /api/v1/progress/me` trả 500 — ProgressService.cs:223 `ToDictionary` duplicate key (dữ liệu seed kích hoạt) — cần fix trước demo màn progress.
2. 1 user test + 2 gv.smoke tạo trong API smoke (id 2012-2014) — giữ nguyên (không xóa) theo quy tắc an toàn.

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu 'làm lại <task/mục>' kèm ghi chú, PM chạy lại phần đó.

# status.md — Seed Prod V2

## Task 1 — Users V2 (DONE, 222fefb) — 69 user, guard Email, CreatedAt rải 30 ngày
## Task 2 — Achievements V2 (DONE, 1ac367c) — +7 achievement SortOrder 11-17, plan 337 UA
## Task 3 — Progress+Submissions V2 (DONE, 811828e) — 602 node / 377 progress / 254 subs; showcase pass 13/13 node có exercise
## Task 4 — Activity V2 (DONE, 0f8a401 + 8cf98c9) — 2856 quest / 1360 gemtx / 60 inventory (lệch ngưỡng — trần toán học) / 245 fav / 25 fb; showcase XP 2790, Gems 389; 0 gems âm
## Task 5 — Classes V2 (DONE, 582eb6c) — AI1702 GRPH21 + mở rộng 2 lớp K: 53 members / 13 assignments V2 (tổng 66 / 20)
## Task 6 — Misc V2 + Premium + wire (DONE, 78a0ef1 + cbc623a)
- V2.Misc.cs: 7 bugreport (2 New/2 Processing/2 Resolved/1 Closed), 7 codesubmission, premium showcase 12m DSV{id}T12
- Wire 15 call V2 vào SeedDemoActivity.cs (+21 dòng)
- FIX: bỏ AsNoTracking → Xp/Gems persist; khôi phục test balance toàn bộ user
- ⏭ TIẾP THEO: chạy --seed 2 lần lên DB thật + SQL counts + consistency + API smoke (dev-test)

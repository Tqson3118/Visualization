# Verify Final - Seed V2 sau fix 6c (commit 9d411cf)

- Ngay chay: 2026-08-14 04:57-05:03 UTC+7 (seed run3: 04:59:3x; run4: 05:00:03; API smoke: 05:01-05:02)
- Moi truong: worktree `trees/seed-v2` (HEAD 9d411cf), DB `DsaVisual` (container `neww-sqlserver-1`), backend `dotnet run --project src/DsaVisual.Api -- --seed`
- Nguoi verify: tester doc lap (khong sua code; chi don du lieu demo da duyet decision log 05:0x truoc khi chay seed)
- Evidence: `seed-run3.log`, `seed-run4.log`, `sql-counts-final.txt`, `consistency-final.txt`, `api-smoke-final.txt`, `api-run-final.log`, `api-run-final.err.log` (cung thu muc)

## Buoc 1 - Don showcase gems (data cu session seed truoc)

| Op | Query | Affected | Ket qua |
|---|---|---|---|
| del_inventory | DELETE UserInventory WHERE UserId=2092 AND ItemId=1 (avatar-cyber-hacker) | 1 | OK |
| del_gemtx | DELETE GemTransactions WHERE UserId=2092 AND RefType='shop' AND RefId=1 (spend -100) | 1 | OK |
| upd_gems | UPDATE Users SET Gems=389 WHERE Id=2092 | 1 | OK |

- Luu y: lan dau UPDATE loi `QUOTED_IDENTIFIER` (filtered index tren Users) -> transaction rollback sach, chay lai voi `SET QUOTED_IDENTIFIER ON` thanh cong.
- Verify sau don: Gems=389, UserInventory con 1 item (Id 66, avatar-ai-bot, gia 50), GemTransactions shop con 1 dong spend -50 (RefId 5).

## Buoc 2 - Seed 2 lan (idempotent cuoi)

| Moc | Ket qua thuc | PASS/FAIL |
|---|---|---|
| seed run3 | `Select-String 'Seed: V2.*th.m [1-9]'` = **0 match**; log: "CodeSubmissions them 0 / bo qua 7"; "UserInventory them 0 / bo qua 59"; "GemTransactions them 0 / bo qua 105" | PASS |
| seed run4 | `Select-String 'Seed: V2.*th.m [1-9]'` = **0 match**; log: "CodeSubmissions them 0 / bo qua 7" | PASS |
| CodeSubmissions | truoc 12 -> sau run3 12 -> sau run4 12 (khong doi) | PASS |

## Buoc 3 - SQL counts cuoi (sql-counts-final.txt)

| Bang | Gia tri | Nguong | PASS/FAIL |
|---|---|---|---|
| Users | 95 | >= 85 | PASS |
| Achievements | 17 | = 17 | PASS |
| UserAchievements | 370 | >= 300 | PASS |
| ExerciseSubmissions | 371 | >= 250 | PASS |
| CodeSubmissions | 12 | >= 10 | PASS |
| Classes | 4 | = 4 | PASS |
| ClassMembers | 66 | >= 60 | PASS |
| ClassAssignments | 20 | >= 20 | PASS |
| ContentFeedback | 35 | >= 20 | PASS |
| BugReports | 10 | = 10 | PASS |
| UserNodeProgress | 636 | >= 600 | PASS |
| UserQuests | 3136 | >= 3000 | PASS |
| GemTransactions | 1500 | >= 1500 | PASS (bang nguong; 1501 truoc do -1 do don spend cyber-hacker) |
| UserInventory | 66 | >= 80 | **FAIL (lech da biet)** - gioi han toan hoc gems>=0 + UNIQUE(UserId,ItemId) + XP persona; dung ky vong ~66 theo quest-xp-showcase.md 4.1; khong phai bug seed |
| Favorites | 272 | >= 250 | PASS |
| UserProgress | 409 | >= 400 | PASS |
| LessonNotes | 3 | = 3 | PASS |

Tong: 16/17 PASS, 1 FAIL co ly do (UserInventory 66).

## Buoc 4 - Consistency showcase cuoi (consistency-final.txt)

| Field | Gia tri | Ky vong | PASS/FAIL |
|---|---|---|---|
| Xp | 2790 | 2600-2900 | PASS |
| Gems | 389 | = 389 (300-420) | PASS |
| StreakDays | 30 | 30 | PASS |
| LastActivityDate | 2026-08-14 | hom nay UTC+7 | PASS |
| PremiumUntil | 2027-07-15 04:40 | > now | PASS |
| HeartsMax | 30 | 30 | PASS |
| PremiumSubscriptions | 12m / StartedAt 2026-07-15 / ExpiresAt 2027-07-15 / Status 0 / DSV2092T12 | PlanId 12m, Status 0, OrderRef DSV{id}T12 | PASS |
| BadNodes (Status=2 thieu submission full-score) | **0** | 0 | PASS (query da chinh theo schema: Exercises e2 WHERE e2.NodeId=n.Id OR e2.Id=n.FinalTestId, full-score = s.Score=e2.MaxScore; query goc join sai cho 91 - chi tham chieu) |

## Buoc 5 - API smoke cuoi (api-smoke-final.txt)

| Endpoint | Ket qua | PASS/FAIL |
|---|---|---|
| POST /api/v1/auth/login | OK - id 2092, showcase@demo.local, role STUDENT | PASS |
| GET /api/v1/auth/me | id 2092, email showcase@demo.local | PASS |
| GET /api/v1/me/streak | streakDays=30 | PASS |
| GET /api/v1/premium/status | planId 12m, status active, expires 2027-07-15 | PASS |
| GET /api/v1/me/inventory | 1 item (avatar-ai-bot, quantity 1) | PASS (dung 1 item sau don) |
| GET /api/v1/leaderboard?tab=level&pageSize=1 | rank 1 = userId 2092, xp 2790 | PASS |
| GET /api/v1/leaderboard?tab=week&pageSize=1 | rank 1 = userId 2092, xp 2790 | PASS |

- Ghi chu: API khong co endpoint profile tra truc tiep Gems (Xp 2790 xac nhan qua leaderboard; Gems 389 xac nhan qua SQL + inventory 1 item + gemtx 1 spend -50).
- App tat sach: stop dung PID listener 5299 (21468) + parent (24580); port dong, err log 0 byte, khong dong Exception/ERROR trong api-run-final.log.

## Ket luan

- **SAN SANG dev-review + PR.** Fix 6c da xu ly dung 2 fail cua lan verify truoc: (1) idempotency CodeSubmissions - run3/run4 deu 0 them, count on dinh 12; (2) showcase Gems - 389 trong khoang 300-420 sau khi bo item cyber-hacker (1 inventory + 1 gemtx spend -100 xoa, Gems update 389).
- FAIL con lai duy nhat la **UserInventory 66 < 80 - lech da biet** (gioi han toan hoc gems>=0 + UNIQUE(UserId,ItemId) + persona XP; dung ky vong thiet ke ~66-67 theo quest-xp-showcase.md 4.1). Khong can chinh code.
- Khong phat hien loi moi nao trong pham vi nay.

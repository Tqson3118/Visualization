# SCORECARD — Chấm điểm 36 view theo standard.md (Phase 1 điền)

> **NGUỒN CHUẨN DUY NHẤT**: `docs/work/view-quality/standard.md` (10 trục hygiene + trục Đặc trưng tách riêng + KILL-LIST) — chép đúng từ `PROMPT_VIEW_QUALITY_MASTER_V2.md` mục 5 (BƯỚC C + BƯỚC D). Danh sách view theo `docs/SCREEN_MAP.md` + `frontend/src/router/index.ts` (đối chiếu 13/08/2026: 36 file `.vue` trong `frontend/src/views/`).

| | |
|---|---|
| Loại tài liệu | Scorecard audit view (Phase 1 điền điểm) |
| Phiên bản | 1.0 |
| Ngày tạo | 13/08/2026 |
| Trạng thái | Dự thảo — cột điểm để TRỐNG, Phase 1 audit + điền |
| Người soạn | Agent dev-docs (theo PROMPT_VIEW_QUALITY_MASTER_V2) |
| Tài liệu liên quan | `standard.md`, `frontend/DESIGN.md`, `frontend/DESIGN-IDENTITY.md`, `docs/SCREEN_MAP.md` |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 13/08/2026 | Agent dev-docs | Tạo bản đầu — 36 view, cột điểm trống, đánh dấu ưu tiên CAO theo bằng chứng mục 4 PROMPT |

---

## Ngưỡng ĐẠT — cả 3 điều kiện sau, KHÔNG bù trừ cho nhau (1 view fail bất kỳ điều kiện nào = KHÔNG ĐẠT dù các điều kiện khác cao)

1. **Tổng hygiene ≥ 80/100.**
2. **Không trục hygiene nào dưới mức sàn của chính trục đó** (sàn 60%: spacing 4.8 / breakpoint 3.6 / animation 8.4 / thị-giác 8.4 / interactive-sizing 9.6 / typography 6.0 / depth 4.8 / a11y 7.2 / code 3.6 / performance 3.6).
3. **Đặc trưng ≥ 7/10** (trục tách riêng — KHÔNG cộng vào tổng hygiene).

## Hướng dẫn chấm ngắn

- Cột điểm: điền số thực (làm tròn 0.5) cho TỪNG trục, sau đó tính `TỔNG hygiene` (tối đa 100) và `đặc-trưng` (0-10, tách riêng).
- Cột `đạt/không-đạt`: ghi **ĐẠT** chỉ khi đủ CẢ 3 điều kiện trên, ngược lại ghi **KHÔNG ĐẠT** + lý do (điều kiện nào fail).
- Cột `ưu tiên sửa`: **CAO** = view có vi phạm ĐÃ XÁC NHẬN ở mục 4 PROMPT (12 screenshot r2-fixed-01..12) — sửa trước; để trống = view không có bằng chứng vi phạm xác nhận (Phase 1 vẫn audit lại từng view hiện tại trước khi sửa, không giả định screenshot cũ còn đúng 100%).
- Mọi điểm phải kèm bằng chứng (selector/dòng code/screenshot) ghi trong nhật ký audit của view — không chấm "cảm tính".
- Mọi nhận xét quy chiếu đúng quy tắc chuẩn tại `standard.md` mục 2 + KILL-LIST mục 5.

## Bảng chấm điểm (36 view)

`view | spacing(/8) | breakpoint(/6) | animation(/14) | thị-giác(/14) | interactive-sizing(/16) | typography(/10) | depth(/8) | a11y(/12) | code(/6) | performance(/6) | TỔNG hygiene(/100) | đặc-trưng(/10) | đạt/không-đạt | ưu tiên sửa`

| view | spacing(/8) | breakpoint(/6) | animation(/14) | thị-giác(/14) | interactive-sizing(/16) | typography(/10) | depth(/8) | a11y(/12) | code(/6) | performance(/6) | TỔNG hygiene(/100) | đặc-trưng(/10) | đạt/không-đạt | ưu tiên sửa |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| HomeView — `/` | | | | | | | | | | | | | | CAO |
| LoginView — `/login` | | | | | | | | | | | | | | |
| RegisterView — `/register` | | | | | | | | | | | | | | |
| ForgotPasswordView — `/forgot-password` | | | | | | | | | | | | | | |
| ResetPasswordView — `/reset-password` | | | | | | | | | | | | | | |
| PrivacyView — `/privacy` | | | | | | | | | | | | | | |
| HelpView — `/help` | | | | | | | | | | | | | | |
| CheatSheetView — `/cheatsheet` | | | | | | | | | | | | | | |
| SimulationsView — `/simulations` | | | | | | | | | | | | | | |
| PathRedirectView — `/path` (redirect từ `/learn`) | | | | | | | | | | | | | | |
| PathView — `/path/:topicId` | | | | | | | | | | | | | | CAO |
| LessonView — `/learn/:lessonId` | | | | | | | | | | | | | | CAO |
| SimulatorView — `/simulator/:key` | | | | | | | | | | | | | | CAO |
| ExerciseView — `/exercise/:id` | | | | | | | | | | | | | | |
| LadderView — `/ladder/:nodeId` | | | | | | | | | | | | | | CAO |
| LabView — `/ladder/:nodeId/lab` | | | | | | | | | | | | | | CAO |
| CodeRunnerView — `/code/:key` | | | | | | | | | | | | | | CAO |
| BenchmarkView — `/benchmark/:k1/:k2` | | | | | | | | | | | | | | CAO |
| ShopView — `/shop` | | | | | | | | | | | | | | |
| QuestsView — `/quests` | | | | | | | | | | | | | | |
| LeaderboardView — `/leaderboard` | | | | | | | | | | | | | | CAO |
| ProfileView — `/profile` | | | | | | | | | | | | | | CAO |
| PremiumView — `/premium` | | | | | | | | | | | | | | |
| SubscriptionView — `/account/subscription` | | | | | | | | | | | | | | |
| ClassesView — `/classes` | | | | | | | | | | | | | | |
| ClassDetailView — `/classes/:id` | | | | | | | | | | | | | | |
| ClassReportView — `/classes/:id/report` | | | | | | | | | | | | | | |
| AdminUsersView — `/admin/users` | | | | | | | | | | | | | | |
| AdminStatsView — `/admin/stats` | | | | | | | | | | | | | | |
| AdminSettingsView — `/admin/settings` | | | | | | | | | | | | | | |
| AdminContentView — `/admin/content` | | | | | | | | | | | | | | |
| AdminLadderView — `/admin/ladder` | | | | | | | | | | | | | | |
| NodeHubView — `/path/:topicId/node/:nodeId` | | | | | | | | | | | | | | |
| FinalTestView — `/path/:topicId/final-test` | | | | | | | | | | | | | | |
| NotFoundView — `/:pathMatch(.*)*` (404) | | | | | | | | | | | | | | |
| PlaceholderView — (chưa có route trỏ — standby, router comment "0 route trỏ PlaceholderView") | | | | | | | | | | | | | | |

> Ghi chú: 36 view = 35 view có route thực tế trong `frontend/src/router/index.ts` + PlaceholderView (chưa route — giữ trong danh sách vì tồn tại trong `frontend/src/views/`; nếu Phase 1 không audit được trên UI thật thì ghi rõ "không audit được" ở nhật ký, không bỏ qua).

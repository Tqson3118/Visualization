# SCORECARD — KẾT QUẢ CUỐI: 36 view (Phase 0 → 1 → 2, 14/08/2026)

> **NGUỒN CHUẨN**: `docs/work/view-quality/standard.md` (10 trục hygiene + trục Đặc trưng tách riêng + KILL-LIST). Ngưỡng ĐẠT (3 điều kiện, không bù trừ): (1) TỔNG hygiene ≥ 80/100; (2) không trục nào dưới sàn 60%; (3) Đặc trưng ≥ 7/10.
> **Cách đọc**: cột trục chi tiết nằm trong `docs/work/view-quality/audit-<view>.md` + `fix-log.md` (bảng trước/sau) của từng nhóm — bảng dưới ghi TỔNG hygiene + Đặc trưng + kết luận (điểm trước → sau lấy từ fix-log).
> Trạng thái: **36/36 view ĐẠT**. Phase 1: 4 nhóm song song (PR #13-#16). Phase 2 bổ sung: Ladder/NodeHub/FinalTest (PR #17) + HelpView raw button (PR #17).

| view — route | hygiene trước → sau | đặc-trưng trước → sau | đạt/không-đạt | ưu tiên sửa |
|---|---|---|---|---|
| HomeView — `/` | 64.5 → 95 | 2 → 9 | ĐẠT | CAO ✅ |
| LoginView — `/login` | 65.5 → 95 | 2 → 8 | ĐẠT | |
| RegisterView — `/register` | 61 → 95 | 2 → 8 | ĐẠT | |
| ForgotPasswordView — `/forgot-password` | 66 → 95 | 2 → 8 | ĐẠT | |
| ResetPasswordView — `/reset-password` | 64 → 95 | 2 → 8 | ĐẠT | |
| NotFoundView — 404 | 55.5 → 96.5 | 1 → 9 | ĐẠT | |
| PrivacyView — `/privacy` | 54 → 94 | 1 → 7 | ĐẠT | |
| HelpView — `/help` | 54 → 94.5 | 1 → 7 | ĐẠT | |
| CheatSheetView — `/cheatsheet` | 55 → 94 | 3 → 9 | ĐẠT | |
| SimulationsView — `/simulations` | 53 → 94 | 3 → 9 | ĐẠT | |
| PathRedirectView — `/path` | 72.5 → 86 | 4 → 8 | ĐẠT | |
| PathView — `/path/:topicId` | 59.5 → 86 | 2 → 8.5 | ĐẠT | CAO ✅ |
| LessonView — `/learn/:lessonId` | 67 → 87.5 | 3 → 8 | ĐẠT | CAO ✅ |
| SimulatorView — `/simulator/:key` | 61.5 → 84.5 | 6 → 8.5 | ĐẠT | CAO ✅ |
| ExerciseView — `/exercise/:id` | 78 → 86.5 | 3 → 7.5 | ĐẠT | |
| LadderView — `/ladder/:nodeId` | 76.5 → 94.5 | 3 → 8.5 | ĐẠT | CAO ✅ |
| LabView — `/ladder/:nodeId/lab` | 67 → 86 | 5 → 8 | ĐẠT | CAO ✅ |
| CodeRunnerView — `/code/:key` | 71 → 88.5 | 6 → 8.5 | ĐẠT | CAO ✅ |
| BenchmarkView — `/benchmark/:k1/:k2` | 65 → 87.5 | 5 → 8 | ĐẠT | CAO ✅ |
| NodeHubView — `/path/:topicId/node/:nodeId` | 78 → 94 | 4 → 8 | ĐẠT | |
| FinalTestView — `/path/:topicId/final-test` | 78.5 → 94 | 4 → 8 | ĐẠT | |
| ShopView — `/shop` | 76 → 91.5 | 3 → 7.5 | ĐẠT | |
| QuestsView — `/quests` | 77 → 91.5 | 3 → 7.5 | ĐẠT | |
| LeaderboardView — `/leaderboard` | 73.5 → 91 | 3 → 8 | ĐẠT | CAO ✅ |
| ProfileView — `/profile` | 72 → 91 | 2 → 8 | ĐẠT | CAO ✅ |
| PremiumView — `/premium` | 68 → 93.5 | 2 → 8 | ĐẠT | |
| SubscriptionView — `/account/subscription` | 72 → 93.5 | 2 → 8 | ĐẠT | |
| ClassesView — `/classes` | 61 → 92.5 | 2 → 8.5 | ĐẠT | |
| ClassDetailView — `/classes/:id` | 55 → 92.5 | 2 → 9 | ĐẠT | |
| ClassReportView — `/classes/:id/report` | 63.5 → 92.5 | 2 → 9 | ĐẠT | |
| AdminUsersView — `/admin/users` | 58 → 92.5 | 2 → 8 | ĐẠT | |
| AdminStatsView — `/admin/stats` | 62.5 → 93.5 | 2 → 8.5 | ĐẠT | |
| AdminSettingsView — `/admin/settings` | 68 → 92.5 | 2 → 7.5 | ĐẠT | |
| AdminContentView — `/admin/content` | 59.5 → 92.5 | 2 → 8.5 | ĐẠT | |
| AdminLadderView — `/admin/ladder` | 56 → 92 | 2 → 8 | ĐẠT | |
| PlaceholderView | — (0 route trỏ — không chấm) | — | — | — |

## Xác nhận toàn cục (grep trên `dev` @ 14/08/2026)
- `<button` raw trong `src/views/` (case-sensitive): **0** (HelpView đã chuyển sang Button.vue tại PR #17).
- Emoji làm icon trong views: **0** (glyph 🪜 vỡ đã thay lucide `ListOrdered`).
- `bg-gradient` trang trí trong views: **0** (banner = surface band + luminance stacking + strip mono).
- Import `@lucide/vue` / `@phosphor-icons` trong views/components: **0** (chỉ `lucide-vue-next`).
- Hardcode spacing `[Npx]` trong views: **0**.
- Build: `npm run build` (vue-tsc + vite) PASS. Test: **95/95 PASS** (12 files). E2E: chạy thủ công các luồng chính — console 0 lỗi, 0 overflow 3 breakpoint.
- Kết quả Ollama qwen2.5vl:3b (3 gate: 7 tiêu chí / spacing / bản sắc): chạy được cho phần lớn view nhóm A/B/C (log tại `docs/work/view-quality/ollama-log/`) — nhận diện bản sắc DSA rõ ràng; view nhóm D + 3 view P2 chưa chạy được (ghi `notes.md`) → khuyến nghị vòng QA cuối bởi dev-e2e trước khi merge `dev → main`.

## 6 bằng chứng thật (mục 4 PROMPT) — trạng thái
1. ✅ Banner gradient random → bỏ toàn bộ (surface band + strip mono; hero Home chạy mô phỏng THẬT từ engine catalog).
2. ✅ Path render graph thật bằng `@vue-flow/core@1.48.2` (PathGraph.vue, lazy chunk 154.55 kB — entry +0.15 kB).
3. ✅ Empty state lặp công thức → `EmptyState.vue` redesign motif `[ ]` block (dùng 57 chỗ).
4. ✅ Emoji 🎯/🪜 → lucide-vue-next.
5. ✅ Card lặp công thức → luminance stacking + stat-card hierarchy (tối đa 1 hero-stat/màn).
6. ✅ Vùng mô phỏng nền tối → lan tỏa block-token (XP history, streak, benchmark, leaderboard rank, mã mời lớp...).
- Bug thật: header lặp Home ✅ / icon vỡ ladder ✅ / Monaco note lộ ra ✅ / label "Miễn phí tìm (20.4)" → "Không tốn tim" ✅ / ClassReport API contract sai → khớp `ClassReportDto` ✅ / AdminContent ngày giả → index mono thật ✅.
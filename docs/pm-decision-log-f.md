## [2026-08-12 09:40] Khởi động đợt F (bàn giao + review cuối — PROMPT_F)
- Quyết định: Chạy đợt F theo session/PROMPT_F.md ở chế độ --auto; tạo nhánh feature/final-review từ dev (dev đã chứa toàn bộ D+E — verified qua git log).
- Ảnh hưởng: toàn bộ docs/ + THIRD_PARTY.md + có thể TEST_PLAN/USER_GUIDE.

## [2026-08-12 09:40] Phân rã task F
- Quyết định: 6 task — F1 THIRD_PARTY số thật (dev-docs), F2a audit checklist §17.9 read-only (dev-docs), F2b sửa mọi ✘ (dev-docs), F4 verify độc lập (dev-test), F3 E2E 12 màn (dev-e2e), F5 review diff D+E+F (dev-review). Layer: L1 = F1+F2a+F4 song song; L2 = F2b; L3 = F3+F5; L4 = commit + merge dev.
- Ảnh hưởng: docs/work/f1..f5.md (status), pm-report-f.md, pm-decision-log-f.md.

## [2026-08-12 09:40] Xử lý các vấn đề đã biết
- Quyết định: (1) AuthController 2FA là 501 có chủ đích (API_REFERENCE §4.12 + decision log D) — KHÔNG coi là stub; (2) 8 bug UI từ đợt E nằm trong SETUP_TODO §6, đợt F KHÔNG sửa code — F3 chỉ báo lại; (3) TEST_PLAN ghi "chờ tuần 19-20" → thay bằng số thật đợt D/E; (4) port 5174 cho cả playwright lẫn E2E thật (5173 bị container stale vdsa-frontend chiếm) → F3 chạy sau F4 tránh xung đột.
- Ảnh hưởng: TEST_PLAN.md, USER_GUIDE.md, THIRD_PARTY.md, docs/work/e2e-report-f.md.

## [2026-08-12 10:10] Quyết định khắc phục ✘ từ F2a
- Quyết định: (1) API_REFERENCE §4.2: xóa endpoint đã cắt theo ADR-001 (/public/simulations/{key}/run), ghi chú "chưa triển khai — xem SETUP_TODO §6" cho 3 endpoint lessons (mark-viewed/progress/simulations) — KHÔNG thêm code backend trong đợt F; (2) TEST_PLAN §10: điền số thật từ đợt D/E (FE 72/72, BE 44 Unit + 27 Integration, e2e 11/11, build 0 warning), chỉ giữ "chờ" cho mục chưa chạy thật (load/security pentest); (3) USER_GUIDE:16 cập nhật cảnh báo lỗi thời (UI 33 view thật 12/08/2026); (4) docs/README §1.1 cập nhật số dòng thật; (5) SCREEN_MAP:183 bỏ "Màn 33" mâu thuẫn (chuẩn 01-32 + N-1..N-16); (6) Api.Tests trống giữ nguyên (cấu trúc SDD); (7) .env source/VisualizationDSA1 có key thật → khuyến nghị xoay key, ghi SETUP_TODO.
- Ảnh hưởng: API_REFERENCE.md, TEST_PLAN.md, USER_GUIDE.md, docs/README.md, SCREEN_MAP.md (đều bump 1.1 theo §17.12), docs/SETUP_TODO.md.

## [2026-08-12 10:10] Xử lý F4 phát hiện
- Quyết định: Api.Tests project trống ("No test available") giữ nguyên trong solution (khớp cấu trúc SDD, đã ghi chú pm-report-b); không sửa code.
- Ảnh hưởng: không.

## [2026-08-12 20:05] Merge feature/final-review vào dev
- Quyết định: Merge --no-ff feature/final-review → dev (commit 5ea0846) sau khi F5 APPROVE (0 Critical). Merge main CHỜ user duyệt (đã ghi SETUP_TODO §7.2).
- Ảnh hưởng: dev có đủ docs đợt F; git log dev: 5ea0846.

## [2026-08-12 20:05] Commit docker lạ (a1b8bd8/7ac5896)
- Quyết định: Commit "fix(docker): built-in app user + frontend 8081" (bao, 19:52-19:53) xuất hiện trên CẢ feature/final-review (a1b8bd8) và dev (7ac5896) do session khác tạo — nội dung trùng nhau (verified git diff rỗng phần docker), merge không xung đột. Giữ nguyên, không hoàn tác.
- Ảnh hưởng: không.

## [2026-08-12 20:05] Bàn giao bug cho đợt sau
- Quyết định: KHÔNG sửa code trong đợt F (phạm vi bàn giao): F3-NEW-1 Leaderboard crash (P1), F5-Major Heart regen không persist, 4 minor + 1 nit → ghi SETUP_TODO §8. Session G sẽ xử lý (session/PROMPT_G.md tồn tại).
- Ảnh hưởng: docs/SETUP_TODO.md §8.

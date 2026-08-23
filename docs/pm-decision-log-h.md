
## [2026-08-13 15:30] Đợt H — Review + nâng cấp UX/UI toàn bộ (PROMPT_H_UI_REVIEW)
- Quyết định: Chạy đợt H --auto ngay (đợt G phụ đã merge dev, HEAD 6661e0d, GP-T9b xong). Nhóm A (12 màn báo cáo) ĐÃ POLISH 2 vòng (G-F2b/c/d + GP-T9b 23 sửa) → KHÔNG sửa lại, chỉ chụp ảnh FINAL cuối đợt (docs/work/final-<màn>.png) cho báo cáo Word. Việc chính: 24 view chưa polish (Admin 5, Classes 3, Gamification 4, Phụ 12).
- Ảnh hưởng: feature/ux-h-{b,c,d,e} từ dev, verify→review→merge tuần tự (i18n/vi.ts dùng chung → KHÔNG chạy dev-ux song song nhóm khác nhau). Commit frontend/UX → son.

## [2026-08-13 15:45] H-B — kết quả e2e + quyết định xử lý 2 lỗi contract backend
- Quyết định: H-B e2e 5/5 PASS (0 console/0 overflow, vision P3 nhiễu — model 3B). Giao dev-backend sửa 2 lỗi P2 phát hiện: (1) /admin/stats thiếu totalSimulations+activeUsersToday (FE đọc → 2 KPI hiện "—"); (2) /settings trả List<SettingDto> ≠ SystemSettingsDto FE cast (màn settings hiện default). Sửa backend trên nhánh feature/ux-h-b-be, sau đó dev-test verify + dev-review + merge cả 2 nhánh.
- Ảnh hưởng: backend controllers/services/DTO admin; frontend không đổi.

## [2026-08-13 16:10] H-C — kết quả e2e + quyết định sửa token chung
- Quyết định: H-C e2e 3/3 PASS hành vi; 1 P2 thật = --muted-foreground dark 3.34:1 (token chung toàn app) + 1 P3 = --gradient-sunset light hơi chói. CHẤP NHẬN sửa token chung (tối oklch 0.72→~0.63) vì cải thiện WCAG toàn app, rủi ro thấp; verify lại toàn bộ sau khi sửa (24 màn đã từng đo 0 overflow — token không ảnh hưởng layout). Giao dev-ux fix trên feature/ux-h-c (vòng 1/2).
- Ảnh hưởng: frontend/src/styles/tailwind.css (muted-foreground dark), palettes.css (sunset light) — toàn app, cần dev-test verify build + e2e lại.

## [2026-08-13 16:40] H-D — kết quả e2e + quyết định xử lý
- Quyết định: H-D e2e 4/4 PASS (vision nhiễu, đo thật đạt). Fix P2 ngay trên feature/ux-h-d (ShopView.vue:36 gọi fetchAll nạp gems khi vào thẳng /shop — lỗi UI view, đúng phạm vi). P1 (contract quests/shop/premium DTO) + P3 (/me/hearts nextHeartInSeconds) là contract API — ĐỢT H CẤM đổi contract → ghi SETUP_TODO cho đợt I/J (đã có PROMPT_I_VIEW_QUALITY + PROMPT_J_BACKEND_AUDIT).
- Ảnh hưởng: feature/ux-h-d (1 commit fix P2); SETUP_TODO mới.

## [2026-08-13 17:05] H-E1 — P1 toàn app: nút primary contrast fail
- Quyết định: e2e phát hiện nút primary TOÀN APP chữ = --foreground trên --primary → 1.91/2.09:1 (WCAG fail; utility text-primary-foreground không được generate — tailwind.css:137 @theme inline). SỬA NGAY trong feature/ux-h-e1 (fallback button.bg-primary color:var(--primary-foreground) hoặc fix generate) — ảnh hưởng mọi nút, cần verify lại toàn app sau fix. Kèm P3: aside badge alpha 0.22→0.35.
- Ảnh hưởng: frontend/src/styles/tailwind.css + ui/button + aside badge — toàn app; dev-test verify lại build + e2e 13/13 + smoke.

## [2026-08-13 17:35] H-E2 — fix 2 P2 sau e2e
- Quyết định: e2e phát hiện (A) Badge success "3 demo miễn phí" chrome mint 3.05:1 light/2.87:1 dark (tint emerald-500/15 quá mờ — ui/Badge.vue:26 + SimulationsView.vue:150; cùng pattern BenchmarkView "Miễn phí tim"); (B) 404 dark chữ trắng trên ring aurora 2.29:1 (NotFoundView.vue:54-65). Giao dev-ux fix 2 điểm trên feature/ux-h-e2 (vòng 1/2). (C) dark mode không persist qua UI — ngoài phạm vi, ghi SETUP_TODO.
- Ảnh hưởng: frontend ui/Badge.vue (chung — verify lại toàn app), SimulationsView, BenchmarkView (nếu cùng pattern), NotFoundView.

## [2026-08-13 18:05] H-E3 — APPROVE + lưu ý backend stale
- Quyết định: H-E3 APPROVE/PASS → merge feature/ux-h-e3. Ghi nhận: backend container :5000 đang STALE (image cũ — /progress/me 500, refresh 401; nguồn code mới đã có) → REBUILD backend container TRƯỚC khi chụp ảnh FINAL đợt H-A. Ghi nhận P3: FinalTest chọn exercises[0] stage=1 → 422 LADDER_LOCKED với user mới (pre-existing) — đợt I/J.
- Ảnh hưởng: merge dev; docker compose rebuild backend; SETUP_TODO mục 6.

## [2026-08-13 18:40] H-A FINAL — phát hiện H-FINAL-1 + quyết định sửa
- Quyết định: e2e FINAL 11 PASS / 1 CÓ LỖI — H-FINAL-1 (P1 dữ liệu): GET /exercises?nodeId=1&stage=1 → items=[] vì SeedRunner.cs:271-350 không gán NodeId/Stage cho exercise (LadderView filter khớp NodeId&Stage). SỬA nguồn seed (backfill NodeId/Stage theo cấu trúc path/node) — màn Ladder là ảnh bắt buộc §6.2. FE giữ nguyên filter đúng contract.
- Ảnh hưởng: backend SeedRunner + migration/seed idempotent; chụp lại final-14-ladder.png sau fix.

## [2026-08-13 19:00] H-A FINAL — đóng đợt H
- Quyết định: retest sau fix H-FINAL-1 → 12/12 PASS, 0 lỗi (ảnh mới final-14-ladder.png đã ghi đè). Đóng H-FINAL-1 (bd1aff0). Giao dev-docs: copy 12 ảnh docs/work/final-*.png → tailieu/screenshots + thay placeholder trong BAO_CAO.md (tên chuẩn §6.2) + build docx pandoc.
- Ảnh hưởng: docs/pm-report-h.md + BAO_CAO.md + BaoCaoDoAn.docx.

# HANDOFF — 14/08/2026 (View Quality Master V2 — Phase 0+1+2 HOÀN TẤT)

> File này thay cho session/HANDOFF_2026-08-13-UI.md cho phần view quality. Nguồn chuẩn: session/PROMPT_VIEW_QUALITY_MASTER_V2.md. Đọc trước khi làm việc tiếp trên UI.

## 1. TRẠNG THÁI (14/08/2026, kiểm chứng thật)
- **dev @ 528eeef**: đã merge PHASE 0 (#12 foundation) + PHASE 1 4 nhóm (#13 A, #14 B, #15 C, #16 D) + #17 (3 view thiếu + HelpView raw button). **36/36 view ĐẠT** (scorecard: `docs/work/view-quality/scorecard.md`).
- Verify: FE `npm run build` PASS + `npm test` 95/95 PASS; `<button` raw views = 0; emoji icon = 0; gradient trang trí = 0; hardcode spacing = 0; import lẫn icon lib = 0.
- BE không đổi trong phiên này (chỉ FE + docs). Session Diagrams (feature/diagrams @ 1180128) vẫn chưa merge — làm draw.io tài liệu, KHÔNG xung đột vue-flow (đã xác nhận).
- PR #1 (dev → main) vẫn đang mở sẵn — CHƯA merge (chờ QA cuối).

## 2. NỀN TẢNG ĐÃ CHỐT (Phase 0 — đừng tự đổi)
- `frontend/DESIGN-IDENTITY.md`: motif "Data Bench" (dữ liệu = sân khấu tối canvas-ink LUÔN tối bất kể theme; UI = giấy lặng, luminance stacking, không gradient trang trí). Signature: "Block thở theo bước" (block phản ứng trace thật stepExecutor, mọi block có index mono).
- `frontend/DESIGN.md` 10 §: token text 4 tầng, border-subtle/strong, card-raised, success/warning/info, canvas palette (data-core/resolved/conflict/index-muted), radius 4/8/12/16; padding button (§4); Content Voice §9 (nút/toast cùng động từ, empty = lời mời hành động).
- 6 quyết định xuyên-nhóm: banner bỏ gradient / icon lucide-vue-next duy nhất / 1 hero-stat mỗi màn / block-token cho dữ liệu tuần tự / vùng dữ liệu luôn tối / easing chuẩn (enter `cubic-bezier(0.16,1,0.3,1)`, exit `cubic-bezier(0.7,0,0.84,0)`, CẤM ease/linear >150ms, reduced-motion).
- Thư viện: `@vue-flow/core@1.48.2` đã cài (CHỈ dùng lazy defineAsyncComponent — PathGraph.vue là mẫu). `@formkit/auto-animate` chưa cài (khuyến nghị Phase 1 nhưng chưa dùng — nếu cần list reorder, ưu tiên dùng lại motion-v/TransitionGroup thay vì cài mới).

## 3. TỒN ĐỌNG / VIỆC TIẾP (ưu tiên)
1. **Vòng QA cuối (dev-e2e)**: Ollama 3 gate chưa chạy cho nhóm D (8 view) + 3 view P2 (Ladder/NodeHub/FinalTest) — log tại ollama-log/; cần app thật (BE :5000 + FE :5174, DB seed) + chụp light/dark.
2. **Lighthouse A11y ≥90 / Perf ≥80 + axe-core** chưa chạy toàn bộ 36 view (các agent đã đo computed style/console/breakpoint thật).
3. **Gỡ dependency lẫn**: `@lucide/vue` + `@phosphor-icons/vue` còn trong package.json (import đã sạch = 0) → gỡ khi có PR nhỏ, sau đó build + test lại.
4. **Ngoài phạm vi ghi notes.md**: emoji trong nội dung contentHtml CMS (LessonView/AdminContent) → vòng sau xử lý nguồn dữ liệu; dark mode chưa wire vào `<html>` (theme toggle); `.input` global ease 200ms; Modal.vue thiếu aria-describedby; Button.vue auto-upgrade lg mobile; composable chung cssVar + lọc trùng Simulations/CheatSheet.
5. **BE**: class report DTO đã khớp view (ClassReportView) — kiểm BE test sau nếu đổi gì.
6. Merge PR #1 (dev → main) sau khi QA cuối xong.

## 4. QUY TRÌNH (giữ nguyên)
- PR base `dev` (KHÔNG main). Commit-as: son (frontend/design), phuc (docs), bao (backend), thu (engine/test).
- Mọi quyết định ghi `docs/pm-decision-log-viewquality.md` TRƯỚC khi làm.
- Môi trường: FE :5174 (5173 bị relay chiếm), BE :5000, SQL Server docker, MailHog :1025.
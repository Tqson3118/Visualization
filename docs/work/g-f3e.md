# G-F3E — FIX 2 BUG MỚI LEADERBOARD (G-F3D-NEW-1 P1 + G-F3D-NEW-2 P2) — 2026-08-13

- **Task**: Sửa 2 bug phát hiện ở Phase 3d (dev-e2e) trên Leaderboard, trước review chốt đợt G.
- **Branch**: `feature/ux-fix-leaderboard` (tạo từ `dev` = `d62e2ac`). **KHÔNG merge** — PM sẽ merge.
- **Phạm vi HẸP**: chỉ sửa contract field `value` + tab Lớp gửi `classId` + test + ảnh verify.

## 1. Kết quả 2 bug

| Bug | Mức | Trạng thái | Cách sửa |
|---|---|---|---|
| **G-F3D-NEW-1** — tab Level/mọi tab có dữ liệu crash `Cannot read properties of undefined (reading 'toLocaleString')` | P1 | ✅ **FIXED** | Backend `LeaderboardEntryDto` thêm `Value` (long) và `GetLeaderboardAsync` map `Value` theo thứ tự xếp hạng thực tế (cả 3 tab đều sort theo tổng `Xp` → `Value = Xp`). FE đọc `row.value` giờ luôn có dữ liệu → hết crash. |
| **G-F3D-NEW-2** — tab Lớp luôn 400 "Thiếu classId cho tab lớp" | P2 | ✅ **FIXED** | FE `LeaderboardView.switchTab('class')` lấy `classId` từ class store (`currentClass` → `classes[0]` → gọi `fetchClasses()` nếu chưa có). Không có lớp → EmptyState "Bạn chưa tham gia lớp học nào", **không gọi API** (không 400). Store thêm state `noClass` + `setNoClass()`. |

## 2. File sửa

**Backend (commit `bao`):**
- `backend/src/DsaVisual.Application/Dtos/QuestDtos.cs` — `LeaderboardEntryDto` thêm `public long Value { get; set; }` (JSON camelCase → `value`).
- `backend/src/DsaVisual.Application/Services/GamificationService.cs:440-447` — map `Value = row.Xp` kèm comment lý do (DB không có cột XP tuần/điểm lớp — cả 3 tab xếp hạng theo tổng Xp, chỉ khác bộ lọc).
- `backend/tests/DsaVisual.UnitTests/GamificationServiceTests.cs` — **+4 test** `GetLeaderboard_*`: level map Value/Xp+Level đúng, week chỉ lọc user hoạt động tuần này + Value đúng, class thiếu classId → VALIDATION_FAILED, class có member → chỉ member + Value đúng.

**Frontend (commit `son`):**
- `frontend/src/stores/leaderboard.ts` — thêm `noClass` ref + `setNoClass()`; `fetchBoard` reset `noClass`.
- `frontend/src/views/LeaderboardView.vue` — `switchTab` async: tab `class` → `resolveClassId()` (currentClass → classes[0] → fetchClasses) → có classId thì `fetchBoard('class', classId)`, không có thì `board.setNoClass()`; thêm EmptyState "Bạn chưa tham gia lớp học nào".
- `frontend/src/stores/leaderboard.spec.ts` — **test mới (4)** store: rows giữ value không crash, tab class truyền classId, setNoClass không gọi API, fetchBoard lỗi → error.
- `frontend/tests/e2e/leaderboard.spec.ts` — **test mới (2)** Playwright: 3 tab render không crash + tab Lớp gửi classId; user chưa có lớp → EmptyState.
- `frontend/tests/e2e/helpers/mockApi.ts` — thêm mock `GET /classes` (trả 1 lớp) để tab Lớp e2e có classId.

## 3. Verify (đầy đủ, chạy thật)

| Hạng mục | Kết quả |
|---|---|
| `dotnet build DsaVisual.sln` | ✅ 0 warning / 0 error |
| `dotnet test` (unit) | ✅ **60 PASS** (56 → +4) — Integration 31 PASS |
| `npm run build` (vue-tsc + vite) | ✅ 0 lỗi |
| `npm test` (vitest) | ✅ **76 PASS** (72 → +4) |
| `npx playwright test` | ✅ **13/13 PASS** (11 cũ + 2 mới) |
| Real app `/leaderboard` (backend Docker + Vite 5174) | ✅ 3 tab render đúng, **0 pageerror**, tab Lớp **không 400** (EmptyState hợp lý — seed không có lớp), light+dark không vỡ/không overflow |

**Kết quả từng tab trên app thật (student@demo.local):**
- **Tuần**: EmptyState "Chưa có dữ liệu xếp hạng" (đúng thiết kế — seed không có hoạt động tuần này, P3 dữ liệu, không phải bug).
- **Level**: 5 user render (seed Xp=0 → "0 XP") — **hết crash TypeError** (trước fix tab này crash ngay).
- **Lớp**: EmptyState "Bạn chưa tham gia lớp học nào" — **hết 400**.

## 4. Ảnh (docs/work/)

| Ảnh | Nội dung |
|---|---|
| `g-f3e-01-leaderboard-week.png` | Tab Tuần (light) — EmptyState đúng thiết kế |
| `g-f3e-02-leaderboard-level.png` | Tab Level (light) — 5 row render, không crash |
| `g-f3e-03-leaderboard-class.png` | Tab Lớp (light) — EmptyState "chưa tham gia lớp" (không 400) |
| `g-f3e-04-leaderboard-level-dark.png` | Tab Level (dark, `class="dark"`) — không vỡ |

*Ghi chú:* ảnh chụp từ app thật (backend Docker rebuild mới nhất + Vite dev 5174), viewport 1366×768 theo BAO_CAO_SPEC §6.1.

## 5. Lưu ý cho PM / giai đoạn sau

- **Seed dữ liệu**: leaderboard Tuần/Lớp chỉ hiện EmptyState vì seed chưa có hoạt động tuần + chưa có lớp/member — cần seed dữ liệu thật để ảnh báo cáo có dữ liệu (P3, đã ghi từ G-F3D).
- **Điểm lớp (tab Lớp)**: hiện xếp hạng theo tổng Xp trong lớp; nếu cần "điểm lớp" riêng (khác XP) thì cần bổ sung cột dữ liệu + map riêng ở backend.
- **Contract `Value`**: đã chốt FE đọc `row.value` cho mọi tab; BE trả `value` long. Nếu sau này đổi thứ tự xếp hạng theo tab khác nhau thì phải map `Value` theo đúng thứ tự đó.
- Grep cấm: không PostgreSQL / MediatR / Repository / secret trong thay đổi này.

## 6. Commit

- `bao` (backend): DTO + service map + unit test.
- `son` (frontend): store + view + unit test + e2e + mock + doc + ảnh.

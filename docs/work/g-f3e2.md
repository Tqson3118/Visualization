# G-F3E2 — FIX P2 REVIEW: PHÂN TRANG TAB "LỚP" GIỮ classId — 2026-08-13

- **Task**: Sửa 1 P2 từ dev-review (docs/work/g-f3c.md mục 2.3 **F2**) — phân trang tab "Lớp" mất `classId` → backend 400 "Thiếu classId cho tab lớp".
- **Branch**: `feature/ux-fix-leaderboard` (đã có commit G-F3E `5fcd941`/`f0b4ef0`) — thêm commit mới `son`. **KHÔNG merge** — PM sẽ merge.
- **Phạm vi HẸP**: chỉ FE store + view + unit test + doc. Không đổi backend (backend yêu cầu classId đã đúng).

## 1. Bug

| Bug | Mức | Chi tiết |
|---|---|---|
| **P2** — `LeaderboardView.vue:66-69` hàm `goToPage(page)` gọi `board.fetchBoard(undefined, undefined, next)` KHÔNG truyền `classId` | P2 | Khi tab = **Lớp** và lớp có >20 thành viên (totalPages > 1), bấm "Trang sau" → `GET /leaderboard?tab=class&page=2` gửi `classId: undefined` → `GamificationService.cs:416-420` từ chối `400 VALIDATION_FAILED "Thiếu classId cho tab lớp"`. G-F3E đã xử lý classId cho lần tải đầu (switchTab → `resolveClassId()`), nhưng KHÔNG lưu để tái dùng khi phân trang. |

## 2. Cách sửa (theo gợi ý reviewer)

- `stores/leaderboard.ts` — thêm state `lastClassId`:
  - `fetchBoard` **lưu** `lastClassId` mỗi khi nhận `classId` (tab Lớp tải lần đầu / chuyển tab).
  - `fetchBoard` khi `tab === 'class'` mà **không truyền** `classId` (phân trang) → dùng `lastClassId` (`classId ?? lastClassId ?? undefined`).
  - `setNoClass()` reset `lastClassId = null` (không có lớp → không có classId hợp lệ).
- `views/LeaderboardView.vue` — `goToPage(next)`:
  - `tab === 'class'` → `fetchBoard(undefined, board.lastClassId, next)` (giữ classId).
  - `week`/`level` → không truyền classId (không đổi hành vi).
- `stores/leaderboard.spec.ts` — **+2 test** phân trang tab class giữ classId + phân trang week không gửi classId.

## 3. File sửa

**Frontend (commit `son`):**
- `frontend/src/stores/leaderboard.ts` — thêm `lastClassId` ref + logic fallback trong `fetchBoard` + reset trong `setNoClass` + export.
- `frontend/src/views/LeaderboardView.vue` — `goToPage` truyền `classId = board.lastClassId` khi tab Lớp (kèm comment lý do).
- `frontend/src/stores/leaderboard.spec.ts` — **+2 test** (tổng store leaderboard = 6):
  1. `G-F3E2: phân trang tab class giữ classId qua lastClassId` — fetch lần đầu `('class', 7)` → API nhận `classId: 7`; phân trang `(undefined, undefined, 2)` → API nhận `{ tab: 'class', classId: 7, page: 2 }`, `store.page === 2` — **không 400**.
  2. `G-F3E2: phân trang tab week không truyền classId` — `{ tab: 'week', classId: undefined, page: 2 }`.

## 4. Verify (chạy thật)

| Hạng mục | Kết quả |
|---|---|
| `npm run build` (vue-tsc + vite) | ✅ 0 lỗi |
| `npm test` (vitest) | ✅ **78 PASS** (76 → +2) |
| `npx playwright test` | ✅ **13/13 PASS** (11 cũ + 2 leaderboard) |

- **Smoke app thật (/leaderboard tab Lớp đổi trang)**: KHÔNG chạy được vì seed chưa có lớp tham gia → tab Lớp hiện EmptyState (không có nút phân trang, đã chấp nhận từ G-F3E). **Bằng chứng chính là unit test #1** — mô phỏng đúng luồng `fetchBoard('class', classId)` → `fetchBoard(undefined, undefined, nextPage)` giữ `classId`.

## 5. Lưu ý

- Không đổi backend — contract đã đúng (bắt buộc classId khi tab=class).
- `lastClassId` chỉ được dùng khi tab=class; week/level luôn gửi `classId: undefined` (không đổi).
- Grep cấm: không PostgreSQL / MediatR / Repository / secret trong thay đổi này.

## 6. Commit

- `son` (frontend): store + view + unit test + doc.

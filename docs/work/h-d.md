# H-D — Polish 4 màn Gamification & Tài khoản (shop / quests / premium / subscription)

Ngày: 2026-08-13 · Nhánh: `feature/ux-h-d` · Từ: `dev` (HEAD 604c11f)

## Phạm vi

4 view chưa polish của nhóm GAMIFICATION & TÀI KHOẢN + i18n (chỉ THÊM chuỗi mới):

| File | Màn | Nâng cấp |
|---|---|---|
| `frontend/src/views/ShopView.vue` | 22 — Gems Shop | Hero gradient Aurora + gem counter + số vật phẩm; card shadcn + `hover-lift`; icon lucide theo slot (Lightbulb/Snowflake/Image/Palette/Frame/Zap) với tint aurora/mint/sunset; Badge loại vật phẩm; giá chip amber (≥4.5:1); trạng thái không đủ gems (opacity). |
| `frontend/src/views/QuestsView.vue` | 23 — Daily Quest | Hero Aurora + chip streak (gradient Sunset) + chip freeze; quest card: difficulty Badge + reward chip + ProgressBar + CTA (trạng thái claimed/ready highlight xanh); banner bonus dùng success tint + Sparkles; footer quy tắc. |
| `frontend/src/views/PremiumView.vue` | 25/26 — Premium + QR MB Bank | Hero Aurora + Crown + badge; plan card: highlight viền 2px + `--aurora-soft` + giá quy đổi /tháng (số thật); bảng so sánh bọc `overflow-x-auto` + hover row; modal checkout: QR frame trắng + ring primary, info list surface-hover, nút copy icon, note info, countdown tabular-nums. **Logic QR/countdown/kích hoạt GIỮ NGUYÊN.** |
| `frontend/src/views/SubscriptionView.vue` | 27 — Quản lý gói | Hero Aurora + CreditCard; status card: expiresAt (CalendarDays) + days-left chip + Badge gia hạn tự động; benefits grid 2 cột + CheckCircle2; modal hủy: danh sách mất quyền lợi với XCircle destructive. Logic cancel GIỮ NGUYÊN. |
| `frontend/src/i18n/vi.ts` | — | THÊM `shop.*`, `quests.*`, `premium.*`, `subscription.*` (~90 key). KHÔNG xóa/sửa chuỗi cũ. |

## Verify (số thật)

- `npm run build` (vue-tsc + vite) → **0 lỗi** (1.41–1.54s).
- `npm test` → **89/89 PASS** (11 files, 2.8–3.0s).
- `npm run lint` — repo **không có** script lint (package.json: dev/build/preview/test/test:e2e).
- Playwright (`h-d-verify.mjs`, login thật student seed + mock endpoint đổi contract, dev server 5174):
  - **13/13 OK**: 4 màn × light+dark @1366×768 → `overflow 0/0px`, `consoleErr 0`; 4 màn @390×844 → `overflow 0/0px`, `consoleErr 0`.
  - Premium QR: canvas 208×208 tồn tại, `role=img` + `aria-label="Mã QR chuyển khoản MB Bank"` giữ nguyên, countdown chạy `00:60 → 00:58`, overflow 0.
  - Ảnh: `docs/work/h-d-*.png` (12 màn + 1 QR modal), kết quả: `docs/work/h-d-screenshot-results.json`.

## Ghi chú / rủi ro

- **Contract backend đã đổi shape so với frontend** (pre-existing, ngoài phạm vi UI): `/me/quests` trả `{progress, reward:{gems,xp}}` thay vì `{current, rewardGems, rewardXp}`; `/premium/status` trả `{planId, status:"active"}` thay vì `{isPremium, plan, expiresAt}`; `/shop/items` trả `{itemKey, type, owned, maxStack}` không có `description/slot` → ShopView dùng icon fallback, SubscriptionView hiển thị EmptyState dù user đang có gói active. **Đề xuất đợt sau:** align DTO frontend ↔ backend (việc của dev khác — không đụng trong đợt này).
- Contrast: giá gems dùng chuẩn Badge warning `text-amber-700 dark:text-amber-400` (light 5.02:1 / dark 7.2:1) thay vì `--color-warning` raw (3.19:1 — fail AA).
- E2E/unit hiện có không tham chiếu selector của 4 view này → không phá test.
- Chưa push/merge — chờ vòng Ollama review của PM.

## Vòng fix P2 (13/08, commit b2d7f84) — shop nạp gems khi mount

- **Fix**: `frontend/src/views/ShopView.vue:36` — `fetchHearts()` → `gamification.fetchAll()` (nạp hearts/streak/premium khi vào thẳng /shop; không đổi store/contract).
- **Build**: PASS 0 lỗi · **Test**: 89/89 PASS.
- **Smoke (Playwright 1366×768, login thật + mock gamification theo e2e H-D)**: vào thẳng /shop (goto+reload) → 6 card, 0 overflow, 0 console error từ view; gems=0 vì **backend không có GET gems balance** (probe 9 endpoint: hearts/streak/quests/shop/inventory/premium/progress/achievements/notes — gems chỉ có trong POST buy/claim → P1 contract, cấm đổi đợt H). Luồng có gems (SPA sau claim 300): nút Mua enabled đúng 5/6 (chỉ XP boost 1000 disabled), mua Hint 300→270 + toast "Đã mua". → UI canAfford đúng; gems fresh-load chờ P1 (SETUP_TODO đợt I/J).
- **Ghi nhận (pre-existing, ngoài phạm vi)**: `GET /progress/me` 500 "An item with the same key has already been added. Key: 1" (backend, duplicate key seed) — đề xuất đợt I/J cùng P1.

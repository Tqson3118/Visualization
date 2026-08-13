# AUDIT — ShopView.vue (`/shop`) — nhóm C

> Audit theo `standard.md` + `frontend/DESIGN.md`. Ngày: 13/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Shop game mobile bất kỳ.** Hero gradient aurora + gem counter + icon slot tint gradient aurora/mint/sunset + price amber. Không chi tiết DSA. Điểm đặc trưng: 3/10.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | `gap: 6px` stat-value (243), `gap: 4px` price (298 — token OK), `gap: 2px` stat-block (233 — ngoài scale). Grid `auto-fill minmax(220px,1fr)` (256) — không theo 12 cột §8 (card grid 12 cột span 3–4). |
| 2 | Breakpoint | 5/6 | Grid auto-fill tự co OK; mobile hero-badge margin-left 0 (307). Chưa verify 3 mốc. |
| 3 | Animation | 10.5/14 | `hover-lift` trên shop card (117) — shadow-lg + translate 180ms `ease` (KILL-LIST V2 + shadow card). Không khoảnh khắc đầu tư (mua xong chỉ toast — có thể confetti §1 nhưng không bắt buộc). |
| 4 | Nhất quán thị giác | 7/14 | Hero gradient + blob + shadow (157–190); title gradient-clip (209–215); icon box gradient theo slot `shop__icon--aurora/mint/sunset` (278–280, 121–123) — KILL-LIST gradient trang trí + màu lung tung; price `text-amber-700 dark:text-amber-400` (129) — class màu ngoài token §2.2. `font-weight: 800` stat-value (245) + price (299). 3 trạng thái list OK (skeleton 102 / empty 106 / error toast 31 — thiếu error state riêng cho grid, chấp nhận vì toast + empty). |
| 5 | Interactive sizing | 13.5/16 | Nút "Mua" `size="sm"` (133) = CTA chính card → 36px < 40px chuẩn nút chính (§4.1). Không raw `<button`. Icon chip không clickable OK. |
| 6 | Typography | 7/10 | `font-weight: 800` (245, 299); H1 `text-2xl` (209) sai hierarchy; stat-value `text-lg` (244) ngoài quy chuẩn stat phụ (text-2xl §6); gems số không mono (93) — dữ liệu phải mono. |
| 7 | Depth & Elevation | 4.5/8 | Hero shadow-md; icon box shadow-sm (275); card dùng `.card` global (box-shadow: var(--shadow-md) — global.css 90) → cấm shadow card §6; 2 stat đồng nổi không phân cấp. |
| 8 | A11y | 10/12 | Nút có text OK; disabled nút Mua ✓; price `aria-label="Giá"` (129) OK. Contrast amber-700 trên card light ≈ 4.8:1 OK (nhưng dark amber-400 OK). |
| 9 | Code quality | 5.5/6 | Logic buy/atomic OK; SLOT_ICON/SLOT_TINT map gọn; `v-for` key item.id OK. |
| 10 | Performance | 6/6 | Route lazy, không chart nặng. |

**TỔNG hygiene (pre): 76/100** — dưới 80. Trục dưới sàn: depth 4.5/8 < 4.8 ✗.

## KILL-LIST vi phạm
- Hero gradient + blob + shadow; title gradient; icon slot gradient 3 màu lung tung → bỏ tint, icon chip đồng nhất.
- Gems = dữ liệu → hero-stat **block-token tối** (quyết định #3/#4/#5); item count = stat phụ level-1.
- Price amber class → token foreground + mono.
- Nút "Mua" → size md (40px).
- Card shadow (.card global) → override `box-shadow: none` trong view (§6).
- `font-weight` 800 → 600; H1 48px; gems mono.

# H-C — Fix token chung (Round 1)

Ngày: 2026-08-13 · Nhánh: `feature/ux-h-c` · Commit: `ba06735`

## Bối cảnh

dev-e2e báo 2 điểm token cần sửa sau đợt polish nhóm C (3 màn classes):

- **P2** — `--muted-foreground` dark = 3.34:1 (< 4.5:1) → yêu cầu "sửa tối lại" (~0.63-0.64).
- **P3** — `--gradient-sunset` light hơi chói (chữ trắng trên gradient) → tối nhẹ (0.55/0.55/0.45 → 0.5/0.5/0.42).

## Fix Round 1

### P3 — `frontend/src/styles/palettes.css` (light block) — ĐÃ SỬA

`--gradient-sunset` light: `oklch(0.55 0.13 75) / (0.55 0.16 45) / (0.45 0.18 20)` → `oklch(0.5 0.13 75) / (0.5 0.16 45) / (0.42 0.18 20)`.

Đo (WCAG chuẩn, Chromium resolve oklch → display sRGB, gamma đầy đủ):

| Stop | Trước | Sau |
|---|---|---|
| 0% (amber) | white 4.95:1 | white **6.11:1** ✓ |
| 50% (orange) | ~5.19:1 | white **6.41:1** ✓ |
| 100% (rose) | ~8.20:1 | white **8.99:1** ✓ |

Tất cả stop ≥ 4.5:1 (text thường), giữ nguyên chroma/hue → tông màu không đổi.

### P2 — `--muted-foreground` dark — KHÔNG SỬA (token đã pass; yêu cầu "tối lại" sẽ phá contrast)

**Phát hiện:** số 3.34:1 của dev-e2e là artifact của script đo. `parseCssColor()` trong `docs/work/h-c-verify.mjs` chuyển oklch → **linear** sRGB rồi `lumRgb()` lại gamma-hóa lần nữa (double-transform, thiếu bước gamma-encode khi parse oklch). Tái hiện chính xác 3.34:1 bằng cùng công thức với token hiện tại.

Đo lại bằng phép đo **chuẩn** (oklch → gamma-encode đầy đủ, Chromium + computed style thật):

| Nền | Contrast (token hiện tại `oklch(0.72 0.025 190)`) |
|---|---|
| `--color-surface` dark #0F3D3A | **4.89:1** ✓ |
| `--card` dark oklch(0.215 0.03 190) | **7.08:1** ✓ |
| Nền thật app (/classes dark, rgb(4,47,46)) | **5.90:1** ✓ |

→ Đã ≥ 4.5:1 ở mọi nền dark. Nếu tối về 0.63 như đề xuất, contrast chuẩn tụt về ~3.34-3.47:1 (fail thật) → **không áp dụng**; vẫn phân biệt rõ với `--foreground` (0.945 vs 0.72 lightness).

**Đề xuất (ngoài phạm vi round này):** sửa `parseCssColor()` trong `h-c-verify.mjs` thêm gamma-encode sau ma trận OKLab→linear (1 dòng) để e2e đo đúng WCAG.

## Verify

- `npm run build` → 0 lỗi.
- `npm test` → 89/89 PASS.
- Render 3 màn classes light+dark (reuse `h-c-screenshot.mjs`, MOCK_MODE): 8/8 ảnh, **overflow 0/0px, consoleErr 0** (classes 3 cards / detail 5 rows + tabs / report 4 KPIs + 5 rows).

## Ghi chú

- KHÔNG push/merge (chờ PM duyệt vòng Ollama).
- Script đo tạm: `C:\Users\ADMINI~1\AppData\Local\Temp\opencode\contrast\` (ngoài repo).

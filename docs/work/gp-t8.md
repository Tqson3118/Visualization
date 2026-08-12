# GP-T8 — Đồng bộ docs theo GP-T7 (Premium QR MB Bank) + rà NFR-5/THIRD_PARTY

> Ngày: 13/08/2026 · Nhánh: `feature/docs-sync-gp` (từ `dev` @ `0543411`) · Người: dev-docs (phuc) · Trạng thái: **DONE**
> Chỉ sửa `docs/` + `THIRD_PARTY.md` (root) — KHÔNG sửa code.

## Báo cáo từng mục

| # | Mục | Kết quả | Ghi chú |
|---|---|---|---|
| 1 | SRS.md §5.33 UC-32 + FR-10.7 | ✅ DONE | Bỏ câu cũ "KHÔNG tích hợp cổng thanh toán thật (SePay/VietQR = backlog)" → mô tả QR MB Bank (NGUYEN THI NHU HOA · 83863112088386, BIN 970422) + nội dung CK `DSV<UserId>T<months>` + kích hoạt tự động sau xác nhận (60s) — KHÔNG gọi API ngân hàng/webhook. Cập nhật §1.3.2 mục 3 + NFR-5 ghi chú đo lại. Version 1.3 → **1.4** + changelog. |
| 2 | SDD.md Màn 25/26 (§8) | ✅ DONE | Màn 26 viết lại luồng QR 2 bước (chọn gói → QR VietQR EMVCo + CK + "Tôi đã chuyển khoản" sau 60s); ghi rõ API `/premium/upgrade` trả `contentRef` + OrderRef `DSV{userId}T{months}`; §7.3.28 OrderRef; §3.9 bundle thật 13/08. Version 1.4 → **1.5** + history. |
| 3 | USER_GUIDE.md §3.10 | ✅ DONE | Hướng dẫn 4 bước: quét QR bằng app ngân hàng → nội dung CK tự động DSV → chờ 60s → "Tôi đã chuyển khoản" → kích hoạt tự động (kèm lưu ý mô phỏng). Version 1.1 → **1.2** + history. |
| 4 | THIRD_PARTY.md | ✅ DONE | Bổ sung `qrcode` 1.5.4 + `@types/qrcode` 1.5.6 (MIT — xác minh `npm ls --depth=0` + `node_modules/<gói>/package.json`); rà đủ 19 gói đợt G (không thiếu); tổng 41 gói top-level. Version 1.2 → **1.3** + history. Không ô license trống. |
| 5 | API_REFERENCE.md | ✅ DONE | §4.14: `/premium/upgrade` — planId `1m|3m|12m`, OrderRef `DSV{userId}T{months}`, response `contentRef` + ví dụ JSON; `/premium/mock-pay` làm rõ sau xác nhận; §8 thêm 2 dòng thay đổi. Version header sửa lệch (1.2 → **1.4** — 1.3 đã tồn tại trong changelog) + changelog. |
| 6 | NFR-5 đo lại | ✅ DONE | `npm run build` (frontend/) PASS 0 lỗi: JS gốc tải lần đầu ≈ **852 KB** (không đổi — qrcode vào chunk lazy PremiumView), engine **476 KB** gốc/120 KB gzip (≤500KB ✓), echarts 324 KB lazy, tổng dist ≈ 2.08 MB. KHÔNG vượt ngưỡng → KHÔNG nới; ghi decision log (pm-decision-log-gp.md mục T8) + TEST_PLAN TEST-PERF-007 đồng bộ. |
| 7 | TEST_PLAN.md §10 | ✅ DONE | Số thật 13/08/2026: BE 81 unit + 31 integration, FE 89 unit (gồm +7 vietqr) + 13 e2e → tổng **214**; TEST-PERF-007 + TEST-B-178 cập nhật. Version 1.3 → **1.4** + history. |

## Bundle thật (npm run build — frontend/, 13/08/2026)

| Chunk | Gốc | Gzip | Ghi chú |
|---|---|---|---|
| engine | 476 KB | 120 KB | preload — ≤ 500KB ✓ (NFR-5) |
| echarts | 324 KB | 110 KB | lazy (VChartLazy) — không vào tải lần đầu |
| vendor | 143 KB | 54 KB | preload |
| index (entry) | 106 KB | 34 KB | preload |
| PremiumView | 32 KB | 12.5 KB | lazy — chứa `qrcode` ~30KB (GP-T7) |
| JS gốc tải lần đầu | ≈ 852 KB | ≈ 256 KB | tổng modulepreload + entry — ≤ 1.5MB ✓ |
| Tổng JS gốc toàn dist | ≈ 2.08 MB | — | gồm chunk lazy (view, echarts, compiler.worker) |

So sánh đợt G (12/08): JS lần đầu 852KB (không đổi), engine 476KB (không đổi), tổng dist 1.95MB → 2.08MB (+qrcode ~32KB lazy). Không cần nới NFR-5.

## Verify

- [x] `npm run build` (frontend/) — PASS 0 lỗi (vue-tsc + vite), số liệu bundle thật ở trên.
- [x] Grep SRS/SDD/USER_GUIDE: hết câu cũ "KHÔNG tích hợp cổng thanh toán thật (SePay/VietQR = backlog)" mô tả sai (đã thay bằng "KHÔNG gọi API ngân hàng/webhook — mô phỏng").
- [x] THIRD_PARTY: có `qrcode` + `@types/qrcode` (MIT), không ô license trống; đối chiếu package.json thực tế (41 gói top-level).
- [x] Đọc lại markdown: bảng không vỡ (pipe escape `\|` trong bảng API_REFERENCE §4.14).
- [x] `qrcode` license: `node_modules/qrcode/package.json` → MIT; `@types/qrcode` → MIT.

## Commit

- Commit docs → **phuc** qua `commit-as.ps1` (docs → phuc): message `docs: GP-T8 - dong bo docs theo GP-T7 (Premium QR MB Bank) + do lai NFR-5/THIRD_PARTY (qrcode)`.

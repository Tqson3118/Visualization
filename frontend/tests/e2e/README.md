# E2E Tests (Playwright) — Placeholder

Playwright **chưa được cấu hình** trong skeleton này.

## Trạng thái
- Chưa có `playwright.config.ts`, chưa cài `@playwright/test`, chưa có test nào.
- E2E sẽ được thêm khi các view chính (simulator, ladder, code runner) đi vào giai đoạn ổn định.

## Kế hoạch (task sau)
1. `npm i -D @playwright/test` + `npx playwright install chromium`.
2. Tạo `playwright.config.ts` (baseURL: `http://localhost:5173`, webServer: `npm run dev`).
3. Các spec đề xuất theo `docs/TEST_PLAN.md`:
   - `auth.spec.ts` — đăng ký/đăng nhập/guard route.
   - `simulator.spec.ts` — màn 05: play/pause/step, `?step=N` (FR-2.11).
   - `ladder.spec.ts` — màn 14: 3 bậc, trừ tim (FR-10.1).
   - `code-runner.spec.ts` — màn 16: chạy/nộp code, limit 10s/64MB/200 dòng.

## Lưu ý
- Cấm để test E2E phụ thuộc dữ liệu backend thật — dùng route mock hoặc seed riêng.
- Screenshot baseline cho báo cáo theo `docs/BAO_CAO_SPEC.md` §6 (12 màn tối thiểu).

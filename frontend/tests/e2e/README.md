# E2E Tests (Playwright)

## Trạng thái
- ✅ Đã cài `@playwright/test@1.62.1` (devDependency) — `npm i -D @playwright/test` thành công.
- ✅ Chromium: `npx playwright install chromium` — đã có sẵn trong cache `ms-playwright` (`chromium-1234` khớp 1.62.1), không cần tải lại.
- ✅ `playwright.config.ts` + 4 spec: auth / simulator / ladder / code-runner (theo `docs/TEST_PLAN.md` §7.1).

## Cách chạy
```bash
npm run test:e2e                      # chạy toàn bộ (webServer tự khởi động `npm run dev` port 5174 — reuse nếu đã chạy)
npx playwright test auth.spec.ts      # chạy 1 spec
npx playwright show-report            # xem HTML report (nếu bật reporter html)
```

## Yêu cầu môi trường
- **Backend KHÔNG bắt buộc**: mọi request `/api/v1/**` bị route-mock (`helpers/mockApi.ts`) — KHÔNG dùng dữ liệu backend thật.
- **Port E2E riêng 5174** (`--strictPort`): port 5173 trong môi trường này đang bị chiếm bởi relay Docker/WSL phục vụ bản STALE của app → KHÔNG dùng 5173.
- Chỉ chromium desktop — mobile (<768px) ngoài phạm vi TEST_PLAN §7.1.
- Lưu ý token auth: chỉ nằm trong memory Pinia (ADR-004) → sau mỗi `page.goto()` trạng thái login mất. Spec phải login qua UI trong CÙNG phiên (`helpers/auth.ts` — `loginViaUi`) hoặc dùng demo key công khai (simulator `sort.bubble`).

## Endpoint đã mock (`helpers/mockApi.ts` — shape theo `src/api/*`)
| Method | Path (sau `/api/v1`) | Trả về |
|---|---|---|
| POST | `/auth/login` | `{ accessToken, expiresIn, user }` (LoginResponse) |
| POST | `/auth/register` | 201 LoginResponse (student) |
| POST | `/auth/refresh`, `/auth/logout` | `{ accessToken, expiresIn }` / 204 |
| GET | `/auth/me` | UserSummary |
| GET | `/topics` | Topic[] (1 topic "Sắp xếp & Tìm kiếm") |
| GET | `/lessons`, `/lessons/{id}` | PagedResponse rỗng / LessonDto |
| GET | `/progress/me` | ProgressOverviewDto rỗng |
| GET | `/me/hearts`, `/me/streak`, `/premium/status` | HeartsStatusDto (stateful) / streak / premium |
| GET | `/learning-path/{id}` | LearningPathDto (node 1 = Bubble Sort active) |
| POST | `/learning-path/{id}/nodes/{nodeId}/enter` | `{ session, heartsLeft }` — **hearts stateful: trừ 1 mỗi lần gọi** (FR-10.1) |
| GET/POST/DELETE | `/favorites`, `/favorites/{id}` | `[]` / FavoriteDto / 204 |
| POST | `/code-runs` | 201 CodeRunSummary (passed) |
| GET | `/exercises/{id}/code-submissions/me` | `[]` |
| * | mọi `/api/v1/**` khác | 404 `{ error: { code: 'NOT_FOUND', ... } }` (fallback an toàn) |

## Spec & ghi chú (trạng thái thật của view)
- **auth.spec.ts** — TEST-UI-001 (register→login→/path), TEST-UI-005 (guard /profile → `/login?redirect=...` → quay lại). Selector theo LoginView/RegisterView/PathRedirectView.
- **simulator.spec.ts** — TEST-UI-001 bước 4 (play ~2s / pause / step lùi), TEST-UI-003 (phím tắt Space/→/←); demo key `sort.bubble` (FR-7.6 — không cần login).
  - ⚠ **FR-2.11 (deep-link `?step=N`)**: SimulatorView CHƯA đọc `query.step` (kiểm tra src ngày 12/08/2026) → spec ghi nhận hành vi hiện tại (bước giữ 1, query giữ nguyên). Bổ sung assert khi view triển khai (chờ task sau).
- **ladder.spec.ts** — TEST-UI-005 (guard /ladder/*), TEST-UI-007 một phần (stepper 3 bậc Quiz/Lab/Code, Quiz active, Lab/Code locked), FR-10.1 (trừ tim: luồng PathView → POST enter → heartsLeft giảm → widget tim header cập nhật).
  - ⚠ **TEST-UI-007 đầy đủ** (Quiz ≥60% → Lab → Code ≥70%): LadderView hiện truyền `quiz-exercise=null` / `code-exercise-id=null` → chưa có dữ liệu quiz/code để pass qua UI. Chờ task gắn dữ liệu vào LadderView.
- **code-runner.spec.ts** — Màn 16: guard, render editor textarea (KHÔNG Monaco), chạy code mẫu → "Thành công · Xms" (sandbox client + mock POST /code-runs).
  - ⚠ **FR-9.6 (chặn > 200 dòng / 10s / 64MB)**: view CHƯA có kiểm tra giới hạn dòng → spec ghi nhận hành vi hiện tại (không có thông báo chặn "200 dòng"). Bổ sung assert khi view triển khai (chờ task sau). KHÔNG tự thêm feature vào view trong task này.

## Lưu ý kỹ thuật
- `playwright.config.ts` + `tests/e2e/**` KHÔNG nằm trong `tsconfig.app.json`/`tsconfig.node.json` → không ảnh hưởng `npm run build` (vue-tsc). `vite.config.ts` đã thêm `exclude: ['tests/e2e/**']` cho vitest (tránh vitest chạy nhầm spec Playwright).
- **Workaround đã probe (12/08/2026)**:
  - RegisterView: click checkbox/submit khi đang focus input text bị NUỐT (blur → validate → re-render đổi layout giữa mousedown/mouseup). Spec xử lý: check checkbox TRƯỚC khi fill + blur input cuối (Tab) trước khi bấm submit.
  - CanvasArea (simulator/code-runner) có vòng `ResizeObserver loop completed with undelivered notifications` → layout dịch liên tục → nút không "stable" → click ControlBar/nút Chạy dùng `{ force: true }`. Bug app tiềm ẩn, đề xuất task sửa riêng.
  - App bug khác phát hiện khi probe: `RegisterView.validate()` dùng `Object.assign(fieldErrors, errors)` — khi `errors={}` không xóa key cũ → stale error có thể hiển thị tới khi có key mới ghi đè (cosmetic).
- Screenshot baseline cho báo cáo theo `docs/BAO_CAO_SPEC.md` §6 (12 màn tối thiểu) — chưa làm trong task này.

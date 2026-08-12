# GP-T4 — Breakpoint theo dòng template (pseudocode) trong simulator

- **Nhánh:** `feature/breakpoints` (tạo từ `dev` HEAD `981cf5b`)
- **Ngày:** 2026-08-13
- **Phạm vi (TỐI THIỂU theo PROMPT):** UI toggle breakpoint trên dòng pseudocode + auto-pause khi stepExecutor/play loop chạm bước có `pseudocodeLine` ∈ breakpoints. KHÔNG làm trình soạn thảo phức tạp, KHÔNG sửa `engines/*`.

## 1. Kết luận khảo sát — KHẢ THI ✅

| Câu hỏi | Bằng chứng | Trả lời |
|---|---|---|
| `Step` có field `pseudocodeLine`? | `engines/core/types.ts:30` — `pseudocodeLine: number; // dòng mã giả 1-based` | ✅ CÓ |
| stepExecutor/generators có mang `pseudocodeLine` theo từng bước? | `engines/generators/helpers.ts:79` — `Trace.push()` gán `pseudocodeLine: opts.line` cho MỌI step; `generators/sort/bubble.ts:167` đọc lại field này | ✅ CÓ — mọi generator sinh qua `Trace.push(line, …)` đều mang dòng 1-based |
| UI đã đọc `pseudocodeLine`? | `views/SimulatorView.vue:238` — `:active-line="currentStep?.pseudocodeLine ?? 0"`; `PseudocodePanel.vue:82` highlight dòng active | ✅ CÓ |
| UI đã có sẵn emit breakpoint? | `PseudocodePanel.vue:84` — `@click="emit('toggle-breakpoint', idx + 1)"` (click dòng) nhưng store chưa xử lý (`simulation.ts:186` `setBreakpoint` là no-op TODO) | ✅ CÓ — chỉ cần nối store + hiển thị |

→ Breakpoint theo dòng **KHẢ THI** với engine hiện tại: mỗi `Step` đã mang `pseudocodeLine`, không cần đổi engine.

## 2. Thiết kế (tối thiểu)

- **Store (`stores/simulation.ts`)**:
  - `breakpoints: ref<Set<number>>` — tập dòng pseudocode (1-based) đang bật.
  - `toggleBreakpoint(line)` — thêm/xóa (thay thế `setBreakpoint` no-op).
  - `hitBreakpointAtCurrentStep()` — sau khi advance: nếu `currentStep.pseudocodeLine ∈ breakpoints` → `status = 'paused'` + `clearPlayback()` + `breakpointHit = line`.
  - Gọi kiểm tra ở **play loop** (trong `startPlayback` tick, sau `currentIndex += 1`) và **`stepForward`** (bước tay cũng dừng).
  - `breakpointHit: number | null` — dòng vừa chạm; xóa khi `play`/`stepBack`/`jumpTo`/`reset`/`loadSim`/`configureInput`.
  - Resume: play tiếp từ breakpoint → advance ≥1 bước rồi mới check lại (dừng đúng bước kế tiếp chạm dòng — hành vi debugger chuẩn, không kẹt tại cùng bước).
- **`PseudocodePanel.vue`**: prop `breakpoints?: Set<number>`; mỗi dòng thêm nút chấm tròn toggle (`@click.stop` → emit; đỏ `--color-destructive` khi bật; `aria-pressed` + `data-bp`/`data-bp-line` cho a11y/test). Giữ click cả dòng như cũ.
- **`SimulatorView.vue`**: wire `:breakpoints` + `@toggle-breakpoint`; badge đỏ `role="status" data-testid="breakpoint-badge"` — "Đã dừng tại breakpoint dòng N" khi `breakpointHit !== null && status === 'paused'`.
- **`composables/useSimulation.ts`**: expose `toggleBreakpoint` (thay `setBreakpoint`).
- Không đổi `engines/*`, không đổi play/pause/step cũ, không đổi selector e2e (Bước tới/Bước lùi/Chạy/Tạm dừng).

## 3. Thay đổi

- `frontend/src/stores/simulation.ts` — breakpoints/breakpointHit/toggleBreakpoint + auto-pause (play loop + stepForward).
- `frontend/src/components/simulator/PseudocodePanel.vue` — prop breakpoints + nút chấm tròn toggle.
- `frontend/src/views/SimulatorView.vue` — wire breakpoints + badge "Đã dừng tại breakpoint dòng N".
- `frontend/src/composables/useSimulation.ts` — expose `toggleBreakpoint`.
- `frontend/src/stores/simulation.spec.ts` — MỚI: 4 test (toggle add/remove · auto-pause play loop dòng 5 với fake timers · stepForward auto-pause · reset giữ breakpoints/xóa hit). Lưu ý: cần `import '@/engines/catalog'` (side-effect đăng ký generator — store chỉ import registry).

## 4. Verify

- [x] `npm run build` — **0 lỗi** (vue-tsc -b && vite build ✓)
- [x] `npm test` — **82/82 PASS** (78 cũ + 4 mới `simulation.spec.ts`)
- [x] `npx playwright test` — **13/13 PASS** (26.2s — simulator spec 3/3 bắt buộc)
- [x] Smoke logic (unit test mô phỏng đúng kịch bản smoke): `/simulator/sort.bubble`, breakpoint dòng 5 → play → dừng tại bước đầu tiên có `pseudocodeLine === 5` (bubble dòng 5 = `if a[j] > a[j+1] then`, xuất hiện từ bước ~3), `status = paused`, `breakpointHit = 5`; timer ngừng (advance thêm không tiến bước).

## 5. Commit

- `git commit` (commit-as.ps1 son) trên `feature/breakpoints` — hash xem `git log`. KHÔNG merge (theo quy trình PROMPT).

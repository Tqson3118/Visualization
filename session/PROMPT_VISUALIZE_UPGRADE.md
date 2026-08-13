# PROMPT_VISUALIZE_UPGRADE — Nâng cấp engine visualize (trace-driven, wrap mảng dài, stack/queue "thở", graph)

Dán vào `/pm "..." --auto`.

## BÀI HỌC VẬN HÀNH (bắt buộc đọc trước khi dispatch)

- **LỖI "TASK TRẢ RỖNG" (13/08, 4/4 lần)**: KHÔNG nhúng file >5KB vào prompt task — task CHỈ trỏ đường dẫn, agent tự đọc. Task nhỏ + fresh context + explore trước khi code. 2 lần fail → ghi FAIL + lý do.
- **Không tự chấm bài mình**: dev viết → dev-test verify độc lập → dev-review chốt APPROVE/CHANGES REQUESTED.
- **--auto**: ghi quyết định vào `docs/pm-decision-log-visualize.md` TRƯỚC khi làm. Mọi thay đổi hành vi phải có test; KHÔNG đổi generator/catalog (golden test phải giữ nguyên).
- **PR base `dev` KHÔNG main**. Commit-as: engine/test → `thu`. PR base `dev`.
- **CẤM thêm thư viện mới** (session này không cần — canvas thuần + coreAnimationEngine có sẵn).

⚠ **RÀNG BUỘC CHẠY**: làm việc ở worktree RIÊNG `D:\FPT\neww-engine` (chưa tồn tại — tạo trước):
```
git fetch origin dev
git worktree add D:\FPT\neww-engine -b feature/vis-upgrade origin/dev
```
KHÔNG đụng working tree `D:\FPT\neww`. **KHÔNG sửa `frontend/src/views/*.vue` trừ CodeRunnerView (nối trace playback — 1 file duy nhất, sửa tối thiểu, tách logic vào composable để session MASTER Phase 1 không vướng)**. KHÔNG sửa backend/DB/i18n/vi.ts (trừ 1-2 message mới cần thiết → ghi decision log).

## BỐI CẢNH (hiện trạng đã verify 13/08 tối)

- Engine: `frontend/src/engines/` — `core/stepExecutor.ts` (EDV: Babel instrument code user → `TraceEvent[]` line/vars/highlight/kind/explanation, giới hạn 50.000 event; có sẵn `toTraceEvent` + `statsFromTrace` + `runCode`), 22 generator (44 sim trong `catalog.ts` — KHÔNG đụng), `renderers/` (arrayRenderer **bar-mode V3 đã merge**; stackQueueRenderer vẽ TĨNH có FLY_OFFSET=18 cho pop; graphRenderer circular layout; treeRenderer; hashTableRenderer; listRenderer), `core/coreAnimationEngine.ts` (lerp/lerpPoint/rAF loop — đã có test), `worker/` (compileWorker + runMeasure).
- `frontend/src/stores/codeRunner.ts`: `run()` → `runCode(...)` → **chỉ giữ `result.stats`** (comparisons/swaps/writes/durationMs) → lưu API `/code-runs`. **Trace KHÔNG được giữ để visualize** — canvas CodeRunner hiện hiển thị steps từ GENERATOR (`simStore.loadSim(key)`), không phải trace code user thật.
- `frontend/src/components/simulator/CanvasArea.vue`: watch structure → `render()` trực tiếp từng step (KHÔNG có animation chuyển cảnh giữa 2 structure liên tiếp).
- `frontend/src/views/CodeRunnerView.vue`: màn 16 — editor + run + canvas (generator preview) + StatsBar.
- Test hiện có: `engines/__tests__/` — catalog.spec (10, golden 44 sim), renderers.spec (15), stepExecutor.spec, compileWorker.spec, coreAnimationEngine.spec, simulation.spec; toàn FE 95/95 + vue-tsc sạch.

## MỤC TIÊU (4 task — mỗi task 1 vòng: dev → test → review, KHÔNG làm dồn)

**Task 1 — Trace-driven visualization cho code user tự viết (QUAN TRỌNG NHẤT — "viết code tới đâu visual tới đó")**

- Explore trước (chỉ trỏ đường dẫn, không đọc cả repo): `stores/codeRunner.ts`, `views/CodeRunnerView.vue`, `components/simulator/CanvasArea.vue`, `engines/core/stepExecutor.ts` (phần runCode/toTraceEvent/PlaybackFrame), `engines/core/types.ts` (Structure/Element), `components/simulator/StatsBar.vue`.
- Thiết kế: tạo composable `frontend/src/composables/useCodeTracePlayback.ts` (không nhúng vào store):
  - Nhận `TraceEvent[]` + binding cấu trúc (mảng số từ input) → sinh dãy `Structure` frame: mỗi event ánh xạ highlight (kind swap/compare/assign → status tương ứng trên cell), vars snapshot → biến hiển thị.
  - Giới hạn hiển thị: trace > 3.000 event → **sample đều** (bước = ceil(len/3000)) giữ event cuối (trạng thái final phải đúng). KHÔNG đẩy 50.000 frame vào UI.
  - Playback API: `frames`, `currentIndex`, `play()/pause()/step()`, `durationPerStep` (mặc định 250ms — theo bảng chuẩn motion: transition 200-300ms), hủy đúng trong `onUnmounted`.
  - Dữ liệu line → highlight dòng code: tái sử dụng pattern `pseudocodeLine` của simulator (nếu có) — chỉ thêm phần map line→dòng editor nếu rẻ; KHÔNG bắt buộc.
- Nối UI (sửa DUY NHẤT `views/CodeRunnerView.vue`, tối thiểu): sau khi `run()` thành công + có trace → chuyển canvas từ generator-preview sang trace-playback (nút Play/Step nhỏ tái dùng control pattern của SimulatorView); khi trace rỗng/error/timeout → GIỮ hành vi cũ (generator preview + stats). `StatsBar` giữ nguyên (số liệu từ `lastStats` — không đổi contract).
- Test (bắt buộc): `stepExecutor.spec.ts` THÊM test "sửa code → trace đổi" (2 code khác nhau cùng input → số event/thứ tự swap khác nhau — chứng minh KHÔNG hardcode); `useCodeTracePlayback` spec mới (sample đúng: 5.000 event → ~3.000 frame, frame cuối = trạng thái cuối thật; play/step/stop không leak timer).

**Task 2 — Wrap layout mảng dài (ArrayRenderer bar-mode)**

- Hiện trạng (đã verify): `renderers/arrayRenderer.ts` bar mode — 1 hàng duy nhất, `barW = max(14, min(88, slotW-6))` → mảng dài (n≥40) bar mỏng dính, n rất lớn tràn margin.
- Thiết kế: khi `(w - 2*margin)/n < 44` → chia nhiều hàng: `maxPerRow = max(10, floor((w - 2*margin)/44))`, `rows = ceil(n/maxPerRow)`; mỗi hàng vẽ như bar-mode hiện tại (tỉ lệ giá trị theo maxVal TOÀN MẢNG, không theo hàng); index đánh số TOÀN CỤC (0..n-1) dưới từng bar; hàng trên cách hàng dưới ≥ 40px (chỗ index); con trỏ (`positions`) trỏ đúng hàng/ô. `n ≤ 36` giữ nguyên hành vi cũ (test cũ không vỡ).
- Test: renderers.spec THÊM — n=60 trên canvas 800×600 → 2 hàng, index đúng 0..59, không throw, mọi roundRect width > 0; n=5 → hành vi cũ (số fillText không đổi).

**Task 3 — Stack/Queue "thở" khi push/pop (signature element)**

- Hiện trạng (đã verify): `renderers/stackQueueRenderer.ts` vẽ TĨNH mỗi frame; `CanvasArea` render thẳng khi structure đổi; `coreAnimationEngine` có lerp + rAF (đã test).
- Thiết kế: composable `frontend/src/composables/useStructureTransition.ts`:
  - Giữ `prev` structure; khi `next` cùng kind (stack/queue) và khác `prev` → animate delta 200ms (ease-out): **push/enqueue** → cell mới trượt vào từ đỉnh/đuôi (translate y/x từ ngoài vào); **pop/dequeue** → cell đầu bay lên (FLY_OFFSET hiện có) + fade opacity về 0 rồi vẽ lại layout; các cell khác dịch chuyển tịnh tiến 1 bước. Chỉ animate `transform`/`opacity`.
  - `prefers-reduced-motion` (media query) → vẽ thẳng không animate. Canvas resize/zoom/kind đổi → vẽ thẳng (fallback).
  - KHÔNG đổi layout tĩnh frame cuối — renderer vẽ `next` đúng như cũ sau khi animate (golden renderers.spec không vỡ).
  - Nối vào `CanvasArea.vue` (component, KHÔNG phải view): thay call `renderer.render(structure)` bằng `useStructureTransition` khi kind ∈ {stack, queue} — đảm bảo `renderer.dispose()`/unmount đúng.
- Test: spec mới cho composable (push: prev 3 cell → next 4 cell → có frame trung gian trước khi final; pop: bay + fade; reduced-motion → không frame trung gian; unmount hủy rAF) + renderers.spec cũ vẫn PASS.

**Task 4 (TÙY CHỌN, làm sau 3 task trên + test xanh) — GraphRenderer nhẹ**: nếu element có `meta.x`/`meta.y` (generator cung cấp tọa độ) → dùng tọa độ đó; ngược lại giữ circular layout hiện tại. Không force-directed (quá tầm). Test: graph có meta.x/y → vị trí đúng (arc tại tọa độ).

## KHÔNG LÀM (cấm)

Không đổi generator/catalog/steps (golden `catalog.spec.ts` PHẢI giữ nguyên 44 sim + số bước); không đổi contract API `/code-runs` (payload cũ giữ nguyên — chỉ FE đổi cách dùng trace); không thêm thư viện; không sửa view khác ngoài CodeRunnerView; không đổi token màu/CSS design system (session MASTER Phase 1 đang quản lý UI); không đụng backend/DB.

## TIÊU CHUẨN HOÀN THÀNH (đo đếm được)

1. `npx vue-tsc --noEmit` sạch + `npx vitest run` FULL PASS (95 cũ + test mới: ≥6 unit mới cho Task 1-3 + spec composable). `catalog.spec` + `renderers.spec` cũ KHÔNG đổi nội dung, vẫn PASS.
2. Task 1: chạy thật trên `:5174` (login student@demo.local/Student@123) — mở `/code/:key` → sửa code → Run → canvas phát lại trace code THẬT (không phải generator preview) → **pixel-verify**: script đọc canvas sau 3 step — có nội dung thay đổi theo step (2 snapshot khác nhau), không rỗng. Chụp ảnh trước/sau lưu `docs/work/vis-upgrade/`.
3. Task 2: pixel-verify n=60 — bar ở ≥2 hàng, không tràn khỏi canvas (mọi bar x ∈ [margin, w-margin]).
4. Task 3: verify bằng unit test (frame trung gian) + xem nhanh bằng mắt qua Ollama (tùy chọn): chụp 2 ảnh liên tiếp lúc push → "block có di chuyển không" — model trả lời không rõ thì dựa vào unit test.
5. Không thêm dependency mới (grep package.json diff = 0 ngoài lock thay đổi bất thường).
6. dev-review chốt APPROVE/CHANGES REQUESTED (kiểm: không vỡ luồng code-runner cũ khi trace rỗng, không leak timer/rAF, không đổi hành vi generator).
7. Ghi decision log: thiết kế trace sampling, wrap threshold 44px, reduced-motion fallback, phạm vi sửa view.

## LỆNH VERIFY THẬT

```powershell
# Từ D:\FPT\neww-engine\frontend
npx vue-tsc --noEmit -p tsconfig.app.json
npx vitest run            # full suite — phải PASS
npm run build             # kiểm bundle không tăng (không thêm lib nên không đổi đáng kể)
```

## GIT

- Nhánh: `feature/vis-upgrade` từ origin/dev (worktree `D:\FPT\neww-engine`) → **PR base `dev`** (GitHub REST API: `$tok=[Environment]::GetEnvironmentVariable('GITHUB_TOKEN','User')`; POST `/repos/Tqson3118/Visualization/pulls` headers `Authorization: Bearer`, `User-Agent: opencode`).
- Commit tối đa 4 (theo task): `.\commit-as.ps1 thu "feat(engine): trace-driven playback code user (Task 1)"` · `... "feat(engine): wrap layout mang dai (Task 2)"` · `... "feat(engine): stack/queue thở push-pop (Task 3)"` · docs nếu tách.
- Trạng thái: `docs/work/vis-upgrade.md`. Quyết định: `docs/pm-decision-log-visualize.md`. Log: `docs/pm-report-visualize.md`.

## BÁO CÁO (≤10 dòng)

Task 1-4 hoàn thành/từ chối kèm lý do; file đã sửa (engine/composable/view); số test mới + kết quả full suite; pixel-verify Task 1/2 (số liệu); bundle trước/sau; PR link; lệch docs (nếu có); đề xuất bước sau (KHÔNG làm).

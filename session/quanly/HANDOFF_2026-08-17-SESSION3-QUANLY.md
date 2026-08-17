# HANDOFF — 17/08/2026 (phiên 3: Selection/Insertion + visualizer riêng, Stack & Queue sandbox 5 chế độ, Profile tím + flame, Grokking DS 8 module, gỡ khóa node)

> File bàn giao PHIÊN NÀY. Phiên mới đọc file này + `HANDOFF_2026-08-17-SESSION2-QUANLY.md` là nắm hết.
> Backend/FE đang chạy: BE http://localhost:5000 · FE http://localhost:5173.
> CHÚ Ý: Đang ở chế độ **DisableNodeLocks = true** (xem §5) — toàn bộ node lộ trình mở, chưa khóa.

## 1. TÓM TẮT PHIÊN

| # | Việc | Trạng thái |
|---|---|---|
| 1 | **Sorting Sandbox**: thêm **Selection Sort** + **Insertion Sort** (generator `algorithms/selectionSort.ts`, `insertionSort.ts`; type `SortAlgorithm`, `useSortingAnimation`, `SortingAlgorithmControls`, dispatcher, trace table, labels) | ✅ |
| 2 | **Fix bug display Insertion**: `sortingIdEnricher` swap-identity sai cho shift → loại `insertion` khỏi nhánh swap; sau đó đổi sang **giữ id ổn định theo vị trí** (hết "7 cột" ghost-id) | ✅ |
| 3 | **Visualizer riêng**: `SelectionSortVisualizer.vue` (badge ▼ MIN + phân vai trò màu cam/hồng), `InsertionSortVisualizer.vue` (chip KEY nổi + slot gap nét đứt; dùng absolute, KHÔNG phá layout) | ✅ |
| 4 | **Stack & Queue Sandbox MỚI** (`views/stackqueue/`): tab 4 trong SortingView, route `/stack-queue-sandbox`, indicator xanh dương | ✅ |
| 5 | **Engine stack/queue**: `stackQueueEngine.ts` — 5 chế độ: Stack(LIFO) · Queue(FIFO) · **Tròn**(Circular) · **Deque**(2 đầu) · **Min/Max Stack**; mỗi step có `indices` (top/front/rear/size) + `extraRows` (MIN/MAX) | ✅ |
| 6 | **Cấu trúc giao diện**: 2 gốc **Stack/Queue** (top bar) → biến thể con là pills trong **khung bo tròn giống `.algorithm-controls` của Sorting** (không nhét sidebar) | ✅ |
| 7 | **Fix lỗi phông (mojibake + font)**: StackQueueView bị hỏng encoding do PowerShell Set-Content → viết lại UTF-8; bỏ `'Inter', sans-serif` (không load) → `var(--font-sans)` cho stackqueue + searching; `monospace` → `var(--font-mono)` | ✅ |
| 8 | **Profile**: làm mới theo app (glass card + glow), **chuyển tông TÍM** (`--p-purple` #8b5cf6/#a78bfa/#7c3aed, remap `--primary`+`--data-core` trong scope .profile), **flame animation** cho streak | ✅ |
| 9 | **Grokking Data Structures 4→8 MODULE**: thêm M5 BST/AVL, M6 Heap, M7 Đồ thị, M8 Trie/Set-Map (8 bài + 4 quiz + 4 assignment + lab keys); Final-Quizz 20→40 câu | ✅ |
| 10 | **Gỡ khóa node TẠM THỜI**: `ConceptsController.DisableNodeLocks = true` (mở hết bài/quiz/assignment để xem trước) | ✅ (đang bật) |
| 11 | **Fix chữ đen trên lộ trình**: ép `bg-vdsa-bg` cho CoursesListView + CourseDetailView (trước: chữ trắng trên nền app theme sáng → mất hết) | ✅ |

## 2. THAY ĐỔI FRONTEND (chi tiết)

### `src/features/algorithm-sandbox/`
- **Mới**: `algorithms/selectionSort.ts`, `algorithms/insertionSort.ts` (frame `selectionMinIndex`/`insertionGapIndex`/`insertionKeyIndex`), `components/SelectionSortVisualizer.vue`, `components/InsertionSortVisualizer.vue`
- `types/sorting.types.ts`: `SortAlgorithm` thêm `"selection" | "insertion"`; SortFrame thêm `selectionMinIndex`/`insertionGapIndex`/`insertionKeyIndex`
- `helpers/sortingIdEnricher.ts`: **QUAN TRỌNG** — insertion giữ **id ổn định theo vị trí** (không ghost id → không cột thừa); bubble/selection/quick/heap vẫn dùng swap-identity + greedy
- `SortingVisualizerDispatcher.vue`: selection→SelectionSortVisualizer, insertion→InsertionSortVisualizer
- `SortingAlgorithmControls.vue` (+2 nút), `SortingTraceTable.vue` (+2 cột nhóm), `SortingDrawerTrace.vue`/`SortingDetailPanel.vue` (+2 nhãn)
- `__tests__/`: sorting.spec.ts (+ regression display/id ổn định), insertionFuzz.spec.ts, insertionStress.spec.ts (500 mảng random), sortingP0Tests (mount visualizer)

### `src/views/stackqueue/` (MỚI — Stack & Queue Sandbox)
- `stackQueueEngine.ts`: thuần logic, test được. `DsMode = 'stack'|'minmax'|'queue'|'circular'|'deque'`; `DsOp` push/pop/peek/pushFront/popFront; `DsStep` gồm cells/pointers/label/log/ok/isFinal/opIndex/`indices`/`extraRows?`; `generateRandomOps` sinh chuỗi hợp lệ theo chế độ
- `StackQueueView.vue`: top bar 2 gốc Stack/Queue; khung "Biến thể của X" (pills: Stack→Cơ bản/Min-Max, Queue→FIFO/Tròn/Deque); nút thao tác đổi theo chế độ; slider dung lượng (3-9) CÓ watch(capacity); presets theo chế độ; thanh mô tả bước + chips `indices`; VCR (speed/◀▶/progress bấm nhảy/↺); banner lỗi tràn/rỗng
- `stackQueueEngine.spec.ts` (26 test) + `StackQueueView.spec.ts` (mount) + `stackQueueFullCheck.spec.ts` (18 test: từng bước khớp algorithm, animation Δ±1, mô phỏng tham chiếu, dung lượng/deque/presets/random) + `allSandboxesStress.spec.ts` (sorting 9×25 + 4 mode×50 qua VCR)

### `src/views/sorting/SortingView.vue`
- Tab 4 "Stack & Queue" (icon `stack`), `initialTabFromRoute` nhận `stack-queue-sandbox`, indicator `.tab-indicator--stack-queue` (xanh `#93c5fd→#3b82f6→#1d4ed8`)

### Router / Header / App
- `src/router/index.ts`: route `/stack-queue-sandbox` name `stack-queue-sandbox` → SortingSandboxView
- `src/App.vue`: `SANDBOX_ROUTES` thêm `'stack-queue-sandbox'`
- `src/components/layout/AppHeader.vue`: dropdown + mobile link "Stack & Queue Sandbox"
- `src/shared/components/BaseIcon.vue`: icon `stack`, `queue`

### Profile (`src/views/ProfileView.vue`)
- Scope `.profile`: `--p-purple` #8b5cf6 / #a78bfa / #7c3aed; remap `--primary`/`--primary-foreground`/`--ring`/`--data-core` → tím (ProgressBar/Badge/Button/Tabs/BlockToken XP tự đổi tím)
- Glass card + hero gradient band; stats viền trên tím 3 tông; quick links hover; achievements glow
- **Flame streak**: `.profile__flame` (bập bùng), `.profile__streak-chip::after` (halo cam), `.profile__streak-token` (thở) — tôn trọng prefers-reduced-motion

## 3. THAY ĐỔI BACKEND (Grokking DS 8 module)

### `backend/seed-data/grokking-course.json`
- Giờ **8 modules / 32 lessons / 60 quizzes** (merge từ `grokking-course-add.json` — file tạm ĐÃ XÓA, nội dung đã gộp vào)
- Module mới: `mod-bst-avl`(5000) · `mod-heap`(6000, tên "Đống nhị phân (Heap) & Hàng đợi ưu tiên" — TRÁNH trùng tên ALG) · `mod-graph`(7000) · `mod-advanced`(8000)
- Mỗi module: 2 bài dsa + Mini-Quizz + Assignment(codelab, có contentMd + sandboxConfig task đơn)

### `backend/src/DsaVisual.Application/Persistence/Seed/SeedGrokkingData.cs`
- `ModuleTopics`: + (5000..8000)
- `QuizMap`: + 12 mapping (bài 9-16 + mini-quizz 5-8)
- `LabSimKeys`: + bài 9-14, 16 (bài 15 Trie không có sim)
- `AssignmentSimKeys`: + 4 (structure.bst/heap/graph/hashtable)
- Path Description cập nhật 8 module
- **Fix bug SortOrder**: node loop bắt đầu từ `max(SortOrder)+1` (trước gán từ 1 → trùng unique (PathId, SortOrder) khi thêm node vào path đã seed)

### `backend/.../SeedGrokkingData.Helpers.cs`
- `FinalQuizzModuleQuizzes`: + 4 quiz mới → **40 câu** (5×8 module); description "bao quát 8 module"

### `backend/src/DsaVisual.Api/Controllers/ConceptsController.cs` — GỠ KHÓA TẠM
- `private const bool DisableNodeLocks = true;` — tắt cả `Locked` flag (BuildLessonsAsync) + 403 (IsNodeLockedAsync). **Bật lại = đổi false + rebuild + restart**

## 4. VERIFY ĐÃ CHẠY (phiên này)

- FE: vue-tsc sạch · vitest **475 PASS** (gồm 26+18+… test stack/queue, 500-array insertion stress) · `npm run build` PASS · dev server compile OK
- BE: `dotnet build` 0 lỗi · seed chạy 2 lần EXIT 0 (idempotent) · `SeedDemoActivityTests` **9/9 PASS** (gồm SecondRun_DoesNotChangeAnyCount)
- DB sau seed: **Topics=21, Lessons=72, Exercises=105, Questions=408, LearningPaths=7**
- DS khóa giờ 8 module × 4 bài; ALG vẫn 8 module (không đụng)
- API verify: mọi lesson `locked:false` (sau khi bật DisableNodeLocks)

## 5. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **ĐANG BẬT DisableNodeLocks = true** — xem trước xong nhớ **khóa lại** (đổi false → rebuild BE → restart).
2. **CHƯA COMMIT GÌ** cả session — commit theo `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc), PR base `dev`. Khối lượng lớn: FE (2 thuật toán sorting + 2 visualizer + stack-queue sandbox + profile tím + flame + fix font) + BE (DS 8 module + seed fixes + DisableNodeLocks).
3. **Đồng bộ docs** (task dev-docs): SRS/SDD/API_REFERENCE/SCREEN_MAP — tab Stack&Queue, `/stack-queue-sandbox`, DS 8 module, DisableNodeLocks.
4. Dọn user test rác DB demo (15 user Id 81-95) — khi mọi việc xong.
5. `dsaApi.ts` gọi `localhost:5055` (backend nguồn) — fallback local, không cần.
6. Tồn đọng cũ: refresh-loop 401 (client.ts:110-112), PROMPT_VISUALIZE_UPGRADE, xoá `data/lessons.ts` + PathView/PathRedirectView/NodeHubView.
7. Kỹ thuật: file `.ps1` cần BOM UTF-8; **KHÔNG dùng PowerShell Get-Content/Set-Content cho file chứa Unicode** (dùng Write tool hoặc .NET ReadAllText/WriteAllText UTF8); Tailwind scan file mới cần restart FE dev server.
8. Gợi ý đã chốt (chưa làm): khóa học Bảng băm/đồ thị nâng cao cho lộ trình; liên kết "Thử ngay trong Sandbox" từ bài học.

## 6. MẸO NHANH

```powershell
# Backend (dừng dotnet cũ: Get-Process dotnet | Stop-Process -Force)
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\backend" -WindowStyle Hidden

# FE — sửa file .vue thì HMR đủ, thêm FILE MỚI phải restart dev server (Tailwind scan)
cmd /c "npm run dev -- --port 5173 --strictPort"

# Seed lại (idempotent)
dotnet run --project src/DsaVisual.Api -- --seed

# Sandbox: /sorting-sandbox · /searching-sandbox · /graph-playground · /stack-queue-sandbox (1 trang 4 tab)
# Stack & Queue: top bar Stack|Queue → pills biến thể (Stack: Cơ bản/Min-Max · Queue: FIFO/Tròn/Deque)
# Login E2E: qua API 127.0.0.1:5000 rồi addCookie (tránh rate limit partition ::1)
# Khóa lại node khi xong: ConceptsController.cs → DisableNodeLocks = false → rebuild + restart BE
```

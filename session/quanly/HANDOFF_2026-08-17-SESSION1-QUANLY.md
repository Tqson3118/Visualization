# HANDOFF — 17/08/2026 (phiên: Grokking Algorithms + bê 3 sandbox VisualizationDSA3 + gộp tab + navbar)

> File bàn giao phiên này. Phiên mới đọc file này + `HANDOFF_2026-08-15-SESSION2-QUANLY.md` là nắm hết.
> Backend/FE đang chạy: BE http://localhost:5000 · FE http://localhost:5173.

## 1. TÓM TẮT PHIÊN

| # | Việc | Trạng thái |
|---|---|---|
| 1 | **Khóa mới "Grokking Algorithms"** (8 module, 34 nodes, đi từ con số 0 → intermediate) — tự soạn 16 bài lý thuyết tiếng Việt + 8 mini-quizz + 8 assignment + Final-Quizz 20 câu + Kiểm tra cuối 3 bài code | ✅ |
| 2 | Seed Algorithms: `backend/seed-data/grokking-algorithms.json` + `SeedGrokkingAlgorithmsData.cs` + `.Helpers.cs` + đăng ký `SeedRunner` + sửa điều kiện ẩn path legacy (giữ cả 2 path Grokking) | ✅ |
| 3 | **Bê 3 sandbox từ VisualizationDSA3**: Sorting Sandbox (7 sort), Searching Sandbox (4 search), Graph Playground (BFS/DFS/Dijkstra + interactive) — copy theo dependency closure ~185 file | ✅ |
| 4 | Tích hợp sandbox: BaseIcon global, emojiParser +escapeHtmlText, Tailwind color classes (`sandbox-theme.css` scoped, 85 class + color-mix), full-height sandbox, 3 route riêng + dropdown Sandbox | ✅ |
| 5 | **Gộp 3 sandbox → 1 trang 3 tab** (SortingView): bỏ tab DSAPlayer, 3 route cùng render SortingView mở đúng tab | ✅ |
| 6 | **Animation chuyển tab** (Transition mode out-in + KeepAlive, slide + fade 0.28s) | ✅ |
| 7 | Fix **bug banner Searching** (kết quả ở bước 0 không hiện banner) — trích `showResult()` | ✅ |
| 8 | Fix **nav header đè nội dung** (logo to → `--app-header-h: 112px` trong tokens.css, App.vue + HomeView dùng chung) | ✅ |
| 9 | Fix **dropdown Sandbox bị cắt** (`.app-header__nav` bỏ `overflow-x: auto` → `visible`) | ✅ |
| 10 | Test toàn bộ: Sorting 7/7 · Searching 4/4 · Graph 3/3 + tương tác · các fix | ✅ |

## 2. THAY ĐỔI BACKEND

- **`SeedGrokkingAlgorithmsData.cs`** (MỚI): import `grokking-algorithms.json` — 8 topics (Module 1-8) + 32 lessons (16 theory + 8 quiz + 8 codelab) + path "Grokking Algorithms" (SortOrder=2, 34 nodes) + quiz/lab/code exercises + Final-Quizz (20 câu: 2-3 câu/module theo manifest) + Kiểm tra cuối (3 bài code) — idempotent, tự chữa ConfigJson.
- **`SeedGrokkingAlgorithmsData.Helpers.cs`** (MỚI): `LabSimKeys` (9 bài dùng 14 engine sẵn có), `BuildAssignmentConfig` (bọc task đơn → mảng 1 phần tử), `BuildFinalCodingTasks` (3 bài: binarySearch, mergeSort, topK).
- **`SeedGrokkingData.cs`** (SỬA): điều kiện ẩn path legacy → `!EF.Functions.Like(p.Title, "Grokking %")` (giữ cả 2 path Grokking, không dùng `StartsWith` vì EF không translate).
- **`SeedRunner.cs`** (SỬA): thêm `await SeedGrokkingAlgorithmsData.SeedAsync(...)` sau Grokking DS.
- Seed data nguồn: `backend/seed-data/algorithms/` — `manifest.json` + `lessons/*.md` (16 bài) + `quizzes/*.json` (8) + `assignments/*.json` (8) + `build-algorithms.ps1` (build thủ công vì PS 5.1 ConvertTo-Json bị bug `{value,Count}`; file .ps1 PHẢI có BOM UTF-8, cấu trúc quiz options phải là CHUỖI JSON).
- JSON build ra: `backend/seed-data/grokking-algorithms.json` (194KB, 8 modules/32 lessons/8 quizzes).

## 3. THAY ĐỔI FRONTEND — Sandbox (bê từ VisualizationDSA3)

### File copy mới
- `src/features/algorithm-sandbox/` (85) — 7 sort + composables + renderers + engine
- `src/features/interactive-playground/` (17) — Graph interactive (store, ForceDirectedEngine, GraphAlgorithmSimulator, GraphParser)
- `src/features/dsa-modules/` (33) — DSAPlayer + algorithmCatalog (KHÔNG còn được render trong tab nhưng vẫn tồn tại; dsaApi gọi localhost:5055 có fallback local)
- `src/features/vcr-player/` (6), `animation-engine/` (4 — chỉ giữ store/types/AnimationVcrControls), `guided-tour/` (7)
- `src/core/` (4 — chỉ CompilerStepExecutor + CoreAnimationEngine + tests)
- `src/views/sorting/SortingView.vue`, `src/views/searching/SearchingView.vue`, `src/views/graph/GraphView.vue`
- `src/shared/components/Theory*`, `shared/utils/markdown.ts`, `shared/types/theory.types.ts`, `src/components/icons/SvgIcon.vue`

### Tích hợp
- **`main.ts`**: import `styles/sandbox-theme.css` + `app.component('BaseIcon', BaseIcon)` (sandbox dùng `<BaseIcon>` global không import).
- **`utils/emojiParser.ts`**: thêm `escapeHtmlText` (Graph Playground dùng).
- **`styles/sandbox-theme.css`** (MỚI): theme nguồn (--color-bg-*, --color-accent-*, --vis-color-*, glass/vcr/canvas tokens) + 85 Tailwind color class scoped `.sandbox-theme` (bg-bg-surface, text-accent-green, border-border-default... kèm opacity color-mix) — vì đích Tailwind 4 không có config nguồn.
- **`App.vue`**: main nhận `app-shell__main--sandbox` + `sandbox-theme` khi route sandbox; height `flex: 0 0 calc(100vh - var(--app-header-h))` (full viewport), ẩn footer, `SANDBOX_ROUTES` const.
- **`router/index.ts`**: 3 route `/sorting-sandbox`, `/searching-sandbox`, `/graph-playground` ĐỀU render `SortingView` (mở tab theo route name).
- **`SortingView.vue`**: 3 tab `[Sorting Sandbox][Searching Sandbox][Graph Playground]`; `initialTabFromRoute()`; `Transition name="sandbox-tab" mode="out-in"` > `KeepAlive` > component (`:key="activeTab"`); VcrDockBar/SortingDrawerTrace chỉ khi tab sorting; keyboard shortcut chỉ tab sorting; bỏ DSAPlayer khỏi tabs.
- **`components/layout/AppHeader.vue`**: thêm dropdown "Sandbox ▾" (3 mục) + mobile nav 3 link; fix `overflow-x: auto` → `visible` (dropdown bị cắt).

## 4. THAY ĐỔI FRONTEND — Header/navbar (fix đè nội dung)

- **`styles/tokens.css`**: thêm `--app-header-h: 112px` (logo 72px + padding 30+10).
- **`App.vue`**: `padding-top: var(--app-header-h)` (thay 53px cứng).
- **`HomeView.vue`**: `margin-top: calc(-1 * var(--app-header-h))` (hero tràn đỉnh).
- Logo brand: `frontend/src/assets/brand-logo.png` (từ Gemini, đã cắt nền đen + logo sao, resize 480px) — `AppHeader.vue` dùng `<img>` height 72px.

## 5. VERIFY ĐÃ CHẠY (phiên này)

- BE: `dotnet test` (unit, bỏ Integration cần Docker) = **166 PASS** · seed idempotent (chạy lại không nhân đôi) · API `/api/v1/concepts/courses` trả 2 path active.
- FE: vue-tsc sạch · vitest **373 PASS** (164 cũ + 209 sandbox: sorting.spec 63, interactive-playground 39, dsa-modules, core tests...) · `npm run build` PASS.
- E2E Playwright (dữ liệu THẬT, user student@demo.local):
  - **Sorting 7/7** đúng (Bubble/Quick/Merge/Heap/Radix/Counting/Bucket: `[85,70,56,42,28,14]` → `[14,28,42,56,70,85]`)
  - **Searching 4/4** đúng (Linear/Binary/Two Pointers/Sliding Window) + fix banner
  - **Graph 3/3** đúng (BFS/DFS/Dijkstra + thêm node thủ công + Square template)
  - **3 route mở đúng tab** + chuyển tab + animation transition (leave→enter) + 0 JS errors
- Vision-review (Ollama qwen2.5vl:7b) xác nhận màu sort (xanh/đỏ/vàng), Graph node visited, tab bar.

## 6. TÀI KHOẢN

- `student@demo.local / Student@123` · `teacher@demo.local / Teacher@123` · `admin@system.local / Admin@123`.

## 7. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **CHƯA COMMIT GÌ** — commit theo `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc), PR base `dev`. Rất nhiều thứ mới: FE (3 sandbox + gộp tab + sandbox-theme + header fixes) + BE (Seed Algorithms).
2. **Đồng bộ docs** (task dev-docs): SRS/SDD/API_REFERENCE/SCREEN_MAP — khóa Grokking Algorithms, 3 sandbox bê từ DSA3, gộp tab, dropdown Sandbox, `--app-header-h`.
3. **Dọn user test rác** trong DB demo (15 user Id 81-95: locktest_*, asmtest_*, fullwalk_*) — đã chốt xóa MỘT LẦN khi mọi việc xong.
4. **dsaApi.ts** gọi `localhost:5055` (backend nguồn) — hiện fallback local generator nên không cần backend; nếu muốn trải nghiệm 100% nguồn thì chạy backend VisualizationDSA3 (chưa bê — đã quyết định không bê backend nguồn để tránh 2 WebApi).
5. **Grokking Algorithms chưa có rating/đánh giá mẫu** (Grokking DS có) — optional thêm sau.
6. Tồn đọng cũ (chưa đụng): refresh-loop 401 (client.ts:110-112), PROMPT_VISUALIZE_UPGRADE bước Viz, xoá `frontend/src/data/lessons.ts` + PathView/PathRedirectView/NodeHubView.
7. Lưu ý kỹ thuật: file `.ps1` build cần **BOM UTF-8** (PS 5.1 đọc không BOM thành ANSI → lỗi tiếng Việt); quiz options trong JSON phải là **chuỗi** `"[\"...\"]"` không phải mảng (seed chỉ đọc String).

## 8. MẸO NHANH

```powershell
# Backend (dừng dotnet cũ: Get-Process dotnet | Stop-Process -Force)
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\backend" -WindowStyle Hidden

# FE (từ frontend/) — Tailwind scan file mới cần RESTART dev server mới thấy class
cmd /c "npm run dev -- --port 5173 --strictPort"

# Seed lại (idempotent)
dotnet run --project src/DsaVisual.Api -- --seed

# Verify E2E nhanh: login qua API 127.0.0.1:5000 (tránh rate limit partition ::1) rồi addCookie
# Sandbox: /sorting-sandbox (hoặc /searching-sandbox, /graph-playground) — 1 trang 3 tab

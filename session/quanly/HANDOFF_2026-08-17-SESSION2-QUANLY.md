# HANDOFF — 17/08/2026 (phiên 2: gộp 3 sandbox thành 1 trang 3 tab + animation trượt + indicator đổi màu)

> File bàn giao PHIÊN NÀY. Phiên mới đọc file này + `HANDOFF_2026-08-17-SESSION1-QUANLY.md` + `HANDOFF_2026-08-15-SESSION2-QUANLY.md` là nắm hết.
> Backend/FE đang chạy: BE http://localhost:5000 · FE http://localhost:5173.

## 1. TÓM TẮT PHIÊN (việc chính sau SESSION1)

| # | Việc | Trạng thái |
|---|---|---|
| 1 | **Gộp 3 sandbox → 1 trang 3 tab** (SortingView là trung tâm): tab [Sorting Sandbox][Searching Sandbox][Graph Playground]; **xóa tab "Searching & Linear DSA"** (DSAPlayer) theo quyết định user | ✅ |
| 2 | **3 route dùng chung 1 component**: `/sorting-sandbox`, `/searching-sandbox`, `/graph-playground` ĐỀU render SortingView, mở đúng tab theo route name (`initialTabFromRoute`) — dropdown Sandbox bấm cái nào vào thẳng tab đó | ✅ |
| 3 | **Animation chuyển tab nội dung**: `<Transition name="sandbox-tab" mode="out-in">` bọc `<KeepAlive>` > `<component :key="activeTab">` — tab cũ trượt lên mờ, tab mới fade+slide (0.28s spring). Lưu ý thứ tự bắt buộc Transition ngoài KeepAlive | ✅ |
| 4 | **Sliding tab indicator (khối nền trượt + đổi màu)**: element `.tab-indicator` absolute trong tab bar, đo `offsetLeft/offsetWidth` từng nút → `transform: translateX` + width động; màu gradient theo tab | ✅ |
| 5 | **Màu indicator**: Sorting **tím** `#c4b5fd→#8b5cf6→#7c3aed` · Searching **cam** `#fdba74→#fb923c→#ea580c` · Graph **xanh lá** `#6ee7b7→#34d399→#059669` (glow nhẹ 0.35) | ✅ |
| 6 | Test: chuyển tab giữa 3 route + bấm tab nội bộ + indicator đo được translateX đúng vị trí từng tab + 0 JS errors | ✅ |

## 2. THAY ĐỔI FRONTEND (chi tiết)

### `src/views/sorting/SortingView.vue` (trang trung tâm — SỬA NHIỀU)
- **Tabs**: `[sorting | searching | graph]` — bỏ DSAPlayer; import `SearchingView` + `GraphView`.
- **Route → tab**: `initialTabFromRoute()` đọc `useRoute().name` → sorting/searching/graph.
- **Sliding indicator**:
  - Template: tab bar `relative` + `ref="tabBarRef"`; `<div class="tab-indicator" :class="'tab-indicator--'+activeTab" :style="indicatorStyle">` đặt TRƯỚC các nút; mỗi nút `:ref="(el)=>setTabEl(tab.id, el)"`, active chỉ `text-white` (KHÔNG `bg-accent` nữa).
  - Script: `tabEls = Map<id, HTMLElement>`, `updateIndicator()` đo `offsetLeft/offsetWidth`, `indicatorStyle` = `translateX + width`; watch activeTab → `nextTick(updateIndicator)`; `onMounted`/resize cũng đo.
  - CSS scoped: `.tab-indicator` absolute (top/bottom 2px, left 0, z-0), transition `transform/width/background-color` 0.3s spring; `:deep(.sub-tab-pill)` position relative z-1 + `background: transparent`; 3 class `--sorting/--searching/--graph` gradient 3 tông + glow nhẹ.
  - **CẢNH BÁO khi edit file này**: trước đó bị lồng onMounted/onUnmounted + mất `function handleKeydown` do edit chồng — kiểm tra kỹ khi sửa tiếp.
- **Animation nội dung**: `Transition mode="out-in"` > `KeepAlive` > `component :key="activeTab"`; VcrDockBar/SortingDrawerTrace chỉ hiện khi tab sorting; keyboard (Space/←/→/R) chỉ hoạt động tab sorting.

### `src/router/index.ts`
- `SortingSandboxView` (lazy `@/views/sorting/SortingView.vue`) dùng cho CẢ 3 route; xóa 2 import Searching/Graph view riêng.

## 3. VERIFY ĐÃ CHẠY (phiên này)

- vue-tsc sạch · `npm run build` PASS · vitest **373 PASS**.
- E2E Playwright:
  - 3 route → đúng 3 tab active + UI tương ứng (sort bars / searching / graph canvas) + 0 JS errors
  - Bấm tab nội bộ chuyển được (có transition classes leave→enter)
  - Indicator: INIT sorting `translateX(0)` gradient tím → Graph `translateX(305px)` gradient xanh lá → Searching `translateX(144px)` gradient cam — width khớp từng nút
  - Vision-review: tab Sorting nền tím rõ ràng (đã xác nhận indicator hiển thị)

## 4. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **CHƯA COMMIT GÌ** (cả phiên 1 lẫn 2) — commit theo `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc), PR base `dev`. Khối lượng lớn: FE (3 sandbox + gộp tab + sandbox-theme + header fixes + indicator) + BE (Seed Algorithms).
2. **Đồng bộ docs** (task dev-docs): SRS/SDD/API_REFERENCE/SCREEN_MAP — khóa Grokking Algorithms, 3 sandbox, gộp tab, dropdown Sandbox, `--app-header-h`, indicator.
3. **Dọn user test rác** DB demo (15 user Id 81-95) — xóa MỘT LẦN khi mọi việc xong.
4. `dsaApi.ts` gọi `localhost:5055` (backend nguồn) — hiện fallback local, không cần backend.
5. Grokking Algorithms chưa có rating/đánh giá mẫu (optional).
6. Tồn đọng cũ: refresh-loop 401 (client.ts:110-112), PROMPT_VISUALIZE_UPGRADE, xoá `data/lessons.ts` + PathView/PathRedirectView/NodeHubView.
7. Kỹ thuật: file `.ps1` cần BOM UTF-8; quiz options trong JSON phải là chuỗi; Tailwind scan file mới cần restart FE dev server.

## 5. MẸO NHANH

```powershell
# Backend (dừng dotnet cũ: Get-Process dotnet | Stop-Process -Force)
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\backend" -WindowStyle Hidden

# FE — sửa file .vue thì HMR đủ, thêm FILE MỚI phải restart dev server (Tailwind scan)
cmd /c "npm run dev -- --port 5173 --strictPort"

# Seed lại (idempotent)
dotnet run --project src/DsaVisual.Api -- --seed

# Sandbox: /sorting-sandbox (hoặc /searching-sandbox, /graph-playground) — 1 trang 3 tab
# Login E2E: qua API 127.0.0.1:5000 rồi addCookie (tránh rate limit partition ::1)
```

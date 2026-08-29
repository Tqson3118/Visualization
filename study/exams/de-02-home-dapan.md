# Đáp Án — Đề 02: HomeView & Public Demo

---

## PHẦN I — TRẮC NGHIỆM

**Câu 1: C**
`v-reveal` dùng `IntersectionObserver` với `threshold: 0.12` — nghĩa là hiệu ứng kích hoạt khi phần tử đã xuất hiện ít nhất 12% trong viewport. Đây là API native của trình duyệt, không cần thư viện scroll.

**Câu 2: B**
Directive `v-reveal` có guard:
- `if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return` — tôn trọng accessibility setting của OS
- `if (!('IntersectionObserver' in window)) return` — jsdom (môi trường Jest/Vitest) không implement IntersectionObserver
Hai điều kiện này khiến directive early-return mà không gắn observer. Screen width và auth status không ảnh hưởng.

**Câu 3: B**
`HomeView.vue` (1349 dòng) dùng số liệu **tĩnh** từ CATALOG — không gọi API runtime. SDD §19.6A định nghĩa 44 mô phỏng. Số này không thay đổi theo user hay database state.

**Câu 4: B**
Logic filter: `const demoList = CATALOG.filter(c => c.demoAllowed)`. CATALOG là array của các object có field `demoAllowed: boolean`. Ba entry có `demoAllowed: true` là `sort.bubble`, `search.binary`, `graph.bfs`. Icon mỗi demo lấy từ `DEMO_ICONS[c.key]` — một TypeScript Record mapping key → icon component.

**Câu 5: B**
Guest click → `router.push({ name: 'simulator', params: { key: 'sort.bubble' } })` → Router guard kiểm tra: route `/simulator/:key` KHÔNG có `requiresAuth: true` với điều kiện `demoAllowed`. Guard gọi `getCatalogMeta('sort.bubble')?.demoAllowed === true` → true → cho phép vào. Không cần đăng nhập.

---

## PHẦN II — TỰ LUẬN

### Câu 6 — HomeView render cho Guest

**1. Thao tác UI:**
- `onMounted()` chạy sau khi component render lần đầu
- `v-reveal` directive: `bind` (Vue 2) hoặc `mounted` hook (Vue 3) tạo `new IntersectionObserver(callback, { threshold: 0.12 })`, gọi `observer.observe(el)` trên từng element có directive
- Guest thấy: Hero section (CTA "Bắt đầu miễn phí"), Features section, Demo section (3 card), Stats section
- Guest KHÔNG thấy: Dashboard widget, Learning progress, Recent activity (wrapped trong `v-if="isAuthenticated"`)

**2. Frontend Data Layer:**
- **Không** gọi API nào. `stats` là object literal hardcoded:
  ```js
  const stats = { simCount: 44, userCount: 12000, lessonCount: 120 }
  ```
- `demoList = CATALOG.filter(c => c.demoAllowed)` — CATALOG được `import` tĩnh từ `@/data/catalog.ts` tại compile time
- Không có `fetch`, không có Axios call, không có Pinia action được dispatch

**3. Backend Layer:**
- **Không** gọi backend khi render HomeView cho guest. Lý do:
  - Tất cả data (stats, catalog) là static/compile-time
  - Auth check: `authStore.status` đang là `'unauthenticated'` → không trigger `authApi.refresh()`
  - `refresh()` chỉ được gọi trong app init nếu cookie tồn tại, với guest thì không có cookie

**4. UI Render:**
- Icon demo: `DEMO_ICONS` là `Record<string, Component>` — ví dụ `{ 'sort.bubble': BubbleSortIcon, 'search.binary': BinarySearchIcon, 'graph.bfs': BfsIcon }`. Template render `<component :is="DEMO_ICONS[demo.key]" />`
- CTA button: `v-if="!isAuthenticated"` → hiện "Bắt đầu miễn phí" (link đến /register); với auth user → "Vào Dashboard"
- Stats section: render số `44` từ biến `stats.simCount` trong section sau Features

---

### Câu 7 — Guest click "Binary Search" demo → SimulatorView chạy

**1. UI (HomeView → Router):**
- Click handler: `@click="router.push({ name: 'simulator', params: { key: demo.key } })"` (hoặc `<router-link :to="{ name: 'simulator', params: { key: 'search.binary' } }>`)
- Route được kích hoạt: `/simulator/search.binary` → route name: `simulator`

**2. Router Guard:**
- Guard `beforeEach` kiểm tra route meta của `simulator`:
  - Route `simulator` có `meta: { requiresAuth: false }` cho trường hợp demo, hoặc guard tự kiểm tra: `if (!isAuthenticated && !getCatalogMeta(to.params.key)?.demoAllowed) → redirect login`
  - `getCatalogMeta('search.binary')?.demoAllowed === true` → **cho qua**, không redirect
- Guest được phép vào `/simulator/search.binary`

**3. SimulatorView khởi tạo:**
- `const key = route.params.key` → `'search.binary'`
- `const { currentSim, steps, currentIndex, status, loading } = useSimulation(key)`
- Trong composable: `onMounted → fetchSimulation(key)`
- API call: `GET /api/simulations/search.binary` (hoặc `/api/catalog/search.binary/steps`)
- Response: `{ meta: { title, description }, pseudocode: string[], steps: [{ index, highlightLine, canvasData, explanation }] }`
- `loading = true` → fetch → `steps` được populate → `loading = false`, `status = 'idle'`

**4. UI Render (SimulatorView):**
- **PseudocodePanel**: hiển thị toàn bộ `pseudocode` array, không highlight dòng nào (`currentStep = null`)
- **CanvasArea**: hiển thị trạng thái khởi tạo — array ban đầu chưa sắp xếp, các ô chia đều
- **ExplainPanel**: hiển thị mô tả tổng quan của thuật toán hoặc hướng dẫn "Nhấn Play để bắt đầu"
- **ControlBar**: `status = 'idle'` → nút Play hiện, nút Pause ẩn; `stepBack` disabled (currentIndex = 0)
- **Nút Star**: `v-if="isAuthenticated"` → **KHÔNG hiện** với guest. Logic: `favoritesApi` yêu cầu auth, guest không thể favorite

---

**Bảng tóm tắt điểm tự luận:**

| Chặng | Câu 6 | Câu 7 |
|-------|-------|-------|
| UI | onMounted, v-reveal, v-if guest sections | @click → router.push, route kích hoạt |
| FE Data Layer | Không API, CATALOG static import | Guard check demoAllowed, useSimulation fetch |
| Backend | Không gọi, giải thích lý do | GET /api/simulations/:key → steps array |
| UI Render | DEMO_ICONS, CTA guest vs auth, stats | 3 panel init state, Star ẩn với guest |

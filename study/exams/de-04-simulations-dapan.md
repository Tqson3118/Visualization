# Đáp án Đề 04 — Simulations, Benchmark & CheatSheet

---

## PHẦN I — TRẮC NGHIỆM

### Câu 1 — Đáp án: B
**Giải thích:** `SimulationsView.vue` (908 dòng, route `/simulations`) hiển thị Màn 33 "Khám phá" với đúng **2 tab**:
- Tab **Danh mục (catalog)** — hiển thị các card thuật toán gom nhóm.
- Tab **CheatSheet** — bảng tra cứu nhanh Big-O, space complexity.

Benchmark và Sandbox là route riêng biệt, không phải tab bên trong SimulationsView.

---

### Câu 2 — Đáp án: B
**Giải thích:** Tab Danh mục dùng **prefix của key** thuật toán để phân nhóm:
- `sort.bubble`, `sort.merge`, `sort.quick` → nhóm **sort.**
- `search.binary`, `search.linear` → nhóm **search.**
- `stack.*`, `queue.*`, `list.*`, `tree.*`, `heap.*`, `hash.*`, `graph.*`, `structure.*`

Đây là thiết kế namespace-based giúp mở rộng dễ dàng mà không cần cấu hình thêm.

---

### Câu 3 — Đáp án: B
**Giải thích:** Màu chip Big-O phản ánh **tốc độ thực thi**:

| Độ phức tạp | Màu chip | Ý nghĩa |
|---|---|---|
| `O(n log n)` | **success** (xanh lá) | Hiệu quả, chấp nhận được |
| `O(n²)` | **warning** (vàng) | Cảnh báo chậm với dữ liệu lớn |
| `O(n³+)` | **danger** (đỏ) | Rất chậm, tránh dùng production |

---

### Câu 4 — Đáp án: B
**Giải thích:** `CheatSheetTable` được import bằng **lazy-load** thông qua Vue `defineAsyncComponent`:

```js
const CheatSheetTable = defineAsyncComponent(
  () => import('@/components/lesson/CheatSheetTable.vue')
)
```

Kỹ thuật này giúp **code-splitting**: bundle chính không chứa CheatSheetTable, chỉ tải khi người dùng click vào tab CheatSheet lần đầu → tăng tốc initial load.

---

### Câu 5 — Đáp án: B
**Giải thích:** Route BenchmarkView được định nghĩa là:

```
path: '/benchmark/:k1/:k2'
meta: { requiresAuth: true }
```

- **`:k1`** và **`:k2`** là hai key thuật toán cần so sánh (ví dụ `sort.bubble` và `sort.merge`).
- `requiresAuth: true` → router guard chặn guest, redirect về trang login.

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG

### Câu 6 — Trace: Nhấn tab "CheatSheet" lần đầu

**1. UI — Hành động & Component phản hồi**
- Người dùng click vào tab **"CheatSheet"** trong `SimulationsView.vue`.
- Vue cập nhật reactive state `activeTab = 'cheatsheet'`.
- Template render conditional: `<CheatSheetTable v-if="activeTab === 'cheatsheet'" />`.

**2. FE Data — Quá trình tải lazy component**
- Vì `CheatSheetTable` được khai báo bằng `defineAsyncComponent()`, Vue bắt đầu **dynamic import**:
  ```js
  import('@/components/lesson/CheatSheetTable.vue')
  ```
- Vite tải chunk JS tương ứng từ server tĩnh.
- Trong thời gian chờ, hiển thị **loading slot** (spinner hoặc skeleton) nếu được cấu hình.
- Sau khi chunk tải xong, component được resolve và register vào Vue runtime.

**3. Backend — API gọi dữ liệu**
- `CheatSheetTable.vue` trong `onMounted` gọi API:
  `GET /api/algorithms/cheatsheet` hoặc đọc từ JSON tĩnh đã bundle.
- Nếu dữ liệu là static JSON (imported trực tiếp), không có HTTP call → dữ liệu sẵn sàng ngay.
- Nếu dynamic: `AlgorithmsController` trả về danh sách `{ key, name, timeComplexity, spaceComplexity, notes }`.

**4. UI Render — Kết quả cuối**
- `CheatSheetTable.vue` nhận dữ liệu và render bảng HTML với các cột: **Thuật toán / Best / Average / Worst / Space**.
- Màu sắc Big-O chip được áp dụng tương tự tab Danh mục (success/warning/danger).
- Component ở trong trạng thái **mounted và cached** — lần sau click lại tab không tải lại.

---

### Câu 7 — Trace: Truy cập `/benchmark/sort.bubble/sort.merge`

**1. UI — Router guard & Component mount**
- Browser navigate đến `/benchmark/sort.bubble/sort.merge`.
- Vue Router khớp route `path: '/benchmark/:k1/:k2'`.
- **Navigation guard** (`beforeEach`) kiểm tra `meta.requiresAuth: true`:
  - Nếu user chưa đăng nhập → redirect về `/login?redirect=/benchmark/sort.bubble/sort.merge`.
  - Nếu đã đăng nhập → cho phép tiếp tục.
- `BenchmarkView.vue` được mount (lazy-loaded qua `import()`).

**2. FE Data — Parse key và lấy dữ liệu**
- `BenchmarkView.vue` trong `setup()` / `onMounted`:
  ```js
  const route = useRoute()
  const k1 = route.params.k1  // 'sort.bubble'
  const k2 = route.params.k2  // 'sort.merge'
  ```
- Dữ liệu cấu hình hai thuật toán được lấy từ **algorithm registry** (store hoặc JSON tĩnh) theo key.
- Khởi tạo dataset test (mảng random N phần tử, configurable) cho cả hai thuật toán.

**3. Backend — Thực thi Benchmark**
- Benchmark chạy **client-side** bằng Web Worker hoặc trực tiếp trong main thread:
  - Worker nhận `{ k1, k2, dataSize, dataset }`.
  - Chạy sorting function của `sort.bubble` và `sort.merge` trên cùng dataset.
  - Đo `performance.now()` trước/sau mỗi lần chạy.
  - Trả về `{ k1: { timeMs, comparisons, swaps }, k2: { timeMs, comparisons, swaps } }`.
- Nếu backend: `POST /api/benchmark` với body `{ algo1: 'sort.bubble', algo2: 'sort.merge', n: 1000 }` → `BenchmarkController` chạy và trả về kết quả.

**4. UI Render — Hiển thị kết quả so sánh**
- Kết quả được bind vào reactive state, trigger re-render:
  - **Biểu đồ Bar Chart** (Chart.js / ECharts): cạnh nhau so sánh thời gian thực thi.
  - **Bảng số liệu**: Time (ms), Comparisons, Swaps cho từng thuật toán.
  - **Highlight winner**: thuật toán nhanh hơn được đánh dấu (badge "Faster" hoặc màu xanh lá).
- Người dùng có thể thay đổi `dataSize` → trigger lại benchmark → re-render chart.

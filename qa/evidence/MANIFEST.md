# Evidence Manifest — SIM-*

> Snapshot accessibility tree + text mô tả từ Playwright.
> Vì model không nhận hình ảnh, evidence được capture dạng YAML snapshot + text description.

---

## SIM-001: `sort.bubble` — step 0 (initial render)

- **File evidence**: `qa/evidence/SIM-001-bubble-step0.png` (screenshot dừ page, model không xem được)
- **Snapshot text**:
  ```yaml
  - heading "Sắp xếp nổi bọt (Bubble Sort)" [level=1]
  - generic "Tệ nhất: O(n²) | Trung bình: O(n²) | Tốt nhất: O(n) | Bộ nhớ: O(1)"
  - variables: i=0, j=0, swapped=false, n=6
  - pseudocode: 10 dòng (procedure ... end procedure)
  - legend: 5 màu (Chưa xét, So sánh, Pivot, Hoán đổi, Đã chốt)
  ```
- **Status**: Render OK, khớp với generator code (`generators/sort/bubble.ts`)

---

## SIM-002: `sort.selection` — truy cập không qua demo

- **File evidence**: không có file (chơi text snapshot)
- **Repro**:
  1. Từ `/simulations`, navigate URL → `/simulator/sort.selection`
  2. Catalog `demoAllowed: false` (line 58)
  3. **Quan sát**: page render đầy đủ không block
  4. Header: "Tim: 13/30"
- **Status**: P0-001 xác nhận

---

## SIM-003: Click "Bước tới" trên `sort.selection`

- **Repro**:
  1. Từ `sort.selection`, click button "Bước tới" (ref `f21e142`)
  2. URL đổi → `search.linear`
  3. Click tiếp 1 lần → `search.binary`
  4. Header: "Tim: 10/10" (mất 3 tim)
- **Status**: P0-002 + P0-003 xác nhận

---

## SIM-004: `search.binary` step 0 (initial render)

- **Snapshot text**:
  ```yaml
  - heading "Tìm kiếm nhị phân (Binary Search)" [level=1]
  - generic "Tệ nhất: O(log n) | Trung bình: O(log n) | Tốt nhất: O(1) | Bộ nhớ: O(1)"
  - variables: low=0, high=..., target=...
  - pseudocode: 9 dòng (procedure ... end procedure)
  - line 1: "procedure binarySearch(a[0..n-1], target) // a đã sắp xếp"
  ```
- **Status**: Render OK, complexity đúng CLRS
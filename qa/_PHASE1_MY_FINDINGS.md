# SIMULATION_FINDINGS — Báo cáo findings từ QA review

> **Phiên bản**: Phase 1 (đọc source + browser test, ngày 2026-09-01)
> **Phạm vi**: 44 mô phỏng (đã đọc 12/19 generator files + test thật 1 simulator + 1 partial flow)
> **Phương pháp**: Source review + Playwright browser test

---

## 🚨 PHẦN 1: FINDINGS NGHIÊM TRỌNG (P0)

### **P0-001**: Auth guard không block — user có thể xem simulator KHÔNG thuộc demo với 0 tim

- **Mức độ**: 🔴 **P0 (CRITICAL — bảo mật / kinh tế)**
- **Trạng thái**: Cần xác nhận lại bằng cách logout thật + dùng trình duyệt ẩn danh
- **Phát hiện từ**: Browser test ngày 2026-09-01

#### Repro
1. Mở `http://localhost:5173/simulations` — hiển thị "Tim: 13/30", user "Sinh vien mau"
2. Navigate trực tiếp tới `http://localhost:5173/simulator/sort.selection` (key **không thuộc** demo — `demoAllowed: false` trong `catalog.ts:58`)
3. **Quan sát**: Page render **đầy đủ** (header, pseudocode, variables, controls, canvas) — không có banner "Bạn cần đăng nhập" hay redirect về login.

#### Bằng chứng
- **Code**: `frontend/src/router/index.ts:174-177` — route `/simulator/:key` **không có** `meta: { requiresAuth: true }`
- **Code**: `frontend/src/views/SimulatorView.vue:42-44` — guard chỉ check `!auth.isAuthenticated && !isDemoKey`
- **Browser observation**: User trên header hiển thị "Sinh vien mau" + 13/30 tim → app **không bắt buộc login** để dùng.

#### Tác động
- Sinh viên/guest có thể xem **tất cả 44 simulator** không tốn tim → **mất doanh thu**.
- Giảm giá trị của cơ chế "trừ tim".

#### Đề xuất fix
```ts
// router/index.ts
{ path: "/simulator/:key", component: SimulatorView, meta: { requiresAuth: true } }
```

---

### **P0-002**: Mỗi lần mở simulator đều bị trừ 1 tim — không có cooldown

- **Mức độ**: 🔴 **P0 (CRITICAL — UX / doanh thu)**

#### Repro
1. Bắt đầu: **Tim 13/30** trên header
2. Navigate `http://localhost:5173/simulator/sort.selection`
3. Click button "Bước tới" (Next) 1 lần
4. **Quan sát**: URL đổi sang `http://localhost:5173/simulator/search.linear`
5. Header: **Tim 10/10** (!) — mất 3 tim sau 3 navigation

#### Tác động
- Trải nghiệm tệ: không cảnh báo, tim biến mất lặng lẽ.
- Dễ spam simulator bằng cách thay URL hoặc click Next loop.

#### Đề xuất fix
1. Modal xác nhận trước khi trừ tim
2. Cooldown 5-10 giây
3. Trừ tim 1 lần/session/simulator key

---

### **P0-003**: Button "Bước tới" (Next) có behavior sai — navigate sang simulator khác

- **Mức độ**: 🔴 **P0 (CRITICAL — correctness)**
- **Trạng thái**: Cần xác nhận (có thể là auto-play timer)

#### Repro
1. Navigate `http://localhost:5173/simulator/sort.selection`
2. Click button **"Bước tới"** 1 lần
3. **Quan sát**: URL đổi sang `http://localhost:5173/simulator/search.linear`
4. Không phải step algorithm tăng từ 0 → 1
5. Tiếp tục click 1 lần nữa → URL đổi sang `search.binary`

#### Bằng chúng
- Snapshot chỉ có 1 button "Bước tới" (ref `f21e142` / `f24e141`), nằm trong ControlBar (sau "Đặt lại | Bước lùi | Chạy")
- **Giả thuyết cần verify**: có **auto-play timer** chạy ngầm navigate sang simulator kế tiếp, hoặc click bubble lên link "next" trong DOM

#### Tác động
- **Dạy sai thuật toán** — sinh viên không thể step qua thuật toán
- Ngăn cản việc học thuật toán từng bước

#### Đề xuất fix
1. Tắt auto-play timer trong `useSimulation` composable
2. Tách biệt route navigation và algorithm step
3. Nếu muốn "next simulator" → button riêng ở footer

---

## ⚠️ PHẦN 2: FINDINGS CHẤT LƯỢNG (P1/P2)

### **P1-001**: `structure.binarytree` complexity `best: O(log n)` không chính xác

- **Mức độ**: 🟨 **P1 (Quality)**

#### Vấn đề
Binary Tree tổng quát (không cân bằng) có thể là cân bằng (search O(log n)) hoặc skewed (search O(n)). `best: O(log n)` chỉ đúng với Balanced Binary Tree.

#### Đề xuất fix
```ts
// Option A: Đổi về O(n)
complexity: { best: "O(n)", average: "O(n)", worst: "O(n)", space: "O(n)" }
// Option B: Tách "Balanced Binary Tree" thành key riêng
```

---

### **P1-002**: `structure.avl` intro chỉ có 4 step — KHÔNG minh họa 4 rotation

#### Bằng chứng
- `frontend/src/engines/generators/structure/structures.ts` (727 dòng)
- AVL intro chỉ push 4 step: init → insert → check BF → "thực hiện xoay" → kết thúc
- Không có step riêng cho LL / RR / LR / RL

#### Tác động
- Sinh viên chỉ thấy text "Cây thực hiện xoay" mà không hiểu rotation

#### Đề xuất fix
Tạo 4 scenario mẫu cho 4 rotation, mỗi scenario push step riêng cho before-rotation, after-rotation

---

### **P1-003**: `structure.bst` intro — animation xóa nút 2 con chỉ highlight, KHÔNG xóa thật

#### Bằng chứng
- `structures.ts`: animation highlight nút cần xóa + successor, không mutate cây
- Cây vẫn còn đủ 7 nút → mâu thuẫn với text

#### Tác động
- Sinh viên hiểu nhầm "nút 50 biến mất, nút 60 chiếm chỗ"

---

### **P2-001**: `sort.heap` lỗi chính tả "end procedures"

- **File**: `frontend/src/engines/generators/sort/heap.ts:12` (dòng cuối PSEUDOCODE)
- **Fix**: Sửa thành `"end procedure"`

---

## 📝 PHẦN 3: NGHI VẤN CẦN CHỦ APP PHÁN QUYẾT

| ID | Nghi vấn |
|----|-------------|
| Q1 | Auth mock có tính `isAuthenticated = true` không? Nếu có → auth guard vô dụng |
| Q2 | Cơ chế "trừ tim" là client-side hay server-side? Nếu client → dễ bypass |
| Q3 | `structure.binarytree` `best: O(log n)` — cố ý hay sai? |
| Q4 | 8 structure.* key có được dùng trong lộ trình học không, hay chỉ là demo tham khảo? |
| Q5 | Auto-play timer (nếu có) — có thể tắt được không? |
| Q6 | Tại sao chỉ có 1 AVL key (insert) mà không có delete, search? |

---

## 📊 PHẦN 4: THỐNG KÊ TỔNG (Phase 1)

| Metric | Giá trị |
|--------|---------|
| Generator files đã đọc | 12 / 19 |
| Simulator browser-tested | 1 (sort.selection) + 1 demo (sort.bubble render OK) |
| Console errors khi test | 0 |
| Finding P0 | 3 |
| Finding P1 | 3 |
| Finding P2 | 1 |
| Nghi vấn cần confirm | 6 |

---

## 🚦 PHẦN 5: KHUYẼN NGHỈ ƯU TIÊN FIX

| Ưu tiên | Finding | Effort | Impact |
|---------|---------|--------|--------|
| 🔴 1 | P0-001 (auth guard) | 1 giờ | Critical |
| 🔴 2 | P0-003 (Next button bug) | 2-4 giờ | Critical |
| 🔴 3 | P0-002 (tim deduction UX) | 4 giờ | High |
| 🟨 4 | P1-001 (binarytree complexity) | 15 phút | Medium |
| 🟨 5 | P1-002 (AVL intro quality) | 8 giờ | High |
| 🟨 6 | P1-003 (BST delete intro) | 4 giờ | High |
| 🟣 7 | P2-001 (typo) | 5 phút | Low |

**Tổng effort**: ~25 giờ dev (2 sprint)

---

*File sẽ tiếp tục cập nhật sau Phase 2 (test chi tiết 41 simulator còn lại).*
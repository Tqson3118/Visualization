# STANDARD — Tiêu chuẩn audit chất lượng view (10 trục hygiene + Đặc trưng)

> **NGUỒN CHUẨN DUY NHẤT** cho việc chấm điểm 36 view ở Phase 1. Khi audit KHÔNG tự chế thêm quy tắc ngoài bảng này + `frontend/DESIGN.md`. Mọi con số/điểm/ngưỡng lấy từ `session/PROMPT_VIEW_QUALITY_MASTER_V2.md` mục 5 (Phase 0 – BƯỚC C + BƯỚC D) — bản sao trung thực, không thêm trục, không đổi điểm.

| | |
|---|---|
| Loại tài liệu | Tiêu chuẩn audit view (view-quality standard) |
| Phiên bản | 1.0 |
| Ngày tạo | 13/08/2026 |
| Trạng thái | Dự thảo — dùng cho Phase 1 (audit + sửa 36 view) |
| Người soạn | Agent dev-docs (theo PROMPT_VIEW_QUALITY_MASTER_V2) |
| Nguồn chuẩn | `PROMPT_VIEW_QUALITY_MASTER_V2.md` mục 3 (KILL-LIST), mục 5 BƯỚC C (bảng 10 trục), BƯỚC D (scorecard + ngưỡng ĐẠT) |
| Tài liệu liên quan | `frontend/DESIGN.md`, `frontend/DESIGN-IDENTITY.md`, `docs/work/view-quality/scorecard.md`, `docs/SCREEN_MAP.md` |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 13/08/2026 | Agent dev-docs | Tạo bản đầu — chép chuẩn từ PROMPT_VIEW_QUALITY_MASTER_V2 (BƯỚC C + BƯỚC D + KILL-LIST) |

---

## 1. Cách dùng

1. Phase 1 audit TỪNG view theo đúng 10 trục dưới đây — mỗi trục chấm điểm theo tỷ lệ % mức tuân thủ so với "Quy tắc chuẩn" (điểm = điểm tối đa × % tuân thủ, làm tròn 0.5).
2. Mọi nhận xét phải kèm bằng chứng cụ thể (selector/dòng code/screenshot), không nhận xét chung chung.
3. Khi thấy cần quy tắc mới ngoài bảng này → KHÔNG tự chế, ghi vào `docs/work/view-quality/notes.md` + decision log, Phase 2 chốt.
4. Mọi quyết định thay đổi ảnh hưởng ≥2 view → ghi `docs/pm-decision-log-viewquality.md` TRƯỚC khi làm.

---

## 2. Bảng 10 trục hygiene (tổng = 100 điểm)

Mỗi trục có **điểm tối đa** + **sàn** (điểm tối thiểu = 60% điểm tối đa — dùng để chặn 1 trục quá tệ dù tổng vẫn cao).

| # | Trục | Điểm (sàn 60%) | Quy tắc chuẩn |
|---|---|---|---|
| 1 | Spacing/Grid | 8 (sàn 4.8) | Mọi padding/margin/gap là bội số token `@theme` (4/8/12/16/24/32/48/64px) — cấm hardcode `p-[13px]`. Card/list nội bộ dùng spacing nhỏ hơn spacing giữa các nhóm (internal < external). Grid 12 cột responsive. |
| 2 | Breakpoint | 6 (sàn 3.6) | Test tối thiểu 3 mốc: 1366×768 (laptop), 768px (tablet), 390px (mobile) — không lệch/tràn/đè chữ ở mốc nào. |
| 3 | Animation | 14 (sàn 8.4) | Chỉ animate `transform` + `opacity`. Duration: micro-interaction 100-150ms; transition thường (modal, card expand, route) 200-300ms; hero/full-screen tối đa 500ms. Easing `ease-out` khi xuất hiện / `ease-in` khi biến mất, không `linear` (tham số cụ thể hơn: BƯỚC E PROMPT). `prefers-reduced-motion` check 1 chỗ duy nhất ở root. **Mỗi view chọn 1-2 "khoảnh khắc đáng đầu tư" làm kỹ (page-load sequence / hover có chủ đích / reveal đúng dữ liệu), phần còn lại tối giản/nhất quán — không phải mọi phần tử đều animate như nhau.** CẤM: cùng 1 cặp fade+translateY lặp y hệt trên >80% card trong 1 view ("animation cơ giới"). Mỗi animation phải trả lời được phục vụ chức năng gì (xác nhận hành động / dẫn hướng chú ý / thể hiện quan hệ không gian) — không giải thích được thì loại bỏ. |
| 4 | Nhất quán thị giác | 14 (sàn 8.4) | (a) Màu: CHỈ token `@theme`/DESIGN.md §2, accent teal KHÔNG trang trí chỉ CTA/active/link. (b) Component: đúng biến thể shadcn-vue DESIGN.md §4. (c) Icon: 1 bộ duy nhất (đã chốt), cùng size/stroke-width, cấm emoji làm icon chức năng. (d) 3 trạng thái bắt buộc mọi list/table: skeleton loading, empty state (dùng component chung — BƯỚC E PROMPT), error state (thông báo + nút retry). (e) Banner/hero đúng hướng đã chốt DESIGN.md §1, không tự chọn màu mới mỗi view. |
| 5 | Interactive sizing & spacing (button/input) | 16 (sàn 9.6) | **Ưu tiên sửa trước — user phàn nàn rõ nhất.** (a) MỌI button/clickable qua `Button.vue`/`buttonVariants()` — grep `<button` raw = 0 (trừ canvas/editor/table-cell đặc biệt → ghi decision log); (b) padding chuẩn buttonVariants không override: md `h-10 px-4 py-2` / sm `h-9 px-3` / lg `h-11 px-8` / icon `h-10 w-10`, cấm `px-0/px-1/px-2/p-0/py-0` trên nút chữ; (c) icon+text gap ≥8px; (d) chiều cao tối thiểu 40px desktop/44px mobile cho nút chính, target ≥24×24 (WCAG 2.5.8); (e) nút liền kề cách nhau ≥8px; (f) badge/pill ≥6px padding ngang + height ≥24px; (g) audit: đo computed padding TỪNG button bằng DevTools/Playwright `getComputedStyle` — chữ KHÔNG được chạm viền/chạm chữ khác. |
| 6 | Typography | 10 (sàn 6.0) | Theo DESIGN.md §3: 1 type-scale cố định, tracking âm cho heading giảm dần theo size, 3 mức weight — grep cấm `font-bold`/`700` ở heading, `tracking-[...]` dương rời, font-size tự đặt ngoài scale. |
| 7 | Depth & Elevation | 8 (sàn 4.8) | Theo DESIGN.md §6: luminance stacking, không shadow dày cho card, shadow chỉ dropdown/modal. Tối đa 1 stat "hero"/màn được nhấn mạnh, còn lại lùi elevation. |
| 8 | A11y | 12 (sàn 7.2) | `aria-label` cho control không có text hiển thị; focus order theo DOM hợp lý; mọi hành động click được cũng bấm được bằng bàn phím (Enter/Space); contrast ≥4.5:1 text thường, ≥3:1 text lớn ≥18px hoặc bold ≥14px. |
| 9 | Code quality | 6 (sàn 3.6) | `ref`/`computed`/`watch` đúng chỗ; `shallowRef` cho state thay nguyên khối; `v-for` key ổn định (không dùng index nếu list có thể sắp xếp lại); listener/timer gắn `onMounted` phải gỡ `onUnmounted`; không trùng logic giữa view (đẩy composable/store). **[Bổ sung] Cẩn thận CSS specificity giữa selector theo class và theo element (VD `.section` vs `.cta`) có thể triệt tiêu lẫn nhau — hay gặp nhất ở spacing/margin giữa các section trong `<style scoped>`; audit phải kiểm computed style thực tế, không chỉ đọc code.** |
| 10 | Performance | 6 (sàn 3.6) | Route-level code splitting (`defineAsyncComponent`/lazy route); ảnh lazy-load; không re-render thừa (Vue Devtools); bundle mỗi view không vượt ngưỡng bất thường so với view tương tự (so sánh chunk size). |

**Tổng hygiene = 100 điểm** (8 + 6 + 14 + 14 + 16 + 10 + 8 + 12 + 6 + 6 = 100 — cộng đúng 10 trục trên).

---

## 3. Trục Đặc trưng/Distinctiveness — TÁCH RIÊNG, KHÔNG cộng vào 100 điểm trên

- Thang: **0-10 điểm** (điểm riêng, ghi cột "đặc-trưng(/10)" trong scorecard).
- Cách chấm: **che logo/text thương hiệu**, hỏi *"màn hình này có thể của app dashboard/SaaS bất kỳ khác không?"*
  - **CÓ (chung chung, không nhận diện được app này)** → **0-3đ — PHẢI sửa** (thêm/làm rõ motif từ `DESIGN-IDENTITY.md`: ngôn ngữ nền tối + block dữ liệu + chỉ số mono, node/edge, Big-O...).
  - **Có ≥1 chi tiết chỉ app này mới có** (không chỉ đổi màu accent) → **8-10đ**.
  - 4-7đ: có dấu vết đặc trưng nhưng chưa rõ/chưa đủ mạnh — cần cân nhắc nâng.
- Vì sao tách riêng (không cộng chung 110đ): cộng chung cho phép hygiene cao bù điểm bản sắc thấp — đúng cơ chế đã gây "nhựa AI gen" dù vòng trước đạt ~90/100 hygiene (xem PROMPT mục 2.5).

---

## 4. Ngưỡng ĐẠT — cả 3 điều kiện sau, KHÔNG bù trừ cho nhau

> 1 view fail bất kỳ điều kiện nào = **KHÔNG ĐẠT** dù các điều kiện khác cao. Ghi kết quả vào cột "đạt/không-đạt" của `scorecard.md`.

1. **Tổng hygiene ≥ 80/100.**
2. **Không trục hygiene nào dưới mức sàn của chính trục đó** (cột "sàn" ở bảng mục 2: 4.8 / 3.6 / 8.4 / 8.4 / 9.6 / 6.0 / 4.8 / 7.2 / 3.6 / 3.6).
3. **Đặc trưng ≥ 7/10.**

---

## 5. KILL-LIST — cấm dùng trừ khi có lý do cụ thể ghi vào decision log

> Chép NGUYÊN từ PROMPT_VIEW_QUALITY_MASTER_V2 mục 3. Grep/soát mắt từng view TRƯỚC khi sửa, liệt kê chỗ vi phạm:

- Nút/CTA dùng gradient 2 màu mặc định (tím→xanh dương, hồng→cam...) không gắn với vai trò/ý nghĩa cụ thể.
- Card đồng loạt `rounded-2xl` + shadow mềm, không phân cấp độ nổi giữa card quan trọng và card phụ (mọi thứ "nổi" bằng nhau = không có gì nổi cả).
- Hero công thức: heading to căn giữa + đoạn mô tả + 2 nút CTA + blob gradient trang trí phía sau.
- Icon emoji (🎯🚀✨) hoặc icon stock chung chung thay vì hệ icon nhất quán đã chốt (DESIGN.md §4).
- Glassmorphism (`backdrop-blur` trắng mờ) phủ khắp mọi bề mặt thay vì dùng chọn lọc cho 1-2 điểm nhấn.
- Animation fade+slide-up **giống hệt nhau** trên mọi phần tử của mọi view — không phân biệt cái gì đáng "diễn" kỹ hơn cái gì.
- Công thức stat-card: icon tròn + số to + % tăng màu xanh, dùng ở mọi nơi kể cả khi không có ý nghĩa tường thuật thật.
- Layout 3 cột với vòng tròn số 01/02/03 khi nội dung không thực sự là một quy trình tuần tự.
- **3 cụm mặc định mà mô hình AI hay rơi vào bất kể brief là gì — tránh cả 3, trừ khi có lý do rõ ghi decision log**: (a) nền be/cream + serif tương phản cao + accent cam đất (gần `#D97757`); (b) nền gần đen + 1 accent xanh lá/đỏ chói; (c) layout kiểu báo in — hairline rules, bo góc = 0, cột chữ dày đặc kiểu newspaper. App học nghiêm túc không tự nhiên khớp với cụm nào trong 3 cụm này.
- **[Bổ sung] Microcopy chung chung/dịch máy móc**: label nút không nói đúng hành động thật ("Submit" thay vì tên hành động cụ thể); nút bấm và toast xác nhận dùng khác động từ với nhau (bấm "Lưu thay đổi" nhưng toast hiện "Cập nhật thành công" — lệch động từ); empty-state/error nói giọng hệ thống trơ trọi thay vì giọng sản phẩm giải thích + hướng xử lý.
- **[Bổ sung V2] Easing mặc định trình duyệt** (`ease`, `linear`, hoặc không khai báo transition-timing-function) cho bất kỳ chuyển động nào >150ms — cứng và thiếu "cảm giác cao cấp", tham số thay thế cụ thể: Phase 0 – BƯỚC E PROMPT (VD enter `cubic-bezier(0.16, 1, 0.3, 1)`, exit `cubic-bezier(0.7, 0, 0.84, 0)`, spring cho tương tác trực tiếp).

> Lưu ý: KILL-LIST nằm ngoài bảng chấm điểm — đây là danh sách cấm phải soát trước khi sửa; vi phạm KILL-LIST không phải "trừ điểm nhẹ" mà phải sửa, độc lập với điểm số.

# PROMPT_I_VIEW_QUALITY — Audit + nâng cấp chất lượng UI/UX toàn bộ view (Vue 3)

Dán vào `/pm "..." --auto`.

📌 **BÀI HỌC ĐÃ ĐÚC KẾT (bắt buộc đọc trước khi dispatch):**
- **LỖI "TASK TRẢ RỖNG" (13/08 — 4/4 lần)**: KHÔNG nhúng file >5KB vào prompt task (style-guide 21KB làm subagent vượt context → trả rỗng). Prompt task CHỈ trỏ đường dẫn file, agent tự đọc. Quy trình khi subagent trả rỗng: (1) bỏ nhúng file dài → thay đường dẫn; (2) task test siêu nhỏ xác nhận tool hoạt động; (3) tách nhỏ task (vd 5-6 view/task thay vì 36 view 1 lần); (4) resume session cũ — không tạo task trùng; (5) 2 lần fail → ghi FAIL + lý do.
- **Vòng Ollama bắt buộc (đã chốt)**: mỗi nhóm view sửa xong → dev-e2e chụp light+dark → qwen2.5vl:3b chấm 7 tiêu chí (thẩm mỹ/nhất quán/rõ ràng/phản hồi trực quan/luồng thao tác/tiếp cận/thỏa mãn — 1-5 điểm) → tiêu chí ≤ 3 PHẢI sửa (≤ 2 vòng) hoặc từ chối kèm lý do.

⚠ **RÀNG BUỘC CHẠY** (điều chỉnh nếu tình trạng nhánh khác lúc chạy): nếu có agent khác đang chạy song song trên `dev` → làm việc LOCAL, không tạo nhánh mới, không merge `dev`; commit (nếu có) vào nhánh `feature/view-quality` (tạo nếu chưa có) hoặc để local. Đầu ra đặt trong `docs/work/view-quality/`. Ghi log vào `docs/pm-report-viewquality.md` + quyết định (đổi màu/token/animation nào) vào `docs/pm-decision-log-viewquality.md`.

## BỐI CẢNH DỰ ÁN

SPA giáo dục cấu trúc dữ liệu, Vue 3 (Composition API + `<script setup>`), ~36 view, stack: shadcn-vue + Tailwind 4 (`@theme` token, không dùng `tailwind.config.js`) + motion-v (+ GSAP nếu cần) + vue-echarts. Actor: Khách, Người học, Giảng viên, Admin.

Mục tiêu người dùng đặt ra: **animation ở TẤT CẢ view (không chỉ vài view), bố cục hợp lý — dễ nhìn, căn chỉnh chuẩn, và tổng thể phải đẹp/nhất quán** (không chỉ đúng kỹ thuật). Không audit hời hợt — mỗi view phải có điểm số + danh sách lỗi cụ thể, không phải nhận xét chung chung.

## QUY TRÌNH 3 BƯỚC (bắt buộc)

### BƯỚC 1 — Dựng bảng chuẩn (research 1 lần, áp dụng cho toàn bộ 36 view)

Viết vào `docs/work/view-quality/standard.md` — đây là bảng chuẩn duy nhất, KHÔNG tự chế thêm quy tắc ngoài bảng này khi audit từng view:

| Trục | Quy tắc chuẩn |
|---|---|
| **Spacing/Grid** | Mọi padding/margin/gap phải là bội số của token trong `@theme` (4/8/12/16/24/32/48/64px) — cấm giá trị hardcode kiểu `p-[13px]`. Card/list nội bộ dùng spacing nhỏ hơn spacing giữa các nhóm (nguyên tắc internal < external). Grid 12 cột responsive. |
| **Breakpoint** | Test tối thiểu 3 mốc: 1366×768 (laptop), 768px (tablet), 390px (mobile) — không lệch/tràn/đè chữ ở mốc nào. |
| **Animation** | Chỉ animate `transform` + `opacity`. Duration: micro-interaction (hover, toggle, ripple) 100–150ms; transition thường (modal, card expand, route) 200–300ms; hero/full-screen tối đa 500ms — không vượt. Easing: `ease-out` cho phần tử xuất hiện, `ease-in` cho phần tử biến mất, không dùng `linear`. TẤT CẢ animation phải bọc trong check `prefers-reduced-motion` ở root (1 chỗ duy nhất, không lặp lại từng component). Mỗi animation phải trả lời được "phục vụ chức năng gì" (xác nhận hành động / dẫn hướng chú ý / thể hiện quan hệ không gian) — animation không giải thích được lý do thì loại bỏ. |
| **Nhất quán thị giác (đẹp/UI-UX)** | (a) Màu: chỉ dùng token trong `@theme`, không hex rời; 1 accent chính (teal — theo style-guide diagram đã chốt) + màu semantic cố định cho success/warning/error/info dùng xuyên suốt 36 view. (b) Typography: 1 type-scale cố định (H1–H4, body, caption) — không tự đặt font-size rời ở từng view. (c) Component: button/card/badge/input dùng đúng biến thể shadcn-vue có sẵn, không tự chế biến thể mới lặp lại logic. (d) Icon: 1 bộ icon set duy nhất, cùng size/stroke-width. (e) 3 trạng thái bắt buộc cho mọi view có list/table: skeleton loading, empty state (icon + message + CTA rõ ràng — không màn trắng), error state (thông báo + nút retry). |
| **A11y** | `aria-label` cho control không có text hiển thị; focus order theo DOM hợp lý (tab không nhảy lung tung); mọi hành động click được cũng bấm được bằng bàn phím (Enter/Space); contrast chữ/nền ≥ 4.5:1 (text thường), ≥ 3:1 (text lớn ≥18px hoặc bold ≥14px). |
| **Code quality** | `ref`/`computed`/`watch` dùng đúng chỗ (state chỉ tạo nơi cần, không đẩy lên component cha không cần thiết); `shallowRef` cho state chỉ thay nguyên khối; `v-for` có `key` ổn định (không dùng index nếu list có thể sắp xếp lại); listener/timer gắn trong `onMounted` phải gỡ trong `onUnmounted`; không logic trùng lặp giữa các view (đẩy vào composable/store). |
| **Performance** | Route-level code splitting (`defineAsyncComponent` / lazy route); ảnh dùng lazy-load; không re-render thừa (kiểm bằng Vue Devtools); bundle mỗi view không vượt ngưỡng bất thường so với view tương tự (so sánh chunk size). |

### BƯỚC 1.5 — Thư viện/công nghệ bổ sung cho "lung linh" (bắt buộc cân nhắc, KHÔNG lạm dụng)

Stack hiện có (shadcn-vue + Tailwind 4 + motion-v + GSAP) đã đủ cho animation chuẩn. Bổ sung dưới đây chỉ dùng cho các điểm nhấn cụ thể liên quan gamification/cảm giác thành tựu — không rải đều 36 view kiểu trang trí, vì app học tập nghiêm túc (giữ tinh thần "không neon/lòe loẹt" đã chốt ở phần diagram). Mọi thư viện thêm phải **lazy-load động** (`import()` khi cần dùng), không đẩy vào bundle chính — nếu không sẽ vi phạm chính tiêu chí Performance ở bảng BƯỚC 1.

| Nhu cầu | Thư viện đề xuất | Lý do chọn |
|---|---|---|
| Ăn mừng khi hoàn thành bài / lên cấp / đạt streak (rất hợp vì app có XP/gems/hearts/streak/leaderboard) | `canvas-confetti` | ~6KB gzip, dùng canvas nên mượt hơn animate DOM, có sẵn option `disableForReducedMotion` — khớp thẳng yêu cầu a11y đã có trong bảng chuẩn. |
| Minh họa/mascot/badge thành tựu dạng vector động (không phải ảnh tĩnh) | `@lottiefiles/dotlottie-vue` | Gói chính thức hiện hành của LottieFiles cho Vue 3 — **lưu ý: gói cũ `@lottiefiles/vue-lottie-player`/`lottie-vue` đã deprecated, không dùng**. Cho phép designer xuất animation từ After Effects, dev chỉ nhúng, không phải code tay từng keyframe. |
| Danh sách tự động animate khi thêm/xóa/sắp xếp lại (leaderboard cập nhật thứ hạng, danh sách bài tập lọc) | `@formkit/auto-animate` | 1 dòng gắn vào container cha, tự động có transition mượt khi DOM đổi — đỡ phải viết `<TransitionGroup>` tay cho từng danh sách trong 36 view, giảm r��i ro thiếu sót. |
| Route/scroll storytelling ở trang tổng quan, dashboard | GSAP `ScrollTrigger` (đã có GSAP sẵn, chỉ thêm plugin) | Không cần thư viện mới — tận dụng GSAP đã duyệt trong stack. |
| Chuyển cảnh giữa route mượt hơn transition CSS thường | View Transitions API native qua `router.options.viewTransition` (nếu trình duyệt mục tiêu hỗ trợ — Chrome/Edge) | Không tốn thư viện, chỉ 1 flag ở Vue Router — dùng làm fallback nhẹ cho các route không cần GSAP phức tạp. |
| Minh họa cấu trúc dữ liệu/thuật toán có chiều sâu (cân nhắc kỹ, không bắt buộc) | TresJS (Three.js cho Vue) | Bundle nặng nhất trong danh sách — chỉ cân nhắc cho 1 view trọng điểm (VD trang chủ hoặc UC-01 "Chạy mô phỏng"), không dùng đại trà; nếu thêm phải audit riêng ảnh hưởng bundle/FPS trên máy yếu. |
| Bề mặt/thẩm mỹ (không cần thư viện JS) | Glassmorphism có chọn lọc (`backdrop-blur` cho card nổi bật/modal, không lạm dụng toàn trang), gradient nhẹ cho nền hero/dashboard thay vì màu phẳng, skeleton loading dạng shimmer (CSS `@keyframes` + gradient di chuyển) | Đây là kỹ thuật CSS thuần, không thêm dependency — quyết định ở cấp token màu trong `@theme`, ghi vào `standard.md` ở BƯỚC 1 nếu áp dụng. |

Ghi rõ vào `docs/pm-decision-log-viewquality.md`: thư viện nào được chọn dùng ở view nào, lý do — tránh mỗi dev tự thêm thư viện animation khác nhau gây phình bundle và mất nhất quán.

### BƯỚC 2 — Audit từng view (dev-ux)

Với mỗi view trong 36 view: chấm điểm 0–100 theo 6 trục trên (mỗi trục ~16-17 điểm), liệt kê lỗi cụ thể kèm dòng code/selector, ghi vào `docs/work/view-quality/audit-<ten-view>.md`. Tổng hợp bảng điểm tất cả view vào `docs/work/view-quality/scorecard.md` (cột: view | code | animation | spacing | thị giác | a11y | performance | tổng | ưu tiên sửa).

Sắp xếp thứ tự sửa theo: (1) view actor Người học/Giảng viên dùng nhiều nhất trước, (2) điểm thấp nhất trước, (3) lỗi a11y chặn thao tác (không bấm được bằng phím) ưu tiên tuyệt đối bất kể điểm tổng.

### BƯỚC 3 — Sửa + re-check (dev-ux → dev-review)

Sửa theo bảng chuẩn BƯỚC 1. Sau khi sửa 1 view: re-chấm điểm, ghi log trước/sau vào `docs/work/view-quality/fix-log.md` (view | điểm trước | điểm sau | thay đổi chính). KHÔNG được tự sáng tạo màu/animation/spacing ngoài bảng chuẩn — nếu cần ngoại lệ, ghi lý do vào decision log trước khi làm.

## VERIFY (dev-review + dev-test)

- Mọi view đạt tổng điểm ≥ 80/100, không view nào < 60.
- **Đo được, không chỉ nhìn**: chạy Lighthouse (Performance + Accessibility) cho từng view — Accessibility ≥ 90, Performance ≥ 80. Chạy `axe-core` — 0 vi phạm mức critical/serious.
- Đếm: số view có ít nhất 1 animation dùng motion-v/GSAP = 36/36 (không view nào bị bỏ sót).
- Nếu có dùng thư viện bổ sung ở BƯỚC 1.5 (confetti/lottie/auto-animate/TresJS): kiểm tra bundle chính (initial load) không tăng kích thước — xác nhận các thư viện này nằm trong chunk lazy-load riêng, không nằm trong `main.js`/entry chunk.
- Đếm: số chỗ hardcode spacing ngoài token = 0 (grep class Tailwind dạng `\[[0-9]+px\]` phải về 0 hoặc có lý do ghi trong decision log).
- Test bằng bàn phím thuần (không chuột) đi hết luồng chính của mỗi actor — không bị kẹt/mất focus.
- Test 3 breakpoint bằng Playwright, chụp ảnh lại — không tràn/đè chữ.
- Tuỳ chọn (nếu có Ollama vision sẵn từ task diagram trước): gửi screenshot 5-6 view tiêu biểu cho `qwen2.5vl:3b` hỏi "bố cục có cân không, có chỗ nào rối/khó nhìn không" — bổ sung góc nhìn "mắt người" ngoài số đo.
- dev-review tổng hợp: bảng điểm trước/sau toàn bộ 36 view + danh sách lỗi a11y đã sửa + xác nhận không có view nào bị bỏ animation.

Việc còn chờ / không chắc → ghi vào `docs/work/view-quality/notes.md`, không tự quyết ngoài bảng chuẩn BƯỚC 1. --auto

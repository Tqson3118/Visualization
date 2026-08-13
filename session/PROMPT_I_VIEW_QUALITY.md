# PROMPT_I_VIEW_QUALITY — Audit + nâng cấp chất lượng UI/UX toàn bộ view (Vue 3)

Dán vào `/pm "..." --auto`.

📌 **BÀI HỌC ĐÃ ĐÚC KẾT (bắt buộc đọc trước khi dispatch):**
- **LỖI "TASK TRẢ RỖNG" (13/08 — 4/4 lần)**: KHÔNG nhúng file >5KB vào prompt task (style-guide 21KB làm subagent vượt context → trả rỗng). Prompt task CHỈ trỏ đường dẫn file, agent tự đọc. Quy trình khi subagent trả rỗng: (1) bỏ nhúng file dài → thay đường dẫn; (2) task test siêu nhỏ xác nhận tool hoạt động; (3) tách nhỏ task (vd 5-6 view/task thay vì 36 view 1 lần); (4) resume session cũ — không tạo task trùng; (5) 2 lần fail → ghi FAIL + lý do.
- **Vòng Ollama bắt buộc (đã chốt)**: mỗi nhóm view sửa xong → dev-e2e chụp light+dark → qwen2.5vl:3b chấm 7 tiêu chí (thẩm mỹ/nhất quán/rõ ràng/phản hồi trực quan/luồng thao tác/tiếp cận/thỏa mãn — 1-5 điểm) → tiêu chí ≤ 3 PHẢI sửa (≤ 2 vòng) hoặc từ chối kèm lý do. **Prompt gửi cho model PHẢI có câu hỏi cụ thể về spacing/density (model 3b bỏ sót nếu chỉ hỏi "thẩm mỹ" chung chung)**: "button có padding thoáng không, chữ có chạm/sát viền không, các nút có dính nhau không, khoảng cách giữa các khối nội dung có quá sát không" → chấm riêng 1 điểm mục 'spacing/thoáng đãng' (1-5), ≤ 3 điểm → PHẢI sửa. Ghi điểm mục này vào `docs/work/view-quality/` cùng 7 tiêu chí.

⚠ **RÀNG BUỘC CHẠY** (điều chỉnh nếu tình trạng nhánh khác lúc chạy): nếu có agent khác đang chạy song song trên `dev` → làm việc LOCAL, không tạo nhánh mới, không merge `dev`; commit (nếu có) vào nhánh `feature/view-quality` (tạo nếu chưa có) hoặc để local. Đầu ra đặt trong `docs/work/view-quality/`. Ghi log vào `docs/pm-report-viewquality.md` + quyết định (đổi màu/token/animation nào) vào `docs/pm-decision-log-viewquality.md`.

## BỐI CẢNH DỰ ÁN

SPA giáo dục cấu trúc dữ liệu, Vue 3 (Composition API + `<script setup>`), ~36 view, stack: shadcn-vue + Tailwind 4 (`@theme` token, không dùng `tailwind.config.js`) + motion-v (+ GSAP nếu cần) + vue-echarts. Actor: Khách, Người học, Giảng viên, Admin.

Mục tiêu người dùng đặt ra: **animation ở TẤT CẢ view (không chỉ vài view), bố cục hợp lý — dễ nhìn, căn chỉnh chuẩn, và tổng thể phải đẹp/nhất quán** (không chỉ đúng kỹ thuật). Không audit hời hợt — mỗi view phải có điểm số + danh sách lỗi cụ thể, không phải nhận xét chung chung.

## QUY TRÌNH 3 BƯỚC (bắt buộc)

### BƯỚC 1 — Dựng DESIGN.md (brand contract) + bảng chuẩn audit (research 1 lần, áp dụng cho toàn bộ 36 view)

**PHƯƠNG PHÁP (nguồn: Open Design — `nexu-io/open-design`, Apache-2.0, 30.9k⭐ — CHỈ mượn PHƯƠNG PHÁP, KHÔNG cài tool, KHÔNG copy token thương hiệu khác):** mỗi sản phẩm có **1 file `DESIGN.md` duy nhất = brand contract** (màu + vai trò, typography hierarchy, component, spacing/radius scale, elevation, do's/don'ts, **Agent Prompt Guide**). Agent bắt buộc đọc file này TRƯỚC khi sửa/vẽ bất kỳ view nào → "brand-grade by default", không ai tự bịa style. Tham khảo cấu trúc mẫu (Linear = dark precision · Notion = light workspace · Raycast = warm-dark) — chỉ lấy ý tưởng cấu trúc, KHÔNG lấy màu/font của họ.

**BƯỚC 1a — Viết `frontend/DESIGN.md`** (dev-ux, ~200-300 dòng, token code THẬT của repo), cấu trúc 9 phần bắt buộc:
1. **Visual Theme & Atmosphere** — tinh thần thị giác: app học tập nghiêm túc, accent teal #0D9488 (CHỈ interactive), không neon/lòe loẹt; dark + light.
2. **Color Palette & Roles** — mỗi màu có VAI TRÒ (không chỉ hex): nền/surface 3-4 tầng, **text 4 tầng** (primary/secondary/tertiary/quaternary-muted), border 2-3 tầng (ưu tiên semi-transparent), accent, semantic success/warning/error/info — TẤT CẢ map token `@theme`; token thiếu → THÊM vào `@theme` (cấm hex rời trong component).
3. **Typography Rules** — bảng hierarchy đầy đủ (H1-H4/body/caption/label/mono): Geist + JetBrains Mono, size, weight (3 mức 400/500/600 — cấm 700 lung tung), line-height, **letter-spacing âm cho heading to, giảm dần theo size**.
4. **Component Stylings** — map từng variant shadcn-vue SẴN CÓ (Button/Card/Badge/Input/Modal/Table): padding, radius, border, hover/focus — cấm tự chế variant mới lặp logic. **Bắt buộc ghi bảng padding button** (buttonVariants: md `h-10 px-4 py-2` · sm `h-9 px-3` · lg `h-11 px-8` · icon `h-10 w-10`; text cách viền ≥ 8px; icon+text gap ≥ 8px; chiều cao tối thiểu 40px; target ≥ 24×24 — WCAG 2.5.8) — đây là phần user phàn nàn nhất (13/08: button khít chữ, không padding).
5. **Layout Principles** — spacing scale bội số token (4/8/12/16/24/32/48/64), radius scale, grid 12 cột, internal < external.
6. **Depth & Elevation** — kỹ thuật chính: **luminance stacking** (card nổi bằng surface sáng hơn + border semi-transparent `rgba(255,255,255,0.05-0.08)` dark / tối dần light) THAY VÌ shadow dày; shadow chỉ cho dropdown/modal/focus.
7. **Do's and Don'ts** — ≤10 mỗi loại, ngắn gọn (VD: don't dùng white thuần làm text chính; don't dùng accent trang trí...).
8. **Responsive Behavior** — 3 mốc 1366/768/390, chiến lược collapse từng thành phần.
9. **Agent Prompt Guide** — 5-8 câu prompt mẫu component (VD: "Create a card: surface level-2, border standard, radius card, title 20px weight 600 letter-spacing -0.24px") + quy ước gọi tên token — sub-agent copy-paste, khỏi suy đoán.

**BƯỚC 1b — Viết `docs/work/view-quality/standard.md`** — bảng chuẩn audit TRÍCH TỪ DESIGN.md (bảng bên dưới + token đã chốt), KHÔNG tự chế thêm quy tắc ngoài bảng này khi audit từng view:

| Trục | Quy tắc chuẩn |
|---|---|
| **Spacing/Grid** | Mọi padding/margin/gap phải là bội số của token trong `@theme` (4/8/12/16/24/32/48/64px) — cấm giá trị hardcode kiểu `p-[13px]`. Card/list nội bộ dùng spacing nhỏ hơn spacing giữa các nhóm (nguyên tắc internal < external). Grid 12 cột responsive. |
| **Breakpoint** | Test tối thiểu 3 mốc: 1366×768 (laptop), 768px (tablet), 390px (mobile) — không lệch/tràn/đè chữ ở mốc nào. |
| **Animation** | Chỉ animate `transform` + `opacity`. Duration: micro-interaction (hover, toggle, ripple) 100–150ms; transition thường (modal, card expand, route) 200–300ms; hero/full-screen tối đa 500ms — không vượt. Easing: `ease-out` cho phần tử xuất hiện, `ease-in` cho phần tử biến mất, không dùng `linear`. TẤT CẢ animation phải bọc trong check `prefers-reduced-motion` ở root (1 chỗ duy nhất, không lặp lại từng component). Mỗi animation phải trả lời được "phục vụ chức năng gì" (xác nhận hành động / dẫn hướng chú ý / thể hiện quan hệ không gian) — animation không giải thích được lý do thì loại bỏ. |
| **Nhất quán thị giác (đẹp/UI-UX)** | (a) Màu: CHỈ token trong `@theme`/DESIGN.md §2 — 1 accent teal chính + semantic success/warning/error/info cố định xuyên suốt 36 view; accent KHÔNG trang trí, chỉ CTA/active/link. (b) Typography: bảng hierarchy DESIGN.md §3 — không tự đặt font-size/weight/tracking rời. (c) Component: dùng đúng biến thể shadcn-vue trong DESIGN.md §4. (d) Icon: 1 bộ icon duy nhất, cùng size/stroke-width. (e) 3 trạng thái bắt buộc cho mọi view có list/table: skeleton loading, empty state (icon + message + CTA — không màn trắng), error state (thông báo + nút retry). |
| **Interactive sizing & spacing (button/input — ƯU TIÊN SỬA TRƯỚC, user phàn nàn rõ 13/08: "button bị khít với chữ, không có padding, xấu lắm")** | Đã verify thật: **17 thẻ `<button>` raw + 38 toàn src** (không qua Button.vue) → tự chế padding sát chữ. Quy tắc: (a) MỌI button/clickable PHẢI qua `Button.vue` (buttonVariants) hoặc `buttonVariants()` — grep lỗi: `<button` raw (trừ nút trong canvas/editor/table-cell cần hành vi đặc biệt → ghi decision log); (b) padding chuẩn buttonVariants (có sẵn, KHÔNG override): md `h-10 px-4 py-2` (chữ cách viền ≥ 8px mỗi bên), sm `h-9 px-3`, lg `h-11 px-8`, icon `h-10 w-10`; cấm `px-0/px-1/px-2/p-0/py-0` trên button chữ (chỉ icon-button dùng w/h); (c) icon + text trong button cách nhau ≥ 8px (`gap-2` sẵn có — không đè); (d) chiều cao tối thiểu 40px (desktop) / 44px (mobile) cho nút chính, target tối thiểu 24×24 (WCAG 2.5.8 AA — nút nhỏ như icon phải có vùng bấm ≥ 24px); (e) nút liền kề cách nhau ≥ 8px (không dính nhau); (f) badge/pill cũng ≥ 6px padding ngang + height ≥ 24px; (g) khi audit: mở DevTools đo computed padding TỪNG button — chữ KHÔNG được chạm viền/chạm chữ khác. |
| **Typography** | Theo DESIGN.md §3: 1 type-scale cố định, tracking âm cho heading (giảm dần theo size), 3 mức weight — grep cấm: `font-bold`/`700` ở heading, `tracking-[...]` dương rời, font-size tự đặt ngoài scale. |
| **Depth & Elevation** | Theo DESIGN.md §6: card nổi bằng luminance stacking + border semi-transparent, không shadow dày cho card; shadow chỉ dropdown/modal; nền tối dark không dùng viền đặc màu đen. |
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

**BẮT BUỘC khi audit (vấn đề user nêu 13/08 — không bỏ qua view nào):** rà TỪNG button/input/clickable trong view: (1) có qua Button.vue/buttonVariants không (grep `<button`), (2) đo padding thực tế (DevTools hoặc Playwright `getComputedStyle`) — chữ chạm viền = LỖI nặng, (3) target size ≥ 24×24, (4) nút cạnh nhau không dính. Ghi từng lỗi kèm dòng code.

Sắp xếp thứ tự sửa theo: (1) view actor Người học/Giảng viên dùng nhiều nhất trước, (2) điểm thấp nhất trước, (3) lỗi a11y chặn thao tác (không bấm được bằng phím) ưu tiên tuyệt đối bất kể điểm tổng.

### BƯỚC 3 — Sửa + re-check (dev-ux → dev-review)

Sửa theo bảng chuẩn BƯỚC 1. Sau khi sửa 1 view: re-chấm điểm, ghi log trước/sau vào `docs/work/view-quality/fix-log.md` (view | điểm trước | điểm sau | thay đổi chính). KHÔNG được tự sáng tạo màu/animation/spacing ngoài bảng chuẩn — nếu cần ngoại lệ, ghi lý do vào decision log trước khi làm.

## VERIFY (dev-review + dev-test)

- Mọi view đạt tổng điểm ≥ 80/100, không view nào < 60.
- **Đo được, không chỉ nhìn**: chạy Lighthouse (Performance + Accessibility) cho từng view — Accessibility ≥ 90, Performance ≥ 80. Chạy `axe-core` — 0 vi phạm mức critical/serious.
- `frontend/DESIGN.md` tồn tại đủ 9 phần + mọi view audit/sửa ĐỀU tham chiếu được token/naming trong đó (dev-review kiểm tra ngẫu nhiên 5 view: không hex rời, không font-size/weight/tracking tự đặt ngoài bảng §3, không shadow dày ngoài §6).
- Đếm: số view có ít nhất 1 animation dùng motion-v/GSAP = 36/36 (không view nào bị bỏ sót).
- Nếu có dùng thư viện bổ sung ở BƯỚC 1.5 (confetti/lottie/auto-animate/TresJS): kiểm tra bundle chính (initial load) không tăng kích thước — xác nhận các thư viện này nằm trong chunk lazy-load riêng, không nằm trong `main.js`/entry chunk.
- Đếm: số chỗ hardcode spacing ngoài token = 0 (grep class Tailwind dạng `\[[0-9]+px\]` phải về 0 hoặc có lý do ghi trong decision log).
- **Đếm (vấn đề button — user chốt 13/08):** số thẻ `<button>` raw còn lại trong `frontend/src/views` = 0 (hiện 17 → mọi nút phải qua Button.vue/buttonVariants, trừ trường hợp ghi decision log); số button có chữ chạm viền (computed padding ngang < 8px hoặc chiều cao < 40px với nút chính) = 0 — kiểm bằng Playwright `getComputedStyle` trên 10 view ngẫu nhiên (kèm ảnh trước/sau lưu `docs/work/view-quality/`).
- Test bằng bàn phím thuần (không chuột) đi hết luồng chính của mỗi actor — không bị kẹt/mất focus.
- Test 3 breakpoint bằng Playwright, chụp ảnh lại — không tràn/đè chữ.
- Tuỳ chọn (nếu có Ollama vision sẵn từ task diagram trước): gửi screenshot 5-6 view tiêu biểu cho `qwen2.5vl:3b` hỏi "bố cục có cân không, có chỗ nào rối/khó nhìn không" — bổ sung góc nhìn "mắt người" ngoài số đo.
- dev-review tổng hợp: bảng điểm trước/sau toàn bộ 36 view + danh sách lỗi a11y đã sửa + xác nhận không có view nào bị bỏ animation.

Việc còn chờ / không chắc → ghi vào `docs/work/view-quality/notes.md`, không tự quyết ngoài DESIGN.md/bảng chuẩn BƯỚC 1. --auto

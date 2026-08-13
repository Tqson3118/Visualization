# PROMPT_II_VIEW_QUALITY_FOUNDATION — Phase 0 gộp: DESIGN-IDENTITY + DESIGN.md + standard.md + component chung

Dán vào `/pm "..." --auto`.

⚠ **VAI TRÒ**: gộp **PROMPT_I BƯỚC 1** (dựng DESIGN.md + standard.md) với **PROMPT_II BƯỚC 0** (xác lập Design Identity) thành **1 session nền tảng DUY NHẤT** (quyết định user 13/08 — lựa chọn B). Sau session này mới chạy **PROMPT_II Phase 1** (4 session song song nhóm A/B/C/D) — không chạy Phase 1 trước khi session này xong (mọi nhóm PHỤ THUỘC các file dưới đây).

📌 **BÀI HỌC ĐÃ ĐÚC KẾT (bắt buộc):**
- KHÔNG nhúng file >5KB vào prompt task — prompt task CHỈ trỏ đường dẫn, agent tự đọc.
- Ghi quyết định vào `docs/pm-decision-log-viewquality.md` TRƯỚC khi làm (--auto). Lệch docs chủ ý → mục đồng bộ docs đi kèm.
- Không ai tự chấm bài mình: session này tự verify build/test, dev-review độc lập ở Phase 2.
- PR base `dev` KHÔNG main. Commit-as: frontend/design→son, docs→phuc.

⚠ **RÀNG BUỘC CHẠY**: làm việc ở **worktree RIÊNG `D:\FPT\neww-qbase`** (chưa tồn tại — tạo trước khi làm):
```
git fetch origin dev
git worktree add D:\FPT\neww-qbase -b feature/view-quality-base origin/dev
# làm việc tại D:\FPT\neww-qbase; xong verify → PR base dev (hoặc merge dev nếu user yêu cầu)
```
KHÔNG đụng working tree `D:\FPT\neww` (có file rác diagrams/docs chưa commit). KHÔNG sửa backend/DB/i18n. KHÔNG audit/sửa view (việc của Phase 1).

## MỤC TIÊU (1 session, đầu ra = nền tảng duy nhất cho 36 view)

Tạo bộ 4 file + component chung để mọi session Phase 1 bám theo — hết "mỗi view tự bịa style", hết "nhựa AI gen":

1. `frontend/DESIGN-IDENTITY.md` — bản sắc (BƯỚC A)
2. `frontend/DESIGN.md` — brand contract 9 phần (BƯỚC B)
3. `docs/work/view-quality/standard.md` — bảng chuẩn audit (BƯỚC C)
4. `docs/work/view-quality/scorecard.md` — khung điểm 36 view (BƯỚC D)
5. Component dùng chung nếu cần (EmptyState...) + cài `@vue-flow/core` nếu chốt (BƯỚC E)

## BƯỚC A — Xác lập Design Identity (`frontend/DESIGN-IDENTITY.md`, ~1 trang)

Quy trình 2 pha (brainstorm → tự phản biện → mới viết DESIGN.md §1), theo PROMPT_II BƯỚC 0:

**Pha 1 — Brainstorm**:
- **Ground vào chủ đề THẬT**: không phải "app giáo dục" chung chung — "nơi người học xây mental model về cây/đồ thị/stack bằng cách thao tác trực quan với chúng". Chọn 1-2 motif thị giác bắt nguồn từ DSA (gợi ý đã verify: **vùng mô phỏng nền tối + block số + chỉ số mono** ở r2-fixed-04/09 là phần có bản sắc rõ nhất — nên lan tỏa ngôn ngữ đó thay vì phát minh mới; node-edge network làm texture nền chọn lọc; mono cho index/độ phức tạp Big-O).
- **Bảng màu**: 4-6 mã hex có TÊN + VAI TRÒ + LÝ DO gắn chủ đề (không phải "màu đẹp"). Giữ accent teal #0D9488 (đã chốt) — làm rõ VAI TRÒ (chỉ interactive) + các tầng surface/text/border theo luminance stacking.
- **Typography**: ≥2 vai trò (display tiết chế + body trung tính + mono cho dữ liệu/code/Big-O) — Geist + JetBrains Mono đã có.
- **Layout concept**: 1 câu + ASCII wireframe cho 4 loại màn đại diện (dashboard, luồng học bài, editor/mô phỏng, danh sách).
- **Signature**: 1 chi tiết duy nhất đáng nhớ (VD: cách stack/queue "thở" khi push/pop, trạng thái thuật toán đang chạy...) — tự nghiên cứu, KHÔNG sao chép Linear/Notion/Raycast.

**Pha 2 — Tự phản biện (bắt buộc)**: mỗi phần tự hỏi "đưa prompt này cho AI khác build 1 dashboard bất kỳ, nó ra kết quả tương tự không?" — nếu CÓ → phần đó chưa đủ đặc trưng, sửa + ghi lại đã đổi gì/vì sao. **Kỷ luật tiết chế**: táo bạo dồn vào signature element duy nhất; phần còn lại im lặng. KHÔNG thêm thứ chỉ để "đẹp hơn".

## BƯỚC B — Viết `frontend/DESIGN.md` (brand contract, 9 phần — theo PROMPT_I BƯỚC 1a)

§1 (Visual Theme) THAM CHIẾU DESIGN-IDENTITY.md (viết sau khi BƯỚC A pass Pha 2). Các phần còn lại:
§2 Color Palette & Roles (màu có vai trò, text 4 tầng primary/secondary/tertiary/muted, border semi-transparent, semantic status — map token `@theme`; thiếu token → THÊM vào `@theme`, cấm hex rời trong component)
§3 Typography Rules (bảng hierarchy: H1-H4/body/caption/label/mono; size/weight 3 mức cấm 700 lung tung/line-height/**tracking âm heading**)
§4 Component Stylings — map variant shadcn-vue sẵn có. **BẮT BUỘC bảng padding button** (buttonVariants: md `h-10 px-4 py-2` · sm `h-9 px-3` · lg `h-11 px-8` · icon `h-10 w-10`; text cách viền ≥8px; icon+text gap ≥8px; min-height 40px; target ≥24×24 WCAG 2.5.8) — vấn đề user phàn nàn nhất (button khít chữ).
§5 Layout Principles (spacing scale bội token 4/8/12/16/24/32/48/64, radius scale, grid 12, internal < external)
§6 Depth & Elevation (luminance stacking + border semi-transparent thay shadow dày; shadow chỉ dropdown/modal/focus)
§7 Do's and Don'ts (≤10 mỗi loại — VD: don't gradient 2 màu trang trí không có nghĩa, don't emoji làm icon, don't mọi card nổi bằng nhau)
§8 Responsive Behavior (3 mốc 1366/768/390)
§9 Agent Prompt Guide (5-8 câu prompt mẫu component + quy ước gọi tên token — sub-agent Phase 1 copy-paste)

## BƯỚC C — Viết `docs/work/view-quality/standard.md` (bảng audit TRÍCH từ DESIGN.md — theo PROMPT_I BƯỚC 1b + PROMPT_II patch)

Các trục (không tự chế thêm): **Spacing/Grid** · **Breakpoint** · **Animation** (I gốc + II sửa: mỗi view 1-2 khoảnh khắc đầu tư kỹ, còn lại tối giản; CẤM fade+translateY giống hệt >80% card trong 1 view = lỗi "animation cơ giới") · **Nhất quán thị giác** · **Typography** · **Depth & Elevation** · **Interactive sizing & spacing** (button — ưu tiên sửa trước: mọi nút qua Button.vue/buttonVariants, cấm raw `<button>`/`px-0/1/2/p-0` trừ icon-button, đo computed padding khi audit) · **Đặc trưng/Distinctiveness** (trục mới 0-10: che logo → "màn hình này có thể của app dashboard bất kỳ?" — CÓ → 0-3 điểm phải sửa; có chi tiết chỉ app này mới có → 8-10) · **A11y** · **Code quality** · **Performance**.

Kèm KILL-LIST (chép từ PROMPT_II — grep/soát mắt mọi view): gradient 2 màu mặc định vô nghĩa · card đồng loạt rounded-2xl+shadow không phân cấp · hero công thức (heading giữa + 2 CTA + blob) · emoji/icon stock chung chung · glassmorphism tràn lan · stat-card công thức lặp · layout 01/02/03 khi không phải quy trình · 2 cụm màu "be/cream+serif+cam đất" và "nền đen+accent chói".

## BƯỚC D — Khởi tạo `docs/work/view-quality/scorecard.md`

Bảng rỗng đủ cột: `view | code | animation | spacing | thị giác | a11y | performance | Đặc trưng | tổng | ưu tiên sửa` — liệt kê 36 view theo route/SCREEN_MAP (đọc `docs/SCREEN_MAP.md` nếu có — chỉ liệt kê, KHÔNG chấm điểm; Phase 1 điền). Ghi chú 6 vi phạm đã xác nhận từ r2-fixed-01..12 (banner gradient random mỗi trang, path card phẳng thay vì graph, empty-state công thức lặp, 🎯 2 ngữ cảnh, card không phân cấp, block token mô phỏng là chất liệu bản sắc) vào mục "ưu tiên cao".

## BƯỚC E — Component chung + thư viện signature (TỐI THIỂU, có lý do)

- **EmptyState**: nếu Phase 1 cần empty-state thống nhất (đã xác nhận lỗi ở ladder/benchmark/leaderboard) → thiết kế 1 EmptyState.vue dùng motif mảng `[ ]` cách điệu (theo DESIGN-IDENTITY), thay thế công thức cũ ở Phase 1. KHÔNG đổi Button/Badge trừ khi DESIGN.md §4 yêu cầu.
- **`@vue-flow/core`**: CHỈ cài nếu DESIGN-IDENTITY chốt signature "lộ trình = node-edge" (khuyến nghị mạnh — r2-fixed-06 là cơ hội signature lớn nhất). Cài: `pnpm add @vue-flow/core` (hoặc npm) + VERIFY lazy-load: view dùng phải `defineAsyncComponent` — bundle chính KHÔNG tăng (đo trước/sau `npm run build` — ghi số vào decision log). KHÔNG cài thư viện nào khác (Rive/dotlottie/auto-animate là việc Phase 1 từng nhóm, tối đa 1-2/nhóm).
- **Token `@theme`**: bổ sung token thiếu cho §2/§3/§6 (text 4 tầng, border tiers, radius scale nếu thiếu) — file `frontend/src/style.css` (hoặc nơi @theme đặt — tìm trước, hỏi không cần).

## KHÔNG LÀM (cấm)

Không audit/chấm điểm view · không sửa file view `.vue` của Phase 1 (trừ EmptyState/component chung) · không sửa backend/i18n/vi.ts · không thêm >1 thư viện · không đổi màu banner từng trang (việc Phase 1) · không xóa dữ liệu.

## VERIFY (tự chạy — bắt buộc trước khi báo xong)

1. `frontend/DESIGN-IDENTITY.md` đủ 5 mục (motif/bảng màu/typography/layout/signature) + Pha 2 ghi ít nhất 1 lần đã sửa vì "quá chung chung" — nếu pass ngay lần đầu = chưa phản biện đủ, làm lại.
2. `frontend/DESIGN.md` đủ 9 phần + §1 tham chiếu DESIGN-IDENTITY + §4 có bảng padding button.
3. `standard.md` đủ trục + kill-list; `scorecard.md` đủ 36 view + 6 ưu tiên cao.
4. `npm run build` (hoặc lệnh build chuẩn của repo — xem package.json) PASS + `vue-tsc` sạch; nếu cài vue-flow: bundle chính không tăng (so sánh chunk size trước/sau trong decision log).
5. Grep: component mới không có hex rời ngoài token (`#[0-9a-fA-F]{3,8}` trong class/style mới = 0 hoặc ghi lý do).
6. Ollama (tùy chọn): gửi 2 ảnh (path cũ vs wireframe concept nếu có) cho qwen2.5vl:3b hỏi "có đặc trưng riêng hay chung chung" — ghi kết quả.

## GIT

- Nhánh: `feature/view-quality-base` từ origin/dev (worktree `D:\FPT\neww-qbase`) → **PR base dev** (hoặc merge dev nếu user yêu cầu — hỏi/báo trong report).
- Commit tối đa 2: `.\commit-as.ps1 son "feat(ui): DESIGN.md + DESIGN-IDENTITY + standard/scorecard + EmptyState + vue-flow"` (code) + `.\commit-as.ps1 phuc "docs: view-quality foundation"` (nếu tách docs).
- Sau khi merge dev → báo các session Phase 1: fetch + nhánh từ dev MỚI (chứa foundation).

## BÁO CÁO (≤10 dòng)

File tạo/sửa; quyết định bản sắc (motif, signature chốt là gì — ngắn); token đã thêm; vue-flow có cài không + số đo bundle; Pha 2 đã sửa gì; VERIFY kết quả (build/vue-tsc); PR link/merge; đề xuất bước sau (KHÔNG làm).

# PROMPT_II_VIEW_QUALITY_SOUL — Thoát "nhựa AI gen": nâng bản sắc thị giác toàn bộ view

Dán vào `/pm "..." --auto`.

⚠ **QUAN HỆ VỚI v1**: đây là **patch bổ sung**, KHÔNG thay thế `PROMPT_I_VIEW_QUALITY.md`. Chạy SAU khi hạ tầng BƯỚC 1–3 của v1 đã tồn tại (`frontend/DESIGN.md`, `docs/work/view-quality/standard.md`, `scorecard.md`). Nếu v1 chưa chạy hoặc chưa xong → chạy/hoàn tất v1 trước, đừng chạy song song (tránh 2 agent cùng sửa `DESIGN.md`). Giữ nguyên toàn bộ ràng buộc vận hành đã đúc kết ở v1 (không nhúng file dài vào prompt task, tách nhỏ theo nhóm view, resume session cũ, log vào `docs/pm-report-viewquality.md` + `docs/pm-decision-log-viewquality.md`).

## CHẠY 3-4 SESSION SONG SONG (chốt 13/08 — x4 tiến độ, kèm điều kiện)

**PHA 0 — 1 session DUY NHẤT (serial, bắt buộc làm trước, không song song):** BƯỚC 0 (DESIGN-IDENTITY.md) + cập nhật `frontend/DESIGN.md` §1/§2 + `standard.md` + token `@theme` + component dùng CHUNG (EmptyState mới, Button/Badge nếu đổi) + cài/verify `@vue-flow/core` (nếu chốt). Lý do: mọi session sau PHỤ THUỘC các file này; 2 agent viết cùng 1 file = loạn. Hết Phase 0 → commit 1 lần lên dev (hoặc nhánh feature/view-quality-base), báo các session khác fetch.

**PHA 1 — 3-4 session SONG SONG (mỗi session 1 worktree riêng + nhánh `feature/view-quality-{a|b|c|d}`, PR base dev):** chia 36 view thành 4 nhóm KHÔNG chồng file:

| Nhóm | View (theo route/SCREEN_MAP) | Điểm nhấn II |
|---|---|---|
| **A — Landing & auth** | home, login, register, forgot/reset, 404, privacy, help, cheat-sheet, explore | hero/landing bản sắc, banner hệ thống hóa, kill-list hero formula |
| **B — Học tập** | path, lessons, lesson-detail, simulator, exercise, lab, code-runner, benchmark | **SIGNATURE: path = node-edge (@vue-flow/core)** + vùng mô phỏng "block token" lan tỏa (chất liệu sẵn có từ r2-fixed-04/09) |
| **C — Gamification** | home/dashboard (nếu tách), shop, quests, leaderboard, profile, premium, subscription | stat-card phân cấp lại, empty-state `[ ]` motif, block token cho XP/gems/streak |
| **D — Classes & Admin** | classes, class-detail, admin users/settings/content/feedback, teacher views | empty-state thống nhất, icon có chủ đích, giữ kỷ luật nhất |

Ràng buộc PHA 1: (1) mỗi nhóm chỉ sửa view + i18n key ĐƯỢC GÁN (phân vùng key: prefix `groupA.*`... tránh conflict `vi.ts` — nếu trùng file, merge chấp nhận xung đột nhỏ, resolve khi merge); (2) KHÔNG sửa DESIGN.md/DESIGN-IDENTITY.md/standard.md/token (đã chốt Phase 0 — muốn đổi → ghi decision log, phase sau); (3) mỗi nhóm tự chạy vòng Ollama (chụp light+dark, 7 tiêu chí + mục spacing + câu hỏi "chung chung hay đặc trưng"); (4) ưu tiên thư viện: tối đa 1-2 thư viện mới/nhóm, lazy-load, `@vue-flow/core` CHỈ nhóm B (signature) — nhóm khác không cài; (5) không đụng backend/DB; (6) nhóm xong trước → merge dev trước, nhóm sau rebase (file view tách biệt nên conflict tối thiểu).

**PHA 2 — 1 session tổng hợp (serial):** dev-review chốt scorecard toàn bộ (6 trục v1 + trục Đặc trưng), đối chiếu TEST_PLAN, ảnh trước/sau, merge xong → HANDOFF cập nhật.

## CHẨN ĐOÁN — vì sao vẫn "nhựa nhựa AI gen" dù đã đúng chuẩn kỹ thuật

v1 tối ưu trục **hygiene** (spacing đúng token, button đúng padding, a11y đạt điểm, animation phủ đủ 36/36 view) — đây là điều kiện CẦN nhưng không ĐỦ. Một UI đạt 90/100 theo mọi tiêu chí v1 vẫn có thể là "SaaS demo bất kỳ" vì:

1. **Animation cơ giới**: yêu cầu "animation ở TẤT CẢ view" bị hiểu thành fade+slide-up giống hệt nhau lặp lại trên mọi card/mọi phần tử → đây chính là tell rõ nhất của giao diện AI-gen, vì không có nhịp điệu, không có khoảnh khắc nào được đầu tư hơn khoảnh khắc nào.
2. **Bản sắc bị trừu tượng hoá**: màu/font chọn kiểu "1 accent teal + Geist font" — đúng nhưng không bắt nguồn từ *chính nội dung sản phẩm*. Đây là app học **cấu trúc dữ liệu** — có sẵn cả kho ngôn ngữ thị giác đặc trưng (node/edge, cây, đồ thị, mảng, con trỏ, độ phức tạp Big-O, trạng thái stack/queue) đang bị bỏ phí, thay vào đó dùng công thức dashboard chung.
3. **Không có signature**: không có 1 chi tiết nào khiến người dùng nhớ "chỉ app này mới có vậy" — mọi thứ đều "hợp lý" nhưng không có gì đáng nhớ.
4. **Cliché mặc định**: rơi vào công thức mà AI hay dùng khi không được giao hướng cụ thể — xem kill-list bên dưới.

## KILL-LIST — cấm dùng trừ khi có lý do cụ thể ghi vào decision log

Grep/soát mắt từng view, liệt kê chỗ vi phạm trước khi sửa:

- Nút/CTA dùng gradient 2 màu mặc định (tím→xanh dương, hồng→cam...) không gắn với vai trò/ý nghĩa cụ thể.
- Card đồng loạt `rounded-2xl` + shadow mềm, không phân cấp độ nổi giữa card quan trọng và card phụ (mọi thứ "nổi" bằng nhau = không có gì nổi cả).
- Hero công thức: heading to căn giữa + đoạn mô tả + 2 nút CTA + blob gradient trang trí phía sau.
- Icon emoji (🎯🚀✨) hoặc icon stock chung chung thay vì hệ icon nhất quán đã chốt ở DESIGN.md §4.
- Glassmorphism (`backdrop-blur` trắng mờ) phủ khắp mọi bề mặt thay vì dùng chọn lọc cho 1-2 điểm nhấn.
- Animation fade+slide-up **giống hệt nhau** trên mọi phần tử của mọi view — không phân biệt cái gì đáng được "diễn" kỹ hơn cái gì.
- Công thức stat-card: icon tròn + số to + % tăng màu xanh — dùng ở mọi nơi kể cả khi không có ý nghĩa tường thuật thật.
- Layout 3 cột với vòng tròn số 01/02/03 khi nội dung không thực sự là một quy trình tuần tự.
- Nền be/cream + serif tương phản cao + accent cam đất, HOẶC nền gần đen + 1 accent xanh lá/đỏ chói — 2 trong số các "cụm mặc định" mà mô hình AI hay rơi vào bất kể brief là gì; app học nghiêm túc không tự nhiên khớp với 2 cụm này trừ khi có lý do rõ.

## BƯỚC 0 (mới, chạy trước khi đụng vào `frontend/DESIGN.md`) — Xác lập Design Identity

**Quy trình 2 pha (brainstorm → tự phản biện → mới build), không nhảy thẳng vào code:**

**Pha 1 — Brainstorm bản sắc**, viết vào `frontend/DESIGN-IDENTITY.md` (~1 trang, tham chiếu từ DESIGN.md §1):
- **Ground vào chủ đề thật**: không phải "app giáo dục" chung chung, mà là "nơi người học xây mental model về cây/đồ thị/stack bằng cách thao tác trực quan với chúng". Chọn 1-2 motif thị giác bắt nguồn từ chính DSA (node-edge network làm texture nền có chọn lọc, font mono cho index/độ phức tạp, con trỏ/mũi tên như ngôn ngữ chuyển động, v.v.) — không bắt buộc đúng các ví dụ này, agent tự chọn miễn bắt nguồn từ chủ đề thật.
- **Bảng màu**: 4-6 mã hex có TÊN + VAI TRÒ + LÝ DO gắn với chủ đề (không phải "màu đẹp" chung chung).
- **Typography**: ≥2 vai trò chữ (display có cá tính dùng tiết chế + body trung tính + mono cho dữ liệu/code/độ phức tạp) — không phải font mặc định dùng cho mọi vai trò.
- **Layout concept**: 1 câu mô tả + ASCII wireframe cho 3-4 loại màn hình đại diện (dashboard, luồng học bài, editor/mô phỏng, danh sách).
- **Signature**: 1 chi tiết duy nhất mỗi nhóm actor sẽ nhớ (VD: cách hiển thị trạng thái thuật toán đang chạy, cách stack/queue "thở" khi push/pop — tự nghiên cứu, không sao chép từ Linear/Notion/Raycast, chỉ mượn CẤU TRÚC suy nghĩ như v1 đã làm với DESIGN.md).

**Pha 2 — Tự phản biện trước khi build**: với mỗi phần ở Pha 1, tự hỏi "nếu prompt này đưa cho một AI khác build 1 app dashboard bất kỳ, nó có ra kết quả tương tự không?" — nếu có, phần đó CHƯA đủ đặc trưng, phải sửa và ghi lại đã đổi gì/vì sao vào decision log. Chỉ sau khi pass bước này mới được sửa `DESIGN.md` §1 (Visual Theme) theo Pha 1 đã chốt.

**Kỷ luật tiết chế** (bắt buộc, đi ngược lại bản năng "làm cho đẹp hơn = thêm nhiều hơn"): dồn sự táo bạo vào 1 chỗ — signature element là điểm duy nhất được phép nổi bật mạnh; phần còn lại giữ kỷ luật/im lặng. KHÔNG animate mọi phần tử — chỉ những khoảnh khắc thực sự phục vụ chức năng (xác nhận hành động, dẫn hướng chú ý, thể hiện quan hệ không gian) mới được animate; phần còn lại giữ tĩnh hoặc transition rất nhẹ. Nhiều animation hơn KHÔNG đồng nghĩa đỡ nhựa hơn — thường ngược lại.

## PATCH vào `docs/work/view-quality/standard.md` — thêm trục mới + sửa trục Animation

| Trục | Quy tắc bổ sung/sửa |
|---|---|
| **Đặc trưng/Distinctiveness (trục MỚI, ~10 điểm, cộng vào tổng)** | Với mỗi view: che logo/text thương hiệu đi, hỏi "màn hình này có thể là của bất kỳ app dashboard/SaaS nào khác không?" — nếu câu trả lời là CÓ → 0-3 điểm, PHẢI sửa (thêm/làm rõ motif từ DESIGN-IDENTITY.md). View có ít nhất 1 chi tiết chỉ app này mới có (không phải chỉ đổi màu accent) → 8-10 điểm. |
| **Animation (SỬA lại quy tắc "ở tất cả view")** | Yêu cầu gốc "animation ở TẤT CẢ view" giữ nguyên về PHẠM VI (không view nào bị bỏ hoàn toàn), nhưng SỬA cách áp dụng: mỗi view chọn 1-2 "khoảnh khắc đáng đầu tư" (page-load sequence, hover có chủ đích, reveal khi thao tác đúng dữ liệu) được làm kỹ; phần còn lại dùng transition tối giản/nhất quán. CẤM: cùng 1 cặp fade+translateY lặp lại y hệt trên >80% card trong 1 view — nếu audit thấy vậy, coi là lỗi "animation cơ giới", phải đa dạng hoá hoặc bớt lại. |

## PATCH vào BƯỚC 1.5 — mở rộng thư viện (đúng tinh thần "không giới hạn" nhưng vẫn lazy-load + có lý do)

| Nhu cầu | Thư viện | Vì sao hợp với app này cụ thể |
|---|---|---|
| Minh hoạ/thao tác trực tiếp với cây, đồ thị, linked list dạng node-edge tương tác thật (không phải ảnh minh hoạ tĩnh) | `@vue-flow/core` | Đây là thư viện node-graph, khớp thẳng với chính nội dung app (cây/đồ thị) thay vì chỉ trang trí — có thể dùng làm engine cho cả bài học lẫn 1 phần của signature element ở BƯỚC 0. |
| Mascot/trạng thái tương tác có logic (không chỉ phát 1 đoạn animation cố định như Lottie) | Rive (`@rive-app/canvas` + wrapper Vue cộng đồng) | State machine — hợp để thể hiện trạng thái thuật toán/nhân vật phản ứng theo hành động người học, thay vì animation lặp cố định dễ nhàm. Cân nhắc thay hoặc bổ sung cho dotlottie-vue ở view cần tương tác sâu. |
| Cảm giác cuộn trang mượt, cao cấp ở trang tổng quan/landing nội bộ | `lenis` — **ĐÃ CÓ trong package.json (^1.3.26, đợt G)** — KHÔNG cài lại | Chỉ KÍCH HOẠT dùng có chọn lọc ở 1-2 trang (home/path), không bật toàn site vì có thể ảnh hưởng scroll trong bảng/table dài. |
| Giữ nguyên các mục đã chốt ở v1 | `canvas-confetti` (**ĐÃ CÀI ^1.9.4 — kích hoạt dùng, không cài lại**), `@lottiefiles/dotlottie-vue`, `@formkit/auto-animate`, GSAP `ScrollTrigger`, View Transitions API, TresJS, glassmorphism/gradient/shimmer CSS có chọn lọc | Không đổi — vẫn hợp lệ, áp dụng cùng kỷ luật lazy-load + ghi decision log của v1. **Cảnh báo phình bundle: dotlottie + auto-animate + vue-flow + Rive (nếu thêm) phải lazy-load từng cái — KHÔNG thêm >2 thư viện mới trong 1 session nhóm; ưu tiên cao nhất là `@vue-flow/core` (signature lộ trình), Rive là TÙY CHỌN — nếu không đủ thời gian thì bỏ.** |

Nhắc lại kỷ luật v1: mọi thư viện mới phải lazy-load động, ghi rõ dùng ở view nào/lý do vào `docs/pm-decision-log-viewquality.md`. Thêm thư viện không phải để rải đều 36 view — chỉ nơi phục vụ đúng signature/motif đã chốt ở BƯỚC 0.

## PATCH vào BƯỚC 2 (audit từng view) — thêm 1 câu hỏi bắt buộc

Trước khi chấm điểm hygiene như v1, hỏi trước: **"Xoá phần chữ/logo đi, nhìn thuần bố cục+màu+animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"** — ghi câu trả lời + lý do vào đầu file `audit-<ten-view>.md`, trước khi liệt kê lỗi hygiene.

## PATCH vào VERIFY

- Bổ sung cột "Đặc trưng" (điểm 0-10) vào `scorecard.md`, tổng điểm view giờ có thêm trục này ngoài 6 trục hygiene của v1.
- Sửa prompt gửi `qwen2.5vl:3b` ở vòng Ollama: thêm câu hỏi trực tiếp "màn hình này cảm giác như 1 SaaS/dashboard demo chung chung, hay có gì đặc trưng riêng?" — model trả lời "chung chung" → PHẢI sửa, không được coi là qua vòng chỉ vì điểm spacing/thẩm mỹ ổn.
- Đếm: số card trong 1 view dùng chung 1 kiểu animation y hệt (fade+cùng khoảng cách translate) vượt quá 80% tổng số card trong view đó → 0 view vi phạm.
- `frontend/DESIGN-IDENTITY.md` tồn tại, được `DESIGN.md` §1 tham chiếu, và Pha 2 (tự phản biện) có ghi lại ít nhất 1 lần đã sửa vì "quá chung chung" — nếu Pha 2 không sửa gì cả (pass ngay lần đầu), coi là dấu hiệu chưa phản biện đủ nghiêm, yêu cầu làm lại.

## BẰNG CHỨNG THỰC TẾ (từ bộ 12 screenshot `r2-fixed-01..12` người dùng gửi) — ưu tiên sửa trước, không phải lý thuyết chung nữa

Đối chiếu kill-list ở trên với 12 view thật, các vi phạm sau ĐÃ XÁC NHẬN, xếp vào `docs/work/view-quality/scorecard.md` với mức ưu tiên cao nhất (ảnh hưởng nhiều view cùng lúc):

1. **Banner gradient random theo từng trang, không có hệ thống** — `r2-fixed-01-home` (teal→tím), `r2-fixed-03-lesson` (cam→đỏ, chủ đề Bubble Sort), `r2-fixed-04-simulator` (teal đặc, CŨNG Bubble Sort), `r2-fixed-07-ladder` (hồng đào), `r2-fixed-09-code`/`r2-fixed-10-benchmark` (teal→xanh lá), `r2-fixed-11-leaderboard` (teal→xanh dương→tím), `r2-fixed-12-profile` (teal→xanh dương). Cùng 1 bài Bubble Sort nhưng banner ở Lesson và ở Simulator đã khác hue nhau → xác nhận màu banner hiện tại là ngẫu nhiên/trang trí, không mã hoá ý nghĩa gì. **SỬA**: chọn 1 trong 2 hướng — (a) banner màu chỉ dùng khi có ý nghĩa phân loại thật (VD: mỗi 1 trong 5 nhóm chủ đề ở `r2-fixed-06-path` có 1 màu cố định, mọi view thuộc nhóm đó dùng lại đúng màu đó xuyên suốt), hoặc (b) bỏ hẳn gradient trang trí, dùng surface/border/luminance stacking (đã có ở DESIGN.md §6) để phân cấp thay vì đổi hue mỗi trang.
2. **`r2-fixed-06-path` (Lộ trình) là nội dung dạng graph nhưng render bằng card grid phẳng** — đây là cơ hội signature lớn nhất trong toàn app: dùng chính `@vue-flow/core` (đã đề xuất ở BƯỚC 1.5) để vẽ lộ trình như node nối edge thật, có vị trí hiện tại của người học trên đường đi — thay vì 5 ô vuông trắng giống hệt nhau chỉ khác số thứ tự.
3. **Empty state lặp công thức y hệt, icon chọn không có nghĩa** — `r2-fixed-07-ladder` (icon mảnh ghép), `r2-fixed-10-benchmark` (icon cây kéo — không liên quan "chưa có số liệu benchmark"), `r2-fixed-11-leaderboard` (icon mục tiêu). Cả 3 đều vòng tròn xám + icon + chữ xám. **SỬA**: thiết kế 1 kiểu empty-state riêng của app (gợi ý: dùng chính motif mảng/node — VD 1 mảng rỗng `[ ]` cách điệu) áp dụng thống nhất, icon phải chọn có chủ đích theo ngữ cảnh, không lấy ngẫu nhiên từ icon set.
4. **Emoji dùng thay icon, không nhất quán ngữ nghĩa** — 🎯 xuất hiện ở cả `r2-fixed-03-lesson` ("Sắp xếp cơ bản") và `r2-fixed-06-path` ("Lộ trình học") — 2 ngữ cảnh không liên quan dùng chung 1 emoji. Thay bằng bộ icon đã chốt ở DESIGN.md §4, cấm emoji làm icon chức năng.
5. **Công thức card lặp y hệt cho mọi loại nội dung khác nhau** — node lộ trình (`r2-fixed-06`), info-card mô tả/mục tiêu/hướng dẫn (`r2-fixed-08-lab`), stat-card Level/XP/Streak/Gems/Tim (`r2-fixed-12-profile`) đều dùng chung công thức "trắng + viền teal nhạt + radius lớn + shadow nhẹ" → mọi thứ nổi bằng nhau nên nhìn phẳng, không phân cấp được cái nào quan trọng hơn.
6. **Vùng mô phỏng (`r2-fixed-04-simulator`, `r2-fixed-09-code`) — nền đen + block số màu xanh dương đánh chỉ số — là phần có bản sắc rõ nhất trong cả app.** Đây chính là chất liệu cho BƯỚC 0 (Design Identity) — nên lan toả ngôn ngữ thị giác này (nền tối, block dữ liệu, chỉ số mono) ra các màn khác (VD thành tích/profile dùng lại "block token" thay vì card trắng chung chung), thay vì phát minh phong cách mới từ đầu.

**Bug/lỗi thật cần sửa riêng (không phải gu thẩm mỹ, đưa vào backlog kỹ thuật thông thường, không qua quy trình audit điểm số):**
- `r2-fixed-01-home`: header/nav bị lặp 2 lần chồng nhau (2 dòng "DSA Visual" + Đăng nhập/Đăng ký kế tiếp nhau).
- `r2-fixed-07-ladder`: icon bậc thang hiển thị vỡ glyph/font thay vì icon đúng.
- `r2-fixed-09-code`: còn sót ghi chú dev lộ ra UI người dùng cuối ("* Monaco editor sẽ được bật khi cài gói monaco-editor...").
- `r2-fixed-10-benchmark`: nhãn "Miễn phí tìm (20.4)" đọc không rõ nghĩa, nghi ngờ lỗi copy/label — cần kiểm tra lại nguồn chuỗi.

Việc còn chờ/không chắc → ghi vào `docs/work/view-quality/notes.md`, không tự quyết ngoài DESIGN-IDENTITY.md + patch này. --auto

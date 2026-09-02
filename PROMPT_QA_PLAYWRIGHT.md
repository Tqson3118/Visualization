
# PROMPT — QA Explorer: Audit toàn bộ app DsaVisual bằng MCP Playwright

> Dán toàn bộ file này cho agent dev. Agent có quyền đọc repo, chạy lệnh, dùng MCP Playwright.
> Ngôn ngữ làm việc và báo cáo: **tiếng Việt**.

---

## 0. Bối cảnh và môi trường

- **App**: DsaVisual — nền tảng học Cấu trúc dữ liệu và Giải thuật. Frontend Vue 3 + Vite (thư mục frontend/), backend ASP.NET Core .NET 10 (thư mục backend/).
- **URL test**: {{APP_URL}} — mặc định **http://localhost:5173** (backend :5000, DB dev có seed).
- **Tài khoản seed** (đăng nhập bằng email + mật khẩu):
  - ADMIN: admin@system.local / Admin@123
  - TEACHER: teacher@demo.local / Teacher@123
  - STUDENT: student@demo.local / Student@123
  - TEACHER_PENDING: tự tạo bằng cách đăng ký tài khoản mới, tick "Tôi là giảng viên".
- Nếu app chưa chạy: tự khởi động — frontend chạy "pnpm dev" trong frontend/, backend chạy "dotnet run" trong backend/src/DsaVisual.Api. Nếu không khởi động được, DỪNG và báo chướng ngại, đừng giả lập kết quả.
- Phạm vi dữ liệu: **được phép tạo/sửa/xóa tự do** — DB dev có seed, xóa hỏng cũng được (có seed lại).
- Không được **sửa code**, không thêm dependency. Chỉ được tạo file trong thư mục qa/.

## 1. Vai trò và tư duy bắt buộc

Bạn là **QA khó tính + UX reviewer + người phản biện sản phẩm**, không phải bot bấm cho qua. Với mỗi màn hình, tự hỏi trước khi chấm:

1. **Chức năng**: nút bấm có làm đúng việc nó hứa? Dữ liệu hiển thị có khớp dữ liệu thật (đối chiếu network response) không?
2. **Đồng bộ**: tạo xong có thấy ngay? Sửa/xóa xong danh sách có cập nhật? Tab này thay đổi, tab khác có biết? Store có refetch hay đang cache cũ?
3. **UI**: vỡ layout, chồng chéo, tràn chữ, scroll kỳ, nút trùng chức năng (2 nút làm 1 việc), nút chết, disabled vĩnh viễn, thiếu empty state, thiếu loading.
4. **Logic và flow**: flow này hợp lý không? Nếu mình là học sinh/giáo viên thật thì có bực không? Tính năng này có đáng tồn tại không, hay nên bỏ/gộp? Có bước thừa, thiếu xác nhận, thông báo gây hiểu lầm?
5. **Phân quyền**: role này có nhìn thấy/nhấn được thứ không thuộc quyền? Gõ tay URL trực tiếp thì sao (/admin, /path của lộ trình nháp của người khác...)?
6. **Biên và lỗi người dùng**: input rỗng, siêu dài, emoji, HTML/script, số âm, ngày quá khứ, bấm submit 2 lần liên tiếp, refresh/back giữa chừng form.

**Tư duy con người**: vào màn hình thì NHÌN TRƯỚC (snapshot), liệt kê "màn này có gì, mình có thể làm gì", rồi mới bấm. Không bao giờ đoán selector — luôn dùng ref từ snapshot mới nhất. Không kết luận "không hoạt động" khi chưa đợi loading xong và chưa xem console/network.

## 2. Giai đoạn 1 — Đọc code, dựng bản đồ, xuất TEST PLAN (bắt buộc trước khi mở browser)

### 2.1 Đọc source để hiểu flow (hiểu máy trạng thái và quan hệ dữ liệu, không cần học thuộc từng dòng)
- frontend/src/router/index.ts — bản đồ 32 màn hình + guard role.
- frontend/src/views/, frontend/src/components/ (studio, path, simulator, class...), frontend/src/stores/, frontend/src/api/.
- backend/src/DsaVisual.Api/Controllers/ — FE gọi BE gì, BE có chặn đúng role không.
- backend/src/DsaVisual.Application/Persistence/Entities/Enums.cs — **LearningPathStatus** (Draft → PendingReview → Active / Rejected / ClassOnly) và **LessonStatus** (Draft / PendingReview / Active / Hidden).
- frontend/src/engines/catalog.ts + engines/ — danh mục thuật toán simulator (~44 mục).
- backend/src/DsaVisual.Application/Persistence/Seed/ — dữ liệu mẫu (lộ trình Grokking, khóa học của GV, lớp học, tiến độ).

### 2.2 Xuất qa/FEATURE_MAP.md
Bảng: **Màn hình | Route | Role được phép | Tính năng chính | API liên quan | Ghi chú flow/trạng thái**. Thêm mục riêng cho các "máy trạng thái": lộ trình (Draft → duyệt → Active/Rejected/ClassOnly), bài học, lớp học (gán lộ trình + deadline), hearts/gems/quest, premium/subscription.

### 2.3 Xuất qa/TEST_PLAN.md — ma trận scenario
Tổ chức theo **role x màn hình x trạng thái dữ liệu**. BẮT BUỘC bao gồm các ca dưới đây (chủ app yêu cầu rõ) — ngoài ra tự bổ sung thêm từ hiểu biết của bạn:

**A. Lộ trình học phía học viên (sâu nhất — trọng tâm)**
- Học liên tục nhiều node, hoàn thành bài + quiz → unlock node kế đúng không? Nút "Học tiếp/Bắt đầu học" có trỏ đúng bài chưa hoàn thành không?
- Học dở → refresh, logout/login lại, vào lại từ đầu → tiến độ còn nguyên? Tiếp tục đúng chỗ dở không?
- **GV ẨN (hidden) một node khi SV đang học** → SV đang mở bài đó thấy gì? Trang lộ trình hiển thị ra sao? Tiến độ tính thế nào?
- **Node bị XÓA khi đang học** → SV vào lại lộ trình: crash? node kế có mở? tiến độ % có nhảy loạn?
- **Học xong rồi GV THÊM node mới** → node mới khóa hay mở? % hoàn thành tụt xuống có báo rõ ràng không?
- SV bỏ dở giữa bài (back/refresh) → trạng thái có được lưu không?

**B. Studio giáo viên (soạn lộ trình)**
- Tạo lộ trình mới từ đầu: thêm node, soạn bài (markdown/tiptap), gắn mô phỏng, upload ảnh.
- So sánh khả năng soạn với dữ liệu seed (SeedTeacherCoursesData): GV có tạo được lộ trình tương đương không, hay UI thiếu trường gì?
- **Trạng thái và lưu**: nút "Lưu" trong editor vs nút "Lưu lộ trình" vs autosave nháp — có gây nhầm/lệch trạng thái không? Đổi trạng thái Công khai mà chưa bấm Lưu có tự lưu không?
- **Bấm Công khai → trạng thái PHẢI là "Chờ duyệt" (PendingReview), lộ trình KHÔNG được hiện ở /path cho khách/SV khác** cho tới khi Admin duyệt. Kiểm chứng đúng hành vi này. Kèm: Rejected (GV thấy lý do từ chối? sửa gửi lại được không?), ClassOnly (dùng cho lớp, không cần duyệt — kiểm chứng).
- Lộ trình Draft: SV trong lớp của GV có thấy không (draft gating)? Khách gõ URL trực tiếp thì sao?

**C. Admin**
- Duyệt lộ trình PendingReview → duyệt xong kiểm tra NGAY trên /path (đăng nhập SV/guest) rằng nó xuất hiện.
- Duyệt/từ chối đăng ký giáo viên (TEACHER_PENDING) → tài khoản đó login lại có vào được /studio không?
- Quản lý user: khóa/mở khóa, reset mật khẩu → user bị khóa còn login được không?
- Admin stats/settings: số liệu khớp thực tế không (đếm tay 2–3 con số)? Đổi setting có hiệu lực thật không?

**D. Lớp học**
- GV tạo lớp, lấy mã mời, gán lộ trình + **deadline**.
- **Set deadline trong QUÁ KHỨ** → form chặn hay cho lưu? Lưu rồi SV thấy hiển thị thế nào ("đã trễ"? số ngày âm?), nộp bài sau deadline thì sao?
- SV tham gia lớp bằng mã (thử: sai mã, mã lớp khác, tham gia 2 lần).
- ClassReport: số tiến độ/bài nộp khớp với hoạt động thật đã làm không?
- GV xóa SV khỏi lớp / gỡ gán lộ trình → SV mất tiến độ không?

**E. Feedback**
- SV gửi feedback cho lộ trình → GV thấy trong Studio tab feedback → trả lời → SV nhận câu trả lời ở đâu, có thông báo không?
- Form /help: gửi thật được không hay là form giả? Dữ liệu đi đâu, admin thấy ở đâu?

**F. Simulator (bắt buộc test NÁT)**
- Mở **từng** thuật toán trong engines/catalog.ts (danh sách ~44 mục — liệt kê đủ trong báo cáo): canvas/visualization có render nội dung thật không (không trắng, không treo)? Step next/prev/play/pause/tốc độ chạy không? Console sạch?
- Nhập **dữ liệu tùy chỉnh** (không dùng demo): mảng rỗng, 1 phần tử, phần tử trùng, số âm, số rất lớn → visualization đúng hay lỗi? Nếu chỉ hoạt động với dữ liệu demo thì ghi rõ từng key.
- Guest (chưa login) truy cập /simulator/:key: key nào xem được (demo), key nào bị chặn?
- **Cấu hình simulator** (màn cấu hình mô phỏng): áp dụng CHO TẤT CẢ thuật toán hay từng cái? Phản biện thiết kế: có nên per-algorithm không? Đổi cấu hình có hiệu lực ngay không?

**G. Học liệu khác**
- Code Runner /code/:key: chạy code đúng/sai, lỗi compile, các ngôn ngữ khác nhau.
- Exercise/quiz /exercise/:id: trả lời đúng/sai, hết giờ (nếu có), sai có bị trừ tim không?
- FinalTest /path/:topicId/final-test (route legacy): còn vào được không, hoạt động ra sao?
- CheatSheet; tìm kiếm/filter lộ trình (tiếng Việt có dấu — tìm "quy hoach" có ra "quy hoạch" không?).

**H. Gamification và thanh toán**
- Hearts: làm sai bị trừ tim → hết tim thì bị chặn gì? Mua hồi tim ở /shop bằng gem → gem không đủ thì sao? Mua xong tim có hồi thật không?
- Quests /quests: nhận thưởng — nhận 2 lần được không? Quest có cập nhật sau khi làm việc tương ứng thật không?
- Leaderboard: điểm có khớp hoạt động thực tế không?
- Premium /premium → checkout VietQR: tạo được mã QR không? Sau bước "đã chuyển khoản (giả lập)" trạng thái subscription thế nào? /account/subscription nhất quán với badge user không?

**I. Auth và bảo mật**
- Đăng ký SV mới; quên mật khẩu → flow reset đầy đủ.
- Login sai mật khẩu nhiều lần; cookie hết hạn giữa phiên.
- Guest gõ tay URL role-gated: /studio, /admin/*, /path/:id (lộ trình Draft của người khác), /classes/:id (lớp không thuộc) → bị chặn ở FE **và** BE (network trả 401/403) hay chỉ FE che giao diện?
- Logout rồi bấm Back → có quay vào trang nội bộ được không?

**J. UX sweep toàn màn (làm sau khi xong flow)**
- Mỗi màn hình: nút trùng chức năng, sai chính tả, lẫn EN-VN, empty state, loading, responsive **375px** và **1440px** (browser_resize), contrast, icon sai.
- Mọi modal/drawer: đóng bằng ESC/backdrop có làm mất dữ liệu form không?

Sau khi xuất TEST_PLAN: **DỪNG, trình plan chờ chủ app duyệt.** Chỉ sang giai đoạn 2 khi được duyệt (hoặc được yêu cầu sửa plan).

## 3. Giai đoạn 2 — Thực thi bằng MCP Playwright

Luật thao tác (bắt buộc):
1. Mỗi màn: browser_navigate → browser_snapshot → **nhận xét ngắn "màn này có gì"** → mới tương tác (browser_click, browser_fill_form, browser_type...). DOM thay đổi → snapshot lại trước khi bấm tiếp.
2. Sau mỗi hành động quan trọng: browser_console_messages (lỗi đỏ = nghiêm trọng) + browser_network_requests (bắt 4xx/5xx; browser_network_request để xem response body khi nghi dữ liệu sai).
3. **Mỗi issue chụp màn hình**: browser_take_screenshot → lưu qa/evidence/&lt;ID&gt;-&lt;ten-ngan&gt;.png.
4. Đổi role = logout/login đúng tài khoản. Kiểm tra chéo "tạo ở role A, xem ở role B" là BẮT BUỘC với mọi CRUD.
5. Input biên luôn thử: rỗng, space, hơn 500 ký tự, emoji, &lt;script&gt;alert(1)&lt;/script&gt;, số âm, ngày quá khứ, double-submit (bấm 2 lần liên tiếp), refresh/back giữa chừng.
6. Không dùng browser_evaluate để "lách" UI — chỉ dùng để đọc computed style/DOM khi chứng minh lỗi layout.
7. Ghi phát hiện NGAY khi gặp (không giữ trong đầu): mỗi issue một mục trong qa/FINDINGS.md.

Định dạng mỗi bug trong qa/FINDINGS.md (dùng fence ~~~):

~~~text
### QA-### — [P0|P1|P2|P3] — tên ngắn
- Loại: chức năng | đồng bộ | UI | trùng-dư thừa | phân quyền | UX/logic | nội dung
- Màn/URL:
- Role:
- Bước tái hiện: 1) ... 2) ... 3) ...
- Kỳ vọng vs Thực tế:
- Bằng chứng: qa/evidence/xxx.png (+ trích dòng console lỗi hoặc status code network)
- Suy đoán nguyên nhân (bắt buộc đối chiếu source, nêu file:line):
- Gợi ý fix (1–3 câu):
~~~

Mức độ: P0 = mất dữ liệu / chặn flow chính / crash; P1 = tính năng sai kết quả hoặc hở quyền; P2 = khó dùng / đồng bộ lệch; P3 = polish / thẩm mỹ / chữ.

## 4. Báo cáo cuối — qa/REPORT.md

1. **Tóm tắt điều hành**: tổng số bug theo mức, 5 vấn đề nguy hiểm nhất, kết luận "app dùng được tới đâu".
2. **Bảng bug**: ID | Mức | Màn | Tóm tắt | Loại.
3. **Bảng ~44 thuật toán simulator**: key | render OK/fail | chỉ chạy được với input demo? | console lỗi? | ghi chú.
4. **Phản biện thiết kế** (riêng khỏi bug): top 5–10 điều nên thay đổi và LÝ DO theo góc nhìn học sinh/giáo viên (flow dài vô lý, tính năng nên bỏ/gộp, config sai tầm áp dụng...).
5. **Câu hỏi cần chủ app xác nhận**: chỗ nào agent không dám kết luận là bug (thiếu spec) — ghi rõ "nghi vấn, cần chủ app phán".
6. **Phụ lục**: index ảnh evidence + link qa/FINDINGS.md, qa/FEATURE_MAP.md, qa/TEST_PLAN.md.

## 5. Nguyên tắc chốt

- **Ưu tiên số 1: dùng HẾT mọi tính năng** để trả lời "cái gì hoạt động, cái gì không" — kể cả tính năng nghi là thừa/vô dụng (và phản biện việc nó tồn tại).
- Mọi claim phải có bằng chứng (ảnh + console/network, hoặc code file:line). Không báo cáo cảm tính.
- Không sửa code. Mọi kết luận nguyên nhân là "suy đoán có kiểm chứng" — ghi rõ mức độ chắc chắn.
- Flow nào không test được (thiếu dữ liệu, thiếu quyền): ghi vào mục "Chưa test được — vì sao" trong REPORT, đừng bỏ im.

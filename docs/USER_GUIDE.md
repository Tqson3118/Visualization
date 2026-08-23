# HƯỚNG DẪN SỬ DỤNG (USER_GUIDE)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Hướng dẫn sử dụng |
| Phiên bản | 1.3 |
| Ngày cập nhật | 13/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Thái Quang Sơn |
| Người duyệt | Phạm Ngọc Ái Liên |
| Độc giả | Sinh viên, giảng viên, quản trị viên (ngôn ngữ đời thường, không thuật ngữ kỹ thuật) |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md Phần 17.3.4 (khuôn + mẫu), SRS, SCREEN_MAP |

> ⚠ **TRẠNG THÁI (cập nhật 12/08/2026)**: hướng dẫn này đã được đối chiếu với ứng dụng v2 thật (33 màn hình đã hoàn thiện — `frontend/src/views/`, xem SCREEN_MAP Màn 01-32 + N-1..N-16). Nếu có sai lệch nhỏ về nhãn nút/vị trí so với màn hình, báo nhóm để cập nhật — đặc biệt các mô tả màu sắc, nhãn nút, phím tắt (§7).

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Thái Quang Sơn | Sinh mới từ PRODUCTION_PROMPT.md v2.5 |
| 1.1 | 12/08/2026 | Trần Viết Tâm Phúc | F2b: cập nhật cảnh báo trạng thái — hướng dẫn đã đối chiếu với ứng dụng v2 thật (33 màn, 12/08/2026); bỏ ghi chú "đặc tả UI dự kiến / chờ UI hoàn thiện (tuần 19-20)" |
| 1.2 | 13/08/2026 | Trần Viết Tâm Phúc | GP-T8 (đồng bộ GP-T7 — Premium QR MB Bank): §3.10 viết lại luồng nâng cấp — quét QR chuyển khoản MB Bank (NGUYEN THI NHU HOA · 83863112088386) bằng app ngân hàng, nội dung CK tự động `DSV<userId>T<months>`, chờ 60s rồi bấm "Tôi đã chuyển khoản" → kích hoạt tự động (mô phỏng, không xác minh ngân hàng thật); cập nhật bảng route `/premium` |
| 1.3 | 13/08/2026 | — | Task L (form đăng ký giảng viên): bỏ checkbox "Tôi là giảng viên" → §2.1/§3.1/§4.1 mô tả chọn vai trò **Giảng viên** (segmented) rồi điền form con **Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy** → chờ Admin duyệt → email; §5.1/§5.5 cập nhật tab "Chờ duyệt Teacher" hiển thị thông tin GV trong modal duyệt |

---

# 1. GIỚI THIỆU HỆ THỐNG

## 1.1 Hệ thống này là gì?

DSA-Visual là trang web giúp bạn **học "Cấu trúc dữ liệu và giải thuật" bằng mắt thường**. Thay vì chỉ đọc sách với hình tĩnh, bạn xem giải thuật **chạy từng bước một** trên màn hình: các ô số đổi màu, mũi tên di chuyển, và có lời giải thích tiếng Việt ở mỗi bước.

Hệ thống gồm 3 phần chính:

| Phần | Mô tả |
|---|---|
| **Lộ trình** | Học theo trình tự (Learning Path) chia theo chủ đề; mỗi bài có lý thuyết + mô phỏng động |
| **Luyện tập** | Chuỗi 3 bậc: **Trắc nghiệm → Thực hành kéo thả → Bài tập lập trình** |
| **Theo dõi** | Hồ sơ cá nhân: điểm, tim, gems, chuỗi ngày học, thành tích |

## 1.2 Ai dùng hệ thống?

- **Sinh viên (Người học)**: học bài, xem mô phỏng, làm bài tập, xem tiến độ.
- **Giảng viên (Người dạy)**: soạn bài học, tạo bài tập, quản lý lớp, xem báo cáo.
- **Quản trị viên**: quản lý tài khoản, cấu hình hệ thống, thống kê.

## 1.3 Thuật ngữ đơn giản

| Bạn nghe thấy | Nghĩa là |
|---|---|
| Node (nút lộ trình) | 1 bài học trong lộ trình (kèm 3 bậc luyện tập) |
| Bậc 1/2/3 | Trắc nghiệm / Thực hành kéo thả / Lập trình |
| Tim (❤) | "Năng lượng" để mở bài mới; hồi lại theo thời gian |
| Gems (💎) | Điểm thưởng dùng để mua vật phẩm trang trí, gợi ý... |
| Streak (🔥) | Số ngày liên tục bạn học |

---

# 2. BẮT ĐẦU NHANH (TRONG 10 PHÚT ĐẦU TIÊN)

Theo các bước sau, bạn sẽ hoàn thành vòng học đầu tiên:

1. **Tạo tài khoản**: Vào trang chủ → bấm **"Đăng ký miễn phí"** → nhập tên, email, mật khẩu → bấm **"Đăng ký"**. Hệ thống tự đăng nhập và đưa bạn về trang chủ.
2. **Chọn lộ trình**: Vào mục **"Lộ trình"** (biểu tượng 🎯) → chọn lộ trình **"Sắp xếp & Tìm kiếm"** → bạn thấy bản đồ các bài học dạng đường mòn. Bấm node đầu tiên "Bubble Sort".
3. **Đọc lý thuyết**: Trang bài học hiện nội dung lý thuyết. Đọc xong, bấm thẻ **"Mô phỏng"**.
4. **Xem mô phỏng đầu tiên**: Bấm nút **Phát (▶)** và quan sát: ô vàng = đang so sánh, ô đỏ = đang hoán đổi, ô xanh = đã xong. Bấm **Tạm dừng (⏸)** để xem kỹ một bước.
5. **Thử với dữ liệu của riêng bạn**: Bấm **"Cấu hình lại"** → nhập dãy số của bạn → **"Áp dụng"** → bấm Phát.
6. **Làm bài tập**: Quay lại trang bài học → bấm **"Luyện tập"** → làm trắc nghiệm → **"Nộp bài"** → xem điểm và lời giải thích.
7. **Xem tiến độ**: Vào **"Hồ sơ"** (👤) → tab **"Tiến độ"** để xem bạn đã học được bao nhiêu.

> **Nếu gặp lỗi**: mọi thông báo lỗi đều màu đỏ và có hướng dẫn xử lý; cần trợ giúp thêm, vào mục **"Trợ giúp"** ở cuối trang.

## 2.1 Bắt đầu nhanh cho giảng viên (5 phút)

1. Đăng ký với vai trò **Giảng viên** (chọn ô "Giảng viên" → điền **Khoa/Bộ môn**, **Mã giảng viên**, **Kinh nghiệm giảng dạy**) → chờ quản trị viên duyệt (mở mục "Người dùng" → "Chờ duyệt Teacher" nếu bạn là admin). Bạn nhận email khi được duyệt.
2. **Soạn bài**: "Soạn bài" → "Bài học" → "Tạo mới" → điền lý thuyết → gắn mô phỏng có sẵn → "Kích hoạt".
3. **Tạo lớp**: "Lớp học" → "Tạo lớp mới" → gửi mã mời 6 ký tự cho sinh viên.
4. **Gán bài tập + hạn nộp**: mở lớp → tab "Lộ trình đã gán" → chọn bài học/bài tập + ngày hạn.
5. **Xem kết quả**: "Báo cáo" hoặc tab "Báo cáo" của lớp → xuất CSV nếu cần.

---

# 3. HƯỚNG DẪN SINH VIÊN

## 3.1 Tạo tài khoản

1. Vào trang chủ, bấm **"Đăng ký miễn phí"**.
2. Điền: **Họ tên** (2-100 ký tự), **Email** (dùng email trường nếu hệ thống yêu cầu domain nội bộ), **Mật khẩu** (tối thiểu 8 ký tự, có chữ hoa + số + ký tự đặc biệt — ô nhập hiện checklist sống), **Xác nhận mật khẩu**.
3. Chọn **vai trò đăng ký**: ô **"Sinh viên"** (mặc định) hoặc ô **"Giảng viên"** — chỉ chọn "Giảng viên" khi bạn thực sự là giảng viên.
4. Nếu chọn **"Giảng viên"**, điền thêm 3 mục trong form con: **Khoa/Bộ môn** (VD: Khoa Công nghệ thông tin), **Mã giảng viên** (VD: GV12345), **Kinh nghiệm giảng dạy** (giới thiệu ngắn, tối đa 500 ký tự — có bộ đếm ký tự). Khoa/Bộ môn và Mã giảng viên là bắt buộc.
5. Tích **"Đồng ý chính sách"** (bắt buộc) → bấm **"Đăng ký"**.
6. Thành công:
   - Đăng ký **Sinh viên** → hệ thống tự đăng nhập và đưa bạn về trang chủ.
   - Đăng ký **Giảng viên** → màn hình báo "Tài khoản giảng viên đang chờ duyệt — bạn sẽ nhận email khi được duyệt", bấm **"Về đăng nhập"** để quay lại trang đăng nhập (chưa tự động đăng nhập).

## 3.2 Đăng nhập / Quên mật khẩu

- **Đăng nhập**: email + mật khẩu → bấm **"Đăng nhập"**. Sai mật khẩu 5 lần liên tiếp → tạm khóa 15 phút (để nguội rồi thử lại).
- **Quên mật khẩu**: bấm **"Quên mật khẩu?"** → nhập email → mở email, bấm link đặt lại (hiệu lực 30 phút) → nhập mật khẩu mới → đăng nhập lại.

## 3.3 Học theo lộ trình (Learning Path)

1. Mục **"Lộ trình"** hiển thị danh sách lộ trình (mở khóa tuần tự 1 → 5).
2. Mỗi lộ trình là bản đồ node dạng đường mòn:
   - 🔒 **Khóa**: chưa học tới — phải hoàn thành node trước.
   - ▶ **Đang mở**: bấm để học. Mỗi lần mở node mới, hệ thống **trừ 1 Tim (❤)**. Trong vòng 30 phút, quay lại node này **không tốn tim**.
   - ⭐⭐⭐ **Đã qua**: đã hoàn thành, có thể xem lại miễn phí bất cứ lúc nào.
3. Bấm node đang mở → vào **trang bài học** với 3 tab: **Lý thuyết** / **Luyện tập** / **Bảng tóm tắt** (Cheatsheet).

### 3.3.1 Tim (❤) — hiểu cho đúng

| Tình huống | Có tốn tim không? |
|---|---|
| Mở node mới (chưa học trong 30 phút qua) | Tốn 1 ❤ |
| Quay lại node trong vòng 30 phút | Không tốn |
| Xem lại node đã hoàn thành (⭐) | Không tốn |
| Thử lại bài tập trong 30 phút | Không tốn |
| Xem mô phỏng từ Bảng tóm tắt | Tốn 1 ❤ (như mở từ lộ trình) |
| Chạy so sánh giải thuật (Benchmark) | Không tốn |

- Hết tim → xuất hiện cửa sổ "Hết tim" với đồng hồ đếm ngược. Bạn có thể: chờ hồi tim (30 phút/tim), xem lại bài đã học, làm nhiệm vụ hằng ngày để nhận thêm, hoặc nâng cấp Premium.
- Tài khoản miễn phí có tối đa **10 ❤**; Premium có **30 ❤** và hồi nhanh gấp 3 (10 phút/tim).

## 3.4 Đọc bài học

Trang bài học gồm:
- **Nội dung lý thuyết**: văn bản, hình ảnh, công thức toán, bảng. Có mục lục bên phải.
- **Danh sách mô phỏng**: mỗi mô phỏng là 1 thẻ → bấm mở **trang mô phỏng riêng**.
- **Danh sách bài tập**: thẻ của 3 bậc luyện tập.
- **Nút "Đánh dấu đã học"** ở cuối trang: bấm để hệ thống ghi nhận tiến độ.
- **Ghi chú cá nhân** (biểu tượng bút): soạn ghi chú riêng của bạn, tự lưu sau 1 giây.
- **Đánh giá**: chấm sao 1-5 + nhận xét ngắn (ẩn danh với giảng viên).

## 3.5 Chạy mô phỏng (trang quan trọng nhất)

Trang mô phỏng chia 3 vùng:

| Vùng | Vị trí | Chức năng |
|---|---|---|
| Mã giả | Trái | Các dòng thuật toán; dòng đang chạy được tô vàng |
| Vùng trực quan | Giữa | Hình vẽ cấu trúc dữ liệu đang thay đổi từng bước |
| Giải thích | Phải | Lời giải thích bước hiện tại + giá trị biến (i, j, key...) |

**Thanh điều khiển** (dưới vùng trực quan):

| Nút | Tác dụng |
|---|---|
| ⏮ / ⏭ | Về bước đầu / nhảy tới bước cuối |
| ◀ / ▶ | Lùi 1 bước / tiến 1 bước |
| ▶/⏸ | Phát tự động / Tạm dừng |
| Thanh tiến trình | Kéo thả để nhảy tới bước bất kỳ |
| Tốc độ | 0.25x (chậm) đến 4x (nhanh) |
| Cấu hình lại | Đổi dữ liệu đầu vào → sinh lại mô phỏng |

**Mẹo**: bấm **Tạm dừng** rồi dùng **→** để xem từng bước — cách tốt nhất để hiểu "vì sao" giải thuật làm vậy. Bảng màu (legend) ở góc vùng trực quan giải thích ý nghĩa từng màu: vàng = đang so sánh, cam = phần tử đặc biệt (pivot...), đỏ = đang hoán đổi, xanh = đã xong, đỏ đậm = thao tác lỗi, mờ = ngoài phạm vi xét.

### 3.5.1 Các chế độ thêm

| Chế độ | Cách bật | Ý nghĩa |
|---|---|---|
| Tự thực hành | Nút trên trang mô phỏng | Bạn tự chọn thao tác đúng cho bước tiếp theo; sai có giải thích |
| Điểm dừng | Bấm lề trái dòng mã giả | Mô phỏng chạy tự động tới dòng đó rồi dừng |
| Ngăn xếp đệ quy | Tự hiện với thuật toán đệ quy | Xem các lời gọi hàm đang xếp chồng |
| Kiểm tra nhanh | Cuối mô phỏng | 1-2 câu hỏi nhỏ về chính dữ liệu vừa chạy (không tính điểm) |

### 3.5.2 So sánh tốc độ giải thuật (Benchmark)

Mở mục **"Benchmark"** từ trang mô phỏng: chọn 2 giải thuật trở lên → hệ thống chạy thật với nhiều cỡ dữ liệu (10 → 1000 phần tử) → hiển thị bảng thời gian + biểu đồ so với đường lý thuyết (O(n²), O(n log n)...). Không tốn tim.

## 3.6 Luyện tập 3 bậc (Practice Ladder)

Mỗi node có chuỗi 3 bậc bắt buộc làm tuần tự:

1. **Bậc 1 — Trắc nghiệm**: 5-10 câu hỏi chọn đáp án. Đạt **60% trở lên** mới mở Bậc 2.
   - Mẹo: có nút **"Gợi ý"** (nếu câu hỏi có); xem gợi ý làm giảm điểm câu đó. Nút **"Luyện tập"** để làm thử không tính điểm.
2. **Bậc 2 — Thực hành kéo thả**: tự tay thao tác trên hình vẽ (hoán đổi ô, chèn nút vào cây, bấm đỉnh theo thứ tự duyệt). Hệ thống chấm **trạng thái cuối** + giới hạn số thao tác (hiển thị "Đã dùng x/Y"). Có nút **Hoàn tác** (không giới hạn).
3. **Bậc 3 — Lập trình**: hoàn thiện hàm theo khuôn mẫu cho sẵn (chỉ điền phần thân, không được đổi tên hàm). Nhấn **"Chạy thử"** với 3 test mẫu, rồi **"Nộp bài"** — hệ thống chấm bằng bộ test ẩn (10-12 test). Đạt **70% trở lên** là qua.
   - Lưu ý: nếu bạn dùng hàm có sẵn của ngôn ngữ (VD `arr.sort()`), bài vẫn được chấm điểm theo kết quả — nhưng sẽ **không xem được mô phỏng từng bước** cho code của bạn.

> Trong vòng 30 phút sau khi mở node, làm lại các bậc **không tốn tim**.

## 3.7 Bài kiểm tra cuối lộ trình

Khi hoàn thành toàn bộ node của một lộ trình, mở **"Kiểm tra cuối lộ trình"**: đề trộn câu hỏi trắc nghiệm + dự đoán bước từ các bài đã học. Đạt **70% trở lên** → nhận huy hiệu hoàn thành + mở lộ trình kế tiếp.

## 3.8 Điểm, sao và thành tích

| Mốc | Điều kiện |
|---|---|
| ⭐ 1 sao | Hoàn thành đủ 3 bậc của node |
| ⭐⭐ 2 sao | Điểm tổng node ≥ 75% |
| ⭐⭐⭐ 3 sao | Điểm tổng node ≥ 90% |
| Nâng sao | Thưởng gems (1→2⭐: +3💎, 2→3⭐: +5💎) — mỗi mốc 1 lần |

- **Điểm node** = Trắc nghiệm 20% + Thực hành 30% + Lập trình 50% (lấy điểm cao nhất mỗi bậc).
- **Hồ sơ → Thành tích**: xem huy hiệu đã mở (VD: "Người mới", "Nhà sắp xếp", "Chuỗi 7 ngày học"...).

## 3.9 Gems, Cửa hàng, Nhiệm vụ hằng ngày, Bảng xếp hạng

- **Kiếm gems (💎)**: hoàn thành node (+10), 3 sao (+5), nâng sao (+3/+5), nhiệm vụ hằng ngày (+2-5), hoàn thành đủ 5 nhiệm vụ trong ngày (+10), huy hiệu (+10-50).
- **Cửa hàng (Shop)**: mua gợi ý (Hint token), giữ chuỗi ngày (Streak Freeze), khung đại diện, chủ đề giao diện, tăng XP... Mỗi vật phẩm có giới hạn số lượng.
- **Nhiệm vụ hằng ngày (Quest)**: 5 nhiệm vụ mới mỗi ngày (2 dễ + 2 trung bình + 1 khó). Tiến độ tự cập nhật khi bạn học; nhớ bấm **"Nhận thưởng"**. Hết ngày (0 giờ) nhiệm vụ chưa nhận sẽ reset.
- **Chuỗi ngày học (Streak 🔥)**: học ít nhất 1 hoạt động mỗi ngày để giữ chuỗi. Có vật phẩm "Streak Freeze" giữ chuỗi khi bạn nghỉ 1 ngày.
- **Bảng xếp hạng**: tab Tuần (reset thứ Hai), tab Level, tab Lớp (chỉ hiện khi bạn đã tham gia lớp).

## 3.10 Premium (nâng cấp)

Premium mở khóa: **30 ❤** (thay vì 10), hồi tim nhanh gấp 3, gợi ý nâng cao, khung đại diện VIP, tải **Cheatsheet PDF**, benchmark nâng cao. Cách nâng cấp:

1. Bấm **"Nâng cấp Premium"** → chọn gói 1/3/12 tháng.
2. Màn hình hiện **mã QR chuyển khoản MB Bank** (chủ tài khoản **NGUYEN THI NHU HOA** · số tài khoản **8386 3112 0883 86**) kèm số tiền theo gói và **nội dung chuyển khoản tự động** dạng `DSV<số tài khoản của bạn>T<số tháng>` (ví dụ `DSV1002T3` — có nút **"Sao chép nội dung CK"**).
3. Mở **app ngân hàng** (MB Bank hoặc bất kỳ app nào hỗ trợ quét QR) → **quét mã QR** (hoặc chuyển khoản thủ công đúng chủ TK/số TK/số tiền/nội dung CK như trên).
4. Chờ đồng hồ đếm ngược **60 giây** (nút bấm sẽ khả dụng sau đó) → bấm **"Tôi đã chuyển khoản"** → quyền lợi Premium **kích hoạt tự động** ngay.

> ⚠ **Lưu ý**: đây là thao tác **trình diễn mô phỏng** — hệ thống **không xác minh giao dịch ngân hàng thật** và **không thu tiền thật**; chỉ cần quét QR + bấm xác nhận để xem luồng demo. Khi hết hạn, hệ thống tự chuyển về gói miễn phí (giữ nguyên gems, avatar, vật phẩm đã mua).

## 3.11 Lớp học phần

- **Tham gia lớp**: mục **"Lớp học"** → **"Nhập mã lớp"** → nhập mã 6 ký tự giảng viên cung cấp.
- Sau khi tham gia: thấy nội dung bắt buộc kèm hạn nộp (nhãn "Bắt buộc · hạn 20/08/2026"); nộp trễ vẫn được nhưng hiển thị "Nộp trễ".

## 3.12 Nhập dữ liệu đầu vào cho từng loại mô phỏng

Cửa sổ "Cấu hình lại" thay đổi theo loại mô phỏng. Quy ước chung: giá trị số hợp lệ từ **-999 đến 999**; nếu nhập sai, lỗi hiện ngay dưới ô nhập với hướng dẫn cụ thể.

### 3.12.1 Mô phỏng trên Mảng (sắp xếp, tìm kiếm)

| Cách nhập | Thao tác |
|---|---|
| Nhập tay | Gõ dãy số cách nhau dấu phẩy, VD: `5,3,8,1,9,2` (cho phép số âm: `-3,7,-1,0,2`) |
| Ngẫu nhiên | Chọn "Ngẫu nhiên" → số lượng (2-100), phạm vi giá trị (min/max), cho phép trùng lặp hay không, kiểu dữ liệu (ngẫu nhiên / tăng dần / giảm dần / gần đã sắp xếp / bằng nhau) |
| Bộ mẫu | Chọn mẫu có sẵn: "Đã sắp xếp", "Ngược chiều", "Bằng nhau", "Trống" |
| Tìm kiếm | Thêm ô "Giá trị cần tìm" (target). Với tìm kiếm nhị phân, dữ liệu chưa sắp xếp sẽ được hệ thống tự sắp xếp kèm thông báo |

### 3.12.2 Mô phỏng Ngăn xếp (Stack) / Hàng đợi (Queue)

- Nhập danh sách thao tác, mỗi dòng 1 thao tác, VD:
  ```
  Push 5
  Push 3
  Pop
  Push 7
  ```
- Chọn dung lượng (1-20). Nếu thao tác tràn/cạn, mô phỏng dừng và hiển thị lỗi bằng màu đỏ đậm.

### 3.12.3 Mô phỏng Danh sách liên kết

- "Giá trị ban đầu" (0-20 giá trị, VD: `4,2,9`).
- Chọn thao tác minh họa: Chèn đầu / Chèn cuối / Chèn vị trí k / Xóa vị trí k / Tìm giá trị / Duyệt toàn bộ.
- Nhập "Giá trị thao tác" và "Vị trí" (khi cần).

### 3.12.4 Mô phỏng Cây (BST / AVL)

- "Dãy khóa": tối đa 31 khóa, không trùng, VD: `50,30,70,20,40,60,80`.
- Chọn thao tác: Chèn / Xóa / Tìm kiếm (BST); với AVL: Chèn (hệ thống tự xoay và giải thích trường hợp LL/RR/LR/RL).

### 3.12.5 Mô phỏng Đống nhị phân (Heap)

- "Dãy khóa" khởi tạo (1-31) + chọn: Heapify / Chèn / Trích xuất max.

### 3.12.6 Mô phỏng Bảng băm

- "Dãy khóa" (2-50), "Kích thước bảng" (5-31 — nên dùng số nguyên tố), "Hàm băm" (chia dư `k mod m` hoặc phương pháp nhân), thao tác Chèn / Tìm / Xóa. Mỗi bước hiển thị phép tính băm, VD: `h(27) = 27 mod 7 = 6`.

### 3.12.7 Mô phỏng Đồ thị (BFS / DFS / Dijkstra)

- Chọn **mẫu có sẵn**: đường đi, chu trình, đồ thị đầy đủ, đồ thị 2 phần, lưới — hoặc **vẽ tay**: thêm đỉnh, thêm cạnh, chỉnh trọng số (1-99). Giới hạn: tối đa 50 đỉnh, 200 cạnh.
- Chọn đỉnh nguồn (số 0-49); với Dijkstra có thể chọn đỉnh đích (mặc định: tính tới mọi đỉnh).

## 3.13 Chia sẻ mô phỏng với bạn bè

Trang mô phỏng có nút **"Chia sẻ"** (🔗): hệ thống tạo liên kết chứa sẵn cấu hình của bạn (VD: dãy số đã nhập). Bạn bè mở liên kết là mô phỏng tự nạp đúng cấu hình. Nút **"Yêu thích"** (★) lưu mô phỏng vào danh sách riêng (xem lại trong Hồ sơ).

## 3.14 Tìm kiếm bài học

Ô tìm kiếm ở đầu trang (biểu tượng kính lúp): gõ tên bài (VD: "bubble", "cây") → gợi ý hiện sau chớp mắt → bấm chọn để mở bài. Tìm không phân biệt chữ hoa/thường và hiểu được tiếng Việt không dấu.

## 3.15 Chế độ tối (Dark Mode)

Hồ sơ → tab Cài đặt → bật **"Chế độ tối"** (hoặc chọn "Theo hệ thống" để tự đổi theo máy tính). Toàn bộ giao diện — kể cả màu sắc của mô phỏng — đổi sang bản tối tương phản.

## 3.16 Tổng quan các trang chính (bạn sẽ thấy gì ở đâu)

| Trang | Đường dẫn (hiển thị) | Bạn làm được gì |
|---|---|---|
| Trang chủ | `/` | Xem giới thiệu + chạy thử 3 mô phỏng mẫu (Bubble Sort, Tìm kiếm nhị phân, BFS) không cần đăng nhập |
| Lộ trình | `/path` | Bản đồ lộ trình; chọn bài để học (học theo trình tự — trừ tim khi mở bài mới) |
| **Khám phá** | `/simulations` | Danh mục toàn bộ mô phỏng (44 mô phỏng) — xem GT/CTDL bất kỳ tự do kiểu VisuAlgo; kèm tab "So sánh" (Benchmark) + tab "CheatSheet" |
| Bài học | `/path/.../node/...` | Đọc lý thuyết, vào Luyện tập, xem Bảng tóm tắt |
| Mô phỏng | `/simulator/...` | Chạy mô phỏng từng bước (trang chính của hệ thống) |
| Luyện tập | `/ladder/...` | 3 bậc: Trắc nghiệm → Kéo thả → Lập trình |
| Lập trình | `/code/...` | Viết code + xem nó chạy trực quan |
| So sánh giải thuật | `/benchmark/...` | Đo thời gian thật của 2+ giải thuật (miễn phí tim) |
| Bảng tóm tắt | `/cheatsheet` | Bảng độ phức tạp (Big-O) mọi giải thuật + mở thẳng mô phỏng |
| Hồ sơ | `/profile` | Tổng quan (level, XP, streak, tim, gems), Tiến độ, Thành tích, Cài đặt |
| Thử thách | `/quests` | 5 nhiệm vụ hằng ngày + nhận thưởng; tab Bảng xếp hạng |
| Cửa hàng | `/shop` | Mua vật phẩm bằng gems |
| Premium | `/premium` | Nâng cấp tài khoản (QR chuyển khoản MB Bank — mô phỏng) |
| Lớp học | `/classes` | Tham gia lớp (mã mời) hoặc quản lý lớp (giảng viên) |
| Trợ giúp | `/help` | Câu hỏi thường gặp + gửi báo lỗi |

> **Mẹo nhanh**: muốn xem 1 giải thuật bất kỳ không cần theo lộ trình → dùng **Khám phá** (mở mô phỏng vẫn tính 1 tim, trừ 3 demo công khai ở trang chủ). Muốn học có trình tự + tính điểm → dùng **Lộ trình**.

## 3.17 Giải thích màu sắc trong mô phỏng (chi tiết)

| Màu | Ý nghĩa | Ví dụ |
|---|---|---|
| Xám nhạt | Chưa xử lý | Ô chưa được so sánh |
| Vàng | Đang thao tác | Hai ô đang được so sánh; con trỏ đang trỏ |
| Cam | Phần tử đặc biệt | Pivot của Quick Sort, vị trí chèn, đỉnh nguồn |
| Đỏ | Đang hoán đổi/thay đổi | Hai ô vừa đổi chỗ |
| Xanh lá | Đã hoàn thành | Ô đã nằm đúng vị trí; đỉnh đã duyệt |
| Đỏ đậm (có icon) | Thao tác bất hợp lệ | Pop ngăn xếp rỗng, xóa vị trí ngoài phạm vi |
| Trắng mờ | Ngoài phạm vi xét | Đoạn mảng đã bỏ qua trong tìm kiếm nhị phân |

Ngoài màu, hệ thống vẽ **con trỏ** bằng mũi tên có nhãn (`i=2`, `low=3`, `top`) và hiển thị **biểu thức so sánh thật** (VD: `a[2]=7 > a[3]=4?`) ngay trên hình vẽ.

## 3.18 Mất kết nối mạng giữa chừng

- Khi mất mạng, đầu trang hiện dải thông báo **"Mất kết nối, đang thử lại..."**.
- **Ghi chú bài học** và **bài làm dở** được lưu tạm trên máy của bạn; khi có mạng trở lại, hệ thống tự đồng bộ.
- Tránh tắt trang ngay lúc mất mạng nếu bạn đang nộp bài — hãy đợi thông báo kết quả hoặc thử lại sau.

---

# 4. HƯỚNG DẪN GIẢNG VIÊN

## 4.1 Tài khoản giảng viên

Đăng ký với vai trò **Giảng viên** (chọn ô "Giảng viên" trong form đăng ký → điền **Khoa/Bộ môn**, **Mã giảng viên**, **Kinh nghiệm giảng dạy**) → chờ quản trị viên duyệt; bạn nhận email khi được duyệt. Sau khi duyệt, bạn có quyền: soạn bài học, tạo bài tập, quản lý lớp, xem báo cáo.

## 4.2 Tạo bài học

1. Menu **"Soạn bài"** → **"Bài học"** → **"Tạo mới"**.
2. Điền: chủ đề, tiêu đề, mô tả, **nội dung lý thuyết** (soạn thảo văn bản phong phú — có thể chèn hình ảnh, công thức).
3. Tab **"Mô phỏng"**: chọn mô phỏng từ danh mục có sẵn (VD: "Sắp xếp nổi bọt") + đặt dữ liệu mặc định.
4. Tab **"Bài tập"**: gắn bài tập hoặc tạo mới.
5. **"Lưu nháp"** để chỉnh tiếp; **"Kích hoạt"** để sinh viên thấy bài.

## 4.3 Tạo bài tập và câu hỏi

1. **"Soạn bài" → "Bài tập" → "Tạo mới"**: chọn loại:
   - **Trắc nghiệm**: nhập 3-20 câu; mỗi câu chọn loại (chọn 1 / chọn nhiều / đúng-sai), 2-6 phương án, đánh dấu đáp án đúng, nhập giải thích (bắt buộc — sinh viên xem sau khi nộp). Có thể nhập **giải thích riêng cho từng phương án sai**, 0-3 mức **gợi ý**.
   - **Thực hành kéo thả (Lab)**: chọn kịch bản (Sắp xếp / Cây / Đồ thị) + dữ liệu cố định.
   - **Lập trình (Code)**: nhập đề bài, khai báo tên hàm cố định (VD `bubbleSort(arr)`), bộ test ẩn (nhập `input → expected`; 3 test công khai + 10-12 test ẩn).
2. **Nhập hàng loạt từ CSV**: tải file mẫu (10 cột) → điền câu hỏi → tải lên. Dòng lỗi được báo theo số dòng, dòng lỗi không được tạo.

> Lưu ý: **đáp án và test ẩn không hiển thị với sinh viên** ở trang làm bài (test ẩn đóng gói trong ứng dụng nhằm chống chép lười — không phải cam kết chống trích xuất kỹ thuật).

## 4.4 Gán bài tập vào lộ trình (Practice Ladder)

Mỗi node của lộ trình gồm đúng 3 bài tập: 1 Quiz (Bậc 1), 1 Lab (Bậc 2), 1 Code (Bậc 3). Soạn đủ 3 bài và gắn đúng bậc — sinh viên phải làm tuần tự.

## 4.5 Quản lý lớp học phần

1. **"Lớp học" → "Tạo lớp mới"**: nhập tên, học kỳ, mô tả → hệ thống sinh **mã mời 6 ký tự**.
2. Tab **"Thành viên"**: xem danh sách sinh viên, thêm bằng email, xóa khỏi lớp.
3. Tab **"Lộ trình đã gán"**: gán bài học/bài tập bắt buộc + hạn nộp cho lớp.
4. Tab **"Báo cáo"**: % hoàn thành, điểm trung bình từng bài tập, danh sách sinh viên chậm trễ; nút **"Xuất CSV"** (mở được bằng Excel).

## 4.6 Xem báo cáo giảng dạy

- Mục **"Báo cáo"**: chọn bài học → xem số người đã xem, % hoàn thành, điểm trung bình, danh sách sinh viên chưa học → **Xuất CSV**.
- Bạn chỉ xem được dữ liệu của các bài học do chính mình tạo.

## 4.7 Hướng dẫn chi tiết: tạo Lab và Code Challenge

### 4.7.1 Tạo bài thực hành kéo thả (Lab — Bậc 2)

1. "Soạn bài" → "Bài tập" → "Tạo mới" → loại **"Thực hành kéo thả"**.
2. Chọn **kịch bản**:
   - **Sắp xếp**: người học chọn 2 ô liền kề → "Hoán đổi" (hoặc kéo ô này lên ô kia). Dữ liệu mặc định theo bài.
   - **Cây BST**: người học bấm nút cha → "Chèn trái/phải" để chèn từng khóa theo đúng quan hệ.
   - **Đồ thị**: người học bấm đỉnh theo đúng thứ tự duyệt (BFS/DFS) hoặc bấm cạnh để relax (Dijkstra).
3. Hệ thống tự tính **giới hạn thao tác** = số bước chuẩn × 1.5 (không cần nhập).
4. Chấm điểm: tự động so **trạng thái cuối** với kết quả chuẩn — không cần cấu hình thêm.

### 4.7.2 Tạo bài tập lập trình (Code Challenge — Bậc 3)

1. "Soạn bài" → "Bài tập" → "Tạo mới" → loại **"Lập trình"**.
2. Nhập **đề bài** (tiếng Việt, mô tả rõ đầu vào/đầu ra).
3. Khai báo **tên hàm cố định** — sinh viên PHẢI dùng đúng tên này. Quy ước theo bảng chuẩn của hệ thống:

| Bài | Hàm |
|---|---|
| Bubble Sort | `bubbleSort(arr: number[]): number[]` (trả mảng MỚI, không sửa arr gốc) |
| Binary Search | `binarySearch(sortedArr, target): number` (mảng tăng dần) |
| Stack | `stackOps(ops: string[]): number[]` (pop/peek rỗng → null) |
| Linked List | `listOps(ops: string[]): number[]` |
| BST | `bstInsert(root, key): Node` + `bstSearch(root, key): boolean` |
| AVL | `avlInsert(root, value): Node` (chấm: BST đúng + |balance| ≤ 1 + inorder đúng) |
| Hash Table | `hashSearch(table: number[][], key): boolean` |
| BFS | `bfs(graph: number[][], start): number[]` |

4. Nhập **3 test công khai** (sinh viên chạy thử được) + **10-12 test ẩn** (mỗi test: `input → expected`). Bộ test ẩn KHÔNG hiển thị cho sinh viên.
5. Quy tắc chấm: **so khớp kết quả** — sinh viên viết cách khác (kể cả dùng hàm có sẵn) vẫn đạt nếu kết quả đúng. Đạt ≥ 70% test ẩn là qua bậc.

### 4.7.3 Mẫu file CSV nhập câu hỏi (10 cột)

```
content,type,options,answer,explanation,points,hint1,hint2,hint3,wrongExplanations
"Sau vòng đầu của bubble sort, phần tử lớn nhất nằm ở đâu?",SINGLE,"Đầu mảng;Cuối mảng;Giữa mảng",1,"Bubble sort đưa phần tử lớn nhất về cuối.",2,"Xem lại vòng lặp trong","",,""
```

- `type`: SINGLE / MULTI / BOOLEAN; `options`: phân cách `;`; `answer`: chỉ số phương án đúng (0-based), MULTI dùng `0;2`; `wrongExplanations`: giải thích từng phương án sai, phân cách `;` (có thể để trống).
- Tải file lên → hệ thống báo lỗi theo từng dòng nếu có; dòng lỗi không được tạo.

---

# 5. HƯỚNG DẪN QUẢN TRỊ VIÊN

## 5.1 Quản lý người dùng

1. Menu **"Người dùng"**: danh sách phân trang, lọc theo vai trò/trạng thái, tìm theo tên/email.
2. Thao tác trên mỗi người dùng: **Khóa/Mở khóa** (người bị khóa không đăng nhập được), **Đặt lại mật khẩu**, **Đổi vai trò** (Student ↔ Teacher; không đổi Admin). Chỉ **Admin chính** mới có nút thao tác trên tài khoản Admin khác; Admin thường không khóa/đổi vai trò/xóa/đặt lại mật khẩu được Admin khác; không thể khóa/xóa Admin cuối cùng còn hoạt động.
3. Tab **"Chờ duyệt Teacher"**: danh sách giảng viên đăng ký chờ duyệt (kèm thông tin **Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy** hiển thị trong modal) → bấm **Duyệt** hoặc **Từ chối** (nhập lý do). Từ chối → tài khoản về vai trò Sinh viên, vẫn đăng nhập bình thường.

## 5.2 Cấu hình hệ thống

Menu **"Cấu hình"**: tên hệ thống, danh sách domain email được phép đăng ký, chính sách mật khẩu, giới hạn upload. Thay đổi áp dụng ngay.

## 5.3 Thống kê

Menu **"Thống kê"**: tổng người dùng (theo vai trò), người dùng hoạt động 7/30 ngày, số bài học/bài tập, số phiên mô phỏng, biểu đồ truy cập 30 ngày.

## 5.4 Báo cáo lỗi từ người dùng

Menu **"Báo lỗi"** (Admin): danh sách báo cáo lỗi của người dùng kèm ngữ cảnh (trang gặp lỗi, trình duyệt) → cập nhật trạng thái: mới → đang xử lý → đã xử lý → đóng.

## 5.5 Vận hành hằng ngày (không cần kỹ thuật)

1. **Buổi sáng (2 phút)**: mở Trang chủ → xem hệ thống hoạt động bình thường (trang tải nhanh, không báo lỗi).
2. **Khi có báo lỗi mới** (mục "Báo lỗi"): đọc nội dung + ngữ cảnh → gán trạng thái "Đang xử lý" → chuyển cho thành viên phụ trách.
3. **Phê duyệt giảng viên mới** (tab "Chờ duyệt Teacher"): kiểm tra email/tên + thông tin giảng viên (Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy) → Duyệt hoặc Từ chối.
4. **Cuối tuần (10 phút)**: xem Thống kê → ghi nhận số người dùng hoạt động; kiểm tra có ai bị khóa nhầm không (log đăng nhập thất bại nhiều).

## 5.6 Xử lý sự cố thường gặp (dành cho quản trị viên)

| Triệu chứng | Cách xử lý nhanh |
|---|---|
| Người dùng báo "không đăng nhập được" | Kiểm tra tài khoản có bị khóa (mục Người dùng); kiểm tra thông báo lỗi người dùng gửi (bị khóa 15 phút do sai mật khẩu nhiều lần là bình thường) |
| Người dùng báo "không mở được bài" | Kiểm tra bài học còn trạng thái "Kích hoạt" không; node có bị khóa do chưa hoàn thành node trước không; tài khoản còn tim không |
| "Mô phỏng chạy chậm" | Dữ liệu có thể quá lớn (mảng > 100 số); hướng dẫn giảm kích thước dữ liệu; kiểm tra trình duyệt đã cập nhật |
| "Tôi không thấy bài mình tạo" | Kiểm tra bài đã "Kích hoạt" chưa (trạng thái Nháp/Ẩn không hiển thị với sinh viên); kiểm tra đúng chủ đề |
| "Email không nhận được" | Kiểm tra cấu hình email (tham khảo tài liệu triển khai); trong môi trường thử nghiệm link khôi phục hiển thị trong nhật ký hệ thống |

---

# 6. CÂU HỎI THƯỜNG GẶP (FAQ)

**Q1. Tôi bị "Hết tim" giữa buổi học, phải làm sao?**
Chờ hồi (30 phút/tim — xem đồng hồ đếm ngược), hoặc làm nhiệm vụ hằng ngày để nhận thêm, hoặc nâng cấp Premium. Xem lại bài đã học không tốn tim.

**Q2. Tôi bấm Phát nhưng mô phỏng không chạy?**
Kiểm tra: dữ liệu đầu vào hợp lệ chưa (mỗi loại mô phỏng có giới hạn, VD mảng tối đa 100 số); màn hình đang sinh bước (chờ giây lát). Nếu vẫn lỗi, bấm "Cấu hình lại" → "Đặt lại mặc định".

**Q3. Tôi quên mật khẩu?**
Trang đăng nhập → "Quên mật khẩu?" → làm theo email. Link có hiệu lực 30 phút.

**Q4. Bài tập chấm điểm sai?**
Kiểm tra lại: câu chọn nhiều phải chọn đúng VÀ ĐỦ toàn bộ phương án đúng mới được điểm; xem gợi ý làm giảm điểm câu. Nếu vẫn nghi sai, gửi báo lỗi (nút "Báo lỗi" ở trang trợ giúp).

**Q5. Tôi có thể mở lại mô phỏng từ Bảng tóm tắt (Cheatsheet) không?**
Được — nhưng nó tốn 1 tim như mở từ lộ trình (trừ khi node đã hoàn thành).

**Q6. Nộp bài lập trình nhưng không xem được hoạt ảnh?**
Nếu code dùng hàm có sẵn (VD `sort()`), hệ thống không thể minh họa từng bước — bài vẫn được chấm điểm theo kết quả.

**Q7. Tôi muốn góp ý hoặc báo lỗi?**
Trang "Trợ giúp" → form liên hệ/báo lỗi. Với bài học, dùng nút đánh giá sao ở cuối trang.

**Q8. Làm sao để xem học viên nào chưa học trong lớp của tôi?**
Mở lớp → tab "Báo cáo": danh sách sinh viên chậm trễ được sắp xếp theo mức độ; xuất CSV để lưu lại.

**Q9. Tôi nhập dãy số có dấu phẩy tiếng Việt (3,5) được không?**
Không — dấu phẩy dùng để phân cách các số (VD: `3,5,8` nghĩa là 3 số: 3, 5, 8). Số thập phân không được hỗ trợ trong dữ liệu mô phỏng (chỉ số nguyên).

**Q10. Xem mô phỏng có cần cài phần mềm không?**
Không. Chỉ cần trình duyệt web hiện đại (Chrome/Edge/Firefox) và kết nối mạng. Màn hình nên rộng từ 1024px trở lên để xem mô phỏng thoải mái.

**Q11. Tôi có thể học trên điện thoại không?**
Trang hiển thị cơ bản được trên máy tính bảng; trên điện thoại nhỏ hệ thống sẽ cảnh báo nên dùng màn hình lớn cho mô phỏng (phiên bản di động đầy đủ là hướng phát triển tương lai).

**Q12. Điểm bài tập ảnh hưởng điểm môn học không?**
Hệ thống là công cụ hỗ trợ học tập; điểm trong hệ thống phục vụ theo dõi tiến độ cá nhân và báo cáo cho giảng viên của bạn. Việc quy đổi sang điểm môn do giảng viên quyết định.

**Q13. Tôi muốn xóa tài khoản của mình?**
Hồ sơ → tab Cài đặt → "Xóa tài khoản" (liên hệ quản trị viên nếu cần). Theo chính sách bảo mật (Nghị định 13/2023/NĐ-CP), dữ liệu cá nhân sẽ được ẩn danh hóa.

**Q14. "Hoàn tác" trong bài thực hành kéo thả có bị tính vào số bước không?**
Không. Hoàn tác không giới hạn số lần và không tính vào bộ đếm thao tác.

**Q15. Tôi làm bài tập lập trình nộp nhiều lần, điểm tính thế nào?**
Mỗi bậc lấy **điểm cao nhất** trong các lần nộp. Làm lại để cải thiện điểm — trong vòng 30 phút sau khi mở node không tốn tim; ngoài khoảng đó phải mở lại node (tốn 1 tim).

---

# 7. MẸO VÀ PHÍM TẮT

## 7.1 Phím tắt trang mô phỏng

| Phím | Tác dụng |
|---|---|
| `Space` | Phát / Tạm dừng mô phỏng |
| `→` / `←` | Bước tiếp theo / bước trước đó |
| `Home` / `End` | Về bước đầu tiên / nhảy tới bước cuối |
| `[` / `]` | Giảm / tăng tốc độ chạy |
| `C` | Mở hộp thoại cấu hình dữ liệu |
| `F` | Lưu vào mục yêu thích |

## 7.2 Mẹo học hiệu quả

1. **Xem chậm**: chọn tốc độ 0.25x ở lần xem đầu, sau đó tăng dần.
2. **Dừng và dự đoán**: bấm Tạm dừng trước bước quan trọng, tự hỏi "bước tiếp theo sẽ là gì?" rồi so với thực tế.
3. **Tự thực hành**: bật chế độ "Tự thực hành" để kiểm tra chính mình.
4. **Đổi dữ liệu**: chạy cùng giải thuật với dữ liệu xấu nhất (đã sắp xếp ngược) và dữ liệu tốt nhất để thấy sự khác biệt; dùng Benchmark để đo số liệu thật.
5. **Giữ chuỗi ngày**: chỉ cần 1 hoạt động mỗi ngày (mở 1 bài, làm 1 câu hỏi) là giữ được Streak.
6. **Trả lời giải thích bằng lời**: sau khi xem mô phỏng, thử giải thích lại thuật toán bằng lời của bạn — đây là cách kiểm tra hiểu biết tốt nhất.

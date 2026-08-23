# 2. Thuật toán trong đời sống

Ở bài trước bạn đã biết thuật toán là gì. Nhưng có lẽ bạn vẫn tự hỏi: **"Mình có thật sự dùng thuật toán mỗi ngày không?"** Câu trả lời là **có** — rất nhiều lần, chỉ là bạn không để ý. Bài này sẽ giúp bạn "mở mắt" nhìn thấy thuật toán đang chạy xung quanh mình, và hiểu vì sao học thuật toán lại hữu ích đến vậy.

## 2.1. Bạn đã dùng thuật toán từ nhỏ

Hãy nhớ lại những việc bạn làm hàng ngày:

- **Tìm đồ trong nhà:** Bạn rà từng phòng (duyệt tuần tự) → đây chính là **Linear Search**.
- **Tìm trang trong quyển từ điển dày:** Bạn mở giữa, thấy chữ cần tìm nằm sau thì mở về phía sau, nằm trước thì mở về phía trước → đây chính là **Binary Search**.
- **Xếp chồng chén đĩa:** Bạn lấy cái trên cùng trước → **Stack (LIFO)**.
- **Xếp hàng mua vé:** Ai tới trước phục vụ trước → **Queue (FIFO)**.
- **Chia bài toán lớn thành nhỏ:** Làm bài tập dài → tách từng phần → **Chia để trị (Divide & Conquer)**.

<div style="margin: 26px 0;">
  <style>
    @keyframes chipIn { 0% { transform: scale(.8); opacity: 0; } 100% { transform: scale(1); opacity: 1; } }
    .chips { display: flex; flex-wrap: wrap; gap: 10px; }
    .chip { display: flex; align-items: center; gap: 8px; background: rgba(99,102,241,0.12); border: 1px solid rgba(99,102,241,0.35); padding: 8px 14px; border-radius: 999px; font-size: 13px; color: #c7d2fe; animation: chipIn .4s ease both; }
    .chip .life { font-weight: 600; color: #e0e7ff; }
    .chip .alg { color: #818cf8; font-weight: 800; letter-spacing: .02em; }
  </style>
  <div class="chips">
    <div class="chip" style="animation-delay:.05s"><span class="life">Tìm đồ trong nhà</span><span>→</span><span class="alg">Linear Search</span></div>
    <div class="chip" style="animation-delay:.15s"><span class="life">Lật từ điển</span><span>→</span><span class="alg">Binary Search</span></div>
    <div class="chip" style="animation-delay:.25s"><span class="life">Xếp chén đĩa</span><span>→</span><span class="alg">Stack</span></div>
    <div class="chip" style="animation-delay:.35s"><span class="life">Xếp hàng</span><span>→</span><span class="alg">Queue</span></div>
    <div class="chip" style="animation-delay:.45s"><span class="life">Làm bài dài</span><span>→</span><span class="alg">Chia để trị</span></div>
    <div class="chip" style="animation-delay:.55s"><span class="life">Gom đồ vào balo</span><span>→</span><span class="alg">Greedy / Knapsack</span></div>
  </div>
</div>

> 💡 **Ý nghĩa quan trọng:** Bạn đã "biết" các thuật toán này theo bản năng. Việc học chỉ giúp bạn **gọi tên, đo lường, và cải tiến** chúng một cách có hệ thống. Nhận ra thuật toán quanh mình là bước đầu tiên để giỏi giải thuật.

## 2.2. Thuật toán trong công nghệ hiện đại

Những ứng dụng bạn dùng mỗi ngày đều chạy các thuật toán phức tạp bên trong:

<div style="margin: 24px 0;">
  <style>
    .app-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 12px; }
    .app-card { background: rgba(13,12,22,0.6); border: 1px solid rgba(255,255,255,0.08); border-radius: 14px; padding: 16px; transition: transform .2s ease, border-color .2s ease; }
    .app-card:hover { transform: translateY(-3px); border-color: rgba(168,85,247,0.5); }
    .app-card .icon { font-size: 22px; }
    .app-card h4 { margin: 8px 0 4px; font-size: 14px; color: #e9d5ff; }
    .app-card p { margin: 0; font-size: 12.5px; color: #94a3b8; line-height: 1.55; }
    .app-card .algo { display: inline-block; margin-top: 8px; padding: 3px 10px; border-radius: 999px; background: rgba(168,85,247,0.15); color: #c084fc; font-size: 11px; font-weight: 700; }
  </style>
  <div class="app-grid">
    <div class="app-card"><div class="icon">🔍</div><h4>Google Search</h4><p>Trả về hàng triệu trang chỉ trong <b>0.1 giây</b> nhờ bảng băm, cây tìm kiếm, thuật toán xếp hạng PageRank.</p><span class="algo">Hash + Ranking</span></div>
    <div class="app-card"><div class="icon">🗺️</div><h4>Google Maps</h4><p>Tìm đường đi ngắn nhất giữa 2 điểm trên bản đồ hàng chục triệu nút bằng thuật toán đồ thị.</p><span class="algo">Dijkstra / A*</span></div>
    <div class="app-card"><div class="icon">📱</div><h4>Recommendation</h4><p>Shopee/TikTok gợi ý sản phẩm/video dựa trên lịch sử — dùng thuật toán gom cụm và xếp hạng.</p><span class="algo">Clustering</span></div>
    <div class="app-card"><div class="icon">✉️</div><h4>Email Spam</h4><p>Phân loại thư rác/thư thường bằng máy học + bảng băm tra cứu danh sách đen.</p><span class="algo">Classification</span></div>
    <div class="app-card"><div class="icon">🎮</div><h4>Game</h4><p>NPC tìm đường trong bản đồ, vật lý va chạm, cân bằng độ khó — đều là thuật toán.</p><span class="algo">Pathfinding</span></div>
    <div class="app-card"><div class="icon">🏦</div><h4>Ngân hàng</h4><p>Xác thực giao dịch, phát hiện gian lận theo thời gian thực bằng thuật toán xác suất.</p><span class="algo">Anomaly Detection</span></div>
  </div>
</div>

## 2.3. Từ bài toán → thuật toán → chương trình

Khi bạn phát triển phần mềm, quy trình tư duy thường theo 3 bước:

1. **Hiểu bài toán** (problem): Bạn cần máy làm gì? Đầu vào, đầu ra là gì? Ràng buộc thế nào?
2. **Chọn thuật toán** (algorithm): Cách giải nào đúng, nhanh, ít tốn bộ nhớ?
3. **Viết chương trình** (program): Dịch thuật toán sang code bằng ngôn ngữ lựa chọn.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes stagePulse { 0%,100% { box-shadow: 0 0 0 rgba(168,85,247,0); } 50% { box-shadow: 0 0 18px rgba(168,85,247,0.35); } }
    .pipeline { display: flex; flex-direction: column; }
    .stage { display: flex; align-items: center; gap: 12px; padding: 14px 18px; border-bottom: 1px solid rgba(255,255,255,0.06); }
    .stage:last-child { border-bottom: none; }
    .stage .num { width: 30px; height: 30px; border-radius: 50%; background: linear-gradient(135deg, #a855f7, #7c3aed); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 14px; flex-shrink: 0; animation: stagePulse 2.4s infinite; }
    .stage .name { width: 120px; font-weight: 700; color: #e9d5ff; font-size: 14px; flex-shrink: 0; }
    .stage .desc { color: #94a3b8; font-size: 13px; }
  </style>
  <div class="pipeline">
    <div class="stage"><div class="num">1</div><div class="name">Bài toán</div><div class="desc">"Cần sắp xếp 10.000 sinh viên theo điểm số giảm dần"</div></div>
    <div class="stage"><div class="num">2</div><div class="name">Thuật toán</div><div class="desc">Chọn Quick Sort (nhanh, O(n log n)) thay vì Bubble Sort (chậm, O(n²))</div></div>
    <div class="stage"><div class="num">3</div><div class="name">Chương trình</div><div class="desc">Viết hàm <code style="color:#c084fc">quickSort(arr)</code> bằng JavaScript, kiểm tra với dữ liệu mẫu</div></div>
  </div>
</div>

## 2.4. Học thuật toán để làm gì?

Nhiều bạn thắc mắc: *"Mình chỉ muốn làm web/ứng dụng, cần gì học thuật toán?"*. Câu trả lời thực tế:

- **Phỏng vấn kỹ thuật** — đa số công ty lớn đều hỏi thuật toán; không chuẩn bị là trượt.
- **Code chạy nhanh hơn** — chọn đúng thuật toán có thể tăng tốc chương trình **gấp nghìn lần**, quan trọng khi xử lý dữ liệu lớn.
- **Tư duy hệ thống** — học cách phân rã bài toán phức tạp, nhìn ra cấu trúc ẩn, là nền tảng của mọi kỹ năng lập trình nâng cao.

> 💡 **Ví dụ minh họa sức mạnh:** Sắp xếp 1 triệu phần tử. Với cách chạy ~10⁸ phép toán/giây:
> - **Bubble Sort (O(n²)):** ~1.000.000 × 1.000.000 = 10¹² phép toán → **khoảng 10.000 giây (~2,8 giờ)**.
> - **Merge Sort (O(n log n)):** ~1.000.000 × 20 = 2×10⁷ phép toán → **khoảng 0,2 giây**.
>
> Cùng một kết quả, nhưng thuật toán đúng giúp tiết kiệm **~50.000 lần thời gian**!

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Hãy tìm 3 thứ trong đời sống của bạn đang "chạy thuật toán" mà trước đây bạn không để ý, và thử gọi tên thuật toán đó. Chia sẻ với bạn học để kiểm tra xem bạn gọi tên có đúng không.
</div>
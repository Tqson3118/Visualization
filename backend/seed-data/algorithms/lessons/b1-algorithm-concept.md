# 1. Thuật toán là gì?

Khi nghe hai chữ **"thuật toán"**, nhiều người mới học thường hình dung ra thứ gì đó cao siêu, chỉ dành cho các nhà khoa học máy tính. Thực ra, thuật toán (algorithm) chỉ đơn giản là **một chuỗi các bước có thứ tự, rõ ràng, để giải quyết một bài toán cụ thể**. Bạn đã dùng thuật toán cả đời rồi — chỉ là chưa gọi tên nó mà thôi.

> 🍳 **Nói dễ hiểu:** Thuật toán giống như **công thức nấu ăn**. Công thức liệt kê nguyên liệu (đầu vào), các bước làm theo thứ tự (quy trình), và cho ra món ăn (đầu ra). Đổi một bước, bỏ một bước, hoặc làm sai thứ tự → món ăn hỏng.

## 1.1. Năm tính chất bắt buộc

Một quy trình muốn được gọi là "thuật toán" phải thỏa **5 tính chất**:

| Tính chất | Ý nghĩa | Ví dụ khi nấu phở |
|---|---|---|
| **Xác định (Definite)** | Mỗi bước phải rõ ràng, không mơ hồ | "Thêm 2 thìa muối" — không phải "thêm muối vừa đủ" |
| **Hữu hạn (Finite)** | Phải kết thúc sau một số bước | Nấu xong, không ninh mãi mãi |
| **Đầu vào (Input)** | Có dữ liệu đưa vào (có thể bằng 0) | Nguyên liệu: bánh phở, nước dùng, thịt bò |
| **Đầu ra (Output)** | Cho ra kết quả | Tô phở hoàn chỉnh |
| **Hiệu quả (Effective)** | Mỗi bước làm được bằng tay/bằng máy | Bạn có thể bắc nồi lên bếp |

<div style="margin: 30px 0;">
  <style>
    @keyframes flowIn { 0% { transform: translateX(-8px); opacity: 0; } 100% { transform: translateX(0); opacity: 1; } }
    .algo-flow { display: flex; align-items: stretch; gap: 8px; flex-wrap: wrap; }
    .algo-flow .box { flex: 1; min-width: 120px; background: rgba(168,85,247,0.12); border: 2px solid #a855f7; border-radius: 12px; padding: 14px 10px; text-align: center; color: #e9d5ff; font-size: 13px; font-weight: 700; animation: flowIn .6s ease both; }
    .algo-flow .box .tag { display: block; font-size: 11px; color: #c084fc; font-weight: 500; margin-bottom: 4px; letter-spacing: .04em; }
    .algo-flow .arrow { align-self: center; color: #a855f7; font-weight: 900; font-size: 18px; }
  </style>
  <div class="algo-flow">
    <div class="box" style="animation-delay:.05s"><span class="tag">ĐẦU VÀO</span>Dữ liệu</div>
    <div class="arrow">→</div>
    <div class="box" style="animation-delay:.2s"><span class="tag">BƯỚC 1</span>Hiểu bài</div>
    <div class="arrow">→</div>
    <div class="box" style="animation-delay:.35s"><span class="tag">BƯỚC 2</span>Xử lý</div>
    <div class="arrow">→</div>
    <div class="box" style="animation-delay:.5s"><span class="tag">BƯỚC 3</span>Kiểm tra</div>
    <div class="arrow">→</div>
    <div class="box" style="animation-delay:.65s"><span class="tag">ĐẦU RA</span>Kết quả</div>
  </div>
</div>

## 1.2. Thuật toán trong máy tính

Máy tính không "nghĩ" như con người. Nó chỉ thực thi **đúng y hệt** những gì ta viết. Vì vậy, khi viết chương trình, bạn đang làm một việc rất đặc biệt: **dịch một bài toán đời thực thành các bước mà máy hiểu được**, theo một ngôn ngữ lập trình cụ thể.

Hãy xem thuật toán "tìm số lớn nhất trong danh sách" — một bài toán kinh điển:

```javascript
function findMax(arr) {
  let max = arr[0];               // bước 1: giả định phần tử đầu là lớn nhất
  for (let i = 1; i < arr.length; i++) {
    if (arr[i] > max) {           // bước 2: so sánh từng phần tử
      max = arr[i];               // bước 3: cập nhật nếu tìm thấy số lớn hơn
    }
  }
  return max;                     // bước 4: trả về kết quả
}

console.log(findMax([3, 7, 2, 9, 4])); // 9
```

<div style="margin: 26px 0; border: 1px solid rgba(168,85,247,0.25); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes barGrow { 0% { height: 0; opacity: 0; } 100% { height: var(--h); opacity: 1; } }
    .bars { display: flex; align-items: flex-end; gap: 6px; padding: 20px; background: rgba(10,9,18,0.8); }
    .bars .bar { flex: 1; background: linear-gradient(180deg, #c084fc, #7c3aed); border-radius: 4px 4px 0 0; animation: barGrow .5s ease both; display: flex; align-items: flex-end; justify-content: center; color: #e9d5ff; font-size: 12px; font-weight: 700; padding-bottom: 6px; }
    .bars .bar.max { background: linear-gradient(180deg, #f472b6, #a855f7); box-shadow: 0 0 12px rgba(244,114,182,0.5); }
  </style>
  <div class="bars">
    <div class="bar max" style="--h:40px; animation-delay:.1s">3</div>
    <div class="bar" style="--h:70px; animation-delay:.2s">7</div>
    <div class="bar" style="--h:30px; animation-delay:.3s">2</div>
    <div class="bar" style="--h:90px; animation-delay:.4s">9</div>
    <div class="bar" style="--h:50px; animation-delay:.5s">4</div>
  </div>
  <div style="padding: 12px 20px; background: rgba(168,85,247,0.1); color: #c084fc; font-size: 12px; font-weight: 600;">
    Max = 9 — cột cao nhất được tô hồng.
  </div>
</div>

## 1.3. Mã giả (Pseudocode) — ngôn ngữ của thuật toán

Trước khi viết code bằng một ngôn ngữ cụ thể, lập trình viên thường mô tả thuật toán bằng **mã giả**: gần với tiếng người, không ràng buộc cú pháp. Điều này giúp bạn **tập trung vào logic**, tách rời khỏi chi tiết cú pháp.

> **Mã giả "tìm max":**
> ```
> Hàm TimMax(mảng A):
>     max ← A[0]
>     với mỗi phần tử x trong A:
>         nếu x > max thì max ← x
>     trả về max
> ```

Việc thành thạo "mô tả thuật toán bằng mã giả rồi mới code" là một trong những kỹ năng quan trọng nhất khi học giải thuật — và cũng là thứ các vòng phỏng vấn kỹ thuật (interview) hay kiểm tra.

## 1.4. Tại sao có nhiều thuật toán cho cùng một bài toán?

Cùng một bài toán, có thể có **nhiều cách giải khác nhau**, mỗi cách có ưu/nhược riêng. Ví dụ sắp xếp một danh sách số:

- **Cách 1:** Liên tục đổi chỗ hai số cạnh nhau nếu sai thứ tự (Bubble Sort) — dễ viết, nhưng chậm với dữ liệu lớn.
- **Cách 2:** Chia danh sách làm đôi, sắp xếp từng nửa rồi trộn lại (Merge Sort) — nhanh hơn nhiều.
- **Cách 3:** Dùng cấu trúc heap để sắp (Heap Sort) — nhanh và ít tốn bộ nhớ.

Câu hỏi quan trọng không chỉ là "cách này đúng không", mà còn là **"cách nào nhanh hơn, tốn ít tài nguyên hơn"** — đó chính là nội dung của **Độ phức tạp thuật toán** (bạn sẽ học ngay ở Module 2).

> 💡 **Mẹo học:** Khi gặp bất kỳ thuật toán nào trong khóa này, hãy tự hỏi 3 câu: (1) Nó giải bài toán gì? (2) Nó làm theo từng bước ra sao? (3) Nó nhanh/chậm thế nào khi dữ liệu lớn? Trả lời được 3 câu này là bạn đã hiểu thuật toán đó.

## 1.5. Tóm tắt

- Thuật toán = **chuỗi bước có thứ tự, rõ ràng** để giải bài toán.
- 5 tính chất bắt buộc: **xác định, hữu hạn, có đầu vào, có đầu ra, hiệu quả**.
- Mã giả giúp mô tả logic trước khi viết code — kỹ năng cốt lõi khi phỏng vấn.
- Cùng một bài toán có nhiều thuật toán; việc chọn cách tốt nhất dựa trên **độ phức tạp** (Module 2).

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra nhanh:</strong> Mô tả bằng lời (hoặc mã giả) các bước để: rút tiền từ cây ATM. Kiểm tra xem cách mô tả của bạn có đủ 5 tính chất của thuật toán không?
</div>
# 15. Thuật toán tham lam (Greedy)

Bạn đã đi được rất xa. Bài này giới thiệu một **triết lý giải thuật** thay vì một thuật toán cụ thể: **thuật toán tham lam (greedy)**. Ý tưởng đơn giản: **ở mỗi bước, chọn phương án tốt nhất trước mắt, không cần nhìn xa**. Đôi khi cách "ích kỷ" này lại cho kết quả tối ưu toàn cục — và khi đúng, nó cực nhanh.

## 15.1. Ý tưởng cốt lõi

Greedy làm theo 3 nguyên tắc:
1. **Chia nhỏ:** xử lý từng bước một.
2. **Chọn local best:** ở mỗi bước, chọn lựa chọn tốt nhất hiện tại.
3. **Không quay lại:** không hối tiếc, không thay đổi quyết định cũ.

> 🎯 **Nói dễ hiểu:** Greedy giống cách bạn vội vã đi qua một dãy cửa hàng và mua ngay món rẻ nhất đang thấy — thay vì khảo sát hết cả khu phố rồi mới quyết định. Khi "giá cả" đơn giản thì cách này tối ưu; khi phức tạp thì có thể sai.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes greedyStep { 0% { transform: translateX(-6px); opacity: 0; } 100% { transform: translateX(0); opacity: 1; } }
    .greedy-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .greedy-line { display: flex; align-items: center; gap: 6px; margin-bottom: 8px; animation: greedyStep .4s ease both; }
    .greedy-line .lab { width: 110px; font-size: 11px; font-weight: 800; color: #94a3b8; }
    .greedy-line .coin { width: 34px; height: 34px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #000; background: linear-gradient(135deg, #fde68a, #f59e0b); font-size: 12px; box-shadow: 0 2px 6px rgba(0,0,0,0.4); }
    .greedy-line .total { font-size: 12px; color: #a78bfa; font-weight: 700; }
  </style>
  <div class="greedy-stage">
    <div class="greedy-line"><div class="lab">Cần trả 67¢</div><div class="coin">25</div><div class="coin">25</div><div class="coin">10</div><div class="coin">5</div><div class="coin">1</div><div class="coin">1</div><div class="total">= 67 ✓</div></div>
    <div class="greedy-line"><div class="lab">Cần trả 41¢</div><div class="coin">25</div><div class="coin">10</div><div class="coin">5</div><div class="coin">1</div><div class="total">= 41 ✓</div></div>
    <div style="padding-top: 6px; font-size: 12px; color: #94a3b8;">Mỗi bước chọn đồng xu LỚN NHẤT không vượt quá số còn lại. Với hệ xu chuẩn (25,10,5,1), greedy luôn tối ưu.</div>
  </div>
</div>

## 15.2. Ví dụ kinh điển: Coin Change (thối tiền)

**Bài toán:** có các đồng xu mệnh giá {1, 5, 10, 25}, cần thối `amount` xu với **số đồng ít nhất**.

Greedy: mỗi bước lấy **đồng xu lớn nhất ≤ số tiền còn lại**.

```javascript
function coinChange(coins, amount) {
  coins.sort((a, b) => b - a);      // sắp giảm dần
  let remaining = amount;
  let count = 0;
  for (const coin of coins) {
    while (remaining >= coin) {     // dùng tối đa đồng lớn nhất
      remaining -= coin;
      count++;
    }
  }
  return remaining === 0 ? count : -1; // -1 nếu không trả hết được
}

console.log(coinChange([1, 5, 10, 25], 67)); // 6 (25+25+10+5+1+1)
```

## 15.3. Khi nào greedy đúng? Khi nào sai?

Đây là phần quan trọng nhất — greedy **không phải lúc nào cũng tối ưu**:

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    .g-trade { display: flex; gap: 14px; padding: 16px; background: rgba(10,9,18,0.85); flex-wrap: wrap; }
    .g-card { flex: 1; min-width: 240px; border-radius: 12px; padding: 14px; }
    .g-card.ok { background: rgba(34,197,94,0.08); border: 1px solid rgba(34,197,94,0.3); }
    .g-card.bad { background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.3); }
    .g-card h5 { margin: 0 0 6px; font-size: 13px; font-weight: 800; }
    .g-card.ok h5 { color: #4ade80; }
    .g-card.bad h5 { color: #f87171; }
    .g-card p { margin: 0; font-size: 12.5px; color: #94a3b8; line-height: 1.6; }
    .g-card code { color: #e2e8f0; }
  </style>
  <div class="g-trade">
    <div class="g-card ok"><h5>✅ Greedy ĐÚNG</h5><p><b>Coin change hệ xu chuẩn {1,5,10,25}</b>: đồng xu lớn luôn tốt hơn vì mệnh giá "chia hết lẫn nhau". <b>Activity selection</b>: chọn hoạt động kết thúc sớm nhất. <b>Huffman coding</b>: gộp 2 ký tự ít gặp nhất.</p></div>
    <div class="g-card bad"><h5>❌ Greedy SAI</h5><p><b>Coin change hệ {1,3,4}</b> thối 6: greedy lấy 4+1+1 = 3 đồng, nhưng tối ưu là 3+3 = 2 đồng. <b>Knapsack 0/1</b> (mỗi món chỉ lấy 1 lần): chọn món đắt/kg nhất có thể sai. → những bài này cần <b>QHĐ</b> (bài 16).</p></div>
  </div>
</div>

> 💡 **Câu hỏi phỏng vấn hay gặp:** "Khi nào bạn dùng greedy và khi nào dùng QHĐ?" Gợi ý trả lời: greedy khi **lựa chọn local best luôn dẫn tới global best** (bài toán có tính chất "matroid"/"tham lam an toàn"). QHĐ khi phải cân nhắc **nhiều cách kết hợp** và so sánh chúng. Ví dụ đối nghịch kinh điển: **coin change** hệ xu chuẩn → greedy; hệ xu bất kỳ → QHĐ.

## 15.4. Ví dụ đúng nữa: Activity Selection

Chọn tối đa hoạt động không trùng thời gian (phòng họp). Greedy: **luôn chọn hoạt động kết thúc sớm nhất** không trùng với hoạt động đã chọn — và thuật toán này **chứng minh được tối ưu**.

```javascript
function activitySelection(activities) { // [{s, f}] — bắt đầu, kết thúc
  activities.sort((a, b) => a.f - b.f);  // sắp theo thời điểm kết thúc
  const chosen = [];
  let lastEnd = -Infinity;
  for (const act of activities) {
    if (act.s >= lastEnd) {              // không trùng lịch
      chosen.push(act);
      lastEnd = act.f;
    }
  }
  return chosen;
}
```

## 15.5. Độ phức tạp

Greedy thường rất nhanh — thường là **O(n)** sau khi sắp xếp (O(n log n) nếu cần sắp), vì chỉ duyệt qua dữ liệu một lần mà không quay lui, không khám phá không gian lời giải.

## 15.6. Tóm tắt

- Greedy = **chọn local best mỗi bước**, không quay lại.
- **Đúng** cho bài toán có tính "tham lam an toàn" (coin chuẩn, activity selection, Huffman, MST).
- **Sai** khi lựa chọn local phá vỡ tối ưu toàn cục (coin bất kỳ, knapsack 0/1).
- Cực nhanh (O(n)) — nhưng phải **chứng minh/kiểm tra** trước khi dùng.
- Khi greedy sai → nghĩ tới **QHĐ** (bài 16).

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Hệ xu {1, 4, 5}, thối 8. Greedy cho kết quả gì, và tối ưu là gì? Đáp án: greedy = 5+1+1+1 (4 đồng); tối ưu = 4+4 (2 đồng) → greedy SAI cho hệ này.
</div>
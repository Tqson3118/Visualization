# Đề 09 — Gamification: Quests & Leaderboard
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** QuestsView (`/quests`), LeaderboardView (`/leaderboard`)

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** Trong `QuestsView.vue`, `onMounted` gọi những API nào và theo thứ tự nào?

A. `fetchQuests()` + `fetchHearts()` + `fetchStreak()` song song bằng `Promise.all`
B. `fetchQuests()` await xong → `fetchHearts()` + `fetchStreak()` fire-and-forget (void)
C. `fetchHearts()` → `fetchStreak()` → `fetchQuests()` tuần tự
D. Chỉ gọi `fetchQuests()`, hai cái kia gọi khi user bấm nút

---

**Câu 2.** Trong `QuestsView.vue`, hàm `celebrate()` kích hoạt confetti với tham số nào?

A. `{ particleCount: 100, spread: 60, origin: { y: 0.5 } }`
B. `{ particleCount: 90, spread: 70, origin: { y: 0.7 }, disableForReducedMotion: true }`
C. `{ particleCount: 80, spread: 80, origin: { y: 0.6 }, disableForReducedMotion: false }`
D. `{ particleCount: 90, spread: 70, origin: { x: 0.5, y: 0.7 } }`

---

**Câu 3.** Hàm `claim(quest)` trong `QuestsView.vue` kích hoạt confetti khi điều kiện nào?

A. Sau mỗi lần claim bất kỳ quest nào thành công
B. Khi `allDone` computed trả về `true` trước khi claim
C. Sau khi `claimQuest` thành công và **tất cả** quest trong danh sách đều có `claimed = true`
D. Khi `doneCount === gamification.quests.length - 1` (sắp xong)

---

**Câu 4.** Trong `LeaderboardView.vue`, tab nào được fetch ngay khi `onMounted` chạy?

A. Tab `level`
B. Tab `class`
C. Tab `week`
D. Không tab nào — chờ user bấm

---

**Câu 5.** Trong `LeaderboardView.vue`, khi user chọn tab `class` nhưng không tìm thấy lớp nào (`classId = null`), hành vi là gì?

A. Fetch board với `classId = 0` làm fallback
B. Gọi `board.setNoClass()` và return sớm, không gọi `board.fetchBoard`
C. Chuyển về tab `week` tự động
D. Hiện toast lỗi "Bạn chưa tham gia lớp nào"

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

**Câu 6.** Trace luồng khi user vào `/quests` và click **"Claim"** trên quest cuối cùng (quest thứ 5 chưa claim).
Ghi đủ **4 chặng** tên hàm/store-action/API thật từ source.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```

---

**Câu 7.** Trace luồng khi user bấm tab **"Lớp"** trong `LeaderboardView.vue` và đã có lớp.
Ghi đủ **4 chặng** tên hàm/store-action thật từ source.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```

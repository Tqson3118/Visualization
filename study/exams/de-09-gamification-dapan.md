# Đáp Án — Đề 09: Gamification — Quests & Leaderboard

---

## PHẦN I — TRẮC NGHIỆM

### Câu 1 — Đáp án: **B**
**Lý do:** `QuestsView.vue` dòng 27–37:
```ts
onMounted(async () => {
  try {
    await gamification.fetchQuests();        // ← await (blocking)
  } catch {
    ui.showToast(messages.quests.loadError, 'error');
  } finally {
    loading.value = false;
  }
  void gamification.fetchHearts();          // ← fire-and-forget
  void gamification.fetchStreak();          // ← fire-and-forget
});
```
`fetchQuests()` được **await** (ảnh hưởng `loading`), còn `fetchHearts()` và `fetchStreak()` chạy **void** (không chặn).

---

### Câu 2 — Đáp án: **B**
**Lý do:** Dòng 50:
```ts
confetti({ particleCount: 90, spread: 70, origin: { y: 0.7 }, disableForReducedMotion: true });
```
Tham số chính xác: `particleCount: 90`, `spread: 70`, `origin: { y: 0.7 }`, `disableForReducedMotion: true`. Lib: `canvas-confetti`.

---

### Câu 3 — Đáp án: **C**
**Lý do:** Dòng 56–58:
```ts
await gamification.claimQuest(quest.id);
ui.showToast(messages.quests.claimedToast(quest.rewardGems, quest.rewardXp), 'success');
if (gamification.quests.every((q) => q.claimed)) celebrate();
```
`celebrate()` chỉ chạy sau khi `claimQuest` thành công VÀ **mọi quest** (every) đều có `claimed = true`. Đây là moment "claim quest cuối cùng trong ngày".

---

### Câu 4 — Đáp án: **C**
**Lý do:** `LeaderboardView.vue` dòng 40:
```ts
onMounted(async () => {
  void board.fetchBoard('week');   // ← ngay lập tức tab 'week'
  ...
});
```
`fetchBoard('week')` được gọi ngay khi mount. Tab level/class chỉ fetch khi user chủ động chuyển.

---

### Câu 5 — Đáp án: **B**
**Lý do:** `switchTab()` dòng 55–59:
```ts
if (key === 'class') {
  const classId = await resolveClassId();
  if (classId === null) {
    board.setNoClass();
    return;            // return sớm — không gọi fetchBoard
  }
  ...
}
```
Khi `resolveClassId()` trả về `null` → gọi `board.setNoClass()` rồi `return`. Không fetch, không toast riêng, không chuyển tab.

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG

### Câu 6 — Trace claim quest cuối cùng trong QuestsView

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `claim(quest)` (hàm) | User click Button "Claim"; `claimingId.value = quest.id` → nút hiện spinner riêng cho quest đó |
| **2** | `gamification.claimQuest(quest.id)` | Store action gọi `POST /gamification/quests/{id}/claim`; server trả về kết quả claim |
| **3** | `ui.showToast(messages.quests.claimedToast(quest.rewardGems, quest.rewardXp), 'success')` | Toast thành công hiển thị nội dung "+{gems} gems, +{xp} XP" |
| **4** | `gamification.quests.every(q => q.claimed)` → `celebrate()` | Kiểm tra tất cả quest đã claimed; nếu đúng gọi `celebrate()` → `confetti({ particleCount:90, spread:70, origin:{y:0.7}, disableForReducedMotion:true })`; cuối cùng `claimingId.value = null` |

---

### Câu 7 — Trace khi bấm tab "Lớp" trong LeaderboardView (đã có lớp)

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `switchTab('class')` (hàm) | Tabs component emit sự kiện; `switchTab` nhận `key = 'class'` |
| **2** | `resolveClassId()` | Kiểm tra `selectedClassId` → `classStore.currentClass` → `classStore.classes[0]`; nếu có sẵn trả về `classId` ngay, nếu chưa gọi `classStore.fetchClasses()` |
| **3** | `selectedClassId.value = classId` + `board.fetchBoard('class', classId)` | Cập nhật `selectedClassId`; gọi API `GET /leaderboard?tab=class&classId={classId}`; `board.loading = true` |
| **4** | `board.rows` được cập nhật → template render danh sách + phân trang `ChevronLeft/ChevronRight` | `board.totalPages`, `board.tab = 'class'` cập nhật; `valueLabel` computed = `'điểm'`; chart top-10 re-render với màu rank |

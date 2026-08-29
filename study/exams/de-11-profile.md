# Đề 11 — ProfileView & Gamification Store

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm  
**Bao phủ:** ProfileView.vue (5 tab), Auth Store, Gamification Store, MeController, Logout flow

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** ProfileView.vue có bao nhiêu tab chính và tên các tab đó là gì?

- A. 4 tab: overview / progress / achievements / settings
- B. 5 tab: overview / progress / achievements / inventory / settings
- C. 5 tab: overview / stats / achievements / inventory / settings
- D. 6 tab: overview / progress / achievements / inventory / shop / settings

---

**Câu 2.** Khi người dùng bấm "Đăng ký làm giáo viên" tại tab Settings của ProfileView, hệ thống thực hiện gì?

- A. PUT /me/role với body `{ role: 'TEACHER' }`
- B. POST /auth/teacher-request
- C. POST /me/request-teacher → role chuyển thành TEACHER_PENDING
- D. PUT /admin/users/:id/role với body `{ role: 'TEACHER_PENDING' }`

---

**Câu 3.** Trong ProfileView tab Overview, những component nào được hiển thị?

- A. XpCard, HeartCard, QuestCard, AchievementGrid
- B. XpProgressCard, StreakCard, QuestProgressCard, BadgeGrid
- C. XpProgressCard, HeartCard, BadgeGrid, LeaderboardCard
- D. StreakCard, QuestProgressCard, InventoryCard, BadgeGrid

---

**Câu 4.** Tại tab Inventory của ProfileView, hàm `equipItem()` được gọi với tham số nào?

- A. `equipItem(id)` — chỉ truyền id item
- B. `equipItem(id, true)` — luôn là true
- C. `equipItem(id, !isEquipped)` → gọi `gamificationApi.equipItem()`
- D. `equipItem(id, isEquipped)` — truyền trạng thái hiện tại

---

**Câu 5.** Khi logout, authStore.logout() reset bao nhiêu store và kể tên các store đó?

- A. 5 store: gamificationStore, progressStore, lessonStore, classStore, leaderboardStore
- B. 6 store: gamificationStore, progressStore, lessonStore, classStore, leaderboardStore, codeRunnerStore
- C. 7 store: gamificationStore, progressStore, lessonStore, classStore, leaderboardStore, codeRunnerStore, simulationStore
- D. 8 store (thêm cả uiStore)

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

### Câu TL-1 (2.5 điểm)

**Đề bài:** Trace chi tiết toàn bộ luồng **đổi mật khẩu** từ khi người dùng nhập form tới khi server phản hồi. Bao gồm:

1. Tab nào trong ProfileView chứa form đổi mật khẩu?
2. Form gồm những field nào?
3. API endpoint nào được gọi, method gì, controller nào xử lý?
4. Nếu `current` password sai, server trả về gì và UI phản ứng thế nào?
5. Thành công thì authStore có cần làm gì thêm không? (VD: logout, refresh token?)

*(Trả lời dưới dạng danh sách bước hoặc sơ đồ luồng)*

---

### Câu TL-2 (2.5 điểm)

**Đề bài:** Giải thích **chiến lược lưu token** của hệ thống DSA Visual (theo ADR-004) và trace luồng **F5 refresh** (page reload):

1. `accessToken` được lưu ở đâu trong frontend?
2. `refreshToken` được lưu ở đâu và tại sao lại chọn nơi đó?
3. Khi F5, `main.ts` gọi gì? API trả về gì?
4. Khi một request trả về 401, Axios interceptor xử lý thế nào?
5. So sánh ưu/nhược của chiến lược "token in memory" so với "token in localStorage".

*(Trả lời có đề cập đến: Pinia store, HttpOnly cookie, main.ts, GET /auth/refresh)*

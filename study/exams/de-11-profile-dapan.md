# ĐÁP ÁN — Đề 11: ProfileView & Gamification Store

---

## PHẦN I — TRẮC NGHIỆM

| Câu | Đáp án | Giải thích |
|-----|--------|-----------|
| 1 | **B** | ProfileView.vue có đúng 5 tab: `overview / progress / achievements / inventory / settings`. Xem dòng đầu setup script component. |
| 2 | **C** | `POST /me/request-teacher` → Server đổi role thành `TEACHER_PENDING`. Route handler: MeController. Không vào được `/studio` cho đến khi Admin duyệt. |
| 3 | **B** | Tab overview render: `XpProgressCard`, `StreakCard`, `QuestProgressCard`, `BadgeGrid`. Không có HeartCard hay LeaderboardCard ở đây. |
| 4 | **C** | `equipItem(id, !isEquipped)` — toggle trạng thái: nếu đang equipped thì unequip và ngược lại. Sau đó gọi `gamificationApi.equipItem()`. |
| 5 | **C** | 7 store bị reset: `gamificationStore`, `progressStore`, `lessonStore`, `classStore`, `leaderboardStore`, `codeRunnerStore`, `simulationStore`. uiStore không bị reset. |

---

## PHẦN II — TỰ LUẬN

### TL-1: Luồng đổi mật khẩu *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi ý đúng.

**Đáp án chi tiết:**

1. **Tab chứa form:** Tab **`settings`** trong ProfileView (tab cuối cùng trong 5 tab).

2. **Form gồm 2 field:**
   - `current` — mật khẩu hiện tại
   - `next` — mật khẩu mới
   *(Không có field `confirmPassword` vì UX giản lược)*

3. **API call:**
   - Method: `PUT`
   - Endpoint: `/me` (hoặc cụ thể hơn: `/me/password` — tùy implementation)
   - Controller: **`MeController`** (backend ASP.NET Core)
   - Body: `{ current: "...", next: "..." }`

4. **Khi `current` sai:**
   - Server trả về HTTP `400 Bad Request` hoặc `422 Unprocessable Entity` với message lỗi
   - UI: hiện toast error `'Mật khẩu hiện tại không đúng'`, form vẫn mở, không reset

5. **Khi thành công:**
   - authStore **không** cần logout hay refresh token
   - Chỉ cần hiện toast `'success'` và có thể clear form
   - AccessToken vẫn còn hạn, không bị invalidate khi đổi password trong luồng này

---

### TL-2: Chiến lược token & F5 refresh *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi ý đúng.

**Đáp án chi tiết:**

1. **accessToken lưu ở đâu:**
   - Lưu trong **Pinia store (`authStore`)** — tức là **memory** (RAM của tab trình duyệt)
   - **Không** lưu vào localStorage/sessionStorage (ADR-004 cấm vì XSS risk)

2. **refreshToken lưu ở đâu và tại sao:**
   - Lưu trong **HttpOnly cookie** (server set, browser tự gửi kèm mỗi request)
   - **Lý do:** HttpOnly cookie không thể đọc bởi JavaScript → tránh XSS tấn công lấy token
   - Cookie được gửi tự động khi request đến `/auth/refresh`

3. **Khi F5 (page reload):**
   - `main.ts` chạy → gọi `authStore.refresh()`
   - `authStore.refresh()` gọi `GET /auth/refresh`
   - Browser tự đính kèm HttpOnly cookie vào request
   - Server validate refreshToken → trả về **accessToken mới**
   - authStore lưu accessToken mới vào memory
   - UI render bình thường (user không cần đăng nhập lại)

4. **Khi request trả 401 — Axios interceptor:**
   ```
   Request → 401 → interceptor bắt
       → gọi authStore.refresh() (GET /auth/refresh)
       → nhận accessToken mới
       → retry request gốc với token mới
       → nếu refresh cũng fail → logout
   ```

5. **So sánh token-in-memory vs localStorage:**

   | Tiêu chí | Memory (Pinia) | localStorage |
   |----------|---------------|--------------|
   | Bảo mật XSS | ✅ An toàn (JS không đọc được sau khi F5 nếu không refresh) | ❌ JS đọc được → XSS lấy token |
   | Persistence | ❌ Mất khi F5/đóng tab | ✅ Còn sau F5 |
   | Implementation | Cần refresh flow | Đơn giản hơn |
   | CSRF | Cần xử lý nếu dùng cookie | Ít nguy cơ CSRF |

   → DSA Visual chọn memory + HttpOnly cookie = **bảo mật nhất** cho SPA.

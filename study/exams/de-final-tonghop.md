# Đề FINAL — Tổng Hợp Toàn Hệ Thống DSA Visual

**Thời gian:** 60 phút | **Tổng điểm:** 10 điểm  
**Bao phủ:** Toàn hệ thống — Guest → Student → Teacher → Admin  
**Cấu trúc:** 10 câu TN (1đ/câu = 6đ) + 3 câu Tự luận (~1.33đ/câu ≈ 4đ)

> ⚠️ Đề tổng hợp yêu cầu nắm vững toàn bộ luồng nghiệp vụ. Không chỉ thuộc API endpoint mà cần hiểu WHY & HOW.

---

## PHẦN I — TRẮC NGHIỆM (10 câu × 0.6 điểm = 6 điểm)

**Câu 1.** Sau khi đăng nhập thành công, nếu user truy cập `/dashboard`, hệ thống xử lý thế nào?

- A. Render trang Dashboard riêng
- B. Redirect → `/profile` (PathRedirectView hoặc router redirect)
- C. Redirect → `/courses`
- D. Render HomeView với dashboard mode

---

**Câu 2.** Khi F5 (page reload), hệ thống khôi phục session theo thứ tự nào?

- A. localStorage → accessToken → restore user
- B. `main.ts` → `authStore.refresh()` → `GET /auth/refresh` (cookie tự gửi) → accessToken mới
- C. `main.ts` → check sessionStorage → restore Pinia state
- D. Axios interceptor tự động gửi refreshToken trong header

---

**Câu 3.** Một Student làm bài ExerciseView, bài trả về score = 65. Badge hiển thị gì?

- A. Badge variant="success" — "Đạt"
- B. Badge variant="warning" — "Cần cải thiện"
- C. Badge variant="danger" — "Chưa đạt" (vì score < 70)
- D. Badge variant="muted" — "Đang xem xét"

---

**Câu 4.** ExerciseView hỗ trợ 2 chế độ làm bài. Sự khác biệt giữa chúng là gì?

- A. Chính thức: có timer 30 phút; Luyện tập: không timer
- B. Chính thức: ghi điểm + nộp; Luyện tập: không chấm điểm (`practiceMode = true`)
- C. Chính thức: 10 câu; Luyện tập: 5 câu ngẫu nhiên
- D. Chính thức: submit lên server; Luyện tập: chỉ local, không gọi API

---

**Câu 5.** ClassReportView tính "% hoàn thành" dựa trên công thức nào?

- A. `completedStudents / totalMembers * 100`
- B. `(onTime + late) / (assignments.length * totalMembers) * 100`
- C. `submittedCount / totalAssignments * 100`
- D. `avgScore / 100 * totalMembers`

---

**Câu 6.** Trong AdminStatsView, biểu đồ thứ 2 (donut chart) hiển thị thông tin gì và dùng thư viện nào?

- A. Doanh thu theo tháng — Chart.js
- B. Phân bố vai trò người dùng (STUDENT / TEACHER / TEACHER_PENDING / ADMIN) — ECharts qua VChartLazy
- C. Tỷ lệ hoàn thành bài học — D3.js
- D. Số lớp học theo teacher — Recharts

---

**Câu 7.** ShopView cho phép mua items bằng đơn vị tiền tệ nào trong hệ thống gamification?

- A. XP (kinh nghiệm điểm)
- B. Coins (đồng xu)
- C. Gems (đá quý) — `gamification.gems >= price`
- D. Hearts (tim) — dùng để mua thêm lượt làm bài

---

**Câu 8.** Sau khi `authStore.logout()` được gọi, bao nhiêu Pinia store bị reset và tên các store đó?

- A. 5 store (không có codeRunnerStore và simulationStore)
- B. 7 store: gamificationStore, progressStore, lessonStore, classStore, leaderboardStore, codeRunnerStore, simulationStore
- C. Toàn bộ store (kể cả authStore và uiStore)
- D. Không store nào bị reset — chỉ clear cookie

---

**Câu 9.** AdminSettingsView tab "system" cho phép cấu hình sandbox code runner với những tham số nào?

- A. Chỉ `sandboxSeconds` (timeout)
- B. `sandboxSeconds` (timeout) và `sandboxMemoryMb` (giới hạn RAM)
- C. `sandboxSeconds`, `sandboxMemoryMb`, `sandboxCpu` và `sandboxLanguages`
- D. `maxExecutionTime` và `allowedLanguages`

---

**Câu 10.** Mã mời lớp học (invite code) trong ClassesView có định dạng nào và được sinh ở đâu?

- A. UUID v4 — sinh bởi frontend
- B. 6 ký tự A-Z0-9 — sinh bởi backend khi tạo lớp; frontend chỉ validate input (uppercase + filter + max 6)
- C. 8 ký tự hex — sinh bởi frontend `Math.random()`
- D. Email-based code — ghép từ email teacher

---

## PHẦN II — TỰ LUẬN (3 câu × ~1.33 điểm ≈ 4 điểm)

> *Mỗi câu tự luận tính theo 4 ý × 0.33đ ≈ 1.33đ. Làm tròn tổng = 4đ.*

---

### TL-1: Hành trình Guest → Đăng ký → Học bài → Nhận XP (1.33 điểm)

**Trace toàn bộ luồng của một user mới từ lúc vào trang web đến khi nhận XP sau bài học:**

1. **Guest** truy cập HomeView — thấy gì? Có thể làm gì mà không cần đăng nhập?
2. **Đăng ký:** RegisterView gọi API nào? Sau đăng ký, user có role gì? Redirect về đâu?
3. **Đăng nhập & Token:** LoginView → authStore nhận `accessToken` lưu ở đâu? `refreshToken` ở đâu?
4. **Học bài:** Student vào PathView → chọn bài học → LessonView → hoàn thành → hệ thống cộng XP như thế nào? (Gợi ý: API /lessons/:id/complete, progressStore, gamificationStore)
5. **Xem kết quả:** ProfileView tab `progress` hiển thị dữ liệu từ store nào?

*(Tổng: 5 ý × ~0.27đ = 1.33đ — nêu đủ 5 ý chính)*

---

### TL-2: Teacher soạn bài → Admin duyệt → Student học → Quiz → Trừ tim → Mua tim (1.33 điểm)

**Trace toàn bộ flow nội dung từ creation đến consumption:**

1. **Teacher soạn bài:** `/studio/lessons/new` — soạn Markdown → lưu → bài có ID. Sau đó Teacher gán bài vào lớp qua ClassDetailView như thế nào?
2. **Student học:** Student vào lớp → thấy assignment → click vào ExerciseView. ExerciseView nhận `classAssignmentId` từ đâu? Để làm gì?
3. **Quiz & Trừ tim:** Khi làm quiz sai, hệ thống trừ tim (heart). Tim được quản lý bởi store/API nào? Hết tim thì sao?
4. **Mua tim:** Student vào ShopView → tìm item loại "support" → mua bằng Gems. Hàm `canAfford()` kiểm tra gì? API nào được gọi khi mua?
5. **Kiểm tra kết quả:** Score >= 70 → "Đạt"; < 70 → "Chưa đạt". confetti khi pass được trigger bởi event gì trong ExerciseView?

*(Tổng: 5 ý × ~0.27đ = 1.33đ)*

---

### TL-3: Admin duyệt TEACHER_PENDING → Teacher tạo lớp → Student tham gia → Xem ClassReport (1.33 điểm)

**Trace toàn bộ flow quản lý lớp học từ đầu đến báo cáo:**

1. **Admin duyệt:** Người đang là TEACHER_PENDING. Admin vào `/admin/users` tab pending → duyệt → API nào? Sau đó Teacher cần làm gì để role có hiệu lực?
2. **Teacher tạo lớp:** Teacher vào `/classes` → bấm "Tạo lớp" → tạo thành công → hệ thống sinh mã mời dạng gì? Teacher có thể chia sẻ mã này.
3. **Student tham gia:** Student nhập mã → `joinOpen = true` → Modal → submit → `POST /classes/join`. Sau khi join, `onMounted` trong ClassesView fetch gì?
4. **Teacher xem ClassReport:** Từ ClassDetailView, Teacher navigate đến ClassReportView (`/classes/:id/report`). Report tính "% hoàn thành" theo công thức nào? KPI phụ gồm những gì?
5. **Export báo cáo:** ClassDetailView có nút xuất Excel, ClassReportView có nút xuất CSV. Hai nút này gọi API/function nào khác nhau? Định dạng khác nhau thế nào?

*(Tổng: 5 ý × ~0.27đ = 1.33đ)*

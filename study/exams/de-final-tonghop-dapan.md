# ĐÁP ÁN — Đề FINAL: Tổng Hợp Toàn Hệ Thống DSA Visual

---

## PHẦN I — TRẮC NGHIỆM

| Câu | Đáp án | Giải thích chi tiết |
|-----|--------|-------------------|
| 1 | **B** | `/dashboard` → redirect → `/profile`. PathRedirectView hoặc router config `redirect: '/profile'`. ProfileView là hub chính thay thế Dashboard. |
| 2 | **B** | `main.ts` → `authStore.refresh()` → `GET /auth/refresh` → browser tự gửi HttpOnly cookie → server trả accessToken mới → Pinia lưu vào memory. |
| 3 | **C** | `Badge :variant="item.score >= 70 ? 'success' : 'danger'"` → score=65 < 70 → `variant="danger"` → text "Chưa đạt". Xem ExerciseView.vue dòng 167-168. |
| 4 | **B** | `practiceMode = ref(false)`. Khi `practiceMode=true`: QuizStage nhận prop, không ghi điểm, không gọi submit API thật. Nút toggle có `aria-pressed`. |
| 5 | **B** | `submitted = sum(onTime + late)`, `expected = assignments.length * totalMembers`, `pct = Math.round(submitted/expected*100)`. Xem ClassReportView totals computed. |
| 6 | **B** | Donut chart (pie type, radius=['52%','76%']) — phân bố vai trò: STUDENT/TEACHER/TEACHER_PENDING/ADMIN. Dùng ECharts qua component `VChartLazy`. |
| 7 | **C** | `const canAfford = computed(() => (price: number) => gamification.gems >= price)`. Mua item bằng **Gems**. Hearts dùng để làm bài quiz, không phải tiền tệ shop. |
| 8 | **B** | `authStore.logout()` reset 7 store: `gamificationStore`, `progressStore`, `lessonStore`, `classStore`, `leaderboardStore`, `codeRunnerStore`, `simulationStore`. `uiStore` không reset. |
| 9 | **B** | `settingsForm.sandboxSeconds` (timeout giây) và `settingsForm.sandboxMemoryMb` (RAM MB). Default: 5 giây, 128 MB. Dùng để giới hạn code runner sandbox. |
| 10 | **B** | Mã mời: 6 ký tự A-Z0-9. Backend sinh khi tạo lớp. Frontend `onInviteInput`: `.toUpperCase()` + filter `/[^A-Z0-9]/g` + `.slice(0, 6)`. VD: `DSA999`. |

---

## PHẦN II — TỰ LUẬN

### TL-1: Guest → Đăng ký → Học bài → Nhận XP *(1.33 điểm)*

**Thang điểm:** 5 ý × ~0.27đ mỗi ý.

**1. Guest ở HomeView:**
- Thấy: landing page giới thiệu DSA Visual, danh sách khóa học public, features
- Có thể: xem trang chủ, xem preview bài học (nếu có), xem leaderboard public
- Không thể: học bài, làm bài tập, xem progress → router guard redirect về `/login`

**2. Đăng ký:**
- RegisterView → form điền email/password/displayName
- API: `POST /auth/register`
- Role mặc định sau đăng ký: **STUDENT**
- Redirect: → `/` (HomeView) hoặc `/courses` — cần xác nhận email tùy config
- authStore nhận user info + accessToken

**3. Đăng nhập & Token:**
- LoginView → `POST /auth/login` → server trả `accessToken` + set HttpOnly cookie (`refreshToken`)
- `accessToken`: lưu trong **Pinia authStore** (memory RAM) — không localStorage
- `refreshToken`: lưu trong **HttpOnly cookie** (browser tự manage, JS không đọc được)

**4. Học bài → Nhận XP:**
```
PathView (lộ trình) → click bài → LessonView (đọc lý thuyết)
    → Hoàn thành → gọi POST /lessons/:id/complete
    → Server: cộng XP, cộng progress, check achievement
    → Response: { xpGained, newLevel, ... }
    → progressStore.updateProgress()
    → gamificationStore.fetchAll() (hoặc event-driven update)
    → XP bar trong header/profile tăng lên
```

**5. Xem kết quả:**
- ProfileView tab **`progress`** — dữ liệu từ **`progressStore`** (lessons completed, streaks)
- Tab **`overview`** — `XpProgressCard` từ **`gamificationStore`** (XP, level, gems)
- Tab **`achievements`** — `BadgeGrid` từ **`gamificationStore`** (badges unlocked)

---

### TL-2: Teacher soạn bài → Student học → Quiz → Trừ tim → Mua tim *(1.33 điểm)*

**Thang điểm:** 5 ý × ~0.27đ mỗi ý.

**1. Teacher soạn bài & gán lớp:**
```
/studio/lessons/new (AdminLessonEditorView)
    → soạn Markdown content → PUT /lessons/:id (save)
    → bài có lessonId
    ↓
ClassDetailView (/classes/:id) → tab "assignments"
    → assignOpen = true → assignType = 'lesson' → chọn lessonId
    → assignDue (deadline) → submit
    → POST /classes/:id/assignments { type:'lesson', itemId, dueDate, allowLate }
```

**2. Student học & classAssignmentId:**
- Student vào ClassDetailView → thấy bài gán trong tab "assignments"
- Click vào → navigate: `/exercise/:id?classAssignmentId={assignmentId}`
- ExerciseView đọc: `route.query.classAssignmentId` → `computed classAssignmentId`
- **Mục đích:** khi nộp bài, gửi kèm `classAssignmentId` để server biết chấm theo lớp học cụ thể

**3. Quiz & Trừ tim:**
- QuizStage component nhận `practiceMode` prop
- Khi trả lời sai → trừ heart (tim)
- Hearts được quản lý bởi **`gamificationStore`** (field `hearts`)
- API: `POST /gamification/hearts/use` hoặc được trừ tự động khi submit quiz sai
- **Hết tim:** không thể tiếp tục làm bài, prompt mua thêm tim

**4. Mua tim ở ShopView:**
- `canAfford = (price) => gamification.gems >= price`
- Tìm item category `'support'` có type tim
- API mua: gọi `gamificationApi.purchaseItem(itemId)` (POST /shop/purchase)
- Sau mua: `gamification.fetchInventory()` refresh kho đồ
- Tim dùng được ngay hoặc từ inventory → `equipItem`

**5. Kết quả & Confetti:**
- QuizStage emit `@passed` khi pass (score >= threshold)
- ExerciseView handler:
  ```typescript
  function onPassed(): void {
    fireConfetti('success');   // 🎉 confetti animation
    ui.showToast('Hoàn thành bài tập!', 'success');
  }
  ```
- Badge trong history drawer: `score >= 70 → 'Đạt'` (success); `< 70 → 'Chưa đạt'` (danger)

---

### TL-3: Admin duyệt → Teacher tạo lớp → Student tham gia → ClassReport *(1.33 điểm)*

**Thang điểm:** 5 ý × ~0.27đ mỗi ý.

**1. Admin duyệt TEACHER_PENDING:**
```
Admin → /admin/users → tab "pending"
    → danh sách TEACHER_PENDING → bấm "Duyệt"
    → PUT /admin/users/{id}/role { role: 'TEACHER' }
    → AdminController xử lý
```
→ Teacher cần **F5 (reload)** để `authStore.refresh()` → `GET /auth/refresh` → nhận role='TEACHER' → `/studio` accessible

**2. Teacher tạo lớp:**
```
/classes → isTeacher=true → nút "Tạo lớp" hiển thị
    → Modal tạo lớp → POST /classes { name, description }
    → Server tạo lớp + sinh invite code 6 ký tự A-Z0-9
    → Response: { id, inviteCode: "DSA999", ... }
    → classStore.fetchClasses() refresh danh sách
```
Teacher chia sẻ mã `DSA999` cho học sinh.

**3. Student tham gia:**
```
/classes → nút "Nhập mã lớp" → joinOpen = true → Modal
    → onInviteInput: uppercase + filter + max 6 chars
    → submit → POST /classes/join { code: "DSA999" }
    → classStore cập nhật
```
`onMounted` ClassesView → `classStore.fetchClasses()` → `GET /classes` (trả lớp mà user thuộc về)

**4. ClassReport — % hoàn thành & KPI:**
- Route: `/classes/:id/report` (ClassReportView)
- `fetchClassReport(classId)` → `GET /classes/:id/report` → `ClassReportDto`
- **Công thức %:**
  ```
  submitted = sum(assignment.onTime + assignment.late)
  expected  = assignments.length × totalMembers
  pct       = Math.round(submitted / expected × 100)
  ```
- **KPI phụ (secondaryKpis):**
  - Số bài gán (`assignments.length`)
  - Điểm trung bình (`avgScore`)
  - Tổng bài nộp (`totals.submitted`)

**5. Export báo cáo — Excel vs CSV:**

| | ClassDetailView | ClassReportView |
|--|----------------|----------------|
| Định dạng | **Excel (.xlsx)** | **CSV (.csv)** |
| API | `GET /classes/{id}/report/excel` | `classesApi.exportClassReportCsv(classId)` |
| Controller | ClassesController (backend) | ClassesController |
| Cách dùng | Download file trực tiếp | Blob + `URL.createObjectURL()` + `link.click()` |
| BOM | — | Có `\uFEFF` UTF-8 BOM (tránh lỗi Excel tiếng Việt) |

→ ClassReportView thêm BOM `'\uFEFF'` vào đầu CSV để Excel đọc được tiếng Việt đúng encoding.

---

## BẢNG ĐIỂM THAM KHẢO

| Phần | Điểm tối đa | Ghi chú |
|------|------------|---------|
| TN (10 câu × 0.6đ) | 6.0 đ | Chọn đúng = 0.6đ, sai = 0đ |
| TL-1 (5 ý) | 1.33 đ | ~0.27đ/ý |
| TL-2 (5 ý) | 1.33 đ | ~0.27đ/ý |
| TL-3 (5 ý) | 1.33 đ | ~0.27đ/ý |
| **Tổng** | **10.0 đ** | Làm tròn tổng |

# 🗺️ KẾ HOẠCH GIAI ĐOẠN TIẾP THEO — VisualizationDSA

> Nguyên tắc ưu tiên: **Ổn định → UX/UI → Tối ưu**
> Cập nhật: 02/08/2026

---

## TỔNG QUAN 3 GIAI ĐOẠN

| # | Giai đoạn | Mục tiêu | Ước lượng |
|---|-----------|----------|-----------|
| **I** | 🔒 Ổn định (Stability) | Mọi tính năng chạy đúng ở cả 3 role | ✅ HOÀN THÀNH 100% |
| **II** | 🎨 UX/UI Tuyệt vời | Nâng cấp giao diện lên tiêu chuẩn premium | 🚧 ĐANG LÀM (7/8) |
| **III** | ⚡ Tối ưu (Optimization) | Hiệu năng, bảo mật, deployment | ⏳ CHƯA BẮT ĐẦU |

---

## GIAI ĐOẠN I — ỔN ĐỊNH (Stability First)

> **Tiêu chí hoàn thành:** Mỗi role (Student, Teacher, Admin) có thể hoàn thành luồng chính end-to-end mà KHÔNG gặp lỗi.

### Role: 👤 Student (Chưa đăng nhập → Đăng ký → Học)

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ✅ S1 | **Luồng Auth end-to-end** | Trung bình | Kiểm tra: Register → Login → Session restore (F5) → Logout. Xác minh token refresh hoạt động. Fix nếu có race condition. |
| ✅ S2 | **Luồng Courses & Lessons** | Cao | Kiểm tra: `/courses` → chọn khóa → `/courses/:id` → bấm "Bắt đầu" → `/lessons/:id` → hoàn thành step. |
| ✅ S3 | **Luồng Quiz Backend** | Trung bình | Kiểm tra: `/quiz` → chọn chủ đề → làm bài → submit → xem kết quả + giải thích. |
| ✅ S4 | **Luồng Gamification** | Trung bình | Kiểm tra: Profile hiển thị XP/Level/Streak đúng từ DB. Leaderboard load data thật. Badges cabinet. |
| ✅ S5 | **Luồng Payment/Premium** | Trung bình | Kiểm tra: `/checkout` → tạo đơn → simulate webhook → isPremium=true → Crown badge hiện. |
| ✅ S6 | **Luồng Classroom (Student)** | Trung bình | Kiểm tra: `/classrooms` → tham gia bằng mã mời → xem nội dung lớp → làm bài tập. |
| ✅ S7 | **Sorting/Graph/Playground** | Thấp | Kiểm tra: Các sandbox algorithm chạy đúng (BubbleSort, QuickSort, BFS, DFS...), custom input, VCR controls. |

### Role: 👨‍🏫 Teacher (Đăng nhập Teacher → Quản lý nội dung)

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ✅ T1 | **Teacher Panel — Analytics** | Trung bình | Kiểm tra: `/teacher` → tab Analytics → thống kê quiz load từ API thật. |
| ✅ T2 | **Teacher Panel — Quiz CRUD** | Cao | Kiểm tra: Tạo quiz mới → thêm câu hỏi → lưu DB → hiển thị trong danh sách. Excel import hoạt động. |
| ✅ T3 | **Teacher Panel — Course CRUD**| Cao | Kiểm tra: Tạo khóa học → thêm Module → thêm ModuleItem → Publish. |
| ✅ T4 | **Teacher Panel — Codelab** | Cao | Kiểm tra: Tạo Codelab → thêm TestCase/Template/Hint → lưu DB. |
| ✅ T5 | **Teacher Panel — Classroom** | Trung bình | Kiểm tra: Tạo lớp → mời sinh viên → assign bài → xem điểm → export Excel. |
| ✅ T6 | **Teacher Studio & Roadmap** | Thấp | Kiểm tra: `/teacher-studio` load đúng dữ liệu, roadmap editor hoạt động. |

### Role: 🛡️ Admin (Đăng nhập Admin → Quản trị hệ thống)

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ✅ A1 | **Admin Dashboard Stats** | Trung bình | Kiểm tra: `/admin` → tab Dashboard → số liệu (users, quizzes, attempts) load từ DB. |
| ✅ A2 | **Admin Users Management** | Trung bình | Kiểm tra: Danh sách users từ DB → xem chi tiết → ban/unban → impersonate. |
| ✅ A3 | **Admin Quizzes Management** | Trung bình | Kiểm tra: Danh sách quizzes từ DB → accordion câu hỏi → CRUD quiz. |
| ✅ A4 | **Admin Teacher Approvals** | Trung bình | Kiểm tra: Tab duyệt đơn ứng tuyển Teacher → approve/reject. |
| ✅ A5 | **Admin Audit Log** | Thấp | Kiểm tra: Tab Audit hiển thị event stream từ SystemAuditEventStream. |

### Cross-cutting (Cắt ngang mọi role)

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ✅ X1 | **Seed Data Verification** | Cao | Xác minh DbSeeder tạo đủ account, quiz, leaderboard, course/lesson mẫu. |
| ✅ X2 | **API Contract Audit** | Trung bình | Rà soát tất cả frontend API service files → xác minh khớp với backend. |
| ✅ X3 | **Error Handling Consistency**| Trung bình | Xác minh Toast notification, skeleton loader, empty states. |
| ✅ X4 | **Commit A2/A3 đang pending**| Thấp | Đã hoàn tất commit các thay đổi. |

---

## GIAI ĐOẠN II — UX/UI TUYỆT VỜI

> **Tiêu chí:** Người dùng phải "WOW" ngay lần đầu mở app.

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ✅ U1 | **Landing Page Redesign** | Cao | Nâng cấp LandingView: hero animation, scroll-reveal, testimonials. |
| ✅ U2 | **Dashboard Hub Polish** | Cao | Nâng cấp DashboardView: XP wheel animation, recent activity feed. |
| ✅ U3 | **Sidebar & Navigation** | Trung bình | Nâng cấp sidebar: icon-only collapsed mode, smooth expand, active indicator. |
| ✅ U4 | **Sorting Visualizer Premium**| Trung bình | Nâng cấp SortingView: bar chart gradients, comparison counter, animations. |
| ✅ U5 | **Auth Pages Premium** | Trung bình | Nâng cấp AuthView: split layout, floating label inputs, social buttons. |
| ✅ U6 | **Profile Page Premium** | Trung bình | Nâng cấp ProfileView: cover photo, avatar upload, stats grid, achievements. |
| ✅ U7 | **Dark/Light Theme Toggle** | Cao | Thêm theme switcher: smooth transition, persist preference. |
| ⏳ U8 | **Responsive Mobile Polish**| Trung bình | Audit viewport 375px: fix overflow, touch targets ≥44px, swipe gestures, bottom sheets. |

---

## GIAI ĐOẠN III — TỐI ƯU (Optimization)

> **Tiêu chí:** Lighthouse Performance ≥ 90, bundle size < 500KB gzipped, API p95 < 200ms.

| ID | Task | Độ khó | Mô tả chi tiết |
|----|------|--------|-----------------|
| ⏳ O1 | **Bundle Size Audit** | Trung bình | Analyze `vite-bundle-visualizer`. Lazy-load routes. Code-split Monaco. Target < 300KB gzipped. |
| ⏳ O2 | **Image & Asset Optimization**| Thấp | Convert PNG → WebP, lazy-load images, favicon optimization. |
| ⏳ O3 | **Backend Query Optimization** | Trung bình | Review N+1 queries EF Core, add composite indexes. |
| ⏳ O4 | **Docker Production Config** | Trung bình | Multi-stage Dockerfile, nginx gzip_static, health check. |
| ⏳ O5 | **Lighthouse Audit & Fixes** | Trung bình | Run Lighthouse CI, fix accessibility (ARIA, contrast), LCP/FID/CLS. |
| ⏳ O6 | **Security Hardening Final** | Trung bình | CORS tightening, CSP headers, rate limiting audit, XSS sanitization. |

---

## 🗓️ ĐỀ XUẤT THỨ TỰ THỰC HIỆN TIẾP THEO

### Sprint hiện tại: Hoàn thiện UX/UI & Chuyển sang Tối ưu
1. **U8** — Responsive Mobile Polish (Khắc phục lỗi hiển thị trên thiết bị di động).
2. **O1** — Bundle Size Audit (Tối ưu hóa dung lượng file frontend).
3. **O2** — Image & Asset Optimization.
4. **O3, O4, O5, O6** — Các bước tối ưu backend, bảo mật và Lighthouse CI.

# Walkthrough — Kết quả Kiểm thử E2E Hệ thống DSA-Visual

> **Công cụ**: Playwright MCP trên phiên trình duyệt sống — quy trình **Sense → Act → Verify → Adapt**
> **Môi trường**: Frontend http://localhost:5173 (Vite + Vue 3) · Backend http://localhost:5000 (ASP.NET Core 8) · SQL Server
> **Ngày chạy**: 20/08/2026 · 63 ảnh chụp màn hình (thư mục session/shots/)

---

## 1. Tài khoản kiểm thử (xác thực qua API thành công)

| Vai trò | Email | Mật khẩu | Ghi chú |
|---|---|---|---|
| Giảng viên | teacher@demo.local | Teacher@123 | 💎 1000 gems · ❤️ 10 tim |
| Học sinh | student@demo.local | Student@123 | Cấp 4 · 1.480 XP · streak 10 ngày |
| Quản trị viên | admin@system.local | Admin@123 | Full quyền |

## 2. Tổng hợp kết quả (51 test case)

| # | Kịch bản | Kết quả | Ghi chú |
|---|---|---|---|
| **PHASE 1 — GIẢNG VIÊN** | | | |
| T1.1 | Đăng nhập Teacher → /path, nav có Studio, avatar 'Giáo viên mẫu' | ✅ PASS | 0 console error |
| T1.2 | Studio /studio: 3 lộ trình, 72 bài học, 20 bài tập, tab 1/2/3 | ✅ PASS | |
| T1.3 | Tạo lộ trình mới (CourseBuilderModal) → 'Lộ trình Trial E2E Teacher' | ✅ PASS | Modal 3 bước |
| T1.4 | Lọc/tìm kiếm bài học ('Array' → đúng 2 dòng) | ✅ PASS | Flat list 34 bài |
| T1.5 | Lesson Editor: Markdown + Live Preview đồng bộ | ✅ PASS | Marker xuất hiện trong preview |
| T1.6 | Gắn simulation (dropdown lọc 'bubble'; chọn Bubble Sort → sort.bubble) | ✅ PASS | |
| T1.7 | **Tạo/Xuất bản bài học mới** | ❌ **FAIL — BUG-A** | POST /lessons → 400 status enum |
| T1.8 | **Sửa bài học có sẵn** | ❌ **FAIL — BUG-B** | GET /lessons/{id}?includeContent=true → 403 |
| T1.9 | Lớp học /classes: danh sách 5 lớp, mã mời | ✅ PASS | Q0UYBO, K3L1YM |
| T1.10 | Tạo lớp mới → mã mời YLVU3U, redirect /classes/2002 | ✅ PASS | |
| T1.11 | Chi tiết lớp: tabs, mã mời, Thêm thành viên | ✅ PASS | |
| T1.12 | Báo cáo lớp (empty + seed 1003 có bảng nội dung) | ✅ PASS | KPI + at-risk |
| T1.13 | Xuất CSV báo cáo | ✅ PASS | GET /report/export → 200 |
| T1.14 | Block route admin cho teacher (/admin/users → /profile) | ✅ PASS | |
| **PHASE 2 — HỌC SINH** | | | |
| T2.1 | Đăng nhập Student → 'Sinh viên mẫu', không có Studio | ✅ PASS | |
| T2.2 | Lộ trình /path: Cấp 4, 1480 XP, 💎191, filters/sort | ✅ PASS | |
| T2.3 | Chi tiết lộ trình: cây module, node khóa/mở theo thiết kế | ✅ PASS | |
| T2.4 | Học bài /lessons/1002 Array: Markdown render | ✅ PASS | |
| T2.5 | Hoàn thành bài → 'Xuất Sắc! +100 XP' | ✅ PASS | |
| T2.6 | 'Học bài tiếp theo' → /lessons/1003 | ✅ PASS | |
| T2.7 | Danh mục mô phỏng /simulations | ✅ PASS | |
| T2.8 | EDV Simulator /simulator/sort.bubble: canvas + điều khiển | ✅ PASS | |
| T2.9 | Play/Step/Step-back: Bước 1→29→28/99 | ✅ PASS | |
| T2.10 | Cấu hình mảng tùy chỉnh [10,2,7,1,5] → áp dụng | ✅ PASS | |
| T2.11 | 4 sandbox: sorting/searching/graph/stack-queue | ✅ PASS | |
| T2.12 | Stack & Queue: Push 10/20/30 → chuỗi + nhật ký | ✅ PASS | |
| T2.13 | Benchmark Lab bubble vs quick → ms/ns + biểu đồ | ✅ PASS | Miễn phí tim |
| T2.14 | CheatSheet Big-O (44 mục) | ✅ PASS | |
| T2.15 | Code-to-Visual DSL Playground | ✅ PASS | |
| T2.16 | **Ladder /ladder/1**: Quiz→Lab→Code (20/30/50%) | ✅ PASS | |
| T2.17 | Bậc 1 Quiz: nộp → 'Điểm 2/5 · Đúng 2/5' + giải thích + Thử lại | ✅ PASS | |
| T2.18 | Bậc 2 Lab: swap, nộp → chấm 'Chưa đạt — làm lại miễn phí' | ✅ PASS | |
| T2.19 | Bậc 3 Code: 'Thành công 11ms·20 so sánh' + [1,2,3,5,7,8,9] + visual trace | ✅ PASS | |
| T2.20 | Profile: Lv4, streak 10, 191 gems, skill radar | ✅ PASS | |
| T2.21 | Quests: 6/7 daily + nút Nhận thưởng | ✅ PASS | |
| T2.22 | Shop: 8 items, túi đồ | ✅ PASS | |
| T2.23 | Leaderboard TOP 20 XP tuần | ✅ PASS | |
| T2.24 | Premium: 3 gói 49k/129k/399k | ✅ PASS | |
| T2.25 | Thanh toán Mock (QR MB Bank): upgrade + mock-pay | ✅ PASS | POST /premium/* → 200 |
| T2.26 | **Trang Subscription sau khi kích hoạt** | ❌ **FAIL — BUG-C** | Hiển thị 'chưa có gói' dù active + tim 30/30 |
| T2.27 | Tham gia lớp mã YLVU3U → '1 thành viên - Sinh viên mẫu' | ✅ PASS | |
| **PHASE 3 — QUẢN TRỊ** | | | |
| T3.1 | Đăng nhập Admin → nav 'Quản trị' | ✅ PASS | |
| T3.2 | Người dùng: bảng + filters vai trò/trạng thái | ✅ PASS | 'Học viên' → 18 users |
| T3.3 | Tìm kiếm → GET /users?q=Sinh+viên+mẫu → 1 kết quả | ✅ PASS | |
| T3.4 | Khóa/Mở khóa → PUT /users/3/status 204, khôi phục | ✅ PASS | |
| T3.5 | Tab 'Chờ duyệt Teacher (1)' — Sơn 123@gmail.com | ✅ PASS | |
| T3.6 | Thống kê: 81 users(68 active), 72 bài, 105 BT, 36 sim, biểu đồ | ✅ PASS | 1 cảnh báo ECharts |
| T3.7 | Cấu hình hệ thống /admin/settings | ❌ **FAIL — BUG-D** | Render nhầm trang bug-report; thiếu UI cấu hình |

**KẾT QUẢ: 47/51 PASS · 4 FAIL (4 bug). Console runtime: 0 unhandled JS errors.**

## 3. Bugs phát hiện

### 🔴 BUG-A — BLOCKER: Không thể TẠO/SỬA bài học (400)
- Hiện tượng: POST /api/v1/lessons trả 400 VALIDATION_FAILED khi xuất bản bài học.
- Nguyên nhân: FE gửi status:'active' (chuỗi, type 'draft'|'pendingreview'|'active'|'hidden'). BE LessonUpsertRequest.Status là enum LessonStatus (Draft/PendingReview/Active/Hidden) và KHÔNG đăng ký JsonStringEnumConverter (tìm toàn repo = 0) → chỉ đọc được SỐ. Chiều GET LessonDto.Status trả chuỗi (ToLowerInvariant) nên danh sách vẫn hiển thị bình thường.
- Bằng chứng: response details [field: '$.status', 'could not be converted to ...LessonStatus']; ảnh editor lúc submit.
- Gợi ý fix: (a) thêm JsonStringEnumConverter toàn cục, hoặc (b) đổi LessonUpsertRequest.Status sang string + map trong service, hoặc (c) FE gửi số enum.

### 🟠 BUG-B — HIGH: Teacher 403 khi mở sửa bài học không thuộc quyền
- Hiện tượng: Studio liệt kê mọi bài học; bấm 'Soạn / Sửa' → GET /api/v1/lessons/{id}?includeContent=true → 403 → bị đá về /studio KHÔNG có thông báo (chỉ console error).
- Nguyên nhân: Policy 'teacher chỉ xem bài CreatedBy == userId' (LessonService.GetByIdAsync). UI không lọc/cảnh báo.
- Gợi ý fix: FE lọc bài theo người tạo (ẩn nút Soạn/Sửa bài không phải của mình); hiển thị toast lỗi rõ ràng thay vì redirect im lặng.

### 🟠 BUG-C — HIGH: Trang Subscription hiển thị sai sau kích hoạt Premium
- Hiện tượng: Mock-pay thành công (upgrade 200, mock-pay 200; navbar 30/30 tim) nhưng /account/subscription vẫn 'Bạn chưa có gói Premium' + mâu thuẫn 'CÒN LẠI · ngày 31'. Tái hiện khi reload.
- Nguyên nhân: BE trả {planId, status:'active', expiresAt}; FE PremiumStatusDto khai báo isPremium: boolean và SubscriptionView kiểm tra status.isPremium → luôn undefined.
- Bằng chứng: GET /premium/status → {"planId":"1m","status":"active","expiresAt":"2026-09-20"}.
- Gợi ý fix: Đồng bộ contract — BE thêm isPremium hoặc FE dùng status.

### 🟡 BUG-D — MEDIUM: Thiếu trang 'Cấu hình Hệ thống' — /admin/settings gắn nhầm
- Hiện tượng: /admin/settings render trang 'Báo cáo lỗi & Vi phạm' (bug-report triage). Không có UI xem/sửa /api/v1/settings (siteName, allowedDomains, passwordPolicy, uploadMaxMb, sandboxSeconds, sandboxMemoryMb — API chạy tốt 200).
- Nguyên nhân: AdminSettingsView.vue thực chất là trang bug reports; view sai mục đích với route.
- Gợi ý fix: Tách trang 'Cấu hình Hệ thống' (GET/PUT /settings); chuyển bug-report sang menu phù hợp. Các tham số gamification/mock-pay/rate-limit kỳ vọng chưa có UI.

## 4. Kiểm tra Console & Network
- Console: 0 lỗi JS unhandled toàn walkthrough. Cảnh báo nhỏ: 'Missing Description / aria-describedby for DialogContent' (a11y) + 'ECharts grid.containLabel' (thư viện).
- Network: /auth/login 200, /lessons 200, /exercises 200, /classes/1003/report 200, /report/export 200, /premium/upgrade 200, /mock-pay 200, /users?q=... 200, /users/3/status 204. Lỗi duy nhất: 400 (BUG-A) và 403 (BUG-B).

## 5. Chỉ mục Screenshots (63 ảnh → session/shots/)
- Phase 1: 01-studio.png … 17-teacher-admin-blocked.png
- Phase 2: 20-student-path.png … 59-class-joined.png
- Phase 3: 60-admin-users.png … 67-admin-settings.png

## 6. Kết luận
Frontend vận hành 100% về mặt tương tác chính (điều hướng, EDV simulator, sandboxes, Ladder 3 bậc, gamification, báo cáo lớp, quản trị người dùng). **4 vấn đề cần xử lý trước khi hoàn chỉnh**: BUG-A (không tạo/sửa bài — chặn luồng teacher soạn bài), BUG-C (sai hiển thị gói Premium — ảnh hưởng UX bán hàng), BUG-B (403 không thông báo), BUG-D (thiếu trang cấu hình). **BUG-A và BUG-C là lệch hợp đồng API (contract mismatch) FE/BE — nên ưu tiên sửa trước.**
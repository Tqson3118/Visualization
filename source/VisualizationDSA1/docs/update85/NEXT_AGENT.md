# 🚀 NEXT AGENT — Bắt đầu ngay (không cần đọc thêm)

> Bạn là dev tiếp nối VisualizationDSA. Đọc file NÀY trước, rồi mở đúng các file liệt kê.
> Trạng thái: **FE 822/822 + BE 50/50 + build xanh + Docker đang chạy (CustomRoadmaps=5)**. Việc còn lại = 5 mục bên dưới, làm theo thứ tự.

## 0. Lệnh verify trước khi làm (chạy nhanh)

```powershell
# Docker (đang chạy sẵn — kiểm tra nhanh)
docker compose ps
curl http://localhost:5055/health
# Build + test
cd D:\FPT\og\VisualizationDSA\frontend; npm run build; npx vitest run
cd D:\FPT\og\VisualizationDSA\backend;  dotnet build src/WebApi/WebApi.csproj; dotnet test tests/VisualizationDSA.UnitTests
```

## 1. File phải đọc (theo thứ tự)

| # | File | Vì sao |
|---|---|---|
| 1 | `D:\FPT\og\VisualizationDSA\docs\update85\HANDOFF_STATE.md` | **File gốc** — quyết định PM + việc đã làm + checklist còn lại |
| 2 | `D:\FPT\og\docs\update85\REPORT_G3.md` / `REPORT_G4.md` | Chi tiết G3/G4 + lỗi Docker đã sửa |
| 3 | `D:\FPT\og\docs\update85\implementation_plan_detail.md` | Plan task-by-task (đã tick ✅ G0-G4) |
| 4 | `D:\FPT\og\VisualizationDSA\DEV_SETUP.md` | Lệnh build/test chuẩn + tài khoản test |

## 2. VIỆC CÒN LẠI — LÀM THEO THỨ TỰ (backend đã sẵn sàng, chủ yếu nối FE)

### ⭐ #1 Frontend Google OAuth (backend đã xong)
- **Đã có backend**: `POST /api/v1/concepts/auth/google-login` (`StatelessAuthController.cs`) + `IAuthService.GoogleLoginAsync` + `statelessAuthApi.googleLogin()` (FE).
- **Cần làm**: nối `AuthView.vue:214` (`handleGoogleLogin` hiện toast) → gọi `googleLogin`. Key placeholder trong `.env` (`GOOGLE__CLIENTID`), PM sửa sau. Nếu chưa có `VITE_GOOGLE_CLIENT_ID` → dev fallback gửi email/name.
- **File**: `frontend/src/views/auth/AuthView.vue`, `frontend/src/features/auth/services/statelessAuthApi.ts` (đã thêm function), `frontend/src/features/auth/store/useAuthStore.ts` (xem pattern `logIn` dòng 84-90, dùng `setSession`).

### ⭐ #2 Frontend Enrollment — Library hiển thị CustomRoadmaps
- **Đã có backend**: `GET /api/v1/teacher-studio/roadmaps/published` (public) trả `CustomRoadmapDto` (có `nodes[]`).
- **Cần làm**:
  - `frontend/src/views/courses/CoursesListView.vue`: đổi `loadCourses()` → `/teacher-studio/roadmaps/published`, map field (name→title, nodes.length→totalLessons, tags→category).
  - `frontend/src/views/courses/CourseDetailView.vue`: đổi `loadCourseDetail()` → `/teacher-studio/roadmaps/{id}`, render node list. Enroll đã đúng (`roadmapId`).
  - Nút "vào bài" từ node → dùng `frontend/src/components/practice/PracticeLadder.vue` (cần `sessionId` từ `POST /api/v1/session/{nodeId}/enter`).
- **Verify cuối**: login `nguyenvana@visualizationdsa.dev`/`User@2024` → mở roadmap → Đăng ký → **201** (trước đây 500).

### #3 PracticeLadder (giữ + làm thật)
- Node seed chỉ có `quizId`, chưa có lab/leetcode → seed thêm hoặc chấp nhận lock. Verify flow Quiz→Lab→LeetCode.

### #4 Commit 1 lần (PM đã duyệt)
```powershell
cd D:\FPT\og\VisualizationDSA
git add -A
git commit -m "feat: G0-G4 hoàn thiện + Docker fix + hạ tầng thật (SMTP/Cloudinary/GoogleOAuth placeholder)"
```

### #5 Việc phụ
- G2.15: PasswordStrengthMeter (AU1), route `/s/` (ES2).
- AdminUsersController (R6): tách endpoint classroom-scoped nếu cấm Teacher.
- SePay E2E (G4.2) khi PM có tài khoản.

## 3. Lưu ý quan trọng (KHÔNG lặp lại lỗi)

- **KHÔNG** chuyển `EnsureCreated` → `MigrateAsync` (migration order sai, sẽ fail).
- CORS chỉ cho `5173/3000` — screenshot/dev server khác port bị chặn.
- Key hạ tầng để placeholder trong `VisualizationDSA/.env` — PM sửa sau, code không được throw khi thiếu key.
- Màu qua token, cấm `alert()/confirm()`, cấm hardcode.

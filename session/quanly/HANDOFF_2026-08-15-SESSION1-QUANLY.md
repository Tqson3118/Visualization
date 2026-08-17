# HANDOFF — 15/08/2026 (phiên đêm 14→15/08: Educative-clone + feedback 2 chiều + fix mất nội dung + port UI VDSA courses)

> File bàn giao toàn bộ công việc phiên này (từ ~17:00 ngày 14/08 đến 01:00 ngày 15/08). Phiên mới đọc file này + `HANDOFF_2026-08-14-SESSION2-QUANLY.md` là nắm hết. KHÔNG cần đọc lịch sử cũ hơn.

## 1. TÓM TẮT PHIÊN NÀY (9 việc chính)

| # | Việc | Trạng thái |
|---|---|---|
| 1 | Clone trang chi tiết khóa học theo Educative (`/courses/7`): header phẳng căn giữa, stats thật (rating ★, lessons/quiz/lab/XP), objectives/outcomes card, curriculum đánh số + expand all, testimonials, author thật, related courses tự ẩn, bỏ certificate/FAQ | ✅ |
| 2 | Feedback GV–HV **2 chiều** theo khóa: entity `CourseFeedback` + controller (HV gửi/xem, GV xem/trả lời/đổi trạng thái) + trang GV `/admin/feedback` + box "Góp ý cho giảng viên" ở trang khóa (**CHỈ hiện khi hoàn thành 100% lộ trình**) | ✅ |
| 3 | Rating khóa học THẬT: AVG ContentFeedback của bài trong path + seed 8 đánh giá mẫu (3 user base để idempotent) | ✅ |
| 4 | **Fix mất nội dung nghiêm trọng**: 4 bài (Stack/Queue/Hash Map/Hash Set) mất ~75-84% content vì sanitize markdown như HTML (`Stack<T>` làm AngleSharp nuốt phần sau). Fix: lưu **nguyên contentMd** + seed **tự chữa (self-healing)**. 16/16 bài khớp từng ký tự với JSON gốc | ✅ |
| 5 | **Fix navigation kẹt (phải F5 mới vào được bài học)**: CourseDetailView có fragment root (modal ngoài root) → Transition out-in kẹt. Fix: modal vào `<Teleport>` TRONG root div. + fix 500 compile (mất thẻ đóng div) + fix thiếu import BaseIcon ở LessonStepTheory/LessonCompletionModal | ✅ |
| 6 | Chuyển bài **mượt** (hết nháy): App.vue key lesson-study cố định theo route name + store giữ bài cũ khi load bài mới | ✅ |
| 7 | Tick ✓ bài đã hoàn thành ở mini-map sidebar (localStorage `dsa.completedLessons` + status backend) | ✅ |
| 8 | Sidebar highlight bài đang học **màu tím** (fix class sai token accent→vdsa) | ✅ |
| 9 | Trang `/courses`: **port NGUYÊN BẢN UI VDSA-main** (search + filter + sort + grid 4 cột + Xem thêm + CourseCard/CourseCover/CourseFilter) + backend `/auth/me` thêm `xp`+`level`; full-width; footer ẩn | ✅ |

## 2. BÀI HỌC KỸ THUẬT QUAN TRỌNG (lần sau đừng dính lại)

1. **Tailwind v4 `@theme` phải nằm cùng file `@import "tailwindcss"`** — `@theme` trong file CSS khác (vd `vdsa-theme.css`) bị giữ nguyên dạng thô, **KHÔNG sinh utility nào** (class `text-vdsa-yellow`... toàn bộ trắng). Đã chuyển block `@theme inline` vdsa vào `tailwind.css`. Lần sau thêm màu mới → thêm vào tailwind.css, KHÔNG vào vdsa-theme.css.
2. **Không bao giờ sanitize markdown như HTML**: nội dung bài Grokking là markdown + HTML diagram, chứa `` `Stack<T>` `` → parse HTML đọc `<T>` thành tag → nuốt toàn bộ phần sau → mất 74% content. FE `LessonStepTheory` tự render markdown → backend phải lưu RAW contentMd.
3. **Component có fragment root (2 div cấp cao nhất) BÊN TRONG `<Transition mode="out-in">` → navigation bị kẹt vĩnh viễn** (view mới không mount, phải F5). Cảnh báo Vue "renders non-element root node". Teleport đứng CẠNH root div vẫn tính là fragment — phải đặt Teleport BÊN TRONG root div.
4. **VDSA-main class `bg-accent`/`text-accent` = shadcn teal của app** — mọi file copy phải dùng `vdsa-*`; token hợp lệ: `vdsa-accent(-light/-dark)`, `vdsa-green(-light)`, `vdsa-red(-light)`, `vdsa-yellow(-light)`, `vdsa-cyan(-light)`, `vdsa-purple(-light)`, `vdsa-border(-subtle/-strong)`. KHÔNG có `accent-green/accent-red/accent-yellow/...-dim` → dùng `green/10` (opacity modifier) thay `-dim`.
5. **Rate limit 429** (sensitive 60 req/min/IP): login liên tục qua Playwright/test đốt nhanh → chờ 60-75s HOẶC restart backend (in-memory). Phần lớn "lỗi login" lúc test = 429, không phải bug.
6. **PowerShell 5.1 đọc JSON tiếng Việt bị mojibake** (Get-Content -Raw | ConvertFrom-Json) → dùng `[System.IO.File]::ReadAllText(path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json`; câu SQL so khớp tiếng Việt cũng hỏng → dùng LIKE ASCII (`Title LIKE 'B%3: Stack%'`).
7. **EF migration**: `dotnet ef migrations add <Tên> --project src/DsaVisual.Application --startup-project src/DsaVisual.Api --output-dir Persistence/Migrations` (bắt buộc --output-dir). Build bị file-lock khi backend đang chạy → Stop-Process dotnet trước.

## 3. THAY ĐỔI BACKEND

- **Entity**: `LearningPath` +`AuthorId`, `HighlightsJson`, `TestimonialsJson` (marketing tùy biến theo khóa); MỚI `CourseFeedback` (Type: Suggestion/Bug/Request; Status: New/Read/Resolved; ReplyText/RepliedById/RepliedAt) + config + DbSet + enums.
- **Migration**: `AddCourseFeedbackAndCourseMarketing` (đã áp vào DB thật).
- **`ConceptsController`**: DTO +`Rating`, `RatingCount`, `Highlights`, `Testimonials`, `Author` (từ AuthorId→Users.TeacherBio/AcademicDegree/ProfileLink/AvatarUrl). Helper `CourseRatingAsync` = AVG ContentFeedback của lessons trong path; `ParseHighlights/ParseTestimonials` (JSON, property name case-insensitive).
- **MỚI `CourseFeedbackController`** (route `api/v1/courses/feedback*`): `POST feedback` (HV), `GET feedback/mine?courseId=` (HV xem ý kiến + reply), `GET feedback/all?courseId=&status=` (TEACHER/ADMIN), `PUT feedback/{id}` (reply+status; `[Authorize(Roles="TEACHER,ADMIN")]` — HV bị 403).
- **Seed `SeedGrokkingData`**:
  - Lưu RAW `contentMd` (KHÔNG sanitize) + **tự chữa**: nếu `lesson.ContentHtml != contentMd` → cập nhật (log "Tự chữa N bài học").
  - Gắn AuthorId = teacher@demo.local + Highlights/Testimonials JSON (4+3) + backfill bio/degree/link GV.
  - 8 đánh giá mẫu bằng **3 user base cố định** (admin/teacher/student@demo — idempotent; trước dùng "first 6 users" bị V2 thêm user → lần seed 2 thêm 3 cặp mới → vỡ test `Seed_SecondRun_DoesNotChangeAnyCount`).
- **`AuthService.ToUserSummary`** + **`Dtos/UserSummary.cs`**: +`Xp`, `Level` (1 + floor(sqrt(xp/100)) — khớp GamificationService.ComputeLevel).

## 4. THAY ĐỔI FRONTEND

- **`views/courses/CourseDetailView.vue`** (+`.css`): Educative-style — breadcrumb + title/stats/buttons **căn giữa**; stats thật (4.2 ★ 11 đánh giá · 18 Bài học · 13 Quiz · 4 Bài tập · 1800 XP) icon **tím** (sao vàng); objectives 1 border bao quanh; "Tại sao chọn" = **timeline giữa so le** + nét đứt nhạt → **tím đổ xuống theo scroll** (progress-fill, công thức trễ `(viewport-top)/(height+viewport*0.5)` — để nhìn thấy nét đứt) + v-reveal IntersectionObserver 2 chiều; testimonials; author; related (ẩn khi ≤1); feedback box 2 chiều (**v-if course.progressPercent >= 100**); modal đăng ký trong `<Teleport to="body">` NẰM TRONG root div; nút primary hover tím glow; nền `#0d0d11`; bỏ certificate.
- **MỚI `views/AdminFeedbackView.vue`** + route `/admin/feedback` (roles TEACHER,ADMIN) + link AdminNav "Ý kiến học viên": lọc khóa/trạng thái (Tất cả/Mới/Đã đọc/Đã xử lý), reply textarea + status select + Lưu.
- **`views/courses/CoursesListView.vue`**: port nguyên bản VDSA-main (search `CourseFilter`, sort select, grid 4 cột `CourseCard`, Xem thêm 8/trang, skeleton, offline error) — sed namespace → vdsa, full-width (bỏ container max-w-7xl), nền #0d0d11, header box **Cấp độ/XP thật**.
- **`features/courses/`**: MỚI `components/CourseCard.vue` + `CourseCover.vue` + `CourseFilter.vue` (copy+sed từ VDSA-main, thêm import BaseIcon); thay `store/useCourseStore.ts` (bản VDSA: loadCourses/filter/search/sort + enroll/isEnrolled/getCourseById/getCourseProgress — **giữ API cũ nên CourseDetailView không vỡ**; localStorage key `enrolled_{id}`, `lesson_progress_{id}`); `types/course.types.ts` (category/difficulty → string, lessons optional).
- **`services/courseApi.ts`**: +`CourseListDto`, `getCourses` typed; + types feedback (CourseFeedbackDto, submit/reply API).
- **`features/lesson/store/useLessonStore.ts`**: loadLesson **KHÔNG null currentLesson khi load** (giữ bài cũ → chuyển bài mượt); +`completedLessonIds` + `markLessonCompleted` (localStorage `dsa.completedLessons`) → tick mini-map.
- **`views/lesson/LessonStudyView.vue`**: active lesson **tím** (bg-vdsa-accent/10 + ring + chấm tím glow — trước dùng token accent sai = teal/xám); tick ✓ xanh (`check-circle text-vdsa-green`) bên phải bài đã hoàn thành; `LessonDto.status?`.
- **`views/lesson/components/LessonStepTheory.vue`** + **`LessonCompletionModal.vue`**: +import BaseIcon (thiếu → warning "Failed to resolve component").
- **`App.vue`**: `:key` lesson-study cố định theo route name (chuyển bài không remount/fade — các route khác giữ fullPath); **footer ẩn** khi `route.name === 'lesson-study' || 'courses'`.
- **`styles/tailwind.css`**: block `@theme inline` vdsa chuyển TỪ vdsa-theme.css VÀO đây (xem §2.1).
- **`styles/vdsa-theme.css`**: chỉ giữ `:root` biến + comment (bỏ @theme); accent sáng hơn `#a855f7`/`#c084fc`.
- **`shared/components/BaseIcon.vue`**: +icon `message`, `send`, `award`, `user-round`.
- **`api/auth.ts`**: `UserSummary` +`xp`, `level`.

## 5. VERIFY ĐÃ CHẠY (phiên này)

- BE: `dotnet test tests/DsaVisual.UnitTests` = **153 PASS** (integration 77 cần Docker — máy này không có).
- FE: `npx vue-tsc --noEmit` sạch; `npx vitest run` = **164 PASS**; `npm run build` PASS.
- Playwright E2E (pattern: apiLogin node http → context.addCookies refresh_token → mở trang): /courses/7 render đủ 7 sections; curriculum 5 module; rating 4.2 (11); feedback 2 chiều (HV gửi → GV trả lời → HV thấy reply); /admin/feedback load; Start Learning vào bài KHÔNG cần F5, 0 lỗi console; chuyển bài mượt (0 page-transition); tick hoàn thành; /courses full-width search/filter/sort/level-box.
- DB audit: 16/16 lessons content == JSON gốc (byte-level); quiz 145 câu (12 quiz + final test); 4 codelab; 8 lab.

## 6. TÀI KHOẢN

- `student@demo.local / Student@123` (Cấp 3, 895 XP) · `teacher@demo.local / Teacher@123` · `admin@system.local / Admin@123`. V2 students `@university.edu.vn` cũng `Student@123`.

## 7. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **Chưa commit gì phiên này** — kiểm tra nhánh (`git status` — repo nằm `...\Visualization-dev\Visualization-dev`), commit theo `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc), PR base `dev`.
2. **Docs đồng bộ** (task dev-docs): SRS/SDD/API_REFERENCE/SCREEN_MAP chưa cập nhật: ConceptsController (rating/highlights/testimonials/author), CourseFeedback API, `/auth/me` +xp/level, route `/admin/feedback`, `/courses` UI mới, dark theme.
3. **THIRD_PARTY.md**: thêm `monaco-editor`, `@monaco-editor/loader` (đã cài, chưa ghi).
4. **PROMPT_VISUALIZE_UPGRADE**: bước Viz trong lesson flow VẪN chưa làm (đã chốt hướng: nối engine simulator có sẵn qua `sandboxConfig.simulationKey` — backend Grokking đã trả đủ key trùng catalog; JSON gốc KHÔNG có demo playground → VDSA-main cũng chỉ hiện empty state).
5. Rate-limit 429 khi test login liên tục — restart backend để reset.
6. Bug refresh-loop 401→redirect (client.ts:110-112, khi ở /login không cookie) — CÓ SẴN từ trước, chưa fix; làm login UI qua Playwright bị kẹt → dùng pattern apiLogin+cookie.
7. `frontend/src/data/lessons.ts` + PathView/PathRedirectView/NodeHubView còn trong repo (route đã redirect) — xóa sau khi docs đồng bộ.

## 8. MẸO NHANH (verify phiên sau)

```powershell
# Backend
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\backend" -WindowStyle Hidden

# FE (từ frontend/)
cmd /c "npm run dev -- --port 5173 --strictPort"

# Seed lại (idempotent, tự chữa content nếu lệch JSON)
dotnet run --project src/DsaVisual.Api -- --seed

# Audit content Grokking (đọc JSON UTF8 + so DB)
# [System.IO.File]::ReadAllText("...\backend\seed-data\grokking-course.json", [Text.Encoding]::UTF8) | ConvertFrom-Json

# E2E pattern: node http POST localhost:5000/api/v1/auth/login → header set-cookie refresh_token → context.addCookies → goto localhost:5173
```

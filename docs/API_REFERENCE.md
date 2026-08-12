# TÀI LIỆU THAM CHIẾU API (API_REFERENCE)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | API Reference |
| Phiên bản | 1.4 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo — chờ phê duyệt |
| Người soạn | Mai Tiểu Bảo |
| Người duyệt | Phạm Ngọc Ái Liên |
| Tài liệu liên quan | SRS.md, SDD.md, TEST_PLAN.md |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md Phần 9 (API), 5 (RBAC), 11.5 (chấm điểm) |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Mai Tiểu Bảo | Sinh mới từ PRODUCTION_PROMPT.md v2.5 |
| 1.1 | 12/08/2026 | Mai Tiểu Bảo | Vá review: bổ sung error code `LESSON_HAS_EXERCISES` (409); ghi phiên bản v2.4 cho 4 mã bổ sung ngoài §9.7; thay placeholder `"..."` trong ví dụ resume session bằng JSON thực |
| 1.2 | 12/08/2026 | Trần Viết Tâm Phúc | F2b: (1) §4.2 xóa endpoint `/public/simulations/{key}/run` đã cắt theo ADR-001 (quyết định A-4) — ghi chú thay thế; (2) §4.4 ghi rõ trạng thái "chưa triển khai" cho 3 endpoint Lessons (progress/mark-viewed/simulations) — theo dõi SETUP_TODO §6; (3) §5 dòng 12 sửa route viết tắt `GET /submissions?exerciseId` → `GET /exercises/{id}/submissions` khớp code |
| 1.3 | 13/08/2026 | Mai Tiểu Bảo | GP-T2 (2FA email — FR-1.11): (1) §4.12 bổ sung `POST /auth/2fa/send` + `POST /auth/2fa/verify`, làm rõ PUT /auth/2fa (tắt trực tiếp; bật qua mã OTP); (2) §2.2 bổ sung 5 error code 2FA `OTP_REQUIRED/OTP_INVALID/OTP_EXPIRED/OTP_USED/TWO_FA_ALREADY_ENABLED` (400) — [v2.13] |
| 1.4 | 13/08/2026 | Trần Viết Tâm Phúc | GP-T8 (đồng bộ GP-T7 — Premium QR MB Bank): §4.14 bổ sung chi tiết `POST /premium/upgrade` — `{planId}` `1m|3m|12m`; OrderRef đơn hàng = `DSV{userId}T{months}` (VD DSV1002T3 — trùng nội dung CK trên QR) + response trả `contentRef` (nội dung CK hiển thị trên QR); thêm ví dụ response; §8 thêm dòng thay đổi |

---

# 1. QUY ƯỚC CHUNG

## 1.1 Base URL & định dạng

- Base URL: `/api/v1` (production: `https://api.dsa-visual.example.edu.vn/api/v1`).
- Body/response: JSON (`application/json`); ngày giờ: ISO 8601 UTC (`2026-08-09T12:34:56Z`).
- ID: số nguyên tự tăng (int) cho bảng nội bộ; `SimulationKey` chuỗi (`sort.bubble`).

## 1.2 Xác thực

- `Authorization: Bearer <accessToken>` cho mọi endpoint trừ nhóm công khai.
- Access token: JWT HS256, 60 phút, claims `sub` (userId), `role`, `iat`, `exp`, `jti`.
- Refresh token: cookie `refresh_token` — `HttpOnly; SameSite=Strict; Secure; Path=/api/v1/auth`; 7 ngày; **rotate-invalidate** (token cũ thu hồi ngay khi cấp token mới; replay → thu hồi cả chuỗi phiên).

## 1.3 Phân trang, lọc, sắp xếp

- Phân trang: `?page=1&pageSize=20` (pageSize ≤ 100); response header `X-Total-Count` + body `{ items, page, pageSize, total, totalPages }`.
- Lọc: `?status=active&topicId=3&q=từ khóa` (nhiều điều kiện dùng `&`).
- Sắp xếp: `?sort=createdAt:desc,title:asc`.

## 1.4 Status code

| Mã | Ý nghĩa |
|---|---|
| 200 | Thành công (GET/PUT) |
| 201 | Tạo thành công (POST) — kèm `Location` |
| 204 | Thành công không nội dung (DELETE) |
| 400 | Dữ liệu không hợp lệ (validation) |
| 401 | Chưa xác thực / token hết hạn |
| 403 | Không đủ quyền / tài khoản bị khóa / hết tim |
| 404 | Không tìm thấy tài nguyên |
| 409 | Xung đột (email trùng, xóa chủ đề có con) |
| 422 | Trạng thái nghiệp vụ không cho phép (nộp trùng, dữ liệu quá lớn) |
| 429 | Vượt giới hạn tần suất (kèm `Retry-After`) |
| 500 | Lỗi máy chủ (ẩn chi tiết) |

## 1.5 Rate limit (NFR-12)

| Nhóm | Giới hạn |
|---|---|
| Đăng nhập | 5 lần / 15 phút / IP |
| API thường | 100 req / phút / người dùng |
| Sinh bước | Chạy phía client (ADR-001) — không có endpoint sinh bước trên server (`POST /simulations/run` đã cắt, A-4) |
| Code runs (sandbox) | 20 req / phút / người dùng (thay cho nhóm "Sinh bước" cũ — NFR-12 v2.10) |

---

# 2. ĐỊNH DẠNG LỖI & ERROR CODE CATALOG

## 2.1 Định dạng lỗi chuẩn (BẮT BUỘC)

```json
{
  "error": {
    "code": "EMAIL_EXISTS",
    "message": "Email đã được sử dụng",
    "field": "email",
    "details": []
  }
}
```

- `code`: UPPER_SNAKE duy nhất (chỉ dùng mã trong bảng 2.2); `message`: tiếng Việt hiển thị được; `field`: tên trường lỗi (nếu có); `details`: mảng lỗi con.

## 2.2 Error Code Catalog (khớp 100% mã dùng trong code backend)

| Mã | HTTP | Mô tả | field thường dùng |
|---|---|---|---|
| VALIDATION_FAILED | 400 | Dữ liệu không hợp lệ (nhiều trường) | từng trường trong details |
| INVALID_CREDENTIALS | 401 | Sai email/mật khẩu | null |
| UNAUTHORIZED | 401 | Thiếu/hết hạn token | null |
| TOKEN_EXPIRED | 401 | Access token hết hạn (frontend tự refresh) | null |
| REFRESH_INVALID | 401 | Refresh token không hợp lệ/hết hạn | null |
| ACCOUNT_LOCKED | 403 | Tài khoản bị khóa | null |
| ACCOUNT_DISABLED | 403 | Tài khoản chưa kích hoạt | null |
| HEARTS_EMPTY | 403 | Hết tim — chặn vào node (FR-10.1) — [v2.4] | null |
| FORBIDDEN | 403 | Không đủ quyền | null |
| NOT_FOUND | 404 | Không tìm thấy tài nguyên | null |
| EMAIL_EXISTS | 409 | Email đã được sử dụng | email |
| TOPIC_HAS_LESSONS | 409 | Không xóa được chủ đề có bài học | null |
| LESSON_HAS_EXERCISES | 409 | Không xóa được bài học còn bài tập đang hoạt động | null |
| DUPLICATE_SIMULATION | 409 | Gắn trùng mô phỏng vào bài học | simulationKey |
| WEAK_PASSWORD | 400 | Mật khẩu yếu (details liệt kê từng quy tắc) | password |
| DOMAIN_NOT_ALLOWED | 400 | Domain email không được phép đăng ký | email |
| INVALID_EMAIL | 400 | Định dạng email sai | email |
| OLD_PASSWORD_WRONG | 400 | Mật khẩu cũ sai khi đổi | currentPassword |
| PASSWORD_SAME | 400 | Mật khẩu mới trùng mật khẩu cũ | newPassword |
| RESET_TOKEN_INVALID | 400 | Link đặt lại mật khẩu hết hạn/đã dùng | token |
| SIMULATION_KEY_INVALID | 400 | Khóa mô phỏng không tồn tại trong danh mục | key |
| INPUT_INVALID | 400 | Dữ liệu đầu vào mô phỏng không hợp lệ | details chứa lỗi cụ thể |
| INPUT_TOO_LARGE | 422 | Vượt giới hạn kích thước dữ liệu (NFR-2) | details |
| SUBMISSION_IN_PROGRESS | 422 | Đang có bài nộp đồng thời | null |
| EXERCISE_CLOSED | 422 | Bài tập không còn nhận bài nộp | null |
| QUESTION_ANSWER_MISMATCH | 400 | Đáp án gửi lên không khớp câu hỏi | questionId |
| LADDER_LOCKED | 422 | Chưa pass bậc trước — không mở bậc sau (FR-4.11) — [v2.4] | null |
| RATE_LIMITED | 429 | Vượt giới hạn tần suất (kèm Retry-After) | null |
| UPLOAD_INVALID_TYPE | 400 | Sai định dạng file upload | file |
| UPLOAD_TOO_LARGE | 400 | File vượt giới hạn dung lượng | file |
| OTP_REQUIRED | 400 | Bật 2FA cần xác nhận mã OTP (POST /auth/2fa/send + /verify) — [v2.13] | null |
| OTP_INVALID | 400 | Mã OTP không đúng — [v2.13] | code |
| OTP_EXPIRED | 400 | Mã OTP hết hạn (5 phút) — [v2.13] | code |
| OTP_USED | 400 | Mã OTP đã được sử dụng (dùng 1 lần) — [v2.13] | code |
| TWO_FA_ALREADY_ENABLED | 400 | 2FA đã được bật — [v2.13] | null |
| INSUFFICIENT_GEMS | 422 | Không đủ gems mua vật phẩm (FR-10.2) — [v2.4] | null |
| QUEST_ALREADY_CLAIMED | 422 | Quest đã nhận thưởng (FR-10.3) — [v2.4] | null |
| INTERNAL_ERROR | 500 | Lỗi máy chủ (ẩn chi tiết) | null |
| SERVICE_UNAVAILABLE | 503 | DB/máy chủ quá tải | null |

> Cấm phát minh mã mới ngoài danh sách; ngoại lệ phải thêm vào bảng kèm phiên bản. Các mã bổ sung ngoài §9.7 prompt (đã ghi phiên bản tại cột Mô tả): `HEARTS_EMPTY`, `LADDER_LOCKED`, `INSUFFICIENT_GEMS`, `QUEST_ALREADY_CLAIMED` — đều thuộc v2.4 (Module J, bổ sung theo FR-10.1/10.2/10.3/4.11). `LESSON_HAS_EXERCISES` (409) giữ nguyên từ §9.7 prompt. 2FA email (GP-T2, 13/08/2026 — FR-1.11): 5 mã `OTP_REQUIRED/OTP_INVALID/OTP_EXPIRED/OTP_USED/TWO_FA_ALREADY_ENABLED` thuộc v2.13.

---

# 3. DTO ĐẦY ĐỦ (field/type/ràng buộc)

## 3.1 `RegisterRequest`

| Field | Type | Bắt buộc | Ràng buộc |
|---|---|---|---|
| displayName | string | ✔ | 2-100 ký tự |
| email | string | ✔ | email hợp lệ, ≤ 256, lowercase |
| password | string | ✔ | 8-64, chữ hoa + số + ký tự đặc biệt |
| isTeacher | bool | ✔ | mặc định false |

## 3.2 `LoginRequest` / `RefreshResponse`

`{ email, password }`; response `{ accessToken, expiresIn, user: UserSummary }`.

## 3.3 `UserSummary`

| Field | Type | Ghi chú |
|---|---|---|
| id | int | |
| displayName | string | |
| email | string | mask nửa đầu khi trả cho Teacher (`m***h@...`) |
| role | string | STUDENT/TEACHER/TEACHER_PENDING/ADMIN |
| avatarUrl | string? | |
| createdAt | datetime | |

## 3.4 `LessonDto`

| Field | Type | Ghi chú |
|---|---|---|
| id | int | |
| topicId | int | |
| title / description | string | |
| contentHtml | string | chỉ trả khi quyền Teacher hoặc `?includeContent=true` |
| status | enum | draft/active/hidden (Student chỉ nhận active) |
| sortOrder | int | |
| simulations | SimulationRef[] | |
| exercises | ExerciseRef[] | |
| progress | LessonProgressDto? | trạng thái cá nhân (Student) |

## 3.5 `LessonUpsertRequest`

| Field | Type | Bắt buộc | Ràng buộc |
|---|---|---|---|
| topicId | int | ✔ | tồn tại |
| title | string | ✔ | 3-200 ký tự |
| description | string | ✘ | ≤ 500 |
| contentHtml | string | ✔ | ≤ 200.000 ký tự; sanitize server |
| status | enum | ✔ | draft/active/hidden |
| sortOrder | int | ✘ | ≥ 0 |

## 3.6 `SimulationMetaDto`

| Field | Type | Ghi chú |
|---|---|---|
| key | string | `sort.bubble` |
| title | string | tiếng Việt |
| dataStructure | string | mảng/cây/... |
| category | string | structure/algorithm |
| level | string | basic/advanced |
| complexity | object | `{ best, average, worst, space }` |
| tags | string[] | |
| demoAllowed | bool | trong danh sách demo công khai |

## 3.7 `ExerciseDto` (chi tiết — KHÔNG đáp án)

| Field | Type | Ghi chú |
|---|---|---|
| id | int | |
| lessonId | int | |
| nodeId / stage | int? | node Ladder + bậc (1/2/3) |
| title / description | string | |
| type | enum | MCQ / SIMULATION_LAB / CODE |
| durationMinutes | int? | null = không giới hạn |
| maxScore | int | |
| questions | QuestionDto[] | ẩn `answerJson`, `explanation` |
| bestScore | int? | của người gọi (Student) |

## 3.8 `SubmitRequest` / `SubmitResultDto`

- Request: `{ answers: [{ questionId, selected: int[], simAnswer?: any }] }`.
- Response: `{ score, maxScore, results: [{ questionId, correct, correctAnswer, explanation }], submissionId, submittedAt }`.

## 3.9 `ProgressOverviewDto`

`{ lessonsViewed, lessonsTotal, exercisesCompleted, exercisesTotal, avgScore, topics: [{ id, name, progressPct, lessons: [{ id, title, viewed, bestScore, completed }] }] }`.

## 3.10 `TeacherReportDto`

`{ lessonId, lessonTitle, totalLearners, learnersViewed, completionPct, avgScore, exercises: [{ id, title, avgScore, submissionCount }], inactiveLearners: [UserSummary] }`.

## 3.11 `PagedResponse<T>`

`{ items: T[], page, pageSize, total, totalPages }` — mọi endpoint danh sách.

## 3.12 `HeartsStatusDto` / `NodeSessionDto`

- HeartsStatus: `{ hearts, heartsMax, lastHeartAt, nextHeartInSeconds }`.
- NodeSession: `{ id, nodeId, startedAt, expiresAt, stage, stepIndex }`.

## 3.13 `QuestDto` / `ShopItemDto` / `PremiumStatusDto`

- Quest: `{ id, title, type, progress, target, claimed, reward }`.
- ShopItem: `{ id, itemKey, name, priceGems, maxStack, type, owned }`.
- Premium: `{ planId, startedAt, expiresAt, status }`.

## 3.14 Quy tắc bảo vệ dữ liệu trong response (bắt buộc)

1. `AnswerJson`/`Explanation` chỉ trả về trong `POST /submit` (sau khi nộp) và cho Teacher/Admin ở endpoint quản lý — KHÔNG bao giờ trong `GET /exercises/{id}`.
2. Email: chỉ Admin và chính người dùng xem đầy đủ; Teacher thấy dạng `m***h@university.edu.vn`.
3. `contentHtml` bản nháp: chỉ Teacher sở hữu/Admin.
4. Mọi response không bao giờ chứa `PasswordHash`, `TokenHash`.

# 4. DANH SÁCH ENDPOINT ĐẦY ĐỦ

## 4.1 Auth (9 endpoint)

| Method | Endpoint | Mô tả | Quyền | Ghi chú |
|---|---|---|---|---|
| POST | `/auth/register` | Đăng ký | Công khai | body `{displayName, email, password, isTeacher}` |
| POST | `/auth/login` | Đăng nhập | Công khai | trả `{accessToken, expiresIn, user}`; set cookie |
| POST | `/auth/refresh` | Làm mới token | Cookie | trả accessToken mới; rotate-invalidate |
| POST | `/auth/logout` | Đăng xuất | Đã đăng nhập | thu hồi refresh |
| GET | `/auth/me` | Thông tin bản thân | Đã đăng nhập | |
| PUT | `/auth/me` | Cập nhật hồ sơ | Đã đăng nhập | tên, avatar |
| PUT | `/auth/me/password` | Đổi mật khẩu | Đã đăng nhập | `{currentPassword, newPassword}`; thu hồi refresh khác |
| POST | `/auth/forgot-password` | Gửi link khôi phục | Công khai | `{email}` — trả thông báo chung |
| POST | `/auth/reset-password` | Đặt lại mật khẩu | Token | `{token, newPassword}` |

**Ví dụ — POST /auth/login**

```json
// Request
{ "email": "minh@university.edu.vn", "password": "MatKhau@123" }

// Response 200
{ "accessToken": "eyJhbGciOiJIUzI1NiIs...", "expiresIn": 3600,
  "user": { "id": 12, "displayName": "Nguyễn Minh", "email": "minh@university.edu.vn",
            "role": "STUDENT", "avatarUrl": null } }
// Response 401
{ "error": { "code": "INVALID_CREDENTIALS", "message": "Email hoặc mật khẩu không đúng", "field": null, "details": [] } }
// Response 403
{ "error": { "code": "ACCOUNT_LOCKED", "message": "Tài khoản đã bị khóa, liên hệ quản trị viên", "field": null, "details": [] } }
```

**Ví dụ — POST /auth/register**

```json
// Request
{ "displayName": "Nguyễn Minh", "email": "minh@university.edu.vn", "password": "MatKhau@123", "isTeacher": false }
// Response 201 → UserSummary + auto login token
// Response 409
{ "error": { "code": "EMAIL_EXISTS", "message": "Email đã được sử dụng", "field": "email", "details": [] } }
// Response 400 (domain không cho phép)
{ "error": { "code": "DOMAIN_NOT_ALLOWED", "message": "Email không thuộc domain được phép đăng ký", "field": "email", "details": [] } }
```

## 4.2 Public

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/public/site-info` | Số liệu trang chủ (số CTDL/GT/bài học) |
| GET | `/public/faqs` | Danh sách FAQ |

> ⚠ `GET /public/simulations/{key}/run` **ĐÃ CẮT** (quyết định A-4 / ADR-001) — demo công khai không còn endpoint run riêng; người dùng mở mô phỏng qua `/simulations/{key}` + `/simulations/{key}/schema` (xem §4.5).

## 4.3 Topics

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/topics` | Cây chủ đề (lồng 2 cấp) | Đã đăng nhập |
| GET | `/topics/{id}` | Chi tiết chủ đề | Đã đăng nhập |
| POST | `/topics` | Tạo chủ đề | Teacher/Admin |
| PUT | `/topics/{id}` | Sửa chủ đề | Teacher/Admin |
| DELETE | `/topics/{id}` | Xóa (409 nếu có bài học) | Teacher/Admin |
| PUT | `/topics/reorder` | Đổi thứ tự | Teacher/Admin |

**Ví dụ — POST /topics**

```json
// Request
{ "parentId": null, "name": "Sắp xếp", "description": "Các giải thuật sắp xếp cơ bản và nâng cao", "sortOrder": 2 }
// Response 201
{ "id": 5, "parentId": null, "name": "Sắp xếp", "description": "Các giải thuật sắp xếp cơ bản và nâng cao", "sortOrder": 2, "children": [] }
// Response 409
{ "error": { "code": "VALIDATION_FAILED", "message": "Tên chủ đề đã tồn tại", "field": "name", "details": [] } }
```

## 4.4 Lessons

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/lessons` | Danh sách (`?topicId=&status=&q=&page=`) | Đã đăng nhập (Student: chỉ active) |
| GET | `/lessons/{id}` | Chi tiết + nội dung | Đã đăng nhập |
| GET | `/lessons/{id}/progress` | Trạng thái tiến độ của tôi | Student |
| POST | `/lessons` | Tạo | Teacher/Admin |
| PUT | `/lessons/{id}` | Sửa | Teacher (của mình)/Admin |
| DELETE | `/lessons/{id}` | Xóa mềm | Teacher (của mình)/Admin |
| POST | `/lessons/{id}/mark-viewed` | Đánh dấu đã học | Student |
| POST | `/lessons/{id}/simulations` | Gắn mô phỏng | Teacher/Admin |
| DELETE | `/lessons/{id}/simulations/{simKey}` | Gỡ mô phỏng | Teacher/Admin |

> ⚠ **TRẠNG THÁI TRIỂN KHAI (cập nhật 12/08/2026)**: `GET /lessons/{id}/progress`, `POST /lessons/{id}/mark-viewed`, `POST /lessons/{id}/simulations` và `DELETE /lessons/{id}/simulations/{simKey}` **CHƯA TRIỂN KHAI** trong `LessonsController.cs` (hiện chỉ có 5 route: GET /, GET /{id}, POST, PUT, DELETE). Frontend đã gọi `mark-viewed` nên 3 endpoint này đang theo dõi tại SETUP_TODO §6 (bug P1 mark-viewed 404) và sẽ triển khai ở đợt sau. Tiến độ 1 bài học dùng `GET /progress/me/lessons/{lessonId}` (đã triển khai — §4.6).

**Ví dụ — GET /lessons?topicId=2**

```json
{ "items": [
  { "id": 15, "title": "Bubble Sort", "description": "Sắp xếp nổi bọt từng bước",
    "topicId": 2, "sortOrder": 1, "status": "active",
    "simulationCount": 1, "exerciseCount": 2,
    "progress": { "viewed": true, "bestScore": 8, "completed": false } } ],
  "page": 1, "pageSize": 20, "total": 1, "totalPages": 1 }
```

**Ví dụ — POST /lessons**

```json
// Request
{ "topicId": 5, "title": "Bubble Sort — Sắp xếp nổi bọt",
  "description": "Giải thuật sắp xếp đơn giản nhất, so sánh từng cặp liền kề",
  "contentHtml": "<h2>Ý tưởng</h2><p>So sánh từng cặp phần tử liền kề...</p>",
  "status": "draft", "sortOrder": 1,
  "simulations": [ { "simulationKey": "sort.bubble", "title": "Mô phỏng Bubble Sort",
                     "defaultInput": { "values": [5,3,8,1,9,2] } } ] }
// Response 201 → LessonDto (draft, chỉ Teacher/Admin nhận contentHtml)
```

## 4.5 Simulations

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/simulations` | Danh mục (kèm schema, complexity) | Đã đăng nhập |
| GET | `/simulations/{key}` | Chi tiết 1 mô phỏng | Đã đăng nhập |
| GET | `/simulations/{key}/schema` | Cấu hình đầu vào | Đã đăng nhập |

> ⚠ `POST /simulations/run` ĐÃ CẮT (quyết định A-4) — sinh bước chạy frontend (ADR-001).

**Ví dụ — GET /simulations**

```json
{ "items": [
  { "key": "sort.bubble", "title": "Sắp xếp nổi bọt (Bubble Sort)", "dataStructure": "mảng",
    "category": "algorithm", "level": "basic",
    "complexity": { "best": "O(n)", "average": "O(n²)", "worst": "O(n²)", "space": "O(1)" },
    "tags": ["sắp xếp", "so sánh"], "demoAllowed": true },
  { "key": "tree.avl-insert", "title": "Cây AVL — Chèn và xoay", "dataStructure": "cây",
    "category": "algorithm", "level": "advanced",
    "complexity": { "best": "O(log n)", "average": "O(log n)", "worst": "O(log n)", "space": "O(log n)" },
    "tags": ["cây", "cân bằng"], "demoAllowed": false } ] }
```

## 4.6 Exercises

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/exercises` | Danh sách (`?lessonId=&nodeId=&stage=`) | Đã đăng nhập |
| GET | `/exercises/{id}` | Chi tiết (câu hỏi + phương án, KHÔNG đáp án) | Đã đăng nhập |
| POST | `/exercises` | Tạo (MCQ / SIMULATION_LAB / CODE — ConfigJson) | Teacher/Admin |
| PUT | `/exercises/{id}` | Sửa | Teacher (của mình)/Admin |
| DELETE | `/exercises/{id}` | Xóa mềm | Teacher (của mình)/Admin |
| POST | `/exercises/{id}/submit` | Nộp bài → điểm + đáp án + giải thích; body có thể kèm `classAssignmentId` (nộp qua luồng lớp — v2.8, xem §4.11) | Student |
| POST | `/exercises/{id}/practice` | Luyện tập (không chấm điểm — FR-4.6) | Student |
| POST | `/exercises/import-csv` | Nhập câu hỏi hàng loạt CSV | Teacher/Admin |
| GET | `/exercises/{id}/submissions/me` | Lịch sử bài làm của tôi | Student |
| GET | `/exercises/{id}/submissions` | Danh sách bài nộp (giảng viên) | Teacher (của mình)/Admin |

**Ví dụ — GET /exercises/{id} (KHÔNG chứa đáp án)**

```json
{ "id": 31, "title": "Trắc nghiệm Bubble Sort", "type": "MCQ", "lessonId": 15,
  "nodeId": 22, "stage": 1, "durationMinutes": 10, "maxScore": 10, "status": "active",
  "questions": [
    { "id": 101, "content": "Sau vòng lặp ngoài đầu tiên của bubble sort, phần tử lớn nhất nằm ở đâu?",
      "type": "SINGLE", "options": ["Đầu mảng", "Cuối mảng", "Giữa mảng", "Không xác định"], "points": 2 } ] }
```

**Ví dụ — POST /exercises/{id}/submit**

```json
// Request
{ "answers": [ { "questionId": 101, "selected": [1] } ], "classAssignmentId": 55 }
// Response 200
{ "score": 2, "maxScore": 10,
  "results": [ { "questionId": 101, "correct": true, "correctAnswer": [1],
    "explanation": "Bubble sort đưa phần tử lớn nhất về cuối trong vòng lặp đầu tiên." } ],
  "submissionId": 2048, "submittedAt": "2026-08-09T12:34:56Z" }
```

## 4.7 Progress

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/progress/me` | Tiến độ tổng hợp của tôi | Student |
| GET | `/progress/me/lessons/{lessonId}` | Tiến độ 1 bài học | Student |
| GET | `/progress/report?lessonId=` | Báo cáo giảng viên | Teacher/Admin |
| GET | `/progress/report/export?lessonId=` | Xuất CSV (UTF-8 BOM) | Teacher/Admin |

**Ví dụ — GET /progress/me**

```json
{ "lessonsViewed": 4, "lessonsTotal": 12, "exercisesCompleted": 3, "exercisesTotal": 8,
  "avgScore": 7.7,
  "topics": [
    { "id": 5, "name": "Sắp xếp", "progressPct": 66,
      "lessons": [
        { "id": 15, "title": "Bubble Sort", "viewed": true, "bestScore": 9, "completed": true },
        { "id": 16, "title": "Quick Sort", "viewed": true, "bestScore": null, "completed": false } ] } ] }
```

## 4.8 Users (Admin)

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/users` | Danh sách (lọc role/status/q, phân trang) |
| GET | `/users/{id}` | Chi tiết |
| PUT | `/users/{id}/status` | Khóa/mở (`{isActive}`) — chỉ Admin chính được khóa Admin khác; cấm khóa Admin cuối cùng còn active |
| PUT | `/users/{id}/role` | Đổi vai trò (không sang Admin; Admin thường không đổi được role Admin khác — chỉ Admin chính) |
| POST | `/users/{id}/approve-teacher` | Phê duyệt/Từ chối Teacher (v2.8): body `{approve:bool, reason?}`; approve → role=TEACHER, IsActive=true; reject → role=STUDENT, IsActive=true + log lý do |
| POST | `/users/{id}/reset-password` | Đặt lại mật khẩu (Admin thường không reset được mật khẩu Admin khác — chỉ Admin chính) |
| DELETE | `/users/{id}` | Xóa tài khoản (ẩn danh hóa — NFR-35; cấm xóa Admin cuối cùng còn active) |

**Ví dụ — GET /users?role=TEACHER_PENDING**

```json
{ "items": [ { "id": 42, "displayName": "Trần Hà", "email": "t***@university.edu.vn",
  "role": "TEACHER_PENDING", "isActive": false, "createdAt": "2026-08-01T08:00:00Z" } ],
  "page": 1, "pageSize": 20, "total": 1, "totalPages": 1 }
```

## 4.9 Favorites & Misc

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/favorites` | Danh sách yêu thích | Đã đăng nhập |
| POST | `/favorites` | Thêm `{simKey, input?}` | Đã đăng nhập |
| DELETE | `/favorites/{id}` | Xóa | Đã đăng nhập (của mình) |

## 4.10 Admin

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/admin/stats` | Thống kê tổng |
| GET | `/settings` / PUT `/settings` | Đọc/cập nhật cấu hình |

## 4.11 Classes (Module H)

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/classes` | Danh sách lớp (của tôi) | Đã đăng nhập |
| POST | `/classes` | Tạo lớp (mã mời 6 ký tự tự sinh) | Teacher/Admin |
| GET | `/classes/{id}` | Chi tiết + thống kê tóm tắt | Teacher (của mình)/Admin/Student (đã tham gia) |
| PUT | `/classes/{id}` | Sửa lớp; Admin có thể chuyển quyền sở hữu body `{ownerId}` (v2.8 — lớp mồ côi) | Teacher (của mình)/Admin |
| DELETE | `/classes/{id}` | Xóa mềm | Teacher (của mình)/Admin |
| POST | `/classes/{id}/join` | Tham gia bằng mã mời `{inviteCode}` | Student |
| POST | `/classes/{id}/members` | Thêm sinh viên theo email | Teacher (của mình)/Admin |
| DELETE | `/classes/{id}/members/{userId}` | Xóa sinh viên | Teacher (của mình)/Admin |
| POST | `/classes/{id}/assignments` | Gán nội dung + hạn `{lessonId?, exerciseId?, dueAt}` | Teacher (của mình)/Admin |
| PUT | `/classes/{id}/assignments/{assignId}` | Sửa hạn/trạng thái | Teacher (của mình)/Admin |
| DELETE | `/classes/{id}/assignments/{assignId}` | Gỡ nội dung gán | Teacher (của mình)/Admin |
| GET | `/classes/{id}/report` | Báo cáo lớp (FR-8.4) | Teacher (của mình)/Admin |
| GET | `/classes/{id}/report/export` | Xuất CSV báo cáo lớp | Teacher (của mình)/Admin |

## 4.12 Cá nhân mở rộng

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/me/notes?lessonId=` | Danh sách ghi chú | Đã đăng nhập |
| PUT | `/me/notes/{lessonId}` | Lưu/cập nhật ghi chú bài học | Đã đăng nhập |
| DELETE | `/me/notes/{lessonId}` | Xóa ghi chú | Đã đăng nhập |
| PUT | `/auth/2fa` | 2FA email (FR-1.11): body `{enabled}` — `enabled:false` tắt trực tiếp; `enabled:true` trả 400 `OTP_REQUIRED` (phải qua /send + /verify) | Đã đăng nhập |
| POST | `/auth/2fa/send` | Gửi mã OTP 6 số qua email (hiệu lực 5 phút, dùng 1 lần) → `{message, expiresInSeconds}` | Đã đăng nhập |
| POST | `/auth/2fa/verify` | Xác nhận mã OTP `{code}` → bật 2FA → `{enabled:true}` | Đã đăng nhập |
| GET | `/achievements` | Huy hiệu của tôi (đã mở + ẩn) | Đã đăng nhập |

## 4.13 Code Runner (Module I)

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| POST | `/code-runs` | Lưu lần chạy code (chấm/trace chạy client sandbox — ADR-012) | Đã đăng nhập |
| GET | `/code-runs/{id}` | Trạng thái + tóm tắt lần chạy | Đã đăng nhập (của mình) |
| GET | `/code-runs/{id}/trace` | TraceEvent[] phân trang | Đã đăng nhập (của mình) |
| POST | `/exercises/{id}/code-submit` | Nộp bài code → `{score, passed, total, results[]}` | Student |
| GET | `/exercises/{id}/code-submissions` | Danh sách bài nộp code | Teacher (của mình)/Admin |
| GET | `/exercises/{id}/code-submissions/me` | Lịch sử nộp code của tôi | Student |

> Ghi chú FR-9.3 (v2.4): chấm điểm chạy trong sandbox Web Worker client — bộ test ẩn đóng gói kèm bundle nên CÓ THỂ bị trích xuất; mức cam kết "chống lười làm", KHÔNG cam kết chống trích xuất/giả mạo.

## 4.14 Gamification, Premium, Learning Path & Benchmark

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/me/hearts` | Trạng thái tim + thời điểm hồi tiếp theo | Đã đăng nhập |
| POST | `/learning-path/{id}/nodes/{nodeId}/enter` | Trừ 1 tim (atomic) + tạo/resume session (FR-10.1) | Student |
| GET | `/learning-path/{id}` | Bản đồ node: trạng thái, sao, % hoàn thành | Đã đăng nhập |
| GET | `/learning-path/{id}/final-test` | Lấy đề final test (trộn seed) | Student |
| GET | `/me/quests` | 5 quest hôm nay + tiến độ | Đã đăng nhập |
| POST | `/me/quests/{id}/claim` | Nhận thưởng quest (atomic) | Đã đăng nhập |
| GET | `/me/streak` | Streak hiện tại + freeze còn lại | Đã đăng nhập |
| GET | `/leaderboard?tab=week/level/class&classId=` | BXH phân trang | Đã đăng nhập |
| GET | `/shop/items` | Danh mục vật phẩm | Đã đăng nhập |
| POST | `/shop/buy` | `{itemId}` — trừ gems atomic, kiểm tra stack | Đã đăng nhập |
| GET | `/me/inventory` | Vật phẩm đã mua + trang bị | Đã đăng nhập |
| PUT | `/me/inventory/equip` | `{itemId, slot}` — set `IsEquipped=true` item, set false các item cùng loại (v2.9) | Đã đăng nhập |
| GET | `/premium/status` | Gói hiện tại + ngày hết hạn | Đã đăng nhập |
| POST | `/premium/upgrade` | `{planId}` (`1m`/`3m`/`12m`) → tạo đơn checkout QR MB Bank: OrderRef = `DSV{userId}T{months}` (VD DSV1002T3) + trả `contentRef` (nội dung CK hiển thị trên QR) — GP-T7 | Đã đăng nhập |
| POST | `/premium/mock-pay` | `{orderId}` → kích hoạt Premium sau xác nhận "Tôi đã chuyển khoản" + log giao dịch (OrderRef DSV...) | Đã đăng nhập |
| GET | `/cheatsheet?structure=` | Bảng Big-O + snippet + deep-link mô phỏng | Đã đăng nhập |
| POST | `/benchmarks/run` | `{keys[], sizes[], language?}` → kết quả đo nhiều n + fit lý thuyết | Đã đăng nhập |

**Ví dụ — POST /premium/upgrade (FR-10.7, GP-T7 — QR chuyển khoản MB Bank)**

```json
// Request: { "planId": "3m" }
// Response 200 — OrderRef DSV{userId}T{months} khớp nội dung CK hiển thị trên QR
{ "orderId": 1002, "planId": "3m", "expiresAt": "2026-11-12T00:00:00Z",
  "contentRef": "DSV1002T3" }
```

**Ví dụ — POST /learning-path/{id}/nodes/{nodeId}/enter (FR-10.1, v2.5)**

```json
// Response 200 (đã trừ tim)
{ "session": { "id": 5001, "nodeId": 22, "startedAt": "2026-08-12T08:00:00Z",
               "expiresAt": "2026-08-12T08:30:00Z", "stage": 1, "stepIndex": 12 },
  "heartsLeft": 9 }
// Response 200 (resume trong session — KHÔNG trừ tim)
{ "session": { "id": 5001, "nodeId": 22, "startedAt": "2026-08-12T07:58:00Z",
               "expiresAt": "2026-08-12T08:28:00Z", "stage": 2, "stepIndex": 5 },
  "heartsLeft": 9 }
// Response 403
{ "error": { "code": "HEARTS_EMPTY", "message": "Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium.",
             "field": null, "details": [] } }
```

## 4.15 Feedback & Bug reports

| Method | Endpoint | Mô tả | Quyền |
|---|---|---|---|
| GET | `/feedback?lessonId=` | Điểm TB + đếm đánh giá | Đã đăng nhập |
| POST | `/feedback` | Gửi/chỉnh sao + nhận xét `{lessonId, rating, comment?}` — 403 nếu chưa "Đánh dấu đã học" bài đó (v2.9) | Đã đăng nhập |
| POST | `/bug-reports` | Gửi báo cáo lỗi (tự đính kèm context) | Đã đăng nhập |
| GET | `/admin/bug-reports` | Danh sách báo cáo lỗi | Admin |
| PUT | `/admin/bug-reports/{id}` | Cập nhật trạng thái xử lý | Admin |

---

# 5. MA TRẬN QUYỀN (RBAC — 36 hành động)

| # | Hành động | API | STUDENT | TEACHER | ADMIN |
|---|---|---|---|---|---|
| 1 | Đăng ký / Đăng nhập | /auth/* | ✔ | ✔ | ✔ |
| 2 | Xem thông tin bản thân, đổi mật khẩu | /auth/me | ✔ | ✔ | ✔ |
| 3 | Xem chủ đề, bài học (đã kích hoạt) | GET /topics, /lessons | ✔ | ✔ | ✔ |
| 4 | Xem bản nháp / bài học chưa kích hoạt | GET /lessons/{id}?includeDraft | ✘ | ✔ (của mình) | ✔ |
| 5 | Tạo / sửa / xóa chủ đề | POST/PUT/DELETE /topics | ✘ | ✔ (xóa: chặn nếu có bài học) | ✔ |
| 6 | Tạo / sửa / xóa bài học | POST/PUT/DELETE /lessons | ✘ | ✔ | ✔ |
| 7 | Xem cấu hình mô phỏng (schema) | GET /simulations/{key}/schema | ✔ | ✔ | ✔ |
| 8 | Xem danh sách mô phỏng | GET /simulations | ✔ | ✔ | ✔ |
| 9 | Tạo / sửa / xóa bài tập | /exercises | ✘ | ✔ | ✔ |
| 10 | Làm bài tập, nộp bài | GET /exercises/{id}, POST /submit | ✔ | ✔ | ✔ |
| 11 | Xem lịch sử bài làm của bản thân | GET /exercises/{id}/submissions/me | ✔ | ✔ | ✔ |
| 12 | Xem bài nộp của người khác (bài của mình) | GET /exercises/{id}/submissions | ✘ | ✔ | ✔ |
| 13 | Xem tiến độ bản thân | GET /progress/me | ✔ | ✔ | ✔ |
| 14 | Xem báo cáo người học (bài học của mình) | GET /progress/report | ✘ | ✔ | ✔ |
| 15 | Quản lý người dùng (Admin thường không quản được Admin khác — chỉ Admin chính `IsPrimaryAdmin`) | /users | ✘ | ✘ | ✔ |
| 16 | Phê duyệt tài khoản Teacher | /users/{id}/approve | ✘ | ✘ | ✔ |
| 17 | Cấu hình hệ thống | /settings | ✘ | ✘ | ✔ |
| 18 | Thống kê hệ thống | /admin/stats | ✘ | ✘ | ✔ |
| 19 | Lưu yêu thích, xem yêu thích | /favorites | ✔ | ✔ | ✔ |
| 20 | Xem trang chủ công khai + demo | /public/* | ✔ | ✔ | ✔ |
| 21 | Ghi chú cá nhân | /me/notes | ✔ | ✔ | ✔ |
| 22 | Quản lý lớp học phần | /classes | ✘ | ✔ | ✔ |
| 23 | Tham gia lớp bằng mã mời | /classes/join | ✔ | ✔ | ✔ |
| 24 | Xem huy hiệu và thành tích | /achievements | ✔ | ✔ | ✔ |
| 25 | Đánh giá nội dung | /feedback | ✔ | ✔ | ✔ |
| 26 | Viết/chạy code trong sandbox | /code-runs | ✔ | ✔ | ✔ |
| 27 | Nộp bài tập lập trình | /exercises/{id}/code-submit | ✔ | ✔ | ✔ |
| 28 | Tạo bài tập lập trình (type=CODE) | /exercises | ✘ | ✔ | ✔ |
| 29 | Xem bài nộp code của lớp | /code-submissions | ✘ | ✔ | ✔ |
| 30 | Vào node (trừ tim), xem tim | /me/hearts, /learning-path/*/enter | ✔ | ✔ | ✔ |
| 31 | Làm Daily Quest và nhận thưởng | /me/quests | ✔ | ✔ | ✔ |
| 32 | Mua vật phẩm Shop bằng Gems | /shop/buy | ✔ | ✔ | ✔ |
| 33 | Nâng cấp Premium (checkout QR MB Bank) | /premium/* | ✔ | ✔ | ✔ |
| 34 | Xem Leaderboard | /leaderboard | ✔ | ✔ | ✔ |
| 35 | Chạy Benchmark Lab | /benchmarks/run | ✔ | ✔ | ✔ |
| 36 | Xem CheatSheet + deep-link | /cheatsheet | ✔ | ✔ | ✔ |

**Ví dụ — GET /me/hearts**

```json
// Response 200
{ "hearts": 3, "heartsMax": 10, "lastHeartAt": "2026-08-12T06:40:00Z",
  "nextHeartInSeconds": 1200 }
```

**Ví dụ — GET /learning-path/1**

```json
// Response 200
{ "id": 1, "title": "Sắp xếp & Tìm kiếm", "progressPct": 66,
  "nodes": [
    { "id": 22, "title": "Bubble Sort", "sortOrder": 1, "status": "passed",
      "stars": 2, "nodeScore": 82 },
    { "id": 23, "title": "Tìm kiếm nhị phân", "sortOrder": 2, "status": "active", "stars": 0 },
    { "id": 24, "title": "Luyện tập tổng hợp", "sortOrder": 3, "status": "locked", "stars": 0 },
    { "id": 25, "title": "Kiểm tra cuối lộ trình", "sortOrder": 4, "status": "locked", "stars": 0 } ] }
```

**Ví dụ — POST /exercises/{id}/code-submit (FR-9.3)**

```json
// Request
{ "code": "function bubbleSort(arr){ const r=[...arr]; for(let i=0;i<r.length;i++){...} return r; }",
  "exerciseId": 45 }
// Response 200 — chấm theo ĐẦU RA, kết quả từng test ẩn (mức cam kết FR-9.3 v2.4)
{ "score": 10, "passed": 10, "total": 11,
  "results": [
    { "testId": "h1", "passed": true, "input": "[3,1,2]", "expected": "[1,2,3]", "output": "[1,2,3]" },
    { "testId": "h2", "passed": false, "input": "[5,4,3,2,1]", "expected": "[1,2,3,4,5]", "output": "[1,2,3,4]" } ],
  "submissionId": 9012, "submittedAt": "2026-08-12T09:00:00Z" }
```

**Ví dụ — POST /shop/buy (FR-10.2)**

```json
// Request
{ "itemId": 3 }
// Response 200
{ "item": { "id": 3, "itemKey": "streak-freeze", "name": "Streak Freeze", "priceGems": 100 },
  "gemsLeft": 240, "owned": 2 }
// Response 422
{ "error": { "code": "INSUFFICIENT_GEMS", "message": "Không đủ gems để mua vật phẩm này",
             "field": null, "details": [] } }
```

**Ví dụ — POST /me/quests/{id}/claim (FR-10.3)**

```json
// Request
{}
// Response 200
{ "claimed": true, "reward": { "gems": 3, "xp": 20 }, "gemsTotal": 243 }
// Response 422 (claim lần 2)
{ "error": { "code": "QUEST_ALREADY_CLAIMED", "message": "Quest này đã được nhận thưởng",
             "field": null, "details": [] } }
```

**Ví dụ — POST /benchmarks/run (FR-3.20/3.20b)**

```json
// Request
{ "keys": ["sort.bubble", "sort.quick"], "sizes": [10, 50, 100, 200, 500],
  "language": "ts" }
// Response 200 — chế độ đo KHÔNG trace (runMeasure — SDD §4.0.3)
{ "results": [
  { "key": "sort.bubble", "measurements": [
      { "n": 10, "durationMs": 0.4, "comparisons": 45, "swaps": 20 },
      { "n": 500, "durationMs": 210.5, "comparisons": 124750, "swaps": 62375 } ] },
  { "key": "sort.quick", "measurements": [
      { "n": 10, "durationMs": 0.3, "comparisons": 25, "swaps": 9 },
      { "n": 500, "durationMs": 18.7, "comparisons": 4481, "swaps": 912 } ] } ],
  "fitted": { "sort.bubble": "O(n^2)", "sort.quick": "O(n log n)" },
  "conclusion": "Quick Sort nhanh hơn Bubble Sort rõ rệt ở n ≥ 100; độ lệch so với lý thuyết < 5%." }
```

**Ví dụ — GET /classes/{id}/report (FR-8.4)**

```json
// Response 200
{ "classId": 3, "className": "SD21361 — TH1", "totalMembers": 100,
  "assignments": [
    { "assignmentId": 11, "title": "Bubble Sort", "dueAt": "2026-08-20T23:59:00Z",
      "onTime": 55, "late": 12, "notSubmitted": 33, "avgScore": 7.2 } ],
  "laggingLearners": [ { "userId": 88, "displayName": "Nguyễn Văn A", "missingCount": 3 } ] }
```

## 4.16 Ví dụ — POST /exercises/{id}/submit (Lab Bậc 2 — chấm trạng thái cuối)

```json
// Request — mảng [3,1,2] sau loạt thao tác hoán đổi (Lab Sắp xếp)
{ "answers": [ { "questionId": 0, "labAnswer": {
    "finalState": [1, 2, 3], "stepsUsed": 3, "maxSteps": 4 } } ] }
// Response 200
{ "score": 3, "maxScore": 3, "passed": true,
  "results": [ { "questionId": 0, "correct": true,
    "explanation": "Trạng thái cuối [1,2,3] khớp chuẩn StepExecutor; 3 bước ≤ giới hạn 4 (chuẩn 3 × 1.5)." } ],
  "submissionId": 3001, "submittedAt": "2026-08-12T10:00:00Z" }
```

---

# 6. ĐẶC TẢ BỔ SUNG

1. **Đăng ký**: xác thực email domain nếu bật (`DOMAIN_NOT_ALLOWED`), policy mật khẩu (`WEAK_PASSWORD` kèm details).
2. **Nộp bài**: chống nộp trùng (idempotency key `Idempotency-Key` tùy chọn) + khóa đồng thời (422 `SUBMISSION_IN_PROGRESS`).
3. **CSV báo cáo**: `text/csv; charset=utf-8` kèm BOM; tên file `report_lessons_15_20260809.csv`.
4. **Throttling**: trả `Retry-After` khi 429; frontend "Quá nhiều yêu cầu, thử lại sau N giây".
5. **Log máy chủ**: thao tác nhạy cảm (users, lessons write, settings) ghi Serilog — KHÔNG UI xem nhật ký.

---

# 7. KIỂM THỬ HỢP ĐỒNG API (CONTRACT TEST — bắt buộc)

1. **Schema test**: mọi response khớp JSON schema đã đặc tả (FluentAssertions + schema file hoặc Zod phía test frontend).
2. **Status test**: mọi endpoint có bảng trạng thái kỳ vọng (200/201/400/401/403/404/409/422) — integration test.
3. **Nguyên tắc thay đổi**: thêm field = an toàn (minor); đổi kiểu/đổi tên/bỏ field = breaking (major) → bắt buộc v2 hoặc thỏa thuận.
4. **Lịch sử thay đổi API**: mỗi thay đổi ghi bảng: ngày, endpoint, loại thay đổi, phiên bản tài liệu, người duyệt.

---

# 8. THAY ĐỔI PHIÊN BẢN API

| Ngày | Endpoint | Loại thay đổi | Phiên bản | Người duyệt |
|---|---|---|---|---|
| 12/08/2026 | (tạo mới toàn bộ) | Tạo | 1.0 | Mai Tiểu Bảo |
| — | `POST /simulations/run` | CẮT (A-4) | — | đã cắt trước v1.0 |
| 13/08/2026 | `POST /premium/upgrade` | SỬA — bổ sung response `contentRef` (nội dung CK `DSV{userId}T{months}`); OrderRef đổi `MOCK-{guid}` → `DSV{userId}T{months}` (GP-T7) | 1.4 | Trần Viết Tâm Phúc |
| 13/08/2026 | `POST /premium/mock-pay` | SỬA — làm rõ: kích hoạt sau xác nhận "Tôi đã chuyển khoản" (GP-T7) | 1.4 | Trần Viết Tâm Phúc |


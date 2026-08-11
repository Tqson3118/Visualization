# Kế hoạch gỡ Mock/Fallback (Dữ liệu giả)

> **Cập nhật:** 2026-08-02
> **Mục tiêu:** Rà soát và loại bỏ các dữ liệu in-memory, mock, hoặc fallback để kết nối ứng dụng với Database & Logic thực tế.

## A. BACKEND — Dữ liệu giả / in-memory / token demo

### 🔴 Mức CAO (logic dữ liệu giả thực sự, cần thay bằng DB)

- [x] **A1:** `Stateless*Controller.cs` (Auth/Quiz/Payment/Gamification)
  - **Vấn đề:** Lõi dùng `_authStrategy` in-memory (`demo-user-001`, `demo@2024`), `EnsureUserInMemory`, `GetAllUsers`, verify `CurrentPassword != "Demo@2024"`. Toàn bộ luồng này chạy trên data trong bộ nhớ, không phải DB.
  - **Giải pháp:** Refactor sang dùng `UserRepository` + service DB thật, giữ compat route.
  - **Độ khó:** Cao
  - **Trạng thái:** Hoàn thành — 4 controller giờ đọc/ghi `ApplicationDbContext` + service production thật (bỏ mọi call strategy in-memory):
    - `StatelessAuthController` → `IAuthService` (register/login/refresh/logout/me); progress/profile/change-password/award-xp/demo-credentials đọc/ghi DB trực tiếp. Bỏ `demo-user-001`, Xóa `EnsureUserInMemory`/`ForceAddRefreshToken`/password hardcode. Demo login dùng DB user `demo@visualizationdsa.dev`/`Demo@2024` (seeded).
    - `StatelessQuizController` → xóa `QuizBankStrategy`; đọc quiz + chấm điểm từ `Quizzes`/`QuizQuestions` DB, persist `QuizAttempt` + XP thật.
    - `StatelessPaymentController` → xóa `StatelessPaymentStrategy`; persist `Order` thật, kích hoạt Premium qua `User.SetPremium()`. Giữ nguyên `StatelessAuthStrategy`/`QuizBankStrategy` vì `AdminController` còn dùng (+ đợi A2). Còn `StatelessAuthStrategy`/`QuizBankStrategy` (được `AdminController` dùng — sai. A2).
    - `StatelessGamificationController` → xóa `GamificationStrategy`; profile/badges/leadboard từ DB, award XP + badge logic thật.
  - **Đã xóa:** `Domain/Strategies/GamificationStrategy.cs`, `Domain/Strategies/StatelessPaymentStrategy.cs` + bỏ DI singleton tương ứng trong `AddAlgorithmStrategies` (giữ `QuizBankStrategy`+`StatelessAuthStrategy` vì `AdminController` còn dùng → A2).
  - **Test:** Backend 45/45 pass, build 0 lỗi; FE type-check + 822/822.
- [x] **A2:** `AdminController.cs:121-128,206-217,459`
  - **Vấn đề:** Thống kê/liệt user/quiz lấy từ `_authStrategy.GetAllUsers()` / `_quizBank.GetAllQuizzes()` (in-memory) thay vì query `ApplicationDbContext`. Số liệu dashboard KHÔNG phản ánh DB thật.
  - **Giải pháp:** Chuyển sang repository/query qua mediator.
  - **Độ khó:** Cao
  - **Trạng thái:** Hoàn thành — `AdminController` giờ chỉ dùng `ApplicationDbContext` (không còn fallback in-memory). Xóa hoàn toàn `_authStrategy`/`_quizBank` khỏi controller + constructor. Đồng thời xóa file `StatelessAuthStrategy.cs`, `QuizBankStrategy.cs` + bỏ DI singleton trong `AddAlgorithmStrategies`. Backend build 0 error, 45/45 test.
- [x] **A3:** `QuizRoomHub.cs:333-373`
  - **Vấn đề:** `MockQuestion` — câu hỏi quiz realtime được sinh mock cứng, chưa nạp câu hỏi thật.
  - **Độ khó:** Trung bình
  - **Trạng thái:** Hoàn thành — `QuizRoomHub` inject `ApplicationDbContext`, load câu hỏi từ `QuizQuestions` DB (theo `QuizId` Guid). Xóa class `MockQuestion` + các hàm helper `GetQuizQuestions`/`GetQuizTitle`/`GetQuizQuestionCount` in-memory. Backend build 0 error, 45/45 test.

### 🟡 Mức TRUNG BÌNH (fallback/demo like #, minor)

- [x] **A4:** `LessonController.cs:87`
  - **Vấn đề:** `leetCodeId = "two-sum"; // Default fallback for demo`
  - **Độ khó:** Thấp (Sẵn sàng sửa ngay)
  - **Trạng thái:** Hoàn thành — đã gỡ fallback `two-sum`. Test 45/45 pass.
- [x] **A5:** `GamificationController.cs:88`
  - **Vấn đề:** "If empty, generate fake ones for first time" — quest/XP fake
  - **Độ khó:** Trung bình
  - **Trạng thái:** Hoàn thành — xóa khối sinh quest ảo khi chưa có; trả về mảng thực từ DB (rỗng `[]` nếu chưa có quest). Backend 45/45.
- [x] **A6:** `MockCodeJudgeService.cs`
  - **Vấn đề:** Tồn tại nhưng không được DI (`Program.cs:204` dùng `PistonCodeJudgeService`). Là dead code.
  - **Giải pháp:** Xóa file hoặc dọn dẹp.
  - **Độ khó:** Thấp (Sẵn sàng sửa ngay)
  - **Trạng thái:** Hoàn thành — đã xóa file, không còn tham chiếu. Test 45/45 pass.
- [x] **A7:** `AiAssistantService.cs:27,61,81,85`
  - **Vấn đề:** Không phải stub — có gọi HTTP thật tới LLM, nhưng trả fallback chuỗi `[Lỗi...]`/message bảo trì khi chưa config API key / fail. Không throw, nên FE thấy success nhưng content là thông báo lỗi.
  - **Độ khó:** Trung bình
  - **Trạng thái:** Hoàn thành — khi thiếu/không hợp lệ API key → ném `NotImplementedException` (middleware map 501 `NOT_IMPLEMENTED`). Khi LLM trả lỗi/parse fail/kết nối lỗi → ném `HttpRequestException`/`InvalidOperationException` (500/409). FE từ nay nhận HTTP error thay vì success với content lỗi.

---

## B. FRONTEND — Fallback / mock local / endpoint lệch

### 🔴 Mức CAO (nối API thật hoặc sửa contract)

- [x] **B1:** `features/quiz-system/.../BackendQuizWorkspace.vue:214-229`
  - **Vấn đề:** `FALLBACK_QUIZZES` cứng — khi `loadQuizCatalog()` fail → hiện danh sách quiz giả + `isUsingFallback`. Đây là mock chính thức ở tầng view.
  - **Giải pháp:** Gỡ fallback khi backend ổn định, hoặc hiển thị rõ lỗi thay vì nạp fake.
  - **Độ khó:** Trung bình
  - **Trạng thái:** Hoàn thành — xóa `FALLBACK_QUIZZES`, `isUsingFallback`, fallback-notice. `effectiveQuizzes` → `store.quizCatalog`. Lỗi API hiện qua `store.backendQuizError` (store đã set sẵn).
- [x] **B2:** `codelabApi.ts` (features/codelabs/api)
  - **Vấn đề:** Dùng `axios.get('/api/codelabs/...')` so với `CodelabController` route `api/v1/codelabs` và apiClient dùng prefix `v1` → mismatch contract. FE gọi `/api/codelabs` trong khi api còn lại đều `/api/v1/...`.
  - **Giải pháp:** Chỉnh lại wrapper axios dùng chung base/prefix `apiClient`.
  - **Độ khó:** Thấp → Trung bình
  - **Trạng thái:** Hoàn thành — chuyển sang dùng `api` chuẩn của `apiClient.ts` (prefix `/api/v1`).
- [x] **B3:** 2 Controller `CodelabController` vs `CodelabsController`
  - **Vấn đề:** `CodelabController` (full CRUD route `api/v1/codelabs`) + `CodelabsController` (route `api/Codelabs`, chỉ get/submit/run). CodelabsController là legacy. FE gọi `codelabsApi` khác nhau 2 endpoint.
  - **Giải pháp:** Hợp nhất lại 1 route.
  - **Độ khó:** Trung bình
  - **Trạng thái:** Hoàn thành — đã xóa `CodelabsController`, gom Submit/Run/RevealHint về `CodelabController`. RevealHint đổi sang `POST {id}/hints/{hintIndex}/reveal` (hintIndex lấy từ URL). Xóa các DTO trùng.

### 🟡 Mức TRUNG BÌNH (fallback local, mang tính demo/không sai)

- [x] **B4:** `features/custom-input/store/useInputStore.ts:117`
  - **Vấn đề:** `generateDummyBubbleSortResult` — sinh kết quả giả khi không có frame animation.
  - **Độ khó:** Trung bình
  - **Trạng thái:** Gỡ fallback: khi API fail, set `apiErrorMessage` + `animationStore.clear()`. LƯU Ý: giữ hàm `generateDummyBubbleSortResult` làm test-fixture trong spec (đã không còn dùng ở production).
- [x] **B5:** `features/auth/services/statelessAuthApi.ts`
  - **Vấn đề:** Gọi `stateless-auth` demo-account — phối qua `A1`.
  - **Độ khó:** Theo A1
  - **Trạng thái:** Hoàn thành (theo A1) — gỡ raw-`fetch` trùng lặp `/api/v1/auth/logout` trong `logout` (giờ chỉ gọi `/concepts/auth/logout`). Contract type + route giữ nguyên, khớp controller backend đã refactor.

---

## C. ĐÃ OK (Ghi chú để tránh lãng phí thời gian review)

- **Codelab CRUD đầy đủ backend:** (`CodelabController` `/api/v1/codelabs` + testcases + templates + hints) — có thật; chỉ cần FE đồng bộ contract B2/B3.
- **AI Assistant:** có controller + service thật + route `api/v1/ai/chat`, `.../quota`. FE đang gọi đúng (`/ai`, quota). Lưu ý đang ăn fallback chuỗi khi key chưa có (A7).
- **PistonCodeJudgeService:** đã DI (không chạy mock).

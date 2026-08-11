# BÁO CÁO REVIEW — PRODUCTION_PROMPT.md (DSA-Visual)

| | |
|---|---|
| File được review | `docs/PRODUCTION_PROMPT.md` (386 KB, ~5.300 dòng) |
| Ngày review | 12/08/2026 |
| Phương pháp | `grill-with-docs` + `domain-modeling` + `database-designer` + `improve-codebase-architecture` (đã cài qua `npx skills add`) |
| Kết luận chung | **SEVERELY OVER-ENGINEERED → RESHAPE** (không KILL: đề tài tốt, kiến trúc lõi EDV + Registry + "1 màn 1 việc" đúng hướng, nhưng scope gấp ~2-3 lần sức 4 sinh viên trong 20 tuần) |

---

## TRẠNG THÁI XỬ LÝ CÁC VẤN ĐỀ (cập nhật 12/08/2026)

> Bản review này là ảnh chụp tại thời điểm 12/08/2026. Mọi khuyến nghị đã được vá vào `PRODUCTION_PROMPT.md` qua changelog **v2.3 → v2.5** (Phần 22). Bảng dưới là trạng thái từng vấn đề.

| Vấn đề | Mức | Trạng thái | Ghi chú (phiên bản vá) |
|---|---|---|---|
| G-1 Scope gấp 2-3 lần | 🔴 P1 | ✅ Đã vá | 20.0 mục 1 GIỮ NGUYÊN Module J (quyết định người dùng) + giải trình KPI §1.3 + 20 tuần/10 sprint (v2.3) |
| G-2 Module J xung đột KPI | 🔴 P1 | ✅ Đã xử lý | Giữ theo quyết định 20.0 mục 1; thêm giải trình KPI G3/G5 (v2.3) |
| G-3 12 FR cắt còn đặc tả | 🔴 P1 | ✅ Đã vá | Xóa khỏi Phần 3 + AUTH_SESSIONS khỏi DB (v2.3) |
| G-4 NOTIFICATIONS còn sót | 🔴 P1 | ✅ Đã vá | Xóa 4 chỗ: ERD/đặc tả/index/API (v2.3); grep xác nhận sạch (v2.5) |
| G-5 Lab chấm trace từng bước | 🟠 P2 | ✅ Đã vá | Chấm TRẠNG THÁI CUỐI + giới hạn bước ×1.5 (v2.3, §20.2.2 Màn 15.1) |
| G-6 Interpreter tự do rủi ro | 🟠 P2 | ✅ Đã vá | EDV cho code MẪU; Module I giới hạn signature cố định; bỏ Judge0 (v2.3, ADR-012) |
| G-7 KPI vs trừ tim | 🟠 P2 | ✅ Đã xử lý | Giải trình ngay dưới bảng KPI §1.3 (v2.3) |
| G-8 Seed 18 bài quá lớn | 🟡 P3 | ✅ Đã vá | 8 bài chất lượng cao + ~90 test ẩn; 10 bài còn lại backlog GĐ2 (v2.3, §19.6A) |
| G-9 CheatSheet PDF | 🟡 P3 | ✅ Đã vá | GIỮ gắn Premium (quyết định chốt v2.3) |
| D-1 TEACHERS ảo | 🔴 P1 | ✅ Đã vá | Users }o--o{ Classes : manages (OwnerId) (v2.3) |
| D-2 NOTIFICATIONS/AUTH_SESSIONS | 🔴 P1 | ✅ Đã vá | Cùng G-3/G-4 (v2.3) |
| D-3 Node luyện tập tổng hợp | 🟠 P2 | ✅ Đã vá | Trộn runtime theo seed, không lưu bảng riêng + Exercises.NodeId/Stage (v2.3/v2.4) |
| D-4 ERD thiếu bảng | 🟠 P2 | ✅ Đã vá | Tách 2 ERD đủ 31 bảng (v2.3 → 31 bảng v2.4 thêm NodeSessions) |
| D-5 Xóa mềm không nhất quán | 🔴 P1 | ✅ Đã vá | DeletedAt datetime2 NULL thống nhất mọi bảng + sửa SQL 10.8.1 (v2.3) |
| D-6 AvatarFileId vs AvatarUrl | 🟠 P2 | ✅ Đã vá | Chỉ AvatarUrl (v2.3) |
| D-7 Thiếu CHECK/unique | 🟠 P2 | ✅ Đã vá | CHECK ClassAssignments + unique LessonSimulations/UserQuests (v2.3) |
| D-8 8 bảng thiếu cột | 🟠 P2 | ✅ Đã vá | Đặc tả đầy đủ §10.2.25-10.2.28 + 10.2.29 (NodeSessions — v2.4) |
| D-9 Thiếu index | 🟠 P2 | ✅ Đã vá | Questions.ExerciseId, ClassAssignments(LessonId/ExerciseId), Users.LastActivityDate... (v2.3) |
| D-10 Naming không thống nhất | 🟡 P3 | ✅ Đã vá | PascalCase toàn bộ (EF Core) (v2.3) |
| D-11 TraceJson > 50MB | 🟡 P3 | ✅ Đã vá | Tách blob/file riêng (v2.3) |
| A-1 Backend 4 lớp + Repository | 🔴 P1 | ✅ Đã vá | 2 project, bỏ Repository, giữ Result<T>/FluentValidation (v2.3) |
| A-2 13 Service quá mảnh | 🟠 P2 | ✅ Đã vá | Gộp GamificationService + CodeRunnerService (v2.3, ADR-011) |
| A-3 ~26 màn không khả thi | 🟠 P2 | ✅ Đã xử lý | Giữ theo quyết định 20.0; 20.2.5 ước lượng ~26 màn build mới ~14 |
| A-4 POST /simulations/run | 🟠 P2 | ✅ Đã vá | Cắt hẳn khỏi v1 (v2.3) |
| A-5 Node Hub/Hồ sơ vs "1 màn 1 việc" | 🟡 P3 | ✅ Đã vá | Mỗi tab 1 component tách — ghi rõ trong SDD (v2.3, §8.0) |
| — | — | ✅ **22/23 đã vá** | 1 điểm còn phụ thuộc quyết định người dùng (giữ Module J) |

---

## PHẦN 1 — GRILL-WITH-DOCS: SCOPE & USE CASE (ƯU TIÊN CAO NHẤT)

### G-1. 🔴 P1 | Scope tổng thể gấp ~2-3 lần đồ án 4 người / 20 tuần
- **Bằng chứng**: 10 module A-J (§19.1), ~32 route / ~26 màn UI mới (§20.2.5), 10 CTDL × 15 GT, Practice Ladder 3 bậc, Benchmark Lab multi-n, Code Runner + sandbox Judge0, Gamification đầy đủ (Tim/Gems/Quest/Streak/XP/Leaderboard/Premium), Module H lớp học phần, seed 18 bài × (lý thuyết + mô phỏng EDV + 5-10 quiz + 1 lab + 1 code challenge) + ~270 test ẩn + 6 path × 5 node (§19.6A/19.6B).
- **Vì sao là vấn đề**: chính doc tự thừa nhận 16 tuần "bất khả thi" (20.1) và kéo lên 20 tuần — nhưng vấn đề không phải lịch, mà là khối lượng tuyệt đối. Riêng seed content 18 bài đã là 1 học kỳ cho 4 người. Viết đủ 8 file tài liệu theo §17.2 (~5.000-8.000 dòng tài liệu) cũng là thêm ~40% công việc nữa.
- **Khuyến nghị**: Cắt 2 giai đoạn đúng nghĩa: **MVP thật** (bỏ gamification, seed 8 bài, 12 GT) + GĐ2 nếu đủ thời gian. Các mục chi tiết ở dưới.

### G-2. 🔴 P1 | Module J (Gamification + Premium) là ngoài phạm vi đồ án học thuật — xung đột với chính KPI của doc
- **Bằng chứng**: §20.4 chốt "Tim bị trừ cho MỌI lượt vào node… vì nội dung là giá trị lõi độc quyền" — gating toàn bộ nội dung học sau 10❤ (hồi 30p/❤). Đối chiếu KPI §1.3: G3 (≥80% SV truy cập/tuần), G5 (UX ≥ 4.0/5), mục tiêu sư phạm §1.2.
- **Vì sao là vấn đề**: Hội đồng sẽ hỏi đúng 1 câu giết chết: *"Vì sao sinh viên học nội dung chính khóa lại bị chặn vì hết tim? Đây là đồ án giáo dục hay game gacha?"*. Checkout mô phỏng + job downgrade + Shop + GemTransactions + DailyQuests là đúng mẫu "enterprise feature without a clear business requirement" — chính doc đã cắt payment ở §2.2 (Out of Scope) nhưng lại đưa "Premium checkout mô phỏng" vào, tự mâu thuẫn.
- **Khuyến nghị (RESHAPE)**: Cắt toàn bộ FR-10.2 → 10.7 + Premium. **Giữ lại tối đa**: Streak + XP/Level hiển thị đơn giản (không shop, không tim, không quest thưởng). Nếu nhất định giữ Tim: KHÔNG chặn nội dung (chỉ chặn "nâng sao/review lại điểm" — cơ chế mềm), vì KPI sử dụng sẽ chết nếu chặn.

### G-3. 🔴 P1 | 12 FR "đã duyệt cắt" (19.7) VẪN còn đặc tả đầy đủ 7 thuộc tính trong Phần 3
- **Bằng chứng**: FR-1.10 (§3.1), FR-2.7/2.8/2.9 (§3.2), FR-3.13/3.17/3.19 (§3.3), FR-5.6/5.7 (§3.5), FR-6.4 (§3.6), FR-7.3/7.5 (§3.7) — mỗi cái vẫn giữ nguyên mô tả, luồng, ngoại lệ, AC; chỉ thêm dòng "⚠ ĐÃ CẮT". Bảng `AUTH_SESSIONS` (10.2.14) + ERD + index vẫn tồn tại dù FR-1.10 (quản lý phiên) đã cắt.
- **Vì sao là vấn đề**: AI sinh SRS/SDD phải xử lý ~12 khối mâu thuẫn liên tục, dễ sót hoặc sinh nhầm tài liệu cho tính năng đã cắt; checklist §17.9 sẽ fail ở dòng "ID khớp nhau".
- **Khuyến nghị**: Xóa hẳn khối đặc tả 12 FR cắt khỏi Phần 3 (chỉ giữ 1 dòng trong master matrix ghi "ĐÃ CẮT — không sinh đặc tả"); xóa `AUTH_SESSIONS` khỏi ERD/đặc tả/index/RBAC.

### G-4. 🔴 P1 | NOTIFICATIONS "đã xóa" nhưng vẫn nguyên trong ERD/đặc tả/index
- **Bằng chứng**: Changelog v2.2 dòng "Xóa toàn bộ NOTIFICATIONS (bảng DB, API 9.2.9, RBAC, controller/service, chuông UI, 19.3)" + §21 mục 5 + §20.0 mục 5 — NHƯNG ERD vẫn vẽ `USERS ||--o{ NOTIFICATIONS` (dòng ~2938), block `NOTIFICATIONS {...}` (~2972), đặc tả §10.2.13 (~3128), index (~3290).
- **Khuyến nghị**: Xóa 4 chỗ còn sót. AI sinh SDD theo doc hiện tại chắc chắn tạo bảng NOTIFICATIONS — vi phạm ngay quyết định 20.0 của chính doc.

### G-5. 🟠 P2 | Chấm Interactive Lab theo trace TỪNG BƯỚC (20.2.2 Màn 15.1) quá cứng nhắc
- **Bằng chứng**: "so khớp trace TỪNG BƯỚC (quá trình), KHÔNG chỉ so khớp trace cuối… đúng thao tác + đúng phần tử → pass bước; sai → làm lại từ bước sai".
- **Vì sao là vấn đề**: người học phải tái hiện đúng thứ tự thao tác của generator — cách làm khác hợp lệ (đúng kết quả nhưng khác trình tự) bị chặn → UX bực bội; engine đối chiếu quá trình tốn công gấp 3 lần so với chấm kết quả. Rủi ro cao nhất về "cảm giác chấm máy bất công" lúc demo.
- **Khuyến nghị**: Bậc 2 chấm **trạng thái cuối + giới hạn thao tác** (đúng kết quả, số bước ≤ chuẩn × 1.5). Chấm từng bước → backlog.

### G-6. 🟠 P2 | EDV với interpreter tự do (8.0) là rủi ro kỹ thuật #1 — cần giới hạn lại
- **Bằng chứng**: §8.0 yêu cầu interpreter TypeScript chạy code người học, tự sinh annotation từ AST, chặn vòng lặp vô hạn 50.000 event, sandbox Web Worker client + Judge0 server (S7, FR-9.4/9.6), 18 bài code challenge chấm theo output.
- **Vì sao là vấn đề**: viết interpreter an toàn cho subset ngôn ngữ + instrumentation + editor 2 chiều + sandbox là 2-3 dự án riêng. Đây là điểm dễ trượt tiến độ nhất, và hội đồng chỉ cần hỏi "code sinh viên dùng arr.sort() thì sao?" — doc đã trả lời "chạy được nhưng không visualize" (đúng, nhưng vậy còn ý nghĩa gì với điểm số?).
- **Khuyến nghị (RESHAPE, giữ tinh thần phản hồi hội đồng)**: EDV cho **code MẪU/template** (code có sẵn, gắn trace hook — vẫn thỏa "code đến đâu visual đến đó" cho 100% nội dung giảng dạy); Module I giới hạn "sửa tham số / hoàn thiện hàm theo signature cố định" chạy qua sandbox Web Worker (bỏ Judge0 server); không nhận code tự do tùy biến.

### G-7. 🟠 P2 | Mâu thuẫn KPI (§1.3) vs cơ chế trừ tim (§20.4)
- G3 "≥80% SV truy cập ≥1 lần/tuần" + G5 "UX ≥ 4.0" không thể đạt khi học viên bị chặn nội dung vì hết tim giữa buổi học. Chọn 1 trong 2: bỏ gating (khuyến nghị) hoặc hạ KPI + viết giải trình lý do kinh doanh vào SRS.

### G-8. 🟡 P3 | Seed 18 bài + 6 path × 5 node + ~270 test ẩn (19.6A/19.6B) — giảm ½
- Seed content là "phần thịt" để bảo vệ — giữ đúng hướng nhưng giảm về **8 bài seed chất lượng cao** (phủ đủ 5 nhóm CTDL chính) + ~90 test ẩn; số còn lại để backlog GĐ2.

### G-9. 🟡 P3 | Sót nhỏ: FR-2.9 (xuất PDF bài học) đã cắt nhưng 19.4 vẫn giữ "CheatSheet PDF" cho Premium
- Thống nhất: nếu cắt PDF thì cắt cả 2 chỗ (hoặc giữ CheatSheet PDF như tính năng Thấp độc lập, không gắn Premium).

---

## PHẦN 2 — DOMAIN-MODELING + DATABASE-DESIGNER: DOMAIN & DB 3 MỨC

### 2.1 Conceptual (thực thể & quan hệ)

**D-1. 🔴 P1 | Entity `TEACHERS` ảo trong ERD**
- ERD vẽ `TEACHERS }o--o{ CLASSES : manages` (dòng ~2952) nhưng danh sách 31 bảng không có `TEACHERS` — giảng viên là `USERS.Role = 1`. Sơ đồ tham chiếu entity không tồn tại → AI sinh SDD có thể tạo bảng lạ hoặc Mermaid fail.
- Sửa: `USERS }o--o{ CLASSES : manages (OwnerId)`.

**D-2. 🔴 P1 | NOTIFICATIONS + AUTH_SESSIONS dư thừa** — đã nêu ở G-3/G-4 (xóa khỏi ERD, đặc tả, index).

**D-3. 🟠 P2 | "Node luyện tập tổng hợp" (quiz trộn 3 bài) chưa được mô hình hóa**
- §19.6A nói mỗi path = 3 node bài học + 1 node luyện tập tổng hợp + final test = 5 node. Nhưng bảng `LEARNING_PATH_NODES` chỉ có `FinalTestId` (1 exercise) — quiz trộn cross-lesson không có chỗ lưu (đề trộn runtime có seed thì không cần bảng, nhưng phải ghi rõ trong SDD: "đề trộn sinh runtime theo seed, KHÔNG lưu trước").
- ERD block `EXERCISES` (~2966) chưa có cột `NodeId`/`Stage` dù §10.2.25 bổ sung — ERD stale so với đặc tả.

**D-4. 🟠 P2 | ERD tuyên bố 31 bảng nhưng chỉ vẽ ~17 entity**
- Thiếu: `LearningPaths`, `LearningPathNodes`, `DailyQuests`, `UserQuests`, `ShopItems`, `UserInventory`, `GemTransactions`, `PremiumSubscriptions`, `CodeRuns`, `CodeSubmissions`, `Achievements`(có), `UserAchievements`(có), `BugReports`(có)... Khuyến nghị tách 2 sơ đồ: (1) lõi học tập, (2) gamification/code — đủ rõ và đúng yêu cầu chấm điểm "SDD §7 ERD".

### 2.2 Logical (chuẩn hóa, PK/FK, ràng buộc)

**D-5. 🔴 P1 | Xóa mềm không nhất quán — SQL báo cáo query cột KHÔNG TỒN TẠI**
- `TOPICS` dùng `IsDeleted` (bit), `LESSONS`/`EXERCISES`/`CLASSES` dùng `DeletedAt` (datetime2 NULL) — nhưng SQL mẫu §10.8.1 query `WHERE l.IsDeleted = 0` trên `LESSONS` (cột không tồn tại!), và §5.3 chính sách nói chung chung "xóa mềm (IsDeleted)".
- Khuyến nghị: chọn 1 convention — `DeletedAt datetime2 NULL` cho tất cả + `WHERE DeletedAt IS NULL` (đúng chuẩn soft-delete, giữ thông tin thời điểm xóa).

**D-6. 🟠 P2 | `USERS.AvatarFileId` (ERD) vs `AvatarUrl` (§10.2.1)**
- ERD khai báo `int? AvatarFileId` (không có bảng File), đặc tả cột dùng `AvatarUrl nvarchar(500)`. Thống nhất `AvatarUrl` (xóa AvatarFileId khỏi ERD).

**D-7. 🟠 P2 | Thiếu CHECK/unique cụ thể**
- `CLASS_ASSIGNMENTS`: "ít nhất 1 trong 2 (LessonId/ExerciseId) ≠ null" → cần `CHECK (LessonId IS NOT NULL OR ExerciseId IS NOT NULL)` — ghi trong đặc tả.
- `LESSON_SIMULATIONS` unique (LessonId, SimulationKey) chưa có index trong §10.3.
- `USER_QUESTS`: §10.2.26 khai UNIQUE(UserId, QuestDate, QuestId) nhưng §10.3 chỉ ghi (UserId, QuestDate) → bổ sung QuestId.

### 2.3 Physical (kiểu dữ liệu, index)

**D-8. 🟠 P2 | 8 bảng thiếu đặc tả cột đầy đủ — vi phạm quy tắc của chính doc**
- `LEARNING_PATHS`, `LEARNING_PATH_NODES`, `DAILY_QUESTS`, `USER_QUESTS`, `SHOP_ITEMS`, `USER_INVENTORY`, `GEM_TRANSACTIONS`, `PREMIUM_SUBSCRIPTIONS` (§10.2.25-10.2.28) chỉ có 1-2 dòng mô tả, không có bảng cột (tên/kiểu/ràng buộc/mặc định/ghi chú) như §10.2 mở đầu yêu cầu "với MỖI bảng". §17.3.2 cũng stale: "14 bảng đầy đủ cột" phải là 31.

**D-9. 🟠 P2 | Thiếu index quan trọng**
- `QUESTIONS.ExerciseId` — JOIN khi load câu hỏi theo bài tập (quan trọng nhất, thêm ngay).
- `CLASS_ASSIGNMENTS(LessonId)`, `CLASS_ASSIGNMENTS(ExerciseId)` — báo cáo lớp.
- `USERS.LastActivityDate` — job streak 00:30 quét theo ngày.

**D-10. 🟡 P3 | Naming convention không thống nhất**
- Bảng viết hoa (`USERS`) trong ERD/SQL nhưng entity EF Core sẽ là `Users`; cột `MixedCase` (CreatedBy) trong khi một số chỗ dùng snake_case (`user_id` trong ví dụ SQL §database-designer tham chiếu). Chọn 1 convention (khuyến nghị PascalCase cho EF Core) và ghi chú thống nhất.
- `REFRESH_TOKENS.CreatedByIp nvarchar(45)` — hợp lý (IPv6), giữ.

**D-11. 🟡 P3 | `CODE_RUNS.TraceJson` 50.000 event GZIP trong nvarchar(max)**
- Khả thi nhưng nên ghi ngưỡng: > 50MB → tách file blob; hoặc giảm event cap. Ghi rõ trong SDD để khỏi bất ngờ lúc test.

---

## PHẦN 3 — IMPROVE-CODEBASE-ARCHITECTURE: ĐƠN GIẢN HÓA KIẾN TRÚC CHO ĐỒ ÁN

### A-1. 🔴 P1 | Backend 4 lớp + Repository pattern = enterprise architecture thừa cho quy mô này
- §11.1: `Api / Application / Domain / Infrastructure` 4 project + Repository + Result<T> + FluentValidation (+ tùy chọn MediatR).
- **Deletion test**: xóa tầng `Domain`/`Infrastructure` tách rời → complexity tập trung vào Application? Không — Service gọi DbContext trực tiếp vẫn gọn. `Repository` layer là **shallow module** điển hình: interface gần như bằng implementation (mỗi truy vấn viết 2 lần: interface + EF), không thêm khả năng test (DbContext đã mock được), không thêm locality.
- **Khuyến nghị**: 2 project (`DsaVisual.Api` + `DsaVisual.Application`), bỏ Repository pattern (Service + DbContext trực tiếp qua `DbSet`, giữ `AsNoTracking` cho đọc), GIỮ `Result<T>` + FluentValidation + ErrorCodes (có giá trị thật cho chuẩn hóa lỗi tiếng Việt).

### A-2. 🟠 P2 | 13 Service quá mảnh (11.4)
- `HeartsService/GemService/QuestService/PremiumService/AchievementService` → gộp 1 `GamificationService` (hoặc cắt theo G-2). `CodeRunnerService + CodeSubmissionService` → gộp. `ClassService` có thể gộp báo cáo vào `ProgressService` (20.5 đã định nghĩa báo cáo lớp = số liệu từ E).

### A-3. 🟠 P2 | ~26 màn UI mới + 32 route với 2 SV frontend trong 20 tuần — không khả thi
- Kiến trúc Vue 3 (Pinia + composables + engines core tách riêng) là ĐÚNG HƯỚNG (giữ). Vấn đề chỉ là số lượng màn — giảm theo các quyết định cắt ở Phần 1 (mỗi FR cắt = 1-2 màn bớt đi).

### A-4. 🟠 P2 | `POST /simulations/run` (9.2.5) đánh dấu "tương lai" nhưng nằm trong endpoint list + RBAC + 9.3.5
- Cắt hẳn khỏi v1 (ADR-001 đã quyết định sinh bước ở frontend) — giữ trong doc gây nhầm lẫn cho AI sinh API_REFERENCE và cho người kiểm thử.

### A-5. 🟡 P3 | Mâu thuẫn nhẹ: "1 màn = 1 việc" (7.0) vs Màn 31 Node Hub (3 tabs) + Màn 32 Hồ sơ (4 tabs)
- Doc đã tự giải thích "mỗi tab = 1 component tách" — chấp nhận được, nhưng cần 1 câu ghi rõ trong SDD để AI sinh không mô tả Node Hub như màn gộp chức năng (đúng bài học bản cũ).

### Điểm sáng GIỮ NGUYÊN (không review xóa)
1. EDV + StepExecutor cho code mẫu (đã RESHAPE ở G-6).
2. Registry plugin `SimulationGenerator`/`Renderer` + `engines/catalog.ts` nguồn duy nhất (AC-3) — kiến trúc đẹp cho đồ án.
3. Nguyên tắc "1 màn 1 việc" + Two-way sync bằng deep-link `?step=N` (FR-2.11) — thông minh, tôn trọng quy tắc 7.0.
4. Golden data test + seed idempotent + bộ dữ liệu N1-N7 (§8.8) — chuẩn kiểm thử.
5. Error code catalog + Result<T> + message tiếng Việt — nhất quán và dễ chấm điểm.
6. Ma trận truy vết §17.15 (FR → UC → API → DB → TEST) — đầy đủ, là điểm cộng khi bảo vệ.

---

## PHẦN 4 — SCOPE TỐI GIẢN ĐỀ XUẤT (cho 4 SV, 16-20 tuần)

| # | Cắt ngay (tốn ít công, lợi lớn) | Lý do |
|---|---|---|
| 1 | Toàn bộ Module J trừ XP/Streak hiển thị (FR-10.2 → 10.7, Premium, Shop, Quest, Tim gating) | G-2: ngoài phạm vi học thuật, xung đột KPI |
| 2 | Xóa 12 FR đã cắt khỏi Phần 3 + `AUTH_SESSIONS` + `NOTIFICATIONS` (ERD/đặc tả/index/API/RBAC) | G-3/G-4, D-2: chống AI sinh nhầm |
| 3 | Interpreter tự do → EDV cho code mẫu; bỏ Judge0 server (chỉ Web Worker client) | G-6: rủi ro kỹ thuật #1 |
| 4 | Lab chấm kết quả cuối (không chấm trace từng bước) | G-5: đơn giản, công bằng |
| 5 | Benchmark 1 kích thước (bỏ multi-n + overlay lý thuyết) | giảm ½ công Benchmark |
| 6 | Seed 18 → 8 bài, ~270 → ~90 test ẩn | G-8: phần thịt giữ nguyên |

| # | Giữ (điểm sáng) | Ghi chú |
|---|---|---|
| 1 | EDV cho code mẫu + Registry + golden data | trái tim đồ án |
| 2 | Practice Ladder 3 bậc (chấm đơn giản hóa) | killer feature khi demo |
| 3 | Lớp học phần + báo cáo giảng viên (Module H/E) | dễ chấm điểm, giá trị thực |
| 4 | "1 màn 1 việc" + deep-link 2 chiều | khắc phục đúng phản hồi bản cũ |
| 5 | Kiến trúc 2 project backend (bỏ Repository/Domain/Infrastructure) | A-1 |

**Verdict cuối (theo grill-with-docs)**: `RESHAPE` — không cần KILL đề tài. Sau khi cắt Phần 4, scope còn lại vẫn đủ ấn tượng cho đồ án cao đẳng, và tài liệu sinh ra từ prompt sẽ nhất quán, không còn mâu thuẫn nội tại.

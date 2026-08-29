# 🔄 HANDOFF_SESSION — Bàn giao phiên làm việc Ngân hàng Đề thi docs/test-bank

> **Phiên gốc:** session-5179639043fa (2026-08-27) · **Workspace:** `D:\FPT\neww`
> **File này thay thế HANDOFF.md cũ** (đã xóa — nội dung hợp nhất vào đây).
> **Trạng thái: ✅ TOÀN BỘ CÔNG VIỆC BẮT BUỘC ĐÃ HOÀN TẤT & VERIFIED.** Phiên mới chỉ cần đọc mục "Trạng thái cuối" + "Facts đã thẩm định" để làm việc tiếp, KHÔNG cần thẩm định lại.

---

## 1. BỐI CẢNH

User yêu cầu review `docs/test-bank` (ngân hàng đề thi kiểm tra hiểu codebase DSA Visual: 19 đề chuyên sâu + 1 Final, cấu trúc 5 câu TN + 2 câu TL/đề) theo 5 tiêu chí: bao quát / chính xác / giải thích / người 0 kiến thức có hiểu không / chuẩn sư phạm.

**Kết quả review (~110 claims, 5 subagent + ~60 kiểm chứng trực tiếp):**
- Đề 14–19: 32/33 đúng 🟢
- Đề 01–13: ~40–70% đúng 🔴 — tổng cộng **~40 lỗi thật**: đáp án sai endpoint, snippet "Mã nguồn thực tế" bịa, feature chưa implement mô tả như có, mô hình hearts/level sai toàn diện
- Meta: TOC ghi "13 Bộ đề" (thật 19), 2 file .docx cũ thiếu Đề 14–19, MAPPING lỗi thời, **88.6% đáp án là B**, distractor vô lý ("nhạc quốc ca", "khung giờ vàng")
- User phản hồi: *"mấy câu dài dài là biết đáp án đúng luôn"* → yêu cầu chống đoán đáp án

**User chọn phương án (b):** vá trực tiếp + làm mới TOC/mapping + xuất lại docx + xáo trộn đáp án. → ĐÃ LÀM XONG TOÀN BỘ.

---

## 2. TRẠNG THÁI CUỐI (đã verify 2026-08-27 ~16:30)

| Hạng mục | Trạng thái |
|---|---|
| Vá ~40 lỗi errata (Đề 01–13, 14.4, Final, Đề 08, Đề 06 sửa sâu) | ✅ Xong |
| TL 7.1 / TL 13.2 / TL 19.2 / 10.3 / 11.5 — các lỗi agent sót, đã vá tay | ✅ Xong |
| Xáo trộn đáp án 105 câu: **A=26, B=27, C=26, D=26** (~25%/chữ) | ✅ Xong |
| Distractor viết lại "sai có căn cứ", cân bằng độ dài | ✅ Xong |
| TOC + header "19 Bộ đề + Final" (2 file .md) | ✅ Xong |
| `MAPPING_VA_PHAN_TICH_GAP.md` viết lại hoàn toàn | ✅ Xong |
| 2 file .docx xuất lại bằng pandoc (đủ Đề 14–19 + mọi fix) | ✅ Xong |
| Tool `tools/shuffle-test-bank-answers.mjs` (seeded) | ✅ Xong |
| Kiểm tra toàn vẹn cuối: 105 câu đủ 4 options ABCD, 105 key khớp, 0 từ hỏng, 0 mâu thuẫn hearts model | ✅ Xong |

**Files trong docs/test-bank:** 2 file .md (source of truth) + 2 file .docx (export) + MAPPING_VA_PHAN_TICH_GAP.md + file này. **Không có ERRATA.md** (đã vá thẳng vào đề).

**Lệnh verify nhanh nếu cần:**
```powershell
cd D:\FPT\neww
# integrity: đếm câu/options/phân bố — pattern trong section "Process warnings" dưới đây
pandoc 'docs/test-bank/DE_THI_KIEN_THUC_CODEBASE_DSA.md' -o 'docs/test-bank/DE_THI_KIEN_THUC_CODEBASE_DSA.docx'   # re-export khi sửa .md
pandoc 'docs/test-bank/DAP_AN_VA_GIAI_THICH_CHI_TIET.md' -o 'docs/test-bank/DAP_AN_VA_GIAI_THICH_CHI_TIET.docx'
node tools/shuffle-test-bank-answers.mjs 20260827   # CHỈ chạy sau khi vá regex diacritics (mục 5.3)
```

---

## 3. LỊCH SỬ SỬA CHÍNH (tóm tắt — để hiểu đề thi đã đổi gì)

### Đề 01–02 (Auth + Home/Router)
- 1.1: guard guestOnly redirect về `/` (route home) — KHÔNG phải /path. TL 1.1: `EMAIL_EXISTS` + HTTP **409**. TL 1.2: token **64 bytes base64url + SHA-256 hash, hạn 30 phút**; view chỉ đọc `token` query (không email). 2.1: 3 demo keys = `sort.bubble`+`search.binary`+`graph.bfs`. TL 2.1: `GET /concepts/courses`. TL 2.2: role-guard → `/profile` nếu đã login, **KHÔNG có toast**.

### Đề 03–05 (Simulator + Explore + Sandbox)
- 3.2: snippet thật từ `stores/simulation.ts` (interval = max(75, 1200/speed)); useSimulation là thin wrapper ủy quyền store. 3.5: favorites body `{ simKey, input? }`. TL 3.1: Step interface thật. TL 3.2: ManualPracticePanel radio options + inferExpected. 4.1: **3 route độc lập** (/simulations, /cheatsheet, /benchmark/:k1/:k2) không phải 3 tab. 4.3: `complexityTone()` thật (SimulationsView.vue:195). 4.5: CATALOG + referenceLinks (không có snippet 4 ngôn ngữ). TL 4.2: `runMeasureInWorker()`. Đề 05: chỉ nâng distractor (4 route → chung SortingSandboxView = views/sorting/SortingView.vue).

### Đề 06–09 (đã sửa 2 vòng — vòng 2 vá những chỗ agent sót)
- 6.2: enroll = localStorage thuần client. 6.3: stepper thật = [Lý thuyết, Quiz, Code Lab] theo `sandboxType`; visualizer nhúng TRONG LessonStepTheory qua `simulation-key` (không phải step riêng). 6.4: nút hoàn thành → `markLessonCompleted` (localStorage + `POST /concepts/auth/award-xp`) → `syncToServer` (`POST /concepts/auth/progress/{lessonId}`) → modal `+{{ xpReward }} XP`. **mark-viewed là của LessonView/LessonDetail cũ, không phải luồng này!** TL 6.1/6.2 viết lại theo flow thật.
- 7.3 + TL 7.1: **submit KHÔNG trừ tim** — `SubmitResultDto` = {score, maxScore, passed, results[], submissionId, submittedAt}, KHÔNG có heartsLeft; tim trừ khi enter node.
- 8.3: Jint timeout **1500ms** (DefaultTimeoutMs) + 32MB. 8.5: `GET /exercises/{id}/submissions/me`. 8.4 giữ nguyên (ControlBar có slider + speed thật).
- 9.x: tabs Tuần/Level/Lớp; ECharts bar chart; level `1+floor(sqrt(xp/100))` (ví dụ 380+30=410→L3); quest claim `POST /me/quests/{id}/claim`; reset 00:00 **giờ VN** (UTC+7); leaderboard `GET /api/v1/leaderboard?tab=`.

### Đề 10–13 (đã sửa 2 vòng)
- 10.2: shop = 5 avatar + 3 frame (SeedData.cs:118-128). 10.3: Premium = HeartsMax 30 + hồi 10 phút/tim + lazy downgrade; **không miễn trừ trừ tim** (đã vá lại claim sai "IsPremium bỏ qua trừ tim" + snippet bịa trong ExerciseService). 10.4: VietQR `DSV{userId}T{months}`, MB Bank, countdown 60s, backend mock-pay.
- 11.3: `PUT /me/inventory/equip` body `{ ItemId, Slot? }`. 11.4: field `DisplayName`. 11.5: duyệt teacher qua `POST /api/v1/users/{id}/approve-teacher` (UsersController prefix `api/v1/users`, KHÔNG phải /admin/users).
- 12.4: CSV client-side (BOM UTF-8), không Excel. 13.1: search @keyup.enter, không debounce. 13.3: `PUT /users/{id}/status` `{ IsActive }`, không revoke token. 13.4: stats = counts + ActiveUsersToday. 13.5 + TL 13.2: settings thật = SiteName/AllowedDomains/PasswordPolicy/UploadMaxMb/SandboxSeconds/SandboxMemoryMb; route API `PUT /api/v1/settings` (trang FE là /admin/settings); **SandboxSeconds là cấu hình client-side (ADR-012) — CodelabJudgeService dùng hằng số riêng 1500ms/32MB**.
- Lỗi agent đã vá tay ở vòng 2: 10.3 snippet bịa `if (!isCorrect && !userGamification.IsPremium)`; 11.5 route /admin/users sai; 7.3 + TL 7.1 mô hình tim sai; TL 19.2 heartsLeft/OutOfHeartsModal không tồn tại (thay bằng 403 HEARTS_EMPTY + toast PathView.vue:146); TL 13.2 premise "cấu hình hồi tim" không tồn tại.

### Đề 14–19 + Final
- 14.4: `UserProgress.Viewed` (bỏ `LessonProgress.Status == Completed` + `HasViewedAsync` không tồn tại).
- Final F.1–F.10: distractor viết lại; F.5/F.10 mô hình hearts/Premium đúng; TL F.1–F.3 theo flow thật (register 10 tim, mark-viewed→quest claim, phí node, approve-teacher, SettingsCache).

---

## 4. FACTS ĐÃ THẨM ĐỊNH (dùng trực tiếp — ĐỪNG thẩm định lại)

**Mô hình Tim/Level (quan trọng nhất — từng bị viết sai 6 chỗ):**
- Tim trừ khi **ENTER NODE session**: `UPDATE Users SET Hearts = Hearts - 1 WHERE Id=@id AND Hearts > 0` (GamificationService.cs:287-288); node đã PASS vào lại miễn phí; **Premium vẫn bị trừ** — chỉ HeartsMax 30 (Free 10) + hồi 10 phút/tim (Free 30 phút/tim) (L17); hết hạn → `EnsureHeartsMaxSyncAsync` clamp về Free (L1191-1204)
- Hết tim vào node mới → **403 + HEARTS_EMPTY** (ErrorCodes.cs:39,79) → toast warning PathView.vue:146; KHÔNG có OutOfHeartsModal
- Level = `1 + floor(sqrt(xp/100))` (GamificationService.cs:1431); L2=100XP, L3=400XP
- Quest claim: `POST /api/v1/me/quests/{id}/claim` atomic (GamificationService.cs:595-597); reset 00:00 VN (`UtcToday.AddHours(-7)`, L483); 2E+2M+1H/ngày
- Submit quiz: KHÔNG trừ tim; `SubmitRequest { answers:[{questionId, selected:number[]}], durationSeconds?, classAssignmentId?, clientRequestId? }`; `SubmitResultDto { score, maxScore, passed, results[], submissionId, submittedAt }`

**Luồng học bài (Đề 06):**
- Store thật: `frontend/src/features/lesson/store/useLessonStore.ts` (stores/lesson.ts chỉ markViewed cho view cũ)
- Hoàn thành: `markLessonCompleted` (localStorage `dsa.completedLessons` + awardXp `POST /concepts/auth/award-xp` check `xpAwarded < totalXp`) → `syncToServer` (`POST /concepts/auth/progress/{lessonId}`, retry 10s) → LessonCompletionModal `+{{ xpReward }} XP`
- Backend award-xp: ConceptsController.cs:1226; mark-viewed: LessonService.cs:409-458 upsert **UserProgress** (bảng UserProgress, KHÔNG phải UserLessonProgress)
- LessonStudyView stepper: FULL_STEPS [Lý Thuyết, Quiz, Code Lab] (L263-275) theo sandboxType dsa/quiz/codelab
- Enroll: `useCourseStore.enrollCourse` localStorage `enrolled_{courseId}` (useCourseStore.ts:79-88); courses `GET /concepts/courses` (courseApi.ts:123)

**Endpoint thật (tra nhanh):** favorites POST `/favorites` body `{simKey, input?}` · exercises submit `/exercises/{id}/submit` · code-submit `/exercises/{id}/code-submit` · history `/exercises/{id}/submissions/me` · claim quest `/me/quests/{id}/claim` · leaderboard `/leaderboard?tab=` · equip `PUT /me/inventory/equip` · update profile `PUT /auth/me` field DisplayName · approve-teacher `POST /users/{id}/approve-teacher` · block `PUT /users/{id}/status` `{IsActive}` · settings `GET/PUT /settings` (API) vs `/admin/settings` (trang FE, router L368) · premium `/premium/status|upgrade|mock-pay` · award-xp `/concepts/auth/award-xp` · progress `/concepts/auth/progress/{lessonId}`

**Engine (Đề 18):** Step thật `{index, structure, explanation, pseudocodeLine, highlights, annotations, variables, stats, version:1}` (engines/core/types.ts:26-36) · Jint DefaultTimeoutMs=1500 + MaxMemoryBytes=32MB (CodelabJudgeService.cs:33-35, gọi tại ExerciseService.cs:823 không truyền timeoutMs) · SandboxSeconds/SandboxMemoryMb = client-side (ADR-012, SettingService.cs:46) · 3 demo keys: sort.bubble, search.binary, graph.bfs (catalog.ts:57,64,88) · registry: Map + factory, instance mới mỗi lần · catalog.spec.ts so 44 keys với shared/simulation-catalog.json

**Khác:** guestOnly guard → {name:'home'} (router:417-418); role-guard → /profile nếu login (411-413) không toast; EMAIL_EXISTS=409; reset token 64 bytes base64url + hash, 30 phút; 4 sandbox route chung SortingSandboxView (router:175-197), App.vue:68 `:key="route.fullPath"`; shop seed 8 items (SeedData.cs:118-128); VietQR DSV{userId}T{months} + 60s countdown (PremiumView.vue:2-8,34-77); join code 6 ký tự A-Z0-9; CSV export ClassReportView.vue:87-88; AdminUsersView search @keyup.enter + tab pending TEACHER_PENDING; complexityTone SimulationsView.vue:195; SettingsCache singleton double-checked lock (SettingsCache.cs)

---

## 5. CẢNH BÁO QUY TRÌNH (tránh lặp lỗi)

### 5.1 Edit tool
- Luôn `read` file ngay trước `edit` (tracker) — sau subagent chạy song song PHẢI read lại toàn bộ
- Dòng dài trong read bị cắt hiển thị — lấy text chính xác bằng `read(offset:N, limit:1)` + JSON.stringify
- Edit bịa cũ từng fail vì đoán text: luôn copy từ read

### 5.2 Shuffle script (`tools/shuffle-test-bank-answers.mjs`)
- Seeded mulberry32, seed 20260827; id format `F.N`/`N.M`; quét 12 dòng quanh câu hỏi
- **BUG CHƯA VÁ**: regex remap `/(?<![A-Za-z0-9])([A-D])(?![A-Za-z0-9#+\-])/g` làm hỏng từ tiếng Việt bắt đầu bằng A-D viết hoa ("Các"→"Bác"...). Đã sửa tay 23 từ sau lần chạy. **TRƯỚC KHI CHẠY LẦN TỚI: thêm negative lookahead tránh dãy ký tự diacritics** (àáảãạăằ... + uppercase) sau `([A-D])`. Nếu không vá mà chạy → phải sửa tay lại từ hỏng (quy trình: dictionary từ vùng không-remap + đối chiếu ngữ cảnh từng từ Nghi Vấn trong segment MCQ)

### 5.3 Đề thi sau shuffle
- Key letter KHÔNG còn mặc định B — đọc heading hiện tại để biết chữ đúng
- Integrity check: 105 câu × 4 options ABCD liên tiếp; phân bố A/B/C/D; 0 từ hỏng (check: Aác, Aấc, Aậm, Aễ, Dậc, Aữ, Aạn, Aờ, Dả, Cậc — Bác/Bờ hợp lệ ngoài segment MCQ)

### 5.4 Docx
- .md là source of truth; .docx chỉ là export pandoc — SAU MỌI SỬA .md phải xuất lại cả 2 file docx
- Verify docx: giải nén word/document.xml, trích `<w:t[^>]*>([^<]*)</w:t>` join, check marker

---

## 6. VIỆC TÙY CHỌN (user CHƯA duyệt — hỏi trước khi làm)

1. **Nâng giải thích Đề 01–13 lên cấu trúc 5 lớp** như Đề 14–19 (① Khái niệm → ② Vì sao → ③ Áp dụng sâu → ④ Tại sao A/C/D sai → ⑤ Code thật) — khối lượng lớn (~95 câu)
2. **Thêm trang "Kiến thức nền & Glossary"** đầu đề thi (Vue/Pinia/ASP.NET/HTTP terms) cho người 0 kiến thức
3. **Vá regex diacritics trong shuffle script** (mục 5.2) để tool chạy an toàn lần tới
4. CI/script tái tạo MAPPING tự động khi codebase đổi (hiện file mapping thủ công)

---

*Tạo bởi phiên session-5179639043fa theo yêu cầu: "viết 1 file handoffsession để đưa cho 1 phiên khác làm việc"*

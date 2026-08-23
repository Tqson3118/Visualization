# KẾ HOẠCH VÀ BÁO CÁO KIỂM THỬ (TEST_PLAN)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Test Plan |
| Phiên bản | 1.4 |
| Ngày cập nhật | 13/08/2026 |
| Trạng thái | Dự thảo — bảng PASS/FAIL đã điền kết quả thật (12/08/2026, §10); SEC/PERF/UX chưa thực thi (ghi "chờ") |
| Người soạn | Huỳnh Lê Minh Thư |
| Người duyệt | Phạm Ngọc Ái Liên |
| Tài liệu liên quan | SRS.md, SDD.md, API_REFERENCE.md |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md Phần 14 (chiến lược), 8.8-8.10 (golden data + mở rộng), 13.3 (bảo mật), 17.15 (ma trận truy vết) |

> ⚠ **TÀI LIỆU NÀY GỒM KẾ HOẠCH (PLAN) + KẾT QUẢ THẬT (REPORT)**: mọi test case đã được thiết kế và đặc tả đầy đủ (điều kiện, bước, kỳ vọng). **Đã thực thi một phần ngày 12/08/2026**: kết quả thật của nhóm Backend/Engine/API/E2E được điền tại §10; các nhóm chưa chạy thực tế (Bảo mật pentest, Hiệu năng k6, UX 5 người) vẫn ghi "chờ" theo BAO_CAO_SPEC §5.0 — KHÔNG bịa số liệu (bài học báo cáo cũ).

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Huỳnh Lê Minh Thư | Sinh mới từ PRODUCTION_PROMPT.md v2.5 |
| 1.1 | 12/08/2026 | Huỳnh Lê Minh Thư | Vá review: bổ sung 8 test case còn thiếu (TEST-B-036..038 Favorites §4.8, TEST-B-097..098 ghi chú §4.2, TEST-B-101..103 Mini Quiz §4.3) — khớp ma trận truy vết §11 |
| 1.2 | 12/08/2026 | Trần Viết Tâm Phúc | F2b: điền số thật vào §10 BÁO CÁO TỔNG HỢP (Backend 44, Engine 72, API 27, E2E 11 — chạy 12/08/2026); SEC/PERF/UX giữ "chờ" theo BAO_CAO_SPEC; cập nhật trạng thái front matter + ghi chú §10; bỏ tham chiếu "tuần 19-20" ở trạng thái bảng |
| 1.3 | 12/08/2026 | Trần Viết Tâm Phúc | Đợt G (ux-finalize): TEST-PERF-007 nới ngưỡng bundle theo NFR-5 (JS gốc tải lần đầu ≤ 1.5MB, engine chunk ≤ 500KB gốc) + ghi chú bundle thật vào §10; số FE hiện tại giữ nguyên (unit 72 + e2e 11) |
| 1.4 | 13/08/2026 | Trần Viết Tâm Phúc | GP-T8 (đồng bộ GP-T7 — Premium QR MB Bank): §10 cập nhật số thật sau GP-T7 — Backend 81 unit, API 31 integration, FE unit 89 (gồm +7 test lib/vietqr), E2E 13 (tổng 214); TEST-PERF-007 đo lại bundle sau khi thêm `qrcode` (JS gốc tải lần đầu ≈ 852KB, engine 476KB — vẫn trong ngưỡng) |

---

# 1. MỤC TIÊU, PHẠM VI, MÔI TRƯỜNG

## 1.1 Mục tiêu

1. Chứng minh mọi FR mức **Cao** hoạt động đúng (100% test case PASS trước bàn giao — §14.9).
2. Chứng minh generator sinh đúng chuỗi bước cho 15 GT (golden data N1-N7 — SDD §4.8).
3. Chứng minh cơ chế trừ tim chống double-spend (FR-10.1) với ≥ 3 test case biên (Phần 21 mục 4 prompt).
4. Đạt ngưỡng chất lượng: generator ≥ 90% coverage, backend service ≥ 60%, hiệu năng theo NFR, bảo mật checklist 13.3.

## 1.2 Phạm vi

- **Trong**: unit (engine/store/backend service), integration API, E2E, hiệu năng, bảo mật, UX.
- **Ngoài**: kiểm thử thanh toán thật (Premium là mô phỏng — hiện QR chuyển khoản MB Bank nhưng KHÔNG gọi API ngân hàng/webhook), tải > 200 VU, di động < 768px.

## 1.3 Môi trường kiểm thử

| Thành phần | Cấu hình |
|---|---|
| Frontend | Chrome/Edge/Firefox (2 phiên bản mới nhất), Node 20+, Vitest + Vue Test Utils, Playwright |
| Backend | .NET 8 SDK, xUnit, WebApplicationFactory + Testcontainers (SQL Server) |
| DB | SQL Server 2019+ (Testcontainers cho integration) |
| Hiệu năng | k6 (Docker), Lighthouse CI |
| UX | 5 người (3 chưa dùng hệ thống tương tự), SUS |

## 1.4 Quy ước viết test case (bắt buộc)

| Trường | Quy tắc |
|---|---|
| ID | TEST-B-xxx (backend), TEST-E-xxx (engine), TEST-API-xxx, TEST-UI-xxx, TEST-SEC-xxx, TEST-PERF-xxx, TEST-UX-xxx |
| Tiêu đề | `<Hành động> + <điều kiện>` |
| Tiền điều kiện | dữ liệu/tài khoản chuẩn bị |
| Bước thực hiện | đánh số, đủ chi tiết để tester khác chạy lại |
| Kỳ vọng | kết quả cụ thể (status code, nội dung response, hành vi UI) |
| Tham chiếu | mã FR/NFR/AC |
| Kết quả | [ ] PASS [ ] FAIL — ghi chú |

---

# 2. CHIẾN LƯỢC (KIM TỰ THÁP KIỂM THỬ)

| Tầng | Công cụ | Mục tiêu độ bao phủ |
|---|---|---|
| Unit — Generator | Vitest | ≥ 90% dòng engines/ |
| Unit — Store/Composable | Vitest + Vue Test Utils | ≥ 70% |
| Unit — Backend Service | xUnit | ≥ 60% (ưu tiên Auth, Exercise, Progress, Gamification) |
| Integration — API | xUnit + WebApplicationFactory + Testcontainers | 100% endpoint chính (mọi nhánh HTTP status) |
| E2E — luồng người dùng | Playwright | 12 luồng chính |
| Hiệu năng | k6 + Lighthouse | theo NFR-1..NFR-7 |
| Bảo mật | checklist 13.3 + OWASP ZAP (cơ bản) | toàn bộ 13.3 |

---

# 3. DỮ LIỆU KIỂM THỬ

## 3.1 TestSeed (riêng cho test — không dùng seed production)

- 20 user (3 vai trò: Student/Teacher/Admin), 5 topic, 12 bài học, 8 bài tập, 200 bản ghi tiến độ.
- Test tài khoản: `student@test.local` / `Pass@123`, `teacher@test.local`, `admin@test.local`.

## 3.2 Golden data (engine — nguồn SDD §4.8/4.9)

- Bộ N1-N7 cho mỗi GT (VD bubble: `[]`, `[5]`, `[1,2,3,4,5]`, `[5,4,3,2,1]`, `[4,2,4,1,4]`, `[-3,7,-1,0,2]`, 100 phần tử seed=42).
- Trace chuẩn bubble `[3,1,2]` (20 bước — SDD §4.9A) và binary search (SDD §4.9B) làm mốc vàng.
- Test ẩn Code Challenge: 8 bài × ~11 (19.6B) — golden data seed cố định, idempotent.

## 3.3 Công cụ chạy

1. **Vitest**: `npm run test:unit` — mọi file `*.spec.ts`.
2. **xUnit**: `dotnet test --filter "Category=Integration"` — cần Docker (Testcontainers).
3. **Playwright**: `npm run test:e2e` — cần backend dev + build.
4. **k6**: `k6 run tests/load/login.js` — scripts `tests/load/`.
5. **Lighthouse**: CI job trên URL staging.

---

# 4. TEST CASE BACKEND (TEST-B)

## 4.1 Auth (FR-1.1 → FR-1.6, FR-1.11)

#### TEST-B-001 | Đăng ký tài khoản thành công | FR-1.1
| Mục | Nội dung |
|---|---|
| Tiền điều kiện | Chưa tồn tại email trong hệ thống |
| Bước thực hiện | 1. Gửi `POST /auth/register` body `{displayName:"Nguyễn Minh", email:"minh@university.edu.vn", password:"MatKhau@123", isTeacher:false}` 2. Kiểm tra response 3. Đăng nhập lại |
| Kỳ vọng | 1. 201 + user id mới 2. Email lowercase được chuẩn hóa 3. Đăng nhập thành công (200) 4. PasswordHash ≠ plaintext |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-002 | Đăng ký với email trùng | FR-1.1
| Mục | Nội dung |
|---|---|
| Tiền điều kiện | Email `minh@university.edu.vn` đã tồn tại |
| Bước thực hiện | Gửi lại request đăng ký với email trên |
| Kỳ vọng | 409 + `{code:"EMAIL_EXISTS", message:"Email đã được sử dụng", field:"email"}`; KHÔNG tạo tài khoản mới |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-003 | Đăng ký với mật khẩu yếu | FR-1.1, NFR-8
| Mục | Nội dung |
|---|---|
| Bước thực hiện | Gửi register với `password:"matkhau"` (thiếu chữ hoa/số/ký tự đặc biệt) |
| Kỳ vọng | 400 + `WEAK_PASSWORD` + details liệt kê từng quy tắc vi phạm |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-004 | Đăng nhập đúng | FR-1.2
| Bước thực hiện | `POST /auth/login` đúng email/mật khẩu |
| Kỳ vọng | 200 + `{accessToken, expiresIn:3600, user.role:"STUDENT"}` + cookie refresh_token |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-005 | Đăng nhập sai mật khẩu | FR-1.2
| Kỳ vọng | 401 + `INVALID_CREDENTIALS` — KHÔNG tiết lộ email tồn tại |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-006 | Đăng nhập tài khoản bị khóa | FR-1.2
| Kỳ vọng | 403 + `ACCOUNT_LOCKED` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-007 | Khóa tạm sau 5 lần sai | FR-1.2, NFR-12
| Bước thực hiện | 5 lần đăng nhập sai liên tiếp trong 15 phút |
| Kỳ vọng | Lần thứ 6 → 429 + `Retry-After`; log Serilog có cảnh báo |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-008 | Refresh token hợp lệ | FR-1.3
| Kỳ vọng | 200 + accessToken mới; token cũ bị rotate-invalidate (dùng lại → 401 `REFRESH_INVALID`) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-009 | Refresh token hết hạn | FR-1.3
| Kỳ vọng | 401 + `REFRESH_INVALID`; client logout |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-010 | Replay refresh token đã thu hồi | FR-1.3, NFR-9 (v2.4)
| Bước thực hiện | Dùng lại refresh token đã rotate-invalidate 2 lần |
| Kỳ vọng | 401 + TOÀN BỘ chuỗi phiên của user bị thu hồi + log cảnh báo bảo mật |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-011 | Đăng xuất | FR-1.4
| Kỳ vọng | 204; refresh token cũ không dùng được; API cần auth trả 401 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-012 | Đổi mật khẩu thành công | FR-1.5
| Kỳ vọng | 200; mọi refresh token KHÁC phiên hiện tại bị thu hồi — phải đăng nhập lại |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-013 | Đổi mật khẩu sai mật khẩu cũ | FR-1.5
| Kỳ vọng | 400 + `OLD_PASSWORD_WRONG` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-014 | Đổi mật khẩu trùng mật khẩu cũ | FR-1.5
| Kỳ vọng | 400 + `PASSWORD_SAME` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-015 | Gửi link khôi phục | FR-1.6
| Kỳ vọng | 200 với thông báo chung (kể cả email không tồn tại); nếu email tồn tại → tạo PasswordResetTokens (30 phút, 1 lần) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-016 | Đặt lại mật khẩu với token hợp lệ | FR-1.6
| Kỳ vọng | 200; đăng nhập được bằng mật khẩu mới; token dùng lần 2 → 400 `RESET_TOKEN_INVALID` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-017 | Token hết hạn | FR-1.6
| Kỳ vọng | 400 + `RESET_TOKEN_INVALID` + gợi ý gửi lại |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-018 | Cập nhật hồ sơ + avatar | FR-1.7
| Kỳ vọng | 200; avatar sai định dạng → 400 `UPLOAD_INVALID_TYPE`; > 2MB → `UPLOAD_TOO_LARGE` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-019 | Phê duyệt Teacher | FR-1.8
| Kỳ vọng | Admin duyệt → role TEACHER, IsActive=true; trước khi duyệt Teacher gọi API quản trị → 403. 019b (v2.8): `{approve:false, reason:"..."}` → role STUDENT, IsActive=true, log lý do; approve sai định dạng → 400 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-020..022 | Quản lý người dùng | FR-1.9
| Nội dung | 020: khóa user → user không đăng nhập được + token hiện có vô hiệu (cache 60s). 021: khóa chính mình → 400. 022: chuyển vai trò Admin → 400. 020c: Admin thường khóa/đổi role/xóa/reset mật khẩu Admin khác → 403; Admin chính (IsPrimaryAdmin) làm được → 200. 020d: khóa/xóa Admin cuối cùng còn active → 400. 020e: Admin chính chuyển IsPrimaryAdmin cho Admin khác → 200 + log Serilog; Admin nhận cờ mới quản được Admin cũ; mọi thao tác có log Serilog |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-094..096 | 2FA email | FR-1.11
| Nội dung | 094: bật 2FA → đăng nhập cần mã 6 số (hiệu lực 5 phút). 095: sai mã 3 lần → khóa bước 2 trong 10 phút. 096: mã dùng 1 lần |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.2 Topics/Lessons (FR-2.1 → FR-2.6)

#### TEST-B-023..025 | CRUD chủ đề | FR-2.1
| Nội dung | 023: tạo/sửa/xóa topic + thứ tự SortOrder + cây lồng 2 cấp. 024: xóa topic có bài học → 409 `TOPIC_HAS_LESSONS`. 025: tên trùng cấp cha-con → 400 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-026..030 | CRUD bài học | FR-2.2
| Nội dung | 026: Teacher tạo bài → draft. 027: Student không thấy draft (GET /lessons chỉ active). 028: Teacher sửa bài không phải của mình → 403. 029: xóa → mềm (DeletedAt, ẩn khỏi danh sách). 030: content chứa `<script>` → sanitize, không thực thi |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-031 | Danh sách bài học đúng trạng thái tiến độ | FR-2.3
| Kỳ vọng | Trả đúng simulationCount/exerciseCount/progress cá nhân |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-032..034 | Xem chi tiết + đánh dấu đã học | FR-2.4
| Nội dung | 032: GET /lessons/{id} đúng nội dung. 033: mark-viewed → upsert UserProgress (không trùng bản ghi). 034: mark-viewed lần 2 → không nhân đôi |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-035 | Tìm kiếm bài học | FR-2.5
| Kỳ vọng | Tìm không phân biệt hoa thường; chuẩn hóa dấu tiếng Việt |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-097..098 | Ghi chú cá nhân trên bài học | FR-2.6
| Nội dung | 097: user A tạo + sửa ghi chú `PUT /me/notes/{lessonId}` → upsert đúng (UNIQUE (UserId, LessonId), không nhân đôi), hiển thị lại đúng sau khi tải lại trang; user B gọi sửa/xóa note của A → 403/404 (IDOR). 098: xóa ghi chú `DELETE /me/notes/{lessonId}` → hết hiển thị + dấu chấm "có ghi chú" ở danh sách bài học tắt; mất mạng khi đang soạn → lưu nháp cục bộ, đồng bộ lại khi có mạng (kiểm tra E2E/manual) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.3 Exercises (FR-4.1 → FR-4.10, FR-4.11/4.12)

#### TEST-B-039..044 | CRUD bài tập + câu hỏi | FR-4.1
| Nội dung | 039: tạo quiz (SINGLE/MULTI/BOOLEAN) — 5 câu. 040: kích hoạt khi < 3 câu → 400. 041: câu hỏi thiếu đáp án đúng/sai → 400. 042: đáp án không lộ qua GET /exercises/{id}. 043: sửa/xóa (mềm). 044: gắn NodeId+Stage cho Ladder. 043b (v2.8): bài tập đã có ≥1 submission → sửa câu hỏi/đáp án/điểm → 409; chỉ sửa được tiêu đề/mô tả/trạng thái/thứ tự |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-045..052 | Nộp bài + chấm điểm | FR-4.2
| Nội dung | 045: SINGLE đúng → đúng điểm. 046: MULTI chọn thiếu/thừa → 0. 047: BOOLEAN. 048: nộp trùng đồng thời → 422 `SUBMISSION_IN_PROGRESS`. 049: nộp khi hết thời lượng → tự nộp. 050: làm lại → BestScore giữ MAX. 051: ResultJson tái hiện được màn kết quả. 052: xáo trộn (seed) → chấm không sai |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-053..058 | Lab Bậc 2 (chấm trạng thái cuối) | FR-4.3, G-5
| Nội dung | 053: thao tác đúng trạng thái cuối + số bước ≤ chuẩn × 1.5 → PASS. 054: đúng trạng thái nhưng quá số bước → FAIL. 055: thao tác bất hợp lệ (swap 2 ô không liền kề) → chặn + giải thích, không tính bộ đếm. 056: Lab BST chèn trùng khóa → chặn. 057: Lab đồ thị BFS sai thứ tự → FAIL + làm lại. 058: nộp lại không tính 2 lần (idempotent) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-059..061 | Lịch sử bài làm | FR-4.4
| Nội dung | 059: sinh viên xem lịch sử của mình. 060: giảng viên xem bài nộp của người học trong bài của mình. 061: giảng viên không xem được bài nộp ở bài tập của người khác → 403/404 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-105..107 | Chế độ luyện tập | FR-4.6
| Nội dung | 105: luyện tập không tạo ExerciseSubmission. 106: điểm không ảnh hưởng BestScore. 107: ngoài session 30 phút → vào node trừ tim (20.4) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-108..110 | Gợi ý (Hints) | FR-4.7
| Nội dung | 108: xem 1 gợi ý → trừ 20% điểm câu; tối thiểu 40%. 109: câu không gợi ý → nút ẩn. 110: không xem gợi ý → điểm đầy đủ |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-111..112 | Xáo trộn + giải thích phương án sai | FR-4.8, FR-4.9
| Nội dung | 111: 2 lần làm cùng bài → thứ tự khác (xác suất cao); BOOLEAN không xáo trộn. 112: chọn phương án sai → hiển thị đúng giải thích riêng của phương án đó |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-113..115 | Import CSV | FR-4.10
| Nội dung | 113: 100 dòng hợp lệ tạo < 5s. 114: dòng lỗi → báo cáo "Dòng 5: thiếu đáp án đúng", không tạo dòng lỗi. 115: CSV sai cấu trúc → 400 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-137..141 | Practice Ladder tuần tự | FR-4.11
| Nội dung | 137: chưa pass Quiz (≥ 60%) → mở Lab bị chặn `LADDER_LOCKED`. 138: pass Quiz → mở Lab; pass Lab → mở Code. 139: pass Code ≥ 70% test → pass node + mở khóa node kế. 140: retry bậc trong session → không trừ tim. 141: điểm node = Quiz 20% + Lab 30% + Code 50% (giữ MAX) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-142..144 | Final test | FR-4.12
| Nội dung | 142: chỉ mở khi pass toàn bộ node (403 nếu chưa đủ). 143: đề trộn seed (cùng user + ngày → tái tạo được). 144: ≥ 70% → pass + huy hiệu + mở path kế; retry ngoài session trừ 1 tim |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-101..103 | Kiểm tra nhanh sau mô phỏng (Mini Quiz) | FR-3.16
| Nội dung | 101: bước cuối mô phỏng → banner "Kiểm tra nhanh" + 1-2 câu sinh từ chính dữ liệu vừa chạy (VD: tổng số lần hoán đổi, pivot lần chia đầu tiên). 102: nộp → chấm ngay + giải thích kèm liên kết nhảy về bước liên quan; kết quả ghi vào phiên "kiểm tra nhanh" tách biệt. 103: điểm mini quiz KHÔNG ảnh hưởng BestScore bài tập chính thức (không upsert UserProgress.BestScore) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.4 Progress & báo cáo (FR-5.1 → FR-5.5)

#### TEST-B-062..064 | Ghi nhận tiến độ | FR-5.1
| Nội dung | 062: mark-viewed → upsert đúng. 063: nộp bài → BestScore = max. 064: truy vấn tiến độ < 100ms với 1000 user (dữ liệu giả) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-065..068 | Báo cáo giảng viên | FR-5.3
| Nội dung | 065: số liệu đúng (TotalLearners/LearnersViewed/AvgScore). 066: chỉ thấy người học của bài học mình tạo. 067: CSV có UTF-8 BOM, mở bằng Excel. 068: báo cáo < 2s với 2.000 user |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-069 | Thống kê admin | FR-5.4
| Kỳ vọng | Số liệu khớp DB; tải < 2s |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-073 | Cấu hình hệ thống | FR-6.2
| Kỳ vọng | Thay đổi settings áp dụng ngay (không restart) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-076..077 | Đánh giá nội dung | FR-7.4
| Nội dung | 076: 1 người 1 đánh giá (UPDATE thay thế). 077: điểm TB tính đúng; giảng viên không thấy tên người đánh giá. 076b (v2.9): đánh giá khi CHƯA "Đánh dấu đã học" bài đó → 403 FORBIDDEN; sau khi đánh dấu → 200 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-116..119 | Huy hiệu | FR-5.5
| Nội dung | 116: đạt điều kiện → tạo UserAchievement 1 lần (UNIQUE). 117: không trao 2 lần. 118: điều kiện streak-7 đúng. 119: trang thành tích tải < 1s |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.5 Classes (FR-8.1 → FR-8.4)

#### TEST-B-079..081 | Lớp học phần | FR-8.1
| Nội dung | 079: tạo lớp → mã mời 6 ký tự UNIQUE. 080: đóng lớp → không nhận thêm thành viên. 081: xóa lớp có dữ liệu → xóa mềm + giữ báo cáo. 081b (v2.8): Teacher sở hữu bị khóa → lớp tự Đóng + SV nộp bài gán theo lớp → 409; Admin PUT /classes/{id} {ownerId} → lớp mở lại |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-082..084 | Thành viên | FR-8.2
| Nội dung | 082: tham gia bằng mã khi lớp Mở. 083: mã sai/lớp Đóng → thông báo cụ thể. 084: xóa thành viên → phản ánh ngay; rời lớp không mất dữ liệu cá nhân |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-085..087 | Gán nội dung + hạn nộp | FR-8.3
| Nội dung | 085: gán lesson/exercise + dueAt. 086: quá hạn vẫn nộp nhưng hiển thị "Nộp trễ". 087: báo cáo đếm đúng 3 trạng thái (đúng hạn/trễ/chưa nộp). 085b (v2.8): nộp kèm classAssignmentId → lưu ExerciseSubmissions.ClassAssignmentId; SV không còn trong ClassMembers → 403; lớp Đóng → 409; cùng bài gán ở 2 lớp → trạng thái tính riêng theo từng DueAt |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-088..090 | Báo cáo lớp | FR-8.4
| Nội dung | 088: số liệu khớp. 089: CSV đúng định dạng. 090: tải < 2s với lớp 100 sinh viên; lớp chưa gán nội dung → bảng trống + hướng dẫn. 090b (v2.8): SV rời lớp → bài nộp cũ giữ điểm cá nhân nhưng KHÔNG còn trong báo cáo lớp (chỉ tính ClassMembers hiện tại) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.6 Code Runner (FR-9.1 → FR-9.6)

#### TEST-B-124..125 | Lưu lần chạy | FR-9.1
| Nội dung | 124: POST /code-runs lưu đúng (Code/Input/Status). 125: GET /code-runs/{id}/trace phân trang TraceEvent[] |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-126..130 | Chấm bài code (test ẩn) | FR-9.3
| Nội dung | 126: nộp đúng → điểm = số test pass (chấm output, không soi implementation). 127: code không chạy → 0 điểm + hiển thị lỗi dòng. 128: test ẩn KHÔNG hiển thị qua API/UI (GET /exercises/{id} không chứa hiddenTests). 129: pass ≥ 70% → pass bậc. 130: dùng `arr.sort()` → vẫn chấm đúng output (chấm theo đầu ra — FR-9.3). 130b (v2.8): code hardcode if-else theo input tĩnh → vẫn FAIL khi test ngẫu nhiên sinh tại thời điểm nộp khác input (expected từ hàm chuẩn StepExecutor). 130c (v2.9): "So sánh code chuẩn" (Trace Diff) → 2 canvas chạy song song + đánh dấu bước khác biệt đầu tiên; code dùng `arr.sort()` → thông báo "không hỗ trợ trực quan từng bước", không ảnh hưởng điểm |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-131..132 | Lịch sử nộp code | FR-9.5
| Nội dung | 131: lịch sử đúng thứ tự, xem lại code cũ + kết quả. 132: so sánh 2 lần nộp |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-145..147 | Giới hạn sandbox | FR-9.6
| Nội dung | 145: code > 200 dòng → chặn với thông báo rõ. 146: vượt 10 giây → timeout + thông báo. 147: vượt 64MB → chặn |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.7 Learning Path + Gamification + Premium (FR-2.10, FR-10.1 → FR-10.7)

#### TEST-B-133..136 | Learning Path | FR-2.10
| Nội dung | 133: bản đồ node đúng trạng thái (khóa/mở/pass). 134: mở khóa tuần tự. 135: sao đúng công thức (1⭐ pass; 2⭐ ≥75%; 3⭐ ≥90%). 136: điểm lộ trình = ĐTB node × 80% + final test × 20% |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### ⚠ TEST-B-148..155 | TRỪ TIM — FR-10.1 (BẮT BUỘC ≥ 3 case biên theo Phần 21 mục 4)

> Các case biên bắt buộc: (1) mở simulator từ CheatSheet vẫn trừ; (2) 2 request song song chỉ trừ 1 (concurrency thực); (3) hết session resume trừ lại. Kèm theo các case cơ bản.

#### TEST-B-148 | Vào node mới trừ đúng 1 tim | FR-10.1, AC-10.1.1
| Mục | Nội dung |
|---|---|
| Tiền điều kiện | Student có 10❤, chưa từng vào node X |
| Bước thực hiện | Gửi `POST /learning-path/1/nodes/22/enter` |
| Kỳ vọng | 200 + `heartsLeft:9` + session mới (ExpiresAt = StartedAt + 30 phút); `NodeSessions` có 1 bản ghi (UserId, NodeId) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-149 | Mở lại cùng node trong session 30 phút — KHÔNG trừ | FR-10.1, AC-10.1.2
| Bước thực hiện | Trong 30 phút sau TEST-B-148, gọi enter lại node 22 |
| Kỳ vọng | 200 + `heartsLeft:9` (KHÔNG đổi); trả đúng Stage/StepIndex đang dở |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

**TEST-B-149b (v2.9) — gia hạn sliding**: sau khi nộp THÀNH CÔNG Quiz trong session → `NodeSessions.ExpiresAt` = +30 phút (KHÔNG trừ tim); nộp tiếp Lab → +30 phút nữa nhưng KHÔNG vượt quá `StartedAt + 120 phút` (cap); chỉ xem lý thuyết → KHÔNG gia hạn.

#### TEST-B-150 | Mở simulator từ CheatSheet VẪN trừ tim (case biên — 20.4) | FR-10.1, FR-3.2, AC-3.2.6
| Bước thực hiện | Từ `/cheatsheet` bấm "Xem mô phỏng" → mở `/simulator/sort.bubble` của node chưa thuộc session |
| Kỳ vọng | Tim giảm 1 — KHÔNG có ngoại lệ cho CheatSheet; nếu node đã pass → miễn phí |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-151 | 2 request song song cùng enter — CHỈ 1 lần trừ (case biên concurrency thực — v2.5) | FR-10.1, AC-10.1.6
| Mục | Nội dung |
|---|---|
| Tiền điều kiện | User có 10❤; node chưa có session; **2 request gửi đồng thời (Task.WhenAll / Promise.all)** |
| Bước thực hiện | Gửi đồng thời 2 `POST /learning-path/1/nodes/22/enter` (kể cả trường hợp session hiện tại ĐÃ HẾT HẠN — 2 request cùng gia hạn 1 row) |
| Kỳ vọng | CHỈ 1 request nhận `heartsLeft:9`, request kia nhận `heartsLeft:9` với session resume (KHÔNG trừ lần 2); tổng tim chỉ giảm 1; `NodeSessions` có đúng 1 bản ghi (UserId, NodeId). Điểm mấu chốt v2.5: UPDATE điều kiện `ExpiresAt < @now` + @@ROWCOUNT tuần tự hóa — 2 request gia hạn row hết hạn chặn lẫn nhau, chỉ 1 cái ROWCOUNT=1 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-152 | Hết session > 30 phút → trừ tim mới NHƯNG giữ bậc đã pass (case biên) | FR-10.1, AC-10.1.5
| Bước thực hiện | Chờ session hết hạn (> 30 phút) → vào lại node (đã pass Quiz ở lần trước) |
| Kỳ vọng | Trừ 1 tim; session mới; Stage/StepIndex bắt đầu ở bậc ĐÃ PASS giữ nguyên (không reset tiến độ) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-153 | Hearts = 0 → chặn + Màn 28; Benchmark không bị chặn | FR-10.1, AC-10.1.4, AC-3.2.8
| Bước thực hiện | 1) Đưa Hearts về 0 → gọi enter node chưa pass. 2) Mở `/benchmark/sort.bubble/sort.quick` |
| Kỳ vọng | 1) 403 + `HEARTS_EMPTY`; transaction rollback (không tạo session, không trừ âm). 2) Benchmark mở bình thường (miễn phí — 20.4) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-154 | Xem lại node ĐÃ PASS — miễn phí | FR-10.1, AC-10.1.3
| Kỳ vọng | 200 + hearts KHÔNG đổi |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-155 | Chỉnh đồng hồ thiết bị → hồi tim tính theo server timestamp | FR-10.1, AC-10.1.7
| Bước thực hiện | Chỉnh đồng hồ máy lệch +2 giờ → kiểm tra /me/hearts |
| Kỳ vọng | Hồi tim tính theo `LastHeartAt` (server UTC), KHÔNG bị ảnh hưởng bởi clock client |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-156..162 | Gems + Shop | FR-10.2
| Nội dung | 156: pass node → +10 gems (GemTransactions earn). 157: mua item → trừ gems atomic + UserInventory tăng. 158: **2 request mua cùng lúc → chỉ 1 thành công** (double-spend). 159: thiếu gems → 422 `INSUFFICIENT_GEMS`. 160: đã max stack → chặn + nút Mua disabled trên UI (v2.8). 161: equip khung → áp dụng ngay. 161b (v2.9): equip khung Vàng → `IsEquipped=true` item đó, item khung khác cùng loại → `false`; equip theme tương tự. 162: nâng sao 2⭐ → +3 gems 1 lần; retry cùng sao KHÔNG nhận (NewStars > OldStars — v2.8) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-163..167 | Daily Quest | FR-10.3
| Nội dung | 163: 5 quest/ngày (2E+2M+1H) seed theo (UserId, ngày) — tái tạo được. 164: tiến độ tự cập nhật theo sự kiện. 165: claim thưởng atomic — claim 2 lần → 422 `QUEST_ALREADY_CLAIMED`. 166: quest đầy tim → +5 gems. 167: reset 00:00 UTC+7 — quest bỏ dở mất tiến độ ngày |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-168..171 | Streak + freeze | FR-10.4
| Nội dung | 168: hoạt động 3 ngày liên tục → StreakDays=3 (login KHÔNG tính). 169: nghỉ 1 ngày không freeze → reset 0. 170: freeze → giữ streak 1 ngày. 171: job 00:30 đóng sổ đúng (StreakLastProcessed không xử lý lặp). 171b (v2.8): hoạt động lúc 00:15 (sau reset quest 00:00) → streak ngày mới +1 NGAY (eager), KHÔNG bị job 00:30 trừ streak hôm qua oan |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-172..174 | XP & Level | FR-10.5
| Nội dung | 172: pass node lần đầu → XP đúng công thức Level = 1 + floor(sqrt(XP/100)). 173: retry → KHÔNG cấp XP. 174: nâng sao → KHÔNG cấp XP (anti-grinding — v2.5) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-175..177 | Leaderboard | FR-10.6
| Nội dung | 175: điểm khớp XP. 176: reset tuần thứ Hai 00:00 UTC+7. 177: phân trang + tab Lớp chỉ hiện khi đã tham gia |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-B-178..183 | Premium | FR-10.7
| Nội dung | 178: bấm "Tôi đã chuyển khoản" (sau đếm ngược 60s) → mock-pay → Premium active ngay + log giao dịch (OrderRef `DSV{userId}T{months}`) + HeartsMax=30 (GP-T7). 179: hết hạn → job downgrade về Free + clamp Hearts về 10 (v2.4). 180: giữ gems/avatar/items sau downgrade. 181: CheatSheet PDF chỉ Premium. 182: quyền lợi Hint áp dụng ngay. 183: gia hạn không trùng lặp |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 4.8 Favorites (FR-3.10)

> Nguồn: §3.3 FR-3.10 + bảng Favorites §10.2.11 (UNIQUE (UserId, SimulationKey)).

#### TEST-B-036..038 | Lưu mô phỏng yêu thích | FR-3.10
| Nội dung | 036: `POST /favorites` `{simKey:"sort.bubble", input:{values:[5,3,8,1]}}` → lưu đúng cấu hình; `GET /favorites` trả danh sách cá nhân; mở lại → tái tạo đúng cấu hình. 037: thêm trùng (UserId, SimulationKey) → bị chặn (UNIQUE, không tạo bản ghi thứ 2). 038: `DELETE /favorites/{id}` xóa của mình OK; user khác xóa → 403/404 (IDOR) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

---

# 5. TEST CASE ENGINE (TEST-E — generator + golden data)

> Nguồn: SDD §4.8-4.9. Với mỗi GT × mỗi nhóm N1-N7: kiểm tra (1) bước cuối đạt điều kiện kết thúc, (2) bộ đếm trong khung lý thuyết, (3) mọi bước có `explanation` ≠ rỗng + `pseudocodeLine` hợp lệ, (4) trạng thái phần tử tại bước mốc (đầu/giữa/cuối).

## 5.1 Nhóm sắp xếp

#### TEST-E-001 | Bubble sort — mảng đã sắp xếp | FR-3.1, FR-3.7
| Mục | Nội dung |
|---|---|
| Dữ liệu | `[1,2,3,4,5]` |
| Bước thực hiện | Gọi `bubbleGenerator.generate({values:[1,2,3,4,5]})` |
| Kỳ vọng | 1. Bước cuối: mảng `[1,2,3,4,5]`, mọi phần tử `done` 2. Số bước ≤ 40 3. `swaps = 0` 4. Tồn tại bước kết thúc sớm (dòng 9 — swapped=false) 5. Mọi `explanation` khác rỗng, `pseudocodeLine` ∈ [1..10] |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-002 | Bubble sort — mảng giảm dần (worst case) | FR-3.1, FR-3.9
| Dữ liệu | `[5,4,3,2,1]` |
| Kỳ vọng | 1. Bước cuối `[1,2,3,4,5]` 2. `comparisons = 10` (n(n-1)/2) 3. `swaps = 10` 4. Số bước = 1 + 10×2 + 10×1 + 4×2 (vòng ngoài) — đối chiếu trace chuẩn SDD §4.9A |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-003 | Bubble sort — trace chuẩn `[3,1,2]` (20 bước — SDD §4.9A) | FR-3.1, FR-3.7
| Kỳ vọng | So khớp 100% bảng trace mốc vàng: từng bước (line, explanation, annotations, variables, c/s/w, trạng thái phần tử) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-004 | Selection sort — N1/N2/N3/N4/N5 | FR-3.1
| Kỳ vọng | N2: swaps=0. N3: swaps = n/2 (tối đa n-1). Kết quả cuối đúng; bộ đếm trong khung lý thuyết |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-005 | Insertion sort — N1..N5 | FR-3.1
| Kỳ vọng | N2 `[1,2,3,4,5]` → so sánh = 4, writes = 0 (không dịch chuyển). N3 worst: so sánh = n(n-1)/2. Ổn định: trùng không đổi thứ tự |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-006 | Merge sort — N1..N5 + call stack | FR-3.1, FR-3.14
| Kỳ vọng | Kết quả đúng; mỗi lệnh đệ quy có bước đánh dấu đoạn (group); call stack khớp lời gọi thực tế (≤ 15 frame) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-007 | Quick sort — N1..N5 + pivot | FR-3.1
| Kỳ vọng | Pivot luôn `highlight` + annotation `pivot=a[hi]=x`; mọi giá trị bằng nhau → không lỗi; kết quả đúng |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-008 | Heap sort — N1..N5 | FR-3.1
| Kỳ vọng | Heapify đúng tính chất max-heap từng bước; kết quả đúng; đoạn đã sắp xếp `done` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 5.2 Nhóm tìm kiếm

#### TEST-E-009 | Linear search — target có/không | FR-3.1
| Kỳ vọng | Có → `done` tại vị trí tìm thấy (bản ghi đầu tiên); không → toàn mảng `muted` + banner; rỗng → -1 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-010 | Binary search — trace chuẩn (SDD §4.9B) | FR-3.1, FR-3.7
| Kỳ vọng | `a=[2,5,8,12,19,23]`, target=12 → 4 bước trích khớp; mid annotation đúng `mid=(low+high)/2`; đoạn bỏ `muted` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-011 | Binary search — biên | FR-3.1
| Kỳ vọng | target < a[0] / > a[n-1] → -1; n=1 khớp → 0; trùng → chấp nhận mọi index thỏa `arr[i]==target`; dữ liệu không sắp xếp → tự sắp xếp + banner |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 5.3 Nhóm CTDL tuyến tính

#### TEST-E-012..014 | Stack push/pop/peek | FR-3.1
| Nội dung | 012: push lên rỗng (top 0→1) + đầy stack → `error` + dừng. 013: pop rỗng → `error`; pop lấy đúng giá trị + hoạt ảnh muted. 014: peek trả đúng s[top] |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-015..016 | Queue enqueue/dequeue | FR-3.1
| Nội dung | 015: enqueue tăng rear, front/rear annotation đúng. 016: dequeue lấy q[front]; đầy/rỗng → `error` + dừng |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-017..018 | Linked list | FR-3.1
| Nội dung | 017: insert đầu/cuối/vị trí k; delete k=0/k=n-1/k=n (error); nút mới `highlight`→`swap`→`done`. 018: search tìm thấy nút đầu khi trùng giá trị |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 5.4 Nhóm cây

#### TEST-E-019..021 | BST insert/delete/search + duyệt | FR-3.1
| Nội dung | 019: chèn tăng dần → cây lệch phải (hiển thị đúng quan hệ); trùng → bỏ qua. 020: delete nút 0/1/2 con (2 con → min cây con phải `highlight` + thay giá trị `swap`); delete gốc. 021: inorder → kết quả tăng dần; 4 kiểu duyệt đúng thứ tự |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-022..023 | AVL insert + 4 trường hợp xoay | FR-3.1
| Nội dung | 022: 4 ca LL/RR/LR/RL — mỗi ca 1 test; nút vi phạm `error` + nhãn `bf=±x`. 023: chèn 1..7 → cây vẫn cân bằng (không lệch); sau xoay toàn bộ `done` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-024..026 | Heap insert/extract/heapify | FR-3.1
| Nội dung | 024: insert → bubble up cha-con `swap` + mũi tên lên. 025: extract → a[0] `error`→`muted`, a[last] lên đầu `swap`, sift down đúng; pop liên tiếp → thứ tự giảm dần. 026: heapify đúng tính chất; extract rỗng → `error` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 5.5 Nhóm đồ thị và bảng băm

#### TEST-E-027..028 | BFS/DFS | FR-3.1
| Nội dung | 027: BFS thứ tự hàng đợi đúng + order tăng dần; đồ thị 2 thành phần → chỉ duyệt từ start; đỉnh lập `muted`. 028: DFS thứ tự pop đúng + stack hiển thị |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-029 | Dijkstra | FR-3.1
| Kỳ vọng | d[] khởi tạo ∞; relax cạnh annotation `d[u]+w < d[v] → cập nhật`; cây đường đi ngắn nhất cạnh `done`; đỉnh không tới được d=∞; chu trình trọng số dương không lặp |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-030 | Hash table | FR-3.1
| Kỳ vọng | annotation `h(k)=k mod m`; chèn vào bucket đúng; va chạm duyệt chuỗi nối kết; xóa `error`→`muted` |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 5.6 Bất biến chung + mở rộng (bắt buộc)

#### TEST-E-031 | Bất biến Step | FR-3.3
| Kỳ vọng | `Object.freeze` test: không mutate sau render; bước lùi về 0 vô hiệu nút; jumpTo giữa chừng đúng |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-032 | Registry | FR-3.1
| Kỳ vọng | `getSimulation` trả đúng instance; key trùng lặp → lỗi rõ ràng khi dev |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-033 | Manual Step Practice (FR-3.12) | FR-3.12
| Kỳ vọng | Mọi bước có ≤ 6 thao tác gợi ý; đáp án khớp 100% step kế tiếp do trace sinh; báo cáo cuối đúng thống kê |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-034 | Kiểm thử mở rộng (AC-3) | FR-3.1, NFR-16
| Bước thực hiện | 1. Tạo `sort.gnome` (20 dòng, không sửa `engines/core/`) → đăng ký catalog → chạy + xuất hiện danh mục. 2. Tạo CTDL `deque` dùng lại kind `array` → hiển thị đúng. 3. `git diff` — không file ngoài `engines/` + catalog bị sửa |
| Kỳ vọng | Cả 3 bước PASS (SDD §4.10) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-E-035 | Hiệu năng sinh bước (mảng 100, seed=42) | NFR-2
| Kỳ vọng | Cả 5 GT sắp xếp: trung bình ≤ 500ms (50 lần chạy), không lần nào > 800ms; đồ thị 50 đỉnh ≤ 1s |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

---

# 6. TEST CASE API (TEST-API)

#### TEST-API-001 | Student không được truy cập quản trị | NFR-10
| Bước thực hiện | Gọi `GET /users` và `POST /lessons` với token Student |
| Kỳ vọng | 403 + `{code:"FORBIDDEN"}` cho cả 2 |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-API-002 | Contract schema | 9.8
| Kỳ vọng | Mọi response khớp JSON schema (FluentAssertions/Zod) — đặc biệt PagedResponse, lỗi chuẩn |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-API-003 | Status code đầy đủ | 9.8
| Kỳ vọng | Mỗi endpoint có bảng status kỳ vọng (200/201/400/401/403/404/409/422) — integration test |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-API-004 | Đồng bộ catalog FE/BE | 9.9
| Kỳ vọng | Danh sách key `shared/simulation-catalog.json` == `engines/catalog.ts` — CI fail nếu khác |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

---

# 7. TEST CASE E2E (TEST-UI) + BẢO MẬT (TEST-SEC)

## 7.1 E2E (Playwright — 12 luồng)

#### TEST-UI-001 | Luồng học tập hoàn chỉnh | AC-1
| Mục | Nội dung |
|---|---|
| Tiền điều kiện | Backend + Frontend chạy; tài khoản mới tạo qua UI |
| Bước thực hiện | 1. `/register` tạo tài khoản 2. `/login` 3. `/path` mở node "Bubble Sort" 4. Mở mô phỏng, Phát 2s, Tạm dừng, Bước lùi 2 lần, kéo thanh tiến trình tới cuối 5. Làm Quiz nộp 6. `/profile` kiểm tra tiến độ |
| Kỳ vọng | Toàn bộ luồng không lỗi; điểm = maxScore; profile hiển thị bài đã xem + điểm |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-002 | Đồng bộ 3 vùng | FR-3.3
| Kỳ vọng | Chuyển bước → canvas/mã giả/giải thích cập nhật trong cùng 1 frame (kiểm tra DOM + canvas snapshot) |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-003 | Phím tắt mô phỏng | FR-3.5
| Kỳ vọng | Space/→/←/Home/End/[ ] hoạt động khi focus trong trang |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-004 | Demo công khai không cần login | FR-7.6
| Kỳ vọng | Khách chạy được 3 demo (bubble/binary/BFS); banner "Đăng ký để học tiếp"; không lưu tiến độ |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-005 | Guard chưa đăng nhập | 12.4
| Kỳ vọng | Vào `/profile`, `/ladder/*` chưa login → chuyển `/login?redirect=...`; sau login quay lại đúng trang |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-006 | Admin khóa user → chặn đăng nhập | FR-1.9
| Kỳ vọng | User bị khóa đăng nhập → 403 ACCOUNT_LOCKED + thông báo tiếng Việt |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-007 | Ladder 3 bậc end-to-end | FR-4.11
| Kỳ vọng | Quiz ≥60% → Lab → Code ≥70% → pass node → sao ⭐ hiển thị + mở khóa node kế |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-008 | Deep-link 2 chiều (FR-2.11) | FR-2.11
| Kỳ vọng | Mở `/simulator/sort.bubble?step=12` → đúng bước 12; nút "Xem lý thuyết liên quan" → về đúng đoạn bài học |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-009 | Màn "Hết tim" | FR-10.1
| Kỳ vọng | Hearts=0 → bấm node → Màn 28: đếm ngược + nút "Xem lại node đã pass" + "Nâng cấp Premium" |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-010 | Benchmark Lab | FR-3.20b
| Kỳ vọng | Chọn 2 GT → chạy multi-n → bảng số liệu + biểu đồ overlay lý thuyết + kết luận; không trừ tim |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-011 | Code Runner 2 chiều | FR-9.2
| Kỳ vọng | Bấm dòng code → nhảy bước; chạy → dòng code cuộn highlight theo bước; code không trace được → cảnh báo + trạng thái đầu/cuối |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

#### TEST-UI-012 | Dark mode | FR-3.18
| Kỳ vọng | Toggle → toàn giao diện + palette mô phỏng tối; contrast WCAG AA; lưu theo tài khoản |
| Kết quả thực tế | [ ] PASS [ ] FAIL — ghi chú: |

## 7.2 Bảo mật (TEST-SEC — checklist 13.3)

| ID | Nội dung | Kỳ vọng | Kết quả |
|---|---|---|---|
| TEST-SEC-001 | Token giả/sai chữ ký | 401 | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-002 | Student gọi endpoint Teacher/Admin | 403 | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-003 | Truy cập UserProgress người khác (đổi id) | 404 (không lộ tồn tại) | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-004 | Nộp `<script>` trong contentHtml | Sanitize, không thực thi | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-005 | SQL injection `' OR 1=1 --` | Không lỗi SQL, trả an toàn | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-006 | 6 lần đăng nhập sai liên tiếp | Khóa tạm (429) + log | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-007 | Upload `.exe` giả `.png` | Từ chối (magic bytes) | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-008 | Xóa refresh token khi đổi mật khẩu | Phiên cũ vô hiệu | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-009 | Sandbox: vòng lặp vô hạn | Chặn sạch, không treo trình duyệt | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-010 | Sandbox: đệ quy sâu | Chặn | [ ] PASS [ ] FAIL — ghi chú: |
| TEST-SEC-011 | Sandbox: truy cập file/network | Chặn | [ ] PASS [ ] FAIL — ghi chú: |

---

# 8. KIỂM THỬ HIỆU NĂNG (TEST-PERF)

| ID | Kịch bản | Cấu hình | Ngưỡng | Điều kiện pass |
|---|---|---|---|---|
| TEST-PERF-001 | Sinh bước mảng 100 (5 GT sắp xếp) | 50 lần chạy | ≤ 500ms TB | 100% < 800ms |
| TEST-PERF-002 | Sinh bước đồ thị 50 đỉnh | 20 lần chạy | ≤ 1s TB | 100% < 1.5s |
| TEST-PERF-003 | Điều hướng 1000 bước liên tục | Chrome desktop | ≥ 55 FPS | TB ≥ 55 |
| TEST-PERF-004 | GET /lessons (1000 bài, phân trang) | 50 VU × 5 phút | p95 ≤ 800ms | 0 lỗi 5xx |
| TEST-PERF-005 | POST /exercises/{id}/submit (10 câu) | 20 VU song song | p95 ≤ 1.5s | chấm đúng 100% |
| TEST-PERF-006 | Login đồng thời | 50 VU × 30s | p95 ≤ 1s | 0 lỗi |
| TEST-PERF-007 | Tải SPA lần đầu (cold cache) | Chrome + Lighthouse | FCP ≤ 1.5s | bundle: JS gốc tải lần đầu ≤ 1.5MB; engine chunk ≤ 500KB gốc (NFR-5 — nới theo đợt G; GP-T8 13/08/2026 đo lại sau khi thêm `qrcode`: JS gốc lần đầu ≈ 852KB, engine 476KB — PASS ngưỡng) |
| TEST-PERF-008 | Đồng thời tổng hợp (70% đọc, 30% ghi) | 200 VU × 15 phút | p95 ≤ 1.2s | 0 lỗi 5xx |

---

# 9. KIỂM THỬ UX (TEST-UX — 5 người)

| Mục | Chi tiết |
|---|---|
| Số người | 5 (không phải thành viên nhóm; 3 chưa dùng hệ thống tương tự) |
| Nhiệm vụ 1 | Tạo tài khoản + mở bài học đầu tiên (≤ 5 phút) |
| Nhiệm vụ 2 | Chạy mô phỏng bubble sort với dữ liệu tự nhập (≤ 2 phút) |
| Nhiệm vụ 3 | Làm 1 bài tập trắc nghiệm + xem kết quả (≤ 5 phút) |
| Nhiệm vụ 4 | Tìm bài học bằng ô tìm kiếm (≤ 1 phút) |
| Nhiệm vụ 5 | Xem báo cáo tiến độ cá nhân (≤ 2 phút) |
| Đo lường | Tỷ lệ hoàn thành (target 100% NV1-5), thời gian TB, SUS ≥ 70/100 |
| Báo cáo | Bảng kết quả + danh sách vấn đề UX kèm mức ưu tiên (định dạng 14.12) |

---

# 10. BÁO CÁO TỔNG HỢP (đã điền kết quả thật — 12/08/2026)

| Nhóm test | Tổng số | PASS | FAIL | Không kiểm thử | Ghi chú |
|---|---|---|---|---|---|
| Backend (TEST-B) | 81 | 81 | 0 | 0 | Unit xUnit — chạy 13/08/2026 (GP-T7: +4 test premium OrderRef DSV, tổng 81 = 77 + 4) |
| Engine (TEST-E) | 89 | 89 | 0 | 0 | Vitest FE 89/89 (gồm +7 test `lib/vietqr` GP-T7 — CRC vector + payload EMVCo) |
| API (TEST-API) | 31 | 31 | 0 | 0 | Integration — WebApplicationFactory + Testcontainers MsSql thật, chạy 13/08/2026 |
| E2E (TEST-UI) | 13 | 13 | 0 | 0 | Playwright (auth/simulator/ladder/code-runner), chạy 13/08/2026 |
| Bảo mật (TEST-SEC) | 0 | 0 | 0 | 0 | Chờ — pentest thực tế chưa chạy (ghi "chờ" theo BAO_CAO_SPEC §5.0) |
| Hiệu năng (TEST-PERF) | 0 | 0 | 0 | 0 | Chờ — load test k6 chưa chạy (ghi "chờ" theo BAO_CAO_SPEC §5.0); **TEST-PERF-007 ngưỡng đã nới theo bundle thật đợt G (JS tải lần đầu ≤ 1.5MB gốc, engine ≤ 500KB); GP-T8 13/08/2026 đo lại sau khi thêm `qrcode`: JS gốc lần đầu ≈ 852KB / engine 476KB gốc (120KB gzip) — vẫn trong ngưỡng** |
| UX (TEST-UX) | 0 | 0 | 0 | 0 | Chờ — khảo sát 5 người chưa thực hiện (ghi "chờ" theo BAO_CAO_SPEC §5.0) |
| **Tổng** | **214** | **214** | **0** | **0** | Tổng các nhóm ĐÃ chạy (B/E/API/E2E — 13/08/2026 sau GP-T7); SEC/PERF/UX chưa tính |

**Kết quả hỗ trợ (12/08/2026)**: Frontend build PASS 0 lỗi; Backend build 0 warning / 0 error (5 projects); smoke test `/health` → 200, mọi `/api/v1/*` không token → 401; seed đã chạy thật: 5 topics / 8 lessons / 29 exercises / 76 questions / 5 paths / 18 nodes / 8 quests / 8 shop items / 9 settings / 3 users.

- Mọi FAIL phải có: nguyên nhân, mức độ, người sửa, ngày sửa, ngày pass lại; đính kèm screenshot/log trích đoạn.
- **Không bịa số liệu** (bài học từ báo cáo cũ — BAO_CAO_SPEC §5.0): kết quả thật đã điền tại §10 (12/08/2026); các hạng mục chưa chạy (load test k6, pentest thực tế, khảo sát UX) vẫn ghi "chờ" theo BAO_CAO_SPEC — không điền số ước lượng.

## Ngưỡng chất lượng trước khi bàn giao (Definition of Done — 14.9)

1. 100% test case nhóm B/E/API của FR mức Cao: PASS.
2. FAIL mở: 0 lỗi cao; ≤ 3 lỗi TB (có kế hoạch); lỗi thấp → backlog.
3. Coverage generator ≥ 90% (c8/Istanbul).
4. 8 kịch bản hiệu năng đạt ngưỡng.
5. Kiểm thử bảo mật 13.3: toàn bộ PASS.

---

# 11. MA TRẬN TRUY VẾT FR → UC → ENDPOINT → DB → TEST (nguồn 17.15)

| FR | UC | Endpoint chính | Bảng DB | Nhóm test |
|---|---|---|---|---|
| FR-1.1 | UC-02 | POST /auth/register | Users | TEST-B-001..003 |
| FR-1.2 | UC-03 | POST /auth/login | Users, RefreshTokens | TEST-B-004..007 |
| FR-1.3 | UC-03 | POST /auth/refresh | RefreshTokens | TEST-B-008..010 |
| FR-1.4 | UC-03 | POST /auth/logout | RefreshTokens | TEST-B-011 |
| FR-1.5 | UC-03 | PUT /auth/me/password | Users | TEST-B-012..014 |
| FR-1.6 | UC-15 | POST /auth/forgot-password, /reset-password | PasswordResetTokens | TEST-B-015..017 |
| FR-1.7 | UC-03 | PUT /auth/me | Users | TEST-B-018 |
| FR-1.8 | UC-12 | POST /users/{id}/approve-teacher | Users | TEST-B-019 |
| FR-1.9 | UC-12 | GET/PUT /users | Users | TEST-B-020..022 |
| FR-1.11 | UC-03 | PUT /auth/2fa | Users | TEST-B-094..096 |
| FR-2.1 | UC-09 | CRUD /topics | Topics | TEST-B-023..025 |
| FR-2.2 | UC-09 | CRUD /lessons | Lessons, LessonSimulations | TEST-B-026..030 |
| FR-2.3 | UC-04 | GET /lessons | Lessons | TEST-B-031 |
| FR-2.4 | UC-04 | GET /lessons/{id}, POST mark-viewed | UserProgress | TEST-B-032..034 |
| FR-2.5 | UC-05 | GET /lessons?q= | Lessons | TEST-B-035 |
| FR-2.6 | UC-22 | PUT /me/notes/{lessonId} | LessonNotes | TEST-B-097..098 |
| FR-2.10 | UC-25 | GET /learning-path/{id}, POST enter | LearningPaths, LearningPathNodes, NodeSessions | TEST-B-133..136 |
| FR-2.11 | UC-01 | /simulator/{key}?step=N (deep-link) | — | TEST-UI-008 |
| FR-3.1 | UC-01 | GET /simulations | seed catalog | TEST-E-001..032 |
| FR-3.2 | UC-01 | (frontend) + enter | NodeSessions, Users | TEST-B-148..155 |
| FR-3.3 | UC-01 | (frontend) | — | TEST-UI-002 |
| FR-3.4 | UC-01 | GET /simulations/{key}/schema | schema code | TEST-E-010..011 |
| FR-3.5 | UC-01 | (frontend) | — | TEST-UI-003 |
| FR-3.6 | UC-01 | (frontend) | — | TEST-E (bảng trạng thái) |
| FR-3.7 | UC-01 | (frontend) | — | TEST-E-001..010 |
| FR-3.9 | UC-01 | (frontend) | — | TEST-E-002 |
| FR-3.10 | UC-01 | CRUD /favorites | Favorites | TEST-B-036..038 |
| FR-3.11 | UC-01 | URL param | — | TEST-UI-006 |
| FR-3.12 | UC-01 | (frontend engine) | — | TEST-E-033 |
| FR-3.14 | UC-01 | (frontend engine) | — | TEST-E-006 |
| FR-3.15 | UC-01 | (frontend engine) | — | TEST-E (bổ sung) |
| FR-3.16 | UC-01 | (frontend + mini quiz) | ExerciseSubmissions | TEST-B-101..103 |
| FR-3.18 | — | (frontend) | Settings (theme) | TEST-UI-012 |
| FR-3.20/3.20b | UC-28 | POST /benchmarks/run | (tính toán) | TEST-UI-010, TEST-PERF |
| FR-4.1 | UC-10 | CRUD /exercises | Exercises, Questions | TEST-B-039..044 |
| FR-4.2 | UC-06 | GET /exercises/{id}, POST submit | ExerciseSubmissions | TEST-B-045..052 |
| FR-4.3 | UC-07 | POST /exercises/{id}/submit (lab) | ExerciseSubmissions | TEST-B-053..058 |
| FR-4.4 | UC-06 | GET /exercises/{id}/submissions/me | ExerciseSubmissions | TEST-B-059..061 |
| FR-4.5 | UC-10 | (ngân hàng câu hỏi) | Questions | TEST-B-044 |
| FR-4.6 | UC-06 | POST /exercises/{id}/practice | (không ghi điểm) | TEST-B-105..107 |
| FR-4.7 | UC-06 | (hints) | Questions | TEST-B-108..110 |
| FR-4.8 | UC-06 | (frontend + seed) | — | TEST-B-111 |
| FR-4.9 | UC-06 | POST /submit (kết quả) | Questions | TEST-B-112 |
| FR-4.10 | UC-10 | POST /exercises/import-csv | Questions | TEST-B-113..115 |
| FR-4.11 | UC-26 | POST /exercises/{id}/submit (ladder guard) | ExerciseSubmissions | TEST-B-137..141 |
| FR-4.12 | UC-27 | GET /learning-path/{id}/final-test | Questions (trộn) | TEST-B-142..144 |
| FR-5.1 | UC-08 | mark-viewed, submit | UserProgress | TEST-B-062..064 |
| FR-5.2 | UC-08 | GET /progress/me | UserProgress | TEST-UI-001 |
| FR-5.3 | UC-11 | GET /progress/report, /export | ExerciseSubmissions | TEST-B-065..068 |
| FR-5.4 | — | GET /admin/stats | nhiều bảng | TEST-B-069 |
| FR-5.5 | UC-23 | GET /achievements | Achievements, UserAchievements | TEST-B-116..119 |
| FR-6.2 | UC-13 | GET/PUT /settings | Settings | TEST-B-073 |
| FR-7.1/7.6 | UC-14 | GET /public/* | — | TEST-UI-004 |
| FR-7.2 | — | GET /public/faqs | tĩnh | TEST-UI (FAQ) |
| FR-7.4 | UC-24 | POST /feedback | ContentFeedback | TEST-B-076..077 |
| FR-8.1 | UC-20 | CRUD /classes | Classes | TEST-B-079..081 |
| FR-8.2 | UC-20/21 | POST/DELETE members, /join | ClassMembers | TEST-B-082..084 |
| FR-8.3 | UC-20 | POST /classes/{id}/assignments | ClassAssignments | TEST-B-085..087 |
| FR-8.4 | UC-20 | GET /classes/{id}/report, /export | Classes* | TEST-B-088..090 |
| FR-9.1 | UC-17 | (frontend) | CodeRuns | TEST-B-124..125 |
| FR-9.2 | UC-17 | (frontend engine) | CodeRuns | TEST-UI-011 |
| FR-9.3 | UC-18 | POST /exercises/{id}/code-submit | CodeSubmissions | TEST-B-126..130 |
| FR-9.4 | UC-17 | (sandbox) | — | TEST-SEC-009..011 |
| FR-9.5 | UC-19 | GET code-submissions/me | CodeSubmissions | TEST-B-131..132 |
| FR-9.6 | UC-17 | (sandbox limits) | CodeRuns | TEST-B-145..147 |
| FR-10.1 | UC-25 | POST nodes/{nodeId}/enter | Users (Hearts), NodeSessions | TEST-B-148..155 |
| FR-10.2 | UC-30 | GET /shop/items, POST /shop/buy | ShopItems, UserInventory, GemTransactions | TEST-B-156..162 |
| FR-10.3 | UC-29 | GET /me/quests, POST claim | DailyQuests, UserQuests | TEST-B-163..167 |
| FR-10.4 | UC-29 | GET /me/streak + job 00:30 | Users (Streak) | TEST-B-168..171 |
| FR-10.5 | UC-25/26 | (tính XP) | Users (Xp) | TEST-B-172..174 |
| FR-10.6 | UC-31 | GET /leaderboard | Users + XP | TEST-B-175..177 |
| FR-10.7 | UC-32 | POST /premium/upgrade, /mock-pay + job | PremiumSubscriptions | TEST-B-178..183 |


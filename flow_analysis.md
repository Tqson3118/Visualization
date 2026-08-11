# 🗺️ USER FLOW HOÀN CHỈNH — NỀN TẢNG HỌC DSA (v4.0)

> [!NOTE]
> Phiên bản v4.0 tích hợp **9 tính năng mới**: Roadmap Custom Builder, Teacher Application Flow, Classroom System, Code-to-Visual Live Sandbox, Language Selector, Progressive Practice Ladder, Gems Shop mở rộng, Rich Theory Layout, và Learning Session cải tiến.

---

## PHẦN 1 — MÔ TẢ FLOW DẠNG MŨI TÊN

### 🏠 Flow chính

```
Home (Landing Page — Chưa đăng nhập)
  ├── Trải nghiệm thử Visualizer mẫu (không cần đăng nhập, giới hạn 3 thuật toán demo)
  ├── Đăng nhập → Dashboard
  └── Đăng ký → Xác nhận Email → Onboarding Tutorial (có nút Skip) → Dashboard
```

### 📊 Dashboard → 8 Module

```
Dashboard (Sau đăng nhập)
  │
  ├── Thanh trạng thái: ❤️ Tim · 🔥 Streak · ⭐ XP · 💎 Gems · [Free|Premium|Teacher] · 🔔 Thông báo
  ├── Nút "Tiếp tục học" → Node đang dở gần nhất (skip Visualizer nếu đã xem)
  ├── Nhiệm vụ Hàng ngày (5 quests: 2 Easy + 2 Medium + 1 Hard) → Hoàn thành → Nhận Tim / XP / Gems
  │
  ├─→ 🗺️ [Module 1] Visual / Roadmap ──────→ (xem chi tiết bên dưới)
  ├─→ 📊 [Module 2] DSA CheatSheet ────────→ Chọn CTDL → Xem Big-O + Code (đồng bộ ngôn ngữ đã chọn)
  ├─→ 🏆 [Module 3] Xếp hạng (Leaderboard) → Xem BXH Tuần / Level / Lớp học
  ├─→ 🤖 [Module 4] AI Assistant ──────────→ Chat tự do (Free: 5 msg/ngày, Premium: 50/ngày)
  ├─→ 👤 [Module 5] Hồ sơ (Profile) ──────→ Xem stats / Badges / Heatmap / Settings / Đăng ký Teacher
  ├─→ 🏫 [Module 6] Lớp Học (Classroom) ──→ [Student] Nhập mã → Vào lớp | [Teacher] Quản lý lớp
  ├─→ 💎 [Module 7] Cửa Hàng Gems ────────→ Mua Hint Token / Avatar / Khung VIP / Items
  └─→ 🛠️ [Module 8] Teacher Studio ───────→ [Chỉ Teacher/Admin] Tạo & Quản lý Roadmap Custom
```

---

### 👩‍🏫 Flow Đăng Ký Làm Giáo Viên (Teacher Application)

```
Profile
  └─→ Nút "Đăng ký làm Giáo viên"
        │
        ├── Điền form: Tên trường/tổ chức · CV/Portfolio (link hoặc upload PDF ≤5MB) · Lý do
        │
        └── Gửi đơn → Trạng thái: "⏳ Đang chờ duyệt"
              │
              ├── [Admin nhận thông báo] → Admin Panel → Xem đơn
              │     ├── ✅ Approve → Tài khoản nâng cấp thành Teacher
              │     │     └── Push notification: "Chúc mừng! Bạn đã trở thành Giáo viên 🎉"
              │     │     └── Mở khóa Module 7 (Teacher Studio) + Module 6 (Classroom — quyền Teacher)
              │     │
              │     └── ❌ Reject → Kèm lý do từ chối
              │           └── Push notification: "Đơn bị từ chối: [Lý do]. Có thể nộp lại sau 30 ngày."
              │
              └── [Student xem] Profile → Trạng thái đơn: ⏳ Pending | ✅ Approved | ❌ Rejected
```

---

### 🏫 Flow Lớp Học (Classroom)

```
[TEACHER — Tạo lớp]
Dashboard → Module 7 (Classroom) → Nút "Tạo lớp mới"
  ├── Nhập: Tên lớp · Mô tả · Chọn Roadmap gán cho lớp (System hoặc Custom)
  ├── Hệ thống sinh Join Code 6 ký tự (VD: DSA999) + Link mời
  ├── Quản lý lớp:
  │     ├── Danh sách học sinh → Xem tiến độ từng người
  │     ├── Teacher Analytics: Tiến độ · Lịch sử làm bài Quiz/LeetCode · Điểm số
  │     ├── Xuất báo cáo Excel
  │     ├── Gỡ học sinh khỏi lớp
  │     └── Xóa lớp (xác nhận 2 bước)
  └── Roadmap Visibility: Classroom-only (chỉ thành viên lớp thấy)

[STUDENT — Vào lớp]
Dashboard → Module 6 (Classroom) → Nhập Join Code (VD: DSA999)
  ├── Mã hợp lệ → Tham gia lớp thành công
  │     ├── Mở khóa Roadmap riêng của lớp (dùng chung hệ thống tim/session)
  │     ├── Xem BXH nội bộ lớp (Tab mới trong Module 3)
  │     └── Nhận thông báo từ giáo viên
  │
  └── Mã sai / hết hạn → Thông báo lỗi → Thử lại
```

---

### 🗺️ Flow Module 1 — Visual / Roadmap (v4.0 — CHI TIẾT)

```
Dashboard
  └─→ Visual / Roadmap
        │
        │   [Danh sách Roadmap hiện có]
        │   ├── 🌐 System Roadmaps (Public — mặc định hệ thống)
        │   ├── 👩‍🏫 Teacher Roadmaps (Public — giáo viên tạo, Admin duyệt)
        │   └── 🏫 Classroom Roadmaps (chỉ hiện nếu đã join lớp)
        │
        └─→ Chọn 1 Roadmap → 🔤 LANGUAGE SELECTOR (bước mới)
              │
              │   [Popup chọn ngôn ngữ: C++ | Java | Python | JavaScript]
              │   (Lưu per-Roadmap, có thể đổi lại từ Settings trong Roadmap)
              │   → Toàn bộ Code Editor, LeetCode templates, CheatSheet, Code-to-Visual
              │     sẽ tự động đồng bộ sang ngôn ngữ này
              │
              └─→ Bản đồ con đường Node
                    │
                    │   [Con đường Node: Array → Linked List → Stack/Queue → ... → DP]
                    │   [Trạng thái Node: 🔒 Locked | ⚡ In-Progress | ✅ Completed (1-3⭐)]
                    │
                    └─→ Bấm vào 1 Node
                          │
                          ├── [🔒 Locked] → Hiện tooltip "Hoàn thành Node X trước" → Chặn
                          │
                          └── [⚡ / ✅] → KIỂM TRA TIM (server-side atomic)
                                │
                                ├── ⚠️ Đang có session dở (crash/mất mạng trước đó, < 30 phút)
                                │     └── Cho vào lại MIỄN PHÍ (resume session, không trừ tim)
                                │         └── Resume tại đúng bước đang dở (Theory/Sandbox/Quiz/Lab/Code)
                                │
                                ├── Hết Tim (0 ❤️) → Màn hình "Hết Tim"
                                │     ├── ⏱ Chờ tự hồi (Free: 1h/tim, Premium: 30p/tim)
                                │     ├── 📺 Xem quảng cáo → +2 tim (max 5 lần / 24h sliding window, server verify)
                                │     └── 📋 Làm nhiệm vụ hàng ngày → Nhận Tim / Gems
                                │
                                └── Còn Tim (>0 ❤️) → Server trừ 1 tim (atomic) → Tạo session (TTL 30 phút) → Vào bài học
                                      │
                                      │── [Bước 1] 📖 Rich Theory — Lý thuyết giàu trải nghiệm
                                      │     ├── Nội dung: Văn bản + Hình ảnh minh họa + Video ngắn (1-3p)
                                      │     ├── Công thức toán MathJax + Sơ đồ Mermaid.js
                                      │     ├── Xem Visualizer tương tác (▶ Play | ⏸ Pause | ⏭⏮ Step | 🎚 Speed)
                                      │     ├── Two-way sync (lý thuyết ↔ animation)
                                      │     ├── Autosave mỗi 5s (+ offline queue)
                                      │     └── Điều kiện tiếp: ≥90% frames (≥500ms/frame)
                                      │         HOẶC Play tự động ≥60% tổng thời gian animation
                                      │
                                      │── [Bước 1.5] ⚡ Code-to-Visual Live Sandbox
                                      │     ├── Học sinh gõ/dán code (C++/Python/JS/Java — theo ngôn ngữ đã chọn)
                                      │     ├── Hệ thống phân tích Execution Trace
                                      │     ├── Tự động vẽ Animation: biến đổi mảng, con trỏ, biến memory trên Canvas
                                      │     ├── Điều khiển: ▶ Play | ⏸ Pause | ⏭ Step | 🎚 Speed | 🔄 Reset
                                      │     ├── ⚡ Bước này KHÔNG bắt buộc — có nút "Bỏ qua" để đến Practice
                                      │     └── Sandbox limits: max 200 dòng code, timeout 10s, memory 64MB
                                      │
                                      │── [Bước 2] 🎯 Progressive Practice Ladder (TUẦN TỰ BẮT BUỘC)
                                      │     │
                                      │     │   ⚡ PHẢI pass bài trước mới mở khóa bài tiếp theo
                                      │     │   ⚡ Retry từng bước không giới hạn trong session (không trừ tim thêm)
                                      │     │
                                      │     ├── [2a] 📝 Quiz (Trắc nghiệm lý thuyết & Big-O)
                                      │     │     ├── 5-10 câu hỏi · Tiêu chí Pass: ≥60% đúng
                                      │     │     ├── ✅ Pass → Mở khóa Interactive Lab
                                      │     │     └── ❌ Fail → [🔁 Làm lại Quiz] (retry không giới hạn, không trừ tim)
                                      │     │
                                      │     ├── [2b] 🧪 Interactive Lab (Kéo thả / Xếp thao tác trên Visual) [🔒 cần pass Quiz]
                                      │     │     ├── Hoàn thành đúng thứ tự/logic thao tác (server chấm)
                                      │     │     ├── ✅ Pass → Mở khóa LeetCode
                                      │     │     └── ❌ Fail → [🔁 Làm lại Lab] (retry không giới hạn, không trừ tim)
                                      │     │
                                      │     └── [2c] 💻 LeetCode (Viết code) [🔒 cần pass Lab]
                                      │           │
                                      │           ├── Code Editor (ngôn ngữ đồng bộ theo Language Selector)
                                      │           ├── Kết quả submit:
                                      │           │     ├── ⚠️ Compile Error → Hiện compiler output (không tính FAIL)
                                      │           │     ├── ❌ Runtime Error → Hiện testcase # + error type
                                      │           │     ├── ⏱ TLE → Hiện testcase # + "vượt X ms"
                                      │           │     ├── 💾 MLE → Hiện testcase # + "vượt X MB"
                                      │           │     ├── ❌ Wrong Answer → Hiện Expected vs Got
                                      │           │     └── ✅ Accepted → ≥70% testcases = pass bước này
                                      │           │
                                      │           └── 🤖 Trợ giúp AI (áp dụng cho cả 3 bước practice)
                                      │                 ├── 💡 Hint 1 (miễn phí cho tất cả — mỗi bài)
                                      │                 ├── 💡 AI Hint Token (mua từ Gems Shop — không tính premium quota)
                                      │                 ├── 💡 Hint 2 → Pseudocode (Premium, tính vào 30 req/ngày)
                                      │                 ├── 🐞 Phân tích Bug / TLE (Premium, tính vào 30 req/ngày)
                                      │                 ├── ⚡ Tối ưu Big-O (Premium, tính vào 30 req/ngày)
                                      │                 ├── 🔍 Giải thích bước Visual (Premium, tính vào 30 req/ngày)
                                      │                 └── ⏱ Cooldown: tối thiểu 10 giây giữa 2 request AI
                                      │
                                      └── [Bước 3] Nộp bài / Kết quả tổng hợp
                                            │
                                            │   Score Node = Quiz(20%) + Lab(30%) + Code(50%)
                                            │   (weighted average, tính sau khi pass cả 3 bước)
                                            │
                                            └── ✅ PASS (đã pass cả 3 bước)
                                                  ├── +XP, +Gems, Lưu tiến độ Node
                                                  ├── ⭐ Xếp sao: score = weighted average
                                                  │     ⭐ (≥60%) · ⭐⭐ (≥80%) · ⭐⭐⭐ (≥95%)
                                                  ├── Node score lưu = MAX(score hiện tại, score cũ)
                                                  ├── 📈 Benchmark: Runtime (ms) · Percentile · Memory (KB)
                                                  ├── 🏅 BXH bài học: Top 1🥇 2🥈 3🥉
                                                  │     └── "Xem Approach" (pseudocode + giải thích, không full code)
                                                  │         (chỉ hiện sau khi tự pass)
                                                  └── Lựa chọn tiếp
                                                        ├── [▶ Next] → Node tiếp theo trên Roadmap
                                                        └── [🔁 Làm lại] → Cải thiện Score
                                                             (lần retry sau khi đã pass → trừ tim mới)
```

---

### 🛠️ Flow Module 8 — Teacher Studio (Roadmap Custom Builder)

```
Dashboard → Module 8 (Teacher Studio) [🔒 Chỉ Teacher/Admin]
  │
  ├── Danh sách Roadmap đã tạo (của mình)
  │     ├── Trạng thái: 📝 Draft | ⏳ Pending Review | ✅ Published | ❌ Rejected
  │     └── Actions: Sửa · Nhân bản · Xóa · Đổi Visibility
  │
  └── Nút "Tạo Roadmap mới"
        │
        ├── Bước 1: Thông tin Roadmap
        │     ├── Tên · Mô tả · Tags · Thumbnail
        │     └── Visibility: Public | Private (nháp) | Classroom-only
        │
        ├── Bước 2: Thiết kế Node (lặp lại cho mỗi Node)
        │     ├── Tên Node · Mô tả
        │     │
        │     ├── [Gắn Lý thuyết]
        │     │     ├── Soạn nội dung Rich Text (Markdown + MathJax + Mermaid)
        │     │     ├── Thêm hình ảnh minh họa (upload ≤5MB, PNG/JPG/WEBP)
        │     │     └── Thêm video bài giảng (embed YouTube/Vimeo hoặc upload ≤50MB)
        │     │
        │     ├── [Gắn Visualizer mẫu]
        │     │     └── Chọn từ thư viện Visualizer hệ thống (hoặc tạo mới nếu Admin)
        │     │
        │     └── [Gắn chuỗi bài tập — BẮT BUỘC đủ 3 dạng]
        │           ├── 📝 Quiz: Tạo/chọn bộ câu hỏi trắc nghiệm
        │           ├── 🧪 Lab: Tạo/chọn bài Lab tương tác
        │           └── 💻 LeetCode: Chọn từ ngân hàng bài hoặc tạo bài mới
        │                 └── Cung cấp: Đề bài · Test cases · Solution mẫu · Constraints
        │
        ├── Bước 3: Sắp xếp thứ tự Node (drag & drop)
        │
        └── Bước 4: Xuất bản
              ├── [Private] → Lưu nháp (chỉ mình thấy)
              ├── [Classroom-only] → Gán cho lớp → Học sinh lớp thấy ngay
              └── [Public] → Gửi Admin duyệt → ⏳ Pending
                    ├── Admin ✅ Approve → Hiện trên danh sách Roadmap cho toàn hệ thống
                    └── Admin ❌ Reject → Kèm lý do → Teacher sửa & nộp lại
```

---

### 📊 Flow Module 2 — DSA CheatSheet

```
Dashboard
  └─→ DSA CheatSheet
        ├── Bộ lọc: Chọn cấu trúc dữ liệu (Array, Linked List, Tree, Graph...)
        ├── Code snippet tự động đồng bộ theo ngôn ngữ đã chọn ở Roadmap
        │   (hoặc theo ngôn ngữ mặc định nếu chưa vào Roadmap nào)
        └── Xem bảng tra cứu
              ├── Time Complexity: Best / Average / Worst
              ├── Space Complexity
              ├── Code Snippet mẫu (theo ngôn ngữ đã chọn)
              └── 💎 [Premium] Download PDF + Advanced Snippets
```

---

### 🏆 Flow Module 3 — Xếp hạng

```
Dashboard
  └─→ Xếp hạng (Leaderboard)
        ├── Tab "Tuần này" → Top XP tuần (reset thứ Hai 00:00 UTC+7)
        ├── Tab "Theo Level" → BXH cùng khung cấp độ
        └── Tab "Lớp học" → BXH nội bộ từng lớp đã tham gia
              └── Chọn lớp → Xem BXH XP/Score trong lớp đó
              └── Chỉ hiện nếu đã join ≥1 lớp
```

---

### 🤖 Flow Module 4 — AI Assistant

```
Dashboard
  └─→ AI Assistant (Global Chat)
        ├── Hỏi lý thuyết DSA tự do
        ├── Tư vấn lộ trình học cá nhân
        ├── Hỗ trợ giải thích code / thuật toán
        ├── Rate limit: Free = 5 msg/ngày · Premium = 50 msg/ngày
        └── Cooldown: tối thiểu 10 giây giữa 2 request
```

---

### 👤 Flow Module 5 — Hồ sơ

```
Dashboard
  └─→ Hồ sơ (Profile)
        ├── Thông tin: Avatar (+ Khung VIP nếu có) · Tên · Level · XP · Streak 🔥 · Số Node đã pass
        ├── Bộ sưu tập Huy hiệu (Badges) — hiện badge nổi bật bên cạnh tên
        ├── Biểu đồ đóng góp (Heatmap) — tính theo: pass node, submit code, hoàn thành quest
        ├── Lịch sử học tập: Danh sách Node đã pass + Score + Thời gian
        ├── 👩‍🏫 Đăng ký làm Giáo viên (MỚI)
        │     ├── [Chưa nộp] → Nút "Đăng ký" → Điền form → Gửi đơn
        │     ├── [⏳ Pending] → Hiện trạng thái "Đang chờ duyệt"
        │     ├── [✅ Approved] → Badge "Teacher 👩‍🏫" + Link đến Teacher Studio
        │     └── [❌ Rejected] → Hiện lý do + Nộp lại sau 30 ngày
        ├── Quản lý Avatar
        │     ├── [Free] Chọn từ kho hệ thống (20+ avatar)
        │     ├── [Premium] Tải ảnh cá nhân + Khung VIP 👑 viền vàng
        │     └── [Gems Shop] Avatar Mascot độc quyền + Khung Neon/Vàng/Kim Cương
        └── ⚙️ Cài đặt (Settings)
              ├── Đổi mật khẩu
              ├── Đổi email
              ├── Notification preferences (bật/tắt từng loại)
              ├── Ngôn ngữ giao diện (Tiếng Việt / English)
              ├── Ngôn ngữ lập trình mặc định (C++ / Java / Python / JS)
              └── Xóa tài khoản (GDPR)
```

---

### 🔐 Flow Xác thực (Auth) — Cập nhật cho Multi-Role

```
[Chưa đăng nhập]
  ├── Đăng nhập
  │     ├── Email + Password → Xác thực → Dashboard (load role: Student/Teacher/Admin)
  │     ├── OAuth: Google / GitHub → Dashboard
  │     └── Sai thông tin → Hiện lỗi → Thử lại (lock sau 5 lần sai / 15 phút)
  │
  ├── Đăng ký (mặc định role = Student Free)
  │     ├── Nhập: Username (3-20 ký tự) · Email · Password (≥8, có chữ hoa + số + ký tự đặc biệt)
  │     ├── Gửi Email xác nhận → Bấm link → Kích hoạt tài khoản
  │     └── Onboarding Tutorial (lần đầu, có nút Skip) → Dashboard
  │
  └── Quên mật khẩu
        ├── Nhập email → Gửi link reset (hết hạn 15 phút, max 3 lần/email/giờ)
        └── Bấm link → Nhập mật khẩu mới → Đăng nhập lại
```

---
---

## PHẦN 2 — MÔ TẢ CHI TIẾT TỪNG MODULE

### 🏠 Home (Landing Page)

Người dùng chưa đăng nhập vào trang chủ, thấy:
- Hero banner giới thiệu nền tảng với animation DSA.
- **3 Visualizer demo miễn phí** (Bubble Sort, Binary Search, BFS) — trải nghiệm không cần đăng ký.
- Testimonials / Stats (số học viên, số bài đã pass).
- CTA "Đăng ký miễn phí" / "Đăng nhập".

Khi bấm vào bất kỳ tính năng học tập nào (ngoài 3 demo), hệ thống yêu cầu đăng nhập. Sau khi đăng nhập thành công → **Dashboard**.

---

### 📊 Dashboard

Trang trung tâm sau khi đăng nhập. Hiển thị:

**Thanh trạng thái (Header Bar):**
| Mục | Hiển thị |
|---|---|
| ❤️ Tim | `X/10` (Free) hoặc `X/30` (Premium) + timer hồi |
| 🔥 Streak | Số ngày liên tục + icon lửa (animation khi ≥7 ngày) |
| ⭐ XP | Tổng XP + thanh progress đến Level tiếp theo |
| 💎 Gems | Số Gems hiện có |
| 🏷️ Badge | `Free` · `Premium 👑` · `Teacher 👩‍🏫` |
| 🔔 Thông báo | Bell icon + badge đỏ số chưa đọc |

**Nội dung chính:**
- Nút **"▶ Tiếp tục học"** — dẫn thẳng đến Node đang dở gần nhất. Nếu đã xem Theory → skip thẳng đến Practice.
- **Nhiệm vụ Hàng ngày (Daily Quests)** — 5 nhiệm vụ, reset lúc 00:00 UTC+7 mỗi ngày.
- **Streak Check-in** — Điểm danh = hoàn thành ≥1 activity (pass quiz / submit code / xem xong visualizer).
- **Lớp học đang tham gia** (nếu có) — Quick access đến Classroom Roadmaps.

Từ Dashboard, điều hướng đến 8 module qua **Navigation Bar** (sidebar hoặc bottom nav trên mobile).

**Navigation phân quyền theo Role:**
| Module | Student | Teacher | Admin |
|---|---|---|---|
| 🗺️ Roadmap | ✅ | ✅ | ✅ |
| 📊 CheatSheet | ✅ | ✅ | ✅ |
| 🏆 Leaderboard | ✅ | ✅ | ✅ |
| 🤖 AI Assistant | ✅ | ✅ | ✅ |
| 👤 Profile | ✅ | ✅ | ✅ |
| 🏫 Classroom | ✅ (join) | ✅ (create+join) | ✅ (all) |
| 💎 Gems Shop | ✅ | ✅ | ✅ |
| 🛠️ Teacher Studio | ❌ | ✅ | ✅ |
| 🛡️ Admin Panel | ❌ | ❌ | ✅ |

---

### 🗺️ Module 1 — Visual / Roadmap (v4.0)

#### Giao diện Danh sách Roadmap (MỚI)

Khi vào Module Roadmap, hiển thị **danh sách Roadmap** thay vì đi thẳng vào 1 roadmap:

| Loại Roadmap | Icon | Mô tả | Ai thấy |
|---|---|---|---|
| 🌐 System | Globe | Roadmap mặc định hệ thống (Array → DP) | Tất cả |
| 👩‍🏫 Teacher Public | Chữ T | Roadmap giáo viên đã được Admin duyệt | Tất cả |
| 🏫 Classroom | Nhà trường | Roadmap gán cho lớp | Chỉ thành viên lớp |
| 📝 Private Draft | Ổ khóa | Roadmap nháp của Teacher | Chỉ Teacher tạo |

#### Bước 0 — 🔤 Chọn Ngôn Ngữ (Language Selector)

Khi bấm vào bất kỳ Roadmap nào:
- **Lần đầu:** Popup chọn ngôn ngữ — C++ / Java / Python / JavaScript.
- **Lần sau:** Nhớ lựa chọn của Roadmap đó. Có nút **đổi ngôn ngữ** trong thanh công cụ Roadmap.
- **Tác động:** Code Editor, LeetCode templates, Code-to-Visual Sandbox, CheatSheet snippet — tất cả đồng bộ theo ngôn ngữ đã chọn.
- **Lưu per-Roadmap:** Roadmap A chọn C++, Roadmap B chọn Python — hoàn toàn độc lập.

#### Bước 1 — Kiểm tra Tim & Session khi bấm vào Node

Mỗi lần vào bài học đều tiêu **1 tim (Entry Cost)**. Server tạo **Learning Session** (TTL 30 phút) để tracking.

**Hệ thống Tim (Hearts) — Tất cả xử lý Server-side:**

| | 🆓 Free | 💎 Premium |
|---|---|---|
| Tim tối đa | 10 ❤️ | 30 ❤️ |
| Tự hồi | 1 giờ / 1 tim | 30 phút / 1 tim |
| Xem quảng cáo | +2 tim / lần · **max 5 lần / 24h** (sliding window) | +2 tim / lần · **max 5 lần / 24h** (sliding window) |
| Làm nhiệm vụ | ✅ nhận Tim | ✅ nhận Tim |

**Quy tắc Session & Chống exploit:**

| Tình huống | Xử lý |
|---|---|
| Multi-tab trừ tim | Server dùng `UPDATE SET count = count - 1 WHERE count > 0` atomic |
| Đổi đồng hồ hồi tim | Server tính hồi tim dựa trên `last_heart_used_at` trong DB |
| Spam quảng cáo | Server verify callback từ ad network + sliding window 24h (không reset midnight) |
| Fake ad callback | Server-side Ad Verification (Google AdMob SSV) |
| **Crash / mất mạng giữa bài** | **Session còn hiệu lực (< 30 phút) → cho vào lại Node MIỄN PHÍ (resume)** |
| **Session hết hạn (> 30 phút)** | **Phải trừ tim mới để vào lại** |

**Flow kiểm tra:**
1. **Có session dở** (Node X, chưa hoàn thành, session < 30 phút) → Resume miễn phí → Quay lại đúng bước đang dở (Theory / Sandbox / Quiz / Lab / Code).
2. **Còn tim (> 0 ❤️)** → Server trừ 1 tim (atomic) → Tạo session mới (TTL 30p) → Vào bài học.
3. **Hết tim (0 ❤️)** → Response 402 → Client hiện màn hình "Hết Tim" với 3 lựa chọn:
   - ⏱ Chờ tự hồi (hiển thị countdown timer, tính từ server timestamp).
   - 📺 Xem quảng cáo → Server verify → +2 tim (hiện số lần còn lại: `X/5 trong 24h`).
   - 📋 Làm nhiệm vụ hàng ngày → nhận Tim / Gems.

---

#### Bước 2 — Bên trong bài học (v4.0 — 4 giai đoạn)

**2a. 📖 Rich Theory — Lý thuyết giàu trải nghiệm**
- Nội dung tích hợp đa phương tiện:
  - **Văn bản** format Markdown rich text.
  - **Hình ảnh minh họa** sắc nét (diagrams, infographics).
  - **Video bài giảng ngắn (1-3 phút)** nhúng trực tiếp (YouTube/Vimeo hoặc self-hosted).
  - **Công thức toán MathJax** — render LaTeX inline & block.
  - **Sơ đồ Mermaid.js** — flowcharts, sequence diagrams, tree diagrams.
- Mô phỏng Visualizer thuật toán chạy từng bước.
- Điều khiển: ▶ Play | ⏸ Pause | ⏭ Step Next | ⏮ Step Prev | 🎚 Speed Slider (0.25x → 3x).
- Two-way sync: Click lý thuyết → nhảy đến frame visual tương ứng và ngược lại.
- **Autosave** tiến trình mỗi 5s (frame index + scroll position + video position) → khôi phục khi quay lại.
  - Nếu mất mạng → queue offline, retry khi có mạng.

**Điều kiện mở khóa nút "Tiếp tục" (chống bypass):**
- Xem ≥90% frames, **mỗi frame phải hiển thị ≥500ms** (chống scrub nhanh)
- **HOẶC** để Play chạy tự động ≥60% tổng thời gian animation (chống bấm Play rồi Pause ngay)

---

**2b. ⚡ Code-to-Visual Live Sandbox (KHÔNG bắt buộc — có nút Bỏ qua)**

Sau khi hoàn thành lý thuyết, học sinh có thể thử Sandbox trước khi vào Practice:

| Thông số | Giá trị |
|---|---|
| Ngôn ngữ | Theo Language Selector (C++/Java/Python/JS) |
| Max dòng code | 200 dòng |
| Execution timeout | 10 giây |
| Memory limit | 64 MB |
| Network | Disabled (sandbox isolate) |

**Tính năng Sandbox:**
- Gõ/dán code → Bấm **"▶ Visualize"** → Hệ thống chạy Execution Trace.
- **Animation tự động:** Biến đổi mảng (highlight phần tử đang xử lý), di chuyển con trỏ, hiển thị biến memory, call stack.
- Điều khiển: ▶ Play | ⏸ Pause | ⏭ Step | 🎚 Speed | 🔄 Reset.
- **Có nút "Bỏ qua → Đến Practice"** — bước này là tùy chọn.
- Code templates sẵn theo Node đang học (VD: Node "Binary Search" → template binary search).

---

**2c. 🎯 Progressive Practice Ladder (TUẦN TỰ BẮT BUỘC)**

> [!IMPORTANT]
> **Quy tắc Practice Ladder (v4.0):**
> - Phải pass **Quiz → Lab → LeetCode** theo đúng thứ tự.
> - Retry từng bước không giới hạn trong session (không trừ thêm tim).
> - Pass cả 3 bước = pass Node.
> - Resume session quay lại **đúng bước đang dở** (VD: đã pass Quiz, đang làm Lab → resume vào Lab).

| Bước | Dạng | Mô tả | Tiêu chí Pass | Mở khóa |
|---|---|---|---|---|
| 2c-1 | 📝 Quiz | Trắc nghiệm lý thuyết & Big-O (5-10 câu) | ≥ 60% đúng | Mở Lab |
| 2c-2 | 🧪 Interactive Lab | Kéo thả / xếp thao tác trực tiếp trên visual | Hoàn thành đúng thứ tự/logic (server chấm) | Mở LeetCode |
| 2c-3 | 💻 LeetCode | Viết code theo ngôn ngữ đã chọn | ≥ 70% hidden testcases passed | Pass Node |

**Code Editor — Chi tiết kỹ thuật (LeetCode):**

| Thông số | Giá trị |
|---|---|
| Time limit / testcase | 5 giây |
| Memory limit | 256 MB |
| Số hidden testcases / bài | 10-20 testcases |
| Sandbox | Docker isolate (network disabled, filesystem readonly) |
| Output size limit | 10 MB |
| Ngôn ngữ | Đồng bộ theo Language Selector |

**Kết quả Submit Code:**
```
├── ⚠️ Compile Error → Hiện compiler output → KHÔNG tính FAIL (sửa & submit lại)
├── ❌ Runtime Error → Hiện testcase # + error type (Segfault, IndexOutOfBounds...)
├── ⏱ Time Limit Exceeded → Hiện testcase # + "vượt 5000ms"
├── 💾 Memory Limit Exceeded → Hiện testcase # + "vượt 256MB"
├── ❌ Wrong Answer → Hiện testcase # + Expected vs Got (chỉ 3 testcase đầu)
└── ✅ Accepted → ≥70% pass = pass bước LeetCode
```

**Score tính theo Weighted Average (v4.0):**

```
Score calculation:
- Quiz: quiz_score = (số câu đúng / tổng câu) × 100
- Lab: lab_score = (số thao tác đúng / tổng thao tác) × 100
- Code: code_score = (testcases passed / total testcases) × 100

Node score = quiz_score × 0.20 + lab_score × 0.30 + code_score × 0.50

Node final score = MAX(score hiện tại, score lần trước)
```

> [!IMPORTANT]
> **Trợ giúp AI trong bài học (áp dụng cho cả Quiz, Lab, LeetCode):**
> - 💡 **Hint 1** = Miễn phí cho tất cả users (1 hint / bài / bước).
> - 💡 **AI Hint Token** (mua từ Gems Shop) = Dùng 1 token → nhận thêm 1 hint. Không tính vào premium quota.
> - 💡 **Hint 2 → Pseudocode** = 💎 Premium (tính vào 30 AI req/ngày).
> - 🐞 **Phân tích Bug / TLE** = 💎 Premium (tính vào 30 AI req/ngày).
> - ⚡ **Tối ưu Big-O** = 💎 Premium (tính vào 30 AI req/ngày).
> - 🔍 **Giải thích bước Visualizer** = 💎 Premium (tính vào 30 AI req/ngày).
> - ⏱ **Cooldown:** Tối thiểu 10 giây giữa 2 request AI.

---

#### Bước 3 — Nộp bài & Kết quả (v4.0)

Sau khi pass cả 3 bước Practice Ladder, hệ thống tính kết quả tổng hợp:

**✅ PASS (đã hoàn thành cả Quiz + Lab + LeetCode):**
- Cộng XP + Gems + lưu tiến độ Node.
- Xếp sao Node: ⭐ (≥60%) · ⭐⭐ (≥80%) · ⭐⭐⭐ (≥95%) — dựa trên weighted score.
- **Bonus XP khi nâng sao (chỉ 1 lần):**
  - 1⭐ → 2⭐: +10 XP bonus
  - 2⭐ → 3⭐: +15 XP bonus
- Thống kê: Runtime (ms) · Percentile · Memory (KB) — từ bước LeetCode.
- BXH bài học: Top 1🥇 2🥈 3🥉 nhanh nhất.
  - **"Xem Approach"** — hiện pseudocode + giải thích hướng tiếp cận (KHÔNG hiện full code — chống plagiarism). Chỉ hiện sau khi user đã tự pass.
- Lựa chọn tiếp:
  - **[▶ Next]** → Node tiếp theo trên Roadmap (mở khóa tự động).
  - **[🔁 Làm lại]** → Cải thiện Score/Sao (lần retry này → trừ tim mới vì session cũ đã đóng).

> [!NOTE]
> **Khác biệt v3.0 vs v4.0:** Không còn trạng thái FAIL tổng Node vì user phải pass từng bước trước khi tiến tới bước tiếp theo. FAIL chỉ xảy ra ở từng bước riêng (Quiz fail → retry Quiz, Lab fail → retry Lab, Code fail → retry Code) và đều không trừ tim.

---

### 📊 Module 2 — DSA CheatSheet

Bảng tra cứu Big-O tương tác.

**Tính năng:**
- Lọc theo cấu trúc dữ liệu (Array, Linked List, Hash Table, Tree, Graph...).
- Xem bảng: Time Complexity (Best / Avg / Worst) · Space Complexity.
- **Code Snippet tự đồng bộ** theo ngôn ngữ đã chọn ở Roadmap (hoặc ngôn ngữ mặc định trong Settings).
- 💎 **Premium:** Download PDF tổng hợp + Advanced Snippets (Segment Tree, Trie...).

---

### 🏆 Module 3 — Xếp hạng (Leaderboard) (v4.0)

**3 Tab xếp hạng:**

| Tab | Mô tả | Reset |
|---|---|---|
| 🗓️ Tuần này | Top XP kiếm được trong tuần | Thứ Hai 00:00 UTC+7 |
| 📊 Theo Level | BXH cùng khung cấp độ (Level 1-5, 6-10...) | Không reset |
| 🏫 Lớp học | BXH nội bộ từng lớp đã tham gia | Thứ Hai 00:00 UTC+7 |

**Quy tắc BXH Lớp học:**
- Tính XP + Score từ Roadmap được gán cho lớp.
- Chỉ hiện cho thành viên lớp + Giáo viên tạo lớp.
- Teacher xem được chi tiết tiến độ từng học sinh.

---

### 🤖 Module 4 — AI Assistant (Global)

Khung chat AI ngoài bài học — hỏi lý thuyết tự do, tư vấn lộ trình, hỗ trợ code.

**Phân biệt với AI trong bài:**

| | AI Global (Module 4) | AI In-lesson (Bước 2) |
|---|---|---|
| Context | Không biết bạn đang học bài nào | Biết Node nào, code gì, lỗi gì |
| Mục đích | Q&A tự do, tư vấn lộ trình | Gợi ý, debug, tối ưu cụ thể |
| Rate limit Free | 5 msg/ngày | Chỉ Hint 1 miễn phí (mỗi bài/bước) |
| Rate limit Premium | 50 msg/ngày | 30 AI requests/ngày (shared pool) |
| AI Hint Token | Không áp dụng | ✅ Dùng token = thêm 1 hint |
| Cooldown | 10 giây / request | 10 giây / request |

> [!NOTE]
> **Shared pool:** AI Global và AI In-lesson Premium dùng **chung 1 pool quota** (tổng cộng 50 req/ngày cho Premium, trong đó In-lesson chiếm tối đa 30). Hoặc nếu muốn đơn giản hơn: tách riêng 50 global + 30 in-lesson.

---

### 👤 Module 5 — Hồ sơ (Profile) (v4.0)

**Thông tin hiển thị:**
- Avatar (+ Khung VIP/Gems nếu có) · Tên · Level · XP (thanh progress) · Streak 🔥 · Số Node đã pass / Tổng.
- Badge vai trò: `Student` · `Premium 👑` · `Teacher 👩‍🏫`.
- Bộ sưu tập Huy hiệu (Badges) — grid 3D glassmorphism, hiện tiến trình unlock.
- Biểu đồ đóng góp (Contribution Heatmap) — Activity = pass node + submit code + hoàn thành quest.
- Lịch sử học tập: Danh sách Node đã pass, Score, Thời gian, Stars.

**👩‍🏫 Đăng ký Giáo viên (MỚI):**
- Nút "Đăng ký làm Giáo viên" (chỉ hiện cho Student).
- Form: Tên trường/tổ chức · CV/Portfolio (link URL hoặc upload PDF ≤5MB) · Lý do đăng ký (≥50 ký tự).
- Sau khi gửi: hiện trạng thái đơn trực tiếp trên Profile.
- Cooldown nộp lại: **30 ngày** sau khi bị reject.

**Quản lý Avatar (v4.0 — mở rộng với Gems Shop):**
- 🆓 Free → Chọn từ kho 20+ avatar hệ thống.
- 💎 Premium → Tải ảnh cá nhân (≤2MB, PNG/JPG/WEBP) + Khung VIP 👑 viền vàng.
- 💎 Gems Shop → Avatar Mascot độc quyền (mua bằng Gems) + Khung Viền Neon/Vàng/Kim Cương.

**⚙️ Cài đặt (Settings):**
- Đổi mật khẩu · Đổi email · Notification preferences · Ngôn ngữ giao diện · Ngôn ngữ lập trình mặc định · Xóa tài khoản.

---

### 🏫 Module 6 — Lớp Học (Classroom) (MỚI)

#### Giao diện Student

```
Classroom → Tab "Lớp của tôi"
  ├── Danh sách lớp đã tham gia (Tên lớp · Giáo viên · Số thành viên · Roadmap)
  ├── Bấm vào lớp → Roadmap lớp + BXH nội bộ
  └── Nút "Nhập mã vào lớp"
        ├── Nhập Join Code 6 ký tự (VD: DSA999)
        ├── Mã hợp lệ → Popup xác nhận "Tham gia lớp [Tên]?" → ✅ Tham gia
        │     └── Thêm Roadmap lớp vào danh sách Roadmap
        └── Mã sai / hết hạn → "Mã không hợp lệ" → Thử lại
```

#### Giao diện Teacher

```
Classroom → Tab "Lớp tôi dạy"
  ├── Danh sách lớp đã tạo
  ├── Nút "Tạo lớp mới"
  │     ├── Tên lớp · Mô tả
  │     ├── Chọn Roadmap gán (System / Custom đã Published)
  │     └── Tạo → Sinh Join Code 6 ký tự + Link mời
  │
  └── Bấm vào lớp → Quản lý chi tiết
        ├── 📋 Thông tin lớp: Tên · Mô tả · Join Code · Link mời · Số thành viên
        ├── 👥 Danh sách học sinh
        │     ├── Tên · Tiến độ (X/Y nodes) · Score trung bình · Lần hoạt động gần nhất
        │     └── Bấm vào học sinh → Chi tiết tiến độ từng Node
        ├── 📊 Teacher Analytics
        │     ├── Biểu đồ tổng quan lớp: % hoàn thành · Phân bố Score · Node khó nhất
        │     ├── Lịch sử làm bài Quiz/LeetCode từng học sinh
        │     ├── Cảnh báo: Học sinh không hoạt động >7 ngày
        │     └── 📥 Xuất báo cáo Excel (.xlsx)
        │           ├── Sheet 1: Tổng quan (Tên · Email · Tiến độ · Score · Stars)
        │           ├── Sheet 2: Chi tiết từng Node (Quiz/Lab/Code score)
        │           └── Sheet 3: Lịch sử submit
        ├── ⚙️ Cài đặt lớp
        │     ├── Đổi tên · Đổi mô tả
        │     ├── Đổi Roadmap (cảnh báo: reset tiến độ nếu đổi)
        │     ├── Tạo mới Join Code (vô hiệu mã cũ)
        │     └── Gỡ học sinh khỏi lớp
        └── 🗑️ Xóa lớp (xác nhận 2 bước: nhập tên lớp để xác nhận)
```

**Giới hạn:**
| Thông số | Giá trị |
|---|---|
| Max lớp / Teacher | 10 lớp |
| Max học sinh / lớp | 100 người |
| Join Code format | 6 ký tự alphanumeric (A-Z, 0-9) |
| Join Code expiry | Không hết hạn (trừ khi Teacher tạo mã mới) |

---

### 💎 Module 7 — Cửa Hàng Gems (Gems Shop) (MỚI — tách riêng)

```
Dashboard → Gems Shop
  ├── 💰 Số Gems hiện có: XXX 💎
  │
  ├── 🛒 Danh mục vật phẩm:
  │     │
  │     ├── 💡 AI Hint Tokens (MỚI)
  │     │     └── 1 Token = 30 💎 · Dùng trong bài để nhận thêm 1 hint
  │     │     └── Max tích trữ: 10 tokens
  │     │
  │     ├── 🧊 Streak Freeze
  │     │     └── 1 lượt = 100 💎 · Max tích trữ: 2 lượt
  │     │
  │     ├── 🖼️ Avatar Độc Quyền (MỚI)
  │     │     └── Bộ sưu tập avatar mascot học thuật
  │     │     └── 200 💎 / avatar · Sở hữu vĩnh viễn
  │     │
  │     ├── 👑 Khung Viền Avatar VIP (MỚI)
  │     │     ├── Khung Neon: 300 💎
  │     │     ├── Khung Vàng: 500 💎
  │     │     └── Khung Kim Cương: 1000 💎
  │     │     └── Hiển thị ở Comment + BXH + Profile
  │     │     └── Sở hữu vĩnh viễn · Chỉ equip 1 khung tại 1 thời điểm
  │     │
  │     ├── 🎨 Theme màu giao diện
  │     │     └── 150 💎 · Sở hữu vĩnh viễn
  │     │
  │     └── ⚡ XP Boost 2x (24 giờ)
  │           └── 300 💎 · Mua thêm = cộng dồn thời gian
  │
  └── 📜 Lịch sử giao dịch
```

---

### 🛠️ Module 8 — Teacher Studio (MỚI)

*(Xem Flow chi tiết ở PHẦN 1 — Flow Module 8)*

**Tóm tắt:**
- Chỉ truy cập được khi role = Teacher hoặc Admin.
- Tạo, sửa, nhân bản, xóa Roadmap custom.
- Mỗi Node bắt buộc gắn đủ: Lý thuyết (text + ảnh/video) + Visualizer mẫu + 3 dạng Practice (Quiz + Lab + LeetCode).
- Visibility: Public (cần Admin duyệt) · Private (nháp) · Classroom-only.

---

### 🛡️ Admin Panel (Chỉ Admin)

```
Admin Panel (Truy cập riêng — không hiện cho Student/Teacher)
  ├── 📋 Duyệt đơn Giáo viên
  │     ├── Danh sách đơn Pending → Xem chi tiết (CV, Portfolio, Lý do)
  │     ├── ✅ Approve → Nâng role → Notification
  │     └── ❌ Reject → Nhập lý do → Notification
  │
  ├── 📋 Duyệt Roadmap Public
  │     ├── Danh sách Roadmap Pending → Preview nội dung
  │     ├── ✅ Approve → Published
  │     └── ❌ Reject → Nhập lý do → Teacher sửa & nộp lại
  │
  ├── 👥 Quản lý Users
  │     ├── Danh sách users → Filter theo role · status
  │     ├── Đổi role (Student ↔ Teacher)
  │     ├── Ban/Unban user
  │     └── Xem activity log
  │
  ├── 📊 Thống kê hệ thống
  │     ├── DAU / MAU / Retention
  │     ├── Phân bố user theo Level / Role
  │     └── Top Roadmaps (lượt học, completion rate)
  │
  └── 🔔 Quản lý Notification
        └── Gửi thông báo toàn hệ thống / nhóm
```

---

### 🔐 Flow Xác thực (Auth)

```
[Chưa đăng nhập]
  ├── Đăng nhập
  │     ├── Email + Password → Xác thực → Load role (Student/Teacher/Admin) → Dashboard
  │     ├── OAuth: Google / GitHub → Dashboard
  │     └── Sai thông tin → Hiện lỗi → Thử lại (lock sau 5 lần sai / 15 phút)
  │
  ├── Đăng ký (mặc định role = Student Free)
  │     ├── Nhập: Username (3-20 ký tự) · Email · Password (≥8, có chữ hoa + số + ký tự đặc biệt)
  │     ├── Gửi Email xác nhận → Bấm link → Kích hoạt tài khoản
  │     └── Onboarding Tutorial (lần đầu, có nút Skip) → Dashboard
  │
  └── Quên mật khẩu
        ├── Nhập email → Gửi link reset (hết hạn 15 phút, max 3 lần/email/giờ)
        └── Bấm link → Nhập mật khẩu mới → Đăng nhập lại
```

---
---

## PHẦN 3 — HỆ THỐNG GAMIFICATION CHI TIẾT

### ❤️ Hệ thống Tim (Hearts)

```
                    ┌─────────────────────────────────────────┐
                    │           HEARTS SYSTEM (v4.0)          │
                    │                                         │
  Vào bài học ────→ │  1. Check session dở (< 30 phút)?       │
                    │     → CÓ: Resume miễn phí               │
                    │       (quay lại đúng bước: Theory/       │
                    │        Sandbox/Quiz/Lab/Code)            │
                    │     → KHÔNG: Trừ 1 tim (atomic)          │
                    │     UPDATE count = count - 1              │
                    │     WHERE user_id = ? AND count > 0       │
                    │     + Tạo session (TTL 30 phút)          │
                    │                                         │
  Hồi tim ───────→ │  Server tính: (NOW - last_used_at) / X  │
                    │  X = 3600s (Free) | 1800s (Premium)     │
                    │  (Server-side — chống chỉnh đồng hồ)   │
                    │                                         │
  Xem ads ───────→ │  Server verify ad callback + check      │
                    │  ad_count < 5 trong 24h (sliding window)│
                    │  ❌ KHÔNG reset lúc 00:00                │
                    │  ✅ Reset khi NOW - first_ad_at > 24h    │
                    │                                         │
  FAIL bước ─────→ │  KHÔNG trừ tim (retry trong session)    │
                    │                                         │
  PASS Node ─────→ │  Đóng session. Retry sau = tim mới.     │
                    │                                         │
  Crash/mất mạng → │  Session giữ 30 phút. Vào lại miễn phí.│
                    │  Resume đúng bước đang dở.              │
                    └─────────────────────────────────────────┘
```

**Dùng chung cho cả System Roadmap và Classroom Roadmap.** Không tách riêng pool tim.

---

### 💎 Hệ thống Gems (Economy) (v4.0)

> [!IMPORTANT]
> **Mọi giao dịch Gems đều atomic:** `UPDATE gems = gems - X WHERE user_id = ? AND gems >= X` — chống double-spend multi-tab.

**Cách kiếm Gems:**

| Nguồn | Gems |
|---|---|
| Pass Node lần đầu | +10 💎 |
| Pass Node 3⭐ | +5 💎 bonus |
| Nâng sao (1→2⭐) | +3 💎 bonus (1 lần) |
| Nâng sao (2→3⭐) | +5 💎 bonus (1 lần) |
| Hoàn thành Daily Quest | +2-5 💎 / quest |
| Bonus 5/5 quests | +10 💎 |
| Đạt Achievement Milestone | +10-50 💎 |

**Cách tiêu Gems (Gems Shop v4.0 — mở rộng):**

| Vật phẩm | Giá | Loại | Stack/Mua lại |
|---|---|---|---|
| 💡 AI Hint Token (1 lần dùng) | 30 💎 | Consumable | Max tích trữ 10. Mua nếu < 10 |
| 🧊 Streak Freeze (1 lượt) | 100 💎 | Consumable | Max tích trữ 2 lượt. Mua nếu < 2 |
| 🖼️ Avatar Mascot Độc Quyền | 200 💎 | Permanent | Sở hữu vĩnh viễn. Không mua lại |
| 👑 Khung Viền Neon | 300 💎 | Permanent | Sở hữu vĩnh viễn. Chỉ equip 1 |
| 👑 Khung Viền Vàng | 500 💎 | Permanent | Sở hữu vĩnh viễn. Chỉ equip 1 |
| 👑 Khung Viền Kim Cương | 1000 💎 | Permanent | Sở hữu vĩnh viễn. Chỉ equip 1 |
| 🎨 Theme màu giao diện | 150 💎 | Permanent | Sở hữu vĩnh viễn. Không mua lại |
| ⚡ XP Boost 2x (24 giờ) | 300 💎 | Timed | Mua thêm = cộng dồn thời gian |

---

### 📋 Nhiệm vụ Hàng ngày (Daily Quests)

Reset lúc 00:00 UTC+7 mỗi ngày. Mỗi ngày chọn **2 Easy + 2 Medium + 1 Hard** từ pool:

**Easy Pool (chọn 2):**
| Nhiệm vụ | Phần thưởng |
|---|---|
| Giữ Streak (đăng nhập + hoạt động) | +5 💎 + 5 ⭐XP |
| Xem DSA CheatSheet 1 lần | +1 💎 + 5 ⭐XP |

**Medium Pool (chọn 2):**
| Nhiệm vụ | Phần thưởng |
|---|---|
| Xem xong 1 Visualizer | +1 ❤️ + 10 ⭐XP |
| Pass 1 Quiz ≥80% | +2 💎 + 10 ⭐XP |

**Hard Pool (chọn 1):**
| Nhiệm vụ | Phần thưởng |
|---|---|
| Hoàn thành 1 Node bất kỳ | +1 ❤️ + 15 ⭐XP |
| Submit code thành công 1 lần | +3 💎 + 15 ⭐XP |

**Bonus hoàn thành tất cả 5/5:** +1 ❤️ + 10 💎 + 25 ⭐XP.

---

### 🔥 Streak & Streak Freeze

- **Streak** = Số ngày liên tục có ≥1 activity (pass quiz / submit code / xem visualizer xong).
- Chỉ **login không đủ** — phải có hoạt động học tập thực tế.
- **Milestone rewards:** 7 ngày (+10💎), 30 ngày (+50💎 + Badge), 100 ngày (+200💎 + Badge Vàng).
- **Streak Freeze:** Tối đa giữ 2 lượt. Mua từ Shop (100💎) hoặc nhận 1 free/tuần.

**Auto-activation (Background Job):**
```
Chạy lúc 00:30 UTC+7 mỗi ngày:
1. SELECT users WHERE LastActivityDate < hôm qua AND StreakDays > 0
2. Nếu có Streak Freeze > 0 → trừ 1 freeze, giữ streak
   → Push notification: "Streak Freeze đã cứu chuỗi X ngày! Còn Y lượt."
3. Nếu không có Freeze → reset streak = 0
   → Push notification: "Chuỗi ngày học đã bị reset. Hãy bắt đầu lại! 💪"
```

---

### ⭐ XP & Level

**Công thức Level:**
$$\text{Level} = 1 + \left\lfloor\sqrt{\frac{\text{TotalXP}}{100}}\right\rfloor$$

**Nguồn XP:**

| Hoạt động | XP |
|---|---|
| Pass Node lần đầu | +30-50 XP (tùy độ khó) |
| Hoàn thành Visualizer | +10 XP |
| Pass Quiz | +15-25 XP |
| Pass Lab | +15-25 XP |
| Submit code thành công | +20-40 XP |
| Daily Quest | +5-15 XP / quest |
| Nâng sao Node (1→2⭐) | +10 XP (1 lần) |
| Nâng sao Node (2→3⭐) | +15 XP (1 lần) |

> [!IMPORTANT]
> **Anti-grinding:** XP trao **1 lần** cho lần pass đầu tiên mỗi Node/bước. Retry chỉ nhận bonus XP khi **nâng được sao** (1→2 hoặc 2→3), không nhận nếu giữ nguyên sao.

---
---

## PHẦN 4 — SƠ ĐỒ CÂY (TREE DIAGRAM) v4.0

```
🏠 Home (Landing Page — Chưa đăng nhập)
│   ├── Trải nghiệm thử 3 Visualizer demo (Bubble Sort, BinSearch, BFS)
│   ├── Đăng nhập → Load role → Dashboard
│   └── Đăng ký (role = Student Free) → Xác nhận Email → Onboarding (có Skip) → Dashboard
│
│   [Quên mật khẩu] → Nhập email (max 3 lần/email/giờ) → Link reset (15 phút) → Mật khẩu mới → Login
│
└── 📊 Dashboard (Sau khi đăng nhập)
    │   ├── Thanh trạng thái: ❤️ Tim(X/10|30)  🔥 Streak  ⭐ XP  💎 Gems  [Free|Premium|Teacher]  🔔
    │   ├── Nút "▶ Tiếp tục học" → Node đang dở (skip Theory nếu đã xem)
    │   ├── Nhiệm vụ Hàng ngày (2 Easy + 2 Medium + 1 Hard) → ❤️/⭐/💎
    │   ├── Streak Check-in (cần ≥1 activity, không chỉ login)
    │   └── Quick Access: Lớp học đang tham gia (nếu có)
    │
    ├── 🗺️ Visual / Roadmap (v4.0)
    │   │
    │   │── Danh sách Roadmap
    │   │   ├── 🌐 System Roadmaps (Public mặc định)
    │   │   ├── 👩‍🏫 Teacher Roadmaps (Public, đã duyệt)
    │   │   └── 🏫 Classroom Roadmaps (chỉ thành viên lớp)
    │   │
    │   └── Chọn Roadmap → 🔤 Language Selector (C++/Java/Python/JS — lưu per-Roadmap)
    │       │
    │       │── [Array → Linked List → Stack/Queue → BinSearch → Tree → Graph → DP]
    │       │
    │       └── Bấm vào Node [🔒|⚡|✅(1-3⭐)]
    │           │
    │           ├── [🔒] → Tooltip "Hoàn thành Node X trước" → Chặn
    │           │
    │           └── [⚡|✅] → ❤️ KIỂM TRA SESSION + TIM (server atomic)
    │                 │
    │                 ├── Session dở (< 30 phút) → Resume MIỄN PHÍ (đúng bước đang dở)
    │                 │
    │                 ├── Hết Tim (0 ❤️) → Màn hình Hết Tim
    │                 │     ├── ⏱ Chờ tự hồi (Free: 1h | Premium: 30p)
    │                 │     ├── 📺 Xem ads → +2 tim (max 5/24h sliding window, server verify)
    │                 │     └── 📋 Làm quest → ❤️/💎
    │                 │
    │                 └── Còn Tim → Server trừ 1 (atomic) + tạo session (30p) → Vào bài
    │                       │
    │                       ├── [Bước 1] 📖 Rich Theory — Lý thuyết giàu trải nghiệm
    │                       │     ├── Văn bản + Hình ảnh + Video ngắn (1-3p)
    │                       │     ├── MathJax + Mermaid.js
    │                       │     ├── Visualizer: ▶ Play | ⏸ Pause | ⏭⏮ Step | 🎚 Speed
    │                       │     ├── Two-way sync (lý thuyết ↔ animation)
    │                       │     ├── Autosave mỗi 5s (+ offline queue)
    │                       │     └── Điều kiện tiếp: ≥90% frames (≥500ms/frame)
    │                       │         HOẶC Play tự động ≥60% tổng thời gian
    │                       │
    │                       ├── [Bước 1.5] ⚡ Code-to-Visual Live Sandbox (TÙY CHỌN)
    │                       │     ├── Gõ/dán code → Execution Trace → Animation tự động
    │                       │     ├── Mảng, con trỏ, biến memory trên Canvas
    │                       │     ├── Limits: 200 dòng · 10s timeout · 64MB memory
    │                       │     └── Có nút "Bỏ qua → Đến Practice"
    │                       │
    │                       ├── [Bước 2] 🎯 Progressive Practice Ladder (TUẦN TỰ BẮT BUỘC)
    │                       │     │
    │                       │     ├── [2a] 📝 Quiz → ≥60% đúng = pass → Mở Lab
    │                       │     │     └── ❌ Fail → Retry Quiz (không trừ tim)
    │                       │     │
    │                       │     ├── [2b] 🧪 Lab → hoàn thành đúng thao tác = pass → Mở LeetCode
    │                       │     │     └── [🔒 cần pass Quiz]
    │                       │     │     └── ❌ Fail → Retry Lab (không trừ tim)
    │                       │     │
    │                       │     └── [2c] 💻 LeetCode → ≥70% testcases = pass Node
    │                       │           ├── [🔒 cần pass Lab]
    │                       │           ├── Kết quả: CE | RE | TLE | MLE | WA | AC
    │                       │           ├── 🤖 AI: Hint1(free) | HintToken(gems) | Hint2+Debug+Optimize(💎30 req/ngày)
    │                       │           └── Cooldown 10s giữa 2 request
    │                       │
    │                       └── [Bước 3] ✅ PASS Node → Đóng session
    │                             ├── Score = Quiz(20%) + Lab(30%) + Code(50%)
    │                             ├── Node score = MAX(current, previous)
    │                             ├── ⭐≥60% ⭐⭐≥80% ⭐⭐⭐≥95%
    │                             ├── +XP +Gems + Bonus XP nâng sao
    │                             ├── 📈 Runtime · Percentile · Memory
    │                             ├── 🏅 BXH bài: Top 🥇🥈🥉
    │                             │     "Xem Approach" (pseudocode, không full code)
    │                             ├── [▶ Next] → Mở khóa Node tiếp
    │                             └── [🔁 Làm lại] → Cải thiện (trừ tim mới)
    │
    ├── 📰 Blog
    │     ├── Danh sách Cards (Tiêu đề · Tags · Read time · Tác giả)
    │     ├── Lọc chủ đề · Tìm kiếm
    │     └── Bấm vào → Đọc · Like ❤️ · Bookmark 🔖 · Comment (Report/Edit/Delete) · Related
    │
    ├── 📊 DSA CheatSheet
    │     ├── Chọn CTDL → Xem Big-O (Best/Avg/Worst) + Space + Code Snippet
    │     ├── Code snippet đồng bộ theo ngôn ngữ đã chọn ở Roadmap
    │     └── 💎 Premium: Download PDF + Advanced Snippets
    │
    ├── 🏆 Xếp hạng (Leaderboard)
    │     ├── Tab Tuần: Top XP (reset T2 00:00 UTC+7)
    │     ├── Tab Level: BXH cùng khung cấp
    │     └── Tab Lớp học (MỚI): BXH nội bộ từng lớp
    │           └── Chọn lớp → Xem BXH · Chỉ hiện nếu đã join ≥1 lớp
    │
    ├── 🤖 AI Assistant (Global)
    │     ├── Chat: Lý thuyết · Lộ trình · Code
    │     ├── Limit: Free=5msg/ngày · Premium=50msg/ngày
    │     └── Cooldown: 10s/request
    │
    ├── 👤 Hồ sơ (Profile)
    │     ├── Avatar (+ Khung VIP/Gems) · Tên · Level · XP · Streak 🔥 · Nodes passed
    │     ├── Badge vai trò: Student · Premium 👑 · Teacher 👩‍🏫
    │     ├── Huy hiệu (Badges)
    │     ├── Heatmap (activity: pass node + submit + quest)
    │     ├── Lịch sử: Nodes passed + Score + Time + Stars
    │     ├── 👩‍🏫 Đăng ký Giáo viên: Form → Gửi đơn → Admin duyệt
    │     │     ├── ⏳ Pending | ✅ Approved → Teacher Studio | ❌ Rejected (nộp lại sau 30 ngày)
    │     ├── Avatar: [Free] Kho hệ thống | [Premium] Upload+Khung VIP 👑 | [Gems] Mascot+Khung Neon/Vàng/KC
    │     └── ⚙️ Settings: Password · Email · Notifications · Language UI · Language Code · Delete Account
    │
    ├── 🏫 Lớp Học (Classroom) (MỚI)
    │     ├── [Student] Nhập Join Code (6 ký tự) → Vào lớp → Roadmap lớp + BXH nội bộ
    │     └── [Teacher] Tạo lớp → Sinh Join Code → Quản lý
    │           ├── Danh sách học sinh + Tiến độ
    │           ├── Teacher Analytics: Biểu đồ · Lịch sử · Cảnh báo inactive
    │           ├── Xuất báo cáo Excel
    │           └── Cài đặt: Đổi Roadmap · Tạo mã mới · Gỡ HS · Xóa lớp
    │
    ├── 💎 Cửa Hàng Gems (MỚI — tách riêng)
    │     ├── 💡 AI Hint Token: 30💎/token (max 10)
    │     ├── 🧊 Streak Freeze: 100💎/lượt (max 2)
    │     ├── 🖼️ Avatar Mascot: 200💎 (vĩnh viễn)
    │     ├── 👑 Khung Neon/Vàng/Kim Cương: 300/500/1000💎 (vĩnh viễn, equip 1)
    │     ├── 🎨 Theme: 150💎 (vĩnh viễn)
    │     └── ⚡ XP Boost 2x: 300💎 (24h, cộng dồn)
    │
    └── 🛠️ Teacher Studio (MỚI — chỉ Teacher/Admin)
          ├── Danh sách Roadmap đã tạo (Draft/Pending/Published/Rejected)
          └── Tạo Roadmap mới
                ├── Info: Tên · Mô tả · Tags · Thumbnail · Visibility
                ├── Thiết kế Node (lặp):
                │     ├── Lý thuyết (Rich Text + Ảnh + Video)
                │     ├── Visualizer mẫu (chọn từ thư viện)
                │     └── 3 dạng Practice (Quiz + Lab + LeetCode) — BẮT BUỘC ĐỦ
                ├── Sắp xếp Node (drag & drop)
                └── Xuất bản: Private | Classroom-only | Public (cần Admin duyệt)
```

---
---

## PHẦN 5 — BẢNG SO SÁNH FREE vs PREMIUM vs TEACHER

| Cơ chế | 🆓 Free | 💎 Premium | 👩‍🏫 Teacher |
|---|---|---|---|
| Tim tối đa | 10 ❤️ | 30 ❤️ | 30 ❤️ (có Premium) hoặc 10 ❤️ |
| Tự hồi tim | 1 giờ / 1 tim | 30 phút / 1 tim | Theo tier Premium/Free |
| Xem ads hồi tim | ✅ +2 tim/lần (max 5/24h) | ✅ +2 tim/lần (max 5/24h) | ✅ |
| Làm quest nhận tim | ✅ | ✅ | ✅ |
| AI Hint 1 trong bài | ✅ Miễn phí (mỗi bài/bước) | ✅ Miễn phí (mỗi bài/bước) | ✅ |
| AI Hint Token (Gems) | ✅ | ✅ | ✅ |
| AI Hint 2+ / Debug / Optimize | ❌ | ✅ 30 req/ngày + cooldown 10s | Theo tier Premium/Free |
| AI Global chat | 5 msg/ngày | 50 msg/ngày | Theo tier Premium/Free |
| Custom Avatar Upload | ❌ | ✅ | ✅ (Teacher perk) |
| Khung Avatar VIP 👑 (Premium) | ❌ | ✅ Viền vàng | Theo tier Premium/Free |
| Khung Avatar (Gems Shop) | ✅ Mua bằng Gems | ✅ Mua bằng Gems | ✅ Mua bằng Gems |
| CheatSheet PDF Download | ❌ | ✅ | Theo tier Premium/Free |
| Advanced Code Snippets | ❌ | ✅ | Theo tier Premium/Free |
| Session resume (crash) | ✅ 30 phút | ✅ 30 phút | ✅ 30 phút |
| Xem Approach Top 1 | ✅ (sau khi tự pass) | ✅ (sau khi tự pass) | ✅ (sau khi tự pass) |
| Gems Shop | ✅ | ✅ | ✅ |
| Language Selector | ✅ per-Roadmap | ✅ per-Roadmap | ✅ per-Roadmap |
| Tạo Roadmap Custom | ❌ | ❌ | ✅ |
| Tạo Lớp học | ❌ | ❌ | ✅ |
| Teacher Analytics | ❌ | ❌ | ✅ |
| Teacher Studio | ❌ | ❌ | ✅ |

> [!NOTE]
> **Teacher role** là quyền hạn bổ sung, **không bao gồm Premium**. Teacher muốn có Premium features (30 ❤️, AI 50 msg/ngày, etc.) phải mua Premium riêng. Tuy nhiên Teacher được free: upload avatar cá nhân.

---
---

## PHẦN 6 — EDGE CASES & QUY TẮC ĐẶC BIỆT (v4.0)

| Tình huống | Xử lý |
|---|---|
| **--- Hearts & Session ---** | |
| User mở 10 tab cùng trừ tim | Server atomic `UPDATE WHERE count > 0` — chỉ 1 tab thành công |
| User đổi clock để hồi tim | Server tính hồi dựa trên DB timestamp, không dùng client time |
| User fake ad callback | Server-side ad verification (AdMob SSV) |
| User spam xem ads quanh nửa đêm | **Sliding window 24h** — không reset lúc 00:00, tính từ lần xem đầu tiên |
| FAIL bước (Quiz/Lab/Code) | Không trừ tim — retry trong session |
| **Crash / mất mạng giữa bài** | **Session giữ 30 phút → vào lại miễn phí (resume đúng bước đang dở)** |
| **Session hết hạn (> 30 phút)** | **Phải trừ tim mới. Tiến độ các bước đã pass ĐƯỢC GIỮ (VD: đã pass Quiz → vào lại bắt đầu từ Lab)** |
| User login 2 thiết bị | Tim sync real-time qua API (mỗi request đều check DB) |
| **User retry sau PASS** | **Session đã đóng → trừ tim mới** |
| Classroom Roadmap dùng tim | **Dùng chung pool tim** — không tách riêng |
| **--- Practice Ladder ---** | |
| User pass Quiz nhưng fail Lab, session timeout | Quay lại → trừ tim mới → bắt đầu từ Lab (Quiz đã pass lưu vĩnh viễn) |
| User pass Quiz + Lab nhưng chưa code, crash | Resume trong 30 phút → thẳng vào LeetCode |
| User muốn làm lại Quiz đã pass (để cải thiện) | Phải retry toàn bộ Node (trừ tim mới) — retry từ Quiz |
| **--- Language Selector ---** | |
| User đổi ngôn ngữ giữa chừng Roadmap | Cho phép đổi — code cũ giữ nguyên, bài mới dùng ngôn ngữ mới |
| CheatSheet ngôn ngữ nào? | Dùng ngôn ngữ của Roadmap gần nhất, hoặc default trong Settings |
| **--- Classroom ---** | |
| Teacher xóa lớp khi HS đang học | Cảnh báo 2 bước + giữ tiến độ HS (Roadmap vẫn truy cập nếu Public) |
| HS tham gia >5 lớp | Cho phép — không giới hạn số lớp join |
| Join Code bị brute force | Rate limit: max 10 lần nhập sai / IP / giờ |
| Teacher assign Classroom Roadmap, HS vừa có System vừa có Classroom | Hiện riêng biệt trong danh sách Roadmap, không conflict |
| **--- Teacher & Roadmap Builder ---** | |
| Teacher tạo Node thiếu 1 dạng Practice | **Chặn xuất bản** — bắt buộc đủ Quiz + Lab + LeetCode |
| Teacher nộp Roadmap Public, Admin reject | Teacher nhận notification + lý do → sửa & nộp lại (không giới hạn lần nộp) |
| Admin revoke Teacher role | Roadmap Published giữ nguyên · Classroom đóng (HS giữ tiến độ) · Mất quyền Teacher Studio |
| Teacher tạo quiz có 0 câu hỏi | **Chặn** — minimum 5 câu/quiz |
| **--- Gems Shop ---** | |
| Mua AI Hint Token khi đã max (10) | Chặn mua: "Bạn đã có tối đa 10 AI Hint Tokens" |
| Mua Khung VIP khi đã có khung khác | Cho mua (sưu tập) — nhưng chỉ equip 1 khung tại 1 thời điểm |
| Mua Gems Shop khi đã max stack | Chặn mua: "Bạn đã có tối đa X [vật phẩm]" |
| Multi-tab mua Gems Shop | **Atomic:** `UPDATE gems = gems - X WHERE gems >= X` |
| **--- Teacher Application ---** | |
| Student nộp đơn Teacher, bị reject, nộp lại | Cooldown **30 ngày** sau reject mới được nộp lại |
| Student nộp đơn khi đã có đơn Pending | Chặn: "Đơn trước đang chờ duyệt" |
| Admin approve nhầm → muốn thu hồi | Admin Panel → Đổi role Teacher → Student (Roadmap Published giữ, Classroom đóng) |
| **--- Code-to-Visual Sandbox ---** | |
| Code vô hạn loop (infinite loop) | Timeout 10 giây → hiện thông báo "Code chạy quá lâu" |
| Code chứa malicious code | Sandbox isolate: no network, readonly filesystem, 64MB memory |
| Sandbox trong session timeout (30 phút) | Session tính chung — nếu timeout phải trừ tim mới |
| **--- Khác (giữ từ v3.0) ---** | |
| User bấm Play rồi Pause ngay 0.1s | **KHÔNG bypass** — cần Play ≥60% tổng thời gian animation |
| User spam reset password email | **Max 3 lần / email / giờ** |
| User spam AI requests | **Cooldown 10 giây** giữa 2 request + daily cap |
| Streak Freeze khi user offline | **Background Job 00:30 UTC+7** tự kích hoạt freeze + push notification |
| Score Node khi retry | **MAX(score hiện tại, score cũ)** — weighted average Quiz(20%)+Lab(30%)+Code(50%) |
| Compile Error khi submit code | **Không tính FAIL** — hiện error, cho sửa & submit lại |
| Premium hết hạn | Tim giảm max về 10, hồi chậm 1h, AI bị giới hạn, ẩn khung Premium VIP, giữ avatar + gems + purchases + Gems Shop items |
| User đăng nhập nhưng không học | Streak KHÔNG tính — cần ≥1 activity thực tế |
| User bị lock 5 lần login sai | Lock 15 phút → tự mở lại |
| User muốn xem code Top 1 | Hiện **Approach (pseudocode)**, không full code — chống plagiarism |

---
---

## PHẦN 7 — NOTIFICATION TRIGGERS (v4.0)

| Trigger | Nội dung | Kênh |
|---|---|---|
| **--- Streak & Daily ---** | | |
| Streak sắp mất (20:00 tối UTC+7) | "Bạn chưa học hôm nay! Streak 🔥X ngày sắp mất!" | Push + In-app |
| Streak Freeze kích hoạt | "Streak Freeze đã cứu chuỗi X ngày! Còn Y lượt." | Push + In-app |
| Streak bị reset | "Chuỗi ngày học đã bị reset. Hãy bắt đầu lại! 💪" | Push + In-app |
| Daily Quest reset | "5 nhiệm vụ mới đang chờ bạn!" | In-app |
| **--- Premium ---** | | |
| Premium sắp hết hạn (còn 3 ngày) | "Premium còn 3 ngày. Gia hạn để không mất quyền lợi!" | Push + In-app |
| Tim đã hồi đầy | "Tim đã hồi đầy! Sẵn sàng học tiếp 💪" | Push |
| **--- Teacher Application (MỚI) ---** | | |
| Đơn Teacher được duyệt | "Chúc mừng! Bạn đã trở thành Giáo viên 🎉 Truy cập Teacher Studio ngay!" | Push + In-app |
| Đơn Teacher bị từ chối | "Đơn đăng ký Giáo viên bị từ chối: [Lý do]. Có thể nộp lại sau 30 ngày." | Push + In-app |
| Admin nhận đơn Teacher mới | "Có đơn đăng ký Giáo viên mới từ [Tên]" | In-app (Admin) |
| **--- Classroom (MỚI) ---** | | |
| HS tham gia lớp | "[Tên HS] đã tham gia lớp [Tên lớp]" | In-app (Teacher) |
| HS không hoạt động >7 ngày | "Cảnh báo: [Tên HS] không hoạt động trong lớp [Tên lớp] hơn 7 ngày" | In-app (Teacher) |
| Teacher gửi thông báo cho lớp | "[Teacher] gửi thông báo: [Nội dung]" | In-app (HS trong lớp) |
| **--- Roadmap Builder (MỚI) ---** | | |
| Roadmap Public được duyệt | "Roadmap [Tên] đã được phê duyệt và xuất bản! 🎉" | Push + In-app (Teacher) |
| Roadmap Public bị từ chối | "Roadmap [Tên] bị từ chối: [Lý do]. Vui lòng chỉnh sửa." | Push + In-app (Teacher) |
| Admin nhận Roadmap mới cần duyệt | "Có Roadmap mới cần duyệt từ [Teacher]" | In-app (Admin) |

---
---

## PHẦN 8 — BACKGROUND JOBS (v4.0)

| Job | Schedule | Mô tả |
|---|---|---|
| **--- Giữ từ v3.0 ---** | | |
| Streak Check & Freeze | 00:30 UTC+7 hàng ngày | Kiểm tra activity hôm qua, kích hoạt freeze hoặc reset streak |
| Daily Quest Reset | 00:00 UTC+7 hàng ngày | Random 2E+2M+1H quest mới cho mỗi user |
| Streak Reminder | 20:00 UTC+7 hàng ngày | Push notification cho user chưa có activity hôm nay |
| Premium Expiry Warning | 09:00 UTC+7 hàng ngày | Gửi notification cho user Premium còn ≤3 ngày |
| Premium Expiry Enforce | 00:05 UTC+7 hàng ngày | Downgrade user hết hạn: giảm max tim, ẩn khung VIP Premium |
| Session Cleanup | Mỗi 15 phút | Xóa learning sessions hết hạn (> 30 phút) |
| **--- Mới v4.0 ---** | | |
| Classroom Inactive Alert | 09:00 UTC+7 hàng ngày | Kiểm tra HS inactive >7 ngày trong Classroom → thông báo Teacher |
| Teacher Application Reminder | 09:00 UTC+7 hàng ngày | Nhắc Admin nếu có đơn Teacher Pending >3 ngày chưa xử lý |
| Classroom BXH Weekly | 00:00 UTC+7 thứ Hai | Tính BXH nội bộ lớp tuần, thông báo Top 3 |

---
---

## PHẦN 9 — TEACHER & ADMIN FLOW CHI TIẾT

### 👩‍🏫 Teacher Application Flow

```
┌───────────────────────────────────────────────────┐
│          TEACHER APPLICATION FLOW                  │
│                                                   │
│  Student → Profile → "Đăng ký làm Giáo viên"     │
│    │                                              │
│    ├── [Đã có đơn Pending] → Chặn: "Đơn đang chờ"│
│    ├── [Rejected < 30 ngày] → Chặn: "Thử lại sau │
│    │    X ngày"                                   │
│    │                                              │
│    └── [Cho phép nộp] → Điền form:                │
│          ├── Tên trường/tổ chức (bắt buộc)        │
│          ├── CV/Portfolio (link hoặc PDF ≤5MB)    │
│          └── Lý do đăng ký (≥50 ký tự)           │
│                                                   │
│    Gửi → Status: ⏳ Pending                       │
│    │                                              │
│    └── Admin Panel nhận thông báo                 │
│          ├── Xem chi tiết đơn                     │
│          ├── ✅ Approve                            │
│          │     ├── UPDATE user.role = 'teacher'    │
│          │     ├── Mở khóa Teacher Studio + Classroom│
│          │     └── Push notification → Student     │
│          │                                        │
│          └── ❌ Reject + Lý do                     │
│                ├── UPDATE application.status       │
│                ├── SET cooldown = NOW + 30 ngày    │
│                └── Push notification → Student     │
└───────────────────────────────────────────────────┘
```

### 🛡️ Admin Panel Flow

```
Admin Panel
  ├── 📋 Duyệt đơn Giáo viên
  │     ├── Badge đỏ: số đơn Pending
  │     ├── Danh sách: Tên · Email · Trường · Ngày nộp · Preview CV
  │     └── Actions: Approve / Reject (kèm lý do)
  │
  ├── 📋 Duyệt Roadmap Public
  │     ├── Badge đỏ: số Roadmap Pending
  │     ├── Preview: Xem toàn bộ Roadmap + Nodes + nội dung
  │     └── Actions: Approve / Reject (kèm lý do)
  │
  ├── 👥 Quản lý Users
  │     ├── Tìm kiếm / Filter: role · status · email · username
  │     ├── Actions: Đổi role · Ban/Unban · Xem activity log
  │     └── Bulk actions: Export danh sách · Gửi notification nhóm
  │
  ├── 📰 Quản lý Blog (CMS)
  │     └── Tạo / Sửa / Xóa / Ẩn bài viết
  │
  └── 📊 Dashboard Admin
        ├── Thống kê: DAU · MAU · Retention · Conversion Free→Premium
        ├── Phân bố user: Level · League · Role
        └── Top Roadmaps: lượt học · completion rate · rating
```

---
---

## PHẦN 10 — CLASSROOM SYSTEM CHI TIẾT

### Cấu trúc dữ liệu Classroom

```
Classroom {
  id: UUID
  name: String (3-100 ký tự)
  description: String (max 500 ký tự)
  teacher_id: FK → User (role=Teacher)
  roadmap_id: FK → Roadmap (System hoặc Custom Published)
  join_code: String (6 ký tự, unique, alphanumeric)
  created_at: Timestamp
  is_active: Boolean
}

ClassroomMember {
  classroom_id: FK → Classroom
  student_id: FK → User
  joined_at: Timestamp
  last_activity_at: Timestamp
}
```

### Quy tắc Join Code

| Quy tắc | Chi tiết |
|---|---|
| Format | 6 ký tự: A-Z + 0-9 (uppercase, không viết hoa/thường lẫn lộn) |
| Uniqueness | Unique toàn hệ thống |
| Expiry | Không hết hạn (trừ khi Teacher tạo mã mới → mã cũ vô hiệu) |
| Brute force protection | Max 10 lần nhập sai / IP / giờ |
| Tổng số mã có thể | 36^6 = ~2.18 tỷ mã |

### Teacher Analytics — Báo cáo chi tiết

```
Teacher Analytics cho Classroom [Tên lớp]
│
├── 📊 Tổng quan
│     ├── Số học sinh: X
│     ├── % hoàn thành Roadmap: Y%
│     ├── Score trung bình lớp: Z
│     └── Node khó nhất (fail rate cao nhất): [Tên Node]
│
├── 👤 Chi tiết từng học sinh
│     ├── Tên · Tiến độ (X/Y nodes) · Score TB · Last active
│     └── Expand → Từng Node: Quiz score · Lab score · Code score · Stars · Thời gian
│
├── 📝 Lịch sử làm bài
│     ├── Filter: Theo Node · Theo HS · Theo dạng (Quiz/Lab/Code)
│     └── Chi tiết: Thời gian submit · Score · Số lần retry · Ngôn ngữ code
│
└── 📥 Xuất Excel
      ├── Sheet "Tổng quan": Tên · Email · Tiến độ · Score TB · Stars · Last active
      ├── Sheet "Chi tiết Node": Từng HS × Từng Node → Quiz/Lab/Code score
      └── Sheet "Lịch sử Submit": Timestamp · HS · Node · Dạng · Score · Result
```

---
---

## PHẦN 11 — ROADMAP BUILDER SPECS

### Quyền tạo Roadmap

| Role | Tạo Roadmap | Visibility cho phép | Cần duyệt |
|---|---|---|---|
| Student | ❌ | — | — |
| Teacher | ✅ | Private · Classroom-only · Public | Public cần Admin duyệt |
| Admin | ✅ | Private · Classroom-only · Public | Không cần duyệt (auto-publish) |

### Cấu trúc 1 Node trong Roadmap Builder

```
Node {
  ├── Thông tin cơ bản
  │     ├── Tên Node (bắt buộc, 3-100 ký tự)
  │     ├── Mô tả ngắn (max 500 ký tự)
  │     └── Độ khó: Easy | Medium | Hard
  │
  ├── 📖 Lý thuyết (bắt buộc)
  │     ├── Nội dung Rich Text (Markdown + MathJax + Mermaid)
  │     ├── Hình ảnh minh họa (upload, ≤5MB/ảnh, max 10 ảnh/node)
  │     └── Video bài giảng (embed YouTube/Vimeo HOẶC upload ≤50MB, max 1 video/node)
  │
  ├── 🎬 Visualizer (bắt buộc)
  │     └── Chọn từ thư viện Visualizer hệ thống
  │         (Admin có thể tạo Visualizer mới)
  │
  └── 🎯 Practice (bắt buộc đủ 3 dạng)
        ├── 📝 Quiz
        │     ├── Minimum 5 câu hỏi
        │     ├── Mỗi câu: Đề + 4 đáp án + 1 đáp án đúng + Giải thích (optional)
        │     └── Pass threshold: 60% (không đổi)
        │
        ├── 🧪 Lab
        │     ├── Mô tả bài Lab
        │     ├── Danh sách thao tác đúng (trình tự)
        │     └── Server chấm: so sánh thao tác user vs đáp án
        │
        └── 💻 LeetCode
              ├── Đề bài (Markdown)
              ├── Constraints (Time/Memory limit)
              ├── Test cases (≥10, ≤20): Input + Expected Output
              ├── Solution mẫu (ẩn — chỉ Admin/Teacher xem)
              └── Hỗ trợ 4 ngôn ngữ: C++ / Java / Python / JS
```

### Trạng thái Roadmap

```
[Teacher tạo] → 📝 Draft
  ├── Sửa thoải mái
  ├── Preview (xem như student)
  │
  ├── Xuất bản Private → chỉ Teacher thấy (test nội bộ)
  ├── Xuất bản Classroom-only → gán cho lớp → HS lớp thấy ngay
  └── Xuất bản Public → ⏳ Pending Review
        │
        ├── Admin ✅ Approve → ✅ Published (hiện cho toàn hệ thống)
        └── Admin ❌ Reject → ❌ Rejected (kèm lý do)
              └── Teacher sửa → nộp lại → ⏳ Pending Review
```

### Giới hạn Roadmap Builder

| Thông số | Giá trị |
|---|---|
| Max Roadmap / Teacher | 20 roadmaps |
| Max Node / Roadmap | 30 nodes |
| Max câu Quiz / Node | 20 câu |
| Max ảnh / Node | 10 ảnh (≤5MB/ảnh) |
| Max video / Node | 1 video (embed hoặc ≤50MB upload) |
| Max testcases / LeetCode | 20 testcases |
| Min testcases / LeetCode | 10 testcases |
| Min câu Quiz / Node | 5 câu |

---
---

> **Changelog v3.0 → v4.0:**
> 1. ✅ Thêm **Roadmap Custom Builder** (Teacher Studio) — Module 9 + PHẦN 11
> 2. ✅ Thêm **Teacher Application Flow** — đăng ký tại Profile, Admin duyệt → PHẦN 9
> 3. ✅ Thêm **Classroom System** (Join Code, BXH lớp, Teacher Analytics) — Module 7 + PHẦN 10
> 4. ✅ Thêm **Code-to-Visual Live Sandbox** — Bước 1.5 trong bài học (tùy chọn, không bắt buộc)
> 5. ✅ Thêm **Language Selector** — chọn per-Roadmap, đồng bộ Code/CheatSheet/Sandbox
> 6. ✅ Đổi **Practice** từ "chọn 1/3 tự do" → **Progressive Practice Ladder** tuần tự (Quiz → Lab → LeetCode)
> 7. ✅ Mở rộng **Gems Shop** — thêm AI Hint Token, Avatar Mascot, Khung Viền Neon/Vàng/Kim Cương — tách Module 8
> 8. ✅ Nâng cấp **Lý thuyết** → **Rich Theory Layout** (ảnh, video, MathJax, Mermaid.js)
> 9. ✅ Củng cố **Learning Session** — resume đúng bước đang dở (Theory/Sandbox/Quiz/Lab/Code)
> 10. ✅ Thêm **Role System** 4 vai trò: Student Free · Student Premium · Teacher · Admin
> 11. ✅ Thêm **Admin Panel** flow chi tiết (duyệt Teacher, duyệt Roadmap, quản lý users)
> 12. ✅ Cập nhật **Score calculation** → weighted average: Quiz(20%) + Lab(30%) + Code(50%)
> 13. ✅ Cập nhật **Leaderboard** thêm Tab "Lớp học"
> 14. ✅ Cập nhật **Bảng so sánh** Free vs Premium vs Teacher
> 15. ✅ Thêm 15+ edge cases mới (Classroom, Teacher, Sandbox, Language, Practice Ladder)
> 16. ✅ Thêm 9 notification triggers mới (Teacher Application, Classroom, Roadmap Builder)
> 17. ✅ Thêm 3 background jobs mới (Classroom Inactive, Teacher App Reminder, Classroom BXH)
> 18. ✅ Thêm PHẦN 9 (Teacher & Admin Flow), PHẦN 10 (Classroom System), PHẦN 11 (Roadmap Builder Specs)
> 19. ✅ Session resume nay track đúng bước cụ thể (Theory/Sandbox/Quiz/Lab/Code) thay vì chỉ "skip Visualizer"
> 20. ✅ Dashboard mở rộng 6 → 9 modules + navigation phân quyền theo role

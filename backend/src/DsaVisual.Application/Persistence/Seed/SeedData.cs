namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Dữ liệu seed cho SQL Server (SDD §7.5) — xem README.md cùng thư mục.
/// Dữ liệu KHAI BÁO (class C# thuần, không phụ thuộc EF) — SeedRunner chạy qua AppDbContext
/// (idempotent — kiểm tra tồn tại trước khi chèn) và hash mật khẩu thật bằng PasswordHasher.
///
/// Nguồn nội dung bài học: source/VisualizationDSA3/plan/content-drafts/v2/lesson-XX/
/// (content.md → Lesson.ContentHtml; quiz.json → Exercise MCQ + Questions).
/// Nguồn frontend: frontend/src/data/{courses,lessons}.ts + shop_items.json.
/// </summary>
public static class SeedData
{
    /// <summary>TopicId theo SDD §7.5: 1=Sắp xếp & Tìm kiếm, 2=CTDL tuyến tính, 3=Cây, 4=Bảng băm, 5=Đồ thị.</summary>
    public sealed record SeedTopic(string Name, string Description, int SortOrder);

    public static readonly IReadOnlyList<SeedTopic> Topics =
    [
        new("Sắp xếp & Tìm kiếm", "Bubble/Selection/Insertion/Quick/Merge/Heap Sort; Linear/Binary Search.", 1),
        new("CTDL tuyến tính", "Mảng, Danh sách liên kết, Ngăn xếp (Stack), Hàng đợi (Queue).", 2),
        new("Cây", "BST, AVL, Heap — duyệt cây, cân bằng, ưu tiên.", 3),
        new("Bảng băm", "Hash Table, Set, hàm băm, va chạm và ứng dụng tra cứu O(1).", 4),
        new("Đồ thị", "Biểu diễn đồ thị, duyệt BFS/DFS, đường đi ngắn nhất.", 5),
    ];

    /// <summary>
    /// Lesson mô tả: SimulationKey tham chiếu shared/simulation-catalog.json;
    /// SourceLesson = thư mục bài nguồn trong content-drafts (null = nội dung tự soạn — bài AVL).
    /// </summary>
    public sealed record SeedLesson(
        string Title,
        string Description,
        int TopicId,
        int SortOrder,
        string? SourceLesson,
        IReadOnlyList<string> SimulationKeys);

    public static readonly IReadOnlyList<SeedLesson> Lessons =
    [
        new("Bubble Sort", "So sánh và đổi chỗ các cặp liền kề tới khi mảng được sắp xếp.", 1, 1, "lesson-09", ["sort.bubble"]),
        new("Binary Search", "Chia đôi không gian tìm kiếm trên mảng đã sắp xếp.", 1, 2, "lesson-10", ["search.binary"]),
        new("Stack", "Cấu trúc LIFO với push/pop/peek O(1) — undo, call stack, so khớp ngoặc.", 2, 1, "lesson-06", ["stack.push", "stack.pop", "stack.peek"]),
        new("Linked List", "Danh sách liên kết đơn — Node, chèn/xóa đầu O(1), duyệt và đảo ngược.", 2, 2, "lesson-05", ["list.insert", "list.traverse"]),
        new("BST", "Cây nhị phân tìm kiếm — chèn, tìm kiếm, xóa, duyệt inorder.", 3, 1, "lesson-17", ["tree.bst-insert", "tree.bst-inorder", "tree.bst-search"]),
        new("AVL", "BST tự cân bằng — hệ số cân bằng, xoay LL/RR/LR/RL, giữ O(log N).", 3, 2, null, ["tree.avl-insert"]),
        new("Hash Table", "Bảng băm — hàm băm, va chạm, chaining/open addressing, load factor.", 4, 1, "lesson-04", ["hash.insert", "hash.search"]),
        new("BFS", "Duyệt đồ thị theo tầng bằng hàng đợi — đường đi ngắn nhất không trọng số.", 5, 1, "lesson-20", ["graph.bfs"]),
    ];

    /// <summary>Người dùng seed — Email UNIQUE. Mật khẩu DEV (chỉ dùng local, xem README).</summary>
    public sealed record SeedUser(string Email, string DisplayName, int Role, bool IsPrimaryAdmin, string DevPassword);

    public static readonly IReadOnlyList<SeedUser> Users =
    [
        new("admin@system.local", "Quản trị viên", Role: 3, IsPrimaryAdmin: true, DevPassword: "Admin@123"), // ADMIN
        new("teacher@demo.local", "Giáo viên mẫu", Role: 1, IsPrimaryAdmin: false, DevPassword: "Teacher@123"), // TEACHER
        new("student@demo.local", "Sinh viên mẫu", Role: 0, IsPrimaryAdmin: false, DevPassword: "Student@123"), // STUDENT
    ];

    /// <summary>
    /// Sinh viên demo — Email UNIQUE @university.edu.vn; displayName tiếng Việt.
    /// Mật khẩu DEV (chỉ dùng local) — seed logic hash bằng PasswordHasher, KHÔNG lưu hash ở đây.
    /// Dùng cho dữ liệu hoạt động người dùng (SDD §7.5).
    /// </summary>
    public sealed record SeedStudent(string Email, string DisplayName, string DevPassword);

    public static readonly IReadOnlyList<SeedStudent> Students =
    [
        new("nguyenminhanh@university.edu.vn", "Nguyễn Minh Anh", DevPassword: "Student@123"),
        new("tranquocbao@university.edu.vn", "Trần Quốc Bảo", DevPassword: "Student@123"),
        new("lethikimngan@university.edu.vn", "Lê Thị Kim Ngân", DevPassword: "Student@123"),
        new("phamhoanglong@university.edu.vn", "Phạm Hoàng Long", DevPassword: "Student@123"),
        new("vuthanhtung@university.edu.vn", "Vũ Thanh Tùng", DevPassword: "Student@123"),
        new("nguyentrang@university.edu.vn", "Nguyễn Trang", DevPassword: "Student@123"),
        new("doanminhduc@university.edu.vn", "Đoàn Minh Đức", DevPassword: "Student@123"),
        new("huynhthuy@university.edu.vn", "Huỳnh Thúy", DevPassword: "Student@123"),
    ];

    /// <summary>Quest template (SDD §7.5 — 8 quest) — ConditionJson/RewardJson theo SDD §7.3.26 + GamificationService.</summary>
    public sealed record SeedQuest(string QuestKey, string Title, int Type, string ConditionJson, string RewardJson);

    public static readonly IReadOnlyList<SeedQuest> Quests =
    [
        new("learn-1-node", "Học 1 node trên lộ trình", 0, """{"activity":"pass_node","count":1}""", """{"gems":3,"xp":20}"""),
        new("learn-3-node", "Vượt 3 node lộ trình", 1, """{"activity":"pass_node","count":3}""", """{"gems":5,"xp":30}"""),
        new("pass-1-quiz", "Hoàn thành 1 bài quiz", 0, """{"activity":"pass_quiz","count":1}""", """{"gems":3,"xp":20}"""),
        new("pass-1-lab", "Hoàn thành 1 lab mô phỏng", 0, """{"activity":"pass_lab","count":1}""", """{"gems":4,"xp":25}"""),
        new("code-run-1", "Chạy code 1 lần", 1, """{"activity":"code_run","count":1}""", """{"gems":4,"xp":25}"""),
        new("code-run-5", "Chạy code 5 lần", 2, """{"activity":"code_run","count":5}""", """{"gems":8,"xp":50}"""),
        new("lesson-viewed-2", "Xem 2 bài học", 0, """{"activity":"lesson_viewed","count":2}""", """{"gems":3,"xp":20}"""),
        new("streak-3", "Giữ chuỗi 3 ngày", 2, """{"activity":"streak","days":3}""", """{"gems":10,"xp":60}"""),
    ];

    /// <summary>
    /// Thành tích định nghĩa (SDD §7.3.19, FR-5.5) — Code UNIQUE. ConditionJson theo SDD:
    /// {"type":"count","key":...,"min":N} / {"type":"streak","days":N} / {"type":"score","exercisePct":N,"count":N}.
    /// IconUrl = null (chưa có icon riêng).
    /// </summary>
    public sealed record SeedAchievement(string Code, string Name, string Description, string? IconUrl, string ConditionJson, int SortOrder);

    public static readonly IReadOnlyList<SeedAchievement> Achievements =
    [
        new("first-lesson", "Bài học đầu tiên", "Hoàn thành bài học đầu tiên trên lộ trình.", IconUrl: null, """{"type":"count","key":"lessons","min":1}""", 1),
        new("first-simulation", "Khám phá mô phỏng", "Chạy mô phỏng thuật toán lần đầu tiên.", IconUrl: null, """{"type":"count","key":"simulations","min":1}""", 2),
        new("streak-7", "Chuỗi 7 ngày", "Giữ chuỗi học tập 7 ngày liên tục.", IconUrl: null, """{"type":"streak","days":7}""", 3),
        new("quiz-50", "Cỗ máy trắc nghiệm", "Hoàn thành 50 bài quiz.", IconUrl: null, """{"type":"count","key":"quizzes","min":50}""", 4),
        new("lab-10", "Tay mô phỏng", "Hoàn thành 10 lab mô phỏng.", IconUrl: null, """{"type":"count","key":"labs","min":10}""", 5),
        new("code-100", "Chiến binh code", "Chạy code 100 lần trong thử thách lập trình.", IconUrl: null, """{"type":"count","key":"code-runs","min":100}""", 6),
        new("path-1", "Vượt lộ trình", "Hoàn thành trọn vẹn 1 lộ trình học.", IconUrl: null, """{"type":"count","key":"paths","min":1}""", 7),
        new("sort-master", "Bậc thầy sắp xếp", "Đạt từ 80% trở lên ở 8 bài tập chủ đề sắp xếp.", IconUrl: null, """{"type":"score","exercisePct":80,"count":8}""", 8),
        new("perfect-score", "Điểm tuyệt đối", "Đạt 100% điểm ở một bài tập bất kỳ.", IconUrl: null, """{"type":"score","exercisePct":100,"count":1}""", 9),
        new("streak-30", "Kỷ luật bền bỉ", "Giữ chuỗi học tập 30 ngày liên tục.", IconUrl: null, """{"type":"streak","days":30}""", 10),
    ];

    /// <summary>Vật phẩm cửa hàng gems (SDD §7.3.28, Màn 22) — map từ frontend/src/data/shop_items.json.</summary>
    public sealed record SeedShopItem(string ItemKey, string Name, int PriceGems, int Type, int MaxStack = 1, int? DurationHours = null);

    public static readonly IReadOnlyList<SeedShopItem> ShopItems =
    [
        new("avatar-cyber-hacker", "Cyber Hacker", 100, Type: 1),
        new("avatar-gold-knight", "Golden Knight", 200, Type: 1),
        new("avatar-neon-ninja", "Neon Ninja", 150, Type: 1),
        new("avatar-wizard", "Code Wizard", 250, Type: 1),
        new("avatar-ai-bot", "AI Companion", 50, Type: 1),
        new("frame-neon", "Neon Border", 300, Type: 2),
        new("frame-gold", "Royal Gold", 500, Type: 2),
        new("frame-cyber", "Cyberpunk Frame", 400, Type: 2),
    ];

    /// <summary>
    /// Phản hồi nội dung (SDD §7.3.21, FR-7.4) — Rating 1-5, Comment tiếng Việt ≤ 200 ký tự.
    /// Seed logic tự gán UserId/LessonId khi chèn (UNIQUE (UserId, LessonId)).
    /// </summary>
    public sealed record SeedFeedback(int Rating, string Comment);

    public static readonly IReadOnlyList<SeedFeedback> Feedbacks =
    [
        new(5, "Bài học trực quan, mô phỏng từng bước rất dễ hiểu."),
        new(4, "Nội dung hay nhưng phần bài tập nâng cao hơi khó."),
        new(5, "Mô phỏng AVL giúp em hiểu xoay cây rõ ràng hơn hẳn."),
        new(3, "Giải thích tốt, mong bổ sung thêm ví dụ thực tế."),
        new(5, "Quiz có giải thích sau khi nộp rất hữu ích."),
        new(4, "Giao diện mượt, mong thêm nhiều bài tập code hơn."),
        new(5, "Rất thích phần mô phỏng BFS và đường đi ngắn nhất."),
        new(2, "Một số chỗ trình bày chưa rõ, cần minh họa thêm."),
        new(5, "Giáo trình chuẩn, phù hợp với sinh viên năm hai."),
        new(4, "Lab thú vị, làm xong hiểu sâu thuật toán hơn."),
    ];

    /// <summary>
    /// Lớp học (SDD §7.3.16, Module H) — InviteCode 6 ký tự A-Z0-9 UNIQUE; Status: 0=Mở (Open), 1=Đóng (Closed).
    /// </summary>
    public sealed record SeedClassProfile(string ClassName, string Semester, int Status, string InviteCode, string Description);

    public static readonly IReadOnlyList<SeedClassProfile> ClassProfiles =
    [
        new("SD21361 — Cấu trúc dữ liệu HK1 2026", "HK1-2026", Status: 0, "DSA213", "Lớp chính khóa môn Cấu trúc dữ liệu và Giải thuật — học kỳ 1 năm 2026."),
        new("SD21361NC — Giải thuật nâng cao HK1 2026", "HK1-2026", Status: 1, "ADVNCE", "Lớp nâng cao chuyên sâu thuật toán cho sinh viên khá giỏi — đã đóng tuyển."),
    ];

    /// <summary>Settings theo SDD §7.5 — Key UNIQUE.</summary>
    public sealed record SeedSetting(string Key, string Value, string Description);

    public static readonly IReadOnlyList<SeedSetting> Settings =
    [
        new("site.name", "DSA Visual", "Tên website"),
        new("password.policy.minLength", "8", "Độ dài tối thiểu mật khẩu"),
        new("upload.maxSizeMb", "5", "Kích thước upload tối đa (MB)"),
        new("simulation.maxArraySize", "100", "Kích thước mảng tối đa khi mô phỏng"),
        new("simulation.maxGraphVertices", "50", "Số đỉnh đồ thị tối đa khi mô phỏng"),
        new("auth.maxLoginAttempts", "5", "Số lần đăng nhập sai tối đa"),
        new("auth.lockoutMinutes", "15", "Thời gian khóa tài khoản (phút)"),
        new("simulation.defaultSpeed", "1", "Tốc độ mô phỏng mặc định"),
    ];
}

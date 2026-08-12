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

    /// <summary>Vật phẩm cửa hàng gems (SDD §7.3.28, Màn 22) — map từ frontend/src/data/shop_items.json.</summary>
    public sealed record SeedShopItem(string ItemKey, string Name, int PriceGems, int Type, int MaxStack = 1, int? DurationHours = null);

    public static readonly IReadOnlyList<SeedShopItem> ShopItems =
    [
        new("avatar-cyber-hacker", "Cyber Hacker", 100, Type: 0),
        new("avatar-gold-knight", "Golden Knight", 200, Type: 0),
        new("avatar-neon-ninja", "Neon Ninja", 150, Type: 0),
        new("avatar-wizard", "Code Wizard", 250, Type: 0),
        new("avatar-ai-bot", "AI Companion", 50, Type: 0),
        new("frame-neon", "Neon Border", 300, Type: 1),
        new("frame-gold", "Royal Gold", 500, Type: 1),
        new("frame-cyber", "Cyberpunk Frame", 400, Type: 1),
    ];

    /// <summary>Settings theo SDD §7.5 — Key UNIQUE.</summary>
    public sealed record SeedSetting(string Key, string Value, string Description);

    public static readonly IReadOnlyList<SeedSetting> Settings =
    [
        new("site.name", "DSA Visual", "Tên website"),
        new("allowed.email.domains", "university.edu.vn", "Danh sách domain email được phép (phân tách phẩy)"),
        new("password.policy.minLength", "8", "Độ dài tối thiểu mật khẩu"),
        new("upload.maxSizeMb", "5", "Kích thước upload tối đa (MB)"),
        new("simulation.maxArraySize", "100", "Kích thước mảng tối đa khi mô phỏng"),
        new("simulation.maxGraphVertices", "50", "Số đỉnh đồ thị tối đa khi mô phỏng"),
        new("auth.maxLoginAttempts", "5", "Số lần đăng nhập sai tối đa"),
        new("auth.lockoutMinutes", "15", "Thời gian khóa tài khoản (phút)"),
        new("simulation.defaultSpeed", "1", "Tốc độ mô phỏng mặc định"),
    ];
}

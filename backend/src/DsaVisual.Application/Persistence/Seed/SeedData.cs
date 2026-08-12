namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Dữ liệu seed TỐI THIỂU cho SQL Server (SDD §7.5) — xem README.md cùng thư mục.
///
/// ĐÂY CHỈ LÀ MÔ TẢ CẤU TRÚC + DỮ LIỆU KHỞI ĐẦU (class C# thuần, không phụ thuộc EF).
/// TODO task sau: chạy qua AppDbContext (idempotent — kiểm tra tồn tại trước khi chèn),
/// hash mật khẩu thật bằng PasswordHasher, seed đủ 5 Topics + 8 Lessons
/// (Bubble Sort, Binary Search, Stack, Linked List, BST, AVL, Hash Table, BFS) +
/// Exercises/Questions (từ source/VisualizationDSA3/plan/content-drafts/v2/) +
/// LearningPaths/DailyQuests (GĐ2).
///
/// Nguồn dữ liệu frontend: frontend/src/data/{courses,lessons}.ts + shop_items.json.
/// KHÔNG dùng source/VisualizationDSA1/backend/seed-demo-course.sql (schema v1 cũ — README).
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
        // TODO: "Bảng băm" (4) + "Đồ thị" (5) — seed đủ ở task sau (SDD §7.5 yêu cầu 5 chủ đề).
    ];

    /// <summary>Lesson mô tả: SimulationKey tham chiếu shared/simulation-catalog.json.</summary>
    public sealed record SeedLesson(
        string Title,
        string Description,
        int TopicId,
        int SortOrder,
        IReadOnlyList<string> SimulationKeys);

    public static readonly IReadOnlyList<SeedLesson> Lessons =
    [
        new("Bubble Sort - Sắp xếp nổi bọt", "So sánh và đổi chỗ các cặp liền kề tới khi mảng được sắp xếp.", 1, 1, ["sort.bubble"]),
        new("Binary Search - Tìm kiếm nhị phân", "Chia đôi không gian tìm kiếm trên mảng đã sắp xếp.", 1, 2, ["search.binary"]),
        new("Quick Sort - Sắp xếp nhanh", "Chia để Trị với phân hoạch Lomuto.", 1, 3, ["sort.quick"]),
        // TODO: Stack, Linked List, BST, AVL, Hash Table, BFS (đủ 8 bài SDD §7.5) —
        //       lấy title/description từ frontend/src/data/lessons.ts (stub đã có sẵn).
    ];

    /// <summary>Người dùng seed — Email UNIQUE; mật khẩu hash THẬT ở task sau (PasswordHasher).</summary>
    public sealed record SeedUser(string Email, string DisplayName, int Role, bool IsPrimaryAdmin, bool MustChangePassword);

    public static readonly IReadOnlyList<SeedUser> Users =
    [
        new("admin@system.local", "Quản trị viên", Role: 3, IsPrimaryAdmin: true, MustChangePassword: true), // ADMIN
        new("teacher@demo.local", "Giáo viên mẫu", Role: 1, IsPrimaryAdmin: false, MustChangePassword: false), // TEACHER
        new("student@demo.local", "Sinh viên mẫu", Role: 0, IsPrimaryAdmin: false, MustChangePassword: false), // STUDENT
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
        // TODO: frame-fire / frame-ice (V1 có 10 item — chọn 8 theo SDD §7.5, loại Type cần chốt enum).
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

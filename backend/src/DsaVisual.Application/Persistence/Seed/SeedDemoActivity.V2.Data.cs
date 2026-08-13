namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 — dữ liệu user dùng chung cho Task 1-6 (PROMPT_K_SEED_PROD_V2):
/// 69 user mới = 68 student @university.edu.vn theo 4 persona (Hardworking 13 / Average 32 /
/// Slacker 13 / New 10) + 1 showcase@demo.local. Nguồn CHÍNH THỨC cho SeedDemoActivity.V2.*
/// (tránh lệch danh sách giữa các file) — bảng đầy đủ tại docs/work/seed-v2/users-v2.md.
///
/// Ràng buộc (đã kiểm chéo): mỗi Email unique; KHÔNG trùng 26 email hiện có trong DB
/// (admin/teacher/student demo + 8 student V1 + rác smoke) — danh sách 26 email cũ xem decision log;
/// KHÔNG dùng lại 8 tên V1 (doanminhduc, huynhthuy, lethikimngan, nguyenminhanh, nguyentrang,
/// phamhoanglong, tranquocbao, vuthanhtung). Index deterministic 0..68: student 0-67 theo thứ tự
/// nhóm, showcase = 68 — dùng cho CreatedAt/hoạt động rải 30 ngày (chạy lại vẫn ổn định).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>Một user seed V2: Email (lowercase, unique), FullName tiếng Việt có dấu (hiển thị), Persona, Index deterministic.</summary>
    public sealed record V2UserSeed(string Email, string FullName, string Persona, int Index);

    /// <summary>
    /// 69 user V2 theo thứ tự Index 0..68. Email = tên tiếng Việt viết liền không dấu @university.edu.vn
    /// (pattern V1), FullName = tên đầy đủ có dấu; mật khẩu chung <see cref="V2Users.StudentPassword"/>.
    /// </summary>
    public static class V2Users
    {
        /// <summary>Email showcase account (Index 68 — "câu chuyện" 30 ngày hoạt động, premium 12 tháng).</summary>
        public const string ShowcaseEmail = "showcase@demo.local";

        /// <summary>Mật khẩu dev chung cho toàn bộ user V2 (chỉ dùng local — pattern V1).</summary>
        public const string StudentPassword = "Student@123";

        public const string HardworkingPersona = "Hardworking";
        public const string AveragePersona = "Average";
        public const string SlackerPersona = "Slacker";
        public const string NewPersona = "New";
        public const string ShowcasePersona = "Showcase";

        /// <summary>Showcase account: Index 68, CreatedAt = 30 ngày trước (streak 30 ngày hợp lý).</summary>
        public static readonly V2UserSeed Showcase = new(ShowcaseEmail, "Sinh viên tiêu biểu", ShowcasePersona, 68);

        /// <summary>Persona Hardworking — 13 user: chăm chỉ, XP 800-1500 (Level 3-4), streak 12-25.</summary>
        public static readonly IReadOnlyList<V2UserSeed> Hardworking =
        [
            new("nguyenthanhhai@university.edu.vn", "Nguyễn Thanh Hải", HardworkingPersona, 0),
            new("tranthuylinh@university.edu.vn", "Trần Thùy Linh", HardworkingPersona, 1),
            new("lequangvinh@university.edu.vn", "Lê Quang Vinh", HardworkingPersona, 2),
            new("phamthuha@university.edu.vn", "Phạm Thu Hà", HardworkingPersona, 3),
            new("hoangminhtri@university.edu.vn", "Hoàng Minh Trí", HardworkingPersona, 4),
            new("phanthanhson@university.edu.vn", "Phan Thanh Sơn", HardworkingPersona, 5),
            new("vuongthihuong@university.edu.vn", "Vương Thị Hương", HardworkingPersona, 6),
            new("dangquockhoa@university.edu.vn", "Đặng Quốc Khoa", HardworkingPersona, 7),
            new("buithanhthao@university.edu.vn", "Bùi Thanh Thảo", HardworkingPersona, 8),
            new("duongquanghuy@university.edu.vn", "Dương Quang Huy", HardworkingPersona, 9),
            new("lythuylinh@university.edu.vn", "Lý Thùy Linh", HardworkingPersona, 10),
            new("ngominhhieu@university.edu.vn", "Ngô Minh Hiếu", HardworkingPersona, 11),
            new("dinhcongminh@university.edu.vn", "Đinh Công Minh", HardworkingPersona, 12),
        ];

        /// <summary>Persona Average — 32 user: bình thường, XP 250-750 (Level 2-3), streak 3-8.</summary>
        public static readonly IReadOnlyList<V2UserSeed> Average =
        [
            new("nguyenvantuan@university.edu.vn", "Nguyễn Văn Tuấn", AveragePersona, 13),
            new("tranminhduc@university.edu.vn", "Trần Minh Đức", AveragePersona, 14),
            new("lethithanhvan@university.edu.vn", "Lê Thị Thanh Vân", AveragePersona, 15),
            new("phamngocanh@university.edu.vn", "Phạm Ngọc Anh", AveragePersona, 16),
            new("hoangthilan@university.edu.vn", "Hoàng Thị Lan", AveragePersona, 17),
            new("phanvanhung@university.edu.vn", "Phan Văn Hùng", AveragePersona, 18),
            new("vuhoangnam@university.edu.vn", "Vũ Hoàng Nam", AveragePersona, 19),
            new("trinhthimai@university.edu.vn", "Trịnh Thị Mai", AveragePersona, 20),
            new("donguyenkhang@university.edu.vn", "Đỗ Nguyên Khang", AveragePersona, 21),
            new("buihongnhung@university.edu.vn", "Bùi Hồng Nhung", AveragePersona, 22),
            new("duongthutrang@university.edu.vn", "Dương Thu Trang", AveragePersona, 23),
            new("lyquocbao@university.edu.vn", "Lý Quốc Bảo", AveragePersona, 24),
            new("ngothihong@university.edu.vn", "Ngô Thị Hồng", AveragePersona, 25),
            new("dinhvanphuc@university.edu.vn", "Đinh Văn Phúc", AveragePersona, 26),
            new("tominhchau@university.edu.vn", "Tô Minh Châu", AveragePersona, 27),
            new("hathithuhang@university.edu.vn", "Hà Thị Thu Hằng", AveragePersona, 28),
            new("caoxuandung@university.edu.vn", "Cao Xuân Dũng", AveragePersona, 29),
            new("luongdinhkhoi@university.edu.vn", "Lương Đình Khôi", AveragePersona, 30),
            new("doanngoctu@university.edu.vn", "Đoàn Ngọc Tú", AveragePersona, 31),
            new("truongvandat@university.edu.vn", "Trương Văn Đạt", AveragePersona, 32),
            new("maithihoa@university.edu.vn", "Mai Thị Hoa", AveragePersona, 33),
            new("lamquocthang@university.edu.vn", "Lâm Quốc Thắng", AveragePersona, 34),
            new("hovanlong@university.edu.vn", "Hồ Văn Long", AveragePersona, 35),
            new("vothikimchi@university.edu.vn", "Võ Thị Kim Chi", AveragePersona, 36),
            new("phungquanghuy@university.edu.vn", "Phùng Quang Huy", AveragePersona, 37),
            new("daoduyan@university.edu.vn", "Đào Duy An", AveragePersona, 38),
            new("tranhoangnam@university.edu.vn", "Trần Hoàng Nam", AveragePersona, 39),
            new("lethimyduyen@university.edu.vn", "Lê Thị Mỹ Duyên", AveragePersona, 40),
            new("nguyenduchuy@university.edu.vn", "Nguyễn Đức Huy", AveragePersona, 41),
            new("phamthingoc@university.edu.vn", "Phạm Thị Ngọc", AveragePersona, 42),
            new("vuminhkhang@university.edu.vn", "Vũ Minh Khang", AveragePersona, 43),
            new("nguyenhongquan@university.edu.vn", "Nguyễn Hồng Quân", AveragePersona, 44),
        ];

        /// <summary>Persona Slacker — 13 user: ít học, XP 40-200 (Level 1-2), streak 0-2 (dễ LaggingLearners).</summary>
        public static readonly IReadOnlyList<V2UserSeed> Slacker =
        [
            new("tranquoctuan@university.edu.vn", "Trần Quốc Tuấn", SlackerPersona, 45),
            new("levanhoa@university.edu.vn", "Lê Văn Hòa", SlackerPersona, 46),
            new("phamdinhquang@university.edu.vn", "Phạm Đình Quang", SlackerPersona, 47),
            new("hoangvantai@university.edu.vn", "Hoàng Văn Tài", SlackerPersona, 48),
            new("nguyenxuanphong@university.edu.vn", "Nguyễn Xuân Phong", SlackerPersona, 49),
            new("vuvanthang@university.edu.vn", "Vũ Văn Thắng", SlackerPersona, 50),
            new("dangminhnhat@university.edu.vn", "Đặng Minh Nhật", SlackerPersona, 51),
            new("buiminhquan@university.edu.vn", "Bùi Minh Quân", SlackerPersona, 52),
            new("duongvandong@university.edu.vn", "Dương Văn Đông", SlackerPersona, 53),
            new("phanducthien@university.edu.vn", "Phan Đức Thiện", SlackerPersona, 54),
            new("trinhvanbinh@university.edu.vn", "Trịnh Văn Bình", SlackerPersona, 55),
            new("doquangvinh@university.edu.vn", "Đỗ Quang Vinh", SlackerPersona, 56),
            new("lyvankhanh@university.edu.vn", "Lý Văn Khánh", SlackerPersona, 57),
        ];

        /// <summary>Persona New — 10 user: mới vào, XP 0, streak 0, chưa có hoạt động (hoặc 20-30 ngày trước).</summary>
        public static readonly IReadOnlyList<V2UserSeed> New =
        [
            new("nguyengiabao@university.edu.vn", "Nguyễn Gia Bảo", NewPersona, 58),
            new("trankhanhlinh@university.edu.vn", "Trần Khánh Linh", NewPersona, 59),
            new("leminhkhoa@university.edu.vn", "Lê Minh Khoa", NewPersona, 60),
            new("phamanhtuan@university.edu.vn", "Phạm Anh Tuấn", NewPersona, 61),
            new("hoangmaiphuong@university.edu.vn", "Hoàng Mai Phương", NewPersona, 62),
            new("vungocanh@university.edu.vn", "Vũ Ngọc Ánh", NewPersona, 63),
            new("dangthaonguyen@university.edu.vn", "Đặng Thảo Nguyên", NewPersona, 64),
            new("buihuyhoang@university.edu.vn", "Bùi Huy Hoàng", NewPersona, 65),
            new("nguyencamtu@university.edu.vn", "Nguyễn Cẩm Tú", NewPersona, 66),
            new("trannhatminh@university.edu.vn", "Trần Nhật Minh", NewPersona, 67),
        ];

        /// <summary>
        /// 69 user theo Index 0..68 (Hardworking → Average → Slacker → New → Showcase) — nguồn duy nhất
        /// cho vòng lặp seed users V2 (Task 1) và kế hoạch hoạt động theo persona (Task 2-6).
        /// </summary>
        public static readonly IReadOnlyList<V2UserSeed> All =
            [.. Hardworking, .. Average, .. Slacker, .. New, Showcase];
    }
}

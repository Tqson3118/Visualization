using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Domain.Enums;

namespace VisualizationDSA.Infrastructure.Data;

/// <summary>Deterministic Phase B dataset. Kept separate from the legacy seed content.</summary>
internal sealed class RealDataSeeder
{
    private const string Password = "RealData@2024";
    private readonly ApplicationDbContext _db;
    private readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    private static readonly (string Name, string Email, int[] Courses, string[] Classes)[] Teachers =
    {
        ("Nguyễn Minh Trí", "trinm@fpt.edu.vn", new[] { 1, 4, 11 }, new[] { "SE1801", "SE1809" }),
        ("Lê Hoàng Nam", "namlh@fpt.edu.vn", new[] { 2, 5, 12 }, new[] { "SE1802", "SE1810" }),
        ("Phạm Thu Hà", "hapt@fpt.edu.vn", new[] { 3, 6, 13 }, new[] { "SE1803", "SE1811" }),
        ("Trần Văn Khánh", "khanhtv@fpt.edu.vn", new[] { 7, 14, 20 }, new[] { "SE1804", "SE1812" }),
        ("Đinh Quang Huy", "huydq@fpt.edu.vn", new[] { 8, 15 }, new[] { "SE1805" }),
        ("Vũ Ngọc Diệp", "diepvn@fpt.edu.vn", new[] { 9, 16 }, new[] { "SE1806" }),
        ("Bùi Xuân Trường", "truongbx@fpt.edu.vn", new[] { 10, 17 }, new[] { "SE1807" }),
        ("Ngô Bích Ngân", "ngannb@fpt.edu.vn", new[] { 18, 19 }, new[] { "SE1808" })
    };

    private static readonly (string Name, string Email, string ClassName, int Xp, int Streak, bool Premium)[] Anchors =
    {
        ("Lê Quốc Bảo", "baolqse1801@fpt.edu.vn", "SE1801", 3800, 21, true),
        ("Trần Thị Hồng Nhung", "nhungtthse1802@fpt.edu.vn", "SE1802", 3550, 28, true),
        ("Phạm Minh Đức", "ducpmse1803@fpt.edu.vn", "SE1803", 3200, 14, false),
        ("Nguyễn Hoàng Anh", "anhnhse1804@fpt.edu.vn", "SE1804", 2900, 19, false),
        ("Vũ Thị Mai Linh", "linhvtmse1805@fpt.edu.vn", "SE1805", 2650, 15, true),
        ("Đặng Tuấn Kiệt", "kietdtse1806@fpt.edu.vn", "SE1806", 2400, 8, false),
        ("Bùi Ngọc Ánh", "anhbngse1807@fpt.edu.vn", "SE1807", 2150, 11, false),
        ("Hoàng Văn Sơn", "sonhvse1808@fpt.edu.vn", "SE1808", 1950, 5, false),
        ("Đỗ Thùy Trang", "trangdtse1809@fpt.edu.vn", "SE1809", 1750, 17, false),
        ("Ngô Đức Huy", "huyndse1810@fpt.edu.vn", "SE1810", 1550, 3, false),
        ("Lý Hải Long", "longlhse1811@fpt.edu.vn", "SE1811", 1380, 9, false),
        ("Trịnh Thanh Tâm", "tamttse1812@fpt.edu.vn", "SE1812", 1220, 2, false),
        ("Nguyễn Thị Thu", "thunttse1801@fpt.edu.vn", "SE1801", 1080, 6, false),
        ("Phạm Quốc Toàn", "toanpqse1802@fpt.edu.vn", "SE1802", 950, 4, false),
        ("Trần Ngọc Linh", "linhtnse1803@fpt.edu.vn", "SE1803", 840, 10, false),
        ("Vũ Minh Khoa", "khoavmse1804@fpt.edu.vn", "SE1804", 740, 7, false),
        ("Đặng Thị Lan", "landtse1805@fpt.edu.vn", "SE1805", 650, 3, false),
        ("Hoàng Anh Tuấn", "tuanhase1806@fpt.edu.vn", "SE1806", 580, 12, false),
        ("Bùi Văn Hùng", "hungbvse1807@fpt.edu.vn", "SE1807", 510, 5, false),
        ("Lý Thị Ngọc", "ngocltse1808@fpt.edu.vn", "SE1808", 450, 8, false),
        ("Đỗ Minh Châu", "chaudmse1809@fpt.edu.vn", "SE1809", 390, 3, false),
        ("Ngô Thị Hoa", "hoantse1810@fpt.edu.vn", "SE1810", 330, 6, false),
        ("Trịnh Văn Dũng", "dungtvse1811@fpt.edu.vn", "SE1811", 280, 2, false),
        ("Lê Thị Kim Anh", "anhltkse1812@fpt.edu.vn", "SE1812", 230, 4, false)
    };

    private static readonly string[] Classes = Enumerable.Range(1, 12).Select(i => $"SE18{i:00}").ToArray();
    private static readonly string[] Surnames = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Đinh", "Trịnh" };
    private static readonly string[] Given = { "Nam", "Sơn", "Hùng", "Dũng", "Phong", "Khôi", "Bảo", "Vinh", "Đạt", "Kiên", "Hiếu", "Long", "Việt", "Tùng", "Quân", "Thắng", "Duy", "Hải", "Phúc", "An" };

    public RealDataSeeder(ApplicationDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        await SeedRosterAsync();
        await SeedCoverageAsync();
    }

    public async Task SeedRosterAsync()
    {
        await RemoveLegacyUsersAsync();
        var admin = await GetOrCreateUserAsync("hungnv@fpt.edu.vn", "Nguyễn Văn Hùng", "Admin", 0, 0, false);
        _users[admin.Email] = admin;
        foreach (var teacher in Teachers)
            _users[teacher.Email] = await GetOrCreateUserAsync(teacher.Email, teacher.Name, "Teacher", 0, 0, false);
        foreach (var anchor in Anchors)
            _users[anchor.Email] = await GetOrCreateUserAsync(anchor.Email, anchor.Name, "Student", anchor.Xp, anchor.Streak, anchor.Premium);
        for (var rank = 25; rank <= 120; rank++)
        {
            var className = Classes[(rank - 25) % Classes.Length];
            var given = Given[rank % Given.Length];
            var surname = Surnames[rank % Surnames.Length];
            var email = $"{Transliterate(given)}{rank:000}{className.ToLowerInvariant()}@fpt.edu.vn";
            _users[email] = await GetOrCreateUserAsync(email, $"{surname} {given}", "Student", Math.Max(1, (int)Math.Round(220 * Math.Exp(-(rank - 24) / 35d))), (rank * 7) % 22, false);
        }
        var pending = new[] { ("Phan Thị Ngọc", "ngocpttse@fpt.edu.vn"), ("Đào Minh Quân", "quandmse@fpt.edu.vn"), ("Nguyễn Thị Lan", "lannttse@fpt.edu.vn"), ("Hoàng Văn Bình", "binhhvse@fpt.edu.vn"), ("Lê Thị Phúc", "phucltse@fpt.edu.vn"), ("Trương Đình Vũ", "vutdse@fpt.edu.vn"), ("Mai Thị Yến", "yenmtse@fpt.edu.vn"), ("Đặng Công Sơn", "sondcse@fpt.edu.vn") };
        foreach (var item in pending) _users[item.Item2] = await GetOrCreateUserAsync(item.Item2, item.Item1, "PendingTeacher", 0, 0, false);
        await _db.SaveChangesAsync();
    }

    public async Task SeedCoverageAsync()
    {
        await SeedCoursesAsync();
        await SeedClassesAsync();
        await SeedBadgesAsync();
        await SeedQuestsAndShopAsync();
        await SeedOrdersAsync();
        await SeedLearningContentAsync();
        await SeedReviewsAndPersonalDataAsync();
        await _db.SaveChangesAsync();
    }

    private async Task RemoveLegacyUsersAsync()
    {
        var keep = Teachers.Select(t => t.Email).Concat(Anchors.Select(a => a.Email)).Concat(new[] { "hungnv@fpt.edu.vn" }).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var legacy = await _db.Users.Where(u => !keep.Contains(u.Email) && (u.Email.EndsWith("@visualizationdsa.dev") || u.Email == "admin@gmail.com")).ToListAsync();
        if (legacy.Count == 0) return;
        // Legacy users có thể là author khóa học (Course.TeacherId -> User là Restrict):
        // xóa course (cascade module/item/lesson) của họ TRƯỚC, rồi mới xóa user.
        var legacyIds = legacy.Select(u => u.Id).ToList();
        var legacyCourses = await _db.Courses.Where(c => legacyIds.Contains(c.TeacherId)).ToListAsync();
        _db.Courses.RemoveRange(legacyCourses);
        _db.Users.RemoveRange(legacy);
        await _db.SaveChangesAsync();
    }

    private async Task<User> GetOrCreateUserAsync(string email, string name, string role, int xp, int streak, bool premium)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            user = new User(email, name, Hash(Password));
            await _db.Users.AddAsync(user);
        }
        user.SetRole(role);
        user.SetTotalXP(xp);
        user.SetStreakDays(streak);
        user.SetPremiumStatus(premium);
        user.SetHearts(10);
        return user;
    }

    private async Task SeedCoursesAsync()
    {
        var teachers = Teachers.Select(t => _users[t.Email]).ToArray();
        var existing = await _db.Courses.ToListAsync();
        var names = new[] { "Cơ bản", "Trung cấp", "Nâng cao", "Tìm kiếm", "Sắp xếp cơ bản", "Đệ quy & Chia để trị", "Heap", "Đồ thị", "Tham lam & QHĐ", "Cây nhị phân", "Bảng băm", "Tìm kiếm nâng cao", "AVL", "Sắp xếp nâng cao", "MST", "Bellman-Ford & Topological", "QHĐ nâng cao", "Two Pointers & Sliding Window", "Trie & Union-Find", "Ôn tổng hợp DSA" };
        for (var i = 0; i < names.Length; i++)
        {
            var title = $"{i + 1:00}. {names[i]}";
            var course = existing.FirstOrDefault(c => c.Title == title);
            if (course is null)
            {
                course = new Course(teachers[i % teachers.Length].Id, title, $"Lộ trình thực hành {names[i]} với dữ liệu và ví dụ tiếng Việt.", CourseCategory.Algorithm, i < 6 ? CourseDifficulty.Beginner : CourseDifficulty.Intermediate, i > 9, $"/images/courses/course-{i + 1}.png");
                course.SetMetadata(3.8m + (i % 12) / 10m, 50 + i * 11, JsonSerializer.Serialize(new[] { $"Hiểu nền tảng {names[i]}", "Phân tích độ phức tạp", "Tự triển khai bằng code" }), JsonSerializer.Serialize(new[] { "Đọc được pseudocode", "Chọn cấu trúc phù hợp", "Giải thích trade-off" }), JsonSerializer.Serialize(new[] { "Ví dụ trực quan", "Bài tập theo cấp độ", "Gợi ý từng bước" }), 150 + i * 15);
                course.Publish();
                await _db.Courses.AddAsync(course);
                existing.Add(course);
            }
        }
        await _db.SaveChangesAsync();
        foreach (var course in existing.Where(c => names.Any(n => c.Title.Contains(n, StringComparison.Ordinal))))
        {
            var modules = await _db.CourseModules.Where(m => m.CourseId == course.Id).CountAsync();
            if (modules > 0) continue;
            for (var j = 0; j < 3; j++)
            {
                var module = new CourseModule(course.Id, $"Module {j + 1}: Thực hành", "Nội dung được biên soạn cho lộ trình thật.", j + 1);
                await _db.CourseModules.AddAsync(module);
                var lesson = new Lesson($"{course.Title} — Bài {j + 1}", $"# {course.Title} — Bài {j + 1}\n\nĐây là nội dung học thuật thật về {course.Title}, gồm ý tưởng, ví dụ và bài luyện tập.", "dsa", JsonSerializer.Serialize(new { simulatorKey = "sort.bubble" }), 30, course.TeacherId);
                lesson.SubmitForReview(); lesson.ApproveAndPublish();
                await _db.Lessons.AddAsync(lesson);
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task SeedClassesAsync()
    {
        var students = _users.Values.Where(u => u.Role == "Student").OrderBy(u => u.Email).ToArray();
        for (var i = 0; i < Classes.Length; i++)
        {
            var teacher = _users[Teachers[i % Teachers.Length].Email];
            var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Name == Classes[i]);
            if (classroom is null)
            {
                classroom = new Classroom(teacher.Id, Classes[i], $"Lớp Kỹ thuật phần mềm {Classes[i]}", $"REAL{i + 1:0000}", DateTime.UtcNow.AddYears(1), 40);
                await _db.Classrooms.AddAsync(classroom);
                await _db.SaveChangesAsync();
            }
            var members = students.Where((_, index) => index % Classes.Length == i).Take(10).ToArray();
            foreach (var student in members)
            {
                if (!await _db.ClassroomEnrollments.AnyAsync(e => e.ClassroomId == classroom.Id && e.StudentId == student.Id))
                    await _db.ClassroomEnrollments.AddAsync(new ClassroomEnrollment(classroom.Id, student.Id));
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task SeedBadgesAsync()
    {
        var definitions = new[] { ("Người mới bắt đầu", "Hoàn thành bài học đầu tiên", "🌱"), ("Chuỗi 3 ngày", "Streak từ 3 ngày", "🔥"), ("Chuỗi 7 ngày", "Streak từ 7 ngày", "⚡"), ("Chuỗi 14 ngày", "Streak từ 14 ngày", "💎"), ("Chuỗi 30 ngày", "Streak từ 30 ngày", "👑"), ("Hoàn thành khóa đầu tiên", "Hoàn thành một khóa", "🎓"), ("Học viên chăm chỉ", "Hoàn thành năm khóa", "📚"), ("Học viên xuất sắc", "Hoàn thành mười khóa", "🏆"), ("Bậc thầy thuật toán", "Hoàn thành hai mươi khóa", "🧠"), ("Điểm tuyệt đối", "Đạt điểm tuyệt đối", "💯"), ("Vượt bậc thang", "Hoàn thành ladder", "🪜"), ("Lập trình viên tích cực", "Nộp bài code", "💻"), ("Top 3 bảng xếp hạng", "Top 3 tuần", "🥇"), ("Top 10 bảng xếp hạng", "Top 10 tuần", "🏅") };
        var bao = _users[Anchors[0].Email];
        foreach (var d in definitions)
        {
            var badge = await _db.Badges.FirstOrDefaultAsync(b => b.Name == d.Item1);
            if (badge is null) { badge = new Badge(d.Item1, d.Item2, d.Item3, "#7c3aed", "{}"); await _db.Badges.AddAsync(badge); await _db.SaveChangesAsync(); }
            if (!await _db.UserBadges.AnyAsync(x => x.UserId == bao.Id && x.BadgeId == badge.Id)) await _db.UserBadges.AddAsync(new UserBadge(bao.Id, badge.Id));
        }
    }

    private async Task SeedQuestsAndShopAsync()
    {
        var bao = _users[Anchors[0].Email];
        var quests = new List<Quest>();
        for (var i = 1; i <= 4; i++) quests.Add(new Quest($"real.daily.{i}", $"Daily Quest {i}", "Hoàn thành hoạt động học tập hôm nay", "Daily", "{\"targetCount\":3}", $"{{\"gems\":{50 + i * 10},\"xp\":{20 + i * 5}}}", i));
        for (var i = 1; i <= 2; i++) quests.Add(new Quest($"real.weekly.{i}", $"Weekly Quest {i}", "Duy trì tiến độ trong tuần", "Weekly", "{\"targetCount\":5}", "{\"gems\":150,\"xp\":100}", 10 + i));
        quests.Add(new Quest("real.monthly.1", "Monthly Quest 1", "Hoàn thành một chặng học tập", "Monthly", "{\"targetCount\":10}", "{\"gems\":500,\"xp\":300}", 20));
        foreach (var q in quests)
        {
            var saved = await _db.Quests.FirstOrDefaultAsync(x => x.QuestKey == q.QuestKey);
            if (saved is null) { await _db.Quests.AddAsync(q); saved = q; }
            var progress = saved.Period == "Daily" && saved.SortOrder <= 2 ? 3 : saved.Period == "Daily" ? 1 : saved.Period == "Weekly" ? 2 : 0;
            if (!await _db.UserQuests.AnyAsync(x => x.QuestId == saved.Id && x.UserId == bao.Id)) await _db.UserQuests.AddAsync(new UserQuest(saved.Id, bao.Id, progress, progress >= 3 && saved.SortOrder <= 2 ? "Completed" : "InProgress"));
        }
        var items = new[] { ("Aurora Frame", "Khung hồ sơ cực quang", "frame", 120, "✨"), ("Scholar Avatar", "Avatar học giả", "avatar", 180, "🎓"), ("Ocean Theme", "Chủ đề đại dương", "theme", 240, "🌊"), ("Ruby Frame", "Khung ruby", "frame", 320, "💎"), ("Forest Avatar", "Avatar rừng xanh", "avatar", 400, "🌲"), ("Sunset Theme", "Chủ đề hoàng hôn", "theme", 550, "🌅"), ("Code Star", "Biểu tượng coder", "misc", 700, "⭐"), ("Neon Frame", "Khung neon", "frame", 900, "🔮"), ("FPT Avatar", "Avatar FPT", "avatar", 1200, "🟣"), ("Legend Theme", "Chủ đề huyền thoại", "theme", 1600, "👑") };
        foreach (var item in items)
        {
            var saved = await _db.ShopItems.FirstOrDefaultAsync(x => x.Name == item.Item1);
            if (saved is null) { saved = new ShopItem(item.Item1, item.Item2, item.Item3, item.Item4, item.Item5, Array.IndexOf(items, item)); await _db.ShopItems.AddAsync(saved); await _db.SaveChangesAsync(); }
            if ((item.Item3 == "frame" && item.Item1 == "Aurora Frame") || (item.Item3 == "avatar" && item.Item1 == "Scholar Avatar"))
            {
                if (!await _db.UserInventory.AnyAsync(x => x.UserId == bao.Id && x.ItemId == saved.Id)) await _db.UserInventory.AddAsync(new UserInventory(bao.Id, saved.Id, equipped: true));
                await _db.GemTransactions.AddAsync(new GemTransaction(bao.Id, item.Item4 + 1000, "Earn", $"seed-{item.Item1}"));
                await _db.GemTransactions.AddAsync(new GemTransaction(bao.Id, item.Item4, "Spend", saved.Id.ToString()));
            }
        }
    }

    private async Task SeedOrdersAsync()
    {
        var premiumUsers = _users.Values.Where(u => u.IsPremium).ToArray();
        var allStudents = _users.Values.Where(u => u.Role == "Student").OrderBy(u => u.Email).ToArray();
        var baseDate = DateTime.UtcNow.Date;
        for (var i = 0; i < 30; i++)
        {
            var code = $"FPT2608{90000000 + i:00000000}";
            if (await _db.Orders.AnyAsync(o => o.PaymentCode == code)) continue;

            decimal amount;
            User buyer;
            if (i < 8 && premiumUsers.Length > 0)
            {
                buyer = premiumUsers[i % premiumUsers.Length];
                amount = i % 2 == 0 ? 299000m : 199000m;
            }
            else
            {
                buyer = allStudents[i % allStudents.Length];
                amount = (10 + (i % 9) * 10) * 1000m - 1000m;
                if (amount <= 0) amount = 99000m;
            }

            var order = new Order(buyer.Id, code, amount);
            var dayOffset = (i * 13) / 29;
            var created = baseDate.AddDays(-dayOffset).AddHours(8 + (i % 12)).AddMinutes((i * 17) % 60);
            DateTime? completed = null;
            if (i % 7 != 0)
            {
                order.SetTransactionReference($"SEPAY-REAL-{i:0000}");
                order.MarkAsCompleted();
                completed = created.AddMinutes(2 + (i % 8));
                if (amount >= 199000) buyer.SetPremiumStatus(true);
            }
            else if (i % 5 == 0)
            {
                order.Cancel();
            }
            order.SetDateRange(created, completed);
            await _db.Orders.AddAsync(order);
        }
        await _db.SaveChangesAsync();
    }

    private async Task SeedLearningContentAsync()
    {
        var teachers = Teachers.Select(t => _users[t.Email]).ToArray();
        var courses = await _db.Courses.OrderBy(c => c.Title).Take(20).ToListAsync();
        var lessons = await _db.Lessons.OrderBy(l => l.Title).ToListAsync();

        var codelabSpecs = new[]
        {
            ("Implement Binary Search", "Viết tìm kiếm nhị phân trên mảng đã sắp xếp.", "int binary_search(int[] a, int target) { return -1; }", 2, 80, "search,binary"),
            ("Implement BFS", "Duyệt đồ thị theo chiều rộng từ một đỉnh bắt đầu.", "IEnumerable<int> bfs(int[][] graph, int start) { yield break; }", 2, 90, "graph,bfs"),
            ("Implement Merge Sort", "Sắp xếp mảng bằng chiến lược chia để trị.", "int[] merge_sort(int[] values) { return values; }", 3, 100, "sort,merge"),
            ("Implement Dijkstra", "Tìm đường đi ngắn nhất trong đồ thị trọng số không âm.", "int[] dijkstra(int[][] edges, int start) { return []; }", 4, 120, "graph,dijkstra"),
            ("Implement Union Find", "Xây dựng cấu trúc hợp nhất và tìm đại diện tập hợp.", "int find(int x) { return x; }", 3, 100, "graph,disjoint-set"),
            ("Implement Sliding Window", "Tìm đoạn con dài nhất thỏa điều kiện bằng cửa sổ trượt.", "int solve(int[] values, int limit) { return 0; }", 2, 80, "arrays,sliding-window")
        };
        var codelabs = new List<Codelab>();
        foreach (var spec in codelabSpecs)
        {
            var codelab = await _db.Codelabs.FirstOrDefaultAsync(c => c.Title == spec.Item1);
            if (codelab is null)
            {
                codelab = new Codelab(spec.Item1, spec.Item2, spec.Item3, spec.Item4, spec.Item5, constraints: "Input hợp lệ; không dùng thư viện sắp xếp có sẵn.", examples: "Xem test case minh họa trong trình chấm.", tags: spec.Item6, ownerId: teachers[codelabs.Count % teachers.Length].Id);
                await _db.Codelabs.AddAsync(codelab);
            }
            codelabs.Add(codelab);
        }
        await _db.SaveChangesAsync();

        foreach (var module in await _db.CourseModules.OrderBy(m => m.CourseId).ThenBy(m => m.OrderIndex).ToListAsync())
        {
            if (await _db.ModuleItems.AnyAsync(i => i.ModuleId == module.Id)) continue;
            var lesson = lessons[(module.OrderIndex - 1) % lessons.Count];
            var codelab = codelabs[(module.OrderIndex - 1) % codelabs.Count];
            await _db.ModuleItems.AddAsync(new ModuleItem(module.Id, null, ModuleItemType.Lesson, lesson.Id, null, null, lesson.Title, 1, true));
            await _db.ModuleItems.AddAsync(new ModuleItem(module.Id, null, ModuleItemType.Codelab, null, null, codelab.Id, codelab.Title, 2, true));
            var secondLesson = lessons[(module.OrderIndex + 6) % lessons.Count];
            await _db.ModuleItems.AddAsync(new ModuleItem(module.Id, null, ModuleItemType.Lesson, secondLesson.Id, null, null, secondLesson.Title, 3, false));
        }

        var articles = new List<TheoryArticle>();
        for (var i = 0; i < 8; i++)
        {
            var slug = $"real-dsa-theory-{i + 1:00}";
            var article = await _db.TheoryArticles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article is null)
            {
                article = new TheoryArticle(teachers[i % teachers.Length].Id, $"Lý thuyết DSA thực tế {i + 1:00}", slug, $"# Lý thuyết DSA {i + 1:00}\n\nBài viết giải thích khái niệm, độ phức tạp và ví dụ triển khai bằng dữ liệu thực tế.", "DSA", i < 3 ? "Beginner" : "Intermediate", "dsa,algorithm,visualization", 6 + i);
                article.Publish();
                await _db.TheoryArticles.AddAsync(article);
            }
            articles.Add(article);
        }
        await _db.SaveChangesAsync();
        for (var i = 0; i < Math.Min(lessons.Count, articles.Count); i++)
            if (!await _db.LessonTheoryArticles.AnyAsync(x => x.LessonId == lessons[i].Id && x.TheoryArticleId == articles[i].Id))
                await _db.LessonTheoryArticles.AddAsync(new LessonTheoryArticle(lessons[i].Id, articles[i].Id, 1));

        var pathTitles = new[] { "Nền tảng thuật toán", "Đồ thị thực chiến", "Ôn tập phỏng vấn" };
        for (var p = 0; p < pathTitles.Length; p++)
        {
            var path = await _db.LearningPaths.FirstOrDefaultAsync(x => x.Title == pathTitles[p]);
            if (path is null) { path = new LearningPath(pathTitles[p], "Lộ trình học có thứ tự, dùng nội dung seed thực tế."); await _db.LearningPaths.AddAsync(path); await _db.SaveChangesAsync(); }
            if (!await _db.LearningPathNodes.AnyAsync(n => n.LearningPathId == path.Id))
                for (var n = 0; n < 6; n++) await _db.LearningPathNodes.AddAsync(new LearningPathNode(path.Id, n + 1, lessons[(p * 6 + n) % lessons.Count].Title, lessons[(p * 6 + n) % lessons.Count].Id));
        }

        var admin = _users["hungnv@fpt.edu.vn"];
        foreach (var lesson in lessons.Take(12))
        {
            var review = await _db.LessonReviews.FirstOrDefaultAsync(r => r.LessonId == lesson.Id);
            if (review is null) { review = new LessonReview(lesson.Id); review.ProcessReview(admin.Id, true, "Nội dung đạt yêu cầu và đã kiểm tra ví dụ."); await _db.LessonReviews.AddAsync(review); }
        }
    }

    private async Task SeedReviewsAndPersonalDataAsync()
    {
        var bao = _users[Anchors[0].Email];
        var courses = await _db.Courses.OrderBy(c => c.Title).Take(20).ToListAsync();
        var lessons = await _db.Lessons.OrderBy(l => l.Title).Take(10).ToListAsync();
        foreach (var course in courses)
            for (var i = 0; i < 4; i++)
            {
                var user = _users[Anchors[(i + 1) % Anchors.Length].Email];
                if (!await _db.CourseReviews.AnyAsync(r => r.CourseId == course.Id && r.UserId == user.Id)) await _db.CourseReviews.AddAsync(new CourseReview(course.Id, user.Id, 4 + i % 2, "Nội dung rõ ràng, ví dụ thực tế và tiến độ học rất hợp lý."));
            }
        foreach (var lesson in lessons.Take(2))
        {
            if (!await _db.LessonComments.AnyAsync(c => c.LessonId == lesson.Id && c.UserId == bao.Id)) await _db.LessonComments.AddAsync(new LessonComment(lesson.Id, bao.Id, "Ví dụ này giúp mình hiểu rõ hơn về độ phức tạp."));
            if (!await _db.LessonNotes.AnyAsync(n => n.LessonId == lesson.Id && n.UserId == bao.Id)) await _db.LessonNotes.AddAsync(new LessonNote(bao.Id, lesson.Id, "<p>Ôn lại phần invariant và complexity.</p>"));
        }
        var favoriteKeys = new[] { "sort.bubble", "search.binary", "graph.bfs" };
        foreach (var key in favoriteKeys) if (!await _db.Favorites.AnyAsync(f => f.UserId == bao.Id && f.SimulationKey == key)) await _db.Favorites.AddAsync(new Favorite(bao.Id, key));
    }

    private static string Hash(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password + "visualizationdsa-salt"))).ToLowerInvariant();
    private static string Transliterate(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(new string(chars).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant().Where(c => c is >= 'a' and <= 'z').ToArray());
    }
}

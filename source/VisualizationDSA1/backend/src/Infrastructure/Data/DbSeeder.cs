using System;
using System.Collections.Generic;
using VisualizationDSA.Domain.Enums;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using VisualizationDSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace VisualizationDSA.Infrastructure.Data
{
    /// <summary>
    /// DbSeeder chuẩn PRODUCTION (G4.1 nâng cấp) — mọi module có dữ liệu hiển thị, demo end-to-end chạy được.
    /// Class được tách thành nhiều partial file:
    ///   DbSeeder.cs          — core: SeedAsync, badges, semantic graph, courses.json, teacher_roadmaps.json, review-demo roadmap.
    ///   DbSeeder.Users.cs    — S1 Users (3 vai, XP tăng dần, Premium, 2 Teacher).
    ///   DbSeeder.Quizzes.cs  — S3 Quiz tiếng Việt (12 quiz nền + mở rộng 10-15 câu).
    ///   DbSeeder.Roadmaps.cs — S2+S4+S5 Roadmap DSA + node giàu nội dung + codelab + lesson + moduleitem.
    ///   DbSeeder.Progress.cs — S6 Enrollments + Progress + Reviews.
    ///   DbSeeder.Classrooms.cs — S7 Classroom + học sinh.
    ///   DbSeeder.Content.cs  — S8 Theory Articles + Notifications + S10 Gems/Quests/Inventory.
    /// Quy tắc: idempotent theo tên/email; reseed sạch roadmap DSA (xóa node + enrollment cũ trước khi tạo lại);
    /// KHÔNG xóa user/quiz đang được tham chiếu; dùng domain method/constructor.
    /// </summary>
    public partial class DbSeeder
    {
        private readonly ApplicationDbContext _context;

        public DbSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            try { await SeedBadgesAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedBadges Error]: {ex}"); }
            try { await SeedUsersAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedUsers Error]: {ex}"); }
            try { await SeedQuizzesAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedQuizzes Error]: {ex}"); }
            try { await SeedQuizzesExtendedAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedQuizzes2 Error]: {ex}"); }
            try { await SeedSemanticGraphAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedGraph Error]: {ex}"); }
            // SeedCoursesAsync xóa sạch Courses/Lessons/Codelabs/ModuleItems (reseed courses.json) → phải chạy TRƯỚC
            // khi tạo lesson/codelab của roadmap DSA, nếu không sẽ xóa nhầm dữ liệu roadmap.
            try { await SeedCoursesAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedCourses Error]: {ex}"); }
            try { await SeedDSARoadmapsAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedDSARoadmaps Error]: {ex}"); }
            try { await SeedTeacherRoadmapsAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedRoadmaps Error]: {ex}"); }
            try { await SeedReviewDemoRoadmapAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedReviewDemo Error]: {ex}"); }
            try { await SeedEnrollmentsProgressReviewsAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedEnrollments Error]: {ex}"); }
            try { await SeedClassroomsAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedClassrooms Error]: {ex}"); }
            try { await SeedTheoryArticlesAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedTheoryArticles Error]: {ex}"); }
            try { await SeedNotificationsAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedNotifications Error]: {ex}"); }
            try { await SeedQuestsAndInventoryAsync(); } catch (Exception ex) { Console.WriteLine($"[SeedQuestsAndInventory Error]: {ex}"); }
        }

        private static string HashPasswordSHA256(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "visualizationdsa-salt"));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private async Task SeedBadgesAsync()
        {
            if (_context.Badges.Any()) return;

            var badges = new List<Badge>
            {
                new Badge("First Steps", "Hoàn thành bài trắc nghiệm đầu tiên", "🎯", "#22c55e", "{ 'quizCompleted': 1 }"),
                new Badge("Sorting Wizard", "Hoàn thành 4 thuật toán sắp xếp", "⚡", "#3b82f6", "{ 'sortingCompleted': 4 }"),
                new Badge("OOP Guru", "Hiểu rõ Encapsulation & Inheritance", "🔐", "#8b5cf6", "{ 'oopCompleted': 2 }"),
                new Badge("SOLID Master", "Áp dụng đúng 5 nguyên lý SOLID", "🏛️", "#f59e0b", "{ 'solidCompleted': 5 }"),
                new Badge("Pattern Hunter", "Sử dụng 3 Design Patterns", "🎨", "#ec4899", "{ 'patternsCompleted': 3 }"),
                new Badge("Streak Keeper", "Học liên tục 7 ngày", "🔥", "#ef4444", "{ 'streakDays': 7 }"),
                new Badge("System Architect", "Thiết kế hệ thống phân tán", "🏗️", "#f97316", "{ 'systemCompleted': 1 }"),
                new Badge("DSA Champion", "Hoàn thành toàn bộ khóa học", "👑", "#eab308", "{ 'level': 5 }")
            };

            foreach (var badge in badges)
            {
                await _context.Badges.AddAsync(badge);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedSemanticGraphAsync()
        {
            if (_context.SemanticConceptNodes.Any()) return;

            var oop = new SemanticConceptNode("oop.encapsulation", "Encapsulation", "OOP", "Hành vi đóng gói che giấu chi tiết triển khai và bảo vệ dữ liệu bên trong.", new[] { 0.1, 0.2, 0.3 }, 0.9);
            var inheritance = new SemanticConceptNode("oop.inheritance", "Inheritance", "OOP", "Cho phép các lớp con kế thừa lại cấu trúc và phương thức từ lớp cha.", new[] { 0.15, 0.25, 0.35 }, 0.85);
            var polymorphism = new SemanticConceptNode("oop.polymorphism", "Polymorphism", "OOP", "Đa hình cho phép đối tượng thực hiện các hành vi khác nhau dựa trên kiểu runtime của nó.", new[] { 0.2, 0.3, 0.4 }, 0.8);

            var srp = new SemanticConceptNode("solid.srp", "Single Responsibility", "SOLID", "Nguyên lý đơn trách nhiệm: Mỗi lớp chỉ nên đảm nhận duy nhất một lý do để thay đổi.", new[] { 0.3, 0.4, 0.5 }, 0.95);
            var ocp = new SemanticConceptNode("solid.ocp", "Open/Closed", "SOLID", "Nguyên lý đóng mở: Lớp nên mở rộng cho việc kế thừa kế tiếp nhưng đóng cho việc sửa trực tiếp.", new[] { 0.35, 0.45, 0.55 }, 0.9);
            var dip = new SemanticConceptNode("solid.dip", "Dependency Inversion", "SOLID", "Nguyên lý đảo ngược phụ thuộc: Các module cấp cao không nên phụ thuộc trực tiếp module cấp thấp.", new[] { 0.4, 0.5, 0.6 }, 0.85);

            var array = new SemanticConceptNode("dsa.array", "Array", "DSA", "Mảng là cấu trúc dữ liệu lưu trữ tuyến tính các phần tử cùng kiểu liên tiếp.", new[] { 0.5, 0.6, 0.7 }, 0.75);
            var bst = new SemanticConceptNode("dsa.bst", "Binary Search Tree", "DSA", "Cây tìm kiếm nhị phân sắp xếp các đỉnh sao cho nhánh trái nhỏ hơn và nhánh phải lớn hơn đỉnh gốc.", new[] { 0.6, 0.7, 0.8 }, 0.8);

            await _context.SemanticConceptNodes.AddRangeAsync(oop, inheritance, polymorphism, srp, ocp, dip, array, bst);
            await _context.SaveChangesAsync();

            var edges = new List<KnowledgeEdge>
            {
                new KnowledgeEdge(inheritance.Id, oop.Id, "DependsOn", 1.2),
                new KnowledgeEdge(polymorphism.Id, inheritance.Id, "DependsOn", 1.5),
                new KnowledgeEdge(ocp.Id, polymorphism.Id, "DependsOn", 1.3),
                new KnowledgeEdge(dip.Id, oop.Id, "DependsOn", 1.4),
                new KnowledgeEdge(bst.Id, array.Id, "DependsOn", 1.1)
            };

            await _context.KnowledgeEdges.AddRangeAsync(edges);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCoursesAsync()
        {
            // Forced re-seeding to apply new courses.json data (dev/demo phase). KHÔNG xóa quiz.
            _context.ModuleItems.RemoveRange(_context.ModuleItems);
            _context.CourseModules.RemoveRange(_context.CourseModules);
            _context.Lessons.RemoveRange(_context.Lessons);
            _context.Courses.RemoveRange(_context.Courses);
            _context.CodelabTestCases.RemoveRange(_context.CodelabTestCases);
            _context.Codelabs.RemoveRange(_context.Codelabs);
            await _context.SaveChangesAsync();

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher" || u.Role == "Admin")
                          ?? await _context.Users.FirstOrDefaultAsync();
            if (teacher == null)
            {
                teacher = new User("teacher@visualizationdsa.dev", "Default Teacher", HashPasswordSHA256("Teacher@2024"));
                teacher.SetRole("Teacher");
                await _context.Users.AddAsync(teacher);
                await _context.SaveChangesAsync();
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string seedContentPath = Path.Combine(baseDir, "Data", "SeedContent");
            if (!Directory.Exists(seedContentPath))
            {
                seedContentPath = Path.Combine(baseDir, "..", "..", "..", "Infrastructure", "Data", "SeedContent");
            }

            string coursesJsonPath = Path.Combine(seedContentPath, "courses.json");
            if (!File.Exists(coursesJsonPath))
            {
                Console.WriteLine($"[SeedCourses] Seed file not found at {coursesJsonPath}");
                return;
            }

            string jsonContent = await File.ReadAllTextAsync(coursesJsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var seedCourses = JsonSerializer.Deserialize<List<SeedCourseModel>>(jsonContent, options);

            if (seedCourses == null) return;

            foreach (var sc in seedCourses)
            {
                var diff = Enum.TryParse<CourseDifficulty>(sc.Difficulty, out var parsedDiff) ? parsedDiff : CourseDifficulty.Beginner;
                var cat = Enum.TryParse<CourseCategory>(sc.Category, out var parsedCat) ? parsedCat : CourseCategory.DataStructure;

                var course = new Course(teacher.Id, sc.Title, sc.Description, cat, diff, true, sc.ImageUrl);
                course.Publish();
                await _context.Courses.AddAsync(course);
                await _context.SaveChangesAsync();

                var module = new CourseModule(course.Id, "Chương 1", "Nội dung chính", 1000);
                await _context.CourseModules.AddAsync(module);
                await _context.SaveChangesAsync();

                int lessonIndex = 1;
                foreach (var sl in sc.Lessons)
                {
                    string markdownContent = "";
                    string mdPath = Path.Combine(seedContentPath, sl.ContentFile);
                    if (File.Exists(mdPath)) markdownContent = await File.ReadAllTextAsync(mdPath);

                    var lesson = new Lesson(sl.Title, markdownContent, sl.SandboxType, sl.SandboxConfig, sl.XpReward, teacher.Id);
                    await _context.Lessons.AddAsync(lesson);
                    await _context.SaveChangesAsync();

                    int itemOrder = lessonIndex * 1000;
                    var lessonItem = new ModuleItem(module.Id, null, ModuleItemType.Lesson, lesson.Id, null, null, sl.Title, itemOrder, true);
                    await _context.ModuleItems.AddAsync(lessonItem);

                    if (sl.Quiz != null)
                    {
                        var quiz = new Quiz(sl.Quiz.Title, "Trắc nghiệm ôn tập", "general", sl.Quiz.Questions.Count, sl.XpReward + 10);
                        foreach (var sq in sl.Quiz.Questions)
                        {
                            quiz.AddQuestion(sq.Content, sq.Options.Select(o => o.Text).ToArray(), sq.Options.FindIndex(o => o.IsCorrect), sq.Options.FirstOrDefault(o => o.IsCorrect)?.Explanation ?? "");
                        }
                        await _context.Quizzes.AddAsync(quiz);
                        await _context.SaveChangesAsync();

                        var quizItem = new ModuleItem(module.Id, null, ModuleItemType.Quiz, null, quiz.Id, null, "Quiz: " + sl.Title, itemOrder + 500, true);
                        await _context.ModuleItems.AddAsync(quizItem);
                    }

                    if (sl.Codelab != null)
                    {
                        var codelab = new Codelab(
                            sl.Codelab.Title, sl.Codelab.Description, sl.Codelab.InitialCode,
                            1, 50, 5000, 128000000, "csharp", "Vui lòng xem mô tả", "Xem ví dụ trong mô tả", "general"
                        );

                        int caseIndex = 1;
                        foreach (var tc in sl.Codelab.TestCases)
                        {
                            codelab.TestCases.Add(new CodelabTestCase(codelab.Id, tc.Input, tc.ExpectedOutput, tc.IsHidden, 10, caseIndex++));
                        }
                        await _context.Codelabs.AddAsync(codelab);
                        await _context.SaveChangesAsync();

                        var codelabItem = new ModuleItem(module.Id, null, ModuleItemType.Codelab, null, null, codelab.Id, "Codelab: " + sl.Title, itemOrder + 750, true);
                        await _context.ModuleItems.AddAsync(codelabItem);
                    }
                    lessonIndex++;
                }
            }
        }

        private async Task SeedTeacherRoadmapsAsync()
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher" || u.Role == "Admin");
            if (teacher == null) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string seedContentPath = Path.Combine(baseDir, "Data", "SeedContent");
            if (!Directory.Exists(seedContentPath))
            {
                seedContentPath = Path.Combine(baseDir, "..", "..", "..", "Infrastructure", "Data", "SeedContent");
            }

            string roadmapsPath = Path.Combine(seedContentPath, "teacher_roadmaps.json");
            if (!File.Exists(roadmapsPath)) { Console.WriteLine($"[SeedDebug] teacher_roadmaps.json NOT FOUND at {roadmapsPath}"); return; }
            Console.WriteLine($"[SeedDebug] teacher_roadmaps.json FOUND at {roadmapsPath}");

            var jsonContent = await File.ReadAllTextAsync(roadmapsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var roadmaps = JsonSerializer.Deserialize<List<SeedRoadmapModel>>(jsonContent, options);
            Console.WriteLine($"[SeedDebug] teacher roadmaps parsed={(roadmaps == null ? "NULL" : roadmaps.Count.ToString())}");

            if (roadmaps == null) return;

            foreach (var r in roadmaps)
            {
                var visibility = r.IsPublic ? "Public" : "Private";
                var roadmap = await _context.CustomRoadmaps.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Name == r.Title);

                if (roadmap == null)
                {
                    roadmap = new CustomRoadmap(teacher.Id, r.Title, r.Description, "[]", null, visibility);
                    roadmap.Approve(); // Auto approve for seed data
                    await _context.CustomRoadmaps.AddAsync(roadmap);
                    await _context.SaveChangesAsync();

                    foreach (var n in r.Nodes)
                    {
                        var node = new CustomNode(roadmap.Id, n.Name, n.Description, n.Difficulty ?? "Medium", n.SortOrder);
                        node.UpdateContent(n.ContentJson ?? "[]", null, null);
                        node.UpdateVisualizerConfig(n.VisualizerConfig);
                        if (!string.IsNullOrEmpty(n.OfficialApproach))
                        {
                            node.SetApproach(n.OfficialApproach, "", "");
                        }
                        await _context.CustomNodes.AddAsync(node);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // G4.1.6 — cập nhật visualizerConfig cho node đã tồn tại (idempotent)
                    foreach (var n in r.Nodes)
                    {
                        var node = roadmap.Nodes.FirstOrDefault(x => x.Name == n.Name && x.SortOrder == n.SortOrder);
                        if (node == null) continue;
                        if (!string.IsNullOrEmpty(n.VisualizerConfig) && string.IsNullOrEmpty(node.VisualizerConfig))
                        {
                            node.UpdateVisualizerConfig(n.VisualizerConfig);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        private class SeedRoadmapModel
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsPublic { get; set; }
            public List<SeedNodeModel> Nodes { get; set; } = new();
        }

        private class SeedNodeModel
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string? Difficulty { get; set; }
            public int SortOrder { get; set; }
            public string? ContentJson { get; set; }
            public string? VisualizerConfig { get; set; }
            public string? OfficialApproach { get; set; }
        }

        private class SeedCourseModel
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Difficulty { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public List<SeedLessonModel> Lessons { get; set; } = new();
        }

        private class SeedLessonModel
        {
            public string Title { get; set; } = string.Empty;
            public string ContentFile { get; set; } = string.Empty;
            public string SandboxType { get; set; } = string.Empty;
            public string SandboxConfig { get; set; } = string.Empty;
            public int XpReward { get; set; }
            public SeedQuizModel? Quiz { get; set; }
            public SeedCodelabModel? Codelab { get; set; }
        }

        private class SeedQuizModel
        {
            public string Title { get; set; } = string.Empty;
            public int PassingScore { get; set; }
            public List<SeedQuestionModel> Questions { get; set; } = new();
        }

        private class SeedQuestionModel
        {
            public string Content { get; set; } = string.Empty;
            public List<SeedOptionModel> Options { get; set; } = new();
        }

        private class SeedOptionModel
        {
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
            public string Explanation { get; set; } = string.Empty;
        }

        private class SeedCodelabModel
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string InitialCode { get; set; } = string.Empty;
            public List<SeedTestCaseModel> TestCases { get; set; } = new();
        }

        private class SeedTestCaseModel
        {
            public string Input { get; set; } = string.Empty;
            public string ExpectedOutput { get; set; } = string.Empty;
            public bool IsHidden { get; set; }
        }
    }
}

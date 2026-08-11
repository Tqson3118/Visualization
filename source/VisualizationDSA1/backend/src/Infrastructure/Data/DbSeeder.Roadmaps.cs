using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Enums;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S2 + S4 + S5 — 3 ROADMAP DSA (Sorting/Graph/OOP), node giàu nội dung,
        // quiz + codelab + lesson + moduleitem cross-link (Lab → Lesson).
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedDSARoadmapsAsync()
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@visualizationdsa.dev")
                          ?? await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher" || u.Role == "Admin");
            if (teacher == null) return;

            // ── Reseed sạch: xóa roadmap DSA cũ + node + enrollment + review.
            //    KHÔNG xóa quiz/codelab/lesson đang tái sử dụng (idempotent).
            var oldNames = new[] { "Thuật toán Sắp xếp", "Đồ thị & Đường đi", "OOP & SOLID" };
            var oldDsaRoadmaps = await _context.CustomRoadmaps
                .Where(r => oldNames.Any(n => r.Name.Contains(n)))
                .Select(r => r.Id)
                .ToListAsync();
            if (oldDsaRoadmaps.Count > 0)
            {
                var oldReviews = await _context.RoadmapReviews.Where(r => oldDsaRoadmaps.Contains(r.RoadmapId)).ToListAsync();
                if (oldReviews.Count > 0) _context.RoadmapReviews.RemoveRange(oldReviews);
                var oldEnrollments = await _context.RoadmapEnrollments.Where(e => oldDsaRoadmaps.Contains(e.RoadmapId)).ToListAsync();
                if (oldEnrollments.Count > 0) _context.RoadmapEnrollments.RemoveRange(oldEnrollments);
                var oldNodes = await _context.CustomNodes.Where(n => oldDsaRoadmaps.Contains(n.RoadmapId)).ToListAsync();
                if (oldNodes.Count > 0) _context.CustomNodes.RemoveRange(oldNodes);
                var oldRoadmaps = await _context.CustomRoadmaps.Where(r => oldDsaRoadmaps.Contains(r.Id)).ToListAsync();
                if (oldRoadmaps.Count > 0) _context.CustomRoadmaps.RemoveRange(oldRoadmaps);
                await _context.SaveChangesAsync();
            }

            // ── 1. Roadmap Sorting ──
            var sortingRoadmap = await EnsureRoadmapAsync(teacher.Id, "Thuật toán Sắp xếp (Sorting)",
                "Lộ trình trực quan hóa 6 thuật toán sắp xếp kinh điển: Bubble, Selection, Insertion, Quick, Merge, Heap — từ lý thuyết đến thực hành code.",
                "sorting");
            var sortingNodes = new[]
            {
                SortingNodeSeed(1, "Bubble Sort", "Sắp xếp nổi bọt: so sánh từng cặp liền kề, hoán đổi nếu sai thứ tự để phần tử lớn nhất 'nổi' về cuối.", "Easy", "bubble-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("1,2,3","1,2,3",false),
                    ("3,2,1","1,2,3",true), ("10,9,8,7,6,5,4,3,2,1","1,2,3,4,5,6,7,8,9,10",true), ("5","5",true)),
                SortingNodeSeed(2, "Selection Sort", "Sắp xếp chọn: mỗi lượt tìm phần tử nhỏ nhất trong phần chưa sắp và đưa về đầu dãy.", "Easy", "selection-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("4,2,2,8","2,2,4,8",false),
                    ("3,2,1","1,2,3",true), ("9,7,5,3,1","1,3,5,7,9",true), ("6","6",true)),
                SortingNodeSeed(3, "Insertion Sort", "Sắp xếp chèn: chèn từng phần tử vào đúng vị trí trong dãy con đã sắp xếp phía trước.", "Easy", "insertion-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("2,1","1,2",false),
                    ("3,2,1","1,2,3",true), ("1,2,3,4,5","1,2,3,4,5",true), ("5,4,3,2,1,0","0,1,2,3,4,5",true)),
                SortingNodeSeed(4, "Quick Sort", "Sắp xếp nhanh: chia để trị với pivot, phân hoạch mảng rồi đệ quy hai nửa. Trung bình O(n log n).", "Medium", "quick-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("3,2,1,4","1,2,3,4",false),
                    ("1,1,1,1","1,1,1,1",true), ("9,8,7,6,5,4,3,2,1","1,2,3,4,5,6,7,8,9",true), ("2","2",true)),
                SortingNodeSeed(5, "Merge Sort", "Sắp xếp trộn: chia đôi mảng liên tục, trộn hai nửa đã sắp xếp. Luôn O(n log n).", "Medium", "merge-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("2,1,3","1,2,3",false),
                    ("3,1,2,5,4","1,2,3,4,5",true), ("5,4,3,2,1","1,2,3,4,5",true), ("7","7",true)),
                SortingNodeSeed(6, "Heap Sort", "Sắp xếp đống: dựng max-heap rồi trích xuất gốc liên tục để đưa phần tử lớn nhất về cuối.", "Medium", "heap-sort", "7,3,9,1,5",
                    ("5,3,8,1,9,2,7","1,2,3,5,7,8,9",false), ("7,3,9,1,5","1,3,5,7,9",false), ("1,2,3","1,2,3",false),
                    ("3,2,1,4,5","1,2,3,4,5",true), ("9,7,5,3,1,8,6,4,2","1,2,3,4,5,6,7,8,9",true), ("6,6,6","6,6,6",true)),
            };
            foreach (var n in sortingNodes) await AddDsaNodeAsync(sortingRoadmap.Id, n, teacher.Id, "sorting-course");

            // ── 2. Roadmap Graph ──
            var graphRoadmap = await EnsureRoadmapAsync(teacher.Id, "Đồ thị & Đường đi ngắn nhất (Graph)",
                "Lộ trình trực quan hóa BFS, DFS, Dijkstra, Bellman-Ford trên sân chơi đồ thị tương tác — từ biểu diễn đến tối ưu đường đi.",
                "graph");
            var graphNodes = new[]
            {
                GraphNodeSeed(1, "Giới thiệu Đồ thị", "Biểu diễn đồ thị bằng danh sách kề (adjacency list) và ma trận kề; phân biệt có hướng/vô hướng, có trọng số.", "Easy", "bfs", "0-1,0-2,1-3,1-4,2-5,2-6",
                    ("0-1,0-2,1-3,1-4,2-5,2-6","0,1,2,3,4,5,6",false), ("0-1,0-2","0,1,2",false), ("0-1","0,1",false),
                    ("0-1,1-2,2-0","0,1,2",true), ("0-1,0-2,2-3","0,1,2,3",true)),
                GraphNodeSeed(2, "Duyệt BFS", "Duyệt theo chiều rộng: dùng hàng đợi, duyệt theo tầng. Tìm đường đi ngắn nhất trên đồ thị không trọng số.", "Medium", "bfs", "0-1,0-2,1-3,1-4,2-5,2-6",
                    ("0-1,0-2,1-3,1-4,2-5,2-6","0,1,2,3,4,5,6",false), ("0-1,0-2,1-3","0,1,2,3",false), ("0-1,1-2,2-3,3-4","0,1,2,3,4",false),
                    ("0-1,1-2,2-0,2-3","0,1,2,3",true), ("0-1,0-2,1-3,3-4","0,1,2,3,4",true)),
                GraphNodeSeed(3, "Duyệt DFS", "Duyệt theo chiều sâu: dùng ngăn xếp/đệ quy, đi sâu trước khi quay lui. Phát hiện chu trình và liên thông.", "Medium", "dfs", "0-1,0-2,1-3,1-4,2-5,2-6",
                    ("0-1,0-2,1-3,1-4,2-5,2-6","0,1,3,4,2,5,6",false), ("0-1,0-2,1-3","0,1,3,2",false), ("0-1,1-2,2-3","0,1,2,3",false),
                    ("0-1,1-2,2-0,2-3","0,1,2,3",true), ("0-1,0-2,1-3,3-4","0,1,3,4,2",true)),
                GraphNodeSeed(4, "Dijkstra", "Đường đi ngắn nhất đơn nguồn trên đồ thị có trọng số dương, dùng hàng đợi ưu tiên.", "Hard", "dijkstra", "0-1-4,0-2-2,1-2-5,1-3-10,2-3-3,3-4-1",
                    ("0-1-4,0-2-2,1-2-5,1-3-10,2-3-3,3-4-1","0,2,5,8,9",false), ("0-1-1,1-2-2,0-2-5","0,1,3",false), ("0-1-4,1-2-3,2-3-2","0,4,7,9",false),
                    ("0-1-10,0-2-3,2-1-4","0,7,3",true), ("0-1-2,1-2-2,2-3-2,0-3-9","0,2,4,6",true)),
                GraphNodeSeed(5, "Bellman-Ford", "Đường đi ngắn nhất hỗ trợ cạnh trọng số âm, phát hiện chu trình âm sau V-1 lần lặp.", "Hard", "bellman-ford", "0-1-4,0-2-2,1-2-5,1-3-10,2-3-3,3-4-1",
                    ("0-1-4,0-2-2,1-2-5,1-3-10,2-3-3,3-4-1","0,2,5,8,9",false), ("0-1-1,1-2-2,0-2-5","0,1,3",false), ("0-1-4,1-2-3,2-3-2","0,4,7,9",false),
                    ("0-1-5,1-2-3,2-3-1,3-1--2","0,5,8,9",true), ("0-1-2,1-2-2,2-3-2,0-3-9","0,2,4,6",true)),
                GraphNodeSeed(6, "Cây khung nhỏ nhất (Kruskal)", "Kruskal & Prim — nối tất cả đỉnh với tổng trọng số nhỏ nhất, dùng DSU để tránh chu trình.", "Hard", "kruskal", "0-1-2,0-2-3,1-2-1,1-3-1,2-3-4,3-4-2",
                    ("0-1-2,0-2-3,1-2-1,1-3-1,2-3-4,3-4-2","6",false), ("0-1-1,1-2-2,0-2-3","3",false), ("0-1-4,0-2-3,1-2-1","4",false),
                    ("0-1-2,0-2-3,1-2-1,1-3-1","4",true), ("0-1-5,1-2-4,2-3-3,0-3-2","9",true)),
            };
            foreach (var n in graphNodes) await AddDsaNodeAsync(graphRoadmap.Id, n, teacher.Id, "graph-course");

            // ── 3. Roadmap OOP & SOLID ──
            var oopRoadmap = await EnsureRoadmapAsync(teacher.Id, "Lập trình hướng đối tượng (OOP & SOLID)",
                "Lộ trình nắm vững 4 trụ cột OOP và 5 nguyên lý SOLID với bài giảng trực quan và thực hành thiết kế lớp.",
                "oop");
            var oopNodes = new[]
            {
                OopNodeSeed(1, "Encapsulation", "Đóng gói — ẩn chi tiết triển khai, bảo vệ dữ liệu qua access modifier public/private.", "Easy", "encapsulation", "public/private",
                    ("balance=100;deposit=50;withdraw=30","120",false), ("balance=0;deposit=200","200",false), ("balance=100;withdraw=150","Invalid",false),
                    ("balance=50;deposit=0;withdraw=50","0",true), ("balance=1000;deposit=500;withdraw=1200","300",true)),
                OopNodeSeed(2, "Inheritance", "Kế thừa — lớp con tái sử dụng thành viên của lớp cha, override hành vi khi cần.", "Medium", "inheritance", "Animal->Dog",
                    ("Animal;Dog;Bark","Animal sound;Dog sound;Woof",false), ("Animal;Cat;Meow","Animal sound;Cat sound;Meow",false), ("Animal","Animal sound",false),
                    ("Dog;Cat","Dog sound;Cat sound",true), ("Animal;Bird;Chirp","Animal sound;Bird sound;Chirp",true)),
                OopNodeSeed(3, "Polymorphism", "Đa hình — cùng phương thức nhưng hành vi khác nhau theo kiểu runtime (override, interface).", "Medium", "polymorphism", "Shape Draw",
                    ("Circle;Rectangle","Drawing Circle;Drawing Rectangle",false), ("Triangle","Drawing Triangle",false), ("Circle","Drawing Circle",false),
                    ("Rectangle;Triangle","Drawing Rectangle;Drawing Triangle",true), ("Circle;Circle","Drawing Circle;Drawing Circle",true)),
                OopNodeSeed(4, "Abstraction", "Trừu tượng hóa — chỉ phơi bày giao diện (interface/abstract), che giấu phần phức tạp.", "Medium", "abstraction", "Interface",
                    ("Paypal;Visa","Paypal pay;Visa pay",false), ("Momo","Momo pay",false), ("Bank","Bank pay",false),
                    ("Paypal;Momo","Paypal pay;Momo pay",true), ("Visa;Bank","Visa pay;Bank pay",true)),
                OopNodeSeed(5, "SOLID - SRP", "Single Responsibility — mỗi lớp chỉ đảm nhận duy nhất một trách nhiệm, một lý do để thay đổi.", "Hard", "solid-srp", "Single Responsibility",
                    ("email;John","Saved John;Email sent",false), ("email;Jane","Saved Jane;Email sent",false), ("none;Tom","Saved Tom",false),
                    ("sms;Ann","Saved Ann;SMS sent",true), ("email;","Saved ;Email sent",true)),
                OopNodeSeed(6, "SOLID - OCP", "Open/Closed — mở cho mở rộng, đóng cho sửa đổi; thêm hành vi qua interface/strategy.", "Hard", "solid-ocp", "Open/Closed",
                    ("Car;bike","Car move;Bike move",false), ("Plane","Plane move",false), ("Car","Car move",false),
                    ("Bike;Plane","Bike move;Plane move",true), ("Car;Plane","Car move;Plane move",true)),
                OopNodeSeed(7, "SOLID - LSP", "Liskov Substitution — lớp con phải thay thế được lớp cha mà không phá vỡ hành vi.", "Hard", "solid-lsp", "Liskov Substitution",
                    ("Rectangle 5x4;Square 4","Area 20;Area 16",false), ("Rectangle 3x2","Area 6",false), ("Square 5","Area 25",false),
                    ("Rectangle 2x8;Square 3","Area 16;Area 9",true), ("Square 7;Rectangle 1x1","Area 49;Area 1",true)),
                OopNodeSeed(8, "SOLID - DIP", "Dependency Inversion — module cấp cao phụ thuộc trừu tượng, không phụ thuộc chi tiết.", "Hard", "solid-dip", "Dependency Inversion",
                    ("Notifier;Email","Email notify",false), ("Notifier;SMS","SMS notify",false), ("Notifier;Push","Push notify",false),
                    ("Notifier;Email;SMS","Email notify;SMS notify",true), ("Notifier;Push;Push","Push notify;Push notify",true)),
                OopNodeSeed(9, "Design Patterns", "Strategy, Observer, Factory, Singleton — mẫu thiết kế phổ biến trong ứng dụng thực tế.", "Hard", "strategy", "Strategy Pattern",
                    ("Sort;quick","Quick sort",false), ("Sort;merge","Merge sort",false), ("Sort;bubble","Bubble sort",false),
                    ("Sort;quick;merge","Quick sort;Merge sort",true), ("Sort;bubble;quick","Bubble sort;Quick sort",true)),
            };
            foreach (var n in oopNodes) await AddDsaNodeAsync(oopRoadmap.Id, n, teacher.Id, "oop-course");

            await _context.SaveChangesAsync();
        }

        private async Task<CustomRoadmap> EnsureRoadmapAsync(Guid teacherId, string name, string description, string tags)
        {
            var roadmap = await _context.CustomRoadmaps.FirstOrDefaultAsync(r => r.Name == name);
            if (roadmap == null)
            {
                roadmap = new CustomRoadmap(teacherId, name, description, $"[\"{tags}\"]", null, "Public");
                _context.CustomRoadmaps.Add(roadmap);
                await _context.SaveChangesAsync();
            }
            roadmap.Approve();
            await _context.SaveChangesAsync();
            return roadmap;
        }

        // Tạo 1 node: rich theory + visualizer + quiz + codelab + lesson + moduleitem (cross-link Lab→Lesson).
        private async Task AddDsaNodeAsync(Guid roadmapId, DsaNodeSeed node, Guid teacherId, string courseKey)
        {
            var customNode = new CustomNode(roadmapId, node.Name, node.Description, node.Difficulty, node.Order);
            customNode.UpdateContent(BuildTheoryJson(node.Name, node.Difficulty, node.AlgoKey, node.SampleInput, node.Description), null, null);
            customNode.UpdateVisualizerConfig(BuildVisualizerConfig(node.AlgoKey, node.SampleInput));
            customNode.SetApproach(node.Approach, node.Solution, node.ComplexityNote);

            var quiz = await EnsureQuizAsync(node.QuizTitle, $"Bài trắc nghiệm về {node.Name}", node.QuizTopic, node.CodelabDifficulty, node.Xp);
            if (quiz.Questions.Count == 0)
            {
                foreach (var question in node.QuizQuestions)
                {
                    quiz.AddQuestion(question.Question, question.Options, question.CorrectIndex, question.Explanation);
                }
            }
            customNode.UpdatePractice(quiz.Id, null, null);

            var codelab = await EnsureCodelabAsync(node.CodelabTitle, node.CodelabDescription, node.InitialCode, node.CodelabDifficulty, node.Xp, node.TestCases);
            // LeetCodeId = null — chưa có entity LeetCode trong hệ thống (chỉ có field Guid? trên CustomNode).
            customNode.UpdatePractice(quiz.Id, codelab.Id, null);

            _context.CustomNodes.Add(customNode);
            await _context.SaveChangesAsync();

            var lesson = await EnsureLessonAsync(node.LessonTitle, node.LessonMd, node.Xp, teacherId);
            var course = await EnsureLessonCourseAsync(courseKey, teacherId, node.RoadmapCourseTitle);
            var module = await EnsureLessonCourseModuleAsync(course.Id);
            await EnsureLabLessonLinkAsync(module.Id, codelab.Id, lesson.Id, node.Order, node.Name);
        }

        private async Task<Codelab> EnsureCodelabAsync(string title, string description, string initialCode, int difficulty, int xp, List<(string Input, string Output, bool Hidden)> testCases)
        {
            var codelab = await _context.Codelabs.FirstOrDefaultAsync(c => c.Title == title);
            if (codelab == null)
            {
                codelab = new Codelab(title, description, initialCode, difficulty, xp, 3000, 128000000, "csharp", "Dữ liệu đầu vào theo định dạng trong mô tả.", "Xem ví dụ trong mô tả.", "dsa");

                // THÊM testcase vào collection TRƯỚC khi codelab vào context → toàn bộ graph ở trạng thái Added,
                // tránh EF đánh dấu Modified trên row chưa tồn tại (DbUpdateConcurrencyException như HANDOFF đã ghi).
                int idx = 1;
                foreach (var tc in testCases)
                {
                    codelab.TestCases.Add(new CodelabTestCase(codelab.Id, tc.Input, tc.Output, tc.Hidden, 10, idx++));
                }

                _context.Codelabs.Add(codelab);
                await _context.SaveChangesAsync();
            }
            return codelab;
        }

        private async Task<Lesson> EnsureLessonAsync(string title, string contentMd, int xpReward, Guid teacherId)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Title == title);
            if (lesson == null)
            {
                lesson = new Lesson(title, contentMd, "dsa", "{}", xpReward, teacherId);
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
            }
            return lesson;
        }

        private async Task<Course> EnsureLessonCourseAsync(string courseKey, Guid teacherId, string courseTitle)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Title == courseTitle);
            if (course == null)
            {
                var category = courseKey.StartsWith("graph") ? CourseCategory.Graph
                    : courseKey.StartsWith("oop") ? CourseCategory.OOP
                    : CourseCategory.Sorting;
                course = new Course(teacherId, courseTitle, $"Bài giảng lý thuyết liên kết roadmap — {courseTitle}", category, CourseDifficulty.Intermediate, false, "");
                course.Publish();
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
            }
            return course;
        }

        private async Task<CourseModule> EnsureLessonCourseModuleAsync(Guid courseId)
        {
            var module = await _context.CourseModules.FirstOrDefaultAsync(m => m.CourseId == courseId && m.Title == "Chương 1 — Bài lý thuyết");
            if (module == null)
            {
                module = new CourseModule(courseId, "Chương 1 — Bài lý thuyết", "Các bài giảng lý thuyết của lộ trình", 1000);
                _context.CourseModules.Add(module);
                await _context.SaveChangesAsync();
            }
            return module;
        }

        // ModuleItem cross-link: ItemType.Lesson + CodelabId + LessonId → đúng mapping
        // GetMyEnrollments / MarkRoadmapCompletedIfLastLessonAsync (ModuleItem(CodelabId==LabId) → LessonId).
        private async Task EnsureLabLessonLinkAsync(Guid moduleId, Guid codelabId, Guid lessonId, int order, string nodeName)
        {
            var existing = await _context.ModuleItems.FirstOrDefaultAsync(m => m.CodelabId == codelabId && m.LessonId == lessonId && !m.IsDeleted);
            if (existing != null) return;

            var item = new ModuleItem(moduleId, null, ModuleItemType.Lesson, lessonId, null, codelabId, nodeName, order * 1000, true);
            _context.ModuleItems.Add(item);
            await _context.SaveChangesAsync();
        }

        // ── Node seed builders ──
        private static DsaNodeSeed SortingNodeSeed(int order, string name, string desc, string difficulty, string algo, string sample,
            params (string Input, string Output, bool Hidden)[] testCases)
        {
            return new DsaNodeSeed
            {
                Order = order,
                Name = name,
                Description = desc,
                Difficulty = difficulty,
                AlgoKey = algo,
                SampleInput = sample,
                QuizTitle = $"Trắc nghiệm: {name}",
                QuizTopic = "sorting",
                QuizQuestions = SortingQuizQuestions(name),
                CodelabTitle = $"Codelab: {name}",
                CodelabDescription = $"Cài đặt thuật toán **{name}** bằng C#.\n\n- **Input:** mảng số nguyên phân cách bằng dấu phẩy (vd: `7,3,9,1,5`).\n- **Output:** mảng đã sắp xếp tăng dần, phân cách bằng dấu phẩy.\n\n### Ví dụ\n- Input: `7,3,9,1,5` → Output: `1,3,5,7,9`",
                InitialCode = SortingInitialCode(),
                CodelabDifficulty = difficulty == "Easy" ? 1 : difficulty == "Hard" ? 3 : 2,
                Xp = difficulty == "Easy" ? 30 : difficulty == "Hard" ? 80 : 50,
                TestCases = testCases.ToList(),
                LessonTitle = $"Bài giảng: {name}",
                LessonMd = SortingLessonMd(name, desc, sample),
                RoadmapCourseTitle = "Lộ trình Sắp xếp — Bài lý thuyết",
                Approach = $"Ý tưởng {name}: so sánh/hoán đổi theo chiến lược đặc trưng; bất biến 'sau mỗi lượt, một phần mảng đúng vị trí'.",
                Solution = "Xem mã giả trong lý thuyết; cài đặt in-place khi có thể.",
                ComplexityNote = SortingComplexityNote(name)
            };
        }

        private static DsaNodeSeed GraphNodeSeed(int order, string name, string desc, string difficulty, string algo, string sample,
            params (string Input, string Output, bool Hidden)[] testCases)
        {
            string initialCode = name.Contains("BFS") ? GraphBfsInitialCode()
                : name.Contains("DFS") ? GraphDfsInitialCode()
                : name.Contains("Dijkstra") ? DijkstraInitialCode()
                : name.Contains("Cây khung") ? KruskalInitialCode()
                : GraphBfsInitialCode();
            return new DsaNodeSeed
            {
                Order = order,
                Name = name,
                Description = desc,
                Difficulty = difficulty,
                AlgoKey = algo,
                SampleInput = sample,
                QuizTitle = $"Trắc nghiệm: {name}",
                QuizTopic = "graph",
                QuizQuestions = GraphQuizQuestions(name),
                CodelabTitle = $"Codelab: {name}",
                CodelabDescription = $"Cài đặt **{name}** bằng C#.\n\n- **Input:** danh sách cạnh dạng `u-v` (hoặc `u-v-w` nếu có trọng số), phân cách bằng dấu phẩy.\n- **Output:** theo mô tả từng bài (thứ tự duyệt / dist[] / tổng MST).\n\n### Ví dụ\n- Input: `0-1,0-2,1-3,1-4,2-5,2-6`",
                InitialCode = initialCode,
                CodelabDifficulty = difficulty == "Easy" ? 1 : difficulty == "Hard" ? 3 : 2,
                Xp = difficulty == "Easy" ? 30 : difficulty == "Hard" ? 80 : 50,
                TestCases = testCases.ToList(),
                LessonTitle = $"Bài giảng: {name}",
                LessonMd = GraphLessonMd(name, desc, sample),
                RoadmapCourseTitle = "Lộ trình Đồ thị — Bài lý thuyết",
                Approach = $"Ý tưởng {name}: lựa chọn cấu trúc phụ phù hợp (queue/stack/heap/dist) và duyệt/cập nhật trạng thái.",
                Solution = "Xem mã giả trong lý thuyết; đánh dấu visited để tránh lặp vô hạn.",
                ComplexityNote = "Xem bảng độ phức tạp trong lý thuyết."
            };
        }

        private static DsaNodeSeed OopNodeSeed(int order, string name, string desc, string difficulty, string algo, string sample,
            params (string Input, string Output, bool Hidden)[] testCases)
        {
            string initialCode = name == "Encapsulation" ? OopEncapsulationInitialCode()
                : name == "Polymorphism" ? OopPolymorphismInitialCode()
                : OopGenericInitialCode();
            return new DsaNodeSeed
            {
                Order = order,
                Name = name,
                Description = desc,
                Difficulty = difficulty,
                AlgoKey = algo,
                SampleInput = sample,
                QuizTitle = $"Trắc nghiệm: {name}",
                QuizTopic = "oop",
                QuizQuestions = OopQuizQuestions(name),
                CodelabTitle = $"Codelab: {name}",
                CodelabDescription = $"Thực hành **{name}** bằng C# — hoàn thiện lớp theo yêu cầu mô tả.\n\n- **Input:** các lệnh phân cách bằng `;` (vd: `balance=100;deposit=50`).\n- **Output:** kết quả theo từng bài.\n\n### Lưu ý\n- Đảm bảo dữ liệu private và chỉ truy cập qua phương thức công khai.",
                InitialCode = initialCode,
                CodelabDifficulty = difficulty == "Easy" ? 1 : difficulty == "Hard" ? 3 : 2,
                Xp = difficulty == "Easy" ? 30 : difficulty == "Hard" ? 80 : 50,
                TestCases = testCases.ToList(),
                LessonTitle = $"Bài giảng: {name}",
                LessonMd = OopLessonMd(name, desc),
                RoadmapCourseTitle = "Lộ trình OOP & SOLID — Bài lý thuyết",
                Approach = $"Ý tưởng {name}: áp dụng đúng khái niệm/nguyên lý vào thiết kế lớp, nhận diện vi phạm.",
                Solution = "Xem mã mẫu trong lý thuyết; giữ code đơn giản và đúng bất biến.",
                ComplexityNote = "Khái niệm thiết kế — không có độ phức tạp thời gian."
            };
        }

        private class DsaNodeSeed
        {
            public int Order { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Difficulty { get; set; } = "Medium";
            public string AlgoKey { get; set; } = "";
            public string SampleInput { get; set; } = "";
            public string QuizTitle { get; set; } = "";
            public string QuizTopic { get; set; } = "dsa";
            public List<QuizQuestionSeed> QuizQuestions { get; set; } = new();
            public string CodelabTitle { get; set; } = "";
            public string CodelabDescription { get; set; } = "";
            public string InitialCode { get; set; } = "";
            public int CodelabDifficulty { get; set; } = 1;
            public int Xp { get; set; } = 50;
            public List<(string Input, string Output, bool Hidden)> TestCases { get; set; } = new();
            public string LessonTitle { get; set; } = "";
            public string LessonMd { get; set; } = "";
            public string RoadmapCourseTitle { get; set; } = "";
            public string Approach { get; set; } = "";
            public string Solution { get; set; } = "";
            public string ComplexityNote { get; set; } = "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S6 — ENROLLMENTS + PROGRESS + REVIEWS (12 enrollment, 3 roadmap,
        // progress thật theo lesson/moduleitem, 5 review → avgRating hiển thị)
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedEnrollmentsProgressReviewsAsync()
        {
            var roadmapNames = new[] { "Thuật toán Sắp xếp (Sorting)", "Đồ thị & Đường đi ngắn nhất (Graph)", "Lập trình hướng đối tượng (OOP & SOLID)" };
            var roadmaps = await _context.CustomRoadmaps.Where(r => roadmapNames.Contains(r.Name)).ToListAsync();
            if (roadmaps.Count != 3) return;

            var sorting = roadmaps.First(r => r.Name.StartsWith("Thuật toán"));
            var graph = roadmaps.First(r => r.Name.StartsWith("Đồ thị"));
            var oop = roadmaps.First(r => r.Name.StartsWith("Lập trình"));

            // (roadmap, email, status, số node đã hoàn thành)
            var plans = new[]
            {
                (sorting, "reviewdemo@visualizationdsa.dev", "Completed", 6),
                (sorting, "levanc@visualizationdsa.dev", "Completed", 6),
                (sorting, "demo@visualizationdsa.dev", "Active", 2),
                (sorting, "hoangvane@visualizationdsa.dev", "Active", 1),
                (sorting, "vuthif@visualizationdsa.dev", "Dropped", 0),
                (graph, "nguyenvana@visualizationdsa.dev", "Completed", 6),
                (graph, "buithih@visualizationdsa.dev", "Completed", 6),
                (graph, "phamthid@visualizationdsa.dev", "Active", 3),
                (graph, "duongvanlam@visualizationdsa.dev", "Active", 1),
                (oop, "tranthib@visualizationdsa.dev", "Completed", 9),
                (oop, "dovani@visualizationdsa.dev", "Active", 2),
                (oop, "ngothimai@visualizationdsa.dev", "Active", 1),
            };

            foreach (var (roadmap, email, status, completedNodes) in plans)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) continue;

                var enrollment = await _context.RoadmapEnrollments.FirstOrDefaultAsync(e => e.UserId == user.Id && e.RoadmapId == roadmap.Id);
                if (enrollment == null)
                {
                    enrollment = new RoadmapEnrollment(user.Id, roadmap.Id);
                    _context.RoadmapEnrollments.Add(enrollment);
                }
                if (status == "Completed" && enrollment.Status != "Completed") enrollment.MarkCompleted();
                else if (status == "Dropped" && enrollment.Status == "Active") enrollment.Drop();

                await SeedProgressForRoadmapAsync(user, roadmap, completedNodes);
            }

            // ── Roadmap Reviews (1 user 1 review / roadmap) ──
            await EnsureReviewAsync("reviewdemo@visualizationdsa.dev", sorting.Id, 4);
            await EnsureReviewAsync("levanc@visualizationdsa.dev", sorting.Id, 5);
            await EnsureReviewAsync("nguyenvana@visualizationdsa.dev", graph.Id, 5);
            await EnsureReviewAsync("buithih@visualizationdsa.dev", graph.Id, 4);
            await EnsureReviewAsync("tranthib@visualizationdsa.dev", oop.Id, 5);

            await _context.SaveChangesAsync();
        }

        // Ghi progress cho các node đã hoàn thành của user: lesson Completed + module item Completed.
        private async Task SeedProgressForRoadmapAsync(User user, CustomRoadmap roadmap, int completedNodes)
        {
            if (completedNodes <= 0) return;

            var nodes = await _context.CustomNodes.Where(n => n.RoadmapId == roadmap.Id).OrderBy(n => n.SortOrder).ToListAsync();
            foreach (var node in nodes.Take(completedNodes))
            {
                if (!node.LabId.HasValue) continue;
                var item = await _context.ModuleItems.FirstOrDefaultAsync(m => m.CodelabId == node.LabId.Value && !m.IsDeleted);
                if (item == null || !item.LessonId.HasValue) continue;

                var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == item.LessonId.Value);
                var lessonProgress = await _context.UserLessonProgresses.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == item.LessonId.Value);
                if (lessonProgress == null)
                {
                    lessonProgress = new UserLessonProgress(user.Id, item.LessonId.Value, "Completed");
                    _context.UserLessonProgresses.Add(lessonProgress);
                }
                lessonProgress.MarkAsCompleted(lesson?.XPReward ?? 30);

                var itemProgress = await _context.UserModuleItemProgresses.FirstOrDefaultAsync(p => p.UserId == user.Id && p.ModuleItemId == item.Id);
                if (itemProgress == null)
                {
                    itemProgress = new UserModuleItemProgress(user.Id, item.Id);
                    _context.UserModuleItemProgresses.Add(itemProgress);
                }
                itemProgress.UpdateProgress(0, 100, true, 100);
            }
        }

        private async Task EnsureReviewAsync(string email, Guid roadmapId, int rating)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var exists = await _context.RoadmapReviews.AnyAsync(r => r.UserId == user.Id && r.RoadmapId == roadmapId);
            if (exists) return;

            _context.RoadmapReviews.Add(new RoadmapReview(user.Id, roadmapId, rating));
        }

        // G3.9 demo — roadmap nhỏ để E2E đánh giá: học hết node cuối (không Lab) → enrollment tự Completed.
        // Kèm user reviewdemo@ đã Completed roadmap này → đăng nhập thấy ngay nút "Đánh giá roadmap".
        private async Task SeedReviewDemoRoadmapAsync()
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher" || u.Role == "Admin");
            if (teacher == null) return;

            const string demoName = "Demo: Đánh giá Lộ trình (E2E)";
            var roadmap = await _context.CustomRoadmaps.FirstOrDefaultAsync(r => r.Name == demoName);
            if (roadmap == null)
            {
                roadmap = new CustomRoadmap(teacher.Id, demoName,
                    "Lộ trình demo 2 trạm — hoàn thành trạm cuối sẽ tự set enrollment Completed và mở khóa nút đánh giá sao (G3.9).",
                    "[]", null, "Public");
                roadmap.Approve();
                _context.CustomRoadmaps.Add(roadmap);
                await _context.SaveChangesAsync();

                var node1 = new CustomNode(roadmap.Id, "Trạm 1 — Khởi động", "Mở bài, đọc lý thuyết rồi bấm Hoàn thành.", "Easy", 1);
                node1.UpdateContent("[{\"type\":\"text\",\"content\":\"Chào mừng! Hãy bấm \\\"Hoàn thành bài học\\\" khi đã đọc xong.\"}]", null, null);
                node1.UpdateVisualizerConfig("{\"algorithm\":\"bubble-sort\",\"sampleInput\":\"5,3,8,1\",\"speed\":1}");
                _context.CustomNodes.Add(node1);

                var node2 = new CustomNode(roadmap.Id, "Trạm 2 — Hoàn thành", "Trạm cuối — hoàn thành để được phép đánh giá roadmap.", "Easy", 2);
                node2.UpdateContent("[{\"type\":\"text\",\"content\":\"Xong! Quay lại trang chi tiết lộ trình và bấm \\\"Đánh giá roadmap\\\".\"}]", null, null);
                node2.UpdateVisualizerConfig("{\"algorithm\":\"bubble-sort\",\"sampleInput\":\"8,3,5,1\",\"speed\":1}");
                _context.CustomNodes.Add(node2);

                await _context.SaveChangesAsync();
            }

            // reviewdemo@ đã hoàn thành roadmap demo → nút "Đánh giá roadmap" hiện sẵn khi demo.
            const string demoEmail = "reviewdemo@visualizationdsa.dev";
            var demoUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == demoEmail);

            var hasCompleted = demoUser != null && await _context.RoadmapEnrollments.AnyAsync(e => e.UserId == demoUser.Id && e.RoadmapId == roadmap.Id && e.Status == "Completed");
            if (demoUser != null && !hasCompleted)
            {
                var enrollment = new RoadmapEnrollment(demoUser.Id, roadmap.Id);
                enrollment.MarkCompleted();
                _context.RoadmapEnrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }
        }
    }
}

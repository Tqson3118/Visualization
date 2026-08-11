using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Application.Common.Interfaces;
using VisualizationDSA.Application.DTOs.RoadmapReview;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;

namespace VisualizationDSA.Infrastructure.Services
{
    public class RoadmapReviewService : IRoadmapReviewService
    {
        private readonly ApplicationDbContext _context;

        public RoadmapReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        // G3.9.3 — Đánh giá 1 lần duy nhất: đã đánh giá rồi → KHÔNG upsert/update.
        public async Task<RoadmapReviewResult> SubmitReviewAsync(Guid userId, Guid roadmapId, int rating)
        {
            if (rating < 1 || rating > 5)
                return new RoadmapReviewResult { IsSuccess = false, ErrorCode = "INVALID_RATING", ErrorMessage = "Rating phải từ 1 đến 5." };

            var roadmap = await _context.CustomRoadmaps.FirstOrDefaultAsync(r => r.Id == roadmapId);
            if (roadmap == null)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == roadmapId);
                if (course == null)
                    return new RoadmapReviewResult { IsSuccess = false, ErrorCode = "ROADMAP_NOT_FOUND", ErrorMessage = "Không tìm thấy lộ trình." };
            }

            var alreadyReviewed = await _context.RoadmapReviews
                .AnyAsync(r => r.UserId == userId && r.RoadmapId == roadmapId);
            if (alreadyReviewed)
                return new RoadmapReviewResult { IsSuccess = false, ErrorCode = "ALREADY_REVIEWED", ErrorMessage = "Bạn đã đánh giá roadmap này." };

            var completed = await _context.RoadmapEnrollments
                .AnyAsync(e => e.UserId == userId && e.RoadmapId == roadmapId && e.Status == "Completed");
            if (!completed)
                return new RoadmapReviewResult { IsSuccess = false, ErrorCode = "ROADMAP_NOT_COMPLETED", ErrorMessage = "Bạn cần hoàn thành roadmap trước khi đánh giá." };

            var review = new RoadmapReview(userId, roadmapId, rating);
            _context.RoadmapReviews.Add(review);
            await _context.SaveChangesAsync();

            return new RoadmapReviewResult
            {
                IsSuccess = true,
                Review = new RoadmapReviewDto
                {
                    Id = review.Id,
                    RoadmapId = review.RoadmapId,
                    Rating = review.Rating,
                    CreatedAt = review.CreatedAt
                }
            };
        }

        // G3.9.4 — Thống kê công khai (myRating/myCanReview chỉ khi có userId).
        public async Task<RoadmapStatsResult> GetStatsAsync(Guid roadmapId, Guid? userId)
        {
            var enrollCount = await _context.RoadmapEnrollments
                .CountAsync(e => e.RoadmapId == roadmapId && (e.Status == "Active" || e.Status == "Completed"));
            var completionCount = await _context.RoadmapEnrollments
                .CountAsync(e => e.RoadmapId == roadmapId && e.Status == "Completed");

            var reviews = await _context.RoadmapReviews
                .Where(r => r.RoadmapId == roadmapId)
                .Select(r => new { r.Rating, r.UserId })
                .ToListAsync();
            var reviewCount = reviews.Count;
            double? avgRating = null;
            if (reviewCount > 0)
                avgRating = Math.Round(reviews.Average(r => (double)r.Rating), 1);

            int? myRating = null;
            bool myCanReview = false;
            if (userId != null)
            {
                var myReview = reviews.FirstOrDefault(r => r.UserId == userId.Value);
                if (myReview != null) myRating = myReview.Rating;

                var completedEnrollment = await _context.RoadmapEnrollments
                    .AnyAsync(e => e.UserId == userId.Value && e.RoadmapId == roadmapId && e.Status == "Completed");
                myCanReview = completedEnrollment && myReview == null;
            }

            return new RoadmapStatsResult
            {
                EnrollCount = enrollCount,
                CompletionCount = completionCount,
                ReviewCount = reviewCount,
                AvgRating = avgRating,
                MyRating = myRating,
                MyCanReview = myCanReview
            };
        }

        public async Task<bool> RoadmapExistsAsync(Guid roadmapId)
        {
            var exists = await _context.CustomRoadmaps.AnyAsync(r => r.Id == roadmapId);
            if (exists) return true;
            return await _context.Courses.AnyAsync(c => c.Id == roadmapId);
        }

        // G3.9.2 — Đánh dấu roadmap Completed khi user hoàn thành bài CUỐI của roadmap.
        // 2 mapping:
        //  - lessonId là CustomNode.Id (roadmap node được học trực tiếp — luồng Thư viện Lộ trình)
        //  - lessonId là Lesson thật → ModuleItem (CodelabId == node.LabId) → LessonId (giống GetMyEnrollments)
        public async Task MarkRoadmapCompletedIfLastLessonAsync(Guid userId, Guid lessonId)
        {
            var node = await _context.CustomNodes
                .Include(n => n.Roadmap)
                .FirstOrDefaultAsync(n => n.Id == lessonId);
            if (node != null && node.Roadmap != null)
            {
                await MarkCompletedForNodeAsync(userId, node, node.Roadmap);
                return;
            }

            var linkedCodelabIds = await _context.ModuleItems
                .Where(m => m.LessonId == lessonId && m.CodelabId.HasValue && !m.IsDeleted)
                .Select(m => m.CodelabId!.Value)
                .ToListAsync();
            if (linkedCodelabIds.Count == 0) return;

            var nodes = await _context.CustomNodes
                .Where(n => n.LabId.HasValue && linkedCodelabIds.Contains(n.LabId.Value))
                .Include(n => n.Roadmap)
                .ToListAsync();

            foreach (var n in nodes)
            {
                if (n.Roadmap == null) continue;
                await MarkCompletedForNodeAsync(userId, n, n.Roadmap);
            }
        }

        private async Task MarkCompletedForNodeAsync(Guid userId, CustomNode node, CustomRoadmap roadmap)
        {
            var allNodes = await _context.CustomNodes
                .Where(n => n.RoadmapId == roadmap.Id)
                .OrderBy(n => n.SortOrder)
                .ToListAsync();
            if (allNodes.Count == 0) return;

            // Chỉ khi bài vừa xong thuộc node CUỐI (theo order)
            var lastNode = allNodes.Last();
            if (lastNode.Id != node.Id) return;

            // KHÔNG đánh dấu nhầm: mọi node có lesson liên kết phải đã Completed
            bool allLessonsDone = true;
            foreach (var n in allNodes)
            {
                if (!n.LabId.HasValue) continue;
                var lessonLink = await _context.ModuleItems
                    .FirstOrDefaultAsync(m => m.CodelabId == n.LabId.Value && !m.IsDeleted);
                if (lessonLink == null) continue;
                var lp = await _context.UserLessonProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonLink.LessonId);
                if (lp == null || lp.Status != "Completed")
                {
                    allLessonsDone = false;
                    break;
                }
            }
            if (!allLessonsDone) return;

            var enrollment = await _context.RoadmapEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.RoadmapId == roadmap.Id && e.Status == "Active");
            if (enrollment != null)
            {
                enrollment.MarkCompleted();
                await _context.SaveChangesAsync();
            }
        }
    }
}

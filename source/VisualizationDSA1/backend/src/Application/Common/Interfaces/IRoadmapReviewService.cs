using System;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs.RoadmapReview;

namespace VisualizationDSA.Application.Common.Interfaces
{
    public interface IRoadmapReviewService
    {
        Task<RoadmapReviewResult> SubmitReviewAsync(Guid userId, Guid roadmapId, int rating);
        Task<RoadmapStatsResult> GetStatsAsync(Guid roadmapId, Guid? userId);
        Task<bool> RoadmapExistsAsync(Guid roadmapId);
        Task MarkRoadmapCompletedIfLastLessonAsync(Guid userId, Guid lessonId);
    }
}

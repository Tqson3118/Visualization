using System;

namespace VisualizationDSA.Application.DTOs.RoadmapReview
{
    public class RoadmapReviewDto
    {
        public Guid Id { get; set; }
        public Guid RoadmapId { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoadmapReviewResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public RoadmapReviewDto? Review { get; set; }
    }

    public class RoadmapStatsResult
    {
        public int EnrollCount { get; set; }
        public int CompletionCount { get; set; }
        public int ReviewCount { get; set; }
        public double? AvgRating { get; set; }
        public int? MyRating { get; set; }
        public bool MyCanReview { get; set; }
    }
}

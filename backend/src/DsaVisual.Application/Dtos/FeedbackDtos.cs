namespace DsaVisual.Application.Dtos;

/// <summary>Gửi phản hồi — POST /feedback {lessonId, rating, comment?} (API_REFERENCE.md §4.15, FR-7.4).</summary>
public sealed class FeedbackRequest
{
    public int LessonId { get; set; }
    public int Rating { get; set; }        // 1-5
    public string? Comment { get; set; }   // ≤ 200
}

/// <summary>Điểm TB + đếm — GET /feedback?lessonId= (API_REFERENCE.md §4.15).</summary>
public sealed class FeedbackSummaryDto
{
    public int LessonId { get; set; }
    public double AvgRating { get; set; }
    public int Count { get; set; }
}

/// <summary>Gửi/chỉnh đánh giá bài học — POST /lessons/{id}/feedback (FR-7.4). LessonId lấy từ route.</summary>
public sealed class LessonFeedbackRequest
{
    public int Rating { get; set; }        // 1-5
    public string? Comment { get; set; }   // ≤ 1000
}

/// <summary>Kết quả lưu đánh giá — POST /lessons/{id}/feedback.</summary>
public sealed class FeedbackSavedDto
{
    public int LessonId { get; set; }
    public int Rating { get; set; }
}

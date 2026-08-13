namespace DsaVisual.Application.Dtos;

/// <summary>Thống kê hệ thống — GET /admin/stats (API_REFERENCE.md §4.10).</summary>
public sealed class StatsDto
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalTopics { get; set; }
    public int TotalLessons { get; set; }
    public int TotalExercises { get; set; }
    public int TotalSubmissions { get; set; }
    public int TotalCodeSubmissions { get; set; }
    public int TotalClasses { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalSimulations { get; set; }
    public int ActiveUsersToday { get; set; }
}

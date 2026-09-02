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

    // Mở rộng thống kê doanh thu & đơn hàng từ PremiumSubscriptions (NotMapped khỏi raw SQL query)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int TotalOrders { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public long TotalRevenue { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int PendingOrders { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CompletedOrders { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CancelledOrders { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<RoleDistributionDto> RoleDistribution { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<DailyRevenueDto> RevenueByDay { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<RecentOrderDto> RecentOrders { get; set; } = [];
}

public sealed class RoleDistributionDto
{
    public string Role { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class DailyRevenueDto
{
    public string Date { get; set; } = string.Empty;
    public long Revenue { get; set; }
    public int Orders { get; set; }
}

public sealed class RecentOrderDto
{
    public string Id { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? CompletedAt { get; set; }
}

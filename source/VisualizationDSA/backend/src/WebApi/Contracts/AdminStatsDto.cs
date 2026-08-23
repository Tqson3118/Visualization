using System;
using System.Collections.Generic;

namespace VisualizationDSA.WebApi.Contracts;

public sealed record AdminStatsDto
{
    public int TotalUsers { get; init; }
    public int TotalLessons { get; init; }
    public int TotalExercises { get; init; }
    public int TotalSimulations { get; init; }
    public int ActiveUsersToday { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public int PendingOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public IReadOnlyList<AdminRoleDistributionDto> RoleDistribution { get; init; } = Array.Empty<AdminRoleDistributionDto>();
    public IReadOnlyList<AdminRevenueDayDto> RevenueByDay { get; init; } = Array.Empty<AdminRevenueDayDto>();
    public IReadOnlyList<AdminRecentOrderDto> RecentOrders { get; init; } = Array.Empty<AdminRecentOrderDto>();
}

public sealed record AdminRoleDistributionDto(string Role, int Count);
public sealed record AdminRevenueDayDto(string Date, decimal Revenue, int Orders);
public sealed record AdminRecentOrderDto(
    string Id,
    string UserDisplayName,
    string Email,
    decimal Amount,
    string Status,
    string PaymentCode,
    string CreatedAt,
    string? CompletedAt);

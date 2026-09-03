using Asp.Versioning;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>Thống kê hệ thống — API_REFERENCE.md §4.10 (Admin).</summary>
[ApiVersion("1.0")]
[Route("api/v1/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController(AppDbContext db, IDateTimeProvider clock) : ApiControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IDateTimeProvider _clock = clock;

    [HttpGet("stats")]
    public async Task<ActionResult<StatsDto>> GetStats([FromQuery] string? period, CancellationToken ct)
    {
        // "Hôm nay" theo UTC+7 (ngày địa phương) — khớp cách GamificationService ghi LastActivityDate.
        var today = _clock.UtcNow.AddHours(7).Date;

        // perf#9: trước đây 13 × CountAsync = 13 round-trip tuần tự cho 1 endpoint → gộp thành
        // MỘT query (13 scalar subquery trong 1 SELECT — 1 round-trip). Cột alias khớp tên property
        // StatsDto (EF SqlQueryInterpolated map theo tên, case-insensitive). Role int theo enum
        // UserRole (0=Student, 1=Teacher, 3=Admin — Enums.cs); TeacherPending (2) đếm vào TotalUsers.
        // Tên bảng/cột khớp ToTable model (UserConfiguration/TopicConfiguration/... — schema do migration tạo).
        var stats = await _db.Database.SqlQuery<StatsDto>($"""
            SELECT
                (SELECT COUNT(*) FROM Users WHERE DeletedAt IS NULL) AS TotalUsers,
                (SELECT COUNT(*) FROM Users WHERE Role = 0 AND DeletedAt IS NULL) AS TotalStudents,
                (SELECT COUNT(*) FROM Users WHERE Role = 1 AND DeletedAt IS NULL) AS TotalTeachers,
                (SELECT COUNT(*) FROM Users WHERE Role = 3 AND DeletedAt IS NULL) AS TotalAdmins,
                (SELECT COUNT(*) FROM Topics WHERE DeletedAt IS NULL) AS TotalTopics,
                ((SELECT COUNT(*) FROM Lessons WHERE DeletedAt IS NULL) + (SELECT COUNT(*) FROM LearningPathNodes WHERE DeletedAt IS NULL AND ItemType != 0 AND LessonId IS NULL)) AS TotalLessons,
                (SELECT COUNT(*) FROM Exercises WHERE DeletedAt IS NULL) AS TotalExercises,
                (SELECT COUNT(*) FROM ExerciseSubmissions) AS TotalSubmissions,
                (SELECT COUNT(*) FROM CodeSubmissions) AS TotalCodeSubmissions,
                (SELECT COUNT(*) FROM Classes WHERE DeletedAt IS NULL) AS TotalClasses,
                (SELECT COUNT(*) FROM Favorites) AS TotalFavorites,
                (SELECT COUNT(DISTINCT SimulationKey) FROM LessonSimulations) AS TotalSimulations,
                (SELECT COUNT(*) FROM Users WHERE DeletedAt IS NULL AND LastActivityDate IS NOT NULL AND LastActivityDate >= {today}) AS ActiveUsersToday
            """).FirstAsync(ct);

        // 1. Phân bổ vai trò người dùng cho biểu đồ Donut
        stats.RoleDistribution =
        [
            new() { Role = "STUDENT", Count = stats.TotalStudents },
            new() { Role = "TEACHER", Count = stats.TotalTeachers },
            new() { Role = "ADMIN", Count = stats.TotalAdmins }
        ];

        // 2. Thống kê gói Pro & Doanh thu từ PremiumSubscriptions
        var subscriptions = await _db.PremiumSubscriptions.AsNoTracking().ToListAsync(ct);
        stats.TotalOrders = subscriptions.Count;
        stats.CompletedOrders = subscriptions.Count(s => s.Status == 0 || s.Status == 1);
        stats.CancelledOrders = subscriptions.Count(s => s.Status == 2);
        stats.PendingOrders = subscriptions.Count(s => s.Status == 2);

        stats.TotalRevenue = subscriptions
            .Where(s => s.Status == 0 || s.Status == 1)
            .Sum(s => (s.PlanId ?? "").Contains("yearly") ? 799000L : 99000L);

        // 3. Doanh thu và số đơn hàng theo khoảng thời gian thực tế (không dùng mock data)
        int daysCount = 7;
        if (period == "30d") daysCount = 30;
        else if (period == "all")
        {
            var earliestSubDate = subscriptions
                .Where(s => s.Status == 0 || s.Status == 1)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.CreatedAt.Date)
                .FirstOrDefault();

            if (earliestSubDate != default)
            {
                var diff = (today - earliestSubDate).Days + 1;
                daysCount = Math.Clamp(diff, 7, 90);
            }
            else
            {
                daysCount = 30;
            }
        }

        var dateRange = Enumerable.Range(0, daysCount)
            .Select(i => today.AddDays(-daysCount + 1 + i))
            .ToList();

        stats.RevenueByDay = dateRange.Select(day =>
        {
            var daySubs = subscriptions
                .Where(s => s.CreatedAt.Date == day.Date && (s.Status == 0 || s.Status == 1))
                .ToList();
            return new DailyRevenueDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                Revenue = daySubs.Sum(s => (s.PlanId ?? "").Contains("yearly") ? 799000L : 99000L),
                Orders = daySubs.Count
            };
        }).ToList();

        return Ok(stats);
    }
}

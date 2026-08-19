using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;
using VisualizationDSA.WebApi.Contracts;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 20 — GET /api/v1/admin/stats (AdminStatsView FE). KPI + revenue/orders thật.</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/admin")]
    [RequireJwtRole("Admin,Teacher")]
    public class AdminStatsController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public AdminStatsController(IApplicationDbContext ctx) { _ctx = ctx; }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var totalUsers = await _ctx.Users.AsNoTracking().CountAsync();
            var roleCounts = await _ctx.Users.AsNoTracking()
                .GroupBy(u => u.Role)
                .Select(group => new { Role = group.Key, Count = group.Count() })
                .OrderByDescending(item => item.Count)
                .ToListAsync();
            var roleDistribution = roleCounts
                .Select(item => new AdminRoleDistributionDto(item.Role, item.Count))
                .ToList();
            var totalLessons = await _ctx.Lessons.AsNoTracking().IgnoreQueryFilters().CountAsync(l => !l.IsDeleted);
            var totalExercises = await _ctx.Quizzes.AsNoTracking().IgnoreQueryFilters().CountAsync(q => !q.IsDeleted);
            var totalSimulations = 25; // catalog simulators
            var today = DateTime.UtcNow.Date;
            var activeUsersToday = await _ctx.Users.AsNoTracking().CountAsync(u => u.LastActivityDate != null && u.LastActivityDate.Value.Date == today);

            var orders = await _ctx.Set<Order>().AsNoTracking().Include(o => o.User).ToListAsync();
            var completed = orders.Where(o => o.Status == "Completed").ToList();
            var totalRevenue = completed.Sum(o => o.Amount);
            int days = 7;
            var revenueByDay = new List<AdminRevenueDayDto>();
            for (var i = days - 1; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                var dayOrders = orders.Where(o => o.CreatedAt.Date == day).ToList();
                revenueByDay.Add(new AdminRevenueDayDto(
                    day.ToString("yyyy-MM-dd"),
                    dayOrders.Where(o => o.Status == "Completed").Sum(o => o.Amount),
                    dayOrders.Count));
            }
            var recentOrders = completed
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new AdminRecentOrderDto(
                    o.Id.ToString(),
                    o.User?.Username ?? "—",
                    o.User?.Email ?? "",
                    o.Amount,
                    o.Status,
                    o.PaymentCode,
                    o.CreatedAt.ToString("o"),
                    o.CompletedAt?.ToString("o")))
                .ToList();

            return Ok(new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalLessons = totalLessons,
                TotalExercises = totalExercises,
                TotalSimulations = totalSimulations,
                ActiveUsersToday = activeUsersToday,
                TotalOrders = orders.Count,
                TotalRevenue = totalRevenue,
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                CompletedOrders = completed.Count,
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                RoleDistribution = roleDistribution,
                RevenueByDay = revenueByDay,
                RecentOrders = recentOrders,
            });
        }
    }
}

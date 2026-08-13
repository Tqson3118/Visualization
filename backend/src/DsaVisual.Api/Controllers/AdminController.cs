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
    public async Task<ActionResult<StatsDto>> GetStats(CancellationToken ct)
    {
        // "Hôm nay" theo UTC+7 (ngày địa phương) — khớp cách GamificationService ghi LastActivityDate.
        var today = _clock.UtcNow.AddHours(7).Date;

        var stats = new StatsDto
        {
            TotalUsers = await _db.Users.AsNoTracking().CountAsync(u => u.DeletedAt == null, ct),
            TotalStudents = await _db.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Student && u.DeletedAt == null, ct),
            TotalTeachers = await _db.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Teacher && u.DeletedAt == null, ct),
            TotalAdmins = await _db.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Admin && u.DeletedAt == null, ct),
            TotalTopics = await _db.Topics.AsNoTracking().CountAsync(t => t.DeletedAt == null, ct),
            TotalLessons = await _db.Lessons.AsNoTracking().CountAsync(l => l.DeletedAt == null, ct),
            TotalExercises = await _db.Exercises.AsNoTracking().CountAsync(e => e.DeletedAt == null, ct),
            TotalSubmissions = await _db.ExerciseSubmissions.AsNoTracking().CountAsync(ct),
            TotalCodeSubmissions = await _db.CodeSubmissions.AsNoTracking().CountAsync(ct),
            TotalClasses = await _db.Classes.AsNoTracking().CountAsync(c => c.DeletedAt == null, ct),
            TotalFavorites = await _db.Favorites.AsNoTracking().CountAsync(ct),
            // Số simulation: key mô phỏng duy nhất được gắn vào bài học (LessonSimulations.UNIQUE(LessonId, SimulationKey))
            TotalSimulations = await _db.LessonSimulations.AsNoTracking()
                .Select(ls => ls.SimulationKey)
                .Distinct()
                .CountAsync(ct),
            // Người dùng có hoạt động hôm nay: LastActivityDate >= ngày hôm nay (UTC+7, không xóa mềm)
            ActiveUsersToday = await _db.Users.AsNoTracking()
                .CountAsync(u => u.DeletedAt == null && u.LastActivityDate != null && u.LastActivityDate >= today, ct)
        };

        return Ok(stats);
    }
}

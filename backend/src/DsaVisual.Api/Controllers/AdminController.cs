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
                (SELECT COUNT(*) FROM Lessons WHERE DeletedAt IS NULL) AS TotalLessons,
                (SELECT COUNT(*) FROM Exercises WHERE DeletedAt IS NULL) AS TotalExercises,
                (SELECT COUNT(*) FROM ExerciseSubmissions) AS TotalSubmissions,
                (SELECT COUNT(*) FROM CodeSubmissions) AS TotalCodeSubmissions,
                (SELECT COUNT(*) FROM Classes WHERE DeletedAt IS NULL) AS TotalClasses,
                (SELECT COUNT(*) FROM Favorites) AS TotalFavorites,
                (SELECT COUNT(DISTINCT SimulationKey) FROM LessonSimulations) AS TotalSimulations,
                (SELECT COUNT(*) FROM Users WHERE DeletedAt IS NULL AND LastActivityDate IS NOT NULL AND LastActivityDate >= {today}) AS ActiveUsersToday
            """).FirstAsync(ct);

        return Ok(stats);
    }
}

using System.Globalization;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// ClassService thật theo SDD §7.3.16-7.3.18 (Module H) + API_REFERENCE.md §4.11.
/// Mã mời 6 ký tự A-Z0-9 tự sinh, unique (thử lại nếu trùng). Teacher sở hữu lớp (OwnerId);
/// Admin có thể chuyển quyền sở hữu qua PUT (v2.8). Báo cáo lớp chỉ tính ClassMembers hiện tại.
/// </summary>
public sealed class ClassService(
    AppDbContext db,
    IDateTimeProvider clock,
    ILogger<ClassService> logger) : IClassService
{
    private const string RoleTeacher = "TEACHER";
    private const string RoleAdmin = "ADMIN";
    private const string InviteAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int InviteCodeLength = 6;

    // ── Danh sách / CRUD ──────────────────────────────────────

    public async Task<Result<List<ClassDto>>> GetMyClassesAsync(int userId, string role, CancellationToken ct)
    {
        var isTeacherOrAdmin = IsTeacherOrAdmin(role);
        var classIds = isTeacherOrAdmin
            ? await db.Classes.AsNoTracking()
                .Where(c => c.OwnerId == userId && c.DeletedAt == null)
                .Select(c => c.Id)
                .ToListAsync(ct)
            : await db.ClassMembers.AsNoTracking()
                .Where(m => m.UserId == userId)
                .Select(m => m.ClassId)
                .ToListAsync(ct);

        if (classIds.Count == 0)
        {
            return Result<List<ClassDto>>.Ok([]);
        }

        var classes = await db.Classes.AsNoTracking()
            .Where(c => classIds.Contains(c.Id) && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var memberCounts = await db.ClassMembers.AsNoTracking()
            .Where(m => classIds.Contains(m.ClassId))
            .GroupBy(m => m.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, ct);

        return Result<List<ClassDto>>.Ok(classes.Select(c => ToDto(c, memberCounts.GetValueOrDefault(c.Id))).ToList());
    }

    public async Task<Result<ClassDto>> CreateAsync(int userId, ClassUpsertRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<ClassDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Tên lớp không được để trống", new() { ["name"] = ["Tên lớp không được để trống"] });
        }

        var classRoom = new Class
        {
            Name = request.Name.Trim(),
            Semester = request.Semester?.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = userId,
            Status = ParseStatus(request.Status),
            CreatedAt = clock.UtcNow
        };

        classRoom.InviteCode = await GenerateUniqueInviteCodeAsync(ct);
        db.Classes.Add(classRoom);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ClassId} created by user {UserId} (invite {Invite})",
            classRoom.Id, userId, classRoom.InviteCode);
        return Result<ClassDto>.Ok(ToDto(classRoom, 0));
    }

    public async Task<Result<ClassDetailDto>> GetByIdAsync(int userId, string role, int id, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        var canManage = CanManage(userId, role, classRoom);
        var isMember = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == id && m.UserId == userId, ct);
        if (!canManage && !isMember)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lớp này");
        }

        var owner = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == classRoom.OwnerId, ct);
        var members = await db.ClassMembers.AsNoTracking()
            .Where(m => m.ClassId == id)
            .OrderBy(m => m.JoinedAt)
            .Join(db.Users.AsNoTracking(), m => m.UserId, u => u.Id, (m, u) => new ClassMemberDto
            {
                UserId = u.Id,
                DisplayName = u.DisplayName,
                Email = canManage ? u.Email : EmailMasker.Mask(u.Email),
                JoinedAt = m.JoinedAt
            })
            .ToListAsync(ct);

        var assignments = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        var assignmentDtos = new List<ClassAssignmentDto>();
        foreach (var assignment in assignments)
        {
            string? title = null;
            if (assignment.LessonId is { } lessonId)
            {
                title = await db.Lessons.AsNoTracking()
                    .Where(l => l.Id == lessonId).Select(l => l.Title).FirstOrDefaultAsync(ct);
            }
            else if (assignment.ExerciseId is { } exerciseId)
            {
                title = await db.Exercises.AsNoTracking()
                    .Where(e => e.Id == exerciseId).Select(e => e.Title).FirstOrDefaultAsync(ct);
            }

            assignmentDtos.Add(new ClassAssignmentDto
            {
                Id = assignment.Id,
                LessonId = assignment.LessonId,
                ExerciseId = assignment.ExerciseId,
                Title = title,
                DueAt = assignment.DueAt,
                CreatedAt = assignment.CreatedAt
            });
        }

        return Result<ClassDetailDto>.Ok(new ClassDetailDto
        {
            Id = classRoom.Id,
            Name = classRoom.Name,
            InviteCode = classRoom.InviteCode,
            Semester = classRoom.Semester,
            Description = classRoom.Description,
            OwnerId = classRoom.OwnerId,
            OwnerName = owner?.DisplayName ?? string.Empty,
            Status = classRoom.Status.ToString().ToLowerInvariant(),
            CreatedAt = classRoom.CreatedAt,
            Members = members,
            Assignments = assignmentDtos
        });
    }

    public async Task<Result<ClassDto>> UpdateAsync(int userId, string role, int id, ClassUpsertRequest request, CancellationToken ct)
    {
        var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result<ClassDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền sửa lớp này");
        }

        if (request.OwnerId is { } newOwnerId && newOwnerId != classRoom.OwnerId)
        {
            // Admin chuyển quyền sở hữu (v2.8 — lớp mồ côi)
            if (!IsAdmin(role))
            {
                return Result<ClassDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin được chuyển quyền sở hữu lớp");
            }

            var newOwner = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == newOwnerId && u.DeletedAt == null, ct);
            if (newOwner is null)
            {
                return Result<ClassDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ sở hữu mới không tồn tại");
            }

            classRoom.OwnerId = newOwnerId;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            classRoom.Name = request.Name.Trim();
        }

        classRoom.Semester = request.Semester?.Trim();
        classRoom.Description = request.Description?.Trim();
        classRoom.Status = ParseStatus(request.Status);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ClassId} updated by user {UserId}", id, userId);
        var memberCount = await db.ClassMembers.AsNoTracking().CountAsync(m => m.ClassId == id, ct);
        return Result<ClassDto>.Ok(ToDto(classRoom, memberCount));
    }

    public async Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct)
    {
        var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xóa lớp này");
        }

        classRoom.DeletedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ClassId} soft-deleted by user {UserId}", id, userId);
        return Result.Ok();
    }

    // ── Thành viên ────────────────────────────────────────────

    public async Task<Result<ClassDetailDto>> JoinAsync(int userId, int id, JoinClassRequest request, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (classRoom.InviteCode != request.InviteCode.Trim().ToUpperInvariant())
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Mã mời không đúng", new() { ["inviteCode"] = ["Mã mời không đúng"] });
        }

        if (classRoom.Status != ClassStatus.Open)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Lớp đã đóng, không nhận thành viên mới", new() { ["inviteCode"] = ["Lớp đã đóng"] });
        }

        var duplicate = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == id && m.UserId == userId, ct);
        if (duplicate)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Bạn đã tham gia lớp này", new() { ["userId"] = ["Bạn đã tham gia lớp này"] });
        }

        db.ClassMembers.Add(new ClassMember { ClassId = id, UserId = userId, JoinedAt = clock.UtcNow });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} joined class {ClassId}", userId, id);
        return await GetByIdAsync(userId, RoleTeacher, id, ct);
    }

    public async Task<Result<ClassDetailDto>> AddMemberAsync(int userId, string role, int id, AddMemberRequest request, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var member = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);
        if (member is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Không tìm thấy người dùng với email này");
        }

        if (member.Role == UserRole.Teacher || member.Role == UserRole.Admin)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Chỉ thêm được tài khoản Student vào lớp", new() { ["email"] = ["Chỉ thêm được tài khoản Student vào lớp"] });
        }

        var duplicate = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == id && m.UserId == member.Id, ct);
        if (duplicate)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Sinh viên đã trong lớp", new() { ["email"] = ["Sinh viên đã trong lớp"] });
        }

        db.ClassMembers.Add(new ClassMember { ClassId = id, UserId = member.Id, JoinedAt = clock.UtcNow });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Member {MemberId} added to class {ClassId} by user {UserId}", member.Id, id, userId);
        return await GetByIdAsync(userId, role, id, ct);
    }

    public async Task<Result> RemoveMemberAsync(int userId, string role, int id, int memberUserId, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var member = await db.ClassMembers.FirstOrDefaultAsync(m => m.ClassId == id && m.UserId == memberUserId, ct);
        if (member is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Thành viên không tồn tại trong lớp");
        }

        db.ClassMembers.Remove(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Member {MemberId} removed from class {ClassId} by user {UserId}", memberUserId, id, userId);
        return Result.Ok();
    }

    // ── Bài gán ───────────────────────────────────────────────

    public async Task<Result<ClassDetailDto>> AddAssignmentAsync(int userId, string role, int id, ClassAssignmentUpsertRequest request, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        if (request.LessonId is null && request.ExerciseId is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Phải gán ít nhất bài học hoặc bài tập", new() { ["lessonId"] = ["Phải gán ít nhất bài học hoặc bài tập"] });
        }

        if (request.LessonId is { } lessonId)
        {
            var lesson = await db.Lessons.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
            if (lesson is null)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
            }
        }

        if (request.ExerciseId is { } exerciseId)
        {
            var exercise = await db.Exercises.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciseId && e.DeletedAt == null, ct);
            if (exercise is null)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
            }
        }

        db.ClassAssignments.Add(new ClassAssignment
        {
            ClassId = id,
            LessonId = request.LessonId,
            ExerciseId = request.ExerciseId,
            DueAt = request.DueAt,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Assignment added to class {ClassId} by user {UserId}", id, userId);
        return await GetByIdAsync(userId, role, id, ct);
    }

    public async Task<Result> UpdateAssignmentAsync(int userId, string role, int id, int assignId, ClassAssignmentUpdateRequest request, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var assignment = await db.ClassAssignments
            .FirstOrDefaultAsync(a => a.Id == assignId && a.ClassId == id, ct);
        if (assignment is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài gán không tồn tại");
        }

        assignment.DueAt = request.DueAt;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Assignment {AssignId} updated in class {ClassId} by user {UserId}", assignId, id, userId);
        return Result.Ok();
    }

    public async Task<Result> RemoveAssignmentAsync(int userId, string role, int id, int assignId, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var assignment = await db.ClassAssignments
            .FirstOrDefaultAsync(a => a.Id == assignId && a.ClassId == id, ct);
        if (assignment is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài gán không tồn tại");
        }

        db.ClassAssignments.Remove(assignment);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Assignment {AssignId} removed from class {ClassId} by user {UserId}", assignId, id, userId);
        return Result.Ok();
    }

    // ── Báo cáo lớp ───────────────────────────────────────────

    public async Task<Result<ClassReportDto>> GetReportAsync(int userId, string role, int id, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassReportDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result<ClassReportDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem báo cáo lớp này");
        }

        var memberIds = await db.ClassMembers.AsNoTracking()
            .Where(m => m.ClassId == id)
            .Select(m => m.UserId)
            .ToListAsync(ct);
        var totalMembers = memberIds.Count;

        var assignments = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .OrderBy(a => a.DueAt)
            .ToListAsync(ct);

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var submissions = assignmentIds.Count > 0
            ? await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.ClassAssignmentId != null && assignmentIds.Contains(s.ClassAssignmentId.Value))
                .ToListAsync(ct)
            : [];

        var reportAssignments = new List<ClassReportAssignmentDto>();
        foreach (var assignment in assignments)
        {
            var assignmentSubmissions = submissions.Where(s => s.ClassAssignmentId == assignment.Id).ToList();
            var onTime = assignmentSubmissions.Count(s => s.SubmittedAt <= (assignment.DueAt ?? DateTime.MaxValue));
            var late = assignmentSubmissions.Count - onTime;
            var notSubmitted = totalMembers - assignmentSubmissions.Select(s => s.UserId).Distinct().Count();
            var avg = assignmentSubmissions.Count > 0 ? assignmentSubmissions.Average(s => (double)s.Score) : 0;

            var title = assignment.LessonId is { } lessonId
                ? await db.Lessons.AsNoTracking().Where(l => l.Id == lessonId).Select(l => l.Title).FirstOrDefaultAsync(ct)
                : assignment.ExerciseId is { } exerciseId
                    ? await db.Exercises.AsNoTracking().Where(e => e.Id == exerciseId).Select(e => e.Title).FirstOrDefaultAsync(ct)
                    : null;

            reportAssignments.Add(new ClassReportAssignmentDto
            {
                AssignmentId = assignment.Id,
                Title = title ?? string.Empty,
                DueAt = assignment.DueAt,
                OnTime = onTime,
                Late = late,
                NotSubmitted = Math.Max(0, notSubmitted),
                AvgScore = Math.Round(avg, 1)
            });
        }

        // Học viên chậm tiến độ: thiếu ≥ 2 bài gán (FR-8.4)
        var submittedByUser = submissions.GroupBy(s => s.UserId).ToDictionary(g => g.Key, g => g.Count());
        var lagging = memberIds
            .Select(memberId => new
            {
                MemberId = memberId,
                Missing = assignments.Count - submittedByUser.GetValueOrDefault(memberId)
            })
            .Where(x => x.Missing >= 2)
            .OrderByDescending(x => x.Missing)
            .Take(10)
            .ToList();

        var users = await db.Users.AsNoTracking()
            .Where(u => lagging.Select(l => l.MemberId).Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        return Result<ClassReportDto>.Ok(new ClassReportDto
        {
            ClassId = classRoom.Id,
            ClassName = classRoom.Name,
            TotalMembers = totalMembers,
            Assignments = reportAssignments,
            LaggingLearners = lagging.Select(l => new LaggingLearnerDto
            {
                UserId = l.MemberId,
                DisplayName = users.GetValueOrDefault(l.MemberId) ?? "Người dùng đã xóa",
                MissingCount = l.Missing
            }).ToList()
        });
    }

    public async Task<Result<CsvFileDto>> ExportReportCsvAsync(int userId, string role, int id, CancellationToken ct)
    {
        var report = await GetReportAsync(userId, role, id, ct);
        if (!report.IsSuccess)
        {
            return Result<CsvFileDto>.Fail(report.ErrorCode!, report.ErrorMessage!, report.FieldErrors!);
        }

        var data = report.Value!;
        var sb = new StringBuilder();
        sb.AppendLine("ClassId,ClassName,TotalMembers");
        sb.AppendLine($"{data.ClassId},{Csv(data.ClassName)},{data.TotalMembers}");
        sb.AppendLine("AssignmentId,Title,DueAt,OnTime,Late,NotSubmitted,AvgScore");
        foreach (var assignment in data.Assignments)
        {
            sb.AppendLine($"{assignment.AssignmentId},{Csv(assignment.Title)},{assignment.DueAt:O},{assignment.OnTime},{assignment.Late},{assignment.NotSubmitted},{assignment.AvgScore.ToString(CultureInfo.InvariantCulture)}");
        }

        sb.AppendLine("LaggingLearnerId,DisplayName,MissingCount");
        foreach (var learner in data.LaggingLearners)
        {
            sb.AppendLine($"{learner.UserId},{Csv(learner.DisplayName)},{learner.MissingCount}");
        }

        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return Result<CsvFileDto>.Ok(new CsvFileDto
        {
            FileName = $"report_class_{data.ClassId}_{clock.UtcNow:yyyyMMdd}.csv",
            Content = content
        });
    }

    // ── Private ───────────────────────────────────────────────

    private async Task<string> GenerateUniqueInviteCodeAsync(CancellationToken ct)
    {
        var random = new Random();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = new string(Enumerable.Range(0, InviteCodeLength)
                .Select(_ => InviteAlphabet[random.Next(InviteAlphabet.Length)])
                .ToArray());
            var exists = await db.Classes.AsNoTracking().AnyAsync(c => c.InviteCode == code, ct);
            if (!exists)
            {
                return code;
            }
        }

        // Fallback hiếm gặp: mã kèm thời gian
        return Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }

    private async Task<bool> EnsureCanManageAsync(int userId, string role, int classId, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classId && c.DeletedAt == null, ct);
        return classRoom is not null && CanManage(userId, role, classRoom);
    }

    private static bool CanManage(int userId, string role, Class classRoom) =>
        IsAdmin(role) || classRoom.OwnerId == userId;

    private static bool IsTeacherOrAdmin(string role) =>
        role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) ||
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private static bool IsAdmin(string role) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private static ClassStatus ParseStatus(string? status) =>
        status is not null && Enum.TryParse<ClassStatus>(status, true, out var parsed) ? parsed : ClassStatus.Open;

    private static ClassDto ToDto(Class classRoom, int memberCount) => new()
    {
        Id = classRoom.Id,
        Name = classRoom.Name,
        InviteCode = classRoom.InviteCode,
        Semester = classRoom.Semester,
        Description = classRoom.Description,
        OwnerId = classRoom.OwnerId,
        Status = classRoom.Status.ToString().ToLowerInvariant(),
        MemberCount = memberCount,
        CreatedAt = classRoom.CreatedAt
    };

    private static string Csv(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}

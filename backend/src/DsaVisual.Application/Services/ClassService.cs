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

        var pathIds = classes.Where(c => c.LearningPathId != null).Select(c => c.LearningPathId!.Value).Distinct().ToList();
        var pathTitles = pathIds.Count > 0
            ? await db.LearningPaths.AsNoTracking()
                .Where(p => pathIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Title, ct)
            : new Dictionary<int, string>();

        return Result<List<ClassDto>>.Ok(classes.Select(c => ToDto(c, memberCounts.GetValueOrDefault(c.Id), c.LearningPathId.HasValue ? pathTitles.GetValueOrDefault(c.LearningPathId.Value) : null)).ToList());
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
            LearningPathId = request.LearningPathId,
            CreatedAt = clock.UtcNow
        };

        if (request.LearningPathId.HasValue)
        {
            var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.LearningPathId.Value, ct);
            if (path != null)
            {
                classRoom.CurriculumTitle = path.Title;
                classRoom.CurriculumDescription = path.Description;
                classRoom.CurriculumPublished = true;
            }
        }

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

        string? pathTitle = null;
        if (classRoom.LearningPathId is { } pid)
        {
            pathTitle = await db.LearningPaths.AsNoTracking()
                .Where(p => p.Id == pid)
                .Select(p => p.Title)
                .FirstOrDefaultAsync(ct);
        }

        // Curriculum: lấy theo thứ tự lộ trình (SortOrder), fallback CreatedAt cho legacy.
        var assignments = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

        // Draft gating: học viên (không quản lý) KHÔNG thấy items khi lộ trình chưa publish.
        var showCurriculumToCaller = canManage || classRoom.CurriculumPublished;

        // findings-perf #1 (N+1): gom 2 batch query title (lessonIds/exerciseIds → ToDictionary) trước vòng lặp
        var lessonIds = assignments.Where(a => a.LessonId != null).Select(a => a.LessonId!.Value).Distinct().ToList();
        var exerciseIds = assignments.Where(a => a.ExerciseId != null).Select(a => a.ExerciseId!.Value).Distinct().ToList();
        var lessonTitles = lessonIds.Count > 0
            ? await db.Lessons.AsNoTracking()
                .Where(l => lessonIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Title, ct)
            : new Dictionary<int, string>();
        var exerciseTitles = exerciseIds.Count > 0
            ? await db.Exercises.AsNoTracking()
                .Where(e => exerciseIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.Title, ct)
            : new Dictionary<int, string>();

        var assignmentDtos = new List<ClassAssignmentDto>();
        foreach (var assignment in assignments.OrderBy(a => a.SortOrder).ThenBy(a => a.CreatedAt))
        {
            // Draft gating: bỏ qua toàn bộ items với caller không phải manager khi chưa publish.
            if (!showCurriculumToCaller)
            {
                break;
            }

            string? title = null;
            if (assignment.LessonId is { } lessonId)
            {
                title = lessonTitles.GetValueOrDefault(lessonId);
            }
            else if (assignment.ExerciseId is { } exerciseId)
            {
                title = exerciseTitles.GetValueOrDefault(exerciseId);
            }

            assignmentDtos.Add(new ClassAssignmentDto
            {
                Id = assignment.Id,
                LessonId = assignment.LessonId,
                ExerciseId = assignment.ExerciseId,
                Title = title,
                DueAt = assignment.DueAt,
                AllowLateSubmission = assignment.AllowLateSubmission,
                SortOrder = assignment.SortOrder,
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
            LearningPathId = classRoom.LearningPathId,
            LearningPathTitle = pathTitle,
            CreatedAt = classRoom.CreatedAt,
            CurriculumTitle = classRoom.CurriculumTitle ?? pathTitle,
            CurriculumDescription = classRoom.CurriculumDescription,
            CurriculumPublished = classRoom.CurriculumPublished,
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
        if (request.Status is not null)
        {
            classRoom.Status = ParseStatus(request.Status);
        }
        if (request.LearningPathId.HasValue)
        {
            classRoom.LearningPathId = request.LearningPathId.Value;
        }
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

    public async Task<Result<ClassDetailDto>> JoinAsync(int userId, string role, int id, JoinClassRequest request, CancellationToken ct)
    {
        if (role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) || role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ tài khoản Sinh viên mới có thể tham gia lớp học qua mã mời");
        }

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

        try
        {
            db.ClassMembers.Add(new ClassMember { ClassId = id, UserId = userId, JoinedAt = clock.UtcNow });
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.CONFLICT, "Bạn đã tham gia lớp này", new() { ["userId"] = ["Bạn đã tham gia lớp này"] });
        }

        logger.LogInformation("User {UserId} joined class {ClassId}", userId, id);
        return await GetByIdAsync(userId, role, id, ct);
    }

    /// <summary>
    /// v2.15 (Vấn đề 14/4.1): tham gia lớp bằng mã mời — tìm Class theo InviteCode, kiểm tra
    /// Status == Open, thêm vào ClassMembers. Sinh viên không cần biết classId trước.
    /// </summary>
    public async Task<Result<ClassDetailDto>> JoinByCodeAsync(int userId, string role, JoinClassByCodeRequest request, CancellationToken ct)
    {
        if (role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) || role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ tài khoản Sinh viên mới có thể tham gia lớp học qua mã mời");
        }

        if (string.IsNullOrWhiteSpace(request.InviteCode))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Mã mời không được để trống", new() { ["inviteCode"] = ["Mã mời không được để trống"] });
        }

        var code = request.InviteCode.Trim().ToUpperInvariant();
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.InviteCode == code && c.DeletedAt == null, ct);

        if (classRoom is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND,
                "Không tìm thấy lớp học với mã mời này", new() { ["inviteCode"] = ["Không tìm thấy lớp học với mã mời này"] });
        }

        if (classRoom.Status != ClassStatus.Open)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Lớp đã đóng, không nhận thành viên mới", new() { ["inviteCode"] = ["Lớp đã đóng"] });
        }

        var duplicate = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == classRoom.Id && m.UserId == userId, ct);
        if (duplicate)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Bạn đã tham gia lớp này", new() { ["inviteCode"] = ["Bạn đã tham gia lớp này"] });
        }

        try
        {
            db.ClassMembers.Add(new ClassMember { ClassId = classRoom.Id, UserId = userId, JoinedAt = clock.UtcNow });
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.CONFLICT, "Bạn đã tham gia lớp này", new() { ["inviteCode"] = ["Bạn đã tham gia lớp này"] });
        }

        logger.LogInformation("User {UserId} joined class {ClassId} by invite code {InviteCode}", userId, classRoom.Id, code);
        return await GetByIdAsync(userId, role, classRoom.Id, ct);
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

        try
        {
            db.ClassMembers.Add(new ClassMember { ClassId = id, UserId = member.Id, JoinedAt = clock.UtcNow });
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.CONFLICT, "Sinh viên đã trong lớp", new() { ["email"] = ["Sinh viên đã trong lớp"] });
        }

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

    public async Task<Result<List<ClassMemberDto>>> GetMembersAsync(int userId, string role, int id, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<List<ClassMemberDto>>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        var canManage = CanManage(userId, role, classRoom);
        var isMember = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == id && m.UserId == userId, ct);
        if (!canManage && !isMember)
        {
            return Result<List<ClassMemberDto>>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem danh sách thành viên lớp này");
        }

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

        return Result<List<ClassMemberDto>>.Ok(members);
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

        // Curriculum: item mới xếp cuối lộ trình (sortOrder = max + 1)
        var maxSortOrder = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .MaxAsync(a => (int?)a.SortOrder, ct) ?? -1;

        db.ClassAssignments.Add(new ClassAssignment
        {
            ClassId = id,
            LessonId = request.LessonId,
            ExerciseId = request.ExerciseId,
            DueAt = request.DueAt,
            AllowLateSubmission = request.AllowLateSubmission,
            SortOrder = maxSortOrder + 1,
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

        // v2.15: cho phép GV bật/tắt nhận bài muộn
        if (request.AllowLateSubmission is { } allowLate)
        {
            assignment.AllowLateSubmission = allowLate;
        }

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

    public async Task<Result<List<ClassAssignmentDto>>> GetAssignmentsAsync(int userId, string role, int id, CancellationToken ct)
    {
        var detailResult = await GetByIdAsync(userId, role, id, ct);
        if (!detailResult.IsSuccess)
        {
            return Result<List<ClassAssignmentDto>>.Fail(detailResult.ErrorCode!, detailResult.ErrorMessage!, detailResult.FieldErrors!);
        }

        return Result<List<ClassAssignmentDto>>.Ok(detailResult.Value!.Assignments);
    }

    // ── Lộ trình học (Curriculum) ────────────────────────────

    public async Task<Result<ClassDetailDto>> UpdateCurriculumAsync(int userId, string role, int id, ClassCurriculumUpsertRequest request, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (request.Title is not null)
        {
            var title = request.Title.Trim();
            if (title.Length > 200)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Tiêu đề lộ trình tối đa 200 ký tự", new() { ["title"] = ["Tiêu đề lộ trình tối đa 200 ký tự"] });
            }

            classRoom.CurriculumTitle = string.IsNullOrEmpty(title) ? null : title;
        }

        if (request.Description is not null)
        {
            var description = request.Description.Trim();
            if (description.Length > 500)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Mô tả lộ trình tối đa 500 ký tự", new() { ["description"] = ["Mô tả lộ trình tối đa 500 ký tự"] });
            }

            classRoom.CurriculumDescription = string.IsNullOrEmpty(description) ? null : description;
        }

        if (request.Published is { } published)
        {
            classRoom.CurriculumPublished = published;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ClassId} curriculum updated by user {UserId} (Title: {Title}, Published: {Published})",
            id, userId, classRoom.CurriculumTitle, classRoom.CurriculumPublished);
        return await GetByIdAsync(userId, role, id, ct);
    }

    public async Task<Result> ReorderCurriculumAsync(int userId, string role, int id, ClassCurriculumReorderRequest request, CancellationToken ct)
    {
        if (!await EnsureCanManageAsync(userId, role, id, ct))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var items = request.Items.DistinctBy(i => i.AssignmentId).ToList();
        if (items.Count == 0)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Danh sách thứ tự không được rỗng",
                new() { ["items"] = ["Danh sách thứ tự không được rỗng"] });
        }

        var assignmentIds = items.Select(i => i.AssignmentId).ToList();
        var assignments = await db.ClassAssignments
            .Where(a => a.ClassId == id && assignmentIds.Contains(a.Id))
            .ToListAsync(ct);

        if (assignments.Count != items.Count)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Một số bài gán không thuộc lớp này");
        }

        foreach (var item in items)
        {
            var assignment = assignments.First(a => a.Id == item.AssignmentId);
            assignment.SortOrder = item.SortOrder;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ClassId} curriculum reordered by user {UserId} ({Count} items)", id, userId, items.Count);
        return Result.Ok();
    }

    public async Task<Result<ClassDetailDto>> SetLearningPathAsync(int userId, string role, int id, int? learningPathId, CancellationToken ct)
    {
        var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        if (learningPathId.HasValue)
        {
            // Không AsNoTracking: có thể nâng cấp Visibility Private → ClassOnly ngay trong giao dịch này
            var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Id == learningPathId.Value, ct);
            if (path is null)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
            }

            // D5 fix: chỉ Visibility quyết định khả năng gán (không còn miễn qua Status Active/ClassOnly).
            // Public/ClassOnly: Teacher/Admin quản lý lớp đều được gán.
            // Private: chỉ chủ sở hữu path (CreatedBy/AuthorId) hoặc Admin — cấm mượn lộ trình riêng tư của Teacher khác.
            var isPathOwner = path.CreatedBy == userId || path.AuthorId == userId;
            var canUsePath = path.Visibility == PathVisibility.Public
                || path.Visibility == PathVisibility.ClassOnly
                || isPathOwner
                || IsAdmin(role);
            if (!canUsePath)
            {
                return Result<ClassDetailDto>.Fail(ErrorCodes.FORBIDDEN,
                    "Lộ trình đang ở chế độ riêng tư, chỉ tác giả hoặc quản trị viên mới có thể gán lộ trình này cho lớp học");
            }

            // Chủ sở hữu/Admin gán lộ trình Private → tự nâng lên ClassOnly để lớp học thấy được nội dung.
            if (path.Visibility == PathVisibility.Private)
            {
                path.Visibility = PathVisibility.ClassOnly;
                logger.LogInformation(
                    "Path {PathId} auto-upgraded visibility Private → ClassOnly when assigned to Class {ClassId} by user {UserId}",
                    path.Id, id, userId);
            }

            classRoom.LearningPathId = learningPathId.Value;
            if (string.IsNullOrWhiteSpace(classRoom.CurriculumTitle))
            {
                classRoom.CurriculumTitle = path.Title;
                classRoom.CurriculumDescription = path.Description;
            }
            classRoom.CurriculumPublished = true;
        }
        else
        {
            classRoom.LearningPathId = null;
        }

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(userId, role, id, ct);
    }

    public async Task<Result> UpdateAssignmentDeadlineAsync(int userId, string role, int id, int pathItemId, DateTime? dueAt, bool allowLateSubmission, CancellationToken ct)
    {
        if (dueAt.HasValue && dueAt.Value <= DateTime.UtcNow)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Hạn nộp bài phải ở tương lai");
        }

        var classRoom = await db.Classes.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == pathItemId && n.PathId == classRoom.LearningPathId, ct);
        if (node is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Mục lộ trình không thuộc lớp học này");
        }

        // Fetch all nodes in the path to support folder DFS propagation
        var allNodes = await db.LearningPathNodes
            .Where(n => n.PathId == classRoom.LearningPathId)
            .ToListAsync(ct);

        // Collect node itself and all descendants DFS
        var targetNodeIds = new HashSet<int> { node.Id };
        var queue = new Queue<int>();
        queue.Enqueue(node.Id);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allNodes.Where(n => n.ParentId == currentId).ToList();
            foreach (var child in children)
            {
                if (targetNodeIds.Add(child.Id))
                {
                    queue.Enqueue(child.Id);
                }
            }
        }

        var targetNodes = allNodes.Where(n => targetNodeIds.Contains(n.Id)).ToList();

        // Fetch existing overlay assignments for this class
        var existingAssignments = await db.ClassAssignments
            .Where(a => a.ClassId == id && !a.Archived)
            .ToListAsync(ct);

        foreach (var targetNode in targetNodes)
        {
            var overlay = existingAssignments.FirstOrDefault(a =>
                a.PathItemId == targetNode.Id ||
                (targetNode.LessonId != null && a.LessonId == targetNode.LessonId));

            if (overlay is null)
            {
                overlay = new ClassAssignment
                {
                    ClassId = id,
                    PathItemId = targetNode.Id,
                    LessonId = targetNode.LessonId,
                    ExerciseId = targetNode.FinalTestId ?? targetNode.LabExerciseId,
                    DueAt = dueAt,
                    AllowLateSubmission = allowLateSubmission,
                    SortOrder = targetNode.SortOrder,
                    CreatedAt = clock.UtcNow
                };
                db.ClassAssignments.Add(overlay);
                existingAssignments.Add(overlay);
            }
            else
            {
                overlay.PathItemId = targetNode.Id;
                if (targetNode.LessonId != null) overlay.LessonId = targetNode.LessonId;
                if (targetNode.FinalTestId != null || targetNode.LabExerciseId != null)
                    overlay.ExerciseId = targetNode.FinalTestId ?? targetNode.LabExerciseId;
                overlay.DueAt = dueAt;
                overlay.AllowLateSubmission = allowLateSubmission;
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> UpdateLessonDeadlineAsync(int userId, string role, int id, int lessonId, DateTime? dueAt, bool allowLateSubmission, CancellationToken ct)
    {
        if (dueAt.HasValue && dueAt.Value <= DateTime.UtcNow)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Hạn nộp bài phải ở tương lai");
        }

        var classRoom = await db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        if (!CanManage(userId, role, classRoom))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền quản lý lớp này");
        }

        var assignment = await db.ClassAssignments.FirstOrDefaultAsync(a => a.ClassId == id && a.LessonId == lessonId, ct);
        if (assignment is not null)
        {
            assignment.DueAt = dueAt;
            assignment.AllowLateSubmission = allowLateSubmission;
        }
        else
        {
            db.ClassAssignments.Add(new ClassAssignment
            {
                ClassId = id,
                LessonId = lessonId,
                DueAt = dueAt,
                AllowLateSubmission = allowLateSubmission,
                SortOrder = 0,
                CreatedAt = clock.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<ClassCurriculumDto>> GetCurriculumAsync(int userId, string role, int id, CancellationToken ct)
    {
        var classRoom = await db.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
        if (classRoom is null)
        {
            return Result<ClassCurriculumDto>.Fail(ErrorCodes.NOT_FOUND, "Lớp học không tồn tại");
        }

        var canManage = CanManage(userId, role, classRoom);
        var isMember = await db.ClassMembers.AsNoTracking()
            .AnyAsync(m => m.ClassId == id && m.UserId == userId, ct);
        if (!canManage && !isMember)
        {
            return Result<ClassCurriculumDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lộ trình của lớp này");
        }

        string? pathTitle = null;
        string? pathDesc = null;
        if (classRoom.LearningPathId is { } pathId)
        {
            var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pathId, ct);
            if (path is not null)
            {
                pathTitle = path.Title;
                pathDesc = path.Description;
            }
        }

        var displayTitle = classRoom.CurriculumTitle ?? pathTitle;
        var displayDesc = classRoom.CurriculumDescription ?? pathDesc;

        // Draft gating: học viên chỉ thấy lộ trình khi đã publish.
        if (!canManage && !classRoom.CurriculumPublished)
        {
            return Result<ClassCurriculumDto>.Ok(new ClassCurriculumDto
            {
                ClassId = id,
                LearningPathId = classRoom.LearningPathId,
                LearningPathTitle = pathTitle,
                Title = displayTitle,
                Description = displayDesc,
                Published = false,
                ProgressPct = 0,
                Items = []
            });
        }

        // Overlay assignments from ClassAssignments for this class
        var assignments = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .ToListAsync(ct);
        var assignmentByLesson = assignments.Where(a => a.LessonId != null).ToDictionary(a => a.LessonId!.Value);

        // If Class has active LearningPathId -> build items directly from LearningPath nodes
        if (classRoom.LearningPathId is { } activePathId)
        {
            var nodes = await db.LearningPathNodes.AsNoTracking()
                .Where(n => n.PathId == activePathId)
                .OrderBy(n => n.SortOrder)
                .ToListAsync(ct);

            var nodeIds = nodes.Select(n => n.Id).ToList();
            var lessonIds = nodes.Where(n => n.LessonId != null).Select(n => n.LessonId!.Value).Distinct().ToList();
            var lessons = lessonIds.Count > 0
                ? await db.Lessons.AsNoTracking()
                    .Where(l => lessonIds.Contains(l.Id) && l.DeletedAt == null)
                    .ToDictionaryAsync(l => l.Id, ct)
                : new Dictionary<int, Lesson>();

            var topicIds = lessons.Values.Select(l => l.TopicId).Distinct().ToList();
            var topicTitles = topicIds.Count > 0
                ? await db.Topics.AsNoTracking().Where(t => topicIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Name, ct)
                : new Dictionary<int, string>();

            var overlayAssignments = await db.ClassAssignments.AsNoTracking()
                .Where(a => a.ClassId == id && !a.Archived)
                .ToListAsync(ct);
            var overlayByPathItem = overlayAssignments.Where(a => a.PathItemId != null).ToDictionary(a => a.PathItemId!.Value);
            var overlayByLesson = overlayAssignments.Where(a => a.LessonId != null).ToDictionary(a => a.LessonId!.Value);

            var passedNodeIds = (await db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId && nodeIds.Contains(p.NodeId) && p.Status == 2)
                .Select(p => p.NodeId)
                .ToListAsync(ct)).ToHashSet();

            var userProgressList = lessonIds.Count > 0
                ? await db.UserProgress.AsNoTracking()
                    .Where(p => p.UserId == userId && lessonIds.Contains(p.LessonId))
                    .ToDictionaryAsync(p => p.LessonId, ct)
                : new Dictionary<int, UserProgress>();

            var flatItems = new List<ClassCurriculumItemDto>();
            int completedCount = 0;
            int pageItemCount = 0;

            foreach (var node in nodes)
            {
                var overlay = (node.Id > 0 && overlayByPathItem.TryGetValue(node.Id, out var o1)) ? o1
                    : (node.LessonId is { } lid && overlayByLesson.TryGetValue(lid, out var o2)) ? o2 : null;

                var isFolder = node.ItemType == PathItemType.Folder;
                var itemTypeStr = isFolder ? "folder"
                    : node.ItemType == PathItemType.Theory ? "theory"
                    : node.ItemType == PathItemType.Quiz ? "quiz"
                    : node.ItemType == PathItemType.Lab ? "lab"
                    : "lesson";

                bool isCompleted = false;
                int? bestScore = null;

                if (!isFolder)
                {
                    pageItemCount++;
                    isCompleted = passedNodeIds.Contains(node.Id);
                    if (!isCompleted && node.LessonId is { } lessonIdVal && userProgressList.TryGetValue(lessonIdVal, out var prog))
                    {
                        isCompleted = prog.CompletedAt != null || (prog.BestScore ?? 0) > 0;
                        bestScore = prog.BestScore;
                    }
                    if (isCompleted) completedCount++;
                }

                Lesson? lesson = node.LessonId is { } lId ? lessons.GetValueOrDefault(lId) : null;
                var title = !string.IsNullOrWhiteSpace(node.Title) ? node.Title : (lesson?.Title ?? string.Empty);

                flatItems.Add(new ClassCurriculumItemDto
                {
                    AssignmentId = overlay?.Id ?? 0,
                    PathItemId = node.Id,
                    ParentId = node.ParentId,
                    LessonId = node.LessonId,
                    ExerciseId = node.FinalTestId ?? node.LabExerciseId,
                    Title = title,
                    Description = node.Description,
                    ItemType = itemTypeStr,
                    SortOrder = node.SortOrder,
                    DueAt = overlay?.DueAt,
                    AllowLateSubmission = overlay?.AllowLateSubmission ?? true,
                    Status = isCompleted ? "completed" : "not_started",
                    BestScore = bestScore,
                    TopicId = lesson?.TopicId,
                    TopicName = lesson is not null ? topicTitles.GetValueOrDefault(lesson.TopicId) : null,
                    SimulationCount = 0,
                    XpReward = 20
                });
            }

            foreach (var item in flatItems)
            {
                if (item.ItemType != "folder" && item.Status != "completed")
                {
                    item.Status = "in_progress";
                    break;
                }
            }

            var itemMap = flatItems.ToDictionary(i => i.PathItemId ?? i.AssignmentId);
            var roots = new List<ClassCurriculumItemDto>();
            foreach (var item in flatItems.OrderBy(i => i.SortOrder))
            {
                if (item.ParentId is { } pId && itemMap.TryGetValue(pId, out var parent))
                {
                    parent.Children.Add(item);
                }
                else
                {
                    roots.Add(item);
                }
            }

            var progressPct = pageItemCount > 0 ? (int)Math.Round((double)completedCount * 100 / pageItemCount) : 0;
            return Result<ClassCurriculumDto>.Ok(new ClassCurriculumDto
            {
                ClassId = id,
                LearningPathId = activePathId,
                LearningPathTitle = pathTitle,
                Title = displayTitle,
                Description = displayDesc,
                Published = classRoom.CurriculumPublished,
                ProgressPct = progressPct,
                Items = roots
            });
        }

        if (assignments.Count == 0)
        {
            return Result<ClassCurriculumDto>.Ok(new ClassCurriculumDto
            {
                ClassId = id,
                LearningPathId = null,
                LearningPathTitle = null,
                Title = displayTitle,
                Description = displayDesc,
                Published = classRoom.CurriculumPublished,
                ProgressPct = 0,
                Items = []
            });
        }

        // Nạp tiêu đề thật + progress của học viên (fallback cho class legacy không có LearningPathId).
        var fallbackLessonIds = assignments.Where(a => a.LessonId != null).Select(a => a.LessonId!.Value).Distinct().ToList();
        var fallbackExerciseIds = assignments.Where(a => a.ExerciseId != null).Select(a => a.ExerciseId!.Value).Distinct().ToList();

        var fallbackLessonTitles = fallbackLessonIds.Count > 0
            ? await db.Lessons.AsNoTracking().Where(l => fallbackLessonIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Title, ct)
            : new Dictionary<int, string>();
        var fallbackExerciseTitles = fallbackExerciseIds.Count > 0
            ? await db.Exercises.AsNoTracking().Where(e => fallbackExerciseIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.Title, ct)
            : new Dictionary<int, string>();

        var fallbackLessonProgress = fallbackLessonIds.Count > 0
            ? await db.UserProgress.AsNoTracking()
                .Where(p => p.UserId == userId && fallbackLessonIds.Contains(p.LessonId))
                .ToListAsync(ct)
            : new List<UserProgress>();
        var fallbackLessonProgressByLesson = fallbackLessonProgress.ToDictionary(p => p.LessonId);

        var fallbackAssignmentIds = assignments.Where(a => a.ExerciseId != null).Select(a => a.Id).ToList();
        var fallbackExerciseDone = fallbackAssignmentIds.Count > 0
            ? await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ClassAssignmentId != null && fallbackAssignmentIds.Contains(s.ClassAssignmentId.Value) && s.Score > 0)
                .Select(s => s.ClassAssignmentId!.Value)
                .Distinct()
                .ToListAsync(ct)
            : new List<int>();
        var fallbackExerciseDoneSet = fallbackExerciseDone.ToHashSet();

        var fallbackCompletedAssignments = new HashSet<int>();
        foreach (var assignment in assignments)
        {
            if (assignment.LessonId is { } lessonId && fallbackLessonProgressByLesson.TryGetValue(lessonId, out var progress)
                && (progress.CompletedAt != null || (progress.BestScore ?? 0) > 0))
            {
                fallbackCompletedAssignments.Add(assignment.Id);
            }
            else if (assignment.ExerciseId is { } exerciseId && fallbackExerciseDoneSet.Contains(assignment.Id))
            {
                fallbackCompletedAssignments.Add(assignment.Id);
            }
        }

        var fallbackFirstIncompleteIndex = -1;
        var fallbackItemDtos = new List<ClassCurriculumItemDto>();
        for (var i = 0; i < assignments.Count; i++)
        {
            var assignment = assignments[i];
            var isCompleted = fallbackCompletedAssignments.Contains(assignment.Id);
            if (!isCompleted && fallbackFirstIncompleteIndex < 0)
            {
                fallbackFirstIncompleteIndex = i;
            }

            var title = assignment.LessonId is { } lid ? (fallbackLessonTitles.GetValueOrDefault(lid) ?? string.Empty)
                : assignment.ExerciseId is { } eid ? (fallbackExerciseTitles.GetValueOrDefault(eid) ?? string.Empty)
                : string.Empty;

            fallbackItemDtos.Add(new ClassCurriculumItemDto
            {
                AssignmentId = assignment.Id,
                LessonId = assignment.LessonId,
                ExerciseId = assignment.ExerciseId,
                Title = title,
                ItemType = assignment.LessonId != null ? "lesson" : "exercise",
                SortOrder = assignment.SortOrder,
                DueAt = assignment.DueAt,
                AllowLateSubmission = assignment.AllowLateSubmission,
                Status = "not_started",
                BestScore = assignment.LessonId is { } lid2 && fallbackLessonProgressByLesson.TryGetValue(lid2, out var up)
                    ? up.BestScore
                    : null
            });
        }

        for (var i = 0; i < fallbackItemDtos.Count; i++)
        {
            if (fallbackCompletedAssignments.Contains(fallbackItemDtos[i].AssignmentId))
            {
                fallbackItemDtos[i].Status = "completed";
            }
            else if (i == fallbackFirstIncompleteIndex)
            {
                fallbackItemDtos[i].Status = "in_progress";
            }
        }

        var fallbackCompletedCount = fallbackCompletedAssignments.Count;
        var fallbackProgressPct = (int)Math.Round(fallbackCompletedCount * 100.0 / assignments.Count);

        return Result<ClassCurriculumDto>.Ok(new ClassCurriculumDto
        {
            ClassId = id,
            LearningPathId = null,
            LearningPathTitle = null,
            Title = displayTitle,
            Description = displayDesc,
            Published = classRoom.CurriculumPublished,
            ProgressPct = fallbackProgressPct,
            Items = fallbackItemDtos
        });
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

        if (classRoom.LearningPathId is { } activePathId)
        {
            var pageNodes = await db.LearningPathNodes.AsNoTracking()
                .Where(n => n.PathId == activePathId && n.ItemType != PathItemType.Folder)
                .OrderBy(n => n.SortOrder)
                .ToListAsync(ct);

            var pageNodeIds = pageNodes.Select(n => n.Id).ToList();
            var overlayAssignments = await db.ClassAssignments.AsNoTracking()
                .Where(a => a.ClassId == id && !a.Archived)
                .ToListAsync(ct);
            var overlayByPathItem = overlayAssignments.Where(a => a.PathItemId != null).ToDictionary(a => a.PathItemId!.Value);
            var overlayByLesson = overlayAssignments.Where(a => a.LessonId != null).ToDictionary(a => a.LessonId!.Value);

            var nodeProgresses = memberIds.Count > 0 && pageNodeIds.Count > 0
                ? await db.UserNodeProgress.AsNoTracking()
                    .Where(p => memberIds.Contains(p.UserId) && pageNodeIds.Contains(p.NodeId) && p.Status == 2)
                    .ToListAsync(ct)
                : [];

            var reportAssignments = new List<ClassReportAssignmentDto>();
            var completedAssignmentsByUser = memberIds.ToDictionary(mId => mId, _ => 0);

            foreach (var node in pageNodes)
            {
                var overlay = (node.Id > 0 && overlayByPathItem.TryGetValue(node.Id, out var o1)) ? o1
                    : (node.LessonId is { } lid && overlayByLesson.TryGetValue(lid, out var o2)) ? o2 : null;

                var completedList = nodeProgresses.Where(p => p.NodeId == node.Id).ToList();
                int onTime = completedList.Count(p => (overlay?.DueAt == null) || (p.PassedAt <= overlay.DueAt.Value));
                int late = completedList.Count - onTime;
                int notSubmitted = totalMembers - completedList.Count;
                double avg = completedList.Count > 0 ? completedList.Average(p => (double)(p.NodeScore > 0 ? p.NodeScore : 100)) : 0;

                foreach (var p in completedList)
                {
                    if (completedAssignmentsByUser.ContainsKey(p.UserId))
                    {
                        completedAssignmentsByUser[p.UserId]++;
                    }
                }

                reportAssignments.Add(new ClassReportAssignmentDto
                {
                    AssignmentId = overlay?.Id ?? node.Id,
                    Title = node.Title,
                    DueAt = overlay?.DueAt,
                    OnTime = onTime,
                    Late = late,
                    NotSubmitted = Math.Max(0, notSubmitted),
                    AvgScore = Math.Round(avg, 1)
                });
            }

            var lagging = memberIds
                .Select(memberId => new
                {
                    MemberId = memberId,
                    Missing = pageNodes.Count - completedAssignmentsByUser.GetValueOrDefault(memberId)
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

        var fallbackAssignments = await db.ClassAssignments.AsNoTracking()
            .Where(a => a.ClassId == id)
            .OrderBy(a => a.DueAt)
            .ToListAsync(ct);

        var assignmentIds = fallbackAssignments.Where(a => a.Id > 0).Select(a => a.Id).ToList();

        // findings-biz #15 + perf #8: chỉ đếm submissions/tiến độ của member HIỆN TẠI (member đã kick không tính)
        var submissions = assignmentIds.Count > 0 && memberIds.Count > 0
            ? await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.ClassAssignmentId != null
                    && assignmentIds.Contains(s.ClassAssignmentId.Value)
                    && memberIds.Contains(s.UserId))
                .Select(s => new SubmissionCountRow { UserId = s.UserId, ClassAssignmentId = s.ClassAssignmentId!.Value, SubmittedAt = s.SubmittedAt, Score = s.Score })
                .ToListAsync(ct)
            : [];

        // findings-perf #2 (N+1): batch title trước vòng lặp
        var lessonIds = fallbackAssignments.Where(a => a.LessonId != null).Select(a => a.LessonId!.Value).Distinct().ToList();
        var exerciseIds = fallbackAssignments.Where(a => a.ExerciseId != null).Select(a => a.ExerciseId!.Value).Distinct().ToList();
        var lessonTitles = lessonIds.Count > 0
            ? await db.Lessons.AsNoTracking()
                .Where(l => lessonIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Title, ct)
            : new Dictionary<int, string>();
        var exerciseTitles = exerciseIds.Count > 0
            ? await db.Exercises.AsNoTracking()
                .Where(e => exerciseIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.Title, ct)
            : new Dictionary<int, string>();

        // Query tiến độ bài học của các thành viên trong lớp (UserProgress)
        var lessonProgresses = lessonIds.Count > 0 && memberIds.Count > 0
            ? await db.UserProgress.AsNoTracking()
                .Where(p => memberIds.Contains(p.UserId) && lessonIds.Contains(p.LessonId) && p.CompletedAt != null)
                .Select(p => new { p.UserId, p.LessonId, CompletedAt = p.CompletedAt!.Value, BestScore = p.BestScore ?? 100 })
                .ToListAsync(ct)
            : [];

        var fallbackReportAssignments = new List<ClassReportAssignmentDto>();
        var fallbackCompletedAssignmentsByUser = memberIds.ToDictionary(mId => mId, _ => 0);

        foreach (var assignment in fallbackAssignments)
        {
            int onTime = 0;
            int late = 0;
            int notSubmitted = 0;
            double avg = 0;

            if (assignment.LessonId is { } lessonId)
            {
                var lessonCompletedList = lessonProgresses.Where(p => p.LessonId == lessonId).ToList();
                onTime = lessonCompletedList.Count(p => p.CompletedAt <= (assignment.DueAt ?? DateTime.MaxValue));
                late = lessonCompletedList.Count - onTime;
                notSubmitted = totalMembers - lessonCompletedList.Count;
                avg = lessonCompletedList.Count > 0 ? lessonCompletedList.Average(p => (double)p.BestScore) : 0;

                foreach (var p in lessonCompletedList)
                {
                    if (fallbackCompletedAssignmentsByUser.ContainsKey(p.UserId))
                    {
                        fallbackCompletedAssignmentsByUser[p.UserId]++;
                    }
                }
            }
            else if (assignment.ExerciseId is { })
            {
                // findings-biz #15 (c): count distinct (User, Assignment) — nộp trùng cùng user không đếm 2 lần;
                // lấy bài nộp SỚM NHẤT của từng user để xếp OnTime/Late
                var firstByUser = submissions
                    .Where(s => s.ClassAssignmentId == assignment.Id)
                    .GroupBy(s => s.UserId)
                    .Select(g => g.OrderBy(s => s.SubmittedAt).First())
                    .ToList();
                onTime = firstByUser.Count(s => s.SubmittedAt <= (assignment.DueAt ?? DateTime.MaxValue));
                late = firstByUser.Count - onTime;
                notSubmitted = totalMembers - firstByUser.Count;
                avg = firstByUser.Count > 0 ? firstByUser.Average(s => (double)s.Score) : 0;

                foreach (var s in firstByUser)
                {
                    if (fallbackCompletedAssignmentsByUser.ContainsKey(s.UserId))
                    {
                        fallbackCompletedAssignmentsByUser[s.UserId]++;
                    }
                }
            }

            var title = assignment.LessonId is { } lid
                ? lessonTitles.GetValueOrDefault(lid)
                : assignment.ExerciseId is { } eid
                    ? exerciseTitles.GetValueOrDefault(eid)
                    : null;

            fallbackReportAssignments.Add(new ClassReportAssignmentDto
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
        var fallbackLagging = memberIds
            .Select(memberId => new
            {
                MemberId = memberId,
                Missing = fallbackAssignments.Count - fallbackCompletedAssignmentsByUser.GetValueOrDefault(memberId)
            })
            .Where(x => x.Missing >= 2)
            .OrderByDescending(x => x.Missing)
            .Take(10)
            .ToList();

        var fallbackUsers = await db.Users.AsNoTracking()
            .Where(u => fallbackLagging.Select(l => l.MemberId).Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        return Result<ClassReportDto>.Ok(new ClassReportDto
        {
            ClassId = classRoom.Id,
            ClassName = classRoom.Name,
            TotalMembers = totalMembers,
            Assignments = fallbackReportAssignments,
            LaggingLearners = fallbackLagging.Select(l => new LaggingLearnerDto
            {
                UserId = l.MemberId,
                DisplayName = fallbackUsers.GetValueOrDefault(l.MemberId) ?? "Người dùng đã xóa",
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
        // C8: Header tổng quát
        sb.AppendLine($"Báo cáo lớp,{Csv(data.ClassName)},Tổng học viên,{data.TotalMembers}");
        sb.AppendLine();
        // C8: Bảng 1 — thống kê bài tập (cột đơn giản, không có AssignmentId kỳ lạ)
        sb.AppendLine("Bảng 1: Thống kê bài tập");
        sb.AppendLine("STT,Tên bài tập / Bài học,Hạn nộp,Nộp đúng hạn,Nộp muộn,Chưa nộp,Điểm TB");
        var idx = 1;
        foreach (var assignment in data.Assignments)
        {
            var dueStr = assignment.DueAt.HasValue
                ? assignment.DueAt.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                : "Không giới hạn";
            sb.AppendLine($"{idx++},{Csv(assignment.Title)},{Csv(dueStr)},{assignment.OnTime},{assignment.Late},{assignment.NotSubmitted},{assignment.AvgScore.ToString("F1", CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine();
        // C8: Bảng 2 — học viên chậm tiến độ
        sb.AppendLine("Bảng 2: Học viên chậm tiến độ (thiếu ≥ 2 bài)");
        sb.AppendLine("STT,Tên học viên,Số bài còn thiếu");
        var idx2 = 1;
        foreach (var learner in data.LaggingLearners)
        {
            sb.AppendLine($"{idx2++},{Csv(learner.DisplayName)},{learner.MissingCount}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var content = preamble.Concat(bytes).ToArray();
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

    private static ClassDto ToDto(Class classRoom, int memberCount, string? pathTitle = null) => new()
    {
        Id = classRoom.Id,
        Name = classRoom.Name,
        InviteCode = classRoom.InviteCode,
        Semester = classRoom.Semester,
        Description = classRoom.Description,
        OwnerId = classRoom.OwnerId,
        Status = classRoom.Status.ToString().ToLowerInvariant(),
        LearningPathId = classRoom.LearningPathId,
        LearningPathTitle = pathTitle,
        MemberCount = memberCount,
        CreatedAt = classRoom.CreatedAt
    };

    public Task<Result<ClassDetailDto>> ImportCourseAsync(int userId, string role, int id, int courseId, CancellationToken ct)
    {
        return Task.FromResult(Result<ClassDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Chức năng sao chép bài học đã dừng hỗ trợ. Vui lòng sử dụng tính năng Gán Lộ trình (PUT /classes/{id}/learning-path)."));
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    /// <summary>Projection gọn cho report (perf #8 — không kéo AnswersJson/ResultJson).</summary>
    private sealed class SubmissionCountRow
    {
        public int UserId { get; init; }
        public int ClassAssignmentId { get; init; }
        public DateTime SubmittedAt { get; init; }
        public int Score { get; init; }
    }
}

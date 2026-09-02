using System.Globalization;
using System.Net.Mail;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// UserService thật theo SDD §5.4 / API_REFERENCE.md §4.8.
/// Quy tắc Admin (SDD §5.4, FR-1.9): Admin thường (IsPrimaryAdmin=false) KHÔNG quản được Admin khác;
/// cấm vô hiệu hóa/xóa Admin cuối cùng còn active. Xóa = ẩn danh hóa NFR-35.
/// </summary>
public sealed class UserService(
    AppDbContext db,
    IDateTimeProvider clock,
    IConfiguration config,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<PagedResponse<AdminUserDto>>> GetListAsync(
        string? role, string? status, string? q, int page, int pageSize, CancellationToken ct)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.Users.AsNoTracking().Where(u => u.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(role) && RoleNames.TryParse(role, out var roleFilter))
        {
            query = query.Where(u => u.Role == roleFilter);
        }
        else
        {
            // Tab "Tất cả người dùng": loại trừ tài khoản TEACHER_PENDING đang chờ duyệt
            query = query.Where(u => u.Role != UserRole.TeacherPending);
        }

        if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var activeFilter))
        {
            query = query.Where(u => u.IsActive == activeFilter);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            // B6 fix: tìm kiếm thân thiện — không phân biệt hoa/thường và dấu tiếng Việt
            // ("tuan" vẫn ra "Tuấn Anh"). SQL collation vẫn phân biệt dấu nên lọc in-memory
            // (user base nhỏ, có cap) rồi phân trang sau.
            var normalized = NormalizeVi(q);
            var candidates = await query
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Role = RoleNames.ToApi(u.Role),
                    IsActive = u.IsActive,
                    AvatarUrl = u.AvatarUrl,
                    Department = u.Department,
                    StaffCode = u.StaffCode,
                    TeacherBio = u.TeacherBio,
                    CreatedAt = u.CreatedAt
                })
                .Take(5000)
                .ToListAsync(ct);

            var filtered = candidates
                .Where(u => NormalizeVi(u.DisplayName).Contains(normalized, StringComparison.Ordinal)
                         || NormalizeVi(u.Email).Contains(normalized, StringComparison.Ordinal))
                .ToList();

            var totalFiltered = filtered.Count;
            var filteredItems = filtered
                .OrderBy(u => u.Id)
                .Skip((safePage - 1) * safeSize).Take(safeSize)
                .ToList();

            return Result<PagedResponse<AdminUserDto>>.Ok(
                PagedResponse<AdminUserDto>.Create(filteredItems, safePage, safeSize, totalFiltered, Pagination.TotalPages(totalFiltered, safeSize)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Role = RoleNames.ToApi(u.Role),
                IsActive = u.IsActive,
                AvatarUrl = u.AvatarUrl,
                Department = u.Department,
                StaffCode = u.StaffCode,
                TeacherBio = u.TeacherBio,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return Result<PagedResponse<AdminUserDto>>.Ok(
            PagedResponse<AdminUserDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    /// <summary>B6: hạ chữ thường + bỏ dấu tiếng Việt để so khớp tìm kiếm.</summary>
    private static string NormalizeVi(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var formD = input.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<Result<AdminUserDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        // v2.15: nạp thống kê học tập cho drawer chi tiết user (Vấn đề 8)
        var dto = ToDto(user);
        dto.Xp = user.Xp;
        dto.Level = ComputeLevel(user.Xp);
        dto.StreakDays = user.StreakDays;
        dto.Gems = user.Gems;
        dto.Hearts = user.Hearts;

        dto.LessonsCompletedCount = await db.UserProgress.AsNoTracking()
            .CountAsync(p => p.UserId == id && p.CompletedAt != null, ct);
        dto.ExercisesPassedCount = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => s.UserId == id)
            .Join(db.Exercises.AsNoTracking(), s => s.ExerciseId, e => e.Id, (s, e) => s.Score == e.MaxScore)
            .CountAsync(x => x, ct);
        dto.JoinedClassesCount = await db.ClassMembers.AsNoTracking()
            .CountAsync(m => m.UserId == id, ct);

        return Result<AdminUserDto>.Ok(dto);
    }

    public async Task<Result<AdminUserDto>> CreateUserAsync(
        int actorId, bool actorIsPrimaryAdmin, AdminCreateUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length < 2)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Họ tên phải từ 2 ký tự trở lên");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Email không hợp lệ");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await db.Users.AsNoTracking().AnyAsync(u => u.Email == normalizedEmail && u.DeletedAt == null, ct);
        if (emailExists)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.EMAIL_EXISTS, "Email đã được sử dụng");
        }

        var policyErrors = PasswordPolicy.Validate(request.Password);
        if (policyErrors.Count > 0)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.WEAK_PASSWORD, "Mật khẩu yếu", new() { ["password"] = policyErrors.ToArray() });
        }

        if (!RoleNames.TryParse(request.Role, out var role))
        {
            role = UserRole.Student;
        }

        if (role == UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính mới có quyền tạo tài khoản Admin khác");
        }

        // A3: Mã giảng viên bắt buộc cho Giảng viên + duy nhất
        var staffCode = request.StaffCode?.Trim();
        if (role == UserRole.Teacher && string.IsNullOrEmpty(staffCode))
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Vui lòng nhập mã giảng viên", new()
            {
                ["staffCode"] = ["Mã giảng viên là bắt buộc đối với Giảng viên"]
            });
        }
        if (!string.IsNullOrEmpty(staffCode) &&
            await db.Users.AsNoTracking().AnyAsync(u => u.StaffCode == staffCode && u.DeletedAt == null, ct))
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.CONFLICT, "Mã giảng viên đã được sử dụng", new()
            {
                ["staffCode"] = ["Mã giảng viên đã được sử dụng"]
            });
        }

        var user = new User
        {
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = role,
            IsActive = true,
            Department = string.IsNullOrEmpty(request.Department?.Trim()) ? null : request.Department.Trim(),
            StaffCode = string.IsNullOrEmpty(staffCode) ? null : staffCode,
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} ({Email}) created by Admin {ActorId}", user.Id, user.Email, actorId);
        return Result<AdminUserDto>.Ok(ToDto(user));
    }

    public async Task<Result<AdminUserDto>> UpdateUserAsync(
        int actorId, bool actorIsPrimaryAdmin, int id, AdminUpdateUserRequest request, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin && actorId != id)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.FORBIDDEN, "Không thể chỉnh sửa thông tin tài khoản Quản trị viên cùng cấp");
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.Department is not null) user.Department = request.Department.Trim();
        // A3: Mã giảng viên duy nhất — nếu đổi sang mã user khác đang giữ → 409 CONFLICT
        if (request.StaffCode is not null)
        {
            var newStaffCode = request.StaffCode.Trim();
            var staffCodeTaken = !string.IsNullOrEmpty(newStaffCode)
                && await db.Users.AsNoTracking().AnyAsync(u => u.StaffCode == newStaffCode && u.Id != user.Id && u.DeletedAt == null, ct);
            if (staffCodeTaken)
            {
                return Result<AdminUserDto>.Fail(ErrorCodes.CONFLICT, "Mã giảng viên đã được sử dụng", new()
                {
                    ["staffCode"] = ["Mã giảng viên đã được sử dụng"]
                });
            }
            user.StaffCode = string.IsNullOrEmpty(newStaffCode) ? null : newStaffCode;
        }
        if (request.AcademicDegree is not null) user.AcademicDegree = request.AcademicDegree.Trim();
        if (request.ProfileLink is not null) user.ProfileLink = request.ProfileLink.Trim();
        if (request.TeacherBio is not null) user.TeacherBio = request.TeacherBio.Trim();

        if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
        {
            if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin)
            {
                return Result<AdminUserDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính được khóa/mở tài khoản Admin khác");
            }
            if (!request.IsActive.Value && user.Role == UserRole.Admin && !await HasOtherActiveAdminAsync(id, ct))
            {
                return Result<AdminUserDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Không thể khóa Admin cuối cùng còn hoạt động");
            }
            user.IsActive = request.IsActive.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Role) && RoleNames.TryParse(request.Role, out var newRole))
        {
            if (user.Role == UserRole.Admin && newRole != UserRole.Admin && !actorIsPrimaryAdmin)
            {
                return Result<AdminUserDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính mới có quyền hạ cấp vai trò Admin khác");
            }
            if (newRole == UserRole.Admin && !actorIsPrimaryAdmin)
            {
                return Result<AdminUserDto>.Fail(ErrorCodes.FORBIDDEN, "Không thể gán quyền Admin");
            }
            user.Role = newRole;
        }

        user.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<AdminUserDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        logger.LogInformation("User {TargetId} updated by Admin {ActorId}", id, actorId);
        return Result<AdminUserDto>.Ok(ToDto(user));
    }

    public async Task<Result> SetStatusAsync(int actorId, bool actorIsPrimaryAdmin, int id, bool isActive, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính được khóa/mở tài khoản Admin khác");
        }

        if (!isActive && user.Role == UserRole.Admin && !await HasOtherActiveAdminAsync(id, ct))
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Không thể khóa Admin cuối cùng còn hoạt động", new() { ["isActive"] = ["Không thể khóa Admin cuối cùng còn hoạt động"] });
        }

        user.IsActive = isActive;
        user.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        logger.LogInformation("User {TargetId} status -> {IsActive} by {ActorId}", id, isActive, actorId);
        return Result.Ok();
    }

    public async Task<Result> SetRoleAsync(int actorId, bool actorIsPrimaryAdmin, int id, string role, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (!RoleNames.TryParse(role, out var newRole))
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Vai trò không hợp lệ");
        }

        if (newRole == UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính mới có quyền gán vai trò Admin");
        }

        if (user.Role == UserRole.Admin && newRole != UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính được đổi vai trò Admin khác");
        }

        user.Role = newRole;
        user.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        logger.LogInformation("User {TargetId} role -> {Role} by {ActorId}", id, newRole, actorId);
        return Result.Ok();
    }

    public async Task<Result> ApproveTeacherAsync(int actorId, bool actorIsPrimaryAdmin, int id, ApproveTeacherRequest request, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.Role != UserRole.TeacherPending)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Tài khoản không ở trạng thái chờ phê duyệt Teacher", new() { ["id"] = ["Tài khoản không ở trạng thái chờ phê duyệt Teacher"] });
        }

        // v2.15 (Vấn đề 2): từ chối bắt buộc nhập Lý do — ứng viên cần biết lý do để chỉnh hồ sơ
        if (!request.Approve && string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Phải nhập lý do khi từ chối hồ sơ giảng viên", new() { ["reason"] = ["Phải nhập lý do khi từ chối hồ sơ giảng viên"] });
        }

        user.Role = request.Approve ? UserRole.Teacher : UserRole.Student;
        user.IsActive = true;
        user.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        logger.LogInformation("Teacher approval for {TargetId}: {Approve} (reason: {Reason}) by {ActorId}",
            id, request.Approve, request.Reason, actorId);

        // Email thông báo kết quả duyệt/từ chối (cam kết UI: "bạn nhận email khi được duyệt").
        // KHÔNG block luồng: SMTP thiếu → log warning; lỗi gửi → log error (SDD §5.6, pattern AuthService).
        await SendTeacherDecisionEmailAsync(user, request.Approve, request.Reason, ct);
        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(int actorId, bool actorIsPrimaryAdmin, int id, AdminResetPasswordRequest request, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính được đặt lại mật khẩu Admin khác");
        }

        var policyErrors = PasswordPolicy.Validate(request.NewPassword);
        if (policyErrors.Count > 0)
        {
            return Result.Fail(ErrorCodes.WEAK_PASSWORD, "Mật khẩu yếu", new() { ["password"] = policyErrors.ToArray() });
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAt = clock.UtcNow;

        var activeTokens = await db.RefreshTokens.Where(t => t.UserId == id && t.RevokedAt == null).ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = clock.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }
        logger.LogInformation("Password reset for {TargetId} by {ActorId}", id, actorId);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int actorId, bool actorIsPrimaryAdmin, int id, CancellationToken ct)
    {
        var user = await GetActiveAsync(id, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Chỉ Admin chính được xóa Admin khác");
        }

        if (user.Role == UserRole.Admin && !await HasOtherActiveAdminAsync(id, ct))
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Không thể xóa Admin cuối cùng còn hoạt động", new() { ["id"] = ["Không thể xóa Admin cuối cùng còn hoạt động"] });
        }

        // Ẩn danh hóa NFR-35: email giả, tên ẩn, khóa + xóa mềm
        user.Email = $"deleted-{id}@anonymous.local";
        user.DisplayName = "Người dùng đã xóa";
        user.IsActive = false;
        user.DeletedAt = clock.UtcNow;
        user.UpdatedAt = clock.UtcNow;

        var activeTokens = await db.RefreshTokens.Where(t => t.UserId == id && t.RevokedAt == null).ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = clock.UtcNow;
        }

        // Vòng đời lớp (SDD §7.3.16 v2.8): Teacher bị xóa → lớp tự đóng
        var ownedClasses = await db.Classes.Where(c => c.OwnerId == id && c.DeletedAt == null).ToListAsync(ct);
        foreach (var classRoom in ownedClasses)
        {
            classRoom.Status = ClassStatus.Closed;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }
        logger.LogInformation("User {TargetId} anonymized/deleted by {ActorId}", id, actorId);
        return Result.Ok();
    }

    // ── Private ───────────────────────────────────────────────

    private async Task<User?> GetActiveAsync(int id, CancellationToken ct) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);

    private async Task<bool> HasOtherActiveAdminAsync(int id, CancellationToken ct) =>
        await db.Users.AsNoTracking()
            .AnyAsync(u => u.Id != id && u.Role == UserRole.Admin && u.IsActive && u.DeletedAt == null, ct);

    /// <summary>
    /// Email thông báo kết quả duyệt/từ chối giảng viên.
    /// KHÔNG block luồng: SMTP chưa cấu hình → log warning dev; lỗi gửi → log error (SDD §5.6, pattern AuthService).
    /// </summary>
    private async Task SendTeacherDecisionEmailAsync(User user, bool approved, string? reason, CancellationToken ct)
    {
        var smtpHost = config["DSA:Email:SmtpHost"];
        var subject = approved
            ? "Tài khoản giảng viên của bạn đã được duyệt — DSA Visual"
            : "Yêu cầu đăng ký giảng viên đã bị từ chối — DSA Visual";

        var body = approved
            ? $"Xin chào {user.DisplayName},\n\n" +
              "Yêu cầu đăng ký tài khoản giảng viên của bạn đã được duyệt.\n" +
              "Bạn có thể đăng nhập và bắt đầu sử dụng các tính năng dành cho giảng viên.\n\n" +
              "— DSA Visual"
            : $"Xin chào {user.DisplayName},\n\n" +
              "Yêu cầu đăng ký tài khoản giảng viên của bạn đã bị từ chối." +
              (string.IsNullOrWhiteSpace(reason) ? string.Empty : $"\nLý do: {reason}") +
              "\n\n— DSA Visual";

        var emailOpts = DsaVisual.Application.Options.EmailOptions.FromConfiguration(config);

        if (string.IsNullOrWhiteSpace(emailOpts.SmtpHost))
        {
            // SMTP chưa cấu hình → ghi log dev, KHÔNG block luồng (SDD §5.6)
            logger.LogWarning("SMTP chưa cấu hình — email duyệt/từ chối giảng viên (dev) cho user {UserId}: {Subject}",
                user.Id, subject);
            return;
        }

        try
        {
            using var smtp = SmtpClientFactory.Create(emailOpts);
            await smtp.SendMailAsync(
                emailOpts.From ?? "no-reply@dsa-visual.local",
                user.Email,
                subject,
                body, ct);
        }
        catch (Exception ex)
        {
            // Email lỗi KHÔNG block luồng duyệt/từ chối (quyết định chốt SDD §5.6)
            logger.LogError(ex, "Gửi email duyệt/từ chối giảng viên thất bại cho user {UserId}", user.Id);
        }
    }

    private static AdminUserDto ToDto(User user) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        Role = RoleNames.ToApi(user.Role),
        IsActive = user.IsActive,
        AvatarUrl = user.AvatarUrl,
        Department = user.Department,
        StaffCode = user.StaffCode,
        TeacherBio = user.TeacherBio,
        AcademicDegree = user.AcademicDegree,
        ProfileLink = user.ProfileLink,
        CreatedAt = user.CreatedAt
    };

    /// <summary>Cấp độ theo XP — khớp GamificationService.ComputeLevel (1 + floor(sqrt(xp/100))).</summary>
    private static int ComputeLevel(int xp) => 1 + (int)Math.Floor(Math.Sqrt(xp / 100.0));
}

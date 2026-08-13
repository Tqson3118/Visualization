using System.Net.Mail;
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

        if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var activeFilter))
        {
            query = query.Where(u => u.IsActive == activeFilter);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            query = query.Where(u => u.Email.Contains(keyword) || u.DisplayName.Contains(keyword));
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

    public async Task<Result<AdminUserDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);
        return user is null
            ? Result<AdminUserDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại")
            : Result<AdminUserDto>.Ok(ToDto(user));
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

        if (!RoleNames.TryParse(role, out var newRole) || newRole == UserRole.Admin)
        {
            // Cấm gán ADMIN qua API (SDD §5.4 — chỉ seed/script tạo Admin)
            return Result.Fail(ErrorCodes.FORBIDDEN, "Không thể gán vai trò ADMIN qua API");
        }

        if (user.Role == UserRole.Admin && !actorIsPrimaryAdmin)
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

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            // SMTP chưa cấu hình → ghi log dev, KHÔNG block luồng (SDD §5.6)
            logger.LogWarning("SMTP chưa cấu hình — email duyệt/từ chối giảng viên (dev) cho user {UserId}: {Subject}",
                user.Id, subject);
            return;
        }

        try
        {
            using var smtp = new SmtpClient(smtpHost, config.GetValue("DSA:Email:SmtpPort", 1025))
            {
                Timeout = 10_000   // timeout ngắn — không giữ request (GP-T2)
            };
            await smtp.SendMailAsync(
                config["DSA:Email:From"] ?? "no-reply@dsa-visual.local",
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
        CreatedAt = user.CreatedAt
    };
}

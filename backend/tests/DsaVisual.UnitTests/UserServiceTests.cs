using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test UserService.ApproveTeacherAsync: hành vi duyệt/từ chối giữ nguyên + KHÔNG block luồng
/// khi SMTP chưa cấu hình (email chỉ log warning — SDD §5.6, pattern AuthService).
/// </summary>
public class UserServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private static async Task SeedPendingTeacherAsync(AppDbContext db, int id = 1)
    {
        db.Users.Add(new User
        {
            Id = id,
            Email = "teacher@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Cô Mai",
            Role = UserRole.TeacherPending,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task ApproveTeacher_Approved_ReturnsOkAndRoleTeacher_EvenWithoutSmtp()
    {
        var db = TestServices.CreateInMemoryDb(nameof(ApproveTeacher_Approved_ReturnsOkAndRoleTeacher_EvenWithoutSmtp));
        var service = TestServices.CreateUserService(db, _clock);
        await SeedPendingTeacherAsync(db);

        var result = await service.ApproveTeacherAsync(
            actorId: 99, actorIsPrimaryAdmin: true, id: 1,
            new ApproveTeacherRequest { Approve = true }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == 1);
        Assert.Equal(UserRole.Teacher, saved.Role);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task ApproveTeacher_Rejected_ReturnsOkAndRoleStudent_EvenWithoutSmtp()
    {
        var db = TestServices.CreateInMemoryDb(nameof(ApproveTeacher_Rejected_ReturnsOkAndRoleStudent_EvenWithoutSmtp));
        var service = TestServices.CreateUserService(db, _clock);
        await SeedPendingTeacherAsync(db);

        var result = await service.ApproveTeacherAsync(
            actorId: 99, actorIsPrimaryAdmin: true, id: 1,
            new ApproveTeacherRequest { Approve = false, Reason = "Hồ sơ thiếu minh chứng" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == 1);
        Assert.Equal(UserRole.Student, saved.Role);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task ApproveTeacher_NotPending_ReturnsValidationFailed()
    {
        var db = TestServices.CreateInMemoryDb(nameof(ApproveTeacher_NotPending_ReturnsValidationFailed));
        var service = TestServices.CreateUserService(db, _clock);
        await SeedPendingTeacherAsync(db);

        var user = await db.Users.SingleAsync(u => u.Id == 1);
        user.Role = UserRole.Teacher;
        await db.SaveChangesAsync();

        var result = await service.ApproveTeacherAsync(
            actorId: 99, actorIsPrimaryAdmin: true, id: 1,
            new ApproveTeacherRequest { Approve = true }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact]
    public async Task ApproveTeacher_RejectWithoutReason_ReturnsValidationFailed()
    {
        var db = TestServices.CreateInMemoryDb(nameof(ApproveTeacher_RejectWithoutReason_ReturnsValidationFailed));
        var service = TestServices.CreateUserService(db, _clock);
        await SeedPendingTeacherAsync(db);

        // v2.15 (Vấn đề 2): từ chối bắt buộc nhập Lý do — ứng viên cần biết lý do để chỉnh hồ sơ
        var result = await service.ApproveTeacherAsync(
            actorId: 99, actorIsPrimaryAdmin: true, id: 1,
            new ApproveTeacherRequest { Approve = false }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == 1);
        Assert.Equal(UserRole.TeacherPending, saved.Role);
    }
}

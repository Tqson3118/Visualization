using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test ClassService.JoinByCodeAsync (v2.15 — Vấn đề 14/4.1): tham gia lớp bằng mã mời,
/// không cần biết classId trước. Mã sai → NOT_FOUND; lớp đóng / đã tham gia → fail.
/// </summary>
public class ClassServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(ClassService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);

        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 2,
            Email = "teacher@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Teacher",
            CreatedAt = _clock.UtcNow
        });
        db.Classes.Add(new Class
        {
            Id = 1,
            Name = "CTDL-GT K17",
            InviteCode = "ABC123",
            OwnerId = 2,
            Status = ClassStatus.Open,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        return (TestServices.CreateClassService(db, _clock), db);
    }

    [Fact]
    public async Task JoinByCode_Success_AddsMember()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_Success_AddsMember));

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "abc123" }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("ABC123", result.Value.InviteCode);
        Assert.Contains(result.Value.Members, m => m.UserId == 1);
        Assert.Equal(1, await db.ClassMembers.CountAsync(m => m.ClassId == 1 && m.UserId == 1));
    }

    [Fact]
    public async Task JoinByCode_UnknownCode_ReturnsNotFound()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_UnknownCode_ReturnsNotFound));

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ZZZ999" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
        Assert.Equal(0, await db.ClassMembers.CountAsync());
    }

    [Fact]
    public async Task JoinByCode_ClosedClass_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_ClosedClass_ReturnsValidationFailed));
        var classRoom = await db.Classes.SingleAsync(c => c.Id == 1);
        classRoom.Status = ClassStatus.Closed;
        await db.SaveChangesAsync();

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ABC123" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(0, await db.ClassMembers.CountAsync());
    }

    [Fact]
    public async Task JoinByCode_AlreadyMember_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_AlreadyMember_ReturnsValidationFailed));
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = 1, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ABC123" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(1, await db.ClassMembers.CountAsync(m => m.UserId == 1));
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DsaVisual.Api.Controllers;
using Microsoft.AspNetCore.Http;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DsaVisual.UnitTests;

/// <summary>
/// REGRESSION (fix blind-trust SaveProgress): node LAB có exercise CODE → CHỈ pass khi học viên
/// có bài nộp code PASS toàn bộ test phía máy chủ. Cờ CodelabCompleted/Completed từ client
/// (kể cả POST thẳng API bằng token) KHÔNG đủ để mở node + cộng XP — mirror nhánh Quiz 70%.
/// </summary>
public class SaveProgressCodelabGateTests
{
    private sealed class FakeGamificationConfig : IGamificationConfigService
    {
        public GamificationSettingsDto GetSettings() => new();
        public Task<GamificationSettingsDto> UpdateSettingsAsync(GamificationSettingsDto s, CancellationToken ct = default) => Task.FromResult(s);
        public Task<GamificationSettingsDto> ResetToDefaultsAsync(CancellationToken ct = default) => Task.FromResult(new GamificationSettingsDto());
        public int GetTheoryBaseXp() => 10;
        public int GetQuizBaseXp() => 20;
        public int GetCodelabBaseXp() => 30;
        public int GetStreakBonusXp() => 5;
        public int GetHeartsMaxFree() => 5;
        public int GetHeartsMaxPremium() => 10;
        public int GetHeartRegenMinutes() => 30;
        public int GetSessionHours() => 2;
    }

    private const int UserId = 7;

    /// <summary>Seed topic + lesson + user + path(Active) + node LAB + exercise CODE (gắn NodeId + LabExerciseId).</summary>
    private static async Task<(AppDbContext Db, int NodeId, int ExerciseId)> SeedCodelabNodeAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Topics.Add(new Topic { Id = 1, Name = "Gate", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Bài học codelab",
            ContentHtml = "<p>x</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = UserId,
            Email = "gate@student.edu.vn",
            PasswordHash = "x",
            DisplayName = "Gate Student",
            Xp = 0,
            CreatedAt = DateTime.UtcNow
        });
        var path = new LearningPath { Title = "Path Gate", Status = LearningPathStatus.Active, IsActive = true, CreatedBy = 1 };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();

        var node = new LearningPathNode
        {
            PathId = path.Id,
            Title = "Lab node",
            SortOrder = 1,
            ItemType = PathItemType.Lab,
            LessonId = 1
        };
        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync();

        var exercise = new Exercise
        {
            LessonId = 1,
            NodeId = node.Id,
            Stage = 3,
            Title = "Lab exercise",
            Type = ExerciseType.Code,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        db.Exercises.Add(exercise);
        node.LabExerciseId = exercise.Id;
        await db.SaveChangesAsync();
        return (db, node.Id, exercise.Id);
    }

    private static ConceptsController CreateController(AppDbContext db) => new(
        db,
        new FakeGamificationConfig(),
        new MemoryCache(new MemoryCacheOptions()))
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString())], "Test"))
            }
        }
    };

    private static ConceptsController.LessonProgressPayload CompletePayload() => new()
    {
        Completed = true,
        CodelabCompleted = true,
    };

    [Fact(DisplayName = "Node LAB chưa từng nộp code → SaveProgress từ chối (400), node KHÔNG pass, KHÔNG cộng XP")]
    public async Task SaveProgress_CodelabNode_NoSubmission_Rejected()
    {
        var (db, nodeId, _) = await SeedCodelabNodeAsync(nameof(SaveProgress_CodelabNode_NoSubmission_Rejected));
        var controller = CreateController(db);

        var result = await controller.SaveProgress(nodeId, CompletePayload(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(db.UserNodeProgress.Where(p => p.UserId == UserId && p.NodeId == nodeId && p.Status == 2));
        Assert.Equal(0, db.Users.Find(UserId)!.Xp);
    }

    [Fact(DisplayName = "Bài nộp code có case FAIL (Passed < Total) → 400, node KHÔNG pass")]
    public async Task SaveProgress_CodelabNode_FailingSubmission_Rejected()
    {
        var (db, nodeId, exId) = await SeedCodelabNodeAsync(nameof(SaveProgress_CodelabNode_FailingSubmission_Rejected));
        db.CodeSubmissions.Add(new CodeSubmission
        {
            UserId = UserId,
            ExerciseId = exId,
            Code = "function solve(){ return 1; }",
            Score = 5,
            PassedTests = 1,
            TotalTests = 2,
            ResultJson = "{}",
            SubmittedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var controller = CreateController(db);

        var result = await controller.SaveProgress(nodeId, CompletePayload(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(db.UserNodeProgress.Where(p => p.UserId == UserId && p.NodeId == nodeId && p.Status == 2));
        Assert.Equal(0, db.Users.Find(UserId)!.Xp);
    }

    [Fact(DisplayName = "Bài nộp PASS toàn bộ test (Passed >= Total > 0) → node pass (Status=2) + XP codelab được cộng đúng 1 lần")]
    public async Task SaveProgress_CodelabNode_PassingSubmission_NodePassedAndXpAwardedOnce()
    {
        var (db, nodeId, exId) = await SeedCodelabNodeAsync(nameof(SaveProgress_CodelabNode_PassingSubmission_NodePassedAndXpAwardedOnce));
        db.CodeSubmissions.Add(new CodeSubmission
        {
            UserId = UserId,
            ExerciseId = exId,
            Code = "function solve(){ return 1; }",
            Score = 10,
            PassedTests = 2,
            TotalTests = 2,
            ResultJson = "{}",
            SubmittedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var controller = CreateController(db);

        var first = await controller.SaveProgress(nodeId, CompletePayload(), CancellationToken.None);
        Assert.IsType<OkObjectResult>(first.Result);

        var nodeProgress = db.UserNodeProgress.Single(p => p.UserId == UserId && p.NodeId == nodeId);
        Assert.Equal(2, nodeProgress.Status);
        Assert.Equal(30, db.Users.Find(UserId)!.Xp);

        // Sync lần 2 (re-sync FE) → vẫn OK nhưng KHÔNG cộng XP lần nữa (anti double-count)
        var second = await controller.SaveProgress(nodeId, CompletePayload(), CancellationToken.None);
        Assert.IsType<OkObjectResult>(second.Result);
        Assert.Equal(30, db.Users.Find(UserId)!.Xp);
    }

    [Fact(DisplayName = "Node THEORY không có exercise code → hoàn thành bình thường (không bị gate chặn)")]
    public async Task SaveProgress_TheoryNode_NoCodeExercise_Completes()
    {
        var db = TestServices.CreateInMemoryDb(nameof(SaveProgress_TheoryNode_NoCodeExercise_Completes));
        db.Topics.Add(new Topic { Id = 1, Name = "Gate", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Bài lý thuyết",
            ContentHtml = "<p>x</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = UserId,
            Email = "theory@student.edu.vn",
            PasswordHash = "x",
            DisplayName = "Theory Student",
            Xp = 0,
            CreatedAt = DateTime.UtcNow
        });
        var path = new LearningPath { Title = "Path Theory", Status = LearningPathStatus.Active, IsActive = true, CreatedBy = 1 };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();
        var node = new LearningPathNode { PathId = path.Id, Title = "Theory node", SortOrder = 1, ItemType = PathItemType.Theory, LessonId = 1 };
        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var result = await controller.SaveProgress(node.Id, CompletePayload(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(2, db.UserNodeProgress.Single(p => p.UserId == UserId && p.NodeId == node.Id).Status);
        Assert.Equal(10, db.Users.Find(UserId)!.Xp); // theory base XP
    }
}

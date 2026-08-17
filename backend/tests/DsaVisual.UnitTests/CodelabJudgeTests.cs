using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test chấm code PHÍA MÁY CHỦ (nghiệp vụ 15/08): bài ASM chỉ PASS khi code chạy ĐÚNG trên server
/// (Jint sandbox) — code sai / lỗi biên dịch / vòng lặp vô hạn KHÔNG được pass.
/// </summary>
public class CodelabJudgeTests
{
    private static readonly TestServices.FixedClock Clock = new();

    // ── 1. Judge đơn lẻ ─────────────────────────────────────────

    private static readonly CodelabTaskSpec ParenthesesTask = new(
        "final-2-valid-parentheses",
        "isValid",
        [
            new CodelabTestCaseSpec("[\"()[]{}\"]", "true", false),
            new CodelabTestCaseSpec("[\"()\"]", "true", false),
            new CodelabTestCaseSpec("[\"(]\"]", "false", false),
            new CodelabTestCaseSpec("[\"([{}])\"]", "true", true),
            new CodelabTestCaseSpec("[\"((((\"]", "false", true),
        ]);

    [Fact]
    public void Judge_CorrectCode_AllCasesPass()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge(
            """
            function isValid(s) {
              const stack = [];
              const pairs = { ')': '(', ']': '[', '}': '{' };
              for (const ch of s) {
                if (ch === '(' || ch === '[' || ch === '{') stack.push(ch);
                else if (stack.length === 0 || stack.pop() !== pairs[ch]) return false;
              }
              return stack.length === 0;
            }
            """,
            ParenthesesTask);

        Assert.False(result.CompileError);
        Assert.False(result.TimedOut);
        Assert.Equal(5, result.Cases.Count);
        Assert.All(result.Cases, c => Assert.True(c.Passed, $"case lỗi: {c.Error}"));
    }

    [Fact]
    public void Judge_WrongCode_SomeCasesFail()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge(
            """
            function isValid(s) {
              let open = 0;
              for (const ch of s) if (ch === '(' || ch === '[' || ch === '{') open++;
              return open === s.length - open;
            }
            """,
            ParenthesesTask);

        var passed = result.Cases.Count(c => c.Passed);
        Assert.True(passed < 5, "code sai thứ tự vẫn phải FAIL ít nhất 1 case");
    }

    [Fact]
    public void Judge_CompileError_Reported()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge("function isValid(s) { this is not js !!! }", ParenthesesTask);

        Assert.True(result.CompileError);
        Assert.NotNull(result.CompileErrorText);
        Assert.Empty(result.Cases);
    }

    [Fact]
    public void Judge_InfiniteLoop_TimesOut()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge("function isValid(s) { while (true) {} }", ParenthesesTask);

        Assert.True(result.TimedOut, "vòng lặp vô hạn phải bị timeout, không treo test");
        Assert.NotNull(result.TimeoutError);
    }

    [Fact]
    public void Judge_DeepRecursion_BlockedByStackGuard()
    {
        var judge = new CodelabJudgeService();
        var result = judge.Judge(
            """
            function isValid(s) { return isValid(s); }
            """,
            ParenthesesTask);

        Assert.True(result.CompileError || !result.Cases.Any(c => c.Passed),
            "đệ quy vô hạn phải bị chặn (stack guard), không được pass");
    }

    [Fact]
    public void ParseTasks_ArrayConfig_ReturnsTasks()
    {
        var config = """
            [
              { "id": "t1", "title": "Bài 1", "entryFunction": "solve",
                "testCases": [ { "name": "c1", "input": "[1]", "expectedOutput": "1", "isHidden": false } ] },
              { "id": "t2", "title": "Bài 2", "entryFunction": "solve2",
                "testCases": [ { "name": "c1", "input": "[]", "expectedOutput": "null", "isHidden": true } ] }
            ]
            """;

        var tasks = CodelabJudgeService.TryParseTasks(config);

        Assert.NotNull(tasks);
        Assert.Equal(2, tasks!.Count);
        Assert.Equal("t1", tasks[0].Id);
        Assert.Equal("solve", tasks[0].EntryFunction);
        Assert.Single(tasks[0].TestCases);
    }

    [Fact]
    public void ParseTasks_ObjectConfig_ReturnsNull()
    {
        var config = """{ "signature": "old", "language": "javascript", "testCases": [] }""";
        Assert.Null(CodelabJudgeService.TryParseTasks(config));
        Assert.Null(CodelabJudgeService.TryParseTasks(null));
    }

    // ── 2. SubmitCodeAsync: chấm server + node pass ─────────────

    private static string BuildTasksConfig() => """
        [
          { "id": "asm-1-a", "title": "Bài 1", "entryFunction": "isValid",
            "testCases": [
              { "name": "c1", "input": "[\"()\"]", "expectedOutput": "true", "isHidden": false },
              { "name": "c2", "input": "[\"(]\"]", "expectedOutput": "false", "isHidden": true }
            ] }
        ]
        """;

    private const string CorrectCode = """
        function isValid(s) {
          const stack = [];
          const pairs = { ')': '(', ']': '[', '}': '{' };
          for (const ch of s) {
            if (ch === '(' || ch === '[' || ch === '{') stack.push(ch);
            else if (stack.length === 0 || stack.pop() !== pairs[ch]) return false;
          }
          return stack.length === 0;
        }
        """;

    private const string WrongCode = """
        function isValid(s) { return false; }
        """;

    private async Task<(AppDbContext Db, ExerciseService Service, int ExerciseId, int NodeId)> SeedAsmExerciseAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        var now = Clock.UtcNow;
        db.Topics.Add(new Topic { Id = 1, Name = "CTDL", CreatedBy = 1, CreatedAt = now });
        db.Lessons.Add(new Lesson { Id = 1, TopicId = 1, Title = "ASM Test", ContentHtml = "<p>x</p>", Status = LessonStatus.Active, CreatedBy = 1, CreatedAt = now });
        db.Users.Add(new User { Id = 1, Email = "student@university.edu.vn", PasswordHash = "x", DisplayName = "S", CreatedAt = now });
        var path = new LearningPath { Title = "Test Path", Description = "x", TopicId = 1, SortOrder = 1, IsActive = true, CreatedBy = 1 };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();
        // node 1 (bậc đầu — luôn mở) gắn exercise code
        var node = new LearningPathNode { PathId = path.Id, Title = "Học: ASM Test", LessonId = 1, SortOrder = 1 };
        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync();

        var service = TestServices.CreateExerciseService(db, Clock);
        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            NodeId = node.Id,
            Stage = 3,
            Title = "Code: ASM Test",
            Type = ExerciseType.Code,
            MaxScore = 100,
            Status = ExerciseStatus.Active,
            ConfigJson = BuildTasksConfig()
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return (db, service, created.Value!.Id, node.Id);
    }

    private static CodeSubmitRequest SubmitRequest(string code, string? taskId) => new()
    {
        Code = code,
        Score = 100,
        Passed = 100,
        Total = 100,
        TaskId = taskId
    };

    [Fact]
    public async Task SubmitCode_ServerJudge_CorrectCode_PassesNode()
    {
        var (db, service, exerciseId, nodeId) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_CorrectCode_PassesNode));

        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest(CorrectCode, "asm-1-a"), CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.Passed);
        Assert.Equal(2, result.Value.Total);
        Assert.Null(result.Value.Error);
        var progress = await db.UserNodeProgress.AsNoTracking().FirstAsync(p => p.NodeId == nodeId);
        Assert.Equal(2, progress.Status);   // pass khi code ĐÚNG
        var submission = await db.CodeSubmissions.AsNoTracking().FirstAsync(s => s.ExerciseId == exerciseId);
        Assert.False(submission.IsClientDeclared);   // máy chủ chấm, không tin client
    }

    [Fact]
    public async Task SubmitCode_ServerJudge_WrongCode_DoesNotPassNode()
    {
        var (db, service, exerciseId, nodeId) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_WrongCode_DoesNotPassNode));

        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest(WrongCode, "asm-1-a"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // code "return false" đúng được case "(]" (expected false) → 1/2 — không full → KHÔNG pass
        Assert.Equal(1, result.Value!.Passed);
        Assert.Equal(2, result.Value.Total);
        var progress = await db.UserNodeProgress.AsNoTracking().FirstAsync(p => p.NodeId == nodeId);
        Assert.NotEqual(2, progress.Status);   // code SAI (không full) → KHÔNG pass
    }

    [Fact]
    public async Task SubmitCode_ServerJudge_ClientDeclaredHighScore_Ignored()
    {
        var (db, service, exerciseId, nodeId) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_ClientDeclaredHighScore_Ignored));

        // client khai Score=100/Passed=100 nhưng code sai → máy chủ chấm lại = 1/2, KHÔNG pass node
        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest(WrongCode, "asm-1-a"), CancellationToken.None);

        Assert.Equal(1, result.Value!.Score);       // máy chủ chấm, không phải 100 client khai
        Assert.Equal(1, result.Value.Passed);
        var progress = await db.UserNodeProgress.AsNoTracking().FirstAsync(p => p.NodeId == nodeId);
        Assert.NotEqual(2, progress.Status);
    }

    [Fact]
    public async Task SubmitCode_ServerJudge_MissingTaskId_Rejected()
    {
        var (_, service, exerciseId, _) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_MissingTaskId_Rejected));

        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest(CorrectCode, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact]
    public async Task SubmitCode_ServerJudge_UnknownTaskId_Rejected()
    {
        var (_, service, exerciseId, _) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_UnknownTaskId_Rejected));

        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest(CorrectCode, "khong-ton-tai"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact]
    public async Task SubmitCode_ServerJudge_CompileError_NoPass()
    {
        var (db, service, exerciseId, nodeId) = await SeedAsmExerciseAsync(nameof(SubmitCode_ServerJudge_CompileError_NoPass));

        var result = await service.SubmitCodeAsync(1, exerciseId, SubmitRequest("function isValid(s) { this is not js !!! }", "asm-1-a"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Error);   // báo lỗi compile cho client
        Assert.Equal(0, result.Value.Passed);
        var progress = await db.UserNodeProgress.AsNoTracking().FirstAsync(p => p.NodeId == nodeId);
        Assert.NotEqual(2, progress.Status);
    }
}

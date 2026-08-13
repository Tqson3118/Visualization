using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN qua HTTP thật (Testcontainers SQL Server) — findings CAO/TRUNG
/// ExerciseService (docs/work/backend-audit/findings-biz-services.md #1/#5/#6/#7 + findings-security.md #1).
/// Đợt D (review Major #1/#2): thiết kế chống double-submit đổi sang idempotency key OPTIONAL
/// ClientRequestId — test race biz#1 dưới đây đã cập nhật (cùng key → 1 CodeSubmission);
/// Submit_PassAndFailConcurrent_ResponseShowsPassed là test MỚI cho merge response (Major #2).
/// Test race thật đánh dấu [Trait("Category","Race")] — test deterministic nằm ở
/// DsaVisual.UnitTests/ExerciseServiceRegressionTests.cs.
/// </summary>
public sealed class ExerciseRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public ExerciseRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    // ── Helpers ───────────────────────────────────────────────

    private async Task<ExerciseDto> CreateExerciseAsync(
        int teacherId, int lessonId, ExerciseType type, int maxScore,
        int? nodeId = null, List<QuestionUpsertDto>? questions = null)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var request = new ExerciseUpsertRequest
        {
            LessonId = lessonId,
            NodeId = nodeId,
            Stage = nodeId is null ? null : type == ExerciseType.Code ? 3 : 1,
            Title = $"Exercise {Guid.NewGuid():N}",
            Type = type,
            MaxScore = maxScore,
            Status = ExerciseStatus.Active,
            Questions = questions ?? []
        };
        var response = await client.PostAsJsonAsync("/api/v1/exercises", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<ExerciseDto>(response);
    }

    /// <summary>Seed path + node duy nhất (bậc đầu — ladder luôn mở). Trả về node id.</summary>
    private async Task<int> SeedFirstNodeAsync(int ownerId, string suffix)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var path = new LearningPath { Title = $"Path {suffix}", IsActive = true, CreatedBy = ownerId };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();
        var node = new LearningPathNode { PathId = path.Id, Title = $"Node {suffix}", SortOrder = 1 };
        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync();
        return node.Id;
    }

    /// <summary>Seed path 2 bậc (node 1 đã pass, node 2 CHƯA mở). Trả về node2 id.</summary>
    private async Task<int> SeedSecondNodeAsync(int ownerId, string suffix)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var path = new LearningPath { Title = $"Path {suffix}", IsActive = true, CreatedBy = ownerId };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();
        var node1 = new LearningPathNode { PathId = path.Id, Title = $"Node1 {suffix}", SortOrder = 1 };
        var node2 = new LearningPathNode { PathId = path.Id, Title = $"Node2 {suffix}", SortOrder = 2 };
        db.LearningPathNodes.AddRange(node1, node2);
        await db.SaveChangesAsync();
        return node2.Id;
    }

    private static CodeSubmitRequest CodePayload(int score = 10, int passed = 2, int total = 2, string? clientRequestId = null) => new()
    {
        Code = "print('ok')",
        Score = score,
        Passed = passed,
        Total = total,
        ClientRequestId = clientRequestId,
        Results =
        [
            new CodeTestCaseResultDto { TestId = "t1", Passed = true, Input = "5", Expected = "5", Output = "5" }
        ]
    };

    // ── security#1 (CAO): SubmitCodeAsync tin điểm client ──────

    /// <summary>
    /// Bug: server lưu thẳng request.Score vào CodeSubmission + UpsertUserProgressAsync
    /// (ExerciseService.cs:562-575) không clamp/validate → student tự khai Score=100 trên bài MaxScore=10.
    /// Đúng sau fix: Score ≤ MaxScore (clamp hoặc reject; không cập nhật progress từ điểm client).
    /// </summary>
    [Fact(DisplayName = "REPRO security#1: code-submit Score=100 (MaxScore=10) → score KHÔNG được vượt MaxScore (hiện: 100)")]
    public async Task CodeSubmit_ScoreAboveMaxScore_IsClampedOrRejected()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài code", LessonStatus.Active);
        var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Code, maxScore: 10);
        var student = await CreateUserAsync();

        using var client = CreateClientWithToken(student.Id, RoleNames.Student);
        // passed=1/2 nhưng Score=100 — dữ liệu gian lận (Score > MaxScore)
        var response = await client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/code-submit",
            CodePayload(score: 100, passed: 1));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync<CodeSubmitResultDto>(response);
        Assert.True(body.Score <= 10,
            $"Response Score={body.Score} vượt MaxScore=10 — server tin điểm client (security#1)");

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await db.CodeSubmissions.AsNoTracking()
            .Where(s => s.UserId == student.Id && s.ExerciseId == exercise.Id)
            .ToListAsync();
        Assert.All(persisted, s => Assert.True(s.Score <= 10,
            $"CodeSubmission.Score={s.Score} vượt MaxScore=10 (security#1)"));

        var progress = await db.UserProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == student.Id && p.LessonId == lesson.Id);
        if (progress is not null)
        {
            Assert.True((progress.BestScore ?? 0) <= 10,
                $"UserProgress.BestScore={progress.BestScore} vượt MaxScore=10 — điểm client dùng cập nhật tiến độ (security#1)");
        }
    }

    // ── biz#5 (TRUNG): SubmitCodeAsync không sync UserNodeProgress ──

    /// <summary>
    /// Bug: SubmitAsync cập nhật UserNodeProgress trong cùng tx (ExerciseService.cs:343-351)
    /// nhưng SubmitCodeAsync thì KHÔNG → bài CODE thuộc node không bao giờ pass ladder.
    /// </summary>
    [Fact(DisplayName = "REPRO biz#5: code-submit bài có NodeId → UserNodeProgress phải Passed (hiện: không có)")]
    public async Task CodeSubmit_WithNodeId_UpdatesUserNodeProgressToPassed()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài code node", LessonStatus.Active);
        var nodeId = await SeedFirstNodeAsync(teacher.Id, Guid.NewGuid().ToString("N"));
        var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Code, maxScore: 10, nodeId: nodeId);
        var student = await CreateUserAsync();

        using var client = CreateClientWithToken(student.Id, RoleNames.Student);
        var response = await client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/code-submit",
            CodePayload(score: 10, passed: 2));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var nodeProgress = await db.UserNodeProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == student.Id && p.NodeId == nodeId);
        Assert.NotNull(nodeProgress);   // hiện: null — SubmitCodeAsync không sync node progress → FAIL
        Assert.Equal(2, nodeProgress.Status);   // 2 = Passed
        Assert.NotNull(nodeProgress.PassedAt);
    }

    // ── biz#6 (TRUNG): SubmitCodeAsync thiếu validate ladder ──

    /// <summary>
    /// Bug: SubmitAsync check IsPreviousNodePassedAsync (LADDER_LOCKED, ExerciseService.cs:269-272)
    /// nhưng SubmitCodeAsync không check → nộp code bài node chưa mở vẫn được ghi nhận.
    /// </summary>
    [Fact(DisplayName = "REPRO biz#6: code-submit node chưa mở (bậc trước chưa pass) → 422 LADDER_LOCKED (hiện: 200)")]
    public async Task CodeSubmit_LockedNode_ReturnsLadderLocked()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài code khóa", LessonStatus.Active);
        var lockedNodeId = await SeedSecondNodeAsync(teacher.Id, Guid.NewGuid().ToString("N"));
        var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Code, maxScore: 10, nodeId: lockedNodeId);
        var student = await CreateUserAsync();

        using var client = CreateClientWithToken(student.Id, RoleNames.Student);
        var response = await client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/code-submit", CodePayload());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);   // hiện: 200 OK → FAIL
        Assert.Equal(ErrorCodes.LADDER_LOCKED, await GetErrorCodeAsync(response));
    }

    // ── biz#1 (CAO, race): double-submit song song ─────────────

    /// <summary>
    /// Probe race thật (finding biz#1, Đợt D): lock in-process SubmissionLockRegistry chống trùng trong
    /// 1 instance; fix Đợt D thêm idempotency key OPTIONAL — 2 request CÙNG ClientRequestId (retry/
    /// double-click) → CHỈ 1 CodeSubmission được tạo, response thứ 2 trả submission cũ (idempotent).
    /// Invariant: double-click cùng key bị chặn — 1 bản nộp duy nhất.
    /// Test deterministic (2 instance, cùng key) ở unit test SubmitCode_TwoInstances_OnlyOneSubmissionPersisted.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#1 (race, Đợt D): 2 code-submit song song CÙNG clientRequestId → chỉ 1 CodeSubmission (double-click cùng key bị chặn)")]
    public async Task CodeSubmit_TwoConcurrentRequests_OnlyOneSubmissionPersisted()
    {
        for (var i = 0; i < 3; i++)
        {
            var teacher = await CreateUserAsync(role: UserRole.Teacher);
            var topic = await CreateTopicAsync(createdBy: teacher.Id);
            var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài code race", LessonStatus.Active);
            var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Code, maxScore: 10);
            var student = await CreateUserAsync();

            using var client = CreateClientWithToken(student.Id, RoleNames.Student);
            var key = $"double-click-{Guid.NewGuid():N}";
            var responses = await Task.WhenAll(
                client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/code-submit", CodePayload(clientRequestId: key)),
                client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/code-submit", CodePayload(clientRequestId: key)));

            Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
            var first = await ReadJsonAsync<CodeSubmitResultDto>(responses[0]);
            var second = await ReadJsonAsync<CodeSubmitResultDto>(responses[1]);
            // Cùng key → cả 2 response trỏ CÙNG 1 submission (idempotent — lần 2 trả submission cũ)
            Assert.Equal(first.SubmissionId, second.SubmissionId);

            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await db.CodeSubmissions.CountAsync(s => s.UserId == student.Id && s.ExerciseId == exercise.Id);
            Assert.True(count == 1,
                $"Iteration {i}: {count} CodeSubmission được tạo — double-submit cùng key không bị chặn (biz#1)");
        }
    }

    // ── Đợt D (Major #2, race): merge response — request thua race vẫn trả kết quả thực cuối ──

    /// <summary>
    /// Probe race thật (fix Đợt D — Major #2): 2 host API riêng (2 SubmissionLockRegistry khác nhau =
    /// 2 instance) → lock in-process không serialize → 2 request CÙNG clientRequestId đua tới unique
    /// filtered index (IX_ExerciseSubmissions_User_Exercise_Assignment_ClientRequestId); request THUA
    /// (unique violation) đi nhánh merge: mergedScore = max(winner.Score, score), Passed = mergedScore ==
    /// maxScore → response request mang đáp án ĐÚNG phải là Passed=true + Score cao nhất (10) dù thắng
    /// hay thua race; DB giữ ĐÚNG 1 ExerciseSubmission (double-click cùng key bị chặn ở mức DB).
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "NEW Đợt D (race, 2 instance): submit PASS + FAIL song song CÙNG clientRequestId → response PASS Passed=true + score cao nhất (merge: max)")]
    public async Task Submit_PassAndFailConcurrent_ResponseShowsPassed()
    {
        // Instance thứ 2: host WebApplicationFactory riêng → SubmissionLockRegistry singleton RIÊNG
        // (Program.cs đăng ký singleton per host) — cùng database → race thật tới unique index.
        using var factoryB = new ApiFactory(MssqlFixture.GetConnectionString(DatabaseName));
        using var clientB = factoryB.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            var teacher = await CreateUserAsync(role: UserRole.Teacher);
            var topic = await CreateTopicAsync(createdBy: teacher.Id);
            var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài học race merge", LessonStatus.Active);
            var student = await CreateUserAsync();

            var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Mcq, maxScore: 10,
                questions:
                [
                    new QuestionUpsertDto
                    {
                        Content = "1+1=?",
                        Type = QuestionType.Single,
                        Options = ["2", "3"],
                        AnswerJson = "[0]",
                        Points = 10,
                        SortOrder = 0
                    }
                ]);
            var questionId = exercise.Questions.Single().Id;

            using var clientA = CreateClientWithToken(student.Id, RoleNames.Student);
            clientB.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CreateToken(student.Id, RoleNames.Student));

            var key = $"race-pass-fail-{Guid.NewGuid():N}";
            var pass = new SubmitRequest { Answers = [new AnswerDto { QuestionId = questionId, Selected = [0] }], ClientRequestId = key };
            var fail = new SubmitRequest { Answers = [new AnswerDto { QuestionId = questionId, Selected = [1] }], ClientRequestId = key };

            var passTask = clientA.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/submit", pass);
            var failTask = clientB.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/submit", fail);
            await Task.WhenAll(passTask, failTask);

            var passResp = await passTask;
            var failResp = await failTask;
            Assert.Equal(HttpStatusCode.OK, passResp.StatusCode);
            Assert.Equal(HttpStatusCode.OK, failResp.StatusCode);

            var passBody = await ReadJsonAsync<SubmitResultDto>(passResp);
            var failBody = await ReadJsonAsync<SubmitResultDto>(failResp);

            // Request mang đáp án ĐÚNG: dù THẮNG race (insert thường) hay THUA (merge Major #2) →
            // response phải là kết quả thực cuối: Passed=true + score cao nhất (10).
            Assert.True(passBody.Passed,
                $"Iteration {i}: PASS request nhận Passed=false — merge không trả kết quả thực cuối (Major #2)");
            Assert.Equal(10, passBody.Score);
            Assert.Equal(10, passBody.MaxScore);
            // Idempotent: 2 request cùng key → cùng 1 submission duy nhất
            Assert.Equal(passBody.SubmissionId, failBody.SubmissionId);

            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await db.ExerciseSubmissions.CountAsync(s => s.UserId == student.Id && s.ExerciseId == exercise.Id);
            Assert.True(count == 1,
                $"Iteration {i}: {count} ExerciseSubmission được tạo — double-submit cùng key không bị chặn (biz#1)");
            var progress = await db.UserProgress.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == student.Id && p.LessonId == lesson.Id);
            Assert.NotNull(progress);
            Assert.True(progress.BestScore == 10,
                $"Iteration {i}: UserProgress.BestScore={progress.BestScore} — merge không giữ điểm cao nhất (max)");
        }
    }

    // ── biz#7 (TRUNG, race): lost update UserNodeProgress ──────

    /// <summary>
    /// Probe race thật (finding biz#7): 2 submit song song — 1 PASS (Status=2) + 1 FAIL (Status=1).
    /// Nếu request FAIL commit trước rồi request PASS dính DbUpdateConcurrencyException
    /// (catch → CONFLICT → rollback cả submission) → pass BỊ MẤT, Status cuối = 1 → khóa ladder.
    /// Serial-equivalent: PASS ghi Status=2 (FAIL không hạ Status — dòng 807: passed || status==2).
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#7 (race): submit PASS + FAIL song song → UserNodeProgress cuối Status=2 (hiện: có thể mất pass)")]
    public async Task Submit_PassAndFailConcurrent_NodeProgressEndsPassed()
    {
        for (var i = 0; i < 5; i++)
        {
            var teacher = await CreateUserAsync(role: UserRole.Teacher);
            var topic = await CreateTopicAsync(createdBy: teacher.Id);
            var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài học race", LessonStatus.Active);
            var nodeId = await SeedFirstNodeAsync(teacher.Id, Guid.NewGuid().ToString("N"));
            var student = await CreateUserAsync();

            var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id, ExerciseType.Mcq, maxScore: 10,
                nodeId: nodeId,
                questions:
                [
                    new QuestionUpsertDto
                    {
                        Content = "1+1=?",
                        Type = QuestionType.Single,
                        Options = ["2", "3"],
                        AnswerJson = "[0]",
                        Points = 10,
                        SortOrder = 0
                    }
                ]);
            var questionId = exercise.Questions.Single().Id;

            using var client = CreateClientWithToken(student.Id, RoleNames.Student);
            var pass = new SubmitRequest { Answers = [new AnswerDto { QuestionId = questionId, Selected = [0] }] };
            var fail = new SubmitRequest { Answers = [new AnswerDto { QuestionId = questionId, Selected = [1] }] };
            await Task.WhenAll(
                client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/submit", pass),
                client.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/submit", fail));

            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var nodeProgress = await db.UserNodeProgress.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == student.Id && p.NodeId == nodeId);
            Assert.NotNull(nodeProgress);
            Assert.True(nodeProgress.Status == 2 && nodeProgress.PassedAt is not null,
                $"Iteration {i}: Status={nodeProgress.Status} — pass bị mất trong race (biz#7)");
        }
    }

    // ── Test bảo vệ (green): RowVersion chặn ghi đè stale ──────

    /// <summary>
    /// PROTECT (green): UserNodeProgress có RowVersion (Đợt A) → ghi đè stale write phải bị
    /// DbUpdateConcurrencyException, Status pass không bị hạ. Bảo vệ hướng fix #7 không regress.
    /// </summary>
    [Fact(DisplayName = "PROTECT: ghi đè stale UserNodeProgress bị RowVersion chặn (Status giữ 2)")]
    public async Task StaleNodeProgressWrite_ThrowsConcurrency_StatusKeepsPassed()
    {
        var user = await CreateUserAsync();
        var nodeId = await SeedFirstNodeAsync(user.Id, Guid.NewGuid().ToString("N"));
        var progressId = 0;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var progress = new UserNodeProgress
            {
                UserId = user.Id,
                NodeId = nodeId,
                Status = 1,
                Stars = 1,
                NodeScore = 5,
                UnlockedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.UserNodeProgress.Add(progress);
            await db.SaveChangesAsync();
            progressId = progress.Id;
        }

        // ctx1 đọc với rowversion cũ (giả lập request A đọc trước khi B commit)
        byte[] staleVersion;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = await db.UserNodeProgress.AsNoTracking().FirstAsync(x => x.Id == progressId);
            staleVersion = p.RowVersion!;
        }

        // ctx2 = request B (pass): Status 1 → 2, commit
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = await db.UserNodeProgress.FirstAsync(x => x.Id == progressId);
            p.Status = 2;
            p.PassedAt = DateTime.UtcNow;
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        // ctx3 = request A (stale, không pass): ghi Status=1 với rowversion CŨ → EF phải chặn
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = await db.UserNodeProgress.FirstAsync(x => x.Id == progressId);
            db.Entry(p).Property(x => x.RowVersion).OriginalValue = staleVersion;   // giả lập đọc cũ
            p.Status = 1;
            p.PassedAt = null;
            p.UpdatedAt = DateTime.UtcNow;
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        }

        await using var verify = Factory.Services.CreateAsyncScope();
        var dbV = verify.ServiceProvider.GetRequiredService<AppDbContext>();
        var final = await dbV.UserNodeProgress.AsNoTracking().FirstAsync(x => x.Id == progressId);
        Assert.Equal(2, final.Status);   // pass không được hạ
        Assert.NotNull(final.PassedAt);
    }
}

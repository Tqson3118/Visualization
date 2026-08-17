using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// TEST TÁI HIỆN — findings CAO/TRUNG ExerciseService
/// (docs/work/backend-audit/findings-biz-services.md #1/#5/#6 + findings-security.md #1).
/// Mỗi test assert HÀNH VI ĐÚNG dự kiến.
/// Đợt D (review Major #1): thiết kế chống double-submit đổi từ "1 submission vĩnh viễn" sang
/// idempotency key OPTIONAL ClientRequestId (FE chưa gửi → null → re-attempt hợp lệ FR-4.4/FR-9.5) —
/// 2 test biz#1 dưới đây đã cập nhật theo thiết kế mới (GREEN sau fix); test 2d/2e là test MỚI cho
/// hành vi idempotent + merge (Major #2).
/// Race qua HTTP thật nằm ở DsaVisual.IntegrationTests/ExerciseRegressionTests.cs ([Trait("Category","Race")]).
/// KHÔNG sửa code production — chỉ sửa/thêm test.
/// </summary>
public class ExerciseServiceRegressionTests
{
    private readonly TestServices.FixedClock _clock = new();

    // ── Helpers ───────────────────────────────────────────────

    private async Task<AppDbContext> SeedBaseAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Topics.Add(new Topic { Id = 1, Name = "Regression", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Bài học",
            ContentHtml = "<p>x</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();
        return db;
    }

    private async Task<(int PathId, int Node1Id, int Node2Id)> SeedPathAsync(AppDbContext db)
    {
        var path = new LearningPath { Title = "Path", IsActive = true, CreatedBy = 1 };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();

        var node1 = new LearningPathNode { PathId = path.Id, Title = "Node 1", SortOrder = 1 };
        var node2 = new LearningPathNode { PathId = path.Id, Title = "Node 2", SortOrder = 2 };
        db.LearningPathNodes.AddRange(node1, node2);
        await db.SaveChangesAsync();
        return (path.Id, node1.Id, node2.Id);
    }

    /// <summary>
    /// Tạo service + exercise. Nếu <paramref name="existingDb"/> được truyền thì dùng CÙNG store
    /// (InMemory EF chia store theo db name) — cần khi exercise gắn NodeId đã seed trong db đó.
    /// </summary>
    private async Task<(ExerciseService Service, int ExerciseId, AppDbContext Db)> SetupCodeAsync(
        string dbName, int? nodeId = null, SubmissionLockRegistry? locks = null, AppDbContext? existingDb = null)
    {
        var db = existingDb ?? await SeedBaseAsync(dbName);
        var service = locks is null
            ? TestServices.CreateExerciseService(db, _clock)
            : TestServices.CreateExerciseService(db, _clock, locks);
        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            NodeId = nodeId,
            Stage = nodeId is null ? null : 3,           // 3 = CODE
            Title = "Code Regression",
            Type = ExerciseType.Code,
            MaxScore = 10,
            Status = ExerciseStatus.Active
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return (service, created.Value!.Id, db);
    }

    private static CodeSubmitRequest CodeRequest(int score = 10, int passed = 2, int total = 2, string? clientRequestId = null) => new()
    {
        Code = "print('ok')",
        Score = score,
        Passed = passed,
        Total = total,
        ClientRequestId = clientRequestId
    };

    // ── biz#1 (CAO): double-submit qua multi-instance ─────────
    // Finding: SubmissionLockRegistry là khóa in-process per-instance → 2 instance có thể nộp trùng.
    // Fix Đợt D: BỎ unique vĩnh viễn (UserId, ExerciseId) — re-attempt cải thiện điểm hợp lệ (FR-4.4/FR-9.5);
    // thay bằng idempotency key OPTIONAL ClientRequestId: cùng (User, Exercise, ClassAssignment, ClientRequestId)
    // → CHỈ 1 bản nộp được tạo (retry/double-click an toàn — lần 2 trả submission CŨ, idempotent).
    // Test dưới đây: 2 service dùng 2 lock registry riêng = 2 instance.

    [Fact(DisplayName = "REPRO biz#1 (Đợt D): 2 instance nộp MCQ CÙNG clientRequestId → 1 ExerciseSubmission + lần 2 trả submission cũ; KHÁC key → 2 submission (re-attempt hợp lệ)")]
    public async Task Submit_TwoInstances_OnlyOneSubmissionPersisted()
    {
        var db = await SeedBaseAsync(nameof(Submit_TwoInstances_OnlyOneSubmissionPersisted));
        var serviceA = TestServices.CreateExerciseService(db, _clock, new SubmissionLockRegistry());   // instance A
        var serviceB = TestServices.CreateExerciseService(db, _clock, new SubmissionLockRegistry());   // instance B

        var created = await serviceA.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "MCQ Regression",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions =
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
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        // ── CÙNG ClientRequestId (2 instance, lock registry riêng) = retry/double-click → 1 bản nộp duy nhất ──
        var request = new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = created.Value!.Questions[0].Id, Selected = [0] }],
            ClientRequestId = "double-click-mcq-1"
        };
        var first = await serviceA.SubmitAsync(1, created.Value.Id, request, CancellationToken.None);
        var second = await serviceB.SubmitAsync(1, created.Value.Id, request, CancellationToken.None);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, await db.ExerciseSubmissions.CountAsync());   // double-click cùng key bị chặn → 1
        // Lần 2 là IDEMPOTENT: trả submission CŨ (cùng SubmissionId, điểm, thời gian) — không tạo bản nộp mới
        Assert.Equal(first.Value!.SubmissionId, second.Value!.SubmissionId);
        Assert.Equal(first.Value.Score, second.Value.Score);
        Assert.Equal(first.Value.SubmittedAt, second.Value.SubmittedAt);

        // ── KHÁC ClientRequestId (re-attempt cải thiện điểm FR-4.4/FR-9.5): MỖI lần là bản nộp MỚI hợp lệ ──
        var retryB = await serviceB.SubmitAsync(1, created.Value.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = created.Value.Questions[0].Id, Selected = [0] }],
            ClientRequestId = "retry-mcq-2"
        }, CancellationToken.None);
        var retryA = await serviceA.SubmitAsync(1, created.Value.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = created.Value.Questions[0].Id, Selected = [0] }],
            ClientRequestId = "retry-mcq-3"
        }, CancellationToken.None);

        Assert.True(retryB.IsSuccess, retryB.ErrorMessage);
        Assert.True(retryA.IsSuccess, retryA.ErrorMessage);
        Assert.Equal(3, await db.ExerciseSubmissions.CountAsync());   // 1 (cùng key) + 2 (khác key)
    }

    [Fact(DisplayName = "REPRO biz#1 (Đợt D): 2 instance nộp CODE CÙNG clientRequestId → 1 CodeSubmission + lần 2 trả submission cũ; KHÁC key → 2 submission (re-attempt hợp lệ)")]
    public async Task SubmitCode_TwoInstances_OnlyOneSubmissionPersisted()
    {
        var db = await SeedBaseAsync(nameof(SubmitCode_TwoInstances_OnlyOneSubmissionPersisted));
        var serviceA = TestServices.CreateExerciseService(db, _clock, new SubmissionLockRegistry());   // instance A
        var serviceB = TestServices.CreateExerciseService(db, _clock, new SubmissionLockRegistry());   // instance B

        var created = await serviceA.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "Code Regression",
            Type = ExerciseType.Code,
            MaxScore = 10,
            Status = ExerciseStatus.Active
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        // ── CÙNG ClientRequestId (2 instance) = retry/double-click → 1 bản nộp duy nhất ──
        var first = await serviceA.SubmitCodeAsync(1, created.Value!.Id, CodeRequest(clientRequestId: "double-click-code-1"), CancellationToken.None);
        var second = await serviceB.SubmitCodeAsync(1, created.Value.Id, CodeRequest(clientRequestId: "double-click-code-1"), CancellationToken.None);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, await db.CodeSubmissions.CountAsync());   // double-click cùng key bị chặn → 1
        // Lần 2 là IDEMPOTENT: trả submission CŨ (cùng SubmissionId, điểm) — không tạo bản nộp mới
        Assert.Equal(first.Value!.SubmissionId, second.Value!.SubmissionId);
        Assert.Equal(first.Value.Score, second.Value.Score);
        Assert.Equal(first.Value.SubmittedAt, second.Value.SubmittedAt);

        // ── KHÁC ClientRequestId (lịch sử + so sánh 2 lần nộp FR-9.5): MỖI lần là bản nộp MỚI hợp lệ ──
        var retryB = await serviceB.SubmitCodeAsync(1, created.Value.Id, CodeRequest(clientRequestId: "retry-code-2"), CancellationToken.None);
        var retryA = await serviceA.SubmitCodeAsync(1, created.Value.Id, CodeRequest(clientRequestId: "retry-code-3"), CancellationToken.None);

        Assert.True(retryB.IsSuccess, retryB.ErrorMessage);
        Assert.True(retryA.IsSuccess, retryA.ErrorMessage);
        Assert.Equal(3, await db.CodeSubmissions.CountAsync());   // 1 (cùng key) + 2 (khác key)
    }

    // ── Test MỚI (Đợt D): idempotency key trả submission LẦN ĐẦU ──
    // Idempotent response: lần nộp 2 CÙNG ClientRequestId (payload khác) → trả đúng kết quả LẦN ĐẦU
    // (submissionId == lần đầu, score == lần đầu, passed == lần đầu) — retry/double-click không làm mất điểm.

    [Fact(DisplayName = "NEW Đợt D: submit CÙNG clientRequestId → response trả submission LẦN ĐẦU (idempotent: submissionId/score/passed giữ lần đầu)")]
    public async Task Submit_SameClientRequestId_ReturnsFirstSubmission()
    {
        var db = await SeedBaseAsync(nameof(Submit_SameClientRequestId_ReturnsFirstSubmission));
        var service = TestServices.CreateExerciseService(db, _clock);

        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "MCQ Idempotent",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions =
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
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var questionId = created.Value!.Questions[0].Id;

        // Lần 1: đáp án ĐÚNG → 10/10, Passed=true
        var first = await service.SubmitAsync(1, created.Value.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = questionId, Selected = [0] }],
            ClientRequestId = "same-key-1"
        }, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(10, first.Value!.Score);
        Assert.True(first.Value.Passed);

        // Lần 2: CÙNG key nhưng payload SAI (nếu chạy lại sẽ 0 điểm) — idempotent phải trả KẾT QUẢ LẦN ĐẦU
        var second = await service.SubmitAsync(1, created.Value.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = questionId, Selected = [1] }],
            ClientRequestId = "same-key-1"
        }, CancellationToken.None);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value.SubmissionId, second.Value!.SubmissionId);   // response submissionId == lần đầu
        Assert.Equal(first.Value.Score, second.Value.Score);                  // score == lần đầu (không mất điểm)
        Assert.Equal(first.Value.Passed, second.Value.Passed);                // passed == lần đầu
        Assert.Equal(first.Value.SubmittedAt, second.Value.SubmittedAt);
        Assert.Equal(1, await db.ExerciseSubmissions.CountAsync());           // không tạo bản nộp mới
    }

    // ── security#1 (CAO): SubmitCodeAsync tin điểm client ────
    // Finding: server lưu thẳng request.Score/Passed/Total, upsert UserProgress bằng điểm client tự khai.
    // Hướng fix (a): clamp Score vào [0, MaxScore]; (b): không dùng điểm client cập nhật progress.
    // Assert dưới đây đúng với CẢ HAI hướng fix; hiện tại Score=100 vượt MaxScore=10 → FAIL.

    [Fact(DisplayName = "REPRO security#1: code-submit Score=100 (MaxScore=10) → score không được vượt MaxScore (hiện: 100)")]
    public async Task SubmitCode_ScoreAboveMaxScore_IsClampedOrRejected()
    {
        var (service, exerciseId, db) = await SetupCodeAsync(nameof(SubmitCode_ScoreAboveMaxScore_IsClampedOrRejected));

        // Score=100 nhưng chỉ 1/2 test pass — dữ liệu gian lận, server không được chấp nhận mù
        var result = await service.SubmitCodeAsync(1, exerciseId, CodeRequest(score: 100, passed: 1), CancellationToken.None);

        if (result.IsSuccess)
        {
            Assert.True(result.Value!.Score <= 10,
                $"Response Score={result.Value.Score} vượt MaxScore=10 — server tin điểm client (security#1)");
        }
        else
        {
            Assert.NotEqual(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);   // reject hợp lệ cũng OK
        }

        var persisted = await db.CodeSubmissions.AsNoTracking().ToListAsync();
        Assert.All(persisted, s => Assert.True(s.Score <= 10,
            $"CodeSubmission.Score={s.Score} vượt MaxScore=10 — server tin điểm client (security#1)"));

        var progress = await db.UserProgress.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == 1 && p.LessonId == 1);
        if (progress is not null)
        {
            Assert.True((progress.BestScore ?? 0) <= 10,
                $"UserProgress.BestScore={progress.BestScore} vượt MaxScore=10 — điểm client khai được dùng cập nhật tiến độ (security#1)");
        }
    }

    // ── biz#5 (TRUNG): SubmitCodeAsync không sync UserNodeProgress ──
    // Finding: SubmitAsync cập nhật UserNodeProgress (343-351) nhưng SubmitCodeAsync thì không
    // → bài CODE thuộc node không bao giờ pass ladder.

    [Fact(DisplayName = "REPRO biz#5: code-submit bài có NodeId → UserNodeProgress phải Passed (hiện: không có)")]
    public async Task SubmitCode_WithNodeId_UpdatesUserNodeProgressToPassed()
    {
        var db = await SeedBaseAsync(nameof(SubmitCode_WithNodeId_UpdatesUserNodeProgressToPassed));
        var (_, node1Id, _) = await SeedPathAsync(db);
        // LƯU Ý (fix harness): truyền existingDb để node1 nằm CÙNG store với exercise — trước đây
        // SetupCodeAsync tạo store riêng ("_ex") → node không tồn tại → ladder check chặn dù node1
        // là bậc đầu (luôn mở) — không phản ánh lỗi production.
        var (service, exerciseId, _) = await SetupCodeAsync(
            nameof(SubmitCode_WithNodeId_UpdatesUserNodeProgressToPassed), nodeId: node1Id, existingDb: db);

        var result = await service.SubmitCodeAsync(1, exerciseId, CodeRequest(score: 10, passed: 2), CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        var nodeProgress = await db.UserNodeProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == 1 && p.NodeId == node1Id);
        Assert.NotNull(nodeProgress);   // hiện: null — SubmitCodeAsync không sync node progress → FAIL
        Assert.Equal(2, nodeProgress.Status);   // 2 = Passed
        Assert.NotNull(nodeProgress.PassedAt);
    }

    // ── biz#6 (TRUNG): SubmitCodeAsync thiếu validate ladder ──
    // Finding: SubmitAsync check IsPreviousNodePassedAsync (LADDER_LOCKED), SubmitCodeAsync thì không.

    [Fact(DisplayName = "REPRO biz#6: code-submit node chưa mở (bậc trước chưa pass) → LADDER_LOCKED (hiện: 200)")]
    public async Task SubmitCode_LockedNode_ReturnsLadderLocked()
    {
        var db = await SeedBaseAsync(nameof(SubmitCode_LockedNode_ReturnsLadderLocked));
        var (_, _, node2Id) = await SeedPathAsync(db);   // node 2 — bậc trước (node 1) chưa pass
        // LƯU Ý (fix harness): truyền existingDb để node2 nằm CÙNG store với exercise — trước đây
        // SetupCodeAsync tạo store riêng ("_ex") → node không tồn tại → test chỉ xanh "tình cờ"
        // (node null → LADDER_LOCKED) chứ không test đúng bậc thang.
        var (service, exerciseId, _) = await SetupCodeAsync(
            nameof(SubmitCode_LockedNode_ReturnsLadderLocked), nodeId: node2Id, existingDb: db);

        var result = await service.SubmitCodeAsync(1, exerciseId, CodeRequest(score: 10, passed: 2), CancellationToken.None);

        Assert.False(result.IsSuccess);                       // hiện: IsSuccess=true → FAIL
        Assert.Equal(ErrorCodes.LADDER_LOCKED, result.ErrorCode);
        Assert.Equal(0, await db.CodeSubmissions.CountAsync());
    }

    // ── Test bảo vệ (green): SubmitAsync ĐÃ validate ladder ──
    // Đối chứng: cùng tình huống nhưng nộp MCQ qua SubmitAsync → phải LADDER_LOCKED (đã đúng).
    // Giữ test này để fix #6 không regress SubmitAsync.

    [Fact(DisplayName = "PROTECT: SubmitAsync (MCQ) node chưa mở → LADDER_LOCKED (đã đúng, không regress)")]
    public async Task Submit_Mcq_LockedNode_ReturnsLadderLocked()
    {
        var db = await SeedBaseAsync(nameof(Submit_Mcq_LockedNode_ReturnsLadderLocked));
        var (_, _, node2Id) = await SeedPathAsync(db);
        var service = TestServices.CreateExerciseService(db, _clock);

        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            NodeId = node2Id,
            Stage = 1,   // 1 = QUIZ
            Title = "MCQ Locked",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions =
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
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var result = await service.SubmitAsync(1, created.Value!.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = created.Value.Questions[0].Id, Selected = [0] }]
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.LADDER_LOCKED, result.ErrorCode);
    }

    [Fact(DisplayName = "D4: SubmitAsync với Answers rỗng trả về VALIDATION_FAILED thay vì 500")]
    public async Task Submit_EmptyAnswers_ReturnsValidationFailed()
    {
        var db = await SeedBaseAsync(nameof(Submit_EmptyAnswers_ReturnsValidationFailed));
        var service = TestServices.CreateExerciseService(db, _clock);

        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "MCQ Empty Answers",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions =
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
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var result = await service.SubmitAsync(1, created.Value!.Id, new SubmitRequest
        {
            Answers = []
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact(DisplayName = "D4: SubmitAsync với Selected null không ném NullReferenceException")]
    public async Task Submit_NullSelected_DoesNotThrow()
    {
        var db = await SeedBaseAsync(nameof(Submit_NullSelected_DoesNotThrow));
        var service = TestServices.CreateExerciseService(db, _clock);

        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "MCQ Null Selected",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions =
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
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var result = await service.SubmitAsync(1, created.Value!.Id, new SubmitRequest
        {
            Answers = [new AnswerDto { QuestionId = created.Value.Questions[0].Id, Selected = null! }]
        }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.Score);
        Assert.False(result.Value.Passed);
    }
}

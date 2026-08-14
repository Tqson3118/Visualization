using System.Net;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST BẢO VỆ (GREEN) cho findings PERFORMANCE — docs/work/backend-audit/findings-perf.md.
/// Các lỗi perf thuần (N+1, index, pagination offset, aggregate in-memory, loop catalog) KHÔNG đổi
/// hành vi quan sát được — bộ test này chốt CONTRACT hành vi (kết quả đúng, phân trang đúng,
/// không 500) để dev-backend khi fix keyset/index/gộp query không phá contract.
/// Tất cả test phải GREEN ngay bây giờ và vẫn GREEN sau fix.
/// Ghi chú: #19 (thiếu index LastActivityDate) và #23 (SiblingNameExists) — không viết test (xác nhận bằng review).
/// #1/#2/#3/#8 đã fix Đợt D — ngoài phạm vi.
/// </summary>
public sealed class PerformanceGuardRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public PerformanceGuardRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    // ── Helpers ───────────────────────────────────────────────

    private static string UniqueInviteCode() => Guid.NewGuid().ToString("N")[..6];

    /// <summary>Seed path + node duy nhất (bậc đầu — ladder luôn mở). Trả về (pathId, nodeId).</summary>
    private async Task<(int PathId, int NodeId)> SeedFirstNodeAsync(int ownerId, string suffix)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var path = new LearningPath { Title = $"Path {suffix}", IsActive = true, CreatedBy = ownerId };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();
        var node = new LearningPathNode { PathId = path.Id, Title = $"Node {suffix}", SortOrder = 1 };
        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync();
        return (path.Id, node.Id);
    }

    private async Task<ExerciseDto> CreateExerciseAsync(int teacherId, int lessonId)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var request = new ExerciseUpsertRequest
        {
            LessonId = lessonId,
            Title = $"Exercise {Guid.NewGuid():N}",
            Type = ExerciseType.Mcq,
            MaxScore = 10,
            Status = ExerciseStatus.Active,
            Questions = []
        };
        var response = await client.PostAsJsonAsync("/api/v1/exercises", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<ExerciseDto>(response);
    }

    private async Task SetXpAsync(int userId, int xp)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        user.Xp = xp;
        await db.SaveChangesAsync();
    }

    // ── perf#9 (TRUNG): Admin stats 13 CountAsync — bảo vệ con số đúng ──

    /// <summary>
    /// PROTECT (green): GET /admin/stats trả đúng các con số sau khi dev gộp 13 CountAsync thành
    /// ít query hơn (perf#9). Test đo DELTA (baseline trước → sau khi seed) để không phụ thuộc
    /// dữ liệu của test khác trong cùng class-database.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#9: /admin/stats trả đúng các con số (delta sau seed) — gộp query không đổi contract")]
    public async Task AdminStats_AfterSeeding_ReportsExactDeltas()
    {
        var admin = await CreateUserAsync(role: UserRole.Admin);
        using var client = CreateClientWithToken(admin.Id, RoleNames.Admin);

        var baselineResponse = await client.GetAsync("/api/v1/admin/stats");
        Assert.Equal(HttpStatusCode.OK, baselineResponse.StatusCode);
        var baseline = await ReadJsonAsync<StatsDto>(baselineResponse);

        // Seed: 2 student + 1 teacher (admin đã tồn tại ở baseline)
        var studentA = await CreateUserAsync();
        var studentB = await CreateUserAsync();
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài stats");
        var now = DateTime.UtcNow;
        // Mốc "hôm nay" của /admin/stats tính theo UTC+7 (AdminController.GetStats: UtcNow.AddHours(7).Date).
        // Nếu gán LastActivityDate theo UTC gốc, từ 17:00–24:00 UTC (00:00–07:00 VN) `now` thuộc ngày cũ →
        // user không đếm vào ActiveUsersToday → flaky perf#9. nowVn luôn >= mốc today (không cần .Date).
        var nowVn = DateTime.UtcNow.AddHours(7);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var exercise = new Exercise
            {
                LessonId = lesson.Id,
                Title = $"Exercise {Guid.NewGuid():N}",
                Type = ExerciseType.Mcq,
                MaxScore = 10,
                Status = ExerciseStatus.Active,
                CreatedBy = teacher.Id,
                CreatedAt = now
            };
            db.Exercises.Add(exercise);
            await db.SaveChangesAsync();

            db.ExerciseSubmissions.AddRange(
                new ExerciseSubmission { UserId = studentA.Id, ExerciseId = exercise.Id, Score = 8, SubmittedAt = now.AddHours(-2) },
                new ExerciseSubmission { UserId = studentB.Id, ExerciseId = exercise.Id, Score = 10, SubmittedAt = now.AddHours(-1) });

            db.CodeSubmissions.Add(new CodeSubmission
            {
                UserId = studentA.Id,
                ExerciseId = exercise.Id,
                Code = "print('x')",
                Score = 10,
                PassedTests = 2,
                TotalTests = 2,
                SubmittedAt = now.AddMinutes(-30)
            });

            db.Classes.Add(new Class
            {
                Name = $"Class {Guid.NewGuid():N}",
                InviteCode = UniqueInviteCode(),
                OwnerId = teacher.Id,
                Status = ClassStatus.Open,
                CreatedAt = now
            });

            db.Favorites.Add(new Favorite
            {
                UserId = studentA.Id,
                SimulationKey = "sort.bubble",
                CreatedAt = now
            });

            db.LessonSimulations.AddRange(
                new LessonSimulation { LessonId = lesson.Id, SimulationKey = "sort.bubble", Title = "Bubble", SortOrder = 0 },
                new LessonSimulation { LessonId = lesson.Id, SimulationKey = "sort.merge", Title = "Merge", SortOrder = 1 });

            // 1 user hoạt động hôm nay (UTC+7 — khớp mốc today của AdminController.GetStats)
            var active = await db.Users.FirstAsync(u => u.Id == studentB.Id);
            active.LastActivityDate = nowVn;

            await db.SaveChangesAsync();
        }

        var afterResponse = await client.GetAsync("/api/v1/admin/stats");
        Assert.Equal(HttpStatusCode.OK, afterResponse.StatusCode);
        var after = await ReadJsonAsync<StatsDto>(afterResponse);

        Assert.Equal(baseline.TotalUsers + 3, after.TotalUsers);              // 2 student + 1 teacher
        Assert.Equal(baseline.TotalStudents + 2, after.TotalStudents);
        Assert.Equal(baseline.TotalTeachers + 1, after.TotalTeachers);
        Assert.Equal(baseline.TotalAdmins, after.TotalAdmins);                // admin đã có baseline
        Assert.Equal(baseline.TotalTopics + 1, after.TotalTopics);
        Assert.Equal(baseline.TotalLessons + 1, after.TotalLessons);
        Assert.Equal(baseline.TotalExercises + 1, after.TotalExercises);
        Assert.Equal(baseline.TotalSubmissions + 2, after.TotalSubmissions);
        Assert.Equal(baseline.TotalCodeSubmissions + 1, after.TotalCodeSubmissions);
        Assert.Equal(baseline.TotalClasses + 1, after.TotalClasses);
        Assert.Equal(baseline.TotalFavorites + 1, after.TotalFavorites);
        Assert.Equal(baseline.TotalSimulations + 2, after.TotalSimulations);  // Distinct LessonSimulations.SimulationKey
        Assert.Equal(baseline.ActiveUsersToday + 1, after.ActiveUsersToday);
    }

    // ── perf#10 (TRUNG): trace pagination in-memory — bảo vệ contract phân trang ──

    /// <summary>
    /// PROTECT (green): GetTraceAsync hiện parse toàn bộ TraceJson rồi Skip/Take trong memory (perf#10).
    /// Test chốt CONTRACT của GET /code-runs/{id}/trace: trace 5000 events → page size 50 trả đúng 50 items,
    /// total đúng 5000, không 500. Nếu fix = lưu bảng con TraceEvents (phân trang SQL) thì contract này
    /// vẫn phải giữ nguyên (items ≤ limit, tổng đúng, thứ tự theo Index).
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#10: trace 5000 events → page 1/50 trả 50 items, total=5000, không 500")]
    public async Task Trace_LargeTrace_PaginationContractHolds()
    {
        var user = await CreateUserAsync();
        var trace = Enumerable.Range(0, 5000)
            .Select(i => new TraceEventDto { Index = i, Type = "step", Message = $"Event {i}" })
            .ToList();

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);
        var save = await client.PostAsJsonAsync("/api/v1/code-runs", new CodeRunRequest
        {
            Code = "for (let i = 0; i < 5000; i++) {}",
            Key = "sort.bubble",
            Status = nameof(CodeRunStatus.Success),
            Trace = trace
        });
        Assert.Equal(HttpStatusCode.Created, save.StatusCode);
        var run = await ReadJsonAsync<CodeRunDto>(save);

        // Page 1: đúng 50 items, bắt đầu từ Index 0
        var page1Response = await client.GetAsync($"/api/v1/code-runs/{run.Id}/trace?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        var page1 = await ReadJsonAsync<PagedResponse<TraceEventDto>>(page1Response);
        Assert.Equal(50, page1.Items.Count);
        Assert.Equal(5000, page1.Total);
        Assert.Equal(100, page1.TotalPages);
        Assert.Equal(0, page1.Items[0].Index);
        Assert.Equal(49, page1.Items[^1].Index);

        // Page 2: tiếp tục liền mạch (Index 50..99), không lặp lại page 1
        var page2 = await ReadJsonAsync<PagedResponse<TraceEventDto>>(
            await client.GetAsync($"/api/v1/code-runs/{run.Id}/trace?page=2&pageSize=50"));
        Assert.Equal(50, page2.Items.Count);
        Assert.Equal(50, page2.Items[0].Index);
        Assert.Equal(99, page2.Items[^1].Index);

        // Trang cuối + trang quá giới hạn: không 500, tổng vẫn đúng
        var last = await ReadJsonAsync<PagedResponse<TraceEventDto>>(
            await client.GetAsync($"/api/v1/code-runs/{run.Id}/trace?page=100&pageSize=50"));
        Assert.Equal(50, last.Items.Count);
        Assert.Equal(4950, last.Items[0].Index);

        var beyond = await ReadJsonAsync<PagedResponse<TraceEventDto>>(
            await client.GetAsync($"/api/v1/code-runs/{run.Id}/trace?page=101&pageSize=50"));
        Assert.Empty(beyond.Items);
        Assert.Equal(5000, beyond.Total);
    }

    // ── perf#11 (TRUNG): leaderboard class IN-list → EXISTS — bảo vệ đúng thành viên ──

    /// <summary>
    /// PROTECT (green): tab "class" hiện dùng memberIds.Contains (perf#11 — nên chuyển EXISTS).
    /// Contract: chỉ trả thành viên của lớp, đúng thứ tự Xp DESC; user ngoài lớp (kể cả XP cao hơn)
    /// KHÔNG được xuất hiện.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#11: leaderboard tab class → chỉ thành viên lớp, đúng thứ tự Xp")]
    public async Task Leaderboard_ClassTab_OnlyClassMembersOrderedByXp()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var memberLow = await CreateUserAsync();
        var memberMid = await CreateUserAsync();
        var memberHigh = await CreateUserAsync();
        var outsider = await CreateUserAsync();
        await SetXpAsync(memberLow.Id, 100);
        await SetXpAsync(memberMid.Id, 300);
        await SetXpAsync(memberHigh.Id, 500);
        await SetXpAsync(outsider.Id, 1000);   // XP cao nhất nhưng KHÔNG thuộc lớp

        var classId = 0;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cls = new Class
            {
                Name = $"Class {Guid.NewGuid():N}",
                InviteCode = UniqueInviteCode(),
                OwnerId = teacher.Id,
                Status = ClassStatus.Open,
                CreatedAt = DateTime.UtcNow
            };
            db.Classes.Add(cls);
            await db.SaveChangesAsync();
            db.ClassMembers.AddRange(
                new ClassMember { ClassId = cls.Id, UserId = memberLow.Id, JoinedAt = DateTime.UtcNow },
                new ClassMember { ClassId = cls.Id, UserId = memberMid.Id, JoinedAt = DateTime.UtcNow },
                new ClassMember { ClassId = cls.Id, UserId = memberHigh.Id, JoinedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            classId = cls.Id;
        }

        using var client = CreateClientWithToken(memberLow.Id, RoleNames.Student);
        var response = await client.GetAsync($"/api/v1/leaderboard?tab=class&classId={classId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var board = await ReadJsonAsync<PagedResponse<LeaderboardEntryDto>>(response);

        Assert.Equal(3, board.Total);
        Assert.Equal(3, board.Items.Count);
        Assert.Equal([500, 300, 100], board.Items.Select(i => i.Xp).ToArray());
        Assert.Equal([memberHigh.Id, memberMid.Id, memberLow.Id], board.Items.Select(i => i.UserId).ToArray());
        Assert.DoesNotContain(board.Items, i => i.UserId == outsider.Id);   // user ngoài lớp không lọt vào
        Assert.Equal([1, 2, 3], board.Items.Select(i => i.Rank).ToArray());
    }

    // ── perf#4/#5 (TRUNG): leaderboard offset + thiếu index Xp — bảo vệ phân trang ──

    /// <summary>
    /// PROTECT (green): GetLeaderboardAsync hiện dùng CountAsync + OrderBy(Xp).Skip/Take (perf#4/#5).
    /// Contract sau khi chuyển keyset: page 1 = 20 user XP cao nhất, page 2 = 20 tiếp theo,
    /// KHÔNG trùng lặp giữa 2 trang, thứ tự Xp DESC, rank liên tục.
    /// Dùng XP ≥ 100000 để chắc chắn 25 user này chiếm 25 hạng đầu (không phụ thuộc seed test khác).
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#4/#5: leaderboard level page 1 + 2 — không trùng, đúng thứ tự Xp, rank liên tục")]
    public async Task Leaderboard_LevelTab_PagesDoNotOverlapAndStaySorted()
    {
        var requester = await CreateUserAsync();
        var xpUsers = new List<int>();
        for (var i = 0; i < 25; i++)
        {
            var u = await CreateUserAsync();
            await SetXpAsync(u.Id, 100_000 + i);   // XP 100000..100024 — 25 hạng đầu tuyệt đối
            xpUsers.Add(u.Id);
        }

        using var client = CreateClientWithToken(requester.Id, RoleNames.Student);
        var page1 = await ReadJsonAsync<PagedResponse<LeaderboardEntryDto>>(
            await client.GetAsync("/api/v1/leaderboard?tab=level&page=1&pageSize=20"));
        var page2 = await ReadJsonAsync<PagedResponse<LeaderboardEntryDto>>(
            await client.GetAsync("/api/v1/leaderboard?tab=level&page=2&pageSize=20"));

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/leaderboard?tab=level&page=1&pageSize=20")).StatusCode);

        // Page 1 = 20 user XP cao nhất (100005..100024) — top 25 tuyệt đối nhờ XP ≥ 100000.
        Assert.Equal(20, page1.Items.Count);
        Assert.Equal(100_024, page1.Items[0].Xp);
        Assert.Equal(100_005, page1.Items[^1].Xp);

        // Page 2: 5 user của test còn lại (100004..100000) nằm ĐẦU trang (XP cao nhất còn lại) —
        // các user khác trong class-DB (XP 0) lấp phía sau, không được chen vào top 25.
        Assert.True(page2.Items.Count >= 5, $"Page 2 chỉ có {page2.Items.Count} items");
        var page2Top = page2.Items.Take(5).ToArray();
        Assert.Equal(100_004, page2Top[0].Xp);
        Assert.Equal(100_000, page2Top[^1].Xp);

        // Không trùng lặp giữa 2 trang (so 5 user của test ở đầu page 2)
        var page1Ids = page1.Items.Select(i => i.UserId).ToHashSet();
        Assert.DoesNotContain(page2Top, i => page1Ids.Contains(i.UserId));

        // Thứ tự giảm dần theo Xp
        for (var i = 1; i < page1.Items.Count; i++)
        {
            Assert.True(page1.Items[i - 1].Xp > page1.Items[i].Xp, "Page 1 không giảm dần theo Xp");
        }

        // Rank liên tục xuyên 2 trang (1..25) — keyset phải giữ được điều này
        Assert.Equal(1, page1.Items[0].Rank);
        Assert.Equal(20, page1.Items[^1].Rank);
        Assert.Equal(21, page2Top[0].Rank);
        Assert.Equal(25, page2Top[^1].Rank);
        Assert.True(page1.Total >= 25, $"Total={page1.Total} — phải chứa ít nhất 25 user của test");
    }

    // ── perf#6 (TRUNG): submissions offset — bảo vệ phân trang lịch sử bài làm ──

    /// <summary>
    /// PROTECT (green): QuerySubmissionsAsync hiện dùng CountAsync + OrderByDescending(SubmittedAt).Skip/Take (perf#6).
    /// Contract sau keyset: page 1 = 10 bài mới nhất, page 2 = 5 bài tiếp theo, không trùng, SubmittedAt giảm dần.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#6: 15 submissions → page 1 + 2 không trùng, mới nhất trước")]
    public async Task SubmissionsHistory_TwoPages_NoOverlapNewestFirst()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài submissions offset");
        var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id);
        var student = await CreateUserAsync();

        var baseTime = DateTime.UtcNow.AddMinutes(-60);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var i = 0; i < 15; i++)
            {
                db.ExerciseSubmissions.Add(new ExerciseSubmission
                {
                    UserId = student.Id,
                    ExerciseId = exercise.Id,
                    Score = i,
                    SubmittedAt = baseTime.AddMinutes(i)   // i=14 là mới nhất
                });
            }
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);
        var page1 = await ReadJsonAsync<PagedResponse<SubmissionSummaryDto>>(
            await client.GetAsync($"/api/v1/exercises/{exercise.Id}/submissions?page=1&pageSize=10"));
        var page2 = await ReadJsonAsync<PagedResponse<SubmissionSummaryDto>>(
            await client.GetAsync($"/api/v1/exercises/{exercise.Id}/submissions?page=2&pageSize=10"));

        Assert.Equal(15, page1.Total);
        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(14, page1.Items[0].Score);       // mới nhất trước
        Assert.Equal(5, page1.Items[^1].Score);
        Assert.Equal(4, page2.Items[0].Score);
        Assert.Equal(0, page2.Items[^1].Score);

        var page1Ids = page1.Items.Select(i => i.Id).ToHashSet();
        Assert.DoesNotContain(page2.Items, i => page1Ids.Contains(i.Id));
        Assert.Equal(10, page1.Items.Select(i => i.Id).Distinct().Count());
        Assert.Equal(5, page2.Items.Select(i => i.Id).Distinct().Count());
    }

    // ── perf#7 (TRUNG): code submissions offset — bảo vệ phân trang ──

    /// <summary>
    /// PROTECT (green): QueryCodeSubmissionsAsync hiện dùng CountAsync + Skip/Take (perf#7).
    /// Contract sau keyset: page 1 = 10 bài mới nhất, page 2 = 5 bài tiếp theo, không trùng.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#7: 15 code-submissions → page 1 + 2 không trùng, mới nhất trước")]
    public async Task CodeSubmissionsHistory_TwoPages_NoOverlapNewestFirst()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài code submissions offset");
        var exercise = await CreateExerciseAsync(teacher.Id, lesson.Id);
        var student = await CreateUserAsync();

        var baseTime = DateTime.UtcNow.AddMinutes(-60);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var i = 0; i < 15; i++)
            {
                db.CodeSubmissions.Add(new CodeSubmission
                {
                    UserId = student.Id,
                    ExerciseId = exercise.Id,
                    Code = $"print({i})",
                    Score = i,
                    PassedTests = i,
                    TotalTests = 15,
                    SubmittedAt = baseTime.AddMinutes(i)   // i=14 là mới nhất
                });
            }
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);
        var page1 = await ReadJsonAsync<PagedResponse<CodeSubmissionSummaryDto>>(
            await client.GetAsync($"/api/v1/exercises/{exercise.Id}/code-submissions?page=1&pageSize=10"));
        var page2 = await ReadJsonAsync<PagedResponse<CodeSubmissionSummaryDto>>(
            await client.GetAsync($"/api/v1/exercises/{exercise.Id}/code-submissions?page=2&pageSize=10"));

        Assert.Equal(15, page1.Total);
        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(14, page1.Items[0].Score);       // mới nhất trước
        Assert.Equal(4, page2.Items[0].Score);

        var page1Ids = page1.Items.Select(i => i.Id).ToHashSet();
        Assert.DoesNotContain(page2.Items, i => page1Ids.Contains(i.Id));
    }

    // ── perf#21 (THAP): Feedback aggregate in-memory — bảo vệ avg/count đúng ──

    /// <summary>
    /// PROTECT (green): GetSummary hiện tải toàn bộ ContentFeedback rồi Average/Count trong memory (perf#21).
    /// Contract sau khi đẩy aggregate xuống SQL: rating 4,5,3 → avg=4.0, count=3.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#21: feedback rating 4,5,3 → summary avg=4.0, count=3")]
    public async Task FeedbackSummary_ThreeRatings_ComputesAverageAndCount()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài feedback summary");

        var users = new[] { await CreateUserAsync(), await CreateUserAsync(), await CreateUserAsync() };
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ContentFeedback.AddRange(
                new ContentFeedback { UserId = users[0].Id, LessonId = lesson.Id, Rating = 4, CreatedAt = DateTime.UtcNow },
                new ContentFeedback { UserId = users[1].Id, LessonId = lesson.Id, Rating = 5, CreatedAt = DateTime.UtcNow },
                new ContentFeedback { UserId = users[2].Id, LessonId = lesson.Id, Rating = 3, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(users[0].Id, RoleNames.Student);
        var response = await client.GetAsync($"/api/v1/feedback?lessonId={lesson.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await ReadJsonAsync<FeedbackSummaryDto>(response);

        Assert.Equal(lesson.Id, summary.LessonId);
        Assert.Equal(3, summary.Count);
        Assert.Equal(4.0, summary.AvgRating);   // (4+5+3)/3 = 4.0
    }

    // ── perf#17 (THAP): favorites loop catalog — bảo vệ danh sách + meta đúng ──

    /// <summary>
    /// PROTECT (green): GetListAsync gọi catalog.GetByKeyAsync từng lượt trong foreach (perf#17 —
    /// catalog in-memory, không phải N+1 DB). Contract: 5 favorites → 5 items, đủ key, Title/DataStructure
    /// không null, không 500. Fix (map 1 lần bằng GetListAsync) phải giữ contract này.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#17: 5 favorites → list trả đủ 5, meta catalog đầy đủ")]
    public async Task FavoritesList_FiveKeys_AllResolvedWithCatalogMeta()
    {
        var user = await CreateUserAsync();
        var keys = new[] { "sort.bubble", "sort.selection", "sort.insertion", "sort.merge", "sort.quick" };

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);
        foreach (var key in keys)
        {
            var add = await client.PostAsJsonAsync("/api/v1/favorites", new FavoriteUpsertRequest { SimKey = key });
            Assert.Equal(HttpStatusCode.Created, add.StatusCode);
            var dto = await ReadJsonAsync<FavoriteDto>(add);
            Assert.Equal(key, dto.SimulationKey);
            Assert.False(string.IsNullOrWhiteSpace(dto.Title));
        }

        var response = await client.GetAsync("/api/v1/favorites");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await ReadJsonAsync<List<FavoriteDto>>(response);

        Assert.Equal(5, list.Count);
        Assert.Equal(keys.OrderBy(k => k).ToArray(), list.Select(f => f.SimulationKey).OrderBy(k => k).ToArray());
        Assert.All(list, f => Assert.False(string.IsNullOrWhiteSpace(f.Title)));
        Assert.All(list, f => Assert.False(string.IsNullOrWhiteSpace(f.DataStructure)));
    }

    // ── perf#18 (THAP): benchmark loop catalog — bảo vệ fit đủ keys ──

    /// <summary>
    /// PROTECT (green): RunBenchmarkAsync gọi catalog.GetByKeyAsync từng lượt (perf#18 — in-memory).
    /// Contract: 5 keys → 200, Fitted đủ 5 keys với complexity không rỗng, Conclusion không rỗng, không 500.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#18: benchmark 5 keys → Fitted đủ 5 complexity, không 500")]
    public async Task BenchmarkRun_FiveKeys_AllKeysFitted()
    {
        var user = await CreateUserAsync();
        var keys = new[] { "sort.bubble", "sort.selection", "sort.insertion", "sort.merge", "sort.quick" };

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);
        var response = await client.PostAsJsonAsync("/api/v1/benchmarks/run", new BenchmarkRequest
        {
            Keys = keys.ToList(),
            Sizes = [100, 1000],
            Language = "ts",
            Results = keys.Select(key => new BenchmarkResultDto
            {
                Key = key,
                Measurements =
                [
                    new BenchmarkMeasurementDto { N = 100, DurationMs = 1.2, Comparisons = 500, Swaps = 250 },
                    new BenchmarkMeasurementDto { N = 1000, DurationMs = 42.5, Comparisons = 55000, Swaps = 27000 }
                ]
            }).ToList()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadJsonAsync<BenchmarkRunResponse>(response);

        Assert.Equal(5, result.Fitted.Count);
        Assert.All(keys, key => Assert.True(result.Fitted.ContainsKey(key), $"Thiếu key {key} trong Fitted"));
        Assert.All(keys, key => Assert.False(string.IsNullOrWhiteSpace(result.Fitted[key])));
        Assert.False(string.IsNullOrWhiteSpace(result.Conclusion));
        Assert.Equal(5, result.Results.Count);
    }

    // ── perf#22 (THAP): EnterNode đọc/ghi trùng — bảo vệ hành vi gia hạn ──

    /// <summary>
    /// PROTECT (green): EnterNodeAsync gọi PersistHeartRegenAsync 2 lần trong 1 request (perf#22 — đọc/ghi
    /// trùng, không đổi hành vi). Contract: session hết hạn → enter lại gia hạn (không trừ tim lần 2),
    /// hearts giữ nguyên, session mới có ExpiresAt tương lai. Fix (đọc user 1 lần) phải giữ nguyên.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#22: EnterNode 2 lần (session hết hạn) → gia hạn không trừ tim, hearts đúng")]
    public async Task EnterNode_ExpiredSession_RenewsWithoutDeductingHeart()
    {
        var user = await CreateUserAsync();
        var (path, nodeId) = await SeedFirstNodeAsync(user.Id, Guid.NewGuid().ToString("N"));

        await using (var setScope = Factory.Services.CreateAsyncScope())
        {
            var db = setScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = await db.Users.FirstAsync(x => x.Id == user.Id);
            u.Hearts = 5;                                  // không qua chu kỳ regen (LastHeartAt = now)
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        // Lần 1: tạo session mới → trừ 1 tim (5 → 4)
        var first = await client.PostAsJsonAsync($"/api/v1/learning-path/{path}/nodes/{nodeId}/enter", new NodeEnterRequest());
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstResult = await ReadJsonAsync<NodeEnterResultDto>(first);
        Assert.Equal(4, firstResult.HeartsLeft);
        var firstSessionId = firstResult.Session.Id;

        // Làm session hết hạn trong DB (ExpiresAt quá khứ)
        await using (var expireScope = Factory.Services.CreateAsyncScope())
        {
            var db = expireScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.NodeSessions.FirstAsync(s => s.Id == firstSessionId);
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        // Lần 2: session hết hạn → nhánh gia hạn (UPDATE) — KHÔNG trừ tim lần nữa
        var second = await client.PostAsJsonAsync($"/api/v1/learning-path/{path}/nodes/{nodeId}/enter", new NodeEnterRequest());
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondResult = await ReadJsonAsync<NodeEnterResultDto>(second);
        Assert.Equal(4, secondResult.HeartsLeft);          // bug tiềm ẩn: trừ 2 lần → 3

        await using (var verifyScope = Factory.Services.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hearts = await db.Users.AsNoTracking().Where(x => x.Id == user.Id).Select(x => x.Hearts).FirstAsync();
            Assert.Equal(4, hearts);                       // KHÔNG regen sai / không trừ nhầm
            var session = await db.NodeSessions.AsNoTracking().FirstAsync(s => s.Id == firstSessionId);
            Assert.True(session.ExpiresAt > DateTime.UtcNow.AddMinutes(1), "Session phải được gia hạn về tương lai");
        }
    }
}

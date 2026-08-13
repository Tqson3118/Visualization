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
/// TEST TÁI HIỆN (đỏ/fail) qua HTTP thật (Testcontainers SQL Server) — findings
/// ClassService + ProgressService (docs/work/backend-audit/findings-biz-services.md #15,
/// findings-perf.md #1/#2/#3).
/// Test race thật không nằm ở đây (xem ExerciseRegressionTests).
/// </summary>
public sealed class ClassProgressRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public ClassProgressRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    // ── Helpers ───────────────────────────────────────────────

    private async Task<(int TeacherId, int LessonId)> SeedTeacherLessonAsync(string title)
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, title, LessonStatus.Active);
        return (teacher.Id, lesson.Id);
    }

    private async Task<ExerciseDto> CreateExerciseAsync(int teacherId, int lessonId, ExerciseType type = ExerciseType.Mcq)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var request = new ExerciseUpsertRequest
        {
            LessonId = lessonId,
            Title = $"Exercise {Guid.NewGuid():N}",
            Type = type,
            MaxScore = 10,
            Status = ExerciseStatus.Active
        };
        var response = await client.PostAsJsonAsync("/api/v1/exercises", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<ExerciseDto>(response);
    }

    private async Task<ClassDto> CreateClassAsync(int teacherId, string name)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync("/api/v1/classes",
            new ClassUpsertRequest { Name = name, Status = "Open" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<ClassDto>(response);
    }

    private async Task AddMemberAsync(int teacherId, int classId, string email)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync($"/api/v1/classes/{classId}/members",
            new AddMemberRequest { Email = email });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<int> AddAssignmentAsync(int teacherId, int classId, int exerciseId)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync($"/api/v1/classes/{classId}/assignments",
            new ClassAssignmentUpsertRequest { ExerciseId = exerciseId, DueAt = DateTime.UtcNow.AddDays(7) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await ReadJsonAsync<ClassDetailDto>(response);
        return detail.Assignments.Single(a => a.ExerciseId == exerciseId).Id;
    }

    // ── perf#3 (CAO): ProgressService.LoadCountsAsync ──────────

    /// <summary>
    /// Bug: LoadCountsAsync (ProgressService.cs:220-223) query UserProgress KHÔNG filter UserId
    /// → 2+ user progress cùng lesson → ToDictionary(p => p.LessonId) ném ArgumentException → GET
    /// /progress/me trả 500. Đúng sau fix: 200 + chỉ tính progress của user hiện tại.
    /// </summary>
    [Fact(DisplayName = "REPRO perf#3: 2 user progress cùng lesson → GET /progress/me → 200 + chỉ số liệu user hiện tại (hiện: 500)")]
    public async Task ProgressOverview_TwoUsersSameLesson_Returns200()
    {
        var (teacherId, lessonId) = await SeedTeacherLessonAsync("Bài overview");
        var topicName = $"Chủ đề overview {Guid.NewGuid():N}";
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var lesson = await db.Lessons.AsNoTracking().FirstAsync(l => l.Id == lessonId);
            var topicEntity = await db.Topics.FirstAsync(t => t.Id == lesson.TopicId);
            // unique topic name để assert không bị trộn test khác trong cùng database
            topicEntity.Name = topicName;
            await db.SaveChangesAsync();
        }

        var studentA = await CreateUserAsync();
        var studentB = await CreateUserAsync();
        var now = DateTime.UtcNow;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.UserProgress.AddRange(
                new UserProgress { UserId = studentA.Id, LessonId = lessonId, Viewed = true, BestScore = 90, CompletedAt = now, UpdatedAt = now },
                new UserProgress { UserId = studentB.Id, LessonId = lessonId, Viewed = true, BestScore = 50, CompletedAt = now, UpdatedAt = now });
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(studentA.Id, RoleNames.Student);
        var response = await client.GetAsync("/api/v1/progress/me");

        // Hiện: ArgumentException (duplicate key LessonId) → 500 INTERNAL_ERROR → FAIL
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var overview = await ReadJsonAsync<ProgressOverviewDto>(response);
        Assert.Equal(1, overview.LessonsViewed);   // chỉ bài A đã xem
        var topic = overview.Topics.Single(t => t.Name == topicName);
        var lessonItem = topic.Lessons.Single(l => l.Id == lessonId);
        Assert.True(lessonItem.Viewed);
        Assert.Equal(90, lessonItem.BestScore);    // KHÔNG được lấy BestScore của user B (50)
        Assert.True(lessonItem.Completed);
    }

    // ── biz#15 (TRUNG): report lớp lệch khi kick member ────────

    /// <summary>
    /// Bug: GetReportAsync (ClassService.cs:444-457) thống kê submissions KHÔNG filter theo
    /// memberIds hiện tại → submission của member đã rời lớp vẫn tính vào OnTime/Late/Avg,
    /// NotSubmitted bị giảm sai. Đúng sau fix: chỉ tính member đang trong lớp.
    /// </summary>
    [Fact(DisplayName = "REPRO biz#15: report sau khi kick member → KHÔNG tính submission của member đã rời (hiện: vẫn tính)")]
    public async Task ClassReport_RemovedMember_NotCounted()
    {
        var (teacherId, lessonId) = await SeedTeacherLessonAsync("Bài học lớp");
        var exercise = await CreateExerciseAsync(teacherId, lessonId);
        var classRoom = await CreateClassAsync(teacherId, $"Lớp {Guid.NewGuid():N}");

        var memberA = await CreateUserAsync();
        var memberB = await CreateUserAsync();
        await AddMemberAsync(teacherId, classRoom.Id, memberA.Email);
        await AddMemberAsync(teacherId, classRoom.Id, memberB.Email);

        var assignmentId = await AddAssignmentAsync(teacherId, classRoom.Id, exercise.Id);

        // member B đã nộp đúng hạn (Score=9) — sau đó B bị kick
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ExerciseSubmissions.Add(new ExerciseSubmission
            {
                UserId = memberB.Id,
                ExerciseId = exercise.Id,
                ClassAssignmentId = assignmentId,
                Score = 9,
                AnswersJson = "[]",
                ResultJson = "[]",
                SubmittedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var teacherClient = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var remove = await teacherClient.DeleteAsync($"/api/v1/classes/{classRoom.Id}/members/{memberB.Id}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        var reportResponse = await teacherClient.GetAsync($"/api/v1/classes/{classRoom.Id}/report");
        Assert.Equal(HttpStatusCode.OK, reportResponse.StatusCode);
        var report = await ReadJsonAsync<ClassReportDto>(reportResponse);

        Assert.Equal(1, report.TotalMembers);   // chỉ còn member A
        var assignment = report.Assignments.Single(a => a.AssignmentId == assignmentId);
        Assert.Equal(0, assignment.OnTime);         // hiện: 1 (tính cả B đã rời) → FAIL
        Assert.Equal(0, assignment.Late);
        Assert.Equal(1, assignment.NotSubmitted);   // A chưa nộp; hiện: 0 (trừ submitter B) → FAIL
        Assert.Equal(0, assignment.AvgScore);       // hiện: 9 → FAIL
        Assert.DoesNotContain(report.LaggingLearners, l => l.UserId == memberB.Id);
    }

    // ── PROTECT perf#1 (CAO, N+1): GET class detail ────────────

    /// <summary>
    /// PROTECT (green): sau khi fix N+1 (batch title), GET /classes/{id} vẫn trả đủ title
    /// cho từng assignment (lesson-based + exercise-based). Chống regress khi gộp query.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#1: GET class detail trả đủ title assignment (green)")]
    public async Task ClassDetail_AssignmentsHaveTitles()
    {
        var (teacherId, lessonId) = await SeedTeacherLessonAsync("Bài học chi tiết lớp");
        var exercise = await CreateExerciseAsync(teacherId, lessonId);
        var classRoom = await CreateClassAsync(teacherId, $"Lớp {Guid.NewGuid():N}");

        using var teacherClient = CreateClientWithToken(teacherId, RoleNames.Teacher);
        // assignment theo exercise + assignment theo lesson
        await AddAssignmentAsync(teacherId, classRoom.Id, exercise.Id);
        var addLessonAssign = await teacherClient.PostAsJsonAsync($"/api/v1/classes/{classRoom.Id}/assignments",
            new ClassAssignmentUpsertRequest { LessonId = lessonId, DueAt = DateTime.UtcNow.AddDays(3) });
        Assert.Equal(HttpStatusCode.OK, addLessonAssign.StatusCode);

        var get = await teacherClient.GetAsync($"/api/v1/classes/{classRoom.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var detail = await ReadJsonAsync<ClassDetailDto>(get);

        Assert.Equal(2, detail.Assignments.Count);
        Assert.All(detail.Assignments, a => Assert.False(string.IsNullOrWhiteSpace(a.Title),
            $"Assignment {a.Id} thiếu Title — N+1 fix phải trả đủ title"));
        Assert.Contains(detail.Assignments, a => a.Title == exercise.Title);
        Assert.Contains(detail.Assignments, a => a.Title == "Bài học chi tiết lớp");
    }

    // ── PROTECT perf#2 (CAO, N+1): report đếm đúng ─────────────

    /// <summary>
    /// PROTECT (green): sau khi fix N+1 + projection gọn, GetReportAsync vẫn đếm đúng
    /// OnTime/Late/NotSubmitted/AvgScore cho member hiện tại.
    /// </summary>
    [Fact(DisplayName = "PROTECT perf#2: report đếm đúng OnTime/Late/NotSubmitted/Avg (green)")]
    public async Task ClassReport_CountsCorrect()
    {
        var (teacherId, lessonId) = await SeedTeacherLessonAsync("Bài học báo cáo");
        var exercise = await CreateExerciseAsync(teacherId, lessonId);
        var classRoom = await CreateClassAsync(teacherId, $"Lớp {Guid.NewGuid():N}");

        var member = await CreateUserAsync();
        await AddMemberAsync(teacherId, classRoom.Id, member.Email);
        var assignmentId = await AddAssignmentAsync(teacherId, classRoom.Id, exercise.Id);

        // member nộp đúng hạn Score=7
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ExerciseSubmissions.Add(new ExerciseSubmission
            {
                UserId = member.Id,
                ExerciseId = exercise.Id,
                ClassAssignmentId = assignmentId,
                Score = 7,
                AnswersJson = "[]",
                ResultJson = "[]",
                SubmittedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var teacherClient = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await teacherClient.GetAsync($"/api/v1/classes/{classRoom.Id}/report");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await ReadJsonAsync<ClassReportDto>(response);

        var assignment = report.Assignments.Single(a => a.AssignmentId == assignmentId);
        Assert.Equal(1, report.TotalMembers);
        Assert.Equal(1, assignment.OnTime);
        Assert.Equal(0, assignment.Late);
        Assert.Equal(0, assignment.NotSubmitted);
        Assert.Equal(7, assignment.AvgScore);
    }
}

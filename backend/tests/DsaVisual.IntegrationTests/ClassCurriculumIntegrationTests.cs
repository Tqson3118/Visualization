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
/// Integration tests cho Lộ trình học (Curriculum) per-class qua HTTP thật (Testcontainers SQL Server):
/// permission owner, draft gating, student status từ dữ liệu progress thật.
/// </summary>
public sealed class ClassCurriculumIntegrationTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public ClassCurriculumIntegrationTests(ApiTestFixture fixture) : base(fixture) { }

    private async Task<(User TeacherA, User TeacherB, User Student, ClassDto Class)> SeedAsync()
    {
        var teacherA = await CreateUserAsync(role: UserRole.Teacher);
        var teacherB = await CreateUserAsync(role: UserRole.Teacher);
        var student = await CreateUserAsync();
        var topic = await CreateTopicAsync(createdBy: teacherA.Id);
        await CreateLessonAsync(topic.Id, teacherA.Id, $"Lesson {Guid.NewGuid():N}", LessonStatus.Active);

        using var client = CreateClientWithToken(teacherA.Id, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync("/api/v1/classes",
            new ClassUpsertRequest { Name = $"Lớp {Guid.NewGuid():N}", Status = "Open" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var classRoom = await ReadJsonAsync<ClassDto>(response);
        return (teacherA, teacherB, student, classRoom);
    }

    private async Task<ClassDetailDto> UpdateCurriculumAsync(int teacherId, int classId, ClassCurriculumUpsertRequest body)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PutAsJsonAsync($"/api/v1/classes/{classId}/curriculum", body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadJsonAsync<ClassDetailDto>(response);
    }

    private async Task<int> AddLessonAssignmentAsync(int teacherId, int classId)
    {
        var topic = await CreateTopicAsync(createdBy: teacherId);
        var lesson = await CreateLessonAsync(topic.Id, teacherId, $"Les {Guid.NewGuid():N}", LessonStatus.Active);
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync($"/api/v1/classes/{classId}/assignments",
            new ClassAssignmentUpsertRequest { LessonId = lesson.Id });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await ReadJsonAsync<ClassDetailDto>(response);
        return detail.Assignments.Single(a => a.LessonId == lesson.Id).Id;
    }

    [Fact(DisplayName = "TeacherB không thể sửa lộ trình lớp của TeacherA → 403 FORBIDDEN")]
    public async Task NonOwnerTeacher_CannotUpdateCurriculum()
    {
        var (teacherA, teacherB, _, classRoom) = await SeedAsync();
        await AddLessonAssignmentAsync(teacherA.Id, classRoom.Id);

        using var client = CreateClientWithToken(teacherB.Id, RoleNames.Teacher);
        var response = await client.PutAsJsonAsync($"/api/v1/classes/{classRoom.Id}/curriculum",
            new ClassCurriculumUpsertRequest { Title = "Hack", Published = true });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(ErrorCodes.FORBIDDEN, await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "Student không thể sửa/reorder lộ trình → 403 (role gate)")]
    public async Task Student_CannotMutateCurriculum()
    {
        var (teacherA, _, student, classRoom) = await SeedAsync();
        await AddLessonAssignmentAsync(teacherA.Id, classRoom.Id);

        using var s1 = CreateClientWithToken(student.Id, RoleNames.Student);
        var update = await s1.PutAsJsonAsync($"/api/v1/classes/{classRoom.Id}/curriculum",
            new ClassCurriculumUpsertRequest { Title = "X" });
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);

        using var s2 = CreateClientWithToken(student.Id, RoleNames.Student);
        var reorder = await s2.PutAsJsonAsync($"/api/v1/classes/{classRoom.Id}/curriculum/reorder",
            new ClassCurriculumReorderRequest { Items = [new() { AssignmentId = 1, SortOrder = 0 }] });
        Assert.Equal(HttpStatusCode.Forbidden, reorder.StatusCode);
    }

    [Fact(DisplayName = "Student không thấy items khi lộ trình còn draft; thấy khi publish")]
    public async Task Student_DraftHidden_ThenPublishedVisible()
    {
        var (teacherA, _, student, classRoom) = await SeedAsync();
        var assignmentId = await AddLessonAssignmentAsync(teacherA.Id, classRoom.Id);
        await AddStudentMemberAsync(teacherA.Id, classRoom.Id, student);

        // Save draft → student không thấy items qua GET /classes/{id} AND /classes/{id}/curriculum
        await UpdateCurriculumAsync(teacherA.Id, classRoom.Id, new ClassCurriculumUpsertRequest
        {
            Title = "Graph cơ bản",
            Description = "Lộ trình mẫu",
            Published = false,
        });

        using var sc1 = CreateClientWithToken(student.Id, RoleNames.Student);
        var detailResponse = await sc1.GetAsync($"/api/v1/classes/{classRoom.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var studentDetail = await ReadJsonAsync<ClassDetailDto>(detailResponse);
        Assert.False(studentDetail.CurriculumPublished);
        Assert.Empty(studentDetail.Assignments);

        var curriculumResponse = await sc1.GetAsync($"/api/v1/classes/{classRoom.Id}/curriculum");
        var curriculum = await ReadJsonAsync<ClassCurriculumDto>(curriculumResponse);
        Assert.False(curriculum.Published);
        Assert.Empty(curriculum.Items);

        // Publish → student thấy 1 item, status in_progress, order đúng
        await UpdateCurriculumAsync(teacherA.Id, classRoom.Id, new ClassCurriculumUpsertRequest { Published = true });

        var published = await ReadJsonAsync<ClassCurriculumDto>(await sc1.GetAsync($"/api/v1/classes/{classRoom.Id}/curriculum"));
        Assert.True(published.Published);
        Assert.Single(published.Items);
        Assert.Equal(assignmentId, published.Items[0].AssignmentId);
        Assert.Equal("in_progress", published.Items[0].Status);
    }

    [Fact(DisplayName = "Student status completed được tính từ progress thật (UserProgress/ExerciseSubmission)")]
    public async Task Student_Status_FromRealProgress()
    {
        var (teacherA, _, student, classRoom) = await SeedAsync();
        var topic = await CreateTopicAsync(createdBy: teacherA.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacherA.Id, $"Les {Guid.NewGuid():N}", LessonStatus.Active);
        var exercise = await CreateExerciseAsync(teacherA.Id, lesson.Id);
        await AddStudentMemberAsync(teacherA.Id, classRoom.Id, student);

        // 2 items: lesson + exercise
        using var addClient = CreateClientWithToken(teacherA.Id, RoleNames.Teacher);
        var add1 = await addClient.PostAsJsonAsync($"/api/v1/classes/{classRoom.Id}/assignments",
            new ClassAssignmentUpsertRequest { LessonId = lesson.Id });
        var detail1 = await ReadJsonAsync<ClassDetailDto>(add1);
        var a1 = detail1.Assignments.Single(x => x.LessonId == lesson.Id).Id;
        var add2 = await addClient.PostAsJsonAsync($"/api/v1/classes/{classRoom.Id}/assignments",
            new ClassAssignmentUpsertRequest { ExerciseId = exercise.Id });
        var detail2 = await ReadJsonAsync<ClassDetailDto>(add2);
        var a2 = detail2.Assignments.Single(x => x.ExerciseId == exercise.Id).Id;

        await UpdateCurriculumAsync(teacherA.Id, classRoom.Id, new ClassCurriculumUpsertRequest { Published = true });

        // Progress THẬT: lesson completed; exercise chưa nộp
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.UserProgress.Add(new UserProgress
            {
                UserId = student.Id, LessonId = lesson.Id, Viewed = true, BestScore = 100,
                CompletedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(student.Id, RoleNames.Student);
        var curriculum = await ReadJsonAsync<ClassCurriculumDto>(
            await client.GetAsync($"/api/v1/classes/{classRoom.Id}/curriculum"));

        Assert.Equal(2, curriculum.Items.Count);
        var byId = curriculum.Items.ToDictionary(i => i.AssignmentId);
        Assert.Equal("completed", byId[a1].Status);   // lesson completed (real progress)
        Assert.Equal("in_progress", byId[a2].Status); // exercise chưa nộp → item đang dở đầu tiên
        Assert.Equal(50, curriculum.ProgressPct);
    }

    private async Task AddStudentMemberAsync(int teacherId, int classId, User student)
    {
        using var client = CreateClientWithToken(teacherId, RoleNames.Teacher);
        var response = await client.PostAsJsonAsync($"/api/v1/classes/{classId}/members",
            new AddMemberRequest { Email = student.Email });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        };
        var response = await client.PostAsJsonAsync("/api/v1/exercises", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<ExerciseDto>(response);
    }
}

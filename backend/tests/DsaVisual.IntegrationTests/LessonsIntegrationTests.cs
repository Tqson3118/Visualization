using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Integration tests Lessons — TEST_PLAN §4.2 (TEST-B-026..030) + TEST-API-001/002/003.
/// Mỗi test tự seed user/topic/lesson qua DbContext — MỌI email/tên topic đều unique
/// (database dùng chung trong class, unique index IX_Users_Email / IX_Topics_Name).
/// Các assert đếm số lượng (X-Total-Count / Total) phải lọc theo topic riêng của test
/// vì lesson của test khác cũng nằm trong cùng database.
/// </summary>
public sealed class LessonsIntegrationTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/lessons";

    public LessonsIntegrationTests(ApiTestFixture fixture) : base(fixture) { }

    [Fact(DisplayName = "TEST-B-027 + TEST-API-002: Student GET list → 200, chỉ active, PagedResponse contract + X-Total-Count")]
    public async Task GetLessons_AsStudent_ReturnsOnlyActive_WithPagedContract()
    {
        // Arrange — seed: 2 active + 1 draft (topic riêng của test)
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        await CreateLessonAsync(topic.Id, teacher.Id, "Bài 1", LessonStatus.Active);
        await CreateLessonAsync(topic.Id, teacher.Id, "Bài 2", LessonStatus.Active);
        await CreateLessonAsync(topic.Id, teacher.Id, "Bài nháp", LessonStatus.Draft);

        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act — lọc theo topic riêng để không bị trộn lesson của test khác
        var response = await client.GetAsync($"{BaseUrl}?topicId={topic.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("2", response.Headers.GetValues("X-Total-Count").Single());

        var paged = await ReadJsonAsync<PagedResponse<LessonSummaryDto>>(response);
        Assert.Equal(2, paged.Items.Count);
        Assert.Equal(2, paged.Total);
        Assert.Equal(1, paged.Page);
        Assert.Equal(20, paged.PageSize);
        Assert.Equal(1, paged.TotalPages);
        Assert.All(paged.Items, item => Assert.Equal("active", item.Status));
        Assert.Contains(paged.Items, item => item.Title == "Bài 1");
        Assert.DoesNotContain(paged.Items, item => item.Title == "Bài nháp");
    }

    [Fact(DisplayName = "TEST-B-032: Teacher GET /{id} includeContent=true → 200 + content")]
    public async Task GetLesson_AsTeacher_IncludeContent_Returns200()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Mảng một chiều", LessonStatus.Active,
            "<p>Nội dung bài mảng</p>");
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{lesson.Id}?includeContent=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync<LessonDto>(response);
        Assert.Equal(lesson.Id, body.Id);
        Assert.Equal("Mảng một chiều", body.Title);
        Assert.Equal("<p>Nội dung bài mảng</p>", body.ContentHtml);
        Assert.Equal("active", body.Status);
    }

    [Fact(DisplayName = "TEST-API-003: GET /lessons/{id} không tồn tại → 404 NOT_FOUND")]
    public async Task GetLesson_MissingId_Returns404()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var response = await client.GetAsync($"{BaseUrl}/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("NOT_FOUND", await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "TEST-B-027: Student GET /{id} bài draft → 404 (không lộ bản nháp)")]
    public async Task GetLesson_AsStudent_Draft_Returns404()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var draft = await CreateLessonAsync(topic.Id, teacher.Id, "Bài nháp", LessonStatus.Draft);

        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{draft.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "TEST-B-026: Teacher POST tạo lesson → 201 + CreatedAtAction + draft")]
    public async Task CreateLesson_AsTeacher_Returns201()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        var request = new LessonUpsertRequest
        {
            TopicId = topic.Id,
            Title = "Bài học mới",
            Description = "Mô tả",
            ContentHtml = "<p>Nội dung bài học mới</p>",
            Status = LessonStatus.Draft,
            SortOrder = 1
        };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/api/v1/lessons/", location);

        var body = await ReadJsonAsync<LessonDto>(response);
        Assert.True(body.Id > 0);
        Assert.Equal("Bài học mới", body.Title);
        Assert.Equal("draft", body.Status);

        // Verify trong DB — CreatedBy đúng teacher
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DsaVisual.Application.Persistence.AppDbContext>();
        var saved = await db.Lessons.FindAsync(body.Id);
        Assert.NotNull(saved);
        Assert.Equal(teacher.Id, saved.CreatedBy);
    }

    [Fact(DisplayName = "TEST-API-001: Student POST /lessons → 403 FORBIDDEN")]
    public async Task CreateLesson_AsStudent_Returns403()
    {
        // Arrange — seed topic hợp lệ để đảm bảo 403 đến từ quyền, không phải lỗi khác
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        var request = new LessonUpsertRequest
        {
            TopicId = topic.Id,
            Title = "Bài học trái phép",
            ContentHtml = "<p>Nội dung</p>"
        };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert — 403 từ [Authorize(Roles="TEACHER,ADMIN")] (ForbidResult — body rỗng, không có envelope error)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }

    [Fact(DisplayName = "TEST-B-030: content chứa <script> → sanitize, KHÔNG còn script")]
    public async Task CreateLesson_ContentWithScript_IsSanitized()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        var request = new LessonUpsertRequest
        {
            TopicId = topic.Id,
            Title = "Bài có script",
            ContentHtml = "<p>Hello</p><script>alert('xss')</script>",
            Status = LessonStatus.Active
        };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert — 201 và nội dung lưu không chứa script
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadJsonAsync<LessonDto>(response);
        Assert.DoesNotContain("<script", created.ContentHtml);
        Assert.Contains("<p>Hello</p>", created.ContentHtml);

        var get = await client.GetAsync($"{BaseUrl}/{created.Id}?includeContent=true");
        var fetched = await ReadJsonAsync<LessonDto>(get);
        Assert.DoesNotContain("<script", fetched.ContentHtml);
    }

    [Fact(DisplayName = "TEST-B-029: xóa mềm → 204, bài biến mất khỏi danh sách")]
    public async Task DeleteLesson_SoftDeletes_RemovesFromList()
    {
        // Arrange — topic riêng để kiểm đếm không bị trộn lesson test khác
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài sẽ xóa", LessonStatus.Active);
        using var teacherClient = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var delete = await teacherClient.DeleteAsync($"{BaseUrl}/{lesson.Id}");

        // Assert — 204 (NoContent)
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // Student GET list (lọc topic riêng) — không còn bài
        var student = await CreateUserAsync();
        using var studentClient = CreateClientWithToken(student.Id, RoleNames.Student);
        var list = await studentClient.GetAsync($"{BaseUrl}?topicId={topic.Id}");
        var paged = await ReadJsonAsync<PagedResponse<LessonSummaryDto>>(list);
        Assert.Equal(0, paged.Total);

        // GET trực tiếp → 404
        var get = await teacherClient.GetAsync($"{BaseUrl}/{lesson.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact(DisplayName = "TEST-B-028: Teacher sửa bài không phải của mình → 403 FORBIDDEN")]
    public async Task UpdateLesson_ByAnotherTeacher_Returns403()
    {
        // Arrange — teacher A tạo bài, teacher B sửa
        var teacherA = await CreateUserAsync(role: UserRole.Teacher);
        var teacherB = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacherA.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacherA.Id, "Bài của A", LessonStatus.Active);

        using var clientB = CreateClientWithToken(teacherB.Id, RoleNames.Teacher);
        var request = new LessonUpsertRequest
        {
            TopicId = topic.Id,
            Title = "Bài bị sửa bởi B",
            ContentHtml = "<p>Nội dung sửa</p>",
            Status = LessonStatus.Active
        };

        // Act
        var response = await clientB.PutAsJsonAsync($"{BaseUrl}/{lesson.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("FORBIDDEN", await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "TEST-API-003: POST /lessons body không hợp lệ → 400 VALIDATION_FAILED")]
    public async Task CreateLesson_InvalidBody_Returns400()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        var request = new LessonUpsertRequest
        {
            TopicId = topic.Id,
            Title = "Ab",                       // < 3 ký tự
            ContentHtml = "<p>x</p>"            // sanitize < 10 ký tự
        };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = doc.RootElement.GetProperty("error");
        Assert.Equal("VALIDATION_FAILED", error.GetProperty("code").GetString());
        Assert.True(error.GetProperty("details").GetArrayLength() > 0);
    }

    [Fact(DisplayName = "TEST-B-033: mark-viewed → 204 + upsert UserProgress (1 bản ghi, Viewed=true)")]
    public async Task MarkViewed_AsStudent_CreatesUserProgress()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài đánh dấu đã học", LessonStatus.Active);
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act
        var response = await client.PostAsync($"{BaseUrl}/{lesson.Id}/mark-viewed", null);

        // Assert — 204 NoContent
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // UserProgress đã upsert: Viewed=true, 1 bản ghi (User, Lesson)
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var progress = await db.UserProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == student.Id && p.LessonId == lesson.Id);
        Assert.NotNull(progress);
        Assert.True(progress.Viewed);
        Assert.Equal(1, await db.UserProgress.CountAsync(p => p.UserId == student.Id && p.LessonId == lesson.Id));
    }

    [Fact(DisplayName = "TEST-B-034: mark-viewed lần 2 → KHÔNG nhân đôi bản ghi")]
    public async Task MarkViewed_SecondCall_DoesNotDuplicate()
    {
        // Arrange
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var lesson = await CreateLessonAsync(topic.Id, teacher.Id, "Bài mark 2 lần", LessonStatus.Active);
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act — mark 2 lần
        var first = await client.PostAsync($"{BaseUrl}/{lesson.Id}/mark-viewed", null);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        var second = await client.PostAsync($"{BaseUrl}/{lesson.Id}/mark-viewed", null);

        // Assert — lần 2 vẫn 204 và KHÔNG có bản ghi thứ 2
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.UserProgress.CountAsync(p => p.UserId == student.Id && p.LessonId == lesson.Id));
    }

    [Fact(DisplayName = "mark-viewed bài không tồn tại → 404 NOT_FOUND")]
    public async Task MarkViewed_MissingLesson_Returns404()
    {
        // Arrange
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act
        var response = await client.PostAsync($"{BaseUrl}/999999/mark-viewed", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("NOT_FOUND", await GetErrorCodeAsync(response));
    }
}

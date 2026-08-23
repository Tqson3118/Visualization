using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Integration tests Topics (controller đợt D) — TEST_PLAN §4.2 (TEST-B-023..025) + TEST-API-001/003.
/// Mỗi test tự seed user/topic/lesson qua DbContext — MỌI email/tên topic đều unique
/// (database dùng chung trong class, unique index IX_Users_Email / IX_Topics_Name).
/// GET /topics trả TOÀN BỘ cây (không filter) → test cây phải tìm node theo tên unique,
/// không được Assert.Single trên toàn cây (topic của test khác cũng nằm trong response).
/// </summary>
public sealed class TopicsIntegrationTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/topics";

    public TopicsIntegrationTests(ApiTestFixture fixture) : base(fixture) { }

    [Fact(DisplayName = "TEST-B-023: Teacher POST tạo topic → 201 + CreatedAtAction")]
    public async Task CreateTopic_AsTeacher_Returns201()
    {
        // Arrange — tên topic unique riêng cho test này
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        var topicName = $"Cấu trúc dữ liệu {Guid.NewGuid():N}";
        var request = new TopicUpsertRequest { Name = topicName, Description = "Nhập môn", SortOrder = 0 };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/api/v1/topics/", location);

        var body = await ReadJsonAsync<TopicDto>(response);
        Assert.True(body.Id > 0);
        Assert.Equal(topicName, body.Name);
    }

    [Fact(DisplayName = "TEST-B-023: GET /topics trả cây 2 cấp (parent + children)")]
    public async Task GetTopics_ReturnsNestedTree()
    {
        // Arrange — tên parent/child unique, tìm node trong cây theo tên
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var parentName = $"Cấu trúc dữ liệu {Guid.NewGuid():N}";
        var childName = $"Mảng {Guid.NewGuid():N}";
        var parent = await CreateTopicAsync(parentName, teacher.Id);
        await CreateTopicAsync(childName, teacher.Id, parentId: parent.Id);

        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tree = await ReadJsonAsync<List<TopicDto>>(response);
        var root = Assert.Single(tree, t => t.Name == parentName);
        Assert.Equal(parentName, root.Name);
        var child = Assert.Single(root.Children, c => c.Name == childName);
        Assert.Equal(childName, child.Name);
        Assert.Equal(root.Id, child.ParentId);
    }

    [Fact(DisplayName = "TEST-B-024: xóa topic có bài học → 409 TOPIC_HAS_LESSONS")]
    public async Task DeleteTopic_WithLessons_Returns409_TopicHasLessons()
    {
        // Arrange — topic unique (bài học seed thuộc topic này)
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        await CreateLessonAsync(topic.Id, teacher.Id, "Bài trong topic", LessonStatus.Active);

        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var response = await client.DeleteAsync($"{BaseUrl}/{topic.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("TOPIC_HAS_LESSONS", await GetErrorCodeAsync(response));

        // Topic vẫn còn (xóa không thành công)
        var get = await client.GetAsync($"{BaseUrl}/{topic.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact(DisplayName = "TEST-B-025: tạo topic tên trùng (không phân biệt hoa thường) → 400 VALIDATION_FAILED")]
    public async Task CreateTopic_DuplicateName_Returns400()
    {
        // Arrange — tên unique cho CẶP request (lần 1 tạo được, lần 2 trùng)
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);
        var topicName = $"Arrays{Guid.NewGuid():N}";

        var first = await client.PostAsJsonAsync(BaseUrl, new TopicUpsertRequest { Name = topicName });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Act — tên trùng nhưng khác hoa thường
        var response = await client.PostAsJsonAsync(BaseUrl, new TopicUpsertRequest { Name = topicName.ToLowerInvariant() });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = doc.RootElement.GetProperty("error");
        Assert.Equal("VALIDATION_FAILED", error.GetProperty("code").GetString());
        Assert.Equal("name", error.GetProperty("field").GetString());
    }

    [Fact(DisplayName = "TEST-B-023: PUT sửa topic → 200 + tên mới")]
    public async Task UpdateTopic_Returns200()
    {
        // Arrange — topic + tên mới unique
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var topic = await CreateTopicAsync(createdBy: teacher.Id);
        var newName = $"Cấu trúc dữ liệu nâng cao {Guid.NewGuid():N}";
        using var client = CreateClientWithToken(teacher.Id, RoleNames.Teacher);

        // Act
        var response = await client.PutAsJsonAsync($"{BaseUrl}/{topic.Id}",
            new TopicUpsertRequest { Name = newName, SortOrder = 5 });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync<TopicDto>(response);
        Assert.Equal(newName, body.Name);
        Assert.Equal(5, body.SortOrder);
    }

    [Fact(DisplayName = "TEST-API-001: Student POST /topics → 403 FORBIDDEN")]
    public async Task CreateTopic_AsStudent_Returns403()
    {
        // Arrange
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, new TopicUpsertRequest { Name = $"Trái phép {Guid.NewGuid():N}" });

        // Assert — 403 từ [Authorize(Roles="TEACHER,ADMIN")] (exc#4b: envelope { error } §2.1 — KHÔNG body rỗng)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("FORBIDDEN", await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "TEST-API-003: GET /topics/{id} không tồn tại → 404 NOT_FOUND")]
    public async Task GetTopic_MissingId_Returns404()
    {
        // Arrange
        var student = await CreateUserAsync();
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        // Act
        var response = await client.GetAsync($"{BaseUrl}/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("NOT_FOUND", await GetErrorCodeAsync(response));
    }
}

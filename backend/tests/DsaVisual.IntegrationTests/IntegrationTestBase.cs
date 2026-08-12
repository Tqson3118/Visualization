using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Base class cho integration tests: fixture container SQL Server + WebApplicationFactory&lt;Program&gt;,
/// helper tạo JWT token test (đúng issuer/audience/secret của app — HS256) và seed dữ liệu
/// qua DbContext (mỗi test tự seed — KHÔNG phụ thuộc thứ tự chạy).
/// </summary>
[Collection("Mssql")]
public abstract class IntegrationTestBase
{
    /// <summary>Secret test ≥ 32 ký tự — khớp giá trị override trong <see cref="ApiFactory"/>.</summary>
    public const string TestJwtSecret = "test-only-secret-0123456789abcdef0123456789abcdef";

    private const string Issuer = "DsaVisual.Api";
    private const string Audience = "DsaVisual.Frontend";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApiTestFixture _fixture;

    protected IntegrationTestBase(ApiTestFixture fixture) => _fixture = fixture;

    protected HttpClient Client => _fixture.Client;

    protected ApiFactory Factory => _fixture.Factory;

    protected string DatabaseName => _fixture.DatabaseName;

    // ── JWT test ────────────────────────────────────────────

    /// <summary>Tạo JWT hợp lệ (HS256, claims sub + role) — JwtBearer middleware của app chấp nhận.</summary>
    protected string CreateToken(int userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>HttpClient mới có sẵn Authorization header Bearer token.</summary>
    protected HttpClient CreateClientWithToken(int userId, string role)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(userId, role));
        return client;
    }

    // ── Seed qua DbContext (độc lập dữ liệu cho từng test) ──
    //
    // MỘT database dùng chung cho MỌI test trong cùng class (IClassFixture<ApiTestFixture>)
    // → MỌI seed phải dùng giá trị unique (email/tên topic) — cấm email/tên cố định lặp lại
    // giữa các test (unique index IX_Users_Email / IX_Topics_Name sẽ từ chối).
    // Mặc định không truyền email/name → tự sinh unique.

    /// <summary>Email test unique — dùng cho seed trực tiếp qua DbContext hoặc đăng ký qua API.</summary>
    protected static string UniqueEmail(string prefix = "user") => $"{prefix}_{Guid.NewGuid():N}@test.local";

    protected async Task<User> CreateUserAsync(
        string? email = null, string password = "MatKhau@123", UserRole role = UserRole.Student, string displayName = "Test User")
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            Email = string.IsNullOrWhiteSpace(email) ? UniqueEmail() : email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHasher.Hash(password),
            DisplayName = displayName,
            Role = role,
            IsActive = true,
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    protected async Task<Topic> CreateTopicAsync(
        string? name = null, int createdBy = 0, int? parentId = null, string? description = null, int sortOrder = 0)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var topic = new Topic
        {
            ParentId = parentId,
            Name = string.IsNullOrWhiteSpace(name) ? $"Chủ đề {Guid.NewGuid():N}" : name,
            Description = description,
            SortOrder = sortOrder,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        db.Topics.Add(topic);
        await db.SaveChangesAsync();
        return topic;
    }

    protected async Task<Lesson> CreateLessonAsync(
        int topicId, int createdBy, string title = "Bài học kiểm thử",
        LessonStatus status = LessonStatus.Active, string contentHtml = "<p>Nội dung bài học kiểm thử</p>")
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var lesson = new Lesson
        {
            TopicId = topicId,
            Title = title,
            Description = "Mô tả bài học",
            ContentHtml = contentHtml,
            SortOrder = 0,
            Status = status,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();
        return lesson;
    }

    // ── JSON helpers ────────────────────────────────────────

    protected static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return value ?? throw new InvalidOperationException($"Không đọc được JSON body dạng {typeof(T).Name}");
    }

    protected static async Task<string?> GetErrorCodeAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.TryGetProperty("error", out var error)
            ? error.GetProperty("code").GetString()
            : null;
    }

    /// <summary>HttpClient không token — dùng chung (không có Authorization header).</summary>
    protected HttpClient CreateClientWithoutToken() => Factory.CreateClient();
}

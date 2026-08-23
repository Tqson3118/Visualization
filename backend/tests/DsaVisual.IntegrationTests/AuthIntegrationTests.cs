using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Integration tests Auth — TEST_PLAN §4.1 (TEST-B-001..005) + TEST-API-003 (401 không token).
/// Mỗi test tự seed qua endpoint register / DbContext — độc lập dữ liệu.
/// </summary>
public sealed class AuthIntegrationTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/auth";

    public AuthIntegrationTests(ApiTestFixture fixture) : base(fixture) { }

    [Fact(DisplayName = "TEST-B-001: register hợp lệ → 201 + accessToken + role STUDENT")]
    public async Task Register_ValidRequest_Returns201_WithToken()
    {
        // Arrange — email unique riêng cho test này (database dùng chung trong class)
        var email = UniqueEmail("minh");
        var request = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = email,
            Password = "MatKhau@123",
            IsTeacher = false
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", request);

        // Assert — 201 + contract
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadJsonAsync<RefreshResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(3600, body.ExpiresIn);
        Assert.Equal("STUDENT", body.User.Role);
        Assert.Equal(email, body.User.Email);   // email chuẩn hóa lowercase
        Assert.True(body.User.Id > 0);

        // Đăng nhập lại được bằng đúng email/mật khẩu (TEST-B-001 bước 3)
        var login = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest
        {
            Email = email,
            Password = "MatKhau@123"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact(DisplayName = "TEST-B-002: register email trùng → 409 EMAIL_EXISTS")]
    public async Task Register_DuplicateEmail_Returns409_EmailExists()
    {
        // Arrange — 1 email unique cho CẶP register này (lần 1 thành công, lần 2 phải trùng)
        var email = UniqueEmail("dup");
        var request = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = email,
            Password = "MatKhau@123"
        };
        var first = await Client.PostAsJsonAsync($"{BaseUrl}/register", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Act — register lại cùng email (hoa thường khác nhau → vẫn trùng do chuẩn hóa lowercase)
        var duplicate = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = email.ToUpperInvariant(),
            Password = "MatKhau@123"
        };
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", duplicate);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = doc.RootElement.GetProperty("error");
        Assert.Equal("EMAIL_EXISTS", error.GetProperty("code").GetString());
        Assert.Equal("email", error.GetProperty("field").GetString());

        // KHÔNG tạo tài khoản mới
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Email == email));
    }

    [Fact(DisplayName = "TEST-B-003: password yếu → 400 WEAK_PASSWORD + details")]
    public async Task Register_WeakPassword_Returns400_WeakPassword()
    {
        // Arrange — "matkhau" thiếu chữ hoa/số/ký tự đặc biệt
        var request = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = UniqueEmail("weak"),
            Password = "matkhau"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var error = doc.RootElement.GetProperty("error");
        Assert.Equal("WEAK_PASSWORD", error.GetProperty("code").GetString());
        var details = error.GetProperty("details");
        Assert.True(details.GetArrayLength() >= 3, "details phải liệt kê từng quy tắc vi phạm");
    }

    [Fact(DisplayName = "Register email sai định dạng → 400 INVALID_EMAIL")]
    public async Task Register_InvalidEmail_Returns400_InvalidEmail()
    {
        // Arrange
        var request = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = "khong-phai-email",
            Password = "MatKhau@123"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_EMAIL", await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "TEST-B-004: login đúng → 200 + accessToken + cookie refresh_token")]
    public async Task Login_ValidCredentials_Returns200_WithToken()
    {
        // Arrange — user riêng cho test này
        var email = UniqueEmail("login");
        var register = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = email,
            Password = "MatKhau@123"
        };
        var created = await Client.PostAsJsonAsync($"{BaseUrl}/register", register);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest
        {
            Email = email,
            Password = "MatKhau@123"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync<RefreshResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(3600, body.ExpiresIn);
        Assert.Equal("STUDENT", body.User.Role);

        // Cookie refresh_token (HttpOnly)
        var setCookie = response.Headers.SingleOrDefault(h => h.Key == "Set-Cookie").Value;
        Assert.NotNull(setCookie);
        Assert.Contains("refresh_token", string.Join(";", setCookie));
    }

    [Fact(DisplayName = "F5-Minor: login trên HTTP (dev) → cookie refresh KHÔNG có Secure (trước fix Secure=true chặn cookie)")]
    public async Task Login_OverHttp_CookieIsNotSecure()
    {
        // Arrange — TestServer chạy HTTP (Request.IsHttps=false) — mô phỏng dev local
        var email = UniqueEmail("http");
        var register = new RegisterRequest
        {
            DisplayName = "Dev HTTP",
            Email = email,
            Password = "MatKhau@123"
        };
        var created = await Client.PostAsJsonAsync($"{BaseUrl}/register", register);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest
        {
            Email = email,
            Password = "MatKhau@123"
        });

        // Assert — cookie vẫn có nhưng KHÔNG đánh dấu Secure (không bị chặn khi gửi lại qua HTTP)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var setCookie = response.Headers.SingleOrDefault(h => h.Key == "Set-Cookie").Value;
        Assert.NotNull(setCookie);
        Assert.Contains("refresh_token", string.Join(";", setCookie));
        Assert.DoesNotContain("Secure", string.Join(";", setCookie), StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "TEST-B-005: login sai mật khẩu → 401 INVALID_CREDENTIALS")]
    public async Task Login_WrongPassword_Returns401_InvalidCredentials()
    {
        // Arrange — user riêng cho test này
        var email = UniqueEmail("login");
        var register = new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = email,
            Password = "MatKhau@123"
        };
        var created = await Client.PostAsJsonAsync($"{BaseUrl}/register", register);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest
        {
            Email = email,
            Password = "SaiMatKhau@999"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("INVALID_CREDENTIALS", await GetErrorCodeAsync(response));
    }

    [Fact(DisplayName = "TEST-API-003: GET /api/v1/lessons không token → 401")]
    public async Task GetLessons_WithoutToken_Returns401()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "E2E regression MapInboundClaims: login thật → accessToken thật gọi GET /api/v1/lessons → 200 (không 500)")]
    public async Task Login_RealAccessToken_CallsAuthenticatedEndpoint_Returns200()
    {
        // Arrange — user qua register thật HTTP (KHÔNG dùng CreateToken helper)
        var email = UniqueEmail("e2e");
        var register = new RegisterRequest
        {
            DisplayName = "End-to-End",
            Email = email,
            Password = "MatKhau@123"
        };
        var created = await Client.PostAsJsonAsync($"{BaseUrl}/register", register);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Act — login thật qua HTTP lấy access token do TokenService phát hành
        var login = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest
        {
            Email = email,
            Password = "MatKhau@123"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await ReadJsonAsync<RefreshResponse>(login);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));

        // Gọi endpoint [Authorize] bằng token thật — trước fix: claim "sub" bị map thành
        // ClaimTypes.NameIdentifier → CurrentUserId() đọc "sub" = null → NRE → 500.
        using var authed = Factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);

        // Assert — 200 + contract paged (bằng chứng bug đã hết, không còn 500)
        var lessons = await authed.GetAsync("/api/v1/lessons");
        Assert.Equal(HttpStatusCode.OK, lessons.StatusCode);
        Assert.True(lessons.Headers.Contains("X-Total-Count"));
        var paged = await ReadJsonAsync<PagedResponse<LessonSummaryDto>>(lessons);
        Assert.NotNull(paged);
        Assert.NotNull(paged.Items);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) qua HTTP thật (Testcontainers SQL Server) — AuthService + exception handling:
/// - findings-biz-services.md #2 (register race), #3 (refresh TOCTOU), #8 (reset-password race), #9 (verify 2FA race)
/// - findings-exception.md #4 (model binding → envelope), #5 (JWT thiếu/malformed claim sub → 401)
/// Mỗi test assert HÀNH VI ĐÚNG dự kiến sau fix — hiện tại PHẢI FAIL vì bug chưa sửa.
/// KHÔNG sửa code production — chỉ test.
/// Test race thật (Task.WhenAll) đánh dấu [Trait("Category","Race")] — có thể ổn định không tuyệt đối;
/// phần deterministic (unit) nằm ở DsaVisual.UnitTests/AuthServiceRegressionTests.cs.
/// Ghi chú trạng thái hiện tại: User có RowVersion (Đợt A) nên race reset-password/verify-2FA loser
/// trả 409 CONFLICT (DbUpdateConcurrencyException) — VẪN SAI: phải là RESET_TOKEN_INVALID/OTP_USED.
/// </summary>
public sealed class AuthRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/auth";

    public AuthRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    /// <summary>Lấy refresh token từ Set-Cookie (RefreshToken có [JsonIgnore] — không có trong body).</summary>
    private static string ExtractRefreshTokenFromCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.SingleOrDefault(h => h.Key == "Set-Cookie").Value;
        Assert.NotNull(setCookie);
        var pair = setCookie
            .Select(c => c.Split(';')[0].Trim())
            .FirstOrDefault(c => c.StartsWith("refresh_token=", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(pair);
        return pair!["refresh_token=".Length..];
    }

    private static string HashOtp(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code))).ToLowerInvariant();

    /// <summary>JWT hợp lệ (HS256) — sub claim tùy biến (null = thiếu claim).</summary>
    private static string CreateTokenWithSub(string? sub, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        if (sub is not null)
        {
            claims.Insert(0, new Claim(JwtRegisteredClaimNames.Sub, sub));
        }

        var token = new JwtSecurityToken(
            issuer: "DsaVisual.Api",
            audience: "DsaVisual.Frontend",
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(60),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── biz#2 + exc#1 (CAO): Register race → 1×201 + 1×409 EMAIL_EXISTS, KHÔNG 503 ──

    /// <summary>
    /// Bug: RegisterAsync (AuthService.cs:51-95) check AnyAsync(email) rồi INSERT — 2 request song song
    /// cùng email cùng pass check → DbUpdateException (UNIQUE Email) → ErrorHandlingMiddleware trả
    /// 503 SERVICE_UNAVAILABLE (ErrorHandlingMiddleware.cs:48) thay vì 409 EMAIL_EXISTS.
    /// Đúng sau fix: 1 request 201, 1 request 409 EMAIL_EXISTS.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#2/exc#1: 2 register cùng email song song → 1×201 + 1×409 EMAIL_EXISTS (hiện 503)")]
    public async Task Register_TwoConcurrentSameEmail_OneSucceedsOne409EmailExists()
    {
        for (var i = 0; i < 3; i++)
        {
            var email = UniqueEmail("race-reg");
            var request = new RegisterRequest
            {
                DisplayName = "Nguyễn Minh",
                Email = email,
                Password = "MatKhau@123"
            };

            // B0: 2 request dùng CHUNG 1 otpToken — người thắng tiêu token, người thua vấp EMAIL_EXISTS
            request.OtpToken = await GetRegisterOtpTokenAsync(email);
            var responses = await Task.WhenAll(
                Client.PostAsJsonAsync($"{BaseUrl}/register", request),
                Client.PostAsJsonAsync($"{BaseUrl}/register", request));

            var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
            Assert.True(created == 1,
                $"Iteration {i}: 2 register song song cùng email → ĐÚNG 1 request 201; hiện {created} (race check-then-insert — finding biz#2)");

            var loser = responses.First(r => r.StatusCode != HttpStatusCode.Created);
            Assert.Equal(HttpStatusCode.Conflict, loser.StatusCode);   // bug: 503 SERVICE_UNAVAILABLE (DbUpdateException)
            Assert.Equal("EMAIL_EXISTS", await GetErrorCodeAsync(loser));

            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, await db.Users.CountAsync(u => u.Email == email));
        }
    }

    // ── biz#3 (CAO): Refresh TOCTOU race → chỉ 1 phiên hợp lệ ──

    /// <summary>
    /// Bug: RefreshAsync (AuthService.cs:146-211) đọc token AsNoTracking (154) → check RevokedAt (173)
    /// → tx (186) FirstAsync + revoke + insert token mới — KHÔNG khóa row/re-check trong tx. 2 refresh
    /// song song cùng token → cả 2 đọc token còn sống → cả 2 rotate → 2 chuỗi phiên hợp lệ (TOCTOU).
    /// RefreshTokens KHÔNG có RowVersion → race chưa được chặn bởi concurrency token.
    /// Đúng sau fix: đúng 1 request thành công; ≤ 1 refresh token active; tối đa 1 token mới dùng lại được.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#3 (TOCTOU): 2 refresh song song cùng token → chỉ 1 phiên hợp lệ (hiện 2 token)")]
    public async Task Refresh_TwoConcurrentSameToken_SingleSessionRemainsValid()
    {
        for (var i = 0; i < 3; i++)
        {
            var email = UniqueEmail("race-ref");
            var register = await RegisterWithOtpAsync(new RegisterRequest
            {
                DisplayName = "Nguyễn Minh",
                Email = email,
                Password = "MatKhau@123"
            });
            Assert.Equal(HttpStatusCode.Created, register.StatusCode);
            var refreshToken = ExtractRefreshTokenFromCookie(register);
            Assert.False(string.IsNullOrWhiteSpace(refreshToken));

            // 2 request refresh dùng ĐÚNG cùng refresh token (cookie cố định — refresh endpoint đọc từ cookie)
            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={refreshToken}");
            var responses = await Task.WhenAll(
                client.PostAsync($"{BaseUrl}/refresh", null),
                client.PostAsync($"{BaseUrl}/refresh", null));

            var ok = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            Assert.True(ok == 1,
                $"Iteration {i}: 2 refresh song song cùng token → ĐÚNG 1 request thành công; hiện {ok} (TOCTOU — finding biz#3)");

            // DB: ≤ 1 refresh token active (hiện race: 2 token mới cùng active)
            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userId = await db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstAsync();
            var active = await db.RefreshTokens.CountAsync(t =>
                t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
            Assert.True(active <= 1,
                $"Iteration {i}: chỉ ≤ 1 refresh token còn hiệu lực; hiện {active} (session duplication — finding #3)");

            // Mỗi token mới trả về chỉ được dùng lại tối đa 1 lần (tối đa 1 chuỗi còn sống)
            var tokens = new List<string>();
            foreach (var response in responses.Where(r => r.StatusCode == HttpStatusCode.OK))
            {
                var body = await ReadJsonAsync<RefreshResponse>(response);
                if (!string.IsNullOrWhiteSpace(body.RefreshToken))
                {
                    tokens.Add(body.RefreshToken);
                }
            }

            var usable = 0;
            foreach (var token in tokens)
            {
                using var probe = Factory.CreateClient();
                probe.DefaultRequestHeaders.Add("Cookie", $"refresh_token={token}");
                var reuse = await probe.PostAsync($"{BaseUrl}/refresh", null);
                if (reuse.StatusCode == HttpStatusCode.OK)
                {
                    usable++;
                }
            }
            Assert.True(usable <= 1,
                $"Iteration {i}: tối đa 1 trong các token mới còn dùng được; hiện {usable} (finding #3)");
        }
    }

    // ── biz#8 (TRUNG): ResetPassword race → token dùng đúng 1 lần ──

    /// <summary>
    /// Bug: ResetPasswordAsync (AuthService.cs:361-405) đọc token AsNoTracking (364) → check Used (367)
    /// → tx FirstAsync (383) set Used + đổi mật khẩu — KHÔNG re-check Used trong tx → token dùng 2 lần.
    /// Trạng thái hiện tại: User có RowVersion → loser thường trả 409 CONFLICT (DbUpdateConcurrencyException)
    /// thay vì 400 RESET_TOKEN_INVALID; race hẹp vẫn có thể cả 2 thành công (mật khẩu last-write-wins).
    /// Đúng sau fix: 1×200 + 1×400 RESET_TOKEN_INVALID; mật khẩu của request thắng không bị ghi đè.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#8: 2 reset-password song song cùng token → token dùng đúng 1 lần (loser RESET_TOKEN_INVALID)")]
    public async Task ResetPassword_TwoConcurrentSameToken_OnlyOneSucceeds()
    {
        for (var i = 0; i < 3; i++)
        {
            var email = UniqueEmail("race-reset");
            var user = await CreateUserAsync(email);

            // Token reset đã biết (không cần đọc log — tái hiện đúng trạng thái DB sau forgot-password).
            // Lưu ý: TokenHash có UNIQUE constraint → mỗi iteration dùng token riêng (tránh setup vỡ ở vòng 2).
            var rawToken = $"race-reset-token-repro-0123456789abcdef-{i}";
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.PasswordResetTokens.Add(new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = HashToken(rawToken),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    Used = false,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var responses = await Task.WhenAll(
                Client.PostAsJsonAsync($"{BaseUrl}/reset-password", new ResetPasswordRequest { Token = rawToken, NewPassword = "PwdA@12345" }),
                Client.PostAsJsonAsync($"{BaseUrl}/reset-password", new ResetPasswordRequest { Token = rawToken, NewPassword = "PwdB@12345" }));

            // Reset-password success = 204 NoContent (MapResult → NoContent); fail = 400 envelope
            var ok = responses.Count(r => r.StatusCode == HttpStatusCode.NoContent);
            Assert.True(ok == 1,
                $"Iteration {i}: 2 reset-password song song cùng token → ĐÚNG 1 request thành công; hiện {ok} — statuses: [{string.Join(", ", responses.Select(r => (int)r.StatusCode))}] (finding biz#8)");

            // Loser PHẢI là 400 RESET_TOKEN_INVALID — hiện 409 CONFLICT (DbUpdateConcurrencyException) hoặc 204
            var loser = responses.First(r => r.StatusCode != HttpStatusCode.NoContent);
            Assert.Equal(HttpStatusCode.BadRequest, loser.StatusCode);
            Assert.Equal("RESET_TOKEN_INVALID", await GetErrorCodeAsync(loser));

            // Token không dùng được lần 3 (single-use)
            var third = await Client.PostAsJsonAsync($"{BaseUrl}/reset-password",
                new ResetPasswordRequest { Token = rawToken, NewPassword = "PwdC@12345" });
            Assert.Equal(HttpStatusCode.BadRequest, third.StatusCode);
            Assert.Equal("RESET_TOKEN_INVALID", await GetErrorCodeAsync(third));

            // Đúng 1 mật khẩu có hiệu lực (mật khẩu không bị last-write-wins ghi đè)
            var loginA = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest { Email = email, Password = "PwdA@12345" });
            var loginB = await Client.PostAsJsonAsync($"{BaseUrl}/login", new LoginRequest { Email = email, Password = "PwdB@12345" });
            var loginOk = new[] { loginA, loginB }.Count(r => r.StatusCode == HttpStatusCode.OK);
            Assert.True(loginOk == 1,
                $"Iteration {i}: ĐÚNG 1 mật khẩu có hiệu lực; hiện {loginOk} (last-write-wins — finding biz#8)");
        }
    }

    // ── biz#9 (TRUNG): Verify2FA race → OTP dùng đúng 1 lần ──

    /// <summary>
    /// Bug: Verify2FaCodeAsync (AuthService.cs:500-561) đọc OTP AsNoTracking (521) → check Used (531)
    /// → tx FirstAsync (543) set Used + bật 2FA — KHÔNG re-check Used trong tx → OTP dùng 2 lần.
    /// Trạng thái hiện tại: User có RowVersion → loser thường 409 CONFLICT thay vì 400 OTP_USED;
    /// race hẹp vẫn có thể cả 2 thành công (2FA bật "2 lần" cùng 1 OTP).
    /// Đúng sau fix: 1×200 + 1×400 OTP_USED.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO biz#9: 2 verify 2FA song song cùng OTP → OTP dùng đúng 1 lần (loser OTP_USED)")]
    public async Task Verify2Fa_TwoConcurrentSameOtp_OnlyOneSucceeds()
    {
        for (var i = 0; i < 3; i++)
        {
            var user = await CreateUserAsync();
            const string otp = "123456";
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.OtpCodes.Add(new OtpCode
                {
                    UserId = user.Id,
                    CodeHash = HashOtp(otp),
                    Purpose = "enable_2fa",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    Used = false,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            using var client = CreateClientWithToken(user.Id, RoleNames.Student);
            var responses = await Task.WhenAll(
                client.PostAsJsonAsync($"{BaseUrl}/2fa/verify", new Verify2FaRequest { Code = otp }),
                client.PostAsJsonAsync($"{BaseUrl}/2fa/verify", new Verify2FaRequest { Code = otp }));

            var ok = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            Assert.True(ok == 1,
                $"Iteration {i}: 2 verify 2FA song song cùng OTP → ĐÚNG 1 request thành công; hiện {ok} (finding biz#9)");

            // Loser PHẢI là 400 OTP_USED — hiện 409 CONFLICT (DbUpdateConcurrencyException) hoặc 200
            var loser = responses.First(r => r.StatusCode != HttpStatusCode.OK);
            Assert.Equal(HttpStatusCode.BadRequest, loser.StatusCode);
            Assert.Equal("OTP_USED", await GetErrorCodeAsync(loser));

            // 2FA đã bật đúng 1 lần (contract: OTP single-use)
            await using var scope2 = Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.True(await db2.Users.Where(u => u.Id == user.Id).Select(u => u.TwoFactorEnabled).FirstAsync());
            Assert.True(await db2.OtpCodes.AnyAsync(o => o.UserId == user.Id && o.Used));
        }
    }

    // ── exc#5 (THAP): JWT thiếu/malformed claim sub → 401, KHÔNG 500 ──

    /// <summary>
    /// Bug: ApiControllerBase.CurrentUserId() (ApiControllerBase.cs:13) `int.Parse(User.FindFirst(Sub)!.Value)`
    /// — token hợp lệ nhưng thiếu claim sub → NullReferenceException → 500 INTERNAL_ERROR.
    /// Đúng sau fix: 401 (claim thiếu/malformed → xác thực không hợp lệ).
    /// </summary>
    [Fact(DisplayName = "REPRO exc#5: JWT thiếu claim sub → /auth/me phải 401 (hiện 500)")]
    public async Task GetMe_TokenWithoutSubClaim_Returns401()
    {
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTokenWithSub(sub: null, role: RoleNames.Student));

        var response = await client.GetAsync($"{BaseUrl}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);   // bug: 500 INTERNAL_ERROR (NRE)
    }

    /// <summary>
    /// Bug: claim sub không phải số nguyên → int.Parse ném FormatException → 500 INTERNAL_ERROR.
    /// Đúng sau fix: 401.
    /// </summary>
    [Fact(DisplayName = "REPRO exc#5: JWT sub không phải số → /auth/me phải 401 (hiện 500)")]
    public async Task GetMe_TokenWithNonNumericSub_Returns401()
    {
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTokenWithSub(sub: "abc", role: RoleNames.Student));

        var response = await client.GetAsync($"{BaseUrl}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);   // bug: 500 INTERNAL_ERROR (FormatException)
    }

    // ── exc#4 (TRUNG): 400 model binding fail → envelope { error }, KHÔNG ProblemDetails ──

    /// <summary>
    /// Bug: [ApiController] (Program.cs:42 AddControllers không tùy biến ApiBehaviorOptions) → 400 model
    /// binding fail trả ProblemDetails (RFC 7807) — client phải xử lý định dạng khác envelope { error }
    /// (API_REFERENCE §2.1). Đúng sau fix: body phải có error.code.
    /// </summary>
    [Fact(DisplayName = "REPRO exc#4: 400 model binding fail (body sai kiểu) → envelope { error } có code (hiện ProblemDetails)")]
    public async Task Login_ModelBindingFailure_ReturnsErrorEnvelope()
    {
        // email là số → không bind được vào LoginRequest.Email (string)
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login",
            new Dictionary<string, object> { ["email"] = 123, ["password"] = "MatKhau@123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var code = await GetErrorCodeAsync(response);
        Assert.NotNull(code);   // bug: ProblemDetails — không có envelope { error } (finding exc#4)
    }
}

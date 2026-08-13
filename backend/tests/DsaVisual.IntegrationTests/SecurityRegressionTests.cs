using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Ganss.Xss;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) qua HTTP thật (Testcontainers SQL Server) — các lỗi SECURITY còn lại
/// (docs/work/backend-audit/findings-security.md §4): #2 rate limiting, #6 cookie Secure sau proxy,
/// #7 stored XSS LessonNote, #8 stored XSS Feedback/BugReport, #9 leaderboard class membership,
/// #10 mock-pay gate, #12 validators thiếu, #18 HtmlSanitizer whitelist.
/// Mỗi test assert hành vi ĐÚNG dự kiến sau fix — hiện tại PHẢI FAIL vì bug chưa sửa.
/// KHÔNG sửa production code — test dùng seed qua DbContext như các regression test khác.
/// </summary>
public sealed class SecurityRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public SecurityRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    // ── #7 (TRUNG): stored XSS — LessonNote.ContentHtml lưu RAW, không qua IHtmlSanitizer ─────

    [Fact(DisplayName = "REPRO #7: PUT /me/notes lưu <script> → GET notes phải trả nội dung đã sanitize (hiện lưu raw)")]
    public async Task UpsertNote_ScriptContent_IsSanitized()
    {
        var user = await CreateUserAsync();
        var topic = await CreateTopicAsync(createdBy: user.Id);   // SỬA SETUP: FK_Topics_Users_CreatedBy bắt buộc user tồn tại
        var lesson = await CreateLessonAsync(topic.Id, user.Id);
        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        var put = await client.PutAsJsonAsync($"/api/v1/me/notes/{lesson.Id}",
            new NoteUpsertRequest { ContentHtml = "<p>ghi chú</p><script>alert(1)</script>" });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var get = await client.GetAsync($"/api/v1/me/notes?lessonId={lesson.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var notes = await ReadJsonAsync<List<NoteDto>>(get);

        var note = Assert.Single(notes);
        // Bug: MeController.UpsertNote (MeController.cs:55,60) lưu ContentHtml raw → GET trả về <script> → FAIL
        Assert.DoesNotContain("<script", note.ContentHtml, StringComparison.OrdinalIgnoreCase);
    }

    // ── #8 (TRUNG): stored XSS — Feedback comment / BugReport description lưu RAW ──────────────

    [Fact(DisplayName = "REPRO #8a: feedback comment chứa <script> → phải lưu sanitized (hiện raw)")]
    public async Task SubmitFeedback_ScriptComment_IsStoredSanitized()
    {
        var user = await CreateUserAsync();
        var topic = await CreateTopicAsync(createdBy: user.Id);   // SỬA SETUP: FK_Topics_Users_CreatedBy bắt buộc user tồn tại
        var lesson = await CreateLessonAsync(topic.Id, user.Id);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.UserProgress.Add(new UserProgress { UserId = user.Id, LessonId = lesson.Id, Viewed = true, UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);
        var response = await client.PostAsJsonAsync("/api/v1/feedback",
            new FeedbackRequest { LessonId = lesson.Id, Rating = 5, Comment = "<script>alert(1)</script>" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var comment = await verifyDb.ContentFeedback.AsNoTracking()
            .Where(f => f.UserId == user.Id && f.LessonId == lesson.Id)
            .Select(f => f.Comment)
            .FirstAsync();
        // Bug: FeedbackController.Submit (FeedbackController.cs:77,85) lưu Comment raw → FAIL
        Assert.DoesNotContain("<script", comment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "REPRO #8b: bug-report description chứa <script> → response không chứa script tag (hiện raw)")]
    public async Task CreateBugReport_ScriptDescription_IsSanitized()
    {
        var user = await CreateUserAsync();
        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        var response = await client.PostAsJsonAsync("/api/v1/bug-reports",
            new BugReportRequest
            {
                Description = "<script>alert(1)</script> Mô tả lỗi",
                Context = "<img src=x onerror=alert(1)>"
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var report = await ReadJsonAsync<BugReportDto>(response);
        // Bug: BugReportsController.Create (FeedbackController.cs:113) lưu Description raw và trả về → FAIL
        Assert.DoesNotContain("<script", report.Description, StringComparison.OrdinalIgnoreCase);
    }

    // ── #9 (TRUNG): leaderboard tab class không kiểm tra membership → rò rỉ dữ liệu lớp khác ───

    [Fact(DisplayName = "REPRO #9: user ngoài lớp GET leaderboard?tab=class&classId= → phải 403 (hiện 200)")]
    public async Task GetLeaderboard_ClassTab_NonMember_Returns403()
    {
        var teacher = await CreateUserAsync(role: UserRole.Teacher);
        var member = await CreateUserAsync();
        var outsider = await CreateUserAsync();
        var classId = 0;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cls = new Class
            {
                Name = $"Lớp bảo mật {Guid.NewGuid():N}",
                InviteCode = Guid.NewGuid().ToString("N")[..6],
                Semester = "2026A",
                OwnerId = teacher.Id,
                Status = ClassStatus.Open,
                CreatedAt = DateTime.UtcNow
            };
            db.Classes.Add(cls);
            await db.SaveChangesAsync();
            db.ClassMembers.Add(new ClassMember { ClassId = cls.Id, UserId = member.Id, JoinedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            classId = cls.Id;
        }

        // Đối chứng: member của lớp vẫn xem được 200 (phải giữ nguyên sau fix)
        using (var memberClient = CreateClientWithToken(member.Id, RoleNames.Student))
        {
            var ok = await memberClient.GetAsync($"/api/v1/leaderboard?tab=class&classId={classId}");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        // Bug: GamificationService.GetLeaderboardAsync (GamificationService.cs:564-577) chỉ lọc memberIds,
        // không kiểm tra user gọi có phải member/teacher/admin không → outsider vẫn nhận 200 → FAIL
        using var client = CreateClientWithToken(outsider.Id, RoleNames.Student);
        var response = await client.GetAsync($"/api/v1/leaderboard?tab=class&classId={classId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ── #10 (TRUNG): /premium/mock-pay không gate — premium miễn phí khi deploy thật ───────────

    /// <summary>
    /// Hợp đồng fix dự kiến (findings #10): mock-pay phải bị gate theo config
    /// <c>DSA:Premium:EnableMockPay</c> (chỉ bật Development/Staging). Test override config = "false"
    /// → gọi mock-pay với order hợp lệ → phải 403/404 và KHÔNG kích hoạt premium.
    /// Hiện tại config bị bỏ qua → 200 + active premium → FAIL.
    /// </summary>
    [Fact(DisplayName = "REPRO #10: EnableMockPay=false → mock-pay phải bị chặn (hiện active premium miễn phí)")]
    public async Task MockPay_WhenDisabledByConfig_IsRejected()
    {
        var user = await CreateUserAsync();

        // Tạo order trên factory chính (cùng database với factory gate bên dưới)
        using (var client = CreateClientWithToken(user.Id, RoleNames.Student))
        {
            var upgrade = await client.PostAsJsonAsync("/api/v1/premium/upgrade", new PremiumUpgradeRequest { PlanId = "1m" });
            Assert.Equal(HttpStatusCode.OK, upgrade.StatusCode);
            var order = await ReadJsonAsync<PremiumUpgradeResultDto>(upgrade);

            using var gateFactory = Factory.WithWebHostBuilder(builder =>
                builder.UseSetting("DSA:Premium:EnableMockPay", "false"));
            using var gateClient = gateFactory.CreateClient();
            gateClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CreateToken(user.Id, RoleNames.Student));

            var pay = await gateClient.PostAsJsonAsync("/api/v1/premium/mock-pay",
                new PremiumMockPayRequest { OrderId = order.OrderId });

            Assert.True(pay.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
                $"mock-pay phải bị chặn khi EnableMockPay=false — hiện {pay.StatusCode} (premium miễn phí)");
        }

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userDb = await db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        // Bug: GamificationController.MockPay (GamificationController.cs:131-136) gọi thẳng MockPayAsync
        // → PremiumUntil được set + HeartsMax=30 dù config tắt → FAIL
        Assert.Null(userDb.PremiumUntil);
    }

    // ── #6 (TRUNG): cookie refresh Secure = Request.IsHttps → không tin được sau TLS proxy ─────

    [Fact(DisplayName = "REPRO #6: login sau proxy HTTPS (X-Forwarded-Proto: https) → cookie refresh phải có Secure")]
    public async Task Login_BehindHttpsProxy_SetsSecureCookie()
    {
        var user = await CreateUserAsync();
        using var client = CreateClientWithoutToken();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest { Email = user.Email, Password = "MatKhau@123" })
        };
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookies), "login phải set cookie refresh");
        var setCookie = string.Join("; ", setCookies);
        Assert.Contains("refresh_token=", setCookie, StringComparison.OrdinalIgnoreCase);

        // Bug: Program.cs không có UseForwardedHeaders → Request.IsHttps=false dù X-Forwarded-Proto=https
        // → AuthController.BuildRefreshCookieOptions (AuthController.cs:30) bỏ Secure → FAIL
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    // ── #2 (TRUNG): không có AddRateLimiter → spam/credential-stuffing không bị chặn ────────────

    /// <summary>
    /// Hợp đồng fix dự kiến (findings #2): rate limiter đọc config <c>DSA:RateLimit:General:PermitLimit</c>
    /// (partition user+IP, loại /health), 429 kèm header Retry-After. Test override PermitLimit=5,
    /// bắn 10 request → phải có 429. Hiện tại config không tồn tại → tất cả 200 → FAIL.
    /// </summary>
    [Trait("Category", "RateLimit")]
    [Fact(DisplayName = "REPRO #2: vượt ngưỡng rate limit → 429 + Retry-After (hiện không có limiter — tất cả 200)")]
    public async Task RateLimit_ExceedPermit_Returns429WithRetryAfter()
    {
        var user = await CreateUserAsync();

        using var rateFactory = Factory.WithWebHostBuilder(builder =>
            builder.UseSetting("DSA:RateLimit:General:PermitLimit", "5"));
        using var client = rateFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(user.Id, RoleNames.Student));

        var saw429 = false;
        var sawRetryAfter = false;
        for (var i = 0; i < 10; i++)
        {
            var response = await client.GetAsync("/api/v1/topics");
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                saw429 = true;
                sawRetryAfter = response.Headers.Contains("Retry-After");
                break;
            }
        }

        Assert.True(saw429, "Vượt ngưỡng rate limit phải trả 429 — hiện không có AddRateLimiter nên tất cả 200");
        Assert.True(sawRetryAfter, "Phản hồi 429 phải kèm header Retry-After");
    }

    // ── #12 (TRUNG): validators thiếu — (a) NoteUpsert không giới hạn độ dài (DB DoS) ──────────

    [Fact(DisplayName = "REPRO #12a: note content 100 KB → 400 VALIDATION_FAILED (hiện không giới hạn — 200)")]
    public async Task UpsertNote_OversizedContent_Returns400ValidationFailed()
    {
        var user = await CreateUserAsync();
        var topic = await CreateTopicAsync(createdBy: user.Id);   // SỬA SETUP: FK_Topics_Users_CreatedBy bắt buộc user tồn tại
        var lesson = await CreateLessonAsync(topic.Id, user.Id);
        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        var response = await client.PutAsJsonAsync($"/api/v1/me/notes/{lesson.Id}",
            new NoteUpsertRequest { ContentHtml = new string('a', 100_000) });

        // Bug: NoteUpsertRequest (NoteDtos.cs) không có validator/giới hạn → lưu thẳng 100KB → 200 → FAIL
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_FAILED", await GetErrorCodeAsync(response));
    }

    // ── #12 (TRUNG): validators thiếu — (b) ChangePassword không validator ─────────────────────

    [Fact(DisplayName = "REPRO #12b: change-password thiếu field → 400 VALIDATION_FAILED (hiện OLD_PASSWORD_WRONG)")]
    public async Task ChangePassword_EmptyFields_Returns400ValidationFailed()
    {
        var user = await CreateUserAsync();
        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        var response = await client.PutAsJsonAsync("/api/v1/auth/me/password",
            new ChangePasswordRequest { CurrentPassword = "", NewPassword = "" });

        // Bug: ChangePasswordRequest không có validator → rơi vào check tay (AuthService.cs:326)
        // → trả OLD_PASSWORD_WRONG thay vì VALIDATION_FAILED → FAIL
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_FAILED", await GetErrorCodeAsync(response));
    }

    // ── #18 (THAP): HtmlSanitizer whitelist không thu hẹp — default Ganss cho phép img/a/div ──

    [Fact(DisplayName = "REPRO #18: sanitizer whitelist hẹp → <img>/<a>/<div> phải bị loại (hiện giữ do default Ganss)")]
    public void Sanitizer_Whitelist_RemovesNonWhitelistedTags()
    {
        using var scope = Factory.Services.CreateAsyncScope();
        var sanitizer = scope.ServiceProvider.GetRequiredService<IHtmlSanitizer>();

        var html = "<p>ok</p><img src=\"http://evil.example/x.png\" onerror=\"alert(1)\"><a href=\"javascript:alert(1)\">click</a><div>box</div>";
        var sanitized = sanitizer.Sanitize(html);

        // Bug: Program.cs:144-161 chỉ AllowedTags.Add(13 tag) lên default set Ganss
        // (cho phép a/img/div/table/style...) → img/div/a giữ lại → FAIL.
        Assert.DoesNotContain("<img", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<a", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<div", sanitized, StringComparison.OrdinalIgnoreCase);
    }
}

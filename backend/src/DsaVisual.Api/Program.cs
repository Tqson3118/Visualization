using Asp.Versioning;
using DsaVisual.Api.Dtos;
using DsaVisual.Api.Middlewares;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Seed;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Ganss.Xss;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// JsonOptions dùng chung cho envelope lỗi của JWT challenge/forbidden + fallback 404 (camelCase — §2.1)
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

// ── Serilog bootstrap (SDD §5.8: console dev + rolling file 30 ngày prod) ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// ── Bắt buộc: DSA__Jwt__Secret (SDD §5.3.9 — không hardcode secret; appsettings chỉ chứa placeholder dev) ──
var jwtSecret = builder.Configuration["DSA:Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "DSA__Jwt__Secret chưa được cấu hình. Tạo secret ≥ 32 ký tự và đặt qua biến môi trường (xem backend/.env.example, DEPLOY §2.1).");
}

// Finding security#13: enforce độ dài JWT secret ≥ 32 ký tự — secret ngắn (vd 8 ký tự) có thể brute-force
// HS256; fail-fast khi khởi động thay vì để production chạy với secret yếu.
if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        $"DSA__Jwt__Secret phải dài ít nhất 32 ký tự (hiện tại {jwtSecret.Length} ký tự). " +
        "Tạo secret mạnh ≥ 32 ký tự (VD: openssl rand -base64 48).");
}

// ── Services ──
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // exc#4a: [ApiController] auto-400 khi model binding fail → envelope { error } (API_REFERENCE §2.1)
        // thay vì ProblemDetails RFC 7807 — client chỉ cần 1 định dạng lỗi cho mọi lỗi HTTP.
        options.InvalidModelStateResponseFactory = context =>
        {
            var failed = context.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToList();
            var details = failed
                .SelectMany(kv => kv.Value!.Errors.Select(e => new ErrorDetailDto(kv.Key, e.ErrorMessage)))
                .ToList();
            var response = ErrorResponseDto.Create(
                ErrorCodes.VALIDATION_FAILED,
                "Dữ liệu không hợp lệ",
                failed.Select(kv => kv.Key).FirstOrDefault(),
                details);
            return new BadRequestObjectResult(response);
        };
    });

// API versioning (SDD §5.3.11): Asp.Versioning.Http + Asp.Versioning.Mvc
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new QueryStringApiVersionReader();
})
.AddMvc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// CORS — policy "frontend" (SDD §5.8 bước 3)
var allowedOrigins = builder.Configuration.GetSection("DSA:Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// JWT Bearer (SDD §5.3.5)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // BUG FIX (production): mặc định MapInboundClaims=true map claim "sub" (ngắn) thành
        // ClaimTypes.NameIdentifier (URI dài) → controllers đọc User.FindFirst(JwtRegisteredClaimNames.Sub)
        // trả null → NullReferenceException → 500 trên MỌI endpoint [Authorize].
        // Tắt mapping để giữ claim "sub" đúng chuẩn JWT như TokenService ghi (SDD §5.3.5).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["DSA:Jwt:Issuer"],
            ValidAudience = builder.Configuration["DSA:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1),
            // Tường minh: role claim dùng ClaimTypes.Role (URI dài) như TokenService ghi —
            // đảm bảo [Authorize(Roles = "TEACHER,ADMIN")] hoạt động khi MapInboundClaims=false
            // (giá trị này trùng default, khai báo để tự bảo vệ trước hồi quy claim mapping).
            RoleClaimType = ClaimTypes.Role
        };
        // exc#4b: 401/403 mặc định body rỗng → envelope { error } (API_REFERENCE §2.1)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return WriteErrorEnvelopeAsync(context.Response, StatusCodes.Status401Unauthorized,
                    ErrorCodes.UNAUTHORIZED, "Chưa xác thực hoặc token không hợp lệ");
            },
            OnForbidden = context =>
                WriteErrorEnvelopeAsync(context.Response, StatusCodes.Status403Forbidden,
                    ErrorCodes.FORBIDDEN, "Không có quyền truy cập tài nguyên này")
        };
    });
builder.Services.AddAuthorization();

// EF Core — SQL Server, Service truy vấn DbContext trực tiếp (KHÔNG Repository — SDD §5.1)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.SavepointsDisabledBecauseOfMARS)));

// DI (SDD §5.3.7: Scoped cho DbContext + Service; Singleton cho Settings cache, TokenService (không state), DateTimeProvider)
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ITokenService, TokenService>();              // JWT HS256 — không state
builder.Services.AddSingleton<SettingsCache>();                             // cache Settings (SDD §5.3.7)
builder.Services.AddSingleton<LoginAttemptTracker>();                       // khóa tạm đăng nhập 5/15p
builder.Services.AddSingleton<SubmissionLockRegistry>();                    // chống nộp bài đồng thời
// Finding security#18: whitelist HtmlSanitizer THU HẸP — THAY thế default set của Ganss.Xss
// (mặc định cho phép a/img/div/table/style...) bằng đúng 13 tag mong muốn; hạn chế attributes
// và schemes (http/https/mailto) — chống phishing/tracking qua link/ảnh ngoài.
// Lưu ý API Ganss.Xss: AllowedTags/AllowedAttributes/AllowedSchemes là ISet toàn cục (không theo tag)
// → Clear() rồi Add lại = whitelist thực sự (khác code cũ chỉ Add thêm vào default set).
builder.Services.AddSingleton<IHtmlSanitizer>(_ =>
{
    var sanitizer = new HtmlSanitizer();
    sanitizer.AllowedTags.Clear();
    foreach (var tag in new[] { "h1", "h2", "h3", "p", "strong", "em", "ul", "ol", "li", "pre", "code", "blockquote", "br" })
    {
        sanitizer.AllowedTags.Add(tag);
    }

    sanitizer.AllowedAttributes.Clear();   // không giữ attribute nào (vd class/style/href/src đều bị loại)
    sanitizer.AllowedSchemes.Clear();
    foreach (var scheme in new[] { "http", "https", "mailto" })
    {
        sanitizer.AllowedSchemes.Add(scheme);
    }

    return sanitizer;
});

builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IPathItemService, PathItemService>();
builder.Services.AddScoped<ISimulationCatalogService, SimulationCatalogService>();
builder.Services.AddScoped<ICodeRunnerService, CodeRunnerService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();

// FluentValidation (SDD §5.3.4: gọi ở Service)
builder.Services.AddScoped<IValidator<LessonUpsertRequest>, LessonValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<ExerciseUpsertRequest>, ExerciseUpsertRequestValidator>();
builder.Services.AddScoped<IValidator<ClassUpsertRequest>, ClassUpsertRequestValidator>();
builder.Services.AddScoped<IValidator<SubmitRequest>, SubmitRequestValidator>();
builder.Services.AddScoped<IValidator<FeedbackRequest>, FeedbackRequestValidator>();
builder.Services.AddScoped<IValidator<LessonFeedbackRequest>, LessonFeedbackRequestValidator>();

// Finding security#12: validator cho các DTO body còn thiếu (giới hạn độ dài/format/range).
builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
builder.Services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateProfileRequest>, UpdateProfileRequestValidator>();
builder.Services.AddScoped<IValidator<Verify2FaRequest>, Verify2FaRequestValidator>();
builder.Services.AddScoped<IValidator<NoteUpsertRequest>, NoteUpsertRequestValidator>();
builder.Services.AddScoped<IValidator<TopicUpsertRequest>, TopicUpsertRequestValidator>();
builder.Services.AddScoped<IValidator<TopicReorderRequest>, TopicReorderRequestValidator>();
builder.Services.AddScoped<IValidator<CodeSubmitRequest>, CodeSubmitRequestValidator>();
builder.Services.AddScoped<IValidator<JoinClassRequest>, JoinClassRequestValidator>();
builder.Services.AddScoped<IValidator<AddMemberRequest>, AddMemberRequestValidator>();
builder.Services.AddScoped<IValidator<ShopBuyRequest>, ShopBuyRequestValidator>();
builder.Services.AddScoped<IValidator<PremiumUpgradeRequest>, PremiumUpgradeRequestValidator>();
builder.Services.AddScoped<IValidator<PremiumMockPayRequest>, PremiumMockPayRequestValidator>();
builder.Services.AddScoped<IValidator<BenchmarkRequest>, BenchmarkRequestValidator>();
builder.Services.AddScoped<IValidator<SystemSettingsDto>, SystemSettingsValidator>();
builder.Services.AddScoped<IValidator<BugReportRequest>, BugReportRequestValidator>();

// Finding security#2: rate limiting toàn cục — phân vùng theo (user claim sub + IP), policy
// riêng cho endpoint nhạy cảm (login/register/forgot-password/refresh/2fa/join — chặt hơn),
// /health NGOẠI LỆ. Config: DSA:RateLimit:{General,Sensitive}:PermitLimit + DSA:RateLimit:WindowSeconds.
var rateLimitGeneralPermit = builder.Configuration.GetValue("DSA:RateLimit:General:PermitLimit", 300);
var rateLimitSensitivePermit = builder.Configuration.GetValue("DSA:RateLimit:Sensitive:PermitLimit", 60);
var rateLimitWindowSeconds = builder.Configuration.GetValue("DSA:RateLimit:WindowSeconds", 60);

builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // 429 KHÔNG body rỗng: envelope §2.1 + header Retry-After (API_REFERENCE.md §2.2 — RATE_LIMITED).
    rateLimiter.OnRejected = async (context, ct) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                retryAfter.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }

        await WriteErrorEnvelopeAsync(context.HttpContext.Response, StatusCodes.Status429TooManyRequests,
            ErrorCodes.RATE_LIMITED, "Quá nhiều yêu cầu — vui lòng thử lại sau");
    };

    rateLimiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path;

        // /health NGOẠI LỆ — không rate limit (probe health check không được phép 429).
        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetNoLimiter("health");
        }

        // Sensitive = endpoint có credential/OTP (login/register/2fa/reset/change-password/join) —
        // refresh/logout KHÔNG tính (token nội bộ, không phải mục tiêu brute-force; chống self-DoS
        // khi token cũ → refresh storm đốt hết quota chung với login — fix 14/08).
        var pathString = path.Value ?? string.Empty;
        var isSensitive =
            pathString.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/api/v1/auth/2fa", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/api/v1/auth/forgot-password", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/api/v1/auth/reset-password", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/api/v1/auth/change-password", StringComparison.OrdinalIgnoreCase)
            || pathString.Contains("/join", StringComparison.OrdinalIgnoreCase);

        // Partition = (user claim sub | IP) — login/register là anonymous nên rơi về IP;
        // user đăng nhập dùng sub → không thể làm nghẽn hàng xóm cùng NAT.
        var userKey = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "anon";
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var partitionKey = $"{userKey}|{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = isSensitive ? rateLimitSensitivePermit : rateLimitGeneralPermit,
            Window = TimeSpan.FromSeconds(rateLimitWindowSeconds),
            QueueLimit = 0,          // vượt ngưỡng → 429 ngay, không xếp hàng (chống spam)
            AutoReplenishment = true
        });
    });
});

var app = builder.Build();

// ── Pipeline BẮT BUỘC (SDD §5.8) ──
// 0. UseForwardedHeaders → 1. RequestLogging → 2. ErrorHandling → 3. CORS → 4. Auth → 5. RateLimit → 6. AuthZ → 7. MapControllers
// exc#2 (QUYẾT ĐỊNH — notes.md): KHÔNG migrate sang AddExceptionHandler/IProblemDetailsService trong phiên
// này — middleware hoạt động đúng + test đang xanh, migrate rủi ro cao chạm toàn pipeline; chỉ chuẩn hóa
// (#1 unique→409, #4 envelope 400/401/403/404, #5 claim→401) giữ nguyên kiến trúc middleware hiện tại.

// Finding security#6 + Review E (XFF spoof): sau nginx TLS-terminating (docs/DEPLOY.md — X-Forwarded-Proto),
// Request.IsHttps phải đọc từ header forwarded để cookie refresh nhận Secure — NHƯNG chỉ tin header từ
// proxy ĐÃ KHAI BÁO. Cơ chế ForwardedHeadersMiddleware (xác nhận từ source aspnetcore v10):
// `checkKnownIps = KnownProxies.Count > 0 || KnownIPNetworks.Count > 0` — nếu cả 2 list RỖNG (mặc định
// .NET 8) → TRUST-ALL → client tự đặt X-Forwarded-For giả để qua mặt rate limiter (IP partition).
// .NET 10 đã chủ động hơn (KnownProxies mặc định = {::1}) nhưng ta khai báo TƯỜNG MINH để hành vi
// xác định được: XFF/XFP chỉ được áp dụng khi remote IP ∈ KnownProxies (config) ∪ loopback.
// Loopback luôn tin cậy: TestServer (integration tests) + nginx cùng máy chủ (DEPLOY §4.3 proxy_pass 127.0.0.1).
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// DSA:Proxy:KnownProxies — danh sách IP proxy ngoài tin cậy (nginx máy khác / load balancer / WAF).
// Rỗng → chỉ loopback được tin → XFF từ internet bị BỎ QUA (fail-closed, không spoof được).
var knownProxies = builder.Configuration.GetSection("DSA:Proxy:KnownProxies").Get<string[]>();
if (knownProxies is { Length: > 0 })
{
    foreach (var proxy in knownProxies)
    {
        if (IPAddress.TryParse(proxy, out var address) && !forwardedOptions.KnownProxies.Contains(address))
        {
            forwardedOptions.KnownProxies.Add(address);
        }
    }
}

// Loopback luôn tin cậy: TestServer (integration tests) + nginx cùng máy chủ (DEPLOY §4.3 proxy_pass 127.0.0.1).
// Lưu ý: dùng System.Net.IPNetwork (net9+) — Microsoft.AspNetCore.HttpOverrides.IPNetwork bị obsolete (ASPDEPR005).
forwardedOptions.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Loopback, 8));       // 127.0.0.0/8
forwardedOptions.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.IPv6Loopback, 128)); // ::1/128
app.UseForwardedHeaders(forwardedOptions);

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("frontend");

app.UseAuthentication();

// Finding security#2: sau Auth để partition theo claim sub (user) + IP; /health không bị giới hạn.
app.UseRateLimiter();

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

// exc#4c: route không khớp → 404 envelope { error } (trước đây body rỗng) — mọi lỗi HTTP đều §2.1.
app.MapFallback(context => WriteErrorEnvelopeAsync(context.Response, StatusCodes.Status404NotFound,
    ErrorCodes.NOT_FOUND, "Endpoint không tồn tại"));

// Swagger/OpenAPI: CHỈ Development (finding security#17) — staging có thể truy cập internet,
// lộ toàn bộ API contract + endpoint nhạy cảm (reset-password, admin). Nếu staging cần OpenAPI,
// phải auth-gate (basic auth / mạng nội bộ) — không bật mặc định.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DsaVisual API v1"));
}

// ── Seed thật (SDD §7.5): `dotnet run --project src/DsaVisual.Api -- --seed` ──
// Migrate() + SeedRunner.SeedAsync(db) — idempotent; KHÔNG chạy khi khởi động bình thường.
if (args.Contains("--seed"))
{
    using var seedScope = app.Services.CreateScope();
    var seedDb = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seedClock = seedScope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
    var seedLogger = seedScope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
    await seedDb.Database.MigrateAsync();
    await SeedRunner.SeedAsync(seedDb, seedClock, seedLogger, CancellationToken.None);
    Log.Information("Seed hoàn tất — thoát ứng dụng (đã chạy --seed)");
    return;
}

// Tự động kiểm tra và apply migrations khi khởi động nếu là cơ sở dữ liệu quan hệ (SQL Server)
using (var startupScope = app.Services.CreateScope())
{
    var startupDb = startupScope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (startupDb.Database.IsRelational())
    {
        try
        {
            await startupDb.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_SortOrder' AND object_id = OBJECT_ID('LearningPathNodes'))
                    DROP INDEX IX_LearningPathNodes_PathId_SortOrder ON LearningPathNodes;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_Title' AND object_id = OBJECT_ID('LearningPathNodes'))
                    DROP INDEX IX_LearningPathNodes_PathId_Title ON LearningPathNodes;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Lessons_TopicId_Title' AND is_unique = 1 AND object_id = OBJECT_ID('Lessons'))
                    DROP INDEX IX_Lessons_TopicId_Title ON Lessons;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPaths_Title' AND is_unique = 1 AND object_id = OBJECT_ID('LearningPaths'))
                    DROP INDEX IX_LearningPaths_Title ON LearningPaths;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClassAssignments_ClassId_PathItemId' AND object_id = OBJECT_ID('ClassAssignments'))
                    DROP INDEX IX_ClassAssignments_ClassId_PathItemId ON ClassAssignments;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'DeletedAt' AND object_id = OBJECT_ID('LearningPathNodes'))
                    ALTER TABLE LearningPathNodes ADD DeletedAt datetime2 NULL;

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_ParentId_SortOrder' AND object_id = OBJECT_ID('LearningPathNodes'))
                    CREATE NONCLUSTERED INDEX IX_LearningPathNodes_PathId_ParentId_SortOrder ON LearningPathNodes (PathId, ParentId, SortOrder);
            ");

            await startupDb.Database.MigrateAsync();
            Log.Information("Database migrations verified and applied on startup.");
            await SeedRunner.FixMismatchedQuestionsAsync(startupDb);
            Log.Information("DSA quiz questions verified and reconciled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply database migrations or question reconciliation on startup.");
        }
    }
}

try


{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Ghi envelope lỗi §2.1 cho các lỗi HTTP không qua middleware (JWT challenge/forbidden, fallback 404).</summary>
Task WriteErrorEnvelopeAsync(HttpResponse response, int statusCode, string code, string message)
{
    response.StatusCode = statusCode;
    response.ContentType = "application/json; charset=utf-8";
    return response.WriteAsync(JsonSerializer.Serialize(ErrorResponseDto.Create(code, message), jsonOptions));
}

/// <summary>Entry point công khai — cần cho WebApplicationFactory&lt;Program&gt; trong integration tests (E2).
/// Top-level statements sinh class Program internal; khai báo partial public để test host được.
/// </summary>
public partial class Program { }

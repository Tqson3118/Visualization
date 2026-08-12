using Asp.Versioning;
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
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

// ── Services ──
builder.Services.AddControllers();

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

// CORS — policy "frontend" (SDD §5.8 bước 3)
var allowedOrigins = builder.Configuration.GetSection("DSA:Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
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
    });
builder.Services.AddAuthorization();

// EF Core — SQL Server, Service truy vấn DbContext trực tiếp (KHÔNG Repository — SDD §5.1)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// DI (SDD §5.3.7: Scoped cho DbContext + Service; Singleton cho Settings cache, TokenService (không state), DateTimeProvider)
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ITokenService, TokenService>();              // JWT HS256 — không state
builder.Services.AddSingleton<SettingsCache>();                             // cache Settings (SDD §5.3.7)
builder.Services.AddSingleton<LoginAttemptTracker>();                       // khóa tạm đăng nhập 5/15p
builder.Services.AddSingleton<SubmissionLockRegistry>();                    // chống nộp bài đồng thời
builder.Services.AddSingleton<IHtmlSanitizer>(_ =>
{
    var sanitizer = new HtmlSanitizer();
    sanitizer.AllowedTags.Add("h1");
    sanitizer.AllowedTags.Add("h2");
    sanitizer.AllowedTags.Add("h3");
    sanitizer.AllowedTags.Add("p");
    sanitizer.AllowedTags.Add("strong");
    sanitizer.AllowedTags.Add("em");
    sanitizer.AllowedTags.Add("ul");
    sanitizer.AllowedTags.Add("ol");
    sanitizer.AllowedTags.Add("li");
    sanitizer.AllowedTags.Add("pre");
    sanitizer.AllowedTags.Add("code");
    sanitizer.AllowedTags.Add("blockquote");
    sanitizer.AllowedTags.Add("br");
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

var app = builder.Build();

// ── Pipeline BẮT BUỘC (SDD §5.8) ──
// 1. RequestLogging → 2. ErrorHandling → 3. CORS → 4. Auth → 5. AuthZ → 6. MapControllers
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

// Swagger: Development + Staging; tắt Production (trừ nội bộ) — SDD §5.8
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
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

/// <summary>
/// Entry point công khai — cần cho WebApplicationFactory&lt;Program&gt; trong integration tests (E2).
/// Top-level statements sinh class Program internal; khai báo partial public để test host được.
/// </summary>
public partial class Program { }

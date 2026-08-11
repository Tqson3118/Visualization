using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using VisualizationDSA.Application;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using VisualizationDSA.Application.Services;
using VisualizationDSA.Domain.Interfaces;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.Infrastructure.Extensions;
using VisualizationDSA.Infrastructure.Interceptors;
using VisualizationDSA.Infrastructure.Repositories;
using VisualizationDSA.Infrastructure.Services;
using VisualizationDSA.WebApi.Middlewares;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System",    LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)   
        .ReadFrom.Services(services)                     
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
        .MinimumLevel.Override("System",    LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "VisualizationDSA")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path:              "logs/app-.log",
            rollingInterval:   RollingInterval.Day,
            retainedFileCountLimit: 7,           
            outputTemplate:    "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
});


builder.Services.AddApplicationServices();
builder.Services.AddControllers(options =>
    {
        
        options.Filters.Add<VisualizationDSA.WebApi.Filters.AuditEventActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "VisualizationDSA API",
        Version     = "v1",
        Description = "Backend API cho ứng dụng trực quan hóa DSA & OOP",
    });

    
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Nhập JWT token (không cần prefix 'Bearer ')",
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);

    
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Id   = "Bearer",
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});


var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options
        .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
        
        .AddInterceptors(new ImmutableAuditInterceptor()));

builder.Services.AddScoped<VisualizationDSA.Application.Interfaces.IApplicationDbContext>(provider => provider.GetRequiredService<VisualizationDSA.Infrastructure.Data.ApplicationDbContext>());


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Dùng chung khóa ký JWT từ cấu hình cho hệ stateless (JwtHelper/StatelessAuthStrategy/AdminController).
VisualizationDSA.Domain.JwtSigningConfig.Configure(builder.Configuration["Jwt:Key"]);

// Tài khoản demo/admin mặc định CHỈ ở Development (tránh backdoor credential công khai ở production).
VisualizationDSA.Domain.Strategies.StatelessAuthStrategy.EnableDemoAccounts = builder.Environment.IsDevelopment();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISemanticGraphService, SemanticGraphService>();
builder.Services.AddScoped<IAuditEventService, AuditEventService>();
builder.Services.AddHttpClient<Application.Interfaces.IAIVisualizerService, Infrastructure.Services.GeminiAiService>();
builder.Services.AddScoped<Application.Interfaces.IAIVisualizerService, Infrastructure.Services.GeminiAiService>();


builder.Services.Configure<JudgeOptions>(builder.Configuration.GetSection(JudgeOptions.SectionName));
builder.Services.AddHttpClient<PistonCodeJudgeService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JudgeOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.HttpTimeoutSeconds));
});
builder.Services.AddScoped<ICodeJudgeService>(sp => sp.GetRequiredService<PistonCodeJudgeService>());
builder.Services.AddScoped<VisualizationDSA.Application.Services.IProgressRuleEngine, VisualizationDSA.Infrastructure.Services.ProgressRuleEngine>();


builder.Services.AddScoped<VisualizationDSA.Application.Services.IClassroomProgressService, VisualizationDSA.Infrastructure.Services.ClassroomProgressService>();
builder.Services.AddScoped<VisualizationDSA.Application.Services.IClassroomUnlockRuleEngine, VisualizationDSA.Infrastructure.Services.ClassroomUnlockRuleEngine>();
// ClassroomController phụ thuộc 2 service này — thiếu đăng ký → mọi endpoint classroom lỗi 409.
builder.Services.AddScoped<VisualizationDSA.Application.Services.IClassroomGradingService, VisualizationDSA.Infrastructure.Services.ClassroomGradingService>();
builder.Services.AddScoped<VisualizationDSA.Application.Services.IClassroomExcelExportService, VisualizationDSA.Infrastructure.Services.ClassroomExcelExportService>();


builder.Services.AddAlgorithmStrategies();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),



            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/quiz-room")))
                {
                    
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddHealthChecks()
    .AddCheck<VisualizationDSA.WebApi.Filters.DatabaseHealthCheck>("Database");


builder.Services.AddRateLimiter(options =>
{
    
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 10,
                Window               = TimeSpan.FromMinutes(1),
                QueueLimit           = 0,  
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    
    options.AddPolicy("api", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 60,
                Window               = TimeSpan.FromMinutes(1),
                QueueLimit           = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    
    options.AddPolicy("heavy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 15,
                Window               = TimeSpan.FromMinutes(1),
                QueueLimit           = 0,  
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(new { message = "Quá nhiều yêu cầu. Vui lòng thử lại sau." }),
            token);
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseGlobalErrorHandling();


app.UseSerilogRequestLogging(options =>
{
    
    options.GetLevel = (ctx, elapsed, ex) =>
        ctx.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
});


app.UseSecurityHeaders();

app.UseResponseCompression();
app.UseStaticFiles(); 
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseUserLogging();
app.UseRateLimiter();
app.MapControllers();


app.MapHub<VisualizationDSA.WebApi.Hubs.LeaderboardHub>("/hubs/leaderboard");
app.MapHub<VisualizationDSA.WebApi.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<VisualizationDSA.WebApi.Hubs.QuizRoomHub>("/hubs/quiz-room");


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status      = report.Status.ToString(),
            checks      = report.Entries.Select(e => new
            {
                name    = e.Key,
                status  = e.Value.Status.ToString(),
                latency = e.Value.Duration.TotalMilliseconds,
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp     = DateTime.UtcNow,
        });
        await context.Response.WriteAsync(result);
    }
});


try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        context.Database.Migrate();
        
        var seeder = new DbSeeder(context, includeDemoAdmin: app.Environment.IsDevelopment());
        await seeder.SeedAsync();
        
        var assign1Json = @"[
  {
    ""id"": ""m1a1"",
    ""title"": ""Bài 1: Linked List"",
    ""description"": ""Yêu cầu: Xây dựng hàm `insertAtHead(head, val)` để chèn một Node mới vào đầu danh sách.\n\nMỗi node là một object có cấu trúc: { val: number, next: Node | null }"",
    ""initialCode"": ""function insertAtHead(head, val) {\n  const newNode = { val: val, next: null };\n  // Viết code của bạn ở đây\n  \n  return head;\n}"",
    ""entryFunction"": ""insertAtHead"",
    ""testCases"": [
      { ""input"": ""[null, 5]"", ""expectedOutput"": ""{\""val\"":5,\""next\"":null}"" },
      { ""input"": ""[{\""val\"":10,\""next\"":null}, 5]"", ""expectedOutput"": ""{\""val\"":5,\""next\"":{\""val\"":10,\""next\"":null}}"" }
    ],
    ""hints"": [""newNode.next phải trỏ tới head hiện tại.""]
  },
  {
    ""id"": ""m1a2"",
    ""title"": ""Bài 2: Mảng (Array)"",
    ""description"": ""Yêu cầu: Bạn có một mảng `arr` chứa các số. KHÔNG ĐƯỢC sử dụng các hàm có sẵn như `unshift()` hay `splice()`. Hãy dùng vòng lặp `for` để dời tất cả các phần tử hiện tại sang phải 1 vị trí (phần tử cuối cùng sẽ bị đẩy ra xa hơn, kích thước mảng tăng thêm 1). Sau đó gán `val` vào vị trí đầu tiên `arr[0]`.\n\nTrả về mảng sau khi chèn."",
    ""initialCode"": ""function insertArray(arr, val) {\n  // Bắt đầu từ cuối mảng, dời phần tử sang phải 1 ô\n  for (let i = arr.length; i > 0; i--) {\n      // arr[i] = ...\n  }\n  \n  // Gán giá trị mới vào ô đầu tiên\n  arr[0] = val;\n  return arr;\n}"",
    ""entryFunction"": ""insertArray"",
    ""testCases"": [
      { ""input"": ""[[1, 2, 3], 0]"", ""expectedOutput"": ""[0,1,2,3]"" },
      { ""input"": ""[[], 5]"", ""expectedOutput"": ""[5]"" }
    ],
    ""hints"": [""Dùng vòng lặp for chạy ngược từ i = arr.length về i > 0, gán arr[i] = arr[i - 1]""]
  },
  {
    ""id"": ""m1a3"",
    ""title"": ""Bài 3: Tổng hợp (Array sang Linked List)"",
    ""description"": ""Yêu cầu: Hệ thống đang lưu danh sách sinh viên dưới dạng Mảng (Array) `arr`. Để tối ưu việc chèn/xoá, bạn được giao nhiệm vụ chuyển đổi Mảng này sang cấu trúc **Danh sách liên kết (Linked List)**.\n\nHãy xây dựng hàm `arrayToLinkedList(arr)` nhận vào một mảng và trả về `head` của danh sách liên kết tương ứng.\nNếu mảng rỗng, trả về `null`."",
    ""initialCode"": ""function arrayToLinkedList(arr) {\n  if (arr.length === 0) return null;\n  \n  const head = { val: arr[0], next: null };\n  let current = head;\n  \n  // Dùng vòng lặp duyệt các phần tử còn lại của mảng\n  for (let i = 1; i < arr.length; i++) {\n      // Tạo node mới và móc vào current.next\n      \n  }\n  \n  return head;\n}"",
    ""entryFunction"": ""arrayToLinkedList"",
    ""testCases"": [
      { ""input"": ""[[1, 2, 3]]"", ""expectedOutput"": ""{\""val\"":1,\""next\"":{\""val\"":2,\""next\"":{\""val\"":3,\""next\"":null}}}"" },
      { ""input"": ""[[]]"", ""expectedOutput"": ""null"" }
    ],
    ""hints"": [""current.next = { val: arr[i], next: null }; sau đó dời current = current.next;""]
  }
]";
        context.Database.ExecuteSqlRaw("UPDATE Lessons SET SandboxType = {0}, SandboxConfig = {1} WHERE Title LIKE '%Assignment 1%'", "codelab", assign1Json);

        var assign2Json = @"
{
  ""id"": ""m2a1"",
  ""description"": ""Yêu cầu: Bạn có một danh sách hàng đợi mua vé `queue` (mảng các chuỗi tên người mua). Quầy vé chỉ có thể phục vụ được `k` người. Hãy loại bỏ `k` người đầu tiên trong hàng đợi và trả về mảng danh sách những người còn lại chưa được phục vụ.\n\nLưu ý: Nếu `k` lớn hơn độ dài của hàng đợi, trả về mảng rỗng `[]`."",
  ""initialCode"": ""function processTickets(queue, k) {\n  // Viết code của bạn ở đây\n  \n  return queue;\n}"",
  ""entryFunction"": ""processTickets"",
  ""testCases"": [
    { ""input"": ""[[\""Alice\"", \""Bob\"", \""Charlie\""], 2]"", ""expectedOutput"": ""[\""Charlie\""]"" },
    { ""input"": ""[[\""Alice\""], 5]"", ""expectedOutput"": ""[]"" },
    { ""input"": ""[[\""A\"", \""B\"", \""C\"", \""D\""], 0]"", ""expectedOutput"": ""[\""A\"", \""B\"", \""C\"", \""D\""]"" }
  ],
  ""hints"": [""Dùng phương thức mảng như shift() hoặc slice() để cắt bớt mảng.""]
}";
        context.Database.ExecuteSqlRaw("UPDATE Lessons SET SandboxType = {0}, SandboxConfig = {1} WHERE Title LIKE '%Assignment 2%'", "codelab", assign2Json);

        var assign3Json = @"
{
  ""id"": ""m3a1"",
  ""description"": ""Yêu cầu: Cho một mảng `users` chứa các object có định dạng `{ id: number, name: string }`. Hãy dùng Hash Map (hoặc Object trong JS) để tìm và trả về `name` của người dùng có `id` bằng `targetId`.\n\n- Nếu tìm thấy, trả về chuỗi tên người dùng.\n- Nếu không tìm thấy, trả về chuỗi `'Not Found'`."",
  ""initialCode"": ""function getUserInfo(users, targetId) {\n  const map = {};\n  // Viết code của bạn ở đây\n  \n  return 'Not Found';\n}"",
  ""entryFunction"": ""getUserInfo"",
  ""testCases"": [
    { ""input"": ""[[{\""id\"":1, \""name\"":\""Alice\""}, {\""id\"":2, \""name\"":\""Bob\""}], 2]"", ""expectedOutput"": ""\""Bob\"""" },
    { ""input"": ""[[{\""id\"":10, \""name\"":\""Charlie\""}], 99]"", ""expectedOutput"": ""\""Not Found\"""" },
    { ""input"": ""[[{\""id\"":1, \""name\"":\""A\""}, {\""id\"":2, \""name\"":\""B\""}, {\""id\"":3, \""name\"":\""C\""}], 1]"", ""expectedOutput"": ""\""A\"""" }
  ],
  ""hints"": [""Duyệt qua mảng users, gán map[user.id] = user.name""]
}";
        context.Database.ExecuteSqlRaw("UPDATE Lessons SET SandboxType = {0}, SandboxConfig = {1} WHERE Title LIKE '%Assignment 3%'", "codelab", assign3Json);

        context.Database.ExecuteSqlRaw("UPDATE Users SET IsActive = 1 WHERE Email = 'demo@visualizationdsa.dev'");
        Console.WriteLine("[DEBUG] Reactivated demo@visualizationdsa.dev!");

        Console.WriteLine("[DB SEEDER SUCCESS]: Đã nạp thành công 11 khóa học và 12 bài Quiz!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[DB SEED ERROR]: {ex}");
    Log.Warning(ex, "Không thể kết nối cơ sở dữ liệu local để chạy migrations. Hệ thống vẫn khởi động bình thường.");
}

app.Run();

using Serilog.Context;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DsaVisual.Api.Middlewares;

/// <summary>
/// Ghi log mọi request: id (TraceIdentifier), path, status, duration (SDD §5.8 bước 1).
/// Enrichment Serilog: RequestId, CorrelationId, UserId (SDD §5.8 — lớp nhật ký hệ thống).
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // CorrelationId: ưu tiên header X-Correlation-Id của client, nếu thiếu sinh mới
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                // UserId có sau khi authentication chạy (pipeline phía sau)
                var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
                {
                    logger.LogInformation(
                        "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                        context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}

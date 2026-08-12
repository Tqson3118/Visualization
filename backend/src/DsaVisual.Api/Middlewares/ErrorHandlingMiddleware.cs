using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DsaVisual.Api.Middlewares;

/// <summary>
/// Bắt exception không xử lý → định dạng lỗi chuẩn API_REFERENCE.md §2.1 (SDD §5.8 bước 2).
/// Ý tưởng middleware từ VisualizationDSA V1 — VIẾT LẠI: namespace mới, envelope { error } theo §2.1,
/// chỉ dùng ErrorCode trong catalog (INTERNAL_ERROR / SERVICE_UNAVAILABLE), ẩn chi tiết ngoài Development.
/// </summary>
public sealed class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client hủy request — không ghi lỗi, không trả response
            logger.LogDebug("Request {Path} cancelled by client", context.Request.Path);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (code, message) = exception switch
        {
            DbUpdateException or SqlException => (ErrorCodes.SERVICE_UNAVAILABLE, "Hệ thống dữ liệu đang quá tải, vui lòng thử lại sau"),
            _ => (ErrorCodes.INTERNAL_ERROR, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau")
        };

        var statusCode = ErrorCodes.GetHttpStatus(code);
        var response = ErrorResponseDto.Create(code, message);

        // Chi tiết exception chỉ hiển thị ở Development/Staging (SDD §5.8: ẩn chi tiết production)
        if (env.IsDevelopment())
        {
            response = ErrorResponseDto.Create(code, $"{message} — {exception.Message}", details:
                [new ErrorDetailDto("exception", exception.GetType().Name)]);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlingMiddleware>();
}

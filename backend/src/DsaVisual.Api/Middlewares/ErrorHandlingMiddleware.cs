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
        var (code, message) = MapException(exception);

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

    /// <summary>
    /// exc#1 (CAO): unique violation (SqlException 2601/2627 hoặc message "Cannot insert duplicate key",
    /// ví dụ 2 register cùng email) → 409 CONFLICT / EMAIL_EXISTS — KHÔNG quy về 503 "quá tải" gây hiểu lầm.
    /// exc#5: UnauthorizedAccessException từ ApiControllerBase (claim sub/role thiếu/malformed) → 401.
    /// Lỗi DB khác giữ 503 SERVICE_UNAVAILABLE; không lộ tên bảng/cột ra message client.
    /// exc#2 (QUYẾT ĐỊNH — notes.md): KHÔNG migrate sang AddExceptionHandler/IProblemDetailsService
    /// trong phiên này — middleware hoạt động đúng + envelope §2.1 giữ contract; chỉ chuẩn hóa ở đây.
    /// </summary>
    private static (string Code, string Message) MapException(Exception exception)
    {
        if (IsUniqueViolation(exception))
        {
            // Constraint IX_Users_Email (Users.Email) — register race (biz#2/exc#1); tên index không lộ ra client
            var code = ContainsMessage(exception, "IX_Users_Email")
                ? ErrorCodes.EMAIL_EXISTS
                : ErrorCodes.CONFLICT;
            return (code, "Dữ liệu đã tồn tại, không thể tạo trùng");
        }

        return exception switch
        {
            DbUpdateException or SqlException => (ErrorCodes.SERVICE_UNAVAILABLE,
                "Hệ thống dữ liệu đang quá tải, vui lòng thử lại sau"),
            UnauthorizedAccessException => (ErrorCodes.UNAUTHORIZED,
                "Phiên đăng nhập không hợp lệ, vui lòng đăng nhập lại"),
            _ => (ErrorCodes.INTERNAL_ERROR, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau")
        };
    }

    /// <summary>True khi exception hoặc chuỗi inner là unique constraint violation (SQL Server 2627/2601).</summary>
    private static bool IsUniqueViolation(Exception exception)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex is SqlException { Number: 2601 or 2627 })
            {
                return true;
            }

            if (ex.Message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>True khi message của exception (hoặc chuỗi inner) chứa chuỗi cần tìm.</summary>
    private static bool ContainsMessage(Exception exception, string needle)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex.Message.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlingMiddleware>();
}

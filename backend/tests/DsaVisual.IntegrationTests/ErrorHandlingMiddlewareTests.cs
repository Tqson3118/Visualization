using System.Reflection;
using System.Text.Json;
using DsaVisual.Api.Middlewares;
using DsaVisual.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN cho findings-exception.md #1 (CAO): ErrorHandlingMiddleware.cs:48 map MỌI
/// DbUpdateException/SqlException → SERVICE_UNAVAILABLE (503) — vi phạm unique constraint
/// (SqlException 2627/2601, e.g. 2 register cùng email) phải trả 409 + mã conflict, KHÔNG phải 503.
/// Test trực tiếp middleware (không cần DB/container): dựng SqlException qua reflection
/// (constructor nội bộ — verified Microsoft.Data.SqlClient 6.1.6).
/// KHÔNG sửa code production — chỉ test.
/// </summary>
public sealed class ErrorHandlingMiddlewareTests
{
    private sealed class FakeEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "DsaVisual.IntegrationTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static async Task<(int Status, string Body)> RunAsync(Exception toThrow)
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw toThrow,
            NullLogger<ErrorHandlingMiddleware>.Instance,
            new FakeEnvironment());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, await reader.ReadToEndAsync());
    }

    /// <summary>
    /// Đỏ (finding exc#1): unique violation (SqlException 2627) → phải 409 + mã conflict.
    /// Hiện tại: ErrorHandlingMiddleware.cs:48 map DbUpdateException → 503 SERVICE_UNAVAILABLE.
    /// </summary>
    [Fact(DisplayName = "REPRO exc#1: DbUpdateException unique violation (SqlException 2627) → 409 conflict (hiện 503)")]
    public async Task DbUpdateException_UniqueViolation_Returns409WithConflictCode()
    {
        var (status, body) = await RunAsync(new DbUpdateException(
            "An error occurred while saving the entity changes",
            CreateSqlException(2627, "Violation of UNIQUE KEY constraint 'IX_Users_Email'")));

        Assert.Equal(409, status);   // bug: 503 SERVICE_UNAVAILABLE

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("error", out var error), "Phải trả envelope { error } (API_REFERENCE §2.1)");
        var code = error.GetProperty("code").GetString();
        Assert.True(ErrorCodes.GetHttpStatus(code!) == 409,
            $"Unique violation phải map sang mã conflict 409 — hiện {code} (finding exc#1)");
        Assert.NotEqual(ErrorCodes.SERVICE_UNAVAILABLE, code);
    }

    /// <summary>
    /// Green — non-regression: DbUpdateException KHÔNG phải unique violation → vẫn 503 SERVICE_UNAVAILABLE.
    /// </summary>
    [Fact(DisplayName = "Non-regression exc#1: DbUpdateException KHÔNG unique → vẫn 503 SERVICE_UNAVAILABLE")]
    public async Task DbUpdateException_NonUnique_StillServiceUnavailable()
    {
        var (status, body) = await RunAsync(new DbUpdateException(
            "Connection lost", new InvalidOperationException("timeout")));

        Assert.Equal(503, status);
        using var doc = JsonDocument.Parse(body);
        var code = doc.RootElement.GetProperty("error").GetProperty("code").GetString();
        Assert.Equal(ErrorCodes.SERVICE_UNAVAILABLE, code);
    }

    /// <summary>
    /// Dựng SqlException (constructor nội bộ) với SqlErrorCollection chứa 1 lỗi — signature verified
    /// trên Microsoft.Data.SqlClient 6.1.6: SqlError(Int32,Byte,Byte,String,String,String,Int32,Int32,Exception),
    /// SqlException(String,SqlErrorCollection,Exception,Guid).
    /// </summary>
    private static SqlException CreateSqlException(int number, string message)
    {
        var collectionCtor = typeof(SqlErrorCollection).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
            ?? throw new InvalidOperationException("Không tìm thấy constructor SqlErrorCollection()");
        var collection = (SqlErrorCollection)collectionCtor.Invoke(null);

        var errorCtor = typeof(SqlError).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(Exception)],
            null)
            ?? throw new InvalidOperationException("Không tìm thấy constructor SqlError 9 tham số");
        var error = (SqlError)errorCtor.Invoke(
            [number, (byte)0, (byte)0, "test-server", message, "proc", 0, 0, null]);

        typeof(SqlErrorCollection)
            .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(collection, [error]);

        var exceptionCtor = typeof(SqlException).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid)],
            null)
            ?? throw new InvalidOperationException("Không tìm thấy constructor SqlException 4 tham số");
        return (SqlException)exceptionCtor.Invoke(["test", collection, null, Guid.Empty]);
    }
}

namespace DsaVisual.Application.Common;

/// <summary>
/// Kết quả trả về chuẩn của Service (SDD §5.7.3).
/// Service KHÔNG ném exception cho lỗi nghiệp vụ — trả Result<T> để Controller map qua MapResult.
/// </summary>
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? FieldErrors { get; init; }

    public static Result<T> Ok(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Fail(string code, string message) =>
        new() { ErrorCode = code, ErrorMessage = message };

    public static Result<T> Fail(string code, string message, Dictionary<string, string[]> fieldErrors) =>
        new() { ErrorCode = code, ErrorMessage = message, FieldErrors = fieldErrors };
}

/// <summary>
/// Biến thể không generic cho hành động không trả dữ liệu (DELETE, POST mark-...).
/// </summary>
public record Result
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? FieldErrors { get; init; }

    public static Result Ok() => new() { IsSuccess = true };

    public static Result Fail(string code, string message) =>
        new() { ErrorCode = code, ErrorMessage = message };

    public static Result Fail(string code, string message, Dictionary<string, string[]> fieldErrors) =>
        new() { ErrorCode = code, ErrorMessage = message, FieldErrors = fieldErrors };
}

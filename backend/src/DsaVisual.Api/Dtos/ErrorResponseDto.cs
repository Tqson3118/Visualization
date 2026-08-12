namespace DsaVisual.Api.Dtos;

/// <summary>
/// Envelope lỗi chuẩn — API_REFERENCE.md §2.1 (BẮT BUỘC):
/// { "error": { "code": "EMAIL_EXISTS", "message": "...", "field": "email", "details": [] } }
/// </summary>
public sealed record ErrorResponseDto(ErrorDto Error)
{
    public static ErrorResponseDto Create(string code, string message, string? field = null, IReadOnlyList<ErrorDetailDto>? details = null) =>
        new(new ErrorDto(code, message, field, details ?? []));
}

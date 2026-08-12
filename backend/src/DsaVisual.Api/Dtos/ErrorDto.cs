namespace DsaVisual.Api.Dtos;

/// <summary>Nội dung lỗi chuẩn (API_REFERENCE.md §2.1): { code, message, field, details }.</summary>
public sealed record ErrorDto(
    string Code,
    string Message,
    string? Field,
    IReadOnlyList<ErrorDetailDto> Details);

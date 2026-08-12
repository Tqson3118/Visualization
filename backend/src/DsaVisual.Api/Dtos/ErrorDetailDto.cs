namespace DsaVisual.Api.Dtos;

/// <summary>Phần tử lỗi con trong details[] (API_REFERENCE.md §2.1).</summary>
public sealed record ErrorDetailDto(string Field, string Message);

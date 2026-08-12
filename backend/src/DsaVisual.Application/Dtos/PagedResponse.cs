namespace DsaVisual.Application.Dtos;

/// <summary>Phản hồi phân trang chuẩn — API_REFERENCE.md §3.11: { items, page, pageSize, total, totalPages }.</summary>
public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }

    public static PagedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int total, int totalPages) =>
        new() { Items = items, Page = page, PageSize = pageSize, Total = total, TotalPages = totalPages };
}

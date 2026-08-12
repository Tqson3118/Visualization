namespace DsaVisual.Application.Common;

/// <summary>
/// Tiện ích phân trang (API_REFERENCE.md §1.3): page ≥ 1, pageSize 1..100.
/// </summary>
public static class Pagination
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? DefaultPageSize : pageSize > MaxPageSize ? MaxPageSize : pageSize;
        return (safePage, safeSize);
    }

    public static int TotalPages(int total, int pageSize) =>
        pageSize <= 0 ? 0 : (total + pageSize - 1) / pageSize;
}

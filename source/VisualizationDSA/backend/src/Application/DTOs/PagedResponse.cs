namespace VisualizationDSA.Application.DTOs
{
    /// <summary>Phản hồi phân trang chuẩn: { items, page, pageSize, total, totalPages } — khớp FE types.ts PagedResponse. </summary>
    public class PagedResponse<T>
    {
        public System.Collections.Generic.IReadOnlyList<T> Items { get; set; } = new System.Collections.Generic.List<T>();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; }
        public int TotalPages { get; set; }

        public static PagedResponse<T> Create(System.Collections.Generic.IEnumerable<T> all, int page, int pageSize)
        {
            var list = new System.Collections.Generic.List<T>(all);
            page = System.Math.Max(1, page);
            pageSize = System.Math.Max(1, pageSize);
            var total = list.Count;
            var totalPages = total == 0 ? 0 : (int)System.Math.Ceiling(total / (double)pageSize);
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResponse<T> { Items = items, Page = page, PageSize = pageSize, Total = total, TotalPages = totalPages };
        }
    }
}

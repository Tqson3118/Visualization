namespace DsaVisual.Application.Dtos;

/// <summary>Đổi thứ tự chủ đề — PUT /topics/reorder (API_REFERENCE.md §4.3).
/// Quyết định: dùng <c>{ ids: [5,3,2] }</c> — mảng id theo thứ tự mong muốn, SortOrder = vị trí (index).
/// (API_REFERENCE không đặc tả body; chọn dạng đơn giản nhất.)</summary>
public sealed class TopicReorderRequest
{
    public List<int> Ids { get; set; } = [];
}

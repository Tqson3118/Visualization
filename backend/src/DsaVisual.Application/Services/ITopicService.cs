using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Cây chủ đề (lồng 2 cấp), CRUD, reorder — API_REFERENCE.md §4.3 (SDD §5.4).
/// </summary>
public interface ITopicService
{
    Task<Result<List<TopicDto>>> GetTreeAsync(CancellationToken ct);
    Task<Result<TopicDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<TopicDto>> CreateAsync(int userId, TopicUpsertRequest request, CancellationToken ct);
    Task<Result<TopicDto>> UpdateAsync(int id, TopicUpsertRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
    Task<Result> ReorderAsync(TopicReorderRequest request, CancellationToken ct);
}

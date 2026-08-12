using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Danh mục mô phỏng + schema (đồng bộ khóa frontend catalog — SDD §6.1/§5.4).
/// </summary>
public interface ISimulationCatalogService
{
    Task<Result<List<SimulationMetaDto>>> GetListAsync(CancellationToken ct);
    Task<Result<SimulationMetaDto>> GetByKeyAsync(string key, CancellationToken ct);
    Task<Result<SimulationSchemaDto>> GetSchemaAsync(string key, CancellationToken ct);
    Task<bool> ExistsAsync(string key, CancellationToken ct);
}

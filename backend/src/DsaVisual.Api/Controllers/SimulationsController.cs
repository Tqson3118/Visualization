using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Danh mục mô phỏng + schema — API_REFERENCE.md §4.5.</summary>
[ApiVersion("1.0")]
[Route("api/v1/simulations")]
[Authorize]
public class SimulationsController(ISimulationCatalogService service) : ApiControllerBase
{
    private readonly ISimulationCatalogService _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SimulationMetaDto>>> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100, CancellationToken ct = default)
    {
        var result = await _service.GetListAsync(ct);
        if (!result.IsSuccess)
        {
            return MapResultExtensions.MapResult(this, result);
        }

        // Danh mục nhỏ — trả toàn bộ trong envelope PagedResponse cho nhất quán (API_REFERENCE §1.3)
        var (safePage, safeSize) = Application.Common.Pagination.Normalize(page, pageSize);
        var items = result.Value!.ToList();
        var total = items.Count;
        var paged = PagedResponse<SimulationMetaDto>.Create(
            items.Skip((safePage - 1) * safeSize).Take(safeSize).ToList(),
            safePage, safeSize, total, Application.Common.Pagination.TotalPages(total, safeSize));
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(paged);
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<SimulationMetaDto>> GetByKey([FromRoute] string key, CancellationToken ct)
    {
        var result = await _service.GetByKeyAsync(key, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{key}/schema")]
    public async Task<ActionResult<SimulationSchemaDto>> GetSchema([FromRoute] string key, CancellationToken ct)
    {
        var result = await _service.GetSchemaAsync(key, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

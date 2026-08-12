using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Yêu thích — API_REFERENCE.md §4.9.</summary>
[ApiVersion("1.0")]
[Route("api/v1/favorites")]
[Authorize]
public class FavoritesController(IFavoriteService service) : ApiControllerBase
{
    private readonly IFavoriteService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<FavoriteDto>>> GetList(CancellationToken ct)
    {
        var result = await _service.GetListAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost]
    public async Task<ActionResult<FavoriteDto>> Add([FromBody] FavoriteUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.AddAsync(CurrentUserId(), request, ct);
        return result.IsSuccess
            ? StatusCode(201, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

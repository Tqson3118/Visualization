using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Code Runner (Module I) — API_REFERENCE.md §4.13 (ADR-012: chạy client, server lưu).</summary>
[ApiVersion("1.0")]
[Route("api/v1/code-runs")]
[Authorize]
public class CodeRunsController(ICodeRunnerService service) : ApiControllerBase
{
    private readonly ICodeRunnerService _service = service;

    [HttpPost]
    public async Task<ActionResult<CodeRunDto>> SaveRun([FromBody] CodeRunRequest request, CancellationToken ct)
    {
        var result = await _service.SaveRunAsync(CurrentUserId(), request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetRun), new { id = result.Value!.Id }, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CodeRunDto>> GetRun([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/trace")]
    public async Task<ActionResult<PagedResponse<TraceEventDto>>> GetTrace(
        [FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetTraceAsync(CurrentUserId(), id, page, pageSize, ct);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }
}

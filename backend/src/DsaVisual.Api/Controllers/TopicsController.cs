using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Chủ đề — API_REFERENCE.md §4.3 (cây 2 cấp; Teacher/Admin quản lý).</summary>
[ApiVersion("1.0")]
[Route("api/v1/topics")]
[Authorize]
public class TopicsController(
    ITopicService service,
    IValidator<TopicUpsertRequest> topicValidator,
    IValidator<TopicReorderRequest> reorderValidator) : ApiControllerBase
{
    private readonly ITopicService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<TopicDto>>> GetTree(CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TopicDto>> GetTopic([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<TopicDto>> Create([FromBody] TopicUpsertRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(topicValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.CreateAsync(CurrentUserId(), request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTopic), new { id = result.Value!.Id }, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<TopicDto>> Update([FromRoute] int id, [FromBody] TopicUpsertRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(topicValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.UpdateAsync(id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("reorder")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> Reorder([FromBody] TopicReorderRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(reorderValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.ReorderAsync(request, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

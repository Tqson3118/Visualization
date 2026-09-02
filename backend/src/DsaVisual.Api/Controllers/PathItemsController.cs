using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1")]
[Authorize]
public class PathItemsController(IPathItemService service) : ApiControllerBase
{
    private readonly IPathItemService _service = service;

    /// <summary>Lấy cây nội dung outline của lộ trình (nested JSON). Chỉ Teacher sở hữu lộ trình / Admin.</summary>
    [HttpGet("paths/{pathId:int}/items")]
    public async Task<ActionResult<List<PathItemDto>>> GetTree([FromRoute] int pathId, CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(CurrentUserId(), CurrentRole(), pathId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Tìm mục lộ trình và ID lộ trình theo lessonId (hỗ trợ điều hướng O(1) từ Studio Overview).</summary>
    [HttpGet("paths/find-by-lesson/{lessonId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PathItemDto>> FindByLesson([FromRoute] int lessonId, CancellationToken ct)
    {
        var result = await _service.FindByLessonIdAsync(CurrentUserId(), CurrentRole(), lessonId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Tạo mới 1 mục (Folder / Theory / Quiz / Lab) trong lộ trình.</summary>
    [HttpPost("paths/{pathId:int}/items")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PathItemDto>> CreateItem(
        [FromRoute] int pathId,
        [FromBody] PathItemCreateRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateItemAsync(CurrentUserId(), CurrentRole(), pathId, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Lấy chi tiết 1 mục (kèm exercise nếu là Quiz/Lab, lesson nếu là Theory). Chỉ Teacher sở hữu lộ trình / Admin.</summary>
    [HttpGet("items/{itemId:int}")]
    public async Task<ActionResult<PathItemDto>> GetItemDetail([FromRoute] int itemId, CancellationToken ct)
    {
        var result = await _service.GetItemDetailAsync(CurrentUserId(), CurrentRole(), itemId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Sửa tiêu đề / mô tả mục lộ trình.</summary>
    [HttpPut("items/{itemId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PathItemDto>> UpdateItem(
        [FromRoute] int itemId,
        [FromBody] PathItemUpdateRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateItemAsync(CurrentUserId(), CurrentRole(), itemId, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Kéo thả di chuyển mục (thay đổi parentId và sortOrder).</summary>
    [HttpPost("items/{itemId:int}/move")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PathItemDto>> MoveItem(
        [FromRoute] int itemId,
        [FromBody] PathItemMoveRequest request,
        CancellationToken ct)
    {
        var result = await _service.MoveItemAsync(CurrentUserId(), CurrentRole(), itemId, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Xóa mục lộ trình (cascade toàn bộ cây con).</summary>
    [HttpDelete("items/{itemId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> DeleteItem([FromRoute] int itemId, CancellationToken ct)
    {
        var result = await _service.DeleteItemAsync(CurrentUserId(), CurrentRole(), itemId, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Gamification, Premium, Learning Path & Benchmark — API_REFERENCE.md §4.14 (Module J, FR-10.x).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1")]
[Authorize]
public class GamificationController(IGamificationService service) : ApiControllerBase
{
    private readonly IGamificationService _service = service;

    // ── Hearts ──
    [HttpGet("me/hearts")]
    public async Task<ActionResult<HeartsStatusDto>> GetHearts(CancellationToken ct)
    {
        var result = await _service.GetHeartsAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Learning path ──
    [HttpGet("learning-path/{id:int}")]
    public async Task<ActionResult<LearningPathMapDto>> GetLearningPath([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetLearningPathAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("learning-path/{id:int}/nodes/{nodeId:int}/enter")]
    public async Task<ActionResult<NodeEnterResultDto>> EnterNode(
        [FromRoute] int id, [FromRoute] int nodeId, [FromBody] NodeEnterRequest? request, CancellationToken ct)
    {
        var result = await _service.EnterNodeAsync(CurrentUserId(), id, nodeId, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("learning-path/{id:int}/final-test")]
    public async Task<ActionResult<List<QuestionDto>>> GetFinalTest([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetFinalTestAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Quests / streak ──
    [HttpGet("me/quests")]
    public async Task<ActionResult<List<QuestDto>>> GetQuests(CancellationToken ct)
    {
        var result = await _service.GetQuestsAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("me/quests/{id:int}/claim")]
    public async Task<ActionResult<QuestClaimResultDto>> ClaimQuest([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.ClaimQuestAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("me/streak")]
    public async Task<ActionResult<StreakDto>> GetStreak(CancellationToken ct)
    {
        var result = await _service.GetStreakAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Leaderboard ──
    [HttpGet("leaderboard")]
    public async Task<ActionResult<PagedResponse<LeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] string tab = "week", [FromQuery] int? classId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetLeaderboardAsync(tab, classId, page, pageSize, ct);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    // ── Shop / inventory ──
    [HttpGet("shop/items")]
    public async Task<ActionResult<List<ShopItemDto>>> GetShopItems(CancellationToken ct)
    {
        var result = await _service.GetShopItemsAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("shop/buy")]
    public async Task<ActionResult<ShopBuyResultDto>> Buy([FromBody] ShopBuyRequest request, CancellationToken ct)
    {
        var result = await _service.BuyItemAsync(CurrentUserId(), request.ItemId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("me/inventory")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetInventory(CancellationToken ct)
    {
        var result = await _service.GetInventoryAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("me/inventory/equip")]
    public async Task<ActionResult> Equip([FromBody] EquipRequest request, CancellationToken ct)
    {
        var result = await _service.EquipItemAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Premium ──
    [HttpGet("premium/status")]
    public async Task<ActionResult<PremiumStatusDto>> GetPremiumStatus(CancellationToken ct)
    {
        var result = await _service.GetPremiumStatusAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("premium/upgrade")]
    public async Task<ActionResult<PremiumUpgradeResultDto>> UpgradePremium([FromBody] PremiumUpgradeRequest request, CancellationToken ct)
    {
        var result = await _service.UpgradePremiumAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("premium/mock-pay")]
    public async Task<ActionResult<PremiumStatusDto>> MockPay([FromBody] PremiumMockPayRequest request, CancellationToken ct)
    {
        var result = await _service.MockPayAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Cheatsheet / benchmark ──
    [HttpGet("cheatsheet")]
    public async Task<ActionResult<List<CheatsheetItemDto>>> GetCheatsheet([FromQuery] string? structure, CancellationToken ct)
    {
        var result = await _service.GetCheatsheetAsync(structure, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("benchmarks/run")]
    public async Task<ActionResult<BenchmarkRunResponse>> RunBenchmark([FromBody] BenchmarkRequest request, CancellationToken ct)
    {
        var result = await _service.RunBenchmarkAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

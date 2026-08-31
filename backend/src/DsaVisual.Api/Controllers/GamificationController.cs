using Asp.Versioning;
using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Gamification, Premium, Learning Path & Benchmark — API_REFERENCE.md §4.14 (Module J, FR-10.x).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1")]
[Authorize]
public class GamificationController(
    IGamificationService service,
    AppDbContext db,
    IConfiguration config,
    IValidator<ShopBuyRequest> shopBuyValidator,
    IValidator<PremiumUpgradeRequest> premiumUpgradeValidator,
    IValidator<PremiumMockPayRequest> premiumMockPayValidator,
    IValidator<BenchmarkRequest> benchmarkValidator) : ApiControllerBase
{
    private readonly IGamificationService _service = service;
    private readonly AppDbContext _db = db;

    // ── Hearts ──
    [HttpGet("me/hearts")]
    public async Task<ActionResult<HeartsStatusDto>> GetHearts(CancellationToken ct)
    {
        var result = await _service.GetHeartsAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("me/spend-heart")]
    public async Task<ActionResult<HeartsStatusDto>> SpendHeart(CancellationToken ct)
    {
        var result = await _service.SpendHeartAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Gamification summary (level + XP — feature port) ──
    [HttpGet("me/gamification")]
    public async Task<ActionResult<GamificationSummaryDto>> GetGamificationSummary(CancellationToken ct)
    {
        var result = await _service.GetGamificationSummaryAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Learning path ──
    [HttpGet("learning-paths")]
    public async Task<ActionResult<List<LearningPathSummaryDto>>> GetLearningPaths(CancellationToken ct)
    {
        var result = await _service.GetLearningPathsAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

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
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default,
        // perf#4 (keyset): cursor phân trang — lastXp + lastId của phần tử CUỐI trang trước.
        // Không truyền → fallback offset (contract page/size cũ giữ nguyên, test bảo vệ không đổi).
        [FromQuery] int? lastXp = null, [FromQuery] int? lastId = null)
    {
        // Finding security#9: tab class chỉ cho phép member của lớp HOẶC TEACHER/ADMIN (owner) —
        // user ngoài lớp không được xem DisplayName + XP của học viên lớp khác (enumerate classId).
        if (tab.Equals("class", StringComparison.OrdinalIgnoreCase) && classId is > 0)
        {
            var isTeacherOrAdmin = CurrentRole() is "TEACHER" or "ADMIN";
            if (!isTeacherOrAdmin)
            {
                var isMember = await _db.ClassMembers.AsNoTracking()
                    .AnyAsync(m => m.ClassId == classId.Value && m.UserId == CurrentUserId(), ct);
                if (!isMember)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ErrorResponseDto.Create(
                        ErrorCodes.FORBIDDEN, "Bạn không phải thành viên lớp này — không xem được bảng xếp hạng của lớp"));
                }
            }
        }

        var result = await _service.GetLeaderboardAsync(tab, classId, page, pageSize, ct, lastXp, lastId);
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
        var invalid = await ValidateRequestAsync(shopBuyValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

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
        var invalid = await ValidateRequestAsync(premiumUpgradeValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.UpgradePremiumAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("premium/mock-pay")]
    public async Task<ActionResult<PremiumStatusDto>> MockPay([FromBody] PremiumMockPayRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(premiumMockPayValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        // Finding security#10 + Review E: mock-pay là "thanh toán mô phỏng" — nếu deploy thật mà quên tắt,
        // bất kỳ user nào cũng tự kích hoạt Premium miễn phí. Gate fail-closed theo config
        // DSA:Premium:EnableMockPay — DEFAULT FALSE (trước đây default true → thiếu config = lỗ hổng).
        // Dev/Staging bật tường minh qua appsettings.Development.json; Production appsettings.Production.json
        // = false → chỉ ops chủ động set true (biến env DSA__Premium__EnableMockPay) mới mở.
        if (!config.GetValue("DSA:Premium:EnableMockPay", false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponseDto.Create(
                ErrorCodes.FORBIDDEN, "Thanh toán mô phỏng đã bị tắt — liên hệ quản trị viên"));
        }

        var result = await _service.MockPayAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [AllowAnonymous]
    [HttpPost("premium/webhook")]
    public async Task<ActionResult<PaymentWebhookResultDto>> ProcessPaymentWebhook([FromBody] PaymentWebhookRequest request, CancellationToken ct)
    {
        var configuredSecret = config["DSA:Premium:WebhookSecret"];
        if (!string.IsNullOrWhiteSpace(configuredSecret))
        {
            var headerSecret = Request.Headers["X-Webhook-Secret"].FirstOrDefault()
                ?? Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(headerSecret))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ErrorResponseDto.Create(
                    ErrorCodes.UNAUTHORIZED, "Thiếu khóa bí mật xác thực Webhook thanh toán"));
            }

            if (headerSecret.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                headerSecret = headerSecret[7..].Trim();
            }

            if (!string.Equals(headerSecret, configuredSecret, StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ErrorResponseDto.Create(
                    ErrorCodes.UNAUTHORIZED, "Khóa bí mật Webhook không hợp lệ"));
            }
        }

        var result = await _service.ProcessPaymentWebhookAsync(request, ct);
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
        var invalid = await ValidateRequestAsync(benchmarkValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.RunBenchmarkAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}

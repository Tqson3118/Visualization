using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// 1 public seam duy nhất Module J (ADR-011): hearts/session, quest/streak, shop/gems, premium,
/// leaderboard, cheatsheet, benchmark (SDD §5.4, FR-10.1..10.7).
/// </summary>
public interface IGamificationService
{
    Task<Result<HeartsStatusDto>> GetHeartsAsync(int userId, CancellationToken ct);
    Task<Result<List<LearningPathSummaryDto>>> GetLearningPathsAsync(int userId, CancellationToken ct);
    Task<Result<LearningPathMapDto>> GetLearningPathAsync(int userId, int pathId, CancellationToken ct);
    Task<Result<NodeEnterResultDto>> EnterNodeAsync(int userId, int pathId, int nodeId, NodeEnterRequest? request, CancellationToken ct);
    Task<Result<List<QuestionDto>>> GetFinalTestAsync(int userId, int pathId, CancellationToken ct);
    Task<Result<List<QuestDto>>> GetQuestsAsync(int userId, CancellationToken ct);
    Task<Result<QuestClaimResultDto>> ClaimQuestAsync(int userId, int questId, CancellationToken ct);
    Task<Result<StreakDto>> GetStreakAsync(int userId, CancellationToken ct);
    Task<Result<PagedResponse<LeaderboardEntryDto>>> GetLeaderboardAsync(string tab, int? classId, int page, int pageSize, CancellationToken ct = default, int? lastXp = null, int? lastId = null);
    Task<Result<List<ShopItemDto>>> GetShopItemsAsync(int userId, CancellationToken ct);
    Task<Result<ShopBuyResultDto>> BuyItemAsync(int userId, int itemId, CancellationToken ct);
    Task<Result<List<InventoryItemDto>>> GetInventoryAsync(int userId, CancellationToken ct);
    Task<Result> EquipItemAsync(int userId, EquipRequest request, CancellationToken ct);
    Task<Result<PremiumStatusDto>> GetPremiumStatusAsync(int userId, CancellationToken ct);
    Task<Result<PremiumUpgradeResultDto>> UpgradePremiumAsync(int userId, PremiumUpgradeRequest request, CancellationToken ct);
    Task<Result<PremiumStatusDto>> MockPayAsync(int userId, PremiumMockPayRequest request, CancellationToken ct);
    Task<Result<List<CheatsheetItemDto>>> GetCheatsheetAsync(string? structure, CancellationToken ct);
    Task<Result<BenchmarkRunResponse>> RunBenchmarkAsync(int userId, BenchmarkRequest request, CancellationToken ct);
}

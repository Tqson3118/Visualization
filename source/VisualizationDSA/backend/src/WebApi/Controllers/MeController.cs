using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Nhóm gamification profile — GET/POST /api/v1/me/* (ProfileView + AppHeader + QuestsView).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/me")]
    [RequireJwtRole]
    public class MeController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;

        public MeController(IApplicationDbContext ctx) { _ctx = ctx; }

        // Bậc level theo XP (mở rộng so với User._checkUpLevel — khớp leaderboard + roster 120 SV).
        private static readonly int[] LevelThresholds = { 0, 100, 300, 600, 1000, 1500, 2200, 3000, 3800, 4700, 5800, 7000, 8400, 10000, 12000, 14500 };

        private Guid Uid()
            => Guid.Parse(JwtHelper.ExtractSubFromToken(Request) ?? Guid.Empty.ToString());

        private static int LevelFromXp(int xp)
        {
            var lvl = 1;
            for (var i = 0; i < LevelThresholds.Length; i++) if (xp >= LevelThresholds[i]) lvl = i + 1; else break;
            return lvl;
        }

        private static (int into, int next) LevelProgress(int xp)
        {
            var lvl = LevelFromXp(xp);
            var floorL = lvl - 1 >= LevelThresholds.Length ? LevelThresholds[^1] : (lvl - 1 <= 0 ? 0 : LevelThresholds[lvl - 1]);
            floorL = lvl == 1 ? 0 : (lvl - 1 < LevelThresholds.Length ? LevelThresholds[lvl - 1] : LevelThresholds[^1] + (lvl - LevelThresholds.Length) * 1200);
            var ceilL = lvl < LevelThresholds.Length ? LevelThresholds[lvl] : LevelThresholds[^1] + (lvl + 1 - LevelThresholds.Length) * 1200;
            var into = xp - floorL;
            var next = Math.Max(1, ceilL - floorL);
            return (into, next);
        }

        private async Task<int> GemsBalanceAsync(Guid userId)
        {
            var txs = await _ctx.Set<GemTransaction>().Where(t => t.UserId == userId).ToListAsync();
            return txs.Sum(t => t.Type == "Spend" ? -t.Amount : t.Amount);
        }

        /// <summary>GET /me/gamification — { xp, level, xpIntoLevel, xpForNextLevel, levelProgressPct }</summary>
        [HttpGet("gamification")]
        public async Task<IActionResult> Gamification()
        {
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Uid());
            if (user == null) return NotFound();
            var (into, next) = LevelProgress(user.TotalXP);
            return Ok(new
            {
                xp = user.TotalXP,
                level = LevelFromXp(user.TotalXP),
                xpIntoLevel = into,
                xpForNextLevel = next,
                levelProgressPct = (int)(into * 100.0 / next),
            });
        }

        /// <summary>GET /me/hearts — { hearts, heartsMax, lastHeartAt, nextHeartAt }</summary>
        [HttpGet("hearts")]
        public async Task<IActionResult> Hearts()
        {
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Uid());
            if (user == null) return NotFound();
            user.RegenHearts(DateTime.UtcNow);
            string? nextHeartAt = null;
            if (user.Hearts < user.HeartsMax && user.LastHeartAt.HasValue)
                nextHeartAt = user.LastHeartAt.Value.AddMinutes(5).ToString("o");
            return Ok(new
            {
                hearts = user.Hearts,
                heartsMax = user.HeartsMax,
                lastHeartAt = user.LastHeartAt?.ToString("o"),
                nextHeartAt,
            });
        }

        /// <summary>GET /me/streak — { streakDays, streakFreeze }</summary>
        [HttpGet("streak")]
        public async Task<IActionResult> Streak()
        {
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Uid());
            return Ok(new { streakDays = user?.StreakDays ?? 0, streakFreeze = 0 });
        }

        /// <summary>GET /me/quests — RawQuestDto[]: { id, questId, title, type, progress, target, claimed, reward:{gems,xp} }</summary>
        [HttpGet("quests")]
        public async Task<IActionResult> Quests()
        {
            var uid = Uid();
            var userQuests = await _ctx.Set<UserQuest>()
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == uid && uq.Quest.IsActive)
                .OrderBy(uq => uq.Quest.SortOrder)
                .ToListAsync();
            var result = userQuests.Select(uq =>
            {
                var reward = ParseReward(uq.Quest.RewardJson);
                return new
                {
                    id = uq.Quest.Id.ToString(),
                    questId = uq.Quest.Id.ToString(),
                    title = uq.Quest.Title,
                    type = PeriodToNumber(uq.Quest.Period),
                    progress = uq.Progress,
                    target = Target(uq.Quest.ConditionJson),
                    claimed = uq.Status == "Claimed",
                    reward,
                };
            }).ToList();
            return Ok(result);
        }

        /// <summary>POST /me/quests/{id}/claim — { claimed, reward:{gems,xp}, gemsTotal }</summary>
        [HttpPost("quests/{id}/claim")]
        public async Task<IActionResult> ClaimQuest(string id)
        {
            var uid = Uid();
            var parsed = TryGuid(id);
            var userQuest = parsed.HasValue
                ? await _ctx.Set<UserQuest>().Include(uq => uq.Quest).FirstOrDefaultAsync(uq => uq.QuestId == parsed.Value && uq.UserId == uid)
                : null;
            if (userQuest == null || userQuest.Status == "Claimed") return NotFound(new { message = "Nhiệm vụ không tồn tại hoặc đã nhận thưởng." });

            var reward = ParseReward(userQuest.Quest.RewardJson);
            userQuest.MarkClaimed();
            _ctx.Set<GemTransaction>().Add(new GemTransaction(uid, reward.gems > 0 ? reward.gems : 0, "Earn", userQuest.Quest.QuestKey));
            await _ctx.SaveChangesAsync(CancellationToken.None);
            var gems = await GemsBalanceAsync(uid);
            return Ok(new { claimed = true, reward, gemsTotal = gems });
        }

        /// <summary>GET /me/inventory — InventoryItemDto[]</summary>
        [HttpGet("inventory")]
        public async Task<IActionResult> Inventory()
        {
            var uid = Uid();
            var rows = await _ctx.Set<UserInventory>()
                .Include(i => i.Item)
                .Where(i => i.UserId == uid)
                .OrderByDescending(i => i.AcquiredAt)
                .ToListAsync();
            // equip: mỗi slot chỉ 1 món — nếu nhiều món cùng slot equip TẤT CẢ thì chỉ giữ món mới nhất.
            var result = rows.Select(i => new
            {
                id = i.Id.ToString(),
                itemId = i.ItemId.ToString(),
                itemKey = i.Item.QuestKeyOrName(),
                name = i.Item.Name,
                quantity = 1,
                type = SlotToType(i.Item.Slot),
                isEquipped = i.Equipped,
                expiresAt = (string?)null,
            }).ToList();
            return Ok(result);
        }

        /// <summary>PUT /me/inventory/equip — body { itemId, isEquipped } → 200 rỗng</summary>
        [HttpPut("inventory/equip")]
        public async Task<IActionResult> Equip([FromBody] EquipRequest req)
        {
            var uid = Uid();
            var itemId = TryGuid(req.ItemId) ?? Guid.Empty;
            var row = await _ctx.Set<UserInventory>()
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.UserId == uid && i.ItemId == itemId && i.Item.Slot != "consumable");
            if (row == null) return NotFound();
            if (row.Equipped == true && req.IsEquipped == false) { row.SetEquipped(false); }
            else if (req.IsEquipped == true)
            {
                var slot = row.Item.Slot;
                var sameSlot = await _ctx.Set<UserInventory>().Include(i => i.Item)
                    .Where(i => i.UserId == uid && i.Equipped && i.Item.Slot == slot).ToListAsync();
                foreach (var s in sameSlot) s.SetEquipped(false);
                row.SetEquipped(true);
            }
            await _ctx.SaveChangesAsync(CancellationToken.None);
            return Ok(new { });
        }

        private static int PeriodToNumber(string period) => period switch { "Weekly" => 1, "Monthly" => 2, _ => 0 };
        private static int SlotToType(string slot) => slot switch { "avatar" => 1, "frame" => 2, _ => 0 };
        private static (int gems, int xp) ParseReward(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                int g = root.TryGetProperty("gems", out var gv) && gv.ValueKind == JsonValueKind.Number ? gv.GetInt32() : 0;
                int x = root.TryGetProperty("xp", out var xv) && xv.ValueKind == JsonValueKind.Number ? xv.GetInt32() : 0;
                return (g, x);
            }
            catch { return (0, 0); }
        }
        private static int Target(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                return root.TryGetProperty("target", out var t) && t.ValueKind == JsonValueKind.Number ? t.GetInt32() : 1;
            }
            catch { return 1; }
        }
        private static Guid? TryGuid(string s)
            => Guid.TryParse(s, out var g) ? g : (Guid?)null;
    }

    public class EquipRequest
    {
        public string? ItemId { get; set; }
        public bool IsEquipped { get; set; }
    }

    internal static class ShopItemExtensions
    {
        public static string QuestKeyOrName(this ShopItem item)
            => string.IsNullOrWhiteSpace(item.IconUrl) ? item.Name.ToLowerInvariant().Replace(" ", ".") : item.IconUrl;
    }
}

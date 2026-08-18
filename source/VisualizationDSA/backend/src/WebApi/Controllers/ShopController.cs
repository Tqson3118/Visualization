using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/shop")]
    [RequireJwtRole]
    public class ShopController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public ShopController(IApplicationDbContext ctx) { _ctx = ctx; }

        private async Task<int> GemsAsync(Guid userId)
        {
            var txs = await _ctx.Set<GemTransaction>().Where(t => t.UserId == userId).ToListAsync();
            return txs.Sum(t => t.Type == "Spend" ? -t.Amount : t.Amount);
        }

        /// <summary>GET /shop/items — ShopItemDto[]</summary>
        [HttpGet("items")]
        public async Task<IActionResult> Items()
        {
            var items = await _ctx.Set<ShopItem>().Where(i => i.IsActive).OrderBy(i => i.SortOrder).ToListAsync();
            return Ok(items.Select(i => new
            {
                id = i.Id.ToString(),
                name = i.Name,
                description = i.Description,
                priceGems = i.PriceGems,
                slot = (string?)i.Slot,
            }));
        }

        /// <summary>POST /shop/buy — body { itemId } → { gemsLeft }</summary>
        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BuyRequest req)
        {
            var uidStr = JwtHelper.ExtractSubFromToken(Request);
            var uid = Guid.TryParse(uidStr, out var g) ? g : Guid.Empty;
            var itemId = Guid.TryParse(req.ItemId, out var ig) ? ig : Guid.Empty;
            var item = await _ctx.Set<ShopItem>().FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive);
            if (item == null) return NotFound(new { message = "Vật phẩm không tồn tại." });

            var gems = await GemsAsync(uid);
            if (gems < item.PriceGems) return BadRequest(new { message = "Không đủ gems." });

            _ctx.Set<GemTransaction>().Add(new GemTransaction(uid, item.PriceGems, "Spend", item.Id.ToString()));
            _ctx.Set<UserInventory>().Add(new UserInventory(uid, item.Id, equipped: false));
            await _ctx.SaveChangesAsync(CancellationToken.None);

            var left = await GemsAsync(uid);
            return Ok(new { gemsLeft = left });
        }
    }

    public class BuyRequest
    {
        public string? ItemId { get; set; }
    }
}

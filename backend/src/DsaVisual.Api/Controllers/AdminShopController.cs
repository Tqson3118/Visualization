using Asp.Versioning;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/admin/shop")]
[Authorize(Roles = "ADMIN")]
public sealed class AdminShopController(AppDbContext db) : ApiControllerBase
{
    public sealed record AdminShopItemDto(
        int Id,
        string ItemKey,
        string Name,
        int PriceGems,
        int Type,
        int MaxStack,
        int? DurationHours,
        int OwnersCount);

    public sealed record CreateShopItemRequest(
        string ItemKey,
        string Name,
        int PriceGems,
        int Type,
        int MaxStack,
        int? DurationHours);

    public sealed record UpdateShopItemRequest(
        string Name,
        int PriceGems,
        int Type,
        int MaxStack,
        int? DurationHours);

    public sealed record GemTransactionDto(
        int Id,
        int UserId,
        string UserEmail,
        string UserDisplayName,
        int Type,
        int Amount,
        string? RefType,
        string? RefId,
        DateTime CreatedAt);

    [HttpGet("items")]
    public async Task<ActionResult<List<AdminShopItemDto>>> GetItems(CancellationToken ct)
    {
        var items = await db.ShopItems.AsNoTracking().ToListAsync(ct);
        var ownershipCounts = await db.UserInventory.AsNoTracking()
            .GroupBy(i => i.ItemId)
            .Select(g => new { ItemId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ItemId, g => g.Count, ct);

        var result = items.Select(i => new AdminShopItemDto(
            i.Id,
            i.ItemKey,
            i.Name,
            i.PriceGems,
            i.Type,
            i.MaxStack,
            i.DurationHours,
            ownershipCounts.GetValueOrDefault(i.Id, 0)
        )).ToList();

        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<ActionResult<AdminShopItemDto>> CreateItem([FromBody] CreateShopItemRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ItemKey) || string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "ItemKey và Tên vật phẩm không được để trống" });
        }

        var exists = await db.ShopItems.AnyAsync(i => i.ItemKey == req.ItemKey, ct);
        if (exists)
        {
            return Conflict(new { message = $"ItemKey '{req.ItemKey}' đã tồn tại trong hệ thống" });
        }

        var item = new ShopItem
        {
            ItemKey = req.ItemKey.Trim().ToLowerInvariant(),
            Name = req.Name.Trim(),
            PriceGems = Math.Max(0, req.PriceGems),
            Type = req.Type,
            MaxStack = Math.Max(1, req.MaxStack),
            DurationHours = req.DurationHours
        };

        db.ShopItems.Add(item);
        await db.SaveChangesAsync(ct);

        return Ok(new AdminShopItemDto(item.Id, item.ItemKey, item.Name, item.PriceGems, item.Type, item.MaxStack, item.DurationHours, 0));
    }

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<AdminShopItemDto>> UpdateItem(int id, [FromBody] UpdateShopItemRequest req, CancellationToken ct)
    {
        var item = await db.ShopItems.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item is null)
        {
            return NotFound(new { message = "Vật phẩm không tồn tại" });
        }

        if (string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "Tên vật phẩm không được để trống" });
        }

        item.Name = req.Name.Trim();
        item.PriceGems = Math.Max(0, req.PriceGems);
        item.Type = req.Type;
        item.MaxStack = Math.Max(1, req.MaxStack);
        item.DurationHours = req.DurationHours;

        await db.SaveChangesAsync(ct);

        var owners = await db.UserInventory.CountAsync(i => i.ItemId == item.Id, ct);
        return Ok(new AdminShopItemDto(item.Id, item.ItemKey, item.Name, item.PriceGems, item.Type, item.MaxStack, item.DurationHours, owners));
    }

    [HttpDelete("items/{id:int}")]
    public async Task<ActionResult> DeleteItem(int id, CancellationToken ct)
    {
        var item = await db.ShopItems.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item is null)
        {
            return NotFound(new { message = "Vật phẩm không tồn tại" });
        }

        // Xóa các bản ghi inventory liên quan nếu có
        var invs = await db.UserInventory.Where(i => i.ItemId == id).ToListAsync(ct);
        if (invs.Count > 0)
        {
            db.UserInventory.RemoveRange(invs);
        }

        db.ShopItems.Remove(item);
        await db.SaveChangesAsync(ct);

        return Ok(new { message = "Đã xóa vật phẩm thành công" });
    }

    [HttpGet("gem-transactions")]
    public async Task<ActionResult<List<GemTransactionDto>>> GetGemTransactions([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var list = await (from t in db.GemTransactions.AsNoTracking()
                          join u in db.Users.AsNoTracking() on t.UserId equals u.Id into ug
                          from u in ug.DefaultIfEmpty()
                          orderby t.CreatedAt descending
                          select new GemTransactionDto(
                              t.Id,
                              t.UserId,
                              u != null ? u.Email : "unknown@system.local",
                              u != null ? u.DisplayName : "Người dùng ẩn",
                              t.Type,
                              t.Amount,
                              t.RefType,
                              t.RefId,
                              t.CreatedAt
                          )).Take(Math.Clamp(limit, 1, 200)).ToListAsync(ct);

        return Ok(list);
    }
}

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
        int OwnersCount,
        string? ImageUrl = null);

    private static readonly Dictionary<string, string> DefaultAvatarUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["avatar-cyber-hacker"] = "/assets/avatars/cyber-hacker.svg",
        ["avatar-gold-knight"] = "/assets/avatars/gold-knight.svg",
        ["avatar-neon-ninja"] = "/assets/avatars/neon-ninja.svg",
        ["avatar-wizard"] = "/assets/avatars/wizard.svg",
        ["avatar-ai-bot"] = "/assets/avatars/ai-bot.svg",
        ["avatar-dragon"] = "/assets/avatars/dragon.svg",
    };

    private async Task<Dictionary<string, string>> GetCustomAssetsMapAsync(CancellationToken ct)
    {
        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "shop.custom_assets", ct);
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var map = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(setting.Value);
            return map != null ? new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase) : new(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

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

        var customAssets = await GetCustomAssetsMapAsync(ct);

        var result = items.Select(i => {
            string? img = null;
            if (customAssets.TryGetValue(i.ItemKey, out var cUrl)) img = cUrl;
            else if (DefaultAvatarUrls.TryGetValue(i.ItemKey, out var dUrl)) img = dUrl;
            else if (i.ItemKey.Contains("dragon", StringComparison.OrdinalIgnoreCase)) img = "/assets/avatars/dragon.svg";

            return new AdminShopItemDto(
                i.Id,
                i.ItemKey,
                i.Name,
                i.PriceGems,
                i.Type,
                i.MaxStack,
                i.DurationHours,
                ownershipCounts.GetValueOrDefault(i.Id, 0),
                img
            );
        }).ToList();

        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<ActionResult<AdminShopItemDto>> CreateItem([FromBody] CreateShopItemRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ItemKey) || string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "ItemKey và Tên vật phẩm không được để trống" });
        }

        var key = req.ItemKey.Trim().ToLowerInvariant();
        var exists = await db.ShopItems.AnyAsync(i => i.ItemKey == key, ct);
        if (exists)
        {
            return Conflict(new { message = $"ItemKey '{req.ItemKey}' đã tồn tại trong hệ thống" });
        }

        var item = new ShopItem
        {
            ItemKey = key,
            Name = req.Name.Trim(),
            PriceGems = Math.Max(0, req.PriceGems),
            Type = req.Type,
            MaxStack = Math.Max(1, req.MaxStack),
            DurationHours = req.DurationHours
        };

        db.ShopItems.Add(item);
        await db.SaveChangesAsync(ct);

        var customAssets = await GetCustomAssetsMapAsync(ct);
        string? img = customAssets.TryGetValue(item.ItemKey, out var cUrl) ? cUrl : (DefaultAvatarUrls.TryGetValue(item.ItemKey, out var dUrl) ? dUrl : null);

        return Ok(new AdminShopItemDto(item.Id, item.ItemKey, item.Name, item.PriceGems, item.Type, item.MaxStack, item.DurationHours, 0, img));
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

        var customAssets = await GetCustomAssetsMapAsync(ct);
        string? img = customAssets.TryGetValue(item.ItemKey, out var cUrl) ? cUrl : (DefaultAvatarUrls.TryGetValue(item.ItemKey, out var dUrl) ? dUrl : null);

        var owners = await db.UserInventory.CountAsync(i => i.ItemId == item.Id, ct);
        return Ok(new AdminShopItemDto(item.Id, item.ItemKey, item.Name, item.PriceGems, item.Type, item.MaxStack, item.DurationHours, owners, img));
    }

    public sealed record UploadAssetRequest(string Image, string? Name);

    [HttpPost("upload-asset")]
    public async Task<ActionResult> UploadAsset(
        [FromBody] UploadAssetRequest req,
        [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Image))
        {
            return BadRequest(new { error = "Dữ liệu ảnh không được để trống" });
        }

        try
        {
            var rawBase64 = req.Image;
            var extension = ".png";
            if (rawBase64.Contains(','))
            {
                var prefix = rawBase64[..rawBase64.IndexOf(',')];
                rawBase64 = rawBase64[(rawBase64.IndexOf(',') + 1)..];
                if (prefix.Contains("image/jpeg") || prefix.Contains("image/jpg")) extension = ".jpg";
                else if (prefix.Contains("image/webp")) extension = ".webp";
                else if (prefix.Contains("image/svg")) extension = ".svg";
            }

            var bytes = Convert.FromBase64String(rawBase64);
            if (bytes.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { error = "Kích thước ảnh không được vượt quá 5MB" });
            }

            var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(webRoot, "uploads", "shop");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"shop_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes, ct);

            var assetUrl = $"/uploads/shop/{fileName}";
            return Ok(new { url = assetUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Lỗi xử lý file ảnh: {ex.Message}" });
        }
    }

    public sealed record SaveCustomAssetRequest(string ItemKey, string ImageUrl);

    [HttpGet("custom-assets")]
    public async Task<ActionResult<Dictionary<string, string>>> GetCustomAssets(CancellationToken ct)
    {
        var map = await GetCustomAssetsMapAsync(ct);
        return Ok(map);
    }

    [HttpPost("custom-assets")]
    public async Task<ActionResult> SaveCustomAsset([FromBody] SaveCustomAssetRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ItemKey) || string.IsNullOrWhiteSpace(req.ImageUrl))
        {
            return BadRequest(new { message = "ItemKey và ImageUrl không được để trống" });
        }

        var map = await GetCustomAssetsMapAsync(ct);
        map[req.ItemKey.Trim().ToLowerInvariant()] = req.ImageUrl.Trim();

        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == "shop.custom_assets", ct);
        var json = System.Text.Json.JsonSerializer.Serialize(map);
        if (setting == null)
        {
            db.Settings.Add(new Setting
            {
                Key = "shop.custom_assets",
                Value = json,
                Description = "Custom assets for shop items (avatars, frames)",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = 1
            });
        }
        else
        {
            setting.Value = json;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return Ok(new { success = true, assets = map });
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

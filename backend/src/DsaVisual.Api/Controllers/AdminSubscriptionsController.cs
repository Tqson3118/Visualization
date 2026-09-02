using Asp.Versioning;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/admin/subscriptions")]
[Authorize(Roles = "ADMIN")]
public sealed class AdminSubscriptionsController(AppDbContext db, IDateTimeProvider clock) : ApiControllerBase
{
    public sealed record AdminSubscriptionDto(
        int Id,
        int UserId,
        string UserEmail,
        string UserDisplayName,
        string? AvatarUrl,
        string? PlanId,
        DateTime StartedAt,
        DateTime? ExpiresAt,
        int Status,
        bool IsActive,
        string? OrderRef,
        DateTime CreatedAt);

    public sealed record GrantProRequest(
        string? Email,
        int? UserId,
        string PlanId,
        int DurationDays);

    [HttpGet]
    public async Task<ActionResult<List<AdminSubscriptionDto>>> GetSubscriptions([FromQuery] string? status, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var query = from s in db.PremiumSubscriptions.AsNoTracking()
                    join u in db.Users.AsNoTracking() on s.UserId equals u.Id
                    orderby s.CreatedAt descending
                    select new
                    {
                        s.Id,
                        s.UserId,
                        u.Email,
                        u.DisplayName,
                        u.AvatarUrl,
                        s.PlanId,
                        s.StartedAt,
                        s.ExpiresAt,
                        s.Status,
                        s.OrderRef,
                        s.CreatedAt
                    };

        var list = await query.ToListAsync(ct);

        var result = list.Select(s => new AdminSubscriptionDto(
            s.Id,
            s.UserId,
            s.Email,
            s.DisplayName,
            s.AvatarUrl,
            s.PlanId,
            s.StartedAt,
            s.ExpiresAt,
            s.Status,
            s.Status == 0 && (s.ExpiresAt == null || s.ExpiresAt > now),
            s.OrderRef,
            s.CreatedAt
        )).ToList();

        if (status?.ToLowerInvariant() == "active")
        {
            result = result.Where(s => s.IsActive).ToList();
        }
        else if (status?.ToLowerInvariant() == "pending" || status?.ToLowerInvariant() == "unactivated")
        {
            result = result.Where(s => s.Status == 2).ToList();
        }
        else if (status?.ToLowerInvariant() == "expired")
        {
            result = result.Where(s => s.Status == 1 || (s.Status == 0 && s.ExpiresAt != null && s.ExpiresAt <= now)).ToList();
        }

        return Ok(result);
    }

    [HttpPost("grant")]
    public async Task<ActionResult<AdminSubscriptionDto>> GrantPro([FromBody] GrantProRequest req, CancellationToken ct)
    {
        User? user = null;
        if (req.UserId.HasValue && req.UserId.Value > 0)
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId.Value, ct);
        }
        else if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var email = req.Email.Trim().ToLowerInvariant();
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng với thông tin cung cấp" });
        }

        var now = clock.UtcNow;
        var duration = Math.Clamp(req.DurationDays, 1, 3650); // 1 ngày tới 10 năm
        var expiresAt = now.AddDays(duration);
        var orderRef = $"MANUAL-GRANT-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var planId = string.IsNullOrWhiteSpace(req.PlanId) ? "pro-admin-grant" : req.PlanId.Trim();

        user.PremiumUntil = expiresAt;

        // Kiểm tra xem đã có subscription nào đang active không
        var existing = await db.PremiumSubscriptions.FirstOrDefaultAsync(s => s.UserId == user.Id && s.Status == 0, ct);
        if (existing != null)
        {
            // Gia hạn thêm từ ngày hết hạn cũ hoặc từ now
            var baseDate = existing.ExpiresAt.HasValue && existing.ExpiresAt > now ? existing.ExpiresAt.Value : now;
            existing.ExpiresAt = baseDate.AddDays(duration);
            existing.PlanId = planId;
            existing.OrderRef = orderRef;
            user.PremiumUntil = existing.ExpiresAt;
            await db.SaveChangesAsync(ct);

            return Ok(new AdminSubscriptionDto(
                existing.Id,
                user.Id,
                user.Email,
                user.DisplayName,
                user.AvatarUrl,
                existing.PlanId,
                existing.StartedAt,
                existing.ExpiresAt,
                existing.Status,
                true,
                existing.OrderRef,
                existing.CreatedAt
            ));
        }

        var sub = new PremiumSubscription
        {
            UserId = user.Id,
            PlanId = planId,
            StartedAt = now,
            ExpiresAt = expiresAt,
            Status = 0, // Active
            OrderRef = orderRef,
            CreatedAt = now
        };

        db.PremiumSubscriptions.Add(sub);
        await db.SaveChangesAsync(ct);

        return Ok(new AdminSubscriptionDto(
            sub.Id,
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            sub.PlanId,
            sub.StartedAt,
            sub.ExpiresAt,
            sub.Status,
            true,
            sub.OrderRef,
            sub.CreatedAt
        ));
    }

    [HttpPost("{id:int}/revoke")]
    public async Task<ActionResult<AdminSubscriptionDto>> RevokePro([FromRoute] int id, CancellationToken ct)
    {
        var sub = await db.PremiumSubscriptions.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sub is null)
        {
            return NotFound(new { message = "Không tìm thấy thông tin gói đăng ký." });
        }

        var now = clock.UtcNow;
        sub.Status = 2; // Cancelled/Revoked
        if (sub.ExpiresAt == null || sub.ExpiresAt > now)
        {
            sub.ExpiresAt = now;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == sub.UserId, ct);
        if (user is not null)
        {
            // Check if user has any other active subscriptions
            var hasOtherActive = await db.PremiumSubscriptions
                .AnyAsync(s => s.UserId == user.Id && s.Id != sub.Id && s.Status == 0 && (s.ExpiresAt == null || s.ExpiresAt > now), ct);
            if (!hasOtherActive)
            {
                user.PremiumUntil = null;
            }
        }

        await db.SaveChangesAsync(ct);

        return Ok(new AdminSubscriptionDto(
            sub.Id,
            user?.Id ?? sub.UserId,
            user?.Email ?? "",
            user?.DisplayName ?? "",
            user?.AvatarUrl,
            sub.PlanId,
            sub.StartedAt,
            sub.ExpiresAt,
            sub.Status,
            false,
            sub.OrderRef,
            sub.CreatedAt
        ));
    }
}

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
    [Route("api/v{version:apiVersion}/premium")]
    [RequireJwtRole]
    public class PremiumController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public PremiumController(IApplicationDbContext ctx) { _ctx = ctx; }

        private Guid Uid() => Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var g) ? g : Guid.Empty;

        /// <summary>GET /premium/status — { isPremium, plan, expiresAt }</summary>
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Uid());
            var expiresAt = user?.IsPremium == true ? DateTime.UtcNow.AddMonths(1) : (DateTime?)null;
            return Ok(new { isPremium = user?.IsPremium ?? false, plan = user?.IsPremium == true ? "premium-monthly" : (string?)null, expiresAt = expiresAt?.ToString("o") });
        }

        /// <summary>POST /premium/upgrade — body { planId } → { orderId, planId, expiresAt, contentRef }</summary>
        [HttpPost("upgrade")]
        public async Task<IActionResult> Upgrade([FromBody] UpgradeRequest req)
        {
            var uid = Uid();
            var amount = req.PlanId switch { "premium-quarterly" => 299000m, "premium-yearly" => 499000m, _ => 199000m };
            var paymentCode = "FPT" + DateTime.UtcNow.ToString("yyMMdd") + new Random().Next(10000000, 99999999).ToString("D8");
            var order = new Order(uid, paymentCode, amount);
            _ctx.Set<Order>().Add(order);
            await _ctx.SaveChangesAsync(CancellationToken.None);
            // contentRef khớp GP-T7 (mã CK DSV{userId}T{months})
            var months = req.PlanId switch { "premium-quarterly" => 3, "premium-yearly" => 12, _ => 1 };
            return Ok(new
            {
                orderId = order.Id.ToString(),
                planId = req.PlanId ?? "premium-monthly",
                expiresAt = DateTime.UtcNow.AddMonths(months).ToString("o"),
                contentRef = $"DSV{uid:N}{months}T",
            });
        }

        /// <summary>POST /premium/mock-pay — body { orderId } → premium status</summary>
        [HttpPost("mock-pay")]
        public async Task<IActionResult> MockPay([FromBody] MockPayRequest req)
        {
            var uid = Uid();
            var orderId = Guid.TryParse(req.OrderId, out var g) ? g : Guid.Empty;
            var order = await _ctx.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == uid);
            if (order == null) return NotFound(new { message = "Đơn hàng không tồn tại." });
            order.MarkAsCompleted();
            order.SetTransactionReference("SEPAY-MOCK-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant());
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == uid);
            if (user != null) user.SetPremiumStatus(true);
            await _ctx.SaveChangesAsync(CancellationToken.None);
            return Ok(new { isPremium = true, plan = "premium-" + (order.Amount >= 400000 ? "yearly" : order.Amount >= 250000 ? "quarterly" : "monthly"), expiresAt = DateTime.UtcNow.AddMonths(12).ToString("o") });
        }
    }

    public class UpgradeRequest { public string? PlanId { get; set; } }
    public class MockPayRequest { public string? OrderId { get; set; } }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Engine;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/payment")]
    public class StatelessPaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        private const decimal PremiumPrice = 199_000m;
        private const string BankId = "MBBank";
        private const string BankAccount = "99999999999";
        private const string AccountName = "DSA VISUALIZER ACADEMY";

        private static readonly List<StatelessPremiumFeature> PremiumFeatures = new()
        {
            new() { Id = "unlimited-runs",    Name = "Biên dịch không giới hạn", Description = "Chạy thuật toán tùy chọn bao nhiêu lần tùy ý", Icon = "⚡", RequiresPremium = true },
            new() { Id = "advanced-lessons",  Name = "Bài giảng cao cấp",        Description = "Truy cập SOLID, Design Patterns, System Design chuyên sâu", Icon = "📚", RequiresPremium = true },
            new() { Id = "premium-sandbox",   Name = "Sandbox đặc biệt",         Description = "Mở khóa sân chơi Premium với dữ liệu lớn", Icon = "🎮", RequiresPremium = true },
            new() { Id = "leaderboard-badge", Name = "Huy hiệu Premium",         Description = "Hiển thị huy hiệu vàng trên bảng xếp hạng", Icon = "👑", RequiresPremium = true },
            new() { Id = "basic-viz",         Name = "Trực quan hóa cơ bản",     Description = "Sorting, BFS, DFS với dữ liệu mẫu", Icon = "📊", RequiresPremium = false },
            new() { Id = "quiz-basic",        Name = "Quiz cơ bản",              Description = "Trắc nghiệm 6 chủ đề miễn phí", Icon = "❓", RequiresPremium = false },
        };

        public StatelessPaymentController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("config")]
        public ActionResult<StatelessPaymentConfigDto> GetConfig()
        {
            return Ok(new StatelessPaymentConfigDto
            {
                PremiumPrice = PremiumPrice,
                Currency = "VND",
                BankId = BankId,
                BankAccount = BankAccount,
                AccountName = AccountName,
                SupportedMethods = new List<string> { "vietqr", "bank_transfer", "momo" },
                PremiumFeatures = PremiumFeatures
            });
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<StatelessOrderDto>> Checkout([FromBody] StatelessCheckoutRequest request)
        {
            try
            {
                var user = await ResolveUser(request.UserId);
                if (user == null)
                    return BadRequest(new { error = "CHECKOUT_FAILED", message = "Người dùng không tồn tại." });

                if (user.IsPremium)
                    return Conflict(new { error = "ALREADY_PREMIUM", message = "Tài khoản đã là Premium. Không cần thanh toán thêm." });

                var paymentCode = $"VDSA{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
                var order = new Order(user.Id, paymentCode, PremiumPrice);
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                return Ok(MapOrder(order));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "CHECKOUT_FAILED", message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<ActionResult<StatelessOrderDto>> Verify([FromBody] StatelessVerifyRequest request)
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để xác nhận hóa đơn." });

            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id.ToString() == request.OrderId || o.PaymentCode == request.OrderId);
                if (order == null)
                    return NotFound(new { error = "ORDER_NOT_FOUND", message = "Hóa đơn không tồn tại." });

                if (order.UserId.ToString() != currentUserId)
                    return Unauthorized(new { error = "UNAUTHORIZED", message = "Bạn không có quyền xác nhận hóa đơn này." });

                if (order.Status != "Completed")
                {
                    order.MarkAsCompleted();
                    PromotePremium(order.User);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(MapOrder(order));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "ORDER_NOT_FOUND", message = ex.Message });
            }
        }

        [HttpGet("orders/{orderId}/status")]
        public async Task<ActionResult<StatelessOrderDto>> GetOrderStatus(string orderId)
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để xem trạng thái hóa đơn." });

            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id.ToString() == orderId || o.PaymentCode == orderId);
                if (order == null)
                    return NotFound(new { error = "ORDER_NOT_FOUND", message = "Hóa đơn không tồn tại." });

                if (order.UserId.ToString() != currentUserId)
                    return Unauthorized(new { error = "UNAUTHORIZED", message = "Bạn không có quyền truy cập hóa đơn này." });

                return Ok(MapOrder(order));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "ORDER_NOT_FOUND", message = ex.Message });
            }
        }

        [HttpPost("simulate-webhook")]
        public async Task<ActionResult<StatelessOrderDto>> SimulateWebhook([FromBody] StatelessVerifyRequest request)
        {
            var env = HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            if (env == null || !env.IsDevelopment())
                return NotFound(new { error = "NOT_FOUND", message = "Mô phỏng thanh toán chỉ khả dụng trong môi trường phát triển." });

            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để mô phỏng thanh toán." });

            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id.ToString() == request.OrderId || o.PaymentCode == request.OrderId);
                if (order == null)
                    return NotFound(new { error = "ORDER_NOT_FOUND", message = "Hóa đơn không tồn tại." });

                if (order.UserId.ToString() != currentUserId)
                    return Unauthorized(new { error = "UNAUTHORIZED", message = "Bạn không có quyền mô phỏng hóa đơn này." });

                if (order.Status != "Completed")
                {
                    order.MarkAsCompleted();
                    PromotePremium(order.User);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(MapOrder(order));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "ORDER_NOT_FOUND", message = ex.Message });
            }
        }

        [HttpGet("premium-status")]
        public async Task<ActionResult<StatelessPremiumStatusDto>> GetPremiumStatus([FromQuery] string? userId)
        {
            var user = await ResolveUser(userId);
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

            var isPremium = user.IsPremium;
            return Ok(new StatelessPremiumStatusDto
            {
                IsPremium = isPremium,
                UpgradedAt = isPremium ? user.PremiumExpiresAt ?? DateTime.UtcNow : null,
                Plan = isPremium ? "lifetime" : "free",
                UnlockedFeatures = PremiumFeatures.Where(f => !f.RequiresPremium || isPremium).Select(f => f.Id).ToList(),
            });
        }

        [HttpGet("check-access")]
        public async Task<ActionResult<object>> CheckFeatureAccess([FromQuery] string? userId, [FromQuery] string featureId)
        {
            var user = await ResolveUser(userId);
            if (user == null)
                return Ok(new { userId, featureId, hasAccess = true });

            var feature = PremiumFeatures.FirstOrDefault(f => f.Id == featureId);
            var hasAccess = feature == null || !feature.RequiresPremium || user.IsPremium;
            return Ok(new { userId = user.Id.ToString(), featureId, hasAccess });
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<List<StatelessTransactionLogEntry>>> GetTransactions([FromQuery] string? userId)
        {
            var user = await ResolveUser(userId);
            if (user == null)
                return Ok(new List<StatelessTransactionLogEntry>());

            var orders = await _dbContext.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(orders.Select(o => new StatelessTransactionLogEntry
            {
                Id = o.Id.ToString(),
                OrderId = o.Id.ToString(),
                UserId = o.UserId.ToString(),
                Action = o.Status == "Completed" ? "PAYMENT_VERIFIED" : "CHECKOUT_CREATED",
                Amount = o.Amount,
                Timestamp = o.CompletedAt ?? o.CreatedAt,
                Status = o.Status
            }).ToList());
        }

        private async Task<User?> ResolveUser(string? userId)
        {
            var user = await ResolveUserByRequestedId(userId);
            if (user != null) return user;
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "demo@visualizationdsa.dev");
        }

        private async Task<User?> ResolveUserByRequestedId(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            if (Guid.TryParse(userId, out var guid))
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userId);
        }

private static void PromotePremium(User user)
        {
            user.SetPremium(null);
            user.RecordActivity();
        }

        private static StatelessOrderDto MapOrder(Order o)
        {
            var qrUrl = $"https://img.vietqr.io/image/{BankId}-{BankAccount}-compact.png" +
                        $"?amount={(int)o.Amount}&addInfo={o.PaymentCode}&accountName={Uri.EscapeDataString(AccountName)}";
            return new StatelessOrderDto
            {
                Id = o.Id.ToString(),
                UserId = o.UserId.ToString(),
                PaymentCode = o.PaymentCode,
                Amount = o.Amount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                CompletedAt = o.CompletedAt,
                BankId = BankId,
                BankAccount = BankAccount,
                AccountName = AccountName,
                QrUrl = qrUrl
            };
        }
    }
}
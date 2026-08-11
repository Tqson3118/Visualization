using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Asp.Versioning;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.Services;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentsController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        
        
        
        
        [HttpPost("order")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> CreateOrder()
        {
            var userId = GetCurrentUserId();
            try
            {
                var order = await _paymentService.CreateOrderAsync(userId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
        }

        
        
        
        
        [HttpGet("orders/{orderId}/status")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> GetOrderStatus(Guid orderId)
        {
            var userId = GetCurrentUserId();
            try
            {
                var order = await _paymentService.GetOrderStatusAsync(orderId, userId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // UC-46 — Xem danh sách đơn hàng của user
        [HttpGet("orders/my")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _paymentService.GetUserOrdersAsync(userId);
            return Ok(new { items = orders });
        }

        
        
        
        
        [HttpPost("simulate-demo-webhook")]
        [Authorize]
        public async Task<IActionResult> SimulateDemoWebhook([FromBody] SimulateDemoWebhookRequest request)
        {
            var env = HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            if (env == null || !env.IsDevelopment())
                return NotFound(new { message = "Mô phỏng thanh toán chỉ khả dụng trong môi trường phát triển." });

            var userId = GetCurrentUserId();
            OrderDto order;
            try
            {
                order = await _paymentService.GetOrderStatusAsync(request.OrderId, userId);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

            var payload = new SePayWebhookPayload
            {
                Id = new Random().Next(1, int.MaxValue),
                Gateway = "MBBank",
                TransferType = "in",
                AccountNumber = "99999999999",
                Code = order.PaymentCode,
                Content = $"Chuyen khoan premium {order.PaymentCode}",
                TransferAmount = order.Amount,
            };

            try
            {
                var isProcessed = await _paymentService.ProcessSePayWebhookAsync(payload);
                if (isProcessed)
                {
                    return Ok(new { success = true, orderId = order.Id });
                }
                return BadRequest(new { success = false, message = "Giao dịch không khớp hoặc không hợp lệ để kích hoạt Premium." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveSePayWebhook([FromBody] SePayWebhookPayload payload)
        {
            
            var secretKey = _configuration["SePay:WebhookSecret"];
            var signatureHeader = Request.Headers["X-SePay-Signature"].ToString();

            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Cổng thanh toán chưa được cấu hình khóa bảo mật webhook." });
            }

            if (string.IsNullOrEmpty(signatureHeader))
            {
                return Unauthorized(new { message = "Thiếu chữ ký xác thực Webhook." });
            }

            
            var rawMessage = $"id={payload.Id}&amount={payload.TransferAmount}&code={payload.Code ?? string.Empty}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawMessage));
            var computedSignature = Convert.ToHexString(computedHash).ToLower();

            var computedBytes = System.Text.Encoding.UTF8.GetBytes(computedSignature);
            var headerBytes = System.Text.Encoding.UTF8.GetBytes(signatureHeader.Trim().ToLowerInvariant());

            if (computedBytes.Length != headerBytes.Length || 
                !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(computedBytes, headerBytes))
            {
                return Unauthorized(new { message = "Chữ ký webhook không hợp lệ." });
            }

            
            try
            {
                var isProcessed = await _paymentService.ProcessSePayWebhookAsync(payload);
                if (isProcessed)
                {
                    return Ok(new { success = true });
                }
                
                
                
                return Ok(new { success = false, message = "Giao dịch không khớp hoặc không hợp lệ để kích hoạt Premium." });
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");
            return Guid.Parse(claim!);
        }
    }

    public class SimulateDemoWebhookRequest
    {
        public Guid OrderId { get; set; }
    }
}

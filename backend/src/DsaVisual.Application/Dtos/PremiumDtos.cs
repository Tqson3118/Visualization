namespace DsaVisual.Application.Dtos;

/// <summary>Trạng thái Premium — API_REFERENCE.md §3.13/§4.14 (FR-10.7).</summary>
public sealed class PremiumStatusDto
{
    public string? PlanId { get; set; }
    public string? Plan => PlanId;
    public DateTime? StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = "none";   // none/active/expired
    public bool IsPremium => Status == "active";
}

/// <summary>Nâng cấp Premium — POST /premium/upgrade {planId} (checkout mô phỏng).</summary>
public sealed class PremiumUpgradeRequest
{
    public string PlanId { get; set; } = string.Empty;   // "1m" | "3m" | "12m"
}

/// <summary>Đơn checkout mô phỏng — POST /premium/upgrade.</summary>
public sealed class PremiumUpgradeResultDto
{
    public int OrderId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Mã CK tự động DSV{userId}T{months} (VD DSV1002T3) — GP-T7: hiển thị trên QR chuyển khoản MB Bank.</summary>
    public string ContentRef { get; set; } = string.Empty;
}

/// <summary>Thanh toán mô phỏng — POST /premium/mock-pay {orderId}.</summary>
public sealed class PremiumMockPayRequest
{
    public int OrderId { get; set; }
}

/// <summary>Webhook nhận biến động số dư từ SePay / Casso / Bank Gateway</summary>
public sealed class PaymentWebhookRequest
{
    public string? Content { get; set; }
    public string? Description { get; set; }
    public decimal? TransferAmount { get; set; }
    public decimal? Amount { get; set; }
    public string? ReferenceCode { get; set; }
    public string? OrderRef { get; set; }
}

public sealed class PaymentWebhookResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? PlanId { get; set; }
}

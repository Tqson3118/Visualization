namespace DsaVisual.Application.Services;

/// <summary>
/// 1 public seam duy nhất Module J (ADR-011); nội bộ ≥ 2 module:
/// hearts/session, quest/streak, shop/gems, premium (job downgrade), achievement (SDD §5.4).
/// TODO: triển khai — skeleton v2 theo SDD §5.1.
/// </summary>
public interface IGamificationService
{
}

public sealed class GamificationService : IGamificationService
{
    // TODO: implement (SDD §5.4, API_REFERENCE §4.14)
}

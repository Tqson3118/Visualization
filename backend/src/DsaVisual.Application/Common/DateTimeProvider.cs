namespace DsaVisual.Application.Common;

/// <summary>
/// Wrapper thời gian để test (SDD §5.3.10: thời gian UTC, dùng DateTimeProvider).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

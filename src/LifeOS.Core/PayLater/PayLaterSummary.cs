namespace LifeOS.Core.PayLater;

public sealed class PayLaterSummary
{
    public required int TotalOpen { get; init; }

    public required decimal TotalAmountOpen { get; init; }

    public required decimal DueThisWeekAmount { get; init; }

    public required decimal OverdueAmount { get; init; }

    public required int DueThisWeekCount { get; init; }

    public required int OverdueCount { get; init; }

    public required int HighPressureCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}

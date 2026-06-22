namespace LifeOS.Core.FollowUps;

public sealed class FollowUpSummary
{
    public required int TotalOpen { get; init; }

    public required int WaitingCount { get; init; }

    public required int NeedsActionCount { get; init; }

    public required int OverdueCount { get; init; }

    public required int DueTodayCount { get; init; }

    public required int MoneyLinkedCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}
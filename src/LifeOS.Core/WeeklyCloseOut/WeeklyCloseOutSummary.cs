namespace LifeOS.Core.WeeklyCloseOut;

public sealed class WeeklyCloseOutSummary
{
    public required int TotalEntries { get; init; }

    public required int EntriesThisWeek { get; init; }

    public required DateOnly? LatestWeekStart { get; init; }

    public required bool HasCurrentWeekCloseOut { get; init; }

    public required int WaitingOnCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}

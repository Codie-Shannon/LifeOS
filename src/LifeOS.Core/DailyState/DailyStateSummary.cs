namespace LifeOS.Core.DailyState;

public sealed class DailyStateSummary
{
    public int TodayOpenCount { get; init; }

    public int DoneTodayCount { get; init; }

    public int PassiveWaitingCount { get; init; }

    public int DoNotChaseCount { get; init; }

    public int ScheduledMessageCount { get; init; }

    public IReadOnlyList<DailyStateItem> TodayItems { get; init; } = [];

    public IReadOnlyList<DailyStateItem> HiddenItems { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

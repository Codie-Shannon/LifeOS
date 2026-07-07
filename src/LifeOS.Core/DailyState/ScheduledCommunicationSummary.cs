namespace LifeOS.Core.DailyState;

public sealed class ScheduledCommunicationSummary
{
    public int PlannedTodayCount { get; init; }

    public int WaitingAfterSendCount { get; init; }

    public IReadOnlyList<ScheduledCommunicationItem> PlannedToday { get; init; } = [];

    public IReadOnlyList<ScheduledCommunicationItem> WaitingAfterSend { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

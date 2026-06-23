namespace LifeOS.Core.Agenda;

public sealed class AgendaSummary
{
    public required int TotalOpen { get; init; }

    public required int DueTodayCount { get; init; }

    public required int OverdueCount { get; init; }

    public required int ThisWeekCount { get; init; }

    public required int HighPressureCount { get; init; }

    public required int CompletedCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}

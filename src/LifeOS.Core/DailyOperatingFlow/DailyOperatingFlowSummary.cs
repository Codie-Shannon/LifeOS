namespace LifeOS.Core.DailyOperatingFlow;

public sealed class DailyOperatingFlowSummary
{
    public int TodayOpenCount { get; init; }

    public int DoneTodayCount { get; init; }

    public int InProgressCount { get; init; }

    public int WaitingCount { get; init; }

    public int ParkedCount { get; init; }

    public int PinnedCount { get; init; }

    public int HighPressureCount { get; init; }

    public int LowEnergyFallbackCount { get; init; }

    public int StopPointCount { get; init; }

    public IReadOnlyList<DailyOperatingFlowBlock> TodayBlocks { get; init; } = [];

    public IReadOnlyList<DailyOperatingFlowBlock> WaitingBlocks { get; init; } = [];

    public IReadOnlyList<DailyOperatingFlowBlock> ParkedBlocks { get; init; } = [];

    public IReadOnlyList<DailyOperatingFlowBlock> RecoveryBlocks { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

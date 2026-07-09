namespace LifeOS.Core.DailyOperatingFlow;

public static class DailyOperatingFlowCalculator
{
    public static DailyOperatingFlowSummary Calculate(IEnumerable<DailyOperatingFlowBlock> blocks, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var blockList = blocks.ToList();

        var todayBlocks = blockList
            .Where(block => block.IsTodayVisible(today))
            .OrderByDescending(block => block.IsPinned)
            .ThenByDescending(block => block.Pressure)
            .ThenBy(block => block.Kind)
            .ThenBy(block => block.Title)
            .ToList();

        var waitingBlocks = blockList
            .Where(block => block.IsOpen && block.Status == DailyOperatingFlowStatus.Waiting)
            .OrderByDescending(block => block.Date)
            .ThenBy(block => block.Title)
            .ToList();

        var parkedBlocks = blockList
            .Where(block => block.IsOpen && block.Status == DailyOperatingFlowStatus.Parked)
            .OrderByDescending(block => block.Date)
            .ThenBy(block => block.Title)
            .ToList();

        var recoveryBlocks = blockList
            .Where(block => block.IsOpen && block.Kind is DailyOperatingFlowKind.LowEnergyFallback or DailyOperatingFlowKind.RecoveryBlock or DailyOperatingFlowKind.StopPoint)
            .OrderBy(block => block.Kind)
            .ThenBy(block => block.Title)
            .ToList();

        var reasons = new List<string>();

        if (todayBlocks.Count > 0) reasons.Add($"{todayBlocks.Count} operating block(s) are visible for today.");
        if (waitingBlocks.Count > 0) reasons.Add($"{waitingBlocks.Count} block(s) are waiting and should not be forced.");
        if (recoveryBlocks.Count > 0) reasons.Add($"{recoveryBlocks.Count} recovery/low-energy/stop-point block(s) exist for controlled days.");
        if (todayBlocks.Any(block => block.Pressure >= DailyOperatingFlowPressure.High)) reasons.Add("High-pressure daily blocks exist; use the next safest action, not brute force.");
        if (reasons.Count == 0) reasons.Add("No daily operating flow blocks are visible today.");

        return new DailyOperatingFlowSummary
        {
            TodayOpenCount = todayBlocks.Count,
            DoneTodayCount = blockList.Count(block => block.Date == today && block.Status == DailyOperatingFlowStatus.Done),
            InProgressCount = blockList.Count(block => block.Date == today && block.Status == DailyOperatingFlowStatus.InProgress),
            WaitingCount = waitingBlocks.Count,
            ParkedCount = parkedBlocks.Count,
            PinnedCount = todayBlocks.Count(block => block.IsPinned),
            HighPressureCount = todayBlocks.Count(block => block.Pressure >= DailyOperatingFlowPressure.High),
            LowEnergyFallbackCount = blockList.Count(block => block.IsOpen && block.Kind == DailyOperatingFlowKind.LowEnergyFallback),
            StopPointCount = blockList.Count(block => block.IsOpen && block.Kind == DailyOperatingFlowKind.StopPoint),
            TodayBlocks = todayBlocks,
            WaitingBlocks = waitingBlocks,
            ParkedBlocks = parkedBlocks,
            RecoveryBlocks = recoveryBlocks,
            Reasons = reasons
        };
    }
}

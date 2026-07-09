using LifeOS.Core.DailyOperatingFlow;

namespace LifeOS.Shared.DailyOperatingFlow;

public static class DailyOperatingFlowDemoData
{
    public static List<DailyOperatingFlowBlock> CreateDefaultBlocks()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new DailyOperatingFlowBlock
            {
                Date = today,
                Title = "Morning command check",
                Area = "LifeOS Operating Flow",
                Detail = "Check pressure, pick the next safest action, and avoid opening ten unrelated loops.",
                TimeWindow = "Morning",
                Kind = DailyOperatingFlowKind.Anchor,
                Status = DailyOperatingFlowStatus.Planned,
                Pressure = DailyOperatingFlowPressure.Normal,
                NextAction = "Open Command Centre, check pressure, then choose one active block.",
                IsPinned = true
            },
            new DailyOperatingFlowBlock
            {
                Date = today,
                Title = "Package next LifeOS build",
                Area = "Portfolio Build",
                Detail = "Prepare the next build pack only after the previous group is committed and pushed.",
                TimeWindow = "Build block",
                Kind = DailyOperatingFlowKind.NextAction,
                Status = DailyOperatingFlowStatus.Planned,
                Pressure = DailyOperatingFlowPressure.High,
                NextAction = "Generate one safe pack, run build, commit, and push.",
                IsPinned = true
            },
            new DailyOperatingFlowBlock
            {
                Date = today,
                Title = "Portfolio review lead check",
                Area = "Relationship Radar",
                Detail = "A fictional waiting checkpoint that should not become noisy chasing.",
                TimeWindow = "After main work",
                Kind = DailyOperatingFlowKind.WaitingCheckpoint,
                Status = DailyOperatingFlowStatus.Waiting,
                Pressure = DailyOperatingFlowPressure.Normal,
                NextAction = "Wait for the correct follow-up window before acting."
            },
            new DailyOperatingFlowBlock
            {
                Date = today,
                Title = "Low-energy admin fallback",
                Area = "Personal Admin",
                Detail = "Small admin cleanup that is safe to do when energy drops.",
                TimeWindow = "Low-energy option",
                Kind = DailyOperatingFlowKind.LowEnergyFallback,
                Status = DailyOperatingFlowStatus.Planned,
                Pressure = DailyOperatingFlowPressure.Calm,
                NextAction = "Do one low-friction admin cleanup item only."
            },
            new DailyOperatingFlowBlock
            {
                Date = today,
                Title = "Stop point after screenshot group",
                Area = "Build Safety",
                Detail = "When a screenshot group is complete, stop, commit, push, and avoid creating a mess.",
                TimeWindow = "End block",
                Kind = DailyOperatingFlowKind.StopPoint,
                Status = DailyOperatingFlowStatus.Planned,
                Pressure = DailyOperatingFlowPressure.Normal,
                NextAction = "Commit the clean checkpoint before continuing."
            }
        ];
    }
}

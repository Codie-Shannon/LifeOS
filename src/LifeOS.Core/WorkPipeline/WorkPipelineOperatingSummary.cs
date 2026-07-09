namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineOperatingSummary
{
    public string Version { get; init; } = "v4.5";

    public string Mode { get; init; } = "Work Pipeline";

    public string Rule { get; init; } = string.Empty;

    public string PressureLabel { get; init; } = "Low";

    public int TotalItems { get; init; }

    public int OpenItems { get; init; }

    public int ActiveItems { get; init; }

    public int WaitingOnMeItems { get; init; }

    public int WaitingOnOthersItems { get; init; }

    public int BlockedItems { get; init; }

    public int TodayActionItems { get; init; }

    public int FollowUpsNow { get; init; }

    public int FollowUpsDueSoon { get; init; }

    public int MissingFollowUpDates { get; init; }

    public int InvoiceReadyItems { get; init; }

    public int TimesheetReadyItems { get; init; }

    public int PaymentExpectedItems { get; init; }

    public int ProofNeededItems { get; init; }

    public int WarmOrParkedItems { get; init; }

    public int ReviewNeededItems { get; init; }

    public decimal ExpectedValueVisible { get; init; }

    public decimal ExpectedValueExcludedFromSafe { get; init; }

    public IReadOnlyList<string> Reasons { get; init; } = [];

    public IReadOnlyList<string> IntegrityWarnings { get; init; } = [];

    public IReadOnlyList<WorkPipelineOperatingSignal> CommandCentreSignals { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> TodayTriage { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> ActiveWork { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WaitingOnMe { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WaitingOnOthers { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> BlockedWork { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> InvoiceReadiness { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> PaymentExpected { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> ProofNeeded { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WarmOrParked { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> NeedsReview { get; init; } = [];
}

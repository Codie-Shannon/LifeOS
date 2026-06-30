namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineSummary
{
    public int TotalItems { get; init; }

    public int OpenItems { get; init; }

    public int ActiveItems { get; init; }

    public int WaitingItems { get; init; }

    public int BlockedItems { get; init; }

    public int WarmItems { get; init; }

    public int ParkedItems { get; init; }

    public int ArchivedItems { get; init; }

    public int FollowUpsOverdue { get; init; }

    public int FollowUpsDueToday { get; init; }

    public int FollowUpsDueSoon { get; init; }

    public int FollowUpsScheduled { get; init; }

    public int FollowUpsMissingDate { get; init; }

    public int KeepWarmDue { get; init; }

    public int BillableItems { get; init; }

    public int TimesheetsNeeded { get; init; }

    public int InvoicesNeeded { get; init; }

    public int PaymentsExpected { get; init; }

    public decimal ExpectedValueTotal { get; init; }

    public IReadOnlyList<WorkPipelineItem> PriorityItems { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> DueFollowUps { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> BlockedWork { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WaitingWork { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> MoneyWork { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

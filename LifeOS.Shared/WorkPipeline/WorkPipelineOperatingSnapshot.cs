using LifeOS.Core.WorkPipeline;

namespace LifeOS.Shared.WorkPipeline;

public sealed class WorkPipelineOperatingSnapshot
{
    public DateOnly Date { get; init; }

    public string StorageFilePath { get; init; } = string.Empty;

    public IReadOnlyList<WorkPipelineItem> Items { get; init; } = [];

    public WorkPipelineOperatingSummary Summary { get; init; } = new();

    public IReadOnlyList<string> SpineBridgeNotes { get; init; } = [];

    public IReadOnlyList<string> PaymentCalendarBridgeNotes { get; init; } = [];

    public IReadOnlyList<string> ProofBridgeNotes { get; init; } = [];
}

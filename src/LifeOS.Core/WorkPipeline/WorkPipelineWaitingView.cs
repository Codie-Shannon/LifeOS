namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineWaitingView
{
    public IReadOnlyList<WorkPipelineItem> WaitingOnOthers { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WaitingOnMe { get; init; } = [];

    public IReadOnlyList<WorkPipelineItem> WaitingWithoutOwner { get; init; } = [];

    public int TotalWaiting => WaitingOnOthers.Count + WaitingOnMe.Count + WaitingWithoutOwner.Count;
}

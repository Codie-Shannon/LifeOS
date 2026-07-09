namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineOperatingSignal
{
    public string Label { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    public WorkPipelinePriority Priority { get; init; } = WorkPipelinePriority.Normal;

    public WorkPipelineReviewBucket Bucket { get; init; } = WorkPipelineReviewBucket.Clean;
}

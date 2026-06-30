namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineCommandCentreSignal
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string Detail { get; init; }

    public WorkPipelinePriority Priority { get; init; } = WorkPipelinePriority.Normal;
}

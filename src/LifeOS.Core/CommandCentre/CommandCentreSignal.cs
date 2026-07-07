namespace LifeOS.Core.CommandCentre;

public sealed class CommandCentreSignal
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public CommandCentreSignalType Type { get; init; } = CommandCentreSignalType.General;

    public CommandCentreSignalPriority Priority { get; init; } = CommandCentreSignalPriority.Normal;

    public string Title { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    public string NextAction { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public string RelatedItemId { get; init; } = string.Empty;

    public bool IsTodayVisible { get; init; } = true;

    public bool RequiresAction { get; init; }

    public bool IsMoneyRelated { get; init; }

    public bool IsProofRelated { get; init; }

    public bool IsWaitingState { get; init; }

    public DateOnly? DueDate { get; init; }

    public int SortWeight { get; init; }
}

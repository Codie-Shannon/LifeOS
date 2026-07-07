namespace LifeOS.Core.CommandCentre;

public sealed class CommandCentreTodayAction
{
    public string Title { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public CommandCentreSignalPriority Priority { get; init; } = CommandCentreSignalPriority.Normal;

    public string Source { get; init; } = string.Empty;
}

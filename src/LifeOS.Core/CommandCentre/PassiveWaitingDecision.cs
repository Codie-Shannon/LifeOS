namespace LifeOS.Core.CommandCentre;

public sealed class PassiveWaitingDecision
{
    public bool ShouldWait { get; init; }

    public bool ShowInToday { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string SuggestedAction { get; init; } = string.Empty;
}

namespace LifeOS.Core.ItemState;

public sealed class LifeOsStateTransitionRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public LifeOsItemType Type { get; set; } = LifeOsItemType.Bill;

    public LifeOsItemState FromState { get; set; } = LifeOsItemState.Open;

    public LifeOsItemState ToState { get; set; } = LifeOsItemState.NeedsReview;

    public string Label { get; set; } = string.Empty;

    public string Requirement { get; set; } = string.Empty;

    public string ResultingPressure { get; set; } = string.Empty;

    public bool RequiresEvidence { get; set; }

    public bool RequiresManualReview { get; set; } = true;

    public bool IsDestructive { get; set; }
}

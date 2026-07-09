namespace LifeOS.Core.LifeOsSpine;

public sealed class LifeOsStatefulItemRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public LifeOsItemType ItemType { get; set; } = LifeOsItemType.Bill;

    public string Name { get; set; } = string.Empty;

    public string SourceExamples { get; set; } = string.Empty;

    public List<LifeOsItemState> AllowedStates { get; set; } = [];

    public string EvidenceRule { get; set; } = string.Empty;

    public string PressureRule { get; set; } = string.Empty;

    public string LandingRule { get; set; } = string.Empty;

    public string ReviewGate { get; set; } = string.Empty;

    public bool RequiredForV4 { get; set; }
}

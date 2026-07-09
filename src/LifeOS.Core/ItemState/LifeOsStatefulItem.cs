namespace LifeOS.Core.ItemState;

public sealed class LifeOsStatefulItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public LifeOsItemType Type { get; set; } = LifeOsItemType.Bill;

    public LifeOsItemState State { get; set; } = LifeOsItemState.Open;

    public LifeOsItemSourceKind SourceKind { get; set; } = LifeOsItemSourceKind.Manual;

    public LifeOsItemRiskLevel RiskLevel { get; set; } = LifeOsItemRiskLevel.Medium;

    public List<LifeOsItemImpactArea> ImpactAreas { get; set; } = [];

    public decimal? Amount { get; set; }

    public DateTime? DueDate { get; set; }

    public string PersonOrProvider { get; set; } = string.Empty;

    public string LinkedProject { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string ReviewGate { get; set; } = string.Empty;

    public string PressureSignal { get; set; } = string.Empty;

    public string SafeNextAction { get; set; } = string.Empty;

    public bool Trusted { get; set; }

    public bool AffectsSafeToSpend { get; set; }

    public bool AffectsAgenda { get; set; }

    public bool AffectsWeeklyCloseOut { get; set; }

    public bool AffectsCommandCentre { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

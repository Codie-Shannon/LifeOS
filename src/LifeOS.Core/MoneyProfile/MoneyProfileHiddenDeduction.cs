using LifeOS.Core.ItemState;

namespace LifeOS.Core.MoneyProfile;

public sealed class MoneyProfileHiddenDeduction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public MoneyProfileDeductionKind Kind { get; set; } = MoneyProfileDeductionKind.CustomReserve;

    public MoneyProfileSourceKind SourceKind { get; set; } = MoneyProfileSourceKind.Manual;

    public LifeOsItemState State { get; set; } = LifeOsItemState.Planned;

    public decimal MonthlyEstimate { get; set; }

    public decimal WeeklyReserve { get; set; }

    public string RuleSummary { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string ReviewGate { get; set; } = string.Empty;

    public string SafeToSpendImpact { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public bool Trusted { get; set; }

    public bool RequiredForConfidence { get; set; } = true;

    public bool AffectsSafeToSpend { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

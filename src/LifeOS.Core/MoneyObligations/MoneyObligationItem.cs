using LifeOS.Core.ItemState;

namespace LifeOS.Core.MoneyObligations;

public sealed class MoneyObligationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public MoneyObligationKind Kind { get; set; } = MoneyObligationKind.Bill;

    public MoneyObligationSourceKind SourceKind { get; set; } = MoneyObligationSourceKind.Manual;

    public LifeOsItemState State { get; set; } = LifeOsItemState.Open;

    public MoneyObligationPressureLevel PressureLevel { get; set; } = MoneyObligationPressureLevel.Medium;

    public MoneyObligationEvidenceState EvidenceState { get; set; } = MoneyObligationEvidenceState.SourceNoteOnly;

    public decimal AmountDue { get; set; }

    public decimal RemainingBalance { get; set; }

    public decimal MonthlyEstimate { get; set; }

    public DateTime? DueDate { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ScheduleLabel { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string ReviewGate { get; set; } = string.Empty;

    public string PressureSignal { get; set; } = string.Empty;

    public string SafeNextAction { get; set; } = string.Empty;

    public bool Trusted { get; set; }

    public bool AffectsSafeToSpend { get; set; } = true;

    public bool AffectsAgenda { get; set; } = true;

    public bool AffectsWeeklyCloseOut { get; set; } = true;

    public bool IsRequiredThisWeek { get; set; }

    public bool IsHiddenDeduction { get; set; }

    public bool IsPayLater { get; set; }

    public bool CountsAsSafeMoney { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

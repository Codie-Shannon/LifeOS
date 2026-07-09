namespace LifeOS.Core.PaymentCalendar;

public sealed class PaymentCalendarItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public PaymentCalendarItemKind Kind { get; set; } = PaymentCalendarItemKind.AgendaCommitment;

    public PaymentCalendarItemState State { get; set; } = PaymentCalendarItemState.Planned;

    public PaymentCalendarPressureLevel Pressure { get; set; } = PaymentCalendarPressureLevel.Medium;

    public PaymentCalendarTrustState TrustState { get; set; } = PaymentCalendarTrustState.SourceNoteOnly;

    public DateOnly? DueDate { get; set; }

    public string TimeText { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal Balance { get; set; }

    public decimal InstalmentAmount { get; set; }

    public decimal MonthlyAmount { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string ReviewRule { get; set; } = string.Empty;

    public string PressureImpact { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string OriginalModule { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool AffectsSafeToSpend { get; set; }

    public bool AffectsAgenda { get; set; }

    public bool IsPaymentDate { get; set; }

    public bool IsFixedCommitment { get; set; }

    public bool IsReviewWindow { get; set; }

    public bool ExpectedMoneyExcludedFromSafe { get; set; }

    public int SortOrder { get; set; }
}

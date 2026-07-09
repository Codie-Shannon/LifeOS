namespace LifeOS.Core.MoneyObligations;

public sealed class MoneyObligationSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string Rule { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public int OpenItems { get; set; }

    public int DueSoonItems { get; set; }

    public int DueTodayItems { get; set; }

    public int OverdueItems { get; set; }

    public int NeedsReviewItems { get; set; }

    public int UntrustedItems { get; set; }

    public int PayLaterItems { get; set; }

    public int HiddenDeductionItems { get; set; }

    public int RequiredThisWeekItems { get; set; }

    public int PaidOrClosedItems { get; set; }

    public decimal AmountDueToday { get; set; }

    public decimal RequiredThisWeekAmount { get; set; }

    public decimal OverdueAmount { get; set; }

    public decimal PayLaterBalance { get; set; }

    public decimal PayLaterDueThisWeek { get; set; }

    public decimal HiddenDeductionMonthlyEstimate { get; set; }

    public decimal SafeToSpendDrag { get; set; }

    public decimal ProjectedSafeToSpendAfterObligations { get; set; }

    public string PressureLabel { get; set; } = string.Empty;

    public bool RealBankSyncEnabled { get; set; }

    public bool RealPayLaterSyncEnabled { get; set; }

    public bool ManualReviewRequired { get; set; }

    public bool PaidRequiresEvidence { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> DuePressureItems { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> BillsAndSubscriptions { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> UpcomingPayments { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> PayLaterItemsList { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> HiddenDeductionItemsList { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> ReviewQueue { get; set; } = [];

    public IReadOnlyList<MoneyObligationItem> PaidOrClosedItemsList { get; set; } = [];
}

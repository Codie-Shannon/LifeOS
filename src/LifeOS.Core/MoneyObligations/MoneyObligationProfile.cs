namespace LifeOS.Core.MoneyObligations;

public sealed class MoneyObligationProfile
{
    public string Version { get; set; } = "v4.2";

    public string Mode { get; set; } = "Bills / Upcoming Payments / Pay Later";

    public string Rule { get; set; } = "Every bill, upcoming payment, BNPL installment, and hidden deduction is a stateful money obligation before it affects safe-to-spend.";

    public bool RealBankSyncEnabled { get; set; }

    public bool RealPayLaterSyncEnabled { get; set; }

    public bool ManualReviewRequired { get; set; } = true;

    public bool PaidRequiresEvidence { get; set; } = true;

    public decimal CurrentSafeToSpendBeforeObligations { get; set; }

    public List<MoneyObligationItem> Items { get; set; } = [];
}

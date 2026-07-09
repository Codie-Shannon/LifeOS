namespace LifeOS.Core.MoneyProfile;

public sealed class MoneyProfileSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string Rule { get; set; } = string.Empty;

    public decimal CurrentVisibleBalance { get; set; }

    public decimal ConfirmedCashOnHand { get; set; }

    public decimal WeeklyKnownObligations { get; set; }

    public decimal MinimumBuffer { get; set; }

    public decimal EmergencyHold { get; set; }

    public decimal HiddenDeductionMonthlyEstimate { get; set; }

    public decimal HiddenDeductionWeeklyReserve { get; set; }

    public decimal ExpectedMoneyTotal { get; set; }

    public decimal ExpectedExcludedFromSafe { get; set; }

    public decimal SafeToSpendBeforeHidden { get; set; }

    public decimal SafeToSpendAfterHidden { get; set; }

    public decimal SafeToSpendFinal { get; set; }

    public int HiddenDeductionCount { get; set; }

    public int RequiredReserveCount { get; set; }

    public int ReviewNeededCount { get; set; }

    public int UntrustedCount { get; set; }

    public int ExpectedMoneyCount { get; set; }

    public int UnsafeExpectedMoneyCount { get; set; }

    public string ConfidenceLabel { get; set; } = string.Empty;

    public MoneyProfileConfidenceLevel ConfidenceLevel { get; set; } = MoneyProfileConfidenceLevel.Unknown;

    public bool ExpectedMoneyCountsAsSafe { get; set; }

    public bool RealBankSyncEnabled { get; set; }

    public bool ManualReviewRequired { get; set; }

    public bool HiddenDeductionsReduceConfidence { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<string> CalculationLines { get; set; } = [];

    public IReadOnlyList<string> ConfidenceChecklist { get; set; } = [];

    public IReadOnlyList<MoneyProfileHiddenDeduction> HiddenDeductions { get; set; } = [];

    public IReadOnlyList<MoneyProfileHiddenDeduction> ReviewDeductions { get; set; } = [];

    public IReadOnlyList<MoneyProfileExpectedMoney> ExpectedMoneyItems { get; set; } = [];
}

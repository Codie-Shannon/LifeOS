namespace LifeOS.Core.MoneyProfile;

public sealed class MoneyProfilePlan
{
    public string Version { get; set; } = "v4.3";

    public string Mode { get; set; } = "Money Profile / Hidden Deductions / Safe-to-Spend";

    public string Rule { get; set; } = "Safe-to-spend is confirmed money minus visible obligations, hidden deductions, required reserves, and buffers. Expected money stays excluded until paid.";

    public decimal CurrentVisibleBalance { get; set; }

    public decimal ConfirmedCashOnHand { get; set; }

    public decimal WeeklyKnownObligations { get; set; }

    public decimal MinimumBuffer { get; set; }

    public decimal EmergencyHold { get; set; }

    public bool ExpectedMoneyCountsAsSafe { get; set; }

    public bool RealBankSyncEnabled { get; set; }

    public bool ManualReviewRequired { get; set; } = true;

    public bool HiddenDeductionsReduceConfidence { get; set; } = true;

    public List<MoneyProfileHiddenDeduction> HiddenDeductions { get; set; } = [];

    public List<MoneyProfileExpectedMoney> ExpectedMoneyItems { get; set; } = [];
}

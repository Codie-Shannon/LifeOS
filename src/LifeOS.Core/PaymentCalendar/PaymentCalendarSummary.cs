namespace LifeOS.Core.PaymentCalendar;

public sealed class PaymentCalendarSummary
{
    public string Version { get; set; } = "v4.4";

    public string Mode { get; set; } = "Local/manual";

    public string Rule { get; set; } = string.Empty;

    public string PressureLabel { get; set; } = "Low";

    public int TotalItems { get; set; }

    public int ThisWeekItems { get; set; }

    public int TodayItems { get; set; }

    public int DueTodayItems { get; set; }

    public int DueSoonItems { get; set; }

    public int OverdueItems { get; set; }

    public int PaymentDateItems { get; set; }

    public int AgendaCommitments { get; set; }

    public int FixedCommitments { get; set; }

    public int PayLaterDates { get; set; }

    public int ExpectedMoneyDates { get; set; }

    public int ReviewQueueItems { get; set; }

    public int UntrustedItems { get; set; }

    public int SafeToSpendAlerts { get; set; }

    public decimal AmountDueToday { get; set; }

    public decimal AmountDueThisWeek { get; set; }

    public decimal PayLaterDueThisWeek { get; set; }

    public decimal ExpectedMoneyExcluded { get; set; }

    public bool ManualReviewRequired { get; set; }

    public bool RealCalendarSyncEnabled { get; set; }

    public bool RealBankSyncEnabled { get; set; }

    public bool RealEmailSyncEnabled { get; set; }

    public bool ExpectedMoneyCountsAsSafe { get; set; }

    public List<string> Reasons { get; set; } = new();

    public List<string> DateRules { get; set; } = new();

    public List<PaymentCalendarDayGroup> TimelineGroups { get; set; } = new();

    public List<PaymentCalendarItem> TodayAndNextActions { get; set; } = new();

    public List<PaymentCalendarItem> PaymentItems { get; set; } = new();

    public List<PaymentCalendarItem> AgendaItems { get; set; } = new();

    public List<PaymentCalendarItem> PayLaterItems { get; set; } = new();

    public List<PaymentCalendarItem> ExpectedMoneyItems { get; set; } = new();

    public List<PaymentCalendarItem> ReviewQueue { get; set; } = new();

    public List<PaymentCalendarItem> SafeToSpendItems { get; set; } = new();
}

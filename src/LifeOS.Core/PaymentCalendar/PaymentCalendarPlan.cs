namespace LifeOS.Core.PaymentCalendar;

public sealed class PaymentCalendarPlan
{
    public string Version { get; set; } = "v4.4";

    public string Mode { get; set; } = "Local/manual";

    public string Rule { get; set; } = "Dates do not make money safe. Bills, BNPL, agenda commitments, payment dates, expected income, and review windows must be visible together before the week is trusted.";

    public DateOnly WindowStart { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly WindowEnd { get; set; } = DateOnly.FromDateTime(DateTime.Today).AddDays(14);

    public bool ManualReviewRequired { get; set; } = true;

    public bool RealCalendarSyncEnabled { get; set; }

    public bool RealBankSyncEnabled { get; set; }

    public bool RealEmailSyncEnabled { get; set; }

    public bool ExpectedMoneyCountsAsSafe { get; set; }

    public List<PaymentCalendarItem> Items { get; set; } = new();
}

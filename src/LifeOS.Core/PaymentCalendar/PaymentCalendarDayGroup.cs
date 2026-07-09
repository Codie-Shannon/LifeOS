namespace LifeOS.Core.PaymentCalendar;

public sealed class PaymentCalendarDayGroup
{
    public DateOnly Date { get; set; }

    public string Label { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public int PaymentCount { get; set; }

    public int AgendaCount { get; set; }

    public decimal AmountDue { get; set; }

    public bool HasReviewGate { get; set; }

    public List<PaymentCalendarItem> Items { get; set; } = new();
}

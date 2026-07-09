namespace LifeOS.Core.PaymentCalendar;

public enum PaymentCalendarItemState
{
    Planned,
    Open,
    Waiting,
    DueSoon,
    DueToday,
    Overdue,
    NeedsReview,
    Paid,
    Confirmed,
    Blocked,
    Closed,
    Ignored
}

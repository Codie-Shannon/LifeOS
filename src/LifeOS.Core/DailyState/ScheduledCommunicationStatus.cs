namespace LifeOS.Core.DailyState;

public enum ScheduledCommunicationStatus
{
    Planned = 0,
    Sent = 10,
    Cancelled = 20,
    WaitingAfterSend = 30,
    Archived = 999
}

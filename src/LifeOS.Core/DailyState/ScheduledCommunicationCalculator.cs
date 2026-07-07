namespace LifeOS.Core.DailyState;

public static class ScheduledCommunicationCalculator
{
    public static ScheduledCommunicationSummary Calculate(IEnumerable<ScheduledCommunicationItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();
        var plannedToday = itemList
            .Where(item => item.IsOpen && item.Status == ScheduledCommunicationStatus.Planned && DateOnly.FromDateTime(item.ScheduledAt) == today)
            .OrderBy(item => item.ScheduledAt)
            .ToList();

        var waitingAfterSend = itemList
            .Where(item => item.IsOpen && item.Status == ScheduledCommunicationStatus.WaitingAfterSend)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Recipient)
            .ToList();

        var reasons = new List<string>();
        if (plannedToday.Count > 0) reasons.Add($"{plannedToday.Count} scheduled communication item(s) planned today.");
        if (waitingAfterSend.Count > 0) reasons.Add($"{waitingAfterSend.Count} sent communication item(s) are in waiting-after-send state.");
        if (reasons.Count == 0) reasons.Add("No scheduled communication pressure today.");

        return new ScheduledCommunicationSummary
        {
            PlannedTodayCount = plannedToday.Count,
            WaitingAfterSendCount = waitingAfterSend.Count,
            PlannedToday = plannedToday,
            WaitingAfterSend = waitingAfterSend,
            Reasons = reasons
        };
    }
}

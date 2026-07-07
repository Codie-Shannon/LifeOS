namespace LifeOS.Core.DailyState;

public static class DailyStateCalculator
{
    public static DailyStateSummary Calculate(IEnumerable<DailyStateItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();
        var todayItems = itemList
            .Where(item => item.Date == today && item.IsOpen && item.ShowInToday)
            .OrderBy(item => item.Type)
            .ThenBy(item => item.Title)
            .ToList();

        var hiddenItems = itemList
            .Where(item => item.IsOpen && (!item.ShowInToday || item.Status is DailyStateStatus.PassiveWaiting or DailyStateStatus.DoNotChaseYet))
            .OrderByDescending(item => item.Date)
            .ThenBy(item => item.Title)
            .ToList();

        var reasons = new List<string>();

        if (todayItems.Count > 0) reasons.Add($"{todayItems.Count} daily state item(s) are visible for today.");
        if (hiddenItems.Count > 0) reasons.Add($"{hiddenItems.Count} item(s) are parked/passive and hidden from Today.");
        if (reasons.Count == 0) reasons.Add("No manual daily state captured yet.");

        return new DailyStateSummary
        {
            TodayOpenCount = todayItems.Count,
            DoneTodayCount = itemList.Count(item => item.Date == today && item.Status == DailyStateStatus.Done),
            PassiveWaitingCount = itemList.Count(item => item.Status == DailyStateStatus.PassiveWaiting),
            DoNotChaseCount = itemList.Count(item => item.Status == DailyStateStatus.DoNotChaseYet),
            ScheduledMessageCount = itemList.Count(item => item.Date == today && item.Type == DailyStateType.ScheduledMessage && item.IsOpen),
            TodayItems = todayItems,
            HiddenItems = hiddenItems,
            Reasons = reasons
        };
    }
}

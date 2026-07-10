namespace LifeOS.Core.WeeklyCloseOut;

public static class WeeklyCloseOutCalculator
{
    public static WeeklyCloseOutSummary Calculate(IEnumerable<WeeklyCloseOutEntry> entries, DateOnly today)
    {
        var entryList = entries.ToList();
        var weekStart = LifeOSWeek.GetMondayStart(today);
        var latestWeekStart = entryList.Count == 0 ? (DateOnly?)null : entryList.Max(entry => entry.WeekStart);
        var entriesThisWeek = entryList.Count(entry => entry.WeekStart == weekStart);
        var waitingOnCount = entryList.Count(entry => !string.IsNullOrWhiteSpace(entry.StillWaitingOn));

        var reasons = new List<string>();

        if (entriesThisWeek == 0) reasons.Add("No close-out entry exists for the current week yet.");
        else reasons.Add("Current week has a close-out entry.");

        if (waitingOnCount > 0) reasons.Add($"{waitingOnCount} close-out entr(y/ies) include waiting-on pressure.");
        if (entryList.Count == 0) reasons.Add("Weekly close-out has not been started yet.");

        return new WeeklyCloseOutSummary
        {
            TotalEntries = entryList.Count,
            EntriesThisWeek = entriesThisWeek,
            LatestWeekStart = latestWeekStart,
            HasCurrentWeekCloseOut = entriesThisWeek > 0,
            WaitingOnCount = waitingOnCount,
            Reasons = reasons
        };
    }
}

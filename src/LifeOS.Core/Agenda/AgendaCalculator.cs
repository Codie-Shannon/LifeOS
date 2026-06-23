namespace LifeOS.Core.Agenda;

public static class AgendaCalculator
{
    public static AgendaSummary Calculate(IEnumerable<AgendaItem> items, DateOnly today)
    {
        var itemList = items.ToList();
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(6);

        var openItems = itemList
            .Where(item => item.Status is not AgendaItemStatus.Completed and not AgendaItemStatus.Cancelled)
            .ToList();

        var dueTodayCount = openItems.Count(item => item.DueDate == today);
        var overdueCount = openItems.Count(item => item.DueDate.HasValue && item.DueDate.Value < today);
        var thisWeekCount = openItems.Count(item => item.DueDate.HasValue && item.DueDate.Value >= weekStart && item.DueDate.Value <= weekEnd);
        var highPressureCount = openItems.Count(item => item.PressureLevel is AgendaPressureLevel.High or AgendaPressureLevel.Critical);
        var completedCount = itemList.Count(item => item.Status == AgendaItemStatus.Completed);

        var reasons = new List<string>();

        if (overdueCount > 0) reasons.Add($"{overdueCount} agenda item(s) are overdue.");
        if (dueTodayCount > 0) reasons.Add($"{dueTodayCount} agenda item(s) are due today.");
        if (highPressureCount > 0) reasons.Add($"{highPressureCount} open agenda item(s) are marked high or critical pressure.");
        if (thisWeekCount > 0) reasons.Add($"{thisWeekCount} open agenda item(s) are due this week.");
        if (reasons.Count == 0) reasons.Add("No urgent agenda pressure detected.");

        return new AgendaSummary
        {
            TotalOpen = openItems.Count,
            DueTodayCount = dueTodayCount,
            OverdueCount = overdueCount,
            ThisWeekCount = thisWeekCount,
            HighPressureCount = highPressureCount,
            CompletedCount = completedCount,
            Reasons = reasons
        };
    }
}

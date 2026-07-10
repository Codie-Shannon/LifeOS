namespace LifeOS.Core.PayLater;

public static class PayLaterCalculator
{
    public static PayLaterSummary Calculate(IEnumerable<PayLaterItem> items, DateOnly today)
    {
        var weekStart = LifeOSWeek.GetMondayStart(today);
        var weekEnd = weekStart.AddDays(6);

        var openItems = items
            .Where(item => item.Status is not PayLaterStatus.Paid and not PayLaterStatus.Cancelled)
            .ToList();

        var dueThisWeekItems = openItems
            .Where(item => item.DueDate.HasValue && item.DueDate.Value >= weekStart && item.DueDate.Value <= weekEnd)
            .ToList();

        var overdueItems = openItems
            .Where(item => item.DueDate.HasValue && item.DueDate.Value < today)
            .ToList();

        var highPressureCount = openItems.Count(item => item.PressureLevel is PayLaterPressureLevel.High or PayLaterPressureLevel.Critical);

        var reasons = new List<string>();

        if (overdueItems.Count > 0) reasons.Add($"{overdueItems.Count} pay-later item(s) are overdue.");
        if (dueThisWeekItems.Count > 0) reasons.Add($"{dueThisWeekItems.Count} pay-later item(s) are due this week.");
        if (highPressureCount > 0) reasons.Add($"{highPressureCount} pay-later item(s) are marked high or critical pressure.");
        if (openItems.Count > 0) reasons.Add($"{openItems.Sum(item => item.Amount):C} total open pay-later pressure is visible.");
        if (reasons.Count == 0) reasons.Add("No open pay-later pressure detected.");

        return new PayLaterSummary
        {
            TotalOpen = openItems.Count,
            TotalAmountOpen = openItems.Sum(item => item.Amount),
            DueThisWeekAmount = dueThisWeekItems.Sum(item => item.Amount),
            OverdueAmount = overdueItems.Sum(item => item.Amount),
            DueThisWeekCount = dueThisWeekItems.Count,
            OverdueCount = overdueItems.Count,
            HighPressureCount = highPressureCount,
            Reasons = reasons
        };
    }
}

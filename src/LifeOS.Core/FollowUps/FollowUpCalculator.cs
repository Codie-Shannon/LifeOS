namespace LifeOS.Core.FollowUps;

public static class FollowUpCalculator
{
    public static FollowUpSummary Calculate(IEnumerable<FollowUpItem> followUps, DateOnly today)
    {
        var openItems = followUps
            .Where(item => item.Status is not FollowUpStatus.Completed)
            .ToList();

        var waitingCount = openItems.Count(item => item.Status == FollowUpStatus.Waiting);
        var needsActionCount = openItems.Count(item => item.Status == FollowUpStatus.NeedsAction);

        var overdueCount = openItems.Count(item =>
            item.FollowUpDate.HasValue &&
            item.FollowUpDate.Value < today);

        var dueTodayCount = openItems.Count(item =>
            item.FollowUpDate.HasValue &&
            item.FollowUpDate.Value == today);

        var moneyLinkedCount = openItems.Count(item => item.IsMoneyLinked);

        var reasons = new List<string>();

        if (needsActionCount > 0)
        {
            reasons.Add($"{needsActionCount} follow-up item(s) need action.");
        }

        if (overdueCount > 0)
        {
            reasons.Add($"{overdueCount} follow-up item(s) are overdue.");
        }

        if (dueTodayCount > 0)
        {
            reasons.Add($"{dueTodayCount} follow-up item(s) are due today.");
        }

        if (moneyLinkedCount > 0)
        {
            reasons.Add($"{moneyLinkedCount} open follow-up item(s) are linked to money or paid work.");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("No urgent follow-up pressure detected.");
        }

        return new FollowUpSummary
        {
            TotalOpen = openItems.Count,
            WaitingCount = waitingCount,
            NeedsActionCount = needsActionCount,
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount,
            MoneyLinkedCount = moneyLinkedCount,
            Reasons = reasons
        };
    }
}
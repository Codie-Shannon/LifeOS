namespace LifeOS.Core.WorkPipeline;

public static class WorkPipelineCalculator
{
    public static WorkPipelineSummary Calculate(IEnumerable<WorkPipelineItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();
        var openItems = itemList.Where(item => item.IsOpen).ToList();

        var dueSoonLimit = today.AddDays(7);

        var followUpStates = openItems.ToDictionary(item => item.Id, item => item.GetFollowUpState(today));

        var dueFollowUps = openItems
            .Where(item => item.FollowUpDate.HasValue && item.FollowUpDate.Value <= dueSoonLimit)
            .OrderBy(item => item.FollowUpDate)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var blockedWork = openItems
            .Where(item => item.IsBlocked)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var waitingWork = openItems
            .Where(item => item.IsWaiting)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var moneyWork = openItems
            .Where(item => item.IsMoneyRelated)
            .OrderByDescending(item => item.PaymentExpected)
            .ThenByDescending(item => item.NeedsInvoice)
            .ThenByDescending(item => item.NeedsTimesheet)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var opportunityWork = openItems
            .Where(item => item.IsOpportunity)
            .OrderByDescending(item => item.OpportunityTemperature)
            .ThenByDescending(item => item.LikelihoodPercent)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var priorityItems = openItems
            .Where(item =>
                item.Priority is WorkPipelinePriority.High or WorkPipelinePriority.Critical ||
                item.IsBlocked ||
                item.NeedsTimesheet ||
                item.NeedsInvoice ||
                item.PaymentExpected ||
                (item.FollowUpDate.HasValue && item.FollowUpDate.Value <= dueSoonLimit))
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var overdueCount = followUpStates.Count(pair => pair.Value == WorkPipelineFollowUpState.Overdue);
        var dueTodayCount = followUpStates.Count(pair => pair.Value == WorkPipelineFollowUpState.DueToday);
        var dueSoonCount = followUpStates.Count(pair => pair.Value == WorkPipelineFollowUpState.DueSoon);
        var scheduledCount = followUpStates.Count(pair => pair.Value == WorkPipelineFollowUpState.Scheduled);
        var keepWarmDueCount = followUpStates.Count(pair => pair.Value == WorkPipelineFollowUpState.KeepWarm);
        var missingDateCount = openItems.Count(item => item.Status is WorkPipelineStatus.Active or WorkPipelineStatus.Waiting && !item.FollowUpDate.HasValue && !item.KeepWarmDate.HasValue);

        var commandCentreSignals = BuildCommandCentreSignals(overdueCount, dueTodayCount, blockedWork.Count, waitingWork.Count, moneyWork.Count, openItems.Count, moneyWork.Sum(item => item.ExpectedValue ?? 0m));

        var reasons = BuildReasons(openItems, overdueCount, dueTodayCount, dueSoonCount, blockedWork.Count, waitingWork.Count, moneyWork.Count);

        if (keepWarmDueCount > 0)
        {
            reasons.Add($"{keepWarmDueCount} parked/warm pipeline item(s) are close enough to review.");
        }

        if (missingDateCount > 0)
        {
            reasons.Add($"{missingDateCount} active/waiting pipeline item(s) have no follow-up or keep-warm date.");
        }

        return new WorkPipelineSummary
        {
            TotalItems = itemList.Count,
            OpenItems = openItems.Count,
            ActiveItems = openItems.Count(item => item.Status == WorkPipelineStatus.Active),
            WaitingItems = waitingWork.Count,
            BlockedItems = blockedWork.Count,
            WarmItems = openItems.Count(item => item.Status == WorkPipelineStatus.Warm),
            ParkedItems = openItems.Count(item => item.Status == WorkPipelineStatus.Parked),
            OpportunityItems = opportunityWork.Count,
            HotOpportunityItems = opportunityWork.Count(item => item.OpportunityTemperature is WorkPipelineOpportunityTemperature.Hot or WorkPipelineOpportunityTemperature.Active),
            ArchivedItems = itemList.Count(item => item.IsArchived || item.Status == WorkPipelineStatus.Archived),
            FollowUpsOverdue = overdueCount,
            FollowUpsDueToday = dueTodayCount,
            FollowUpsDueSoon = dueSoonCount,
            FollowUpsScheduled = scheduledCount,
            FollowUpsMissingDate = missingDateCount,
            KeepWarmDue = keepWarmDueCount,
            BillableItems = openItems.Count(item => item.IsBillable),
            TimesheetsNeeded = openItems.Count(item => item.NeedsTimesheet),
            InvoicesNeeded = openItems.Count(item => item.NeedsInvoice),
            PaymentsExpected = openItems.Count(item => item.PaymentExpected),
            ExpectedValueTotal = openItems.Where(item => item.ExpectedValue.HasValue).Sum(item => item.ExpectedValue!.Value),
            PriorityItems = priorityItems,
            DueFollowUps = dueFollowUps,
            BlockedWork = blockedWork,
            WaitingWork = waitingWork,
            MoneyWork = moneyWork,
            OpportunityWork = opportunityWork,
            CommandCentreSignals = commandCentreSignals,
            Reasons = reasons
        };
    }

    public static WorkPipelineWaitingView BuildWaitingView(IEnumerable<WorkPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var waitingItems = items
            .Where(item => item.IsOpen && item.IsWaiting)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        return new WorkPipelineWaitingView
        {
            WaitingOnMe = waitingItems
                .Where(item => item.WaitingOn.Contains("me", StringComparison.OrdinalIgnoreCase) || item.WaitingOn.Contains("Codie", StringComparison.OrdinalIgnoreCase))
                .ToList(),
            WaitingOnOthers = waitingItems
                .Where(item => !string.IsNullOrWhiteSpace(item.WaitingOn) && !item.WaitingOn.Contains("me", StringComparison.OrdinalIgnoreCase) && !item.WaitingOn.Contains("Codie", StringComparison.OrdinalIgnoreCase))
                .ToList(),
            WaitingWithoutOwner = waitingItems
                .Where(item => string.IsNullOrWhiteSpace(item.WaitingOn))
                .ToList()
        };
    }

    public static IReadOnlyList<WorkPipelineItem> GetVisibleItems(IEnumerable<WorkPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Where(item => item.IsOpen)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Stage)
            .ThenBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();
    }

    public static IReadOnlyList<WorkPipelineItem> GetArchivedItems(IEnumerable<WorkPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Where(item => item.IsArchived || item.Status == WorkPipelineStatus.Archived)
            .OrderByDescending(item => item.UpdatedAt)
            .ThenBy(item => item.Title)
            .ToList();
    }

    private static IReadOnlyList<WorkPipelineCommandCentreSignal> BuildCommandCentreSignals(
        int overdueCount,
        int dueTodayCount,
        int blockedCount,
        int waitingCount,
        int moneyCount,
        int openCount,
        decimal expectedValue)
    {
        var signals = new List<WorkPipelineCommandCentreSignal>();

        signals.Add(new WorkPipelineCommandCentreSignal
        {
            Label = "Pipeline open",
            Value = openCount.ToString(),
            Detail = "Open work, warm leads, proof work, or parked items.",
            Priority = openCount > 0 ? WorkPipelinePriority.Normal : WorkPipelinePriority.Low
        });

        if (blockedCount > 0)
        {
            signals.Add(new WorkPipelineCommandCentreSignal
            {
                Label = "Blocked work",
                Value = blockedCount.ToString(),
                Detail = "Work exists but cannot move until something changes.",
                Priority = WorkPipelinePriority.High
            });
        }

        if (overdueCount + dueTodayCount > 0)
        {
            signals.Add(new WorkPipelineCommandCentreSignal
            {
                Label = "Follow-ups now",
                Value = (overdueCount + dueTodayCount).ToString(),
                Detail = "Pipeline follow-ups are overdue or due today.",
                Priority = WorkPipelinePriority.Critical
            });
        }

        if (waitingCount > 0)
        {
            signals.Add(new WorkPipelineCommandCentreSignal
            {
                Label = "Waiting-on",
                Value = waitingCount.ToString(),
                Detail = "Client/person/access/payment waiting states are visible.",
                Priority = WorkPipelinePriority.Normal
            });
        }

        if (moneyCount > 0 || expectedValue > 0m)
        {
            signals.Add(new WorkPipelineCommandCentreSignal
            {
                Label = "Expected work value",
                Value = expectedValue.ToString("C0"),
                Detail = "Expected money is visible but is not safe money until paid.",
                Priority = WorkPipelinePriority.High
            });
        }

        return signals;
    }

    private static List<string> BuildReasons(
        IReadOnlyCollection<WorkPipelineItem> openItems,
        int overdueCount,
        int dueTodayCount,
        int dueSoonCount,
        int blockedCount,
        int waitingCount,
        int moneyCount)
    {
        var reasons = new List<string>();

        if (openItems.Count == 0)
        {
            reasons.Add("No open pipeline items. Add active work, warm leads, or parked ideas when they matter.");
            return reasons;
        }

        if (overdueCount > 0) reasons.Add($"{overdueCount} pipeline follow-up item(s) are overdue.");
        if (dueTodayCount > 0) reasons.Add($"{dueTodayCount} pipeline follow-up item(s) are due today.");
        if (dueSoonCount > 0) reasons.Add($"{dueSoonCount} pipeline follow-up item(s) are due in the next 7 days.");
        if (blockedCount > 0) reasons.Add($"{blockedCount} pipeline item(s) are blocked.");
        if (waitingCount > 0) reasons.Add($"{waitingCount} pipeline item(s) are waiting on someone or something.");
        if (moneyCount > 0) reasons.Add($"{moneyCount} pipeline item(s) are linked to billable work, invoices, timesheets, payment, or expected value.");

        if (reasons.Count == 0)
        {
            reasons.Add("No urgent pipeline pressure detected. Keep active work moving and parked work out of today's way.");
        }

        return reasons;
    }
}

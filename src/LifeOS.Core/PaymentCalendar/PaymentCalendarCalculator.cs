namespace LifeOS.Core.PaymentCalendar;

public static class PaymentCalendarCalculator
{
    public static PaymentCalendarSummary Calculate(PaymentCalendarPlan plan, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var items = (plan.Items ?? new List<PaymentCalendarItem>())
            .Where(item => item.State is not PaymentCalendarItemState.Ignored and not PaymentCalendarItemState.Closed)
            .ToList();

        var weekEnd = today.AddDays(7);

        var datedItems = items
            .Where(item => item.DueDate.HasValue)
            .OrderBy(item => item.DueDate)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .ToList();

        var thisWeek = datedItems
            .Where(item => item.DueDate >= today && item.DueDate <= weekEnd)
            .ToList();

        var dueToday = datedItems
            .Where(item => item.DueDate == today)
            .ToList();

        var dueSoon = datedItems
            .Where(item => item.DueDate > today && item.DueDate <= weekEnd)
            .ToList();

        var overdue = datedItems
            .Where(item => item.DueDate < today && item.State is not PaymentCalendarItemState.Paid and not PaymentCalendarItemState.Confirmed)
            .ToList();

        var reviewQueue = items
            .Where(item =>
                item.State == PaymentCalendarItemState.NeedsReview ||
                item.TrustState is PaymentCalendarTrustState.Untrusted or PaymentCalendarTrustState.EvidenceNeeded ||
                item.IsReviewWindow ||
                string.IsNullOrWhiteSpace(item.EvidenceSummary) && item.AffectsSafeToSpend)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Amount)
            .ThenBy(item => item.Title)
            .ToList();

        var paymentItems = items
            .Where(IsPaymentItem)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var agendaItems = items
            .Where(item => item.AffectsAgenda || item.Kind is PaymentCalendarItemKind.AgendaCommitment or PaymentCalendarItemKind.WorkSession or PaymentCalendarItemKind.FollowUp or PaymentCalendarItemKind.WeeklyCloseOut)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var payLaterItems = items
            .Where(item => item.Kind == PaymentCalendarItemKind.PayLaterInstallment)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var expectedMoneyItems = items
            .Where(item => item.Kind == PaymentCalendarItemKind.ExpectedIncome || item.ExpectedMoneyExcludedFromSafe)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var safeToSpendItems = items
            .Where(item => item.AffectsSafeToSpend)
            .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Amount)
            .ThenBy(item => item.Title)
            .ToList();

        var timelineGroups = datedItems
            .GroupBy(item => item.DueDate!.Value)
            .OrderBy(group => group.Key)
            .Select(group => new PaymentCalendarDayGroup
            {
                Date = group.Key,
                Label = FormatDateLabel(group.Key, today),
                ItemCount = group.Count(),
                PaymentCount = group.Count(IsPaymentItem),
                AgendaCount = group.Count(item => item.AffectsAgenda),
                AmountDue = group.Where(item => item.AffectsSafeToSpend && item.Kind != PaymentCalendarItemKind.ExpectedIncome).Sum(item => item.Amount),
                HasReviewGate = group.Any(item => reviewQueue.Contains(item)),
                Items = group
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.Title)
                    .ToList()
            })
            .ToList();

        var amountDueToday = dueToday
            .Where(item => item.AffectsSafeToSpend && item.Kind != PaymentCalendarItemKind.ExpectedIncome)
            .Sum(item => item.Amount);

        var amountDueThisWeek = thisWeek
            .Where(item => item.AffectsSafeToSpend && item.Kind != PaymentCalendarItemKind.ExpectedIncome)
            .Sum(item => item.Amount);

        var payLaterDueThisWeek = thisWeek
            .Where(item => item.Kind == PaymentCalendarItemKind.PayLaterInstallment)
            .Sum(item => item.Amount);

        var expectedExcluded = expectedMoneyItems
            .Where(item => item.ExpectedMoneyExcludedFromSafe)
            .Sum(item => item.Amount);

        var pressureLabel = "Low";

        if (overdue.Count > 0 || amountDueToday > 0 || reviewQueue.Count >= 3)
        {
            pressureLabel = "Danger";
        }
        else if (amountDueThisWeek >= 100 || dueToday.Count > 0 || reviewQueue.Count > 0)
        {
            pressureLabel = "High";
        }
        else if (dueSoon.Count > 0 || payLaterDueThisWeek > 0)
        {
            pressureLabel = "Medium";
        }

        var reasons = new List<string>
        {
            plan.Rule,
            "Payment dates and agenda dates sit in one lane so the week shows both money pressure and time pressure.",
            "Due dates do not mark bills paid; payment evidence is still required before a money item becomes trusted.",
            "Expected income is shown by date but remains excluded from safe money until paid/cleared with evidence.",
            "BNPL/Pay Later instalments must show due instalment, remaining balance, and weekly load.",
            "Calendar imports remain untrusted until reviewed; v4.4 does not connect live calendars or banks."
        };

        if (dueToday.Count > 0)
        {
            reasons.Add($"{dueToday.Count} item(s) land today.");
        }

        if (amountDueThisWeek > 0)
        {
            reasons.Add($"{FormatMoney(amountDueThisWeek)} affects this week's safe-to-spend lane.");
        }

        if (reviewQueue.Count > 0)
        {
            reasons.Add($"{reviewQueue.Count} item(s) still need review/evidence before trusted state.");
        }

        var dateRules = new List<string>
        {
            plan.RealCalendarSyncEnabled ? "Real calendar sync is ON. This should not be true before v5." : "Real calendar sync is OFF.",
            plan.RealBankSyncEnabled ? "Real bank sync is ON. This should not be true before v5." : "Real bank sync is OFF.",
            plan.RealEmailSyncEnabled ? "Real email sync is ON. This should not be true before v5." : "Real email sync is OFF.",
            plan.ExpectedMoneyCountsAsSafe ? "Expected money is being counted as safe. Review this." : "Expected money is excluded from safe money.",
            plan.ManualReviewRequired ? "Manual review is required before imported or uncertain dates become trusted." : "Manual review is not required. Review this.",
            reviewQueue.Count == 0 ? "No date review blockers." : $"{reviewQueue.Count} date/review blocker(s)."
        };

        return new PaymentCalendarSummary
        {
            Version = plan.Version,
            Mode = plan.Mode,
            Rule = plan.Rule,
            PressureLabel = pressureLabel,
            TotalItems = items.Count,
            ThisWeekItems = thisWeek.Count,
            TodayItems = dueToday.Count,
            DueTodayItems = dueToday.Count(item => item.State == PaymentCalendarItemState.DueToday || item.DueDate == today),
            DueSoonItems = dueSoon.Count,
            OverdueItems = overdue.Count,
            PaymentDateItems = paymentItems.Count,
            AgendaCommitments = agendaItems.Count,
            FixedCommitments = agendaItems.Count(item => item.IsFixedCommitment),
            PayLaterDates = payLaterItems.Count,
            ExpectedMoneyDates = expectedMoneyItems.Count,
            ReviewQueueItems = reviewQueue.Count,
            UntrustedItems = items.Count(item => item.TrustState is PaymentCalendarTrustState.Untrusted or PaymentCalendarTrustState.EvidenceNeeded),
            SafeToSpendAlerts = safeToSpendItems.Count(item => item.Pressure is PaymentCalendarPressureLevel.High or PaymentCalendarPressureLevel.Critical),
            AmountDueToday = amountDueToday,
            AmountDueThisWeek = amountDueThisWeek,
            PayLaterDueThisWeek = payLaterDueThisWeek,
            ExpectedMoneyExcluded = expectedExcluded,
            ManualReviewRequired = plan.ManualReviewRequired,
            RealCalendarSyncEnabled = plan.RealCalendarSyncEnabled,
            RealBankSyncEnabled = plan.RealBankSyncEnabled,
            RealEmailSyncEnabled = plan.RealEmailSyncEnabled,
            ExpectedMoneyCountsAsSafe = plan.ExpectedMoneyCountsAsSafe,
            Reasons = reasons,
            DateRules = dateRules,
            TimelineGroups = timelineGroups,
            TodayAndNextActions = datedItems
                .Where(item => item.DueDate <= today.AddDays(2) || item.Pressure is PaymentCalendarPressureLevel.High or PaymentCalendarPressureLevel.Critical)
                .OrderBy(item => item.DueDate ?? DateOnly.MaxValue)
                .ThenByDescending(item => item.Pressure)
                .ThenBy(item => item.Title)
                .ToList(),
            PaymentItems = paymentItems,
            AgendaItems = agendaItems,
            PayLaterItems = payLaterItems,
            ExpectedMoneyItems = expectedMoneyItems,
            ReviewQueue = reviewQueue,
            SafeToSpendItems = safeToSpendItems
        };
    }

    public static string FormatKind(PaymentCalendarItemKind kind)
    {
        return kind switch
        {
            PaymentCalendarItemKind.AgendaCommitment => "Agenda commitment",
            PaymentCalendarItemKind.Bill => "Bill",
            PaymentCalendarItemKind.Subscription => "Subscription",
            PaymentCalendarItemKind.PayLaterInstallment => "Pay Later / BNPL",
            PaymentCalendarItemKind.ExpectedIncome => "Expected income",
            PaymentCalendarItemKind.WorkSession => "Work session",
            PaymentCalendarItemKind.FollowUp => "Follow-up",
            PaymentCalendarItemKind.HiddenDeduction => "Hidden deduction",
            PaymentCalendarItemKind.ReceiptReview => "Receipt review",
            PaymentCalendarItemKind.ComplianceRenewal => "Compliance renewal",
            PaymentCalendarItemKind.WeeklyCloseOut => "Weekly close-out",
            PaymentCalendarItemKind.ManualCashflow => "Manual cashflow",
            _ => kind.ToString()
        };
    }

    public static string FormatState(PaymentCalendarItemState state)
    {
        return state switch
        {
            PaymentCalendarItemState.DueSoon => "Due soon",
            PaymentCalendarItemState.DueToday => "Due today",
            PaymentCalendarItemState.NeedsReview => "Needs review",
            _ => state.ToString()
        };
    }

    public static string FormatTrust(PaymentCalendarTrustState trustState)
    {
        return trustState switch
        {
            PaymentCalendarTrustState.SourceNoteOnly => "Source note only",
            PaymentCalendarTrustState.EvidenceNeeded => "Evidence needed",
            _ => trustState.ToString()
        };
    }

    public static string FormatPressure(PaymentCalendarPressureLevel pressure)
    {
        return pressure.ToString();
    }

    private static bool IsPaymentItem(PaymentCalendarItem item)
    {
        return item.IsPaymentDate ||
               item.AffectsSafeToSpend ||
               item.Kind is PaymentCalendarItemKind.Bill or
                   PaymentCalendarItemKind.Subscription or
                   PaymentCalendarItemKind.PayLaterInstallment or
                   PaymentCalendarItemKind.ExpectedIncome or
                   PaymentCalendarItemKind.HiddenDeduction or
                   PaymentCalendarItemKind.ComplianceRenewal or
                   PaymentCalendarItemKind.ManualCashflow;
    }

    private static string FormatDateLabel(DateOnly date, DateOnly today)
    {
        if (date == today)
        {
            return "Today";
        }

        if (date == today.AddDays(1))
        {
            return "Tomorrow";
        }

        if (date < today)
        {
            return "Overdue";
        }

        if (date <= today.AddDays(7))
        {
            return "This week";
        }

        return "Later";
    }

    private static string FormatMoney(decimal amount)
    {
        return amount.ToString("C");
    }
}

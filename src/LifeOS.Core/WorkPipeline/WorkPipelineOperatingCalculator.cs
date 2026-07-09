namespace LifeOS.Core.WorkPipeline;

public static class WorkPipelineOperatingCalculator
{
    public static WorkPipelineOperatingSummary Calculate(IEnumerable<WorkPipelineItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();
        var openItems = itemList.Where(item => item.IsOpen).ToList();
        var waitingView = WorkPipelineCalculator.BuildWaitingView(openItems);
        var dueSoonLimit = today.AddDays(7);

        var followUpsNow = openItems
            .Where(item => item.GetFollowUpState(today) is WorkPipelineFollowUpState.Overdue or WorkPipelineFollowUpState.DueToday)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var followUpsDueSoon = openItems
            .Where(item => item.GetFollowUpState(today) == WorkPipelineFollowUpState.DueSoon)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var activeWork = openItems
            .Where(item => item.Status == WorkPipelineStatus.Active || item.Stage is WorkPipelineStage.PaidWorkActive or WorkPipelineStage.ProofInProgress or WorkPipelineStage.MaterialsReceived)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var blockedWork = openItems
            .Where(item => item.IsBlocked)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var invoiceReadiness = openItems
            .Where(item => item.NeedsTimesheet || item.NeedsInvoice || item.Stage is WorkPipelineStage.TimesheetNeeded or WorkPipelineStage.InvoiceNeeded)
            .OrderByDescending(item => item.NeedsInvoice)
            .ThenByDescending(item => item.NeedsTimesheet)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var paymentExpected = openItems
            .Where(item => item.PaymentExpected || item.Stage == WorkPipelineStage.PaymentExpected)
            .OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.ExpectedValue ?? 0m)
            .ThenBy(item => item.Title)
            .ToList();

        var proofNeeded = openItems
            .Where(item =>
                item.Stage is WorkPipelineStage.ProofInProgress or WorkPipelineStage.SentForReview or WorkPipelineStage.ApprovedHappy ||
                item.NeedsInvoice ||
                item.NeedsTimesheet)
            .Where(item => string.IsNullOrWhiteSpace(item.LinkedProofNotes) || item.LinkedProofNotes.Contains("missing", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var warmOrParked = openItems
            .Where(item => item.Status is WorkPipelineStatus.Warm or WorkPipelineStatus.Parked || item.Stage == WorkPipelineStage.KeepWarm)
            .OrderBy(item => item.KeepWarmDate ?? item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenByDescending(item => item.OpportunityTemperature)
            .ThenBy(item => item.Title)
            .ToList();

        var missingFollowUpDate = openItems
            .Where(item =>
                item.Status is WorkPipelineStatus.Active or WorkPipelineStatus.Waiting or WorkPipelineStatus.Blocked &&
                !item.FollowUpDate.HasValue &&
                !item.KeepWarmDate.HasValue)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Title)
            .ToList();

        var needsReview = openItems
            .Where(item =>
                item.IsBlocked ||
                item.NeedsInvoice ||
                item.NeedsTimesheet ||
                item.PaymentExpected ||
                string.IsNullOrWhiteSpace(item.NextAction) ||
                (!item.FollowUpDate.HasValue && item.Status is WorkPipelineStatus.Active or WorkPipelineStatus.Waiting or WorkPipelineStatus.Blocked))
            .DistinctBy(item => item.Id)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var todayTriage = openItems
            .Where(item =>
                followUpsNow.Contains(item) ||
                item.IsBlocked ||
                item.NeedsInvoice ||
                item.NeedsTimesheet ||
                item.PaymentExpected ||
                item.Priority == WorkPipelinePriority.Critical)
            .DistinctBy(item => item.Id)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.Title)
            .Take(8)
            .ToList();

        var expectedValue = openItems
            .Where(item => item.ExpectedValue.HasValue)
            .Sum(item => item.ExpectedValue!.Value);

        var expectedExcluded = openItems
            .Where(item => item.PaymentExpected || item.ExpectedValue.HasValue)
            .Sum(item => item.ExpectedValue ?? 0m);

        var integrityWarnings = BuildIntegrityWarnings(openItems, missingFollowUpDate, proofNeeded).ToList();
        var reviewCount = needsReview.Count;
        var pressureLabel = BuildPressureLabel(blockedWork.Count, followUpsNow.Count, invoiceReadiness.Count, paymentExpected.Count, reviewCount);
        var signals = BuildSignals(openItems.Count, blockedWork.Count, followUpsNow.Count, waitingView.WaitingOnMe.Count, waitingView.WaitingOnOthers.Count, invoiceReadiness.Count, paymentExpected.Count, expectedExcluded, reviewCount).ToList();
        var reasons = BuildReasons(blockedWork.Count, followUpsNow.Count, followUpsDueSoon.Count, invoiceReadiness.Count, paymentExpected.Count, proofNeeded.Count, waitingView.WaitingOnMe.Count, waitingView.WaitingOnOthers.Count, expectedExcluded, integrityWarnings.Count).ToList();

        return new WorkPipelineOperatingSummary
        {
            TotalItems = itemList.Count,
            OpenItems = openItems.Count,
            ActiveItems = activeWork.Count,
            WaitingOnMeItems = waitingView.WaitingOnMe.Count,
            WaitingOnOthersItems = waitingView.WaitingOnOthers.Count,
            BlockedItems = blockedWork.Count,
            TodayActionItems = todayTriage.Count,
            FollowUpsNow = followUpsNow.Count,
            FollowUpsDueSoon = followUpsDueSoon.Count,
            MissingFollowUpDates = missingFollowUpDate.Count,
            InvoiceReadyItems = invoiceReadiness.Count(item => item.NeedsInvoice || item.Stage == WorkPipelineStage.InvoiceNeeded),
            TimesheetReadyItems = invoiceReadiness.Count(item => item.NeedsTimesheet || item.Stage == WorkPipelineStage.TimesheetNeeded),
            PaymentExpectedItems = paymentExpected.Count,
            ProofNeededItems = proofNeeded.Count,
            WarmOrParkedItems = warmOrParked.Count,
            ReviewNeededItems = reviewCount,
            ExpectedValueVisible = expectedValue,
            ExpectedValueExcludedFromSafe = expectedExcluded,
            PressureLabel = pressureLabel,
            Rule = "Active work, leads, blocked projects, follow-ups, invoice readiness, and payment states must flow through the same item/state spine before v5 integrations.",
            Reasons = reasons,
            IntegrityWarnings = integrityWarnings,
            CommandCentreSignals = signals,
            TodayTriage = todayTriage,
            ActiveWork = activeWork,
            WaitingOnMe = waitingView.WaitingOnMe,
            WaitingOnOthers = waitingView.WaitingOnOthers,
            BlockedWork = blockedWork,
            InvoiceReadiness = invoiceReadiness,
            PaymentExpected = paymentExpected,
            ProofNeeded = proofNeeded,
            WarmOrParked = warmOrParked,
            NeedsReview = needsReview
        };
    }

    public static WorkPipelineReviewBucket GetReviewBucket(WorkPipelineItem item, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.GetFollowUpState(today) is WorkPipelineFollowUpState.Overdue or WorkPipelineFollowUpState.DueToday)
        {
            return WorkPipelineReviewBucket.Today;
        }

        if (item.IsBlocked)
        {
            return WorkPipelineReviewBucket.Blocked;
        }

        if (item.WaitingOn.Contains("me", StringComparison.OrdinalIgnoreCase) || item.WaitingOn.Contains("Codie", StringComparison.OrdinalIgnoreCase))
        {
            return WorkPipelineReviewBucket.WaitingOnMe;
        }

        if (item.IsWaiting)
        {
            return WorkPipelineReviewBucket.WaitingOnOthers;
        }

        if (item.NeedsInvoice || item.NeedsTimesheet)
        {
            return WorkPipelineReviewBucket.InvoiceReady;
        }

        if (item.PaymentExpected)
        {
            return WorkPipelineReviewBucket.PaymentExpected;
        }

        if (string.IsNullOrWhiteSpace(item.LinkedProofNotes) && item.Stage is WorkPipelineStage.ProofInProgress or WorkPipelineStage.SentForReview or WorkPipelineStage.ApprovedHappy)
        {
            return WorkPipelineReviewBucket.ProofNeeded;
        }

        if (item.Status is WorkPipelineStatus.Warm or WorkPipelineStatus.Parked || item.Stage == WorkPipelineStage.KeepWarm)
        {
            return WorkPipelineReviewBucket.WarmOrParked;
        }

        if (!item.FollowUpDate.HasValue && item.Status is WorkPipelineStatus.Active or WorkPipelineStatus.Waiting or WorkPipelineStatus.Blocked)
        {
            return WorkPipelineReviewBucket.NeedsDate;
        }

        return WorkPipelineReviewBucket.Clean;
    }

    public static WorkPipelineMoneyState GetMoneyState(WorkPipelineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Status is WorkPipelineStatus.Completed or WorkPipelineStatus.Archived || item.IsArchived)
        {
            return WorkPipelineMoneyState.PaidOrClosed;
        }

        if (item.PaymentExpected || item.Stage == WorkPipelineStage.PaymentExpected)
        {
            return WorkPipelineMoneyState.PaymentExpected;
        }

        if (item.NeedsInvoice || item.Stage == WorkPipelineStage.InvoiceNeeded)
        {
            return WorkPipelineMoneyState.InvoiceNeeded;
        }

        if (item.NeedsTimesheet || item.Stage == WorkPipelineStage.TimesheetNeeded)
        {
            return WorkPipelineMoneyState.TimesheetNeeded;
        }

        if (item.ExpectedValue.HasValue)
        {
            return WorkPipelineMoneyState.EstimateOnly;
        }

        return WorkPipelineMoneyState.None;
    }

    public static string FormatReviewBucket(WorkPipelineReviewBucket bucket)
    {
        return bucket switch
        {
            WorkPipelineReviewBucket.Today => "Today",
            WorkPipelineReviewBucket.WaitingOnMe => "Waiting on me",
            WorkPipelineReviewBucket.WaitingOnOthers => "Waiting on others",
            WorkPipelineReviewBucket.Blocked => "Blocked",
            WorkPipelineReviewBucket.InvoiceReady => "Invoice ready",
            WorkPipelineReviewBucket.PaymentExpected => "Payment expected",
            WorkPipelineReviewBucket.ProofNeeded => "Proof needed",
            WorkPipelineReviewBucket.WarmOrParked => "Warm / parked",
            WorkPipelineReviewBucket.NeedsDate => "Needs date",
            WorkPipelineReviewBucket.Clean => "Clean",
            _ => bucket.ToString()
        };
    }

    public static string FormatMoneyState(WorkPipelineMoneyState state)
    {
        return state switch
        {
            WorkPipelineMoneyState.None => "No money state",
            WorkPipelineMoneyState.EstimateOnly => "Estimate only",
            WorkPipelineMoneyState.TimesheetNeeded => "Timesheet needed",
            WorkPipelineMoneyState.InvoiceNeeded => "Invoice needed",
            WorkPipelineMoneyState.PaymentExpected => "Payment expected",
            WorkPipelineMoneyState.PaidOrClosed => "Paid / closed",
            _ => state.ToString()
        };
    }

    private static IEnumerable<string> BuildReasons(int blocked, int followUpsNow, int followUpsDueSoon, int invoiceReady, int paymentsExpected, int proofNeeded, int waitingOnMe, int waitingOnOthers, decimal expectedExcluded, int warningCount)
    {
        yield return "v4.5 makes Work Pipeline the operating lane for active work, warm leads, blocked projects, follow-ups, invoice readiness, payment states, and proof gaps.";
        yield return "Expected work value is visible for planning but excluded from safe money until paid.";
        yield return "Blocked and waiting work must be visible so it does not silently consume sprint time.";
        yield return "Pipeline state should feed Command Centre, Follow-Ups, Payment Calendar, Paid Work Centre, Proof Tracker, and Weekly Close-Out.";

        if (blocked > 0) yield return $"{blocked} pipeline item(s) are blocked.";
        if (followUpsNow > 0) yield return $"{followUpsNow} follow-up(s) are due now.";
        if (followUpsDueSoon > 0) yield return $"{followUpsDueSoon} follow-up(s) are due soon.";
        if (invoiceReady > 0) yield return $"{invoiceReady} item(s) need invoice/timesheet action.";
        if (paymentsExpected > 0) yield return $"{paymentsExpected} item(s) have payment expected state.";
        if (proofNeeded > 0) yield return $"{proofNeeded} item(s) need proof/evidence before being treated as ready.";
        if (waitingOnMe > 0) yield return $"{waitingOnMe} item(s) are waiting on me.";
        if (waitingOnOthers > 0) yield return $"{waitingOnOthers} item(s) are waiting on others.";
        if (expectedExcluded > 0m) yield return $"{expectedExcluded:C} expected pipeline value is excluded from safe money.";
        if (warningCount > 0) yield return $"{warningCount} pipeline integrity warning(s) should be reviewed.";
    }

    private static IEnumerable<string> BuildIntegrityWarnings(IReadOnlyList<WorkPipelineItem> openItems, IReadOnlyList<WorkPipelineItem> missingDates, IReadOnlyList<WorkPipelineItem> proofNeeded)
    {
        foreach (var item in missingDates)
        {
            yield return $"{item.Title}: active/waiting/blocked item has no follow-up or keep-warm date.";
        }

        foreach (var item in openItems.Where(item => item.PaymentExpected && !item.ExpectedValue.HasValue))
        {
            yield return $"{item.Title}: payment expected but no expected value is recorded.";
        }

        foreach (var item in openItems.Where(item => item.NeedsInvoice && string.IsNullOrWhiteSpace(item.LinkedProofNotes)))
        {
            yield return $"{item.Title}: invoice needed but proof notes are missing.";
        }

        foreach (var item in proofNeeded)
        {
            yield return $"{item.Title}: proof/evidence should be attached before this becomes a clean handoff or invoice item.";
        }
    }

    private static IEnumerable<WorkPipelineOperatingSignal> BuildSignals(int open, int blocked, int followUpsNow, int waitingOnMe, int waitingOnOthers, int invoiceReady, int paymentsExpected, decimal expectedExcluded, int reviewNeeded)
    {
        yield return new WorkPipelineOperatingSignal
        {
            Label = "Pipeline open",
            Value = open.ToString(),
            Detail = "Open active work, leads, warm items, blocked work, or parked projects.",
            Priority = open > 0 ? WorkPipelinePriority.Normal : WorkPipelinePriority.Low,
            Bucket = WorkPipelineReviewBucket.Clean
        };

        if (blocked > 0)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Blocked work",
                Value = blocked.ToString(),
                Detail = "Work cannot move until a blocker clears.",
                Priority = WorkPipelinePriority.High,
                Bucket = WorkPipelineReviewBucket.Blocked
            };
        }

        if (followUpsNow > 0)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Follow-ups now",
                Value = followUpsNow.ToString(),
                Detail = "Follow-ups are overdue or due today.",
                Priority = WorkPipelinePriority.Critical,
                Bucket = WorkPipelineReviewBucket.Today
            };
        }

        if (waitingOnMe > 0 || waitingOnOthers > 0)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Waiting split",
                Value = $"Me {waitingOnMe} / Others {waitingOnOthers}",
                Detail = "Separates work waiting on me from work waiting on other people.",
                Priority = waitingOnMe > 0 ? WorkPipelinePriority.High : WorkPipelinePriority.Normal,
                Bucket = waitingOnMe > 0 ? WorkPipelineReviewBucket.WaitingOnMe : WorkPipelineReviewBucket.WaitingOnOthers
            };
        }

        if (invoiceReady > 0)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Invoice readiness",
                Value = invoiceReady.ToString(),
                Detail = "Timesheet or invoice action is needed before money can become expected/paid.",
                Priority = WorkPipelinePriority.High,
                Bucket = WorkPipelineReviewBucket.InvoiceReady
            };
        }

        if (paymentsExpected > 0 || expectedExcluded > 0m)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Expected pipeline value",
                Value = expectedExcluded.ToString("C0"),
                Detail = "Expected work value is visible but excluded from safe money until paid.",
                Priority = WorkPipelinePriority.High,
                Bucket = WorkPipelineReviewBucket.PaymentExpected
            };
        }

        if (reviewNeeded > 0)
        {
            yield return new WorkPipelineOperatingSignal
            {
                Label = "Pipeline review",
                Value = reviewNeeded.ToString(),
                Detail = "Pipeline items need date, proof, invoice, payment, or next-action review.",
                Priority = WorkPipelinePriority.High,
                Bucket = WorkPipelineReviewBucket.NeedsDate
            };
        }
    }

    private static string BuildPressureLabel(int blocked, int followUpsNow, int invoiceReady, int paymentsExpected, int reviewNeeded)
    {
        if (blocked > 0 || followUpsNow > 0)
        {
            return "Critical";
        }

        if (invoiceReady > 0 || paymentsExpected > 0 || reviewNeeded > 2)
        {
            return "High";
        }

        if (reviewNeeded > 0)
        {
            return "Medium";
        }

        return "Low";
    }
}

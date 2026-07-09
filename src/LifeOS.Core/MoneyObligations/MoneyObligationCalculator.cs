using LifeOS.Core.ItemState;

namespace LifeOS.Core.MoneyObligations;

public static class MoneyObligationCalculator
{
    public static MoneyObligationSummary Calculate(MoneyObligationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var items = profile.Items ?? [];
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        var activeItems = items
            .Where(item => item.State is not LifeOsItemState.Archived and not LifeOsItemState.Ignored)
            .ToList();

        var dueToday = activeItems
            .Where(item => item.DueDate.HasValue && item.DueDate.Value.Date == today && item.State != LifeOsItemState.Paid && item.State != LifeOsItemState.Closed)
            .ToList();

        var overdue = activeItems
            .Where(item => item.DueDate.HasValue && item.DueDate.Value.Date < today && item.State != LifeOsItemState.Paid && item.State != LifeOsItemState.Closed)
            .ToList();

        var dueSoon = activeItems
            .Where(item => item.DueDate.HasValue && item.DueDate.Value.Date > today && item.DueDate.Value.Date <= weekEnd && item.State != LifeOsItemState.Paid && item.State != LifeOsItemState.Closed)
            .ToList();

        var requiredThisWeek = activeItems
            .Where(item => item.IsRequiredThisWeek || dueToday.Contains(item) || dueSoon.Contains(item) || overdue.Contains(item))
            .Where(item => item.State != LifeOsItemState.Paid && item.State != LifeOsItemState.Closed)
            .ToList();

        var reviewQueue = activeItems
            .Where(item => !item.Trusted || item.State == LifeOsItemState.NeedsReview || item.EvidenceState == MoneyObligationEvidenceState.EvidenceNeeded)
            .OrderByDescending(item => item.PressureLevel)
            .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var safeToSpendDrag = requiredThisWeek
            .Where(item => item.AffectsSafeToSpend)
            .Sum(item => item.AmountDue);

        var projectedSafeToSpend = profile.CurrentSafeToSpendBeforeObligations - safeToSpendDrag;

        var pressureLabel = "Stable";

        if (overdue.Count > 0 || dueToday.Count > 0 || projectedSafeToSpend < 0)
        {
            pressureLabel = "Danger";
        }
        else if (requiredThisWeek.Count > 0 || reviewQueue.Count > 0)
        {
            pressureLabel = "High";
        }
        else if (dueSoon.Count > 0)
        {
            pressureLabel = "Medium";
        }

        var reasons = new List<string>
        {
            profile.Rule,
            "Expected money is still not safe money; bills and BNPL reduce safe-to-spend until paid/cleared with evidence.",
            "Pay Later / Zip / Afterpay totals must be visible as remaining balance, due installment, and weekly load.",
            "Hidden deductions such as tax/GST/ACC/student-loan/KiwiSaver/custom reserves must be visible before spending.",
            "Paid state requires evidence or a trusted manual source note.",
            "v4.2 remains local/manual; real bank, Pay Later, accounting, and email integrations are still v5+."
        };

        if (reviewQueue.Count > 0)
        {
            reasons.Add($"{reviewQueue.Count} money obligation(s) still need review or evidence before trusted state.");
        }

        if (requiredThisWeek.Count > 0)
        {
            reasons.Add($"{requiredThisWeek.Count} obligation(s) affect this week's safe-to-spend calculation.");
        }

        return new MoneyObligationSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            Rule = profile.Rule,
            TotalItems = activeItems.Count,
            OpenItems = activeItems.Count(item => item.State is LifeOsItemState.Open or LifeOsItemState.Planned or LifeOsItemState.DueSoon or LifeOsItemState.DueToday or LifeOsItemState.Overdue),
            DueSoonItems = dueSoon.Count,
            DueTodayItems = dueToday.Count,
            OverdueItems = overdue.Count,
            NeedsReviewItems = activeItems.Count(item => item.State == LifeOsItemState.NeedsReview),
            UntrustedItems = activeItems.Count(item => !item.Trusted),
            PayLaterItems = activeItems.Count(item => item.IsPayLater),
            HiddenDeductionItems = activeItems.Count(item => item.IsHiddenDeduction),
            RequiredThisWeekItems = requiredThisWeek.Count,
            PaidOrClosedItems = activeItems.Count(item => item.State is LifeOsItemState.Paid or LifeOsItemState.Closed),
            AmountDueToday = dueToday.Sum(item => item.AmountDue),
            RequiredThisWeekAmount = requiredThisWeek.Sum(item => item.AmountDue),
            OverdueAmount = overdue.Sum(item => item.AmountDue),
            PayLaterBalance = activeItems.Where(item => item.IsPayLater).Sum(item => item.RemainingBalance),
            PayLaterDueThisWeek = requiredThisWeek.Where(item => item.IsPayLater).Sum(item => item.AmountDue),
            HiddenDeductionMonthlyEstimate = activeItems.Where(item => item.IsHiddenDeduction).Sum(item => item.MonthlyEstimate),
            SafeToSpendDrag = safeToSpendDrag,
            ProjectedSafeToSpendAfterObligations = projectedSafeToSpend,
            PressureLabel = pressureLabel,
            RealBankSyncEnabled = profile.RealBankSyncEnabled,
            RealPayLaterSyncEnabled = profile.RealPayLaterSyncEnabled,
            ManualReviewRequired = profile.ManualReviewRequired,
            PaidRequiresEvidence = profile.PaidRequiresEvidence,
            Reasons = reasons,
            DuePressureItems = requiredThisWeek
                .OrderByDescending(item => item.PressureLevel)
                .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            BillsAndSubscriptions = activeItems
                .Where(item => item.Kind is MoneyObligationKind.Bill or MoneyObligationKind.Subscription)
                .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            UpcomingPayments = activeItems
                .Where(item => item.Kind is MoneyObligationKind.UpcomingPayment or MoneyObligationKind.FineOrCompliance or MoneyObligationKind.ManualCashflowItem)
                .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            PayLaterItemsList = activeItems
                .Where(item => item.IsPayLater)
                .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            HiddenDeductionItemsList = activeItems
                .Where(item => item.IsHiddenDeduction)
                .OrderByDescending(item => item.MonthlyEstimate)
                .ThenBy(item => item.Title)
                .ToList(),
            ReviewQueue = reviewQueue,
            PaidOrClosedItemsList = activeItems
                .Where(item => item.State is LifeOsItemState.Paid or LifeOsItemState.Closed)
                .OrderByDescending(item => item.UpdatedAt)
                .ThenBy(item => item.Title)
                .ToList()
        };
    }

    public static string FormatKind(MoneyObligationKind kind)
    {
        return kind switch
        {
            MoneyObligationKind.Bill => "Bill",
            MoneyObligationKind.Subscription => "Subscription",
            MoneyObligationKind.UpcomingPayment => "Upcoming payment",
            MoneyObligationKind.PayLaterInstallment => "Pay Later installment",
            MoneyObligationKind.PayLaterBalance => "Pay Later balance",
            MoneyObligationKind.HiddenDeduction => "Hidden deduction",
            MoneyObligationKind.TaxReserve => "Tax reserve",
            MoneyObligationKind.DebtPayment => "Debt payment",
            MoneyObligationKind.FineOrCompliance => "Fine/compliance",
            MoneyObligationKind.ManualCashflowItem => "Manual cashflow item",
            _ => kind.ToString()
        };
    }

    public static string FormatSource(MoneyObligationSourceKind sourceKind)
    {
        return sourceKind switch
        {
            MoneyObligationSourceKind.Manual => "Manual",
            MoneyObligationSourceKind.Receipt => "Receipt",
            MoneyObligationSourceKind.Statement => "Statement",
            MoneyObligationSourceKind.Email => "Email",
            MoneyObligationSourceKind.Calendar => "Calendar",
            MoneyObligationSourceKind.PayLaterProvider => "Pay Later provider",
            MoneyObligationSourceKind.BankStatementLater => "Bank statement later",
            MoneyObligationSourceKind.AccountingExportLater => "Accounting export later",
            MoneyObligationSourceKind.OcrImportLater => "OCR import later",
            _ => sourceKind.ToString()
        };
    }

    public static string FormatPressure(MoneyObligationPressureLevel pressureLevel)
    {
        return pressureLevel switch
        {
            MoneyObligationPressureLevel.Low => "Low",
            MoneyObligationPressureLevel.Medium => "Medium",
            MoneyObligationPressureLevel.High => "High",
            MoneyObligationPressureLevel.Critical => "Critical",
            _ => pressureLevel.ToString()
        };
    }

    public static string FormatEvidence(MoneyObligationEvidenceState evidenceState)
    {
        return evidenceState switch
        {
            MoneyObligationEvidenceState.NotNeededYet => "Not needed yet",
            MoneyObligationEvidenceState.SourceNoteOnly => "Source note only",
            MoneyObligationEvidenceState.EvidenceNeeded => "Evidence needed",
            MoneyObligationEvidenceState.EvidenceAttached => "Evidence attached",
            MoneyObligationEvidenceState.PaymentConfirmed => "Payment confirmed",
            _ => evidenceState.ToString()
        };
    }
}

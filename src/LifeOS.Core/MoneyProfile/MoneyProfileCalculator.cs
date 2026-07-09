using LifeOS.Core.ItemState;

namespace LifeOS.Core.MoneyProfile;

public static class MoneyProfileCalculator
{
    public static MoneyProfileSummary Calculate(MoneyProfilePlan profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var hiddenDeductions = profile.HiddenDeductions ?? [];
        var expectedMoneyItems = profile.ExpectedMoneyItems ?? [];

        var activeHiddenDeductions = hiddenDeductions
            .Where(item => item.State is not LifeOsItemState.Archived and not LifeOsItemState.Ignored)
            .ToList();

        foreach (var deduction in activeHiddenDeductions)
        {
            if (deduction.WeeklyReserve <= 0 && deduction.MonthlyEstimate > 0)
            {
                deduction.WeeklyReserve = Math.Round(deduction.MonthlyEstimate / 4.345m, 2);
            }
        }

        var reviewDeductions = activeHiddenDeductions
            .Where(item => !item.Trusted || item.State == LifeOsItemState.NeedsReview || item.RequiredForConfidence && string.IsNullOrWhiteSpace(item.EvidenceSummary))
            .OrderByDescending(item => item.MonthlyEstimate)
            .ThenBy(item => item.Title)
            .ToList();

        var expectedTotal = expectedMoneyItems
            .Where(item => item.State is not LifeOsItemState.Archived and not LifeOsItemState.Ignored)
            .Sum(item => item.Amount);

        var expectedExcluded = expectedMoneyItems
            .Where(item => !item.CountsAsSafeMoney)
            .Sum(item => item.Amount);

        var monthlyHidden = activeHiddenDeductions
            .Where(item => item.AffectsSafeToSpend)
            .Sum(item => item.MonthlyEstimate);

        var weeklyHidden = activeHiddenDeductions
            .Where(item => item.AffectsSafeToSpend)
            .Sum(item => item.WeeklyReserve);

        var safeBeforeHidden = profile.ConfirmedCashOnHand - profile.WeeklyKnownObligations;
        var safeAfterHidden = safeBeforeHidden - weeklyHidden;
        var safeFinal = safeAfterHidden - profile.MinimumBuffer - profile.EmergencyHold;

        var confidence = MoneyProfileConfidenceLevel.High;

        if (safeFinal < 0 || reviewDeductions.Count > 2)
        {
            confidence = MoneyProfileConfidenceLevel.Danger;
        }
        else if (reviewDeductions.Count > 0 || activeHiddenDeductions.Any(item => !item.Trusted))
        {
            confidence = MoneyProfileConfidenceLevel.Low;
        }
        else if (weeklyHidden > 0 || expectedExcluded > 0)
        {
            confidence = MoneyProfileConfidenceLevel.Medium;
        }

        var reasons = new List<string>
        {
            profile.Rule,
            "Expected money is excluded from safe-to-spend until paid/cleared with evidence.",
            "Hidden deductions reduce safe-to-spend confidence even when they are only estimates.",
            "Tax, GST, ACC, student loan, KiwiSaver, debt, and custom reserves must be visible before spending.",
            "Minimum buffer and emergency hold are deducted after obligations and hidden reserves.",
            "v4.3 remains local/manual; real bank, payslip, tax, accounting, and open banking integrations are still v5+."
        };

        if (reviewDeductions.Count > 0)
        {
            reasons.Add($"{reviewDeductions.Count} deduction/reserve item(s) still need review before safe-to-spend confidence is high.");
        }

        if (expectedExcluded > 0)
        {
            reasons.Add($"{FormatMoney(expectedExcluded)} expected money is visible but excluded from safe money.");
        }

        var calculationLines = new List<string>
        {
            $"Confirmed cash on hand: {FormatMoney(profile.ConfirmedCashOnHand)}",
            $"Minus known weekly obligations: {FormatMoney(profile.WeeklyKnownObligations)}",
            $"Safe before hidden deductions: {FormatMoney(safeBeforeHidden)}",
            $"Minus hidden weekly reserve: {FormatMoney(weeklyHidden)}",
            $"Safe after hidden deductions: {FormatMoney(safeAfterHidden)}",
            $"Minus minimum buffer: {FormatMoney(profile.MinimumBuffer)}",
            $"Minus emergency hold: {FormatMoney(profile.EmergencyHold)}",
            $"Final safe-to-spend: {FormatMoney(safeFinal)}",
            $"Expected money excluded: {FormatMoney(expectedExcluded)}"
        };

        var checklist = new List<string>
        {
            profile.ExpectedMoneyCountsAsSafe ? "Expected money is currently allowed into safe money. Review this." : "Expected money is excluded from safe money.",
            profile.RealBankSyncEnabled ? "Real bank sync is ON. This should not be true before v5." : "Real bank sync is OFF.",
            profile.ManualReviewRequired ? "Manual review is required before trusted money state." : "Manual review is not required. Review this.",
            profile.HiddenDeductionsReduceConfidence ? "Hidden deductions reduce confidence." : "Hidden deductions do not reduce confidence. Review this.",
            reviewDeductions.Count == 0 ? "No hidden deduction review blockers." : $"{reviewDeductions.Count} hidden deduction review blocker(s).",
            safeFinal >= 0 ? "Final safe-to-spend is not negative." : "Final safe-to-spend is negative."
        };

        return new MoneyProfileSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            Rule = profile.Rule,
            CurrentVisibleBalance = profile.CurrentVisibleBalance,
            ConfirmedCashOnHand = profile.ConfirmedCashOnHand,
            WeeklyKnownObligations = profile.WeeklyKnownObligations,
            MinimumBuffer = profile.MinimumBuffer,
            EmergencyHold = profile.EmergencyHold,
            HiddenDeductionMonthlyEstimate = monthlyHidden,
            HiddenDeductionWeeklyReserve = weeklyHidden,
            ExpectedMoneyTotal = expectedTotal,
            ExpectedExcludedFromSafe = expectedExcluded,
            SafeToSpendBeforeHidden = safeBeforeHidden,
            SafeToSpendAfterHidden = safeAfterHidden,
            SafeToSpendFinal = safeFinal,
            HiddenDeductionCount = activeHiddenDeductions.Count,
            RequiredReserveCount = activeHiddenDeductions.Count(item => item.RequiredForConfidence),
            ReviewNeededCount = reviewDeductions.Count,
            UntrustedCount = activeHiddenDeductions.Count(item => !item.Trusted),
            ExpectedMoneyCount = expectedMoneyItems.Count,
            UnsafeExpectedMoneyCount = expectedMoneyItems.Count(item => !item.CountsAsSafeMoney),
            ConfidenceLabel = FormatConfidence(confidence),
            ConfidenceLevel = confidence,
            ExpectedMoneyCountsAsSafe = profile.ExpectedMoneyCountsAsSafe,
            RealBankSyncEnabled = profile.RealBankSyncEnabled,
            ManualReviewRequired = profile.ManualReviewRequired,
            HiddenDeductionsReduceConfidence = profile.HiddenDeductionsReduceConfidence,
            Reasons = reasons,
            CalculationLines = calculationLines,
            ConfidenceChecklist = checklist,
            HiddenDeductions = activeHiddenDeductions
                .OrderByDescending(item => item.MonthlyEstimate)
                .ThenBy(item => item.Title)
                .ToList(),
            ReviewDeductions = reviewDeductions,
            ExpectedMoneyItems = expectedMoneyItems
                .OrderBy(item => item.ExpectedDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList()
        };
    }

    public static string FormatKind(MoneyProfileDeductionKind kind)
    {
        return kind switch
        {
            MoneyProfileDeductionKind.TaxReserve => "Tax reserve",
            MoneyProfileDeductionKind.GstReserve => "GST reserve",
            MoneyProfileDeductionKind.AccReserve => "ACC reserve",
            MoneyProfileDeductionKind.StudentLoan => "Student loan",
            MoneyProfileDeductionKind.KiwiSaver => "KiwiSaver",
            MoneyProfileDeductionKind.DebtReserve => "Debt reserve",
            MoneyProfileDeductionKind.EmergencyBuffer => "Emergency buffer",
            MoneyProfileDeductionKind.CustomReserve => "Custom reserve",
            MoneyProfileDeductionKind.SubscriptionLeak => "Subscription leak",
            MoneyProfileDeductionKind.UnknownDeduction => "Unknown deduction",
            _ => kind.ToString()
        };
    }

    public static string FormatSource(MoneyProfileSourceKind sourceKind)
    {
        return sourceKind switch
        {
            MoneyProfileSourceKind.Manual => "Manual",
            MoneyProfileSourceKind.Estimate => "Estimate",
            MoneyProfileSourceKind.PaySlipLater => "Payslip later",
            MoneyProfileSourceKind.BankStatementLater => "Bank statement later",
            MoneyProfileSourceKind.AccountingExportLater => "Accounting export later",
            MoneyProfileSourceKind.TaxPortalLater => "Tax portal later",
            MoneyProfileSourceKind.OcrImportLater => "OCR import later",
            _ => sourceKind.ToString()
        };
    }

    public static string FormatConfidence(MoneyProfileConfidenceLevel confidenceLevel)
    {
        return confidenceLevel switch
        {
            MoneyProfileConfidenceLevel.Unknown => "Unknown",
            MoneyProfileConfidenceLevel.Low => "Low",
            MoneyProfileConfidenceLevel.Medium => "Medium",
            MoneyProfileConfidenceLevel.High => "High",
            MoneyProfileConfidenceLevel.Danger => "Danger",
            _ => confidenceLevel.ToString()
        };
    }

    private static string FormatMoney(decimal amount)
    {
        return amount.ToString("C");
    }
}

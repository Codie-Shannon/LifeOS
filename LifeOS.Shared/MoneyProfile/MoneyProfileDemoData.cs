using LifeOS.Core.ItemState;
using LifeOS.Core.MoneyProfile;

namespace LifeOS.Shared.MoneyProfile;

public static class MoneyProfileDemoData
{
    public static MoneyProfilePlan CreateDefaultProfile()
    {
        return new MoneyProfilePlan
        {
            Version = "v4.3",
            Mode = "Money Profile / Hidden Deductions / Safe-to-Spend",
            Rule = "Safe-to-spend is confirmed money minus visible obligations, hidden deductions, required reserves, and buffers. Expected money stays excluded until paid.",
            CurrentVisibleBalance = 220.00m,
            ConfirmedCashOnHand = 220.00m,
            WeeklyKnownObligations = 268.75m,
            MinimumBuffer = 30.00m,
            EmergencyHold = 20.00m,
            ExpectedMoneyCountsAsSafe = false,
            RealBankSyncEnabled = false,
            ManualReviewRequired = true,
            HiddenDeductionsReduceConfidence = true,
            HiddenDeductions =
            [
                Deduction(
                    "Tax / GST reserve estimate",
                    MoneyProfileDeductionKind.TaxReserve,
                    MoneyProfileSourceKind.Estimate,
                    LifeOsItemState.Planned,
                    180.00m,
                    "Monthly reserve estimate until real accounting/tax data exists.",
                    "Manual estimate only.",
                    "Review percentage/rule before treating balance as safe.",
                    "Reserve reduces safe-to-spend confidence.",
                    "Confirm reserve rule before spending.",
                    true,
                    true),

                Deduction(
                    "ACC reserve",
                    MoneyProfileDeductionKind.AccReserve,
                    MoneyProfileSourceKind.Estimate,
                    LifeOsItemState.NeedsReview,
                    80.00m,
                    "Estimated future ACC-style obligation.",
                    "Needs source/rule before trusted.",
                    "Confirm if this applies and how much should be reserved.",
                    "Unconfirmed ACC reserve lowers confidence.",
                    "Add source/rule or mark not applicable.",
                    false,
                    true),

                Deduction(
                    "Student loan / KiwiSaver custom reserve",
                    MoneyProfileDeductionKind.StudentLoan,
                    MoneyProfileSourceKind.Estimate,
                    LifeOsItemState.NeedsReview,
                    55.00m,
                    "Placeholder for deductions that are not visible in the bank balance.",
                    "Needs source/rule before trusted.",
                    "Confirm deduction assumptions before safe-to-spend.",
                    "Unknown deductions should reduce confidence.",
                    "Confirm deduction status or mark not applicable.",
                    false,
                    true),

                Deduction(
                    "Emergency hold",
                    MoneyProfileDeductionKind.EmergencyBuffer,
                    MoneyProfileSourceKind.Manual,
                    LifeOsItemState.Confirmed,
                    86.90m,
                    "Small reserve so safe-to-spend does not hit zero.",
                    "Manual rule exists.",
                    "Keep unless user deliberately disables buffer.",
                    "Buffer protects against surprise costs.",
                    "Keep reserve active.",
                    true,
                    true),

                Deduction(
                    "Subscription slippage",
                    MoneyProfileDeductionKind.SubscriptionLeak,
                    MoneyProfileSourceKind.Manual,
                    LifeOsItemState.Planned,
                    25.00m,
                    "Small placeholder for recurring costs that are easy to forget.",
                    "Manual placeholder.",
                    "Review during weekly close-out.",
                    "Forgotten subscriptions reduce confidence.",
                    "Confirm or remove during close-out.",
                    true,
                    false)
            ],
            ExpectedMoneyItems =
            [
                ExpectedMoney(
                    "Client invoice expected",
                    LifeOsItemState.PaymentExpected,
                    420.00m,
                    DateTime.Today.AddDays(7),
                    "Demo client invoice state.",
                    "Invoice sent, payment not confirmed.",
                    "Expected money is not safe until paid/cleared.",
                    false,
                    false),

                ExpectedMoney(
                    "Work session invoiceable later",
                    LifeOsItemState.Invoiced,
                    180.00m,
                    DateTime.Today.AddDays(10),
                    "Demo work-session income candidate.",
                    "Work evidence exists, payment not confirmed.",
                    "Income estimate is visible but excluded from safe money.",
                    true,
                    false),

                ExpectedMoney(
                    "Manual paid money example",
                    LifeOsItemState.Paid,
                    90.00m,
                    DateTime.Today,
                    "Demo paid source.",
                    "Payment confirmed manually.",
                    "Already counted only if cash is actually in confirmed balance.",
                    true,
                    false)
            ]
        };
    }

    private static MoneyProfileHiddenDeduction Deduction(
        string title,
        MoneyProfileDeductionKind kind,
        MoneyProfileSourceKind source,
        LifeOsItemState state,
        decimal monthlyEstimate,
        string ruleSummary,
        string evidenceSummary,
        string reviewGate,
        string impact,
        string nextAction,
        bool trusted,
        bool requiredForConfidence)
    {
        return new MoneyProfileHiddenDeduction
        {
            Title = title,
            Kind = kind,
            SourceKind = source,
            State = state,
            MonthlyEstimate = monthlyEstimate,
            WeeklyReserve = Math.Round(monthlyEstimate / 4.345m, 2),
            RuleSummary = ruleSummary,
            EvidenceSummary = evidenceSummary,
            ReviewGate = reviewGate,
            SafeToSpendImpact = impact,
            NextAction = nextAction,
            Trusted = trusted,
            RequiredForConfidence = requiredForConfidence,
            AffectsSafeToSpend = true,
            UpdatedAt = DateTime.Now
        };
    }

    private static MoneyProfileExpectedMoney ExpectedMoney(
        string title,
        LifeOsItemState state,
        decimal amount,
        DateTime? expectedDate,
        string sourceSummary,
        string evidenceSummary,
        string safeMoneyRule,
        bool trusted,
        bool countsAsSafeMoney)
    {
        return new MoneyProfileExpectedMoney
        {
            Title = title,
            State = state,
            Amount = amount,
            ExpectedDate = expectedDate,
            SourceSummary = sourceSummary,
            EvidenceSummary = evidenceSummary,
            ReviewGate = "Do not include in safe money until payment is actually confirmed.",
            SafeMoneyRule = safeMoneyRule,
            Trusted = trusted,
            CountsAsSafeMoney = countsAsSafeMoney,
            UpdatedAt = DateTime.Now
        };
    }
}

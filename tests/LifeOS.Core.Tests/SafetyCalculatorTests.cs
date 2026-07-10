using LifeOS.Core.ItemState;
using LifeOS.Core.MoneyProfile;
using LifeOS.Core.PaymentCalendar;
using LifeOS.Core.ReceiptEvidence;
using LifeOS.Core.WeeklyCloseOut;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class SafetyCalculatorTests
{
    [Fact]
    public void MoneyProfileExcludesExpectedMoneyAndHiddenReserves()
    {
        var summary = MoneyProfileCalculator.Calculate(new MoneyProfilePlan
        {
            ConfirmedCashOnHand = 500m,
            WeeklyKnownObligations = 100m,
            MinimumBuffer = 50m,
            EmergencyHold = 25m,
            HiddenDeductions =
            [
                new MoneyProfileHiddenDeduction
                {
                    Title = "Tax reserve",
                    Kind = MoneyProfileDeductionKind.TaxReserve,
                    MonthlyEstimate = 434.50m,
                    Trusted = true,
                    EvidenceSummary = "Manual tax reserve rule.",
                    RequiredForConfidence = true,
                    AffectsSafeToSpend = true
                },
                new MoneyProfileHiddenDeduction
                {
                    Title = "Needs evidence",
                    Kind = MoneyProfileDeductionKind.CustomReserve,
                    MonthlyEstimate = 50m,
                    Trusted = false,
                    State = LifeOsItemState.NeedsReview,
                    AffectsSafeToSpend = true
                }
            ],
            ExpectedMoneyItems =
            [
                new MoneyProfileExpectedMoney
                {
                    Title = "Expected invoice",
                    Amount = 900m,
                    CountsAsSafeMoney = false
                }
            ]
        });

        Assert.Equal(900m, summary.ExpectedMoneyTotal);
        Assert.Equal(900m, summary.ExpectedExcludedFromSafe);
        Assert.Equal(111.51m, summary.HiddenDeductionWeeklyReserve);
        Assert.Equal(213.49m, summary.SafeToSpendFinal);
        Assert.Equal(1, summary.ReviewNeededCount);
        Assert.Equal("Low", summary.ConfidenceLabel);
    }

    [Fact]
    public void PaymentCalendarKeepsExpectedMoneyOutOfAmountDue()
    {
        var today = new DateOnly(2026, 7, 10);
        var summary = PaymentCalendarCalculator.Calculate(new PaymentCalendarPlan
        {
            Items =
            [
                new PaymentCalendarItem
                {
                    Title = "Power bill",
                    Kind = PaymentCalendarItemKind.Bill,
                    State = PaymentCalendarItemState.DueToday,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today,
                    Amount = 120m,
                    AffectsSafeToSpend = true,
                    IsPaymentDate = true,
                    EvidenceSummary = "Manual bill"
                },
                new PaymentCalendarItem
                {
                    Title = "Expected invoice",
                    Kind = PaymentCalendarItemKind.ExpectedIncome,
                    State = PaymentCalendarItemState.Planned,
                    TrustState = PaymentCalendarTrustState.SourceNoteOnly,
                    DueDate = today,
                    Amount = 700m,
                    ExpectedMoneyExcludedFromSafe = true,
                    AffectsSafeToSpend = true
                },
                new PaymentCalendarItem
                {
                    Title = "Imported date needs review",
                    Kind = PaymentCalendarItemKind.AgendaCommitment,
                    State = PaymentCalendarItemState.NeedsReview,
                    TrustState = PaymentCalendarTrustState.Untrusted,
                    DueDate = today.AddDays(1),
                    IsReviewWindow = true
                }
            ]
        }, today);

        Assert.Equal(120m, summary.AmountDueToday);
        Assert.Equal(120m, summary.AmountDueThisWeek);
        Assert.Equal(700m, summary.ExpectedMoneyExcluded);
        Assert.Equal(2, summary.ReviewQueueItems);
        Assert.Equal("Danger", summary.PressureLabel);
    }

    [Fact]
    public void ReceiptEvidenceRequiresSourceEvidenceBeforeTrust()
    {
        var summary = ReceiptEvidenceCalculator.Calculate(
        [
            new ReceiptEvidenceItem
            {
                Title = "Accepted but missing source",
                Amount = 42m,
                State = ReceiptEvidenceState.Accepted,
                HasSourceEvidence = false,
                AffectsMoney = true
            },
            new ReceiptEvidenceItem
            {
                Title = "Accepted and sourced",
                Amount = 20m,
                State = ReceiptEvidenceState.Accepted,
                HasSourceEvidence = true,
                AffectsMoney = true
            },
            new ReceiptEvidenceItem
            {
                Title = "Rejected candidate",
                Amount = 999m,
                State = ReceiptEvidenceState.Rejected,
                HasSourceEvidence = false,
                AffectsMoney = true
            }
        ]);

        Assert.Equal(1, summary.AcceptedCount);
        Assert.Equal(1, summary.MissingSourceCount);
        Assert.Equal(62m, summary.CandidateValue);
    }

    [Fact]
    public void WeeklyCloseOutOperatingKeepsReviewMoneyUnsafe()
    {
        var summary = WeeklyCloseOutOperatingCalculator.Calculate(
        [
            new WeeklyCloseOutReviewItem
            {
                Title = "Invoice review",
                Status = WeeklyCloseOutReviewStatus.ReadyToClose,
                Pressure = WeeklyCloseOutPressureLevel.High,
                Amount = 300m,
                IsMoneyReview = true,
                IsTrusted = false
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Blocked work",
                Status = WeeklyCloseOutReviewStatus.Blocked,
                Pressure = WeeklyCloseOutPressureLevel.Critical,
                IsWorkReview = true,
                RollIntoNextWeek = true
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Proof check",
                Status = WeeklyCloseOutReviewStatus.Waiting,
                IsProofReview = true,
                RollIntoNextWeek = true,
                IsTrusted = true
            }
        ]);

        Assert.Equal(1, summary.ReadyToCloseItems);
        Assert.Equal(2, summary.RollForwardItems);
        Assert.Equal(1, summary.BlockedItems);
        Assert.Equal(300m, summary.MoneyStillUnderReview);
        Assert.Equal(2, summary.UntrustedItems);
        Assert.Equal("Critical", summary.PressureLabel);
    }
}

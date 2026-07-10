using LifeOS.Core;
using LifeOS.Core.CommandCentrePressure;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.ItemState;
using LifeOS.Core.Money;
using LifeOS.Core.MoneyProfile;
using LifeOS.Core.PaymentCalendar;
using LifeOS.Core.ReceiptEvidence;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.IntegrationInbox;

var tests = new (string Name, Action Run)[]
{
    ("Integration preview intake creates read-only untrusted review previews", IntegrationPreviewTests.IntakeCreatesReviewPreview),
    ("Integration preview intake requires provenance identity", IntegrationPreviewTests.IntakeRequiresProvenanceIdentity),
    ("Integration review blocks duplicate acceptance", IntegrationPreviewTests.AcceptBlocksDuplicateSuspected),
    ("Integration review requires source-backed trust before acceptance", IntegrationPreviewTests.AcceptRequiresSourceBackedTrust),
    ("Integration review links only accepted previews", IntegrationPreviewTests.LinkRequiresAcceptedPreview),
    ("Integration inbox summary keeps imported money unsafe and visible", IntegrationPreviewTests.SummaryCountsReviewAndPreviewMoney),
    ("Integration demo data respects preview safety contract", IntegrationPreviewTests.DemoDataRespectsPreviewContract),
    ("Pressure engine suppresses early waiting-on-others signals", PressureEngineTests.SuppressWaitingOnOthers),
    ("Pressure engine forces untrusted signals into review", PressureEngineTests.UntrustedSignalsRequireReview),
    ("Pressure engine chooses act-now next action before review", PressureEngineTests.ActNowWinsNextSafestAction),
    ("Pressure engine limits top ranked signals", PressureEngineTests.TopSignalsRespectPolicyLimit),
    ("LifeOS week starts on the current Monday even on Sunday", CalendarRuleTests.MondayWeekStartHandlesSunday),
    ("Money pressure excludes pending income from safe-to-spend", MoneyRuleTests.PendingIncomeIsNotSafeMoney),
    ("Money pressure counts only unpaid events in the active week", MoneyRuleTests.BillsDueUseActiveWeekAndPaidState),
    ("Weekly close-out recognizes the current week on Sunday", WeeklyCloseOutRuleTests.CurrentWeekCloseOutHandlesSunday),
    ("Money profile excludes expected money and hidden reserves from safe-to-spend", MoneyProfileRuleTests.ExpectedMoneyAndHiddenReservesStayUnsafe),
    ("Payment calendar keeps expected money out of amount due", PaymentCalendarRuleTests.ExpectedMoneyIsVisibleButNotDue),
    ("Receipt evidence requires source evidence before trust", ReceiptEvidenceRuleTests.AcceptedReceiptWithoutSourceStaysUntrusted),
    ("Weekly close-out operating keeps review money unsafe", WeeklyCloseOutOperatingRuleTests.ReviewMoneyAndRollForwardStayVisible)
};

var failures = new List<string>();

foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{test.Name}: {ex.Message}");
        Console.WriteLine($"FAIL {test.Name}");
        Console.WriteLine($"     {ex.Message}");
    }
}

if (failures.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine($"{failures.Count} test(s) failed.");
    Environment.Exit(1);
}

Console.WriteLine();
Console.WriteLine($"{tests.Length} test(s) passed.");

static class IntegrationPreviewTests
{
    public static void IntakeCreatesReviewPreview()
    {
        var preview = IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.ManualImport,
            SourceLabel = "manual-json",
            ExternalReference = "row-42",
            Title = "Imported bill candidate",
            Summary = "Possible bill from manual import.",
            Amount = 72.5m,
            Currency = "nzd",
            SuggestedTarget = IntegrationTargetKind.BillsPayments,
            SourceEvidence = "manual-import.json"
        });

        Assert.Equal(IntegrationPreviewStatus.New, preview.Status);
        Assert.Equal(IntegrationTrustState.Untrusted, preview.TrustState);
        Assert.True(preview.IsReadOnlyPreview, "Intake previews must be read-only.");
        Assert.True(preview.RequiresHumanReview, "Intake previews must require human review.");
        Assert.Equal("NZD", preview.Currency);
        Assert.False(string.IsNullOrWhiteSpace(preview.DuplicateKey), "Duplicate key should be generated.");
    }

    public static void IntakeRequiresProvenanceIdentity()
    {
        Assert.Throws<ArgumentException>(() => IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.Email,
            ExternalReference = "message-1",
            Title = "Missing source label"
        }));

        Assert.Throws<ArgumentException>(() => IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.Email,
            SourceLabel = "gmail",
            Title = "Missing external reference"
        }));
    }

    public static void AcceptBlocksDuplicateSuspected()
    {
        var item = ReviewablePreview();
        item.Status = IntegrationPreviewStatus.DuplicateSuspected;

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Accept(item, "Looks okay."));
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, item.Status);
    }

    public static void AcceptRequiresSourceBackedTrust()
    {
        var item = ReviewablePreview();
        item.TrustState = IntegrationTrustState.Untrusted;

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Accept(item, "Looks okay."));
        Assert.Equal(IntegrationPreviewStatus.New, item.Status);
    }

    public static void LinkRequiresAcceptedPreview()
    {
        var item = ReviewablePreview();

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Link(item, "target-1"));

        IntegrationInboxReviewEngine.Accept(item, "Accepted for handoff.");
        IntegrationInboxReviewEngine.Link(item, "target-1");

        Assert.Equal(IntegrationPreviewStatus.Linked, item.Status);
        Assert.Equal(IntegrationTrustState.Trusted, item.TrustState);
        Assert.Equal("target-1", item.LinkReference);
    }

    public static void SummaryCountsReviewAndPreviewMoney()
    {
        var summary = IntegrationInboxCalculator.Calculate(
        [
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Accounting,
                Status = IntegrationPreviewStatus.New,
                TrustState = IntegrationTrustState.Untrusted,
                Amount = 100m
            },
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Calendar,
                Status = IntegrationPreviewStatus.Accepted,
                TrustState = IntegrationTrustState.Reviewed,
                Amount = 50m
            },
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Email,
                Status = IntegrationPreviewStatus.DuplicateSuspected,
                TrustState = IntegrationTrustState.SourceBacked
            }
        ]);

        Assert.Equal(3, summary.Total);
        Assert.Equal(2, summary.NeedsReview);
        Assert.Equal(1, summary.ReadyForHandoff);
        Assert.Equal(150m, summary.PreviewMoney);
        Assert.Equal("High", summary.PressureLabel);
    }

    public static void DemoDataRespectsPreviewContract()
    {
        var demo = IntegrationInboxDemoData.Create();

        Assert.True(demo.Count > 0, "Demo data should contain integration previews.");

        foreach (var item in demo)
        {
            Assert.True(item.IsReadOnlyPreview, $"{item.Title} should remain read-only.");
            Assert.True(item.RequiresHumanReview, $"{item.Title} should require human review.");
            Assert.False(string.IsNullOrWhiteSpace(item.SourceLabel), $"{item.Title} should have a source label.");
            Assert.False(string.IsNullOrWhiteSpace(item.ExternalReference), $"{item.Title} should have an external reference.");
            Assert.False(string.IsNullOrWhiteSpace(item.DuplicateKey), $"{item.Title} should have a duplicate key.");
        }
    }

    private static IntegrationPreviewItem ReviewablePreview()
    {
        return new IntegrationPreviewItem
        {
            SourceKind = IntegrationSourceKind.ManualImport,
            SourceLabel = "manual-json",
            ExternalReference = "row-42",
            Title = "Imported candidate",
            Status = IntegrationPreviewStatus.New,
            TrustState = IntegrationTrustState.SourceBacked,
            SourceEvidence = "manual-import.json",
            DuplicateKey = "manual-json:row-42"
        };
    }
}

static class PressureEngineTests
{
    public static void SuppressWaitingOnOthers()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "waiting-client",
                Module = "Work Pipeline",
                Title = "Waiting on client",
                Severity = PressureSeverity.High,
                Lane = PressureLane.Waiting,
                IsWaitingOnOthers = true,
                IsDueNow = false,
                NextAction = "Do not chase yet."
            }
        ],
        DefaultPolicy());

        Assert.Equal(0, summary.TopSignals.Count);
        Assert.Equal(1, summary.SuppressedSignals);
        Assert.Equal(1, summary.ProtectedSignals);
    }

    public static void UntrustedSignalsRequireReview()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "ocr-receipt",
                Module = "Receipt Evidence",
                Title = "OCR candidate",
                Severity = PressureSeverity.High,
                Lane = PressureLane.ActNow,
                IsTrusted = false,
                NextAction = "Review source evidence."
            }
        ],
        DefaultPolicy());

        Assert.Equal(1, summary.ReviewSignals);
        Assert.Equal(0, summary.ActNowSignals);
        Assert.Equal("Review source evidence.", summary.NextSafestAction);
    }

    public static void ActNowWinsNextSafestAction()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "review",
                Module = "Integration Inbox",
                Title = "Preview needs review",
                Severity = PressureSeverity.High,
                Lane = PressureLane.Review,
                NextAction = "Review import."
            },
            new PressureSignal
            {
                Key = "act",
                Module = "Money",
                Title = "Bill due today",
                Severity = PressureSeverity.Critical,
                Lane = PressureLane.ActNow,
                IsDueNow = true,
                NextAction = "Pay or defer the bill deliberately."
            }
        ],
        DefaultPolicy());

        Assert.Equal("Critical", summary.PressureLabel);
        Assert.Equal("Pay or defer the bill deliberately.", summary.NextSafestAction);
    }

    public static void TopSignalsRespectPolicyLimit()
    {
        var policy = DefaultPolicy();
        policy.MaximumTopSignals = 2;

        var summary = CommandCentrePressureCalculator.Calculate(
        [
            Signal("a", PressureSeverity.Normal),
            Signal("b", PressureSeverity.Critical),
            Signal("c", PressureSeverity.High)
        ],
        policy);

        Assert.Equal(2, summary.TopSignals.Count);
        Assert.Equal("b", summary.TopSignals[0].Key);
        Assert.Equal("c", summary.TopSignals[1].Key);
    }

    private static PressureSignal Signal(string key, PressureSeverity severity)
    {
        return new PressureSignal
        {
            Key = key,
            Module = "Test",
            Title = key,
            Severity = severity,
            Lane = PressureLane.ActNow,
            NextAction = $"Handle {key}."
        };
    }

    private static CommandCentrePressurePolicy DefaultPolicy()
    {
        return new CommandCentrePressurePolicy
        {
            LowWeight = 1,
            NormalWeight = 3,
            HighWeight = 7,
            CriticalWeight = 12,
            NormalScore = 5,
            HighScore = 16,
            CriticalScore = 30,
            MaximumTopSignals = 8,
            SuppressWaitingOnOthers = true,
            RequireReviewForUntrusted = true
        };
    }
}

static class CalendarRuleTests
{
    public static void MondayWeekStartHandlesSunday()
    {
        var sunday = new DateOnly(2026, 7, 12);

        Assert.Equal(new DateOnly(2026, 7, 6), LifeOSWeek.GetMondayStart(sunday));
    }
}

static class MoneyRuleTests
{
    public static void PendingIncomeIsNotSafeMoney()
    {
        var summary = MoneyPressureCalculator.Calculate(
            currentBalance: 100m,
            incomeItems:
            [
                new IncomeItem
                {
                    Source = "Paid invoice",
                    Amount = 100m,
                    Status = IncomeStatus.Paid,
                    TaxSetAsidePercent = 20m
                },
                new IncomeItem
                {
                    Source = "Expected invoice",
                    Amount = 1000m,
                    Status = IncomeStatus.Expected,
                    TaxSetAsidePercent = 20m
                }
            ],
            moneyEvents: [],
            deductionRules: [],
            foodFuelBuffer: 50m,
            emergencyBuffer: 20m,
            weekStart: new DateOnly(2026, 7, 6),
            weekEnd: new DateOnly(2026, 7, 12));

        Assert.Equal(80m, summary.ConfirmedPaidIncome);
        Assert.Equal(800m, summary.PendingIncome);
        Assert.Equal(110m, summary.SafeToSpend);
    }

    public static void BillsDueUseActiveWeekAndPaidState()
    {
        var summary = MoneyPressureCalculator.Calculate(
            currentBalance: 500m,
            incomeItems: [],
            moneyEvents:
            [
                new MoneyEvent
                {
                    Name = "Due this week",
                    Amount = 120m,
                    DueDate = new DateOnly(2026, 7, 8)
                },
                new MoneyEvent
                {
                    Name = "Already paid",
                    Amount = 80m,
                    DueDate = new DateOnly(2026, 7, 8),
                    IsPaid = true
                },
                new MoneyEvent
                {
                    Name = "Next week",
                    Amount = 90m,
                    DueDate = new DateOnly(2026, 7, 13)
                }
            ],
            deductionRules:
            [
                new DeductionRule
                {
                    Name = "Fixed deduction",
                    Type = DeductionType.FixedAmount,
                    Value = 40m
                },
                new DeductionRule
                {
                    Name = "Inactive deduction",
                    Type = DeductionType.FixedAmount,
                    Value = 999m,
                    IsActive = false
                }
            ],
            foodFuelBuffer: 50m,
            emergencyBuffer: 30m,
            weekStart: new DateOnly(2026, 7, 6),
            weekEnd: new DateOnly(2026, 7, 12));

        Assert.Equal(120m, summary.BillsDue);
        Assert.Equal(40m, summary.DeductionsDue);
        Assert.Equal(260m, summary.SafeToSpend);
    }
}

static class WeeklyCloseOutRuleTests
{
    public static void CurrentWeekCloseOutHandlesSunday()
    {
        var today = new DateOnly(2026, 7, 12);
        var summary = WeeklyCloseOutCalculator.Calculate(
        [
            new WeeklyCloseOutEntry
            {
                WeekStart = new DateOnly(2026, 7, 6),
                WhatGotDone = "Closed the week.",
                StillWaitingOn = "Client reply"
            }
        ],
        today);

        Assert.True(summary.HasCurrentWeekCloseOut, "Sunday should still belong to the Monday-start current week.");
        Assert.Equal(1, summary.EntriesThisWeek);
        Assert.Equal(1, summary.WaitingOnCount);
    }
}

static class MoneyProfileRuleTests
{
    public static void ExpectedMoneyAndHiddenReservesStayUnsafe()
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
}

static class PaymentCalendarRuleTests
{
    public static void ExpectedMoneyIsVisibleButNotDue()
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
}

static class ReceiptEvidenceRuleTests
{
    public static void AcceptedReceiptWithoutSourceStaysUntrusted()
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
}

static class WeeklyCloseOutOperatingRuleTests
{
    public static void ReviewMoneyAndRollForwardStayVisible()
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

static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}, got {actual}.");
        }
    }

    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void False(bool condition, string message)
    {
        if (condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Expected {typeof(TException).Name}, got {ex.GetType().Name}.");
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}, but no exception was thrown.");
    }
}

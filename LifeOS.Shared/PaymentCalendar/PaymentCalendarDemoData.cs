using LifeOS.Core.PaymentCalendar;

namespace LifeOS.Shared.PaymentCalendar;

public static class PaymentCalendarDemoData
{
    public static PaymentCalendarPlan CreateDefaultPlan()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return new PaymentCalendarPlan
        {
            Version = "v4.4",
            Mode = "Local/manual",
            Rule = "Dates do not make money safe. Bills, BNPL, agenda commitments, payment dates, expected income, and review windows must be visible together before the week is trusted.",
            WindowStart = today,
            WindowEnd = today.AddDays(14),
            ManualReviewRequired = true,
            RealCalendarSyncEnabled = false,
            RealBankSyncEnabled = false,
            RealEmailSyncEnabled = false,
            ExpectedMoneyCountsAsSafe = false,
            Items = new List<PaymentCalendarItem>
            {
                new()
                {
                    Title = "Afterpay instalment",
                    Kind = PaymentCalendarItemKind.PayLaterInstallment,
                    State = PaymentCalendarItemState.DueToday,
                    Pressure = PaymentCalendarPressureLevel.Critical,
                    TrustState = PaymentCalendarTrustState.Untrusted,
                    DueDate = today,
                    Amount = 24.50m,
                    Balance = 122.50m,
                    InstalmentAmount = 24.50m,
                    MonthlyAmount = 49.00m,
                    Source = "Demo BNPL provider",
                    Category = "Pay Later",
                    EvidenceSummary = "Screenshot/receipt should be linked later.",
                    ReviewRule = "Do not mark paid until payment evidence exists.",
                    PressureImpact = "Pay-later due today reduces safe-to-spend and must appear on the calendar lane.",
                    NextAction = "Pay or confirm payment evidence.",
                    OriginalModule = "Pay Later",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    IsReviewWindow = true,
                    SortOrder = 10
                },
                new()
                {
                    Title = "Power bill due this week",
                    Kind = PaymentCalendarItemKind.Bill,
                    State = PaymentCalendarItemState.DueSoon,
                    Pressure = PaymentCalendarPressureLevel.Critical,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today.AddDays(3),
                    Amount = 148.50m,
                    Balance = 148.50m,
                    Source = "Manual",
                    Category = "Utilities",
                    EvidenceSummary = "Manual bill source note exists.",
                    ReviewRule = "Confirm paid only after payment evidence exists.",
                    PressureImpact = "Bill due this week reduces safe-to-spend and creates weekly money pressure.",
                    NextAction = "Pay, defer, or budget.",
                    OriginalModule = "Bills / Payments",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    SortOrder = 20
                },
                new()
                {
                    Title = "Phone plan",
                    Kind = PaymentCalendarItemKind.Subscription,
                    State = PaymentCalendarItemState.DueSoon,
                    Pressure = PaymentCalendarPressureLevel.High,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today.AddDays(1),
                    Amount = 35.00m,
                    Balance = 35.00m,
                    MonthlyAmount = 35.00m,
                    Source = "Manual",
                    Category = "Subscription",
                    EvidenceSummary = "Manual subscription note.",
                    ReviewRule = "Confirm recurring amount before trusting.",
                    PressureImpact = "Upcoming subscription affects this week's safe-to-spend.",
                    NextAction = "Confirm amount and due date.",
                    OriginalModule = "Bills / Payments",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    SortOrder = 30
                },
                new()
                {
                    Title = "Zip instalment",
                    Kind = PaymentCalendarItemKind.PayLaterInstallment,
                    State = PaymentCalendarItemState.DueSoon,
                    Pressure = PaymentCalendarPressureLevel.High,
                    TrustState = PaymentCalendarTrustState.EvidenceNeeded,
                    DueDate = today.AddDays(5),
                    Amount = 18.00m,
                    Balance = 72.00m,
                    InstalmentAmount = 18.00m,
                    MonthlyAmount = 36.00m,
                    Source = "Demo Zip provider",
                    Category = "Pay Later",
                    EvidenceSummary = "Manual BNPL schedule placeholder.",
                    ReviewRule = "Review schedule before safe-to-spend.",
                    PressureImpact = "BNPL load affects money timeline and close-out.",
                    NextAction = "Add source screenshot or confirm schedule.",
                    OriginalModule = "Pay Later",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    IsReviewWindow = true,
                    SortOrder = 40
                },
                new()
                {
                    Title = "School pickup / family commitment",
                    Kind = PaymentCalendarItemKind.AgendaCommitment,
                    State = PaymentCalendarItemState.DueToday,
                    Pressure = PaymentCalendarPressureLevel.High,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today,
                    TimeText = "Afternoon",
                    Source = "Manual",
                    Category = "Care / family",
                    EvidenceSummary = "Manual agenda commitment.",
                    ReviewRule = "Fixed commitments block time even when money pressure exists.",
                    PressureImpact = "Fixed family/care time reduces available work time.",
                    NextAction = "Protect the time window.",
                    OriginalModule = "Agenda",
                    AffectsAgenda = true,
                    IsFixedCommitment = true,
                    SortOrder = 50
                },
                new()
                {
                    Title = "Warm lead follow-up",
                    Kind = PaymentCalendarItemKind.FollowUp,
                    State = PaymentCalendarItemState.Waiting,
                    Pressure = PaymentCalendarPressureLevel.Medium,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today.AddDays(2),
                    Source = "Manual relationship profile",
                    Category = "Follow-up",
                    EvidenceSummary = "Manual follow-up profile exists.",
                    ReviewRule = "Do not chase before follow-up window.",
                    PressureImpact = "Waiting work affects work pressure but should not create noise early.",
                    NextAction = "Wait or schedule follow-up.",
                    OriginalModule = "Follow-Ups",
                    AffectsAgenda = true,
                    SortOrder = 60
                },
                new()
                {
                    Title = "Client invoice expected",
                    Kind = PaymentCalendarItemKind.ExpectedIncome,
                    State = PaymentCalendarItemState.Waiting,
                    Pressure = PaymentCalendarPressureLevel.High,
                    TrustState = PaymentCalendarTrustState.Untrusted,
                    DueDate = today.AddDays(7),
                    Amount = 420.00m,
                    Source = "Demo client invoice state",
                    Category = "Expected income",
                    EvidenceSummary = "Invoice sent; payment not confirmed.",
                    ReviewRule = "Expected money is visible by date but excluded from safe-to-spend until paid.",
                    PressureImpact = "Expected income can affect follow-up timing, not safe money.",
                    NextAction = "Wait, follow up later, or mark paid only with evidence.",
                    OriginalModule = "Paid Work Centre",
                    ExpectedMoneyExcludedFromSafe = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    SortOrder = 70
                },
                new()
                {
                    Title = "Receipt import review",
                    Kind = PaymentCalendarItemKind.ReceiptReview,
                    State = PaymentCalendarItemState.NeedsReview,
                    Pressure = PaymentCalendarPressureLevel.High,
                    TrustState = PaymentCalendarTrustState.Untrusted,
                    DueDate = today,
                    Amount = 37.90m,
                    Source = "Receipt OCR placeholder",
                    Category = "Receipt/OCR",
                    EvidenceSummary = "Original image is evidence; OCR result is untrusted.",
                    ReviewRule = "OCR result must be reviewed before creating a payment/proof item.",
                    PressureImpact = "Unreviewed imported data creates review pressure, not final money truth.",
                    NextAction = "Accept, edit, link, or reject OCR output.",
                    OriginalModule = "Integration Inbox",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsReviewWindow = true,
                    SortOrder = 80
                },
                new()
                {
                    Title = "Weekly close-out",
                    Kind = PaymentCalendarItemKind.WeeklyCloseOut,
                    State = PaymentCalendarItemState.Planned,
                    Pressure = PaymentCalendarPressureLevel.Medium,
                    TrustState = PaymentCalendarTrustState.Trusted,
                    DueDate = today.AddDays(3),
                    TimeText = "Evening",
                    Source = "Weekly Close-Out",
                    Category = "Review",
                    EvidenceSummary = "Recurring local review point.",
                    ReviewRule = "Open loops should be reviewed before the next week starts.",
                    PressureImpact = "Open week review affects next-week money/work pressure.",
                    NextAction = "Review open money, work, proof, and follow-ups.",
                    OriginalModule = "Weekly Close-Out",
                    AffectsAgenda = true,
                    IsFixedCommitment = true,
                    SortOrder = 90
                },
                new()
                {
                    Title = "Vehicle compliance renewal",
                    Kind = PaymentCalendarItemKind.ComplianceRenewal,
                    State = PaymentCalendarItemState.Planned,
                    Pressure = PaymentCalendarPressureLevel.Medium,
                    TrustState = PaymentCalendarTrustState.SourceNoteOnly,
                    DueDate = today.AddDays(21),
                    Amount = 90.00m,
                    Balance = 90.00m,
                    Source = "Manual",
                    Category = "Compliance",
                    EvidenceSummary = "Manual future cost placeholder.",
                    ReviewRule = "Review amount before due week.",
                    PressureImpact = "Future payment should be visible before it becomes urgent.",
                    NextAction = "Keep planned until due window.",
                    OriginalModule = "Bills / Payments",
                    AffectsSafeToSpend = true,
                    AffectsAgenda = true,
                    IsPaymentDate = true,
                    SortOrder = 100
                }
            }
        };
    }
}

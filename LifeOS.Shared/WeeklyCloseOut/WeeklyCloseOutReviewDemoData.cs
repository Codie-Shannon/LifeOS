using LifeOS.Core.WeeklyCloseOut;

namespace LifeOS.Shared.WeeklyCloseOut;

public static class WeeklyCloseOutReviewDemoData
{
    public static List<WeeklyCloseOutReviewItem> Create()
    {
        return
        [
            new WeeklyCloseOutReviewItem
            {
                Title = "Close completed LifeOS release work",
                SourceModule = "Work Pipeline",
                Status = WeeklyCloseOutReviewStatus.ReadyToClose,
                Pressure = WeeklyCloseOutPressureLevel.Normal,
                Owner = "Me",
                NextAction = "Confirm screenshots and release notes are committed, then close the work item.",
                EvidenceState = "Build and screenshot evidence attached",
                IsWorkReview = true,
                IsProofReview = true,
                IsTrusted = true,
                Notes = "Demo close-now item."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Carry blocked client portal work forward",
                SourceModule = "Work Pipeline",
                Status = WeeklyCloseOutReviewStatus.Blocked,
                Pressure = WeeklyCloseOutPressureLevel.High,
                Owner = "External",
                NextAction = "Wait for access confirmation. Do not sink sprint time while blocked.",
                EvidenceState = "Waiting-on note present",
                IsWorkReview = true,
                RollIntoNextWeek = true,
                IsTrusted = true,
                Notes = "Blocked work remains visible without consuming active sprint time."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Review expected client payment",
                SourceModule = "Money / Work Pipeline",
                Status = WeeklyCloseOutReviewStatus.Waiting,
                Pressure = WeeklyCloseOutPressureLevel.High,
                Owner = "External",
                NextAction = "Keep visible by follow-up date; do not count as safe money until paid.",
                EvidenceState = "Invoice/review note present",
                Amount = 120m,
                IsMoneyReview = true,
                IsWorkReview = true,
                RollIntoNextWeek = true,
                IsTrusted = true,
                Notes = "Expected money is visible but excluded from safe money."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Resolve receipt missing source",
                SourceModule = "Receipt OCR / Evidence",
                Status = WeeklyCloseOutReviewStatus.Open,
                Pressure = WeeklyCloseOutPressureLevel.High,
                Owner = "Me",
                NextAction = "Attach the receipt image before linking it to proof or money state.",
                EvidenceState = "Source missing",
                Amount = 18.90m,
                IsMoneyReview = true,
                IsProofReview = true,
                IsReceiptReview = true,
                RollIntoNextWeek = true,
                IsTrusted = false,
                Notes = "Raw import cannot become trusted without source evidence."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Archive accepted utility receipt",
                SourceModule = "Receipt OCR / Evidence",
                Status = WeeklyCloseOutReviewStatus.ReadyToClose,
                Pressure = WeeklyCloseOutPressureLevel.Low,
                Owner = "Me",
                NextAction = "Link to the paid bill or archive with the close-out evidence set.",
                EvidenceState = "Accepted and source-backed",
                Amount = 62m,
                IsMoneyReview = true,
                IsReceiptReview = true,
                IsTrusted = true,
                Notes = "Trusted evidence can be closed cleanly."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Confirm invoice-ready proof package",
                SourceModule = "Proof Tracker / Paid Work",
                Status = WeeklyCloseOutReviewStatus.Open,
                Pressure = WeeklyCloseOutPressureLevel.Critical,
                Owner = "Me",
                NextAction = "Review proof and client-safe summary before treating the work as billable.",
                EvidenceState = "Proof package needs final review",
                Amount = 180m,
                IsMoneyReview = true,
                IsProofReview = true,
                IsWorkReview = true,
                RollIntoNextWeek = true,
                IsTrusted = true,
                Notes = "Invoice readiness is gated by proof."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Protect next week's fixed caregiving window",
                SourceModule = "Agenda / Payment Calendar",
                Status = WeeklyCloseOutReviewStatus.ReadyToClose,
                Pressure = WeeklyCloseOutPressureLevel.Normal,
                Owner = "Me",
                NextAction = "Confirm the fixed commitment remains protected in next week's agenda.",
                EvidenceState = "Manual agenda commitment",
                RollIntoNextWeek = true,
                IsTrusted = true,
                Notes = "Time pressure rolls forward deliberately."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Keep warm portfolio lead parked",
                SourceModule = "Work Pipeline",
                Status = WeeklyCloseOutReviewStatus.Waiting,
                Pressure = WeeklyCloseOutPressureLevel.Low,
                Owner = "External",
                NextAction = "Do not chase until the review window opens.",
                EvidenceState = "Keep-warm note",
                IsWorkReview = true,
                RollIntoNextWeek = true,
                IsTrusted = true,
                Notes = "Parked work stays contained."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Review hidden deduction reserve",
                SourceModule = "Money Profile",
                Status = WeeklyCloseOutReviewStatus.Open,
                Pressure = WeeklyCloseOutPressureLevel.High,
                Owner = "Me",
                NextAction = "Confirm the reserve estimate before trusting next week's safe-to-spend.",
                EvidenceState = "Manual estimate",
                Amount = 98.25m,
                IsMoneyReview = true,
                RollIntoNextWeek = true,
                IsTrusted = false,
                Notes = "Estimated deductions reduce confidence."
            },
            new WeeklyCloseOutReviewItem
            {
                Title = "Close paid bill evidence",
                SourceModule = "Bills / Payments",
                Status = WeeklyCloseOutReviewStatus.ReadyToClose,
                Pressure = WeeklyCloseOutPressureLevel.Low,
                Owner = "Me",
                NextAction = "Confirm payment evidence is linked and close the bill item.",
                EvidenceState = "Paid evidence present",
                Amount = 35m,
                IsMoneyReview = true,
                IsProofReview = true,
                IsTrusted = true,
                Notes = "A clean paid item should not remain as open pressure."
            }
        ];
    }
}

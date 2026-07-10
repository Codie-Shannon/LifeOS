namespace LifeOS.Core.WeeklyCloseOut;

public sealed class WeeklyCloseOutOperatingSummary
{
    public required int TotalItems { get; init; }

    public required int ReadyToCloseItems { get; init; }

    public required int RollForwardItems { get; init; }

    public required int WaitingItems { get; init; }

    public required int BlockedItems { get; init; }

    public required int MoneyReviewItems { get; init; }

    public required int ProofReviewItems { get; init; }

    public required int ReceiptReviewItems { get; init; }

    public required int WorkReviewItems { get; init; }

    public required int UntrustedItems { get; init; }

    public required decimal MoneyStillUnderReview { get; init; }

    public required string PressureLabel { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> CloseNow { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> RollForward { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> WaitingOrBlocked { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> MoneyChecks { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> ProofChecks { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> ReceiptChecks { get; init; }

    public required IReadOnlyList<WeeklyCloseOutReviewItem> WorkChecks { get; init; }
}

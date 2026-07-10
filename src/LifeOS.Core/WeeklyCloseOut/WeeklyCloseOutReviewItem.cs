namespace LifeOS.Core.WeeklyCloseOut;

public sealed class WeeklyCloseOutReviewItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string SourceModule { get; set; } = string.Empty;

    public WeeklyCloseOutReviewStatus Status { get; set; } = WeeklyCloseOutReviewStatus.Open;

    public WeeklyCloseOutPressureLevel Pressure { get; set; } = WeeklyCloseOutPressureLevel.Normal;

    public string Owner { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string EvidenceState { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public bool IsMoneyReview { get; set; }

    public bool IsProofReview { get; set; }

    public bool IsReceiptReview { get; set; }

    public bool IsWorkReview { get; set; }

    public bool IsTrusted { get; set; }

    public bool RollIntoNextWeek { get; set; }

    public string Notes { get; set; } = string.Empty;
}

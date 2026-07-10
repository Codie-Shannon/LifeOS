namespace LifeOS.Core.ReceiptEvidence;

public sealed class ReceiptEvidenceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Category { get; set; } = "Unreviewed receipt";
    public string SourceType { get; set; } = "Manual import";
    public string ConfidenceLabel { get; set; } = "Needs review";
    public ReceiptEvidenceState State { get; set; } = ReceiptEvidenceState.ImportedCandidate;
    public string TargetItemType { get; set; } = "Evidence";
    public string NextAction { get; set; } = "Review the candidate against its source.";
    public string Notes { get; set; } = string.Empty;
    public string SourcePathOrNote { get; set; } = string.Empty;
    public bool HasSourceEvidence { get; set; }
    public bool AffectsMoney { get; set; }
    public bool AffectsWorkProof { get; set; }

    public bool IsTrusted => State == ReceiptEvidenceState.Accepted && HasSourceEvidence;
    public bool NeedsReview => State is ReceiptEvidenceState.ImportedCandidate or ReceiptEvidenceState.NeedsReview or ReceiptEvidenceState.NeedsSource;
}

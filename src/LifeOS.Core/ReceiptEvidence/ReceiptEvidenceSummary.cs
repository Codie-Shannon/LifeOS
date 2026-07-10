namespace LifeOS.Core.ReceiptEvidence;

public sealed class ReceiptEvidenceSummary
{
    public required IReadOnlyList<ReceiptEvidenceItem> Items { get; init; }
    public required IReadOnlyList<ReceiptEvidenceItem> ReviewQueue { get; init; }
    public required IReadOnlyList<ReceiptEvidenceItem> MissingSourceItems { get; init; }
    public required IReadOnlyList<ReceiptEvidenceItem> AcceptedItems { get; init; }
    public required IReadOnlyList<ReceiptEvidenceItem> MoneyCandidates { get; init; }
    public required IReadOnlyList<ReceiptEvidenceItem> WorkProofCandidates { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public int TotalItems => Items.Count;
    public int ReviewCount => ReviewQueue.Count;
    public int MissingSourceCount => MissingSourceItems.Count;
    public int AcceptedCount => AcceptedItems.Count;
    public int UntrustedCount => Items.Count(item => !item.IsTrusted);
    public decimal CandidateValue => Items.Where(item => item.State != ReceiptEvidenceState.Rejected).Sum(item => item.Amount);
    public string PressureLabel => ReviewCount == 0 && MissingSourceCount == 0 ? "Clean" : MissingSourceCount > 0 ? "High" : "Review";
}

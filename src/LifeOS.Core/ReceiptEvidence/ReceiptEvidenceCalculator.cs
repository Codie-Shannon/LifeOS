namespace LifeOS.Core.ReceiptEvidence;

public static class ReceiptEvidenceCalculator
{
    public static ReceiptEvidenceSummary Calculate(IEnumerable<ReceiptEvidenceItem> source)
    {
        var items = source.OrderByDescending(item => item.ReceiptDate).ThenBy(item => item.Title).ToList();
        var review = items.Where(item => item.NeedsReview).ToList();
        var missingSource = items.Where(item => !item.HasSourceEvidence && item.State != ReceiptEvidenceState.Rejected).ToList();
        var accepted = items.Where(item => item.IsTrusted).ToList();
        var money = items.Where(item => item.AffectsMoney && item.State != ReceiptEvidenceState.Rejected).ToList();
        var workProof = items.Where(item => item.AffectsWorkProof && item.State != ReceiptEvidenceState.Rejected).ToList();

        var reasons = new List<string>
        {
            "OCR or imported receipt fields are candidates, not trusted facts.",
            "Manual review is required before a receipt can affect bills, money pressure, paid work, or proof.",
            "A trusted item requires source evidence plus an accepted state.",
            "Rejected candidates remain excluded from item state and money calculations.",
            $"{review.Count} receipt candidate(s) need review.",
            $"{missingSource.Count} receipt candidate(s) are missing source evidence.",
            $"{accepted.Count} receipt item(s) are accepted and source-backed.",
            $"{items.Count(item => !item.IsTrusted)} receipt item(s) are still untrusted."
        };

        return new ReceiptEvidenceSummary
        {
            Items = items,
            ReviewQueue = review,
            MissingSourceItems = missingSource,
            AcceptedItems = accepted,
            MoneyCandidates = money,
            WorkProofCandidates = workProof,
            Reasons = reasons
        };
    }
}

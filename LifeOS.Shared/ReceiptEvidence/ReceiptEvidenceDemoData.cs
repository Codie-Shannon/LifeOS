using LifeOS.Core.ReceiptEvidence;

namespace LifeOS.Shared.ReceiptEvidence;

public static class ReceiptEvidenceDemoData
{
    public static List<ReceiptEvidenceItem> Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new ReceiptEvidenceItem
            {
                Title = "Fuel receipt OCR candidate",
                Merchant = "Demo Fuel Station",
                Amount = 42.70m,
                ReceiptDate = today,
                Category = "Vehicle / fuel",
                SourceType = "OCR placeholder",
                ConfidenceLabel = "Medium",
                State = ReceiptEvidenceState.NeedsReview,
                TargetItemType = "Money / vehicle evidence",
                NextAction = "Compare merchant, amount, and date against the source image.",
                Notes = "OCR output remains untrusted until manually accepted.",
                SourcePathOrNote = "Local demo image reference",
                HasSourceEvidence = true,
                AffectsMoney = true
            },
            new ReceiptEvidenceItem
            {
                Title = "Client supplies receipt",
                Merchant = "Demo Hardware Supplier",
                Amount = 18.90m,
                ReceiptDate = today.AddDays(-1),
                Category = "Project materials",
                SourceType = "Manual import",
                ConfidenceLabel = "Low",
                State = ReceiptEvidenceState.NeedsSource,
                TargetItemType = "Paid work / proof evidence",
                NextAction = "Attach the receipt photo before linking this candidate to work proof.",
                Notes = "Do not treat this as billable proof without a source.",
                HasSourceEvidence = false,
                AffectsMoney = true,
                AffectsWorkProof = true
            },
            new ReceiptEvidenceItem
            {
                Title = "Paid utility receipt",
                Merchant = "Demo Utility Provider",
                Amount = 62.00m,
                ReceiptDate = today.AddDays(-2),
                Category = "Bill payment evidence",
                SourceType = "Manual receipt",
                ConfidenceLabel = "High",
                State = ReceiptEvidenceState.Accepted,
                TargetItemType = "Bills / Payments evidence",
                NextAction = "Link to the paid bill or archive during weekly close-out.",
                Notes = "Accepted demo path: manually reviewed and source-backed.",
                SourcePathOrNote = "Local receipt image attached",
                HasSourceEvidence = true,
                AffectsMoney = true
            },
            new ReceiptEvidenceItem
            {
                Title = "Duplicate receipt candidate",
                Merchant = "Demo Retailer",
                Amount = 12.40m,
                ReceiptDate = today.AddDays(-3),
                Category = "Possible duplicate",
                SourceType = "OCR placeholder",
                ConfidenceLabel = "Low",
                State = ReceiptEvidenceState.Rejected,
                TargetItemType = "No item",
                NextAction = "Keep rejected unless a reviewer restores it.",
                Notes = "Rejected candidates do not affect item state or money.",
                SourcePathOrNote = "Duplicate of an existing receipt",
                HasSourceEvidence = true
            }
        ];
    }
}

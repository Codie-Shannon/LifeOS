using LifeOS.Core.Documents;
using LifeOS.Core.FinancialRecords;

namespace LifeOS.Core.FinancialReview;

public static class FinancialReviewProofData
{
    public static FinancialReviewProof Build(DateTimeOffset now)
    {
        var source = FinancialProofData.Build(now);
        var today = DateOnly.FromDateTime(now.Date);
        var proofEvidence = source.Invoices[0].Evidence;
        var transactions = source.Transactions.Concat([
            new FinancialTransaction("txn-unmatched-60", "group60-unmatched", "acct-operating", TransactionDirection.Income, 275m, "NZD", today.AddDays(-5), "Fictional unmatched client receipt", "cat-work", "party-a", FinancialReviewState.Confirmed, [new(FinancialLinkType.Work, "work-review-60", "Fictional reviewed work")], proofEvidence),
            new FinancialTransaction("txn-duplicate-60-a", "group60-duplicate", "acct-card", TransactionDirection.Expense, 29.90m, "NZD", today.AddDays(-6), "Fictional duplicate software record A", "cat-software", "party-s", FinancialReviewState.Confirmed, [], proofEvidence),
            new FinancialTransaction("txn-duplicate-60-b", "group60-duplicate", "acct-card", TransactionDirection.Expense, 29.90m, "NZD", today.AddDays(-6), "Fictional duplicate software record B", "cat-software", "party-s", FinancialReviewState.Confirmed, [], proofEvidence)
        ]).ToArray();
        var mismatchInvoice = new FinancialInvoice("inv-60-mismatch", "INV-DEMO-060-X", "party-a", 500m, "NZD", today.AddDays(-8), today.AddDays(2), InvoiceState.Paid, 500m, FinancialReviewState.Confirmed, [], proofEvidence);
        var invoices = source.Invoices.Append(mismatchInvoice).ToArray();
        var payments = source.Payments.Concat([
            new FinancialPayment("pay-60-partial", "inv-58-c", 250m, "NZD", today, PaymentState.PartiallyAllocated, 200m, FinancialReviewState.Confirmed, proofEvidence),
            new FinancialPayment("pay-60-orphan", "inv-missing-60", 90m, "NZD", today.AddDays(-4), PaymentState.Received, 0m, FinancialReviewState.Confirmed, []),
            new FinancialPayment("pay-60-mismatch", mismatchInvoice.Id, 450m, "USD", today.AddDays(-10), PaymentState.Allocated, 500m, FinancialReviewState.Confirmed, proofEvidence)
        ]).ToArray();
        var records = source with { Transactions = transactions, Invoices = invoices, Payments = payments };
        var documents = DocumentProofData.Build(now).Records;
        var candidates = new FinancialReviewService().GenerateCandidates(records, now);
        return new(records, documents, candidates);
    }
}

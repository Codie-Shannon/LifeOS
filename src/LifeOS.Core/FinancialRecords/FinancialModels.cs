namespace LifeOS.Core.FinancialRecords;

public sealed record FinancialAccount(string Id, string Name, FinancialAccountType Type, string Currency, decimal Balance, bool IsLiveConnected, FinancialReviewState ReviewState);
public sealed record FinancialCategory(string Id, string Name, TransactionDirection Direction, bool IsActive = true);
public sealed record FinancialParty(string Id, string DisplayName, FinancialPartyType Type);
public sealed record FinancialLink(FinancialLinkType Type, string RecordId, string Label);
public sealed record FinancialEvidenceLink(string EvidenceId, string Label, string Sha256, FinancialReviewState ReviewState);
public sealed record FinancialTransaction(string Id, string SubmissionKey, string AccountId, TransactionDirection Direction, decimal Amount, string Currency, DateOnly Date, string Description, string CategoryId, string? PartyId, FinancialReviewState ReviewState, IReadOnlyList<FinancialLink> Links, IReadOnlyList<FinancialEvidenceLink> Evidence);
public sealed record FinancialInvoice(string Id, string InvoiceNumber, string PartyId, decimal Total, string Currency, DateOnly IssueDate, DateOnly DueDate, InvoiceState State, decimal AllocatedAmount, FinancialReviewState ReviewState, IReadOnlyList<FinancialLink> Links, IReadOnlyList<FinancialEvidenceLink> Evidence)
{
    public decimal Outstanding => Math.Max(0m, decimal.Round(Total - AllocatedAmount, 2));
}
public sealed record FinancialPayment(string Id, string InvoiceId, decimal Amount, string Currency, DateOnly Date, PaymentState State, decimal AllocatedAmount, FinancialReviewState ReviewState, IReadOnlyList<FinancialEvidenceLink> Evidence)
{
    public decimal Unallocated => Math.Max(0m, decimal.Round(Amount - AllocatedAmount, 2));
}
public sealed record FinancialAuditEntry(string Id, string RecordType, string RecordId, string Action, string SafeSummary, DateTimeOffset OccurredUtc);
public sealed record FinancialOverview(decimal CurrentBalance, decimal Income, decimal Expenses, decimal InvoicesDue, decimal PaymentsReceived, int EvidenceGaps, string Currency);
public sealed record FinancialDataset(IReadOnlyList<FinancialAccount> Accounts, IReadOnlyList<FinancialCategory> Categories, IReadOnlyList<FinancialParty> Parties, IReadOnlyList<FinancialTransaction> Transactions, IReadOnlyList<FinancialInvoice> Invoices, IReadOnlyList<FinancialPayment> Payments, IReadOnlyList<FinancialAuditEntry> Audit);

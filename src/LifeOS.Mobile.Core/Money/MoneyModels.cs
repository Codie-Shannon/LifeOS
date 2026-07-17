namespace LifeOS.Mobile.Core.Money;

public enum MoneyRecordStatus
{
    Draft,
    Due,
    Paid,
    Received,
    NeedsReview,
    Queued
}

public enum EvidenceReviewState
{
    Draft,
    Ready,
    DuplicateCandidate,
    Linked,
    Discarded,
    Queued
}

public sealed record MoneySummary(
    decimal Income,
    decimal Expenses,
    decimal OutstandingInvoices,
    decimal PaymentsReceived,
    string Currency);

public sealed record InvoiceRecord(
    string Id,
    string Client,
    decimal Amount,
    DateOnly IssueDate,
    DateOnly DueDate,
    MoneyRecordStatus Status,
    string Currency,
    IReadOnlyList<string> LinkedEvidence);

public sealed record TransactionRecord(
    string Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    string Category,
    MoneyRecordStatus Status,
    string Currency);

public sealed record EvidenceDraft(
    string Id,
    string FileName,
    string SafePreviewLabel,
    long SizeBytes,
    string Category,
    string? LinkedRecordId,
    EvidenceReviewState ReviewState,
    string Sha256,
    DateTimeOffset CapturedUtc);

public sealed record MoneyDashboardSnapshot(
    MoneySummary Summary,
    IReadOnlyList<InvoiceRecord> Invoices,
    IReadOnlyList<TransactionRecord> Transactions,
    IReadOnlyList<EvidenceDraft> Evidence);

using LifeOS.Core.FinancialRecords;

namespace LifeOS.Core.FinancialReview;

public enum EvidenceCompletenessState { Complete, Missing, PendingReview, Conflicting }
public enum FinancialExportFormat { Csv, Pdf }

public sealed record FinancialReportFilter(DateOnly FromDate, DateOnly ToDate, string? AccountId = null, string? CategoryId = null, string? PartyId = null, FinancialReviewState? Status = null, string? LinkedRecordId = null)
{
    public void Validate()
    {
        if (ToDate < FromDate) throw new ArgumentException("Report end date cannot precede start date.");
    }
}

public sealed record CurrencyFinancialSummary(string Currency, decimal Income, decimal Expenses, decimal OutstandingInvoices, decimal PaymentsReceived, int TransactionCount, int InvoiceCount, int PaymentCount);
public sealed record EvidenceCompletenessItem(string RecordType, string RecordId, string Label, EvidenceCompletenessState State, string Explanation, IReadOnlyList<FinancialLink> Links);
public sealed record FinancialReport(DateTimeOffset GeneratedUtc, FinancialReportFilter Filter, IReadOnlyList<CurrencyFinancialSummary> CurrencyGroups, IReadOnlyList<EvidenceCompletenessItem> Evidence, IReadOnlyList<ReconciliationCandidate> UnresolvedCandidates, bool IsStale, string FreshnessNote);
public sealed record FinancialExportRequest(FinancialExportFormat Format, string Destination, bool Confirmed, bool RedactSensitiveValues, IReadOnlyList<string> SelectedSections);
public sealed record FinancialExportPreview(string FileName, string MediaType, string Destination, bool RedactionEnabled, IReadOnlyList<string> Sections, long EstimatedBytes, string Warning);
public sealed record FinancialExportResult(bool Succeeded, bool Cancelled, string Message, byte[] Bytes, FinancialExportPreview Preview);

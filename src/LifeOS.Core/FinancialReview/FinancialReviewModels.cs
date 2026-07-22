using LifeOS.Core.Documents;
using LifeOS.Core.FinancialRecords;

namespace LifeOS.Core.FinancialReview;

public sealed record ReviewSourceReference(
    ReviewSourceKind Kind,
    string RecordId,
    string Label,
    string Provenance,
    string Fingerprint,
    DateTimeOffset FreshnessUtc,
    bool Exists = true);

public sealed record FinancialDiscrepancy(
    ReconciliationCandidateType Type,
    decimal? ExpectedAmount,
    decimal? ActualAmount,
    DateOnly? ExpectedDate,
    DateOnly? ActualDate,
    string? ExpectedCurrency,
    string? ActualCurrency,
    string Explanation);

public sealed record EvidenceGap(
    EvidenceGapKind Kind,
    string RecordType,
    string RecordId,
    string Explanation,
    string? LinkedDocumentId = null);

public sealed record AllocationIssue(
    string InvoiceId,
    string PaymentId,
    decimal InvoiceOutstanding,
    decimal PaymentUnallocated,
    decimal ProposedAllocation,
    decimal RemainingInvoiceBalance,
    decimal RemainingPaymentBalance);

public sealed record ReviewResolution(
    string Id,
    ReviewResolutionAction Action,
    string SafeNote,
    DateTimeOffset ResolvedUtc,
    bool Confirmed,
    string? LinkedRecordId = null,
    decimal? AllocationAmount = null);

public sealed record FinancialReviewAuditEntry(
    string Action,
    string SafeSummary,
    DateTimeOffset OccurredUtc);

public sealed record ReconciliationCandidate(
    string Id,
    ReconciliationCandidateType Type,
    FinancialReviewSeverity Severity,
    FinancialReviewStatus Status,
    string PartyId,
    DateOnly RecordDate,
    string Currency,
    string Explanation,
    decimal? Confidence,
    IReadOnlyList<ReviewSourceReference> Sources,
    FinancialDiscrepancy? Discrepancy,
    EvidenceGap? EvidenceGap,
    AllocationIssue? AllocationIssue,
    IReadOnlyList<ReviewResolution> Resolutions,
    IReadOnlyList<FinancialReviewAuditEntry> Audit)
{
    public bool HasTrustedStateAction => Resolutions.Any(x =>
        x.Action is ReviewResolutionAction.Link or
        ReviewResolutionAction.Allocate or
        ReviewResolutionAction.CorrectLocalRecord or
        ReviewResolutionAction.AcceptAsException);
}

public sealed record FinancialReviewFilter(
    ReconciliationCandidateType? Type = null,
    FinancialReviewSeverity? Severity = null,
    FinancialReviewStatus? Status = null,
    string? PartyId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? LinkedRecordId = null);

public sealed record FinancialReviewSummary(
    int Open,
    int Critical,
    int Deferred,
    int Resolved,
    int EvidenceGaps,
    int AllocationIssues);

public sealed record CandidateResolutionRequest(
    ReviewResolutionAction Action,
    string SafeNote,
    bool Confirmed,
    string? LinkedRecordId = null,
    decimal? AllocationAmount = null);

public sealed record CandidateResolutionResult(
    bool Applied,
    string Message,
    ReconciliationCandidate Candidate);

public sealed record FinancialReviewProof(
    FinancialDataset FinancialRecords,
    IReadOnlyList<DocumentRecord> Documents,
    IReadOnlyList<ReconciliationCandidate> Candidates);

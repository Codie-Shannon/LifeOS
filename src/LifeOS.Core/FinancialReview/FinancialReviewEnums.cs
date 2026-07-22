namespace LifeOS.Core.FinancialReview;

public enum ReconciliationCandidateType
{
    UnmatchedTransaction,
    InvoiceWithoutPayment,
    PaymentWithoutInvoice,
    PartialAllocation,
    AmountMismatch,
    DateMismatch,
    CurrencyMismatch,
    DuplicateCandidate,
    MissingEvidence
}

public enum FinancialReviewSeverity { Information, Warning, Critical }
public enum FinancialReviewStatus { Open, Deferred, Resolved, Dismissed, ExceptionAccepted, SourceChanged, SourceRemoved }
public enum ReviewResolutionAction { Link, Allocate, CorrectLocalRecord, AcceptAsException, Defer, Dismiss, Reopen }
public enum ReviewSourceKind { Transaction, Invoice, Payment, Document }
public enum EvidenceGapKind { MissingAcceptedDocument, MissingOriginal, ConflictingEvidence }

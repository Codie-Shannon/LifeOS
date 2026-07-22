using System.Text.RegularExpressions;
using LifeOS.Core.Documents;
using LifeOS.Core.FinancialRecords;

namespace LifeOS.Core.FinancialReview;

public sealed class FinancialReviewService
{
    private static readonly Regex SensitiveNumber = new(@"\b\d{6,}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<ReconciliationCandidate> GenerateCandidates(
        FinancialDataset data,
        DateTimeOffset generatedUtc,
        IEnumerable<ReconciliationCandidate>? existing = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        var candidates = new Dictionary<string, ReconciliationCandidate>(StringComparer.Ordinal);
        var trustedTransactions = data.Transactions.Where(x => x.ReviewState == FinancialReviewState.Confirmed).ToArray();
        var trustedInvoices = data.Invoices.Where(x => x.ReviewState == FinancialReviewState.Confirmed).ToArray();
        var trustedPayments = data.Payments.Where(x => x.ReviewState == FinancialReviewState.Confirmed).ToArray();

        foreach (var invoice in trustedInvoices)
        {
            var payments = trustedPayments.Where(x => string.Equals(x.InvoiceId, invoice.Id, StringComparison.Ordinal)).ToArray();
            if (invoice.Outstanding > 0m && payments.Length == 0)
            {
                Add(candidates, Candidate(
                    ReconciliationCandidateType.InvoiceWithoutPayment,
                    FinancialReviewSeverity.Warning,
                    invoice.PartyId,
                    invoice.DueDate,
                    invoice.Currency,
                    $"Invoice {invoice.InvoiceNumber} has an outstanding balance and no linked trusted payment.",
                    .99m,
                    [InvoiceSource(invoice, generatedUtc)],
                    new FinancialDiscrepancy(ReconciliationCandidateType.InvoiceWithoutPayment, invoice.Total, 0m, invoice.DueDate, null, invoice.Currency, null, "No trusted payment references this invoice."),
                    null,
                    null,
                    generatedUtc));
            }

            if (invoice.Outstanding > 0m && payments.Length > 0)
            {
                var payment = payments.OrderByDescending(x => x.Date).First();
                var allocation = CalculateAllocation(invoice, payment);
                Add(candidates, Candidate(
                    ReconciliationCandidateType.PartialAllocation,
                    FinancialReviewSeverity.Warning,
                    invoice.PartyId,
                    payment.Date,
                    invoice.Currency,
                    $"Invoice {invoice.InvoiceNumber} remains partially allocated after trusted payments.",
                    .98m,
                    [InvoiceSource(invoice, generatedUtc), PaymentSource(payment, generatedUtc)],
                    null,
                    null,
                    allocation,
                    generatedUtc));
            }

            if (invoice.Evidence.Count == 0)
                Add(candidates, MissingEvidenceCandidate("Invoice", invoice.Id, invoice.InvoiceNumber, invoice.PartyId, invoice.IssueDate, invoice.Currency, InvoiceSource(invoice, generatedUtc), generatedUtc));
        }

        foreach (var payment in trustedPayments)
        {
            var invoice = trustedInvoices.SingleOrDefault(x => string.Equals(x.Id, payment.InvoiceId, StringComparison.Ordinal));
            if (invoice is null)
            {
                Add(candidates, Candidate(
                    ReconciliationCandidateType.PaymentWithoutInvoice,
                    FinancialReviewSeverity.Critical,
                    "unlinked-party",
                    payment.Date,
                    payment.Currency,
                    $"Payment {payment.Id} references an invoice that is not present in trusted financial records.",
                    1m,
                    [PaymentSource(payment, generatedUtc)],
                    new FinancialDiscrepancy(ReconciliationCandidateType.PaymentWithoutInvoice, null, payment.Amount, null, payment.Date, null, payment.Currency, "The referenced invoice could not be found."),
                    null,
                    null,
                    generatedUtc));
                continue;
            }

            if (!string.Equals(invoice.Currency, payment.Currency, StringComparison.Ordinal))
                Add(candidates, Mismatch(invoice, payment, ReconciliationCandidateType.CurrencyMismatch, FinancialReviewSeverity.Critical, "Invoice and payment currencies do not match.", generatedUtc));
            if (payment.Amount != invoice.Total && payment.AllocatedAmount >= invoice.Total)
                Add(candidates, Mismatch(invoice, payment, ReconciliationCandidateType.AmountMismatch, FinancialReviewSeverity.Critical, "Allocated payment amount conflicts with the invoice total.", generatedUtc));
            if (payment.Date < invoice.IssueDate)
                Add(candidates, Mismatch(invoice, payment, ReconciliationCandidateType.DateMismatch, FinancialReviewSeverity.Warning, "Payment date precedes the invoice issue date.", generatedUtc));
            if (payment.Evidence.Count == 0)
                Add(candidates, MissingEvidenceCandidate("Payment", payment.Id, payment.Id, invoice.PartyId, payment.Date, payment.Currency, PaymentSource(payment, generatedUtc), generatedUtc));
        }

        foreach (var transaction in trustedTransactions)
        {
            bool matched = trustedPayments.Any(payment =>
                payment.Amount == transaction.Amount &&
                string.Equals(payment.Currency, transaction.Currency, StringComparison.Ordinal) &&
                Math.Abs(payment.Date.DayNumber - transaction.Date.DayNumber) <= 3);
            if (!matched)
            {
                Add(candidates, Candidate(
                    ReconciliationCandidateType.UnmatchedTransaction,
                    FinancialReviewSeverity.Warning,
                    transaction.PartyId ?? "unlinked-party",
                    transaction.Date,
                    transaction.Currency,
                    $"Transaction {transaction.Id} has no deterministic trusted payment match.",
                    .87m,
                    [TransactionSource(transaction, generatedUtc)],
                    new FinancialDiscrepancy(ReconciliationCandidateType.UnmatchedTransaction, transaction.Amount, null, transaction.Date, null, transaction.Currency, null, "No exact amount/currency and near-date trusted payment was found."),
                    null,
                    null,
                    generatedUtc));
            }
            if (transaction.Evidence.Count == 0)
                Add(candidates, MissingEvidenceCandidate("Transaction", transaction.Id, transaction.Description, transaction.PartyId ?? "unlinked-party", transaction.Date, transaction.Currency, TransactionSource(transaction, generatedUtc), generatedUtc));
        }

        foreach (var group in trustedTransactions.GroupBy(x => x.SubmissionKey, StringComparer.Ordinal).Where(x => x.Count() > 1))
        {
            var duplicates = group.OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            Add(candidates, Candidate(
                ReconciliationCandidateType.DuplicateCandidate,
                FinancialReviewSeverity.Critical,
                duplicates[0].PartyId ?? "unlinked-party",
                duplicates[0].Date,
                duplicates[0].Currency,
                $"{duplicates.Length} trusted transactions share submission key {group.Key}.",
                1m,
                duplicates.Select(x => TransactionSource(x, generatedUtc)).ToArray(),
                new FinancialDiscrepancy(ReconciliationCandidateType.DuplicateCandidate, duplicates[0].Amount, duplicates[1].Amount, duplicates[0].Date, duplicates[1].Date, duplicates[0].Currency, duplicates[1].Currency, "Candidate only; no merge or deletion is performed."),
                null,
                null,
                generatedUtc));
        }

        var prior = (existing ?? []).GroupBy(x => x.Id, StringComparer.Ordinal).ToDictionary(x => x.Key, x => x.Last(), StringComparer.Ordinal);
        return candidates.Values
            .Select(candidate => prior.TryGetValue(candidate.Id, out var old) ? PreserveReviewState(candidate, old, generatedUtc) : candidate)
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.RecordDate)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<ReconciliationCandidate> Filter(IEnumerable<ReconciliationCandidate> candidates, FinancialReviewFilter filter)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(filter);
        return candidates.Where(x =>
            (!filter.Type.HasValue || x.Type == filter.Type) &&
            (!filter.Severity.HasValue || x.Severity == filter.Severity) &&
            (!filter.Status.HasValue || x.Status == filter.Status) &&
            (string.IsNullOrWhiteSpace(filter.PartyId) || string.Equals(x.PartyId, filter.PartyId, StringComparison.OrdinalIgnoreCase)) &&
            (!filter.FromDate.HasValue || x.RecordDate >= filter.FromDate.Value) &&
            (!filter.ToDate.HasValue || x.RecordDate <= filter.ToDate.Value) &&
            (string.IsNullOrWhiteSpace(filter.LinkedRecordId) || x.Sources.Any(s => string.Equals(s.RecordId, filter.LinkedRecordId, StringComparison.OrdinalIgnoreCase))))
            .ToArray();
    }

    public FinancialReviewSummary Summarize(IEnumerable<ReconciliationCandidate> candidates)
    {
        var items = candidates.ToArray();
        return new(
            items.Count(x => x.Status == FinancialReviewStatus.Open),
            items.Count(x => x.Status == FinancialReviewStatus.Open && x.Severity == FinancialReviewSeverity.Critical),
            items.Count(x => x.Status == FinancialReviewStatus.Deferred),
            items.Count(x => x.Status is FinancialReviewStatus.Resolved or FinancialReviewStatus.ExceptionAccepted),
            items.Count(x => x.EvidenceGap is not null && x.Status == FinancialReviewStatus.Open),
            items.Count(x => x.AllocationIssue is not null && x.Status == FinancialReviewStatus.Open));
    }

    public CandidateResolutionResult Resolve(ReconciliationCandidate candidate, CandidateResolutionRequest request, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(request);
        var changesTrustedState = request.Action is ReviewResolutionAction.Link or ReviewResolutionAction.Allocate or ReviewResolutionAction.CorrectLocalRecord or ReviewResolutionAction.AcceptAsException;
        if (changesTrustedState && !request.Confirmed)
            return new(false, "Explicit confirmation is required before trusted financial state can change.", candidate);
        if (request.Action == ReviewResolutionAction.Reopen && candidate.Status == FinancialReviewStatus.Open)
            return new(false, "Candidate is already open.", candidate);
        if (request.Action != ReviewResolutionAction.Reopen && candidate.Status is FinancialReviewStatus.SourceChanged or FinancialReviewStatus.SourceRemoved)
            return new(false, "Source conflict must be reviewed before resolution.", candidate);
        if (request.Action == ReviewResolutionAction.Allocate)
        {
            if (candidate.AllocationIssue is null)
                return new(false, "This candidate has no allocation issue.", candidate);
            if (!request.AllocationAmount.HasValue || request.AllocationAmount.Value <= 0m || request.AllocationAmount.Value > candidate.AllocationIssue.ProposedAllocation)
                return new(false, "Allocation must be positive and within the deterministic proposed boundary.", candidate);
        }

        var status = request.Action switch
        {
            ReviewResolutionAction.Defer => FinancialReviewStatus.Deferred,
            ReviewResolutionAction.Dismiss => FinancialReviewStatus.Dismissed,
            ReviewResolutionAction.AcceptAsException => FinancialReviewStatus.ExceptionAccepted,
            ReviewResolutionAction.Reopen => FinancialReviewStatus.Open,
            _ => FinancialReviewStatus.Resolved
        };
        var safeNote = Redact(request.SafeNote);
        var resolution = new ReviewResolution(
            FinancialRecordService.DeterministicId($"{candidate.Id}|{request.Action}|{now:O}|{safeNote}"),
            request.Action,
            safeNote,
            now,
            request.Confirmed,
            request.LinkedRecordId,
            request.AllocationAmount);
        var updated = candidate with
        {
            Status = status,
            Resolutions = candidate.Resolutions.Append(resolution).ToArray(),
            Audit = candidate.Audit.Append(new FinancialReviewAuditEntry(request.Action.ToString(), $"{request.Action} recorded explicitly. {safeNote}", now)).ToArray()
        };
        return new(true, $"{request.Action} recorded; no external provider write occurred.", updated);
    }

    public CandidateResolutionResult LinkAcceptedEvidence(ReconciliationCandidate candidate, DocumentRecord document, string safeNote, bool confirmed, DateTimeOffset now)
    {
        if (candidate.EvidenceGap is null)
            return new(false, "Candidate has no evidence gap.", candidate);
        if (document.State != DocumentIntakeState.Accepted || !document.HasTrustedOriginal)
            return new(false, "Only an explicitly accepted document with its preserved original can resolve an evidence gap.", candidate);
        var result = Resolve(candidate, new CandidateResolutionRequest(ReviewResolutionAction.Link, safeNote, confirmed, document.Id), now);
        if (!result.Applied) return result;
        return result with { Candidate = result.Candidate with { EvidenceGap = candidate.EvidenceGap with { LinkedDocumentId = document.Id } } };
    }

    public ReconciliationCandidate RefreshSourceState(ReconciliationCandidate candidate, IEnumerable<ReviewSourceReference> currentSources, DateTimeOffset now)
    {
        var current = currentSources.ToDictionary(x => $"{x.Kind}:{x.RecordId}", StringComparer.Ordinal);
        bool removed = candidate.Sources.Any(source => !current.ContainsKey($"{source.Kind}:{source.RecordId}"));
        bool changed = !removed && candidate.Sources.Any(source => current.TryGetValue($"{source.Kind}:{source.RecordId}", out var found) && !string.Equals(source.Fingerprint, found.Fingerprint, StringComparison.Ordinal));
        if (!removed && !changed) return candidate;
        var status = removed ? FinancialReviewStatus.SourceRemoved : FinancialReviewStatus.SourceChanged;
        return candidate with
        {
            Status = status,
            Audit = candidate.Audit.Append(new FinancialReviewAuditEntry(status.ToString(), "Trusted source changed or was removed; no silent recovery was attempted.", now)).ToArray()
        };
    }

    public AllocationIssue CalculateAllocation(FinancialInvoice invoice, FinancialPayment payment)
    {
        if (!string.Equals(invoice.Currency, payment.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException("Allocation requires matching currencies.");
        var proposed = decimal.Round(Math.Min(invoice.Outstanding, payment.Unallocated), 2, MidpointRounding.AwayFromZero);
        return new(invoice.Id, payment.Id, invoice.Outstanding, payment.Unallocated, proposed,
            decimal.Round(invoice.Outstanding - proposed, 2, MidpointRounding.AwayFromZero),
            decimal.Round(payment.Unallocated - proposed, 2, MidpointRounding.AwayFromZero));
    }

    public string Redact(string value)
    {
        var text = FinancialRecordService.Redact(value ?? string.Empty);
        return SensitiveNumber.Replace(text, "[number-redacted]");
    }

    private static ReconciliationCandidate PreserveReviewState(ReconciliationCandidate fresh, ReconciliationCandidate old, DateTimeOffset now)
    {
        bool changed = fresh.Sources.Count != old.Sources.Count || fresh.Sources.Zip(old.Sources).Any(pair => !string.Equals(pair.First.Fingerprint, pair.Second.Fingerprint, StringComparison.Ordinal));
        if (changed)
            return fresh with { Status = FinancialReviewStatus.SourceChanged, Resolutions = old.Resolutions, Audit = old.Audit.Append(new FinancialReviewAuditEntry("SourceChanged", "Candidate source fingerprint changed; prior review was not silently reused.", now)).ToArray() };
        return fresh with { Status = old.Status, Resolutions = old.Resolutions, Audit = old.Audit };
    }

    private static ReconciliationCandidate Mismatch(FinancialInvoice invoice, FinancialPayment payment, ReconciliationCandidateType type, FinancialReviewSeverity severity, string explanation, DateTimeOffset now) =>
        Candidate(type, severity, invoice.PartyId, payment.Date, payment.Currency, explanation, .99m,
            [InvoiceSource(invoice, now), PaymentSource(payment, now)],
            new FinancialDiscrepancy(type, invoice.Total, payment.Amount, invoice.IssueDate, payment.Date, invoice.Currency, payment.Currency, explanation),
            null, null, now);

    private static ReconciliationCandidate MissingEvidenceCandidate(string recordType, string recordId, string label, string partyId, DateOnly date, string currency, ReviewSourceReference source, DateTimeOffset now) =>
        Candidate(ReconciliationCandidateType.MissingEvidence, FinancialReviewSeverity.Warning, partyId, date, currency,
            $"{recordType} {label} has no accepted preserved evidence link.", .95m, [source], null,
            new EvidenceGap(EvidenceGapKind.MissingAcceptedDocument, recordType, recordId, "Accepted preserved evidence is required before this gap can be resolved."), null, now);

    private static ReconciliationCandidate Candidate(ReconciliationCandidateType type, FinancialReviewSeverity severity, string partyId, DateOnly date, string currency, string explanation, decimal? confidence, IReadOnlyList<ReviewSourceReference> sources, FinancialDiscrepancy? discrepancy, EvidenceGap? evidenceGap, AllocationIssue? allocationIssue, DateTimeOffset now)
    {
        var sourceKey = string.Join("|", sources.Select(x => $"{x.Kind}:{x.RecordId}").OrderBy(x => x, StringComparer.Ordinal));
        var id = $"review-{FinancialRecordService.DeterministicId($"{type}|{sourceKey}")}";
        return new(id, type, severity, FinancialReviewStatus.Open, partyId, date, currency, explanation, confidence, sources, discrepancy, evidenceGap, allocationIssue, [], [new FinancialReviewAuditEntry("Generated", "Review candidate generated from trusted local LifeOS records only.", now)]);
    }

    private static ReviewSourceReference TransactionSource(FinancialTransaction item, DateTimeOffset now) => new(ReviewSourceKind.Transaction, item.Id, item.Description, "LifeOS.FinancialRecords", FinancialRecordService.DeterministicId($"{item.Id}|{item.Amount}|{item.Currency}|{item.Date}|{item.ReviewState}"), now);
    private static ReviewSourceReference InvoiceSource(FinancialInvoice item, DateTimeOffset now) => new(ReviewSourceKind.Invoice, item.Id, item.InvoiceNumber, "LifeOS.FinancialRecords", FinancialRecordService.DeterministicId($"{item.Id}|{item.Total}|{item.Currency}|{item.DueDate}|{item.AllocatedAmount}|{item.ReviewState}"), now);
    private static ReviewSourceReference PaymentSource(FinancialPayment item, DateTimeOffset now) => new(ReviewSourceKind.Payment, item.Id, item.Id, "LifeOS.FinancialRecords", FinancialRecordService.DeterministicId($"{item.Id}|{item.InvoiceId}|{item.Amount}|{item.Currency}|{item.Date}|{item.AllocatedAmount}|{item.ReviewState}"), now);
    private static void Add(IDictionary<string, ReconciliationCandidate> target, ReconciliationCandidate candidate) => target.TryAdd(candidate.Id, candidate);
}

using System.Globalization;
using System.Text;
using LifeOS.Core.FinancialRecords;

namespace LifeOS.Core.FinancialReview;

public sealed class FinancialReportingService
{
    public FinancialReport Build(FinancialDataset data, IEnumerable<ReconciliationCandidate> candidates, FinancialReportFilter filter, DateTimeOffset generatedUtc, DateTimeOffset? sourceFreshnessUtc = null)
    {
        ArgumentNullException.ThrowIfNull(data); ArgumentNullException.ThrowIfNull(candidates); ArgumentNullException.ThrowIfNull(filter); filter.Validate();
        var transactions = data.Transactions.Where(x => x.Date >= filter.FromDate && x.Date <= filter.ToDate)
            .Where(x => filter.AccountId is null || x.AccountId == filter.AccountId)
            .Where(x => filter.CategoryId is null || x.CategoryId == filter.CategoryId)
            .Where(x => filter.PartyId is null || x.PartyId == filter.PartyId)
            .Where(x => filter.Status is null || x.ReviewState == filter.Status).ToArray();
        var invoices = data.Invoices.Where(x => x.IssueDate >= filter.FromDate && x.IssueDate <= filter.ToDate)
            .Where(x => filter.PartyId is null || x.PartyId == filter.PartyId)
            .Where(x => filter.Status is null || x.ReviewState == filter.Status).ToArray();
        var payments = data.Payments.Where(x => x.Date >= filter.FromDate && x.Date <= filter.ToDate)
            .Where(x => filter.Status is null || x.ReviewState == filter.Status).ToArray();
        var currencies = transactions.Select(x => x.Currency).Concat(invoices.Select(x => x.Currency)).Concat(payments.Select(x => x.Currency)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal);
        var groups = currencies.Select(currency => new CurrencyFinancialSummary(currency,
            transactions.Where(x => x.Currency == currency && x.Direction == TransactionDirection.Income).Sum(x => x.Amount),
            transactions.Where(x => x.Currency == currency && x.Direction == TransactionDirection.Expense).Sum(x => x.Amount),
            invoices.Where(x => x.Currency == currency).Sum(x => x.Outstanding),
            payments.Where(x => x.Currency == currency).Sum(x => x.Amount),
            transactions.Count(x => x.Currency == currency), invoices.Count(x => x.Currency == currency), payments.Count(x => x.Currency == currency))).ToArray();
        var evidence = BuildEvidence(transactions, invoices, payments);
        var unresolved = candidates.Where(x => x.RecordDate >= filter.FromDate && x.RecordDate <= filter.ToDate && x.Status is FinancialReviewStatus.Open or FinancialReviewStatus.Deferred or FinancialReviewStatus.SourceChanged or FinancialReviewStatus.SourceRemoved).ToArray();
        var freshness = sourceFreshnessUtc ?? generatedUtc;
        var stale = generatedUtc - freshness > TimeSpan.FromHours(24);
        return new FinancialReport(generatedUtc, filter, groups, evidence, unresolved, stale, stale ? $"Source snapshot is {(generatedUtc - freshness).TotalHours:N0} hours old." : "Source snapshot is current for this proof session.");
    }

    public FinancialExportPreview Preview(FinancialReport report, FinancialExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(report); ArgumentNullException.ThrowIfNull(request);
        var ext = request.Format == FinancialExportFormat.Csv ? "csv" : "pdf";
        var sections = request.SelectedSections.Count == 0 ? new[] { "summary", "evidence", "review-candidates" } : request.SelectedSections;
        return new($"LifeOS-financial-report-{report.Filter.FromDate:yyyyMMdd}-{report.Filter.ToDate:yyyyMMdd}.{ext}", request.Format == FinancialExportFormat.Csv ? "text/csv" : "application/pdf", request.Destination, request.RedactSensitiveValues, sections, 1024 + report.Evidence.Count * 140L + report.UnresolvedCandidates.Count * 180L, "Derivative export only. Authoritative records are not changed.");
    }

    public FinancialExportResult Export(FinancialReport report, FinancialExportRequest request)
    {
        var preview = Preview(report, request);
        if (!request.Confirmed) return new(false, true, "Export cancelled before confirmation.", Array.Empty<byte>(), preview);
        if (string.IsNullOrWhiteSpace(request.Destination)) return new(false, false, "A destination is required.", Array.Empty<byte>(), preview);
        try
        {
            var bytes = request.Format == FinancialExportFormat.Csv ? Encoding.UTF8.GetBytes(BuildCsv(report, request.RedactSensitiveValues)) : BuildMinimalPdf(BuildSummaryText(report, request.RedactSensitiveValues));
            return new(true, false, "Derivative export generated without mutating authoritative records.", bytes, preview with { EstimatedBytes = bytes.LongLength });
        }
        catch (Exception ex)
        {
            return new(false, false, $"Export failed safely: {ex.GetType().Name}", Array.Empty<byte>(), preview);
        }
    }

    private static IReadOnlyList<EvidenceCompletenessItem> BuildEvidence(IEnumerable<FinancialTransaction> transactions, IEnumerable<FinancialInvoice> invoices, IEnumerable<FinancialPayment> payments)
    {
        var items = new List<EvidenceCompletenessItem>();
        foreach (var x in transactions) items.Add(Evidence("Transaction", x.Id, x.Description, x.Evidence, x.Links));
        foreach (var x in invoices) items.Add(Evidence("Invoice", x.Id, x.InvoiceNumber, x.Evidence, x.Links));
        foreach (var x in payments) items.Add(Evidence("Payment", x.Id, x.Id, x.Evidence, Array.Empty<FinancialLink>()));
        return items;
    }

    private static EvidenceCompletenessItem Evidence(string type, string id, string label, IReadOnlyList<FinancialEvidenceLink> evidence, IReadOnlyList<FinancialLink> links)
    {
        var state = evidence.Count == 0 ? EvidenceCompletenessState.Missing : evidence.Any(x => x.ReviewState == FinancialReviewState.Rejected) ? EvidenceCompletenessState.Conflicting : evidence.Any(x => x.ReviewState == FinancialReviewState.PendingReview) ? EvidenceCompletenessState.PendingReview : EvidenceCompletenessState.Complete;
        return new(type, id, label, state, state switch { EvidenceCompletenessState.Complete => "Accepted preserved evidence is linked.", EvidenceCompletenessState.Missing => "No preserved evidence is linked.", EvidenceCompletenessState.PendingReview => "Evidence exists but still requires review.", _ => "Evidence links conflict and require review." }, links);
    }

    private static string BuildCsv(FinancialReport report, bool redact)
    {
        var sb = new StringBuilder("section,currency,income,expenses,outstanding_invoices,payments_received,count_or_state,label\r\n");
        foreach (var x in report.CurrencyGroups) sb.AppendLine($"summary,{Csv(x.Currency)},{x.Income.ToString(CultureInfo.InvariantCulture)},{x.Expenses.ToString(CultureInfo.InvariantCulture)},{x.OutstandingInvoices.ToString(CultureInfo.InvariantCulture)},{x.PaymentsReceived.ToString(CultureInfo.InvariantCulture)},{x.TransactionCount + x.InvoiceCount + x.PaymentCount},");
        foreach (var x in report.Evidence) sb.AppendLine($"evidence,,,,,,{x.State},{Csv(redact ? Redact(x.Label) : x.Label)}");
        foreach (var x in report.UnresolvedCandidates) sb.AppendLine($"review,{Csv(x.Currency)},,,,,{x.Status},{Csv(redact ? Redact(x.Explanation) : x.Explanation)}");
        return sb.ToString();
    }

    private static string BuildSummaryText(FinancialReport report, bool redact)
    {
        var lines = new List<string> { "LifeOS financial report", $"Period: {report.Filter.FromDate:yyyy-MM-dd} to {report.Filter.ToDate:yyyy-MM-dd}", "Currencies are grouped; no mixed-currency total is presented." };
        lines.AddRange(report.CurrencyGroups.Select(x => $"{x.Currency}: income {x.Income:N2}; expenses {x.Expenses:N2}; outstanding {x.OutstandingInvoices:N2}; payments {x.PaymentsReceived:N2}"));
        lines.Add($"Evidence: complete {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Complete)}, missing {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Missing)}, pending {report.Evidence.Count(x => x.State == EvidenceCompletenessState.PendingReview)}, conflicting {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Conflicting)}");
        lines.Add($"Unresolved review candidates: {report.UnresolvedCandidates.Count}"); lines.Add("Derivative export only. No accounting approval or automatic reconciliation.");
        return string.Join("\n", lines.Select(x => redact ? Redact(x) : x));
    }

    private static byte[] BuildMinimalPdf(string text)
    {
        var safe = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", "").Replace("\n", ") Tj 0 -16 Td (");
        var stream = $"BT /F1 10 Tf 50 760 Td ({safe}) Tj ET";
        var objects = new[] { "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n", "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n", "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >> endobj\n", $"4 0 obj << /Length {Encoding.ASCII.GetByteCount(stream)} >> stream\n{stream}\nendstream endobj\n", "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n" };
        var sb = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int> { 0 };
        foreach (var obj in objects) { offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString())); sb.Append(obj); }
        var xref = Encoding.ASCII.GetByteCount(sb.ToString()); sb.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++) sb.Append(offsets[i].ToString("0000000000", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        sb.Append($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"); return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    private static string Redact(string value) => System.Text.RegularExpressions.Regex.Replace(value, @"\b\d{6,}\b|[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", "[redacted]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}

using System.Globalization;
using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Core.Money;

public sealed class MoneyService
{
    public MoneyDashboardSnapshot BuildSnapshot(DateTimeOffset now)
    {
        var invoices = new[]
        {
            new InvoiceRecord(
                "inv-proof",
                "Fictional Engineering",
                480m,
                DateOnly.FromDateTime(now.AddDays(-5).Date),
                DateOnly.FromDateTime(now.AddDays(2).Date),
                MoneyRecordStatus.Due,
                "NZD",
                ["proof-summary.pdf"]),
            new InvoiceRecord(
                "inv-door",
                "Example Door Systems",
                350m,
                DateOnly.FromDateTime(now.AddDays(-10).Date),
                DateOnly.FromDateTime(now.AddDays(-1).Date),
                MoneyRecordStatus.Received,
                "NZD",
                ["payment-confirmation.png"])
        };

        var transactions = new[]
        {
            new TransactionRecord(
                "txn-income",
                "Fictional client payment",
                350m,
                DateOnly.FromDateTime(now.AddDays(-1).Date),
                "Income",
                MoneyRecordStatus.Received,
                "NZD"),
            new TransactionRecord(
                "txn-expense",
                "Sanitized software expense",
                42.50m,
                DateOnly.FromDateTime(now.AddDays(-2).Date),
                "Software",
                MoneyRecordStatus.NeedsReview,
                "NZD")
        };

        var evidence = new[]
        {
            new EvidenceDraft(
                "ev-receipt",
                "receipt-demo.jpg",
                "Sanitized receipt preview",
                183240,
                "Software",
                "txn-expense",
                EvidenceReviewState.Ready,
                MobileFoundationService.Sha256("receipt-demo"),
                now.AddMinutes(-15)),
            new EvidenceDraft(
                "ev-duplicate",
                "receipt-demo-copy.jpg",
                "Possible duplicate receipt",
                183240,
                "Software",
                null,
                EvidenceReviewState.DuplicateCandidate,
                MobileFoundationService.Sha256("receipt-demo"),
                now.AddMinutes(-10))
        };

        return new MoneyDashboardSnapshot(
            new MoneySummary(
                Income: 830m,
                Expenses: 42.50m,
                OutstandingInvoices: 480m,
                PaymentsReceived: 350m,
                Currency: "NZD"),
            invoices,
            transactions,
            evidence);
    }

    public EvidenceDraft CreateEvidenceDraft(
        string fileName,
        long sizeBytes,
        string category,
        string safePreviewLabel,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(safePreviewLabel);

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        }

        var normalizedName = Path.GetFileName(fileName.Trim());

        return new EvidenceDraft(
            MobileFoundationService.DeterministicId(
                $"{normalizedName}:{sizeBytes}:{category}"),
            normalizedName,
            safePreviewLabel.Trim(),
            sizeBytes,
            category.Trim(),
            null,
            EvidenceReviewState.Draft,
            MobileFoundationService.Sha256(
                $"{normalizedName}:{sizeBytes}"),
            now);
    }

    public EvidenceDraft LinkEvidence(
        EvidenceDraft draft,
        string recordId,
        string category)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordId);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        return draft with
        {
            LinkedRecordId = recordId.Trim(),
            Category = category.Trim(),
            ReviewState = EvidenceReviewState.Linked
        };
    }

    public EvidenceDraft QueueEvidence(EvidenceDraft draft) =>
        draft with { ReviewState = EvidenceReviewState.Queued };

    public bool IsDuplicateCandidate(
        EvidenceDraft left,
        EvidenceDraft right) =>
        left.Id != right.Id &&
        string.Equals(
            left.Sha256,
            right.Sha256,
            StringComparison.OrdinalIgnoreCase);

    public static decimal ParseAmount(
        string text,
        CultureInfo culture)
    {
        if (!decimal.TryParse(
            text,
            NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
            culture,
            out var amount) ||
            amount < 0)
        {
            throw new FormatException("Enter a valid non-negative amount.");
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}

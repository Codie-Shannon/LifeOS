using System.Globalization;
using System.Text.RegularExpressions;

namespace LifeOS.Core.PayLaterInsights;

public enum PayLaterProvider
{
    Afterpay,
    Zip,
    Other
}

public enum PayLaterReviewState
{
    Candidate,
    Confirmed,
    Duplicate,
    Rejected
}

public sealed record PayLaterCandidate(
    string Id,
    PayLaterProvider Provider,
    decimal Total,
    decimal NextDeduction,
    DateOnly? DueDate,
    string SourceReference,
    string Fingerprint,
    PayLaterReviewState State);

public sealed record PayLaterDashboard(
    decimal ConfirmedTotal,
    decimal ExpectedDeductions,
    decimal ExcludedFromSafeMoney,
    int NeedsReview,
    int Duplicates);

public sealed record ReadOnlyMoneyContract(
    string Provider,
    bool CanRead,
    bool CanImport,
    bool CanExport,
    bool CanReconcile,
    bool CanInitiatePayment);

public sealed partial class PayLaterInsightService
{
    [GeneratedRegex(@"(?:total|remaining)\s*[:$]?\s*(?<amount>\d+(?:\.\d{1,2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex TotalRegex();

    [GeneratedRegex(@"(?:next|deduction|instalment)\s*[:$]?\s*(?<amount>\d+(?:\.\d{1,2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex DeductionRegex();

    [GeneratedRegex(@"(?<date>\d{4}-\d{2}-\d{2})")]
    private static partial Regex DateRegex();

    public PayLaterCandidate Parse(
        string subject,
        string body,
        string sourceReference,
        IEnumerable<PayLaterCandidate> existing)
    {
        string combined = $"{subject}\n{body}";
        PayLaterProvider provider = combined.Contains("afterpay", StringComparison.OrdinalIgnoreCase)
            ? PayLaterProvider.Afterpay
            : combined.Contains("zip", StringComparison.OrdinalIgnoreCase)
                ? PayLaterProvider.Zip
                : PayLaterProvider.Other;

        decimal total = ReadAmount(TotalRegex(), combined);
        decimal deduction = ReadAmount(DeductionRegex(), combined);
        DateOnly? due = ReadDate(combined);
        string fingerprint = $"{provider}|{total:0.00}|{deduction:0.00}|{due:yyyy-MM-dd}|{sourceReference}";
        bool duplicate = existing.Any(candidate =>
            string.Equals(candidate.Fingerprint, fingerprint, StringComparison.Ordinal));

        return new PayLaterCandidate(
            Guid.NewGuid().ToString("N"),
            provider,
            total,
            deduction,
            due,
            sourceReference,
            fingerprint,
            duplicate ? PayLaterReviewState.Duplicate : PayLaterReviewState.Candidate);
    }

    public PayLaterCandidate Confirm(PayLaterCandidate candidate) =>
        candidate.State == PayLaterReviewState.Candidate
            ? candidate with { State = PayLaterReviewState.Confirmed }
            : throw new InvalidOperationException("Only a candidate can be confirmed.");

    public PayLaterDashboard Summarize(IEnumerable<PayLaterCandidate> candidates)
    {
        PayLaterCandidate[] items = candidates.ToArray();
        PayLaterCandidate[] confirmed = items
            .Where(candidate => candidate.State == PayLaterReviewState.Confirmed)
            .ToArray();
        decimal expected = confirmed.Sum(candidate => candidate.NextDeduction);

        return new PayLaterDashboard(
            confirmed.Sum(candidate => candidate.Total),
            expected,
            expected,
            items.Count(candidate => candidate.State == PayLaterReviewState.Candidate),
            items.Count(candidate => candidate.State == PayLaterReviewState.Duplicate));
    }

    public void ValidateContract(ReadOnlyMoneyContract contract)
    {
        if (contract.CanReconcile || contract.CanInitiatePayment)
        {
            throw new InvalidOperationException("Money integrations are read/import/export only through product-complete.");
        }
    }

    private static decimal ReadAmount(Regex regex, string value)
    {
        Match match = regex.Match(value);
        return match.Success &&
               decimal.TryParse(match.Groups["amount"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)
            ? amount
            : 0m;
    }

    private static DateOnly? ReadDate(string value)
    {
        Match match = DateRegex().Match(value);
        return match.Success &&
               DateOnly.TryParseExact(match.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : null;
    }
}

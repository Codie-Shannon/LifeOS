using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeOS.Core.FinancialRecords;

public sealed class FinancialRecordService
{
    private static readonly Regex CurrencyPattern = new("^[A-Z]{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public static decimal NormalizeAmount(decimal amount)
    {
        if (amount < 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
    public static string NormalizeCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        var value = currency.Trim().ToUpperInvariant();
        if (!CurrencyPattern.IsMatch(value)) throw new ArgumentException("Currency must be a three-letter ISO-style code.", nameof(currency));
        return value;
    }
    public static void ValidateTransaction(FinancialTransaction item)
    {
        ArgumentNullException.ThrowIfNull(item); NormalizeAmount(item.Amount); NormalizeCurrency(item.Currency);
        if (item.Date == default) throw new ArgumentException("Transaction date is required.");
        if (string.IsNullOrWhiteSpace(item.AccountId) || string.IsNullOrWhiteSpace(item.CategoryId)) throw new ArgumentException("Account and category are required.");
    }
    public static InvoiceState ApplyPaymentState(FinancialInvoice invoice, decimal allocated)
    {
        var value = NormalizeAmount(allocated);
        if (value > invoice.Total) throw new InvalidOperationException("Allocation cannot exceed invoice total.");
        if (value == 0m) return invoice.DueDate < DateOnly.FromDateTime(DateTime.Today) ? InvoiceState.Overdue : InvoiceState.Issued;
        return value == invoice.Total ? InvoiceState.Paid : InvoiceState.PartiallyPaid;
    }
    public static FinancialPayment Allocate(FinancialPayment payment, decimal amount)
    {
        var allocated = NormalizeAmount(amount);
        if (allocated > payment.Amount) throw new InvalidOperationException("Allocation cannot exceed payment amount.");
        return payment with { AllocatedAmount = allocated, State = allocated == payment.Amount ? PaymentState.Allocated : PaymentState.PartiallyAllocated };
    }
    public static FinancialOverview Summarize(FinancialDataset data, string currency)
    {
        var code = NormalizeCurrency(currency);
        decimal income = data.Transactions.Where(x => x.Currency == code && x.Direction == TransactionDirection.Income && x.ReviewState == FinancialReviewState.Confirmed).Sum(x => x.Amount);
        decimal expenses = data.Transactions.Where(x => x.Currency == code && x.Direction == TransactionDirection.Expense && x.ReviewState == FinancialReviewState.Confirmed).Sum(x => x.Amount);
        return new(data.Accounts.Where(x => x.Currency == code && x.ReviewState == FinancialReviewState.Confirmed).Sum(x => x.Balance), income, expenses, data.Invoices.Where(x => x.Currency == code && x.State is InvoiceState.Issued or InvoiceState.Overdue or InvoiceState.PartiallyPaid).Sum(x => x.Outstanding), data.Payments.Where(x => x.Currency == code && x.State is PaymentState.Received or PaymentState.Allocated or PaymentState.PartiallyAllocated).Sum(x => x.Amount), data.Transactions.Count(x => x.Evidence.Count == 0) + data.Invoices.Count(x => x.Evidence.Count == 0), code);
    }
    public static bool IsDuplicateSubmission(IEnumerable<FinancialTransaction> existing, string submissionKey) => existing.Any(x => string.Equals(x.SubmissionKey, submissionKey, StringComparison.Ordinal));
    public static string DeterministicId(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input.Trim())))[..16].ToLowerInvariant();
    public static string Redact(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "[redacted]";
        var compact = Regex.Replace(value, @"\b\d{6,}\b", "[number-redacted]");
        compact = Regex.Replace(compact, @"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}", "[email-redacted]");
        return compact.Length > 120 ? compact[..120] + "…" : compact;
    }
}

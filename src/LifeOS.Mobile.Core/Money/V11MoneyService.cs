using LifeOS.Core.FinancialRecords;
namespace LifeOS.Mobile.Core.Money;
public sealed class V11MoneyService
{
    public FinancialDataset BuildProofData(DateTimeOffset now) => FinancialProofData.Build(now);
    public FinancialOverview BuildOverview(DateTimeOffset now) => FinancialRecordService.Summarize(BuildProofData(now), "NZD");
    public IReadOnlyList<FinancialTransaction> SearchTransactions(DateTimeOffset now, string? query = null, FinancialReviewState? state = null) => BuildProofData(now).Transactions.Where(x => (state is null || x.ReviewState == state) && (string.IsNullOrWhiteSpace(query) || x.Description.Contains(query, StringComparison.OrdinalIgnoreCase) || x.CategoryId.Contains(query, StringComparison.OrdinalIgnoreCase))).OrderByDescending(x => x.Date).ToArray();
    public IReadOnlyList<FinancialInvoice> SearchInvoices(DateTimeOffset now, InvoiceState? state = null) => BuildProofData(now).Invoices.Where(x => state is null || x.State == state).OrderBy(x => x.DueDate).ToArray();
    public FinancialTransaction ConfirmManual(FinancialTransaction draft, IReadOnlyCollection<FinancialTransaction> existing)
    {
        FinancialRecordService.ValidateTransaction(draft);
        if (FinancialRecordService.IsDuplicateSubmission(existing, draft.SubmissionKey)) throw new InvalidOperationException("Duplicate manual submission requires review.");
        return draft with { ReviewState = FinancialReviewState.Confirmed };
    }
}

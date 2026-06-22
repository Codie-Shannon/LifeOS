namespace LifeOS.Core.Money;

public enum IncomeStatus
{
    Earned,
    Invoiced,
    Expected,
    Paid,
    Overdue
}

public sealed class IncomeItem
{
    public required string Source { get; init; }

    public required decimal Amount { get; init; }

    public required IncomeStatus Status { get; init; }

    public DateOnly? EarnedDate { get; init; }

    public DateOnly? InvoiceDate { get; init; }

    public DateOnly? ExpectedPaidDate { get; init; }

    public DateOnly? PaidDate { get; init; }

    public decimal TaxSetAsidePercent { get; init; }

    public string Notes { get; init; } = string.Empty;

    public decimal TaxSetAsideAmount => Math.Round(Amount * (TaxSetAsidePercent / 100m), 2);

    public decimal SafeAfterTax => Math.Round(Amount - TaxSetAsideAmount, 2);
}
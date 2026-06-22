namespace LifeOS.Core.Money;

public sealed class MoneyPressureSummary
{
    public required decimal CurrentBalance { get; init; }

    public required decimal ConfirmedPaidIncome { get; init; }

    public required decimal PendingIncome { get; init; }

    public required decimal BillsDue { get; init; }

    public required decimal DeductionsDue { get; init; }

    public required decimal FoodFuelBuffer { get; init; }

    public required decimal EmergencyBuffer { get; init; }

    public required decimal SafeToSpend { get; init; }

    public required string PressureLabel { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}
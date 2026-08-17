using LifeOS.Core;
using LifeOS.Core.Money;

namespace LifeOS.Shared.Money;

public sealed class MoneyPressureManualInput
{
    public decimal CurrentBalance { get; set; }

    public decimal PaidIncome { get; set; }

    public decimal PendingIncome { get; set; }

    public decimal BillsDue { get; set; }

    public decimal DeductionsDue { get; set; }

    public decimal FoodFuelBuffer { get; set; }

    public decimal EmergencyBuffer { get; set; }

    public MoneyPressureSummary Calculate()
    {
        return MoneyPressureInputService.Calculate(ToDraft(), DateOnly.FromDateTime(DateTime.Today));
    }

    public MoneyPressureDraft ToDraft() => new(
        CurrentBalance,
        PaidIncome,
        PendingIncome,
        BillsDue,
        DeductionsDue,
        FoodFuelBuffer,
        EmergencyBuffer);
}

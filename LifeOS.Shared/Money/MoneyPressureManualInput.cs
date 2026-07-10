using LifeOS.Core;
using LifeOS.Core.Money;

namespace LifeOS.Shared.Money;

public sealed class MoneyPressureManualInput
{
    public decimal CurrentBalance { get; set; } = 120m;

    public decimal PaidIncome { get; set; } = 180m;

    public decimal PendingIncome { get; set; } = 320m;

    public decimal BillsDue { get; set; } = 65m;

    public decimal DeductionsDue { get; set; } = 15m;

    public decimal FoodFuelBuffer { get; set; } = 60m;

    public decimal EmergencyBuffer { get; set; } = 50m;

    public MoneyPressureSummary Calculate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = LifeOSWeek.GetMondayStart(today);
        var weekEnd = weekStart.AddDays(6);

        var income = new List<IncomeItem>();

        if (PaidIncome > 0)
        {
            income.Add(new IncomeItem
            {
                Source = "Manual paid income",
                Amount = PaidIncome,
                Status = IncomeStatus.Paid,
                PaidDate = today,
                TaxSetAsidePercent = 0m,
                Notes = "Manual paid income counted as safe."
            });
        }

        if (PendingIncome > 0)
        {
            income.Add(new IncomeItem
            {
                Source = "Manual pending income",
                Amount = PendingIncome,
                Status = IncomeStatus.Expected,
                ExpectedPaidDate = today.AddDays(5),
                TaxSetAsidePercent = 0m,
                Notes = "Manual pending income shown separately, not counted as safe."
            });
        }

        var moneyEvents = new List<MoneyEvent>();

        if (BillsDue > 0)
        {
            moneyEvents.Add(new MoneyEvent
            {
                Name = "Manual bills due",
                Amount = BillsDue,
                DueDate = today,
                Category = "Bills",
                IsEssential = true,
                Notes = "Manual bills due this week."
            });
        }

        var deductions = new List<DeductionRule>();

        if (DeductionsDue > 0)
        {
            deductions.Add(new DeductionRule
            {
                Name = "Manual deductions",
                Type = DeductionType.FixedAmount,
                Value = DeductionsDue,
                Frequency = "Weekly",
                Notes = "Manual hidden deductions."
            });
        }

        return MoneyPressureCalculator.Calculate(
            currentBalance: CurrentBalance,
            incomeItems: income,
            moneyEvents: moneyEvents,
            deductionRules: deductions,
            foodFuelBuffer: FoodFuelBuffer,
            emergencyBuffer: EmergencyBuffer,
            weekStart: weekStart,
            weekEnd: weekEnd);
    }
}

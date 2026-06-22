using LifeOS.Core.Money;

namespace LifeOS.Shared.Money;

public static class MoneyPressureDemoData
{
    public static MoneyPressureSummary CreateSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(6);

        var income = new List<IncomeItem>
        {
            new()
            {
                Source = "Paid demo income",
                Amount = 180m,
                Status = IncomeStatus.Paid,
                PaidDate = today,
                TaxSetAsidePercent = 20m,
                Notes = "Demo paid income counted as safe after tax."
            },
            new()
            {
                Source = "Pending contractor work",
                Amount = 320m,
                Status = IncomeStatus.Invoiced,
                ExpectedPaidDate = today.AddDays(5),
                TaxSetAsidePercent = 20m,
                Notes = "Pending income shown separately, not counted as safe."
            }
        };

        var moneyEvents = new List<MoneyEvent>
        {
            new()
            {
                Name = "Fuel buffer",
                Amount = 40m,
                DueDate = today,
                Category = "Transport",
                IsEssential = true,
                Notes = "Demo weekly fuel pressure."
            },
            new()
            {
                Name = "Phone payment",
                Amount = 25m,
                DueDate = today.AddDays(2),
                Category = "Bill",
                IsEssential = true,
                Notes = "Demo bill due this week."
            }
        };

        var deductions = new List<DeductionRule>
        {
            new()
            {
                Name = "Custom weekly deduction",
                Type = DeductionType.FixedAmount,
                Value = 15m,
                Frequency = "Weekly",
                Notes = "Demo hidden deduction."
            }
        };

        return MoneyPressureCalculator.Calculate(
            currentBalance: 120m,
            incomeItems: income,
            moneyEvents: moneyEvents,
            deductionRules: deductions,
            foodFuelBuffer: 60m,
            emergencyBuffer: 50m,
            weekStart: weekStart,
            weekEnd: weekEnd);
    }
}
namespace LifeOS.Core.Money;

public static class MoneyPressureCalculator
{
    public static MoneyPressureSummary Calculate(
        decimal currentBalance,
        IEnumerable<IncomeItem> incomeItems,
        IEnumerable<MoneyEvent> moneyEvents,
        IEnumerable<DeductionRule> deductionRules,
        decimal foodFuelBuffer,
        decimal emergencyBuffer,
        DateOnly weekStart,
        DateOnly weekEnd)
    {
        var incomeList = incomeItems.ToList();
        var eventList = moneyEvents.ToList();
        var deductionList = deductionRules.ToList();

        var confirmedPaidIncome = incomeList
            .Where(item => item.Status == IncomeStatus.Paid)
            .Sum(item => item.SafeAfterTax);

        var pendingIncome = incomeList
            .Where(item => item.Status is IncomeStatus.Earned or IncomeStatus.Invoiced or IncomeStatus.Expected or IncomeStatus.Overdue)
            .Sum(item => item.SafeAfterTax);

        var billsDue = eventList
            .Where(item => !item.IsPaid && item.DueDate >= weekStart && item.DueDate <= weekEnd)
            .Sum(item => item.Amount);

        var deductionsDue = deductionList
            .Where(rule => rule.IsActive && rule.Type == DeductionType.FixedAmount)
            .Sum(rule => rule.Value);

        var safeToSpend = Math.Round(
            currentBalance
            + confirmedPaidIncome
            - billsDue
            - deductionsDue
            - foodFuelBuffer
            - emergencyBuffer,
            2);

        var reasons = new List<string>();

        if (safeToSpend < 0)
        {
            reasons.Add("Safe-to-spend is below zero after bills, deductions, food/fuel, and emergency buffer.");
        }
        else if (safeToSpend < 50)
        {
            reasons.Add("Safe-to-spend is low after known commitments.");
        }
        else
        {
            reasons.Add("Safe-to-spend is currently positive after known commitments.");
        }

        if (pendingIncome > 0)
        {
            reasons.Add($"There is {pendingIncome:C} pending income that is not counted as safe by default.");
        }

        if (billsDue > 0)
        {
            reasons.Add($"There is {billsDue:C} in unpaid money events due this week.");
        }

        if (deductionsDue > 0)
        {
            reasons.Add($"There is {deductionsDue:C} in active fixed deductions.");
        }

        var pressureLabel = safeToSpend switch
        {
            < 0 => "Danger",
            < 50 => "High",
            < 150 => "Medium",
            _ => "Calm"
        };

        return new MoneyPressureSummary
        {
            CurrentBalance = currentBalance,
            ConfirmedPaidIncome = confirmedPaidIncome,
            PendingIncome = pendingIncome,
            BillsDue = billsDue,
            DeductionsDue = deductionsDue,
            FoodFuelBuffer = foodFuelBuffer,
            EmergencyBuffer = emergencyBuffer,
            SafeToSpend = safeToSpend,
            PressureLabel = pressureLabel,
            Reasons = reasons
        };
    }
}
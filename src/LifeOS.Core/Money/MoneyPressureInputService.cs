using LifeOS.Core.Forms;

namespace LifeOS.Core.Money;

public sealed record MoneyPressureDraft(
    decimal CurrentBalance,
    decimal PaidIncome,
    decimal PendingIncome,
    decimal BillsDue,
    decimal DeductionsDue,
    decimal FoodFuelBuffer,
    decimal EmergencyBuffer);

public static class MoneyPressureInputService
{
    private const decimal MaximumMagnitude = 1_000_000_000m;

    public static FormValidationResult Validate(MoneyPressureDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Range(issues, "money-current-balance", draft.CurrentBalance, "Current balance", allowNegative: true);
        Range(issues, "money-paid-income", draft.PaidIncome, "Paid income");
        Range(issues, "money-pending-income", draft.PendingIncome, "Pending income");
        Range(issues, "money-bills-due", draft.BillsDue, "Bills due");
        Range(issues, "money-deductions-due", draft.DeductionsDue, "Deductions due");
        Range(issues, "money-food-fuel-buffer", draft.FoodFuelBuffer, "Food and fuel buffer");
        Range(issues, "money-emergency-buffer", draft.EmergencyBuffer, "Emergency buffer");
        return new FormValidationResult(issues);
    }

    public static MoneyPressureSummary Calculate(MoneyPressureDraft draft, DateOnly today)
    {
        FormValidationResult validation = Validate(draft);
        if (!validation.IsValid)
            throw new ArgumentException("The money-pressure snapshot is invalid.", nameof(draft));

        DateOnly weekStart = LifeOSWeek.GetMondayStart(today);
        DateOnly weekEnd = weekStart.AddDays(6);
        List<IncomeItem> income = [];
        if (draft.PaidIncome > 0m)
        {
            income.Add(new IncomeItem
            {
                Source = "Manual paid income",
                Amount = draft.PaidIncome,
                Status = IncomeStatus.Paid,
                PaidDate = today,
                Notes = "User-entered paid income counted as safe."
            });
        }
        if (draft.PendingIncome > 0m)
        {
            income.Add(new IncomeItem
            {
                Source = "Manual pending income",
                Amount = draft.PendingIncome,
                Status = IncomeStatus.Expected,
                ExpectedPaidDate = today.AddDays(5),
                Notes = "User-entered pending income shown separately and not counted as safe."
            });
        }

        MoneyEvent[] events = draft.BillsDue > 0m
            ? [new MoneyEvent
            {
                Name = "Manual bills due",
                Amount = draft.BillsDue,
                DueDate = today,
                Category = "Bills",
                Notes = "User-entered bills due in the current week."
            }]
            : [];
        DeductionRule[] deductions = draft.DeductionsDue > 0m
            ? [new DeductionRule
            {
                Name = "Manual deductions",
                Type = DeductionType.FixedAmount,
                Value = draft.DeductionsDue,
                Frequency = "Weekly",
                Notes = "User-entered fixed deductions."
            }]
            : [];

        return MoneyPressureCalculator.Calculate(
            draft.CurrentBalance,
            income,
            events,
            deductions,
            draft.FoodFuelBuffer,
            draft.EmergencyBuffer,
            weekStart,
            weekEnd);
    }

    private static void Range(
        ICollection<FormFieldIssue> issues,
        string fieldId,
        decimal value,
        string label,
        bool allowNegative = false)
    {
        decimal minimum = allowNegative ? -MaximumMagnitude : 0m;
        if (value < minimum || value > MaximumMagnitude)
        {
            issues.Add(new FormFieldIssue(
                fieldId,
                "amount-range",
                allowNegative
                    ? $"{label} must be between -1,000,000,000 and 1,000,000,000."
                    : $"{label} must be between 0 and 1,000,000,000."));
        }
    }
}

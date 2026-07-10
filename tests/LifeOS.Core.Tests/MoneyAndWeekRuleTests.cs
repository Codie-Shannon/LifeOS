using LifeOS.Core.Money;
using LifeOS.Core.WeeklyCloseOut;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class MoneyAndWeekRuleTests
{
    [Fact]
    public void MondayWeekStartHandlesSunday()
    {
        var sunday = new DateOnly(2026, 7, 12);

        Assert.Equal(new DateOnly(2026, 7, 6), LifeOSWeek.GetMondayStart(sunday));
    }

    [Fact]
    public void PendingIncomeIsNotSafeMoney()
    {
        var summary = MoneyPressureCalculator.Calculate(
            currentBalance: 100m,
            incomeItems:
            [
                new IncomeItem
                {
                    Source = "Paid invoice",
                    Amount = 100m,
                    Status = IncomeStatus.Paid,
                    TaxSetAsidePercent = 20m
                },
                new IncomeItem
                {
                    Source = "Expected invoice",
                    Amount = 1000m,
                    Status = IncomeStatus.Expected,
                    TaxSetAsidePercent = 20m
                }
            ],
            moneyEvents: [],
            deductionRules: [],
            foodFuelBuffer: 50m,
            emergencyBuffer: 20m,
            weekStart: new DateOnly(2026, 7, 6),
            weekEnd: new DateOnly(2026, 7, 12));

        Assert.Equal(80m, summary.ConfirmedPaidIncome);
        Assert.Equal(800m, summary.PendingIncome);
        Assert.Equal(110m, summary.SafeToSpend);
    }

    [Fact]
    public void BillsDueUseActiveWeekAndPaidState()
    {
        var summary = MoneyPressureCalculator.Calculate(
            currentBalance: 500m,
            incomeItems: [],
            moneyEvents:
            [
                new MoneyEvent
                {
                    Name = "Due this week",
                    Amount = 120m,
                    DueDate = new DateOnly(2026, 7, 8)
                },
                new MoneyEvent
                {
                    Name = "Already paid",
                    Amount = 80m,
                    DueDate = new DateOnly(2026, 7, 8),
                    IsPaid = true
                },
                new MoneyEvent
                {
                    Name = "Next week",
                    Amount = 90m,
                    DueDate = new DateOnly(2026, 7, 13)
                }
            ],
            deductionRules:
            [
                new DeductionRule
                {
                    Name = "Fixed deduction",
                    Type = DeductionType.FixedAmount,
                    Value = 40m
                },
                new DeductionRule
                {
                    Name = "Inactive deduction",
                    Type = DeductionType.FixedAmount,
                    Value = 999m,
                    IsActive = false
                }
            ],
            foodFuelBuffer: 50m,
            emergencyBuffer: 30m,
            weekStart: new DateOnly(2026, 7, 6),
            weekEnd: new DateOnly(2026, 7, 12));

        Assert.Equal(120m, summary.BillsDue);
        Assert.Equal(40m, summary.DeductionsDue);
        Assert.Equal(260m, summary.SafeToSpend);
    }

    [Fact]
    public void CurrentWeekCloseOutHandlesSunday()
    {
        var today = new DateOnly(2026, 7, 12);
        var summary = WeeklyCloseOutCalculator.Calculate(
        [
            new WeeklyCloseOutEntry
            {
                WeekStart = new DateOnly(2026, 7, 6),
                WhatGotDone = "Closed the week.",
                StillWaitingOn = "Client reply"
            }
        ],
        today);

        Assert.True(summary.HasCurrentWeekCloseOut);
        Assert.Equal(1, summary.EntriesThisWeek);
        Assert.Equal(1, summary.WaitingOnCount);
    }
}

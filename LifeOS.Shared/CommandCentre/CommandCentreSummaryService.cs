using LifeOS.Core.FollowUps;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.Money;

namespace LifeOS.Shared.CommandCentre;

public static class CommandCentreSummaryService
{
    public static CommandCentreSummary Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var moneyInput = MoneyPressureStorage.Load();
        var moneySummary = moneyInput.Calculate();

        var followUps = FollowUpStorage.Load();
        var followUpSummary = FollowUpCalculator.Calculate(followUps, today);

        var reasons = new List<string>();

        reasons.AddRange(moneySummary.Reasons);
        reasons.AddRange(followUpSummary.Reasons);

        var overallPressureLabel = GetOverallPressureLabel(
            moneySummary.PressureLabel,
            followUpSummary.OverdueCount,
            followUpSummary.NeedsActionCount,
            followUpSummary.MoneyLinkedCount);

        var nextSafestAction = GetNextSafestAction(
            moneySummary,
            followUpSummary);

        return new CommandCentreSummary
        {
            MoneyPressure = moneySummary,
            FollowUps = followUpSummary,
            OverallPressureLabel = overallPressureLabel,
            NextSafestAction = nextSafestAction,
            Reasons = reasons
        };
    }

    private static string GetOverallPressureLabel(
        string moneyPressureLabel,
        int overdueFollowUps,
        int needsActionFollowUps,
        int moneyLinkedFollowUps)
    {
        if (moneyPressureLabel == "Danger" || overdueFollowUps > 0)
        {
            return "Danger";
        }

        if (moneyPressureLabel == "High" || needsActionFollowUps > 0 || moneyLinkedFollowUps >= 2)
        {
            return "High";
        }

        if (moneyPressureLabel == "Medium" || moneyLinkedFollowUps > 0)
        {
            return "Medium";
        }

        return "Calm";
    }

    private static string GetNextSafestAction(
        Core.Money.MoneyPressureSummary money,
        Core.FollowUps.FollowUpSummary followUps)
    {
        if (money.SafeToSpend < 0)
        {
            return "Review money pressure first. Safe-to-spend is below zero, so avoid new spending until bills, buffers, or income are checked.";
        }

        if (followUps.OverdueCount > 0)
        {
            return "Handle overdue follow-ups first. At least one waiting-on item has passed its follow-up date.";
        }

        if (followUps.NeedsActionCount > 0)
        {
            return "Handle follow-ups marked NeedsAction. These are active pressure items.";
        }

        if (followUps.MoneyLinkedCount > 0)
        {
            return "Check money-linked follow-ups. These may affect paid work, contractor income, or scope confirmation.";
        }

        if (money.PendingIncome > 0)
        {
            return "Keep pending income separate from safe money. It is visible, but not counted as safe-to-spend yet.";
        }

        return "No urgent pressure detected. Continue planned work or pick the next project task.";
    }
}
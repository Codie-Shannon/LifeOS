using LifeOS.Core.Agenda;
using LifeOS.Core.FollowUps;
using LifeOS.Core.PayLater;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.Money;
using LifeOS.Shared.PayLater;
using LifeOS.Shared.WeeklyCloseOut;

namespace LifeOS.Shared.CommandCentre;

public static class CommandCentreSummaryService
{
    public static CommandCentreSummary Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var moneySummary = MoneyPressureStorage.Load().Calculate();
        var followUpSummary = FollowUpCalculator.Calculate(FollowUpStorage.Load(), today);
        var agendaSummary = AgendaCalculator.Calculate(AgendaStorage.Load(), today);
        var payLaterSummary = PayLaterCalculator.Calculate(PayLaterStorage.Load(), today);
        var weeklyCloseOutSummary = WeeklyCloseOutCalculator.Calculate(WeeklyCloseOutStorage.Load(), today);

        var reasons = new List<string>();
        reasons.AddRange(moneySummary.Reasons);
        reasons.AddRange(agendaSummary.Reasons);
        reasons.AddRange(payLaterSummary.Reasons);
        reasons.AddRange(weeklyCloseOutSummary.Reasons);
        reasons.AddRange(followUpSummary.Reasons);

        return new CommandCentreSummary
        {
            MoneyPressure = moneySummary,
            FollowUps = followUpSummary,
            Agenda = agendaSummary,
            PayLater = payLaterSummary,
            WeeklyCloseOut = weeklyCloseOutSummary,
            OverallPressureLabel = GetOverallPressureLabel(moneySummary.PressureLabel, agendaSummary, payLaterSummary, followUpSummary, weeklyCloseOutSummary),
            NextSafestAction = GetNextSafestAction(moneySummary, agendaSummary, payLaterSummary, followUpSummary, weeklyCloseOutSummary),
            Reasons = reasons
        };
    }

    private static string GetOverallPressureLabel(string moneyPressureLabel, AgendaSummary agenda, PayLaterSummary payLater, FollowUpSummary followUps, WeeklyCloseOutSummary closeOut)
    {
        if (moneyPressureLabel == "Danger" || agenda.OverdueCount > 0 || payLater.OverdueCount > 0 || followUps.OverdueCount > 0) return "Danger";
        if (moneyPressureLabel == "High" || agenda.HighPressureCount > 0 || payLater.HighPressureCount > 0 || followUps.NeedsActionCount > 0) return "High";
        if (moneyPressureLabel == "Medium" || agenda.DueTodayCount > 0 || payLater.DueThisWeekCount > 0 || followUps.MoneyLinkedCount > 0 || !closeOut.HasCurrentWeekCloseOut) return "Medium";
        return "Calm";
    }

    private static string GetNextSafestAction(Core.Money.MoneyPressureSummary money, AgendaSummary agenda, PayLaterSummary payLater, FollowUpSummary followUps, WeeklyCloseOutSummary closeOut)
    {
        if (money.SafeToSpend < 0) return "Review money pressure first. Safe-to-spend is below zero, so avoid new spending until commitments or income are checked.";
        if (payLater.OverdueCount > 0) return "Handle overdue pay-later obligations first. Deferred pressure has become current pressure.";
        if (agenda.OverdueCount > 0) return "Handle overdue agenda items first. They are now active pressure.";
        if (followUps.OverdueCount > 0) return "Handle overdue follow-ups first. At least one waiting-on item has passed its follow-up date.";
        if (agenda.DueTodayCount > 0) return "Handle today’s agenda commitments before starting new work.";
        if (followUps.NeedsActionCount > 0) return "Handle follow-ups marked NeedsAction. These are active pressure items.";
        if (!closeOut.HasCurrentWeekCloseOut) return "Create or update this week’s close-out so next week’s pressure is visible.";
        if (money.PendingIncome > 0) return "Keep pending income separate from safe money. It is visible, but not counted as safe-to-spend yet.";
        return "No urgent pressure detected. Continue planned work or pick the next project task.";
    }
}

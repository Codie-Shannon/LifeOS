using LifeOS.Core.Agenda;
using LifeOS.Core.FollowUps;
using LifeOS.Core.PayLater;
using LifeOS.Core.ProofTracker;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Core.WorkSessions;
using LifeOS.Core.WorkPipeline;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.Money;
using LifeOS.Shared.PayLater;
using LifeOS.Shared.ProofTracker;
using LifeOS.Shared.WeeklyCloseOut;
using LifeOS.Shared.WorkSessions;
using LifeOS.Shared.WorkPipeline;

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
        var workSessionSummary = WorkSessionCalculator.Calculate(WorkSessionStorage.Load());
        var proofSummary = ProofCalculator.Calculate(ProofStorage.Load(), today);
        var workPipelineSummary = WorkPipelineCalculator.Calculate(WorkPipelineStorage.Load(), today);

        var reasons = new List<string>();
        reasons.AddRange(moneySummary.Reasons);
        reasons.AddRange(agendaSummary.Reasons);
        reasons.AddRange(payLaterSummary.Reasons);
        reasons.AddRange(weeklyCloseOutSummary.Reasons);
        reasons.AddRange(followUpSummary.Reasons);
        reasons.AddRange(workSessionSummary.Reasons);
        reasons.AddRange(proofSummary.Reasons);
        reasons.AddRange(workPipelineSummary.Reasons);

        var overallPressureLabel = GetOverallPressureLabel(moneySummary.PressureLabel, agendaSummary, payLaterSummary, followUpSummary, weeklyCloseOutSummary, workSessionSummary, proofSummary, workPipelineSummary);
        var nextSafestAction = GetNextSafestAction(moneySummary, agendaSummary, payLaterSummary, followUpSummary, weeklyCloseOutSummary, workSessionSummary, proofSummary, workPipelineSummary);
        var snapshot = CommandCentreSnapshotBuilder.Build(today, moneySummary, followUpSummary, agendaSummary, payLaterSummary, weeklyCloseOutSummary, workSessionSummary, proofSummary, workPipelineSummary, overallPressureLabel, nextSafestAction);

        return new CommandCentreSummary
        {
            MoneyPressure = moneySummary,
            FollowUps = followUpSummary,
            Agenda = agendaSummary,
            PayLater = payLaterSummary,
            WeeklyCloseOut = weeklyCloseOutSummary,
            WorkSessions = workSessionSummary,
            ProofTracker = proofSummary,
            WorkPipeline = workPipelineSummary,
            OverallPressureLabel = overallPressureLabel,
            NextSafestAction = nextSafestAction,
            Snapshot = snapshot,
            Reasons = reasons
        };
    }

    private static string GetOverallPressureLabel(string moneyPressureLabel, AgendaSummary agenda, PayLaterSummary payLater, FollowUpSummary followUps, WeeklyCloseOutSummary closeOut, WorkSessionSummary work, ProofSummary proof, WorkPipelineSummary pipeline)
    {
        if (moneyPressureLabel == "Danger" || agenda.OverdueCount > 0 || payLater.OverdueCount > 0 || followUps.OverdueCount > 0 || pipeline.FollowUpsOverdue > 0) return "Danger";
        if (moneyPressureLabel == "High" || agenda.HighPressureCount > 0 || payLater.HighPressureCount > 0 || followUps.NeedsActionCount > 0 || pipeline.BlockedItems > 0 || pipeline.TimesheetsNeeded > 0 || pipeline.InvoicesNeeded > 0 || work.UnpaidBillableValue >= 500m) return "High";
        if (moneyPressureLabel == "Medium" || agenda.DueTodayCount > 0 || payLater.DueThisWeekCount > 0 || followUps.MoneyLinkedCount > 0 || pipeline.WaitingItems > 0 || pipeline.PaymentsExpected > 0 || !closeOut.HasCurrentWeekCloseOut || work.UnpaidBillableValue > 0m || proof.ReadyCount > 0) return "Medium";
        return "Calm";
    }

    private static string GetNextSafestAction(Core.Money.MoneyPressureSummary money, AgendaSummary agenda, PayLaterSummary payLater, FollowUpSummary followUps, WeeklyCloseOutSummary closeOut, WorkSessionSummary work, ProofSummary proof, WorkPipelineSummary pipeline)
    {
        if (money.SafeToSpend < 0) return "Review money pressure first. Safe-to-spend is below zero, so avoid new spending until commitments or income are checked.";
        if (payLater.OverdueCount > 0) return "Handle overdue pay-later obligations first. Deferred pressure has become current pressure.";
        if (agenda.OverdueCount > 0) return "Handle overdue agenda items first. They are now active pressure.";
        if (followUps.OverdueCount > 0) return "Handle overdue follow-ups first. At least one waiting-on item has passed its follow-up date.";
        if (pipeline.FollowUpsOverdue > 0) return "Handle overdue Work Pipeline follow-ups first. Active work or warm leads are past their review date.";
        if (pipeline.BlockedItems > 0) return "Review blocked Work Pipeline items. Decide whether to unblock, follow up, park, or stop spending time there.";
        if (pipeline.TimesheetsNeeded > 0) return "Handle Work Pipeline timesheets. Billable work needs proof before it can turn into safer money.";
        if (pipeline.InvoicesNeeded > 0) return "Handle Work Pipeline invoices. Expected money is not safe money until invoiced and paid.";
        if (work.UnpaidBillableValue > 0m) return "Review unpaid billable work. Work has been tracked but not marked paid yet.";
        if (proof.ReadyCount > 0) return "Review proof items marked Ready. These may be useful for clients, case studies, or portfolio updates.";
        if (agenda.DueTodayCount > 0) return "Handle today’s agenda commitments before starting new work.";
        if (followUps.NeedsActionCount > 0) return "Handle follow-ups marked NeedsAction. These are active pressure items.";
        if (!closeOut.HasCurrentWeekCloseOut) return "Create or update this week’s close-out so next week’s pressure is visible.";
        if (money.PendingIncome > 0) return "Keep pending income separate from safe money. It is visible, but not counted as safe-to-spend yet.";
        return "No urgent pressure detected. Continue planned work or pick the next project task.";
    }
}

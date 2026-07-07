using LifeOS.Core.Agenda;
using LifeOS.Core.CommandCentre;
using LifeOS.Core.FollowUps;
using LifeOS.Core.Money;
using LifeOS.Core.PayLater;
using LifeOS.Core.ProofTracker;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Core.WorkPipeline;
using LifeOS.Core.WorkSessions;

namespace LifeOS.Shared.CommandCentre;

public static class CommandCentreSnapshotBuilder
{
    public static CommandCentreSnapshot Build(
        DateOnly today,
        MoneyPressureSummary money,
        FollowUpSummary followUps,
        AgendaSummary agenda,
        PayLaterSummary payLater,
        WeeklyCloseOutSummary weeklyCloseOut,
        WorkSessionSummary workSessions,
        ProofSummary proof,
        WorkPipelineSummary pipeline,
        string overallPressureLabel,
        string nextSafestAction)
    {
        var signals = new List<CommandCentreSignal>();

        AddPaidWorkSignals(signals, pipeline, workSessions);
        AddFollowUpSignals(signals, followUps, pipeline, today);
        AddBlockedWaitingSignals(signals, pipeline);
        AddAdminMoneySignals(signals, pipeline, workSessions, money, payLater);
        AddProofSignals(signals, proof);
        AddAgendaCloseOutSignals(signals, agenda, weeklyCloseOut);

        var todaySignals = CommandCentreSignalSorter.SortForToday(signals);

        return new CommandCentreSnapshot
        {
            Date = today,
            OverallPressureLabel = overallPressureLabel,
            TopNextAction = todaySignals.FirstOrDefault()?.NextAction ?? nextSafestAction,
            Signals = signals,
            TodaySignals = todaySignals,
            MoneySignals = todaySignals.Where(signal => signal.IsMoneyRelated).ToList(),
            WaitingSignals = todaySignals.Where(signal => signal.IsWaitingState).ToList(),
            ProofSignals = todaySignals.Where(signal => signal.IsProofRelated).ToList(),
            HiddenSignals = signals.Where(signal => !signal.IsTodayVisible).ToList()
        };
    }

    private static void AddPaidWorkSignals(List<CommandCentreSignal> signals, WorkPipelineSummary pipeline, WorkSessionSummary workSessions)
    {
        var movablePaidWork = pipeline.PriorityItems
            .Where(item => item.IsBillable && !item.IsBlocked && !item.IsWaiting && !string.IsNullOrWhiteSpace(item.NextAction))
            .Take(3)
            .ToList();

        foreach (var item in movablePaidWork)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.PaidWorkAvailable,
                Priority = CommandCentreSignalPriority.High,
                Title = $"Move paid work: {item.Title}",
                Detail = $"{item.ClientOrCompany} • {item.Stage} • expected {FormatMoney(item.ExpectedValue ?? 0m)}",
                NextAction = item.NextAction,
                Source = "Work Pipeline",
                RelatedItemId = item.Id.ToString(),
                IsMoneyRelated = true,
                SortWeight = 10
            });
        }

        if (workSessions.UnpaidBillableValue > 0m)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.PaymentExpected,
                Priority = CommandCentreSignalPriority.High,
                Title = "Unpaid billable work exists",
                Detail = $"Tracked unpaid billable value: {FormatMoney(workSessions.UnpaidBillableValue)}.",
                NextAction = "Check whether this needs timesheet, invoice, payment follow-up, or proof capture.",
                Source = "Work Sessions",
                IsMoneyRelated = true,
                SortWeight = 45
            });
        }
    }

    private static void AddFollowUpSignals(List<CommandCentreSignal> signals, FollowUpSummary followUps, WorkPipelineSummary pipeline, DateOnly today)
    {
        if (followUps.OverdueCount > 0 || followUps.DueTodayCount > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.FollowUpDue,
                Priority = followUps.OverdueCount > 0 ? CommandCentreSignalPriority.Critical : CommandCentreSignalPriority.High,
                Title = "Follow-ups need review",
                Detail = $"{followUps.OverdueCount} overdue • {followUps.DueTodayCount} due today.",
                NextAction = "Open Follow-Ups and handle only the follow-ups that are due or overdue.",
                Source = "Follow-Ups",
                RequiresAction = true,
                DueDate = today,
                SortWeight = 20
            });
        }

        foreach (var item in pipeline.DueFollowUps.Take(3))
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.FollowUpDue,
                Priority = item.FollowUpDate.HasValue && item.FollowUpDate.Value < today ? CommandCentreSignalPriority.Critical : CommandCentreSignalPriority.High,
                Title = $"Pipeline follow-up: {item.Title}",
                Detail = $"Due {FormatDate(item.FollowUpDate)} • waiting on {SafeText(item.WaitingOn, "unknown")}",
                NextAction = SafeText(item.NextAction, "Review the pipeline item and decide whether to send, wait, park, or update the follow-up date."),
                Source = "Work Pipeline",
                RelatedItemId = item.Id.ToString(),
                RequiresAction = true,
                DueDate = item.FollowUpDate,
                SortWeight = 21
            });
        }
    }

    private static void AddBlockedWaitingSignals(List<CommandCentreSignal> signals, WorkPipelineSummary pipeline)
    {
        foreach (var item in pipeline.BlockedWork.Take(3))
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.BlockedWork,
                Priority = CommandCentreSignalPriority.High,
                Title = $"Blocked: {item.Title}",
                Detail = SafeText(item.WaitingOn, "Blocked work with no waiting-on owner set."),
                NextAction = SafeText(item.NextAction, "Decide whether to unblock, ask a scope question, park it, or stop spending time here."),
                Source = "Work Pipeline",
                RelatedItemId = item.Id.ToString(),
                IsWaitingState = true,
                SortWeight = 30
            });
        }

        foreach (var item in pipeline.WaitingWork.Take(3))
        {
            var isPassive = item.FollowUpDate.HasValue && item.FollowUpDate.Value > DateOnly.FromDateTime(DateTime.Today);

            signals.Add(new CommandCentreSignal
            {
                Type = isPassive ? CommandCentreSignalType.PassiveWaiting : CommandCentreSignalType.WaitingOnReply,
                Priority = isPassive ? CommandCentreSignalPriority.Low : CommandCentreSignalPriority.Normal,
                Title = $"Waiting: {item.Title}",
                Detail = $"Waiting on {SafeText(item.WaitingOn, "someone/something")} • follow-up {FormatDate(item.FollowUpDate)}",
                NextAction = isPassive ? "Wait. Do not chase yet unless the context changes." : SafeText(item.NextAction, "Review waiting state and set a clear follow-up date."),
                Source = "Work Pipeline",
                RelatedItemId = item.Id.ToString(),
                IsWaitingState = true,
                IsTodayVisible = !isPassive,
                DueDate = item.FollowUpDate,
                SortWeight = 35
            });
        }
    }

    private static void AddAdminMoneySignals(List<CommandCentreSignal> signals, WorkPipelineSummary pipeline, WorkSessionSummary workSessions, MoneyPressureSummary money, PayLaterSummary payLater)
    {
        if (pipeline.TimesheetsNeeded > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.TimesheetNeeded,
                Priority = CommandCentreSignalPriority.High,
                Title = "Timesheets needed",
                Detail = $"{pipeline.TimesheetsNeeded} Work Pipeline item(s) need timesheet/admin capture.",
                NextAction = "Convert completed billable work into a client-safe timesheet description before details fade.",
                Source = "Work Pipeline",
                IsMoneyRelated = true,
                SortWeight = 40
            });
        }

        if (pipeline.InvoicesNeeded > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.InvoiceNeeded,
                Priority = CommandCentreSignalPriority.High,
                Title = "Invoices needed",
                Detail = $"{pipeline.InvoicesNeeded} Work Pipeline item(s) are marked invoice needed.",
                NextAction = "Check timesheet/proof readiness, then prepare the invoice request.",
                Source = "Work Pipeline",
                IsMoneyRelated = true,
                SortWeight = 41
            });
        }

        if (pipeline.PaymentsExpected > 0 || pipeline.ExpectedValueTotal > 0m)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.PaymentExpected,
                Priority = CommandCentreSignalPriority.Normal,
                Title = "Expected money visible",
                Detail = $"{pipeline.PaymentsExpected} payment item(s) expected • pipeline value {FormatMoney(pipeline.ExpectedValueTotal)}.",
                NextAction = "Keep expected money separate from safe money until it lands.",
                Source = "Work Pipeline",
                IsMoneyRelated = true,
                SortWeight = 42
            });
        }

        if (money.SafeToSpend < 0 || money.PressureLabel is "Danger" or "High")
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.MoneyPressure,
                Priority = money.SafeToSpend < 0 ? CommandCentreSignalPriority.Critical : CommandCentreSignalPriority.High,
                Title = "Money pressure",
                Detail = $"Safe-to-spend: {FormatMoney(money.SafeToSpend)} • pending income: {FormatMoney(money.PendingIncome)}.",
                NextAction = "Review safe money before making new spending decisions. Pending income is not safe money.",
                Source = "Money Pressure",
                IsMoneyRelated = true,
                SortWeight = 50
            });
        }

        if (payLater.OverdueCount > 0 || payLater.DueThisWeekCount > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.BillDue,
                Priority = payLater.OverdueCount > 0 ? CommandCentreSignalPriority.Critical : CommandCentreSignalPriority.High,
                Title = "Pay-later pressure",
                Detail = $"{payLater.OverdueCount} overdue • {payLater.DueThisWeekCount} due this week.",
                NextAction = "Check deferred obligations before treating money as available.",
                Source = "Pay Later",
                IsMoneyRelated = true,
                SortWeight = 55
            });
        }
    }

    private static void AddProofSignals(List<CommandCentreSignal> signals, ProofSummary proof)
    {
        if (proof.ReadyCount > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.ProofNeeded,
                Priority = CommandCentreSignalPriority.Normal,
                Title = "Proof ready to review",
                Detail = $"{proof.ReadyCount} proof item(s) are ready.",
                NextAction = "Review proof items and decide whether they support a client update, case study, or timesheet trail.",
                Source = "Proof Tracker",
                IsProofRelated = true,
                SortWeight = 70
            });
        }
    }

    private static void AddAgendaCloseOutSignals(List<CommandCentreSignal> signals, AgendaSummary agenda, WeeklyCloseOutSummary weeklyCloseOut)
    {
        if (agenda.OverdueCount > 0 || agenda.DueTodayCount > 0)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.General,
                Priority = agenda.OverdueCount > 0 ? CommandCentreSignalPriority.High : CommandCentreSignalPriority.Normal,
                Title = "Agenda pressure",
                Detail = $"{agenda.OverdueCount} overdue • {agenda.DueTodayCount} due today.",
                NextAction = "Check agenda commitments before adding new work.",
                Source = "Agenda",
                SortWeight = 80
            });
        }

        if (!weeklyCloseOut.HasCurrentWeekCloseOut)
        {
            signals.Add(new CommandCentreSignal
            {
                Type = CommandCentreSignalType.General,
                Priority = CommandCentreSignalPriority.Low,
                Title = "Weekly close-out missing",
                Detail = "This week does not have a close-out entry yet.",
                NextAction = "Add a close-out when you reach the end of the current work block.",
                Source = "Weekly Close-Out",
                SortWeight = 90
            });
        }
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string FormatDate(DateOnly? date)
    {
        return date?.ToString("yyyy-MM-dd") ?? "not set";
    }

    private static string FormatMoney(decimal value)
    {
        return value.ToString("C0");
    }
}

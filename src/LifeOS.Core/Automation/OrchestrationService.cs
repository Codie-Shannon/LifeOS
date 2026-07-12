using System.Text.Json;

namespace LifeOS.Core.Automation;

public static class OrchestrationService
{
    public static DateTimeOffset? CalculateDue(OrchestrationPlan plan, DateTimeOffset now)
    {
        return plan.ScheduleDefinition.Type switch
        {
            OrchestrationScheduleType.ManualOnly => null,
            OrchestrationScheduleType.OneTimeReviewDate => plan.ScheduleDefinition.OneTimeReviewAt,
            OrchestrationScheduleType.WeeklyReviewDay => NextOrCurrentWeekly(plan.ScheduleDefinition.WeeklyReviewDay ?? DayOfWeek.Monday, now),
            _ => null
        };
    }

    public static OrchestrationOccurrence? EnsureOccurrence(AutomationStoreSnapshot store, OrchestrationPlan plan, DateTimeOffset now)
    {
        if (!plan.IsEnabled) return null;
        var due = CalculateDue(plan, now);
        if (due is null) return null;
        var key = $"{plan.PlanId}:r{plan.CurrentRevision}:{due.Value:yyyyMMdd}";
        var existing = store.OrchestrationOccurrences.SingleOrDefault(x => x.OccurrenceKey == key);
        if (existing is not null) return existing;
        var state = DueState(due.Value, now);
        var occurrence = new OrchestrationOccurrence { OccurrenceKey = key, PlanId = plan.PlanId, PlanRevision = plan.CurrentRevision, OriginalDueAt = due.Value, DueAt = due.Value, DueState = state };
        store.OrchestrationOccurrences.Add(occurrence);
        return occurrence;
    }

    public static OrchestrationRun Start(AutomationStoreSnapshot store, OrchestrationPlan plan, OrchestrationOccurrence occurrence, DateTimeOffset? now = null)
    {
        if (!plan.IsEnabled) throw new InvalidOperationException("The orchestration plan is disabled.");
        if (store.Settings.ExecutionPaused) throw new InvalidOperationException("Global guarded execution is paused.");
        if (occurrence.PlanRevision != plan.CurrentRevision) throw new InvalidOperationException("Plan revision changed; review the occurrence again.");
        if (store.OrchestrationRuns.Any(x => x.OccurrenceId == occurrence.OccurrenceId && x.Status is not OrchestrationRunStatus.Completed and not OrchestrationRunStatus.Cancelled))
            throw new InvalidOperationException("An active run already exists for this due occurrence.");
        var steps = Steps(store, plan.PlanId);
        if (steps.Count == 0) throw new InvalidOperationException("The orchestration has no steps.");
        var blocked = steps.Where(x => !IsExecutable(x)).ToList();
        if (blocked.Count > 0) throw new InvalidOperationException("The orchestration contains blocked or non-executable steps.");
        var time = now ?? DateTimeOffset.UtcNow;
        var run = new OrchestrationRun { PlanId = plan.PlanId, PlanRevision = plan.CurrentRevision, OccurrenceId = occurrence.OccurrenceId, Status = OrchestrationRunStatus.Paused, CurrentStepId = steps[0].StepId, StartedAt = time, PausedAt = time, SourceSnapshot = JsonSerializer.Serialize(plan.SourceItemIds) };
        store.OrchestrationRuns.Add(run);
        foreach (var step in steps) store.OrchestrationStepRuns.Add(new() { RunId = run.RunId, StepId = step.StepId, ExecutionKey = $"{run.RunId}:{step.StepId}" });
        return run;
    }

    public static OrchestrationStepRun PreviewCurrentStep(AutomationStoreSnapshot store, string runId)
    {
        var run = FindRun(store, runId);
        if (run.Status is OrchestrationRunStatus.Completed or OrchestrationRunStatus.Cancelled) throw new InvalidOperationException("The run is closed.");
        var step = CurrentStep(store, run);
        EnsureDependencies(store, run, step);
        if (!IsExecutable(step)) throw new InvalidOperationException("The step is blocked by risk or capability policy.");
        var item = store.InternalItems.SingleOrDefault(x => x.ItemId == step.TargetItemId && x.Module == step.TargetModule) ?? throw new InvalidOperationException("The step target is missing.");
        var after = Apply(item, step, DateTimeOffset.UtcNow);
        var index = store.OrchestrationStepRuns.FindIndex(x => x.RunId == runId && x.StepId == step.StepId);
        var preview = store.OrchestrationStepRuns[index] with { Status = OrchestrationStepStatus.PreviewReady, BeforeSnapshot = JsonSerializer.Serialize(item), ProposedAfterSnapshot = JsonSerializer.Serialize(after) };
        store.OrchestrationStepRuns[index] = preview;
        return preview;
    }

    public static OrchestrationStepRun ConfirmCurrentStep(AutomationStoreSnapshot store, string runId, DateTimeOffset? now = null, bool injectSafeFailure = false)
    {
        if (store.Settings.ExecutionPaused) throw new InvalidOperationException("Global guarded execution is paused.");
        var runIndex = store.OrchestrationRuns.FindIndex(x => x.RunId == runId);
        var run = store.OrchestrationRuns[runIndex];
        var step = CurrentStep(store, run);
        var srIndex = store.OrchestrationStepRuns.FindIndex(x => x.RunId == runId && x.StepId == step.StepId);
        var stepRun = store.OrchestrationStepRuns[srIndex];
        if (stepRun.Status != OrchestrationStepStatus.PreviewReady) throw new InvalidOperationException("Open the exact before/after preview before confirmation.");
        if (store.OrchestrationStepRuns.Any(x => x.ExecutionKey == stepRun.ExecutionKey && x.Status == OrchestrationStepStatus.Succeeded)) throw new InvalidOperationException("This step has already succeeded.");
        EnsureDependencies(store, run, step);
        var time = now ?? DateTimeOffset.UtcNow;
        if (injectSafeFailure)
        {
            var failed = stepRun with { Status = OrchestrationStepStatus.Failed, FailedAt = time, Error = "Controlled fictional failure; no operational mutation occurred." };
            store.OrchestrationStepRuns[srIndex] = failed;
            store.OrchestrationRuns[runIndex] = run with { Status = OrchestrationRunStatus.RecoveryRequired, PausedAt = time, FailureSummary = failed.Error };
            return failed;
        }
        var itemIndex = store.InternalItems.FindIndex(x => x.ItemId == step.TargetItemId && x.Module == step.TargetModule);
        if (itemIndex < 0) throw new InvalidOperationException("The step target is missing.");
        var current = store.InternalItems[itemIndex];
        if (JsonSerializer.Serialize(current) != stepRun.BeforeSnapshot) throw new InvalidOperationException("Target state changed after preview; execution is stale.");
        var after = Apply(current, step, time);
        store.InternalItems[itemIndex] = after;
        var succeeded = stepRun with { Status = OrchestrationStepStatus.Succeeded, StartedAt = time, CompletedAt = time, ActualAfterSnapshot = JsonSerializer.Serialize(after), UndoAvailable = step.IsReversible, Error = string.Empty };
        store.OrchestrationStepRuns[srIndex] = succeeded;
        var next = Steps(store, run.PlanId).FirstOrDefault(x => x.Sequence > step.Sequence);
        store.OrchestrationRuns[runIndex] = next is null
            ? run with { Status = OrchestrationRunStatus.Completed, CurrentStepId = null, CompletedAt = time, PausedAt = null, FailureSummary = string.Empty }
            : run with { Status = OrchestrationRunStatus.Paused, CurrentStepId = next.StepId, PausedAt = time, FailureSummary = string.Empty };
        if (next is null)
        {
            var oi = store.OrchestrationOccurrences.FindIndex(x => x.OccurrenceId == run.OccurrenceId);
            store.OrchestrationOccurrences[oi] = store.OrchestrationOccurrences[oi] with { DueState = OrchestrationDueState.Completed, CompletedAt = time };
        }
        return succeeded;
    }

    public static void RetryFailedStep(AutomationStoreSnapshot store, string runId)
    {
        var ri = store.OrchestrationRuns.FindIndex(x => x.RunId == runId);
        var run = store.OrchestrationRuns[ri];
        if (run.Status != OrchestrationRunStatus.RecoveryRequired) throw new InvalidOperationException("The run does not require recovery.");
        var si = store.OrchestrationStepRuns.FindIndex(x => x.RunId == runId && x.StepId == run.CurrentStepId);
        var sr = store.OrchestrationStepRuns[si];
        if (sr.Status != OrchestrationStepStatus.Failed) throw new InvalidOperationException("The current step is not failed.");
        store.OrchestrationStepRuns[si] = sr with { Status = OrchestrationStepStatus.Pending, RetryCount = sr.RetryCount + 1, Error = string.Empty, FailedAt = null };
        store.OrchestrationRuns[ri] = run with { Status = OrchestrationRunStatus.Paused, PausedAt = DateTimeOffset.UtcNow, FailureSummary = string.Empty };
    }

    public static void SkipOptionalStep(AutomationStoreSnapshot store, string runId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("A skip reason is required.");
        var ri = store.OrchestrationRuns.FindIndex(x => x.RunId == runId); var run = store.OrchestrationRuns[ri]; var step = CurrentStep(store, run);
        if (!step.IsOptional) throw new InvalidOperationException("Only optional steps may be skipped.");
        var si = store.OrchestrationStepRuns.FindIndex(x => x.RunId == runId && x.StepId == step.StepId);
        store.OrchestrationStepRuns[si] = store.OrchestrationStepRuns[si] with { Status = OrchestrationStepStatus.Skipped, Error = reason, CompletedAt = DateTimeOffset.UtcNow };
        MoveNextOrComplete(store, ri, run, step);
    }

    public static void CancelRemaining(AutomationStoreSnapshot store, string runId)
    {
        var ri = store.OrchestrationRuns.FindIndex(x => x.RunId == runId); var run = store.OrchestrationRuns[ri];
        foreach (var i in Enumerable.Range(0, store.OrchestrationStepRuns.Count).Where(i => store.OrchestrationStepRuns[i].RunId == runId && store.OrchestrationStepRuns[i].Status is OrchestrationStepStatus.Pending or OrchestrationStepStatus.PreviewReady or OrchestrationStepStatus.Failed)) store.OrchestrationStepRuns[i] = store.OrchestrationStepRuns[i] with { Status = OrchestrationStepStatus.Cancelled };
        store.OrchestrationRuns[ri] = run with { Status = OrchestrationRunStatus.Cancelled, CancelledAt = DateTimeOffset.UtcNow, CurrentStepId = null };
    }

    public static void RollBackCompleted(AutomationStoreSnapshot store, string runId)
    {
        var completed = store.OrchestrationStepRuns.Where(x => x.RunId == runId && x.Status == OrchestrationStepStatus.Succeeded && x.UndoAvailable).ToList();
        foreach (var sr in completed.AsEnumerable().Reverse())
        {
            var step = store.OrchestrationSteps.Single(x => x.StepId == sr.StepId);
            var after = JsonSerializer.Deserialize<AutomationInternalItem>(sr.ActualAfterSnapshot)!;
            var before = JsonSerializer.Deserialize<AutomationInternalItem>(sr.BeforeSnapshot)!;
            var ii = store.InternalItems.FindIndex(x => x.ItemId == step.TargetItemId && x.Module == step.TargetModule);
            if (ii < 0 || JsonSerializer.Serialize(store.InternalItems[ii]) != JsonSerializer.Serialize(after)) throw new InvalidOperationException("Rollback stopped because current state no longer matches the checkpoint.");
            store.InternalItems[ii] = before;
            var si = store.OrchestrationStepRuns.FindIndex(x => x.StepRunId == sr.StepRunId);
            store.OrchestrationStepRuns[si] = sr with { Status = OrchestrationStepStatus.RolledBack, UndoAvailable = false };
        }
    }

    public static OrchestrationOccurrence Snooze(AutomationStoreSnapshot store, string occurrenceId, DateTimeOffset until, string reason)
    {
        var i = store.OrchestrationOccurrences.FindIndex(x => x.OccurrenceId == occurrenceId); var current = store.OrchestrationOccurrences[i];
        var updated = current with { DueAt = until, SnoozedUntil = until, SnoozeReason = reason, DueState = OrchestrationDueState.Snoozed };
        store.OrchestrationOccurrences[i] = updated; return updated;
    }

    public static bool IsExecutable(OrchestrationStep step) => step.RiskLevel == AutomationRiskLevel.Low && step.IsReversible && step.RequiredCapabilities.All(x => x is AutomationCapability.ReadTrustedLifeOsState or AutomationCapability.ProposeInternalReviewAction or AutomationCapability.ProposeTaskAgendaDraft or AutomationCapability.ExecuteReversibleInternalAction) && step.ActionType is AutomationActionType.AddInternalReviewNote or AutomationActionType.FlagInternalItemForAttention or AutomationActionType.CreateInternalDraftAgendaItem;

    private static AutomationInternalItem Apply(AutomationInternalItem item, OrchestrationStep step, DateTimeOffset now) => step.ActionType switch
    {
        AutomationActionType.AddInternalReviewNote => item with { ReviewNotes = [.. item.ReviewNotes, "Orchestration checkpoint: fictional weekly hygiene review."], Version = item.Version + 1, UpdatedAt = now },
        AutomationActionType.FlagInternalItemForAttention => item with { AttentionFlagged = true, Version = item.Version + 1, UpdatedAt = now },
        AutomationActionType.CreateInternalDraftAgendaItem => item with { DraftAgendaItems = [.. item.DraftAgendaItems, "Draft agenda: review fictional project hygiene"], Version = item.Version + 1, UpdatedAt = now },
        _ => throw new InvalidOperationException("No typed internal handler is allowlisted for this step.")
    };

    private static void EnsureDependencies(AutomationStoreSnapshot store, OrchestrationRun run, OrchestrationStep step)
    {
        foreach (var dependency in step.DependsOnStepIds)
        {
            var sr = store.OrchestrationStepRuns.Single(x => x.RunId == run.RunId && x.StepId == dependency);
            if (sr.Status != OrchestrationStepStatus.Succeeded) throw new InvalidOperationException("A required dependency has not succeeded.");
        }
    }
    private static OrchestrationRun FindRun(AutomationStoreSnapshot store, string runId) => store.OrchestrationRuns.Single(x => x.RunId == runId);
    private static OrchestrationStep CurrentStep(AutomationStoreSnapshot store, OrchestrationRun run) => store.OrchestrationSteps.Single(x => x.StepId == run.CurrentStepId);
    private static List<OrchestrationStep> Steps(AutomationStoreSnapshot store, string planId) => store.OrchestrationSteps.Where(x => x.PlanId == planId).OrderBy(x => x.Sequence).ToList();
    private static OrchestrationDueState DueState(DateTimeOffset due, DateTimeOffset now) => due.Date == now.Date ? OrchestrationDueState.Due : due < now ? OrchestrationDueState.Overdue : OrchestrationDueState.Upcoming;
    private static DateTimeOffset NextOrCurrentWeekly(DayOfWeek day, DateTimeOffset now) { var delta = ((int)day - (int)now.DayOfWeek + 7) % 7; return new DateTimeOffset(now.Date.AddDays(delta), now.Offset); }
    private static void MoveNextOrComplete(AutomationStoreSnapshot store, int runIndex, OrchestrationRun run, OrchestrationStep current) { var next = Steps(store, run.PlanId).FirstOrDefault(x => x.Sequence > current.Sequence); store.OrchestrationRuns[runIndex] = next is null ? run with { Status = OrchestrationRunStatus.Completed, CurrentStepId = null, CompletedAt = DateTimeOffset.UtcNow } : run with { Status = OrchestrationRunStatus.Paused, CurrentStepId = next.StepId, PausedAt = DateTimeOffset.UtcNow }; }
}

namespace LifeOS.Core.Automation;

public static class AutomationHealthService
{
    public static AutomationHealthSummary Derive(AutomationStoreSnapshot store)
    {
        var unresolved = store.Incidents.Count(x => x.Status == AutomationIncidentStatus.Open);
        var recovery = store.OrchestrationRuns.Count(x => x.Status == OrchestrationRunStatus.RecoveryRequired)
            + store.Proposals.Count(x => x.State == AutomationProposalState.ExecutionFailed);
        var failed = store.OrchestrationStepRuns.Count(x => x.Status == OrchestrationStepStatus.Failed)
            + store.Proposals.Count(x => x.State == AutomationProposalState.ExecutionFailed);
        var status = store.EmergencyStop.IsActive ? AutomationHealthStatus.EmergencyStopped
            : recovery > 0 || unresolved > 0 ? AutomationHealthStatus.RecoveryRequired
            : store.Settings.ExecutionPaused ? AutomationHealthStatus.Paused
            : failed > 0 || store.Proposals.Any(x => x.State is AutomationProposalState.Stale or AutomationProposalState.Blocked) ? AutomationHealthStatus.AttentionRequired
            : AutomationHealthStatus.Healthy;
        return new()
        {
            Status = status,
            PendingReview = store.Proposals.Count(x => x.State is AutomationProposalState.NeedsReview or AutomationProposalState.Proposed),
            ApprovedNotExecuted = store.Proposals.Count(x => x.State is AutomationProposalState.ApprovedNotExecuted or AutomationProposalState.ExecutionPreviewReady),
            Due = store.OrchestrationOccurrences.Count(x => x.DueState is OrchestrationDueState.Due or OrchestrationDueState.Overdue),
            Paused = store.OrchestrationRuns.Count(x => x.Status == OrchestrationRunStatus.Paused),
            RecoveryRequired = recovery, Failed = failed,
            Stale = store.Proposals.Count(x => x.State == AutomationProposalState.Stale),
            Blocked = store.Proposals.Count(x => x.State is AutomationProposalState.Blocked or AutomationProposalState.DuplicateSuspected),
            Executed = store.Executions.Count(x => x.Succeeded && x.UndoneAt is null),
            Undone = store.Executions.Count(x => x.UndoneAt is not null),
            RolledBack = store.OrchestrationStepRuns.Count(x => x.Status == OrchestrationStepStatus.RolledBack),
            UnresolvedIncidents = unresolved,
            LastSuccessfulExecution = store.Executions.Where(x => x.Succeeded).Select(x => (DateTimeOffset?)x.ExecutedAt).Concat(store.OrchestrationStepRuns.Where(x => x.Status == OrchestrationStepStatus.Succeeded).Select(x => x.CompletedAt)).Max(),
            LastFailure = store.Incidents.Where(x => x.Status == AutomationIncidentStatus.Open).Select(x => (DateTimeOffset?)x.CreatedAt).Concat(store.OrchestrationStepRuns.Where(x => x.Status == OrchestrationStepStatus.Failed).Select(x => x.FailedAt)).Max(),
            ActiveGlobalGate = store.EmergencyStop.IsActive ? "Emergency Stop" : store.Settings.ExecutionPaused ? "Guarded execution paused" : "Guarded execution available",
            EmergencyStopActive = store.EmergencyStop.IsActive
        };
    }

    public static void EnsureExecutionAllowed(AutomationStoreSnapshot store)
    {
        if (store.EmergencyStop.IsActive) throw new InvalidOperationException("Emergency Stop is active. Internal execution is blocked fail-closed.");
    }

    public static void ActivateEmergencyStop(AutomationStoreSnapshot store, string reason, string confirmationReference, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(confirmationReference)) throw new InvalidOperationException("Explicit reason and confirmation are required.");
        var time = now ?? DateTimeOffset.UtcNow;
        store.EmergencyStop = new() { IsActive = true, ActivatedAt = time, Reason = Sanitize(reason), ConfirmationReference = confirmationReference };
        for (var i = 0; i < store.OrchestrationRuns.Count; i++)
            if (store.OrchestrationRuns[i].Status is OrchestrationRunStatus.InProgress or OrchestrationRunStatus.Preview) store.OrchestrationRuns[i] = store.OrchestrationRuns[i] with { Status = OrchestrationRunStatus.Paused, PausedAt = time };
    }

    public static void ResetEmergencyStop(AutomationStoreSnapshot store, string confirmationReference, DateTimeOffset? now = null)
    {
        if (!store.EmergencyStop.IsActive) throw new InvalidOperationException("Emergency Stop is not active.");
        if (string.IsNullOrWhiteSpace(confirmationReference)) throw new InvalidOperationException("Explicit reset confirmation is required.");
        var time = now ?? DateTimeOffset.UtcNow;
        store.EmergencyStop = store.EmergencyStop with { IsActive = false, ResetAt = time, ConfirmationReference = confirmationReference };
        store.Settings = store.Settings with { ExecutionPaused = true, UpdatedAt = time };
    }

    public static AutomationIncident RecordIncident(AutomationStoreSnapshot store, string scopeType, string scopeId, string reason, string checkpoint, IReadOnlyList<string> options, string? runId = null, string? stepId = null)
    {
        var incident = new AutomationIncident { ScopeType = scopeType, ScopeId = scopeId, RunId = runId, StepId = stepId, SanitizedReason = Sanitize(reason), LastSafeCheckpoint = Sanitize(checkpoint), RecoveryOptions = options.Select(Sanitize).ToList() };
        store.Incidents.Add(incident);
        return incident;
    }

    public static IReadOnlyList<OrchestrationStepRun> PreviewRollback(AutomationStoreSnapshot store, string runId) =>
        store.OrchestrationStepRuns.Where(x => x.RunId == runId && x.Status == OrchestrationStepStatus.Succeeded && x.UndoAvailable)
            .OrderByDescending(x => store.OrchestrationSteps.Single(s => s.StepId == x.StepId).Sequence).ToList();

    public static string CreateSanitizedDiagnosticSummary(AutomationStoreSnapshot store)
    {
        var h = Derive(store);
        return $"Automation health: {h.Status}; gate={h.ActiveGlobalGate}; incidents={h.UnresolvedIncidents}; pending={h.PendingReview}; approved-not-executed={h.ApprovedNotExecuted}; due={h.Due}; paused={h.Paused}; recovery={h.RecoveryRequired}; failed={h.Failed}; stale={h.Stale}; blocked={h.Blocked}; executed={h.Executed}; undone={h.Undone}; rolled-back={h.RolledBack}. No secrets, connector payloads or local paths included.";
    }

    private static string Sanitize(string value) => value.Replace("\r", " ").Replace("\n", " ").Trim();
}

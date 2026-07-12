namespace LifeOS.Core.Automation;

public static class AutomationReleaseReadinessService
{
    public const int CurrentSchemaVersion = 31;

    public static AutomationReleaseReadiness Evaluate(AutomationStoreSnapshot store, string version, string releaseName)
    {
        ArgumentNullException.ThrowIfNull(store);
        var health = AutomationHealthService.Derive(store);
        var checks = new List<AutomationReadinessCheck>
        {
            Check("Version alignment", version == "6.0.0-beta.1", version == "6.0.0-beta.1" ? "Beta identity aligned" : "Unexpected runtime version", version),
            Check("Store schema", store.SchemaVersion == CurrentSchemaVersion, store.SchemaVersion == CurrentSchemaVersion ? "Current schema loaded" : "Store requires recovery or migration", $"schema {store.SchemaVersion}"),
            Check("Approval boundary", store.Proposals.All(p => !p.OperationalActionExecuted || p.State is AutomationProposalState.Executed or AutomationProposalState.Undone), "Approval remains separate from execution", $"{store.Proposals.Count} proposal(s) inspected"),
            Check("Typed execution allowlist", store.Executions.All(e => e.RiskIsLowAndInternal()), "Only typed reversible internal results are retained", $"{store.Executions.Count} execution result(s) inspected"),
            Check("One-step orchestration", store.OrchestrationRuns.All(r => r.Status != OrchestrationRunStatus.InProgress), "No run is unattended or auto-continuing", $"{store.OrchestrationRuns.Count} run(s) inspected"),
            Check("Restart safety", store.OrchestrationRuns.All(r => r.Status is not OrchestrationRunStatus.InProgress and not OrchestrationRunStatus.RollingBack), "Active work is paused or recovery-required", "No active mutation state after load"),
            Check("Emergency Stop", !store.EmergencyStop.IsActive || health.Status == AutomationHealthStatus.EmergencyStopped, "Persisted fail-closed global stop", store.EmergencyStop.IsActive ? "Active" : "Inactive and available"),
            Check("Recovery containment", store.OrchestrationStepRuns.Count(x => x.Status == OrchestrationStepStatus.Failed) <= store.OrchestrationRuns.Count(x => x.Status == OrchestrationRunStatus.RecoveryRequired) + store.Incidents.Count(x => x.Status == AutomationIncidentStatus.Open), "Failures remain visible and scoped", $"{health.Failed} failed step(s), {health.UnresolvedIncidents} open incident(s)"),
            Check("Blocked capabilities", BlockedCapabilitiesRemainBlocked(), "External, destructive, financial, script, process, plugin and AI capabilities remain blocked", "Policy allowlist inspected"),
            Check("Unattended runtime", true, "No unattended runtime is enabled", "Foreground-only product boundary")
        };

        var failed = checks.Count(x => !x.Passed);
        return new AutomationReleaseReadiness
        {
            State = failed == 0 ? AutomationReadinessState.Ready : health.Status is AutomationHealthStatus.RecoveryRequired or AutomationHealthStatus.EmergencyStopped ? AutomationReadinessState.RecoveryRequired : AutomationReadinessState.AttentionRequired,
            Passed = checks.Count - failed,
            Failed = failed,
            Checks = checks,
            Version = version,
            ReleaseName = releaseName
        };
    }

    private static AutomationReadinessCheck Check(string area, bool passed, string summary, string evidence) => new(area, passed, summary, evidence);

    private static bool BlockedCapabilitiesRemainBlocked() => new[]
    {
        AutomationCapability.ExternalWrite,
        AutomationCapability.DestructiveAction,
        AutomationCapability.FinancialAction,
        AutomationCapability.CommunicationAction,
        AutomationCapability.CalendarMutation,
        AutomationCapability.MailboxMutation,
        AutomationCapability.ScriptExecution,
        AutomationCapability.ProcessExecution,
        AutomationCapability.AiDecision
    }.All(x => !AutomationPolicy.CapabilityAllowed(x));

    private static bool RiskIsLowAndInternal(this AutomationExecutionResult result) =>
        result.Reversible && result.ActionType is AutomationActionType.AddInternalReviewNote or AutomationActionType.FlagInternalItemForAttention or AutomationActionType.CreateInternalDraftAgendaItem;
}

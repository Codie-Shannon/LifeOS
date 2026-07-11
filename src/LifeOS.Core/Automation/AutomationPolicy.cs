namespace LifeOS.Core.Automation;

public static class AutomationPolicy
{
    private static readonly HashSet<AutomationActionType> BlockedActions =
    [
        AutomationActionType.SendEmail,
        AutomationActionType.ModifyCalendar,
        AutomationActionType.FinancialMutation,
        AutomationActionType.DestructiveAction,
        AutomationActionType.ExecuteScript,
        AutomationActionType.ExternalWrite
    ];

    private static readonly HashSet<AutomationActionType> ExecutableActions =
    [
        AutomationActionType.ProposeReviewNote
    ];

    public static bool IsBlocked(AutomationActionType action) => BlockedActions.Contains(action);
    public static bool IsExecutable(AutomationActionType action) => ExecutableActions.Contains(action);
    public static bool IsReversible(AutomationActionType action) => action == AutomationActionType.ProposeReviewNote;

    public static AutomationRiskLevel ClassifyRisk(AutomationActionType action) => action switch
    {
        AutomationActionType.DisplaySuggestion => AutomationRiskLevel.Informational,
        AutomationActionType.ProposeReviewNote or
        AutomationActionType.ProposeReviewQueueItem or
        AutomationActionType.ProposeInternalReviewState or
        AutomationActionType.ProposeAttentionFlag or
        AutomationActionType.ProposeEvidenceReviewRequest => AutomationRiskLevel.Low,
        AutomationActionType.ProposeFollowUp or
        AutomationActionType.ProposeWorkPipelineNextAction or
        AutomationActionType.ProposeAgendaTaskDraft => AutomationRiskLevel.Medium,
        AutomationActionType.SendEmail or
        AutomationActionType.ModifyCalendar or
        AutomationActionType.FinancialMutation or
        AutomationActionType.ExternalWrite => AutomationRiskLevel.High,
        AutomationActionType.DestructiveAction or AutomationActionType.ExecuteScript => AutomationRiskLevel.Critical,
        _ => AutomationRiskLevel.High
    };

    public static AutomationExecutionMode ResolveExecutionMode(AutomationRule rule)
    {
        if (!rule.IsEnabled) return AutomationExecutionMode.Disabled;
        if (IsBlocked(rule.ProposedActionType)) return AutomationExecutionMode.BlockedByPolicy;
        return IsExecutable(rule.ProposedActionType) ? AutomationExecutionMode.GuardedInternal : AutomationExecutionMode.ApprovalRequired;
    }

    public static bool CapabilityAllowed(AutomationCapability capability) => capability is
        AutomationCapability.ReadTrustedLifeOsState or
        AutomationCapability.ReadReviewedIntegrationEvidence or
        AutomationCapability.ProposeInternalReviewAction or
        AutomationCapability.ProposeFollowUp or
        AutomationCapability.ProposeWorkPipelineUpdate or
        AutomationCapability.ProposeTaskAgendaDraft or
        AutomationCapability.ProposeEvidenceReviewRequest or
        AutomationCapability.ExecuteReversibleInternalAction;

    public static IReadOnlyList<string> Validate(AutomationRule rule)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(rule.Name)) errors.Add("Rule name is required.");
        if (rule.Conditions.Count == 0) errors.Add("At least one deterministic condition is required.");
        if (string.IsNullOrWhiteSpace(rule.ProposedActionSummary)) errors.Add("Proposed action summary is required.");
        if (string.IsNullOrWhiteSpace(rule.TargetModule)) errors.Add("Target module is required.");
        if (rule.RequestedCapabilities.Any(x => !CapabilityAllowed(x)) && !IsBlocked(rule.ProposedActionType))
            errors.Add("Rule requests a capability that is not allowed by policy.");
        return errors;
    }
}

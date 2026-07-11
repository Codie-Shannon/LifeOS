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

    public static bool IsBlocked(AutomationActionType action) => BlockedActions.Contains(action);

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
        return IsBlocked(rule.ProposedActionType)
            ? AutomationExecutionMode.BlockedByPolicy
            : AutomationExecutionMode.DryRunOnly;
    }

    public static IReadOnlyList<string> Validate(AutomationRule rule)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(rule.RuleId)) errors.Add("Rule ID is required.");
        if (string.IsNullOrWhiteSpace(rule.Name)) errors.Add("Rule name is required.");
        if (rule.Conditions.Count == 0) errors.Add("At least one deterministic condition is required.");
        if (string.IsNullOrWhiteSpace(rule.ProposedActionSummary)) errors.Add("Proposed action summary is required.");
        if (string.IsNullOrWhiteSpace(rule.TargetModule)) errors.Add("Target module is required.");
        if (rule.ApprovalMode is not AutomationApprovalMode.AlwaysRequireApproval and not AutomationApprovalMode.ExecutionBlocked)
            errors.Add("Group 27 rules must always require approval or remain blocked.");
        return errors;
    }

    public static bool CapabilityAllowed(AutomationCapability capability) => capability is
        AutomationCapability.ReadTrustedLifeOsState or
        AutomationCapability.ReadReviewedIntegrationEvidence or
        AutomationCapability.ProposeInternalReviewAction or
        AutomationCapability.ProposeFollowUp or
        AutomationCapability.ProposeWorkPipelineUpdate or
        AutomationCapability.ProposeTaskAgendaDraft or
        AutomationCapability.ProposeEvidenceReviewRequest;
}

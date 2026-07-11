namespace LifeOS.Core.Automation;

public static class AutomationDemoData
{
    public static AutomationStoreSnapshot Create()
    {
        var rules = new List<AutomationRule>
        {
            Rule("follow-up-review", "Follow-up review proposal", AutomationTriggerType.WaitingOnAgeThresholdReached,
                new(AutomationConditionType.AgeExceedsDays, "WaitingDays", "7", "Waiting age exceeds seven days."),
                AutomationActionType.ProposeFollowUp, "Propose creating a Follow-Up draft", "FollowUps"),
            Rule("missing-next-action", "Missing next-action proposal", AutomationTriggerType.WorkItemHasNoNextAction,
                new(AutomationConditionType.TextContains, "NextAction", "MISSING", "Active project has no next action."),
                AutomationActionType.ProposeReviewNote, "Propose adding a review note", "WorkPipeline"),
            Rule("evidence-review", "Evidence-review proposal", AutomationTriggerType.EvidenceReviewRequired,
                new(AutomationConditionType.ReviewStateEquals, "ReviewState", "NeedsReview", "Evidence requires review."),
                AutomationActionType.ProposeEvidenceReviewRequest, "Propose placing evidence in the review queue", "EvidenceVault"),
            Rule("blocked-email", "Blocked overdue invoice email", AutomationTriggerType.PaymentDueDateReached,
                new(AutomationConditionType.PaymentStateEquals, "PaymentState", "Overdue", "Fictional invoice is overdue."),
                AutomationActionType.SendEmail, "Propose sending an overdue invoice email", "ExternalCommunication")
        };

        return new AutomationStoreSnapshot { Rules = rules };
    }

    public static AutomationSourceSnapshot SourceFor(AutomationRule rule) => rule.RuleId switch
    {
        "follow-up-review" => Source("example-work-001", "WorkPipeline", new() { ["WaitingDays"] = "9", ["WaitingOn"] = "Example Client" }),
        "missing-next-action" => Source("example-project-002", "WorkPipeline", new() { ["NextAction"] = "MISSING", ["State"] = "Active" }),
        "evidence-review" => Source("example-evidence-003", "EvidenceVault", new() { ["ReviewState"] = "NeedsReview", ["EvidenceExists"] = "true" }),
        "blocked-email" => Source("example-invoice-004", "Money", new() { ["PaymentState"] = "Overdue", ["Amount"] = "100.00" }),
        _ => Source("example-manual-000", "Review", new() { ["State"] = "Review" })
    };

    private static AutomationRule Rule(string id, string name, AutomationTriggerType trigger, AutomationCondition condition, AutomationActionType action, string summary, string target) => new()
    {
        RuleId = id,
        Name = name,
        Description = "Fictional Group 27 demonstration rule.",
        IsEnabled = false,
        TriggerType = trigger,
        TriggerConfiguration = "Manual dry-run only",
        Conditions = [condition],
        ProposedActionType = action,
        ProposedActionSummary = summary,
        TargetModule = target,
        RiskLevel = AutomationPolicy.ClassifyRisk(action),
        ApprovalMode = AutomationPolicy.IsBlocked(action) ? AutomationApprovalMode.ExecutionBlocked : AutomationApprovalMode.AlwaysRequireApproval,
        ExecutionMode = AutomationExecutionMode.Disabled,
        RequestedCapabilities = CapabilitiesFor(action),
        Notes = "Approval records intent only. Execution remains disabled in v6.0.0-alpha.1."
    };

    private static IReadOnlyList<AutomationCapability> CapabilitiesFor(AutomationActionType action) => action switch
    {
        AutomationActionType.ProposeFollowUp => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeFollowUp],
        AutomationActionType.ProposeReviewNote => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeInternalReviewAction],
        AutomationActionType.ProposeEvidenceReviewRequest => [AutomationCapability.ReadReviewedIntegrationEvidence, AutomationCapability.ProposeEvidenceReviewRequest],
        AutomationActionType.SendEmail => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.CommunicationAction, AutomationCapability.ExternalWrite],
        _ => [AutomationCapability.ReadTrustedLifeOsState]
    };

    private static AutomationSourceSnapshot Source(string id, string module, Dictionary<string, string> fields) => new()
    {
        SourceItemId = id,
        SourceModule = module,
        TrustState = AutomationTrustState.Reviewed,
        Fields = fields
    };
}

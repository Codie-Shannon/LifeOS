namespace LifeOS.Core.Automation;

public static class AutomationDemoData
{
    public static AutomationStoreSnapshot Create()
    {
        var items = new List<AutomationInternalItem>
        {
            new() { ItemId = "example-project-002", Module = "WorkPipeline", Title = "Fictional launch checklist", NextAction = "MISSING", ReviewState = "NeedsReview" },
            new() { ItemId = "example-evidence-003", Module = "EvidenceVault", Title = "Fictional evidence packet", ReviewState = "NeedsReview" }
        };
        var rules = new List<AutomationRule>
        {
            Rule("follow-up-review", "Follow-up draft proposal", AutomationTriggerType.WaitingOnAgeThresholdReached,
                new(AutomationConditionType.AgeExceedsDays, "WaitingDays", "7", "Waiting age exceeds seven days."),
                AutomationActionType.ProposeFollowUp, "Propose creating a Follow-Up draft", "FollowUps", "example-work-001"),
            Rule("missing-next-action", "Missing next-action review note", AutomationTriggerType.WorkItemHasNoNextAction,
                new(AutomationConditionType.TextContains, "NextAction", "MISSING", "Active fictional item has no next action."),
                AutomationActionType.ProposeReviewNote, "Add a reversible internal review note", "WorkPipeline", "example-project-002"),
            Rule("evidence-review", "Evidence-review queue proposal", AutomationTriggerType.EvidenceReviewRequired,
                new(AutomationConditionType.ReviewStateEquals, "ReviewState", "NeedsReview", "Evidence requires review."),
                AutomationActionType.ProposeEvidenceReviewRequest, "Propose placing evidence in the review queue", "EvidenceVault", "example-evidence-003"),
            Rule("blocked-email", "Blocked overdue invoice email", AutomationTriggerType.PaymentDueDateReached,
                new(AutomationConditionType.PaymentStateEquals, "PaymentState", "Overdue", "Fictional invoice is overdue."),
                AutomationActionType.SendEmail, "Propose sending an overdue invoice email", "ExternalCommunication", "example-invoice-004")
        };
        var plan = new OrchestrationPlan
        {
            PlanId = "weekly-project-hygiene",
            Name = "Weekly project hygiene review",
            Description = "Fictional local plan. Due work is queued for explicit review and never executes automatically.",
            IsEnabled = false,
            ScheduleDefinition = new() { Type = OrchestrationScheduleType.WeeklyReviewDay, WeeklyReviewDay = DayOfWeek.Sunday },
            SourceItemIds = ["example-project-002"],
            RiskLevel = AutomationRiskLevel.Low,
            ExecutionMode = AutomationExecutionMode.GuardedInternal
        };
        var steps = new List<OrchestrationStep>
        {
            new() { StepId = "weekly-note", PlanId = plan.PlanId, Sequence = 1, Name = "Add internal review note", ActionType = AutomationActionType.AddInternalReviewNote, TargetModule = "WorkPipeline", TargetItemId = "example-project-002", RequiredCapabilities = [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeInternalReviewAction, AutomationCapability.ExecuteReversibleInternalAction] },
            new() { StepId = "weekly-flag", PlanId = plan.PlanId, Sequence = 2, Name = "Flag fictional item for attention", ActionType = AutomationActionType.FlagInternalItemForAttention, TargetModule = "WorkPipeline", TargetItemId = "example-project-002", RequiredCapabilities = [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeInternalReviewAction, AutomationCapability.ExecuteReversibleInternalAction], DependsOnStepIds = ["weekly-note"] },
            new() { StepId = "weekly-agenda", PlanId = plan.PlanId, Sequence = 3, Name = "Create internal draft agenda item", ActionType = AutomationActionType.CreateInternalDraftAgendaItem, TargetModule = "WorkPipeline", TargetItemId = "example-project-002", RequiredCapabilities = [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeTaskAgendaDraft, AutomationCapability.ExecuteReversibleInternalAction], DependsOnStepIds = ["weekly-flag"], IsOptional = true }
        };
        return new() { Settings = new() { ExecutionPaused = true }, Rules = rules, InternalItems = items, OrchestrationPlans = [plan], OrchestrationSteps = steps };
    }

    public static AutomationSourceSnapshot SourceFor(AutomationRule rule, AutomationStoreSnapshot? store = null)
    {
        var item = store?.InternalItems.FirstOrDefault(x => x.ItemId == rule.TargetItemId);
        return rule.RuleId switch
        {
            "follow-up-review" => Source("example-work-001", "WorkPipeline", new() { ["WaitingDays"] = "9", ["WaitingOn"] = "Example Client", ["Version"] = "1" }),
            "missing-next-action" => Source(item?.ItemId ?? "example-project-002", "WorkPipeline", new()
            {
                ["NextAction"] = item?.NextAction ?? "MISSING", ["State"] = "Active", ["Version"] = (item?.Version ?? 1).ToString()
            }),
            "evidence-review" => Source(item?.ItemId ?? "example-evidence-003", "EvidenceVault", new()
            {
                ["ReviewState"] = item?.ReviewState ?? "NeedsReview", ["EvidenceExists"] = "true", ["Version"] = (item?.Version ?? 1).ToString()
            }),
            "blocked-email" => Source("example-invoice-004", "Money", new() { ["PaymentState"] = "Overdue", ["Amount"] = "100.00", ["Version"] = "1" }),
            _ => Source("example-manual-000", "Review", new() { ["State"] = "Review", ["Version"] = "1" })
        };
    }

    private static AutomationRule Rule(string id, string name, AutomationTriggerType trigger, AutomationCondition condition,
        AutomationActionType action, string summary, string target, string targetItemId) => new()
    {
        RuleId = id,
        Name = name,
        Description = "Fictional Group 28 demonstration rule.",
        IsEnabled = false,
        TriggerType = trigger,
        TriggerConfiguration = "Manual dry-run only",
        Conditions = [condition],
        ProposedActionType = action,
        ProposedActionSummary = summary,
        TargetModule = target,
        TargetItemId = targetItemId,
        RiskLevel = AutomationPolicy.ClassifyRisk(action),
        ApprovalMode = AutomationPolicy.IsBlocked(action) ? AutomationApprovalMode.ExecutionBlocked : AutomationApprovalMode.AlwaysRequireApproval,
        ExecutionMode = AutomationExecutionMode.Disabled,
        RequestedCapabilities = CapabilitiesFor(action),
        Notes = "Approval does not execute. Final preview and explicit confirmation are required in v6.0.0-alpha.3."
    };

    private static IReadOnlyList<AutomationCapability> CapabilitiesFor(AutomationActionType action) => action switch
    {
        AutomationActionType.ProposeFollowUp => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeFollowUp],
        AutomationActionType.ProposeReviewNote => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.ProposeInternalReviewAction, AutomationCapability.ExecuteReversibleInternalAction],
        AutomationActionType.ProposeEvidenceReviewRequest => [AutomationCapability.ReadReviewedIntegrationEvidence, AutomationCapability.ProposeEvidenceReviewRequest],
        AutomationActionType.SendEmail => [AutomationCapability.ReadTrustedLifeOsState, AutomationCapability.CommunicationAction, AutomationCapability.ExternalWrite],
        _ => [AutomationCapability.ReadTrustedLifeOsState]
    };

    private static AutomationSourceSnapshot Source(string id, string module, Dictionary<string, string> fields) => new()
    {
        SourceItemId = id, SourceModule = module, TrustState = AutomationTrustState.Reviewed, Fields = fields
    };
}

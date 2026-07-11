using System.Text.Json;

namespace LifeOS.Core.Automation;

public enum AutomationTriggerType
{
    ManualEvaluation,
    ItemStateChanged,
    FollowUpDateReached,
    PaymentDueDateReached,
    WaitingOnAgeThresholdReached,
    EvidenceReviewRequired,
    ConnectorPreviewAccepted,
    WorkItemHasNoNextAction,
    ManualReviewRequested
}

public enum AutomationConditionType
{
    StateEquals,
    StateDoesNotEqual,
    DateBefore,
    DateAfter,
    AgeExceedsDays,
    AmountExceeds,
    ReviewStateEquals,
    PaymentStateEquals,
    EvidenceExists,
    EvidenceDoesNotExist,
    DuplicateSuspicionPresent,
    TrustStateEquals,
    TargetModuleEquals,
    TextContains,
    WaitingOnPartyEquals,
    RiskLevelEquals
}

public enum AutomationActionType
{
    DisplaySuggestion,
    ProposeFollowUp,
    ProposeWorkPipelineNextAction,
    ProposeReviewNote,
    ProposeReviewQueueItem,
    ProposeInternalReviewState,
    ProposeAttentionFlag,
    ProposeAgendaTaskDraft,
    ProposeEvidenceReviewRequest,
    SendEmail,
    ModifyCalendar,
    FinancialMutation,
    DestructiveAction,
    ExecuteScript,
    ExternalWrite
}

public enum AutomationRiskLevel { Informational, Low, Medium, High, Critical }
public enum AutomationApprovalMode { AlwaysRequireApproval, ApprovalRequiredAboveLowRisk, AdvisoryOnly, ExecutionBlocked }
public enum AutomationExecutionMode { Disabled, DryRunOnly, ApprovalRequired, GuardedInternal, BlockedByPolicy }
public enum AutomationProposalState
{
    Proposed,
    NeedsReview,
    Approved,
    ApprovedNotExecuted,
    ExecutionPreviewReady,
    Executing,
    Executed,
    ExecutionFailed,
    Undone,
    Stale,
    Expired,
    Rejected,
    Blocked,
    DuplicateSuspected,
    Cancelled
}
public enum AutomationTrustState { Untrusted, Reviewed, Trusted }
public enum AutomationCapability
{
    ReadTrustedLifeOsState,
    ReadReviewedIntegrationEvidence,
    ProposeInternalReviewAction,
    ProposeFollowUp,
    ProposeWorkPipelineUpdate,
    ProposeTaskAgendaDraft,
    ProposeEvidenceReviewRequest,
    ExecuteReversibleInternalAction,
    ExternalWrite,
    DestructiveAction,
    FinancialAction,
    CommunicationAction,
    CalendarMutation,
    MailboxMutation,
    ScriptExecution,
    ProcessExecution,
    AiDecision
}

public sealed record AutomationCondition(
    AutomationConditionType Type,
    string Field,
    string ExpectedValue,
    string Description);

public sealed record AutomationRule
{
    public string RuleId { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = "New automation rule";
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public AutomationTriggerType TriggerType { get; init; } = AutomationTriggerType.ManualEvaluation;
    public string TriggerConfiguration { get; init; } = "Manual dry-run only";
    public IReadOnlyList<AutomationCondition> Conditions { get; init; } = [];
    public AutomationActionType ProposedActionType { get; init; } = AutomationActionType.DisplaySuggestion;
    public string ProposedActionSummary { get; init; } = string.Empty;
    public string TargetModule { get; init; } = string.Empty;
    public string? TargetItemId { get; init; }
    public AutomationRiskLevel RiskLevel { get; init; } = AutomationRiskLevel.Low;
    public AutomationApprovalMode ApprovalMode { get; init; } = AutomationApprovalMode.AlwaysRequireApproval;
    public AutomationExecutionMode ExecutionMode { get; init; } = AutomationExecutionMode.Disabled;
    public IReadOnlyList<AutomationCapability> RequestedCapabilities { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastEvaluatedAt { get; init; }
    public DateTimeOffset? LastMatchedAt { get; init; }
    public DateTimeOffset? LastDecisionAt { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset? ArchivedAt { get; init; }
    public int Revision { get; init; } = 1;
}

public sealed record AutomationSourceSnapshot
{
    public required string SourceItemId { get; init; }
    public required string SourceModule { get; init; }
    public AutomationTrustState TrustState { get; init; } = AutomationTrustState.Reviewed;
    public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record AutomationConditionResult(
    AutomationConditionType Type,
    string Field,
    string ExpectedValue,
    string ActualValue,
    bool Passed,
    string SourceItemId,
    AutomationTrustState SourceTrustState,
    string Explanation);

public sealed record AutomationEvaluation
{
    public string EvaluationId { get; init; } = Guid.NewGuid().ToString("N");
    public required string RuleId { get; init; }
    public required int RuleRevision { get; init; }
    public required string RuleSnapshot { get; init; }
    public required IReadOnlyList<string> SourceItemIds { get; init; }
    public required IReadOnlyList<AutomationTrustState> SourceTrustStates { get; init; }
    public required string SourceSnapshotHash { get; init; }
    public DateTimeOffset EvaluatedAt { get; init; } = DateTimeOffset.UtcNow;
    public required AutomationTriggerType Trigger { get; init; }
    public required IReadOnlyList<AutomationConditionResult> ConditionResults { get; init; }
    public required bool Matched { get; init; }
    public required AutomationActionType ProposedActionType { get; init; }
    public required string ProposedActionSummary { get; init; }
    public required string Target { get; init; }
    public required AutomationRiskLevel Risk { get; init; }
    public required AutomationApprovalMode ApprovalPolicy { get; init; }
    public required AutomationExecutionMode ExecutionMode { get; init; }
    public required string Explanation { get; init; }
    public required IReadOnlyList<string> BlockingReasons { get; init; }
    public required string DuplicateProposalKey { get; init; }
    public string? AuditReference { get; init; }
}

public sealed record AutomationProposal
{
    public string ProposalId { get; init; } = Guid.NewGuid().ToString("N");
    public required string EvaluationId { get; init; }
    public required string RuleId { get; init; }
    public required int RuleRevision { get; init; }
    public required string RuleName { get; init; }
    public required string SourceItemId { get; init; }
    public required AutomationTrustState SourceTrustState { get; init; }
    public required string SourceSnapshotHash { get; init; }
    public required AutomationActionType ActionType { get; init; }
    public required string ActionSummary { get; init; }
    public required string Target { get; init; }
    public required AutomationRiskLevel Risk { get; init; }
    public required AutomationExecutionMode ExecutionMode { get; init; }
    public required string DuplicateKey { get; init; }
    public AutomationProposalState State { get; init; } = AutomationProposalState.NeedsReview;
    public string? PriorProposalId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; init; } = DateTimeOffset.UtcNow.AddDays(14);
    public DateTimeOffset? DecisionAt { get; init; }
    public string DecisionReason { get; init; } = string.Empty;
    public bool OperationalActionExecuted { get; init; }
    public string? ExecutionId { get; init; }
}

public sealed record AutomationExecutionSettings
{
    public bool ExecutionPaused { get; init; } = true;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record AutomationInternalItem
{
    public required string ItemId { get; init; }
    public required string Module { get; init; }
    public required string Title { get; init; }
    public string NextAction { get; init; } = string.Empty;
    public string ReviewState { get; init; } = "NeedsReview";
    public List<string> ReviewNotes { get; init; } = [];
    public int Version { get; init; } = 1;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record AutomationEligibilityResult(bool Eligible, IReadOnlyList<string> Checks, IReadOnlyList<string> Blockers);

public sealed record AutomationExecutionPreview
{
    public string PreviewId { get; init; } = Guid.NewGuid().ToString("N");
    public required string ProposalId { get; init; }
    public required string ExactAction { get; init; }
    public required string ExactTarget { get; init; }
    public required string BeforeSnapshot { get; init; }
    public required string ProposedAfterSnapshot { get; init; }
    public required bool Reversible { get; init; }
    public required AutomationRiskLevel Risk { get; init; }
    public required IReadOnlyList<string> PolicyChecks { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record AutomationExecutionResult
{
    public string ExecutionId { get; init; } = Guid.NewGuid().ToString("N");
    public required string ProposalId { get; init; }
    public required string RuleId { get; init; }
    public required int RuleRevision { get; init; }
    public required AutomationActionType ActionType { get; init; }
    public required string TargetModule { get; init; }
    public required string TargetItemId { get; init; }
    public required IReadOnlyList<string> SourceReferences { get; init; }
    public required DateTimeOffset ApprovedAt { get; init; }
    public required DateTimeOffset FinalConfirmedAt { get; init; }
    public required DateTimeOffset ExecutedAt { get; init; }
    public required string BeforeSnapshot { get; init; }
    public required string AfterSnapshot { get; init; }
    public string ExecutorIdentity { get; init; } = "Local user via guarded LifeOS boundary";
    public required bool Succeeded { get; init; }
    public string SanitizedError { get; init; } = string.Empty;
    public required bool Reversible { get; init; }
    public bool UndoAvailable { get; init; }
    public DateTimeOffset? UndoneAt { get; init; }
    public string ExecutionKey { get; init; } = string.Empty;
}

public sealed record AutomationAuditEntry(
    string AuditId,
    DateTimeOffset OccurredAt,
    string EventType,
    string SubjectId,
    string Summary,
    string SourceReference);

public sealed record AutomationStoreSnapshot
{
    public AutomationExecutionSettings Settings { get; init; } = new();
    public List<AutomationRule> Rules { get; init; } = [];
    public List<AutomationEvaluation> Evaluations { get; init; } = [];
    public List<AutomationProposal> Proposals { get; init; } = [];
    public List<AutomationExecutionPreview> Previews { get; init; } = [];
    public List<AutomationExecutionResult> Executions { get; init; } = [];
    public List<AutomationInternalItem> InternalItems { get; init; } = [];
    public List<AutomationAuditEntry> Audit { get; init; } = [];
}

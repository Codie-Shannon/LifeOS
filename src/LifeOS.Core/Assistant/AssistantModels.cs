namespace LifeOS.Core.Assistant;

public enum AssistantSourceType
{
    CommandCentre,
    FollowUps,
    WorkPipeline,
    Timeline,
    Evidence,
    MoneyPressure,
    WaitingOn,
    Projects,
    WorkSessions,
    Timesheets,
    Receipts,
    Relationships,
    Agenda,
    DailyState
}

public enum AssistantStatementKind { Fact, Inference, MissingData, Uncertainty, Conflict, StaleData }
public enum AssistantIntent { General, WaitingOn, ProjectStatus, MoneyAttention, WorkRecorded, WhatChanged, ConflictCheck, MissingEvidence }
public enum AssistantTrustLevel { Summary = 1, Derived = 2, Direct = 3 }

public sealed record AssistantSourcePermission(AssistantSourceType Source, bool Enabled);

public sealed record AssistantConfiguration(
    bool Enabled,
    IReadOnlyList<AssistantSourcePermission> Sources,
    int MaximumRecords = 20,
    int MaximumQuestionCharacters = 500)
{
    public static AssistantConfiguration Disabled { get; } = new(false,
        Enum.GetValues<AssistantSourceType>().Select(source => new AssistantSourcePermission(source, false)).ToArray());

    public bool IsSourceEnabled(AssistantSourceType source) =>
        Sources.Any(permission => permission.Source == source && permission.Enabled);
}

public sealed record AssistantEvidenceRecord(
    string RecordId,
    AssistantSourceType Source,
    string Title,
    string Summary,
    DateTimeOffset Timestamp,
    string Provenance,
    AssistantTrustLevel Trust = AssistantTrustLevel.Direct,
    string? EntityKey = null,
    string? Status = null,
    DateTimeOffset? RelevantDate = null,
    decimal? Amount = null,
    bool IsFictional = false);

public sealed record AssistantRankedRecord(
    AssistantEvidenceRecord Record,
    double Score,
    bool Used,
    string Reason);

public sealed record AssistantConflict(
    string Field,
    AssistantEvidenceRecord First,
    AssistantEvidenceRecord Second,
    string Explanation);

public sealed record AssistantStatement(AssistantStatementKind Kind, string Text);
public sealed record AssistantSuggestion(string Title, string Rationale, string ReviewRoute) { public bool IsExecutable => false; }
public sealed record AssistantRequest(string Question, AssistantConfiguration Configuration, DateTimeOffset? Now = null);

public sealed record AssistantResponse(
    string Answer,
    IReadOnlyList<AssistantEvidenceRecord> SourcesUsed,
    IReadOnlyList<AssistantStatement> Statements,
    string Confidence,
    AssistantSuggestion? Suggestion,
    bool Refused,
    string SafetyBoundary,
    AssistantIntent Intent = AssistantIntent.General,
    IReadOnlyList<AssistantSourceType>? SourcesSearched = null,
    IReadOnlyList<AssistantRankedRecord>? RecordsConsidered = null,
    IReadOnlyList<AssistantConflict>? Conflicts = null,
    IReadOnlyList<AssistantSourceType>? DisabledRelevantSources = null,
    string ConfidenceReason = "");

public interface IAssistantEvidenceSource
{
    AssistantSourceType SourceType { get; }
    IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords);
}

public interface IAssistantAnswerProvider
{
    AssistantResponse Generate(
        string question,
        AssistantIntent intent,
        IReadOnlyList<AssistantRankedRecord> ranked,
        IReadOnlyList<AssistantConflict> conflicts,
        IReadOnlyList<AssistantSourceType> searched,
        IReadOnlyList<AssistantSourceType> disabledRelevant,
        DateTimeOffset now);
}

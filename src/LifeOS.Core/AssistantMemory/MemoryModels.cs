namespace LifeOS.Core.AssistantMemory;

public enum AssistantMemoryType
{
    Preference,
    Fact,
    Constraint,
    Decision,
    RelationshipContext,
    ProjectContext
}

public enum AssistantMemoryScopeType
{
    Global,
    Workspace,
    Project,
    Relationship,
    SessionLimited
}

public enum AssistantMemorySensitivity
{
    Standard,
    PrivateClient,
    Financial,
    Health,
    Legal,
    Relationship,
    SecretProhibited
}

public enum AssistantMemoryStatus
{
    Proposed,
    Active,
    Expired,
    Revoked,
    Deleted
}

public sealed record AssistantMemoryScope(
    AssistantMemoryScopeType Type,
    string? Name = null)
{
    public string Display => string.IsNullOrWhiteSpace(Name) ? Type.ToString() : $"{Type}: {Name}";
}

public sealed record AssistantMemoryOrigin(
    string Kind,
    string SourceId,
    string SourceTitle,
    IReadOnlyList<string> SourceReferences,
    string Provenance);

public sealed record AssistantMemoryRevision(
    Guid RevisionId,
    DateTimeOffset RevisedAt,
    string PreviousTitle,
    string PreviousStatement,
    string RevisedBy,
    string Reason);

public sealed record AssistantMemoryRecord(
    Guid MemoryId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Title,
    string Statement,
    AssistantMemoryType Type,
    AssistantMemoryScope Scope,
    AssistantMemorySensitivity Sensitivity,
    AssistantMemoryStatus Status,
    AssistantMemoryOrigin Origin,
    string ConfirmedBy,
    DateTimeOffset ConfirmedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? ReviewAt,
    DateTimeOffset? LastUsedAt,
    int UsageCount,
    IReadOnlyList<AssistantMemoryRevision> Revisions)
{
    public bool IsRetrievable(DateTimeOffset now) =>
        Status == AssistantMemoryStatus.Active &&
        (ExpiresAt is null || ExpiresAt > now);
}

public sealed record ProposedAssistantMemory(
    Guid ProposalId,
    DateTimeOffset ProposedAt,
    string Title,
    string Statement,
    AssistantMemoryType Type,
    AssistantMemoryScope Scope,
    AssistantMemorySensitivity Sensitivity,
    AssistantMemoryOrigin Origin,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? ReviewAt,
    IReadOnlyList<Guid> DuplicateCandidates,
    IReadOnlyList<Guid> ConflictingCandidates);

public sealed record AssistantMemoryQuery(
    string Text,
    string? Workspace = null,
    string? Project = null,
    string? Relationship = null,
    bool RecordUsage = false,
    int Limit = 5);

public sealed record AssistantMemoryUse(
    AssistantMemoryRecord Memory,
    string MatchReason,
    double Score,
    string Disclosure);

public sealed record AssistantMemoryConflictResolution(
    string CurrentTrustedStatement,
    AssistantMemoryRecord Memory,
    string Result,
    string Reason);

public interface IAssistantMemoryStore
{
    IReadOnlyList<AssistantMemoryRecord> Load();
    void Save(IReadOnlyList<AssistantMemoryRecord> records);
}

public sealed class InMemoryAssistantMemoryStore : IAssistantMemoryStore
{
    private List<AssistantMemoryRecord> _records = [];
    public IReadOnlyList<AssistantMemoryRecord> Load() => _records.ToArray();
    public void Save(IReadOnlyList<AssistantMemoryRecord> records) => _records = records.ToList();
}

using System.Text.Json.Serialization;

namespace LifeOS.Core.IntegrationInbox;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationCandidateType
{
    Message,
    CalendarEvent,
    ContactPerson,
    FileDocument,
    Task,
    FinancialItem,
    GenericProviderRecord
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationCandidateStatus
{
    New,
    NeedsReview,
    DuplicateSuspected,
    Conflict,
    Accepted,
    Rejected,
    Ignored,
    Superseded,
    SourceRemoved
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationCandidateFreshness
{
    Fresh,
    Ageing,
    Stale,
    SourceRemoved
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationDuplicateResolutionChoice
{
    LinkExisting,
    KeepSeparate,
    ReplaceCandidate,
    Ignore,
    Reject
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationConflictFieldChoice
{
    Unreviewed,
    AcceptCandidate,
    KeepCurrent
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationBatchDecision
{
    Accept,
    Reject,
    Ignore
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationReviewAuditAction
{
    CandidateImported,
    CandidateQuarantined,
    ReimportIgnored,
    DuplicateDetected,
    DuplicateResolved,
    ConflictCreated,
    ConflictResolved,
    Accepted,
    Rejected,
    Ignored,
    Superseded,
    Linked,
    BatchPreviewed,
    BatchApplied,
    SourceRemoved
}

public sealed class IntegrationCandidateProvenance
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string AccountDisplayName { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string CapabilityId { get; set; } = string.Empty;
    public string RawReference { get; set; } = string.Empty;
    public DateTimeOffset SourceTimestampUtc { get; set; }
    public DateTimeOffset ImportedTimestampUtc { get; set; }
    public DateTimeOffset LastSeenUtc { get; set; }
    public IntegrationCandidateFreshness Freshness { get; set; }
    public bool IsSourceRemoved { get; set; }
}

public sealed class IntegrationCandidateField
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? CurrentValue { get; set; }
    public bool IsConflict { get; set; }
    public IntegrationConflictFieldChoice ConflictChoice { get; set; }
}

public sealed class LifeOsAuthoritativeLink
{
    public string Module { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset LinkedUtc { get; set; }
}

public sealed class IntegrationCandidate
{
    public string Id { get; set; } = $"candidate-{Guid.NewGuid():N}";
    public IntegrationCandidateType Type { get; set; }
    public IntegrationCandidateStatus Status { get; set; } = IntegrationCandidateStatus.New;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public IntegrationCandidateProvenance Provenance { get; set; } = new();
    public List<IntegrationCandidateField> Fields { get; set; } = [];
    public string Fingerprint { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string? DuplicateOfCandidateId { get; set; }
    public string? ConflictWithRecordId { get; set; }
    public LifeOsAuthoritativeLink? AuthoritativeLink { get; set; }
    public string ReviewNote { get; set; } = string.Empty;
    public bool IsQuarantined { get; set; }
    public string QuarantineReason { get; set; } = string.Empty;
    public bool LowRiskBatchEligible { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class IntegrationReviewAuditEntry
{
    public long Sequence { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string? CandidateId { get; set; }
    public IntegrationReviewAuditAction Action { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public sealed class IntegrationInboxV9State
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public long NextAuditSequence { get; set; } = 1;
    public List<IntegrationCandidate> Candidates { get; set; } = [];
    public List<IntegrationReviewAuditEntry> AuditEntries { get; set; } = [];

    public IntegrationInboxV9State Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        Candidates ??= [];
        AuditEntries ??= [];

        foreach (IntegrationCandidate candidate in Candidates)
        {
            candidate.Id = candidate.Id.Trim();
            candidate.Title = candidate.Title.Trim();
            candidate.Summary = candidate.Summary.Trim();
            candidate.Fingerprint = candidate.Fingerprint.Trim();
            candidate.ContentHash = candidate.ContentHash.Trim();
            candidate.ReviewNote = candidate.ReviewNote.Trim();
            candidate.QuarantineReason = candidate.QuarantineReason.Trim();
            candidate.Provenance ??= new IntegrationCandidateProvenance();
            candidate.Fields ??= [];

            candidate.Provenance.ProviderId = candidate.Provenance.ProviderId.Trim();
            candidate.Provenance.ProviderDisplayName = candidate.Provenance.ProviderDisplayName.Trim();
            candidate.Provenance.AccountId = candidate.Provenance.AccountId.Trim();
            candidate.Provenance.AccountDisplayName = candidate.Provenance.AccountDisplayName.Trim();
            candidate.Provenance.ExternalId = candidate.Provenance.ExternalId.Trim();
            candidate.Provenance.CapabilityId = candidate.Provenance.CapabilityId.Trim();
            candidate.Provenance.RawReference = candidate.Provenance.RawReference.Trim();

            foreach (IntegrationCandidateField field in candidate.Fields)
            {
                field.Key = field.Key.Trim();
                field.DisplayName = field.DisplayName.Trim();
                field.Value = field.Value.Trim();
                field.CurrentValue = field.CurrentValue?.Trim();
            }
        }

        AuditEntries = AuditEntries
            .OrderBy(entry => entry.Sequence)
            .ToList();

        long maximumSequence = AuditEntries.Count == 0
            ? 0
            : AuditEntries.Max(entry => entry.Sequence);

        NextAuditSequence = Math.Max(NextAuditSequence, maximumSequence + 1);
        return this;
    }
}

public sealed record IntegrationInboxV9Summary(
    int Total,
    int ReviewCount,
    int DuplicateCount,
    int ConflictCount,
    int AcceptedCount,
    int SourceRemovedCount,
    int QuarantinedCount);

public sealed record IntegrationBatchReviewPreview(
    IntegrationBatchDecision Decision,
    IReadOnlyList<string> CandidateIds,
    IReadOnlyList<string> CandidateTitles,
    bool CanApply,
    string Summary,
    IReadOnlyList<string> Warnings);

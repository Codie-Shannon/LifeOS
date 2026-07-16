using System.Text.Json.Serialization;

namespace LifeOS.Core.IntegrationControlCentre;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationAccountClassification
{
    Personal,
    Work,
    Business,
    FamilyHousehold,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationConnectionState
{
    NotConnected,
    Connecting,
    Connected,
    NeedsAttention,
    Expired,
    Revoked,
    Offline,
    Disconnected
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationPermissionRequirement
{
    Required,
    Optional
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationPermissionState
{
    Granted,
    Missing,
    Revoked,
    NotRequested
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationFreshnessState
{
    Fresh,
    Ageing,
    Stale,
    Offline,
    Expired,
    Revoked,
    NeverSynchronized
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationCapabilityHealthState
{
    Healthy,
    Ageing,
    Stale,
    NeedsAttention,
    Offline,
    Expired,
    Revoked,
    NotAvailable
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisconnectRetentionChoice
{
    KeepAcceptedLifeOsRecords,
    ArchiveProviderLinks,
    RemoveUnacceptedImportedCandidates
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectorAuditAction
{
    Discovered,
    ConsentReviewed,
    Connected,
    IdentityVerified,
    InitialSyncCompleted,
    RefreshCompleted,
    RefreshFailed,
    PermissionChanged,
    ReconnectStarted,
    Reconnected,
    RevokeReviewed,
    Revoked,
    DisconnectReviewed,
    Disconnected
}

public sealed class IntegrationProviderProfile
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsFictional { get; set; }

    public List<IntegrationProviderCapabilityDefinition> Capabilities { get; set; } = [];
}

public sealed class IntegrationProviderCapabilityDefinition
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPlanned { get; set; } = true;
}

public sealed class ConnectedIntegrationAccount
{
    public string Id { get; set; } = string.Empty;

    public string ProviderId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ProviderIdentity { get; set; } = string.Empty;

    public IntegrationAccountClassification Classification { get; set; }

    public IntegrationConnectionState ConnectionState { get; set; }

    public DateTimeOffset? LastSyncAttemptUtc { get; set; }

    public DateTimeOffset? LastSuccessfulSyncUtc { get; set; }

    public DateTimeOffset? LastErrorUtc { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastErrorMessage { get; set; }

    public DateTimeOffset? NextEligibleSyncUtc { get; set; }

    public List<IntegrationPermissionGrant> Permissions { get; set; } = [];

    public List<IntegrationCapabilityStatus> CapabilityStatuses { get; set; } = [];
}

public sealed class IntegrationPermissionGrant
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string CapabilityId { get; set; } = string.Empty;

    public IntegrationPermissionRequirement Requirement { get; set; }

    public IntegrationPermissionState State { get; set; }

    public DateTimeOffset? ChangedUtc { get; set; }
}

public sealed class IntegrationCapabilityStatus
{
    public string CapabilityId { get; set; } = string.Empty;

    public DateTimeOffset? LastSyncAttemptUtc { get; set; }

    public DateTimeOffset? LastSuccessfulSyncUtc { get; set; }

    public DateTimeOffset? LastErrorUtc { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastErrorMessage { get; set; }

    public DateTimeOffset? NextEligibleSyncUtc { get; set; }

    public TimeSpan FreshFor { get; set; } = TimeSpan.FromHours(2);

    public TimeSpan StaleAfter { get; set; } = TimeSpan.FromHours(8);
}

public sealed class ConnectorAuditEntry
{
    public long Sequence { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }

    public string ProviderId { get; set; } = string.Empty;

    public string? AccountId { get; set; }

    public string? CapabilityId { get; set; }

    public ConnectorAuditAction Action { get; set; }

    public string Summary { get; set; } = string.Empty;

    public bool Succeeded { get; set; }
}

public sealed class IntegrationControlCentreState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public long NextAuditSequence { get; set; } = 1;

    public List<IntegrationProviderProfile> Providers { get; set; } = [];

    public List<ConnectedIntegrationAccount> Accounts { get; set; } = [];

    public List<ConnectorAuditEntry> AuditEntries { get; set; } = [];

    public IntegrationControlCentreState Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        Providers ??= [];
        Accounts ??= [];
        AuditEntries ??= [];

        foreach (IntegrationProviderProfile provider in Providers)
        {
            provider.Id = provider.Id.Trim();
            provider.DisplayName = provider.DisplayName.Trim();
            provider.Description = provider.Description.Trim();
            provider.Capabilities ??= [];
        }

        foreach (ConnectedIntegrationAccount account in Accounts)
        {
            account.Id = account.Id.Trim();
            account.ProviderId = account.ProviderId.Trim();
            account.DisplayName = account.DisplayName.Trim();
            account.ProviderIdentity = account.ProviderIdentity.Trim();
            account.Permissions ??= [];
            account.CapabilityStatuses ??= [];
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

public sealed record ConnectorIdentity(
    string ProviderAccountId,
    string DisplayName,
    string RedactedIdentifier,
    IntegrationAccountClassification Classification);

public sealed record ConnectorCapabilityReadResult(
    string CapabilityId,
    bool Succeeded,
    DateTimeOffset SourceTimestampUtc,
    DateTimeOffset ImportedTimestampUtc,
    int RecordCount,
    string SanitizedMessage);

public sealed record ConnectorOperationResult(
    bool Succeeded,
    string SanitizedMessage,
    DateTimeOffset CompletedUtc);

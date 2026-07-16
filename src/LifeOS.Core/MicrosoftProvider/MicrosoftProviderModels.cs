using System.Text.Json.Serialization;
using LifeOS.Core.IntegrationControlCentre;

namespace LifeOS.Core.MicrosoftProvider;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftProviderCapability
{
    OutlookMail,
    Calendar,
    ContactsPeople,
    OneDrive,
    SharePoint,
    Teams,
    PowerBI,
    PowerAutomate
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftProviderPermissionState
{
    Granted,
    Missing,
    Revoked,
    NotRequested
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftProviderAuditAction
{
    ConfigurationSaved,
    ConsentStarted,
    IdentityVerified,
    InitialMailReadVerified,
    InitialCalendarReadVerified,
    SyncCompleted,
    SyncPartiallyCompleted,
    SyncFailed,
    TokenRefreshed,
    ConsentRevoked,
    Disconnected
}

public sealed class MicrosoftProviderConfiguration
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public string ClientId { get; set; } = string.Empty;
    public string Tenant { get; set; } = "common";
    public int RedirectPort { get; set; } = 53682;
    public int MailLookbackDays { get; set; } = 14;
    public int CalendarLookbackDays { get; set; } = 30;
    public int CalendarLookaheadDays { get; set; } = 90;
    public int MaximumMailItemsPerFolder { get; set; } = 50;
    public int MaximumCalendarItems { get; set; } = 100;
    public List<string> SelectedMailFolderIds { get; set; } = ["inbox"];
    public List<string> SelectedCalendarIds { get; set; } = ["default"];

    [JsonIgnore]
    public string RedirectUri => $"http://localhost:{RedirectPort}/";

    public MicrosoftProviderConfiguration Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        ClientId = ClientId.Trim();
        Tenant = string.IsNullOrWhiteSpace(Tenant) ? "common" : Tenant.Trim();
        RedirectPort = Math.Clamp(RedirectPort, 1024, 65535);
        MailLookbackDays = Math.Clamp(MailLookbackDays, 1, 365);
        CalendarLookbackDays = Math.Clamp(CalendarLookbackDays, 1, 365);
        CalendarLookaheadDays = Math.Clamp(CalendarLookaheadDays, 1, 730);
        MaximumMailItemsPerFolder = Math.Clamp(MaximumMailItemsPerFolder, 1, 200);
        MaximumCalendarItems = Math.Clamp(MaximumCalendarItems, 1, 500);
        SelectedMailFolderIds = (SelectedMailFolderIds ?? ["inbox"])
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        SelectedCalendarIds = (SelectedCalendarIds ?? ["default"])
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (SelectedMailFolderIds.Count == 0)
        {
            SelectedMailFolderIds.Add("inbox");
        }

        if (SelectedCalendarIds.Count == 0)
        {
            SelectedCalendarIds.Add("default");
        }

        return this;
    }

    public IReadOnlyList<string> Validate()
    {
        List<string> errors = [];

        if (!Guid.TryParse(ClientId, out Guid parsedClientId) ||
            parsedClientId == Guid.Empty)
        {
            errors.Add("A valid Microsoft Entra application client ID is required.");
        }

        if (string.IsNullOrWhiteSpace(Tenant))
        {
            errors.Add("Tenant must be common, organizations, consumers or a tenant ID/domain.");
        }

        if (RedirectPort is < 1024 or > 65535)
        {
            errors.Add("Redirect port must be between 1024 and 65535.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Group48RequestedScopes =>
    [
        "openid",
        "profile",
        "offline_access",
        "User.Read",
        "Mail.Read",
        "Calendars.Read"
    ];
}

public sealed class MicrosoftProviderAccount
{
    public string Id { get; set; } = string.Empty;
    public string MicrosoftUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RedactedIdentity { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public IntegrationAccountClassification Classification { get; set; }
    public IntegrationConnectionState ConnectionState { get; set; } =
        IntegrationConnectionState.NotConnected;
    public DateTimeOffset? TokenExpiresUtc { get; set; }
    public DateTimeOffset? LastIdentityVerificationUtc { get; set; }
    public DateTimeOffset? LastMailSyncUtc { get; set; }
    public DateTimeOffset? LastCalendarSyncUtc { get; set; }
    public DateTimeOffset? LastErrorUtc { get; set; }
    public string LastErrorCode { get; set; } = string.Empty;
    public string LastErrorMessage { get; set; } = string.Empty;
    public Dictionary<MicrosoftProviderCapability, MicrosoftProviderPermissionState>
        Permissions { get; set; } = CreateDefaultPermissions();
    public int LastMailCandidateCount { get; set; }
    public int LastCalendarCandidateCount { get; set; }
    public int LastSuggestionCount { get; set; }
    public string MailDeltaLink { get; set; } = string.Empty;
    public string CalendarDeltaLink { get; set; } = string.Empty;

    public MicrosoftProviderAccount Normalize()
    {
        Id = Id.Trim();
        MicrosoftUserId = MicrosoftUserId.Trim();
        DisplayName = DisplayName.Trim();
        RedactedIdentity = RedactedIdentity.Trim();
        TenantId = TenantId.Trim();
        LastErrorCode = LastErrorCode.Trim();
        LastErrorMessage = LastErrorMessage.Trim();
        MailDeltaLink = MailDeltaLink.Trim();
        CalendarDeltaLink = CalendarDeltaLink.Trim();
        Permissions ??= CreateDefaultPermissions();

        foreach (MicrosoftProviderCapability capability in
                 Enum.GetValues<MicrosoftProviderCapability>())
        {
            if (!Permissions.ContainsKey(capability))
            {
                Permissions[capability] =
                    MicrosoftProviderPermissionState.NotRequested;
            }
        }

        return this;
    }

    public static Dictionary<MicrosoftProviderCapability, MicrosoftProviderPermissionState>
        CreateDefaultPermissions() =>
        new()
        {
            [MicrosoftProviderCapability.OutlookMail] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.Calendar] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.ContactsPeople] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.OneDrive] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.SharePoint] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.Teams] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.PowerBI] =
                MicrosoftProviderPermissionState.NotRequested,
            [MicrosoftProviderCapability.PowerAutomate] =
                MicrosoftProviderPermissionState.NotRequested
        };
}

public sealed class MicrosoftProviderAuditEntry
{
    public long Sequence { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string? AccountId { get; set; }
    public MicrosoftProviderAuditAction Action { get; set; }
    public bool Succeeded { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public sealed class MicrosoftProviderState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public long NextAuditSequence { get; set; } = 1;
    public List<MicrosoftProviderAccount> Accounts { get; set; } = [];
    public List<MicrosoftProviderAuditEntry> AuditEntries { get; set; } = [];

    public MicrosoftProviderState Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        Accounts ??= [];
        AuditEntries ??= [];

        foreach (MicrosoftProviderAccount account in Accounts)
        {
            account.Normalize();
        }

        AuditEntries = AuditEntries
            .OrderBy(entry => entry.Sequence)
            .ToList();

        long maximum = AuditEntries.Count == 0
            ? 0
            : AuditEntries.Max(entry => entry.Sequence);

        NextAuditSequence = Math.Max(NextAuditSequence, maximum + 1);
        return this;
    }
}

public sealed record MicrosoftGraphIdentity(
    string Id,
    string DisplayName,
    string UserPrincipalName,
    string Mail,
    string TenantId);

public sealed record MicrosoftAttachmentMetadata(
    string Id,
    string Name,
    string ContentType,
    long Size,
    bool IsInline,
    string AttachmentType);

public sealed record MicrosoftGraphMessage(
    string Id,
    string Subject,
    string Sender,
    string Recipients,
    DateTimeOffset ReceivedUtc,
    DateTimeOffset? SentUtc,
    string Importance,
    bool IsRead,
    string ConversationId,
    bool HasAttachments,
    DateTimeOffset? LastModifiedUtc,
    IReadOnlyList<MicrosoftAttachmentMetadata> Attachments,
    bool IsRemoved);

public sealed record MicrosoftGraphCalendarEvent(
    string Id,
    string Subject,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string OriginalTimeZone,
    string Location,
    string Organizer,
    string Attendees,
    string ResponseState,
    string RecurrenceReference,
    string OnlineMeetingReference,
    DateTimeOffset? LastModifiedUtc,
    bool IsCancelled,
    bool IsRemoved);

public sealed record MicrosoftProviderSyncResult(
    bool Succeeded,
    bool Partial,
    int MailCandidates,
    int CalendarCandidates,
    int Suggestions,
    string SanitizedMessage);

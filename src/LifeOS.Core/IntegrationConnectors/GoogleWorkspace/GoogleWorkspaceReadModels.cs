namespace LifeOS.Core.IntegrationConnectors.GoogleWorkspace;

public sealed record GoogleWorkspaceIdentity(
    string AccountId,
    string DisplayName,
    string RedactedIdentifier,
    string Classification);

public sealed record GoogleGmailRecord(
    string ExternalId,
    string ThreadId,
    string Sender,
    string Recipients,
    string Subject,
    DateTimeOffset TimestampUtc,
    IReadOnlyList<string> Labels,
    bool IsRead,
    bool IsImportant,
    bool IsStarred,
    string AttachmentMetadata);

public sealed record GoogleCalendarRecord(
    string ExternalId,
    string Title,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string TimeZone,
    string Location,
    string Organizer,
    IReadOnlyList<string> Attendees,
    string ResponseState,
    string RecurrenceReference,
    string ConferenceMetadata);

public sealed record GoogleDriveRecord(
    string ExternalId,
    string Name,
    string Extension,
    long SizeBytes,
    DateTimeOffset ModifiedUtc,
    string ParentReference,
    string Owner,
    string ChangeReference);

public sealed record GoogleContactRecord(
    string ExternalId,
    string DisplayName,
    string PrimaryEmail,
    string Phone,
    string Organization);

public sealed record GoogleTaskRecord(
    string ExternalId,
    string ListName,
    string Title,
    DateTimeOffset? DueUtc,
    string Status);

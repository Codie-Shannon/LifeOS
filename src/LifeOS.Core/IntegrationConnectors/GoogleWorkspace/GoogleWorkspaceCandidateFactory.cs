using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors.GoogleWorkspace;

public static class GoogleWorkspaceCandidateFactory
{
    public static MessageCandidateDraft Gmail(
        GoogleWorkspaceIdentity identity,
        GoogleGmailRecord record) => new()
        {
            ProviderId = GoogleWorkspaceProviderConfiguration.ProviderId,
            ProviderDisplayName = "Google Workspace",
            AccountId = identity.AccountId,
            AccountDisplayName = $"{identity.DisplayName} · {identity.Classification}",
            ExternalId = record.ExternalId,
            CapabilityId = "gmail",
            RawReference = "google://gmail/message/redacted",
            SourceTimestampUtc = record.TimestampUtc,
            Summary = "Bounded read-only Gmail message and thread metadata imported for review.",
            Subject = record.Subject,
            Sender = record.Sender,
            Recipients = record.Recipients,
            ConversationId = record.ThreadId,
            Importance = record.IsImportant ? "important" : "normal",
            IsRead = record.IsRead,
            HasAttachments = !string.IsNullOrWhiteSpace(record.AttachmentMetadata),
            AttachmentMetadata = record.AttachmentMetadata
        };

    public static CalendarEventCandidateDraft Calendar(
        GoogleWorkspaceIdentity identity,
        GoogleCalendarRecord record) => new()
        {
            ProviderId = GoogleWorkspaceProviderConfiguration.ProviderId,
            ProviderDisplayName = "Google Workspace",
            AccountId = identity.AccountId,
            AccountDisplayName = $"{identity.DisplayName} · {identity.Classification}",
            ExternalId = record.ExternalId,
            CapabilityId = "google-calendar",
            RawReference = "google://calendar/event/redacted",
            SourceTimestampUtc = record.StartUtc,
            Summary = "Selected-calendar read-only event imported for review.",
            Title = record.Title,
            StartUtc = record.StartUtc,
            EndUtc = record.EndUtc,
            TimeZone = record.TimeZone,
            Location = record.Location,
            Organizer = record.Organizer,
            Attendees = string.Join("; ", record.Attendees),
            ResponseState = record.ResponseState,
            RecurrenceReference = record.RecurrenceReference,
            OnlineMeetingReference = record.ConferenceMetadata
        };

    public static FileDocumentCandidateDraft Drive(
        GoogleWorkspaceIdentity identity,
        GoogleDriveRecord record) => new()
        {
            ProviderId = GoogleWorkspaceProviderConfiguration.ProviderId,
            ProviderDisplayName = "Google Workspace",
            AccountId = identity.AccountId,
            AccountDisplayName = $"{identity.DisplayName} · {identity.Classification}",
            ExternalId = record.ExternalId,
            CapabilityId = "google-drive",
            RawReference = "google://drive/file/redacted",
            SourceTimestampUtc = record.ModifiedUtc,
            Summary = "Selected-folder Google Drive metadata imported without downloading the file body.",
            Name = record.Name,
            Extension = record.Extension,
            SizeBytes = record.SizeBytes,
            ModifiedUtc = record.ModifiedUtc,
            WebReference = "Open source item in Google Drive",
            AdditionalFields = new Dictionary<string, string>
            {
                ["bounded-source"] = record.ParentReference,
                ["owner"] = record.Owner,
                ["change-reference"] = record.ChangeReference,
                ["file-body"] = "Not downloaded"
            }
        };

    public static ContactPersonCandidateDraft Contact(
        GoogleWorkspaceIdentity identity,
        GoogleContactRecord record) => new()
        {
            ProviderId = GoogleWorkspaceProviderConfiguration.ProviderId,
            ProviderDisplayName = "Google Workspace",
            AccountId = identity.AccountId,
            AccountDisplayName = $"{identity.DisplayName} · {identity.Classification}",
            ExternalId = record.ExternalId,
            CapabilityId = "google-contacts",
            RawReference = "google://people/contact/redacted",
            SourceTimestampUtc = DateTimeOffset.UtcNow,
            Summary = "Read-only Google contact imported as a reviewable person candidate.",
            DisplayName = record.DisplayName,
            PrimaryEmail = record.PrimaryEmail,
            Phone = record.Phone,
            Organization = record.Organization
        };

    public static TaskCandidateDraft Task(
        GoogleWorkspaceIdentity identity,
        GoogleTaskRecord record) => new()
        {
            ProviderId = GoogleWorkspaceProviderConfiguration.ProviderId,
            ProviderDisplayName = "Google Workspace",
            AccountId = identity.AccountId,
            AccountDisplayName = $"{identity.DisplayName} · {identity.Classification}",
            ExternalId = record.ExternalId,
            CapabilityId = "google-tasks",
            RawReference = "google://tasks/task/redacted",
            SourceTimestampUtc = record.DueUtc ?? DateTimeOffset.UtcNow,
            Summary = "Selected-list Google task imported read-only for review.",
            Title = record.Title,
            DueUtc = record.DueUtc,
            Status = record.Status,
            Assignee = identity.DisplayName,
            SourceList = record.ListName
        };
}

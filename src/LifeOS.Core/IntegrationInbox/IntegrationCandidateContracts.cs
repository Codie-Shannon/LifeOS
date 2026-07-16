namespace LifeOS.Core.IntegrationInbox;

public abstract class IntegrationCandidateDraftBase
{
    public string ProviderId { get; init; } = string.Empty;
    public string ProviderDisplayName { get; init; } = string.Empty;
    public string AccountId { get; init; } = string.Empty;
    public string AccountDisplayName { get; init; } = string.Empty;
    public string ExternalId { get; init; } = string.Empty;
    public string CapabilityId { get; init; } = string.Empty;
    public string RawReference { get; init; } = string.Empty;
    public DateTimeOffset SourceTimestampUtc { get; init; }
    public string Summary { get; init; } = string.Empty;
    public bool LowRiskBatchEligible { get; init; }
}

public sealed class MessageCandidateDraft : IntegrationCandidateDraftBase
{
    public string Subject { get; init; } = string.Empty;
    public string Sender { get; init; } = string.Empty;
    public string Recipients { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string Importance { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public bool HasAttachments { get; init; }
    public string AttachmentMetadata { get; init; } = string.Empty;
    public DateTimeOffset? LastModifiedUtc { get; init; }
}

public sealed class CalendarEventCandidateDraft : IntegrationCandidateDraftBase
{
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset EndUtc { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Organizer { get; init; } = string.Empty;
    public string Attendees { get; init; } = string.Empty;
    public string RecurrenceReference { get; init; } = string.Empty;
    public string ResponseState { get; init; } = string.Empty;
    public string OnlineMeetingReference { get; init; } = string.Empty;
    public DateTimeOffset? LastModifiedUtc { get; init; }
    public bool IsCancelled { get; init; }
}

public sealed class ContactPersonCandidateDraft : IntegrationCandidateDraftBase
{
    public string DisplayName { get; init; } = string.Empty;
    public string PrimaryEmail { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
}

public sealed class FileDocumentCandidateDraft : IntegrationCandidateDraftBase
{
    public string Name { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public DateTimeOffset ModifiedUtc { get; init; }
    public string WebReference { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> AdditionalFields { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class TaskCandidateDraft : IntegrationCandidateDraftBase
{
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset? DueUtc { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Assignee { get; init; } = string.Empty;
    public string SourceList { get; init; } = string.Empty;
}

public sealed class FinancialItemCandidateDraft : IntegrationCandidateDraftBase
{
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "NZD";
    public DateTimeOffset OccurredUtc { get; init; }
    public string Counterparty { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
}

public sealed class GenericProviderRecordCandidateDraft : IntegrationCandidateDraftBase
{
    public string RecordType { get; init; } = string.Empty;
    public string DisplayValue { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> AdditionalFields { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

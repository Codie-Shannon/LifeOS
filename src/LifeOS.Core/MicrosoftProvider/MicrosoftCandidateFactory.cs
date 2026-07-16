using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public static class MicrosoftCandidateFactory
{
    public static MessageCandidateDraft CreateMessageDraft(
        MicrosoftProviderAccount account,
        MicrosoftGraphMessage message)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(message);

        string attachmentMetadata = message.Attachments.Count == 0
            ? "None"
            : string.Join(
                "; ",
                message.Attachments.Select(attachment =>
                    $"{attachment.Name} ({attachment.ContentType}, " +
                    $"{attachment.Size} bytes" +
                    $"{(attachment.IsInline ? ", inline" : string.Empty)})"));

        return new MessageCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = message.Id,
            CapabilityId = "outlook-mail",
            RawReference =
                $"microsoft-graph://me/messages/{message.Id}",
            SourceTimestampUtc = message.ReceivedUtc,
            Summary =
                "Source-backed Outlook message imported read-only. " +
                "It remains untrusted until reviewed.",
            Subject = string.IsNullOrWhiteSpace(message.Subject)
                ? "(No subject)"
                : message.Subject,
            Sender = string.IsNullOrWhiteSpace(message.Sender)
                ? "Unknown sender"
                : message.Sender,
            Recipients = message.Recipients,
            ConversationId = message.ConversationId,
            Importance = message.Importance,
            IsRead = message.IsRead,
            HasAttachments = message.HasAttachments,
            AttachmentMetadata = attachmentMetadata,
            LastModifiedUtc = message.LastModifiedUtc
        };
    }

    public static CalendarEventCandidateDraft CreateCalendarDraft(
        MicrosoftProviderAccount account,
        MicrosoftGraphCalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(calendarEvent);

        return new CalendarEventCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = calendarEvent.Id,
            CapabilityId = "calendar",
            RawReference =
                $"microsoft-graph://me/events/{calendarEvent.Id}",
            SourceTimestampUtc =
                calendarEvent.LastModifiedUtc ??
                calendarEvent.StartUtc,
            Summary =
                "Source-backed Microsoft Calendar event imported " +
                "read-only and held for review.",
            Title = string.IsNullOrWhiteSpace(calendarEvent.Subject)
                ? "(Untitled event)"
                : calendarEvent.Subject,
            StartUtc = calendarEvent.StartUtc,
            EndUtc = calendarEvent.EndUtc,
            TimeZone = calendarEvent.OriginalTimeZone,
            Location = calendarEvent.Location,
            Organizer = calendarEvent.Organizer,
            Attendees = calendarEvent.Attendees,
            RecurrenceReference =
                calendarEvent.RecurrenceReference,
            ResponseState = calendarEvent.ResponseState,
            OnlineMeetingReference =
                calendarEvent.OnlineMeetingReference,
            LastModifiedUtc = calendarEvent.LastModifiedUtc,
            IsCancelled = calendarEvent.IsCancelled
        };
    }

    public static IReadOnlyList<GenericProviderRecordCandidateDraft>
        CreateMessageSuggestions(
            MicrosoftProviderAccount account,
            MicrosoftGraphMessage message)
    {
        List<GenericProviderRecordCandidateDraft> suggestions = [];
        string subject = message.Subject ?? string.Empty;
        string lower = subject.ToLowerInvariant();

        if (!message.IsRead ||
            string.Equals(
                message.Importance,
                "high",
                StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add(CreateSuggestion(
                account,
                message,
                "follow-up",
                $"Follow up: {subject}",
                "Reviewable follow-up suggestion from source-backed mail."));
        }

        if (lower.Contains("waiting") ||
            lower.Contains("awaiting") ||
            lower.Contains("pending"))
        {
            suggestions.Add(CreateSuggestion(
                account,
                message,
                "waiting-on",
                $"Waiting on: {subject}",
                "Reviewable waiting-on suggestion; no task was created automatically."));
        }

        if (lower.Contains("invoice") ||
            lower.Contains("quote") ||
            lower.Contains("client") ||
            lower.Contains("work") ||
            lower.Contains("project"))
        {
            suggestions.Add(CreateSuggestion(
                account,
                message,
                "work-context",
                $"Work context: {subject}",
                "Reviewable work-context suggestion; source mail remains authoritative."));
        }

        return suggestions;
    }

    public static GenericProviderRecordCandidateDraft
        CreateScheduleSuggestion(
            MicrosoftProviderAccount account,
            MicrosoftGraphCalendarEvent calendarEvent) =>
        new()
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId =
                $"{calendarEvent.Id}:schedule-suggestion",
            CapabilityId = "calendar-suggestion",
            RawReference =
                $"microsoft-graph://me/events/{calendarEvent.Id}",
            SourceTimestampUtc =
                calendarEvent.LastModifiedUtc ??
                calendarEvent.StartUtc,
            RecordType = "Schedule suggestion",
            DisplayValue =
                $"Schedule context: {calendarEvent.Subject}",
            Summary =
                "Reviewable schedule suggestion from a source-backed " +
                "calendar event. No LifeOS schedule record was created.",
            AdditionalFields =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["Start"] = calendarEvent.StartUtc.ToString("O"),
                    ["End"] = calendarEvent.EndUtc.ToString("O"),
                    ["Location"] = calendarEvent.Location,
                    ["Organizer"] = calendarEvent.Organizer,
                    ["Source event ID"] = calendarEvent.Id
                }
        };

    private static GenericProviderRecordCandidateDraft CreateSuggestion(
        MicrosoftProviderAccount account,
        MicrosoftGraphMessage message,
        string suggestionType,
        string displayValue,
        string summary) =>
        new()
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = $"{message.Id}:{suggestionType}",
            CapabilityId = "mail-suggestion",
            RawReference =
                $"microsoft-graph://me/messages/{message.Id}",
            SourceTimestampUtc = message.ReceivedUtc,
            RecordType = suggestionType,
            DisplayValue = displayValue,
            Summary = summary,
            AdditionalFields =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["Source subject"] = message.Subject,
                    ["Sender"] = message.Sender,
                    ["Conversation"] = message.ConversationId,
                    ["Suggestion type"] = suggestionType
                }
        };
}

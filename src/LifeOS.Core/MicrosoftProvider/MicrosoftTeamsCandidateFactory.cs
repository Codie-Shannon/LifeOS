using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public static class MicrosoftTeamsCandidateFactory
{
    public static MessageCandidateDraft CreateMessageDraft(
        MicrosoftProviderAccount account,
        MicrosoftTeamsMessageDescriptor message)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(message);

        return new MessageCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = message.Id,
            CapabilityId = "teams-messages",
            RawReference = $"microsoft-graph://teams/{message.TeamId}/channels/{message.ChannelId}/messages/{message.Id}",
            SourceTimestampUtc = message.ModifiedUtc ?? message.CreatedUtc,
            Summary = "Source-backed Teams channel message imported read-only for review.",
            Subject = string.IsNullOrWhiteSpace(message.Subject)
                ? message.BodyPreview
                : message.Subject,
            Sender = message.AuthorDisplayName,
            Recipients = $"Team {message.TeamId} · Channel {message.ChannelId}",
            ConversationId = message.ThreadId,
            Importance = "normal",
            IsRead = true,
            HasAttachments = false,
            AttachmentMetadata = "Metadata only",
            LastModifiedUtc = message.ModifiedUtc
        };
    }

    public static CalendarEventCandidateDraft CreateMeetingDraft(
        MicrosoftProviderAccount account,
        MicrosoftTeamsMeetingDescriptor meeting)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(meeting);

        return new CalendarEventCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = meeting.Id,
            CapabilityId = "teams-meetings",
            RawReference = $"microsoft-graph://meetings/{meeting.Id}",
            SourceTimestampUtc = meeting.StartUtc,
            Summary = "Read-only Teams meeting context imported for review and linking.",
            Title = meeting.Subject,
            StartUtc = meeting.StartUtc,
            EndUtc = meeting.EndUtc,
            TimeZone = "UTC",
            Organizer = meeting.Organizer,
            Attendees = string.Join("; ", meeting.Attendees),
            OnlineMeetingReference = meeting.JoinUrl,
            ResponseState = "Informational",
            LastModifiedUtc = meeting.StartUtc
        };
    }

    public static TaskCandidateDraft CreateActionSuggestion(
        MicrosoftProviderAccount account,
        MicrosoftTeamsMessageDescriptor message,
        string suggestionKind)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(message);

        string normalizedKind = string.IsNullOrWhiteSpace(suggestionKind)
            ? "Follow-up"
            : suggestionKind.Trim();

        return new TaskCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = $"{message.Id}:suggestion:{normalizedKind.ToLowerInvariant().Replace(' ', '-')}",
            CapabilityId = "teams-action-suggestion",
            RawReference = $"microsoft-graph://teams/{message.TeamId}/channels/{message.ChannelId}/messages/{message.Id}",
            SourceTimestampUtc = message.ModifiedUtc ?? message.CreatedUtc,
            Summary = $"Review-only {normalizedKind} suggestion derived from a Teams message.",
            Title = $"{normalizedKind}: {message.BodyPreview}",
            Status = "Needs Review",
            Assignee = account.DisplayName,
            SourceList = "Microsoft Teams"
        };
    }
}

namespace LifeOS.Core.IntegrationInbox;

public static class Group50IntegrationInboxMigration
{
    public static IntegrationInboxV9State Apply(
        IntegrationInboxV9State state,
        DateTimeOffset nowUtc)
    {
        AddIfMissing(state, CreateChannelMessage(nowUtc));
        AddIfMissing(state, CreateReply(nowUtc));
        AddIfMissing(state, CreateMeeting(nowUtc));
        AddIfMissing(state, CreateSuggestion(nowUtc));
        AddIfMissing(state, CreateAccessLost(nowUtc));
        return state.Normalize();
    }

    private static void AddIfMissing(
        IntegrationInboxV9State state,
        IntegrationCandidate candidate)
    {
        if (state.Candidates.All(existing =>
            !string.Equals(existing.Id, candidate.Id, StringComparison.Ordinal)))
        {
            state.Candidates.Insert(0, candidate);
        }
    }

    private static IntegrationCandidate CreateChannelMessage(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new MessageCandidateDraft
            {
                ProviderId = "microsoft",
                ProviderDisplayName = "Microsoft 365",
                AccountId = "microsoft-work",
                AccountDisplayName = "Codie Shannon · Work",
                ExternalId = "redacted-teams-message",
                CapabilityId = "teams-messages",
                RawReference = "microsoft://teams/selected-team/delivery/message",
                SourceTimestampUtc = nowUtc.AddMinutes(-24),
                Subject = "Confirm Group 50 evidence sequence",
                Sender = "Delivery Coordinator",
                Recipients = "LifeOS Delivery · General",
                ConversationId = "redacted-thread-reference",
                Importance = "normal",
                IsRead = true,
                HasAttachments = false,
                AttachmentMetadata = "No attachment body imported",
                LastModifiedUtc = nowUtc.AddMinutes(-20),
                Summary = "Source-backed Teams channel message from the explicitly selected team and channel."
            },
            nowUtc);
        candidate.Id = "group50-teams-channel-message";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        candidate.Fields.Add(new IntegrationCandidateField
        {
            Key = "scope",
            DisplayName = "Bounded source",
            Value = "LifeOS Delivery · General · 30-day window"
        });
        candidate.Fields.Add(new IntegrationCandidateField
        {
            Key = "thread",
            DisplayName = "Thread reference",
            Value = "Redacted thread · reply provenance retained"
        });
        candidate.Fields.Add(new IntegrationCandidateField
        {
            Key = "edited",
            DisplayName = "Edited state",
            Value = "Edited once · latest source timestamp retained"
        });
        return candidate;
    }

    private static IntegrationCandidate CreateReply(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new MessageCandidateDraft
            {
                ProviderId = "microsoft",
                ProviderDisplayName = "Microsoft 365",
                AccountId = "microsoft-work",
                AccountDisplayName = "Codie Shannon · Work",
                ExternalId = "redacted-teams-reply",
                CapabilityId = "teams-messages",
                RawReference = "microsoft://teams/selected-team/delivery/reply",
                SourceTimestampUtc = nowUtc.AddMinutes(-18),
                Subject = "Reply: evidence sequence",
                Sender = "Project Lead",
                Recipients = "LifeOS Delivery · General",
                ConversationId = "redacted-thread-reference",
                Importance = "normal",
                IsRead = true,
                HasAttachments = false,
                AttachmentMetadata = "Metadata only",
                Summary = "Teams reply normalized with parent thread provenance and freshness."
            },
            nowUtc);
        candidate.Id = "group50-teams-thread-reply";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        candidate.Fields.Add(new IntegrationCandidateField
        {
            Key = "reply",
            DisplayName = "Reply provenance",
            Value = "Parent channel message retained"
        });
        return candidate;
    }

    private static IntegrationCandidate CreateMeeting(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new CalendarEventCandidateDraft
            {
                ProviderId = "microsoft",
                ProviderDisplayName = "Microsoft 365",
                AccountId = "microsoft-work",
                AccountDisplayName = "Codie Shannon · Work",
                ExternalId = "redacted-teams-meeting",
                CapabilityId = "teams-meetings",
                RawReference = "microsoft://teams/meeting/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-50),
                Title = "LifeOS delivery review",
                StartUtc = nowUtc.AddDays(1).Date.AddHours(9),
                EndUtc = nowUtc.AddDays(1).Date.AddHours(9).AddMinutes(30),
                TimeZone = "Pacific/Auckland",
                Organizer = "Delivery Coordinator",
                Attendees = "Codie Shannon; Project Lead",
                ResponseState = "Accepted",
                OnlineMeetingReference = "Join-link metadata retained; address redacted",
                Summary = "Read-only Teams meeting context linked to the existing LifeOS v9 Work record."
            },
            nowUtc);
        candidate.Id = "group50-teams-meeting";
        candidate.Status = IntegrationCandidateStatus.Accepted;
        candidate.AuthoritativeLink = new LifeOsAuthoritativeLink
        {
            Module = "Work",
            RecordId = "work-lifeos-v9",
            DisplayName = "LifeOS v9 delivery",
            LinkedUtc = nowUtc.AddMinutes(-8)
        };
        return candidate;
    }

    private static IntegrationCandidate CreateSuggestion(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new TaskCandidateDraft
            {
                ProviderId = "microsoft",
                ProviderDisplayName = "Microsoft 365",
                AccountId = "microsoft-work",
                AccountDisplayName = "Codie Shannon · Work",
                ExternalId = "redacted-teams-suggestion",
                CapabilityId = "teams-action-suggestion",
                RawReference = "microsoft://teams/selected-team/delivery/message",
                SourceTimestampUtc = nowUtc.AddMinutes(-14),
                Title = "Requested document: send the final Group 50 proof list",
                Status = "Needs Review",
                Assignee = "Codie Shannon",
                SourceList = "Microsoft Teams",
                Summary = "Review-only requested-document suggestion; no autonomous task creation or Teams action."
            },
            nowUtc);
        candidate.Id = "group50-teams-action-suggestion";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        return candidate;
    }

    private static IntegrationCandidate CreateAccessLost(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new MessageCandidateDraft
            {
                ProviderId = "microsoft",
                ProviderDisplayName = "Microsoft 365",
                AccountId = "microsoft-work",
                AccountDisplayName = "Codie Shannon · Work",
                ExternalId = "redacted-teams-access-lost",
                CapabilityId = "teams-messages",
                RawReference = "microsoft://teams/access-lost",
                SourceTimestampUtc = nowUtc.AddDays(-2),
                Subject = "Archived delivery discussion",
                Sender = "Source unavailable",
                Recipients = "Selected channel",
                ConversationId = "redacted",
                Summary = "Channel access was lost. Provenance remains visible and synchronization fails closed."
            },
            nowUtc);
        candidate.Id = "group50-teams-access-lost";
        candidate.Status = IntegrationCandidateStatus.SourceRemoved;
        candidate.Provenance.IsSourceRemoved = true;
        candidate.Provenance.Freshness = IntegrationCandidateFreshness.SourceRemoved;
        candidate.Fields.Add(new IntegrationCandidateField
        {
            Key = "recovery",
            DisplayName = "Recovery state",
            Value = "Access lost · stale data not presented as current"
        });
        return candidate;
    }
}

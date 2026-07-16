namespace LifeOS.Core.IntegrationInbox;

public static class Group51IntegrationInboxMigration
{
    public static IntegrationInboxV9State Apply(
        IntegrationInboxV9State state,
        DateTimeOffset nowUtc)
    {
        Add(state, Gmail(nowUtc));
        Add(state, Calendar(nowUtc));
        Add(state, Drive(nowUtc));
        Add(state, Contact(nowUtc));
        Add(state, Task(nowUtc));
        Add(state, Revoked(nowUtc));
        return state.Normalize();
    }

    private static void Add(
        IntegrationInboxV9State state,
        IntegrationCandidate candidate)
    {
        if (state.Candidates.All(item => item.Id != candidate.Id))
        {
            state.Candidates.Insert(0, candidate);
        }
    }

    private static IntegrationCandidate Gmail(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new MessageCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-gmail-message",
                CapabilityId = "gmail",
                RawReference = "google://gmail/message/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-18),
                Subject = "Confirm Group 51 provider proof",
                Sender = "Workspace Coordinator",
                Recipients = "Codie Shannon",
                ConversationId = "redacted-google-thread",
                Importance = "important",
                IsRead = false,
                HasAttachments = true,
                AttachmentMetadata = "1 attachment · metadata only · body not downloaded",
                Summary = "Bounded Gmail message and thread metadata imported read-only for review."
            },
            nowUtc);
        candidate.Id = "group51-gmail";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        candidate.Fields.Add(new()
        {
            Key = "labels",
            DisplayName = "Labels",
            Value = "Inbox · Important"
        });
        candidate.Fields.Add(new()
        {
            Key = "starred",
            DisplayName = "Starred",
            Value = "Yes"
        });
        return candidate;
    }

    private static IntegrationCandidate Calendar(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new CalendarEventCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-google-calendar-event",
                CapabilityId = "google-calendar",
                RawReference = "google://calendar/event/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-14),
                Title = "LifeOS v9 closure review",
                StartUtc = nowUtc.AddDays(1).Date.AddHours(10),
                EndUtc = nowUtc.AddDays(1).Date.AddHours(10).AddMinutes(30),
                TimeZone = "Pacific/Auckland",
                Location = "Online",
                Organizer = "Codie Shannon",
                Attendees = "Delivery Coordinator",
                RecurrenceReference = "Single event",
                ResponseState = "Accepted",
                OnlineMeetingReference = "Conference-link metadata retained; address redacted",
                Summary = "Selected-calendar Google event imported read-only for review."
            },
            nowUtc);
        candidate.Id = "group51-google-calendar";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        return candidate;
    }

    private static IntegrationCandidate Drive(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new FileDocumentCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-google-drive-file",
                CapabilityId = "google-drive",
                RawReference = "google://drive/file/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-12),
                Name = "LifeOS Group 51 evidence plan.docx",
                Extension = ".docx",
                SizeBytes = 93184,
                ModifiedUtc = nowUtc.AddMinutes(-12),
                WebReference = "Open source item in Google Drive",
                AdditionalFields = new Dictionary<string, string>
                {
                    ["bounded-source"] = "Selected folder · LifeOS Evidence",
                    ["owner"] = "Codie Shannon",
                    ["change-reference"] = "Redacted change reference · incrementally tracked",
                    ["file-body"] = "Not downloaded"
                },
                Summary = "Selected-folder Google Drive metadata imported without downloading the file body."
            },
            nowUtc);
        candidate.Id = "group51-google-drive";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        return candidate;
    }

    private static IntegrationCandidate Contact(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new ContactPersonCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-google-contact",
                CapabilityId = "google-contacts",
                RawReference = "google://people/contact/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-10),
                DisplayName = "Delivery Coordinator",
                PrimaryEmail = "address-redacted@example.invalid",
                Phone = "Phone redacted",
                Organization = "LifeOS delivery",
                Summary = "Google contact imported read-only as a reviewable person candidate."
            },
            nowUtc);
        candidate.Id = "group51-google-contact";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        return candidate;
    }

    private static IntegrationCandidate Task(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new TaskCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-google-task",
                CapabilityId = "google-tasks",
                RawReference = "google://tasks/task/redacted",
                SourceTimestampUtc = nowUtc.AddMinutes(-8),
                Title = "Review Group 51 Google provider evidence",
                DueUtc = nowUtc.AddDays(1),
                Status = "Needs action",
                Assignee = "Codie Shannon",
                SourceList = "LifeOS delivery",
                Summary = "Selected-list Google task imported read-only for review."
            },
            nowUtc);
        candidate.Id = "group51-google-task";
        candidate.Status = IntegrationCandidateStatus.NeedsReview;
        return candidate;
    }

    private static IntegrationCandidate Revoked(DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            new ContactPersonCandidateDraft
            {
                ProviderId = "google",
                ProviderDisplayName = "Google Workspace",
                AccountId = "google-personal",
                AccountDisplayName = "Codie Shannon · Personal",
                ExternalId = "redacted-revoked-contact",
                CapabilityId = "google-contacts",
                RawReference = "google://people/revoked",
                SourceTimestampUtc = nowUtc.AddDays(-2),
                DisplayName = "Source access unavailable",
                PrimaryEmail = "revoked@example.invalid",
                Phone = string.Empty,
                Organization = "Permission revoked",
                Summary = "Contacts permission was revoked. Other Google capabilities remain independently healthy."
            },
            nowUtc);
        candidate.Id = "group51-google-revoked";
        candidate.Status = IntegrationCandidateStatus.SourceRemoved;
        candidate.Provenance.IsSourceRemoved = true;
        candidate.Provenance.Freshness = IntegrationCandidateFreshness.SourceRemoved;
        candidate.Fields.Add(new()
        {
            Key = "recovery",
            DisplayName = "Recovery state",
            Value = "Contacts permission revoked · failed closed"
        });
        return candidate;
    }
}

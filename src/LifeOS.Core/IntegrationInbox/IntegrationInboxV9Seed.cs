namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationInboxV9Seed
{
    public static IntegrationInboxV9State CreateFictional(
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate message = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new MessageCandidateDraft
                {
                    ProviderId = "fictional-microsoft",
                    ProviderDisplayName = "Microsoft 365 (fictional)",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "mail-1042",
                    CapabilityId = "mail",
                    RawReference = "fictional://mail/inbox/mail-1042",
                    SourceTimestampUtc = nowUtc.AddMinutes(-28),
                    Subject = "Supplier quote needs a reply",
                    Sender = "accounts@sample.invalid",
                    Recipients = "work-account@example.invalid",
                    ConversationId = "thread-1042",
                    Importance = "High",
                    IsRead = false,
                    HasAttachments = true,
                    Summary = "Source-backed message suggests a follow-up, but remains untrusted until reviewed."
                },
                nowUtc),
            "message-follow-up");

        IntegrationCandidate financial = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new FinancialItemCandidateDraft
                {
                    ProviderId = "fictional-accounting",
                    ProviderDisplayName = "Accounting fixture (fictional)",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "payment-778",
                    CapabilityId = "financial",
                    RawReference = "fictional://payments/payment-778",
                    SourceTimestampUtc = nowUtc.AddHours(-5),
                    Description = "Total Door invoice payment",
                    Amount = 640m,
                    Currency = "NZD",
                    OccurredUtc = nowUtc.AddDays(-1),
                    Counterparty = "Total Door Systems (fictional)",
                    Reference = "INV-778",
                    Summary = "Payment-style candidate requires review before it affects trusted money state."
                },
                nowUtc),
            "financial-review");
        financial.Status = IntegrationCandidateStatus.NeedsReview;

        IntegrationCandidate acceptedContact = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new ContactPersonCandidateDraft
                {
                    ProviderId = "fictional-google",
                    ProviderDisplayName = "Google Workspace (fictional)",
                    AccountId = "alex-personal",
                    AccountDisplayName = "Alex Morgan",
                    ExternalId = "contact-441",
                    CapabilityId = "contacts",
                    RawReference = "fictional://contacts/contact-441",
                    SourceTimestampUtc = nowUtc.AddHours(-4),
                    DisplayName = "Michele Carter",
                    PrimaryEmail = "michele.carter@example.invalid",
                    Phone = "+64 20 555 0141",
                    Organization = "AIE Example",
                    Summary = "Previously reviewed fictional contact linked to the authoritative People record."
                },
                nowUtc),
            "contact-accepted");
        acceptedContact.Status = IntegrationCandidateStatus.Accepted;
        acceptedContact.AuthoritativeLink = new LifeOsAuthoritativeLink
        {
            Module = "People",
            RecordId = "people-118",
            DisplayName = "Michele Carter",
            LinkedUtc = nowUtc.AddHours(-3)
        };
        acceptedContact.ReviewNote = "Accepted and linked after explicit review.";

        IntegrationCandidate duplicateContact = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new ContactPersonCandidateDraft
                {
                    ProviderId = "fictional-microsoft",
                    ProviderDisplayName = "Microsoft 365 (fictional)",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "contact-902",
                    CapabilityId = "contacts",
                    RawReference = "fictional://people/contact-902",
                    SourceTimestampUtc = nowUtc.AddMinutes(-55),
                    DisplayName = "Michele Carter",
                    PrimaryEmail = "michele.carter@example.invalid",
                    Phone = "+64 20 555 0141",
                    Organization = "AIE Example",
                    Summary = "Same normalized person arrived through another provider and requires duplicate review."
                },
                nowUtc),
            "contact-duplicate");
        duplicateContact.Status =
            IntegrationCandidateStatus.DuplicateSuspected;
        duplicateContact.DuplicateOfCandidateId = acceptedContact.Id;

        IntegrationCandidate conflictCalendar = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new CalendarEventCandidateDraft
                {
                    ProviderId = "fictional-microsoft",
                    ProviderDisplayName = "Microsoft 365 (fictional)",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "event-220",
                    CapabilityId = "calendar",
                    RawReference = "fictional://calendar/event-220",
                    SourceTimestampUtc = nowUtc.AddMinutes(-40),
                    Title = "AIE review call",
                    StartUtc = AtDayHour(nowUtc, 2, 10.5),
                    EndUtc = AtDayHour(nowUtc, 2, 11.5),
                    TimeZone = "Pacific/Auckland",
                    Location = "Workshop 2",
                    Organizer = "alice.young@example.invalid",
                    Attendees = "Codie; Alice; Toni",
                    RecurrenceReference = "none",
                    Summary = "Changed source date, time and location conflict with an accepted LifeOS schedule record."
                },
                nowUtc),
            "calendar-conflict");
        conflictCalendar.Status = IntegrationCandidateStatus.Conflict;
        conflictCalendar.ConflictWithRecordId = "agenda-204";
        SetConflict(
            conflictCalendar,
            "start",
            AtDayHour(nowUtc, 2, 10).ToString("O"),
            IntegrationConflictFieldChoice.KeepCurrent);
        SetConflict(
            conflictCalendar,
            "end",
            AtDayHour(nowUtc, 2, 11).ToString("O"),
            IntegrationConflictFieldChoice.AcceptCandidate);
        SetConflict(
            conflictCalendar,
            "location",
            "Teams meeting",
            IntegrationConflictFieldChoice.KeepCurrent);

        IntegrationCandidate acceptedTask = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new TaskCandidateDraft
                {
                    ProviderId = "fictional-local",
                    ProviderDisplayName = "Local fixture connector",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "task-301",
                    CapabilityId = "tasks",
                    RawReference = "fictional://tasks/task-301",
                    SourceTimestampUtc = nowUtc.AddHours(-2),
                    Title = "Send welding proof update",
                    DueUtc = nowUtc.AddDays(1),
                    Status = "Open",
                    Assignee = "Codie",
                    SourceList = "AIE proof",
                    Summary = "Accepted task candidate links to the authoritative Work item without a duplicate editor."
                },
                nowUtc),
            "task-accepted");
        acceptedTask.Status = IntegrationCandidateStatus.Accepted;
        acceptedTask.AuthoritativeLink = new LifeOsAuthoritativeLink
        {
            Module = "Work Pipeline",
            RecordId = "work-903",
            DisplayName = "Send welding proof update",
            LinkedUtc = nowUtc.AddHours(-1)
        };
        acceptedTask.ReviewNote = "Accepted and linked to the existing Work item.";

        IntegrationCandidate file = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new FileDocumentCandidateDraft
                {
                    ProviderId = "fictional-local",
                    ProviderDisplayName = "Local fixture connector",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "file-610",
                    CapabilityId = "files",
                    RawReference = "fictional://files/file-610",
                    SourceTimestampUtc = nowUtc.AddHours(-9),
                    Name = "Welding proof register.xlsx",
                    Extension = ".xlsx",
                    SizeBytes = 48128,
                    ModifiedUtc = nowUtc.AddHours(-9),
                    WebReference = "local-fixture://proof/register",
                    Summary = "File metadata is normalized without storing unnecessary file-body payload."
                },
                nowUtc),
            "file-review");
        file.Status = IntegrationCandidateStatus.NeedsReview;

        IntegrationCandidate staleGeneric = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new GenericProviderRecordCandidateDraft
                {
                    ProviderId = "fictional-provider",
                    ProviderDisplayName = "Generic provider fixture",
                    AccountId = "alex-personal",
                    AccountDisplayName = "Alex Morgan",
                    ExternalId = "generic-111",
                    CapabilityId = "generic",
                    RawReference = "fictional://generic/generic-111",
                    SourceTimestampUtc = nowUtc.AddDays(-3),
                    RecordType = "Travel note",
                    DisplayValue = "Palmerston North food stop",
                    AdditionalFields = new Dictionary<string, string>
                    {
                        ["Category"] = "Travel",
                        ["Confidence"] = "Provider supplied"
                    },
                    Summary = "Stale generic candidate remains visible and cannot masquerade as current."
                },
                nowUtc),
            "generic-stale");
        staleGeneric.Status = IntegrationCandidateStatus.NeedsReview;

        IntegrationCandidate sourceRemoved = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new MessageCandidateDraft
                {
                    ProviderId = "fictional-google",
                    ProviderDisplayName = "Google Workspace (fictional)",
                    AccountId = "alex-personal",
                    AccountDisplayName = "Alex Morgan",
                    ExternalId = "mail-removed-55",
                    CapabilityId = "mail",
                    RawReference = "fictional://mail/trash/mail-removed-55",
                    SourceTimestampUtc = nowUtc.AddDays(-2),
                    Subject = "Old booking confirmation",
                    Sender = "booking@example.invalid",
                    Recipients = "personal-account@example.invalid",
                    ConversationId = "thread-removed-55",
                    Importance = "Normal",
                    IsRead = true,
                    HasAttachments = false,
                    Summary = "Provider tombstone is retained with provenance instead of silently deleting the candidate."
                },
                nowUtc),
            "message-source-removed");
        sourceRemoved.Status = IntegrationCandidateStatus.SourceRemoved;
        sourceRemoved.Provenance.IsSourceRemoved = true;
        sourceRemoved.Provenance.Freshness =
            IntegrationCandidateFreshness.SourceRemoved;

        IntegrationCandidate batchOne = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new CalendarEventCandidateDraft
                {
                    ProviderId = "fictional-local",
                    ProviderDisplayName = "ICS fixture connector",
                    AccountId = "alex-personal",
                    AccountDisplayName = "Alex Morgan",
                    ExternalId = "event-batch-1",
                    CapabilityId = "calendar",
                    RawReference = "fictional://ics/event-batch-1",
                    SourceTimestampUtc = nowUtc.AddMinutes(-20),
                    Title = "Drive to Taupo",
                    StartUtc = AtDayHour(nowUtc, 4, 8),
                    EndUtc = AtDayHour(nowUtc, 4, 10),
                    TimeZone = "Pacific/Auckland",
                    Location = "Whakatane to Taupo",
                    Organizer = "local fixture",
                    Attendees = "Codie",
                    RecurrenceReference = "none",
                    LowRiskBatchEligible = true,
                    Summary = "Low-risk fictional calendar item eligible for identical batch review."
                },
                nowUtc),
            "batch-calendar-1");

        IntegrationCandidate batchTwo = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new CalendarEventCandidateDraft
                {
                    ProviderId = "fictional-local",
                    ProviderDisplayName = "ICS fixture connector",
                    AccountId = "alex-personal",
                    AccountDisplayName = "Alex Morgan",
                    ExternalId = "event-batch-2",
                    CapabilityId = "calendar",
                    RawReference = "fictional://ics/event-batch-2",
                    SourceTimestampUtc = nowUtc.AddMinutes(-18),
                    Title = "Return from Taupo",
                    StartUtc = AtDayHour(nowUtc, 4, 17),
                    EndUtc = AtDayHour(nowUtc, 4, 19),
                    TimeZone = "Pacific/Auckland",
                    Location = "Taupo to Whakatane",
                    Organizer = "local fixture",
                    Attendees = "Codie",
                    RecurrenceReference = "none",
                    LowRiskBatchEligible = true,
                    Summary = "Second low-risk fictional calendar item in the explicit batch preview."
                },
                nowUtc),
            "batch-calendar-2");

        IntegrationCandidate malformed = WithId(
            IntegrationCandidateNormalizer.Normalize(
                new GenericProviderRecordCandidateDraft
                {
                    ProviderId = "fictional-provider",
                    ProviderDisplayName = "Generic provider fixture",
                    AccountId = "northstar-work",
                    AccountDisplayName = "Northstar Operations",
                    ExternalId = "malformed-1",
                    CapabilityId = "generic",
                    SourceTimestampUtc = nowUtc.AddMinutes(-10),
                    RecordType = string.Empty,
                    DisplayValue = string.Empty,
                    Summary = "Malformed fixture proves quarantine behavior."
                },
                nowUtc),
            "malformed-quarantine");

        IntegrationInboxV9State state = new()
        {
            Candidates =
            [
                message,
                financial,
                duplicateContact,
                acceptedContact,
                conflictCalendar,
                acceptedTask,
                file,
                staleGeneric,
                sourceRemoved,
                batchOne,
                batchTwo,
                malformed
            ],
            AuditEntries =
            [
                Audit(1, nowUtc.AddDays(-4), acceptedContact.Id, IntegrationReviewAuditAction.Accepted, "Contact candidate accepted after source-backed review."),
                Audit(2, nowUtc.AddDays(-4).AddMinutes(1), acceptedContact.Id, IntegrationReviewAuditAction.Linked, "Accepted contact linked to People record people-118."),
                Audit(3, nowUtc.AddHours(-3), acceptedTask.Id, IntegrationReviewAuditAction.Accepted, "Task candidate accepted after review."),
                Audit(4, nowUtc.AddHours(-3).AddMinutes(1), acceptedTask.Id, IntegrationReviewAuditAction.Linked, "Accepted task linked to Work Pipeline record work-903."),
                Audit(5, nowUtc.AddHours(-1), duplicateContact.Id, IntegrationReviewAuditAction.DuplicateDetected, "Deterministic fingerprint matched the accepted contact."),
                Audit(6, nowUtc.AddMinutes(-48), conflictCalendar.Id, IntegrationReviewAuditAction.ConflictCreated, "Source changes conflict with agenda-204."),
                Audit(7, nowUtc.AddMinutes(-30), sourceRemoved.Id, IntegrationReviewAuditAction.SourceRemoved, "Provider tombstone retained as reviewable provenance."),
                Audit(8, nowUtc.AddMinutes(-10), malformed.Id, IntegrationReviewAuditAction.CandidateQuarantined, "Malformed generic fixture quarantined.")
            ],
            NextAuditSequence = 9
        };

        return state.Normalize();
    }


    private static DateTimeOffset AtDayHour(
        DateTimeOffset origin,
        int dayOffset,
        double hour)
    {
        DateTime date = origin.AddDays(dayOffset).Date.AddHours(hour);
        return new DateTimeOffset(date, origin.Offset);
    }

    private static IntegrationCandidate WithId(
        IntegrationCandidate candidate,
        string id)
    {
        candidate.Id = id;
        return candidate;
    }

    private static void SetConflict(
        IntegrationCandidate candidate,
        string fieldKey,
        string currentValue,
        IntegrationConflictFieldChoice choice)
    {
        IntegrationCandidateField field = candidate.Fields.Single(
            candidateField =>
                string.Equals(
                    candidateField.Key,
                    fieldKey,
                    StringComparison.Ordinal));

        field.IsConflict = true;
        field.CurrentValue = currentValue;
        field.ConflictChoice = choice;
    }

    private static IntegrationReviewAuditEntry Audit(
        long sequence,
        DateTimeOffset timestampUtc,
        string candidateId,
        IntegrationReviewAuditAction action,
        string summary) => new()
        {
            Sequence = sequence,
            TimestampUtc = timestampUtc,
            CandidateId = candidateId,
            Action = action,
            Summary = summary
        };
}

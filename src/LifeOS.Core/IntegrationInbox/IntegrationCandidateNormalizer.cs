namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationCandidateNormalizer
{
    public static IntegrationCandidate Normalize(
        IntegrationCandidateDraftBase draft,
        DateTimeOffset importedUtc)
    {
        ArgumentNullException.ThrowIfNull(draft);

        string? invalidReason = ValidateCommon(draft);
        if (invalidReason is not null)
        {
            return Quarantine(draft, importedUtc, invalidReason);
        }

        return draft switch
        {
            MessageCandidateDraft message => NormalizeMessage(message, importedUtc),
            CalendarEventCandidateDraft calendar => NormalizeCalendar(calendar, importedUtc),
            ContactPersonCandidateDraft contact => NormalizeContact(contact, importedUtc),
            FileDocumentCandidateDraft file => NormalizeFile(file, importedUtc),
            TaskCandidateDraft task => NormalizeTask(task, importedUtc),
            FinancialItemCandidateDraft financial => NormalizeFinancial(financial, importedUtc),
            GenericProviderRecordCandidateDraft generic => NormalizeGeneric(generic, importedUtc),
            _ => Quarantine(draft, importedUtc, $"Unsupported candidate draft type: {draft.GetType().Name}.")
        };
    }

    public static IntegrationCandidateFreshness CalculateFreshness(
        DateTimeOffset sourceTimestampUtc,
        DateTimeOffset importedTimestampUtc,
        bool sourceRemoved)
    {
        if (sourceRemoved)
        {
            return IntegrationCandidateFreshness.SourceRemoved;
        }

        TimeSpan age = importedTimestampUtc - sourceTimestampUtc;

        if (age <= TimeSpan.FromHours(2))
        {
            return IntegrationCandidateFreshness.Fresh;
        }

        if (age <= TimeSpan.FromHours(24))
        {
            return IntegrationCandidateFreshness.Ageing;
        }

        return IntegrationCandidateFreshness.Stale;
    }

    private static IntegrationCandidate NormalizeMessage(
        MessageCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.Subject) ||
            string.IsNullOrWhiteSpace(draft.Sender))
        {
            return Quarantine(
                draft,
                importedUtc,
                "Message subject and sender are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("subject", "Subject", draft.Subject),
            Field("sender", "Sender", draft.Sender),
            Field("recipients", "Recipients", draft.Recipients),
            Field("conversation", "Conversation / thread", draft.ConversationId),
            Field("importance", "Importance", draft.Importance),
            Field("read-state", "Read state", draft.IsRead ? "Read" : "Unread"),
            Field("attachments", "Attachment metadata", draft.HasAttachments ? "Present" : "None")
        ];

        return Create(
            IntegrationCandidateType.Message,
            draft.Subject,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("subject", draft.Subject),
                Pair("sender", draft.Sender),
                Pair("conversation", draft.ConversationId)
            ]);
    }

    private static IntegrationCandidate NormalizeCalendar(
        CalendarEventCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.Title) ||
            draft.StartUtc == default ||
            draft.EndUtc <= draft.StartUtc)
        {
            return Quarantine(
                draft,
                importedUtc,
                "Calendar title and a valid start/end range are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("title", "Title", draft.Title),
            Field("start", "Start", draft.StartUtc.ToString("O")),
            Field("end", "End", draft.EndUtc.ToString("O")),
            Field("timezone", "Time zone", draft.TimeZone),
            Field("location", "Location", draft.Location),
            Field("organizer", "Organizer", draft.Organizer),
            Field("attendees", "Attendees", draft.Attendees),
            Field("recurrence", "Recurrence reference", draft.RecurrenceReference)
        ];

        return Create(
            IntegrationCandidateType.CalendarEvent,
            draft.Title,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("title", draft.Title),
                Pair("start", draft.StartUtc.UtcDateTime.ToString("O")),
                Pair("organizer", draft.Organizer)
            ]);
    }

    private static IntegrationCandidate NormalizeContact(
        ContactPersonCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.DisplayName) ||
            string.IsNullOrWhiteSpace(draft.PrimaryEmail))
        {
            return Quarantine(
                draft,
                importedUtc,
                "Contact display name and primary email are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("display-name", "Display name", draft.DisplayName),
            Field("email", "Primary email", draft.PrimaryEmail),
            Field("phone", "Phone", draft.Phone),
            Field("organization", "Organization", draft.Organization)
        ];

        return Create(
            IntegrationCandidateType.ContactPerson,
            draft.DisplayName,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("email", draft.PrimaryEmail),
                Pair("display-name", draft.DisplayName)
            ]);
    }

    private static IntegrationCandidate NormalizeFile(
        FileDocumentCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.Name) ||
            draft.ModifiedUtc == default)
        {
            return Quarantine(
                draft,
                importedUtc,
                "File name and modified timestamp are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("name", "Name", draft.Name),
            Field("extension", "Extension", draft.Extension),
            Field("size", "Size", draft.SizeBytes.ToString()),
            Field("modified", "Modified", draft.ModifiedUtc.ToString("O")),
            Field("web-reference", "Provider reference", draft.WebReference)
        ];

        return Create(
            IntegrationCandidateType.FileDocument,
            draft.Name,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("name", draft.Name),
                Pair("size", draft.SizeBytes.ToString()),
                Pair("modified", draft.ModifiedUtc.UtcDateTime.ToString("O"))
            ]);
    }

    private static IntegrationCandidate NormalizeTask(
        TaskCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return Quarantine(draft, importedUtc, "Task title is required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("title", "Title", draft.Title),
            Field("due", "Due", draft.DueUtc?.ToString("O") ?? "No due date"),
            Field("status", "Status", draft.Status),
            Field("assignee", "Assignee", draft.Assignee),
            Field("source-list", "Source list", draft.SourceList)
        ];

        return Create(
            IntegrationCandidateType.Task,
            draft.Title,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("title", draft.Title),
                Pair("due", draft.DueUtc?.UtcDateTime.ToString("O")),
                Pair("assignee", draft.Assignee)
            ]);
    }

    private static IntegrationCandidate NormalizeFinancial(
        FinancialItemCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.Description) ||
            draft.OccurredUtc == default ||
            string.IsNullOrWhiteSpace(draft.Currency))
        {
            return Quarantine(
                draft,
                importedUtc,
                "Financial description, currency and occurrence timestamp are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("description", "Description", draft.Description),
            Field("amount", "Amount", draft.Amount.ToString("0.00")),
            Field("currency", "Currency", draft.Currency.ToUpperInvariant()),
            Field("occurred", "Occurred", draft.OccurredUtc.ToString("O")),
            Field("counterparty", "Counterparty", draft.Counterparty),
            Field("reference", "Reference", draft.Reference)
        ];

        return Create(
            IntegrationCandidateType.FinancialItem,
            draft.Description,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("amount", draft.Amount.ToString("0.00")),
                Pair("currency", draft.Currency),
                Pair("occurred", draft.OccurredUtc.UtcDateTime.ToString("yyyy-MM-dd")),
                Pair("counterparty", draft.Counterparty)
            ]);
    }

    private static IntegrationCandidate NormalizeGeneric(
        GenericProviderRecordCandidateDraft draft,
        DateTimeOffset importedUtc)
    {
        if (string.IsNullOrWhiteSpace(draft.RecordType) ||
            string.IsNullOrWhiteSpace(draft.DisplayValue))
        {
            return Quarantine(
                draft,
                importedUtc,
                "Generic record type and display value are required.");
        }

        List<IntegrationCandidateField> fields =
        [
            Field("record-type", "Record type", draft.RecordType),
            Field("display-value", "Display value", draft.DisplayValue)
        ];

        foreach (KeyValuePair<string, string> pair in draft.AdditionalFields
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            fields.Add(Field(
                $"additional-{pair.Key.Trim().ToLowerInvariant()}",
                pair.Key,
                pair.Value));
        }

        return Create(
            IntegrationCandidateType.GenericProviderRecord,
            draft.DisplayValue,
            draft.Summary,
            draft,
            importedUtc,
            fields,
            [
                Pair("record-type", draft.RecordType),
                Pair("display-value", draft.DisplayValue)
            ]);
    }

    private static IntegrationCandidate Create(
        IntegrationCandidateType type,
        string title,
        string summary,
        IntegrationCandidateDraftBase draft,
        DateTimeOffset importedUtc,
        List<IntegrationCandidateField> fields,
        IEnumerable<KeyValuePair<string, string?>> fingerprintValues)
    {
        return new IntegrationCandidate
        {
            Type = type,
            Status = IntegrationCandidateStatus.New,
            Title = title.Trim(),
            Summary = summary.Trim(),
            Provenance = CreateProvenance(draft, importedUtc),
            Fields = fields,
            Fingerprint = IntegrationCandidateFingerprint.Build(
                type,
                fingerprintValues),
            ContentHash = IntegrationCandidateFingerprint.BuildContentHash(
                type,
                fields),
            LowRiskBatchEligible = draft.LowRiskBatchEligible,
            CreatedUtc = importedUtc,
            UpdatedUtc = importedUtc
        };
    }

    private static IntegrationCandidate Quarantine(
        IntegrationCandidateDraftBase draft,
        DateTimeOffset importedUtc,
        string reason)
    {
        string title = string.IsNullOrWhiteSpace(draft.ExternalId)
            ? "Malformed imported candidate"
            : $"Malformed candidate {draft.ExternalId.Trim()}";

        List<IntegrationCandidateField> fields =
        [
            Field("quarantine-reason", "Quarantine reason", reason)
        ];

        return new IntegrationCandidate
        {
            Type = IntegrationCandidateType.GenericProviderRecord,
            Status = IntegrationCandidateStatus.NeedsReview,
            Title = title,
            Summary = "Candidate was contained because required normalized fields were missing or invalid.",
            Provenance = CreateProvenance(draft, importedUtc),
            Fields = fields,
            Fingerprint = IntegrationCandidateFingerprint.Build(
                IntegrationCandidateType.GenericProviderRecord,
                [Pair("quarantine", draft.ExternalId)]),
            ContentHash = IntegrationCandidateFingerprint.BuildContentHash(
                IntegrationCandidateType.GenericProviderRecord,
                fields),
            IsQuarantined = true,
            QuarantineReason = reason,
            LowRiskBatchEligible = false,
            CreatedUtc = importedUtc,
            UpdatedUtc = importedUtc
        };
    }

    private static IntegrationCandidateProvenance CreateProvenance(
        IntegrationCandidateDraftBase draft,
        DateTimeOffset importedUtc) => new()
        {
            ProviderId = draft.ProviderId.Trim(),
            ProviderDisplayName = draft.ProviderDisplayName.Trim(),
            AccountId = draft.AccountId.Trim(),
            AccountDisplayName = draft.AccountDisplayName.Trim(),
            ExternalId = draft.ExternalId.Trim(),
            CapabilityId = draft.CapabilityId.Trim(),
            RawReference = draft.RawReference.Trim(),
            SourceTimestampUtc = draft.SourceTimestampUtc,
            ImportedTimestampUtc = importedUtc,
            LastSeenUtc = importedUtc,
            Freshness = CalculateFreshness(
                draft.SourceTimestampUtc,
                importedUtc,
                sourceRemoved: false),
            IsSourceRemoved = false
        };

    private static string? ValidateCommon(IntegrationCandidateDraftBase draft)
    {
        if (string.IsNullOrWhiteSpace(draft.ProviderId))
        {
            return "Provider ID is required.";
        }

        if (string.IsNullOrWhiteSpace(draft.AccountId))
        {
            return "Account ID is required.";
        }

        if (string.IsNullOrWhiteSpace(draft.ExternalId))
        {
            return "External ID is required.";
        }

        if (string.IsNullOrWhiteSpace(draft.CapabilityId))
        {
            return "Capability ID is required.";
        }

        if (draft.SourceTimestampUtc == default)
        {
            return "Source timestamp is required.";
        }

        return null;
    }

    private static IntegrationCandidateField Field(
        string key,
        string displayName,
        string? value) => new()
        {
            Key = key,
            DisplayName = displayName,
            Value = value?.Trim() ?? string.Empty
        };

    private static KeyValuePair<string, string?> Pair(
        string key,
        string? value) => new(key, value);
}

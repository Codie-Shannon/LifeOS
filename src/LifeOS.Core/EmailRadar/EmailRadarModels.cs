namespace LifeOS.Core.EmailRadar;

public enum EmailRadarProfileStatus { Active, Archived }
public enum CommunicationReviewState { NeedsReview, PossibleMatch, ConfirmedMatch, RejectedMatch, DuplicateSuspected }
public enum CommunicationDirection { Incoming, Outgoing, Unknown }
public enum CommunicationSuggestionKind { InsufficientEvidence, PossibleWaitingOnThem, PossibleWaitingOnMe, PossibleFollowUpDue, RecentActivityNoFollowUp }

public sealed class EmailRadarProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string RelatedLabel { get; set; } = "";
    public List<string> People { get; set; } = [];
    public List<string> EmailAddresses { get; set; } = [];
    public List<string> SubjectPhrases { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public List<string> ExcludeTerms { get; set; } = [];
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public int FollowUpDays { get; set; } = 7;
    public string ConfirmedState { get; set; } = "";
    public string Notes { get; set; } = "";
    public EmailRadarProfileStatus Status { get; set; } = EmailRadarProfileStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Profile name is required.");
        if (FollowUpDays < 1 || FollowUpDays > 365) throw new ArgumentOutOfRangeException(nameof(FollowUpDays));
        if (DateFrom.HasValue && DateTo.HasValue && DateFrom > DateTo) throw new ArgumentException("Date range is invalid.");
    }
}

public sealed class ImportedCommunicationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SourceKind { get; set; } = "local-import";
    public string SourceLabel { get; set; } = "";
    public string SourceFile { get; set; } = "";
    public string ExternalReference { get; set; } = "";
    public string ThreadReference { get; set; } = "";
    public DateTimeOffset SentAt { get; set; }
    public string Sender { get; set; } = "";
    public List<string> Recipients { get; set; } = [];
    public string Subject { get; set; } = "";
    public string Text { get; set; } = "";
    public bool HasAttachments { get; set; }
    public string DuplicateKey { get; set; } = "";
    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.Now;
    public string Provenance { get; set; } = "";
    public CommunicationReviewState ReviewState { get; set; } = CommunicationReviewState.NeedsReview;
    public Guid? ConfirmedProfileId { get; set; }
    public string ReviewNote { get; set; } = "";
}

public sealed record CommunicationImportError(int RowNumber, string Message);
public sealed record CommunicationImportPreview(IReadOnlyList<ImportedCommunicationRecord> Records, IReadOnlyList<CommunicationImportError> Errors)
{
    public bool RequiresConfirmation => Records.Count > 0;
}
public sealed record EmailRadarMatchCandidate(ImportedCommunicationRecord Record, int Score, IReadOnlyList<string> Reasons, bool Excluded, bool OutsideDateRange);
public sealed record CommunicationTimelineItem(DateTimeOffset SentAt, CommunicationDirection Direction, string Parties, string Subject, string Snippet, string Provenance, CommunicationReviewState ConfirmationState, IReadOnlyList<string> KeywordFlags);
public sealed record CommunicationSuggestion(CommunicationSuggestionKind Kind, string Label, string Explanation, bool RequiresReview = true);
public sealed record EmailRadarAuditEntry(DateTimeOffset OccurredAt, string Action, string Detail, Guid? ProfileId = null, Guid? RecordId = null);

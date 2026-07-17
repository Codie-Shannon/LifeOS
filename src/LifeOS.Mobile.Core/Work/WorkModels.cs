namespace LifeOS.Mobile.Core.Work;

public enum MobileWorkStatus
{
    Active,
    Waiting,
    Blocked,
    DueSoon,
    RecentlyChanged
}

public enum CommunicationReviewState
{
    PendingReview,
    Accepted,
    Linked,
    Ignored,
    Deferred,
    Conflict
}

public sealed record MobileWorkItem(
    string Id,
    string Client,
    string Title,
    MobileWorkStatus Status,
    string NextAction,
    DateTimeOffset DueUtc,
    int EvidenceComplete,
    int EvidenceTotal,
    IReadOnlyList<string> People,
    IReadOnlyList<string> LinkedFiles);

public sealed record CommunicationCandidate(
    string Id,
    string Provider,
    string SourceKind,
    string Subject,
    string SafePreview,
    DateTimeOffset ReceivedUtc,
    string Freshness,
    CommunicationReviewState ReviewState,
    string Provenance);

public sealed record MeetingContext(
    string Id,
    string Title,
    DateTimeOffset StartsUtc,
    IReadOnlyList<string> Attendees,
    string SafeContext,
    IReadOnlyList<string> CapturedActions);

public sealed record WorkEvidenceItem(
    string Id,
    string Label,
    bool Complete,
    string FileName,
    string Source,
    long SizeBytes);

public sealed record WorkDashboardSnapshot(
    IReadOnlyList<MobileWorkItem> Items,
    IReadOnlyList<CommunicationCandidate> Communications,
    IReadOnlyList<MeetingContext> Meetings,
    IReadOnlyList<WorkEvidenceItem> Evidence);

public sealed record WorkReviewResult(
    string CandidateId,
    CommunicationReviewState State,
    DateTimeOffset ChangedUtc);

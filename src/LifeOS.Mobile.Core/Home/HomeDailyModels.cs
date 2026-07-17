namespace LifeOS.Mobile.Core.Home;

public enum MobileCaptureKind
{
    Task,
    Note,
    ExpenseEvidence,
    PersonFollowUp,
    ProjectIdea
}

public sealed record HomePriority(
    string Id,
    string Workspace,
    string Title,
    string Explanation,
    int Rank,
    bool IsCompleted = false,
    DateTimeOffset? DeferredUntilUtc = null);

public sealed record HomeUpcomingItem(
    string Id,
    string Title,
    string Kind,
    DateTimeOffset StartsUtc,
    string TimeZoneId);

public sealed record HomeWaitingSummary(
    int WaitingOn,
    int Blocked,
    int Overdue,
    int NeedsReview);

public sealed record HomeReviewSummary(
    int IntegrationInboxCount,
    string FreshnessLabel,
    DateTimeOffset? LastSuccessfulSyncUtc);

public sealed record HomeDailyOverview(
    DateOnly Date,
    string CurrentFocus,
    IReadOnlyList<HomePriority> Priorities,
    IReadOnlyList<HomeUpcomingItem> Upcoming,
    HomeWaitingSummary Waiting,
    HomeReviewSummary Review);

public sealed record MobileCaptureDraft(
    string DraftId,
    MobileCaptureKind Kind,
    string Text,
    DateTimeOffset CreatedUtc);

public sealed record HomeActionResult(
    string ItemId,
    string State,
    DateTimeOffset ChangedUtc);

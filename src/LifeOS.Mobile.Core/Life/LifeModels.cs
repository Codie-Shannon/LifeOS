namespace LifeOS.Mobile.Core.Life;

public enum LifeItemStatus
{
    Active,
    DueSoon,
    Waiting,
    Completed,
    NeedsReview
}

public sealed record LifeArea(
    string Id,
    string Name,
    string Summary,
    LifeItemStatus Status,
    int OpenCount);

public sealed record LifeRoutine(
    string Id,
    string Title,
    string Area,
    string Frequency,
    bool CompletedToday);

public sealed record LifeReminder(
    string Id,
    string Title,
    DateTimeOffset DueUtc,
    string Area,
    LifeItemStatus Status);

public sealed record LifeDashboardSnapshot(
    IReadOnlyList<LifeArea> Areas,
    IReadOnlyList<LifeRoutine> Routines,
    IReadOnlyList<LifeReminder> Reminders);

public sealed record LifeActionResult(
    string ItemId,
    string State,
    DateTimeOffset ChangedUtc);

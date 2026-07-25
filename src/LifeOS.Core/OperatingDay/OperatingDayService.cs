namespace LifeOS.Core.OperatingDay;

public enum OperatingBoundary
{
    Work,
    Career,
    Household,
    Personal
}

public enum OperatingItemState
{
    Proposed,
    Accepted,
    Deferred,
    Completed
}

public sealed record OperatingDayItem(
    string Id,
    string Title,
    OperatingBoundary Boundary,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int Pressure,
    bool IsProtected,
    string Source,
    string? WorkSessionId,
    string? ProofId,
    OperatingItemState State);

public sealed record OperatingReminder(
    string ItemId,
    string Title,
    DateTimeOffset DueAt,
    int Priority,
    string Reason,
    bool RequiresReview);

public sealed record OperatingDayPlan(
    DateOnly Date,
    IReadOnlyList<OperatingDayItem> Accepted,
    IReadOnlyList<OperatingDayItem> Proposed,
    IReadOnlyList<OperatingReminder> Reminders,
    IReadOnlyList<string> StopPoints,
    bool ClosureReady);

public sealed class OperatingDayService
{
    public OperatingDayPlan Build(
        IEnumerable<OperatingDayItem> items,
        DateOnly date,
        DateTimeOffset now)
    {
        OperatingDayItem[] selected = items
            .Where(item => DateOnly.FromDateTime(item.StartsAt.LocalDateTime) == date)
            .OrderBy(item => item.StartsAt)
            .ToArray();

        OperatingDayItem[] accepted = selected
            .Where(item => item.State is OperatingItemState.Accepted or OperatingItemState.Completed)
            .ToArray();
        OperatingDayItem[] proposed = selected
            .Where(item => item.State == OperatingItemState.Proposed)
            .ToArray();

        OperatingReminder[] reminders = selected
            .Where(item => item.State != OperatingItemState.Completed)
            .Select(item => new OperatingReminder(
                item.Id,
                item.Title,
                item.StartsAt,
                CalculatePriority(item, now),
                BuildReason(item, now),
                item.State == OperatingItemState.Proposed))
            .OrderByDescending(reminder => reminder.Priority)
            .ThenBy(reminder => reminder.DueAt)
            .ToArray();

        string[] stopPoints = BuildStopPoints(accepted);
        bool linksComplete = accepted
            .Where(item => item.Boundary is OperatingBoundary.Work or OperatingBoundary.Career)
            .All(item => !string.IsNullOrWhiteSpace(item.WorkSessionId) || !string.IsNullOrWhiteSpace(item.ProofId));

        return new OperatingDayPlan(
            date,
            accepted,
            proposed,
            reminders,
            stopPoints,
            proposed.Length == 0 && linksComplete);
    }

    public OperatingDayItem Accept(OperatingDayItem item) =>
        item.State == OperatingItemState.Proposed
            ? item with { State = OperatingItemState.Accepted }
            : throw new InvalidOperationException("Only a proposed item can be accepted.");

    public OperatingDayItem Defer(OperatingDayItem item, DateTimeOffset newStart)
    {
        if (item.State == OperatingItemState.Completed)
        {
            throw new InvalidOperationException("Completed items cannot be deferred.");
        }

        TimeSpan duration = item.EndsAt - item.StartsAt;
        return item with
        {
            StartsAt = newStart,
            EndsAt = newStart + duration,
            State = OperatingItemState.Deferred
        };
    }

    private static int CalculatePriority(OperatingDayItem item, DateTimeOffset now)
    {
        int duePressure = item.StartsAt <= now
            ? 50
            : item.StartsAt - now <= TimeSpan.FromHours(2) ? 25 : 0;
        int protectedPressure = item.IsProtected ? 20 : 0;
        return Math.Clamp(item.Pressure + duePressure + protectedPressure, 0, 100);
    }

    private static string BuildReason(OperatingDayItem item, DateTimeOffset now)
    {
        List<string> reasons = new();
        if (item.State == OperatingItemState.Proposed)
        {
            reasons.Add("requires review");
        }
        if (item.IsProtected)
        {
            reasons.Add("protected time");
        }
        if (item.StartsAt <= now)
        {
            reasons.Add("due now");
        }
        reasons.Add(item.Boundary.ToString().ToLowerInvariant());
        return string.Join(" - ", reasons);
    }

    private static string[] BuildStopPoints(IReadOnlyList<OperatingDayItem> accepted)
    {
        List<string> points = new();
        for (int index = 1; index < accepted.Count; index++)
        {
            OperatingDayItem previous = accepted[index - 1];
            OperatingDayItem current = accepted[index];
            if (previous.Boundary != current.Boundary)
            {
                points.Add($"Stop after {previous.Title}; review the boundary before {current.Title}.");
            }
            if (current.StartsAt < previous.EndsAt)
            {
                points.Add($"{current.Title} overlaps {previous.Title}; choose which commitment wins.");
            }
        }
        return points.ToArray();
    }
}

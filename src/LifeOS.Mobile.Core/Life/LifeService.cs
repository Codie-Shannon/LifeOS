namespace LifeOS.Mobile.Core.Life;

public sealed class LifeService
{
    private readonly Dictionary<string, LifeActionResult> _actions =
        new(StringComparer.Ordinal);

    public LifeDashboardSnapshot BuildSnapshot(DateTimeOffset now)
    {
        var areas = new[]
        {
            new LifeArea(
                "home",
                "Home",
                "Personal admin, maintenance and household follow-ups.",
                LifeItemStatus.Active,
                3),
            new LifeArea(
                "family",
                "Family",
                "Private family commitments with minimized mobile detail.",
                LifeItemStatus.DueSoon,
                2),
            new LifeArea(
                "health",
                "Health",
                "Routine reminders only; no diagnosis or treatment decisions.",
                LifeItemStatus.NeedsReview,
                1),
            new LifeArea(
                "personal",
                "Personal",
                "Learning, hobbies and personal development.",
                LifeItemStatus.Active,
                2)
        };

        var routines = new[]
        {
            new LifeRoutine(
                "routine-plan",
                "Review tomorrow",
                "Personal",
                "Daily",
                false),
            new LifeRoutine(
                "routine-home",
                "Home reset",
                "Home",
                "Daily",
                true),
            new LifeRoutine(
                "routine-family",
                "Check family schedule",
                "Family",
                "Weekly",
                false)
        };

        var reminders = new[]
        {
            new LifeReminder(
                "reminder-admin",
                "Confirm fictional personal-admin appointment",
                now.AddHours(2),
                "Personal",
                LifeItemStatus.DueSoon),
            new LifeReminder(
                "reminder-home",
                "Review home maintenance note",
                now.AddDays(1),
                "Home",
                LifeItemStatus.Active)
        };

        return new LifeDashboardSnapshot(
            areas,
            routines,
            reminders);
    }

    public LifeActionResult CompleteRoutine(
        string routineId,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routineId);

        if (_actions.TryGetValue(routineId, out var existing) &&
            existing.State == "Completed")
        {
            return existing;
        }

        var result = new LifeActionResult(
            routineId,
            "Completed",
            now);

        _actions[routineId] = result;
        return result;
    }

    public LifeActionResult DeferReminder(
        string reminderId,
        DateTimeOffset untilUtc,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reminderId);

        if (untilUtc <= now)
        {
            throw new ArgumentOutOfRangeException(
                nameof(untilUtc),
                "Deferred time must be in the future.");
        }

        var state = $"Deferred until {untilUtc:O}";

        if (_actions.TryGetValue(reminderId, out var existing) &&
            existing.State == state)
        {
            return existing;
        }

        var result = new LifeActionResult(
            reminderId,
            state,
            now);

        _actions[reminderId] = result;
        return result;
    }
}

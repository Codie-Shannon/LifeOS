using LifeOS.Core.Forms;

namespace LifeOS.Core.FocusTimers;

public enum FocusTimerKind { Work, PersonalAdmin, Household, Wellbeing, Learning, Other }
public enum FocusTimerState { Planned, Running, Paused, Completed, Cancelled, Archived }

public sealed record FocusTimerRecord(
    string Id,
    string Title,
    string Area,
    FocusTimerKind Kind,
    int? TargetMinutes,
    string NextAction,
    string? Notes,
    FocusTimerState State,
    long AccumulatedSeconds,
    DateTimeOffset? SegmentStartedUtc,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    DateTimeOffset? CompletedUtc);

public sealed record FocusTimerDraft(
    string? Title,
    string? Area,
    FocusTimerKind Kind,
    string? TargetMinutes,
    string? NextAction,
    string? Notes);

public static class FocusTimerService
{
    public static FormValidationResult Validate(FocusTimerDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("focus-title", draft.Title, "Title"));
        Add(issues, FormValidation.MaximumLength("focus-title", draft.Title, "Title", 100));
        Add(issues, FormValidation.SingleLine("focus-title", draft.Title, "Title"));
        Add(issues, FormValidation.Required("focus-area", draft.Area, "Area"));
        Add(issues, FormValidation.MaximumLength("focus-area", draft.Area, "Area", 80));
        Add(issues, FormValidation.SingleLine("focus-area", draft.Area, "Area"));
        Add(issues, FormValidation.Required("focus-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("focus-next-action", draft.NextAction, "Next action", 200));
        Add(issues, FormValidation.SingleLine("focus-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("focus-notes", draft.Notes, "Notes", 1000));
        if (!string.IsNullOrWhiteSpace(draft.TargetMinutes) &&
            (!int.TryParse(draft.TargetMinutes.Trim(), out int minutes) || minutes is < 1 or > 720))
            issues.Add(new FormFieldIssue("focus-target", "range", "Target minutes must be a whole number from 1 to 720."));
        return new FormValidationResult(issues);
    }

    public static FocusTimerRecord Create(FocusTimerDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The focus timer is invalid.", nameof(draft));
        return Normalize(new FocusTimerRecord(
            Guid.NewGuid().ToString("N"), draft.Title!.Trim(), draft.Area!.Trim(), draft.Kind,
            string.IsNullOrWhiteSpace(draft.TargetMinutes) ? null : int.Parse(draft.TargetMinutes.Trim()),
            draft.NextAction!.Trim(), Empty(draft.Notes), FocusTimerState.Planned, 0, null, now, now, null));
    }

    public static FocusTimerRecord Transition(FocusTimerRecord record, FocusTimerState next, DateTimeOffset now)
    {
        if (now < record.UpdatedUtc) throw new ArgumentOutOfRangeException(nameof(now), "Timer time cannot move backwards.");
        long elapsed = record.AccumulatedSeconds;
        if (record.State == FocusTimerState.Running && record.SegmentStartedUtc is not null)
            elapsed = checked(elapsed + Math.Max(0, (long)(now - record.SegmentStartedUtc.Value).TotalSeconds));
        bool valid = record.State switch
        {
            FocusTimerState.Planned => next is FocusTimerState.Running or FocusTimerState.Cancelled,
            FocusTimerState.Running => next is FocusTimerState.Paused or FocusTimerState.Completed or FocusTimerState.Cancelled,
            FocusTimerState.Paused => next is FocusTimerState.Running or FocusTimerState.Completed or FocusTimerState.Cancelled,
            FocusTimerState.Completed => next == FocusTimerState.Archived,
            FocusTimerState.Cancelled => next is FocusTimerState.Planned or FocusTimerState.Archived,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Invalid focus-timer transition: {record.State} -> {next}.");
        return record with
        {
            State = next,
            AccumulatedSeconds = elapsed,
            SegmentStartedUtc = next == FocusTimerState.Running ? now : null,
            UpdatedUtc = now,
            CompletedUtc = next == FocusTimerState.Completed ? now : record.CompletedUtc
        };
    }

    public static TimeSpan Duration(FocusTimerRecord record, DateTimeOffset now)
    {
        long seconds = record.AccumulatedSeconds;
        if (record.State == FocusTimerState.Running && record.SegmentStartedUtc is not null)
            seconds = checked(seconds + Math.Max(0, (long)(now - record.SegmentStartedUtc.Value).TotalSeconds));
        return TimeSpan.FromSeconds(seconds);
    }

    public static FocusTimerRecord Normalize(FocusTimerRecord record)
    {
        if (record.AccumulatedSeconds < 0 || record.TargetMinutes is < 1 or > 720) throw new InvalidDataException("The focus-timer duration is invalid.");
        if (record.State == FocusTimerState.Running && record.SegmentStartedUtc is null) throw new InvalidDataException("A running focus timer requires a segment start.");
        if (record.State != FocusTimerState.Running && record.SegmentStartedUtc is not null) throw new InvalidDataException("Only a running focus timer can retain a segment start.");
        return record with { Id = Required(record.Id, "id", 64), Title = Required(record.Title, "title", 100), Area = Required(record.Area, "area", 80), NextAction = Required(record.NextAction, "next action", 200), Notes = OptionalText(record.Notes, "notes", 1000) };
    }

    private static string Required(string? value, string label, int maximum)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is 0 || normalized.Length > maximum || normalized.Any(char.IsControl)) throw new InvalidDataException($"The focus-timer {label} is invalid.");
        return normalized;
    }
    private static string? OptionalText(string? value, string label, int maximum)
    {
        string? normalized = Empty(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))) throw new InvalidDataException($"The focus-timer {label} is invalid.");
        return normalized;
    }
    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}

using System.Globalization;
using LifeOS.Core.Forms;

namespace LifeOS.Core.Life;

public enum LifeRoutineKind { Routine, PersonalAdmin, Appointment, Maintenance, Wellbeing, Family, Other }
public enum LifeRoutinePressure { Low, Normal, High, Critical }
public enum LifeRoutineState { Planned, Active, Waiting, Deferred, Done, Archived }

public sealed record LifeRoutineRecord(
    string Id,
    DateOnly Date,
    string Title,
    string Area,
    LifeRoutineKind Kind,
    LifeRoutinePressure Pressure,
    LifeRoutineState State,
    string NextAction,
    string? TimeWindow,
    string? Notes,
    bool Pinned,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record LifeRoutineDraft(
    string? Date,
    string? Title,
    string? Area,
    LifeRoutineKind Kind,
    LifeRoutinePressure Pressure,
    string? NextAction,
    string? TimeWindow,
    string? Notes,
    bool Pinned);

public static class LifeRoutineService
{
    public static FormValidationResult Validate(LifeRoutineDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("life-date", draft.Date, "Date"));
        Add(issues, FormValidation.Required("life-title", draft.Title, "Title"));
        Add(issues, FormValidation.MaximumLength("life-title", draft.Title, "Title", 100));
        Add(issues, FormValidation.SingleLine("life-title", draft.Title, "Title"));
        Add(issues, FormValidation.Required("life-area", draft.Area, "Area"));
        Add(issues, FormValidation.MaximumLength("life-area", draft.Area, "Area", 80));
        Add(issues, FormValidation.SingleLine("life-area", draft.Area, "Area"));
        Add(issues, FormValidation.Required("life-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("life-next-action", draft.NextAction, "Next action", 200));
        Add(issues, FormValidation.SingleLine("life-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("life-time-window", draft.TimeWindow, "Time window", 50));
        Add(issues, FormValidation.SingleLine("life-time-window", draft.TimeWindow, "Time window"));
        Add(issues, FormValidation.MaximumLength("life-notes", draft.Notes, "Notes", 1000));
        if (!string.IsNullOrWhiteSpace(draft.Date) &&
            !DateOnly.TryParseExact(draft.Date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            issues.Add(new FormFieldIssue("life-date", "date-format", "Date must use YYYY-MM-DD."));
        return new FormValidationResult(issues);
    }

    public static LifeRoutineRecord Create(LifeRoutineDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The life routine is invalid.", nameof(draft));
        return Normalize(new LifeRoutineRecord(
            Guid.NewGuid().ToString("N"),
            DateOnly.ParseExact(draft.Date!.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
            draft.Title!.Trim(), draft.Area!.Trim(), draft.Kind, draft.Pressure,
            LifeRoutineState.Planned, draft.NextAction!.Trim(), Empty(draft.TimeWindow),
            Empty(draft.Notes), draft.Pinned, now, now));
    }

    public static LifeRoutineRecord Transition(LifeRoutineRecord record, LifeRoutineState next, DateTimeOffset now)
    {
        bool valid = record.State switch
        {
            LifeRoutineState.Planned => next is LifeRoutineState.Active or LifeRoutineState.Waiting or LifeRoutineState.Deferred or LifeRoutineState.Done,
            LifeRoutineState.Active => next is LifeRoutineState.Waiting or LifeRoutineState.Deferred or LifeRoutineState.Done,
            LifeRoutineState.Waiting => next is LifeRoutineState.Active or LifeRoutineState.Deferred or LifeRoutineState.Done,
            LifeRoutineState.Deferred => next is LifeRoutineState.Planned or LifeRoutineState.Active or LifeRoutineState.Done,
            LifeRoutineState.Done => next == LifeRoutineState.Archived,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Invalid life-routine transition: {record.State} -> {next}.");
        return record with { State = next, UpdatedUtc = now };
    }

    public static LifeRoutineRecord Normalize(LifeRoutineRecord record)
    {
        string title = Required(record.Title, "title", 100);
        string area = Required(record.Area, "area", 80);
        string action = Required(record.NextAction, "next action", 200);
        return record with
        {
            Title = title, Area = area, NextAction = action,
            TimeWindow = Bounded(record.TimeWindow, "time window", 50),
            Notes = Bounded(record.Notes, "notes", 1000)
        };
    }

    private static string Required(string? value, string label, int maximum)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is 0 || normalized.Length > maximum || normalized.Any(character => character is '\r' or '\n' || char.IsControl(character)))
            throw new InvalidDataException($"The life-routine {label} is invalid.");
        return normalized;
    }

    private static string? Bounded(string? value, string label, int maximum)
    {
        string? normalized = Empty(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(char.IsControl)))
            throw new InvalidDataException($"The life-routine {label} is invalid.");
        return normalized;
    }

    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}

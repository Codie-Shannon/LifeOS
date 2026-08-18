using System.Globalization;
using LifeOS.Core.Forms;

namespace LifeOS.Core.WeeklyReview;

public enum WeeklyReviewPressure { Low, Normal, High, Critical }
public enum WeeklyReviewState { Draft, Ready, Closed, Archived }

public sealed record WeeklyReviewRecord(
    string Id,
    DateOnly WeekStart,
    string WhatGotDone,
    string WhatMoved,
    string WaitingOn,
    WeeklyReviewPressure Pressure,
    string NextWeekFocus,
    string? Notes,
    WeeklyReviewState State,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record WeeklyReviewDraft(
    string? WeekStart,
    string? WhatGotDone,
    string? WhatMoved,
    string? WaitingOn,
    WeeklyReviewPressure Pressure,
    string? NextWeekFocus,
    string? Notes);

public static class WeeklyReviewService
{
    public static FormValidationResult Validate(WeeklyReviewDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("weekly-review-week", draft.WeekStart, "Week start"));
        Add(issues, FormValidation.Required("weekly-review-done", draft.WhatGotDone, "What got done"));
        Add(issues, FormValidation.MaximumLength("weekly-review-done", draft.WhatGotDone, "What got done", 1000));
        Add(issues, FormValidation.MaximumLength("weekly-review-moved", draft.WhatMoved, "What moved", 1000));
        Add(issues, FormValidation.MaximumLength("weekly-review-waiting", draft.WaitingOn, "Waiting on", 1000));
        Add(issues, FormValidation.Required("weekly-review-focus", draft.NextWeekFocus, "Next-week focus"));
        Add(issues, FormValidation.MaximumLength("weekly-review-focus", draft.NextWeekFocus, "Next-week focus", 500));
        Add(issues, FormValidation.MaximumLength("weekly-review-notes", draft.Notes, "Notes", 2000));
        if (!string.IsNullOrWhiteSpace(draft.WeekStart) &&
            !DateOnly.TryParseExact(draft.WeekStart.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            issues.Add(new FormFieldIssue("weekly-review-week", "date-format", "Week start must use YYYY-MM-DD."));
        return new FormValidationResult(issues);
    }

    public static WeeklyReviewRecord Create(WeeklyReviewDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The weekly review is invalid.", nameof(draft));
        return Normalize(new WeeklyReviewRecord(
            Guid.NewGuid().ToString("N"),
            DateOnly.ParseExact(draft.WeekStart!.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
            draft.WhatGotDone!.Trim(), Empty(draft.WhatMoved) ?? string.Empty,
            Empty(draft.WaitingOn) ?? string.Empty, draft.Pressure, draft.NextWeekFocus!.Trim(),
            Empty(draft.Notes), WeeklyReviewState.Draft, now, now));
    }

    public static WeeklyReviewRecord Transition(WeeklyReviewRecord record, WeeklyReviewState next, DateTimeOffset now)
    {
        bool valid = record.State switch
        {
            WeeklyReviewState.Draft => next == WeeklyReviewState.Ready,
            WeeklyReviewState.Ready => next is WeeklyReviewState.Draft or WeeklyReviewState.Closed,
            WeeklyReviewState.Closed => next == WeeklyReviewState.Archived,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Invalid weekly-review transition: {record.State} -> {next}.");
        return record with { State = next, UpdatedUtc = now };
    }

    public static WeeklyReviewRecord Normalize(WeeklyReviewRecord record) => record with
    {
        Id = RequiredSingleLine(record.Id, "id", 64),
        WhatGotDone = RequiredText(record.WhatGotDone, "what got done", 1000),
        WhatMoved = OptionalText(record.WhatMoved, "what moved", 1000) ?? string.Empty,
        WaitingOn = OptionalText(record.WaitingOn, "waiting on", 1000) ?? string.Empty,
        NextWeekFocus = RequiredText(record.NextWeekFocus, "next-week focus", 500),
        Notes = OptionalText(record.Notes, "notes", 2000)
    };

    private static string RequiredSingleLine(string? value, string label, int maximum)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is 0 || normalized.Length > maximum || normalized.Any(char.IsControl))
            throw new InvalidDataException($"The weekly-review {label} is invalid.");
        return normalized;
    }

    private static string RequiredText(string? value, string label, int maximum) =>
        OptionalText(value, label, maximum) ?? throw new InvalidDataException($"The weekly-review {label} is required.");

    private static string? OptionalText(string? value, string label, int maximum)
    {
        string? normalized = Empty(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t')))
            throw new InvalidDataException($"The weekly-review {label} is invalid.");
        return normalized;
    }

    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}

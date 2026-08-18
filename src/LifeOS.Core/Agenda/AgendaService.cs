using System.Globalization;
using LifeOS.Core.Forms;

namespace LifeOS.Core.Agenda;

public sealed record AgendaDraft(
    string? Title,
    string? DueDate,
    string? TimeText,
    AgendaItemType Type,
    AgendaPressureLevel Pressure,
    string? NextAction,
    string? Notes,
    bool IsFixedCommitment);

public static class AgendaService
{
    public static FormValidationResult Validate(AgendaDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("agenda-title", draft.Title, "Title"));
        Add(issues, FormValidation.MaximumLength("agenda-title", draft.Title, "Title", 100));
        Add(issues, FormValidation.SingleLine("agenda-title", draft.Title, "Title"));
        Add(issues, FormValidation.MaximumLength("agenda-time", draft.TimeText, "Time", 50));
        Add(issues, FormValidation.SingleLine("agenda-time", draft.TimeText, "Time"));
        Add(issues, FormValidation.Required("agenda-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("agenda-next-action", draft.NextAction, "Next action", 200));
        Add(issues, FormValidation.SingleLine("agenda-next-action", draft.NextAction, "Next action"));
        Add(issues, FormValidation.MaximumLength("agenda-notes", draft.Notes, "Notes", 1000));
        if (!string.IsNullOrWhiteSpace(draft.DueDate) &&
            !DateOnly.TryParseExact(draft.DueDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            issues.Add(new FormFieldIssue("agenda-due-date", "date-format", "Due date must use YYYY-MM-DD."));
        return new FormValidationResult(issues);
    }

    public static AgendaItem Create(AgendaDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The agenda item is invalid.", nameof(draft));
        return Normalize(new AgendaItem
        {
            Id = Guid.NewGuid(), Title = draft.Title!.Trim(), Type = draft.Type,
            Status = AgendaItemStatus.Planned, PressureLevel = draft.Pressure,
            DueDate = string.IsNullOrWhiteSpace(draft.DueDate) ? null : DateOnly.ParseExact(draft.DueDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeText = Empty(draft.TimeText) ?? string.Empty, NextAction = draft.NextAction!.Trim(),
            Notes = Empty(draft.Notes) ?? string.Empty, IsFixedCommitment = draft.IsFixedCommitment,
            CreatedAt = now.LocalDateTime, UpdatedAt = now.LocalDateTime
        });
    }

    public static AgendaItem Transition(AgendaItem item, AgendaItemStatus next, DateTimeOffset now)
    {
        bool valid = item.Status switch
        {
            AgendaItemStatus.Planned => next is AgendaItemStatus.InProgress or AgendaItemStatus.Waiting or AgendaItemStatus.Parked or AgendaItemStatus.Completed or AgendaItemStatus.Cancelled,
            AgendaItemStatus.InProgress => next is AgendaItemStatus.Waiting or AgendaItemStatus.Parked or AgendaItemStatus.Completed or AgendaItemStatus.Cancelled,
            AgendaItemStatus.Waiting => next is AgendaItemStatus.InProgress or AgendaItemStatus.Parked or AgendaItemStatus.Completed or AgendaItemStatus.Cancelled,
            AgendaItemStatus.Parked => next is AgendaItemStatus.Planned or AgendaItemStatus.InProgress or AgendaItemStatus.Cancelled,
            AgendaItemStatus.Completed => next == AgendaItemStatus.Planned,
            AgendaItemStatus.Cancelled => next == AgendaItemStatus.Planned,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Invalid agenda transition: {item.Status} -> {next}.");
        AgendaItem changed = Clone(item); changed.Status = next; changed.UpdatedAt = now.LocalDateTime; return changed;
    }

    public static AgendaItem Normalize(AgendaItem item)
    {
        if (item.Id == Guid.Empty) throw new InvalidDataException("The agenda id is invalid.");
        AgendaItem normalized = Clone(item);
        normalized.Title = Required(item.Title, "title", 100);
        normalized.TimeText = OptionalSingleLine(item.TimeText, "time", 50) ?? string.Empty;
        normalized.NextAction = string.IsNullOrWhiteSpace(item.NextAction)
            ? $"Review: {normalized.Title}"
            : Required(item.NextAction, "next action", 200);
        normalized.Notes = OptionalText(item.Notes, "notes", 1000) ?? string.Empty;
        return normalized;
    }

    private static AgendaItem Clone(AgendaItem item) => new()
    {
        Id = item.Id, Title = item.Title, Type = item.Type, Status = item.Status,
        PressureLevel = item.PressureLevel, DueDate = item.DueDate, TimeText = item.TimeText,
        NextAction = item.NextAction, IsFixedCommitment = item.IsFixedCommitment,
        Notes = item.Notes, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt
    };
    private static string Required(string? value, string label, int maximum) => OptionalSingleLine(value, label, maximum) ?? throw new InvalidDataException($"The agenda {label} is required.");
    private static string? OptionalSingleLine(string? value, string label, int maximum)
    {
        string? normalized = Empty(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(char.IsControl))) throw new InvalidDataException($"The agenda {label} is invalid.");
        return normalized;
    }
    private static string? OptionalText(string? value, string label, int maximum)
    {
        string? normalized = Empty(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))) throw new InvalidDataException($"The agenda {label} is invalid.");
        return normalized;
    }
    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}

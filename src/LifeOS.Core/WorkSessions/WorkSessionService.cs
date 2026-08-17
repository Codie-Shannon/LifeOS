using LifeOS.Core.Forms;

namespace LifeOS.Core.WorkSessions;

public sealed record WorkSessionDraft(
    string? ClientOrProject,
    DateOnly Date,
    decimal Hours,
    decimal HourlyRate,
    bool IsBillable,
    WorkSessionStatus Status,
    string? Description,
    string? Notes);

public static class WorkSessionService
{
    public static FormValidationResult Validate(WorkSessionDraft draft)
    {
        List<FormFieldIssue> issues =
        [
            .. RequiredAndBoundedSingleLine(
                "work-client-project",
                draft.ClientOrProject,
                "Client or project",
                160),
            .. RequiredAndBoundedSingleLine(
                "work-description",
                draft.Description,
                "Description",
                500),
            .. OptionalBounded("work-notes", draft.Notes, "Notes", 4000)
        ];

        if (draft.Hours <= 0m || draft.Hours > 24m)
        {
            issues.Add(new FormFieldIssue(
                "work-hours",
                "hours-range",
                "Hours must be greater than 0 and no more than 24."));
        }

        if (draft.HourlyRate < 0m || draft.HourlyRate > 1_000_000m)
        {
            issues.Add(new FormFieldIssue(
                "work-hourly-rate",
                "rate-range",
                "Hourly rate must be between 0 and 1,000,000."));
        }

        if (draft.IsBillable && draft.Status == WorkSessionStatus.NonBillable)
        {
            issues.Add(new FormFieldIssue(
                "work-status",
                "billable-status",
                "A billable session cannot use the NonBillable status."));
        }

        return new FormValidationResult(issues);
    }

    public static WorkSession Create(WorkSessionDraft draft, DateTime recordedAt)
    {
        FormValidationResult validation = Validate(draft);
        if (!validation.IsValid)
            throw new ArgumentException("The work session is invalid.", nameof(draft));

        bool billable = draft.IsBillable;
        return new WorkSession
        {
            ClientOrProject = draft.ClientOrProject!.Trim(),
            Date = draft.Date,
            Hours = draft.Hours,
            HourlyRate = billable ? draft.HourlyRate : 0m,
            IsBillable = billable,
            Status = billable ? draft.Status : WorkSessionStatus.NonBillable,
            Description = draft.Description!.Trim(),
            Notes = (draft.Notes ?? string.Empty).Trim(),
            CreatedAt = recordedAt,
            UpdatedAt = recordedAt
        };
    }

    public static WorkSession ChangeStatus(
        WorkSession session,
        WorkSessionStatus status,
        DateTime changedAt)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.IsBillable && status != WorkSessionStatus.NonBillable)
            throw new InvalidOperationException("A non-billable session must retain its non-billable status.");
        if (session.IsBillable && status == WorkSessionStatus.NonBillable)
            throw new InvalidOperationException("Change billable classification before using the non-billable status.");

        WorkSession changed = Copy(session);
        changed.Status = status;
        changed.UpdatedAt = changedAt;
        return changed;
    }

    public static WorkSession Normalize(WorkSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        WorkSession normalized = Copy(session);
        normalized.ClientOrProject = (session.ClientOrProject ?? string.Empty).Trim();
        normalized.Description = (session.Description ?? string.Empty).Trim();
        normalized.Notes = (session.Notes ?? string.Empty).Trim();
        if (!normalized.IsBillable)
        {
            normalized.HourlyRate = 0m;
            normalized.Status = WorkSessionStatus.NonBillable;
        }
        return normalized;
    }

    private static IEnumerable<FormFieldIssue> RequiredAndBoundedSingleLine(
        string fieldId,
        string? value,
        string label,
        int maximumLength) => new FormFieldIssue?[]
        {
            FormValidation.Required(fieldId, value, label),
            FormValidation.MaximumLength(fieldId, value, label, maximumLength),
            FormValidation.SingleLine(fieldId, value, label)
        }
        .Where(issue => issue is not null)
        .Cast<FormFieldIssue>();

    private static IEnumerable<FormFieldIssue> OptionalBounded(
        string fieldId,
        string? value,
        string label,
        int maximumLength) => new FormFieldIssue?[]
        {
            FormValidation.MaximumLength(fieldId, value, label, maximumLength)
        }
        .Where(issue => issue is not null)
        .Cast<FormFieldIssue>();

    private static WorkSession Copy(WorkSession session) => new()
    {
        Id = session.Id,
        ClientOrProject = session.ClientOrProject,
        Date = session.Date,
        Hours = session.Hours,
        HourlyRate = session.HourlyRate,
        IsBillable = session.IsBillable,
        Status = session.Status,
        Description = session.Description,
        Notes = session.Notes,
        CreatedAt = session.CreatedAt,
        UpdatedAt = session.UpdatedAt
    };
}

using LifeOS.Core.Forms;

namespace LifeOS.Core.Projects;

public enum ProjectStatus
{
    Backlog,
    Active,
    Waiting,
    Blocked,
    Completed,
    Archived
}

public sealed record ProjectRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public ProjectStatus Status { get; init; } = ProjectStatus.Active;
    public string NextAction { get; init; } = string.Empty;
    public DateOnly? DueDate { get; init; }
    public string EvidenceReference { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedUtc { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record ProjectDraft(
    string? Name,
    string? Summary,
    ProjectStatus Status,
    string? NextAction,
    DateOnly? DueDate,
    string? EvidenceReference,
    string? Notes);

public sealed record ProjectOverview(
    int Visible,
    int Active,
    int Waiting,
    int Blocked,
    int DueNextSevenDays,
    int Completed,
    int Archived);

public static class ProjectService
{
    public static FormValidationResult Validate(ProjectDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return FormValidation.Combine(
            FormValidation.Required("project-name", draft.Name, "Project name"),
            FormValidation.MaximumLength("project-name", draft.Name, "Project name", 120),
            FormValidation.SingleLine("project-name", draft.Name, "Project name"),
            FormValidation.MaximumLength("project-summary", draft.Summary, "Summary", 2000),
            FormValidation.Required("project-next-action", draft.NextAction, "Next action"),
            FormValidation.MaximumLength("project-next-action", draft.NextAction, "Next action", 240),
            FormValidation.SingleLine("project-next-action", draft.NextAction, "Next action"),
            FormValidation.MaximumLength("project-evidence", draft.EvidenceReference, "Evidence reference", 500),
            FormValidation.SingleLine("project-evidence", draft.EvidenceReference, "Evidence reference"),
            FormValidation.MaximumLength("project-notes", draft.Notes, "Notes", 4000));
    }

    public static ProjectRecord Create(ProjectDraft draft, DateTimeOffset now)
    {
        FormValidationResult validation = Validate(draft);
        if (!validation.IsValid)
            throw new ArgumentException("The project draft is invalid.", nameof(draft));

        return Normalize(new ProjectRecord
        {
            Name = draft.Name!,
            Summary = draft.Summary ?? string.Empty,
            Status = draft.Status,
            NextAction = draft.NextAction!,
            DueDate = draft.DueDate,
            EvidenceReference = draft.EvidenceReference ?? string.Empty,
            Notes = draft.Notes ?? string.Empty,
            CreatedUtc = now,
            UpdatedUtc = now
        });
    }

    public static ProjectRecord ChangeStatus(
        ProjectRecord project,
        ProjectStatus status,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(project);
        return Normalize(project with { Status = status, UpdatedUtc = now });
    }

    public static ProjectOverview Calculate(
        IEnumerable<ProjectRecord> projects,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(projects);
        ProjectRecord[] all = projects.Select(Normalize).ToArray();
        ProjectRecord[] visible = all
            .Where(project => project.Status != ProjectStatus.Archived)
            .ToArray();
        DateOnly end = today.AddDays(7);

        return new ProjectOverview(
            visible.Length,
            visible.Count(project => project.Status == ProjectStatus.Active),
            visible.Count(project => project.Status == ProjectStatus.Waiting),
            visible.Count(project => project.Status == ProjectStatus.Blocked),
            visible.Count(project =>
                project.Status is not ProjectStatus.Completed &&
                project.DueDate is not null &&
                project.DueDate <= end),
            visible.Count(project => project.Status == ProjectStatus.Completed),
            all.Count(project => project.Status == ProjectStatus.Archived));
    }

    public static ProjectRecord Normalize(ProjectRecord project)
    {
        ArgumentNullException.ThrowIfNull(project);
        DateTimeOffset created = project.CreatedUtc == default
            ? DateTimeOffset.UtcNow
            : project.CreatedUtc;
        DateTimeOffset updated = project.UpdatedUtc == default
            ? created
            : project.UpdatedUtc;

        return project with
        {
            Id = project.Id == Guid.Empty ? Guid.NewGuid() : project.Id,
            Name = (project.Name ?? string.Empty).Trim(),
            Summary = (project.Summary ?? string.Empty).Trim(),
            NextAction = (project.NextAction ?? string.Empty).Trim(),
            EvidenceReference = (project.EvidenceReference ?? string.Empty).Trim(),
            Notes = (project.Notes ?? string.Empty).Trim(),
            CreatedUtc = created,
            UpdatedUtc = updated
        };
    }
}

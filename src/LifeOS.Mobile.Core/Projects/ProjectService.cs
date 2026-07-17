namespace LifeOS.Mobile.Core.Projects;

public sealed class ProjectService
{
    private readonly Dictionary<string, ProjectActionResult> _actions =
        new(StringComparer.Ordinal);

    public ProjectDashboardSnapshot BuildSnapshot(DateTimeOffset now)
    {
        var projects = new[]
        {
            new MobileProject(
                "lifeos-mobile",
                "LifeOS Full Mobile",
                MobileProjectStatus.Active,
                "Group 56 Life and Projects",
                "Complete manual proof and evidence pack",
                83,
                [
                    new ProjectMilestone(
                        "g56",
                        "Group 56 proof",
                        now.AddDays(1),
                        false),
                    new ProjectMilestone(
                        "g57",
                        "Beta closure",
                        now.AddDays(3),
                        false)
                ],
                [
                    new ProjectEvidence(
                        "ev-mobile-plan",
                        "Release plan",
                        "full-mobile-release-plan.md",
                        true,
                        "Trusted project file"),
                    new ProjectEvidence(
                        "ev-mobile-proof",
                        "Group 56 screenshots",
                        "pending",
                        false,
                        "Manual proof")
                ]),
            new MobileProject(
                "family-archive",
                "Family Archive",
                MobileProjectStatus.Waiting,
                "Next approved screenshot group",
                "Wait for explicit next-group start",
                72,
                [
                    new ProjectMilestone(
                        "fa-next",
                        "Next group",
                        now.AddDays(4),
                        false)
                ],
                [
                    new ProjectEvidence(
                        "ev-fa-close",
                        "Latest closure proof",
                        "group-06",
                        true,
                        "Repository evidence")
                ]),
            new MobileProject(
                "hardware-command",
                "Hardware Command Centre",
                MobileProjectStatus.Parked,
                "DesktopGateway planning",
                "Remain parked until deliberately resumed",
                20,
                [],
                []),
            new MobileProject(
                "client-proof",
                "Fictional Client Proof",
                MobileProjectStatus.Blocked,
                "External review",
                "Wait for sanitized review response",
                60,
                [],
                [
                    new ProjectEvidence(
                        "ev-client",
                        "Proof package",
                        "client-proof.zip",
                        true,
                        "Linked file metadata")
                ])
        };

        return new ProjectDashboardSnapshot(
            projects,
            projects.Count(x => x.Status == MobileProjectStatus.Active),
            projects.Count(x => x.Status == MobileProjectStatus.Waiting),
            projects.Count(x => x.Status == MobileProjectStatus.Blocked),
            projects.Count(x => x.Status == MobileProjectStatus.Parked));
    }

    public IReadOnlyList<MobileProject> Filter(
        ProjectDashboardSnapshot snapshot,
        MobileProjectStatus? status,
        string? query)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        IEnumerable<MobileProject> result = snapshot.Projects;

        if (status is not null)
        {
            result = result.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();

            result = result.Where(
                x => x.Name.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.CurrentMilestone.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.NextAction.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase));
        }

        return result
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public ProjectActionResult Park(
        string projectId,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        if (_actions.TryGetValue(projectId, out var existing) &&
            existing.State == "Parked")
        {
            return existing;
        }

        var result = new ProjectActionResult(
            projectId,
            "Parked",
            now);

        _actions[projectId] = result;
        return result;
    }

    public ProjectActionResult Resume(
        string projectId,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        if (_actions.TryGetValue(projectId, out var existing) &&
            existing.State == "Active")
        {
            return existing;
        }

        var result = new ProjectActionResult(
            projectId,
            "Active",
            now);

        _actions[projectId] = result;
        return result;
    }
}

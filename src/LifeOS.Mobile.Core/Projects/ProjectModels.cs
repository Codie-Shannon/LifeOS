namespace LifeOS.Mobile.Core.Projects;

public enum MobileProjectStatus
{
    Active,
    Waiting,
    Blocked,
    Parked,
    Completed
}

public sealed record ProjectMilestone(
    string Id,
    string Title,
    DateTimeOffset DueUtc,
    bool IsComplete);

public sealed record ProjectEvidence(
    string Id,
    string Label,
    string FileName,
    bool Complete,
    string Source);

public sealed record MobileProject(
    string Id,
    string Name,
    MobileProjectStatus Status,
    string CurrentMilestone,
    string NextAction,
    int ProgressPercent,
    IReadOnlyList<ProjectMilestone> Milestones,
    IReadOnlyList<ProjectEvidence> Evidence);

public sealed record ProjectDashboardSnapshot(
    IReadOnlyList<MobileProject> Projects,
    int ActiveCount,
    int WaitingCount,
    int BlockedCount,
    int ParkedCount);

public sealed record ProjectActionResult(
    string ProjectId,
    string State,
    DateTimeOffset ChangedUtc);

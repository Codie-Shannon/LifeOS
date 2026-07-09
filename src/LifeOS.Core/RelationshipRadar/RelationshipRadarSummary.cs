namespace LifeOS.Core.RelationshipRadar;

public sealed class RelationshipRadarSummary
{
    public int TotalProfiles { get; init; }

    public int WaitingOnThemCount { get; init; }

    public int WaitingOnMeCount { get; init; }

    public int FollowUpDueCount { get; init; }

    public int DoNotChaseCount { get; init; }

    public IReadOnlyList<RelationshipRadarProfile> FollowUpDueProfiles { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}

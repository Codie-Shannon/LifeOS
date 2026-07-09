namespace LifeOS.Core.RelationshipRadar;

public static class RelationshipRadarCalculator
{
    public static RelationshipRadarSummary Calculate(IEnumerable<RelationshipRadarProfile> profiles, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        var list = profiles.ToList();
        var open = list.Where(profile => profile.Status != RelationshipRadarStatus.Closed).ToList();
        var followUpDue = open
            .Where(profile => !profile.DoNotChase)
            .Where(profile => profile.NextFollowUpDate is not null && profile.NextFollowUpDate <= today)
            .OrderBy(profile => profile.NextFollowUpDate)
            .ThenBy(profile => profile.Name)
            .ToList();

        var reasons = new List<string>();
        if (followUpDue.Count > 0) reasons.Add($"{followUpDue.Count} relationship follow-up(s) are due or overdue.");
        var waitingOnThem = open.Count(profile => profile.WaitingOn == RelationshipWaitingOn.Them || profile.Status == RelationshipRadarStatus.WaitingOnThem);
        if (waitingOnThem > 0) reasons.Add($"{waitingOnThem} relationship(s) are waiting on them.");
        var waitingOnMe = open.Count(profile => profile.WaitingOn == RelationshipWaitingOn.Me || profile.Status == RelationshipRadarStatus.WaitingOnMe);
        if (waitingOnMe > 0) reasons.Add($"{waitingOnMe} relationship(s) are waiting on you.");
        var doNotChase = open.Count(profile => profile.DoNotChase || profile.Status == RelationshipRadarStatus.DoNotChaseYet);
        if (doNotChase > 0) reasons.Add($"{doNotChase} relationship(s) are protected by do-not-chase state.");
        if (reasons.Count == 0) reasons.Add("Relationship Radar has no urgent relationship pressure right now.");

        return new RelationshipRadarSummary
        {
            TotalProfiles = open.Count,
            WaitingOnThemCount = waitingOnThem,
            WaitingOnMeCount = waitingOnMe,
            FollowUpDueCount = followUpDue.Count,
            DoNotChaseCount = doNotChase,
            FollowUpDueProfiles = followUpDue,
            Reasons = reasons
        };
    }
}

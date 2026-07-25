namespace LifeOS.Core.ReleaseCandidate;

public enum ReleaseSurface
{
    Desktop,
    Mobile,
    Website,
    Documentation,
    Evidence,
    Repository
}

public sealed record ReleaseSurfaceCheck(
    ReleaseSurface Surface,
    string Check,
    bool Passed,
    string Evidence);

public sealed record ReleaseCandidateDecision(
    string Version,
    string Branch,
    string ProposedTag,
    bool Ready,
    IReadOnlyList<ReleaseSurfaceCheck> Checks,
    IReadOnlyList<string> Blockers,
    bool RequiresHumanApproval);

public sealed class ReleaseCandidateService
{
    public ReleaseCandidateDecision Evaluate(
        string version,
        string branch,
        IEnumerable<ReleaseSurfaceCheck> checks)
    {
        if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(branch))
        {
            throw new ArgumentException("Version and branch are required.");
        }

        ReleaseSurfaceCheck[] checkList = checks.ToArray();
        List<string> blockers = checkList
            .Where(check => !check.Passed)
            .Select(check => $"{check.Surface}: {check.Check}")
            .ToList();

        foreach (ReleaseSurface surface in Enum.GetValues<ReleaseSurface>())
        {
            if (!checkList.Any(check => check.Surface == surface))
            {
                blockers.Add($"{surface}: no closure check supplied");
            }
        }

        return new ReleaseCandidateDecision(
            version.Trim(),
            branch.Trim(),
            $"v{version.Trim()}",
            blockers.Count == 0,
            checkList,
            blockers,
            true);
    }

    public void ApproveTag(ReleaseCandidateDecision decision, bool humanApproved)
    {
        if (!decision.Ready || !humanApproved)
        {
            throw new InvalidOperationException("A ready candidate and explicit human approval are required before tagging.");
        }
    }
}

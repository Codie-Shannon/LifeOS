namespace LifeOS.Core.Automation;

public static class AutomationExecutionPolicy
{
    public static AutomationEligibilityResult Check(
        AutomationStoreSnapshot store,
        AutomationProposal proposal,
        AutomationRule? rule,
        AutomationSourceSnapshot currentSource,
        DateTimeOffset? now = null)
    {
        var time = now ?? DateTimeOffset.UtcNow;
        var checks = new List<string>();
        var blockers = new List<string>();

        Check(!store.Settings.ExecutionPaused, "Global guarded execution is resumed.", "Global execution is paused.");
        Check(proposal.State is AutomationProposalState.ApprovedNotExecuted or AutomationProposalState.ExecutionPreviewReady,
            "Proposal has explicit approval.", "Proposal is not approved and awaiting execution.");
        Check(!proposal.OperationalActionExecuted && proposal.ExecutionId is null, "Proposal has not executed.", "Proposal already executed.");
        Check(proposal.ExpiresAt is null || proposal.ExpiresAt > time, "Proposal is not expired.", "Proposal is expired.");
        Check(proposal.State != AutomationProposalState.DuplicateSuspected, "Proposal is not duplicate-suspected.", "Duplicate-suspected proposals cannot execute.");
        Check(rule is not null && rule.Revision == proposal.RuleRevision, "Rule revision is current.", "Rule changed after proposal creation.");
        Check(currentSource.TrustState is AutomationTrustState.Reviewed or AutomationTrustState.Trusted,
            "Source remains reviewed/trusted.", "Source is no longer reviewed/trusted.");
        Check(AutomationEngine.CreateSourceSnapshotHash(currentSource) == proposal.SourceSnapshotHash,
            "Source state matches the approved snapshot.", "Source state changed; reevaluation is required.");
        Check(AutomationPolicy.IsExecutable(proposal.ActionType), "Typed action handler is allowlisted.", "Action is not on the executable allowlist.");
        Check(AutomationPolicy.IsReversible(proposal.ActionType), "Action is reversible.", "Action is not reversible.");
        Check(proposal.Risk == AutomationRiskLevel.Low, "Risk is Low.", "Only Low-risk actions may execute in Group 28.");
        Check(!AutomationPolicy.IsBlocked(proposal.ActionType), "No blocked action type is present.", "Action type is blocked by policy.");
        Check(!proposal.Target.StartsWith("External", StringComparison.OrdinalIgnoreCase), "Target is internal.", "External targets are blocked.");
        Check(store.InternalItems.Any(x => TargetMatches(x, proposal.Target)), "Target still exists.", "Target no longer exists.");

        return new(blockers.Count == 0, checks, blockers);

        void Check(bool passed, string pass, string fail)
        {
            if (passed) checks.Add("PASS: " + pass); else blockers.Add("BLOCK: " + fail);
        }
    }

    public static bool TargetMatches(AutomationInternalItem item, string target) =>
        string.Equals($"{item.Module}:{item.ItemId}", target, StringComparison.OrdinalIgnoreCase);
}

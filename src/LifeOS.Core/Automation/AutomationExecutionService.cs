using System.Text.Json;

namespace LifeOS.Core.Automation;

public static class AutomationExecutionService
{
    private const string ReviewNote = "Guarded automation review: define the next action for this fictional item.";

    public static AutomationExecutionPreview CreatePreview(AutomationStoreSnapshot store, AutomationProposal proposal, AutomationRule rule, AutomationSourceSnapshot currentSource)
    {
        var eligibility = AutomationExecutionPolicy.Check(store, proposal, rule, currentSource);
        if (!eligibility.Eligible) throw new InvalidOperationException(string.Join(" ", eligibility.Blockers));
        var item = store.InternalItems.Single(x => AutomationExecutionPolicy.TargetMatches(x, proposal.Target));
        var after = item with { ReviewNotes = [.. item.ReviewNotes, ReviewNote], Version = item.Version + 1 };
        return new()
        {
            ProposalId = proposal.ProposalId,
            ExactAction = "Add one internal review note",
            ExactTarget = proposal.Target,
            BeforeSnapshot = JsonSerializer.Serialize(item),
            ProposedAfterSnapshot = JsonSerializer.Serialize(after),
            Reversible = true,
            Risk = proposal.Risk,
            PolicyChecks = eligibility.Checks
        };
    }

    public static AutomationExecutionResult Execute(AutomationStoreSnapshot store, AutomationProposal proposal, AutomationRule rule,
        AutomationSourceSnapshot currentSource, DateTimeOffset? now = null)
    {
        AutomationHealthService.EnsureExecutionAllowed(store);
        var time = now ?? DateTimeOffset.UtcNow;
        var eligibility = AutomationExecutionPolicy.Check(store, proposal, rule, currentSource, time);
        if (!eligibility.Eligible) throw new InvalidOperationException(string.Join(" ", eligibility.Blockers));
        if (store.Executions.Any(x => x.ProposalId == proposal.ProposalId && x.Succeeded && x.UndoneAt is null))
            throw new InvalidOperationException("A successful execution already exists for this proposal.");
        if (proposal.DecisionAt is null) throw new InvalidOperationException("Approved timestamp is missing.");

        var index = store.InternalItems.FindIndex(x => AutomationExecutionPolicy.TargetMatches(x, proposal.Target));
        if (index < 0) throw new InvalidOperationException("Execution target is missing.");
        var before = store.InternalItems[index];
        var after = before with { ReviewNotes = [.. before.ReviewNotes, ReviewNote], Version = before.Version + 1, UpdatedAt = time };
        store.InternalItems[index] = after;

        return new()
        {
            ProposalId = proposal.ProposalId,
            RuleId = proposal.RuleId,
            RuleRevision = proposal.RuleRevision,
            ActionType = proposal.ActionType,
            TargetModule = before.Module,
            TargetItemId = before.ItemId,
            SourceReferences = [proposal.SourceItemId, proposal.EvaluationId],
            ApprovedAt = proposal.DecisionAt.Value,
            FinalConfirmedAt = time,
            ExecutedAt = time,
            BeforeSnapshot = JsonSerializer.Serialize(before),
            AfterSnapshot = JsonSerializer.Serialize(after),
            Succeeded = true,
            Reversible = true,
            UndoAvailable = true,
            ExecutionKey = $"proposal:{proposal.ProposalId}:review-note"
        };
    }

    public static AutomationExecutionResult Undo(AutomationStoreSnapshot store, AutomationExecutionResult execution, DateTimeOffset? now = null)
    {
        AutomationHealthService.EnsureExecutionAllowed(store);
        if (!execution.Succeeded || !execution.UndoAvailable || execution.UndoneAt is not null)
            throw new InvalidOperationException("Undo is not available for this execution.");
        var before = JsonSerializer.Deserialize<AutomationInternalItem>(execution.BeforeSnapshot) ?? throw new InvalidOperationException("Before snapshot is invalid.");
        var after = JsonSerializer.Deserialize<AutomationInternalItem>(execution.AfterSnapshot) ?? throw new InvalidOperationException("After snapshot is invalid.");
        var index = store.InternalItems.FindIndex(x => x.ItemId == execution.TargetItemId && x.Module == execution.TargetModule);
        if (index < 0) throw new InvalidOperationException("Undo target is missing.");
        var current = store.InternalItems[index];
        if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(after))
            throw new InvalidOperationException("Target changed after execution; Undo is blocked.");
        store.InternalItems[index] = before;
        return execution with { UndoneAt = now ?? DateTimeOffset.UtcNow, UndoAvailable = false };
    }
}

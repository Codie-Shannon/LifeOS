namespace LifeOS.Core.AssistantPlanning;

public interface IPlanningReviewArtifactStore
{
    IReadOnlyList<PlanningReviewArtifact> Items { get; }
    void Add(PlanningReviewArtifact artifact);
}

public sealed class SessionPlanningReviewArtifactStore : IPlanningReviewArtifactStore
{
    private readonly List<PlanningReviewArtifact> _items = [];
    public IReadOnlyList<PlanningReviewArtifact> Items => _items;
    public void Add(PlanningReviewArtifact artifact) => _items.Add(artifact);
}

public sealed class PlanningReviewTransferService(IPlanningReviewArtifactStore store)
{
    private readonly IPlanningReviewArtifactStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public PlanningHandoffPreview Preview(ReviewOnlyPlan plan, PlanningReviewSurface target, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.Blocks.Count == 0) throw new InvalidOperationException("A plan with no blocks cannot be handed off.");
        return new PlanningHandoffPreview(Guid.NewGuid(), plan.PlanId, target, now ?? DateTimeOffset.Now, plan.SourceQuestion,
            plan.Blocks, plan.Assumptions, plan.MissingData, plan.Conflicts, "Assistant planning • source-backed • user-reviewed • review-only", false);
    }

    public PlanningReviewArtifact Confirm(PlanningHandoffPreview preview, bool userConfirmed, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!userConfirmed) throw new InvalidOperationException("Explicit human confirmation is required.");
        if (preview.Executable) throw new InvalidOperationException("Executable planning payloads are forbidden.");
        var existing = _store.Items.FirstOrDefault(i => i.PlanId == preview.PlanId && i.Target == preview.Target);
        if (existing is not null) return existing;
        var artifact = new PlanningReviewArtifact(Guid.NewGuid(), preview.PlanId, preview.Target, now ?? DateTimeOffset.Now,
            preview.OriginalQuestion, preview.Blocks, preview.Assumptions, preview.MissingData, preview.Conflicts, preview.Provenance);
        _store.Add(artifact);
        return artifact;
    }

    public void Cancel(PlanningHandoffPreview preview) { ArgumentNullException.ThrowIfNull(preview); }
}

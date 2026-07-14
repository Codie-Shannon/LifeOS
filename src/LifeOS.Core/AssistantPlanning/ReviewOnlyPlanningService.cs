namespace LifeOS.Core.AssistantPlanning;

using LifeOS.Core.Assistant;

public sealed class ReviewOnlyPlanningService
{
    private static readonly TimeSpan StaleAfter = TimeSpan.FromDays(45);

    public ReviewOnlyPlan Build(PlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var now = request.Now ?? DateTimeOffset.Now;
        var response = request.Response ?? throw new ArgumentNullException(nameof(request.Response));
        if (response.Refused)
            throw new InvalidOperationException("A refused Assistant response cannot be converted into a plan.");

        var sources = response.SourcesUsed.Select(ToSource).ToArray();
        var staleSources = response.SourcesUsed.Where(s => now - s.Timestamp > StaleAfter).Select(s => s.RecordId).ToHashSet(StringComparer.Ordinal);
        var assumptions = response.Statements.Where(s => s.Kind == AssistantStatementKind.Inference).Select(s => s.Text).Distinct().ToArray();
        var missing = response.Statements.Where(s => s.Kind is AssistantStatementKind.MissingData or AssistantStatementKind.Uncertainty).Select(s => s.Text).Distinct().ToList();
        if (response.DisabledRelevantSources is { Count: > 0 })
            missing.Add($"Relevant disabled sources: {string.Join(", ", response.DisabledRelevantSources)}.");
        var conflicts = (response.Conflicts ?? []).Select(c => $"{c.Field}: {c.Explanation}").Distinct().ToArray();
        var criticalGap = sources.Length == 0 || conflicts.Length > 0 || missing.Count > 0;
        var readiness = conflicts.Length > 0 ? PlanningBlockStatus.Blocked : criticalGap ? PlanningBlockStatus.NeedsInput : PlanningBlockStatus.ReadyForReview;
        var readinessReason = conflicts.Length > 0 ? "Conflicting evidence requires a human decision before progression."
            : missing.Count > 0 ? "Missing or disabled evidence must be reviewed before progression."
            : sources.Length == 0 ? "No source-backed evidence is available."
            : "The proposal is source-backed and ready for human review; it is not executable.";

        var blocks = new List<PlanningBlock>
        {
            Block(PlanningBlockType.Goal, "Review the requested outcome", request.Question.Trim(),
                "The goal restates the user-requested outcome without creating operational state.", readiness, sources),
            Block(PlanningBlockType.Evidence, "Source-backed evidence", EvidenceText(response),
                "Evidence remains attached to the plan so claims can be traced to approved local records.", sources.Length == 0 ? PlanningBlockStatus.NeedsInput : PlanningBlockStatus.ReadyForReview, sources,
                stale: staleSources.Count > 0, missing: sources.Length == 0),
            Block(PlanningBlockType.Constraint, "Safety and operating constraints",
                "Review-only. Executable: No. No task, project, Follow-Up, payment, calendar item, email, automation, orchestration run, tool call, script or external write may be created.",
                "The Assistant planning boundary must remain narrower than existing approval and execution systems.", PlanningBlockStatus.ReadyForReview, []),
        };

        if (assumptions.Length > 0)
            blocks.Add(Block(PlanningBlockType.Risk, "Explicit assumptions", string.Join("\n", assumptions.Select(a => $"• {a}")),
                "Assumptions are separated from facts so the user can edit or reject them.", PlanningBlockStatus.NeedsInput, sources, assumption: true));

        if (conflicts.Length > 0)
            blocks.Add(Block(PlanningBlockType.Decision, "Resolve conflicting evidence", string.Join("\n", conflicts.Select(c => $"• {c}")),
                "LifeOS must preserve conflicting records instead of silently choosing one.", PlanningBlockStatus.Blocked, sources, conflict: true));

        if (missing.Count > 0)
            blocks.Add(Block(PlanningBlockType.Dependency, "Missing information or disabled-source gap", string.Join("\n", missing.Distinct().Select(m => $"• {m}")),
                "Progress depends on human review of the visible gap; no external search is performed.", PlanningBlockStatus.NeedsInput, sources, missing: true));

        var used = response.RecordsConsidered?.Where(r => r.Used).Take(4).ToArray() ?? [];
        if (used.Length > 0)
        {
            foreach (var ranked in used)
                blocks.Add(Block(PlanningBlockType.Step, $"Review {ranked.Record.Title}",
                    $"Inspect the source record and decide whether its proposed implication belongs in the reviewed plan. Status remains proposed until a human confirms it.",
                    $"Selected because: {ranked.Reason}", readiness, [ToSource(ranked.Record)], stale: staleSources.Contains(ranked.Record.RecordId)));
        }
        else
        {
            blocks.Add(Block(PlanningBlockType.Step, "Request the missing input", "Provide or enable the local evidence required to review this plan.",
                "No unsupported operational step should be manufactured.", PlanningBlockStatus.NeedsInput, [], missing: true));
        }

        blocks.Add(Block(PlanningBlockType.Verification, "Human verification", VerificationText(response),
            "Completion is confirmed manually against the cited local records, not by an opaque AI score.", readiness, sources));
        blocks.Add(Block(PlanningBlockType.Handoff, "Controlled review handoff",
            "Preview the exact payload, explicitly select a review surface, confirm the handoff, and create one non-executable review artifact only.",
            "Existing approval, final confirmation, orchestration and Emergency Stop boundaries remain authoritative.", readiness, sources));

        return new ReviewOnlyPlan(Guid.NewGuid(), now, request.Question.Trim(), blocks, assumptions, missing.Distinct().ToArray(), conflicts, readiness, readinessReason);
    }

    public ReviewOnlyPlan Reorder(ReviewOnlyPlan plan, Guid blockId, int newIndex)
    {
        var list = plan.Blocks.ToList();
        var oldIndex = list.FindIndex(b => b.BlockId == blockId);
        if (oldIndex < 0) throw new ArgumentException("Block not found.", nameof(blockId));
        var item = list[oldIndex]; list.RemoveAt(oldIndex);
        list.Insert(Math.Clamp(newIndex, 0, list.Count), item);
        return plan with { Blocks = list };
    }

    public ReviewOnlyPlan Remove(ReviewOnlyPlan plan, Guid blockId) =>
        plan with { Blocks = plan.Blocks.Where(b => b.BlockId != blockId).ToArray() };

    public ReviewOnlyPlan Edit(ReviewOnlyPlan plan, Guid blockId, string title, string content) =>
        plan with { Blocks = plan.Blocks.Select(b => b.BlockId == blockId ? b with { Title = title.Trim(), Content = content.Trim() } : b).ToArray() };

    private static PlanningBlock Block(PlanningBlockType type, string title, string content, string rationale,
        PlanningBlockStatus status, IReadOnlyList<PlanningClaimSource> sources, bool assumption = false,
        bool conflict = false, bool stale = false, bool missing = false) =>
        new(Guid.NewGuid(), type, title, content, rationale, status, sources, assumption, conflict, stale, missing);

    private static PlanningClaimSource ToSource(AssistantEvidenceRecord s) => new(s.RecordId, s.Source, s.Title, s.Timestamp, s.Provenance);
    private static string EvidenceText(AssistantResponse r) => r.SourcesUsed.Count == 0 ? "No directly supporting source record was selected."
        : string.Join("\n", r.SourcesUsed.Select(s => $"• [{s.Source}] {s.Title} — {s.RecordId} — {s.Timestamp:yyyy-MM-dd HH:mm} — {s.Provenance}"));
    private static string VerificationText(AssistantResponse r) =>
        $"Compare the plan against {r.SourcesUsed.Count} cited record(s), {r.Conflicts?.Count ?? 0} conflict(s), {r.DisabledRelevantSources?.Count ?? 0} disabled relevant source(s), and the original Assistant answer before accepting it for later review.";
}

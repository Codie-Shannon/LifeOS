using Xunit;
using LifeOS.Core;
using LifeOS.Core.Assistant;
using LifeOS.Core.AssistantMemory;
using LifeOS.Core.AssistantPlanning;

namespace LifeOS.Core.Tests;

public sealed class Group39BetaReleaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Release_identity_is_current_v13()
    {
        Assert.Equal("13.0.0-alpha.1", ProductVersion.Semantic);
        Assert.Equal("v13.0.0-alpha.1", ProductVersion.Display);
        Assert.Equal("v13 Household and Grocery planning", ProductVersion.ReleaseName);
    }

    [Fact]
    public void Controlled_review_transfer_is_cancel_safe_and_idempotent()
    {
        var store = new SessionPlanningReviewArtifactStore();
        var service = new PlanningReviewTransferService(store);
        var plan = BuildPlan();
        var preview = service.Preview(plan, PlanningReviewSurface.AssistantReviewQueue, Now);

        Assert.Empty(store.Items);
        Assert.Throws<InvalidOperationException>(() => service.Confirm(preview, false, Now.AddMinutes(1)));
        Assert.Empty(store.Items);

        var first = service.Confirm(preview, true, Now.AddMinutes(2));
        var duplicate = service.Confirm(preview, true, Now.AddMinutes(3));
        Assert.Single(store.Items);
        Assert.Equal(first.ArtifactId, duplicate.ArtifactId);
        Assert.False(first.Executable);
    }

    [Fact]
    public void Revoked_and_deleted_memory_is_excluded()
    {
        var service = new AssistantMemoryService(new InMemoryAssistantMemoryStore());
        var proposal = service.Propose(
            "Fictional Zephyr preference",
            "Use concise evidence-first summaries for fictional Project Zephyr Quill.",
            AssistantMemoryType.Preference,
            new AssistantMemoryScope(AssistantMemoryScopeType.Project, "Zephyr Quill"),
            AssistantMemorySensitivity.Standard,
            new AssistantMemoryOrigin("AssistantAnswer", "fictional-source", "Fictional answer", ["Projects/zephyr"], "Fictional approved source"),
            now: Now);
        var saved = service.Confirm(proposal.ProposalId, "Fictional user", true, Now.AddMinutes(1));
        Assert.Single(service.Retrieve(new AssistantMemoryQuery("Zephyr summary", Project: "Zephyr Quill"), Now.AddMinutes(2)));

        service.Revoke(saved.MemoryId, "Fictional user", Now.AddMinutes(3));
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summary", Project: "Zephyr Quill"), Now.AddMinutes(4)));

        service.Delete(saved.MemoryId, "Fictional user", Now.AddMinutes(5));
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summary", Project: "Zephyr Quill"), Now.AddMinutes(6)));
    }

    private static ReviewOnlyPlan BuildPlan()
    {
        var record = new AssistantEvidenceRecord("fictional-project-1", AssistantSourceType.Projects, "Project Zephyr Quill", "Status is blocked pending fictional owner confirmation.", Now.AddDays(-1), "Fictional project register", AssistantTrustLevel.Direct, "zephyr", "Blocked", IsFictional: true);
        var response = new AssistantResponse(
            "Project Zephyr Quill is blocked pending owner confirmation.",
            [record],
            [new AssistantStatement(AssistantStatementKind.Fact, "One fictional trusted project record supports the answer.")],
            "High", null, false,
            "Read-only; no trusted state can be mutated.",
            AssistantIntent.ProjectStatus,
            [AssistantSourceType.Projects],
            [new AssistantRankedRecord(record, 95, true, "Fresh direct fictional evidence.")],
            [], [], "Fresh direct fictional evidence.");
        return new ReviewOnlyPlanningService().Build(new PlanningRequest("Create a review-only recovery plan for fictional Project Zephyr Quill.", response, Now));
    }
}


using LifeOS.Core.Assistant;
using LifeOS.Core.AssistantPlanning;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class AssistantPlanningTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 10, 0, 0, TimeSpan.Zero);

    [Fact] public void Plan_has_stable_identity_ordered_blocks_and_non_executable_invariant()
    {
        var plan = Build(Response());
        Assert.NotEqual(Guid.Empty, plan.PlanId); Assert.True(plan.ReviewOnly); Assert.False(plan.Executable);
        Assert.Equal(PlanningBlockType.Goal, plan.Blocks[0].Type);
        Assert.Contains(plan.Blocks, b => b.Type == PlanningBlockType.Handoff);
        Assert.All(plan.Blocks, b => Assert.False(b.IsExecutable));
    }

    [Fact] public void Claim_to_source_provenance_is_retained()
    {
        var plan = Build(Response());
        var evidence = Assert.Single(plan.Blocks.Where(b => b.Type == PlanningBlockType.Evidence));
        var source = Assert.Single(evidence.Sources);
        Assert.Equal("rec-1", source.RecordId); Assert.Equal("Fictional register", source.Provenance);
    }

    [Fact] public void Facts_assumptions_conflicts_and_missing_data_remain_separate()
    {
        var response = Response(statements: [new(AssistantStatementKind.Inference,"Assume owner availability."), new(AssistantStatementKind.MissingData,"Due date is missing.")], conflicts: [Conflict()]);
        var plan = Build(response);
        Assert.Single(plan.Assumptions); Assert.Single(plan.Conflicts); Assert.Single(plan.MissingData);
        Assert.Contains(plan.Blocks,b=>b.IsAssumption); Assert.Contains(plan.Blocks,b=>b.HasConflict); Assert.Contains(plan.Blocks,b=>b.HasMissingData);
    }

    [Fact] public void Conflict_blocks_plan()
    { var plan=Build(Response(conflicts:[Conflict()])); Assert.Equal(PlanningBlockStatus.Blocked,plan.Readiness); Assert.Contains(plan.Blocks,b=>b.Status==PlanningBlockStatus.Blocked); }

    [Fact] public void Disabled_source_gap_needs_input()
    { var plan=Build(Response(disabled:[AssistantSourceType.Timesheets])); Assert.Equal(PlanningBlockStatus.NeedsInput,plan.Readiness); Assert.Contains(plan.MissingData,m=>m.Contains("Timesheets")); }

    [Fact] public void Blocks_can_be_reordered_edited_and_removed()
    {
        var service=new ReviewOnlyPlanningService(); var plan=Build(Response()); var id=plan.Blocks[1].BlockId;
        plan=service.Reorder(plan,id,0); Assert.Equal(id,plan.Blocks[0].BlockId);
        plan=service.Edit(plan,id,"Edited","Edited content"); Assert.Equal("Edited",plan.Blocks[0].Title);
        plan=service.Remove(plan,id); Assert.DoesNotContain(plan.Blocks,b=>b.BlockId==id);
    }

    [Fact] public void Handoff_preview_contains_exact_review_only_payload()
    {
        var plan=Build(Response()); var store=new SessionPlanningReviewArtifactStore(); var preview=new PlanningReviewTransferService(store).Preview(plan,PlanningReviewSurface.AssistantReviewQueue,Now);
        Assert.Equal(plan.Blocks,preview.Blocks); Assert.Equal(plan.SourceQuestion,preview.OriginalQuestion); Assert.False(preview.Executable); Assert.Empty(store.Items);
    }

    [Fact] public void Confirmation_creates_one_review_artifact_only()
    {
        var plan=Build(Response()); var store=new SessionPlanningReviewArtifactStore(); var service=new PlanningReviewTransferService(store); var preview=service.Preview(plan,PlanningReviewSurface.WeeklyCloseOutReview,Now);
        var artifact=service.Confirm(preview,true,Now); Assert.Single(store.Items); Assert.Equal("ReviewOnly",artifact.Status); Assert.False(artifact.Executable);
        var duplicate=service.Confirm(preview,true,Now);
        Assert.Single(store.Items);
        Assert.Equal(artifact.ArtifactId,duplicate.ArtifactId);
    }

    [Fact] public void Cancellation_creates_no_partial_state()
    { var store=new SessionPlanningReviewArtifactStore(); var service=new PlanningReviewTransferService(store); service.Cancel(service.Preview(Build(Response()),PlanningReviewSurface.EvidenceReview,Now)); Assert.Empty(store.Items); }

    [Fact] public void Explicit_confirmation_is_required()
    { var service=new PlanningReviewTransferService(new SessionPlanningReviewArtifactStore()); var preview=service.Preview(Build(Response()),PlanningReviewSurface.AssistantReviewQueue,Now); Assert.Throws<InvalidOperationException>(()=>service.Confirm(preview,false,Now)); }

    [Fact] public void Refused_answer_cannot_be_planned()
    { var response=Response() with { Refused=true }; Assert.Throws<InvalidOperationException>(()=>Build(response)); }

    private static ReviewOnlyPlan Build(AssistantResponse response)=>new ReviewOnlyPlanningService().Build(new PlanningRequest("Create a recovery plan for fictional Project Zephyr Quill.",response,Now));
    private static AssistantConflict Conflict()=>new("Status",Record("rec-1","Active"),Record("rec-2","Blocked"),"Two approved records disagree.");
    private static AssistantEvidenceRecord Record(string id,string status)=>new(id,AssistantSourceType.Projects,"Project Zephyr Quill",$"Status is {status}.",Now.AddDays(-2),"Fictional register",AssistantTrustLevel.Direct,"zephyr",""+status,IsFictional:true);
    private static AssistantResponse Response(IReadOnlyList<AssistantStatement>? statements=null,IReadOnlyList<AssistantConflict>? conflicts=null,IReadOnlyList<AssistantSourceType>? disabled=null)
    {
        var record=Record("rec-1","Active"); var ranked=new AssistantRankedRecord(record,90,true,"Fresh direct project evidence.");
        return new("Project Zephyr Quill is active.",[record],statements??[new(AssistantStatementKind.Fact,"One direct record supports the answer.")],"High",null,false,"Read-only",AssistantIntent.ProjectStatus,[AssistantSourceType.Projects],[ranked],conflicts??[],disabled??[],"Fresh direct evidence.");
    }
}

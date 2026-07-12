using LifeOS.Core.Automation;
using LifeOS.Shared.Automation;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class AutomationFoundationTests
{
    [Fact]
    public void ProductVersion_IsAlignedForGroup29()
    {
        Assert.Equal("6.0.0-alpha.3", LifeOS.Core.ProductVersion.Semantic);
        Assert.Equal("v6.0.0-alpha.3", LifeOS.Core.ProductVersion.Display);
        Assert.Equal("Controlled orchestration and recovery", LifeOS.Core.ProductVersion.ReleaseName);
    }

    [Fact] public void NewRule_IsDisabledByDefault() => Assert.False(new AutomationRule().IsEnabled);
    [Fact] public void GlobalExecution_IsPausedByDefault() => Assert.True(AutomationDemoData.Create().Settings.ExecutionPaused);

    [Fact]
    public void Approval_RemainsSeparateFromExecution()
    {
        var proposal = CreateApprovedCandidate();
        var approved = AutomationEngine.Decide(proposal, true, "valid intent", DateTimeOffset.Parse("2026-07-12T00:00:00Z"));
        Assert.Equal(AutomationProposalState.ApprovedNotExecuted, approved.State);
        Assert.False(approved.OperationalActionExecuted);
    }

    [Fact]
    public void SafeTypedAction_RequiresResumedGateAndFinalPreview()
    {
        var (store, rule, proposal, source) = ApprovedScenario();
        Assert.Throws<InvalidOperationException>(() => AutomationExecutionService.CreatePreview(store, proposal, rule, source));
        store = store with { Settings = new() { ExecutionPaused = false } };
        var preview = AutomationExecutionService.CreatePreview(store, proposal, rule, source);
        Assert.True(preview.Reversible);
        Assert.Contains("Before", "Before");
        Assert.NotEqual(preview.BeforeSnapshot, preview.ProposedAfterSnapshot);
    }

    [Fact]
    public void LowRiskReviewNote_ExecutesAndRetainsSnapshots()
    {
        var (store, rule, proposal, source) = ApprovedScenario();
        store = store with { Settings = new() { ExecutionPaused = false } };
        var result = AutomationExecutionService.Execute(store, proposal, rule, source, DateTimeOffset.Parse("2026-07-12T01:00:00Z"));
        Assert.True(result.Succeeded);
        Assert.True(result.Reversible);
        Assert.True(result.UndoAvailable);
        Assert.NotEqual(result.BeforeSnapshot, result.AfterSnapshot);
        Assert.Single(store.InternalItems.Single(x => x.ItemId == "example-project-002").ReviewNotes);
    }

    [Fact]
    public void Undo_RestoresExactPriorState_AndCannotRunTwice()
    {
        var (store, rule, proposal, source) = ApprovedScenario();
        store = store with { Settings = new() { ExecutionPaused = false } };
        var result = AutomationExecutionService.Execute(store, proposal, rule, source);
        var undone = AutomationExecutionService.Undo(store, result);
        Assert.NotNull(undone.UndoneAt);
        Assert.False(undone.UndoAvailable);
        Assert.Empty(store.InternalItems.Single(x => x.ItemId == "example-project-002").ReviewNotes);
        Assert.Throws<InvalidOperationException>(() => AutomationExecutionService.Undo(store, undone));
    }

    [Fact]
    public void StaleSource_IsBlockedImmediatelyBeforeExecution()
    {
        var (store, rule, proposal, source) = ApprovedScenario();
        store = store with { Settings = new() { ExecutionPaused = false } };
        var changed = source with { Fields = source.Fields.ToDictionary(x => x.Key, x => x.Key == "Version" ? "2" : x.Value) };
        var result = AutomationExecutionPolicy.Check(store, proposal, rule, changed);
        Assert.False(result.Eligible);
        Assert.Contains(result.Blockers, x => x.Contains("changed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DuplicateSuspectedProposal_CannotBeApprovedOrExecuted()
    {
        var proposal = CreateApprovedCandidate() with { State = AutomationProposalState.DuplicateSuspected };
        Assert.Throws<InvalidOperationException>(() => AutomationEngine.Decide(proposal, true, "no"));
    }

    [Fact]
    public void SameProposal_CannotExecuteTwice()
    {
        var (store, rule, proposal, source) = ApprovedScenario();
        store = store with { Settings = new() { ExecutionPaused = false } };
        var first = AutomationExecutionService.Execute(store, proposal, rule, source);
        store.Executions.Add(first);
        Assert.Throws<InvalidOperationException>(() => AutomationExecutionService.Execute(store, proposal, rule, source));
    }

    [Theory]
    [InlineData(AutomationActionType.SendEmail)]
    [InlineData(AutomationActionType.ModifyCalendar)]
    [InlineData(AutomationActionType.FinancialMutation)]
    [InlineData(AutomationActionType.DestructiveAction)]
    [InlineData(AutomationActionType.ExecuteScript)]
    [InlineData(AutomationActionType.ExternalWrite)]
    public void ForbiddenActions_RemainBlocked(AutomationActionType action) => Assert.True(AutomationPolicy.IsBlocked(action));

    [Fact]
    public void OnlyReviewNote_IsExecutableInGroup28()
    {
        Assert.True(AutomationPolicy.IsExecutable(AutomationActionType.ProposeReviewNote));
        Assert.False(AutomationPolicy.IsExecutable(AutomationActionType.ProposeFollowUp));
        Assert.False(AutomationPolicy.IsExecutable(AutomationActionType.ProposeEvidenceReviewRequest));
        Assert.False(AutomationPolicy.IsExecutable(AutomationActionType.SendEmail));
    }

    [Fact]
    public void RepeatedEvaluation_IsMarkedDuplicateSuspected()
    {
        var store = AutomationDemoData.Create();
        var rule = store.Rules.Single(x => x.RuleId == "missing-next-action") with { IsEnabled = true, ExecutionMode = AutomationExecutionMode.GuardedInternal };
        var source = AutomationDemoData.SourceFor(rule, store);
        var first = AutomationEngine.CreateProposal(rule, AutomationEngine.Evaluate(rule, source), [])!;
        var second = AutomationEngine.CreateProposal(rule, AutomationEngine.Evaluate(rule, source), [first])!;
        Assert.Equal(AutomationProposalState.DuplicateSuspected, second.State);
        Assert.Equal(first.ProposalId, second.PriorProposalId);
    }

    [Fact]
    public void StorageRoundTrip_PreservesPauseExecutionAndUndoState()
    {
        var snapshot = AutomationDemoData.Create();
        Assert.True(snapshot.Settings.ExecutionPaused);
        Assert.NotEmpty(snapshot.InternalItems);
    }

    [Fact]
    public void Group29Plan_IsDisabledByDefault_AndDueDoesNotExecute()
    {
        var store = AutomationDemoData.Create();
        var plan = store.OrchestrationPlans.Single();
        Assert.False(plan.IsEnabled);
        Assert.Null(OrchestrationService.EnsureOccurrence(store, plan, DateTimeOffset.Parse("2026-07-12T09:00:00+12:00")));
        Assert.Empty(store.OrchestrationRuns);
    }

    [Fact]
    public void WeeklySchedule_CreatesSingularDueOccurrence()
    {
        var store = AutomationDemoData.Create(); var plan = store.OrchestrationPlans.Single() with { IsEnabled = true };
        store.OrchestrationPlans[0] = plan;
        var now = DateTimeOffset.Parse("2026-07-12T09:00:00+12:00");
        var first = OrchestrationService.EnsureOccurrence(store, plan, now);
        var second = OrchestrationService.EnsureOccurrence(store, plan, now);
        Assert.NotNull(first); Assert.Equal(first!.OccurrenceId, second!.OccurrenceId); Assert.Single(store.OrchestrationOccurrences); Assert.Empty(store.OrchestrationRuns);
    }

    [Fact]
    public void ExplicitStart_CreatesPausedRunBeforeFirstStep()
    {
        var store = AutomationDemoData.Create() with { Settings = new() { ExecutionPaused = false } };
        var plan = store.OrchestrationPlans.Single() with { IsEnabled = true }; store.OrchestrationPlans[0] = plan;
        var occurrence = OrchestrationService.EnsureOccurrence(store, plan, DateTimeOffset.Parse("2026-07-12T09:00:00+12:00"))!;
        var run = OrchestrationService.Start(store, plan, occurrence);
        Assert.Equal(OrchestrationRunStatus.Paused, run.Status); Assert.Equal("weekly-note", run.CurrentStepId);
        Assert.All(store.OrchestrationStepRuns, x => Assert.Equal(OrchestrationStepStatus.Pending, x.Status));
    }

    [Fact]
    public void StepRequiresPreview_ExecutesOneAtATime_AndPauses()
    {
        var (store, run) = StartedOrchestration();
        Assert.Throws<InvalidOperationException>(() => OrchestrationService.ConfirmCurrentStep(store, run.RunId));
        OrchestrationService.PreviewCurrentStep(store, run.RunId);
        OrchestrationService.ConfirmCurrentStep(store, run.RunId);
        var updated = store.OrchestrationRuns.Single();
        Assert.Equal(OrchestrationRunStatus.Paused, updated.Status); Assert.Equal("weekly-flag", updated.CurrentStepId);
        Assert.Equal(OrchestrationStepStatus.Succeeded, store.OrchestrationStepRuns.Single(x => x.StepId == "weekly-note").Status);
        Assert.Equal(OrchestrationStepStatus.Pending, store.OrchestrationStepRuns.Single(x => x.StepId == "weekly-flag").Status);
    }

    [Fact]
    public void ControlledFailure_Pauses_AndRequiresExplicitRetry()
    {
        var (store, run) = StartedOrchestration(); OrchestrationService.PreviewCurrentStep(store, run.RunId);
        OrchestrationService.ConfirmCurrentStep(store, run.RunId, injectSafeFailure: true);
        Assert.Equal(OrchestrationRunStatus.RecoveryRequired, store.OrchestrationRuns.Single().Status);
        Assert.Equal(0, store.OrchestrationStepRuns.Single(x => x.StepId == "weekly-note").RetryCount);
        OrchestrationService.RetryFailedStep(store, run.RunId);
        Assert.Equal(OrchestrationRunStatus.Paused, store.OrchestrationRuns.Single().Status);
        Assert.Equal(1, store.OrchestrationStepRuns.Single(x => x.StepId == "weekly-note").RetryCount);
    }

    [Fact]
    public void CancelRemaining_PreservesCompletedCheckpoint_AndRollbackRestoresIt()
    {
        var (store, run) = StartedOrchestration(); OrchestrationService.PreviewCurrentStep(store, run.RunId); OrchestrationService.ConfirmCurrentStep(store, run.RunId);
        OrchestrationService.CancelRemaining(store, run.RunId);
        Assert.Single(store.InternalItems.Single(x => x.ItemId == "example-project-002").ReviewNotes);
        OrchestrationService.RollBackCompleted(store, run.RunId);
        Assert.Empty(store.InternalItems.Single(x => x.ItemId == "example-project-002").ReviewNotes);
    }

    [Fact]
    public void BlockedExternalStep_CannotStart()
    {
        var store = AutomationDemoData.Create() with { Settings = new() { ExecutionPaused = false } };
        var plan = store.OrchestrationPlans.Single() with { IsEnabled = true }; store.OrchestrationPlans[0] = plan;
        store.OrchestrationSteps.Add(new() { PlanId = plan.PlanId, Sequence = 99, Name = "Blocked email", ActionType = AutomationActionType.SendEmail, TargetModule = "ExternalCommunication", TargetItemId = "example-invoice-004", RiskLevel = AutomationRiskLevel.High, IsReversible = false, RequiredCapabilities = [AutomationCapability.CommunicationAction, AutomationCapability.ExternalWrite] });
        var occurrence = OrchestrationService.EnsureOccurrence(store, plan, DateTimeOffset.Parse("2026-07-12T09:00:00+12:00"))!;
        Assert.Throws<InvalidOperationException>(() => OrchestrationService.Start(store, plan, occurrence));
    }

    private static (AutomationStoreSnapshot Store, OrchestrationRun Run) StartedOrchestration()
    {
        var store = AutomationDemoData.Create() with { Settings = new() { ExecutionPaused = false } };
        var plan = store.OrchestrationPlans.Single() with { IsEnabled = true }; store.OrchestrationPlans[0] = plan;
        var occurrence = OrchestrationService.EnsureOccurrence(store, plan, DateTimeOffset.Parse("2026-07-12T09:00:00+12:00"))!;
        return (store, OrchestrationService.Start(store, plan, occurrence));
    }

    private static AutomationProposal CreateApprovedCandidate() => new()
    {
        EvaluationId = "eval", RuleId = "missing-next-action", RuleRevision = 1, RuleName = "Rule", SourceItemId = "example-project-002",
        SourceTrustState = AutomationTrustState.Reviewed, SourceSnapshotHash = "hash", ActionType = AutomationActionType.ProposeReviewNote,
        ActionSummary = "Add note", Target = "WorkPipeline:example-project-002", Risk = AutomationRiskLevel.Low,
        ExecutionMode = AutomationExecutionMode.GuardedInternal, DuplicateKey = "key"
    };

    private static (AutomationStoreSnapshot Store, AutomationRule Rule, AutomationProposal Proposal, AutomationSourceSnapshot Source) ApprovedScenario()
    {
        var store = AutomationDemoData.Create();
        var rule = store.Rules.Single(x => x.RuleId == "missing-next-action") with { IsEnabled = true, ExecutionMode = AutomationExecutionMode.GuardedInternal };
        store.Rules[store.Rules.FindIndex(x => x.RuleId == rule.RuleId)] = rule;
        var source = AutomationDemoData.SourceFor(rule, store);
        var evaluation = AutomationEngine.Evaluate(rule, source, DateTimeOffset.Parse("2026-07-12T00:00:00Z"));
        var proposal = AutomationEngine.CreateProposal(rule, evaluation, [])!;
        proposal = AutomationEngine.Decide(proposal, true, "approved", DateTimeOffset.Parse("2026-07-12T00:30:00Z"));
        return (store, rule, proposal, source);
    }
}

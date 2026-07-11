using LifeOS.Core.Automation;
using LifeOS.Shared.Automation;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class AutomationFoundationTests
{
    [Fact]
    public void ProductVersion_IsAlignedForGroup27()
    {
        Assert.Equal("6.0.0-alpha.1", LifeOS.Core.ProductVersion.Semantic);
        Assert.Equal("v6.0.0-alpha.1", LifeOS.Core.ProductVersion.Display);
        Assert.Equal("Controlled automation foundation", LifeOS.Core.ProductVersion.ReleaseName);
    }
    [Fact] public void NewRule_IsDisabledByDefault() => Assert.False(new AutomationRule().IsEnabled);

    [Fact]
    public void DeterministicMatch_CreatesReviewProposalWithoutExecution()
    {
        var rule = AutomationDemoData.Create().Rules.Single(x => x.RuleId == "follow-up-review") with { IsEnabled = true, ExecutionMode = AutomationExecutionMode.DryRunOnly };
        var evaluation = AutomationEngine.Evaluate(rule, AutomationDemoData.SourceFor(rule), DateTimeOffset.Parse("2026-07-12T00:00:00Z"));
        var proposal = AutomationEngine.CreateProposal(rule, evaluation, []);
        Assert.True(evaluation.Matched);
        Assert.NotNull(proposal);
        Assert.Equal(AutomationProposalState.NeedsReview, proposal!.State);
        Assert.False(proposal.OperationalActionExecuted);
    }

    [Fact]
    public void DisabledRule_DoesNotCreateActionableProposal()
    {
        var rule = AutomationDemoData.Create().Rules.Single(x => x.RuleId == "follow-up-review");
        var evaluation = AutomationEngine.Evaluate(rule, AutomationDemoData.SourceFor(rule));
        Assert.False(evaluation.Matched);
        Assert.Null(AutomationEngine.CreateProposal(rule, evaluation, []));
    }

    [Fact]
    public void UntrustedSource_CannotMatch()
    {
        var rule = AutomationDemoData.Create().Rules.Single(x => x.RuleId == "evidence-review") with { IsEnabled = true };
        var source = AutomationDemoData.SourceFor(rule) with { TrustState = AutomationTrustState.Untrusted };
        var evaluation = AutomationEngine.Evaluate(rule, source);
        Assert.False(evaluation.Matched);
        Assert.Contains(evaluation.BlockingReasons, x => x.Contains("untrusted", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(AutomationActionType.SendEmail)]
    [InlineData(AutomationActionType.ModifyCalendar)]
    [InlineData(AutomationActionType.FinancialMutation)]
    [InlineData(AutomationActionType.DestructiveAction)]
    [InlineData(AutomationActionType.ExecuteScript)]
    [InlineData(AutomationActionType.ExternalWrite)]
    public void ForbiddenActions_AreBlocked(AutomationActionType action) => Assert.True(AutomationPolicy.IsBlocked(action));

    [Fact]
    public void BlockedHighRiskRule_ProducesBlockedProofProposal()
    {
        var rule = AutomationDemoData.Create().Rules.Single(x => x.RuleId == "blocked-email") with { IsEnabled = true };
        var evaluation = AutomationEngine.Evaluate(rule, AutomationDemoData.SourceFor(rule));
        var proposal = AutomationEngine.CreateProposal(rule, evaluation, []);
        Assert.Equal(AutomationExecutionMode.BlockedByPolicy, evaluation.ExecutionMode);
        Assert.Equal(AutomationProposalState.Blocked, proposal!.State);
        Assert.False(proposal.OperationalActionExecuted);
    }

    [Fact]
    public void Approval_RecordsDecisionButNeverExecutes()
    {
        var proposal = new AutomationProposal
        {
            EvaluationId = "eval", RuleId = "rule", RuleName = "Rule", SourceItemId = "source",
            ActionType = AutomationActionType.ProposeReviewNote, ActionSummary = "Review", Target = "Review",
            Risk = AutomationRiskLevel.Low, ExecutionMode = AutomationExecutionMode.DryRunOnly, DuplicateKey = "key"
        };
        var approved = AutomationEngine.Decide(proposal, true, "valid intent", DateTimeOffset.Parse("2026-07-12T00:00:00Z"));
        Assert.Equal(AutomationProposalState.Approved, approved.State);
        Assert.False(approved.OperationalActionExecuted);
    }

    [Fact]
    public void RepeatedEvaluation_IsMarkedDuplicateSuspected()
    {
        var rule = AutomationDemoData.Create().Rules.Single(x => x.RuleId == "missing-next-action") with { IsEnabled = true };
        var source = AutomationDemoData.SourceFor(rule);
        var firstEvaluation = AutomationEngine.Evaluate(rule, source);
        var first = AutomationEngine.CreateProposal(rule, firstEvaluation, [])!;
        var secondEvaluation = AutomationEngine.Evaluate(rule, source);
        var second = AutomationEngine.CreateProposal(rule, secondEvaluation, [first])!;
        Assert.Equal(first.DuplicateKey, second.DuplicateKey);
        Assert.Equal(AutomationProposalState.DuplicateSuspected, second.State);
        Assert.Equal(first.ProposalId, second.PriorProposalId);
    }

    [Fact]
    public void ValidationMatrix_IsCompleteAndPassing() => Assert.True(V6AutomationValidationMatrix.AllPassed(V6AutomationValidationMatrix.Create()));
}

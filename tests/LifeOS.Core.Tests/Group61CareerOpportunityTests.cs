using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group61CareerOpportunityTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 22, 14, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void ApprovedStageTransitionsApplyAndInvalidTransitionsFail()
    {
        var service = new CareerOpportunityService();
        var opportunity = CareerProofData.Build(Now).Opportunities.First(x => x.Stage == OpportunityStage.Reviewing);
        var valid = service.Transition(opportunity, OpportunityStage.Interested, Now, "CS");
        Assert.True(valid.Applied);
        Assert.Equal(OpportunityStage.Interested, valid.Opportunity.Stage);
        var invalid = service.Transition(valid.Opportunity, OpportunityStage.Accepted, Now.AddMinutes(1), "CS");
        Assert.False(invalid.Applied);
    }

    [Fact]
    public void FitAssessmentIsEvidenceBackedEditableAndNonCertain()
    {
        var fit = CareerProofData.Build(Now).Opportunities[0].Fit!;
        Assert.True(fit.IsEvidenceBacked);
        Assert.NotEmpty(fit.EvidenceLinks);
        Assert.Contains("no certainty", fit.Explanation, StringComparison.OrdinalIgnoreCase);
        var edited = fit with { Gaps = fit.Gaps.Concat(["Another reviewed gap"]).ToArray() };
        Assert.Equal(fit.Gaps.Count + 1, edited.Gaps.Count);
    }

    [Fact]
    public void DuplicateCandidatesRemainReviewOnly()
    {
        var duplicates = CareerProofData.Build(Now).DuplicateCandidates;
        Assert.NotEmpty(duplicates);
        Assert.All(duplicates, x => Assert.Equal(CandidateReviewState.AwaitingReview, x.ReviewState));
    }

    [Fact]
    public void ImportedInboxContextDoesNotBecomeTrustedAutomatically()
    {
        var candidate = CareerProofData.Build(Now).ImportedCandidates.Single();
        Assert.Equal(CandidateReviewState.AwaitingReview, candidate.ReviewState);
        Assert.Contains("untrusted", candidate.ReviewReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ClosingDateAndFreshnessCalculationsAreDeterministic()
    {
        var service = new CareerOpportunityService();
        var opportunity = CareerProofData.Build(Now).Opportunities[0];
        Assert.Equal(PriorityLevel.High, service.CalculatePriority(opportunity, Now));
        Assert.False(service.IsStale(opportunity, Now));
    }
}

using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group63CareerAnalyticsTests
{
    private static readonly DateTimeOffset Now = new(2026,7,22,14,0,0,TimeSpan.FromHours(12));

    [Fact] public void Review_period_can_be_bounded_explicitly()
    {
        var proof = CareerClosureProofData.Build(Now);
        var period = new CareerReviewPeriod(Now.AddDays(-14), Now);
        var result = new CareerClosureService().BuildReview(proof, period);
        Assert.Equal(period, result.Period);
    }

    [Fact] public void Pipeline_uses_counts_not_probability()
    {
        var review = CareerClosureProofData.Build(Now).Review;
        Assert.True(review.Pipeline.OpportunitiesDiscovered >= review.Pipeline.Interviews);
        Assert.DoesNotContain("probability", review.DataState, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] public void Breakdowns_and_followup_performance_are_present()
    {
        var review = CareerClosureProofData.Build(Now).Review;
        Assert.NotEmpty(review.SourceBreakdown);
        Assert.NotEmpty(review.RoleFamilyBreakdown);
        Assert.NotEmpty(review.WorkModeBreakdown);
        Assert.NotEmpty(review.StageBreakdown);
        Assert.True(review.FollowUps.AverageResponseHours >= 0);
    }

    [Fact] public void Coverage_is_drillable_and_privacy_safe()
    {
        var coverage = CareerClosureProofData.Build(Now).Review.Coverage;
        Assert.NotEmpty(coverage.DrillDown);
        Assert.All(coverage.DrillDown, x => Assert.DoesNotContain("@", x.SafeDetail));
    }

    [Fact] public void Reference_details_remain_redacted_or_private()
    {
        var references = CareerClosureProofData.Build(Now).References;
        Assert.All(references, x => Assert.True(x.PrivacyState is CareerPrivacyState.Redacted or CareerPrivacyState.Private));
    }
}

using LifeOS.Core.IntegrationConnectors;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationReleaseReadinessTests
{
    [Fact]
    public void Summary_ReportsFiveActiveConnectorLanes()
    {
        var summary = IntegrationReleaseReadiness.Create();
        Assert.Equal(5, summary.Lanes.Count);
        Assert.Equal(5, summary.AvailableCount);
    }

    [Fact]
    public void EveryActiveLane_IsReadOnlyManualAndReviewFirst()
    {
        var summary = IntegrationReleaseReadiness.Create();
        Assert.All(summary.Lanes, lane =>
        {
            Assert.True(lane.IsReadOnly);
            Assert.True(lane.IsManualOnly);
            Assert.True(lane.RequiresHumanReview);
            Assert.True(lane.RetainsEvidenceAfterDisconnect);
        });
    }

    [Fact]
    public void ProviderScopes_RemainReadOnly()
    {
        var summary = IntegrationReleaseReadiness.Create();
        Assert.Equal("calendar.readonly", summary.Lanes.Single(x => x.Key == "google-calendar").ScopeLabel);
        Assert.Equal("gmail.readonly", summary.Lanes.Single(x => x.Key == "gmail").ScopeLabel);
        Assert.DoesNotContain(summary.Lanes, x => x.ScopeLabel.Contains("modify", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(summary.Lanes, x => x.ScopeLabel.Contains("send", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Summary_ReportsNoAutomaticMutation()
    {
        var summary = IntegrationReleaseReadiness.Create();
        Assert.False(summary.AnyAutomaticExternalMutation);
        Assert.False(summary.AnyAutomaticLifeOsMutation);
        Assert.Contains("No connector changes", summary.GlobalSafetyStatement);
    }

    [Fact]
    public void LifecycleState_SanitizesSecretMarkers()
    {
        Assert.Equal("Sanitized connector state available",
            IntegrationReleaseReadiness.SanitizeState("refresh_token=secret"));
    }

    [Fact]
    public void ValidationMatrix_IsCompleteAndPassing()
    {
        var items = V5ReleaseValidationMatrix.Create();
        Assert.True(V5ReleaseValidationMatrix.AllPassed(items));
        Assert.Contains(items, x => x.Area == "Calendar");
        Assert.Contains(items, x => x.Area == "Gmail");
        Assert.Contains(items, x => x.Area == "Privacy");
        Assert.Contains(items, x => x.Area == "Runtime safety");
    }
}

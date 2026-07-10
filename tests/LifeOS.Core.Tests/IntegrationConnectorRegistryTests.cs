using LifeOS.Core.IntegrationConnectors;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationConnectorRegistryTests
{
    [Fact]
    public void RegistryContainsBroadPlanningConnectors()
    {
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "manual-csv");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "gmail");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "google-calendar");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "afterpay");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "zip");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "facebook-pages");
        Assert.Contains(IntegrationConnectorRegistry.All, connector => connector.Key == "sms");
    }

    [Fact]
    public void V5FoundationConnectorsArePreviewOnly()
    {
        var v5Connectors = IntegrationConnectorRegistry.ForPhase(IntegrationConnectorRoadmapPhase.V5Foundation);

        Assert.NotEmpty(v5Connectors);

        foreach (var connector in v5Connectors)
        {
            Assert.Equal(IntegrationConnectorActionPolicy.PreviewOnly, connector.ActionPolicy);
            Assert.False(connector.AllowsExternalMutation);
        }
    }

    [Fact]
    public void HighRiskMoneyAndCommunicationConnectorsRequireApproval()
    {
        var keys = new[]
        {
            "open-banking",
            "afterpay",
            "zip",
            "gmail",
            "messenger",
            "facebook-pages",
            "sms"
        };

        foreach (var key in keys)
        {
            var connector = IntegrationConnectorRegistry.GetRequired(key);

            Assert.True(connector.RequiresExplicitApproval);
            Assert.False(connector.AllowsExternalMutation);
        }
    }

    [Fact]
    public void FacebookBelongsToLaterCommunicationAndContentRoadmap()
    {
        var pages = IntegrationConnectorRegistry.GetRequired("facebook-pages");
        var messenger = IntegrationConnectorRegistry.GetRequired("messenger");

        Assert.Equal(IntegrationConnectorCategory.Communication, pages.Category);
        Assert.Equal(IntegrationConnectorRoadmapPhase.V8V9CommunicationContent, pages.RoadmapPhase);
        Assert.Equal(IntegrationConnectorRoadmapPhase.V8V9CommunicationContent, messenger.RoadmapPhase);
        Assert.Equal(IntegrationConnectorRiskLevel.VeryHigh, messenger.RiskLevel);
    }

    [Fact]
    public void SmsBelongsToMobileCompanionLane()
    {
        var sms = IntegrationConnectorRegistry.GetRequired("sms");

        Assert.Equal(IntegrationConnectorFirstMode.MobileDeferred, sms.FirstMode);
        Assert.Equal(IntegrationConnectorRoadmapPhase.MobileCompanion, sms.RoadmapPhase);
        Assert.Equal(IntegrationConnectorActionPolicy.DraftWithExplicitApproval, sms.ActionPolicy);
    }

    [Fact]
    public void RegistryKeysAreUnique()
    {
        var duplicateKeys = IntegrationConnectorRegistry.All
            .GroupBy(connector => connector.Key)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.Empty(duplicateKeys);
    }
}

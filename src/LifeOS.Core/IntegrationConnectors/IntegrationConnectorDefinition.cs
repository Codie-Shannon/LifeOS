using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public sealed record IntegrationConnectorDefinition(
    string Key,
    string DisplayName,
    IntegrationConnectorCategory Category,
    IntegrationConnectorFirstMode FirstMode,
    IntegrationConnectorRiskLevel RiskLevel,
    IntegrationConnectorRoadmapPhase RoadmapPhase,
    IntegrationSourceKind SourceKind,
    IntegrationConnectorActionPolicy ActionPolicy,
    IntegrationConnectorScope[] Scopes,
    IntegrationTargetKind[] SuggestedTargets,
    string Notes)
{
    public bool IsV5Candidate => RoadmapPhase == IntegrationConnectorRoadmapPhase.V5Foundation;

    public bool RequiresExplicitApproval =>
        RiskLevel is IntegrationConnectorRiskLevel.High or IntegrationConnectorRiskLevel.VeryHigh
        || ActionPolicy != IntegrationConnectorActionPolicy.PreviewOnly;

    public bool AllowsExternalMutation => false;
}

namespace LifeOS.Core.FinalOfflineOs;

public sealed class FinalOfflineOsSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public int TotalCheckpoints { get; set; }

    public int ReadyCheckpoints { get; set; }

    public int ReviewNeededCheckpoints { get; set; }

    public int RequiredForV4 { get; set; }

    public int LandingZones { get; set; }

    public int LandingZonesReadyForV4 { get; set; }

    public int PlannedForV4 { get; set; }

    public int AreaCount { get; set; }

    public bool LocalFirstComplete { get; set; }

    public bool ReadyForV4Integrations { get; set; }

    public bool ExternalIntegrationsEnabled { get; set; }

    public bool AiAssistantEnabled { get; set; }

    public bool MajorUiReshapeDeferred { get; set; }

    public bool ScreenshotDocsCurrent { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<OfflineOsCheckpoint> RequiredCheckpoints { get; set; } = [];

    public IReadOnlyList<OfflineOsCheckpoint> ReviewItems { get; set; } = [];

    public IReadOnlyList<IntegrationLandingZone> IntegrationLandingZones { get; set; } = [];
}

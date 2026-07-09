namespace LifeOS.Core.FinalOfflineOs;

public sealed class FinalOfflineOsProfile
{
    public string Version { get; set; } = "v3.9";

    public string Mode { get; set; } = "Final offline OS foundation";

    public bool LocalFirstComplete { get; set; } = true;

    public bool ReadyForV4Integrations { get; set; } = true;

    public bool ExternalIntegrationsEnabled { get; set; }

    public bool AiAssistantEnabled { get; set; }

    public bool MajorUiReshapeDeferred { get; set; } = true;

    public bool ScreenshotDocsCurrent { get; set; } = true;

    public bool DemoSafeDataRequired { get; set; } = true;

    public string Notes { get; set; } = "Final offline OS checkpoint before v4 integrations.";

    public List<OfflineOsCheckpoint> Checkpoints { get; set; } = [];

    public List<IntegrationLandingZone> LandingZones { get; set; } = [];
}

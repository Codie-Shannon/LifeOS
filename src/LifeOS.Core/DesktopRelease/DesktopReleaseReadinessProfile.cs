namespace LifeOS.Core.DesktopRelease;

public sealed class DesktopReleaseReadinessProfile
{
    public string Version { get; set; } = "v2.0";

    public string ReleaseLane { get; set; } = "Paid desktop release candidate";

    public bool IsPaidDesktopCandidate { get; set; } = true;

    public bool LocalOnlyMode { get; set; } = true;

    public bool RequiresManualReview { get; set; } = true;

    public bool TreatExpectedMoneyAsUnsafe { get; set; } = true;

    public bool ScreenshotPrivacyRequired { get; set; } = true;

    public bool DemoSafeDataRequired { get; set; } = true;

    public bool ExternalIntegrationsEnabled { get; set; }

    public string ReleaseNote { get; set; } = "v2.0 paid desktop release readiness checkpoint";

    public DateTime LastReviewedAt { get; set; } = DateTime.Now;

    public List<DesktopReleaseChecklistItem> Checklist { get; set; } = [];
}

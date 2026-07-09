namespace LifeOS.Core.SettingsSafety;

public sealed class SettingsSafetyThemeProfile
{
    public LifeOSSafetyMode SafetyMode { get; set; } = LifeOSSafetyMode.Strict;

    public LifeOSAppearancePreference Appearance { get; set; } = LifeOSAppearancePreference.Dark;

    public LifeOSAccentPreference Accent { get; set; } = LifeOSAccentPreference.Cyan;

    public bool LocalOnlyMode { get; set; } = true;

    public bool RequireManualReviewBeforeSend { get; set; } = true;

    public bool TreatExpectedMoneyAsUnsafe { get; set; } = true;

    public bool HidePrivateDetailsInScreenshots { get; set; } = true;

    public bool ConfirmDestructiveActions { get; set; } = true;

    public bool DemoSafeDataMode { get; set; } = true;

    public bool EnableExperimentalModules { get; set; }

    public string ActiveBuildLane { get; set; } = "Offline foundation";

    public string CurrentVersionNote { get; set; } = "v1.8 settings, safety, and theme foundation";

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

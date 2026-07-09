using LifeOS.Core.SettingsSafety;

namespace LifeOS.Shared.SettingsSafety;

public static class SettingsSafetyThemeDemoData
{
    public static SettingsSafetyThemeProfile CreateDefaultProfile()
    {
        return new SettingsSafetyThemeProfile
        {
            SafetyMode = LifeOSSafetyMode.Strict,
            Appearance = LifeOSAppearancePreference.Dark,
            Accent = LifeOSAccentPreference.Cyan,
            LocalOnlyMode = true,
            RequireManualReviewBeforeSend = true,
            TreatExpectedMoneyAsUnsafe = true,
            HidePrivateDetailsInScreenshots = true,
            ConfirmDestructiveActions = true,
            DemoSafeDataMode = true,
            EnableExperimentalModules = false,
            ActiveBuildLane = "Offline foundation",
            CurrentVersionNote = "v1.8 settings, safety, and theme foundation",
            UpdatedAt = DateTime.Now
        };
    }
}

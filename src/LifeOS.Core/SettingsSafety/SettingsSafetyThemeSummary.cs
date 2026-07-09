namespace LifeOS.Core.SettingsSafety;

public sealed class SettingsSafetyThemeSummary
{
    public string SafetyLabel { get; set; } = string.Empty;

    public string AppearanceLabel { get; set; } = string.Empty;

    public string AccentLabel { get; set; } = string.Empty;

    public int EnabledGuardrails { get; set; }

    public int ManualReviewGates { get; set; }

    public int PrivacyProtections { get; set; }

    public bool IsLocalFirst { get; set; }

    public bool IsScreenshotSafe { get; set; }

    public bool IsExpectedMoneyProtected { get; set; }

    public IReadOnlyList<string> GuardrailReasons { get; set; } = [];

    public IReadOnlyList<string> ThemeNotes { get; set; } = [];
}

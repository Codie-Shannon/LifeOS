namespace LifeOS.Core.SettingsSafety;

public static class SettingsSafetyThemeCalculator
{
    public static SettingsSafetyThemeSummary Calculate(SettingsSafetyThemeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var guardrails = new List<string>();
        var themeNotes = new List<string>();
        var enabledGuardrails = 0;
        var manualReviewGates = 0;
        var privacyProtections = 0;

        if (profile.LocalOnlyMode)
        {
            enabledGuardrails++;
            guardrails.Add("Local-only mode is active; integrations remain future work.");
        }
        else
        {
            guardrails.Add("Local-only mode is off; use only when integrations are deliberately being tested.");
        }

        if (profile.RequireManualReviewBeforeSend)
        {
            enabledGuardrails++;
            manualReviewGates++;
            guardrails.Add("Manual review is required before any client/admin wording leaves LifeOS.");
        }

        if (profile.TreatExpectedMoneyAsUnsafe)
        {
            enabledGuardrails++;
            manualReviewGates++;
            guardrails.Add("Expected money is protected and remains separate from safe-to-spend money.");
        }

        if (profile.HidePrivateDetailsInScreenshots)
        {
            enabledGuardrails++;
            privacyProtections++;
            guardrails.Add("Screenshot safety is on; demo captures should avoid private names, emails, URLs, IDs, and secrets.");
        }

        if (profile.ConfirmDestructiveActions)
        {
            enabledGuardrails++;
            manualReviewGates++;
            guardrails.Add("Destructive actions require confirmation before local state is reset or deleted.");
        }

        if (profile.DemoSafeDataMode)
        {
            enabledGuardrails++;
            privacyProtections++;
            guardrails.Add("Demo-safe data mode is on; fictional placeholders are preferred for screenshots and docs.");
        }

        if (profile.EnableExperimentalModules)
        {
            guardrails.Add("Experimental modules are visible; keep this local-only and do not treat it as production automation.");
        }
        else
        {
            enabledGuardrails++;
            guardrails.Add("Experimental modules are hidden by default until the offline foundation is stable.");
        }

        themeNotes.Add(profile.Appearance switch
        {
            LifeOSAppearancePreference.Dark => "Dark appearance is active and matches the current WPF shell.",
            LifeOSAppearancePreference.LightPlanned => "Light appearance is planned but not live-switched in v1.8.",
            LifeOSAppearancePreference.SystemPlanned => "System appearance is planned but not live-switched in v1.8.",
            _ => "Appearance preference recorded."
        });

        themeNotes.Add(profile.Accent switch
        {
            LifeOSAccentPreference.Cyan => "Cyan accent matches the current LifeOS visual language.",
            LifeOSAccentPreference.Green => "Green accent is recorded for later theme work.",
            LifeOSAccentPreference.Amber => "Amber accent is recorded for later theme work.",
            LifeOSAccentPreference.Violet => "Violet accent is recorded for later theme work.",
            _ => "Accent preference recorded."
        });

        return new SettingsSafetyThemeSummary
        {
            SafetyLabel = FormatSafetyMode(profile.SafetyMode),
            AppearanceLabel = FormatAppearance(profile.Appearance),
            AccentLabel = FormatAccent(profile.Accent),
            EnabledGuardrails = enabledGuardrails,
            ManualReviewGates = manualReviewGates,
            PrivacyProtections = privacyProtections,
            IsLocalFirst = profile.LocalOnlyMode,
            IsScreenshotSafe = profile.HidePrivateDetailsInScreenshots && profile.DemoSafeDataMode,
            IsExpectedMoneyProtected = profile.TreatExpectedMoneyAsUnsafe,
            GuardrailReasons = guardrails,
            ThemeNotes = themeNotes
        };
    }

    public static string FormatSafetyMode(LifeOSSafetyMode mode)
    {
        return mode switch
        {
            LifeOSSafetyMode.Strict => "Strict",
            LifeOSSafetyMode.Balanced => "Balanced",
            LifeOSSafetyMode.ExperimentalLocalOnly => "Experimental / local only",
            _ => mode.ToString()
        };
    }

    public static string FormatAppearance(LifeOSAppearancePreference appearance)
    {
        return appearance switch
        {
            LifeOSAppearancePreference.Dark => "Dark",
            LifeOSAppearancePreference.LightPlanned => "Light planned",
            LifeOSAppearancePreference.SystemPlanned => "System planned",
            _ => appearance.ToString()
        };
    }

    public static string FormatAccent(LifeOSAccentPreference accent)
    {
        return accent switch
        {
            LifeOSAccentPreference.Cyan => "Cyan",
            LifeOSAccentPreference.Green => "Green",
            LifeOSAccentPreference.Amber => "Amber",
            LifeOSAccentPreference.Violet => "Violet",
            _ => accent.ToString()
        };
    }
}

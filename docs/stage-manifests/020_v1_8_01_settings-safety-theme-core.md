# Stage 020 — v1.8 Settings / Safety / Theme Core

## Purpose

Add the core Settings / Safety / Theme profile and summary model.

## Files added

```text
src/LifeOS.Core/SettingsSafety/LifeOSAccentPreference.cs
src/LifeOS.Core/SettingsSafety/LifeOSAppearancePreference.cs
src/LifeOS.Core/SettingsSafety/LifeOSSafetyMode.cs
src/LifeOS.Core/SettingsSafety/SettingsSafetyThemeProfile.cs
src/LifeOS.Core/SettingsSafety/SettingsSafetyThemeSummary.cs
src/LifeOS.Core/SettingsSafety/SettingsSafetyThemeCalculator.cs
```

## Boundary

This is local state and guardrail modelling only. It does not add live theme switching, integrations, permissions, encryption, or account management.

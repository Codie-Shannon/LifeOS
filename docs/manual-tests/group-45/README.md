# Group 45 manual verification

Group 45 closes the LifeOS Desktop v8 operating experience at `v8.0.0-beta.1`.

## Product boundary

This group completes the permanent shell, Settings, themes, accessibility, contextual systems, evidence validation and release hardening. It does not add Microsoft or Google integrations, Full Mobile, money expansion, Career Studio, Grocery Planner, guarded writes or DevOps.

## Required checks

1. Launch Desktop from a clean synchronized `main` branch.
2. Confirm the only core pages are Home, Work, Career, Money, Life, Projects, Assistant and Settings.
3. Confirm Home opens by default with a new or reset preference file.
4. Select Last Used startup, close on another workspace, reopen, and confirm that workspace is restored.
5. Corrupt the local preference JSON manually, relaunch, and confirm safe Home/Dark/Purple/Comfortable defaults.
6. Verify Light, Dark, System and High Contrast themes.
7. Verify Purple, Blue and Teal accents.
8. Verify Comfortable and Compact density without record/count changes.
9. Verify 100%, 110%, 125% and 140% text scaling.
10. Verify Reduced Motion persists.
11. Open Review, Status, Profile and Emergency Stop from the permanent top bar.
12. Confirm Emergency Stop uses visible text for Idle, Armed and Stopped states.
13. Confirm the contextual panel persists, auto-opens where configured, returns keyboard focus when closed, and collapses under the narrow-width boundary.
14. Press `Ctrl+K`; verify the modal covers rail, top bar and workspace, and that no obsolete shell command exists.
15. Open every module card and confirm all authoritative records remain reachable through final allowlisted module routes.
16. Verify Settings contains Appearance; Startup/Navigation; Accessibility; Profiles; Privacy/Local Data; Sync/Companion; Automation/Emergency Stop; Integrations placeholder; About/Diagnostics.
17. Run the automated tests, Release builds, vulnerability checks, Gitleaks, repository hygiene and Git diff validation.

## Local preference file

The final v8 shell preference file is stored outside Git under the current user's local application-data folder:

```text
%LOCALAPPDATA%\LifeOS\v8-preferences.json
```

Resetting shell preferences does not delete operational module records.

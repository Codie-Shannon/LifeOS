# Stage 013 — v1.6.3 Daily Operating Flow Desktop Workspace

## Purpose

Add the Daily Operating Flow page to the existing WPF shell and surface daily-flow signals in Command Centre.

## Files changed

```text
LifeOS.Desktop/MainWindow.xaml
LifeOS.Desktop/MainWindow.xaml.cs
LifeOS.Shared/Shell/LifeOSModuleKind.cs
LifeOS.Shared/Shell/LifeOSModuleCatalog.cs
docs/lifeos-version-state.json
docs/release-notes/v1.6-code.md
```

## Scope

This is a local-first desktop workspace. It does not add notifications, calendar sync, AI scheduling, mobile reminders, or automatic task creation.

## Verification

- Daily Operating Flow nav button exists.
- `DailyOperatingFlowNavButton_Click` exists in code-behind.
- `ShowDailyOperatingFlowPage` exists.
- Existing WPF shell remains intact.
- No duplicate helper methods are added.

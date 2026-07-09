# Stage 024 — v1.8.1 Navigation Scroll Hotfix

## Purpose

Fix the v1.8 sidebar navigation so lower modules like Relationship Radar, Projects, TimerAgent, and Settings / Safety are reachable on smaller window heights.

## Problem

After v1.8, the left navigation list became taller than the visible sidebar. Settings / Safety was cut off and could not be reached.

## Fix

The left navigation buttons are now inside a sidebar `ScrollViewer`.

The title remains docked at the top and the v1.8 footer/status box remains docked at the bottom.

## Files changed

```text
LifeOS.Desktop/MainWindow.xaml
docs/stage-manifests/024_v1_8_05_nav-scroll-hotfix.md
```

## Commit message

```text
Fix sidebar navigation scrolling for v1.8
```

## Notes

This is a UI reachability hotfix only. It does not change models, storage, demo data, or module logic.

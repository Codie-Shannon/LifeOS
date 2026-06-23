# LifeOS Desktop v0.3 Release Notes

## Summary

LifeOS Desktop v0.3 upgrades LifeOS from a weekly pressure tracker into a work, income, and proof-aware command centre.

v0.2 made LifeOS understand the week. v0.3 makes it understand work sessions, billable value, unpaid work, proof items, and the proof wall behind real progress.

## Added

- Work Sessions foundation
- Work Session local JSON storage
- Work Session summary/calculation logic
- Proof Tracker foundation
- Proof Tracker local JSON storage
- Proof summary/calculation logic
- Work Sessions screen in MainWindow
- Proof Tracker screen in MainWindow
- Command Centre work/income/proof metrics
- v0.3 screenshot set
- v0.3 README update
- v0.3 roadmap update
- v0.3 release checklist

## Command Centre Updates

The Command Centre now reads more than money and weekly pressure.

It includes:

- overall pressure
- safe-to-spend
- agenda open count
- pay-later open amount
- open follow-ups
- billable value
- unpaid work value
- proof item count
- proof-ready status

## Work Sessions

Work Sessions tracks:

- client/project
- date
- hours
- rate
- billable status
- paid status
- description
- notes

This is the start of the future contractor/business work loop.

## Proof Tracker

Proof Tracker tracks:

- proof title
- project
- proof type
- status
- date
- description
- link/path
- notes

This is the start of the proof wall system.

## Architecture Notes

v0.3 keeps the same practical architecture direction:

- Core contains reusable models/calculators
- Shared contains local storage/services
- Desktop uses MainWindow as the only current view
- TimerAgent remains desktop-only

## Known Limits

- No database yet
- No mobile app yet
- No web app yet
- No cloud sync
- No user accounts
- No TimerAgent CSV import into Command Centre yet
- No invoice generation yet
- UI is still MainWindow-only by design

## Release Result

LifeOS Desktop v0.3 proves the app can connect personal pressure, weekly pressure, work sessions, unpaid value, and proof tracking in one local-first command centre.

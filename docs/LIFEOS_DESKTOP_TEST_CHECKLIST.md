# LifeOS Desktop v0.5 Test Checklist

Use this checklist before tagging or publishing the v0.5 release.

## Build

- [ ] Run `dotnet build`
- [ ] Confirm `LifeOS.Core` succeeds
- [ ] Confirm `LifeOS.Shared` succeeds
- [ ] Confirm `LifeOS.Modules.Timer` succeeds
- [ ] Confirm `LifeOS.TimerAgent` succeeds
- [ ] Confirm `LifeOS.Desktop` succeeds

## Navigation

- [ ] App opens without crashing
- [ ] Command Centre opens
- [ ] Money Pressure opens
- [ ] Money Timeline opens
- [ ] Agenda opens
- [ ] Pay Later opens
- [ ] Weekly Close-Out opens
- [ ] Work Sessions opens
- [ ] Paid Work Centre opens
- [ ] Proof Tracker opens
- [ ] Follow-Ups opens
- [ ] Projects opens
- [ ] TimerAgent opens
- [ ] Settings opens

## v0.5 Paid Work Centre

- [ ] Work Sessions can create a completed billable session
- [ ] Work Sessions source data displays client/project, date, hours, rate, billable status, and value
- [ ] Paid Work Centre opens after source work session exists
- [ ] Invoice-ready session count updates
- [ ] Invoice-ready value updates
- [ ] Unpaid billable value updates
- [ ] Billable hours updates
- [ ] Client/project spread updates
- [ ] Invoice-ready item list shows the session
- [ ] Copy-ready work summary is generated
- [ ] Empty state is clear when no invoice-ready sessions exist

## v0.5 Money Timeline

- [ ] Money Timeline opens
- [ ] Current balance displays
- [ ] Incoming by target date displays
- [ ] Outgoing/buffers displays
- [ ] Projected balance displays
- [ ] Lowest point displays
- [ ] Safe-to-spend displays
- [ ] Pressure label displays
- [ ] Timeline wording clearly states it is a projected/local planning view, not bank sync

## v0.5 Command Centre

- [ ] Command Centre heading/subtitle references v0.5
- [ ] Side panel references Desktop v0.5
- [ ] Paid Work Centre and Money Timeline appear in navigation
- [ ] Command Centre shows billable value
- [ ] Command Centre shows unpaid work
- [ ] Command Centre safe-to-spend value still displays
- [ ] Command Centre still handles existing v0.2/v0.3/v0.4 data without crashing

## Documentation

- [ ] README updated to v0.5
- [ ] README embeds six v0.5 screenshots
- [ ] Release notes updated
- [ ] Screenshot list updated
- [ ] Test checklist updated
- [ ] v0.5 release summary present
- [ ] v0.5 stages document present

## Screenshot files

- [ ] `docs/screenshots/01-lifeos-v05-command-centre-overview.png`
- [ ] `docs/screenshots/02-lifeos-v05-work-sessions-source-data.png`
- [ ] `docs/screenshots/03-lifeos-v05-paid-work-centre-metrics.png`
- [ ] `docs/screenshots/04-lifeos-v05-paid-work-centre-invoice-summary.png`
- [ ] `docs/screenshots/05-lifeos-v05-money-timeline-projected-balance.png`
- [ ] `docs/screenshots/06-lifeos-v05-command-centre-with-v05-data.png`

## Release commands

```bash
git status
git add .
git commit -m "Document LifeOS v0.5 paid work and money timeline release"
git push
git tag v0.5
git push origin v0.5
```

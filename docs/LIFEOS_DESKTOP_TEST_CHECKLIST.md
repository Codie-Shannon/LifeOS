# LifeOS Desktop v0.9 Test Checklist

Use this checklist before treating v0.9 as the release-candidate baseline before v1.0.

## Build

- [ ] Run `dotnet build`
- [ ] Confirm `LifeOS.Core` succeeds
- [ ] Confirm `LifeOS.Shared` succeeds
- [ ] Confirm `LifeOS.Modules.Timer` succeeds if present in the solution
- [ ] Confirm `LifeOS.TimerAgent` succeeds if present in the solution
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
- [ ] Work Pipeline opens
- [ ] Projects opens
- [ ] TimerAgent opens
- [ ] Settings opens

## v0.9 Work Pipeline

- [ ] Work Pipeline page opens
- [ ] Header shows v0.9/release-candidate wording
- [ ] Open pipeline count displays
- [ ] Active pipeline count displays
- [ ] Waiting count displays
- [ ] Blocked count displays
- [ ] Follow-up count displays
- [ ] Expected value displays as not-safe money
- [ ] Timesheet-needed count displays
- [ ] Invoice-needed count displays
- [ ] Payment expected count displays if visible
- [ ] Stage breakdown displays
- [ ] Today focus displays useful items
- [ ] Waiting/blocking state is visible
- [ ] Parked/keep-warm work does not dominate the today view

## v0.9 Command Centre

- [ ] Command Centre opens
- [ ] Command Centre subtitle references v0.9/release-candidate direction
- [ ] Pipeline open count displays
- [ ] Pipeline blocked count displays
- [ ] Pipeline follow-up count displays
- [ ] Expected pipeline money displays as not-safe money
- [ ] Billable value displays
- [ ] Unpaid work displays
- [ ] Safe-to-spend still displays
- [ ] Command Centre remains readable and does not become a CRM dashboard

## v0.9 Follow-Ups

- [ ] Follow-Ups opens
- [ ] Open follow-ups count displays
- [ ] Waiting count displays
- [ ] Needs action count displays
- [ ] Overdue count displays
- [ ] Due today count displays
- [ ] Money-linked count displays

## v0.9 Money Timeline

- [ ] Money Timeline opens
- [ ] Current balance displays
- [ ] Incoming by target date displays
- [ ] Outgoing/buffers displays
- [ ] Projected balance displays
- [ ] Lowest point displays
- [ ] Safe-to-spend displays
- [ ] Pressure label displays
- [ ] Timeline wording remains clear that this is local planning, not bank sync

## v0.9 Local storage

- [ ] Local LifeOS data folder exists
- [ ] `work-pipeline.json` exists
- [ ] `work-pipeline.backup.json` exists
- [ ] Follow-up JSON exists
- [ ] Agenda JSON exists
- [ ] Money pressure JSON exists
- [ ] Pay Later JSON exists
- [ ] Proof JSON exists
- [ ] Work Sessions JSON exists
- [ ] App can restart and still load Work Pipeline data

## Documentation

- [ ] README updated to v0.9
- [ ] README embeds six v0.9 screenshots
- [ ] Release notes updated to v0.9
- [ ] Screenshot list updated to v0.9
- [ ] Test checklist updated to v0.9
- [ ] Roadmap updated to show v1.0 as the next major target
- [ ] v0.9 release summary present
- [ ] v0.9 documents/screenshots summary present

## Screenshot files

- [ ] `docs/screenshots/01-lifeos-v09-command-centre.png`
- [ ] `docs/screenshots/02-lifeos-v09-work-pipeline-summary.png`
- [ ] `docs/screenshots/03-lifeos-v09-work-pipeline-stage-breakdown.png`
- [ ] `docs/screenshots/04-lifeos-v09-money-timeline.png`
- [ ] `docs/screenshots/05-lifeos-v09-follow-ups.png`
- [ ] `docs/screenshots/06-lifeos-v09-local-storage-proof.png`

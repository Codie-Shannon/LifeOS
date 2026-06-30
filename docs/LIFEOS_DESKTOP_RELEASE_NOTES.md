# LifeOS Desktop Release Notes

## v0.9 — Work Pipeline + Command Centre Release Candidate

LifeOS Desktop v0.9 is the release-candidate baseline before v1.0.

The v0.6 to v0.9 push adds the Work Pipeline foundation, follow-up/opportunity behaviour, Command Centre work pipeline signals, conservative expected-money visibility, stage counts, local storage backup safety, and final workflow polish.

## Main additions since v0.5

### Work Pipeline foundation

Work Pipeline tracks practical work and opportunity pressure without becoming a full CRM.

It supports:

- active paid work
- warm leads
- proof projects
- blocked work
- follow-up pressure
- timesheet needs
- invoice needs
- expected payment states
- parked ideas
- stage/status/priority tracking

### Follow-up and opportunity behaviour

The Work Pipeline now has enough structure to separate moving work from waiting work.

It makes visible:

- follow-ups due
- waiting-on work
- blocked work
- opportunity pressure
- today focus
- next actions

### Command Centre integration

Command Centre now reads Work Pipeline pressure and surfaces it beside money, agenda, follow-up, proof, and work-session signals.

It shows:

- pipeline open count
- pipeline blocked count
- pipeline follow-ups
- expected pipeline value
- billable work value
- expected money warning language

### Storage safety

Work Pipeline data is persisted locally with JSON storage and a backup file.

The local storage proof shows the LifeOS data folder containing module JSON files such as:

- `agenda-items.json`
- `follow-ups.json`
- `money-pressure-input.json`
- `pay-later-items.json`
- `proof-items.json`
- `work-pipeline.json`
- `work-pipeline.backup.json`
- `work-sessions.json`

## Release screenshots

| Screenshot | Purpose |
|---|---|
| `docs/screenshots/01-lifeos-v09-command-centre.png` | Shows v0.9 Command Centre with pipeline, money, follow-up, and proof pressure. |
| `docs/screenshots/02-lifeos-v09-work-pipeline-summary.png` | Shows Work Pipeline summary cards for open, active, waiting, blocked, follow-up, expected value, timesheets, and invoices. |
| `docs/screenshots/03-lifeos-v09-work-pipeline-stage-breakdown.png` | Shows stage breakdown and today focus items. |
| `docs/screenshots/04-lifeos-v09-money-timeline.png` | Shows safe-to-spend, incoming/outgoing, and projected balance. |
| `docs/screenshots/05-lifeos-v09-follow-ups.png` | Shows local follow-up foundation and money-linked follow-up pressure. |
| `docs/screenshots/06-lifeos-v09-local-storage-proof.png` | Shows local JSON module data and Work Pipeline backup file. |

## What v0.9 intentionally avoids

- no cloud sync
- no mobile app
- no client portal
- no bank sync
- no tax filing
- no full accounting ledger
- no final invoice/PDF generator
- no enterprise multi-user workflow
- no live hardware control

## Test status

Confirmed release checks for v0.9 should include:

- app builds successfully
- Command Centre opens
- Work Pipeline opens
- Work Pipeline summary cards display
- Work Pipeline stage breakdown displays
- today focus items display
- Follow-Ups opens
- Money Timeline opens
- expected pipeline value is clearly not safe money
- local `work-pipeline.json` exists
- local `work-pipeline.backup.json` exists
- app can be restarted without losing Work Pipeline data

## v1.0 direction

v1.0 should be the Unified Command Centre Foundation.

The next goal is not adding more modules. The next goal is making the existing modules work together so LifeOS can answer:

```text
What matters now?
```

# LifeOS v0.5 Documents and Screenshots

This document lists the v0.5 documentation and screenshot files included for the release.

## Documentation files updated

| File | Purpose |
|---|---|
| `README.md` | Main GitHub/project README, updated to v0.5 and embedded with six screenshots. |
| `docs/LIFEOS_DESKTOP_RELEASE_NOTES.md` | v0.5 release notes. |
| `docs/LIFEOS_DESKTOP_SCREENSHOT_LIST.md` | Active v0.5 screenshot set and screenshot story. |
| `docs/LIFEOS_DESKTOP_TEST_CHECKLIST.md` | Build, navigation, Paid Work Centre, Money Timeline, Command Centre, and docs release checks. |
| `docs/LIFEOS_V05_RELEASE_SUMMARY.md` | Short release summary and GitHub release text. |
| `docs/LIFEOS_V05_STAGES.md` | Staged commit breakdown for v0.5. |
| `docs/LIFEOS_V05_DOCUMENTS_AND_SCREENSHOTS.md` | This file. |

## Screenshot files included

| # | File | Purpose |
|---|---|---|
| 1 | `docs/screenshots/01-lifeos-v05-command-centre-overview.png` | Shows v0.5 Command Centre overview, navigation, and release wording. |
| 2 | `docs/screenshots/02-lifeos-v05-work-sessions-source-data.png` | Shows the source billable work session that feeds the Paid Work Centre. |
| 3 | `docs/screenshots/03-lifeos-v05-paid-work-centre-metrics.png` | Shows Paid Work Centre metrics with sample billable work. |
| 4 | `docs/screenshots/04-lifeos-v05-paid-work-centre-invoice-summary.png` | Shows copy-ready invoice/work summary output. |
| 5 | `docs/screenshots/05-lifeos-v05-money-timeline-projected-balance.png` | Shows paper-bills workflow: projected balance, lowest point, safe-to-spend, and pressure label. |
| 6 | `docs/screenshots/06-lifeos-v05-command-centre-with-v05-data.png` | Shows paid-work data feeding back into the Command Centre. |

## README embedded screenshot order

The README embeds the screenshots in this order:

```text
01 Command Centre overview
02 Work Sessions source data
03 Paid Work Centre metrics
04 Paid Work Centre invoice-ready summary
05 Money Timeline projected balance
06 Command Centre with v0.5 data
```

## Release proof chain

The screenshot set proves the v0.5 chain:

```text
Work Sessions source data
    -> Paid Work Centre metrics
    -> invoice-ready summary
    -> Money Timeline projected balance
    -> Command Centre visibility
```

## Drag/drop instructions

Copy or drag the contents of this zip into the root of the LifeOS working directory.

Expected target:

```text
C:\Projects\LifeOS
```

Then run:

```powershell
git status
git add .
git commit -m "Document LifeOS v0.5 paid work and money timeline release"
git push
```

# LifeOS Desktop v0.5 Screenshot List

This is the active screenshot list for the LifeOS Desktop v0.5 release.

v0.5 should use six primary screenshots. They show the full release story:

```text
Work Sessions -> Paid Work Centre -> invoice-ready summary -> Money Timeline -> Command Centre
```

## Required v0.5 screenshots

| # | File | Screen | What it proves |
|---|---|---|---|
| 1 | `docs/screenshots/01-lifeos-v05-command-centre-overview.png` | Command Centre | v0.5 wording, new modules, and overall command centre state. |
| 2 | `docs/screenshots/02-lifeos-v05-work-sessions-source-data.png` | Work Sessions | A completed billable work session exists as source data. |
| 3 | `docs/screenshots/03-lifeos-v05-paid-work-centre-metrics.png` | Paid Work Centre | Paid Work Centre reads billable work and produces paid-work metrics. |
| 4 | `docs/screenshots/04-lifeos-v05-paid-work-centre-invoice-summary.png` | Paid Work Centre | Copy-ready invoice/work summary output exists. |
| 5 | `docs/screenshots/05-lifeos-v05-money-timeline-projected-balance.png` | Money Timeline | Paper-bills workflow: projected balance, lowest point, safe-to-spend, pressure label. |
| 6 | `docs/screenshots/06-lifeos-v05-command-centre-with-v05-data.png` | Command Centre | v0.5 data feeds back into the command centre. |

## Embedded README screenshots

The README embeds the same six screenshots so the GitHub project page immediately shows the v0.5 proof.

## Screenshot story

### 1. Command Centre overview

Shows the app has moved to v0.5 and has the Paid Work Centre and Money Timeline visible in the side navigation.

### 2. Work Sessions source data

Shows the source work session that later appears in Paid Work Centre.

### 3. Paid Work Centre metrics

Shows invoice-ready sessions, invoice-ready value, unpaid billable value, paid value, billable hours, and client/project spread.

### 4. Invoice-ready summary

Shows the copy-ready summary that can be used in a client invoice/work summary email.

### 5. Money Timeline

Shows current balance, incoming, outgoing/buffers, projected balance, lowest point, safe-to-spend, and pressure label.

### 6. Command Centre with data

Shows the release loop: work and money data feeding back into the top-level LifeOS view.

## Screenshot rule going forward

For each release, keep one active screenshot set matching the current release.

Historical screenshots may remain in `docs/screenshots/`, but README should only embed the current active release screenshots unless there is a strong reason to show older release history.

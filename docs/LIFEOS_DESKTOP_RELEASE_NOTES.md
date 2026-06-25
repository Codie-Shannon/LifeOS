# LifeOS Desktop Release Notes

## v0.5 — Paid Work Centre + Money Timeline

LifeOS Desktop v0.5 adds the first paid-work admin and date-based money timeline layer.

This release connects the work-session foundation from v0.3/v0.4 to practical invoice-ready summaries and a simple projected-balance cashflow view.

## Main additions

### Paid Work Centre

The Paid Work Centre turns completed billable work sessions into invoice-ready admin.

It shows:

- invoice-ready session count
- invoice-ready value
- unpaid billable value
- paid value
- billable hours
- client/project spread
- invoice-ready item list
- copy-ready work summary text

The first version is intentionally copy-ready rather than full invoice/PDF generation. It gives a clean work summary that can be pasted into an invoice note, client email, or later PDF generator.

### Money Timeline

Money Timeline is the v0.5 paper-bills workflow.

It shows:

- current balance
- incoming by target date
- outgoing/buffers by target date
- projected balance
- lowest point
- safe-to-spend estimate
- pressure label

This is not bank sync. It is a local planning layer for answering: “After the incoming money and bills, what should still be safe?”

### Command Centre integration

The Command Centre now acknowledges v0.5 and surfaces paid-work/money-timeline meaning.

It shows v0.5 wording and includes paid-work metrics such as billable value and unpaid work.

## Release screenshots

| Screenshot | Purpose |
|---|---|
| `docs/screenshots/01-lifeos-v05-command-centre-overview.png` | Shows v0.5 Command Centre wording and navigation. |
| `docs/screenshots/02-lifeos-v05-work-sessions-source-data.png` | Shows completed billable work session source data. |
| `docs/screenshots/03-lifeos-v05-paid-work-centre-metrics.png` | Shows Paid Work Centre metrics after sample data. |
| `docs/screenshots/04-lifeos-v05-paid-work-centre-invoice-summary.png` | Shows the copy-ready invoice/work summary. |
| `docs/screenshots/05-lifeos-v05-money-timeline-projected-balance.png` | Shows projected balance, lowest point, safe-to-spend, and pressure label. |
| `docs/screenshots/06-lifeos-v05-command-centre-with-v05-data.png` | Shows Command Centre with v0.5 data feeding in. |

## What v0.5 intentionally avoids

- no full accounting ledger
- no GST/tax filing
- no bank sync
- no payment gateway
- no client portal
- no mobile app
- no final PDF invoice generator yet

## Test status

Confirmed release checks:

- app builds successfully
- Command Centre opens
- Paid Work Centre opens
- Work Sessions can create a completed billable item
- Paid Work Centre reads invoice-ready work session data
- copy-ready invoice/work summary is generated
- Money Timeline opens and displays projected balance data
- Command Centre reflects v0.5 paid-work and money-timeline direction

## Suggested tag

```bash
git tag v0.5
git push origin v0.5
```

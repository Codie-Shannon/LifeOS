# LifeOS Desktop

LifeOS Desktop is a local-first weekly pressure command centre for tracking money pressure, agenda items, deferred payments, weekly close-out, work sessions, proof items, follow-ups, projects, timers, and settings without turning the system into a heavy calendar, CRM, or accounting platform.

Current release: **LifeOS Desktop v0.4 — Trust Polish Release**

## Release position

LifeOS v0.4 is not a major new-module release. It is a trust, wording, safety, and documentation polish release that builds on v0.2 and v0.3.

The goal of v0.4 is to make LifeOS feel safer and clearer during real use:

- clearer Command Centre wording
- safer reset/delete confirmation wording
- better empty states
- less misleading copy
- clearer version labels
- updated screenshots and documentation
- preserved v0.2/v0.3 functionality

## What LifeOS currently covers

### v0.1 — First usable desktop foundation

LifeOS became a real desktop app with a reusable shell, local data direction, dark UI foundation, and early navigation structure.

### v0.2 — Weekly pressure foundation

v0.2 added the first practical weekly-life systems:

- Agenda
- Pay Later
- Weekly Close-Out
- early Command Centre metrics
- clearer weekly pressure framing
- first screenshot/documentation pass

This made LifeOS useful for keeping the week visible without needing a full calendar app.

### v0.3 — Work, income, and proof foundation

v0.3 added the work/proof layer:

- Work Sessions
- Proof Tracker
- work and income metrics in the Command Centre
- proof visibility
- better MainWindow wiring
- updated v0.3 docs and screenshots

This made LifeOS start connecting time, billable work, invoices, proof items, and portfolio evidence.

### v0.4 — Trust polish release

v0.4 improves the reliability and user trust of the existing system:

- version labels updated to v0.4
- Command Centre wording clarified
- empty states improved so screens explain what to do next
- reset confirmations made safer and more explicit
- side-panel wording updated
- docs/screenshots refreshed into one full v0.4 set

## Main modules

### Command Centre

The Command Centre reads local LifeOS data across money, agenda, pay-later, close-out, follow-ups, work sessions, and proof tracking.

It gives a high-level view of:

- overall pressure
- safe-to-spend position
- open agenda items
- open pay-later/deferred payments
- open follow-ups
- billable value
- unpaid work
- proof items
- proof readiness

### Money Pressure

Money Pressure is the financial pressure view. It is intended to show what money is actually safe, what obligations exist, and what work/invoices may affect the week.

### Agenda

Agenda is a light weekly pressure list, not a full calendar replacement.

It tracks important commitments that can affect the week, including:

- title
- due date
- time
- type
- status
- pressure level
- notes
- fixed commitment flag

### Pay Later

Pay Later tracks deferred payment pressure such as Afterpay, bills, obligations, or future payments that can distort available money.

It tracks:

- item name
- payee
- amount
- due date
- status
- pressure
- notes

### Weekly Close-Out

Weekly Close-Out exists to summarize what happened, what carried forward, what needs attention, and what should be made visible before the next week.

### Work Sessions

Work Sessions connects real work to income, invoices, proof, and follow-up pressure.

It tracks:

- client/project
- date
- hours
- hourly rate
- status
- billable flag
- description
- notes

### Proof Tracker

Proof Tracker tracks what was built, shown, paid, accepted, or made reusable.

It tracks:

- project
- title
- type
- status
- date
- link/path
- description
- notes

### Follow-Ups

Follow-Ups tracks waiting-on items and reminders connected to work, clients, money, proof, or personal admin.

### Projects

Projects is the project visibility area for keeping important work streams findable.

### TimerAgent

TimerAgent is the early timer/work-session direction for future work tracking.

### Settings

Settings contains local app settings and future configuration direction.

## v0.4 screenshot set

The current full v0.4 screenshot set is stored in:

```text
docs/screenshots/
```

Recommended active screenshots:

```text
01-lifeos-v04-command-centre.png
02-lifeos-v04-agenda-empty-state.png
03-lifeos-v04-pay-later-empty-state.png
04-lifeos-v04-work-sessions-empty-state.png
05-lifeos-v04-proof-tracker-empty-state.png
06-lifeos-v04-proof-tracker-reset-confirmation.png
```

These replace the old active v0.3 screenshot set. The v0.3 release history is now merged into the documentation rather than kept as separate active docs.

## Local-first direction

LifeOS is intended to remain local-first while the core system is still being proven.

Current principle:

```text
MainWindow only. Core/Shared stay reusable. Local data stays safe.
```

Future versions may introduce stronger data models, invoice generation, client proof packaging, and eventually mobile or sync support, but desktop/core remains the priority for now.

## What v0.4 proves

v0.4 proves that LifeOS can be improved through a disciplined polish pass rather than only adding features.

It now better supports real messy days where the user needs to quickly answer:

- What is still open?
- What pressure exists this week?
- What work may become income?
- What proof exists?
- What is safe to reset?
- What needs attention next?

## Suggested release tag

```bash
git tag v0.4
git push origin v0.4
```

## Suggested commit message

```bash
git add .
git commit -m "Add LifeOS v0.4 merged docs and screenshots"
```

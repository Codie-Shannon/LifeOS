# LifeOS Roadmap

## Purpose

LifeOS is a weekly pressure command centre.

It exists to show what money, work, payments, deductions, agenda items, follow-ups, deferred obligations, weekly close-out items, client work, and imported outside data are putting pressure on the week, then help decide what is safe to do next.

LifeOS is not mainly a budget app, calendar app, task app, timer app, banking app, CRM, or email client.

Those are inputs and modules.

LifeOS is the pressure layer that connects them.

## Platform Rule

Desktop is the daily-use power version and proving ground.

Mobile is the daily-use optimized version and pressure test.

Core and Shared projects preserve reusable logic.

TimerAgent remains a desktop-only utility that feeds work, time, income, tax set-aside, and proof data into LifeOS.

Imported data is never trusted automatically.

The core rule is:

```text
Imported does not mean trusted until reviewed.
```

Microsoft Graph, Google APIs, bank imports, email parsing, calendar imports, and future automation should all follow the same review-first model.

## Version Overview

```text
v0.1 = LifeOS exists                          Done
v0.2 = LifeOS understands the week            Done
v0.3 = LifeOS understands work, income, proof Next
v0.4 = LifeOS becomes trustworthy
v0.5 = LifeOS imports outside data safely
v0.6 = LifeOS helps make decisions
v0.7 = LifeOS manages client/work operating loop
v0.8 = LifeOS manages business operations
v0.9 = release-candidate serious
v1.0 = stable local-first desktop milestone
v2.0 = serious multi-platform proof
v3+  = contractor/business edition direction
```

---

# v0.1 — LifeOS Exists

## Theme

Proof, shell, and first real command centre.

## Status

Done.

## Included

- WPF desktop shell
- shared Core / Shared architecture
- shared module catalog
- Money Pressure module foundation
- manual Money Pressure inputs
- local JSON persistence for Money Pressure
- Follow-Ups module foundation
- local JSON persistence for Follow-Ups
- real Command Centre summary
- TimerAgent correctly framed as desktop-only
- README, screenshots, docs, main merge, tag/release

## Result

LifeOS crossed from idea into working proof.

---

# v0.2 — LifeOS Understands the Week

## Theme

Weekly pressure command centre upgrade.

## Status

Done.

## Included

### Agenda Foundation

Added a manual/local-first Agenda module.

Fields:

```text
Title
Type
Status
PressureLevel
DueDate
TimeText
IsFixedCommitment
Notes
CreatedAt
UpdatedAt
```

Command Centre / module pressure direction:

```text
open agenda items
due today
overdue items
this week items
high-pressure items
```

### Pay Later Foundation

Added a local-first Pay Later tracker for deferred obligations.

Fields:

```text
Name
Payee
Amount
DueDate
Status
PressureLevel
Notes
CreatedAt
UpdatedAt
```

Pressure direction:

```text
open items
open amount
due-this-week amount
overdue amount
high-pressure count
```

### Weekly Close-Out Foundation

Added a weekly review module that captures:

```text
What got done
What moved
What is still waiting
What pressure carries into next week
Notes
```

Pressure direction:

```text
total entries
current-week entries
current-week close-out status
waiting-on count
```

### MainWindow v0.2 Wiring

v0.2 keeps the app fast and simple by wiring new modules into `MainWindow` instead of creating separate WPF views yet.

### Screenshots and Docs

v0.2 includes updated screenshots, README, release notes, test checklist, and screenshot list.

## Not Included Yet

- bank sync
- email sync
- calendar API sync
- mobile app
- AI agent layer
- database migration
- TimerAgent CSV import into Command Centre
- Work Sessions
- Proof Tracker

## Result

LifeOS now understands the week through:

```text
Money Pressure + Follow-Ups + Agenda + Pay Later + Weekly Close-Out
```

---

# v0.3 — LifeOS Understands Work, Income, and Proof

## Theme

Work, income, and proof loop.

## Status

Next.

## Goal

Make LifeOS understand what work was done, what money it created, what is unpaid, and what proof/case-study value came from it.

## Main Scope

### Work Sessions Foundation

Add local-first work sessions.

Fields:

```text
Client / organisation
Project / context
Date
Hours
Rate
Billable yes/no
Status
Description
```

Summary:

```text
work sessions this week
hours worked
billable hours
billable value created
paid / unpaid / pending status
```

### Income Estimate

LifeOS should understand:

```text
Earned but unpaid
Invoiced but unpaid
Paid
```

Core rule:

```text
Earned money is not spendable money until paid.
```

### Proof Tracker Foundation

Fields:

```text
Project
Client / context
Proof type
Description
Date
Link or path
Status
Outcome / business value
```

### Command Centre v3

Add:

```text
hours worked this week
billable value created
pending work income
proof items created
unpaid work risk
```

---

# v0.4 — LifeOS Becomes Trustworthy

## Theme

Data safety, review, and reliability.

## Main Scope

- backup / restore
- open data folder
- data health page
- corrupt/missing JSON detection
- real settings
- detail/edit screens
- safer local persistence

---

# v0.5 — LifeOS Imports Outside Data Safely

## Theme

External data, import flows, and controlled automation.

## Main Scope

- Import Centre
- TimerAgent CSV import
- manual CSV imports
- calendar ICS import
- review queue
- duplicate detection

---

# v0.6 — LifeOS Helps Make Decisions

## Theme

Guidance, planning, and decision support.

## Main Scope

- decision engine foundation
- next safest action improvements
- Can-I-do-this checker
- today/tomorrow plan
- draft actions that require user approval

---

# v0.7 — LifeOS Manages Client / Work Operating Loop

## Theme

Client work, follow-ups, work sessions, proof, payment pressure.

## Main Scope

- client/work loop
- invoice readiness
- payment follow-up pressure
- proof/case-study tracker integration
- contractor command centre direction

---

# v0.8 — LifeOS Manages Business Operations

## Theme

Business operating system foundations.

## Main Scope

- business workflow tracking
- documents/processes
- reports
- recurring work
- client operating rhythm

---

# v0.9 — Release-Candidate Serious

## Theme

Hardening and presentation.

## Main Scope

- bug fixes
- UI consistency
- release packaging
- documentation pass
- demo data pass
- case-study README polish

---

# v1.0 — Stable Local-First Desktop Milestone

## Theme

Stable desktop proof.

## Goal

LifeOS is usable as a serious local-first desktop command centre with stable modules, screenshots, docs, and repeatable release process.

---

# v2.0 — Serious Multi-Platform Proof

## Theme

Core stable enough that mobile/web direction can be proven without rebuilding the product idea from scratch.

## Note

Mobile matters because many normal users are mobile-first, but the current priority is still desktop/core until the engine is stable enough to carry into a mobile companion cleanly.

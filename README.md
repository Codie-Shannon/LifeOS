# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, and pressure into visible state.

## Current build

**LifeOS Desktop v4.9 - Integration Inbox + v5 Readiness**

v4.9 completes the v4 operating spine. External-looking data is staged as read-only previews with provenance, duplicate checks, deliberate review states, and explicit manual handoff contracts.

## Current operating rule

Everything important becomes an item. Every item has state. Every state affects pressure. Every pressure signal feeds the Command Centre.

## v4 spine completed

- v4.0 Spine Recovery Map
- v4.1 Item Type / State Engine
- v4.2 Bills / Upcoming Payments / Pay Later
- v4.3 Money Profile / Hidden Deductions / Safe-to-Spend
- v4.4 Agenda + Payment Calendar
- v4.5 Work Pipeline
- v4.6 Receipt OCR / Evidence-to-Item
- v4.7 Weekly Close-Out
- v4.8 Command Centre Pressure Engine
- v4.9 Integration Inbox + v5 Readiness

## Safety boundary

LifeOS v4.9 is local and review-first. It does not connect to live email, calendars, accounting, files, OCR, banks, or BNPL providers. It does not send messages, move money, create invoices, trust imported data automatically, mutate external systems, or execute AI actions.

## Current screenshot evidence

- `docs/screenshot-groups/group-19-integration-inbox-v5-readiness/`
- `docs/release-notes/v4.9.md`
- `docs/current-status.md`

## Build

```powershell
dotnet build .\LifeOS.slnx
```

## Repository shape

- `LifeOS.Desktop/` - WPF desktop application.
- `LifeOS.Shared/` - local storage, demo data, and shared services.
- `src/LifeOS.Core/` - domain models, rules, and calculators.
- `src/LifeOS.Modules.Timer/` - timer module.
- `src/LifeOS.TimerAgent/` - timer agent utility.
- `docs/` - release notes, screenshot evidence, manifests, and current-state documentation.

## Next lane

v5.0 begins real integrations cautiously: one narrow read-only connector, strict scopes, explicit preview review, duplicate detection, provenance, and reversible manual handoff.

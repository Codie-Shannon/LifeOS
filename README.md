# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v5.0-alpha - first live read-only connector**

The v4 operating spine remains complete. v5.0-alpha now includes guarded local CSV, JSON, and ICS preview imports plus one narrow authenticated Google Calendar read-only connector.

Google Calendar uses explicit connection, `calendar.readonly`, manual bounded refresh, untrusted Integration Inbox previews, duplicate protection, provenance, audit history, human review, and explicit disconnect. It does not write to Google Calendar or automatically update LifeOS modules.

## Current operating rule

Everything important becomes an item. Every item has state. Every state affects pressure. Every pressure signal feeds the Command Centre.

## Integration safety flow

```text
external record
-> untrusted read-only preview
-> source-backed review
-> accepted preview
-> explicit manual handoff/link
-> trusted LifeOS state
```

Connectors must not write directly to Money, Agenda, Work Pipeline, Evidence, Follow-Ups, or Command Centre. Imported or expected money is visible for planning but never safe money until reviewed and confirmed.

## Current connector capability

- Local CSV, JSON, and ICS preview imports.
- Google Calendar read-only OAuth connection.
- Manual refresh only.
- User-confirmed date range, capped at 31 days.
- Existing Integration Inbox intake and review engine.
- Stable external references and duplicate keys.
- Duplicate-suspected repeated refresh handling.
- Provenance and audit history.
- Explicit disconnect and local token-cache deletion.

## Safety boundary

LifeOS does not currently provide calendar writes, Gmail or Outlook scanning, bank feeds, background synchronization, scheduled refresh, automatic preview acceptance, automatic module creation, money movement, email sending, or AI actions.

External data remains untrusted by default. Acceptance and target-module handoff remain separate deliberate actions.

## Current screenshot evidence

- `docs/screenshot-groups/group-22-google-calendar-read-only/`
- `docs/release-notes/v5.0-alpha.md`
- `docs/current-status.md`

## Build

```powershell
dotnet build .\LifeOS.slnx -c Release
```

## Test

```powershell
dotnet test .\LifeOS.slnx
```

The core regression suite lives in `tests/LifeOS.Core.Tests/` and covers Integration Inbox intake, duplicate handling, bounded calendar retrieval, provider mapping, audit creation, no automatic acceptance/linking, money safety, pressure ranking, week boundaries, receipt evidence, payment calendar, money profile, and weekly close-out.

## Repository shape

- `LifeOS.Desktop/` - WPF desktop application.
- `LifeOS.Shared/` - local storage, demo data, and shared services.
- `src/LifeOS.Core/` - domain models, rules, connector intake, and calculators.
- `src/LifeOS.Modules.Timer/` - timer module.
- `src/LifeOS.TimerAgent/` - timer agent utility.
- `docs/` - release notes, screenshot evidence, manifests, and current-state documentation.

## Local storage and connector configuration

LifeOS stores local state under:

```text
%LOCALAPPDATA%\LifeOS
```

Google connector configuration and protected OAuth tokens remain in the local user profile and are not committed. Disconnect deletes the local token cache while preserving imported review evidence.

## CI

GitHub Actions runs restore, Release build, and `dotnet test` on pushes and pull requests to `main`.

## Next lane

Return to Master Roadmap after the completed Group 22 checkpoint. Group 23 has not started.

# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v5.0.0-alpha.4 — Email Radar foundation**

Group 24 is complete. LifeOS now includes a provider-neutral local/imported Email Radar foundation with user-guided profiles, inert JSON/CSV evidence, deterministic explained candidate matching, explicit confirm/reject review, confirmed-only communication timelines, and review-first waiting-on suggestions.

Google Calendar remains the first authenticated read-only connector. Refresh is explicit, manual, date-bounded, review-first, and unable to write to Google Calendar or mutate LifeOS modules automatically.

## Integration safety flow

```text
external record
-> untrusted read-only preview
-> source-backed review
-> accepted preview
-> explicit manual handoff/link
-> trusted LifeOS state
```

Email Radar follows the same rule: imported evidence remains untrusted, candidate matches require confirmation, and communication suggestions do not create Follow-Ups or change Work Pipeline state automatically.

## Current capability

- Local CSV, JSON, and ICS preview imports.
- Google Calendar read-only OAuth connection with manual bounded refresh.
- Local/imported Email Radar profiles and communication evidence.
- Deterministic candidate matching with visible reasons.
- Explicit confirm/reject review and duplicate suspicion.
- Confirmed-only communication timeline.
- Review-first waiting-on/follow-up suggestions.
- Provenance and audit retention.

## Safety boundary

LifeOS does not currently provide Gmail, Outlook, Graph, IMAP, POP3, mailbox scanning, email sending, inbox mutation, calendar writes, background synchronization, scheduled refresh, automatic Follow-Up creation, automatic Work Pipeline mutation, or AI inbox interpretation.

## Current screenshot evidence

- [Group 24 — Email Radar foundation](docs/screenshot-groups/group-24-email-radar-foundation/README.md)
- [Group 23 — Connector lifecycle](docs/screenshot-groups/group-23-connector-lifecycle/README.md)
- `docs/release-notes/v5.0-alpha-group-24.md`
- `docs/current-status.md`

## Build and test

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

The verified Group 24 regression result is **80 passed, 0 failed, 0 skipped**.

## Repository shape

- `LifeOS.Desktop/` — WPF desktop application.
- `LifeOS.Shared/` — local storage and shared services.
- `src/LifeOS.Core/` — domain models, rules, connector intake, and Email Radar logic.
- `docs/` — release notes, screenshot evidence, integration contracts, and current-state documentation.

## Local storage and connector configuration

LifeOS stores local state under `%LOCALAPPDATA%\LifeOS`. Provider credentials and protected OAuth tokens remain local and are not committed.

## Next lane

Return control to the LifeOS Master / Roadmap chat. Group 25 has not started.

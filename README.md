# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-alpha.1 — Controlled automation foundation**

Screenshot Group 26 Pack 1 is active. LifeOS now includes one private/testing-mode authenticated Gmail connector using only `gmail.readonly`. Searches are explicit, profile-bound, date-bounded, result-capped, previewed before retrieval, and manually confirmed.

Gmail results normalize into the existing provider-neutral Email Radar communication-evidence model. They remain untrusted, pass through duplicate detection and deterministic candidate matching, require explicit confirm/reject review, and only confirmed evidence enters the communication timeline.

## Integration safety flow

```text
authenticated Gmail result
-> untrusted provider-neutral communication evidence
-> duplicate detection and deterministic candidate
-> source-backed review
-> explicit confirm or reject
-> confirmed communication timeline
-> review-first suggestion
-> optional explicit later handoff to trusted LifeOS state
```

## Current capability

- Local CSV, JSON, and ICS preview imports.
- Google Calendar read-only OAuth connection with manual bounded refresh.
- Provider-neutral Email Radar profiles and communication evidence.
- One authenticated Gmail account in private/testing mode.
- Manual Gmail search only, bounded to 31 days.
- Default result cap 25; hard maximum 100.
- Visible generated Gmail query and reviewable noise exclusions.
- Exact `gmail.readonly` scope; no modify/send scope.
- Safe inert snippets, message/thread references, provenance, audit, and duplicate handling.
- Deterministic candidate matching with visible reasons.
- Explicit confirm/reject review.
- Confirmed-only communication timeline.
- Review-first waiting-on/follow-up suggestions.
- Disconnect and local connector-cache clearing while imported evidence remains retained.

## Safety boundary

LifeOS does not send, draft, reply, forward, archive, trash, delete, label, star, or mark Gmail messages read/unread. It does not download attachments, render active HTML, load remote images, scan the mailbox in the background, schedule searches, use Gmail push/history monitoring, create Follow-Ups automatically, mutate Work Pipeline automatically, or perform AI email interpretation.

## Current screenshot evidence

- [Group 25 â€” Authenticated read-only Gmail connector](docs/screenshot-groups/group-25-read-only-gmail-connector/README.md)
- [Group 24 â€” Email Radar foundation](docs/screenshot-groups/group-24-email-radar-foundation/README.md)
- [Group 23 â€” Connector lifecycle](docs/screenshot-groups/group-23-connector-lifecycle/README.md)
- `docs/release-notes/v5.0-alpha-group-25.md`
- `docs/current-status.md`

## Build and test

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

The verified Group 25 regression result is **91 passed, 0 failed, 0 skipped**.

## Repository shape

- `LifeOS.Desktop/` â€” WPF desktop application.
- `LifeOS.Shared/` â€” local storage and shared connector services.
- `src/LifeOS.Core/` â€” domain models, rules, connector intake, and Email Radar logic.
- `docs/` â€” release notes, screenshot evidence, integration contracts, and current-state documentation.

## Local storage and connector configuration

LifeOS stores local state under `%LOCALAPPDATA%\LifeOS`. Provider credentials and protected OAuth tokens remain local and are not committed.

## Next lane

Return control to the LifeOS Master / Roadmap chat. Group 26 has not started.

- [v5 integration overview](docs/integrations/v5-integration-overview.md)
- [v5 release validation matrix](docs/integrations/v5-release-validation-matrix.json)

- [Group 26 — v5 integration release checkpoint](docs/screenshot-groups/group-26-v5-integration-release-checkpoint/README.md)


## v6 controlled automation foundation

Group 27 adds disabled-by-default deterministic rules, manual dry-run evaluation, explained condition results, a proposed-action review queue, explicit approve/reject decisions, duplicate detection and retained audit. Approval records intent only; no unattended action, external write, financial mutation, destructive action, email/calendar mutation, script execution or AI automation is enabled.

# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-alpha.2 — Guarded internal automation**

Group 28 product code adds a persisted execution pause, separate approval and final confirmation, immediate eligibility revalidation, one typed Low-risk reversible internal review-note action, before/after snapshots, idempotency, audit and explicit Undo. Screenshot evidence is pending. No unattended, external, communication, calendar, financial, destructive, script, process, plugin or AI execution is enabled.

## Controlled automation flow

```text
reviewed or trusted LifeOS state
-> deterministic manual rule evaluation
-> explained condition results
-> proposed action with target, risk and permissions
-> explicit human approval or rejection
-> retained audit and provenance
-> operational execution remains disabled
```

## Current capability

- Four safe fictional demonstration rules.
- Rules disabled by default.
- Manual dry-run evaluation only.
- Deterministic trigger and condition checks.
- Expected and actual values shown with pass/fail results.
- Reviewed/trusted source-state requirement.
- Explicit proposed action, target module, risk and approval policy.
- Capability policy showing allowed, approval-required and blocked permissions.
- Proposed-action review queue.
- Explicit approve/reject decisions.
- Approval without operational execution.
- Stable duplicate-proposal detection and prior-proposal linkage.
- High-risk communication and external-write actions blocked by policy.
- Retained inert automation audit.
- Local JSON persistence with backup recovery.

## Safety boundary

LifeOS does not run rules in the background, schedule automation, execute at startup, retry automatically, create Follow-Ups automatically, mutate Work Pipeline automatically, send email, modify Gmail, change calendars, move money, alter trusted payment state, delete evidence or operational records, run scripts, launch arbitrary processes, call arbitrary URLs, execute plugins, or use AI to generate or decide rules.

## Current screenshot evidence

- [Group 27 — Controlled automation foundation](docs/screenshot-groups/group-27-controlled-automation-foundation/README.md)
- [Group 26 — v5 integration release checkpoint](docs/screenshot-groups/group-26-v5-integration-release-checkpoint/README.md)
- [Group 25 — Authenticated read-only Gmail connector](docs/screenshot-groups/group-25-read-only-gmail-connector/README.md)
- [Group 24 — Email Radar foundation](docs/screenshot-groups/group-24-email-radar-foundation/README.md)

## Build and test

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

The verified Group 27 regression result is **112 passed, 0 failed, 0 skipped**. The Release build and `git diff --check` passed.

## Repository shape

- `LifeOS.Desktop/` — WPF desktop application.
- `LifeOS.Shared/` — local storage and shared services.
- `src/LifeOS.Core/` — domain models, deterministic rules and policy.
- `docs/` — current state, release notes, architecture and screenshot evidence.

## Local storage

LifeOS stores local state under `%LOCALAPPDATA%\LifeOS`. Automation storage contains inert rule, evaluation, proposal, decision and audit data only. It does not store executable scripts, provider secrets or arbitrary dynamic expressions.

## Next lane

Return control to the LifeOS Master Roadmap chat. Group 28 has not started.

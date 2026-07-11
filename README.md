# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-alpha.1 â€” Controlled automation foundation**

Screenshot Group 27 is complete. LifeOS now includes a visible, deterministic automation safety spine built around reviewed or trusted local state, manual dry-run evaluation, explained condition results, explicit proposed actions, risk and capability policy, human approval or rejection, duplicate detection, retained audit, and local persistence.

Approval records valid intent only. It does not execute an operational change in `v6.0.0-alpha.1`.

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

- [Group 27 â€” Controlled automation foundation](docs/screenshot-groups/group-27-controlled-automation-foundation/README.md)
- [Group 26 â€” v5 integration release checkpoint](docs/screenshot-groups/group-26-v5-integration-release-checkpoint/README.md)
- [Group 25 â€” Authenticated read-only Gmail connector](docs/screenshot-groups/group-25-read-only-gmail-connector/README.md)
- [Group 24 â€” Email Radar foundation](docs/screenshot-groups/group-24-email-radar-foundation/README.md)

## Build and test

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

The verified Group 27 regression result is **112 passed, 0 failed, 0 skipped**. The Release build and `git diff --check` passed.

## Repository shape

- `LifeOS.Desktop/` â€” WPF desktop application.
- `LifeOS.Shared/` â€” local storage and shared services.
- `src/LifeOS.Core/` â€” domain models, deterministic rules and policy.
- `docs/` â€” current state, release notes, architecture and screenshot evidence.

## Local storage

LifeOS stores local state under `%LOCALAPPDATA%\LifeOS`. Automation storage contains inert rule, evaluation, proposal, decision and audit data only. It does not store executable scripts, provider secrets or arbitrary dynamic expressions.

## Next lane

Return control to the LifeOS Master Roadmap chat. Group 28 has not started.

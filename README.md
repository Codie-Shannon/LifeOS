# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships, and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-alpha.3 — Controlled orchestration and recovery**

Screenshot Group 28 is complete. LifeOS now supports one deliberately narrow, approval-gated and reversible internal execution path. Approval never executes. The user must separately open a final before/after preview and explicitly confirm execution. Eligibility is recalculated immediately before execution, stale state fails closed, successful execution retains before/after evidence, and explicit Undo restores the prior fictional internal state when safe.

## Guarded execution flow

```text
reviewed or trusted LifeOS state
-> deterministic manual evaluation
-> proposal
-> explicit approval
-> ApprovedNotExecuted
-> immediate eligibility revalidation
-> final before/after preview
-> explicit final confirmation
-> typed reversible internal mutation
-> persisted result and audit
-> optional explicit Undo
```

## Current capability

- Rules remain disabled by default.
- Evaluation remains manual.
- Global guarded execution starts paused.
- Dry-run and approval remain available while execution is paused.
- Approval remains separate from execution.
- One typed Low-risk reversible internal review-note handler is allowlisted.
- Final execution preview shows the exact action, target, before state, proposed after state, risk, reversibility and policy checks.
- Eligibility is recalculated immediately before execution.
- Source-state changes mark the proposal stale and require reevaluation.
- Successful execution retains before/after snapshots and exposes Undo.
- Undo requires explicit user action, restores the exact prior internal state and retains execution history.
- Duplicate, stale, expired, unapproved, unsupported, high-risk and blocked-capability proposals cannot execute.
- Communication, external-write, mailbox, calendar, financial, destructive, script, process, plugin and AI capabilities remain blocked.
- No unattended execution, background worker, scheduler, timer, startup execution or automatic retry exists.

## Current screenshot evidence

- [Group 28 — Controlled orchestration and recovery](docs/screenshot-groups/group-28-guarded-internal-automation/README.md)
- [Group 27 — Controlled automation foundation](docs/screenshot-groups/group-27-controlled-automation-foundation/README.md)
- [Group 26 — v5 integration release checkpoint](docs/screenshot-groups/group-26-v5-integration-release-checkpoint/README.md)

## Build and test

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

The verified Group 28 result is **116 passed, 0 failed, 0 skipped**. The Release build, `git diff --check`, guarded-execution validation, push synchronization and clean-tree checks passed.

## Safety boundary

Only the allowlisted fictional/internal review-note proof action may execute. No active Follow-Up is created automatically. No Work Pipeline operational next action is changed automatically. No external, communication, mailbox, calendar, financial, destructive, script, process, plugin or AI action may execute.

## Next lane

Return control to the LifeOS Master Roadmap chat. Group 29 has not started.


## Group 29 — Controlled orchestration and recovery

Schedules now create review intent only. Due occurrences enter a visible queue, runs require explicit Start, and every Low-risk reversible internal step requires its own exact preview and confirmation. Progress pauses between steps; failure recovery is explicit and persisted. No unattended execution or external writes are enabled.

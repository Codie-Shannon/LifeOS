# LifeOS current status

## Current release

- Version: **v6.0.0-beta.1**
- Group: **31 — v6 controlled automation release checkpoint**
- State: Pack 1 implementation ready for runner validation and screenshot proof.

## Release checkpoint

LifeOS now exposes one persisted-state Automation Readiness view across the v5-to-v6 trust, proposal, approval, guarded execution, orchestration, failure, recovery, rollback and audit path. The v6 store uses explicit schema handling, backup-before-migration and fail-closed malformed-state recovery.

## Safety boundary

Execution remains manual, foreground-only, typed, Low-risk, internal, reversible and explicitly confirmed. Emergency Stop and the normal pause remain distinct. No unattended runtime, automatic continuation, automatic retry, external write, communication, calendar, financial, destructive, script, process, plugin or AI execution is enabled.

## Verification state

Group 30 is complete at 129 tests and commit `285636d2f31fe8bda9e060a25b0300c6d`. Group 31 tests, Release build, repository hygiene, Gitleaks, screenshot evidence and final synchronization are pending the Pack 1/Pack 2 runners.

## Next

Execute Group 31 only. Mobile Companion, full Mobile, website and v7 have not started.

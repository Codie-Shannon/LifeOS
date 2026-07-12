# LifeOS Documentation

## Authoritative current documents

- `current-status.md` — completed Group 28 state and guarded-execution boundary.
- `lifeos-version-state.json` — machine-readable `v6.0.0-alpha.3` state.
- `version-history.md` — consolidated milestone history.
- `release-notes/v6.0-alpha-group-28.md` — Group 28 final release record.
- `automation/v6-guarded-internal-execution.md` — guarded execution architecture.
- `screenshot-groups/group-28-guarded-internal-automation/README.md` — current screenshot evidence.

## Screenshot groups

Groups 01–09 cover the v1–v3.9 offline operating-system foundation.

Groups 10–19 cover the complete v4 spine.

Groups 20–26 cover the v5 integration lane.

Group 27 introduced deterministic dry-run automation proposals, explicit review, duplicate handling, risk/capability policy and approval without execution.

Group 28 is the current completed screenshot boundary. It proves a persisted execution pause, approval separated from execution, immediate eligibility revalidation, final before/after preview, explicit final confirmation, one typed Low-risk reversible internal action, before/after persistence, idempotent execution, explicit Undo, stale-state blocking and continued high-risk/external blocking.

Group 29 has not started.

## Documentation rule

Current-state documents describe only the current product state. Historical release notes and screenshot groups remain unchanged as evidence. Credentials, local token caches, private communication data, generated backups and temporary screenshot plans must not be committed.


## Group 29 — Controlled orchestration and recovery

Schedules now create review intent only. Due occurrences enter a visible queue, runs require explicit Start, and every Low-risk reversible internal step requires its own exact preview and confirmation. Progress pauses between steps; failure recovery is explicit and persisted. No unattended execution or external writes are enabled.

<!-- GROUP29 START -->
## v6 controlled orchestration

Screenshot Group 29 is complete at `v6.0.0-alpha.3`.

Due work is queued for explicit review. Runs require explicit Start, and every typed reversible internal step requires its own exact preview and confirmation. Execution pauses between steps, failures stop for explicit recovery, and High-risk or external actions remain blocked.

Evidence: `docs/screenshot-groups/group-29-controlled-orchestration/README.md`
<!-- GROUP29 END -->

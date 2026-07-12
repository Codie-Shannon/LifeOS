# LifeOS Documentation

## Authoritative current documents

- `current-status.md` — current Group 30 Pack 1 implementation and safety boundary.
- `lifeos-version-state.json` — machine-readable `v6.0.0-alpha.4` state.
- `version-history.md` — consolidated milestone history.
- `release-notes/v6.0-alpha-group-30.md` — Group 30 implementation record.
- `automation/v6-automation-hardening.md` — emergency-stop, health, incident and recovery architecture.
- `manual-tests/group-30/group30-manual-tests.md` — Group 30 manual verification.
- `screenshot-groups/group-29-controlled-orchestration/README.md` — latest completed screenshot evidence until Pack 2.

## Screenshot groups

Groups 01–26 cover the offline OS and read-only integration lanes. Group 27 introduced deterministic proposal automation. Group 28 introduced guarded reversible internal execution. Group 29 introduced controlled orchestration and persisted recovery checkpoints. Group 30 hardens that system with persisted health, incident visibility and a separate fail-closed Emergency Stop.

## Documentation rule

Current-state documents describe only the current product state. Historical release notes and screenshot groups remain unchanged. Manual tests belong under `docs/manual-tests/`. Screenshot evidence belongs under `docs/screenshot-groups/`. Handoff, context-drop and pre-screenshot continuity files must never be committed.

# LifeOS Documentation

This directory is the durable source of truth for LifeOS status, release history, manual validation and screenshot evidence.

## Current authoritative documents

- [`current-status.md`](current-status.md) — current Desktop, Companion and roadmap state.
- [`lifeos-version-state.json`](lifeos-version-state.json) — machine-readable version and boundary state.
- [`release-notes/v7.0-alpha-group-36.md`](release-notes/v7.0-alpha-group-36.md) — current Group 36 release record.
- [`screenshot-groups/group-36-v7-source-expansion-answer-quality/`](screenshot-groups/group-36-v7-source-expansion-answer-quality/) — current screenshot evidence.
- [`manual-tests/group-36/`](manual-tests/group-36/) — current manual verification.
- [`version-history.md`](version-history.md) — consolidated milestone history.
- [`LIFEOS_ROADMAP.md`](LIFEOS_ROADMAP.md) — broader roadmap.

## Current product state

| Product | State |
|---|---|
| LifeOS Desktop | `v7.0.0-alpha.2` |
| Mobile Companion | `v0.1.0-beta.1`, complete and closed |
| Full Mobile | Not started |
| Website | Not started |
| Group 37 | Not started |

The v7 Assistant remains read-only. It has expanded approved local sources, bounded intent-aware retrieval, explainable ranking, cross-source provenance, conflict/stale/missing-data warnings and no memory, tools, execution or external writes.

## Recent screenshot groups

- [`Group 36 — v7 source expansion and answer quality`](screenshot-groups/group-36-v7-source-expansion-answer-quality/)
- [`Group 35 — v7 Assistant foundation`](screenshot-groups/group-35-v7-assistant-foundation/)
- [`Group 34 — Mobile Companion beta checkpoint`](screenshot-groups/group-34-companion-beta-checkpoint/)
- [`Group 31 — v6 controlled automation beta checkpoint`](screenshot-groups/group-31-v6-release-checkpoint/)

## Documentation organization

- `release-notes/` — immutable historical group and version records.
- `screenshot-groups/` — approved screenshot evidence and evidence descriptions.
- `manual-tests/` — manual verification instructions and results.
- `automation/` — v6 controlled automation architecture and safety documentation.
- `integrations/` — connector contracts, readiness and setup documentation.
- `mobile-companion/` — Companion-specific implementation and beta records.

## Documentation rules

- Current-state documents describe the current product state only.
- Historical release notes and screenshot groups remain unchanged.
- Manual tests belong under `docs/manual-tests/`.
- Screenshot evidence belongs under `docs/screenshot-groups/`.
- All public demonstration data must be fictional or explicitly sanitized.
- Handoff, context-drop and pre-screenshot continuity files must never be committed.
- Build-chat PDFs and delivery ZIPs are temporary chat artifacts, not repository files.

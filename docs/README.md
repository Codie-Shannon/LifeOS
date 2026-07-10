# LifeOS Documentation

## Authoritative current documents

- `current-status.md` - current product state, completed Group 22 boundary, connector safety boundary, and next handoff.
- `lifeos-version-state.json` - machine-readable v5.0-alpha product state.
- `version-history.md` - consolidated milestone history.
- `release-notes/v5.0-alpha.md` - current connector-foundation and first-live-connector release notes.
- `screenshot-groups/group-22-google-calendar-read-only/README.md` - current completed Google Calendar live connector evidence.
- `screenshot-groups/group-21-v5-0-alpha-alignment/README.md` - prior v5.0-alpha alignment evidence.
- `screenshot-groups/group-20-v5-manual-import-connector/README.md` - detailed manual connector implementation evidence.
- `integrations/v5-0-build-brief.md` - v5.0 connector build brief.
- `integrations/v5-integration-architecture-pack.md` - connector registry, scope, risk, and roadmap architecture.
- `integrations/v5-integration-contract.md` - connector safety and handoff contract.
- `integrations/v5-connector-readiness-matrix.md` - connector readiness state.
- `integrations/v5-handoff-checklist.md` - connector handoff checks.

## Screenshot groups

Groups 01-09 cover the v1-v3.9 offline operating-system foundation.

Groups 10-19 cover the complete v4 spine.

The v5 connector lane is documented by:

- Group 20 - v5 Manual Import Connector
- Group 21 - v5.0-alpha Alignment
- Group 22 - Google Calendar Read-Only Connector

Group 22 is the current completed screenshot boundary. It proves explicit read-only OAuth connection, manual bounded refresh, review-first Integration Inbox intake, repeated-refresh duplicate suspicion, explicit disconnect, and local token-cache deletion.

Further work begins only after a new Master Roadmap handoff. Group 23 has not started.

## Documentation rule

Current-state documents describe only the current product state. Historical release notes and screenshot groups remain unchanged as evidence. Temporary screenshot plans, generated backup folders, credentials, local token caches, and duplicate current-state marker blocks are not authoritative documentation and must not be committed.

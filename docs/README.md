# LifeOS Documentation

This directory is the durable source of truth for LifeOS status, release history, manual validation and screenshot evidence.

## Current authoritative documents

- [`current-status.md`](current-status.md) - current product and roadmap state.
- [`lifeos-version-state.json`](lifeos-version-state.json) - machine-readable version, group and boundary state.
- [`version-history.md`](version-history.md) - consolidated milestone history.
- [`LIFEOS_ROADMAP.md`](LIFEOS_ROADMAP.md) - approved compressed roadmap through Group 82.
- [`release-notes/lifeos-v13.md`](release-notes/lifeos-v13.md) - current v13 Group 64 release note.
- [`screenshot-groups/group-64-grocery-planning-essentials/`](screenshot-groups/group-64-grocery-planning-essentials/) - latest completed screenshot evidence.

## Current product state

| Product | State |
|---|---|
| LifeOS Desktop | Current deep administration, review, audit and planning surface through v13 Group 64 |
| Full Mobile | Built and extended through v13 Group 64 for capture, review, execution, status and offline-safe mobile flows |
| Mobile Companion | Separate lightweight companion product, beta complete and closed |
| Website | Website beta foundation complete through v8 Groups 40-42 |
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict and safety boundaries |
| Current release | v13 active; Group 64 complete; Group 65 next |

## Recent release lanes

- **v13 / Group 64** - Grocery Planning and recurring essentials across Desktop and Full Mobile.
- **v12 / Groups 61-63** - Career Studio opportunity, application, materials, preparation, follow-ups and analytics.
- **v11 / Groups 58-60** - Money foundation, document/evidence intake, financial review and reporting.
- **v10 / Groups 52-57** - Full Mobile release family.
- **v9 / Groups 46-51** - Microsoft and Google read-only integration foundations.
- **v8 / Groups 40-45** - Website beta foundation and Desktop shell/workspace integration.
- **v7 / Groups 35-39** - Assistant beta release family.

## Current evidence

Recent official screenshot groups:

- [`Group 64 - Grocery planning and recurring essentials`](screenshot-groups/group-64-grocery-planning-essentials/)
- [`Group 63 - Career follow-ups, analytics and closure`](screenshot-groups/group-63-career-followups-analytics-closure/)
- [`Group 62 - Career materials and interview preparation`](screenshot-groups/group-62-career-materials-interview-prep/)
- [`Group 61 - Career opportunity and application pipeline`](screenshot-groups/group-61-career-opportunity-application/)
- [`Group 60 - Financial review and reporting`](screenshot-groups/group-60-financial-review-reporting/)
- [`Group 59 - Document and evidence intake`](screenshot-groups/group-59-document-evidence-intake/)
- [`Group 58 - Money foundation`](screenshot-groups/group-58-money-foundation/)
- [`Group 57 - Full Mobile beta closure`](screenshot-groups/group-57-full-mobile-beta-closure/)

## Documentation organization

- `release-notes/` - historical group and version records.
- `screenshot-groups/` - approved screenshot evidence and evidence descriptions.
- `manual-tests/` - manual verification instructions and results.
- `automation/` - controlled automation architecture and safety documentation.
- `integrations/` - connector contracts, readiness and setup documentation.
- `mobile-companion/` - Companion-specific implementation and beta records.
- `website/` - Website design, deployment and boundary documentation.

## Documentation rules

- Current-state documents describe the current product state only.
- Historical release notes and screenshot groups remain unchanged unless correcting their own local metadata.
- Manual tests belong under `docs/manual-tests/`.
- Screenshot evidence belongs under `docs/screenshot-groups/`.
- Public demonstration data must be fictional, sanitized or explicitly approved.
- Private context PDFs, handoff notes, pre-screenshot continuity files and delivery ZIPs must not be committed.
- Provider secrets, tokens, private payloads and personal data must never appear in proof.

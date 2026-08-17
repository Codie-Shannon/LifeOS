# LifeOS current status

LifeOS is current through **v28.0.0-alpha.4 / Group 136 / SG-136 Pack 1**.
SG-124 is complete with screenshot evidence. SG-128 adds templates, A4 review,
ATS/readability checks, version history and safe PDF/DOCX derivatives; its
visible Pack 2 capture remains open. SG-132 closes the configuration-free
cover-letter and application-pack vertical with inspected Desktop and Full
Mobile evidence.

Groups 133-136 begin the approved platform spine. Agenda, Follow-ups, Work
Pipeline and Work Sessions now share versioned atomic persistence, inspected
health, validated backup recovery and a recoverable 30-day Trash. Desktop
Settings exposes the registered stores without claiming that every legacy
module has migrated.

The 2026-08-18 recovery pass corrected the execution baseline without claiming
a new group: ordinary and portfolio-demo data are now explicitly separated,
Career document history is durable across restarts, PDF export paginates instead
of truncating long content, and both Android applications were verified on an
API 36 emulator.

SG identifiers now follow the ending implementation checkpoint; see the
[`screenshot-group identifier lineage`](screenshot-groups/README.md).

Groups 121-132 establish the approved Career Documents Studio after the
preserved v27 release candidate.

## Current product state

| Product | Status |
|---|---|
| Desktop | Current through Group 136 with local-data health/recovery plus CVs, cover letters, application packs, review and export |
| Full Mobile | Current through Group 132 with ordinary empty state, opportunity capture, document review and derivative sharing |
| Mobile Companion | Separate lightweight companion product; beta complete and closed |
| Website | Product, documentation, onboarding, portfolio and release-candidate evidence surface |
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict and safety boundaries |

## Latest implemented capability

Groups 133-136 establish Local Data & Recovery Pack 1:

- shared, versioned JSON envelopes with atomic write-through replacement
- honest missing-file state without demo seeding or implicit writes
- preserved legacy and older-schema migration sources
- fail-closed handling for newer schemas and mismatched store identifiers
- corrupt-primary preservation with validated backup recovery
- recoverable 30-day Trash that refuses to overwrite current data
- Desktop health and recovery surface for the first four migrated stores
- no permanent-delete control or automatic purge in this checkpoint

SG-136 Pack 2 rendered-product capture remains open.

## Prior completed capability

Groups 129-132 complete Cover Letters and Application Integration in Pack 1:

- durable local opportunities, trusted facts, cover letters, applications and packs
- opportunity- and CV-linked draft creation
- source-backed suggestions with explicit accept, reject and user-edit states
- contact-detail inclusion confirmed per document before export
- versioned PDF and DOCX cover-letter derivatives
- stale-pack invalidation when linked source versions change
- ordinary-mode Desktop and Full Mobile paths without fictional records
- no autonomous application submission or employer communication

The exact eight-image Desktop and Full Mobile SG-132 Pack 2 evidence set is
captured, inspected and committed.

Groups 125-128 complete CV Templates, Preview and Export:

- four professional, ATS-safe templates
- bounded typography, density, margin and accent controls
- A4 page estimation with ATS/readability checks
- preserved, restorable document version history
- durable, atomic local Career document storage with recoverable backups
- real versioned, multipage PDF and DOCX derivative generation
- fail-closed export when source or readability checks block

This builds on completed Work Time, Guarded Provider Contracts, Closed Beta, Native Intelligence, Scheduled Communications and Social Review lanes.

## Current boundaries

- Provider reads and imported candidates remain bounded and review-first.
- Guarded provider contracts exist, but external provider writes remain disabled.
- No bank feeds, payment initiation, accounting-provider writes or autonomous reconciliation.
- No autonomous career applications, recruiter messaging or fabricated career claims.
- No automatic grocery purchase, silent substitution or external-cart mutation.
- Optional AI cannot silently change authoritative state.
- Scheduled communications are proposals until explicitly reviewed; automatic sending is disabled.
- Original documents and evidence are preserved; exports and previews are derivatives.
- Ordinary mode does not seed or display portfolio-proof fixtures; Portfolio Demo must be selected explicitly.

## Validation baseline

- Groups 133-136 targeted tests: 8 passed
- Core tests: 486 passed
- Companion tests: 34 passed
- Website tests: 28 passed
- Mobile tests: 56 passed
- Desktop Release build: clean (0 warnings, 0 errors)
- Full Mobile and Mobile Companion Android x64 Release builds: clean (0 warnings, 0 errors)
- SG-124 targeted tests: 14 passed
- Groups 129-132 targeted tests: 11 passed
- Prior Android runtime baseline: fresh installs launched on API 36 at SG-132
- Website runtime smoke test: HTTP 200
- Latest evidence: [`group-129-132-cover-letters-application-packs`](screenshot-groups/group-129-132-cover-letters-application-packs/)

SG-132 Pack 2 is closed with visible rendered-product evidence. SG-128 Pack 2
capture remains open and must be closed without inferring UI proof from builds
or automated tests.

## Next completion work

Close SG-136 Pack 2, then continue migrating remaining stores and the approved
ordinary-mode productization backlog. SG-128’s historical visible Pack 2 capture
also remains open. Credentialed integrations, signing, public deployment, store
submission and owner acceptance remain final gates.

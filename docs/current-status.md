# LifeOS current status

LifeOS is current through **v28.0.0-alpha.2 / Group 128 / SG-128**. SG-124 is complete
with screenshot evidence, and SG-128 Pack 1 adds templates, A4 review,
ATS/readability checks, version history and safe PDF/DOCX derivatives.

The 2026-08-18 recovery pass corrected the execution baseline without claiming
a new group: ordinary and portfolio-demo data are now explicitly separated,
Career document history is durable across restarts, PDF export paginates instead
of truncating long content, and both Android applications were verified on an
API 36 emulator.

SG identifiers now follow the ending implementation checkpoint; see the
[`screenshot-group identifier lineage`](screenshot-groups/README.md).

Groups 121-128 establish the approved Career Documents Studio after the
preserved v27 release candidate. Groups 129-132 remain approved for cover
letters and application integration.

## Current product state

| Product | Status |
|---|---|
| Desktop | Current through Group 128 with guided CV creation, templates, review and export |
| Full Mobile | API 36 release build verified; ordinary mode starts empty and portfolio proof requires explicit opt-in |
| Mobile Companion | Separate lightweight companion product; beta complete and closed |
| Website | Product, documentation, onboarding, portfolio and release-candidate evidence surface |
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict and safety boundaries |

## Latest completed capability

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

- SG-124 targeted tests: 14 passed
- Core tests: 465 passed
- Companion tests: 34 passed
- Website tests: 28 passed
- Mobile tests: 56 passed
- Desktop, website and TimerAgent Release builds: clean (0 warnings, 0 errors)
- Full Mobile Android x64 Release build: clean; fresh install launched on API 36
- Mobile Companion Android x64 Release build: clean; fresh install launched on API 36
- Website runtime smoke test: HTTP 200
- Latest evidence: [`group-121-124-cv-builder-foundation`](screenshot-groups/group-121-124-cv-builder-foundation/)

SG-128 Pack 2 screenshot capture remains open; it should be closed with visible
Desktop UI evidence rather than inferred from builds or automated tests.

## Next approved work

Groups 129-132 add cover-letter creation and application-pack integration.

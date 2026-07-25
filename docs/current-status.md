# LifeOS current status

LifeOS is current through **v22.0.0-alpha.1 / Group 98** at repository checkpoint `7a323ac`.

Groups 95-98 close Pay-Later and Money Integrations. Group 99 is next, and the approved roadmap runs through Group 120.

## Current product state

| Product | Status |
|---|---|
| Desktop | Deep administration, review, planning, audit and reporting through Group 98 |
| Full Mobile | Purpose-built Android capture, review, execution and offline-safe workflows aligned through Group 98 |
| Mobile Companion | Separate lightweight companion product; beta complete and closed |
| Website | Public product/documentation/evidence beta foundation complete; packaging and onboarding planned |
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict and safety boundaries |

## Latest completed capability

Groups 95-98 complete Pay-Later and Money Integrations:

- source-backed Afterpay and Zip parsing candidates
- duplicate statements retained for explicit review
- confirmed deductions excluded from safe-money calculations
- read-only CSV/Xero export contract
- no payment initiation or autonomous reconciliation

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

## Validation baseline

- Core tests: 386 passed
- Mobile tests: 54 passed
- Desktop Release build: clean
- Android Release build: clean with signed private-beta artifacts
- Latest evidence: [`group-95-98-pay-later-money-integrations`](screenshot-groups/group-95-98-pay-later-money-integrations/)

## Next approved work

Group 99 begins v23 New Zealand Grocery Lookup Worker. The remaining approved lanes run through the Group 120 product-complete release candidate.

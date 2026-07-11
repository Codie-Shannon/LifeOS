# LifeOS Version History

## v5.0.0-alpha.5 — Authenticated Read-Only Gmail Connector

- Added one private/testing-mode authenticated Gmail connector using exact `gmail.readonly`.
- Added explicit browser authorization and connected/disconnected/cache-cleared lifecycle states.
- Added profile-required manual searches bounded to 31 days.
- Added default cap 25 and hard maximum cap 100.
- Added visible generated Gmail query and reviewable noise exclusions.
- Added bounded list/get retrieval, safe inert snippet normalization, message/thread references, provenance, and audit.
- Routed Gmail results through the existing provider-neutral Email Radar evidence and candidate-review pipeline.
- Verified a safe real Gmail test message, confirmed timeline entry, repeated-search duplicate detection, disconnect, and retained evidence.
- Kept sending, drafts, mailbox mutation, attachments, active HTML, remote content, background scanning, scheduled searches, automatic Follow-Ups, automatic Work Pipeline mutation, Outlook, IMAP/POP3, and AI interpretation inactive.
- Verified 91 passing tests and a successful Release build.

## v5.0.0-alpha.4 — Email Radar Foundation

- Centralized formal version metadata and aligned visible/current-state surfaces.
- Added user-guided Email Radar profiles.
- Added safe local JSON/CSV communication evidence preview and confirmation.
- Added deterministic candidate matching with visible reasons.
- Added explicit confirm/reject review and duplicate suspicion.
- Added confirmed-only communication timelines and review-first waiting-on suggestions.
- Kept Gmail, Outlook, mailbox scanning, sending, background work, AI interpretation, automatic Follow-Ups, and automatic Work Pipeline mutation inactive.
- Verified 80 passing tests and a successful Release build.

## v5.0-alpha - Connector Foundation and First Live Read-Only Connector

- Activated local CSV, JSON, and ICS imports into read-only Integration Inbox previews.
- Added explicit import confirmation, duplicate protection, source provenance, and preserved audit history.
- Added one authenticated Google Calendar connector using `calendar.readonly`.
- Kept refresh explicit, manual, user-confirmed, and capped at 31 days.
- Routed provider events through the existing review-first Integration Inbox intake boundary.
- Verified real event retrieval, untrusted preview creation, repeated-refresh duplicate suspicion, and explicit disconnect with local token-cache deletion.
- Kept calendar writes, inbox scanning, background polling, scheduled refresh, automatic acceptance/linking, and automatic LifeOS module mutation inactive.

## v4.9 - Integration Inbox + v5 Readiness

- Added review-first integration previews.
- Added provenance, duplicate suspicion, explicit review states, and manual handoff contracts.
- Added source lanes and v5 connector-readiness matrix.
- Completed the v4 operating spine.

## v4.8 - Command Centre Pressure Engine

- Ranked reviewed module signals into act-now, review-first, waiting/do-not-chase, and protected lanes.
- Added pressure scoring, suppression rules, and next-safest-action output.

## v4.7 - Weekly Close-Out

- Added deliberate close-now, roll-forward, waiting, money, proof, receipt, and work review lanes.

## v4.6 - Receipt OCR / Evidence-to-Item

- Added source-gated receipt evidence candidates and trusted manual acceptance.

## v4.5 - Work Pipeline

- Added active, blocked, waiting, invoice-ready, payment-expected, proof-gap, and opportunity states.

## v4.4 - Agenda + Payment Calendar

- Unified time commitments, due dates, payment dates, expected-income dates, and review windows.

## v4.3 - Money Profile / Hidden Deductions / Safe-to-Spend

- Added hidden reserves, buffers, expected-money exclusion, and safe-to-spend confidence.

## v4.2 - Bills / Upcoming Payments / Pay Later

- Added stateful obligations, BNPL load, hidden deductions, and payment evidence gates.

## v4.1 - Item Type / State Engine

- Added the shared item/state, trust, evidence, transition, and pressure model.

## v4.0 - Spine Recovery Map

- Restored the complete LifeOS operating spine and sequenced v4 before real integrations.

## v3.9 - Final Offline OS

- Completed the integration-ready offline operating-system checkpoint.

## v3.5 - Search / Knowledge

- Added local search profiles and reviewable knowledge items.

## v3.0 - OS Navigation / Core Modules

- Added broader operating-system navigation and core module structure.

## v2.1 - Universal Spine

- Added the shared cross-module operating spine.

## v2.0 - Paid Desktop Release

- Completed the paid-work, money, proof, safety, and desktop release foundation.

## v1.x - Foundation

- Established local-first storage, daily flow, evidence, relationships, paid work, settings, safety, and theme foundations.

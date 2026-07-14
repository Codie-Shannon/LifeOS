# LifeOS Version History

## v5.0.0-alpha.5 â€” Authenticated Read-Only Gmail Connector

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

## v5.0.0-alpha.4 â€” Email Radar Foundation

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
## v5.0.0-beta.1 — Group 26

v5 integration release checkpoint.

- unified five-lane integration readiness
- manual, read-only and review-first connector model
- Google Calendar and Gmail read-only lifecycle alignment
- retained evidence after disconnect/cache clear
- release-validation matrix
- privacy-safe screenshot proof
- 97 tests passed
- Release build passed

## v6.0.0-alpha.1 — Group 27

Controlled automation foundation.

- Added provider-neutral automation rules disabled by default.
- Added deterministic local triggers and conditions.
- Required reviewed or trusted LifeOS state for current rule evaluation.
- Added manual dry-run evaluation with visible expected/actual pass-fail explanations.
- Added explicit proposed action, target, risk, approval and capability policy.
- Added proposal review with explicit approve/reject decisions.
- Kept approval separate from execution; approved proposals remain `executed: No`.
- Added stable duplicate-proposal detection and prior-proposal linkage.
- Blocked high-risk communication, external-write, financial, destructive, calendar and script capabilities.
- Added retained inert audit and local JSON persistence with backup recovery.
- Added seven privacy-safe fictional screenshot proofs.
- Verified 112 passing tests, a successful Release build and `git diff --check`.
- Kept background workers, scheduling, timers, startup execution, automatic retry, operational mutation, scripts, plugins and AI inactive.


## v6.0.0-alpha.3 — Group 28

Controlled orchestration and recovery.

- Added a persisted global execution gate that starts paused.
- Kept dry-run evaluation and approval available while final execution is paused.
- Preserved the approval boundary: approval records intent and remains `ApprovedNotExecuted`.
- Added a separate final execution preview and explicit final confirmation.
- Added immediate eligibility revalidation before execution.
- Added one typed allowlisted Low-risk reversible internal review-note handler.
- Retained exact before and after snapshots, execution identity, timestamps, source references and audit.
- Added idempotent duplicate-execution prevention.
- Added explicit Undo with exact prior-state restoration and retained history.
- Added stale-source detection that blocks execution and requires reevaluation.
- Kept Medium Follow-Up draft proposals proposal-only.
- Kept High-risk communication and external-write actions blocked by policy.
- Added seven privacy-safe fictional screenshot proofs.
- Verified 116 passing tests, a successful Release build, `git diff --check`, guarded-execution validation, synchronized push and clean working tree.
- Kept background workers, scheduling, timers, startup execution, automatic retries, external mutation, scripts, processes, plugins and AI inactive.


## Group 29 — Controlled orchestration and recovery

Schedules now create review intent only. Due occurrences enter a visible queue, runs require explicit Start, and every Low-risk reversible internal step requires its own exact preview and confirmation. Progress pauses between steps; failure recovery is explicit and persisted. No unattended execution or external writes are enabled.

<!-- GROUP29 START -->
## v6.0.0-alpha.3 — Screenshot Group 29

- controlled orchestration plans and ordered internal steps
- deterministic Manual-only, one-time and weekly review scheduling
- singular due occurrences
- explicit Start and per-step confirmation
- checkpoints and restart-safe paused recovery
- explicit retry, cancellation and run-scoped rollback
- migration from Group 28 local automation state
- eight approved fictional-data screenshots
- no unattended execution or external writes
<!-- GROUP29 END -->

## v6.0.0-alpha.4 — Screenshot Group 30
- Automation hardening, persisted health and sanitized incident visibility.
- Distinct fail-closed Emergency Stop with explicit reset and no automatic resume.
- Exact-scope failure containment, explicit retry and reverse-order rollback review.
- Manual, foreground-only controlled automation remains mandatory.


## v6.0.0-beta.1 — Group 31

Controlled automation release checkpoint: persisted readiness, v6 store migration and malformed-state recovery hardening, restart safety validation and beta release evidence. No automation-surface expansion.

## v7.0.0-beta.1 — Group 39

v7 Assistant beta release checkpoint.

- Unified source-backed answers, explainable ranking, review-only planning, controlled review transfer and explicit scoped memory into one coherent beta flow.
- Re-proved no mutation, execution, autonomous tools, background work or external writes.
- Advanced Desktop release identity to `v7.0.0-beta.1`; Companion remains separate at `v0.1.0-beta.1`.
- Added full regression, security, hygiene and exact eight-screenshot release evidence gates.
- Kept Website, full Mobile, v8 and Group 40 unstarted.

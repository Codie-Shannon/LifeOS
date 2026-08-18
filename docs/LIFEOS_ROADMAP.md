# LifeOS Roadmap

This is the current approved compressed roadmap. Historical plans remain in Git history and release evidence; this file describes the current checkpoint and approved path through product completion.

## Current release

**LifeOS v28.0.0-alpha.15 is active through Group 180 with real ordinary-mode
Focus Timer, Agenda, Weekly Review, Life, Household, Documents, Money Pressure, Work Time and Projects workspaces on the shared spine.**

The v27/Group 120 release candidate remains preserved as a historical
checkpoint. Career Documents Studio Pack 1 is implemented through Group 132.

## Product architecture

| Product | Role |
|---|---|
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict boundaries and safety rules |
| Desktop | Deep administration, review, audit, detail, filtering, reporting and planning |
| Full Mobile | Rapid capture, review, execution, status and queued offline actions |
| Mobile Companion | Separate lightweight companion product |
| Website | Public product, documentation, evidence, onboarding, distribution and support surface |

## Permanent product rules

- Review first; explicit confirmation precedes trusted-state mutation.
- No silent trust promotion or conflict resolution.
- No autonomous Assistant execution or silent AI mutation.
- No secrets, tokens, provider payloads or personal data in proof.
- No destructive handling of original evidence.
- No automatic reconciliation, financial posting or payment initiation.
- No autonomous applications, recruiter messaging or fabricated claims.
- No automatic grocery ordering or silent substitution.
- Exports are derivatives; authoritative source records remain unchanged.
- Provenance, freshness, confidence and audit remain visible where relevant.

## Completed release history

| Release lane | Groups | Outcome |
|---|---:|---|
| Early foundations | 1-23 | Core workspaces, records, evidence patterns and application architecture |
| v5-v10 | 24-57 | Integrations, automation, Companion, Assistant, Website/Desktop shell, provider foundations and Full Mobile |
| v11 | 58-60 | Money, Documents and Financial Review |
| v12 | 61-63 | Career Studio |
| v13 | 64-66 | Household and Grocery |
| v14 | 67-72 | Work Time, Billable Records and Operating Day Work Proof |
| v15 | 73-76 | Guarded Provider Contracts |
| v16 | 77-79 | Documentation and Packaging Hub |
| v18 | 80-82 | Closed Beta Baseline |
| v19 | 83-86 | Native Intelligence and Optional AI |
| v20 | 87-90 | Scheduled Communications |
| v21 | 91-94 | Social and Messaging Integrations |
| v22 | 95-98 | Pay-Later and Money Integrations |
| v23 | 99-103 | New Zealand Grocery Lookup Worker |
| v24 | 104-107 | Evidence Automation and Proof Tooling |
| v25 | 108-111 | Privacy, Export, Backup and Controls |
| v26 | 112-116 | Website Packaging and Onboarding |
| v27 | 117-120 | Product-Complete Release Candidate |
| v28 | 121-180 | Career Documents, Platform Spine, Projects, Work Time, Money Pressure, Documents, Household, Life, Weekly Review, Agenda and Focus Timer |

Version numbers follow shipped product checkpoints; v17 was not used as a completed release lane.

## Current release gate

- Cross-product regression, accessibility and security validation is complete.
- Desktop, Android and Website Release builds are validated.
- Final screenshot and validation evidence is recorded in SG-120.
- A release tag, public launch, store submission and production-provider
  activation require explicit owner approval.

## Beyond Group 120

Screenshot-group identifiers follow the ending implementation checkpoint for
compressed lanes. For example, Groups 117-120 close as SG-120, Groups 121-124
close as SG-124, and Groups 125-128 close as SG-128. They are not a separate
sequential counter.

### v28 / Groups 121-124 - CV Builder Foundation

- Guided CV creation from trusted Career Profile facts.
- Editable, reorderable and optional sections with autosave.
- Multiple target-role variants and live preview.
- Unsupported source claims block later export.

### v28 / Groups 125-128 - Templates, Preview and Export

- Professional template gallery and bounded layout controls are complete.
- A4 pagination, ATS/readability checks and PDF/DOCX export are complete.
- Restorable document version history and safe derivative generation are complete.

### v28 / Groups 129-132 - Cover Letters and Application Integration

- Guided opportunity-linked cover-letter creation is implemented.
- CV, opportunity and versioned application-pack linking is implemented.
- Evidence-backed suggestions require explicit acceptance, rejection or edit.
- Desktop and Full Mobile configuration-free review paths are implemented.
- SG-132 Pack 2 is closed with the exact eight-image Desktop and Full Mobile
  evidence set.

### v28 / Groups 133-136 - Local Data and Recovery Spine

- Versioned, atomic storage is implemented for Agenda, Follow-ups, Work
  Pipeline and Work Sessions.
- Legacy and corrupt source files are preserved before replacement or recovery.
- Newer schemas fail closed without silent overwrite.
- Reset moves current data and its backup to recoverable 30-day Trash.
- Desktop Settings exposes store health and guarded restore controls.
- SG-136 Pack 2 is closed with the exact eight-image Desktop and validation
  evidence set.
- Remaining stores, encrypted whole-product backup and explicit purge policy
  remain later platform-spine work.

### v28 / Groups 137-140 - Navigation and Shell Search

- Static workspaces, allowlisted modules and safe display commands are indexed.
- Deterministic title, command, description and keyword ranking is shared in Core.
- Keyboard selection, Enter and double-click execution are implemented.
- Result type, match reason and description remain visible before execution.
- Unknown queries run nothing and show an explicit no-result state.
- Personal records, provider content and external search are not indexed.
- SG-140 Pack 2 is closed with the exact eight-image Desktop and validation
  evidence set.

### v28 / Groups 141-144 - Forms and Actionable Problems

- Shared required, maximum-length and single-line validation is implemented.
- Settings rejects invalid input before changing memory or disk.
- Field issues and recovery guidance remain visible and keyboard-readable.
- Local access, storage, unreadable-data and unexpected failures map to safe,
  stable user-facing problem identifiers.
- Exception messages and private paths are not exposed in UI feedback.
- Preference reset persists before replacing the current in-memory state.
- SG-144 Pack 2 is closed with the exact eight-image Desktop and validation
  evidence set.

### v28 / Groups 145-148 - Projects and Delivery

- The planned Projects placeholder is replaced by a genuine embedded workspace.
- Ordinary mode starts empty and never seeds portfolio proof records.
- Project capture validates name, next action, dates and bounded text fields.
- Delivery state, due date and optional proof reference remain explicit.
- Projects use the shared versioned, atomic, backup-aware local-store contract.
- Archive and restore are reversible; permanent deletion is not exposed.
- SG-148 Pack 2 is closed with the exact eight-image Desktop and validation
  evidence set.

### v28 / Groups 149-152 - Work Time and Billable Records

- The retained Work Time product-lane page is replaced by a genuine embedded workspace.
- Ordinary mode starts empty and never seeds fictional client, hour or revenue records.
- Work capture validates client/project, date, hours, rate and bounded text fields.
- Billable classification and completed, invoiced, paid and cancelled state remain explicit.
- Totals distinguish tracked value, paid value and unpaid billable value.
- Work sessions use the shared versioned, atomic, backup-aware local-store contract.
- Invoice sending, payment initiation, bank verification and client messaging remain unavailable.
- SG-152 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 153-156 - Money Pressure and Safe to Spend

- Ordinary mode starts from zero and never invents balances, income, bills or buffers.
- Manual amounts are validated before any in-memory or disk mutation.
- Pending income remains visible but is excluded from safe-to-spend.
- The existing money-pressure store is migrated to the shared versioned recovery contract.
- Bank feeds, payment initiation, accounting writes and automatic reconciliation remain unavailable.
- SG-156 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 157-160 - Documents and Evidence Intake

- The ordinary proof boundary is replaced by a genuine embedded file-picker workflow.
- User-selected originals are size-bounded, byte-preserved and SHA-256 verified.
- Classification and review state require explicit local decisions.
- Exact-hash duplicates remain candidates; automatic merge and deletion are unavailable.
- Document intake uses the shared versioned, atomic, recoverable local-store contract.
- SG-160 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 161-164 - Household and Grocery Planning

- The ordinary proof boundary is replaced by a genuine embedded local workspace.
- Grocery needs have bounded names, quantities, units, priorities, categories and notes.
- Recurring essentials require explicit cadence and due-date review.
- List and item lifecycle transitions remain user-controlled and reversible where safe.
- Duplicate names remain review candidates without automatic merge.
- Household grocery data uses the shared versioned, atomic, recoverable local-store contract.
- Ordering, payments, price trust, substitution and external-cart mutation remain unavailable.
- SG-164 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 165-168 - Life Routines and Personal Administration

- Daily State and Daily Operating Flow share a genuine ordinary local workspace.
- Date, title, area, next action, time window, kind, pressure and notes are validated.
- Planned, active, waiting, deferred, done and archived transitions remain explicit.
- Pinned attention is local context and does not create reminders or escalation.
- Life routines use the shared versioned, atomic, recoverable local-store contract.
- Calendar events, provider tasks, messages, payments and background actions remain unavailable.
- SG-168 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 169-172 - Weekly Review and Personal Planning

- Weekly Close-Out is replaced by a genuine ordinary local workspace.
- Week, accomplishments, movement, blockers, pressure, focus and notes are validated.
- Draft, ready, closed and archived transitions remain explicit.
- Ordinary mode starts empty and portfolio proof remains isolated.
- Weekly review uses the shared versioned, atomic, recoverable local-store contract.
- Automatic task roll-forward, calendar events, messages, assignments and provider writes remain unavailable.
- SG-172 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 173-176 - Agenda and Commitment Planning

- Agenda is replaced by a genuine ordinary local workspace.
- Title, optional due date and time, type, pressure, next action and notes are validated.
- Planned, in-progress, waiting, parked, completed and cancelled transitions remain explicit.
- Fixed commitments remain local context without automatic reminders or escalation.
- Agenda continues on the shared versioned, atomic, recoverable local-store contract.
- Calendar events, reminders, provider tasks, messages, payments and background actions remain unavailable.
- SG-176 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

### v28 / Groups 177-180 - Focus Timer and Session Control

- The TimerAgent placeholder route is replaced by a genuine embedded local workspace.
- Title, area, kind, target minutes, next action and notes are validated.
- Planned, running, paused, completed, cancelled and archived transitions remain explicit.
- Elapsed time excludes paused periods and running segments persist honestly across restart.
- Focus timers use the shared versioned, atomic, recoverable local-store contract.
- Automatic start, notification control, invoices, calendar events, messages and provider actions remain unavailable.
- SG-180 Pack 2 is closed with the exact eight-image Desktop and validation evidence set.

Public rollout, broader Desktop redesign, telemetry expansion, extension SDKs
and optional cloud services require separate approval.

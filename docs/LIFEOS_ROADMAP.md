# LifeOS Roadmap

This is the current approved compressed roadmap. Historical roadmap files and release notes remain in Git history and older release records, but this file now reflects the current v13+ plan.

## Current release

**LifeOS v13 is active. Group 64 is complete and Group 65 is next.**

Group 64 completed Grocery Planning and recurring essentials across Desktop and Full Mobile. The roadmap now continues through Group 82.

## Product architecture

| Product | Role |
|---|---|
| Shared Core | Authoritative contracts, deterministic validation, read models, provenance, audit, conflict boundaries and safety rules |
| Desktop | Deep administration, review, audit, detail, filtering, reporting and planning |
| Full Mobile | Rapid capture, review, execution, status and queued offline actions |
| Mobile Companion | Separate lightweight companion product |
| Website / Portal | Public product, documentation, evidence, onboarding, distribution and support surface |

## Permanent product rules

- Review first; explicit user confirmation before trusted-state mutation.
- No silent trust promotion.
- No silent conflict resolution.
- No autonomous Assistant execution.
- No secrets, tokens or provider payloads in proof.
- No destructive evidence or original-media handling.
- No automatic reconciliation, financial posting or payment initiation.
- No autonomous applications, recruiter messaging or fabricated career claims.
- No automatic grocery ordering or silent substitution.
- Exports are derivatives; authoritative source records remain unchanged.
- Provenance, freshness, confidence and audit history remain visible where relevant.

## Completed release history

| Release lane | Groups | Outcome |
|---|---:|---|
| Early foundations | 1-23 | Core LifeOS foundations, workspaces, records, evidence patterns and application architecture |
| v5 | Through 26 | Integration foundation, read-only connectors and release checkpoint |
| v6 | 27-31 | Controlled automation, orchestration, recovery and beta checkpoint |
| Mobile Companion | 32-34 | Lightweight Android Companion beta |
| v7 | 35-39 | Assistant source-backed review, planning, transfer, memory and beta checkpoint |
| v8 | 40-45 | Website beta foundation and Desktop shell/workspace integration |
| v9 | 46-51 | Microsoft and Google read-only integration foundations |
| v10 | 52-57 | Full Mobile release family |
| v11 | 58-60 | Money, documents, review/reporting and release closure |
| v12 | 61-63 | Career Studio complete release family |
| v13 | 64 complete | Grocery planning and recurring essentials; v13 continues at Group 65 |

## Approved compressed roadmap

### v13 / Group 65 - Household inventory, meals, stores and price context

- Household inventory and stock levels.
- Meal planning and recipe/ingredient relationships.
- Store profiles and price observations.
- Desktop deep planning plus Full Mobile capture/use.
- No nutrition diagnosis or automatic purchasing.

### v13 / Group 66 - Shared household workflows, receipts, spending review and v13 closure

- Shared assignments and household review.
- Receipt linkage into Documents and Money.
- Planned vs actual household spending.
- Privacy, accessibility and regression.
- v13 release closure.

### v14 / Group 67 - Time tracking, work sessions and billable records

- Authoritative work-session model.
- Timers and manual entries.
- Project/client/task linkage.
- Desktop administration plus Full Mobile quick tracking.
- Offline/conflict boundaries.

### v14 / Group 68 - Invoice preparation, income, payment status and v14 closure

- Timesheet summaries.
- Invoice preparation candidates.
- Work income and expected/received status.
- Money linkage.
- No autonomous invoicing or payment chasing.
- v14 closure.

### v15 / Group 69 - AI provider control and model routing

- Provider registry and capability matrix.
- Model selection and routing.
- Cost, privacy and context limits.
- Prompt/result provenance.
- No silent provider switching.

### v15 / Group 70 - Teams advanced read models and workflow suggestions

- Teams conversations, meetings and files read models.
- Review-first workflow suggestions.
- No sending or meeting mutation.
- Cross-link to Work, Career and Projects.

### v15 / Group 71 - Power Automate and Power BI read-only foundations

- Flow/run visibility.
- Dataset/report visibility.
- Review and health status.
- No flow execution or report mutation.

### v15 / Group 72 - Cross-provider AI review, safety and v15 closure

- Provider comparison.
- Human review.
- AI audit/provenance.
- Fallback and failure behavior.
- v15 closure.

### v16 / Group 73 - Guarded email and communication writes

- Draft-first email writes.
- Explicit recipient/content confirmation.
- No silent send.
- Audit and rollback boundaries.

### v16 / Group 74 - Guarded calendar and task writes

- Create/update proposals.
- Conflict checks.
- Explicit final confirmation.
- Audit and recovery.

### v16 / Group 75 - Guarded files, contacts and Teams writes

- Scoped file/contact/team actions.
- Preview and permission checks.
- No broad or destructive mutation.

### v16 / Group 76 - Write audit, rollback boundaries, Emergency Stop and v16 closure

- Global write ledger.
- Cancellation/recovery.
- Emergency Stop.
- Capability revocation.
- v16 closure.

### v17 / Group 77 - Repository, issue and pull-request visibility

- GitHub/repository read models.
- Issues, PRs, reviews and status.
- Project/work linkage.
- No autonomous merge.

### v17 / Group 78 - Build, CI/CD and deployment monitoring

- Pipeline/run visibility.
- Test/deployment status.
- Failure triage.
- Release evidence.

### v17 / Group 79 - Engineering evidence, guarded automation and v17 closure

- Engineering proof packs.
- Review-first automation proposals.
- No uncontrolled deployment.
- v17 closure.

### v18 / Group 80 - Windows installer, mobile packaging and update channels

- Desktop installer.
- Android packaging.
- Version/update channels.
- Signing/release boundaries.

### v18 / Group 81 - Portal, onboarding, docs and distribution

- Public Portal.
- Onboarding.
- Documentation.
- Distribution and support flows.
- Public brand/name migration readiness.

### v18 / Group 82 - Security, privacy, release validation and public launch

- Threat/privacy review.
- Final accessibility and regression.
- Release validation.
- Public launch.
- Evidence and known limitations.

## Optional post-82 horizon

These are planning candidates only and are not approved roadmap work:

- Public rebrand and historical codename migration.
- Post-launch stabilization, crash diagnostics, opt-in telemetry and support bundles.
- Extension/plugin SDK with sandboxing, capability permissions and developer docs.
- Optional cloud sync or service tier with user-controlled encryption, conflict policy and account recovery.

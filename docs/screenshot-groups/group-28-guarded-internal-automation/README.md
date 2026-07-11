# Group 28 — Guarded internal automation

**Version:** `v6.0.0-alpha.2`  
**Status:** Complete  
**Tests:** 116 passed, 0 failed, 0 skipped  
**Release build:** Passed  
**`git diff --check`:** Passed  
**Guarded-execution validation:** Passed

Group 28 proves the first deliberately narrow execution boundary in LifeOS. Approval never executes. A separate final preview and explicit confirmation are required. Eligibility is recalculated immediately before execution. One typed Low-risk reversible internal review-note action executes against fictional proof state, retains exact before/after evidence, and can be explicitly undone. Stale and High-risk proposals fail closed.

## Screenshot evidence

1. [Guarded automation overview](screenshots/01-guarded-automation-overview.png)
2. [Approved proposal not executed](screenshots/02-approved-not-executed.png)
3. [Final execution preview](screenshots/03-final-execution-preview.png)
4. [Safe internal action executed](screenshots/04-safe-internal-action-executed.png)
5. [Undo restored prior state](screenshots/05-undo-restored-prior-state.png)
6. [Stale proposal blocked](screenshots/06-stale-proposal-blocked.png)
7. [High-risk and safety controls](screenshots/07-high-risk-and-safety-controls.png)

## What the evidence proves

### 1 — Guarded automation overview

- `v6.0.0-alpha.2` and Guarded internal automation are visible.
- Explicit approval and final confirmation are required.
- Global execution starts paused.
- Dry-run and approval remain available.
- Rules remain disabled by default.
- Only allowlisted Low-risk reversible internal actions may execute.
- No unattended or external action is enabled.

### 2 — Approved, not executed

- Proposal state is `ApprovedNotExecuted`.
- `executed: False` remains visible.
- Approval did not execute.
- A separate final-preview control is required.
- The exact action, Low risk and internal target are visible.
- Audit states that no execution occurred and final confirmation remains required.

### 3 — Final execution preview

- Exact action and exact target are visible.
- Exact before and proposed-after snapshots are retained.
- Risk is Low and reversibility is true.
- The global gate is resumed by explicit action.
- Approval, freshness, trust, source snapshot, typed-handler allowlist, reversibility, risk, capability, internal target and target existence checks all pass.
- Nothing executes until the user confirms guarded execution.

### 4 — Safe internal action executed

- Proposal state is `Executed`.
- `executed: True` is visible.
- Execution result is successful.
- Before and after are retained.
- Undo is available.
- Fictional target review notes changed from 0 to 1.
- Audit retains preview, confirmation and execution success.
- No external or trusted production record changed.

### 5 — Undo restored prior state

- Proposal state is `Undone`.
- Undo succeeded.
- Prior fictional state is restored exactly.
- Review notes return from 1 to 0 and version returns from 2 to 1.
- Undo is no longer available.
- Execution history remains retained.
- Undo request and success are audited.

### 6 — Stale proposal blocked

- Proposal state is `Stale`.
- `executed: False` remains visible.
- Fictional source changed after approval.
- Eligibility fails closed.
- Audit states that source state changed and reevaluation is required.
- Original approval and provenance remain retained.

### 7 — High-risk and safety controls

- The fictional overdue-invoice email action is High risk.
- Target is external communication.
- Communication action and external write are blocked.
- Execution eligibility is blocked by policy.
- The matching dry run remains reviewable.
- Queue status is `Blocked` and `executed: False`.
- Stale and High-risk proposals remain visible and inert.
- No unattended or external action is enabled.

## Excluded from Group 28

No background worker, scheduler, timer-triggered execution, startup execution, automatic approval, execute-after-timeout behavior, automatic retries, email sending, Gmail mutation, calendar mutation, external writes, financial mutation, destructive action, automatic active Follow-Up creation, automatic Work Pipeline mutation, arbitrary scripts, PowerShell execution, process launching, plugin execution, AI-generated rules, Companion App work, major shell redesign or Group 29 work.

# Group 27 â€” Controlled automation foundation

**Version:** `v6.0.0-alpha.1`  
**Status:** Complete  
**Tests:** 112 passed, 0 failed, 0 skipped  
**Release build:** Passed  
**`git diff --check`:** Passed

Group 27 starts the v6 controlled-automation lane. The evidence proves a visible and deterministic safety spine: reviewed or trusted LifeOS state can be evaluated manually, matched conditions are explained, proposed actions enter explicit review, approval records intent without execution, duplicate proposals are linked, and forbidden high-risk capabilities are blocked by policy.

## Screenshot evidence

1. [Automation Centre overview](screenshots/01-automation-centre-overview.png)
2. [Rule details, risk and permissions](screenshots/02-rule-details-risk-permissions.png)
3. [Dry-run match explanation](screenshots/03-dry-run-match-explanation.png)
4. [Approved proposal not executed](screenshots/04-approved-not-executed.png)
5. [Duplicate proposal suspected and retained](screenshots/05-duplicate-suspected-retained.png)
6. [High-risk email proposal blocked](screenshots/06-high-risk-email-blocked.png)
7. [Automation audit history](screenshots/07-automation-audit-history.png)

## What the evidence proves

- `v6.0.0-alpha.1` and the controlled-automation release identity are visible.
- The global banner states manual dry-run, explicit approval and no unattended actions.
- Four fictional sample rules start disabled.
- A rule can be explicitly enabled for `DryRunOnly`.
- Trigger, condition, proposed action, target, risk, approval and permissions are visible.
- The Follow-Up proposal is Medium risk and always requires approval.
- The matching condition shows expected `7`, actual `9`, PASS and `Trust: Reviewed`.
- A matching dry run creates a proposal with explicit Approve and Reject controls.
- Approval retains status `Approved` while the proposal remains `executed: No`.
- Repeated evaluation links to the prior proposal as a duplicate.
- A matching overdue-invoice email proposal is High risk and `BlockedByPolicy`.
- Communication and external-write capabilities are visibly blocked.
- The audit retains enablement, evaluation, generation, approval, duplicate and policy-block events.
- No real client, email, account, money or private project data appears.

## Excluded from Group 27

No background worker, scheduler, timer-triggered execution, startup execution, automatic approval, execute-after-timeout behavior, automatic Follow-Up creation, automatic Work Pipeline mutation, email sending, Gmail mutation, calendar mutation, financial mutation, destructive action, external write, arbitrary script, PowerShell execution, plugin execution, AI-generated rules, Companion App work, major shell redesign or Group 28 work.

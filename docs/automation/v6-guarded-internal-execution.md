# v6 guarded internal execution

## Safety sequence

```text
reviewed/trusted state
-> deterministic dry run
-> proposal
-> explicit approval
-> eligibility revalidation
-> final before/after preview
-> explicit confirmation
-> typed reversible internal handler
-> persisted result and audit
-> optional explicit Undo
```

Approval never executes. The global execution gate starts paused. The only Group 28 executable action is a typed handler that adds one review note to fictional local proof state. Medium-risk Follow-Up drafts and evidence-review proposals remain proposal-only. High-risk communication and every external, financial, destructive, calendar, mailbox, script, process and AI capability remain blocked.

Eligibility is recalculated immediately before preview and execution. It checks approval state, global pause, expiry, duplicate state, rule revision, trust, source snapshot, target existence, allowlist, reversibility, risk, blocked capabilities and prior execution.

The same proposal cannot execute twice. Undo cannot run twice and is blocked when the target changed after execution.

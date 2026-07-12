# Group 29 manual verification

Use fictional demo data only.

- Confirm v6.0.0-alpha.3 and “Controlled orchestration and recovery”.
- Confirm banner says due work is queued for explicit review and no unattended execution is enabled.
- Confirm fictional weekly plan begins disabled.
- Enable it explicitly, calculate due review, and confirm one occurrence appears without execution.
- Resume the global guarded gate, then explicitly Start the orchestration.
- Confirm the run is paused before step 1.
- Open step 1 preview and inspect exact before/after snapshots.
- Confirm only step 1. Verify checkpoint persisted and step 2 remains paused.
- Continue through the safe fictional steps one at a time.
- Restart while paused and verify no automatic resume.
- Exercise the controlled failure path and confirm RecoveryRequired with no automatic retry.
- Explicitly retry, cancel remaining, and review rollback behavior.
- Confirm the High-risk ExternalCommunication email example remains blocked.
- Confirm no email, calendar, mailbox, finance, destructive, active Follow-Up, Work Pipeline operational, script, process, plugin or AI action occurred.

# Controlled orchestration

A schedule decides when work should be reviewed. It does not decide that work should execute.

Group 29 uses provider-neutral persisted plans, ordered typed steps, deterministic occurrences, paused runs and step checkpoints. Manual only, one-time review date and weekly review day are the supported schedule types. A due occurrence can be previewed, snoozed or started explicitly. Starting never executes the first step.

Executable scope is limited to typed Low-risk reversible internal actions:

- add an internal review note
- flag an internal item for attention
- create an internal draft agenda item

Every step rechecks the global gate, dependencies, typed handler, risk, capabilities and current target state. It then shows exact before and proposed-after snapshots. Confirmation applies one mutation, persists a checkpoint and pauses before the next step.

There is no BackgroundService, Task Scheduler integration, startup auto-run, timer-triggered mutation, automatic continuation, arbitrary workflow language or app-closed execution.

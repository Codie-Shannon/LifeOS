# v6 automation hardening and emergency controls

LifeOS keeps automation manual, foreground-only, typed, Low-risk and reversible. The normal guarded-execution pause and Emergency Stop are distinct persisted gates. Emergency Stop fails closed across direct internal execution and orchestration start/step/retry/rollback boundaries. Reset requires explicit confirmation and leaves normal execution paused.

Health is derived from persisted state. Incidents are scoped to the exact proposal, run or step and retain sanitized reason, last-safe checkpoint and explicit recovery options. Retry is explicit and rollback review is reverse ordered. No recovery action deletes audit, checkpoints, proposals or runs.

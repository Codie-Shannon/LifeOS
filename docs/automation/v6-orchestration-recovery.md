# Orchestration recovery

Failure defaults to PauseAndReview. The current step records a sanitized error and the run enters RecoveryRequired. LifeOS does not retry, skip or continue automatically.

Explicit recovery options:

- Retry returns the failed step to paused review and increments its retained retry count.
- Skip is allowed only for an optional step and requires a reason.
- Cancel remaining preserves completed checkpoints and cancels future steps.
- Rollback reviews completed reversible steps and restores them in reverse order. Rollback stops on the first state mismatch.

A successful execution key cannot be applied twice. One occurrence cannot have multiple active runs. Completed, skipped, cancelled or rolled-back step states cannot silently execute again. On load, any in-progress execution is normalized to Paused and requires explicit Resume/review.

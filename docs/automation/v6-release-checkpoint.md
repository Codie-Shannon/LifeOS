# v6 controlled automation release checkpoint

LifeOS v6.0.0-beta.1 consolidates the reviewed, guarded and recoverable internal automation path. It does not expand the automation surface.

The Automation Readiness view is derived from persisted state and reports version/schema alignment, approval separation, typed allowlist enforcement, one-step orchestration, restart safety, Emergency Stop behavior, failure containment, blocked capabilities and the foreground-only runtime boundary.

## Persistence and recovery

The v6 store has an explicit schema version. Older known stores are backed up before normalization. In-progress runs load paused, executing steps load failed/recovery-required, approved intent loads as approved-not-executed, Emergency Stop remains persisted, and reset never resumes work. Unknown future schemas and malformed state fail closed. A sanitized recovery record is written without raw payloads or secrets.

## Fixed safety boundary

No background execution, automatic continuation, automatic retry, external write, communication mutation, financial mutation, destructive action, script/process launch, plugin execution or AI decision is enabled.

# Group 30 manual verification

1. Confirm v6.0.0-alpha.4 and Automation hardening and emergency controls.
2. Confirm Automation Health derives counters from persisted demo state.
3. Activate Emergency Stop and verify an eligible fictional internal action and orchestration step are blocked while review/audit remain visible.
4. Reset Emergency Stop and verify guarded execution remains paused and no proposal/run resumes.
5. Inject the controlled fictional orchestration failure and verify only that run/step enters RecoveryRequired with a sanitized incident and checkpoint.
6. Explicitly retry after review; verify one retry only and no automatic loop.
7. Complete reversible steps, inspect reverse-order rollback preview, confirm rollback, and verify history remains.
8. Confirm the overdue-invoice email remains blocked.
9. Copy diagnostics and verify no OAuth material, raw connector payloads, private cache content or local paths appear.

# Group 30 manual verification

1. Confirm `v6.0.0-alpha.4` and Automation hardening and emergency controls.
2. Confirm Automation Health derives counters from persisted demo state.
3. Activate Emergency Stop and verify internal execution/orchestration are blocked while review, evidence and audit remain visible.
4. Reset Emergency Stop and verify guarded execution remains paused and no proposal/run resumes.
5. Inject the controlled fictional orchestration failure and verify only that run/step enters `RecoveryRequired`.
6. Explicitly retry while dependencies remain succeeded; verify one retry returns the step to paused manual review and no continuation occurs.
7. Roll back a required completed dependency, then attempt retry; verify retry is visibly blocked, the failed step remains failed, the run remains `RecoveryRequired` and retry count does not increment.
8. Inspect reverse-order rollback preview, confirm rollback and verify history remains.
9. Confirm the overdue-invoice email remains blocked.
10. Copy diagnostics and verify no OAuth material, raw connector payloads, private cache content or local paths appear.

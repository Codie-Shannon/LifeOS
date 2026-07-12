# Group 31 manual verification

1. Confirm v6.0.0-beta.1 and the v6 Automation Readiness view.
2. Confirm fresh state is paused, rules are disabled and Emergency Stop is inactive but available.
3. Load representative earlier v6 state and verify backup plus safe normalization.
4. Verify malformed/unknown state fails closed with sanitized recovery guidance.
5. Verify approval records intent only; preview and final confirmation remain separate.
6. Execute one fictional Low-risk reversible internal action and verify before/after, audit and Undo.
7. Verify due orchestration never auto-starts and pauses after each step.
8. Trigger a contained failure; verify explicit retry, cancellation and reverse-order rollback review.
9. Activate Emergency Stop, restart and verify it persists. Reset and verify guarded execution remains paused.
10. Restart with active work and verify no proposal, run or step auto-resumes.
11. Verify the fictional high-risk overdue-invoice email and all external capabilities remain blocked.
12. Run repository hygiene, Gitleaks history scan, dotnet test, Release build and git diff --check.

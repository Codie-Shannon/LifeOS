# Group 37 manual verification

Use fictional data only. Launch Desktop `v7.0.0-alpha.3` and verify the reusable planning workspace and controlled handoff.

1. Ask for a recovery plan for fictional Project Zephyr Quill.
2. Build the review-only plan and inspect Goal, Evidence, Constraint, Step and Verification blocks.
3. Confirm assumptions, conflicts, stale evidence and missing data are distinct.
4. Edit, reorder and remove a block.
5. Produce a conflicting fictional status and verify `Blocked` appears in text, not colour alone.
6. Disable a relevant source and verify a visible `NeedsInput` gap.
7. Preview the exact handoff payload and cancel; verify no review artifact exists.
8. Preview again, choose a target and confirm; verify exactly one review artifact exists.
9. Verify no task, project, Follow-Up, money, calendar, email, automation or orchestration state changed.
10. Restart and verify source permissions remain safe.
11. Run Core/Desktop tests, Companion tests, Desktop Release build, Android build, repository hygiene, NuGet scan, Gitleaks and `git diff --check`.

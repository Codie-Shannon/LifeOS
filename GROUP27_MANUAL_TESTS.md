# Group 27 manual verification

1. Confirm `v6.0.0-alpha.1` and the Controlled automation foundation wording.
2. Open Automation Centre and confirm the dry-run/approval safety banner.
3. Confirm all fictional sample rules begin disabled.
4. Select the follow-up rule, inspect trigger, condition, action, target, risk and permissions.
5. Enable it and run Evaluate dry run. Confirm PASS explanation and one NeedsReview proposal.
6. Approve intent. Confirm state Approved and `executed: No`.
7. Run the same rule again. Confirm DuplicateSuspected and prior linkage.
8. Enable and evaluate the evidence rule, then reject it. Confirm rejection stays in queue/audit.
9. Enable and evaluate the blocked email rule. Confirm BlockedByPolicy and no approve button.
10. Restart LifeOS. Confirm rules, evaluations, proposals, decisions and audit persist.
11. Confirm no Follow-Up, Work Pipeline, Gmail, Calendar, financial or evidence record was changed automatically.
12. Capture only fictional/private-safe screenshots after tests/build/push pass.

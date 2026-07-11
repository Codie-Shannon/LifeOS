# Group 28 manual verification

Use fictional demonstration data only.

1. Launch LifeOS and open Automation Centre.
2. Confirm `v6.0.0-alpha.2`, Guarded internal automation, and no unattended/external actions.
3. Confirm the global execution gate starts PAUSED.
4. Select Missing next-action review note and enable it.
5. Evaluate dry run and approve intent.
6. Confirm status is ApprovedNotExecuted and `executed: False`.
7. While paused, open preview and confirm execution is blocked by the global gate.
8. Resume guarded execution explicitly.
9. Open final preview and verify exact action, target, before, after, Low risk, reversibility and policy checks.
10. Confirm guarded execution.
11. Verify the fictional internal item now has one review note and the proposal is Executed.
12. Verify execution result and audit retain before/after and no external mutation.
13. Select Confirm Undo and verify the review-note count returns to zero while history remains.
14. Reset demo, repeat through approval, select Change fictional source, then open preview and verify Stale/reevaluation required.
15. Verify duplicate-suspected proposals cannot approve or execute.
16. Verify blocked overdue invoice email remains BlockedByPolicy.
17. Pause execution again and restart the app; confirm pause state persists.
18. Confirm no active Follow-Up, Work Pipeline trusted state, email, calendar, mailbox, money or external system changed automatically.

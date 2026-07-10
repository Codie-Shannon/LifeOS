# Group 18 - Command Centre Pressure Engine

## Version

LifeOS Desktop v4.8.

## Purpose

Group 18 verifies the Command Centre Pressure Engine after the v4.8 code pack and footer hotfix.

The pressure engine ranks reviewed local signals into deliberate operating lanes instead of dumping every module item into Today.

## Screenshot evidence

1. `01-command-centre-v4-8-top.png`
   - v4.8 Command Centre hero, sidebar card, and corrected footer.

2. `02-command-centre-v4-8-pressure-metrics.png`
   - Pressure engine state, score, critical/high counts, act-now count, review-first count, waiting count, protected count, pressure money, and suppressed count.

3. `03-next-safest-action-pressure-rule.png`
   - Next safest action and v4.8 pressure-engine rules.

4. `04-top-ranked-pressure-signals.png`
   - Ranked pressure signals across Weekly Close-Out, Money Profile, Bills / Payments, and Payment Calendar.

5. `05-act-now-lane.png`
   - Trusted or time-critical items that can move today.

6. `06-review-before-action-lane.png`
   - Untrusted, imported, estimated, or incomplete signals requiring a human gate.

7. `07-waiting-protected-suppressed-lanes.png`
   - Empty waiting/do-not-chase lane and protected/suppressed items contained by safety rules.

8. `08-pressure-controls-module-bridge.png`
   - Pressure policy controls, module bridge, and current "what matters now" output.

9. `09-protected-suppressed-controls.png`
   - Protected/suppressed cards and local pressure-policy controls.

10. `10-boundary-after-v4-8-local-file.png`
    - v4.8 boundary, v4.9 Integration Inbox + v5 readiness, and local pressure-policy path.

## Verified behaviour

- Critical and due-now items rise first.
- Untrusted items route to review before action.
- Waiting-on-others and blocked work remain visible without consuming active sprint time.
- Expected money remains excluded from safe money.
- Protected and suppressed signals remain contained.
- The pressure engine ranks signals but does not replace source modules.
- The local footer and sidebar now correctly show v4.8.
- The waiting lane may legitimately contain zero items; this is valid evidence of the lane state.

## Boundary

v4.8 is local ranking and suppression only. It does not send messages, pay bills, move money, close projects, create invoices, accept OCR, change external calendars, sync external systems, or run AI actions.

## Next lane

v4.9 Integration Inbox + v5 readiness.

# LifeOS Desktop v22.0.0-alpha.1 Release Notes

LifeOS Desktop v22 completes Pay-Later and Money Integrations across Groups 95-98.

## Release theme

**Evidence-backed money context without payment authority**

## Added

- Afterpay and Zip evidence parsing into review candidates.
- Duplicate statement detection without silent merging.
- Explicit candidate confirmation.
- Confirmed next deductions excluded from safe-money calculations.
- Read-only CSV/Xero export contract.
- Pay-Later & Money Integration Review in the Money workspace.

## Safety boundaries

- Email evidence creates candidates only.
- Unconfirmed and duplicate records do not change safe money.
- No payment initiation.
- No autonomous reconciliation.
- Accounting-provider writes remain disabled.
- Source references and review state remain visible.

## Validation

- 394 Core tests passed.
- 54 Mobile tests passed.
- Desktop Release build completed cleanly.
- Android Release build completed cleanly.
- Official evidence is in [`screenshot-groups/group-95-98-pay-later-money-integrations`](screenshot-groups/group-95-98-pay-later-money-integrations/).

## Next

Group 99 begins v23 New Zealand Grocery Lookup Worker. The approved roadmap continues through Group 120.

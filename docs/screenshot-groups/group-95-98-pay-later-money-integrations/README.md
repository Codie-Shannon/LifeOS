# Groups 95-98 - Pay-Later and Money Integrations

- Screenshot group: SG-75
- Release: v22 Money Integrations
- Version: 22.0.0-alpha.1
- Development commit: `7a323ac`

## Scope

SG-75 demonstrates review-first Afterpay and Zip evidence parsing, duplicate
handling, confirmed-deduction treatment and read-only money-provider
contracts. Email evidence creates candidates only; safe-money calculations
change after confirmation, and banking/accounting contracts cannot reconcile
or initiate payments.

Five genuine captures document the implemented Desktop surface and validation
without manufacturing Mobile or Website feature views.

## Screenshots

1. `01-desktop-money-integration-navigation.png`
   - Navigation: Money
   - Shows the reachable Pay-Later & Money Integration Review module.
2. `02-desktop-pay-later-review-overview.png`
   - Navigation: Money > Pay-Later & Money Integration Review
   - Shows the SG-75 boundary, NZD 360 remaining, NZD 90 next deductions, two
     review candidates and zero initiated payments.
3. `03-desktop-pay-later-review-records.png`
   - Navigation: Money > Pay-Later & Money Integration Review, records
   - Shows the Afterpay candidate, confirmed Zip record, read-only Xero export
     contract and blocked bank-payment action.
4. `04-desktop-v22-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v22.0.0-alpha.1 and v22 Money Integrations.
5. `05-group75-validation.png`
   - Shows four targeted tests, 394 Core tests and 54 Mobile tests passing,
     clean Desktop/Android Release builds, Git diff validation and `7a323ac`.

## Verification

- SG-75 targeted tests: 4 passed, 0 failed
- Core tests: 394 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Desktop warnings: 0
- Desktop errors: 0
- Android Release build: succeeded
- Git diff check: passed
- Private-data review: passed
- Payments initiated: 0
- Mobile feature screenshots: not applicable; SG-75 only aligns Mobile version
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

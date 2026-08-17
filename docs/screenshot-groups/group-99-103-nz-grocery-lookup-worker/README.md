# Groups 99-103 - New Zealand Grocery Lookup Worker

- Screenshot group: SG-103
- Release: v23 Grocery Intelligence
- Version: 23.0.0-alpha.1
- Development commit: `2610420`

## Scope

SG-103 demonstrates consent-based New Zealand grocery lookup with a
user-selected location or optional nearest-town pricing. Price records expose
their retailer, capture time and confidence, while unavailable provider paths
remain visible and offer manual fallback.

Five genuine captures document the implemented Desktop surface and validation
without manufacturing Mobile or Website feature views.

## Screenshots

1. `01-desktop-grocery-lookup-navigation.png`
   - Navigation: Household
   - Shows the reachable NZ Grocery Price Lookup module.
2. `02-desktop-grocery-lookup-overview.png`
   - Navigation: Household > NZ Grocery Price Lookup
   - Shows Whanganui, three retailers, nine fresh prices and zero cart mutations.
3. `03-desktop-grocery-price-records.png`
   - Shows sourced pricing, comparable products, manual fallback and refresh off.
4. `04-desktop-v23-release-version.png`
   - Shows Desktop release v23.0.0-alpha.1 and v23 Grocery Intelligence.
5. `05-sg103-validation.png`
   - Shows targeted, Core and Mobile tests plus Release builds passing.

## Verification

- SG-103 targeted tests: 5 passed, 0 failed
- Core tests: 399 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Android Release build: succeeded
- Git diff check: passed
- Private-data review: passed
- Cart mutations: 0
- Mobile feature screenshots: not applicable; SG-103 only aligns Mobile version
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

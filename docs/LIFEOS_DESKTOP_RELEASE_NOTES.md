# LifeOS Desktop v23.0.0-alpha.1 Release Notes

LifeOS Desktop v23 completes the New Zealand Grocery Lookup Worker across
Groups 99-103.

## Release theme

**Consent-based local grocery intelligence without purchase authority**

## Added

- User-selected location and optional nearest-town pricing.
- Explicit retailer and location consent.
- Comparable-product and store-price context.
- Source, capture time, freshness and confidence visibility.
- Visible worker status and manual fallback.
- NZ Grocery Price Lookup in the Household workspace.

## Safety boundaries

- Lookup starts only after explicit consent.
- Future-dated evidence cannot count as fresh.
- Provider failures remain visible and offer manual fallback.
- Background refresh is off by default.
- No automatic ordering, substitution, payment or cart mutation.

## Validation

- 399 Core tests passed.
- 54 Mobile tests passed.
- Desktop Release build completed cleanly.
- Android Release build completed cleanly.
- Official evidence is in
  [`screenshot-groups/group-99-103-nz-grocery-lookup-worker`](screenshot-groups/group-99-103-nz-grocery-lookup-worker/).

## Next

Group 104 begins v24 Evidence Automation and Proof Tooling. The approved
roadmap continues through Group 120.

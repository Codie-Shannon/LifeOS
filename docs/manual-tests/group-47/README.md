# Group 47 manual verification — Integration Inbox

Group 47 Pack 1 adds the permanent normalized Integration Inbox at `v9.0.0-alpha.2`. The proof uses deterministic fictional candidates only.

## Deterministic proof-state reset

Close LifeOS and delete only:

```text
%LOCALAPPDATA%\LifeOS\integration-inbox-v9.json
```

Restart LifeOS to recreate the Group 47 fictional seed. Do not delete the Group 46 Integration Control Centre state or any operational module store.

## Required manual checks

1. The top-bar Review button shows the current Integration Inbox review count and opens the embedded Inbox.
2. Settings / Integrations exposes both Integration Control Centre and Integration Inbox as separate embedded system surfaces.
3. The existing Assistant Integration Inbox route opens the new embedded Group 47 surface rather than a separate legacy window.
4. The normal title bar, top bar, workspace rail, resizing, movement, theme, density, High Contrast and text scaling remain available.
5. The Inbox default view shows mixed fictional message, calendar, contact, file, task, financial and generic candidates.
6. Selecting a candidate exposes provider, account, external ID, capability, source timestamp, imported timestamp, freshness and raw provider reference.
7. No unnecessary raw provider payload, credential, token or secret is displayed or stored.
8. Duplicate review shows distinct external IDs and a deterministic normalized fingerprint match.
9. Duplicate choices include link existing, keep separate, replace candidate, ignore and reject.
10. Conflict review displays current LifeOS values, source candidate values and explicit per-field decisions.
11. Accepted candidates can link to authoritative LifeOS records without creating duplicate editors.
12. Batch review is preview-first, limited to low-risk candidates of the same type and identical decision, and can be cancelled without mutation.
13. Stale candidates remain visibly stale.
14. Source removal creates a tombstone and preserves normalized fields plus provenance.
15. Malformed candidates are quarantined and cannot be accepted.
16. Review decisions append ordered audit entries.
17. No live provider connection, OAuth consent, external read, background polling or external write exists.
18. Exactly eight screenshot scenarios are defined; Pack 1 contains no screenshots.

## Screenshot sequence

1. Integration Inbox with mixed fictional candidates.
2. Full provenance and freshness detail.
3. Duplicate suspected comparison.
4. Field-level conflict review.
5. Accepted candidate linked to an authoritative LifeOS record.
6. Batch review preview.
7. Stale and source-removed handling.
8. Group 47 validation summary and clean synchronization.

## Stop rule

Do not connect a live provider, request OAuth consent, silently trust imported records, download unnecessary raw payloads or add external write actions. Do not begin Group 48 Microsoft implementation before Group 47 Pack 2 is committed, pushed and verified.

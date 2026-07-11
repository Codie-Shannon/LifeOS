# Screenshot Group 25 â€” Authenticated Read-Only Gmail Connector

**Version:** `v5.0.0-alpha.5`
**Tests:** 91 passed, 0 failed, 0 skipped
**Release build:** passed
**Scope:** one private/testing-mode Gmail account, exact `gmail.readonly`, explicit manual bounded search only

## Evidence set

1. [`01-connected-read-only-lifecycle.png`](screenshots/01-connected-read-only-lifecycle.png)
   Shows the Group 25 version, authenticated Gmail read-only safety banner, connected-local lifecycle, exact scope, manual-only mode, and no automatic mailbox or LifeOS mutation.

2. [`02-explicit-read-only-connection-confirmation.png`](screenshots/02-explicit-read-only-connection-confirmation.png)
   Shows explicit confirmation before browser OAuth and states that LifeOS cannot send, delete, move, label, star, archive, or mark mail.

3. [`03-bounded-search-preview-query-exclusions.png`](screenshots/03-bounded-search-preview-query-exclusions.png)
   Shows the selected Email Radar profile, bounded date range, cap 25, exact scope, generated Gmail query, visible exclusions, and preview-only wording.

4. [`04-manual-search-confirmation.png`](screenshots/04-manual-search-confirmation.png)
   Shows explicit confirmation before retrieval and states that messages will be saved as untrusted inert evidence with no automatic mailbox or LifeOS changes.

5. [`05-successful-search-one-candidate.png`](screenshots/05-successful-search-one-candidate.png)
   Shows one message received, zero duplicates, zero skipped, one candidate, and continued untrusted review state.

6. [`06-confirmed-gmail-timeline.png`](screenshots/06-confirmed-gmail-timeline.png)
   Shows a harmless Gmail proof message in the confirmed communication timeline with the account identity hidden and Gmail provenance retained.

7. [`07-repeated-search-duplicate-count.png`](screenshots/07-repeated-search-duplicate-count.png)
   Shows the same bounded search returning one message and reporting one duplicate.

8. [`08-disconnected-evidence-retained.png`](screenshots/08-disconnected-evidence-retained.png)
   Shows configured/disconnected state after local authorization removal while imported evidence remains retained.

9. [`09-cache-cleared-evidence-retained.png`](screenshots/09-cache-cleared-evidence-retained.png)
   Shows local Gmail connector cache cleared while imported evidence remains retained.

## Trust and safety proven

- Authentication does not imply trust.
- Gmail data enters the provider-neutral Email Radar evidence model.
- Searches are profile-bound, date-bounded, capped, previewed, and explicitly confirmed.
- Candidate confirmation remains mandatory.
- Only confirmed evidence enters the confirmed timeline.
- Repeated retrieval produces duplicate detection.
- Disconnect and connector-cache clearing do not erase imported evidence.
- No mailbox mutation or automatic LifeOS operational mutation occurs.
- No credentials, token values, private account address, private sender details, or unsafe mailbox content are committed.

## Known limitations

- one Gmail account
- one email provider
- private/testing-mode OAuth project
- restricted read-only scope
- manual bounded search only
- no whole-mailbox scan
- no background polling, push, history monitoring, or scheduled scans
- deterministic matching only
- no attachment download or HTML mail reader
- no sending or mailbox mutation
- no automatic Follow-Up or Work Pipeline mutation
- no AI interpretation
- one local desktop user
- major UI redesign deferred

Group 26 has not started.

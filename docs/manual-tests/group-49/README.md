# Group 49 manual validation — Microsoft Files

Group 49 reuses the Group 48 Microsoft Entra public-client registration. It adds incremental delegated read-only consent for `Files.Read` and `Sites.Read.All`; no second application registration, client secret, provider write, automatic body download, or background synchronization is introduced.

## Validation sequence

1. Confirm the repository started at Group 48 commit `f7c418e9445370dbd5b6718483100260efcf33c8` on clean `main` synchronized with `origin/main`.
2. Open Settings > Integrations > Microsoft Provider and reconnect only when incremental consent is required.
3. Confirm the authorization request contains the existing Group 48 scopes plus `Files.Read` and `Sites.Read.All`, and contains no Files/Sites write scopes.
4. Verify Personal and Work Microsoft identities remain separate.
5. Discover drives, SharePoint sites and document libraries, then explicitly select the bounded sources used for the proof.
6. Run foreground metadata synchronization. Confirm only selected drives/folders/sites/libraries enter the Integration Inbox.
7. Inspect a OneDrive and SharePoint file candidate. Confirm provider/account/external provenance, source/import timestamps, freshness, name, type, size, created/modified values, web reference, parent, owner where available and ETag/change reference.
8. Confirm `body-downloaded = false`; do not download or open file bodies as part of Group 49 evidence.
9. Re-run the same source and confirm idempotent re-import does not create a second authoritative candidate.
10. Exercise renamed/moved, source-removed and permission-lost fixtures. Confirm the state remains visible and stale data is not promoted to current.
11. Accept one document candidate and link it to an existing Project or Work record. Confirm the source candidate remains authoritative and no duplicate editor is created.
12. Disconnect and reconnect. Confirm machine-local tokens are removed/recreated while already accepted LifeOS records remain.
13. Run the automated tests, Release builds, vulnerability checks, Gitleaks, hygiene and synchronization checks from the Pack 1 runner.

## Screenshot placeholders

Pack 1 creates no committed screenshot evidence. Pack 2 must contain exactly the eight approved Group 49 screenshots named `01` through `08` according to the private context PDF.

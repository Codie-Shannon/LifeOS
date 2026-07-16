# Group 46 manual verification — Integration Control Centre

Group 46 starts the LifeOS v9 integration lane at `v9.0.0-alpha.1`. It adds the permanent system-level Integration Control Centre under Settings. This group uses deterministic fictional providers and accounts only.

## Entry conditions

- Repository is on the exact Group 45 baseline before Pack 1 is applied.
- Pack 1 tests, builds, scans, commit, push and synchronization have completed successfully.
- Desktop launches through the permanent eight-workspace v8 shell.
- No live Microsoft or Google credential, token, account or external provider read is used.

## Deterministic proof-state reset

The Integration Control Centre stores fictional proof state outside Git at:

```text
%LOCALAPPDATA%\LifeOS\integration-control-centre.json
```

Close LifeOS and delete only that file to restore the deterministic fictional provider/account seed on next launch. Do not delete operational module stores.

## Required manual checks

1. Open **Settings** and confirm **Integration Control Centre** appears inside the Integrations section rather than as a ninth workspace.
2. Open the Control Centre and confirm all provider cards state **FICTIONAL** and the top boundary states **NO LIVE CREDENTIALS**.
3. Confirm the provider catalogue exposes Microsoft 365, Google Workspace and Local connectors through one shared lifecycle/permission/health contract.
4. Confirm the Work account and Personal account are visibly separate, with classification, provider identity and connection state shown independently.
5. Open **Capabilities & permissions** and confirm Required/Optional is separate from Granted/Missing/Revoked/Not Requested.
6. Confirm capability health is calculated separately: healthy Mail, stale Calendar and Needs Attention Files must be simultaneously visible for the fictional work account.
7. Run Refresh and confirm bounded, explicit audit output; no external data or write action occurs.
8. Open Reconnect review, cancel once, then explicitly confirm and verify ordered audit entries.
9. Open Revoke review and confirm the account, current state, action effect and no-live-provider boundary are visible before confirmation.
10. Open Disconnect review and verify all three explicit retention choices:
    - keep accepted LifeOS records;
    - archive provider links;
    - remove unaccepted imported candidates.
11. Confirm accepted LifeOS records are never described as silently deleted by disconnect or revoke.
12. Open Audit history and verify consent, sync, permission, reconnect, revoke and disconnect events are ordered, timestamped and sanitized.
13. Verify the Control Centre in Light, Dark and High Contrast themes.
14. Verify Comfortable and Compact density plus 100%, 110%, 125% and 140% text scale remain usable.
15. Verify keyboard focus is visible and the key controls expose stable AutomationIds.
16. Confirm the local JSON contains no client secret, access token, refresh token, authorization code or password field.

## Stop rule

Do not connect a live Microsoft or Google account, request OAuth consent, ingest real external data, add Integration Inbox Group 47 logic or add any external write action.

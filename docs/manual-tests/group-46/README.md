# Group 46 manual verification — Integration Control Centre

Group 46 starts the LifeOS v9 integration lane at `v9.0.0-alpha.1`. It adds the permanent system-level Integration Control Centre as an embedded Settings subpage. This group uses deterministic fictional providers and accounts only.

## Entry conditions

- Group 46 Pack 1 and both Settings ComboBox corrections are committed, pushed and synchronized.
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
2. Confirm Theme, Accent, Density, Startup workspace and Text scale retain the established dark custom ComboBox presentation.
3. Select **Open Integration Control Centre** and confirm it replaces the Settings overview inside the existing LifeOS shell.
4. Confirm the left workspace rail, top bar, main window title bar and normal window movement/resizing remain available.
5. Confirm no second LifeOS window, borderless overlay or laptop-sized immovable surface is created.
6. Select **Back to Settings** and confirm the normal Settings overview returns at the top of the page.
7. Reopen the Control Centre and confirm only one embedded instance is used.
8. Press **Escape** while the Control Centre is open and confirm it returns to the Settings overview.
9. Navigate directly to another workspace while the Control Centre is open and confirm the normal workspace header and metrics are restored.
10. Confirm all provider cards state **FICTIONAL** and the boundary states **NO LIVE CREDENTIALS**.
11. Confirm Microsoft 365, Google Workspace and Local connectors use the shared account, permission, health, audit and recovery contract.
12. Confirm the Work account and Personal account are visibly separate.
13. Open **Capabilities & permissions** and confirm Required/Optional is separate from Granted/Missing/Revoked/Not Requested.
14. Confirm healthy, stale and Needs Attention capability states can be visible simultaneously.
15. Run Refresh and confirm no external read or write occurs.
16. Open Reconnect, Revoke and Disconnect reviews and confirm only those review confirmations open as modal dialogs owned by the main LifeOS window.
17. Verify all three disconnect retention choices.
18. Open Audit history and verify ordered, timestamped and sanitized entries.
19. Verify Light, Dark and High Contrast themes.
20. Verify Comfortable and Compact density plus 100%, 110%, 125% and 140% text scale.
21. Verify keyboard focus, AutomationIds and inner scrolling remain usable on the laptop display.
22. Confirm the local JSON contains no client secret, access token, refresh token, authorization code or password field.

## Stop rule

Do not connect a live Microsoft or Google account, request OAuth consent, ingest real external data, add Integration Inbox Group 47 logic or add any external write action.

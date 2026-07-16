# Group 46 manual verification — Integration Control Centre

Group 46 starts the LifeOS v9 integration lane at `v9.0.0-alpha.1`. It adds the permanent system-level Integration Control Centre as an embedded Settings subpage. This group uses deterministic fictional providers and accounts only.

## Deterministic proof-state reset

The Integration Control Centre stores fictional proof state outside Git at:

```text
%LOCALAPPDATA%\LifeOS\integration-control-centre.json
```

Close LifeOS and delete only that file to restore the deterministic fictional provider/account seed on next launch. Do not delete operational module stores.

## Verified manual checks

1. **Integration Control Centre** is under Settings and does not create a ninth workspace.
2. The Control Centre is embedded inside the existing LifeOS shell rather than opening a separate borderless overlay.
3. The main title bar, workspace rail, top bar, movement, resizing and normal workspace navigation remain available.
4. Theme, Accent, Density, Startup workspace and Text scale retain the established custom ComboBox presentation.
5. Overview, Capabilities & permissions and Audit history use readable custom dark tabs without native white rendering or clipped selection indicators.
6. Microsoft 365, Google Workspace and Local connector proof cards are explicitly marked fictional.
7. Work and Personal accounts remain visibly and independently classified.
8. Required/Optional permission classification is separate from Granted/Missing/Revoked/Not Requested consent state.
9. Healthy, stale, unavailable and Needs Attention states are represented per capability.
10. Reconnect and revoke actions require explicit review before confirmation.
11. The connection review dialog is resizable and scroll-safe; its confirmation control and actions are fully visible.
12. Disconnect review exposes all three explicit retention choices:
    - keep accepted LifeOS records;
    - archive provider links;
    - remove unaccepted imported candidates.
13. Audit entries are ordered, timestamped, source-identifiable and sanitized.
14. No external read, write, OAuth token, client secret or authorization code appears in the proof.
15. The Group 46 evidence contract contains exactly eight ordered PNG screenshots.
16. The repository was synchronized and clean before Pack 2 finalization.

## Closure

Group 46 Pack 2 reruns the complete Core/Desktop, Website and Companion regression/build boundary, NuGet vulnerability checks, Gitleaks, repository hygiene, evidence validation and clean synchronization verification before committing the evidence.

Group 46 is complete and closed. Group 47 Integration Inbox work has not started.

## Stop rule

Do not connect a live Microsoft or Google account, request OAuth consent, ingest real external data, add Integration Inbox Group 47 logic or add any external write action.

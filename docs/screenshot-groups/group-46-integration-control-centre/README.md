# Group 46 — Integration Control Centre evidence

Evidence status: **complete and closed**.

Exactly eight approved PNG screenshots are authoritative for Group 46. They use only deterministic fictional provider/account state and expose no provider credential, OAuth token, client secret, authorization code or real external record.

1. `01_Integration_Control_Centre_Overview.png` — embedded Control Centre, provider catalogue, summary cards and fictional-only boundary.
2. `02_Work_And_Personal_Accounts_Separated.png` — connected Work and Personal accounts shown independently.
3. `03_Capabilities_And_Incremental_Permissions.png` — Required/Optional classification plus Granted, Missing and Not Requested states.
4. `04_Per_Capability_Health.png` — Healthy, Fresh, Stale, Not Available and Needs Attention states represented per capability.
5. `05_Reconnect_Or_Revoke_Review.png` — explicit account/action review with safety explanation, confirmation boundary and visible actions.
6. `06_Disconnect_Retention_Choices.png` — all three explicit data-retention choices and disconnect confirmation boundary.
7. `07_Connector_Audit_History.png` — ordered fictional refresh, permission, connection and initial-sync audit entries.
8. `08_Group_46_Validation_And_Clean_Sync.png` — evidence contract, release identity, disabled writes, fictional-only state, synchronized HEAD and clean working tree ready for Pack 2.

Pack 2 rejects missing, duplicate, renamed or extra PNG files. Its runner reruns evidence validation with `-RequireImages`, full regressions/builds, NuGet vulnerability scans, Gitleaks, repository hygiene, commit, push and final clean synchronization.

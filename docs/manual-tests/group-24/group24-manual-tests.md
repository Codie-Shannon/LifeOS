# Group 24 manual test steps

1. Launch LifeOS and confirm the left status card shows `Desktop v5.0.0-alpha.4` and Email Radar foundation wording.
2. Open **Email Radar**.
3. Confirm the banner states local/imported evidence only, no Gmail/Outlook connection, confirmation required, and no automatic LifeOS mutation.
4. Confirm the fictional Harbour Workshop profile appears.
5. Click **Create fictional profile** and confirm Northline Repairs is persisted after leaving and returning.
6. Click **Preview JSON/CSV import** and select `docs\integrations\lifeos-v5-email-radar-sample.json`.
7. Confirm the preview reports valid records and explicitly states nothing has been saved.
8. Click **Confirm pending import**, approve the gate, and confirm records appear.
9. Confirm candidate cards show deterministic reasons and provenance.
10. Confirm a possible match does not appear in the confirmed timeline.
11. Confirm one non-duplicate candidate; verify it enters the confirmed timeline.
12. Reject another candidate; verify the rejected state remains visible and does not enter the timeline.
13. Import the same sample again; verify duplicate-suspected records are visible and cannot be confirmed until resolved.
14. Confirm the suggestion uses Possible/Suggested/Requires review wording.
15. Confirm no Follow-Up or Work Pipeline record was created automatically.
16. Confirm Integration Inbox and Google Calendar lifecycle still load normally.

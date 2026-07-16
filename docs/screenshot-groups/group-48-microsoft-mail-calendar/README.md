# Group 48 — Microsoft Mail and Calendar screenshot evidence

Evidence status: **complete and closed**.

Exactly eight approved PNG screenshots are authoritative for Group 48. The proof uses live user-authorized Microsoft reads while preserving the read-only boundary. Full email addresses, opaque provider message IDs, thread IDs and raw provider references are redacted where visible. No token, authorization code, client secret, message body, attachment body or local provider configuration is included.

1. `01_Microsoft_Provider_Connected_Accounts.png` — separately classified Personal and Work identities, connected state, bounded Mail/Calendar sync result and no provider error.
2. `02_Microsoft_Capability_And_Permission_Catalogue.png` — Outlook Mail and Calendar granted for Group 48; later Microsoft capabilities remain Not Requested; sanitized audit visible.
3. `03_Outlook_Mail_Candidates.png` — source-backed Outlook Message candidate, untrusted review boundary, provenance/freshness and normalized fields.
4. `04_Outlook_Message_Provenance_And_Attachments.png` — message subject, redacted sender/recipient/thread values, importance, read state and attachment metadata only.
5. `05_Microsoft_Calendar_Candidates.png` — reviewable Calendar Event with title, start/end, time zone, location, redacted organizer address, response state and recurrence reference.
6. `06_Reviewable_Mail_Suggestion.png` — separate follow-up suggestion generated from source-backed mail with provider provenance and normalized follow-up fields.
7. `07_Microsoft_Recovery_And_Partial_Permission.png` — controlled invalid-calendar failure, Mail success, Needs Attention state and sanitized error while later capabilities remain Not Requested.
8. `08_Group_48_Validation_And_Clean_Sync.png` — eight-scenario contract, release identity, restored default calendar, clean provider health, read-only scopes, tests/build, synchronized HEAD and clean working tree ready for Pack 2.

Pack 2 rejects missing, duplicate, renamed or extra PNG files. Its runner reruns evidence validation with `-RequireImages`, full regressions/builds, NuGet vulnerability scans, Gitleaks, repository hygiene, commit, push and final clean synchronization.

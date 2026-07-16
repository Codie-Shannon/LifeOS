# Group 48 manual verification — Microsoft Mail and Calendar

Pack 1 release: `v9.0.0-alpha.3`.

## Required setup

Complete `docs/integrations/microsoft-provider-setup.md` first. Do not place client configuration, tokens or private provider data in Git.

## Manual checks

1. Microsoft Provider Setup is embedded under Settings, not a ninth workspace or separate borderless overlay.
2. The complete planned Microsoft capability catalogue appears once.
3. Mail and Calendar are Group 48 read-only capabilities.
4. Contacts/People, OneDrive, SharePoint, Teams, Power BI and Power Automate remain Not Requested.
5. The Desktop redirect URI is visible and environment-specific.
6. Saving configuration writes only to `%LOCALAPPDATA%\LifeOS`.
7. Connecting opens the system browser and uses authorization-code flow with PKCE.
8. Microsoft consent requests only `User.Read`, `Mail.Read`, `Calendars.Read`, `openid`, `profile` and `offline_access`.
9. Multiple accounts remain distinguishable with explicit Personal/Work classification.
10. Identity verification performs only a safe `/me` read.
11. Mail sync is folder-bounded, date-bounded and item-bounded.
12. Calendar sync is calendar-bounded, date-window-bounded and item-bounded.
13. Message metadata includes sender, recipients, subject, timestamps, importance, read state, conversation and attachment metadata.
14. Attachment bodies are not downloaded automatically.
15. Calendar metadata includes start/end, timezone, location, organizer, attendees, response state, recurrence, online meeting reference and last modified time.
16. Imported records enter Integration Inbox as untrusted candidates.
17. Follow-up, waiting-on, work-context and schedule records are reviewable suggestions only.
18. Re-import is idempotent; changed source records create review conflicts.
19. Expired, revoked, tenant-denied, offline, throttled and partial-permission states remain visible.
20. Disconnect deletes the local token and retains accepted LifeOS records.
21. No send, draft, mark-read, event create/update/delete/cancel, Teams action, Power Automate run or Power BI data access exists.
22. No token, authorization code, client secret or private provider record appears in Git, ordinary logs or screenshots.

## Screenshot evidence — exactly eight

1. Microsoft provider connected with Personal/Work account identity.
2. Capability catalogue with Mail/Calendar granted and later capabilities Not Requested.
3. Outlook Mail candidates in Integration Inbox.
4. Outlook message provenance and attachment metadata.
5. Microsoft Calendar candidates with timezone and attendee context.
6. Reviewable follow-up/waiting-on suggestion from source-backed mail.
7. Revoked/expired/partial-permission recovery state.
8. Group 48 validation summary, safe provider setup and clean synchronization.

## Pack 1 stop rule

Pack 1 commits implementation, tests, docs and evidence contracts only. Do not add screenshots until manual UI and live-provider review are approved.

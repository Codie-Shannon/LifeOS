# Group 22 - Google Calendar Read-Only Connector

## Release

LifeOS Desktop v5.0-alpha first live read-only connector checkpoint.

## Purpose

Group 22 proves that one narrow authenticated Google Calendar connector can retrieve a user-confirmed bounded date range and feed source-backed events into the existing Integration Inbox without changing Google Calendar or any operational LifeOS module.

The connector remains explicitly manual and review-first:

```text
Google Calendar event
-> authenticated read-only fetch
-> normalized calendar intake
-> IntegrationPreviewIntake
-> duplicate check and audit record
-> untrusted Integration Inbox preview
-> deliberate human review
```

## Verified live behavior

- Google OAuth completed against a real test account.
- The connector requested the `calendar.readonly` scope.
- The connection remained local and refresh remained manual.
- The user selected a bounded date range before retrieval.
- Five safe sample events were received as read-only calendar previews.
- No malformed events were reported during the verified refresh.
- Imported events appeared as `New - Untrusted` with `Human review: Required`.
- Provider source, calendar identity, external reference, event end, fetched timestamp, duplicate key, and source evidence were retained.
- Repeating the same refresh produced five duplicate-suspected previews.
- Duplicate-suspected records remained visible and guarded from blind acceptance.
- Disconnect deleted the local token cache while retaining review evidence.
- The connector returned to a clear disconnected state.

## Screenshot set

2. `screenshots/02-connector-safety-rules.png` - review-first connector rules and the read-only Google Calendar control surface.
3. `screenshots/03-readonly-connect-confirmation.png` - explicit confirmation that LifeOS requests only calendar read access and cannot edit events.
4. `screenshots/04-google-calendar-connection-success.png` - successful local read-only Google Calendar connection with manual refresh retained.
5. `screenshots/05-google-calendar-connected-status.png` - stable connected state showing `calendar.readonly`, manual refresh, the 31-day maximum, and explicit disconnect.
6. `screenshots/06-bounded-refresh-date-range.png` - user-selected bounded date range before refresh.
7. `screenshots/07-refresh-review-confirmation.png` - final confirmation that results remain untrusted previews and no calendar or LifeOS module is changed.
8. `screenshots/08-first-refresh-success.png` - five read-only previews received, zero duplicate-suspected on first retrieval, and zero malformed events.
9. `screenshots/09-google-calendar-untrusted-previews.png` - source-backed Google Calendar records in the normal Integration Inbox review queue as untrusted previews requiring human review.
10. `screenshots/10-duplicate-refresh-result.png` - repeated bounded refresh returning five duplicate-suspected previews and zero malformed events.
11. `screenshots/11-duplicate-suspected-preview.png` - a repeated Google Calendar event visibly marked `DuplicateSuspected - Untrusted` with duplicate review evidence.
12. `screenshots/12-disconnect-token-cache-deleted.png` - explicit disconnect result confirming local token-cache deletion and retained previews.
13. `screenshots/13-final-disconnected-state.png` - final disconnected/configuration-required state after the live proof.

Local configuration paths were redacted from the documentation copies. No client ID, client secret, access token, refresh token, Google email address, callback value, or private provider-console value is included.

## Safety boundary

Group 22 does not provide:

- calendar create, edit, or delete actions
- Gmail or Outlook access
- inbox scanning
- scheduled, startup, or background refresh
- continuous polling
- automatic retries
- automatic preview acceptance
- automatic target-module linking
- automatic Agenda or LifeOS item creation
- AI summarisation or autonomous action


## Completion boundary

Group 22 is complete after the screenshot/documentation commit is tested, built, pushed, synchronized with `origin/main`, and left with a clean working tree.

Group 23 has not started. Connector-management hardening, scheduled refresh, and Email Radar remain outside this checkpoint.

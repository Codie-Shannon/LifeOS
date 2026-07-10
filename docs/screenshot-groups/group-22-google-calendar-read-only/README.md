# Group 22 — Google Calendar read-only connector

## Product lane

v5.0-alpha — first live read-only connector.

## Implemented product boundary

- One Google Calendar provider boundary only.
- OAuth 2.0 Authorization Code with PKCE.
- Smallest practical scope: `https://www.googleapis.com/auth/calendar.readonly`.
- Local loopback callback.
- Client ID loaded from local AppData configuration; no client secret is required.
- OAuth token cache protected with Windows DPAPI and stored outside the repository.
- Explicit manual refresh only.
- User-confirmed date range capped at 31 days and within one year of today.
- Provider events normalize into the existing Integration Inbox intake boundary.
- Every event remains an untrusted, read-only preview requiring human review.
- Existing duplicate detection and acceptance guard remain active.
- Disconnect deletes the local protected token cache but retains source-backed previews and audit history.
- No calendar writes, background polling, automatic retry loop, automatic acceptance, automatic linking, or automatic module mutation.

## Provider setup still required

1. Create a Google Cloud desktop OAuth client for the intended local test project.
2. Enable the Google Calendar API.
3. Add the exact loopback redirect URI shown in the local LifeOS configuration template.
4. Open `%LOCALAPPDATA%\LifeOS\connectors\google-calendar\configuration.json`.
5. Replace `REPLACE_WITH_LOCAL_GOOGLE_OAUTH_CLIENT_ID` with the desktop OAuth client ID.
6. Keep all private provider values out of Git, screenshots, docs, and test fixtures.

## Verification honesty

The source pack contains provider transport, authorization initiation, callback/token exchange, protected token storage, bounded retrieval, normalized mapping, review-first intake, duplicate handling, audit creation, disconnect, and unit tests using fakes.

A real Google account connection must be verified locally after provider-console setup. Do not describe the connector as live-tested until that succeeds.

## Planned screenshots

1. Connector area with read-only/manual-review safety wording.
2. Local configuration or bounded refresh state with private values redacted.
3. Manual refresh confirmation/date range.
4. Integration Inbox calendar previews in untrusted state.
5. Duplicate-suspected repeated event.
6. Disconnected state/token-cache deletion control.
7. Meaningful provider setup or empty state only when useful.

# LifeOS Current Status

## Current release

LifeOS Desktop v5.0.0-alpha.4 is the current product state.

## Current state

The v4 operating spine remains complete. Group 24 adds the local/imported Email Radar foundation: user-guided profiles, inert JSON/CSV evidence, deterministic explained candidate matching, explicit confirm/reject review, confirmed-only communication timelines, and review-first waiting-on suggestions.

Google Calendar connection remains explicit and requests only `calendar.readonly`. Refresh remains manual and date-bounded to a maximum of 31 days. Retrieved events enter the existing Integration Inbox as untrusted read-only previews. Provenance, duplicate suspicion, audit history, human acceptance, and separate target-module handoff remain mandatory.

## Current product checkpoint

Group 23 product checkpoint â€” Google Calendar connector lifecycle hardening.

Implemented before screenshot finalization:

- configuration, connection, token-cache and lifecycle visibility
- persisted non-secret last-attempt and last-success metadata
- last bounded range and result counts
- explicit empty, partial, failed, expired/revoked and missing-token states
- sanitized last-error persistence
- explicit reconnect and manual retry
- clarified disconnect behavior
- explicit local connector cache clearing
- connector lifecycle actions in the existing integration audit trail
- imported Integration Inbox evidence retained after disconnect/cache clearing

## Important boundary

No calendar write, Gmail, Outlook, inbox scanning, scheduled refresh, startup refresh, background polling, automatic retry, automatic reconnect, automatic preview acceptance, automatic linking, automatic LifeOS module mutation, or AI action is active.

External records remain untrusted by default. Accepted previews still require a separate deliberate target-module handoff.

## Credential and local-data boundary

Google client configuration and OAuth tokens remain local to the user profile and are not committed. Lifecycle metadata contains no raw tokens, client secret, authorization code, callback URL, private provider payload, or private event payload solely for diagnostics.

## Next checkpoint

Run Group 23 validation, capture only approved lifecycle screenshots, then finalize screenshot/docs Pack 2. Group 24 has not started.

<!-- GROUP23_CONNECTOR_LIFECYCLE_START -->
## Group 23 complete â€” connector lifecycle hardening

The Google Calendar read-only connector now exposes configuration, connection, token-cache and refresh lifecycle state. It records the last selected range, last attempt, last successful refresh, received/duplicate/skipped counts and a sanitized last error.

Manual recovery controls include reconnect, retry last bounded refresh, disconnect local authorization and clear local connector cache. Imported Integration Inbox evidence remains after disconnect and cache clearing.

Verified visible states include successful connection, successful refresh, empty result, sanitized network failure, disconnected/reconnect state and final cache-cleared state. No scheduled refresh, background polling, automatic retry, automatic reconnect, second provider, calendar write, Email Radar or Group 24 work is active.
<!-- GROUP23_CONNECTOR_LIFECYCLE_END -->

## Group 24 active boundary

Email Radar is local/imported evidence only. No Gmail or Outlook mailbox is connected. No message is sent, moved, deleted, or labelled. No background scanning, AI interpretation, automatic Follow-Up creation, or automatic Work Pipeline mutation is active. Group 25 has not started.

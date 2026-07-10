# LifeOS Current Status

## Current release

LifeOS Desktop v5.0-alpha is the current product state.

## Current state

The v4 operating spine remains complete. The v5.0-alpha connector foundation includes guarded local CSV, JSON, and ICS imports plus one narrow authenticated Google Calendar read-only connector.

Google Calendar connection is explicit and requests only `calendar.readonly`. Refresh occurs only after the user chooses and confirms a bounded date range, with a maximum of 31 days. Retrieved events enter the existing Integration Inbox as untrusted read-only previews. Provenance, stable external references, duplicate suspicion, audit history, human acceptance, and separate target-module handoff remain mandatory.

## Current completed build group

Group 22 — Google Calendar read-only connector.

The verified live evidence demonstrates:

- successful local OAuth connection to a real test account
- read-only Calendar scope
- manual bounded refresh
- five source-backed event previews received
- zero malformed events in the verified refresh
- normal Integration Inbox review-first intake
- repeated-refresh duplicate suspicion
- local token-cache deletion
- final disconnected state with previews retained

Evidence is stored in:

`docs/screenshot-groups/group-22-google-calendar-read-only/`

## Important boundary

No calendar write, Gmail, Outlook, Microsoft Calendar, inbox scanning, bank feed, scheduled refresh, startup refresh, background polling, automatic retry loop, automatic preview acceptance, automatic linking, automatic LifeOS module mutation, or AI action is active.

External records remain untrusted by default. Accepted previews still require a separate deliberate target-module handoff.

## Credential and local-data boundary

Google client configuration and OAuth tokens remain local to the user profile and are not committed. The token cache is protected locally and can be explicitly deleted through disconnect. Documentation screenshots contain no provider credentials or private account identifiers.

## Next lane

Return control to Master Roadmap after the Group 22 screenshot/documentation checkpoint. Group 23 has not started.

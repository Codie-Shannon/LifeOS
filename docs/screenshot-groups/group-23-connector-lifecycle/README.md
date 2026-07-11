# Group 23 — Google Calendar connector lifecycle evidence

LifeOS v5.0-alpha hardens the existing read-only Google Calendar connector into a visible, diagnosable, recoverable lifecycle.

## Verified lifecycle evidence

1. Connection succeeds with read-only authorization and refresh remains manual.
2. Connected state shows configuration validity and local token-cache presence.
3. Manual bounded refresh returns review-first Integration Inbox previews.
4. Last refresh attempt, last successful refresh, selected date range and result counts are visible.
5. A naturally empty date range returns zero records without becoming an error.
6. A network failure is sanitized and leaves retry manual.
7. Disconnect removes local authorization while retaining imported preview evidence.
8. Configured-but-disconnected state exposes explicit reconnect controls.
9. Clearing local connector cache removes token/lifecycle metadata only.
10. Imported Integration Inbox evidence remains after disconnect and cache clearing.

## Safety boundary

- `calendar.readonly` only
- manual refresh only
- maximum 31-day range
- no scheduled or background refresh
- no automatic retry or reconnect
- no calendar writes
- no automatic LifeOS module mutation
- imported records remain untrusted until deliberate human review

## Screenshots

| # | File | Evidence |
|---|---|---|
| 1 | `screenshots/group23-01-connect-success.png` | Explicit successful local connection |
| 2 | `screenshots/group23-02-connected-lifecycle.png` | Connected lifecycle status, valid configuration, token cache present |
| 3 | `screenshots/group23-03-refresh-success.png` | Successful bounded manual refresh result |
| 4 | `screenshots/group23-04-refresh-success-metrics.png` | Last attempt, last success, counts and selected range |
| 5 | `screenshots/group23-05-empty-refresh.png` | Safe zero-event result |
| 6 | `screenshots/group23-06-network-failure.png` | Sanitized network failure |
| 7 | `screenshots/group23-07-disconnect-confirmation.png` | Disconnect retains Integration Inbox evidence |
| 8 | `screenshots/group23-08-disconnect-complete.png` | Local authorization removed |
| 9 | `screenshots/group23-09-disconnected-reconnect-state.png` | Explicit disconnected/reconnect state |
| 10 | `screenshots/group23-10-clear-cache-confirmation.png` | Cache-clear boundary explained |
| 11 | `screenshots/group23-11-clear-cache-complete.png` | Cache clear completed, previews retained |
| 12 | `screenshots/group23-12-cache-cleared-state.png` | Final cache-cleared lifecycle state |

Group 24 has not started.

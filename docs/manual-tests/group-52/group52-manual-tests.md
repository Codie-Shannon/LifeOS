# Group 52 manual tests — Full Mobile foundation

Use fictional or sanitized data only. Full Mobile is a separate application from Desktop and Mobile Companion.

1. Launch `LifeOS.Mobile` and verify loading resolves to the ready Home surface without exposing secrets.
2. Verify Home, Work, Money, Projects and More are reachable from the bottom navigation.
3. Open More and verify Career, Life, Assistant and Settings are reachable.
4. Open Settings and verify theme, accent, density, sensitive-preview and app-lock controls are visible.
5. Verify the Home freshness and pending-queue summary remains clear while offline.
6. Activate Emergency Stop and verify diagnostics report `Stopped`; clear it only through Settings.
7. Open Diagnostics and verify tokens, email addresses, local keys and raw provider payloads are absent.
8. Confirm touch targets, text scaling and screen-reader labels remain usable.

Pack 1 does not commit screenshots and does not implement full workspace scope.

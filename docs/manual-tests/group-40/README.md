# Group 40 Website foundation — manual verification

Status before screenshots: **Pack 1 implementation complete; manual browser review required.**

## Scope

This group builds the static-first public Website foundation only. It does not include screenshots, public downloads, authentication, a portal, mass documentation migration, Desktop redesign or Full Mobile.

## Local launch

```powershell
dotnet run --project .\src\LifeOS.Website\LifeOS.Website.csproj -c Release
```

Use the localhost URL printed by the development server.

## Required browser checks

1. Open `/` with the browser in light preference. Confirm the approved message, three audience paths and no Download control.
2. Clear the LifeOS theme override, switch browser preference to dark and reload. Confirm dark theme is selected on first load.
3. Use the Theme control twice. Confirm the override persists across reloads and both themes remain readable.
4. Review the accent-token implementation. The Pack 1 default is purple; final accent remains subject to screenshot review against blue and teal candidates.
5. Navigate every primary route and all Product, Solutions and Docs child routes.
6. At a narrow browser width, verify the mobile menu opens, closes, is keyboard reachable and does not hide the Theme control.
7. Verify Desktop and Companion show beta complete and Full Mobile shows Planned.
8. Verify Evidence states that public identities are fictional or sanitized.
9. Verify Safety and Trust explains local-first, review-first and fail-closed boundaries.
10. Verify Early Access is disabled, does not submit, says no data is collected and links to Privacy.
11. Verify Privacy states no analytics, non-essential cookies, tracking, login, portal or collection in Group 40.
12. Search Docs for `safety`, `desktop state` and `release`; confirm deterministic local results.
13. Keyboard through the header, mobile navigation, page links, theme control and footer. Confirm visible focus and skip-to-content behavior.
14. Enable reduced-motion preference and confirm the interface remains usable without essential motion.
15. Inspect browser console and network activity for unexpected errors or tracking requests.

## Pack 2 boundary

Do not add screenshots during Pack 1. Pack 2 must contain exactly eight approved screenshots and final Group 40 documentation.

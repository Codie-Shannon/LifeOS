# Group 48 manual verification — Microsoft Mail and Calendar

Group 48 creates and closes the live read-only Microsoft provider boundary at `v9.0.0-alpha.3`.

## Local-only setup boundary

The Entra application uses the **Mobile and desktop applications** public-client boundary with the loopback redirect:

```text
http://localhost:53682/
```

The accepted registration name used during proof was:

```text
LifeOS Mail Calendar Connector
```

No client secret is created or stored. Public configuration, provider state and DPAPI-protected tokens remain under `%LOCALAPPDATA%\LifeOS` and outside Git.

## Verified manual checks

1. Microsoft Provider Setup opens inside the existing Settings shell.
2. Two identities can remain connected and separately classified as Personal and Work.
3. Displayed identities are masked in provider setup.
4. Consent requests only identity/offline access, `Mail.Read` and `Calendars.Read`.
5. Outlook Mail and Calendar show Granted; later Microsoft capabilities remain Not Requested.
6. Bounded foreground Outlook Mail sync creates source-backed Message candidates in Integration Inbox.
7. Imported messages remain untrusted until reviewed.
8. Provider, account, source/import timestamps, freshness and normalized fields remain visible.
9. Full provider payloads are not stored or displayed unnecessarily.
10. Attachment filename, MIME type and size are normalized without automatic attachment-body download.
11. A personal Microsoft Calendar event imports as a reviewable Calendar Event candidate.
12. Calendar title, start, end, time zone, location, organizer, response state and recurrence reference are represented.
13. Source-backed mail creates a separate reviewable follow-up suggestion rather than silently creating authoritative work.
14. An intentionally invalid calendar selection produces a visible partial failure while Mail still succeeds.
15. The invalid calendar selection can be removed and the default calendar restored.
16. The final clean sync reports `50 mail`, `1 calendar`, `44 suggestions`, Connected and no current provider error.
17. Background synchronization remains disabled.
18. No Mail send, Mail read/write or Calendar read/write scope/action exists.
19. Exactly eight ordered Group 48 screenshots are committed.
20. Full email addresses and opaque provider IDs/references are redacted in committed evidence.
21. The repository was synchronized and clean before Pack 2 finalization.

## Evidence sequence

1. Connected Microsoft accounts.
2. Capability and incremental permission catalogue.
3. Outlook Mail candidates.
4. Message provenance and attachment metadata.
5. Microsoft Calendar candidates.
6. Reviewable mail suggestion.
7. Recovery and partial-permission state.
8. Group 48 validation and clean synchronization.

## Closure

Group 48 Pack 2 reruns the complete Core/Desktop, Website and Companion regression/build boundary, NuGet vulnerability scans, Gitleaks, repository hygiene, evidence validation and clean synchronization verification before committing the evidence.

Group 48 is complete and closed. Group 49 has not started.

## Stop rule

Do not commit local provider configuration, state, tokens, consent callbacks, message bodies or attachment bodies. Do not add background polling, silent trust or external write actions. Do not begin Group 49 until this Pack 2 commit is pushed and verified.

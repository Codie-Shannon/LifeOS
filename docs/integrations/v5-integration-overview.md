# LifeOS v5 Integration Overview

## Release checkpoint

LifeOS `v5.0.0-beta.1` is the first coherent v5 integration beta checkpoint. It is not a release candidate or stable production release.

## Trust model

```text
external provider record
-> authenticated or imported read-only result
-> untrusted provider-neutral preview/evidence
-> duplicate detection and provenance
-> source-backed human review
-> explicit confirmation
-> confirmed timeline or accepted preview
-> trusted LifeOS state
```

Authentication never implies trust. Acceptance and operational linking remain separate.

## Active connector lanes

| Lane | Authentication | Operation | Scope | Review |
|---|---|---|---|---|
| Manual CSV / JSON | None | Explicit preview import | Local file only | Required |
| Local ICS | None | Explicit bounded calendar import | Local file only | Required |
| Google Calendar | Desktop OAuth | Manual bounded refresh | `calendar.readonly` | Required |
| Local Email Radar import | None | Explicit JSON / CSV evidence import | Local file only | Required |
| Gmail | Desktop OAuth | Manual profile-bound bounded search | `gmail.readonly` | Required |

## Global safety boundary

No connector changes an external system or LifeOS operational module automatically.

There are no write scopes, email-send actions, calendar-write actions, background scans, scheduled refreshes, startup refreshes, automatic retry loops, active HTML rendering, remote-image loading, attachment downloads, automatic Follow-Ups, automatic Work Pipeline mutations, or AI interpretation.

## Duplicate, provenance and audit

Local and provider-backed imports retain stable source references, duplicate identity, review state, timestamps, provenance and sanitized audit summaries. Duplicate suspicion remains visible and blocks trusted acceptance.

## Disconnect, cache clear and evidence retention

Disconnect removes local authorization. Connector cache clear removes authorization and local lifecycle metadata. Neither operation deletes retained imported previews or communication evidence.

Deleting retained evidence remains a separate deliberate action and is not bundled into connector lifecycle controls.

## Local configuration

Google provider configuration and protected tokens remain under `%LOCALAPPDATA%\LifeOS\connectors\...` and must never be committed. The repository ignores common local connector credential, token, lifecycle and private-evidence file patterns.

## Known limitations

- one local desktop user
- private/testing-mode Google OAuth
- one Google Calendar source
- one Gmail account
- manual bounded operations only
- no Outlook or second email provider
- no background or scheduled operations
- deterministic matching only
- no mailbox or calendar mutation
- no attachment download or HTML mail reader
- no automatic operational mutation
- major UI redesign deferred

## Next roadmap boundary

Group 27 and v6 controlled automation have not started.

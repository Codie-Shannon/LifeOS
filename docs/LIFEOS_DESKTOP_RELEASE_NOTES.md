# LifeOS Desktop v25.0.0-alpha.1 Release Notes

LifeOS Desktop v25 completes Privacy, Export, Backup and Controls across
Groups 108-111.

## Release theme

**Explicit permission and recoverable control**

## Added

- Independently permissioned sensitive categories.
- Sanitized crash-report controls with 90-day default retention.
- Credential-rejecting, SHA-256 integrity-checked backups.
- Explicit restore previews and schema validation.
- Audited global Emergency Stop.
- Reachable Privacy, Backup & Emergency Control module in Settings.

## Safety boundaries

- A timestamped grant is required for sensitive-category access.
- Credential-like fields cannot enter backup payloads.
- Restore is a separate reviewed action.
- Clearing Emergency Stop does not reconnect providers automatically.

## Validation

- 411 Core tests passed.
- 54 Mobile tests passed.
- Desktop and Android Release builds completed cleanly.
- Official evidence is in
  [`screenshot-groups/group-108-111-privacy-export-backup-controls`](screenshot-groups/group-108-111-privacy-export-backup-controls/).

## Next

Group 112 begins v26 Website Packaging and Onboarding.

# Groups 73-76 - Guarded Provider Contracts

- Screenshot group: SG-76
- Release: v15 Guarded Integrations
- Version: 15.0.0-beta.1
- Development commit: `5d1d2f3`

## Scope

SG-76 demonstrates the guarded provider registry, source-backed preview
boundary, duplicate and conflict review, permission gates, auditable decisions
and fail-closed Emergency Stop behaviour.

Provider data remains untrusted until reviewed. External writes are disabled
by default, no provider credentials are represented in the demonstration
records, and Emergency Stop takes precedence.

## Screenshots

1. `01-desktop-assistant-workspace.png`
   - Navigation: Assistant
   - Shows the permanent review workspace and SG-76-ready provider module.
2. `02-desktop-guarded-providers-overview.png`
   - Navigation: Assistant > Guarded Provider Contracts
   - Shows the SG-76 boundary, providers, review queue, duplicates and zero external writes.
3. `03-desktop-guarded-provider-records.png`
   - Navigation: Assistant > Guarded Provider Contracts
   - Shows New, Conflict, Setup later and Emergency Stop provider records.
4. `04-desktop-integration-inbox.png`
   - Navigation: Assistant > Integration Inbox
   - Shows the read-only current-account boundary and an empty completed queue.
5. `05-desktop-automation-safety-overview.png`
   - Navigation: Settings > Automation Centre
   - Shows guarded, foreground-only automation readiness and provider-write restrictions.
6. `06-desktop-automation-emergency-stop.png`
   - Navigation: Settings > Automation Centre
   - Shows the available Emergency Stop control and fail-closed readiness checks.
7. `07-desktop-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v15.0.0-beta.1 and v15 Guarded Integrations.
8. `08-sg76-validation.png`
   - Shows 367 Core tests passing, the successful Desktop Release build,
     zero warnings, zero errors, Git diff validation and commit `5d1d2f3`.

## Verification

- Core tests: 367 passed, 0 failed
- Desktop Release build: succeeded
- Build warnings: 0
- Build errors: 0
- Git diff check: passed
- Private-data review: passed
- External writes shown: 0
- Mobile screenshots: not applicable to this Desktop/shared-core pack
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

# Groups 108-111 - Privacy, Export, Backup and Controls

- Screenshot group: SG-78
- Release: v25 Privacy Control Plane
- Version: 25.0.0-alpha.1
- Development commit: `6f52229`

## Scope

SG-78 demonstrates independently permissioned sensitive categories,
credential-free integrity-checked backup and restore previews, sanitized crash
report controls and a global Emergency Stop that disconnects providers while
preserving an audit and explicit reconnection boundary.

## Screenshots

1. `01-desktop-privacy-control-navigation.png` - reachable Settings route.
2. `02-desktop-privacy-control-overview.png` - SG-78 boundary and metrics.
3. `03-desktop-privacy-control-records.png` - permissions, restore and stop state.
4. `04-desktop-v25-release-version.png` - v25 release identity.
5. `05-group78-validation.png` - complete validation proof.

## Verification

- SG-78 targeted tests: 8 passed, 0 failed
- Core tests: 411 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Android Release build: succeeded
- Credential guard: passed
- Backup integrity: passed
- Git diff check: passed
- Private-data review: passed
- Mobile feature screenshots: not applicable; SG-78 only aligns Mobile version
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

# Groups 80-82 - Closed Beta Baseline

- Screenshot group: SG-71
- Release: v18 Closed Beta Baseline
- Version: 18.0.0-beta.1
- Development commit: `6dab161`

## Scope

SG-71 demonstrates the private-beta setup and release-readiness boundary
across Desktop and the Full Mobile app. Core local use remains available
without external providers, while optional integrations can be configured
now, later or declined and changed from Settings.

The evidence also verifies the signed Android private-beta artifact, the
connected test device and the installed v18 release.

## Screenshots

1. `01-desktop-settings-beta-navigation.png`
   - Navigation: Settings > About / Diagnostics
   - Shows the reachable Private Beta Readiness control and v18 version.
2. `02-desktop-beta-readiness-overview.png`
   - Navigation: Settings > Private Beta Readiness
   - Shows the SG-71 boundary, local modules, optional providers, platforms
     and seven closed-beta release checks.
3. `03-desktop-beta-readiness-records.png`
   - Navigation: Settings > Private Beta Readiness
   - Shows local setup, declined optional AI, signed beta distribution and
     the explicit baseline-closure review.
4. `04-mobile-more-beta-navigation.png`
   - Navigation: Full Mobile > More
   - Shows the device-native Private beta readiness entry.
5. `05-mobile-private-beta-overview.jpg`
   - Navigation: Full Mobile > More > Private beta readiness
   - Shows local readiness and now/later/decline setup choices.
6. `06-mobile-private-beta-details.jpg`
   - Navigation: Full Mobile > More > Private beta readiness, lower page
   - Shows optional-provider, AI and sanitized crash-report boundaries.
7. `07-desktop-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v18.0.0-beta.1 and v18 Closed Beta Baseline.
8. `08-group71-validation.png`
   - Shows 375 Core tests and 54 Mobile tests passing, a clean Desktop
     Release build, signed Android APK verification, connected v18 device,
     Git diff validation and commit `6dab161`.

## Verification

- Core tests: 375 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Desktop warnings: 0
- Desktop errors: 0
- Android Release: signed APK verified
- Android beta device: connected
- Installed phone version: 18.0.0-beta.1
- Git diff check: passed
- Private-data review: passed
- Website screenshots: not applicable to this private-beta pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

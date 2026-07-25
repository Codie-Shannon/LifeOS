# Groups 83-86 - Native Intelligence and Optional AI

- Screenshot group: SG-72
- Release: v19 Native Intelligence
- Version: 19.0.0-alpha.1
- Development commit: `41dbc2b`

## Scope

SG-72 demonstrates deterministic, offline-capable native suggestions and the
strict boundary around optional external AI. External enrichment is
category-scoped, cost-controlled and review-first; it cannot mutate LifeOS
directly.

The complete suggestion set is intentionally presented on one review surface.
Five distinct captures document the actual interface without manufacturing
duplicate or impossible per-record screenshots.

## Screenshots

1. `01-desktop-assistant-intelligence-navigation.png`
   - Navigation: Assistant
   - Shows the reachable Native Intelligence & Optional AI module.
2. `02-desktop-intelligence-overview.png`
   - Navigation: Assistant > Native Intelligence & Optional AI
   - Shows the SG-72 boundary, native suggestion count, Ask mode, monthly cap,
     review count and the first source-backed suggestions.
3. `03-desktop-intelligence-review-records.png`
   - Navigation: Assistant > Native Intelligence & Optional AI, records
   - Shows native high-confidence ranking, ask-first OpenAI enrichment,
     blocked sensitive health data and configured Off/Ask/Capped controls.
4. `04-desktop-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v19.0.0-alpha.1 and v19 Native Intelligence.
5. `05-group72-validation.png`
   - Shows 381 Core tests and 54 Mobile tests passing, a clean Desktop
     Release build, signed APK/AAB verification, Git diff validation and
     commit `41dbc2b`.

## Verification

- Core tests: 381 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Desktop warnings: 0
- Desktop errors: 0
- Android Release: signed APK verified
- Android bundle: signed AAB verified
- Git diff check: passed
- Private-data review: passed
- Mobile feature screenshots: not applicable; SG-72 adds no Mobile UI surface
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

# Groups 87-90 - Scheduled Communications

- Screenshot group: SG-73
- Release: v20 Scheduled Communications
- Version: 20.0.0-alpha.1
- Development commit: `b95a0dd`

## Scope

SG-73 demonstrates scheduled SMS, Gmail and Outlook communication drafts with
explicit approval, quiet-hours enforcement, approval revocation after edits
or rescheduling, auditable send state and an Emergency Stop that blocks every
channel.

No message is sent automatically merely because it was drafted or scheduled.

## Screenshots

1. `01-desktop-assistant-communications-navigation.png`
   - Navigation: Assistant
   - Shows the reachable Scheduled Communications module.
2. `02-desktop-scheduled-communications-overview.png`
   - Navigation: Assistant > Scheduled Communications
   - Shows the SG-73 boundary, draft and approval counts, local quiet hours,
     zero automatic sends and the first communication records.
3. `03-desktop-communication-review-records.png`
   - Navigation: Assistant > Scheduled Communications, records
   - Shows SMS Draft, Outlook Approved, Gmail Needs review after revoked
     approval, and the ready Communication Emergency Stop.
4. `04-desktop-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v20.0.0-alpha.1 and v20 Scheduled Communications.
5. `05-group73-validation.png`
   - Shows 386 Core tests and 54 Mobile tests passing, a clean Desktop
     Release build, signed APK/AAB verification, Git diff validation and
     commit `b95a0dd`.

## Verification

- Core tests: 386 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Desktop Release build: succeeded
- Desktop warnings: 0
- Desktop errors: 0
- Android Release: signed APK verified
- Android bundle: signed AAB verified
- Git diff check: passed
- Private-data review: passed
- Automatic messages shown: 0
- Mobile feature screenshots: not applicable; SG-73 adds no Mobile UI surface
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

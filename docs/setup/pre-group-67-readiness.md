# Pre-Group-67 readiness baseline

Baseline recorded: 2026-07-25 (Pacific/Auckland)

## Product and release identity

- Public product name: Life Control OS
- Desktop release: 13.0.0-alpha.1
- Android application ID: `nz.codieshannon.lifeos.mobile`
- Android release: 13.0.0-alpha.1
- Distribution stage: private beta

## Connected-service setup

- Dedicated Google testing identity configured with account recovery and
  multi-factor authentication.
- Google OAuth remains in testing mode with bounded read-only Gmail, Calendar,
  Drive, Contacts and Tasks scopes.
- Dedicated Microsoft testing identity configured with account recovery and
  multi-factor authentication.
- Microsoft identity uses delegated read-only mail and calendar permissions.
- Connector credentials and token caches are stored outside the repository.

## Crash-reporting setup

- Separate Sentry projects exist for desktop and Android.
- Server-side data scrubbing and default scrubbers are enabled.
- IP address storage is disabled.
- Crash reporting is disabled by default and requires explicit user opt-in.
- Default PII transmission is disabled.
- Sentry DSNs are stored outside the repository under the local LifeOS
  application-data directory.
- Logs, tracing, profiling, replay and metrics remain disabled.

## Android private-beta setup

- Android SDK platforms 33, 35 and 36 are installed.
- A permanent release keystore is stored outside the repository with an
  encrypted backup.
- The signed private-beta APK has been verified against the permanent signing
  certificate.
- The Samsung Note 9 private-beta device is authorized for Android debugging.
- The Visual Studio Android emulator is configured and can be started when
  required.

## Verification result

- Full Release test suite: 470 passed, 0 failed, 0 skipped.
- Desktop Release build: succeeded with 0 warnings and 0 errors.
- Mobile tests: included in the full suite, 54 passed.
- Signed Android APK SHA-256:
  `467E1CB32C1428CF9794D4F554D72624D1944A73B5AF0A9787FFE690CFB2C4FE`
- Android signing certificate SHA-1:
  `83:67:71:26:F3:77:58:B5:81:BC:D0:36:CC:CF:1E:69:8C:4B:94:81`
- Android signing certificate SHA-256:
  `40:6B:6A:19:C5:53:1B:9F:53:62:1F:0F:E7:0A:3E:77:D7:21:F4:96:B2:BA:48:AB:00:9A:CA:82:6D:69:1E:41`

## Deferred until later product stages

- Company incorporation and final legal entity details
- New Zealand product domains
- Microsoft Store registration and production submission
- Public website deployment
- Production accounting, grocery-pricing and AI-provider credentials
- SMS feature implementation and permission flow
- Expansion to the remaining private-beta devices

This baseline completes the external setup required before Group 67. New
provider permissions must continue to be added only when a feature requires
them, using the least-privilege and explicit-consent rules.

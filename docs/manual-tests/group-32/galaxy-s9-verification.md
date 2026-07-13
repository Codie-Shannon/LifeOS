# Group 32 Galaxy S9 manual verification

Use fictional data only. Do not expose full ADB serials in screenshots or committed files.

## Environment record

- Date:
- Windows version:
- Visual Studio version:
- .NET SDK:
- Installed MAUI workload:
- Android SDK/build tools:
- Java/JDK:
- Galaxy S9 Android version:
- Galaxy S9 API level (`adb shell getprop ro.build.version.sdk`):
- ADB model (`adb shell getprop ro.product.model`):

## Required checks

- [ ] `adb devices` shows one authorized physical Galaxy S9.
- [ ] `dotnet build src/LifeOS.Companion/LifeOS.Companion.csproj -c Debug -f net10.0-android` passes.
- [ ] App installs and launches on the Galaxy S9.
- [ ] Product identity is LifeOS Mobile Companion `v0.1.0-alpha.1`.
- [ ] No account, OAuth, or desktop connection is required.
- [ ] Empty capture is rejected visibly.
- [ ] Fictional Quick Capture saves locally.
- [ ] Saved item appears in capture list and Pending outbox.
- [ ] NotPaired status remains honest.
- [ ] Force-close/reopen preserves capture and outbox.
- [ ] Phone restart preserves capture and outbox, if practical.
- [ ] Editing preserves CaptureId and CreatedAt.
- [ ] Wi-Fi-off capture works.
- [ ] Restoring Wi-Fi does not automatically send.
- [ ] Logs/screenshots contain no sensitive data.
- [ ] Desktop tests and Companion tests pass after physical-device proof.

## Result

Status: NOT YET VERIFIED

Do not mark Group 32 complete until all required physical-device checks and screenshot evidence are committed.

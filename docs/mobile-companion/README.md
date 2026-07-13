# LifeOS Mobile Companion

LifeOS Mobile Companion is a separate lightweight product, not the full LifeOS Mobile application and not a desktop replacement.

## Group 32 scope

Version: `v0.1.0-alpha.1`

Android-first local foundation:

1. Quick Capture validates meaningful content.
2. Capture fields are protected with AES-GCM.
3. The AES key is generated randomly and stored through Android SecureStorage.
4. SQLite persists captures, schema revision, local device identity, and outbox ordering.
5. Desktop-bound captures enter an honest `Pending` state.
6. No automatic sender, background timer, cloud account, OAuth, or operational desktop transfer exists.

The local device identifier is random and does not use IMEI, phone number, advertising ID, or hardware serial.

## Security boundary

This implementation provides field-level protection for title, body, and category. Metadata needed for local ordering and state remains in SQLite. The database and debug build are test-stage artifacts; Group 32 uses fictional data only. Production signing material is not created.

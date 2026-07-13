# Group 33 final evidence

## Release
- Desktop baseline: v6.0.0-beta.1
- Mobile Companion: v0.1.0-alpha.2
- Proof device: Samsung Galaxy S9
- Transport: explicit same-LAN manual pairing and transfer

## Verified manual proof
- Desktop receiving was explicitly enabled.
- Galaxy S9 paired using a short-lived one-time code and matching verification.
- A fictional Quick Capture moved from Pending to Delivered only after Desktop acknowledgement.
- Desktop persisted the intake as untrusted `NeedsReview` evidence.
- Confirming evidence retained provenance and did not create an operational module mutation.
- Resending the same capture returned `Duplicate` and did not create a second Desktop intake.
- An interrupted Wi-Fi transfer failed visibly and succeeded only after manual retry.
- Desktop revocation blocked the next transfer with HTTP 401 while previously received evidence remained.

## Evidence set
1. S9 Pending capture.
2. S9 Delivered — AcceptedForReview.
3. Desktop NeedsReview intake.
4. Desktop Confirmed evidence.
5. S9 Delivered — Duplicate.
6. S9 Failed — Connection failure.
7. S9 manual retry Delivered.
8. S9 revocation blocked — 401 Unauthorized.

No pairing secret, full private endpoint, one-time code, verification code, APK, database, or device credential is committed.

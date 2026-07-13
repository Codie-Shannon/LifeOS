# Group 34 — Mobile Companion beta manual verification

Proof device: Samsung Galaxy S9. Companion: `v0.1.0-beta.1`. Desktop remains `v6.0.0-beta.1`.

## Required run
1. Install and launch over the existing alpha.2 data. Confirm pairing/outbox migration and restart normalization.
2. Refresh Agenda/Waiting-on glance explicitly. Record source, last-updated, read-only and stale wording.
3. Disable Wi-Fi. Confirm cached glance says stale/offline. Restore Wi-Fi and confirm no queued capture sends.
4. Fail a transfer, then use **Retry manually**. Delivery requires a verified acknowledgement.
5. Confirm notifications begin disabled. Test denied permission, explicit opt-in and privacy-safe lock-screen text.
6. Re-send a CaptureId with changed content. Confirm conflict choices; no silent overwrite. Complete one explicit choice and verify audit state.
7. Reset phone pairing. Pending captures must remain. Re-pair using a fresh code/verification. Revoke from Desktop and confirm future transfer is blocked.
8. Check large font/display scaling, touch targets, semantic labels, dark theme contrast and status text independent of colour.
9. Run Pack 1 runner final validations. Record exact Desktop/Companion/combined totals and all build/scan results.

## Screenshot gate
Do not capture Group 33 evidence again. Group 34 Pack 2 accepts exactly the eight approved screenshots listed in `docs/screenshot-groups/group-34-companion-beta-checkpoint/README.md`.

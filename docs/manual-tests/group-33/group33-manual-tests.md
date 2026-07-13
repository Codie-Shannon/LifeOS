# Group 33 manual verification

1. Put the Windows Desktop and Galaxy S9 on the same trusted LAN.
2. Open Integration Inbox. Confirm **Receiving disabled by default**.
3. Select **Pair Companion**. Note the masked local endpoint, six-digit code and expiry.
4. If Windows blocks the listener, stop and run the pack's optional firewall command only after reviewing it.
5. On the S9, open **Pair**, enter the endpoint and code, then verify the matching six-digit verification value and confirm.
6. Restart both apps and confirm the phone still retains the pairing credential.
7. Create fictional Quick Capture data. Open Outbox and select **Send manually**.
8. Observe Pending → Sending → Delivered only after the Desktop acknowledgement.
9. Confirm Desktop persists the item in the Mobile Companion lane as **NeedsReview**.
10. Confirm or reject it. Verify no task, follow-up, agenda, project, payment, email or calendar module changed.
11. Resend the same capture and verify Desktop count does not increase; phone may receive Duplicate and remain Delivered.
12. Turn off Wi-Fi during another send. Verify Failed, never Delivered, then use explicit manual send again.
13. Revoke paired devices on Desktop and verify future sends are blocked while existing intake remains.

## Pack 2 evidence completed
- Eight approved screenshots committed.
- Pairing, acknowledgement, duplicate, retry and revocation verified on Galaxy S9.

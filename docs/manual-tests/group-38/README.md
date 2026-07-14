# Group 38 manual verification

Use fictional data only. Launch Desktop `v7.0.0-alpha.4` and open **Memory**.

1. Verify the workspace states **no automatic retention** and shows permissions.
2. Create a fictional proposed Project-scoped memory, edit the exact text, set expiry and sensitivity, then preview and confirm.
3. Verify the record shows origin, confirmer, timestamp, scope, sensitivity, expiry and usage audit.
4. Ask a relevant Assistant question and verify memory appears separately under **Memory used**.
5. Ask an unrelated question and verify memory is excluded.
6. Create duplicate/conflicting candidates and verify they are disclosed before saving.
7. Revoke a memory and verify immediate retrieval exclusion.
8. Delete it and verify the audit tombstone/source provenance remains.
9. Verify a secret-bearing statement is rejected and cancellation creates no record.
10. Restart Desktop and verify safe local protected persistence.
11. Run all tests, Release builds, hygiene, NuGet scan, Gitleaks and `git diff --check`.

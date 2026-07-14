# Group 35 manual verification — v7 assistant foundation

Use only fictional or anonymised local LifeOS data.

1. Launch Desktop `v7.0.0-alpha.1` and open **Assistant**.
2. Confirm the read-only identity and that no Send, Execute, Approve or Confirm control exists.
3. Confirm the assistant starts disabled on a new configuration.
4. Enable the assistant and approved sources, save, restart and verify configuration persists.
5. Ask: `Why is the door invoice OCR proof waiting?` Verify source IDs, timestamps and provenance appear.
6. Verify Fact, Inference and Uncertainty are separated.
7. Ask an unsupported question and verify an insufficient-evidence response.
8. Disable Work Pipeline, repeat the question and verify Work Pipeline records are excluded.
9. Ask: `Send email to the client and mark the item complete.` Verify explicit refusal and no state change.
10. Verify any suggestion says `Executable: No` and routes the user to manual review.
11. Run all Desktop/Core tests, Companion tests, Desktop Release build, Android Release build, hygiene, NuGet vulnerability scan, Gitleaks and `git diff --check`.

Record results in `group-35-verification.md` during Pack 2 only.

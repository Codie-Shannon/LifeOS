# LifeOS Desktop v24.0.0-alpha.1 Release Notes

LifeOS Desktop v24 completes Evidence Automation and Proof Tooling across
Groups 104-107.

## Release theme

**Repeatable proof without altering originals**

## Added

- Old-evidence exclusion during screenshot intake.
- Stable group-prefixed screenshot naming.
- Copy-only evidence export.
- Evidence PDF generation from approved PNG files.
- Fail-closed completion gates.
- Reachable Evidence Automation module in Settings.

## Safety boundaries

- Original evidence is never renamed or overwritten.
- Empty or incomplete proof cannot pass the completion gate.
- Repository export reports zero canonical mutations.
- Generated PDFs remain derivative evidence.

## Validation

- 403 Core tests passed.
- 54 Mobile tests passed.
- Desktop and Android Release builds completed cleanly.
- Evidence PDF smoke test completed successfully.
- Official evidence is in
  [`screenshot-groups/group-104-107-evidence-automation-proof-tooling`](screenshot-groups/group-104-107-evidence-automation-proof-tooling/).

## Next

Group 108 begins v25 Privacy, Export, Backup and Controls.

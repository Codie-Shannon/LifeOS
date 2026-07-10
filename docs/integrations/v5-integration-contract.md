# LifeOS v5 Integration Contract

## Non-negotiable flow

1. Connector retrieves a narrowly scoped record.
2. LifeOS stores a read-only preview with provenance.
3. Duplicate and trust rules run locally.
4. The user accepts, rejects, or defers the preview.
5. Accepted previews remain inert until an explicit target-module handoff.
6. The target module validates its own rules.
7. Every handoff is auditable and reversible where possible.

## Prohibited by default

- Automatic state mutation
- Automatic payment or invoice state
- Automatic email sending
- Automatic calendar changes
- Automatic project closure
- Automatic OCR trust
- Silent duplicate merging
- Treating expected/imported money as safe
- Broad mailbox or drive access
- Hidden connector failures

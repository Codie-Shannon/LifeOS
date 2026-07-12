# LifeOS v5 Integration Contract

## Non-negotiable flow

1. Connector retrieves a narrowly scoped record.
2. LifeOS stores a read-only `IntegrationPreviewItem` with provenance.
3. Duplicate and trust rules run locally.
4. The user accepts, rejects, or defers the preview.
6. The target module validates its own rules.

## Code contract

- New connector records must enter through `IntegrationPreviewIntake.CreatePreview`.
- Intake-created previews must be read-only, untrusted, and require human review.
- Every preview requires a source label, external reference, title, and deterministic duplicate key.
- Acceptance is blocked for duplicate-suspected, untrusted, or source-evidence-free previews.

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

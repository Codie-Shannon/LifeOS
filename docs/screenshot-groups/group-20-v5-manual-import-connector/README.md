# Group 20 - v5 Manual Import Connector

## Release

LifeOS Desktop v5 manual import connector checkpoint.

## Purpose

Group 20 proves the first real v5 connector path: a local CSV/JSON file can be selected, imported, normalized into read-only Integration Inbox previews, rejected at row level when required fields are missing, and blocked from acceptance until the preview is source-backed and reviewed.

## Screenshot set

1. `01-command-centre-v5-start.png` - Command Centre starting point before the manual connector evidence flow.
2. `02-integration-inbox-top-local-controls.png` - Integration Inbox summary and local controls, including the new CSV/JSON import button.
3. `03-manual-import-file-picker.png` - Local file picker scoped to CSV/JSON preview files.
4. `04-manual-import-success-message.png` - Successful local CSV import through `manual-csv`.
5. `05-review-queue-imported-preview.png` - Imported manual preview records in the review queue.
6. `06-imported-preview-provenance.png` - Imported preview provenance: source label, external reference, duplicate key, source evidence, read-only state, and human review requirement.
7. `07-import-row-error-warning.png` - Bad-row handling: one valid preview imported and one missing-title row skipped.
8. `08-review-gate-warning.png` - Review gate blocks acceptance of an untrusted imported preview.
9. `09-v5-connector-readiness-matrix.png` - Wider v5 connector readiness matrix remains visible after the manual import work.
10. `10-import-preview-confirmation.png` - Pre-save confirmation summary for a JSON import, including connector key, counts, duplicate state, and preview money.
11. `11-json-import-success-message.png` - JSON import success path after explicit confirmation.

## Supporting fixtures

- `docs/integrations/lifeos-v5-manual-import-sample.csv` - clean local CSV used for import success and provenance screenshots.
- `docs/integrations/lifeos-v5-manual-import-bad-row.csv` - mixed valid/invalid CSV used for row-level rejection screenshots.
- `docs/integrations/lifeos-v5-manual-import-sample.json` - clean local JSON used for confirmation and JSON import screenshots.

## Verified behavior

- Manual CSV import runs from a local file picker.
- Imported rows enter as `ManualImport` previews with `manual-csv` source labels.
- Imported previews are `New`, `Untrusted`, read-only, and require human review.
- Duplicate keys are deterministic and visible.
- Source evidence points back to the imported local file and row number.
- Missing title/name/subject/description/summary values are rejected at row level.
- Acceptance is blocked until the preview is source-backed and reviewed.
- Manual imports show a confirmation summary before LifeOS saves previews or writes an audit entry.
- JSON imports use the same preview, confirmation, provenance, and review path as CSV imports.
- No live API, OAuth, external polling, target-module mutation, or external write is active.

## Boundary

This checkpoint proves the v5 connector foundation only. It does not activate Gmail, Outlook, Google Calendar, Microsoft Calendar, Xero, SharePoint, Drive, OCR providers, banking providers, BNPL providers, OAuth, live polling, automatic item creation, automatic updates, or AI actions.

## Next lane

The next v5 connector should reuse the same path: external source -> connector -> `IntegrationPreviewDraft` -> Integration Inbox review -> explicit handoff.

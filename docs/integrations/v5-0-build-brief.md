# LifeOS v5.0 Build Brief - Manual Import Preview Connector

Status: implemented as the first local v5 connector foundation, now extended with the local `.ics` calendar preview connector.

## Purpose

v5.0 should prove the real integration path without OAuth, live polling, or external mutation. The first connector is a narrow manual CSV/JSON import that turns local rows into read-only Integration Inbox previews.

## First Connector

Manual CSV/JSON import, followed by local `.ics` calendar import.


The `.ics` connector proves calendar-shaped data without OAuth or live Google/Outlook access.

## Scope

- Read a user-selected local CSV or JSON file.
- Read a user-selected local `.ics` calendar file.
- Map each row/record into an `IntegrationPreviewDraft`.
- Map each VEVENT into a calendar `IntegrationPreviewDraft`.
- Create previews only through `IntegrationPreviewIntake.CreatePreview`.
- Show a preview confirmation summary before saving imported previews.
- Store previews in the existing Integration Inbox local file.
- Store a manual import audit entry with file identity, hash, imported/skipped counts, row errors, preview IDs, and timestamp.
- Mark incoming previews as duplicate-suspected when their duplicate key already exists or repeats inside the same import batch.
- Show imported records as review-only until the user accepts, defers, rejects, or links them.
- Report row-level import errors without creating partial trusted records.

## Required Fields

- Source label.
- External reference.
- Title.
- Summary or source note when available.
- Suggested target module when known.
- Source evidence pointing to the imported file and row/record identifier.
- Deterministic duplicate key.

The current implementation accepts `title`, `name`, `subject`, `description`, or `summary` as the title source. If an external reference is missing, it creates a deterministic `row-N` reference for the imported file row.

## Prohibited

- No target-module mutation during import.
- No automatic bill, agenda, follow-up, work, proof, or money item creation.
- No expected/imported money counted as safe.
- No OAuth, background polling, live mailbox/calendar/file access, or bank connection.
- No silent duplicate merging.
- No AI action or auto-classification beyond transparent draft mapping.

## Test Plan

- Import record becomes read-only, untrusted, human-review preview.
- Missing source label, external reference, or title is rejected by intake.
- Duplicate key is deterministic for repeated imports.
- Duplicate-suspected previews cannot be accepted.
- Imported money appears as preview value only and never safe money.
- Reject/defer paths preserve audit notes.

## Definition Of Done

- Core tests cover importer mapping, fallback row references, missing title errors, quoted CSV fields, and review gates.
- Manual import creates previews through the shared intake guard.
- Integration Inbox UI imports local `.csv` and `.json` files, shows a pre-save summary, and shows imported previews with provenance.
- Integration Inbox UI imports local `.ics` files through the same confirmation, duplicate, audit, and preview path.
- Integration Inbox UI shows recent manual import audit runs and stores them locally.
- Duplicate reimports are marked `DuplicateSuspected` and remain blocked by the review gate.
- Build is green.
- No live connector or target-module mutation exists.

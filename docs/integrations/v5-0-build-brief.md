# LifeOS v5.0 Build Brief - Manual Import Preview Connector

## Purpose

v5.0 should prove the real integration path without OAuth, live polling, or external mutation. The first connector is a narrow manual CSV/JSON import that turns local rows into read-only Integration Inbox previews.

## First Connector

Manual CSV/JSON import.

This is the safest first connector because it exercises the same intake, provenance, duplicate, review, and handoff rules that later Gmail, Outlook, calendar, accounting, file, OCR, and banking connectors must obey.

## Scope

- Read a user-selected local CSV or JSON file.
- Map each row/record into an `IntegrationPreviewDraft`.
- Create previews only through `IntegrationPreviewIntake.CreatePreview`.
- Store previews in the existing Integration Inbox local file.
- Show imported records as review-only until the user accepts, defers, rejects, or links them.

## Required Fields

- Source label.
- External reference.
- Title.
- Summary or source note when available.
- Suggested target module when known.
- Source evidence pointing to the imported file and row/record identifier.
- Deterministic duplicate key.

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
- Accepted previews remain inert until explicit handoff.
- Imported money appears as preview value only and never safe money.
- Reject/defer paths preserve audit notes.

## Definition Of Done

- Core tests cover the importer mapping and review gates.
- Manual import creates previews through the shared intake guard.
- Integration Inbox UI shows imported previews with provenance.
- Build is green.
- No live connector or target-module mutation exists.

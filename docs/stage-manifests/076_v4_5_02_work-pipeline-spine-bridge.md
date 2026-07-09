# Stage 076 - v4.5 Work Pipeline Spine Bridge

## Purpose

Add shared v4.5 bridge services that explain how Work Pipeline connects to Command Centre, Payment Calendar, Proof Tracker, Paid Work Centre, Follow-Ups, and the item/state spine.

## Adds

- `WorkPipelineOperatingSnapshot`
- `WorkPipelineSpineBridgeService`

## Notes

This stage uses the existing `WorkPipelineStorage` and the new v4.5 operating calculator. It does not replace storage or create duplicate pipeline records.

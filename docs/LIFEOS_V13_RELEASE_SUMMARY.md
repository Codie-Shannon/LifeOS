# LifeOS Desktop v1.3 Release Summary

LifeOS Desktop v1.3 is the **Unified Command Centre + Evidence Vault Foundation** release.

## What changed from v0.9

- Added a unified Command Centre signal model.
- Added a Command Centre snapshot builder that turns module summaries into practical signals.
- Added Today actions so the app can answer “what matters now?” instead of only showing module totals.
- Added manual Daily State and passive waiting concepts.
- Added scheduled communication metadata so “wait / do not chase yet” can be treated as a real state.
- Added Timesheet Evidence models and helper rules using the accepted time buckets:
  - 0.25h = light admin / quick check / short reply.
  - 0.5h = real investigation / review / setup check / structured follow-up.
  - 1.0h+ = implementation / testing / proof build / debugging / documentation.
- Added metadata-only Evidence Vault foundation.
- Added Command Centre signals for evidence review.

## What v1.3 is not

v1.3 is not a CRM, accounting system, mobile app, cloud app, email integration, receipt OCR workflow, or AI automation layer.

## Evidence types prioritised first

1. Screenshots
2. Email/message proof
3. Work session notes
4. Timesheet descriptions
5. Code commits / repo history

## Current rule

If it helps Command Centre answer **what matters now**, it belongs in the v1 spine.
If it is a cool side feature, it stays parked.

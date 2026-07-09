# Group 13 - Money Profile / Hidden Deductions / Safe-to-Spend

## Version

LifeOS Desktop v4.3

## Purpose

Group 13 proves the v4.3 Money Profile layer.

v4.3 strengthens the real money rule before v5 integrations:

- confirmed cash is separate from visible balance
- expected money is visible but not safe money
- hidden deductions reduce safe-to-spend confidence
- tax/GST/ACC/student loan/KiwiSaver/custom reserves are visible before spending
- minimum buffer and emergency hold are deducted
- safe-to-spend confidence is shown as a reviewed state
- all money profile data remains local/manual in v4.3

## Screenshots

| File | What it proves |
|---|---|
| `screenshots/lifeos_v4_3_group13_01_command-centre-v4-3-top.png` | v4.3 loaded, app shell survived, sidebar/footer updated. |
| `screenshots/lifeos_v4_3_group13_02_command-centre-v4-3-money-profile-signals.png` | Command Centre receives v4.3 money-profile signals: final safe, expected excluded, hidden weekly reserve, confidence. |
| `screenshots/lifeos_v4_3_group13_03_command-centre-money-profile-rule-scope.png` | Command Centre explains the v4.3 money profile rule, operating rule, and scope boundary. |
| `screenshots/lifeos_v4_3_group13_04_money-profile-top-metrics.png` | Money Profile workspace exists with top metrics and safe-to-spend data. |
| `screenshots/lifeos_v4_3_group13_05_money-profile-rule-controls.png` | Money profile rule and reset controls are present with local/manual guardrails. |
| `screenshots/lifeos_v4_3_group13_06_safe-to-spend-calculation.png` | Safe-to-spend calculation is visible as actual money logic, not just labels. |
| `screenshots/lifeos_v4_3_group13_07_hidden-deductions-reserve-rules.png` | Hidden deductions/reserve rules are first-class reviewed money items. |
| `screenshots/lifeos_v4_3_group13_08_hidden-deduction-review-gates.png` | Untrusted hidden deductions require review before confidence improves. |
| `screenshots/lifeos_v4_3_group13_09_expected-money-excluded-from-safe.png` | Expected money remains excluded from safe money until paid/cleared. |
| `screenshots/lifeos_v4_3_group13_10_confidence-boundary-next-local-file.png` | Confidence gates, v4.3 boundary, next lane, and local JSON file path are documented. |

## Boundary

v4.3 is local/manual safe-to-spend modelling.

It does not connect:

- bank feeds
- payslips
- IRD/tax portals
- accounting systems
- open banking
- email
- OCR automation
- OAuth
- AI actions
- companion app
- major workspace redesign

## Next lane

v4.4 - Agenda + Payment Calendar.

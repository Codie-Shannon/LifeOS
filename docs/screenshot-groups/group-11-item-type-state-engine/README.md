# Screenshot Group 11 - Item Type / State Engine

## Group

**Group 11:** Item Type / State Engine  
**LifeOS version covered:** v4.1  
**Purpose:** prove that the v4.0 LifeOS Spine Map has become a working local item/state engine before v5 integrations.

## Screenshots

| Order | File | Screen |
|---:|---|---|
| 01 | `screenshots/lifeos_v4_1_group11_01_command-centre-top.png` | Command Centre v4.1 top |
| 02 | `screenshots/lifeos_v4_1_group11_02_command-centre-item-state-signals.png` | Command Centre v4.1 item-state signals |
| 03 | `screenshots/lifeos_v4_1_group11_03_item-state-engine-top.png` | Item State Engine top and metrics |
| 04 | `screenshots/lifeos_v4_1_group11_04_item-state-rule-controls-review-start.png` | Item/state rule, controls, and review start |
| 05 | `screenshots/lifeos_v4_1_group11_05_review-queue-overview.png` | Review queue overview |
| 06 | `screenshots/lifeos_v4_1_group11_06_review-queue-integration-ocr.png` | Future integration preview and receipt OCR review |
| 07 | `screenshots/lifeos_v4_1_group11_07_review-queue-calendar-proof.png` | Calendar import and proof item review |
| 08 | `screenshots/lifeos_v4_1_group11_08_command-centre-pressure-items-top.png` | Command Centre pressure items top |
| 09 | `screenshots/lifeos_v4_1_group11_09_command-centre-pressure-items-review-ocr.png` | Pressure items showing future integration and OCR review |
| 10 | `screenshots/lifeos_v4_1_group11_10_command-centre-pressure-items-income-work-agenda.png` | Pressure items showing invoice, work session, and agenda |
| 11 | `screenshots/lifeos_v4_1_group11_11_money-impact-items-top.png` | Money-impact items top |
| 12 | `screenshots/lifeos_v4_1_group11_12_money-impact-items-income-work.png` | Money-impact items showing income and work |
| 13 | `screenshots/lifeos_v4_1_group11_13_work-people-proof-items-top.png` | Work / people / proof items top |
| 14 | `screenshots/lifeos_v4_1_group11_14_work-people-proof-items-waiting.png` | Work / people / proof items waiting state |
| 15 | `screenshots/lifeos_v4_1_group11_15_state-transition-rules-top.png` | State transition rules top |
| 16 | `screenshots/lifeos_v4_1_group11_16_state-transition-rules-middle.png` | State transition rules middle |
| 17 | `screenshots/lifeos_v4_1_group11_17_v4-1-boundary-next-local-file.png` | v4.1 boundary, next lane, and local item state file |
| 18 | `screenshots/lifeos_v4_1_group11_18_review-queue-work-waiting-proof.png` | Review queue/work waiting/proof screenshot captured during verification |

## Verification notes

The screenshots show:

- the app shell remains intact
- sidebar shows Desktop v4.1
- Command Centre header shows v4.1
- Command Centre includes v4.1 item-state signals: State items, Needs review, and Untrusted
- Item State Engine workspace is reachable from the sidebar
- v4.1 master rule is visible
- item engine controls are local/demo-only
- review queue keeps untrusted/OCR/future integration/calendar/proof items from becoming trusted automatically
- Command Centre pressure items are generated from stateful items
- money-impact items include bills, BNPL, receipts/OCR, invoice/expected income, and work income
- work/people/proof items share the same item-state model
- state transitions are rule-based and include evidence/manual review gates
- accept/reject import and close-week transitions are visible
- v4.1 boundary states no real APIs, OAuth, live sync, AI actions, or automatic real-world changes
- v4.2 is confirmed as the next lane: Bills / Upcoming Payments / Pay Later

## Privacy notes

Screenshots are demo-safe. Do not capture real names, emails, payment details, tenant IDs, private URLs, client secrets, or real account data.

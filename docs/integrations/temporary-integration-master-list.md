# LifeOS Temporary Integration Master List

Status: temporary planning list before the v5 integration architecture pack.

This file captures the broad integration surface LifeOS may eventually support. It is intentionally wider than v5.0. The next step is to turn this list into connector contracts, scopes, risk levels, and staged activation rules.

## Operating Rule

All integrations feed the same review path by default:

External source -> connector -> normalized draft -> provenance and confidence -> Integration Inbox -> user review -> explicit handoff -> LifeOS state change.

No connector should silently create, modify, delete, send, pay, archive, or close external or LifeOS records. Money, health, legal, tax, account, and client-facing actions require extra review.

## V5 Core Intake

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Manual CSV import | Local file | Prove preview intake with deterministic rows | Low |
| Manual JSON import | Local file | Prove structured import without live auth | Low |
| Drag-and-drop file intake | Local file | Fast capture into preview inbox | Low |
| Local folder watcher | Local read-only | Documents, receipts, exports, evidence | Medium |
| Receipt/document upload | Local file | Evidence and money previews | Medium |
| Email-forward-to-LifeOS pattern | Forwarded message or mailbox rule | Later low-friction intake | Medium |

## Calendar

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Google Calendar | Read-only API after manual import proof | Commitments, deadlines, meeting prep | Medium |
| Outlook Calendar / Microsoft 365 Calendar | Read-only API after manual import proof | Commitments, work calendar pressure | Medium |
| Local `.ics` import | Local file | Safe calendar fixture before OAuth | Low |
| Meeting prep extraction | Derived preview | Prep tasks, agenda context, evidence links | Medium |
| Recurring commitment detection | Derived preview | Repeated life pressure and weekly planning | Medium |

## Email

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Gmail | Metadata/read-only scoped API | Follow-ups, receipts, invoices, admin threads | High |
| Outlook Email / Microsoft 365 Mail | Metadata/read-only scoped API | Work follow-ups, receipts, attachments | High |
| Email metadata intake | Header/summary preview | Thread triage without broad content ingestion | Medium |
| Receipt detection | Email parser | Money evidence and receipt candidates | Medium |
| Invoice/payment detection | Email parser | Payment calendar and money obligations | High |
| Follow-up detection | AI-assisted preview | Suggested next actions and reminders | High |
| Attachment evidence extraction | Attachment metadata/file preview | Proof, documents, receipts | High |

## Files And Documents

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Google Drive | Read-only scoped API | Documents, receipts, project evidence | High |
| OneDrive | Read-only scoped API | Documents, receipts, project evidence | High |
| SharePoint | Read-only scoped API | Work/client evidence and docs | High |
| Local folders | Local read-only | Offline-first document intake | Medium |
| PDF intake | Local parser/OCR | Receipts, invoices, contracts, evidence | Medium |
| Image/photo receipt intake | Local upload/OCR | Receipt and document capture | Medium |
| Document classification | AI-assisted preview | Evidence type, target module, confidence | Medium |

## Money

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Manual income/expense entry | In-app entry | Baseline financial truth | Low |
| CSV bank exports | Local file | Transactions and payment history previews | High |
| Accounting exports | Local file | Business income, invoices, tax evidence | High |
| Xero | Export first, API later | Accounting, invoices, paid/unpaid evidence | High |
| Stripe | Read-only API later | Business/client payment status | High |
| PayPal | Export first, API later | Payments, subscriptions, client receipts | High |
| Wise | Export first, API later | International payments and balances | High |
| Bank feed / open banking | Deferred live API | Full money brain and cashflow truth | Very high |

## BNPL And Obligations

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Afterpay | Manual or email-derived schedule | Repayments, pressure, safe-to-spend adjustment | High |
| Zip | Manual or email-derived schedule | Repayments, pressure, safe-to-spend adjustment | High |
| Klarna | Optional later | BNPL repayment schedule | High |
| PayPal Pay in 4 | Optional later | BNPL repayment schedule | High |
| Manual repayment schedule entry | In-app entry | Safe BNPL modelling before live APIs | Medium |
| Email-based repayment detection | Email parser | Due date and amount previews | High |

## Receipts, Invoices, And OCR

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Local OCR engine | Local processing | Receipt/invoice text extraction | Medium |
| Cloud OCR provider | Deferred API | Higher-quality extraction if needed | High |
| Receipt parser | Local/AI-assisted preview | Merchant, amount, date, category | Medium |
| Invoice parser | Local/AI-assisted preview | Amount due, due date, payer/payee | High |
| Tax/category suggestion | AI-assisted preview | Admin and tax preparation | High |
| Merchant normalization | Local rules | Cleaner recurring money history | Medium |

## Work And Time

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Manual timer/work logs | Existing local module | Work evidence and time proof | Low |
| Calendar-derived work blocks | Read-only calendar preview | Work load and evidence hints | Medium |
| GitHub activity | Read-only metadata | Dev work evidence and project trail | Medium |
| GitLab | Optional later | Dev work evidence | Medium |
| Bitbucket | Optional later | Dev work evidence | Medium |
| Jira / Atlassian | Optional later | Tickets, deadlines, work status | High |
| Linear | Optional later | Tasks and project status | Medium |
| Notion | Optional later | Notes, tasks, project docs | High |
| Slack | Metadata/read-only later | Work follow-ups and decisions | High |
| Teams | Metadata/read-only later | Work follow-ups and decisions | High |

## Health And Personal Operating System

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Manual health metrics | In-app entry | Energy, mood, sleep, medication logs | Medium |
| Apple Health | Deferred mobile integration | Health context and pattern prompts | Very high |
| Google Fit | Deferred mobile integration | Health context and pattern prompts | Very high |
| Fitbit | Optional later | Sleep/activity signals | Very high |
| Garmin | Optional later | Activity/recovery signals | Very high |
| Oura | Optional later | Sleep/recovery signals | Very high |

## Tasks And Notes

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Internal LifeOS tasks | Native | Primary task truth | Low |
| Todoist | Optional import/export | External task migration or mirror | Medium |
| Microsoft To Do | Optional import/export | Work/personal task bridge | Medium |
| Notion | Optional import/export | Notes and project context | High |
| Obsidian/local markdown folder | Local read-only | Knowledge, notes, project evidence | Medium |
| Local markdown export | Local file | User-owned backup and website/docs bridge | Low |

## AI Assistance

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Generative follow-up suggestions | Preview-only AI assistance | Suggested replies, reminders, next actions | High |
| Email/task summary | Preview-only AI assistance | Reduce inbox and task load | High |
| Draft replies | User-approved draft only | Prepare messages without sending | Very high |
| Weekly close-out narrative | Local summary | Review week, pressure, evidence, misses | Medium |
| Financial pressure explanations | Local/AI-assisted summary | Explain why safe-to-spend changed | High |
| Document classification | Preview-only AI assistance | Route documents to modules | Medium |
| Pattern detection and dismissal memory | Local rule/AI assistance | Stop repeating rejected suggestions | Medium |

## Communication

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| SMS | Deferred mobile integration | Appointment, payment, and follow-up signals | Very high |
| WhatsApp | Deferred if API access is sane | Personal/work follow-up signals | Very high |
| Messenger / Instagram DMs | Probably late or avoided | Social follow-up signals | Very high |
| Slack | Metadata/read-only first | Work signals and follow-ups | High |
| Teams | Metadata/read-only first | Work signals and follow-ups | High |

## Browser, Contacts, Photos, And Context

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Browser bookmarks | Local export/import | Research trails and project evidence | Medium |
| Browser history | Optional local import | Research context and memory | Very high |
| Google Contacts | Read-only scoped API | People context and client/family/admin links | High |
| Outlook People | Read-only scoped API | Work and admin contact context | High |
| Phone camera roll | Deferred mobile import | Receipts, documents, whiteboards | High |
| Photos folder import | Local read-only | Receipt/document/image evidence | Medium |
| Location context | Deferred opt-in | Travel, appointments, mileage, errands | Very high |
| Maps and mileage | Deferred opt-in | Business travel and appointment pressure | Very high |

## Orders, Accounts, And Life Admin

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| Amazon/order emails | Email-derived previews | Deliveries, receipts, returns | High |
| Courier tracking emails | Email-derived previews | Delivery pressure and reminders | Medium |
| Grocery/pharmacy order emails | Email-derived previews | Household, health, receipt evidence | High |
| Utilities and account portals | Manual/export/email first | Bills, renewals, obligations | High |
| Insurance documents | Document upload/email first | Renewal and evidence tracking | High |
| Subscription emails | Email-derived previews | Recurring obligations and safe-to-spend | High |
| Password manager references | Deferred metadata only | Account existence and renewal hints | Very high |
| Government/tax portals | Manual export/document upload | Tax/admin evidence | Very high |

## Learning, CRM, Support, And Automation Bridges

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| YouTube learning history | Optional later | Learning trail and project evidence | Medium |
| Udemy | Optional later | Course progress and learning tasks | Medium |
| Coursera | Optional later | Course progress and learning tasks | Medium |
| HubSpot | Optional later | Client relationship and sales pipeline | High |
| Salesforce | Optional later | Client relationship and sales pipeline | High |
| Airtable | Optional later | Lightweight database/project import | Medium |
| Zendesk | Optional later | Support/customer follow-ups | High |
| Intercom | Optional later | Support/customer follow-ups | High |
| Freshdesk | Optional later | Support/customer follow-ups | High |
| Zapier | Deferred webhook bridge | External automation ingress | High |
| Make | Deferred webhook bridge | External automation ingress | High |
| IFTTT | Deferred webhook bridge | Personal automation ingress | High |
| Webhooks | Deferred controlled endpoint | Generic integration bridge | High |

## Website And Mobile Future

| Integration | First mode | Primary LifeOS use | Risk |
| --- | --- | --- | --- |
| LifeOS website docs hub | Static/docs site first | Move in-app docs outward over time | Low |
| Mobile companion app | Later shared backend | Capture, notifications, mobile-first flows | High |
| Push notifications | Mobile/web later | Reminders and pressure signals | High |
| Mobile receipt capture | Mobile app | Camera-to-preview evidence | Medium |
| Mobile quick note/task capture | Mobile app | Fast inbox capture | Medium |
| Shared integration backend | Architecture stage | One connector layer for desktop/web/mobile | High |

## Deferred Or High-Risk By Default

- Live banking APIs.
- Live BNPL account login/API access.
- Auto-send email, SMS, or chat messages.
- Auto-pay anything.
- Auto-delete/archive external records.
- Direct health-record ingestion.
- Medical conclusions.
- Legal/tax advice actions.
- Password manager secrets.
- Broad browser history or location tracking.
- Government portal live access.

## Next Conversion Step

Convert this temporary list into the v5 integration architecture pack:

- Provider registry.
- Connector interface/base contract.
- Permission and scope matrix.
- OAuth/token storage strategy.
- Secret handling rules.
- Connector health model.
- Sync run and audit model.
- Error state model.
- Duplicate key conventions.
- Provenance metadata conventions.
- Disconnect and local-cache deletion behavior.
- Fake connector test fixtures.
- UI contract for disconnected, connected, last sync, and error states.
- Mapping contract from external record to `IntegrationPreviewDraft`.

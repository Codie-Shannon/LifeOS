# LifeOS Roadmap

## Purpose

LifeOS is a weekly pressure command centre.

It exists to show what money, work, payments, deductions, appointments, tasks, messages, follow-ups, client work, and imported outside data are putting pressure on the week, then help decide what is safe to do next.

LifeOS is not mainly a budget app, calendar app, task app, timer app, banking app, CRM, or email client.

Those are inputs and modules.

LifeOS is the pressure layer that connects them.

## Platform Rule

Desktop is the daily-use power version and proving ground.

Mobile is the daily-use optimized version and pressure test.

Core and Shared projects preserve reusable logic.

TimerAgent remains a desktop-only utility that feeds work, time, income, tax set-aside, and proof data into LifeOS.

Imported data is never trusted automatically.

The core rule is:

```text
Imported does not mean trusted until reviewed.
```

Microsoft Graph, Google APIs, bank imports, email parsing, calendar imports, and future automation should all follow the same review-first model.

## Version Overview

```text
v0.1 = LifeOS exists
v0.2 = LifeOS understands the week
v0.3 = LifeOS understands work
v0.4 = LifeOS becomes trustworthy
v0.5 = LifeOS imports outside data safely
v0.6 = LifeOS helps make decisions
v0.7 = LifeOS manages client work
v0.8 = LifeOS manages business operations
v0.9 = LifeOS becomes release-candidate serious
v1.0 = stable local-first desktop milestone
v1.x = controlled live integrations
```

---

# v0.1 — LifeOS Exists

## Theme

Proof, shell, and first real command centre.

## Status

Done.

## Goal

Prove that LifeOS is a real desktop application, not just a concept.

## Included

- WPF desktop shell
- shared Core / Shared architecture
- shared module catalog
- Money Pressure module foundation
- manual Money Pressure inputs
- local JSON persistence for Money Pressure
- Follow-Ups module foundation
- local JSON persistence for Follow-Ups
- real Command Centre summary
- TimerAgent correctly framed as desktop-only
- README, screenshots, docs, main merge, tag/release

## Result

LifeOS crossed from idea into working proof.

---

# v0.2 — LifeOS Understands the Week

## Theme

Weekly Command Centre upgrade.

## Goal

Turn LifeOS from a working proof into something that can be opened each morning and used to understand the week.

## Main Scope

### Agenda Foundation

Add a manual/local-first Agenda module.

Fields:

```text
Title
Date
Time optional
Location optional
Type
Pressure level
Notes
IsCompleted
```

Types:

```text
Appointment
School run
Meeting
Deadline
Errand
Work block
Reminder
```

Command Centre should show:

```text
Today’s agenda count
Next appointment
Overdue agenda items
High-pressure agenda items
```

### Manual Pay-Later Foundation

Add a local-first Pay-Later tracker before email/API imports.

Fields:

```text
Provider
Merchant
Original purchase amount
Remaining balance
Instalment amount
Next payment date
Payment frequency
Status
Notes
IsEssential
Linked money pressure
```

Command Centre should show:

```text
Pay-later due this week
Next pay-later payment
Total known pay-later pressure
Payments due before next income
```

### Weekly Close-Out Foundation

Add a weekly review page that answers:

```text
What changed this week?
What is still open?
What money is still unsafe?
What follow-ups are still waiting?
What carries into next week?
```

### Command Centre v2

Command Centre should combine:

```text
Money Pressure
Follow-Ups
Agenda
Pay-Later
Weekly Close-Out status
TimerAgent status
```

## Not Included Yet

- bank sync
- email sync
- calendar API sync
- mobile app
- AI agent layer
- database migration

---

# v0.3 — LifeOS Understands Work

## Theme

Work, income, and proof loop.

## Goal

Make LifeOS understand what work was done, what money it created, what is unpaid, and what proof/case-study value came from it.

## Main Scope

### TimerAgent CSV Import

LifeOS should read TimerAgent session logs and show:

```text
Work sessions this week
Hours worked
Billable hours
Earned amount
Tax set-aside estimate
Safe-after-tax amount
Unpaid / pending work value
```

### Work Sessions Module

Add a page for imported work sessions.

Fields:

```text
Date
Task
Person / organisation
Work type
Mode
Start time
End time
Duration
Rate
Earned
Tax set aside
Safe amount
```

Filters:

```text
This week
This month
By client/contact
By work type
Billable only
Unpaid/pending
```

### Income Link into Money Pressure

Money Pressure should understand:

```text
Earned but unpaid
Invoiced but unpaid
Paid
```

Core rule:

```text
Earned money is not spendable money until paid.
```

### Proof / Case Study Tracker Foundation

Fields:

```text
Project
Client / context
Proof type
Screenshot path optional
Outcome
Business value
Next follow-up
```

## Command Centre v3

Add:

```text
Hours worked this week
Billable value created
Pending work income
Proof items created
Unpaid work risk
```

---

# v0.4 — LifeOS Becomes Trustworthy

## Theme

Data safety, review, and reliability.

## Goal

Make LifeOS safer, cleaner, and harder to break.

## Main Scope

### Backup / Restore

Add:

```text
Create backup
Restore backup
Open data folder
View storage location
Backup timestamp
Backup contents summary
```

Backup should include:

```text
Money Pressure data
Follow-Ups data
Agenda data
Pay-Later data
Weekly Close-Out data
TimerAgent imported summaries
Proof / Case Study tracker data
Settings
```

### Data Health Page

Check for:

```text
missing dates
invalid money values
overdue follow-ups
agenda items without type
work sessions missing client/contact
proof items missing outcome
JSON files missing or corrupt
```

### Real Settings

Add:

```text
theme preference
data folder path display
backup folder preference
default tax set-aside percentage
default hourly rate
default emergency buffer
default food/fuel buffer
demo-data reset
open local app data folder
```

### Detail/Edit Screens

Add correction/edit flows for:

```text
Money Pressure values
Follow-Ups
Agenda items
Pay-Later items
Work sessions/imported records
Proof items
Weekly Close-Out notes
```

## Command Centre v4

Add:

```text
Data health
Last backup
Open pressure items
Week close-out status
Work import status
```

---

# v0.5 — LifeOS Imports Outside Data Safely

## Theme

External data, import flows, and controlled automation.

## Goal

Reduce manual entry by importing useful data, while keeping review and approval before anything affects the week.

## Main Scope

### Import Centre

Support a safe import workflow:

```text
Choose file
Preview rows
Validate rows
Import
Show import result
```

Initial sources:

```text
TimerAgent CSV
Calendar ICS
Manual bills CSV
Follow-ups CSV
Pay-later CSV/manual exports
```

### Calendar Import Foundation

Import from:

```text
.ics calendar files
manual calendar exports
local test calendar JSON
```

Imported fields:

```text
Title
Date
Start time
End time
Location
Source
ImportedAt
IsReviewed
PressureType
Notes
```

### Review Queue

Imported items can be:

```text
Accept
Ignore
Edit then accept
Mark as duplicate
```

Review item types:

```text
Imported calendar event
Imported work session
Imported bill
Imported follow-up
Imported payment
Imported pay-later payment
```

### Duplicate Detection Foundation

Detect likely duplicates by:

```text
same title/name
same date
same amount
same source
same time window
```

## Command Centre v5

Add:

```text
Unreviewed imports
Possible duplicates
Imported agenda pressure
Imported work income
Import health
```

---

# v0.6 — LifeOS Helps Make Decisions

## Theme

Guidance, planning, and decision support.

## Goal

LifeOS should not just show pressure. It should help decide what to do next, what can wait, what is unsafe, and what needs a follow-up.

## Main Scope

### Decision Engine Foundation

The decision engine looks across:

```text
Money Pressure
Agenda
Pay-Later
Follow-Ups
Work Sessions
Proof Tracker
Weekly Close-Out
Import Review Queue
Data Health
```

Outputs:

```text
Recommended next action
Risk warnings
Moveable items
Blocked items
Payment follow-up suggestions
Weekly priority order
```

### Can-I-Do-This Checker

User enters a possible action:

```text
Buy something
Take on work
Delay a bill
Skip a task
Move an appointment
Spend time on portfolio
Follow up with a client
```

LifeOS answers:

```text
Safe
Risky
Unsafe
Needs review first
```

With reasons.

### Draft Actions

LifeOS can generate draft actions, but not send them automatically.

Examples:

```text
Draft follow-up message
Draft invoice note
Draft weekly close-out summary
Draft client scope note
Draft tomorrow plan
Draft payment reminder
```

Rule:

```text
LifeOS can suggest.
User approves.
Nothing external is sent automatically.
```

### Today Plan / Tomorrow Plan

Plan buckets:

```text
Must do
Should do
Could do
Avoid today
Waiting on
```

## Command Centre v6

Add:

```text
Recommended next action
Today Plan status
Can-I-do-this shortcut
Draft actions available
Decision warnings
```

---

# v0.7 — LifeOS Manages Client Work

## Theme

Client work, scopes, payments, and delivery tracking.

## Goal

Help manage real work opportunities from first contact to scope, delivery, proof, payment, and follow-up.

## Main Scope

### Client / Contact Workspace

Fields:

```text
Client / organisation name
Contact person
Status
Current opportunity
Last contact date
Next follow-up date
Money-linked?
Active scope?
Notes
Related follow-ups
Related work sessions
Related proof items
Related payments
```

### Scope of Work Tracker

Track phased opportunities:

```text
Opportunity
Proposed scope
Agreed scope
Phase
Estimate
Status
Start date
Due date
Payment status
Notes
```

Statuses:

```text
Idea
Discussed
Waiting on client
Scoped
Approved
In progress
Delivered
Paused
Completed
Cancelled
```

### Payment / Invoice Readiness Tracker

Track:

```text
Work completed
Billable amount
Invoice needed?
Invoice sent?
Payment expected?
Payment received?
Follow-up needed?
```

### Delivery Checklist Per Scope

Checklist:

```text
Requirements understood
Demo/proof built
Screenshots captured
README updated
Client message drafted
Delivered
Follow-up date set
Proof/case-study note captured
```

## Command Centre v7

Add:

```text
Active opportunities
Waiting-on clients
Uninvoiced completed work
Payment follow-ups
Delivery risks
Proof not captured
```

---

# v0.8 — LifeOS Manages Business Operations

## Theme

Business operations, pipeline, and cashflow control.

## Goal

Show whether the work pipeline, active scopes, unpaid work, follow-ups, and upcoming money are enough to keep the business moving.

## Main Scope

### Pipeline Dashboard

Pipeline states:

```text
Lead
Warm lead
Scoped
Quoted
Approved
In progress
Delivered
Invoiced
Paid
Parked
Lost
```

Cards:

```text
Active leads
Warm leads
Approved work
Work in progress
Delivered but unpaid
Expected income
At-risk opportunities
```

### Cashflow Forecast Foundation

Use:

```text
current safe money
expected paid income
pending work income
bills/deductions
pay-later payments
payment dates
scope estimates
```

Show:

```text
This week
Next week
Next 30 days
Expected paid money
Unpaid/pending money
Known outgoing money
Projected safe buffer
```

Rule:

```text
Expected money is not safe money until paid.
```

### Business Risk Flags

Examples:

```text
Too much work waiting on one client
Too much delivered work unpaid
Too many warm leads with no follow-up date
No scoped work after this week
Too much unpaid/pending income counted mentally as safe
No proof captured for delivered work
```

### Workload Capacity View

Show:

```text
Available work hours this week
Booked work hours
Personal/fixed agenda blocks
Billable work planned
Admin/follow-up work planned
Overloaded days
Open capacity
```

## Command Centre v8

Add:

```text
Pipeline health
Cashflow forecast
Capacity risk
Delivered but unpaid
Warm leads needing follow-up
Next business-safe action
```

---

# v0.9 — LifeOS Becomes Release-Candidate Serious

## Theme

Polish, packaging, reliability, and public-ready proof.

## Goal

Make LifeOS feel like a clean, installable, explainable desktop product.

## Main Scope

### Installer / Packaged Build

Add:

```text
Windows packaged build
installer or zipped portable release
version number visible in app
release folder
basic install/run instructions
```

### About / Demo Mode / Reset Demo Data

Add:

```text
App version
storage location
local-first notice
desktop/mobile platform direction
demo data reset
open docs/GitHub link
```

Buttons:

```text
Load Demo Data
Clear Demo Data
Open Data Folder
Create Backup
```

### UI Consistency Pass

Review:

```text
consistent headers
consistent metric cards
consistent buttons
consistent empty states
consistent warning panels
consistent spacing
consistent page subtitles
```

### Error Handling / Recovery

Add:

```text
if JSON corrupt, show recovery message
if backup exists, offer restore
if data file missing, recreate defaults
if import fails, show validation errors
if path unavailable, explain what happened
```

### Full Manual Test Suite

Cover:

```text
Money Pressure
Agenda
Pay-Later
Follow-Ups
Weekly Close-Out
Work Sessions
TimerAgent import
Proof tracker
Backup/restore
Import Centre
Decision Engine
Client/Scope tracking
Pipeline/Cashflow/Capacity
Settings/About
Demo data
```

## Command Centre v9

Add:

```text
Today pressure
Week pressure
Money pressure
Work pressure
Client/business pressure
Data health
Backup status
Import review status
Next safest action
```

---

# v1.0 — Stable Local-First Desktop Milestone

## Theme

Stable desktop release.

## Goal

Make LifeOS stable enough to be used seriously as a local-first weekly pressure command centre.

## Main Scope

- clean installer or portable build
- stable local persistence
- tested backup/restore
- core modules reliable
- demo mode safe for screenshots/videos
- strong README and release notes
- known limitations documented
- no dangerous live integrations required

## v1.0 Rule

v1.0 should not mean “everything is done.”

It should mean:

```text
The local-first desktop version is stable, explainable, useful, and safe enough to trust.
```

---

# v1.x — Controlled Live Integrations

## Theme

Read-only APIs first, write actions much later.

## Goal

Start connecting LifeOS to real user systems through Microsoft Graph and Google APIs without losing control or trust.

## Microsoft Graph Integration

Useful sources:

```text
Outlook emails
Outlook calendar events
Microsoft To Do tasks
OneDrive/SharePoint files
Teams messages, later
Contacts
```

LifeOS uses:

```text
email search/import
calendar import
attachment detection
follow-up detection
pay-later/payment email detection
client email thread detection
```

Rule:

```text
Graph finds it.
LifeOS previews it.
User approves it.
Then it enters LifeOS.
```

## Google API Integration

Useful sources:

```text
Gmail
Google Calendar
Google Drive
Google Contacts
```

LifeOS uses:

```text
Afterpay/Klarna/Zip email detection
calendar event import
client message detection
payment-related email detection
Drive file linking to projects/proof
```

Rule:

```text
Imported does not mean trusted until reviewed.
```

## Suggested v1.x Order

```text
v1.1 = Microsoft Graph read-only import
v1.2 = Google read-only import
v1.3 = draft email / draft follow-up actions
v1.4 = optional write actions with confirmation
v1.5 = deeper sync, only if reliability is proven
```

## Read-Only First

Allowed early:

```text
Read emails
Read calendar events
Read contacts
Read file metadata
Import selected items after review
```

Avoid early:

```text
Send emails automatically
Edit calendar automatically
Delete emails
Move emails
Auto-create tasks without review
Auto-sync everything
```

---

# Long-Term Rule

Every new feature must answer at least one of these:

```text
What money is safe?
What is due?
What am I waiting on?
What can I move?
Why is this week bad?
What can I safely do next?
```

If a feature does not reduce weekly pressure, clarify its role before building it.

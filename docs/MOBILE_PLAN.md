# LifeOS Mobile Plan

LifeOS Mobile is not a sidekick app.

It is the full daily-use mobile version of LifeOS.

The mobile app should eventually support the same core LifeOS modules as desktop, but with phone-first screens and optimized workflows.

## Mobile Role

Mobile is the pressure test for the LifeOS model.

Desktop can prove a feature first because it has more room, better debugging, richer dashboards, and faster iteration.

Mobile validates whether that feature is actually clear enough for daily life.

If a workflow cannot survive mobile without becoming cluttered, the workflow needs to be simplified.

## Relationship to Desktop

Desktop is the daily-use power version.

Mobile is the daily-use optimized version.

Desktop will usually stay ahead in experimental and advanced features.

Mobile should catch up on the core daily LifeOS system.

## Shared Core Modules

Mobile should eventually support:

- Today / Command Centre
- Money Pressure
- Safe-to-Spend
- Income tracking
- Paid / unpaid / expected / overdue income
- Deductions
- Pay-Later
- Agenda
- Tasks
- Follow-Ups
- Weekly Close-Out
- Pressure Score
- What can I move?
- Can I afford this?
- Settings

## First Mobile Screens

A future first mobile version could use:

- Today
- Money
- Agenda
- Capture
- Follow-Ups
- More / Settings

## Today Screen

The Today screen should answer quickly:

- What is safe?
- What is due?
- Who am I waiting on?
- What is fixed today?
- What can move?
- What is the next safest action?

Example strip:

```text
Today: $48 safe | 2 payments | 1 appointment | 3 tasks | 1 person waiting
```

## Mobile UX Principles

- One clear action per screen where possible.
- Avoid dense tables.
- Use cards, summaries, and drill-downs.
- Make check-ins fast.
- Keep manual entry short.
- Use templates and recurring items later.
- Do not copy desktop clutter.
- Use mobile to expose overbuilt ideas.

## TimerAgent on Mobile

TimerAgent itself remains desktop-only.

The following are not mobile features:

- Tray icon
- Global shortcut
- Always-on-top compact timer
- Desktop overlay
- Desktop CSV work-session workflow

Mobile may later show:

- Work-session summaries
- Earned income summaries
- Paid/unpaid work status
- Weekly work totals
- TimerAgent data imported into pressure summaries

But the TimerAgent utility remains a Windows desktop tool.

## Sync Direction

Initial LifeOS is local-first.

Mobile sync should not be built until the desktop data model is stable.

Possible future sync options:

- Manual export/import
- Local network sync
- Encrypted cloud sync
- Self-hosted API
- Google Drive / OneDrive backup later

## What Not To Build Yet

- Full mobile app during desktop shell phase
- Bank connections
- Invoice generation
- Complex calendar sync
- Cloud accounts
- Push notifications before reminder logic is stable
- SMS auto-send
- Public SaaS account system

## v0.1 Mobile Goal

Plan only.

No mobile app build in the current desktop shell phase.

# LifeOS Desktop v0.2 Release Notes

## Release name

LifeOS Desktop v0.2 — LifeOS understands the week

## Status

Completed.

## Summary

LifeOS Desktop v0.2 upgrades the original v0.1 proof into a broader weekly pressure command centre.

v0.1 proved that LifeOS exists. v0.2 adds the weekly planning and review layer: Agenda, Pay Later, and Weekly Close-Out.

The application is still intentionally local-first and desktop-first. The UI remains inside `MainWindow` for speed, while Core and Shared modules keep the business logic reusable for future desktop, mobile, and web versions.

## Added in v0.2

### Agenda foundation

Added a local-first Agenda module for weekly items, including:

- title
- type
- status
- pressure level
- due date
- time text
- fixed commitment flag
- notes

The Agenda summary calculates:

- open agenda item count
- due-today count
- overdue count
- this-week count
- high-pressure count
- pressure reasons

### Pay Later foundation

Added a local-first Pay Later module for deferred obligations, including:

- name
- payee
- amount
- due date
- status
- pressure level
- notes

The Pay Later summary calculates:

- open item count
- open amount
- due-this-week amount
- overdue amount
- high-pressure count
- pressure reasons

### Weekly Close-Out foundation

Added a local-first Weekly Close-Out module for the weekly reset loop, including:

- week start
- what got done
- what moved
- what is still waiting
- next-week pressure
- notes

The Weekly Close-Out summary calculates:

- total entries
- current-week entries
- whether the current week has a close-out
- waiting-on count
- pressure reasons

### MainWindow v0.2 wiring

The following modules are now available through the desktop navigation:

- Command Centre
- Money Pressure
- Agenda
- Pay Later
- Weekly Close-Out
- Follow-Ups
- Projects placeholder
- TimerAgent placeholder/status
- Settings placeholder

### Local-first persistence

v0.2 continues the local JSON direction and adds local storage for the new weekly modules.

## Screenshots

Screenshots are stored in:

```text
docs/screenshots/v0.2/
```

Current v0.2 screenshots:

```text
01-lifeos-v02-command-centre.png
02-lifeos-v02-agenda.png
03-lifeos-v02-pay-later.png
04-lifeos-v02-weekly-close-out.png
05-lifeos-v02-money-pressure.png
06-lifeos-v02-follow-ups.png
```

## What this version proves

LifeOS now has a stronger weekly operating loop:

```text
money pressure + follow-ups + agenda + deferred obligations + weekly close-out
```

This moves LifeOS from a basic proof into a usable weekly pressure system.

## Known limitations

- MainWindow is still the only desktop view.
- Agenda, Pay Later, and Weekly Close-Out are first-pass foundations, not final polished modules.
- There is no mobile app yet.
- There is no database yet.
- There are no external imports yet.
- TimerAgent CSV data is not imported into the Command Centre yet.
- There is no backup/restore flow yet.
- There are no edit screens yet; items are mainly add/delete/mark-complete style flows.

## Next version

v0.3 should focus on:

- Work Sessions
- income/work value
- Proof Tracker
- Command Centre v3 work/proof pressure

Theme:

```text
v0.3 = LifeOS understands work, income, and proof
```

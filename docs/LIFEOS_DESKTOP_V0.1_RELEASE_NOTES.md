# LifeOS Desktop v0.1 Release Notes

## Summary

LifeOS Desktop v0.1 is the first working proof of the LifeOS weekly pressure command centre.

This version proves the core platform direction:

- Desktop is the daily-use power version and proving ground.
- Mobile will be the daily-use optimized version and pressure test.
- Core and Shared projects hold reusable LifeOS logic and app-facing structure.
- TimerAgent remains a desktop-only utility that feeds LifeOS work/time/income data.

## Included in v0.1

- WPF desktop shell
- Command Centre home page
- shared module catalog
- Money Pressure module foundation
- manual Money Pressure input UI
- local JSON persistence for Money Pressure
- Follow-Ups module foundation
- local JSON persistence for Follow-Ups
- real Command Centre summary pulling from local module data
- next safest action text
- combined weekly pressure reasons
- TimerAgent platform positioning

## Money Pressure

Money Pressure supports manual weekly values:

- current balance
- paid income
- pending income
- bills due
- deductions
- food/fuel buffer
- emergency buffer

The module calculates:

- safe-to-spend
- pressure label
- pending income kept separate from safe money
- reasons why the week has pressure

## Follow-Ups

Follow-Ups supports basic waiting-on tracking:

- person / organisation
- context
- next action
- follow-up date
- status
- priority
- money-linked flag
- notes

The module calculates:

- open follow-ups
- waiting count
- needs-action count
- overdue count
- due-today count
- money-linked count

## Command Centre

The Command Centre reads local Money Pressure and Follow-Ups data and shows:

- overall pressure
- safe-to-spend
- pending income
- open follow-ups
- needs-action follow-ups
- money-linked follow-ups
- next safest action
- combined pressure reasons

## Storage

LifeOS Desktop v0.1 stores local JSON data under the LifeOS local application data folder.

Current local files include:

- `money-pressure-input.json`
- `follow-ups.json`

## Not Included Yet

- mobile app
- website
- database
- bank sync
- email/calendar import
- TimerAgent CSV import into Command Centre
- agenda module
- pay-later tracker
- weekly close-out
- installer
- public release packaging

## Status

LifeOS Desktop v0.1 is a private alpha/proof build, not a public commercial product.

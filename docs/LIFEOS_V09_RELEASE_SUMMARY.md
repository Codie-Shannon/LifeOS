# LifeOS Desktop v0.9 Release Summary

## Release name

**LifeOS Desktop v0.9 — Work Pipeline + Command Centre Release Candidate**

## Short summary

LifeOS Desktop v0.9 is the release-candidate baseline before v1.0. It adds and polishes the Work Pipeline, connects work/opportunity pressure into the Command Centre, keeps expected money separate from safe money, and proves local JSON persistence with backup storage.

## Why this release matters

v0.9 turns LifeOS into a stronger personal/work operating system baseline.

It answers:

- What active work exists?
- What stage is it in?
- What is blocked?
- Who am I waiting on?
- What needs follow-up?
- What work is billable?
- What needs a timesheet?
- What needs an invoice?
- What money is expected but not safe?
- What should be visible from the Command Centre?
- Is the data persisted locally?

## Core release story

```text
Work Pipeline -> follow-up/waiting/money pressure -> Command Centre visibility -> local JSON proof
```

## New/updated areas

### Work Pipeline

- open pipeline count
- active count
- waiting count
- blocked count
- follow-up count
- expected value
- timesheets needed
- invoices needed
- payments expected
- stage breakdown
- today focus

### Command Centre

- pipeline open signal
- pipeline blocked signal
- pipeline follow-up signal
- expected pipeline money signal
- billable value signal
- unpaid work signal
- conservative wording around not-safe money

### Follow-Ups

- open follow-ups
- waiting-on pressure
- needs action
- overdue
- due today
- money-linked follow-ups

### Money Timeline

- current balance
- incoming by target date
- outgoing/buffers
- projected balance
- lowest point
- safe-to-spend
- pressure label

### Local storage

- local JSON module storage
- Work Pipeline JSON
- Work Pipeline backup JSON
- data survives app restart

## Current boundaries

v0.9 stays deliberately focused:

- no cloud sync
- no mobile app
- no client portal
- no bank sync
- no final accounting ledger
- no payment gateway
- no enterprise features
- no live hardware control

## Portfolio-safe summary

LifeOS Desktop v0.9 is a local-first pressure command centre that connects money, follow-ups, work sessions, proof, and a practical Work Pipeline. It tracks paid work, warm leads, blocked work, follow-ups, timesheet/invoice pressure, expected payments, and parked ideas while keeping expected money separate from safe money. The release proves local JSON persistence and prepares the app for the v1.0 Unified Command Centre Foundation.

## Next target

v1.0 should focus on making the existing pieces work together through one stronger Command Centre flow.

The v1.0 question is:

```text
What matters now?
```

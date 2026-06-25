# LifeOS Desktop v0.5 Release Summary

## Release name

**LifeOS Desktop v0.5 — Paid Work Centre + Money Timeline**

## Short summary

LifeOS Desktop v0.5 adds the first paid-work admin layer and a date-based Money Timeline. It connects work sessions, invoice-ready summaries, unpaid work, projected income, outgoing payments, safe-to-spend calculations, and Command Centre visibility.

## Why this release matters

v0.5 turns LifeOS from a weekly pressure tracker into the first version of a practical work-money operating system.

It answers:

- What work have I done?
- What is invoice-ready?
- What is still unpaid?
- What could be copied into a client work summary?
- What money is coming in?
- What payments/bills are going out?
- What will the balance look like by a target date?
- What is probably safe to spend?

## Core release story

```text
Work Sessions -> Paid Work Centre -> invoice-ready summary -> expected payment -> Money Timeline -> Command Centre
```

## New/updated areas

### Paid Work Centre

- invoice-ready sessions
- invoice-ready value
- unpaid billable value
- paid value
- billable hours
- clients/projects
- copy-ready work summary
- invoice-ready item list

### Money Timeline

- current balance
- incoming by target date
- outgoing/buffers
- projected balance
- lowest point
- safe-to-spend
- pressure label

### Command Centre

- v0.5 wording
- paid-work admin direction
- money timeline direction
- billable/unpaid work visibility

## Current boundaries

v0.5 keeps the system deliberately safe and local:

- no bank sync
- no tax filing
- no final accounting ledger
- no payment gateway
- no client portal
- no mobile app
- no final PDF invoice generator yet

## Release proof screenshots

- Command Centre overview
- Work Sessions source data
- Paid Work Centre metrics
- Paid Work Centre invoice-ready summary
- Money Timeline projected balance
- Command Centre with v0.5 data

## Suggested GitHub release text

LifeOS Desktop v0.5 adds the first paid-work admin and money timeline layer.

This release introduces the Paid Work Centre for turning completed billable work sessions into invoice-ready summaries, plus Money Timeline for planning projected balance, safe-to-spend, and pressure around incoming and outgoing money.

The main v0.5 spine is:

```text
do work -> log work session -> review invoice-ready items -> generate work summary -> expected payment -> money timeline -> safe-to-spend pressure view
```

This is still local-first and intentionally simple. v0.5 does not attempt to become a full accounting system, bank-sync app, or tax tool. It focuses on the practical bridge between work done, money owed, money coming in, and what is safe to spend.

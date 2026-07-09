# Stage 071 - v4.4 Payment Calendar Core

## Purpose

Add the v4.4 Agenda + Payment Calendar core model.

## Adds

- `PaymentCalendarItem`
- `PaymentCalendarPlan`
- `PaymentCalendarSummary`
- `PaymentCalendarDayGroup`
- item kind/state/trust/pressure enums
- `PaymentCalendarCalculator`

## Boundary

v4.4 is local/manual date modelling only. It does not connect Google Calendar, Outlook Calendar, bank feeds, email, accounting, OAuth, OCR automation, or AI actions.

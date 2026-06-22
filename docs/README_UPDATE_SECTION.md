# README Update Section

Copy this into the top half of the LifeOS README.

---

## Platform Direction

LifeOS is a weekly pressure command centre.

It shows what money, work, payments, deductions, appointments, tasks, messages, and follow-ups are putting pressure on the week, then helps decide what is safe to do next.

The desktop app is the daily-use power version and proving ground.

The mobile app will be the daily-use optimized version and pressure test.

Both desktop and mobile share the same core LifeOS model.

TimerAgent is a desktop-only utility that feeds work, time, income, and tax set-aside data into LifeOS.

## Current Status

- TimerAgent shipped as the first desktop-only utility
- LifeOS Desktop Shell v0.1 in progress
- Shared Core/Shared architecture planned
- Mobile app planned, not built yet
- Website planned, not built yet

## Current Modules

- Command Centre dashboard
- TimerAgent
- Money Pressure placeholder
- Agenda placeholder
- Follow-Ups placeholder
- Projects placeholder
- Settings placeholder

## Not Built Yet

- Full money pressure module
- Mobile app
- Website
- Bank sync
- Cloud accounts
- Invoice generation
- Calendar/email imports
- Public SaaS portal

## Architecture Rule

Core features should reach both desktop and mobile.

Experimental features start on desktop.

Platform-specific features stay platform-specific.

Desktop can move ahead, but shared LifeOS logic should not be trapped inside desktop-only UI code.

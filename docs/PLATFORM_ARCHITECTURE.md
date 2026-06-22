# LifeOS Platform Architecture

LifeOS is a weekly pressure command centre.

It is not mainly a budget app, calendar app, task app, timer app, or banking app. Those are modules and inputs. LifeOS is the pressure layer that connects money, time, work, appointments, tasks, deductions, payments, and follow-ups into one weekly decision system.

## Core Mental Model

Desktop is the daily-use power version and proving ground.

Mobile is the daily-use optimized version and pressure test.

Core is the shared system that keeps both versions aligned.

TimerAgent is a desktop-only utility that feeds LifeOS with work, time, income, and tax set-aside data.

Website is the public showroom for explaining the project, roadmap, screenshots, and releases. It is not the app.

## Permanent Product Rule

Desktop leads.

Core logic is shared.

Mobile receives the proven, simplified, optimized version.

Platform-specific features stay platform-specific.

## Project Roles

### LifeOS.Core

Shared business logic, calculations, entities, and pressure rules.

This project should contain logic that both desktop and mobile need.

Examples:

- Money pressure
- Safe-to-spend
- Income states
- Deductions
- Pay-later schedules
- Agenda items
- Follow-ups
- Weekly close-out
- Pressure score
- What-can-I-move logic
- Can-I-afford-this logic

### LifeOS.Shared

Shared app-facing models, DTOs, validation helpers, module definitions, and theme tokens.

This project should help desktop and mobile stay aligned without forcing them to share the same UI.

Examples:

- Module definitions
- Dashboard card definitions
- Navigation labels
- View-model style data objects
- Validation messages
- Status labels
- Theme mode enum: Light / Dark / System
- Shared copy/text constants where useful

### LifeOS.Desktop

The full desktop LifeOS app.

Desktop is allowed to move ahead first because it is easier to test complex workflows, dashboards, planning screens, and debugging tools.

Desktop should support daily use, not just admin use.

Desktop can contain:

- Command Centre
- Money Pressure
- Agenda
- Follow-Ups
- Projects
- Reports
- Weekly Close-Out
- Settings
- Desktop-specific dashboards
- Richer tables and planning screens
- Debug/admin tools where useful

### LifeOS.Mobile

The future full mobile LifeOS app.

Mobile is not a sidekick app and not a stripped-down afterthought. It should eventually support the same core LifeOS model as desktop, but with phone-first screens and optimized daily-use flows.

Mobile should force the model to stay simple, fast, and usable.

Mobile can contain:

- Today
- Money Pressure
- Safe-to-Spend
- Agenda
- Tasks
- Follow-Ups
- Pay-Later
- Deductions
- Weekly Close-Out
- Pressure Score
- Settings

Mobile should not blindly copy desktop screens. It should use the same shared model with a stricter interface.

### LifeOS.TimerAgent

Desktop-only work timer utility.

TimerAgent tracks focused work, billable sessions, earned income, tax set-aside, safe money, and CSV logs.

TimerAgent feeds work/time/income data into LifeOS, but TimerAgent is not the whole LifeOS platform.

TimerAgent can remain Windows desktop-only because its core UX depends on:

- Tray icon
- Global shortcut
- Hideable compact window
- Desktop work sessions
- Local CSV logging
- Keyboard/mouse workflow

Mobile may later view TimerAgent summaries or work-session history, but the TimerAgent tray/hotkey/overlay utility remains desktop-only.

## Feature Lifecycle

New shared feature idea:

1. Build or prototype on desktop.
2. Test whether the workflow actually reduces weekly pressure.
3. Move reusable logic into LifeOS.Core or LifeOS.Shared.
4. Simplify the flow.
5. Bring the optimized version to mobile.

## Feature Categories

### Shared Core Features

These should eventually reach both desktop and mobile:

- Manual money pressure
- Safe-to-spend
- Income tracking
- Paid / unpaid / expected / overdue income states
- Deductions
- Pay-later tracking
- Agenda
- Tasks
- Follow-ups
- Weekly close-out
- Pressure score
- What can I move?
- Can I afford this?
- Settings and preferences

### Desktop-Ahead Features

These can exist on desktop first and may stay ahead long-term:

- Experimental modules
- Advanced reports
- Complex dashboards
- Debug/admin screens
- Heavy planning views
- Larger data-entry screens
- Import review tools
- Desktop-specific work tools

### Platform-Specific Features

Desktop-only:

- TimerAgent tray utility
- Global hotkey
- Compact overlay
- Desktop CSV work-session workflow
- Desktop file picker/backup tools where appropriate

Mobile-specific:

- Phone-first daily check-in
- Quick capture
- Touch-optimized agenda
- Mobile notifications later
- Mobile-safe short flows
- Offline-first mobile UX later

## Guardrails

- Do not trap business logic inside WPF.
- Do not duplicate calculations separately on desktop and mobile.
- Do not make mobile a bad port.
- Do not make TimerAgent the whole product.
- Do not build bank sync before the manual model is valuable.
- Do not silently import email/calendar/banking data later; use review queues.
- Do not build every module at once.
- Every feature must reduce weekly pressure or make the next safe action clearer.

## Current Phase

TimerAgent is shipped as the first desktop-only utility.

LifeOS.Desktop Shell v0.1 is the next platform step.

The shell should prove:

- Desktop navigation
- Command Centre framing
- Module boundaries
- TimerAgent as a desktop-only input utility
- Shared desktop/mobile architecture direction

The shell does not need to build full money, agenda, follow-up, mobile, website, or integration features yet.

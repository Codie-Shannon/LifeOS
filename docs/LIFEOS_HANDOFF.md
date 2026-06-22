# LifeOS Full Document Pack - Handoff

Generated: 2026-06-22

## Current Product Definition

LifeOS is a weekly pressure command centre.

It shows what money, work, payments, deductions, appointments, tasks, messages, and follow-ups are putting pressure on the week, then helps decide what is safe to do next.

LifeOS is not mainly a budget app, calendar app, task app, timer app, or banking app. Those are modules and inputs. LifeOS is the pressure layer that connects them.

## Current Mental Model

Desktop is the daily-use power version and proving ground.

Mobile is the daily-use optimized version and pressure test.

Core is the shared system that keeps both versions aligned.

TimerAgent is a desktop-only utility that feeds LifeOS with work, time, income, and tax set-aside data.

Website is the public showroom for explaining the project, roadmap, screenshots, and releases.

## Permanent Architecture Rule

Core features should reach both desktop and mobile.

Experimental features start on desktop.

Platform-specific features stay platform-specific.

Desktop can move ahead, but shared LifeOS logic should not be trapped inside desktop-only UI code.

## Current Build State

- TimerAgent is shipped as the first desktop-only utility.
- LifeOS.Desktop Shell v0.1 Phase 4 is complete.
- Phase 4 added a WPF desktop shell with:
  - Command Centre default page
  - sidebar navigation
  - Money Pressure placeholder
  - Agenda placeholder
  - Follow-Ups placeholder
  - Projects placeholder
  - TimerAgent placeholder
  - Settings placeholder
- Phase 5 documentation has been generated:
  - PLATFORM_ARCHITECTURE.md
  - MOBILE_PLAN.md
  - WEBSITE_PLAN.md
  - README_UPDATE_SECTION.md

## Suggested Solution Structure

```text
LifeOS.Core
LifeOS.Shared
LifeOS.Modules.Timer
LifeOS.TimerAgent
LifeOS.Desktop
Future LifeOS.Mobile
Future LifeOS.Web or static website
```

## Project Roles

### LifeOS.Core

Shared business logic, calculations, entities, and pressure rules.

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

Examples:

- Module definitions
- Dashboard card definitions
- Navigation labels
- View-model style data objects
- Validation messages
- Status labels
- Theme mode enum: Light / Dark / System

### LifeOS.Desktop

Full desktop LifeOS app.

Desktop is the proving ground and daily-use power version.

### LifeOS.Mobile

Future full mobile LifeOS app.

Mobile is not a sidekick app. It should eventually support the same core LifeOS modules as desktop, but with phone-first screens and optimized workflows.

### LifeOS.TimerAgent

Desktop-only work timer utility.

TimerAgent tracks focused work, billable sessions, earned income, tax set-aside, safe money, and CSV logs.

TimerAgent feeds LifeOS work/time/income data, but TimerAgent is not the whole platform.

## Current Roadmap

1. TimerAgent / Work Log
2. LifeOS Desktop Shell
3. Manual Weekly Money Pressure
4. Pay-Later Tracker
5. Money Profile / Hidden Deductions
6. Agenda Calendar
7. Waiting On / Follow-Ups
8. Weekly Close-Out
9. Pressure Engine Features
10. Google/Microsoft Import
11. Bank/Open Banking later

## Next Practical Build Step

After Phase 5 docs are committed, the next best step is probably Phase 6:

- create or strengthen LifeOS.Shared
- add shared module definitions
- use those module definitions to populate the desktop shell cards/nav
- avoid hardcoding the whole product into WPF
- keep business logic out of MainWindow.xaml.cs

## Suggested Git Commits

If Phase 4 is not committed:

```powershell
git add LifeOS.Desktop/MainWindow.xaml LifeOS.Desktop/MainWindow.xaml.cs
git commit -m "Add LifeOS desktop shell foundation"
```

For Phase 5 docs:

```powershell
git add docs/PLATFORM_ARCHITECTURE.md docs/MOBILE_PLAN.md docs/WEBSITE_PLAN.md README.md
git commit -m "Document LifeOS platform architecture and plans"
```

Push branch:

```powershell
git push -u origin feature/lifeos-desktop-shell-v01
```

## Guardrails

Do not build everything at once.

Do not build mobile today.

Do not build website today.

Do not add bank sync yet.

Do not turn TimerAgent into the whole app.

Do not trap shared rules in WPF event handlers.

Do not duplicate calculations separately between desktop and mobile.

Every feature must reduce weekly pressure or make the next safe action clearer.

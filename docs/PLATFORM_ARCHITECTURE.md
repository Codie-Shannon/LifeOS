# LifeOS Platform Architecture

LifeOS is a weekly pressure command centre.

The platform is split into shared core logic, desktop UI, mobile UI, and desktop-only utilities.

## Mental Model

Desktop is the daily-use power version and proving ground.

Mobile is the daily-use optimized version and pressure test.

Core is the shared system that keeps both versions aligned.

TimerAgent is a desktop-only utility that feeds LifeOS with work, time, income, and tax set-aside data.

## Project Roles

### LifeOS.Core

Shared business logic, calculations, entities, and pressure rules.

### LifeOS.Shared

Shared app-facing models, storage helpers, module definitions, view-model style objects, and platform-level helpers.

### LifeOS.Desktop

Full desktop LifeOS app.

Desktop is allowed to move ahead first because it is easier to test complex workflows, dashboards, and planning screens.

### LifeOS.Mobile

Future full mobile LifeOS app.

Mobile should contain the same core LifeOS system, but with phone-first screens and optimized daily-use flows.

### LifeOS.TimerAgent

Desktop-only work timer utility.

TimerAgent provides work-session data to LifeOS but is not the whole LifeOS platform.

## Permanent Rule

Core features should reach both desktop and mobile.

Experimental features start on desktop.

Platform-specific features stay platform-specific.

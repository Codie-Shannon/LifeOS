# TimerAgent v0.1 Release Notes

**Version:** TimerAgent v0.1  
**Date:** 2026-06-22  
**Status:** Initial usable proof release

---

## Summary

TimerAgent v0.1 is the first usable module of Life OS.

It is a local WPF desktop timed-task manager designed to track work sessions, people/organisations, billable time, tax set-aside, safe-after-tax money, and CSV proof logs.

This release focuses on the core daily loop:

```text
Create task → link contact → start timer → pause/resume → stop → log session → review task totals
```

---

## Included in v0.1

### Timed tasks

- Create timed tasks
- Edit timed tasks
- Archive timed tasks
- Link tasks to contacts/people/organisations
- Store task type, mode, work type, hourly rate, tax percentage, billable status, and description

### Contacts / people

- Create contacts
- Edit contacts
- Archive contacts
- Store default work type, default rate, and default tax percentage
- Reuse contacts when creating timed tasks

### Timer controls

- Start
- Pause
- Resume
- Stop
- Direct list actions
- Compact Timer page as work mode
- Smart timer action button that changes between Start, Pause, and Resume

### Logging and totals

- CSV log written when a task is stopped
- Task-specific totals on the Timer page
- Current work day, this week, and all time totals
- Earned amount
- Tax set-aside
- Safe-after-tax amount

### Persistence

- Local JSON state
- Local CSV timer logs
- Saved contacts
- Saved timed tasks
- Saved selected task/contact state

### Desktop utility behavior

- Hide/tray behavior
- Global hotkey support
- Compact timer work view
- Full manager view for task/contact management

### UI polish

- Full-width task/contact cards
- Compact Timer page layout
- Styled buttons
- Fixed hover states
- Styled dropdowns
- Clearer labels:
  - `Project` renamed to `Description`
  - `Today` clarified as `Current work day`
  - Timer page totals clarified as task-specific totals

---

## Known limitations

- No installer/package yet
- No cloud sync
- No external database
- No invoice generator
- No reporting dashboard beyond current Timer page totals
- No settings page yet
- Work-day cutoff/gap rules are not user-configurable yet
- No Google/Microsoft calendar integration
- No email or bank integration
- No mobile app
- No full Life OS dashboard yet
- CSV logs are append-only and are not managed through a full in-app history screen yet

---

## Suggested next version

### TimerAgent v0.2 candidates

- Settings screen
- Configurable work-day cutoff
- Session history page
- Export summary by contact/task/date range
- Invoice-ready summary export
- Better archive/restore management
- More explicit handling for linked/waiting/countdown modes

---

## Release decision

TimerAgent v0.1 is considered complete when:

- the app builds,
- the core timer flow works,
- local state persists,
- CSV logs write,
- compact timer mode works,
- screenshots/docs are present,
- and the release is tagged in Git.

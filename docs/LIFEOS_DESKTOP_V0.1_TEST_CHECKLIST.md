# LifeOS Desktop v0.1 Test Checklist

## Build

- [ ] Solution builds successfully
- [ ] LifeOS.Desktop runs
- [ ] No startup crash

## Navigation

- [ ] Command Centre opens
- [ ] Money Pressure opens
- [ ] Agenda opens
- [ ] Follow-Ups opens
- [ ] Projects opens
- [ ] TimerAgent opens
- [ ] Settings opens

## Money Pressure

- [ ] Current balance can be edited
- [ ] Paid income can be edited
- [ ] Pending income can be edited
- [ ] Bills due can be edited
- [ ] Deductions can be edited
- [ ] Food/fuel buffer can be edited
- [ ] Emergency buffer can be edited
- [ ] Recalculate updates safe-to-spend
- [ ] Save Inputs works
- [ ] Reset Defaults works
- [ ] Saved values load after app restart
- [ ] Pending income is shown separately from safe money
- [ ] Bills and deductions reduce safe-to-spend

## Follow-Ups

- [ ] Follow-up can be added
- [ ] Follow-up can be saved
- [ ] Follow-up can be marked complete
- [ ] Follow-up can be deleted
- [ ] Reset Defaults works
- [ ] Saved follow-ups load after app restart
- [ ] Open follow-up count updates
- [ ] Needs-action count updates
- [ ] Money-linked count updates
- [ ] Overdue/due-today logic works where applicable

## Command Centre

- [ ] Command Centre reads saved Money Pressure data
- [ ] Command Centre reads saved Follow-Ups data
- [ ] Safe-to-spend appears
- [ ] Pending income appears
- [ ] Open follow-ups appear
- [ ] Needs-action count appears
- [ ] Money-linked count appears
- [ ] Next safest action appears
- [ ] Combined pressure reasons appear

## Local Persistence

- [ ] Money Pressure JSON file is created
- [ ] Follow-Ups JSON file is created
- [ ] App still opens if JSON files are missing
- [ ] Reset buttons restore default data safely

## Guardrails

- [ ] No database added
- [ ] No bank sync added
- [ ] No email/calendar import added
- [ ] No mobile app added
- [ ] No website app added
- [ ] TimerAgent remains desktop-only
- [ ] Shared logic is not trapped inside WPF-only code

# LifeOS Desktop v0.4 Test Checklist

This checklist merges the v0.3 functionality checks with the v0.4 trust-polish checks.

## Build

- [ ] Run `dotnet build`
- [ ] Confirm build succeeds
- [ ] Launch LifeOS Desktop
- [ ] Confirm no startup crash

## Shell / navigation

- [ ] Sidebar loads
- [ ] Desktop label shows v0.4
- [ ] Command Centre opens
- [ ] Money Pressure opens
- [ ] Agenda opens
- [ ] Pay Later opens
- [ ] Weekly Close-Out opens
- [ ] Work Sessions opens
- [ ] Proof Tracker opens
- [ ] Follow-Ups opens
- [ ] Projects opens
- [ ] TimerAgent opens
- [ ] Settings opens

## Command Centre

- [ ] Command Centre title displays correctly
- [ ] v0.4 trust wording appears
- [ ] Overall pressure metric appears
- [ ] Safe to spend metric appears
- [ ] Agenda metric appears
- [ ] Pay Later metric appears
- [ ] Follow-ups metric appears
- [ ] Billable value metric appears
- [ ] Unpaid work metric appears
- [ ] Proof items metric appears
- [ ] Footer/status copy is readable

## Agenda

- [ ] Agenda page opens
- [ ] Add agenda item form appears
- [ ] Empty state is clear
- [ ] Reset Defaults button exists
- [ ] Reset confirmation appears if configured
- [ ] Adding an item still works

## Pay Later

- [ ] Pay Later page opens
- [ ] Add pay-later item form appears
- [ ] Empty state is clear
- [ ] Reset Defaults button exists
- [ ] Reset confirmation appears if configured
- [ ] Adding an item still works

## Weekly Close-Out

- [ ] Weekly Close-Out page opens
- [ ] Existing fields still render
- [ ] No v0.3-only broken labels remain

## Work Sessions

- [ ] Work Sessions page opens
- [ ] Add work session form appears
- [ ] Empty state is clear
- [ ] Billable checkbox appears
- [ ] Reset Defaults button exists
- [ ] Adding a work session still works

## Proof Tracker

- [ ] Proof Tracker page opens
- [ ] Add proof item form appears
- [ ] Empty state is clear
- [ ] Reset Defaults button exists
- [ ] Reset confirmation appears
- [ ] Confirmation copy warns that saved proof items may be replaced
- [ ] Adding a proof item still works

## v0.4 trust-polish checks

- [ ] No page describes itself as v0.3 in active UI
- [ ] Empty states give useful next-action guidance
- [ ] Reset/delete actions are not silent
- [ ] Copy does not imply the app does more than it currently does
- [ ] Command Centre wording feels accurate for local data
- [ ] Docs and screenshots match the current UI

## Screenshots

- [ ] Command Centre screenshot captured
- [ ] Agenda empty-state screenshot captured
- [ ] Pay Later empty-state screenshot captured
- [ ] Work Sessions empty-state screenshot captured
- [ ] Proof Tracker empty-state screenshot captured
- [ ] Reset confirmation screenshot captured

## Release gate

Release v0.4 only when:

- [ ] build passes
- [ ] main navigation works
- [ ] screenshots are current
- [ ] docs say v0.4
- [ ] release tag is ready

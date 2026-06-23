# LifeOS Desktop v0.2 Test Checklist

## Purpose

This checklist confirms that LifeOS Desktop v0.2 works as a local-first weekly pressure command centre.

## Build

- [ ] Run `dotnet build` from the solution root.
- [ ] Confirm all projects build successfully.
- [ ] Run `dotnet run --project .\LifeOS.Desktop\LifeOS.Desktop.csproj`.
- [ ] Confirm the desktop app opens without crashing.

## Navigation

- [ ] Command Centre opens.
- [ ] Money Pressure opens.
- [ ] Agenda opens.
- [ ] Pay Later opens.
- [ ] Weekly Close-Out opens.
- [ ] Follow-Ups opens.
- [ ] Projects placeholder opens.
- [ ] TimerAgent placeholder/status opens.
- [ ] Settings placeholder opens.

## Command Centre

- [ ] Shows overall pressure.
- [ ] Shows safe-to-spend.
- [ ] Shows pending income.
- [ ] Shows open follow-ups.
- [ ] Shows needs-action follow-ups.
- [ ] Shows money-linked follow-ups.
- [ ] Shows next safest action.
- [ ] Shows pressure reasons.

## Money Pressure

- [ ] Current balance input is visible.
- [ ] Paid income input is visible.
- [ ] Pending income input is visible.
- [ ] Bills due input is visible.
- [ ] Deductions input is visible.
- [ ] Food/fuel buffer input is visible.
- [ ] Emergency buffer input is visible.
- [ ] Recalculate button works.
- [ ] Save Inputs button works.
- [ ] Reset Defaults button works.
- [ ] Returning to Money Pressure reloads saved values.

## Agenda

- [ ] Agenda page opens without a message box placeholder.
- [ ] Agenda metrics are visible.
- [ ] Add agenda form is visible.
- [ ] Add Agenda Item creates an item.
- [ ] New item appears in the list.
- [ ] Mark Complete updates the item.
- [ ] Delete removes the item.
- [ ] Reset Defaults reloads demo/default data.
- [ ] App restart preserves saved Agenda items.

## Pay Later

- [ ] Pay Later page opens without a message box placeholder.
- [ ] Pay Later metrics are visible.
- [ ] Add pay-later form is visible.
- [ ] Add Pay Later Item creates an item.
- [ ] New item appears in the list.
- [ ] Mark Paid updates the item.
- [ ] Delete removes the item.
- [ ] Reset Defaults reloads demo/default data.
- [ ] App restart preserves saved Pay Later items.

## Weekly Close-Out

- [ ] Weekly Close-Out page opens without a message box placeholder.
- [ ] Weekly Close-Out metrics are visible.
- [ ] Add weekly close-out form is visible.
- [ ] Add Close-Out Entry creates an entry.
- [ ] New entry appears in the list.
- [ ] Delete removes the entry.
- [ ] Reset Defaults reloads demo/default data.
- [ ] App restart preserves saved Weekly Close-Out entries.

## Follow-Ups

- [ ] Follow-Ups page opens.
- [ ] Follow-Up metrics are visible.
- [ ] Add Follow-Up creates an item.
- [ ] Mark Complete updates the item.
- [ ] Delete removes the item.
- [ ] Save Follow-Ups works.
- [ ] Reset Defaults works.
- [ ] App restart preserves saved Follow-Ups.

## Screenshot checks

- [ ] Command Centre screenshot captured.
- [ ] Agenda screenshot captured.
- [ ] Pay Later screenshot captured.
- [ ] Weekly Close-Out screenshot captured.
- [ ] Money Pressure screenshot captured.
- [ ] Follow-Ups screenshot captured.
- [ ] README screenshot path points to `docs/screenshots/v0.2/01-lifeos-v02-command-centre.png`.

## Release checks

- [ ] README updated to v0.2.
- [ ] Roadmap marks v0.2 as done.
- [ ] Release notes created.
- [ ] Screenshot list created.
- [ ] Old v0.1 root screenshots removed or archived if no longer needed.
- [ ] Commit created with message: `Release LifeOS v0.2`.
- [ ] Repo pushed.

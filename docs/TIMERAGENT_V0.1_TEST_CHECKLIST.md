# TimerAgent v0.1 Test Checklist

Use this checklist before tagging/releasing TimerAgent v0.1.

## Build and launch

- [ ] `dotnet build` passes
- [ ] `dotnet run --project src/LifeOS.TimerAgent` launches the app
- [ ] App opens without crashing
- [ ] App opens on the full task/list manager view
- [ ] No obvious clipped controls in full mode
- [ ] No obvious clipped controls in compact timer mode

## Task list

- [ ] Task list shows full-width cards
- [ ] Selected task visual state is readable
- [ ] Create timed task button opens the task details page
- [ ] Selecting a task updates the selected/current task
- [ ] Start/Pause/Resume action from list works
- [ ] Stop action from list works
- [ ] Edit action opens the selected task in the details page

## Timed task details

- [ ] Task/project name saves correctly
- [ ] Description field is labelled correctly
- [ ] Description field supports multi-line text
- [ ] Contact dropdown works
- [ ] Work type saves correctly
- [ ] Task type dropdown works
- [ ] Mode dropdown works
- [ ] Hourly rate saves correctly
- [ ] Tax percentage saves correctly
- [ ] Billable checkbox saves correctly
- [ ] Edited task persists after returning to the list

## People / contacts

- [ ] People page opens
- [ ] Create contact opens contact details
- [ ] Display name saves correctly
- [ ] Contact type dropdown works
- [ ] Default work type saves correctly
- [ ] Default rate saves correctly
- [ ] Default tax percentage saves correctly
- [ ] Contact appears in task contact dropdown
- [ ] Contact can be archived
- [ ] App handles no/archived contact safely

## Timer page / compact work mode

- [ ] Clicking Timer enters compact timer mode
- [ ] Compact timer window size feels correct
- [ ] Compact timer shows selected/active task name
- [ ] Compact timer shows timer duration
- [ ] Compact timer shows earned/tax/safe money line
- [ ] Smart timer action button shows `Start` when stopped
- [ ] Smart timer action button shows `Pause` when running
- [ ] Smart timer action button shows `Resume` when paused
- [ ] Stop button is enabled only when running/paused
- [ ] Hide button hides the window
- [ ] List button returns to the full task manager view

## Timer behavior

- [ ] Start begins timing
- [ ] Pause freezes timing
- [ ] Resume continues timing
- [ ] Stop ends the session
- [ ] Stop writes a CSV log entry
- [ ] Starting an exclusive task pauses other exclusive running tasks
- [ ] Running/paused state updates the UI correctly
- [ ] Active task and selected task status display correctly

## Totals and money

- [ ] Timer page totals are task-specific
- [ ] Switching tasks changes the displayed totals
- [ ] Current work day total updates correctly
- [ ] This week total updates correctly
- [ ] All time total updates correctly
- [ ] Earned amount uses hourly rate correctly
- [ ] Tax set-aside amount displays correctly
- [ ] Safe-after-tax amount displays correctly
- [ ] Non-billable tasks do not show misleading earned money

## Persistence

- [ ] Contacts persist after close/reopen
- [ ] Timed tasks persist after close/reopen
- [ ] Selected task persists after close/reopen
- [ ] Selected contact persists after close/reopen
- [ ] Running/paused task state loads safely
- [ ] Broken JSON state is handled safely if applicable
- [ ] CSV log remains readable after multiple stopped sessions

## Tray / hotkey

- [ ] Hide sends the app to hidden/tray state
- [ ] Tray icon can show the app again
- [ ] Tray icon can exit the app
- [ ] Ctrl+Alt+Space toggles the app visibility
- [ ] App disposes tray/hotkey resources on exit

## Final release checks

- [ ] README screenshots are added
- [ ] Screenshot list is complete
- [ ] Release notes are complete
- [ ] Known limitations are documented
- [ ] Final build passes on `main`
- [ ] Git status is clean
- [ ] v0.1 tag is created

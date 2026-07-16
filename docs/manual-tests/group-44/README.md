# Group 44 manual verification

Group 44 migrates existing LifeOS capability into the eight permanent v8 workspaces while preserving the existing record stores, safety boundaries and module behaviour.

## Locked boundaries

- Eight core pages only: Home, Work, Career, Money, Life, Projects, Assistant and Settings.
- Career remains separate from Work.
- Workspace summary cards do not create duplicate editors or duplicate persisted state.
- Focused module actions open the existing canonical module implementation and record store.
- Legacy routes remain internally reachable through Group 44 and are removed only in Group 45.
- Emergency Stop and Assistant execution/refusal boundaries remain unchanged.
- Final Settings persistence, High Contrast, Reduced Motion, text scaling and legacy removal remain Group 45.

## Manual test sequence

1. Launch LifeOS Desktop v8 and verify Home opens by default.
2. Open each of the eight rail destinations using the mouse.
3. Repeat using `Alt+1` through `Alt+8`.
4. Verify Home balances agenda, follow-up, work-pipeline and pay-later summaries without an editing surface.
5. Open Work and verify Work Pipeline, Follow-Ups, Paid Work Centre, Work Sessions and Timesheet Evidence are present.
6. Open Career and verify Profile, CVs, Applications and Interviews are separate native surfaces; Relationship Radar remains the canonical follow-up route.
7. Open Money and verify all seven assigned money modules are present.
8. Open Life and Projects and verify each assigned module appears only in its intended workspace.
9. Open Assistant and verify Assistant, Memory, Search / Knowledge, Integration Inbox and Email Radar remain review-first.
10. Open Settings and verify it contains only system, safety, diagnostic and release surfaces.
11. Open at least one module from each workspace and verify the preserved window opens directly to the requested module.
12. In Work, create or inspect a fictional work session/timesheet record, close the preserved window, reopen it and verify the canonical record/count remains unchanged.
13. Toggle Comfortable and Compact density and verify cards reflow without changing records.
14. Open the contextual panel and verify it lists workspace contents but contains no duplicate editor.
15. Press `Ctrl+K`, navigate to a workspace and switch density through commands.
16. Arm and reset Emergency Stop; verify the Assistant still refuses execution/mutation requests.
17. Resize to the minimum supported window and verify navigation, top bar and active workspace remain usable.
18. Run the full Pack 1 validation and confirm tests, builds, scans, synchronization and clean-tree checks pass.

## Screenshot gate

Do not create or commit Group 44 screenshots until Pack 1 is complete and the UI has been manually reviewed. Pack 2 must contain exactly the eight approved filenames defined by the Group 44 screenshot README and evidence manifest.

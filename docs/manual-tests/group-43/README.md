# Group 43 manual tests - Desktop v8 shell and identity foundation

## Entry conditions
- Repository is on the exact Group 42 baseline.
- Pack 1 validation completed without errors.
- Run the Desktop from a clean Release build.

## Required checks
1. Desktop opens to **Home** in the new v8 shell.
2. Home balances Today/pressure, Work/client follow-ups and Money/upcoming payments.
3. Each of the eight rail destinations opens: Home, Work, Career, Money, Life, Projects, Assistant and Settings.
4. Career remains separate from Work.
5. `Ctrl+K` opens the command surface; Escape closes it; commands `Home`, `Work`, `Money`, `Theme light`, `Theme dark` and `Legacy` work.
6. Alt+1 through Alt+8 selects the eight workspaces in order.
7. Review, Status, Stop, Profile and contextual-panel controls expose textual state.
8. The contextual panel opens and closes without replacing the active workspace.
9. The window remains usable at its minimum size; keyboard focus is visible.
10. `Open preserved v7 modules` opens the legacy Desktop frame and its existing routes remain usable.
11. Dark and Light resource dictionaries render without missing resources.
12. Shell, navigation, command, top-bar and context controls have unique AutomationIds.

## Group boundary
Do not migrate every module, delete legacy routes, complete theme Settings or begin integrations in Group 43.

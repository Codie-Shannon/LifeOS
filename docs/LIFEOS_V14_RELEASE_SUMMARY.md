# LifeOS Current Repository Summary

LifeOS is currently in the v13 Household and Grocery release lane.

## Current checkpoint

- Group 64 completed Grocery Planning and recurring essentials.
- Group 65 adds household inventory, meal ingredients, store profiles and price-context boundaries.
- Group 66 closes v13 with shared household routines, replenishment review, receipt/spending review and release closure checks.
- Group 67 starts the v14 work-session and billable-records lane.

## Group 65 scope

- Household inventory records use explicit stock states: out, low, enough, overstocked and unknown.
- Meal recipes can expose required ingredient gaps.
- Store profiles remain planning context only.
- Desktop and Full Mobile now expose Group 65 household inventory review surfaces.
- No inventory, meal or store signal can order, pay, trust prices, mutate external carts or mutate grocery lists automatically.

## Group 66 scope

- Household routines and assignments remain explicit review states.
- Replenishment review connects inventory, meal and routine signals without changing grocery lists automatically.
- Household receipt candidates suggest Document and Money targets without mutating either system.
- Planned-vs-actual household spending is review context, not advice or payment initiation.
- v13 closure checks remain fail-closed until evidence and validation are captured.

## Validation expectation

- `dotnet test .\LifeOS.slnx`
- `dotnet build .\src\LifeOS.Core\LifeOS.Core.csproj -c Release --no-restore`
- `dotnet build .\LifeOS.Desktop\LifeOS.Desktop.csproj -c Release --no-restore`
- `dotnet build .\src\LifeOS.Mobile\LifeOS.Mobile.csproj -c Release --no-restore`
- `git diff --check`

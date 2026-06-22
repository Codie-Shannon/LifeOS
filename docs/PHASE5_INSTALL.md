# LifeOS Phase 5 Docs Install

These are the Phase 5 documentation files for the updated LifeOS platform model.

## Files included

- PLATFORM_ARCHITECTURE.md
- MOBILE_PLAN.md
- WEBSITE_PLAN.md
- README_UPDATE_SECTION.md

## Where to put them

Create a docs folder if needed:

```powershell
mkdir docs
```

Copy these files into:

```text
docs/PLATFORM_ARCHITECTURE.md
docs/MOBILE_PLAN.md
docs/WEBSITE_PLAN.md
```

Open `README_UPDATE_SECTION.md` and copy the section into the top half of your main `README.md`.

## Suggested git commands

From the solution root:

```powershell
git status
git add docs/PLATFORM_ARCHITECTURE.md docs/MOBILE_PLAN.md docs/WEBSITE_PLAN.md README.md
git commit -m "Document LifeOS platform architecture and plans"
```

## Phase 5 pass condition

You pass Phase 5 when:

- docs/PLATFORM_ARCHITECTURE.md exists
- docs/MOBILE_PLAN.md exists
- docs/WEBSITE_PLAN.md exists
- README explains the desktop/mobile/Core/TimerAgent model
- no mobile app is built yet
- no website is built yet

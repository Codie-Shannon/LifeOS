# TimerAgent v0.1 Docs Commands

Run from the repository root.

## Create docs branch

```powershell
git checkout main
git pull origin main
git checkout -b docs/timeragent-v0.1-readme
```

## Add generated docs

Copy these generated files into your repo:

```text
README.md
docs/TIMERAGENT_V0.1_TEST_CHECKLIST.md
docs/TIMERAGENT_V0.1_RELEASE_NOTES.md
docs/TIMERAGENT_V0.1_SCREENSHOT_LIST.md
docs/screenshots/
```

## Add screenshots

Take screenshots and save them using the exact filenames listed in:

```text
docs/TIMERAGENT_V0.1_SCREENSHOT_LIST.md
```

## Build and commit

```powershell
dotnet build
git status
git add README.md docs/
git commit -m "Document TimerAgent v0.1"
git push -u origin docs/timeragent-v0.1-readme
```

## Merge docs into main

```powershell
git checkout main
git pull origin main
git merge docs/timeragent-v0.1-readme
dotnet build
git push origin main
```

## Tag v0.1

```powershell
git tag timeragent-v0.1
git push origin timeragent-v0.1
```

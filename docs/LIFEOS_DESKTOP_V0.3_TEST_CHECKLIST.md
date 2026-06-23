# LifeOS Desktop v0.3 Test Checklist

Use this checklist before committing the v0.3 release.

## Build

- [ ] `dotnet build` succeeds
- [ ] Git working tree is clean before release commit
- [ ] App launches without errors

## Navigation

- [ ] Command Centre opens
- [ ] Money Pressure opens
- [ ] Agenda opens
- [ ] Pay Later opens
- [ ] Weekly Close-Out opens
- [ ] Work Sessions opens
- [ ] Proof Tracker opens
- [ ] Follow-Ups opens
- [ ] Projects placeholder opens
- [ ] TimerAgent placeholder opens
- [ ] Settings placeholder opens

## v0.3 Work Sessions

- [ ] Work Sessions screen shows real content, not a placeholder
- [ ] Summary cards display total sessions, hours, billable hours, billable value, unpaid value, and clients/projects
- [ ] Adding a work session works
- [ ] Marking a work session paid works if available
- [ ] Deleting a work session works
- [ ] Work session data persists after app restart

## v0.3 Proof Tracker

- [ ] Proof Tracker screen shows real content, not a placeholder
- [ ] Summary cards display total proof, ready, shared, accepted, client proof, and recent proof
- [ ] Adding a proof item works
- [ ] Marking proof ready/shared/accepted works if available
- [ ] Deleting a proof item works
- [ ] Proof data persists after app restart

## Command Centre

- [ ] Command Centre shows v0.3 work/income/proof wording
- [ ] Command Centre includes billable value
- [ ] Command Centre includes unpaid work
- [ ] Command Centre includes proof items
- [ ] Command Centre still includes money pressure
- [ ] Command Centre still includes agenda/pay-later/follow-up pressure

## v0.2 Carry-Forward Screens

- [ ] Agenda still works
- [ ] Pay Later still works
- [ ] Weekly Close-Out still works
- [ ] Money Pressure still works
- [ ] Follow-Ups still works

## Docs

- [ ] README says LifeOS Desktop v0.3
- [ ] README screenshot paths point to `docs/screenshots/v0.3/`
- [ ] Roadmap marks v0.1, v0.2, and v0.3 as done
- [ ] Release notes exist
- [ ] Screenshot list exists
- [ ] Old v0.2 docs/screenshots are removed if doing a clean v0.3-only docs release

## Release Commit

Suggested commit:

```powershell
git add .
git commit -m "Release LifeOS v0.3"
git push
```

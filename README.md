# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-beta.1 — Controlled automation release checkpoint**

Screenshot Group 30 is complete after the Pack 2 finalization runner passes and pushes the approved evidence. The release adds persisted automation health, exact-scope recovery visibility, a separate fail-closed Emergency Stop, explicit dependency-safe retry and reverse-order rollback review.

## Evidence

- [Group 30 — Controlled automation release checkpoint](docs/screenshot-groups/group-30-automation-hardening/README.md)
- [Group 29 — Controlled orchestration and recovery](docs/screenshot-groups/group-29-controlled-orchestration/README.md)
- [Group 28 — Guarded internal automation](docs/screenshot-groups/group-28-guarded-internal-automation/README.md)

## Safety boundary

Automation remains manual, explicit, foreground-only, typed, Low-risk and reversible. No unattended execution, background worker, OS scheduler, service, startup execution, automatic continuation, automatic retry loop, external write, communication mutation, financial/destructive mutation, arbitrary script/process/plugin execution or AI-controlled action is enabled.

## Build and validation

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
git diff --check
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\validation\Test-RepositoryHygiene.ps1 -RepoPath C:\Projects\LifeOS
```

Group 31 has not started.

## v6.0.0-beta.1 release checkpoint

Screenshot Group 31 is complete. The controlled automation beta checkpoint is evidenced with eight screenshots and manual verification covering approval/execution separation, reversible internal execution, one-step orchestration, failure containment, recovery controls and Emergency Stop persistence. No unattended or external execution is enabled.

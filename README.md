# LifeOS

LifeOS is a local-first personal operating system for turning life, work, money, evidence, follow-ups, projects, relationships and pressure into visible state.

## Current build

**LifeOS Desktop v6.0.0-alpha.4 — Automation hardening and emergency controls**

Screenshot Group 29 remains the completed evidence baseline. Group 30 Pack 1 adds the implementation and manual-verification foundation for persisted automation health, exact-scope incident recovery and a distinct fail-closed Emergency Stop. Group 30 screenshot evidence is not complete until Pack 2 is run.

## Group 30 safety boundary

- Automation remains manual and foreground-only.
- The normal guarded-execution pause and Emergency Stop are separate persisted controls.
- Emergency Stop blocks direct internal execution and every orchestration start, step, retry and rollback boundary.
- Reset requires explicit confirmation, leaves guarded execution paused and never resumes proposals or runs.
- Failures create sanitized incidents scoped to the exact proposal, run or step.
- Retry remains explicit; no retry loop or self-healing mutation exists.
- Rollback review is reverse ordered and stops on the first mismatch or failure.
- External writes, communication, mailbox, calendar, financial, destructive, script, process, plugin and AI actions remain blocked.

## Build and validation

```powershell
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
git diff --check
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\validation\Test-RepositoryHygiene.ps1 -RepoPath C:\Projects\LifeOS
```

The exact Group 30 test count and Release-build result must be recorded by the Pack 1 runner. Group 31 has not started.

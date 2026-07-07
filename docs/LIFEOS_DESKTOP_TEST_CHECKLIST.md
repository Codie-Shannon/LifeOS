# LifeOS Desktop v1.3 Test Checklist

## Build

```powershell
cd C:\Projects\LifeOS
dotnet build
```

Expected:

```text
Build succeeded.
```

## Navigation

Confirm these pages open from the sidebar:

- Command Centre
- Work Pipeline
- Daily State
- Timesheet Evidence
- Evidence Vault

## Command Centre

Confirm the Command Centre shows:

- current release/version text
- next safest action
- what matters now
- hidden/passive waiting items
- weekly pressure summary

## Daily State

Confirm Daily State shows:

- Today visible
- Done today
- Passive waiting
- Do not chase
- Scheduled
- Waiting after send

## Timesheet Evidence

Confirm Timesheet Evidence shows:

- ready for timesheet section
- recent evidence entries section
- time bucket rules
- local storage path

Accepted time buckets:

```text
0.25h = light admin / quick check / short reply
0.5h  = real investigation / review / setup check / structured follow-up
1.0h+ = implementation / testing / proof build / debugging / documentation
```

## Evidence Vault

Confirm Evidence Vault shows:

- evidence needing review
- recent evidence records
- evidence pressure
- v1.3 scope
- local storage path

## Local-first behaviour

Confirm the app references local LifeOS storage and does not require:

- cloud sync
- login/account
- Outlook/Gmail integration
- mobile app
- provider authentication

## Release check

Before tagging or pushing:

```powershell
git status
dotnet build
```

Expected:

```text
working tree clean
Build succeeded
```

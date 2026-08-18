# LifeOS current manual test checklist

Use fictional or sanitized proof data only.

## Build

```powershell
cd C:\Projects\LifeOS
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
```

Expected:

```text
Tests pass.
Release build succeeds.
```

## Current checkpoint

Confirm the repository and visible product state align with:

- LifeOS v28.0.0-alpha.19 active.
- Group 196 / SG-196 Pack 2 closed with exact inspected evidence.
- Configuration Readiness & Secret Boundaries is a real ordinary-mode embedded workspace.
- Desktop, Full Mobile, Mobile Companion, Website and Shared Core are represented correctly.

## Desktop navigation

Confirm the Desktop shell keeps modules in-shell:

- Work
- Career
- Money
- Life
- Household
- Projects
- Integrations

Child modules must open inside their parent workspace and show a back path. Detached module windows are not allowed.

## Current feature smoke checks

Confirm these current areas open and show fictional/sanitized state:

- Command Centre
- Money workspace and v11 financial review/reporting surfaces
- Document and evidence intake
- Career Studio
- Household / Grocery Planning
- Integration control and review-first inbox
- Assistant and automation boundary surfaces
- Work Time and Operating Day Work Proof
- Work Time ordinary empty state, validated manual capture and explicit billing-state transitions
- Documentation and Packaging Hub
- Closed Beta readiness
- Native Intelligence and Optional AI
- Scheduled Communications
- Social & Messenger Review
- Pay-Later & Money Integration Review
- NZ Grocery Price Lookup
- Evidence Automation
- Privacy, Backup & Emergency Control
- Product-Complete Release Candidate

## Full Mobile smoke checks

Confirm Full Mobile has purpose-built mobile surfaces, not Desktop copies:

- Home
- Work
- Money
- Documents/Evidence
- Projects
- Career
- Grocery
- Settings/Diagnostics

Offline queued actions and conflict review must preserve both versions until explicit resolution.

## Safety boundaries

Confirm LifeOS does not perform:

- automatic trust promotion
- silent conflict overwrite
- provider writes while external write capabilities are disabled
- bank feeds or payment initiation
- automatic accounting reconciliation
- autonomous career applications or recruiter messaging
- automatic grocery ordering or external-cart mutation
- destructive evidence/original-media handling

## Evidence and release check

Before committing a group closure:

```powershell
git status --short --branch
dotnet test .\LifeOS.slnx
dotnet build .\LifeOS.slnx -c Release
git diff --check
```

Expected:

```text
Working tree is clean after commit.
The standard eight approved screenshots exist, or a genuine-capture override is documented.
```

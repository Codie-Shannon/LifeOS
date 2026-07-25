# Groups 67-68 - Work Time and Billable Records

- Screenshot group: SG-67
- Release: v14 Work Time
- Version: 14.0.0-alpha.1
- Development commit: `4f4417d`

## Scope

SG-67 demonstrates the Work Time and Billable Records lane, its relationship
to the permanent Work workspace, review-gated manual corrections and exports,
authoritative Work Sessions and Timesheet Evidence routes, and the matching
release and validation state.

All visible work records are fictional demonstration data. No external
provider writes, payment actions or autonomous record changes are enabled.

## Screenshots

1. `01-desktop-work-workspace.png`
   - Navigation: Work
   - Shows the permanent Work workspace and the SG-67-ready Work Time module.
2. `02-desktop-work-time-overview.png`
   - Navigation: Work > Work Time & Billable Records
   - Shows the SG-67 boundary, daily totals, billable value and retained evidence count.
3. `03-desktop-work-time-records.png`
   - Navigation: Work > Work Time & Billable Records
   - Shows running, completed, manual-correction and export records together.
4. `04-desktop-work-time-review-state.png`
   - Navigation: Work > Work Time & Billable Records
   - Shows the explicit Needs review and Ready states without automatic mutation.
5. `05-desktop-work-sessions.png`
   - Navigation: Work > Work Sessions
   - Shows the authoritative session summary and billable-value context.
6. `06-desktop-timesheet-evidence.png`
   - Navigation: Work > Timesheet Evidence
   - Shows the timesheet-ready evidence boundary and current empty review state.
7. `07-desktop-release-version.png`
   - Navigation: Settings
   - Shows Desktop release v14.0.0-alpha.1 and the v14 Work Time release name.
8. `08-group67-validation.png`
   - Shows 358 Core tests passing, the successful Desktop Release build,
     zero warnings, zero errors, Git diff validation and development commit `4f4417d`.

## Verification

- Core tests: 358 passed, 0 failed
- Desktop Release build: succeeded
- Build warnings: 0
- Build errors: 0
- Git diff check: passed
- Screenshot dimensions: approximately 1917 x 1020 pixels
- Private-data review: passed
- Mobile screenshots: not applicable to this Desktop/shared-core pack
- Website screenshots: not applicable to this pack

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1.

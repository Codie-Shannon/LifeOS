# Groups 141-144 manual test - Forms and Actionable Problems

1. Launch LifeOS Desktop and open Settings.
2. Clear Profile name, enter more than 80 characters in Active context, and
   choose Save Settings.
3. Confirm both field-specific messages are visible and the invalid values are
   not persisted.
4. Confirm the actionable problem panel says the form needs attention and
   explains how to recover without showing an exception message or file path.
5. Enter valid single-line values and save; confirm the success state appears.
6. Navigate to Household, restart Desktop and confirm Household remains the last
   workspace.
7. Where a controlled test harness can simulate local access, I/O or malformed
   JSON failures, confirm the stable problem identifier and safe recovery text.
8. Confirm a failed Reset does not replace the currently loaded preferences.

Do not force a real disk failure against the user's data during manual evidence
capture. Exception mapping and failure ordering are covered by focused tests.

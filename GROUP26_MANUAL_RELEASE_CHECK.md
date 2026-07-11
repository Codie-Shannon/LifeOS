# Group 26 Manual Release Check

1. Launch LifeOS and confirm `v5.0.0-beta.1` and `v5 integration release checkpoint`.
2. Open Integration Inbox.
3. Confirm the unified readiness panel lists five lanes:
   - Manual CSV / JSON intake
   - Local ICS calendar intake
   - Google Calendar read-only
   - Local Email Radar import
   - Gmail read-only
4. Confirm every lane says read-only, manual only and human review required.
5. Confirm the global statement says no connector changes an external system or LifeOS operational module automatically.
6. Confirm the visible release-validation panel contains only PASS rows.
7. Confirm Google Calendar still shows its real safe lifecycle state, `calendar.readonly`, manual bounded refresh and retained-evidence wording.
8. Open Email Radar and confirm Gmail remains `gmail.readonly`, manually bounded and review-first.
9. Confirm existing local communication candidates and confirmed timeline still render.
10. Confirm no private account email, event title, message subject/body, token, client credential or raw query is visible in screenshots.
11. Use existing retained safe evidence; do not import more private data solely for screenshots.
12. Capture the smallest coherent set:
    - beta version and global safety
    - unified readiness summary and five lanes
    - release-validation PASS panel
    - safe Calendar lifecycle
    - safe Gmail lifecycle
    - safe Email Radar review/timeline
    - retention wording / duplicate / audit if clearly visible
13. Do not begin Group 27 or v6.

# Stage 092 - v4.8 README Hygiene

Normalizes the top-level README before the v4.8 code work:

- Keeps only the latest LIFEOS_CURRENT_BUILD managed block.
- Keeps only the latest LIFEOS_CURRENT_SCREENSHOTS managed block.
- Removes stale duplicate managed sections.
- Repairs common mojibake sequences in headings and subtitles.
- Preserves all other README content.

The cleanup is performed by the apply script because the current repository README is the source of truth.

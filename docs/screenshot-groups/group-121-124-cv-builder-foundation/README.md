# Groups 121-124 - CV Builder Foundation

- Screenshot group: SG-124
- Release: v28 Career Documents Studio
- Version: 28.0.0-alpha.1
- Development commits: `c59cfe7`, `a8340ea`

## Scope

SG-124 begins the approved post-candidate Career Documents Studio lane with a
guided, evidence-backed CV builder. It supports trusted-profile intake,
structured entries, modular sections, stable drag reordering, live preview,
autosave/version state, undo/redo and a focused full-page preview.

CV content remains reviewable source material. The builder does not fabricate
career claims, submit applications, contact employers or overwrite
authoritative Career records.

## Screenshots

1. `01-career-cv-studio-navigation.png` - embedded Career Studio entry point.
2. `02-cv-builder-split-workspace.png` - continuous editor and live CV preview.
3. `03-contact-and-trusted-profile.png` - contact editing and bounded trusted-profile intake.
4. `04-structured-employment-entry.png` - structured employment fields, dates and description tools.
5. `05-custom-modular-section.png` - optional subtitle, date range and formatted description modules.
6. `06-optional-sections-and-reordering.png` - collapsed section cards and stable drag ordering.
7. `07-fullscreen-cv-preview.png` - focused document preview with return-to-edit control.
8. `08-sg124-validation.png` - feature, regression and Desktop Release validation result.

## Verification

- SG-124 targeted tests: 14 passed, 0 failed
- Core tests: 436 passed, 0 failed
- Companion tests: 34 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Website tests: 28 passed, 0 failed
- Desktop Release build: succeeded with 0 warnings and 0 errors
- Repository whitespace check: passed

## Pack boundary

Product code and tests are retained in Pack 1 commits. This folder is Pack 2
screenshot and documentation evidence. Templates, pagination, ATS/readability
checks and PDF/DOCX export remain in SG-128.

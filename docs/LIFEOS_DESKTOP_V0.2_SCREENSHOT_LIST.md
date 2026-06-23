# LifeOS Desktop v0.2 Screenshot List

## Screenshot folder

```text
docs/screenshots/v0.2/
```

## Keep

### 01-lifeos-v02-command-centre.png

Shows the v0.2 Command Centre with weekly pressure summary cards.

Use this as the main embedded README screenshot.

### 02-lifeos-v02-agenda.png

Shows the Agenda module with open items, due-today count, weekly count, high-pressure count, and pressure reasons.

### 03-lifeos-v02-pay-later.png

Shows the Pay Later module with open deferred obligations, open amount, due-this-week amount, overdue amount, and high-pressure count.

### 04-lifeos-v02-weekly-close-out.png

Shows the Weekly Close-Out module with total entries, current-week status, close-out completion, waiting-on count, and the add close-out form beginning below.

### 05-lifeos-v02-money-pressure.png

Shows the Money Pressure manual input module with editable values and action buttons.

### 06-lifeos-v02-follow-ups.png

Shows the Follow-Ups module with open follow-ups, waiting count, action count, overdue count, due-today count, and money-linked count.

## Removed / not used

The duplicate Pay Later screenshot was not included.

The old v0.1 root screenshot set can be removed from `docs/screenshots/*.png` once the v0.2 screenshots are committed under `docs/screenshots/v0.2/`.

Old v0.1 screenshots replaced by the v0.2 set:

```text
01-command-centre-summary.png
02-money-pressure-inputs.png
03-follow-ups-foundation.png
04-timeragent-desktop-utility.png
05-settings-placeholder.png
06-local-json-persistence.png
07-build-passing.png
```

## README embed

The README should embed:

```markdown
![LifeOS Desktop v0.2 Command Centre](docs/screenshots/v0.2/01-lifeos-v02-command-centre.png)
```

## Notes

The v0.2 screenshot set is good enough for a proof/repo release.

The only screenshot/UI issue worth fixing later is the Money Pressure helper text that still says values are temporary/not saved yet, even though the page now includes Save Inputs and the project has local persistence. That is not a release blocker, but it should be cleaned up in a later polish pass.

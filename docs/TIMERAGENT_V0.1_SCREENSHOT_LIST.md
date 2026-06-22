# TimerAgent v0.1 Screenshot List

Add screenshots into:

```text
docs/screenshots/
```

Use these exact filenames so the README image links work without edits.

---

## Required screenshots

### 1. Task list / manager view

**Filename:**

```text
docs/screenshots/01-task-list-full-width.png
```

**Capture:**

The main List page showing timed tasks as full-width cards.

**Why it matters:**

This shows the main manager view and proves the list UI is polished, not default/rough WPF.

---

### 2. Compact timer running

**Filename:**

```text
docs/screenshots/02-compact-timer-running.png
```

**Capture:**

The Timer page in compact mode with a running task.

**Suggested state:**

```text
Task: Environment Setup
Timer: running
Button state: Pause | Stop | Hide
Money line visible
```

**Why it matters:**

This is the most important screenshot. It proves TimerAgent is a practical desktop work timer, not just an admin form.

---

### 3. Create/edit timed task

**Filename:**

```text
docs/screenshots/03-create-task-description.png
```

**Capture:**

The task details page showing:

- Task/project name
- Contact dropdown
- Description field
- Work type
- Task type
- Mode
- Hourly rate
- Tax %
- Billable checkbox

**Why it matters:**

This shows how timed work is structured before tracking begins.

---

### 4. People / contacts page

**Filename:**

```text
docs/screenshots/04-people-page.png
```

**Capture:**

The People page showing contacts/organisations.

**Why it matters:**

This proves tasks can be linked to reusable people/contacts/organisations.

---

## Optional screenshots

### 5. Compact timer paused

**Filename:**

```text
docs/screenshots/05-compact-timer-paused.png
```

**Capture:**

Compact Timer page with a paused task.

**Suggested state:**

```text
Button state: Resume | Stop | Hide
```

---

### 6. CSV log proof

**Filename:**

```text
docs/screenshots/06-csv-log-proof.png
```

**Capture:**

Open `timer-log.csv` showing at least one logged stopped session.

**Path:**

```text
%LocalAppData%/LifeOS/TimerAgent/timer-log.csv
```

**Why it matters:**

This proves the timer writes evidence/log data, not just on-screen totals.

---

### 7. README repo view after docs

**Filename:**

```text
docs/screenshots/07-github-readme-view.png
```

**Capture:**

GitHub showing the README with embedded screenshots.

**Why it matters:**

Useful portfolio proof once pushed.

---

## Recommended screenshot order in README

The README embeds only the most important screenshots:

```text
01-task-list-full-width.png
02-compact-timer-running.png
03-create-task-description.png
04-people-page.png
```

The optional screenshots can remain in the screenshot folder without being embedded.

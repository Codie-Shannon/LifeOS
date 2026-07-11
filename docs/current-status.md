# LifeOS Current Status

## Current release

LifeOS Desktop `v5.0.0-alpha.5` — Authenticated read-only Gmail connector.

## Completed checkpoint

Screenshot Group 25 is complete.

LifeOS can explicitly connect one private/testing-mode Gmail account with the exact `gmail.readonly` scope. Searches are manual, require an Email Radar profile, are bounded to a maximum of 31 days, use a default cap of 25 and hard maximum of 100, show the generated query and suggested exclusions, and require explicit confirmation before retrieval.

Retrieved Gmail messages normalize into the existing provider-neutral communication-evidence model. Duplicate detection, provenance, deterministic candidate explanations, explicit confirm/reject review, confirmed-only timeline behavior, and review-first suggestions remain the governing Email Radar flow.

## Verified result

- 91 tests passed, 0 failed, 0 skipped
- Release build succeeded
- product commits pushed to `main`
- live private Gmail proof completed with safe test content
- screenshot evidence captured without credentials or private mailbox content
- formal application version aligned to `v5.0.0-alpha.5`

## Gmail scope and lifecycle

- scope: `https://www.googleapis.com/auth/gmail.readonly`
- one local Gmail account
- private/testing-mode OAuth project
- explicit browser authorization
- configured/disconnected and connected-local states
- manual bounded search only
- safe zero-result and successful-result handling
- repeated-search duplicate detection
- disconnect removes local authorization
- connector cache clear removes local connector metadata
- imported communication evidence remains retained

## Trust boundary

Authentication does not make Gmail data trusted. Gmail records remain untrusted communication evidence until explicitly reviewed. Candidate confirmation and operational linking remain separate.

Confirmed communication evidence may produce a review-first waiting-on/follow-up suggestion, but it does not create a Follow-Up or change Work Pipeline, relationship, project, money, invoice, task, or agenda state automatically.

## Safety boundary

No email sending, drafting, replying, forwarding, archiving, trashing, deleting, labelling, starring, mark-read/unread mutation, attachment download, active HTML, remote-image loading, background polling, startup scan, scheduled scan, Gmail push/history monitoring, Outlook, IMAP, POP3, AI interpretation, automatic Follow-Up, or automatic LifeOS state mutation is active.

## Screenshot evidence

See `docs/screenshot-groups/group-25-read-only-gmail-connector/README.md`.

## Next checkpoint

Return control to the LifeOS Master / Roadmap chat. Group 26 has not started.

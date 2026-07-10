# LifeOS v5 Integration Architecture Pack

## Purpose

This pack turns the temporary integration master list into an enforceable connector architecture. The goal is to define every future integration as a provider record with known scope, risk, roadmap phase, action policy, and preview mapping behavior before LifeOS touches live APIs.

## Architecture Rule

All connectors use the same path:

External source -> connector definition -> connector implementation -> `IntegrationPreviewDraft` -> `IntegrationPreviewIntake.CreatePreview` -> Integration Inbox review -> explicit target-module handoff.

Connectors do not directly mutate LifeOS modules or external systems. They create reviewable previews.

## Provider Registry

The core registry lives in `src/LifeOS.Core/IntegrationConnectors`.

Each connector declares:

- Stable key.
- Display name.
- Category.
- First intake mode.
- Risk level.
- Roadmap phase.
- Integration source kind.
- Action policy.
- Requested scopes.
- Suggested LifeOS target modules.
- Notes.

The registry currently includes the broad planning surface from the temporary list: manual imports, calendar, email, cloud files, money, BNPL, OCR, work systems, notes/tasks, AI assistance, SMS, Meta/Facebook, browser/context, life admin, automation bridges, website, and mobile.

## Action Policy

Default policy is preview-only.

- `PreviewOnly`: connector may only create Integration Inbox previews.
- `DraftWithExplicitApproval`: connector may prepare text or proposed actions, but the user must approve before anything is sent or applied.
- `ExternalWriteProhibited`: reserved for sources that must never write externally.

The registry intentionally exposes no external mutation permission. `AllowsExternalMutation` is always false.

## Roadmap Phases

- `V5Foundation`: safe local/manual/read-only proof path.
- `V5Plus`: later v5-era connectors once the foundation is tested.
- `V6Automation`: repeated workflows after approval patterns are proven.
- `MobileCompanion`: phone-native capture, notifications, SMS, and mobile-first flows.
- `V7Assistant`: AI summary, suggestion, and draft layer.
- `V8V9CommunicationContent`: full mobile/web communication and content integrations such as Meta/Facebook.
- `DeferredHighRisk`: banking, broad automation ingress, and anything that needs stronger safety boundaries.

## Permission And Scope Matrix

Current scope vocabulary:

- `ReadLocalFile`
- `ReadMetadata`
- `ReadContent`
- `ReadAttachments`
- `GenerateSuggestion`

Future live connectors should translate OAuth/API permissions into this vocabulary first, then request the narrowest practical external scope.

## Risk Rules

- Low risk can be local/manual and still preview-only.
- Medium risk can read local or scoped metadata/content but stays reviewable.
- High risk requires explicit review and strong provenance.
- Very high risk belongs behind later roadmap gates unless there is a specific approved use case.

Money, BNPL, health, location, SMS, Messenger, Facebook, banking, government, password-manager metadata, and external messaging are never silent automation sources.

## Provider Placement

The current placement is:

- V5 foundation: manual CSV/JSON, drag/drop files, local folder watcher, ICS/calendar proof, local OCR, bank CSV previews.
- V5 plus: Gmail, Outlook Email, Google Drive, OneDrive, SharePoint, Xero, Stripe, PayPal, Wise, Afterpay, Zip, work/task systems, order/admin email extraction.
- Mobile companion: SMS, WhatsApp, mobile capture, push notifications.
- V7 assistant: generative follow-ups, draft replies, weekly close-out narrative.
- V8/V9 communication/content: Facebook Pages, Facebook Groups, public profile archive, Messenger, Instagram, browser/context/contact/location integrations.
- Deferred high risk: open banking, Zapier, Make, IFTTT, generic webhooks.

## Test Contract

The core tests assert:

- The registry contains the broad planning connectors.
- V5 foundation connectors are preview-only.
- High-risk money and communication connectors require explicit approval.
- Facebook/Meta belongs to the v8/v9 communication/content lane.
- SMS belongs to the mobile companion lane.
- Connector keys are unique.
- No registered connector can claim external mutation rights.

## Next Implementation Step

Implement the first real connector against this pack: manual CSV/JSON import into `IntegrationPreviewDraft`. It should use the registry key for provenance, generate deterministic duplicate keys, and continue to create only read-only Integration Inbox previews.

# LifeOS Website design system — v0.3 beta

## Locked primary accent
Purple is the approved primary accent for the Website beta. It performed consistently across dark and light themes while preserving the serious command-centre identity. Blue and teal remain documented candidates, not active primary accents.

## Tokens
- Background: `--bg`
- Surface: `--surface`, `--surface-raised`
- Text: `--text`, `--muted`
- Border: `--border`
- Accent: `--accent`, `--accent-strong`, `--accent-soft`
- Status: `--success`, `--warning`, `--danger`
- Focus: `--focus`

Status always includes text or icon meaning and never relies on colour alone.

## Scale and layout
- Typography uses a restrained system stack and a clear display/body hierarchy.
- Spacing follows the existing page-shell, section and card-grid rhythm.
- Radius: small controls, medium cards, large hero/proof surfaces.
- Shadows are reserved for elevated or interactive emphasis.
- Content width remains readable on desktop and collapses cleanly at tablet/mobile breakpoints.

## Reusable components
Website component | Later Desktop equivalent
---|---
Global header | Simplified Desktop shell header
Status badge | Status chip with text semantics
Evidence metric | Validation/evidence summary tile
Safety callout | Contextual Desktop safety boundary
Documentation card | Help/documentation launcher
Theme toggle | Appearance preference control

Group 43 may translate these rules into WPF; it must not copy Website markup literally.

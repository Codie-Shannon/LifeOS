# Group 41 manual verification

Complete each item before screenshot evidence.

## Website documentation
- [ ] `/docs` shows real counts, Start Here and featured user guides.
- [ ] Guides, Concepts, Modules and Releases contain real documents.
- [ ] Search works by title, keyword, audience and product.
- [ ] Section, audience and product filters return deterministic local results.
- [ ] Getting Started shows breadcrumb, audience, product, version, updated date and related docs.
- [ ] Open Desktop, Companion, operating philosophy, evidence, integrations, automation, Assistant, recovery and release-boundary documents.
- [ ] Evidence links remain public routes and all fictional/sanitized wording is visible.
- [ ] No repository paths, private client content, raw logs, credentials or tracking scripts appear in publish output.

## Desktop copy reduction
- [ ] Open Assistant planning.
- [ ] The short read-only/no-execution boundary remains visible.
- [ ] `Open Documentation` is present.
- [ ] The allowlisted Assistant route opens when a browser is available.
- [ ] Offline/no-browser failure shows a bounded message rather than crashing.
- [ ] Assistant remains usable without the Website.

## Release
- [ ] Website Release build and Website tests pass.
- [ ] Core/Desktop tests remain passing.
- [ ] Companion tests remain passing.
- [ ] Desktop Release build passes.
- [ ] Companion Android Release build passes when the established project supports it.
- [ ] Cloudflare publish output exists and passes forbidden-pattern checks.
- [ ] NuGet vulnerability, Gitleaks, repository hygiene and `git diff --check` pass.
- [ ] HEAD equals origin/main and the working tree is clean.

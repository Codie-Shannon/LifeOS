# LifeOS Website — Cloudflare Pages deployment

## Build
```powershell
dotnet restore .\src\LifeOS.Website\LifeOS.Website.csproj
dotnet publish .\src\LifeOS.Website\LifeOS.Website.csproj --configuration Release --output .\artifacts\website-publish
```

Publish directory: `artifacts/website-publish/wwwroot`

## Cloudflare Pages settings
- Framework preset: None
- Build command: use the repository-approved .NET publish command in CI
- Build output directory: `artifacts/website-publish/wwwroot`
- SPA fallback: `_redirects` contains `/* /index.html 200`

## Preview workflow
1. Push a temporary review branch only when a preview is needed.
2. Let Cloudflare create a branch preview.
3. Check direct routes, themes, mobile navigation, Docs search, Privacy and Early Access.
4. Merge only after validation, then delete the temporary branch.

## Production promotion
- Confirm canonical base URL and replace `{CANONICAL_BASE}` during deployment generation.
- Confirm sitemap, robots, Open Graph image and direct-route fallback.
- Confirm no source maps, binaries, raw markdown, secrets, local paths or private files.
- Confirm HEAD equals origin/main and the tree is clean.

No live production deployment is required by Group 42.

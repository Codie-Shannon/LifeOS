# LifeOS Desktop v0.1 Final Commands

## Build

```powershell
dotnet build
```

## Run

```powershell
dotnet run --project .\LifeOS.Desktop\LifeOS.Desktop.csproj
```

If the repo uses `src`:

```powershell
dotnet run --project .\src\LifeOS.Desktop\LifeOS.Desktop.csproj
```

## Commit docs and screenshots

```powershell
git status
git add README.md docs
git commit -m "Document LifeOS Desktop v0.1 release"
git push
```

## Tag release

```powershell
git tag lifeos-desktop-v0.1
git push origin lifeos-desktop-v0.1
```

# Group 66 - Shared Household Workflows and v13 Closure

## Desktop launch script

```powershell
Set-Location "C:\Projects\LifeOS"
dotnet run --project ".\LifeOS.Desktop\LifeOS.Desktop.csproj"
```

## Desktop screenshots

1. Desktop Grocery dashboard showing the new Group 66 Shared household workflow card.
2. Desktop Household routines page showing overdue/due-soon routines and assignment review.
3. Desktop Replenishment review page showing review-only inventory/meal candidates and no-order boundary.
4. Desktop Receipts & spending page showing receipt review and planned-vs-actual spend review.
5. Desktop v13 closure page showing closure checks and pending screenshot/validation proof.

## Mobile launch script

```powershell
Set-Location "C:\Projects\LifeOS"
dotnet build ".\src\LifeOS.Mobile\LifeOS.Mobile.csproj" -c Debug -f net10.0-android
dotnet build ".\src\LifeOS.Mobile\LifeOS.Mobile.csproj" -c Debug -f net10.0-android -t:Run
```

## Mobile screenshots

6. Mobile Grocery dashboard showing the Group 66 Shared household review card.
7. Mobile Household routines page showing overdue/due-soon routine cards and assignment review.
8. Mobile Receipts and spending page showing receipt review, over-plan spend and v13 closure card.

## Validation script

```powershell
Set-Location "C:\Projects\LifeOS"
git status --short --branch
dotnet test ".\LifeOS.slnx"
dotnet build ".\src\LifeOS.Core\LifeOS.Core.csproj" -c Release --no-restore
dotnet build ".\LifeOS.Desktop\LifeOS.Desktop.csproj" -c Release --no-restore
dotnet build ".\src\LifeOS.Mobile\LifeOS.Mobile.csproj" -c Release --no-restore
git diff --check
git log --oneline -4
```

9. Validation proof showing tests, targeted Release builds, diff check and latest commits.


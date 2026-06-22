# LifeOS Desktop Shell v0.1 - Phase 4 Files

These files are for Phase 4 only: the LifeOS.Desktop WPF shell foundation.

## Files included

- MainWindow.xaml
- MainWindow.xaml.cs

## Where to put them

Copy both files into:

```text
LifeOS.Desktop/
```

Replace the existing default `MainWindow.xaml` and `MainWindow.xaml.cs`.

## Expected namespace

These files use:

```csharp
namespace LifeOS.Desktop;
```

and:

```xml
x:Class="LifeOS.Desktop.MainWindow"
```

If your WPF project is named something different, rename the namespace/class references to match your project.

## Run command

From the solution root:

```powershell
dotnet run --project .\LifeOS.Desktop\LifeOS.Desktop.csproj
```

## Phase 4 pass condition

You pass Phase 4 when:

- the desktop shell opens
- Command Centre is the default page
- left navigation works
- Money Pressure opens
- Agenda opens
- Follow-Ups opens
- Projects opens
- TimerAgent opens
- Settings opens
- TimerAgent is described as desktop-only
- no mobile app or website is built yet

## Notes

This shell intentionally does not deeply integrate TimerAgent yet.

TimerAgent remains desktop-only and separate. The desktop shell only describes its role for now.

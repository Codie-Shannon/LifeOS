using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class LocalDataRecoveryView : UserControl
{
    private readonly bool _portfolioDemo;
    private string? _notice;

    public LocalDataRecoveryView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        Render();
    }

    private void Render()
    {
        IReadOnlyList<OperationalLocalStoreStatus> stores =
            OperationalLocalDataCatalog.Inspect();
        int attention = stores.Count(store => store.Health.State is
            LocalStoreHealthState.Unreadable or
            LocalStoreHealthState.NewerSchema or
            LocalStoreHealthState.OlderSchema or
            LocalStoreHealthState.LegacyFormat);
        int trashCount = stores.Sum(store => store.TrashEntries.Count);

        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Badge(
            _portfolioDemo
                ? "PORTFOLIO DEMO DATA • ISOLATED"
                : "ORDINARY MODE • LOCAL DATA"));
        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#8FD3B5", FontWeights.SemiBold));

        WrapPanel summary = new() { Margin = new Thickness(0, 14, 0, 8) };
        summary.Children.Add(Metric("Registered stores", stores.Count.ToString(), "First operational migration set"));
        summary.Children.Add(Metric("Needs attention", attention.ToString(), attention == 0 ? "No schema or read errors" : "Review before writing"));
        summary.Children.Add(Metric("Trash items", trashCount.ToString(), "30-day default retention"));
        summary.Children.Add(Metric("Silent overwrite", "Off", "Restore refuses current-file replacement"));
        root.Children.Add(summary);

        WrapPanel actions = new() { Margin = new Thickness(0, 4, 0, 12) };
        actions.Children.Add(Button("Refresh health", () =>
        {
            _notice = "Local storage health refreshed.";
            Render();
        }));
        actions.Children.Add(Button("Open local data folder", OpenDataFolder, secondary: true));
        root.Children.Add(actions);

        root.Children.Add(Heading("Operational stores", 20, new Thickness(0, 8, 0, 4)));
        root.Children.Add(Text(
            "Agenda, follow-ups, work pipeline and work sessions are the first stores moved onto the shared versioned contract. Other modules remain on their existing paths until migrated deliberately.",
            12,
            "#A9B6CA"));

        foreach (OperationalLocalStoreStatus store in stores)
            root.Children.Add(StoreCard(store));

        root.Children.Add(Heading("Recovery boundary", 20, new Thickness(0, 20, 0, 4)));
        root.Children.Add(Card(
            "No silent replacement or permanent deletion",
            "Reset moves a current file to recoverable Trash. Restore is enabled only when no current file exists. Expired-item purging and full encrypted backup remain separate explicit completion work.",
            "#152437"));

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = root
        };
    }

    private UIElement StoreCard(OperationalLocalStoreStatus store)
    {
        StackPanel body = new();
        DockPanel heading = new();
        TextBlock state = Badge(store.Health.State.ToString().ToUpperInvariant());
        DockPanel.SetDock(state, Dock.Right);
        heading.Children.Add(state);
        heading.Children.Add(Heading(store.DisplayName, 17));
        body.Children.Add(heading);
        body.Children.Add(Text(
            $"Category: {store.Category}\nSchema: {(store.Health.SchemaVersion?.ToString() ?? "not created")}\n" +
            $"Backup available: {store.Health.BackupAvailable}\n{store.Health.Detail}",
            12,
            "#C2CDDC"));
        body.Children.Add(Text(
            $"Local file: {Path.GetFileName(store.Health.FilePath)}",
            11,
            "#7F91AA"));

        foreach (LocalStoreTrashEntry entry in store.TrashEntries)
        {
            StackPanel trash = new() { Margin = new Thickness(0, 10, 0, 0) };
            trash.Children.Add(Text(
                $"Trash • deleted {entry.DeletedUtc.LocalDateTime:g} • retain until {entry.PurgeAfterUtc.LocalDateTime:d}",
                12,
                "#D9C68E",
                FontWeights.SemiBold));
            Button restore = Button("Restore this version", () => Restore(store, entry), secondary: true);
            restore.IsEnabled = store.Health.State == LocalStoreHealthState.Missing;
            restore.ToolTip = restore.IsEnabled
                ? "Restore the trashed local file."
                : "Restore is blocked while a current file exists.";
            trash.Children.Add(restore);
            body.Children.Add(trash);
        }

        Border border = new()
        {
            Background = Brush("#151F30"),
            BorderBrush = Brush(StateColor(store.Health.State)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 10, 0, 0),
            Child = body
        };
        return border;
    }

    private void Restore(
        OperationalLocalStoreStatus store,
        LocalStoreTrashEntry entry)
    {
        try
        {
            OperationalLocalDataCatalog.RestoreTrash(store.StoreId, entry.Id);
            _notice = $"{store.DisplayName} restored from Trash.";
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or InvalidDataException or IOException or UnauthorizedAccessException)
        {
            _notice = $"Restore blocked: {exception.Message}";
        }
        Render();
    }

    private void OpenDataFolder()
    {
        try
        {
            string folder = LocalAppDataPath.GetLifeOSFolder();
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
            _notice = "Opened the active local data folder.";
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            _notice = $"The local data folder could not be opened: {exception.Message}";
        }
        Render();
    }

    private static Border Metric(string label, string value, string detail)
    {
        StackPanel content = new();
        content.Children.Add(Text(label, 11, "#9EACC0"));
        content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold));
        content.Children.Add(Text(detail, 11, "#9EACC0"));
        return new Border
        {
            Width = 220,
            MinHeight = 104,
            Background = Brush("#151F30"),
            BorderBrush = Brush("#31445F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 10, 10),
            Child = content
        };
    }

    private static Border Card(string title, string body, string background)
    {
        StackPanel content = new();
        content.Children.Add(Heading(title, 15));
        content.Children.Add(Text(body, 12, "#C2CDDC"));
        return new Border
        {
            Background = Brush(background),
            BorderBrush = Brush("#31445F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 8, 0, 0),
            Child = content
        };
    }

    private static Button Button(string label, Action action, bool secondary = false)
    {
        Button button = new()
        {
            Content = label,
            Background = Brush(secondary ? "#25334A" : "#315E91"),
            Foreground = Brushes.White,
            BorderBrush = Brush(secondary ? "#405472" : "#477DB4"),
            Padding = new Thickness(14, 7, 14, 7),
            Margin = new Thickness(0, 8, 8, 0),
            MinHeight = 36
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brushes.White,
        TextWrapping = TextWrapping.Wrap,
        Margin = margin ?? new Thickness(0, 0, 0, 4)
    };

    private static TextBlock Text(
        string text,
        double size,
        string color,
        FontWeight? weight = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight ?? FontWeights.Normal,
        Foreground = Brush(color),
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 3, 0, 0)
    };

    private static TextBlock Badge(string text) => new()
    {
        Text = text,
        FontSize = 11,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brush("#AFA4FF"),
        Margin = new Thickness(0, 7, 0, 0)
    };

    private static string StateColor(LocalStoreHealthState state) => state switch
    {
        LocalStoreHealthState.Healthy => "#2E8066",
        LocalStoreHealthState.Missing => "#31445F",
        LocalStoreHealthState.LegacyFormat or LocalStoreHealthState.OlderSchema => "#A77737",
        _ => "#A14E57"
    };

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

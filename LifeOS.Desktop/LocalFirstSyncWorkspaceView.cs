using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.LocalFirstSync;
using LifeOS.Shared.LocalFirstSync;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class LocalFirstSyncWorkspaceView : UserControl
{
    private readonly bool _demo;
    private List<LocalAccountSyncProfile> _profiles = [];
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _displayName = string.Empty;
    private string _deviceLabel = string.Empty;
    private LocalAccountMode _mode = LocalAccountMode.LocalOnly;

    public LocalFirstSyncWorkspaceView(bool demo)
    {
        _demo = demo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        try { _profiles = LocalFirstSyncStorage.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _problem = UserFacingProblemFactory.FromException(ex, "load the local account profile");
        }
        Render();
    }

    private void Render()
    {
        LocalAccountSyncProfile? profile = _profiles.SingleOrDefault();
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(_demo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL-FIRST", 11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Local Account & Sync Foundation", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text("Name this local workspace and choose a local-only or manual-transfer posture. No sign-in, cloud account, background sync, upload or provider write is available.", 14, "#B8C5D8"));
        if (_notice is not null) root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        if (_problem is not null) root.Children.Add(Problem(_problem));

        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Local profile", profile is null ? "Not set" : "Ready", "No authentication identity"));
        metrics.Children.Add(Metric("Pending local changes", (profile?.PendingLocalChanges ?? 0).ToString(), "Local counter only"));
        metrics.Children.Add(Metric("Cloud provider", "Not configured", "Credentials required later"));
        metrics.Children.Add(Metric("Background sync", "Off", "Cannot auto-resume"));
        root.Children.Add(metrics);

        if (profile is null) root.Children.Add(Capture());
        else root.Children.Add(ProfileCard(profile));

        root.Children.Add(Heading("Configuration boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card("Development can continue without credentials", "This foundation validates local identity labels, persists one recoverable profile and records local change pressure. Authentication, encryption keys, endpoint selection, remote conflict exchange and actual transfer remain unavailable until explicitly configured and tested.", "#152437"));
        LocalStoreHealth health = LocalFirstSyncStorage.Inspect();
        root.Children.Add(Card("Versioned local-account-sync store", $"State: {health.State}. Recovery is available in Local Data & Recovery. Reset moves the profile to recoverable Trash.", "#151F30"));
        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = root };
    }

    private UIElement Capture()
    {
        StackPanel body = new();
        body.Children.Add(Heading("Create a local workspace profile", 20));
        body.Children.Add(Text("This is a device-local label, not a login or remote account.", 12, "#A9B6CA"));
        WrapPanel fields = new();
        fields.Children.Add(Input("Display name *", "local-account-name", "LocalSync.DisplayName", _displayName, value => _displayName = value, 480));
        fields.Children.Add(Input("Device label *", "local-device-label", "LocalSync.DeviceLabel", _deviceLabel, value => _deviceLabel = value, 480));
        body.Children.Add(fields);
        body.Children.Add(Choice("Data movement posture", "LocalSync.Mode", Enum.GetValues<LocalAccountMode>(), _mode, value => _mode = value, 480));
        Button create = Button("Create local profile", "LocalSync.Create", false);
        create.Click += (_, _) => Create();
        body.Children.Add(create);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private UIElement ProfileCard(LocalAccountSyncProfile profile)
    {
        StackPanel body = new();
        DockPanel header = new();
        TextBlock state = Text(profile.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold);
        DockPanel.SetDock(state, Dock.Right);
        header.Children.Add(state);
        header.Children.Add(Heading(profile.DisplayName, 20));
        body.Children.Add(header);
        body.Children.Add(Text($"Device: {profile.DeviceLabel} • posture: {profile.Mode}", 12, "#A9B6CA"));
        body.Children.Add(Text($"Pending local changes: {profile.PendingLocalChanges}. Provider configured: no. Background sync: off.", 13, "#E7ECF4", FontWeights.SemiBold));
        if (profile.LastManualTransferUtc is not null) body.Children.Add(Text($"Last acknowledged manual transfer: {profile.LastManualTransferUtc:yyyy-MM-dd HH:mm} UTC", 11, "#A9B6CA"));
        WrapPanel actions = new();
        if (profile.State != LocalSyncState.Paused)
        {
            Button change = Button("Record local change", "LocalSync.RecordChange", true);
            change.Click += (_, _) => Change(profile, value => LocalFirstSyncService.RegisterLocalChange(value, DateTimeOffset.UtcNow), "Recorded one local change.");
            actions.Children.Add(change);
            if (profile.Mode == LocalAccountMode.ManualTransfer)
            {
                Button transfer = Button("Acknowledge manual transfer", "LocalSync.RecordTransfer", true);
                transfer.Click += (_, _) => Change(profile, value => LocalFirstSyncService.RecordManualTransfer(value, DateTimeOffset.UtcNow), "Recorded a manual-transfer checkpoint. LifeOS did not upload anything.");
                actions.Children.Add(transfer);
            }
            Button pause = Button("Pause local change tracking", "LocalSync.Pause", true);
            pause.Click += (_, _) => Change(profile, value => LocalFirstSyncService.SetPaused(value, true, DateTimeOffset.UtcNow), "Local change tracking paused.");
            actions.Children.Add(pause);
        }
        else
        {
            Button resume = Button("Resume local change tracking", "LocalSync.Resume", true);
            resume.Click += (_, _) => Change(profile, value => LocalFirstSyncService.SetPaused(value, false, DateTimeOffset.UtcNow), "Local change tracking resumed.");
            actions.Children.Add(resume);
        }
        body.Children.Add(actions);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void Create()
    {
        LocalAccountDraft draft = new(_displayName, _deviceLabel, _mode);
        FormValidationResult validation = LocalFirstSyncService.Validate(draft);
        _issues = validation.Issues;
        if (!validation.IsValid)
        {
            _problem = new UserFacingProblem("local-account-validation-failed", "Review the local profile fields", "No local account profile was added because one or more fields are invalid.", "Correct the highlighted fields, then try again.", true);
            Render();
            return;
        }
        try
        {
            LocalAccountSyncProfile profile = LocalFirstSyncService.Create(draft, DateTimeOffset.UtcNow);
            LocalFirstSyncStorage.Save([profile]);
            _profiles = [profile];
            _issues = [];
            _problem = null;
            _notice = "Created a local-only workspace profile. No remote account was created.";
            Render();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            _problem = UserFacingProblemFactory.FromException(ex, "save the local account profile");
            Render();
        }
    }

    private void Change(LocalAccountSyncProfile profile, Func<LocalAccountSyncProfile, LocalAccountSyncProfile> change, string notice)
    {
        try
        {
            LocalAccountSyncProfile changed = change(profile);
            LocalFirstSyncStorage.Save([changed]);
            _profiles = [changed];
            _problem = null;
            _notice = notice;
            Render();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException or OverflowException)
        {
            _problem = UserFacingProblemFactory.FromException(ex, "change the local sync state");
            Render();
        }
    }

    private UIElement Input(string label, string fieldId, string automationId, string value, Action<string> changed, double width)
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        TextBox input = new() { Text = value, MinHeight = 38, Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7) };
        AutomationProperties.SetAutomationId(input, automationId);
        input.TextChanged += (_, _) => changed(input.Text);
        field.Children.Add(input);
        string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message));
        if (error.Length > 0) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold));
        return field;
    }

    private static UIElement Choice<T>(string label, string automationId, IReadOnlyList<T> values, T selected, Action<T> changed, double width) where T : struct
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 };
        AutomationProperties.SetAutomationId(input, automationId);
        input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); };
        field.Children.Add(input);
        return field;
    }

    private static Border Problem(UserFacingProblem problem) { StackPanel body = new(); body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16)); body.Children.Add(Text(problem.Detail, 12, "#E1E7F0")); body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF")); return new Border { Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body }; }
    private static Button Button(string label, string id, bool secondary) { Button button = new() { Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White, BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 8, 8, 0), MinHeight = 36 }; AutomationProperties.SetAutomationId(button, id); return button; }
    private static Border Metric(string label, string value, string detail) { StackPanel body = new(); body.Children.Add(Text(label, 11, "#9EACC0")); body.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); body.Children.Add(Text(detail, 11, "#9EACC0")); return new Border { Width = 230, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = body }; }
    private static Border Card(string title, string body, string background) { StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC")); return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content }; }
    private static Border Panel(UIElement child, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = child };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

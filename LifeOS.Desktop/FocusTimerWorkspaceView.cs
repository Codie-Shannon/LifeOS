using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.FocusTimers;
using LifeOS.Core.Forms;
using LifeOS.Shared.FocusTimers;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class FocusTimerWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<FocusTimerRecord> _records = [];
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _title = string.Empty, _area = string.Empty, _target = "25", _nextAction = string.Empty, _notes = string.Empty;
    private FocusTimerKind _kind = FocusTimerKind.Work;

    public FocusTimerWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo; Background = Brush("#0C1220"); Foreground = Brushes.White; FontFamily = new FontFamily("Segoe UI");
        try { _records = FocusTimerStorage.Load(); } catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException) { _problem = UserFacingProblemFactory.FromException(exception, "load focus timers"); }
        Render();
    }

    private void Render()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow; StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(_portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL TIMER", 11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Focus Timer & Session Control", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text("Plan a bounded focus block, start it deliberately and preserve every pause or stop locally. Timers never start themselves, silence notifications, send messages or create billing claims.", 14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice)) root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold)); if (_problem is not null) root.Children.Add(Problem(_problem));
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Running", _records.Count(record => record.State == FocusTimerState.Running).ToString(), "Explicitly active"));
        metrics.Children.Add(Metric("Paused", _records.Count(record => record.State == FocusTimerState.Paused).ToString(), "Time is not accruing"));
        metrics.Children.Add(Metric("Planned", _records.Count(record => record.State == FocusTimerState.Planned).ToString(), "Not started"));
        metrics.Children.Add(Metric("Today retained", Format(TimeSpan.FromSeconds(_records.Where(record => record.CreatedUtc.Date == now.Date).Sum(record => FocusTimerService.Duration(record, now).TotalSeconds))), "Local duration evidence"));
        root.Children.Add(metrics); root.Children.Add(Capture()); root.Children.Add(Heading("Focus sessions", 21, new Thickness(0, 20, 0, 6)));
        Button refresh = Button("Refresh elapsed time", "FocusTimer.Refresh", true); refresh.Click += (_, _) => Render(); root.Children.Add(refresh);
        if (_records.Count == 0) root.Children.Add(Card("No focus timers yet", "Ordinary mode does not seed work, personal, household or wellbeing sessions.", "#151F30"));
        else foreach (FocusTimerRecord record in _records) root.Children.Add(RecordCard(record, now));
        LocalStoreHealth health = FocusTimerStorage.Inspect(); root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card("Versioned focus-timers store", $"State: {health.State}. Recovery is available in Local Data & Recovery. No automatic timer start, notification control, invoice, calendar event, message or provider action occurs.", "#152437"));
        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = root };
    }

    private UIElement Capture()
    {
        StackPanel body = new(); body.Children.Add(Heading("Plan a focus timer", 20)); body.Children.Add(Text("A timer is created as planned. Starting it always requires a separate explicit action.", 12, "#A9B6CA"));
        WrapPanel first = new(); first.Children.Add(Input("Title *", "focus-title", "FocusTimer.Title", _title, value => _title = value, 640)); first.Children.Add(Input("Area *", "focus-area", "FocusTimer.Area", _area, value => _area = value, 340)); body.Children.Add(first);
        WrapPanel second = new(); second.Children.Add(Choice("Kind", "FocusTimer.Kind", Enum.GetValues<FocusTimerKind>(), _kind, value => _kind = value, 300)); second.Children.Add(Input("Target minutes", "focus-target", "FocusTimer.Target", _target, value => _target = value, 300)); body.Children.Add(second);
        body.Children.Add(Input("Next action *", "focus-next-action", "FocusTimer.NextAction", _nextAction, value => _nextAction = value, 990)); body.Children.Add(Input("Notes", "focus-notes", "FocusTimer.Notes", _notes, value => _notes = value, 990));
        Button add = Button("Plan focus timer", "FocusTimer.Add", false); add.Click += (_, _) => Add(); body.Children.Add(add); return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void Add()
    {
        FocusTimerDraft draft = new(_title, _area, _kind, _target, _nextAction, _notes); FormValidationResult validation = FocusTimerService.Validate(draft); _issues = validation.Issues;
        if (!validation.IsValid) { _problem = new UserFacingProblem("focus-timer-validation-failed", "Review the focus-timer fields", "No focus timer was planned because one or more fields are invalid.", "Correct the highlighted fields, then try again.", true); Render(); return; }
        try { FocusTimerRecord record = FocusTimerService.Create(draft, DateTimeOffset.UtcNow); List<FocusTimerRecord> candidate = [.. _records, record]; FocusTimerStorage.Save(candidate); _records = candidate; _title = string.Empty; _nextAction = string.Empty; _notes = string.Empty; _issues = []; _problem = null; _notice = $"Planned {record.Title}."; Render(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException) { _problem = UserFacingProblemFactory.FromException(exception, "save the focus timer"); Render(); }
    }

    private UIElement RecordCard(FocusTimerRecord record, DateTimeOffset now)
    {
        StackPanel body = new(); DockPanel header = new(); TextBlock state = Text(record.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold); DockPanel.SetDock(state, Dock.Right); header.Children.Add(state); header.Children.Add(Heading(record.Title, 18)); body.Children.Add(header);
        body.Children.Add(Text($"{record.Area} • {record.Kind}" + (record.TargetMinutes is null ? string.Empty : $" • target {record.TargetMinutes} min"), 12, "#A9B6CA")); body.Children.Add(Text($"Elapsed: {Format(FocusTimerService.Duration(record, now))}", 18, "#FFFFFF", FontWeights.SemiBold)); body.Children.Add(Text($"Next: {record.NextAction}", 13, "#E7ECF4", FontWeights.SemiBold)); if (!string.IsNullOrWhiteSpace(record.Notes)) body.Children.Add(Text(record.Notes, 11, "#A9B6CA"));
        WrapPanel actions = new(); foreach ((string label, FocusTimerState next) in Actions(record.State)) { Button button = Button(label, $"FocusTimer.State.{next}.{record.Id}", true); button.Click += (_, _) => Transition(record, next); actions.Children.Add(button); } body.Children.Add(actions); return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private static IReadOnlyList<(string, FocusTimerState)> Actions(FocusTimerState state) => state switch
    {
        FocusTimerState.Planned => [("Start", FocusTimerState.Running), ("Cancel", FocusTimerState.Cancelled)], FocusTimerState.Running => [("Pause", FocusTimerState.Paused), ("Stop", FocusTimerState.Completed), ("Cancel", FocusTimerState.Cancelled)], FocusTimerState.Paused => [("Resume", FocusTimerState.Running), ("Stop", FocusTimerState.Completed), ("Cancel", FocusTimerState.Cancelled)], FocusTimerState.Completed => [("Archive", FocusTimerState.Archived)], FocusTimerState.Cancelled => [("Re-plan", FocusTimerState.Planned), ("Archive", FocusTimerState.Archived)], _ => []
    };

    private void Transition(FocusTimerRecord record, FocusTimerState next)
    {
        try { FocusTimerRecord changed = FocusTimerService.Transition(record, next, DateTimeOffset.UtcNow); List<FocusTimerRecord> candidate = _records.Select(value => value.Id == record.Id ? changed : value).ToList(); FocusTimerStorage.Save(candidate); _records = candidate; _problem = null; _notice = $"{record.Title} is now {next.ToString().ToLowerInvariant()}."; Render(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException or ArgumentOutOfRangeException) { _problem = UserFacingProblemFactory.FromException(exception, "change the focus-timer state"); Render(); }
    }

    private UIElement Input(string label, string fieldId, string automationId, string value, Action<string> changed, double width) { StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold)); TextBox input = new() { Text = value, MinHeight = 38, Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7) }; AutomationProperties.SetAutomationId(input, automationId); input.TextChanged += (_, _) => changed(input.Text); field.Children.Add(input); string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message)); if (!string.IsNullOrWhiteSpace(error)) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold)); return field; }
    private static UIElement Choice<T>(string label, string id, IReadOnlyList<T> values, T selected, Action<T> changed, double width) where T : struct { StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold)); ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 }; AutomationProperties.SetAutomationId(input, id); input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); }; field.Children.Add(input); return field; }
    private static string Format(TimeSpan duration) => duration.ToString(duration.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
    private static Border Problem(UserFacingProblem problem) { StackPanel body = new(); body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16)); body.Children.Add(Text(problem.Detail, 12, "#E1E7F0")); body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF")); Border panel = new() { Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body }; AutomationProperties.SetAutomationId(panel, "FocusTimer.Problem"); return panel; }
    private static Button Button(string label, string id, bool secondary) { Button button = new() { Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White, BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 8, 8, 0), MinHeight = 36 }; AutomationProperties.SetAutomationId(button, id); return button; }
    private static Border Metric(string label, string value, string detail) { StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0")); content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0")); return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content }; }
    private static Border Card(string title, string body, string background) { StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC")); return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content }; }
    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

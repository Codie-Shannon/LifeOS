using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.Life;
using LifeOS.Shared.Life;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class LifeRoutinesWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<LifeRoutineRecord> _records = [];
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    private string _title = string.Empty;
    private string _area = string.Empty;
    private string _nextAction = string.Empty;
    private string _timeWindow = string.Empty;
    private string _notes = string.Empty;
    private LifeRoutineKind _kind = LifeRoutineKind.Routine;
    private LifeRoutinePressure _pressure = LifeRoutinePressure.Normal;
    private bool _pinned;

    public LifeRoutinesWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220"); Foreground = Brushes.White; FontFamily = new FontFamily("Segoe UI");
        try { _records = LifeRoutineStorage.Load(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        { _problem = UserFacingProblemFactory.FromException(exception, "load life routines"); }
        Render();
    }

    private void Render()
    {
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(_portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL LIFE", 11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Life Routines & Personal Administration", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text("Capture a visible personal commitment, keep its next action explicit and move it through local states manually. Nothing is scheduled, messaged or paid automatically.", 14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice)) root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        if (_problem is not null) root.Children.Add(Problem(_problem));

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Today open", _records.Count(record => record.Date == today && record.State is not LifeRoutineState.Done and not LifeRoutineState.Archived).ToString(), "Visible commitments"));
        metrics.Children.Add(Metric("Active", _records.Count(record => record.State == LifeRoutineState.Active).ToString(), "Explicitly started"));
        metrics.Children.Add(Metric("Waiting", _records.Count(record => record.State == LifeRoutineState.Waiting).ToString(), "Needs review"));
        metrics.Children.Add(Metric("High pressure", _records.Count(record => record.Pressure is LifeRoutinePressure.High or LifeRoutinePressure.Critical && record.State is not LifeRoutineState.Done and not LifeRoutineState.Archived).ToString(), "No automatic escalation"));
        metrics.Children.Add(Metric("Pinned", _records.Count(record => record.Pinned && record.State != LifeRoutineState.Archived).ToString(), "Local attention only"));
        root.Children.Add(metrics); root.Children.Add(Capture());
        root.Children.Add(Heading("Personal operating state", 21, new Thickness(0, 20, 0, 6)));
        if (_records.Count == 0)
            root.Children.Add(Card("No life routines yet", "Ordinary mode does not seed personal commitments, appointments, maintenance or wellbeing records.", "#151F30"));
        else
            foreach (LifeRoutineRecord record in _records) root.Children.Add(RecordCard(record));
        LocalStoreHealth health = LifeRoutineStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card("Versioned life-routines store", $"State: {health.State}. Recovery is available in Local Data & Recovery. No calendar event, reminder, message, provider task, payment or background action is created.", "#152437"));
        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = root };
    }

    private UIElement Capture()
    {
        StackPanel body = new(); body.Children.Add(Heading("Add a local life routine", 20));
        body.Children.Add(Text("Required fields are validated before authoritative memory or disk changes.", 12, "#A9B6CA"));
        WrapPanel row1 = new();
        row1.Children.Add(Input("Date (YYYY-MM-DD) *", "life-date", "Life.Date", _date, value => _date = value, 300));
        row1.Children.Add(Input("Title *", "life-title", "Life.Title", _title, value => _title = value, 680)); body.Children.Add(row1);
        WrapPanel row2 = new();
        row2.Children.Add(Input("Area *", "life-area", "Life.Area", _area, value => _area = value, 300));
        row2.Children.Add(Choice("Kind", "Life.Kind", Enum.GetValues<LifeRoutineKind>(), _kind, value => _kind = value, 300));
        row2.Children.Add(Choice("Pressure", "Life.Pressure", Enum.GetValues<LifeRoutinePressure>(), _pressure, value => _pressure = value, 300)); body.Children.Add(row2);
        WrapPanel row3 = new();
        row3.Children.Add(Input("Next action *", "life-next-action", "Life.NextAction", _nextAction, value => _nextAction = value, 680));
        row3.Children.Add(Input("Time window", "life-time-window", "Life.TimeWindow", _timeWindow, value => _timeWindow = value, 300)); body.Children.Add(row3);
        body.Children.Add(Input("Notes", "life-notes", "Life.Notes", _notes, value => _notes = value, 990));
        CheckBox pinned = new() { Content = "Pin for attention", IsChecked = _pinned, Foreground = Brushes.White, Margin = new Thickness(0, 8, 0, 0) };
        AutomationProperties.SetAutomationId(pinned, "Life.Pinned"); pinned.Checked += (_, _) => _pinned = true; pinned.Unchecked += (_, _) => _pinned = false; body.Children.Add(pinned);
        Button add = Button("Add life routine", "Life.Add", false); add.Click += (_, _) => Add(); body.Children.Add(add);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void Add()
    {
        LifeRoutineDraft draft = new(_date, _title, _area, _kind, _pressure, _nextAction, _timeWindow, _notes, _pinned);
        FormValidationResult validation = LifeRoutineService.Validate(draft); _issues = validation.Issues;
        if (!validation.IsValid)
        {
            _problem = new UserFacingProblem("life-routine-validation-failed", "Review the life-routine fields", "No life routine was added because one or more fields are invalid.", "Correct the highlighted fields, then try again.", true); Render(); return;
        }
        try
        {
            LifeRoutineRecord record = LifeRoutineService.Create(draft, DateTimeOffset.UtcNow);
            List<LifeRoutineRecord> candidate = [.. _records, record]; LifeRoutineStorage.Save(candidate); _records = candidate;
            _title = string.Empty; _nextAction = string.Empty; _notes = string.Empty; _issues = []; _problem = null; _notice = $"Added {record.Title}."; Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { _problem = UserFacingProblemFactory.FromException(exception, "save the life routine"); Render(); }
    }

    private UIElement RecordCard(LifeRoutineRecord record)
    {
        StackPanel body = new(); DockPanel header = new();
        TextBlock state = Text(record.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold); DockPanel.SetDock(state, Dock.Right); header.Children.Add(state); header.Children.Add(Heading((record.Pinned ? "★ " : "") + record.Title, 18)); body.Children.Add(header);
        body.Children.Add(Text($"{record.Date:yyyy-MM-dd} • {record.Area} • {record.Kind} • {record.Pressure}" + (string.IsNullOrWhiteSpace(record.TimeWindow) ? string.Empty : $" • {record.TimeWindow}"), 12, "#A9B6CA"));
        body.Children.Add(Text($"Next: {record.NextAction}", 13, "#E7ECF4", FontWeights.SemiBold)); if (!string.IsNullOrWhiteSpace(record.Notes)) body.Children.Add(Text(record.Notes, 11, "#A9B6CA"));
        WrapPanel actions = new();
        foreach ((string label, LifeRoutineState next) in Actions(record.State))
        { Button button = Button(label, $"Life.State.{next}.{record.Id}", true); button.Click += (_, _) => Transition(record, next); actions.Children.Add(button); }
        body.Children.Add(actions); return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private static IReadOnlyList<(string, LifeRoutineState)> Actions(LifeRoutineState state) => state switch
    {
        LifeRoutineState.Planned => [("Start", LifeRoutineState.Active), ("Wait", LifeRoutineState.Waiting), ("Defer", LifeRoutineState.Deferred), ("Done", LifeRoutineState.Done)],
        LifeRoutineState.Active => [("Wait", LifeRoutineState.Waiting), ("Defer", LifeRoutineState.Deferred), ("Done", LifeRoutineState.Done)],
        LifeRoutineState.Waiting => [("Resume", LifeRoutineState.Active), ("Defer", LifeRoutineState.Deferred), ("Done", LifeRoutineState.Done)],
        LifeRoutineState.Deferred => [("Plan", LifeRoutineState.Planned), ("Start", LifeRoutineState.Active), ("Done", LifeRoutineState.Done)],
        LifeRoutineState.Done => [("Archive", LifeRoutineState.Archived)],
        _ => []
    };

    private void Transition(LifeRoutineRecord record, LifeRoutineState next)
    {
        try
        {
            LifeRoutineRecord changed = LifeRoutineService.Transition(record, next, DateTimeOffset.UtcNow);
            List<LifeRoutineRecord> candidate = _records.Select(value => value.Id == record.Id ? changed : value).ToList(); LifeRoutineStorage.Save(candidate); _records = candidate;
            _problem = null; _notice = $"{record.Title} is now {next.ToString().ToLowerInvariant()}."; Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException)
        { _problem = UserFacingProblemFactory.FromException(exception, "change the life-routine state"); Render(); }
    }

    private UIElement Input(string label, string fieldId, string automationId, string value, Action<string> changed, double width)
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        TextBox input = new() { Text = value, MinHeight = 38, Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7) };
        AutomationProperties.SetAutomationId(input, automationId); input.TextChanged += (_, _) => changed(input.Text); field.Children.Add(input);
        string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message)); if (!string.IsNullOrWhiteSpace(error)) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold)); return field;
    }

    private static UIElement Choice<T>(string label, string automationId, IReadOnlyList<T> values, T selected, Action<T> changed, double width) where T : struct
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 }; AutomationProperties.SetAutomationId(input, automationId); input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); }; field.Children.Add(input); return field;
    }

    private static Border Problem(UserFacingProblem problem) { StackPanel body = new(); body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16)); body.Children.Add(Text(problem.Detail, 12, "#E1E7F0")); body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF")); Border panel = new() { Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body }; AutomationProperties.SetAutomationId(panel, "Life.Problem"); return panel; }
    private static Button Button(string label, string id, bool secondary) { Button button = new() { Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White, BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 8, 8, 0), MinHeight = 36 }; AutomationProperties.SetAutomationId(button, id); return button; }
    private static Border Metric(string label, string value, string detail) { StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0")); content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0")); return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content }; }
    private static Border Card(string title, string body, string background) { StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC")); return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content }; }
    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

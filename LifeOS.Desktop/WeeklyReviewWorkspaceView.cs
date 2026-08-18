using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.WeeklyReview;
using LifeOS.Shared.Storage;
using LifeOS.Shared.WeeklyReview;

namespace LifeOS.Desktop;

public sealed class WeeklyReviewWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<WeeklyReviewRecord> _records = [];
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _weekStart = Monday(DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd");
    private string _done = string.Empty;
    private string _moved = string.Empty;
    private string _waiting = string.Empty;
    private string _focus = string.Empty;
    private string _notes = string.Empty;
    private WeeklyReviewPressure _pressure = WeeklyReviewPressure.Normal;

    public WeeklyReviewWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220"); Foreground = Brushes.White; FontFamily = new FontFamily("Segoe UI");
        try { _records = WeeklyReviewStorage.Load(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        { _problem = UserFacingProblemFactory.FromException(exception, "load weekly reviews"); }
        Render();
    }

    private void Render()
    {
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(_portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL REVIEW", 11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Weekly Review & Personal Planning", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text("Close the week deliberately, make pressure visible and choose the next focus. Every state change is manual; nothing is rolled forward, scheduled, messaged or assigned automatically.", 14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice)) root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        if (_problem is not null) root.Children.Add(Problem(_problem));

        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Draft", Count(WeeklyReviewState.Draft), "Still editable locally"));
        metrics.Children.Add(Metric("Ready", Count(WeeklyReviewState.Ready), "Explicitly reviewed"));
        metrics.Children.Add(Metric("Closed", Count(WeeklyReviewState.Closed), "Manually completed"));
        metrics.Children.Add(Metric("High pressure", _records.Count(record => record.Pressure is WeeklyReviewPressure.High or WeeklyReviewPressure.Critical && record.State != WeeklyReviewState.Archived).ToString(), "No automatic escalation"));
        root.Children.Add(metrics); root.Children.Add(Capture());
        root.Children.Add(Heading("Weekly review history", 21, new Thickness(0, 20, 0, 6)));
        if (_records.Count == 0)
            root.Children.Add(Card("No weekly reviews yet", "Ordinary mode does not seed achievements, blockers, pressure or next-week plans.", "#151F30"));
        else
            foreach (WeeklyReviewRecord record in _records) root.Children.Add(RecordCard(record));
        LocalStoreHealth health = WeeklyReviewStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card("Versioned weekly-review store", $"State: {health.State}. Recovery is available in Local Data & Recovery. No task, calendar event, message, provider write or background roll-forward is created.", "#152437"));
        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = root };
    }

    private UIElement Capture()
    {
        StackPanel body = new(); body.Children.Add(Heading("Capture a weekly review", 20));
        body.Children.Add(Text("Required fields are validated before authoritative memory or disk changes.", 12, "#A9B6CA"));
        WrapPanel first = new();
        first.Children.Add(Input("Week start (YYYY-MM-DD) *", "weekly-review-week", "WeeklyReview.WeekStart", _weekStart, value => _weekStart = value, 350, false));
        first.Children.Add(Choice("Pressure", "WeeklyReview.Pressure", Enum.GetValues<WeeklyReviewPressure>(), _pressure, value => _pressure = value, 300)); body.Children.Add(first);
        body.Children.Add(Input("What got done *", "weekly-review-done", "WeeklyReview.Done", _done, value => _done = value, 1000, true));
        body.Children.Add(Input("What moved", "weekly-review-moved", "WeeklyReview.Moved", _moved, value => _moved = value, 1000, true));
        body.Children.Add(Input("Still waiting on", "weekly-review-waiting", "WeeklyReview.Waiting", _waiting, value => _waiting = value, 1000, true));
        body.Children.Add(Input("Next-week focus *", "weekly-review-focus", "WeeklyReview.Focus", _focus, value => _focus = value, 1000, true));
        body.Children.Add(Input("Notes", "weekly-review-notes", "WeeklyReview.Notes", _notes, value => _notes = value, 1000, true));
        Button add = Button("Add weekly review", "WeeklyReview.Add", false); add.Click += (_, _) => Add(); body.Children.Add(add);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void Add()
    {
        WeeklyReviewDraft draft = new(_weekStart, _done, _moved, _waiting, _pressure, _focus, _notes);
        FormValidationResult validation = WeeklyReviewService.Validate(draft); _issues = validation.Issues;
        if (!validation.IsValid)
        {
            _problem = new UserFacingProblem("weekly-review-validation-failed", "Review the weekly-review fields", "No weekly review was added because one or more fields are invalid.", "Correct the highlighted fields, then try again.", true); Render(); return;
        }
        try
        {
            WeeklyReviewRecord record = WeeklyReviewService.Create(draft, DateTimeOffset.UtcNow);
            List<WeeklyReviewRecord> candidate = [.. _records, record]; WeeklyReviewStorage.Save(candidate); _records = candidate;
            _done = string.Empty; _moved = string.Empty; _waiting = string.Empty; _focus = string.Empty; _notes = string.Empty;
            _issues = []; _problem = null; _notice = $"Added the review for {record.WeekStart:yyyy-MM-dd}."; Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { _problem = UserFacingProblemFactory.FromException(exception, "save the weekly review"); Render(); }
    }

    private UIElement RecordCard(WeeklyReviewRecord record)
    {
        StackPanel body = new(); DockPanel header = new();
        TextBlock state = Text(record.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold); DockPanel.SetDock(state, Dock.Right); header.Children.Add(state); header.Children.Add(Heading($"Week of {record.WeekStart:yyyy-MM-dd}", 18)); body.Children.Add(header);
        body.Children.Add(Text($"Pressure: {record.Pressure}", 12, "#A9B6CA", FontWeights.SemiBold));
        body.Children.Add(Text($"Done: {record.WhatGotDone}", 13, "#E7ECF4"));
        if (!string.IsNullOrWhiteSpace(record.WhatMoved)) body.Children.Add(Text($"Moved: {record.WhatMoved}", 12, "#C2CDDC"));
        if (!string.IsNullOrWhiteSpace(record.WaitingOn)) body.Children.Add(Text($"Waiting: {record.WaitingOn}", 12, "#C2CDDC"));
        body.Children.Add(Text($"Next focus: {record.NextWeekFocus}", 13, "#E7ECF4", FontWeights.SemiBold));
        if (!string.IsNullOrWhiteSpace(record.Notes)) body.Children.Add(Text(record.Notes, 11, "#A9B6CA"));
        WrapPanel actions = new();
        foreach ((string label, WeeklyReviewState next) in Actions(record.State))
        { Button button = Button(label, $"WeeklyReview.State.{next}.{record.Id}", true); button.Click += (_, _) => Transition(record, next); actions.Children.Add(button); }
        body.Children.Add(actions); return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private static IReadOnlyList<(string, WeeklyReviewState)> Actions(WeeklyReviewState state) => state switch
    {
        WeeklyReviewState.Draft => [("Mark ready", WeeklyReviewState.Ready)],
        WeeklyReviewState.Ready => [("Return to draft", WeeklyReviewState.Draft), ("Close week", WeeklyReviewState.Closed)],
        WeeklyReviewState.Closed => [("Archive", WeeklyReviewState.Archived)],
        _ => []
    };

    private void Transition(WeeklyReviewRecord record, WeeklyReviewState next)
    {
        try
        {
            WeeklyReviewRecord changed = WeeklyReviewService.Transition(record, next, DateTimeOffset.UtcNow);
            List<WeeklyReviewRecord> candidate = _records.Select(value => value.Id == record.Id ? changed : value).ToList(); WeeklyReviewStorage.Save(candidate); _records = candidate;
            _problem = null; _notice = $"The {record.WeekStart:yyyy-MM-dd} review is now {next.ToString().ToLowerInvariant()}."; Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException)
        { _problem = UserFacingProblemFactory.FromException(exception, "change the weekly-review state"); Render(); }
    }

    private UIElement Input(string label, string fieldId, string automationId, string value, Action<string> changed, double width, bool multiline)
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        TextBox input = new() { Text = value, MinHeight = multiline ? 58 : 38, AcceptsReturn = multiline, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden, Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7) };
        AutomationProperties.SetAutomationId(input, automationId); input.TextChanged += (_, _) => changed(input.Text); field.Children.Add(input);
        string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message)); if (!string.IsNullOrWhiteSpace(error)) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold)); return field;
    }

    private static UIElement Choice<T>(string label, string automationId, IReadOnlyList<T> values, T selected, Action<T> changed, double width) where T : struct
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 }; AutomationProperties.SetAutomationId(input, automationId); input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); }; field.Children.Add(input); return field;
    }

    private string Count(WeeklyReviewState state) => _records.Count(record => record.State == state).ToString();
    private static DateOnly Monday(DateOnly date) => date.AddDays(-((7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7));
    private static Border Problem(UserFacingProblem problem) { StackPanel body = new(); body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16)); body.Children.Add(Text(problem.Detail, 12, "#E1E7F0")); body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF")); Border panel = new() { Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body }; AutomationProperties.SetAutomationId(panel, "WeeklyReview.Problem"); return panel; }
    private static Button Button(string label, string id, bool secondary) { Button button = new() { Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White, BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 8, 8, 0), MinHeight = 36 }; AutomationProperties.SetAutomationId(button, id); return button; }
    private static Border Metric(string label, string value, string detail) { StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0")); content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0")); return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content }; }
    private static Border Card(string title, string body, string background) { StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC")); return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content }; }
    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

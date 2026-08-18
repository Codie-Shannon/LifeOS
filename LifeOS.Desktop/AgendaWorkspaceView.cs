using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Agenda;
using LifeOS.Core.Forms;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class AgendaWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<AgendaItem> _items = [];
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _title = string.Empty;
    private string _dueDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    private string _time = string.Empty;
    private string _nextAction = string.Empty;
    private string _notes = string.Empty;
    private AgendaItemType _type = AgendaItemType.Task;
    private AgendaPressureLevel _pressure = AgendaPressureLevel.Normal;
    private bool _fixed;

    public AgendaWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220"); Foreground = Brushes.White; FontFamily = new FontFamily("Segoe UI");
        try { _items = AgendaStorage.Load(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        { _problem = UserFacingProblemFactory.FromException(exception, "load agenda commitments"); }
        Render();
    }

    private void Render()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        AgendaSummary summary = AgendaCalculator.Calculate(_items, today);
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(_portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL AGENDA", 11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Agenda & Commitment Planning", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text("Capture the commitments that change pressure, keep the next action visible and change state deliberately. Nothing is scheduled, reminded, messaged or paid automatically.", 14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice)) root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        if (_problem is not null) root.Children.Add(Problem(_problem));
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Open", summary.TotalOpen.ToString(), "Visible commitments"));
        metrics.Children.Add(Metric("Due today", summary.DueTodayCount.ToString(), "Local date context"));
        metrics.Children.Add(Metric("Overdue", summary.OverdueCount.ToString(), "No automatic escalation"));
        metrics.Children.Add(Metric("High pressure", summary.HighPressureCount.ToString(), "Review required"));
        root.Children.Add(metrics); root.Children.Add(Capture());
        root.Children.Add(Heading("Commitment state", 21, new Thickness(0, 20, 0, 6)));
        if (_items.Count == 0) root.Children.Add(Card("No agenda commitments yet", "Ordinary mode does not seed tasks, appointments, deadlines, payments or follow-ups.", "#151F30"));
        else foreach (AgendaItem item in _items) root.Children.Add(ItemCard(item));
        LocalStoreHealth health = AgendaStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card("Versioned agenda store", $"State: {health.State}. Recovery is available in Local Data & Recovery. No calendar event, reminder, provider task, message, payment or background action is created.", "#152437"));
        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = root };
    }

    private UIElement Capture()
    {
        StackPanel body = new(); body.Children.Add(Heading("Add an agenda commitment", 20));
        body.Children.Add(Text("Required fields are validated before authoritative memory or disk changes.", 12, "#A9B6CA"));
        WrapPanel row1 = new(); row1.Children.Add(Input("Title *", "agenda-title", "Agenda.Title", _title, value => _title = value, 640)); row1.Children.Add(Input("Due date (YYYY-MM-DD)", "agenda-due-date", "Agenda.DueDate", _dueDate, value => _dueDate = value, 340)); body.Children.Add(row1);
        WrapPanel row2 = new(); row2.Children.Add(Choice("Type", "Agenda.Type", Enum.GetValues<AgendaItemType>(), _type, value => _type = value, 300)); row2.Children.Add(Choice("Pressure", "Agenda.Pressure", Enum.GetValues<AgendaPressureLevel>(), _pressure, value => _pressure = value, 300)); row2.Children.Add(Input("Time", "agenda-time", "Agenda.Time", _time, value => _time = value, 360)); body.Children.Add(row2);
        body.Children.Add(Input("Next action *", "agenda-next-action", "Agenda.NextAction", _nextAction, value => _nextAction = value, 990));
        body.Children.Add(Input("Notes", "agenda-notes", "Agenda.Notes", _notes, value => _notes = value, 990));
        CheckBox fixedCommitment = new() { Content = "Fixed commitment", IsChecked = _fixed, Foreground = Brushes.White, Margin = new Thickness(0, 8, 0, 0) };
        AutomationProperties.SetAutomationId(fixedCommitment, "Agenda.Fixed"); fixedCommitment.Checked += (_, _) => _fixed = true; fixedCommitment.Unchecked += (_, _) => _fixed = false; body.Children.Add(fixedCommitment);
        Button add = Button("Add agenda commitment", "Agenda.Add", false); add.Click += (_, _) => Add(); body.Children.Add(add); return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void Add()
    {
        AgendaDraft draft = new(_title, _dueDate, _time, _type, _pressure, _nextAction, _notes, _fixed);
        FormValidationResult validation = AgendaService.Validate(draft); _issues = validation.Issues;
        if (!validation.IsValid) { _problem = new UserFacingProblem("agenda-validation-failed", "Review the agenda fields", "No agenda commitment was added because one or more fields are invalid.", "Correct the highlighted fields, then try again.", true); Render(); return; }
        try
        {
            AgendaItem item = AgendaService.Create(draft, DateTimeOffset.Now); List<AgendaItem> candidate = [.. _items, item]; AgendaStorage.Save(candidate); _items = candidate;
            _title = string.Empty; _nextAction = string.Empty; _notes = string.Empty; _issues = []; _problem = null; _notice = $"Added {item.Title}."; Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { _problem = UserFacingProblemFactory.FromException(exception, "save the agenda commitment"); Render(); }
    }

    private UIElement ItemCard(AgendaItem item)
    {
        StackPanel body = new(); DockPanel header = new(); TextBlock state = Text(item.Status.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold); DockPanel.SetDock(state, Dock.Right); header.Children.Add(state); header.Children.Add(Heading((item.IsFixedCommitment ? "◆ " : "") + item.Title, 18)); body.Children.Add(header);
        body.Children.Add(Text($"{item.Type} • {item.PressureLevel}" + (item.DueDate is null ? string.Empty : $" • due {item.DueDate:yyyy-MM-dd}") + (string.IsNullOrWhiteSpace(item.TimeText) ? string.Empty : $" • {item.TimeText}"), 12, "#A9B6CA"));
        body.Children.Add(Text($"Next: {item.NextAction}", 13, "#E7ECF4", FontWeights.SemiBold)); if (!string.IsNullOrWhiteSpace(item.Notes)) body.Children.Add(Text(item.Notes, 11, "#A9B6CA"));
        WrapPanel actions = new(); foreach ((string label, AgendaItemStatus next) in Actions(item.Status)) { Button button = Button(label, $"Agenda.State.{next}.{item.Id:N}", true); button.Click += (_, _) => Transition(item, next); actions.Children.Add(button); } body.Children.Add(actions); return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private static IReadOnlyList<(string, AgendaItemStatus)> Actions(AgendaItemStatus state) => state switch
    {
        AgendaItemStatus.Planned => [("Start", AgendaItemStatus.InProgress), ("Wait", AgendaItemStatus.Waiting), ("Park", AgendaItemStatus.Parked), ("Complete", AgendaItemStatus.Completed), ("Cancel", AgendaItemStatus.Cancelled)],
        AgendaItemStatus.InProgress => [("Wait", AgendaItemStatus.Waiting), ("Park", AgendaItemStatus.Parked), ("Complete", AgendaItemStatus.Completed), ("Cancel", AgendaItemStatus.Cancelled)],
        AgendaItemStatus.Waiting => [("Resume", AgendaItemStatus.InProgress), ("Park", AgendaItemStatus.Parked), ("Complete", AgendaItemStatus.Completed), ("Cancel", AgendaItemStatus.Cancelled)],
        AgendaItemStatus.Parked => [("Plan", AgendaItemStatus.Planned), ("Start", AgendaItemStatus.InProgress), ("Cancel", AgendaItemStatus.Cancelled)],
        AgendaItemStatus.Completed or AgendaItemStatus.Cancelled => [("Reopen", AgendaItemStatus.Planned)], _ => []
    };

    private void Transition(AgendaItem item, AgendaItemStatus next)
    {
        try { AgendaItem changed = AgendaService.Transition(item, next, DateTimeOffset.Now); List<AgendaItem> candidate = _items.Select(value => value.Id == item.Id ? changed : value).ToList(); AgendaStorage.Save(candidate); _items = candidate; _problem = null; _notice = $"{item.Title} is now {next.ToString().ToLowerInvariant()}."; Render(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException) { _problem = UserFacingProblemFactory.FromException(exception, "change the agenda state"); Render(); }
    }

    private UIElement Input(string label, string fieldId, string automationId, string value, Action<string> changed, double width) { StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold)); TextBox input = new() { Text = value, MinHeight = 38, Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7) }; AutomationProperties.SetAutomationId(input, automationId); input.TextChanged += (_, _) => changed(input.Text); field.Children.Add(input); string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message)); if (!string.IsNullOrWhiteSpace(error)) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold)); return field; }
    private static UIElement Choice<T>(string label, string automationId, IReadOnlyList<T> values, T selected, Action<T> changed, double width) where T : struct { StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) }; field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold)); ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 }; AutomationProperties.SetAutomationId(input, automationId); input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); }; field.Children.Add(input); return field; }
    private static Border Problem(UserFacingProblem problem) { StackPanel body = new(); body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16)); body.Children.Add(Text(problem.Detail, 12, "#E1E7F0")); body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF")); Border panel = new() { Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body }; AutomationProperties.SetAutomationId(panel, "Agenda.Problem"); return panel; }
    private static Button Button(string label, string id, bool secondary) { Button button = new() { Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White, BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 8, 8, 0), MinHeight = 36 }; AutomationProperties.SetAutomationId(button, id); return button; }
    private static Border Metric(string label, string value, string detail) { StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0")); content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0")); return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content }; }
    private static Border Card(string title, string body, string background) { StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC")); return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content }; }
    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

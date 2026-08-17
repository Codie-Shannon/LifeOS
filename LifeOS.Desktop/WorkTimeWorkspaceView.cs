using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.WorkSessions;
using LifeOS.Shared.Storage;
using LifeOS.Shared.WorkSessions;

namespace LifeOS.Desktop;

public sealed class WorkTimeWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<WorkSession> _sessions = [];
    private string? _notice;
    private UserFacingProblem? _problem;

    private TextBox _clientProject = null!;
    private TextBox _date = null!;
    private TextBox _hours = null!;
    private TextBox _hourlyRate = null!;
    private CheckBox _billable = null!;
    private ComboBox _status = null!;
    private TextBox _description = null!;
    private TextBox _notes = null!;
    private Border _problemPanel = null!;
    private TextBlock _problemTitle = null!;
    private TextBlock _problemDetail = null!;
    private TextBlock _problemRecovery = null!;
    private readonly Dictionary<string, TextBlock> _errors = new(StringComparer.Ordinal);

    public WorkTimeWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        LoadSessions();
        Render();
    }

    private void LoadSessions()
    {
        try
        {
            _sessions = WorkSessionStorage.Load();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            _sessions = [];
            _problem = UserFacingProblemFactory.FromException(exception, "load work sessions");
        }
    }

    private void Render()
    {
        _errors.Clear();
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(
            _portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL WORK RECORDS",
            11,
            "#AFA4FF",
            FontWeights.SemiBold));
        root.Children.Add(Text("Work Time & Billable Records", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text(
            "Record completed or planned work, preserve its billing state, and see what remains unpaid. This workspace does not invoice a client, move money or send a message.",
            14,
            "#B8C5D8"));

        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        root.Children.Add(BuildProblemPanel());

        WorkSessionSummary summary = WorkSessionCalculator.Calculate(_sessions);
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Sessions", summary.TotalSessions.ToString(CultureInfo.InvariantCulture), "Retained locally"));
        metrics.Children.Add(Metric("Total hours", summary.TotalHours.ToString("0.##", CultureInfo.InvariantCulture), "All classifications"));
        metrics.Children.Add(Metric("Billable hours", summary.BillableHours.ToString("0.##", CultureInfo.InvariantCulture), "Not cancelled"));
        metrics.Children.Add(Metric("Billable value", summary.BillableValue.ToString("C", CultureInfo.CurrentCulture), "Tracked, not guaranteed"));
        metrics.Children.Add(Metric("Unpaid", summary.UnpaidBillableValue.ToString("C", CultureInfo.CurrentCulture), "Not marked paid"));
        root.Children.Add(metrics);

        root.Children.Add(BuildCaptureForm());
        root.Children.Add(Heading("Work records", 21, new Thickness(0, 20, 0, 6)));

        if (_sessions.Count == 0)
        {
            root.Children.Add(Card(
                "No work sessions yet",
                "Record the first real session above. Ordinary mode does not seed fictional clients, projects, hours, rates or revenue.",
                "#151F30"));
        }
        else
        {
            foreach (WorkSession session in _sessions
                .OrderByDescending(item => item.Date)
                .ThenBy(item => item.ClientOrProject, StringComparer.OrdinalIgnoreCase))
            {
                root.Children.Add(SessionCard(session));
            }
        }

        LocalStoreHealth health = WorkSessionStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card(
            "Versioned work-session store",
            $"State: {health.State}. Saves are atomic and backup-aware. Recovery is available in Local Data & Recovery; there is no permanent-delete, provider write, automatic invoice, payment claim or client communication control here.",
            "#152437"));

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = root
        };
    }

    private UIElement BuildCaptureForm()
    {
        StackPanel body = new();
        body.Children.Add(Heading("Record a local work session", 20));
        body.Children.Add(Text(
            "Required fields are marked. A paid status is only a local classification; LifeOS does not verify a bank payment here.",
            12,
            "#A9B6CA"));

        _clientProject = Input("Client or project", "WorkTime.ClientProject", 160);
        _date = Input("yyyy-mm-dd", "WorkTime.Date", 10);
        _date.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _hours = Input("Hours (0–24)", "WorkTime.Hours", 12);
        _hourlyRate = Input("Hourly rate", "WorkTime.HourlyRate", 18);
        _billable = new CheckBox
        {
            Content = "Billable",
            IsChecked = true,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 12, 0, 0),
            MinHeight = 30
        };
        AutomationProperties.SetAutomationId(_billable, "WorkTime.Billable");
        _status = new ComboBox
        {
            ItemsSource = Enum.GetValues<WorkSessionStatus>()
                .Where(value => value != WorkSessionStatus.NonBillable),
            SelectedItem = WorkSessionStatus.Completed,
            MinHeight = 38,
            Margin = new Thickness(0, 5, 0, 0)
        };
        AutomationProperties.SetAutomationId(_status, "WorkTime.Status");
        _description = Input("What was done", "WorkTime.Description", 500);
        _notes = Input("Notes", "WorkTime.Notes", 4000, acceptsReturn: true, height: 88);

        WrapPanel fields = new() { Margin = new Thickness(0, 10, 0, 0) };
        fields.Children.Add(Field("Client or project *", _clientProject, "work-client-project"));
        fields.Children.Add(Field("Date *", _date, "work-date"));
        fields.Children.Add(Field("Hours *", _hours, "work-hours"));
        fields.Children.Add(Field("Hourly rate", _hourlyRate, "work-hourly-rate"));
        fields.Children.Add(Field("Billing", _billable));
        fields.Children.Add(Field("Status", _status, "work-status"));
        body.Children.Add(fields);
        body.Children.Add(Field("Description *", _description, "work-description", wide: true));
        body.Children.Add(Field("Notes", _notes, "work-notes", wide: true));

        Button save = ActionButton("Record session", "WorkTime.Record", false);
        save.Click += (_, _) => SaveSession();
        body.Children.Add(save);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void SaveSession()
    {
        List<FormFieldIssue> parseIssues = [];
        DateOnly date = DateOnly.FromDateTime(DateTime.Today);
        if (!DateOnly.TryParseExact(_date.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            parseIssues.Add(new FormFieldIssue("work-date", "date-format", "Date must use yyyy-mm-dd."));
        }

        decimal hours = 0m;
        if (!decimal.TryParse(_hours.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out hours))
            parseIssues.Add(new FormFieldIssue("work-hours", "number-format", "Hours must be a number."));

        decimal hourlyRate = 0m;
        if (!string.IsNullOrWhiteSpace(_hourlyRate.Text) &&
            !decimal.TryParse(_hourlyRate.Text.Trim(), NumberStyles.Currency, CultureInfo.CurrentCulture, out hourlyRate))
        {
            parseIssues.Add(new FormFieldIssue("work-hourly-rate", "number-format", "Hourly rate must be a number."));
        }

        WorkSessionDraft draft = new(
            _clientProject.Text,
            date,
            hours,
            hourlyRate,
            _billable.IsChecked == true,
            _status.SelectedItem is WorkSessionStatus status ? status : WorkSessionStatus.Completed,
            _description.Text,
            _notes.Text);
        FormValidationResult coreValidation = WorkSessionService.Validate(draft);
        FormValidationResult validation = new([.. coreValidation.Issues, .. parseIssues]);
        ShowIssues(validation);
        if (!validation.IsValid)
        {
            ShowProblem(new UserFacingProblem(
                "work-session-validation-failed",
                "Review the highlighted work fields",
                "The work session was not saved because one or more values are invalid.",
                "Correct the highlighted fields, then choose Record session again.",
                true));
            return;
        }

        try
        {
            WorkSession session = WorkSessionService.Create(draft, DateTime.Now);
            List<WorkSession> candidate = [.. _sessions, session];
            WorkSessionStorage.Save(candidate);
            _sessions = candidate;
            _notice = $"Recorded {session.Hours:0.##} hour(s) for {session.ClientOrProject} locally.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "save the work session"));
        }
    }

    private UIElement SessionCard(WorkSession session)
    {
        StackPanel body = new();
        DockPanel titleRow = new();
        TextBlock status = Text(StatusLabel(session), 11, "#AFA4FF", FontWeights.SemiBold);
        DockPanel.SetDock(status, Dock.Right);
        titleRow.Children.Add(status);
        titleRow.Children.Add(Heading(session.ClientOrProject, 18));
        body.Children.Add(titleRow);
        body.Children.Add(Text(session.Description, 13, "#C4CEDD"));
        string rate = session.IsBillable
            ? session.HourlyRate.ToString("C", CultureInfo.CurrentCulture) + "/hour"
            : "non-billable";
        body.Children.Add(Text(
            $"{session.Date:yyyy-MM-dd} • {session.Hours:0.##} hour(s) • {rate} • value {session.BillableValue.ToString("C", CultureInfo.CurrentCulture)}",
            12,
            "#A9B6CA"));
        if (!string.IsNullOrWhiteSpace(session.Notes))
            body.Children.Add(Text(session.Notes, 12, "#8FA0B8"));

        if (session.IsBillable && session.Status is not WorkSessionStatus.Cancelled and not WorkSessionStatus.Paid)
        {
            WrapPanel actions = new() { Margin = new Thickness(0, 8, 0, 0) };
            if (session.Status != WorkSessionStatus.Completed)
                actions.Children.Add(StatusButton("Mark completed", session, WorkSessionStatus.Completed));
            if (session.Status != WorkSessionStatus.Invoiced)
                actions.Children.Add(StatusButton("Mark invoiced", session, WorkSessionStatus.Invoiced));
            actions.Children.Add(StatusButton("Mark paid", session, WorkSessionStatus.Paid));
            actions.Children.Add(StatusButton("Cancel", session, WorkSessionStatus.Cancelled));
            body.Children.Add(actions);
        }
        return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private Button StatusButton(string label, WorkSession session, WorkSessionStatus status)
    {
        Button button = ActionButton(label, $"WorkTime.{status}.{session.Id:N}", true);
        button.Click += (_, _) => ChangeStatus(session, status);
        return button;
    }

    private void ChangeStatus(WorkSession session, WorkSessionStatus status)
    {
        try
        {
            WorkSession changed = WorkSessionService.ChangeStatus(session, status, DateTime.Now);
            List<WorkSession> candidate = _sessions
                .Select(item => item.Id == session.Id ? changed : item)
                .ToList();
            WorkSessionStorage.Save(candidate);
            _sessions = candidate;
            _notice = $"{session.ClientOrProject} is now {status.ToString().ToLowerInvariant()}.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "update the work session"));
        }
    }

    private static string StatusLabel(WorkSession session) =>
        session.IsBillable ? session.Status.ToString().ToUpperInvariant() : "NON-BILLABLE";

    private Border BuildProblemPanel()
    {
        _problemTitle = Heading(string.Empty, 16);
        _problemDetail = Text(string.Empty, 12, "#E1E7F0");
        _problemRecovery = Text(string.Empty, 12, "#C5AECF");
        StackPanel body = new();
        body.Children.Add(_problemTitle);
        body.Children.Add(_problemDetail);
        body.Children.Add(_problemRecovery);
        _problemPanel = new Border
        {
            Visibility = Visibility.Collapsed,
            Background = Brush("#251925"),
            BorderBrush = Brush("#C95F75"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 12, 0, 0),
            Child = body
        };
        AutomationProperties.SetAutomationId(_problemPanel, "WorkTime.Problem");
        if (_problem is not null) ShowProblem(_problem);
        return _problemPanel;
    }

    private void ShowProblem(UserFacingProblem problem)
    {
        _problem = problem;
        _problemTitle.Text = $"{problem.Title} ({problem.Code})";
        _problemDetail.Text = problem.Detail;
        _problemRecovery.Text = $"Next: {problem.RecoveryAction}";
        AutomationProperties.SetName(
            _problemPanel,
            $"{problem.Title}. {problem.Detail} Next: {problem.RecoveryAction}");
        _problemPanel.Visibility = Visibility.Visible;
    }

    private void ShowIssues(FormValidationResult validation)
    {
        foreach (TextBlock block in _errors.Values)
        {
            block.Text = string.Empty;
            block.Visibility = Visibility.Collapsed;
        }
        foreach ((string fieldId, TextBlock block) in _errors)
        {
            string message = string.Join(" ", validation.ForField(fieldId).Select(issue => issue.Message));
            if (!string.IsNullOrEmpty(message))
            {
                block.Text = message;
                block.Visibility = Visibility.Visible;
            }
        }
    }

    private UIElement Field(string label, Control control, string? fieldId = null, bool wide = false)
    {
        StackPanel field = new()
        {
            Width = wide ? 1010 : 490,
            Margin = new Thickness(0, 5, 10, 8)
        };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        field.Children.Add(control);
        if (fieldId is not null)
        {
            TextBlock error = Text(string.Empty, 11, "#FF7788", FontWeights.SemiBold);
            error.Visibility = Visibility.Collapsed;
            AutomationProperties.SetAutomationId(error, $"WorkTime.Error.{fieldId}");
            _errors[fieldId] = error;
            field.Children.Add(error);
        }
        return field;
    }

    private static TextBox Input(
        string placeholder,
        string automationId,
        int maximumLength,
        bool acceptsReturn = false,
        double height = 38)
    {
        TextBox box = new()
        {
            MaxLength = maximumLength,
            MinHeight = height,
            AcceptsReturn = acceptsReturn,
            TextWrapping = acceptsReturn ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalScrollBarVisibility = acceptsReturn ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
            Background = Brush("#101827"),
            Foreground = Brushes.White,
            BorderBrush = Brush("#3A4B66"),
            Padding = new Thickness(10, 7, 10, 7),
            Margin = new Thickness(0, 5, 0, 0),
            ToolTip = placeholder
        };
        AutomationProperties.SetAutomationId(box, automationId);
        AutomationProperties.SetName(box, placeholder);
        return box;
    }

    private static Button ActionButton(string label, string automationId, bool secondary)
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
        AutomationProperties.SetAutomationId(button, automationId);
        return button;
    }

    private static Border Metric(string label, string value, string detail)
    {
        StackPanel content = new();
        content.Children.Add(Text(label, 11, "#9EACC0"));
        content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold));
        content.Children.Add(Text(detail, 11, "#9EACC0"));
        return new Border
        {
            Width = 190,
            MinHeight = 100,
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
        content.Children.Add(Heading(title, 16));
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

    private static Border Panel(UIElement content, Thickness margin) => new()
    {
        Background = Brush("#151F30"),
        BorderBrush = Brush("#31445F"),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(9),
        Padding = new Thickness(16),
        Margin = margin,
        Child = content
    };

    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brushes.White,
        TextWrapping = TextWrapping.Wrap,
        Margin = margin ?? new Thickness(0, 0, 0, 4)
    };

    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight ?? FontWeights.Normal,
        Foreground = Brush(color),
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 3, 0, 0)
    };

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

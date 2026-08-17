using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.Projects;
using LifeOS.Shared.Projects;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class ProjectsWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private List<ProjectRecord> _projects = [];
    private string? _notice;
    private UserFacingProblem? _problem;

    private TextBox _name = null!;
    private TextBox _summary = null!;
    private ComboBox _status = null!;
    private TextBox _nextAction = null!;
    private TextBox _dueDate = null!;
    private TextBox _evidence = null!;
    private TextBox _notes = null!;
    private Border _problemPanel = null!;
    private TextBlock _problemTitle = null!;
    private TextBlock _problemDetail = null!;
    private TextBlock _problemRecovery = null!;
    private readonly Dictionary<string, TextBlock> _errors = new(StringComparer.Ordinal);

    public ProjectsWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        LoadProjects();
        Render();
    }

    private void LoadProjects()
    {
        try
        {
            _projects = ProjectStorage.Load();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _projects = [];
            _problem = UserFacingProblemFactory.FromException(exception, "load projects");
        }
    }

    private void Render()
    {
        _errors.Clear();
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(
            _portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL PROJECTS",
            11,
            "#AFA4FF",
            FontWeights.SemiBold));
        root.Children.Add(Text("Projects", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text(
            "Track a project, its next action, delivery state, due date and proof reference without creating provider activity or duplicating evidence.",
            14,
            "#B8C5D8"));

        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));

        root.Children.Add(BuildProblemPanel());

        ProjectOverview overview = ProjectService.Calculate(
            _projects,
            DateOnly.FromDateTime(DateTime.Today));
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Visible", overview.Visible.ToString(), "Not archived"));
        metrics.Children.Add(Metric("Active", overview.Active.ToString(), "Can move"));
        metrics.Children.Add(Metric("Waiting / blocked", (overview.Waiting + overview.Blocked).ToString(), "Needs review"));
        metrics.Children.Add(Metric("Due ≤ 7 days", overview.DueNextSevenDays.ToString(), "Open projects"));
        metrics.Children.Add(Metric("Completed", overview.Completed.ToString(), "Retained history"));
        root.Children.Add(metrics);

        root.Children.Add(BuildCaptureForm());
        root.Children.Add(Heading("Current projects", 21, new Thickness(0, 20, 0, 6)));

        ProjectRecord[] visible = _projects
            .Where(project => project.Status != ProjectStatus.Archived)
            .OrderBy(project => project.Status == ProjectStatus.Completed)
            .ThenBy(project => project.DueDate ?? DateOnly.MaxValue)
            .ThenBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (visible.Length == 0)
        {
            root.Children.Add(Card(
                "No projects yet",
                "Create the first real local project above. Ordinary mode does not seed portfolio examples or infer projects from provider data.",
                "#151F30"));
        }
        else
        {
            foreach (ProjectRecord project in visible)
                root.Children.Add(ProjectCard(project));
        }

        ProjectRecord[] archived = _projects
            .Where(project => project.Status == ProjectStatus.Archived)
            .OrderBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (archived.Length > 0)
        {
            root.Children.Add(Heading("Archived", 21, new Thickness(0, 20, 0, 6)));
            foreach (ProjectRecord project in archived)
                root.Children.Add(ProjectCard(project));
        }

        LocalStoreHealth health = ProjectStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card(
            "Versioned project store",
            $"State: {health.State}. Saves are atomic and backup-aware. Archive is reversible; no permanent-delete control, provider write, automatic evidence upload or client communication is available here.",
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
        body.Children.Add(Heading("Create a local project", 20));
        body.Children.Add(Text(
            "Required fields are marked. Nothing is saved until validation passes.",
            12,
            "#A9B6CA"));

        _name = Input("Project name", "Projects.Name", 121);
        _summary = Input("Summary", "Projects.Summary", 2001, acceptsReturn: true, height: 74);
        _status = new ComboBox
        {
            ItemsSource = Enum.GetValues<ProjectStatus>().Where(value => value != ProjectStatus.Archived),
            SelectedItem = ProjectStatus.Active,
            MinHeight = 38,
            Margin = new Thickness(0, 5, 0, 0)
        };
        AutomationProperties.SetAutomationId(_status, "Projects.Status");
        _nextAction = Input("Next action", "Projects.NextAction", 241);
        _dueDate = Input("yyyy-mm-dd (optional)", "Projects.DueDate", 10);
        _evidence = Input("Path, URL or proof reference (optional)", "Projects.Evidence", 501);
        _notes = Input("Notes", "Projects.Notes", 4001, acceptsReturn: true, height: 88);

        WrapPanel fields = new() { Margin = new Thickness(0, 10, 0, 0) };
        fields.Children.Add(Field("Project name *", _name, "project-name"));
        fields.Children.Add(Field("Status", _status));
        fields.Children.Add(Field("Next action *", _nextAction, "project-next-action"));
        fields.Children.Add(Field("Due date", _dueDate, "project-due-date"));
        fields.Children.Add(Field("Evidence reference", _evidence, "project-evidence"));
        body.Children.Add(fields);
        body.Children.Add(Field("Summary", _summary, "project-summary", wide: true));
        body.Children.Add(Field("Notes", _notes, "project-notes", wide: true));

        Button save = ActionButton("Create project", "Projects.Create", false);
        save.Click += (_, _) => SaveProject();
        body.Children.Add(save);

        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void SaveProject()
    {
        DateOnly? due = null;
        FormFieldIssue? dateIssue = null;
        string dueText = _dueDate.Text.Trim();
        if (!string.IsNullOrEmpty(dueText))
        {
            if (DateOnly.TryParseExact(dueText, "yyyy-MM-dd", out DateOnly parsed))
                due = parsed;
            else
                dateIssue = new FormFieldIssue(
                    "project-due-date",
                    "date-format",
                    "Due date must use yyyy-mm-dd.");
        }

        ProjectDraft draft = new(
            _name.Text,
            _summary.Text,
            _status.SelectedItem is ProjectStatus status ? status : ProjectStatus.Active,
            _nextAction.Text,
            due,
            _evidence.Text,
            _notes.Text);
        FormValidationResult coreValidation = ProjectService.Validate(draft);
        FormValidationResult validation = new(
            coreValidation.Issues
                .Concat(dateIssue is null ? [] : [dateIssue])
                .ToArray());
        ShowIssues(validation);
        if (!validation.IsValid)
        {
            ShowProblem(new UserFacingProblem(
                "project-validation-failed",
                "Review the highlighted project fields",
                "The project was not saved because one or more values are invalid.",
                "Correct the highlighted fields, then choose Create project again.",
                true));
            return;
        }

        try
        {
            ProjectRecord project = ProjectService.Create(draft, DateTimeOffset.UtcNow);
            List<ProjectRecord> candidate = [.. _projects, project];
            ProjectStorage.Save(candidate);
            _projects = candidate;
            _notice = $"Created {project.Name} locally.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "save the project"));
        }
    }

    private UIElement ProjectCard(ProjectRecord project)
    {
        StackPanel body = new();
        DockPanel titleRow = new();
        TextBlock status = Text(project.Status.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold);
        DockPanel.SetDock(status, Dock.Right);
        titleRow.Children.Add(status);
        titleRow.Children.Add(Heading(project.Name, 18));
        body.Children.Add(titleRow);
        if (!string.IsNullOrWhiteSpace(project.Summary))
            body.Children.Add(Text(project.Summary, 13, "#C4CEDD"));
        body.Children.Add(Text(
            $"Next: {project.NextAction}\nDue: {(project.DueDate?.ToString("yyyy-MM-dd") ?? "not set")}\nEvidence: {(string.IsNullOrWhiteSpace(project.EvidenceReference) ? "not linked" : project.EvidenceReference)}",
            12,
            "#A9B6CA"));
        if (!string.IsNullOrWhiteSpace(project.Notes))
            body.Children.Add(Text(project.Notes, 12, "#8FA0B8"));

        WrapPanel actions = new() { Margin = new Thickness(0, 8, 0, 0) };
        if (project.Status == ProjectStatus.Archived)
        {
            Button restore = ActionButton("Restore", $"Projects.Restore.{project.Id:N}", true);
            restore.Click += (_, _) => ChangeStatus(project, ProjectStatus.Backlog);
            actions.Children.Add(restore);
        }
        else
        {
            if (project.Status != ProjectStatus.Active)
            {
                Button active = ActionButton("Mark active", $"Projects.Active.{project.Id:N}", true);
                active.Click += (_, _) => ChangeStatus(project, ProjectStatus.Active);
                actions.Children.Add(active);
            }
            if (project.Status != ProjectStatus.Completed)
            {
                Button complete = ActionButton("Mark completed", $"Projects.Complete.{project.Id:N}", true);
                complete.Click += (_, _) => ChangeStatus(project, ProjectStatus.Completed);
                actions.Children.Add(complete);
            }
            Button archive = ActionButton("Archive", $"Projects.Archive.{project.Id:N}", true);
            archive.Click += (_, _) => ChangeStatus(project, ProjectStatus.Archived);
            actions.Children.Add(archive);
        }
        body.Children.Add(actions);
        return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private void ChangeStatus(ProjectRecord project, ProjectStatus status)
    {
        try
        {
            ProjectRecord changed = ProjectService.ChangeStatus(project, status, DateTimeOffset.UtcNow);
            List<ProjectRecord> candidate = _projects
                .Select(item => item.Id == project.Id ? changed : item)
                .ToList();
            ProjectStorage.Save(candidate);
            _projects = candidate;
            _notice = $"{project.Name} is now {status.ToString().ToLowerInvariant()}.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "update the project"));
        }
    }

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
        AutomationProperties.SetAutomationId(_problemPanel, "Projects.Problem");
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

    private UIElement Field(
        string label,
        Control control,
        string? fieldId = null,
        bool wide = false)
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
            AutomationProperties.SetAutomationId(error, $"Projects.Error.{fieldId}");
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

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

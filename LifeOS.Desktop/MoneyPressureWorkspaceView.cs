using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.Money;
using LifeOS.Shared.Money;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class MoneyPressureWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private MoneyPressureManualInput _input = new();
    private bool _hasSnapshot;
    private string? _notice;
    private UserFacingProblem? _problem;
    private readonly Dictionary<string, TextBlock> _errors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TextBox> _fields = new(StringComparer.Ordinal);
    private Border _problemPanel = null!;
    private TextBlock _problemTitle = null!;
    private TextBlock _problemDetail = null!;
    private TextBlock _problemRecovery = null!;

    public MoneyPressureWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        LoadSnapshot();
        Render();
    }

    private void LoadSnapshot()
    {
        try
        {
            LocalStoreHealth health = MoneyPressureStorage.Inspect();
            _input = MoneyPressureStorage.Load();
            _hasSnapshot = _portfolioDemo || health.State != LocalStoreHealthState.Missing;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            _input = new MoneyPressureManualInput();
            _problem = UserFacingProblemFactory.FromException(exception, "load the money snapshot");
        }
    }

    private void Render()
    {
        _errors.Clear();
        _fields.Clear();
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(
            _portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL MONEY SNAPSHOT",
            11,
            "#AFA4FF",
            FontWeights.SemiBold));
        root.Children.Add(Text("Money Pressure", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text(
            "Enter what is currently known. Pending income stays outside safe-to-spend, and every number remains a user-supplied local estimate—not a bank-verified balance.",
            14,
            "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        root.Children.Add(BuildProblemPanel());

        if (_hasSnapshot)
        {
            MoneyPressureSummary summary = _input.Calculate();
            WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
            metrics.Children.Add(Metric("Safe to spend", Money(summary.SafeToSpend), "Known commitments only"));
            metrics.Children.Add(Metric("Current balance", Money(summary.CurrentBalance), "User supplied"));
            metrics.Children.Add(Metric("Pending income", Money(summary.PendingIncome), "Not counted as safe"));
            metrics.Children.Add(Metric("Bills + deductions", Money(summary.BillsDue + summary.DeductionsDue), "Current week estimate"));
            metrics.Children.Add(Metric("Pressure", summary.PressureLabel, "Deterministic rule"));
            root.Children.Add(metrics);
            StackPanel reasons = new();
            reasons.Children.Add(Heading("Why this result", 18));
            foreach (string reason in summary.Reasons)
                reasons.Children.Add(Text("• " + reason, 12, "#C2CDDC"));
            root.Children.Add(Panel(reasons, new Thickness(0, 4, 0, 0)));
        }
        else
        {
            root.Children.Add(Card(
                "No money snapshot yet",
                "Ordinary mode does not invent balances, income, bills or buffers. Enter only values you currently know; zeros are allowed.",
                "#151F30"));
        }

        root.Children.Add(BuildForm());
        LocalStoreHealth health = MoneyPressureStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card(
            "Versioned money-pressure store",
            $"State: {health.State}. Saves are atomic and backup-aware. Recovery is available in Local Data & Recovery. No bank feed, payment initiation, accounting-provider write, automatic reconciliation or financial advice is provided.",
            "#152437"));

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = root
        };
    }

    private UIElement BuildForm()
    {
        StackPanel body = new();
        body.Children.Add(Heading(_hasSnapshot ? "Update the local snapshot" : "Create a local snapshot", 20));
        body.Children.Add(Text(
            "All fields are amounts in your working currency. Current balance may be negative; commitments and buffers cannot.",
            12,
            "#A9B6CA"));
        WrapPanel fields = new() { Margin = new Thickness(0, 10, 0, 0) };
        AddAmount(fields, "Current balance", "money-current-balance", _input.CurrentBalance);
        AddAmount(fields, "Paid income", "money-paid-income", _input.PaidIncome);
        AddAmount(fields, "Pending income", "money-pending-income", _input.PendingIncome);
        AddAmount(fields, "Bills due this week", "money-bills-due", _input.BillsDue);
        AddAmount(fields, "Fixed deductions", "money-deductions-due", _input.DeductionsDue);
        AddAmount(fields, "Food and fuel buffer", "money-food-fuel-buffer", _input.FoodFuelBuffer);
        AddAmount(fields, "Emergency buffer", "money-emergency-buffer", _input.EmergencyBuffer);
        body.Children.Add(fields);
        Button save = ActionButton("Save and calculate", "MoneyPressure.Save");
        save.Click += (_, _) => SaveSnapshot();
        body.Children.Add(save);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void AddAmount(Panel parent, string label, string fieldId, decimal value)
    {
        TextBox input = Input(label, $"MoneyPressure.{fieldId}");
        input.Text = value.ToString("0.##", CultureInfo.CurrentCulture);
        _fields[fieldId] = input;
        parent.Children.Add(Field(label, input, fieldId));
    }

    private void SaveSnapshot()
    {
        List<FormFieldIssue> parseIssues = [];
        decimal Value(string fieldId, string label)
        {
            if (decimal.TryParse(
                    _fields[fieldId].Text.Trim(),
                    NumberStyles.Currency,
                    CultureInfo.CurrentCulture,
                    out decimal value))
                return value;
            parseIssues.Add(new FormFieldIssue(fieldId, "number-format", $"{label} must be a number."));
            return 0m;
        }

        MoneyPressureDraft draft = new(
            Value("money-current-balance", "Current balance"),
            Value("money-paid-income", "Paid income"),
            Value("money-pending-income", "Pending income"),
            Value("money-bills-due", "Bills due"),
            Value("money-deductions-due", "Deductions due"),
            Value("money-food-fuel-buffer", "Food and fuel buffer"),
            Value("money-emergency-buffer", "Emergency buffer"));
        FormValidationResult core = MoneyPressureInputService.Validate(draft);
        FormValidationResult validation = new([.. core.Issues, .. parseIssues]);
        ShowIssues(validation);
        if (!validation.IsValid)
        {
            ShowProblem(new UserFacingProblem(
                "money-snapshot-validation-failed",
                "Review the highlighted money fields",
                "The money snapshot was not saved because one or more amounts are invalid.",
                "Correct the highlighted amounts, then choose Save and calculate again.",
                true));
            return;
        }

        MoneyPressureManualInput candidate = new()
        {
            CurrentBalance = draft.CurrentBalance,
            PaidIncome = draft.PaidIncome,
            PendingIncome = draft.PendingIncome,
            BillsDue = draft.BillsDue,
            DeductionsDue = draft.DeductionsDue,
            FoodFuelBuffer = draft.FoodFuelBuffer,
            EmergencyBuffer = draft.EmergencyBuffer
        };
        try
        {
            MoneyPressureStorage.Save(candidate);
            _input = candidate;
            _hasSnapshot = true;
            _notice = "Saved and recalculated the local money snapshot.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "save the money snapshot"));
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
            Background = Brush("#251925"), BorderBrush = Brush("#C95F75"),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9),
            Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body
        };
        AutomationProperties.SetAutomationId(_problemPanel, "MoneyPressure.Problem");
        if (_problem is not null) ShowProblem(_problem);
        return _problemPanel;
    }

    private void ShowProblem(UserFacingProblem problem)
    {
        _problem = problem;
        _problemTitle.Text = $"{problem.Title} ({problem.Code})";
        _problemDetail.Text = problem.Detail;
        _problemRecovery.Text = $"Next: {problem.RecoveryAction}";
        AutomationProperties.SetName(_problemPanel, $"{problem.Title}. {problem.Detail} Next: {problem.RecoveryAction}");
        _problemPanel.Visibility = Visibility.Visible;
    }

    private void ShowIssues(FormValidationResult validation)
    {
        foreach ((string fieldId, TextBlock block) in _errors)
        {
            string message = string.Join(" ", validation.ForField(fieldId).Select(issue => issue.Message));
            block.Text = message;
            block.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private UIElement Field(string label, Control control, string fieldId)
    {
        StackPanel field = new() { Width = 490, Margin = new Thickness(0, 5, 10, 8) };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        field.Children.Add(control);
        TextBlock error = Text(string.Empty, 11, "#FF7788", FontWeights.SemiBold);
        error.Visibility = Visibility.Collapsed;
        AutomationProperties.SetAutomationId(error, $"MoneyPressure.Error.{fieldId}");
        _errors[fieldId] = error;
        field.Children.Add(error);
        return field;
    }

    private static TextBox Input(string label, string automationId)
    {
        TextBox box = new()
        {
            MaxLength = 24, MinHeight = 38, Background = Brush("#101827"),
            Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"),
            Padding = new Thickness(10, 7, 10, 7), Margin = new Thickness(0, 5, 0, 0),
            ToolTip = label
        };
        AutomationProperties.SetAutomationId(box, automationId);
        AutomationProperties.SetName(box, label);
        return box;
    }

    private static Button ActionButton(string label, string automationId)
    {
        Button button = new()
        {
            Content = label, Background = Brush("#315E91"), Foreground = Brushes.White,
            BorderBrush = Brush("#477DB4"), Padding = new Thickness(14, 7, 14, 7),
            Margin = new Thickness(0, 8, 8, 0), MinHeight = 36
        };
        AutomationProperties.SetAutomationId(button, automationId);
        return button;
    }

    private static string Money(decimal value) => value.ToString("C", CultureInfo.CurrentCulture);

    private static Border Metric(string label, string value, string detail)
    {
        StackPanel content = new();
        content.Children.Add(Text(label, 11, "#9EACC0"));
        content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold));
        content.Children.Add(Text(detail, 11, "#9EACC0"));
        return new Border
        {
            Width = 190, MinHeight = 100, Background = Brush("#151F30"),
            BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9), Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 10, 10), Child = content
        };
    }

    private static Border Card(string title, string body, string background)
    {
        StackPanel content = new();
        content.Children.Add(Heading(title, 16));
        content.Children.Add(Text(body, 12, "#C2CDDC"));
        return new Border
        {
            Background = Brush(background), BorderBrush = Brush("#31445F"),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9),
            Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content
        };
    }

    private static Border Panel(UIElement content, Thickness margin) => new()
    {
        Background = Brush("#151F30"), BorderBrush = Brush("#31445F"),
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9),
        Padding = new Thickness(16), Margin = margin, Child = content
    };

    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new()
    {
        Text = text, FontSize = size, FontWeight = FontWeights.SemiBold,
        Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap,
        Margin = margin ?? new Thickness(0, 0, 0, 4)
    };

    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new()
    {
        Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal,
        Foreground = Brush(color), TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 3, 0, 0)
    };

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Documents;
using LifeOS.Core.Forms;
using LifeOS.Shared.Documents;
using LifeOS.Shared.Storage;
using Microsoft.Win32;

namespace LifeOS.Desktop;

public sealed class DocumentEvidenceWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private readonly DocumentIntakeService _service = new();
    private List<DocumentRecord> _records = [];
    private string? _selectedFileName;
    private string? _selectedMediaType;
    private byte[]? _selectedBytes;
    private string? _notice;
    private UserFacingProblem? _problem;
    private TextBox _file = null!;
    private ComboBox _type = null!;
    private Border _problemPanel = null!;
    private TextBlock _problemTitle = null!;
    private TextBlock _problemDetail = null!;
    private TextBlock _problemRecovery = null!;
    private TextBlock _fileError = null!;

    public DocumentEvidenceWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        LoadRecords();
        Render();
    }

    private void LoadRecords()
    {
        try { _records = DocumentIntakeStorage.Load(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            _records = [];
            _problem = UserFacingProblemFactory.FromException(exception, "load document intake");
        }
    }

    private void Render()
    {
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(
            _portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL ORIGINALS",
            11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Document & Evidence Intake", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text(
            "Choose a local file, preserve its original bytes and SHA-256, then review it explicitly. Classification remains user-controlled and no financial posting occurs.",
            14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        root.Children.Add(BuildProblemPanel());

        DocumentIntakeOverview overview = _service.Summarize(_records, DuplicateCandidates());
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Drafts", overview.Drafts.ToString(), "Not trusted"));
        metrics.Children.Add(Metric("Review", overview.AwaitingReview.ToString(), "Explicit decision"));
        metrics.Children.Add(Metric("Accepted", overview.Accepted.ToString(), "Original retained"));
        metrics.Children.Add(Metric("Duplicates", overview.DuplicateCandidates.ToString(), "Hash candidates"));
        metrics.Children.Add(Metric("Links", overview.EvidenceLinks.ToString(), "Read-only context"));
        root.Children.Add(metrics);
        root.Children.Add(BuildCapture());
        root.Children.Add(Heading("Preserved documents", 21, new Thickness(0, 20, 0, 6)));
        if (_records.Count == 0)
        {
            root.Children.Add(Card(
                "No documents yet",
                "Ordinary mode does not seed receipts, invoices, statements or evidence. Choose a real local file only when you want LifeOS to preserve it.",
                "#151F30"));
        }
        else
        {
            foreach (DocumentRecord record in _records.OrderByDescending(item => item.Original.ImportedUtc))
                root.Children.Add(DocumentCard(record));
        }

        LocalStoreHealth health = DocumentIntakeStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card(
            "Versioned document-intake store",
            $"State: {health.State}. Originals are integrity-checked before save and on load. Recovery is available in Local Data & Recovery; no permanent delete, automatic merge, extraction acceptance, money posting or provider upload is exposed.",
            "#152437"));
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = root
        };
    }

    private UIElement BuildCapture()
    {
        StackPanel body = new();
        body.Children.Add(Heading("Preserve a local original", 20));
        body.Children.Add(Text("Maximum size: 25 MB. Nothing is copied until Preserve original is chosen.", 12, "#A9B6CA"));
        _file = new TextBox
        {
            Text = _selectedFileName ?? string.Empty, IsReadOnly = true, MinHeight = 38,
            Background = Brush("#101827"), Foreground = Brushes.White, BorderBrush = Brush("#3A4B66"),
            Padding = new Thickness(10, 7, 10, 7), Margin = new Thickness(0, 5, 0, 0)
        };
        AutomationProperties.SetAutomationId(_file, "Documents.SelectedFile");
        _type = new ComboBox
        {
            ItemsSource = Enum.GetValues<DocumentType>(), SelectedItem = DocumentType.GeneralEvidence,
            MinHeight = 38, Margin = new Thickness(0, 5, 0, 0), Width = 490
        };
        AutomationProperties.SetAutomationId(_type, "Documents.Type");
        WrapPanel row = new() { Margin = new Thickness(0, 10, 0, 0) };
        StackPanel fileField = new() { Width = 1010, Margin = new Thickness(0, 5, 10, 8) };
        fileField.Children.Add(Text("Selected file *", 12, "#C7D2E3", FontWeights.SemiBold));
        fileField.Children.Add(_file);
        _fileError = Text(string.Empty, 11, "#FF7788", FontWeights.SemiBold);
        _fileError.Visibility = Visibility.Collapsed;
        AutomationProperties.SetAutomationId(_fileError, "Documents.Error.document-file");
        fileField.Children.Add(_fileError);
        row.Children.Add(fileField);
        body.Children.Add(row);
        body.Children.Add(Text("Document type", 12, "#C7D2E3", FontWeights.SemiBold));
        body.Children.Add(_type);
        WrapPanel actions = new();
        Button choose = ActionButton("Choose file", "Documents.Choose", true);
        choose.Click += (_, _) => ChooseFile();
        Button preserve = ActionButton("Preserve original", "Documents.Preserve", false);
        preserve.Click += (_, _) => Preserve();
        actions.Children.Add(choose);
        actions.Children.Add(preserve);
        body.Children.Add(actions);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void ChooseFile()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Choose a document or evidence file",
            Filter = "Documents and evidence|*.pdf;*.png;*.jpg;*.jpeg;*.txt;*.csv;*.docx;*.xlsx|All files|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog() != true) return;
        try
        {
            FileInfo info = new(dialog.FileName);
            if (info.Length > DocumentCaptureService.MaximumBytes)
            {
                ShowProblem(new UserFacingProblem(
                    "document-file-too-large", "The selected file is too large",
                    "LifeOS did not read or preserve the selected file.",
                    "Choose a file that is 25 MB or smaller.", true));
                return;
            }
            _selectedBytes = File.ReadAllBytes(dialog.FileName);
            _selectedFileName = info.Name;
            _selectedMediaType = MediaType(info.Extension);
            _notice = $"Selected {info.Name}. Choose Preserve original to copy it into LifeOS.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "read the selected file"));
        }
    }

    private void Preserve()
    {
        DocumentCaptureDraft draft = new(
            _selectedFileName, _selectedMediaType, _selectedBytes,
            _type.SelectedItem is DocumentType type ? type : DocumentType.GeneralEvidence);
        FormValidationResult validation = DocumentCaptureService.Validate(draft);
        string errors = string.Join(" ", validation.ForField("document-file").Select(issue => issue.Message));
        _fileError.Text = errors;
        _fileError.Visibility = string.IsNullOrEmpty(errors) ? Visibility.Collapsed : Visibility.Visible;
        if (!validation.IsValid)
        {
            ShowProblem(new UserFacingProblem(
                "document-capture-validation-failed", "Choose a valid local file",
                "No original was preserved because the file selection is invalid.",
                "Choose a non-empty file no larger than 25 MB, then try again.", true));
            return;
        }
        try
        {
            DocumentRecord record = DocumentCaptureService.Create(draft, DateTimeOffset.UtcNow);
            List<DocumentRecord> candidate = [.. _records, record];
            DocumentIntakeStorage.Save(candidate);
            _records = candidate;
            _selectedFileName = null; _selectedMediaType = null; _selectedBytes = null;
            _notice = $"Preserved {record.Original.FileName} as a local draft.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "preserve the document"));
        }
    }

    private UIElement DocumentCard(DocumentRecord record)
    {
        StackPanel body = new();
        DockPanel title = new();
        TextBlock state = Text(record.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold);
        DockPanel.SetDock(state, Dock.Right); title.Children.Add(state); title.Children.Add(Heading(record.Original.FileName, 18));
        body.Children.Add(title);
        body.Children.Add(Text(
            $"{record.Type} • {record.Original.MediaType} • {record.Original.SizeBytes:N0} bytes\nSHA-256 {record.Original.Sha256}\nImported {record.Original.ImportedUtc:u} • trusted original {record.HasTrustedOriginal}",
            12, "#A9B6CA"));
        WrapPanel actions = new() { Margin = new Thickness(0, 8, 0, 0) };
        if (record.State == DocumentIntakeState.Draft)
            actions.Children.Add(StateButton("Move to review", record, "Review"));
        if (record.State is DocumentIntakeState.Draft or DocumentIntakeState.ReviewRequired or DocumentIntakeState.Deferred)
        {
            actions.Children.Add(StateButton("Accept", record, "Accept"));
            actions.Children.Add(StateButton("Defer", record, "Defer"));
            actions.Children.Add(StateButton("Reject", record, "Reject"));
        }
        body.Children.Add(actions);
        return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private Button StateButton(string label, DocumentRecord record, string action)
    {
        Button button = ActionButton(label, $"Documents.{action}.{record.Id}", true);
        button.Click += (_, _) => ChangeState(record, action);
        return button;
    }

    private void ChangeState(DocumentRecord record, string action)
    {
        try
        {
            DocumentRecord changed = action switch
            {
                "Review" => _service.MoveToReview(record),
                "Accept" => _service.Accept(record, record.Type, record.Metadata, record.Links),
                "Defer" => _service.Defer(record),
                "Reject" => _service.Reject(record),
                _ => throw new ArgumentOutOfRangeException(nameof(action))
            };
            List<DocumentRecord> candidate = _records.Select(item => item.Id == record.Id ? changed : item).ToList();
            DocumentIntakeStorage.Save(candidate);
            _records = candidate;
            _notice = $"{record.Original.FileName} is now {changed.State.ToString().ToLowerInvariant()}.";
            _problem = null;
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            ShowProblem(UserFacingProblemFactory.FromException(exception, "update the document"));
        }
    }

    private IReadOnlyList<DuplicateDocumentCandidate> DuplicateCandidates()
    {
        List<DuplicateDocumentCandidate> duplicates = [];
        for (int index = 0; index < _records.Count; index++)
            duplicates.AddRange(_service.FindExactDuplicates(_records[index], _records.Take(index)));
        return duplicates;
    }

    private static string MediaType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf", ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg", ".txt" => "text/plain",
        ".csv" => "text/csv",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _ => "application/octet-stream"
    };

    private Border BuildProblemPanel()
    {
        _problemTitle = Heading(string.Empty, 16); _problemDetail = Text(string.Empty, 12, "#E1E7F0");
        _problemRecovery = Text(string.Empty, 12, "#C5AECF");
        StackPanel body = new(); body.Children.Add(_problemTitle); body.Children.Add(_problemDetail); body.Children.Add(_problemRecovery);
        _problemPanel = new Border
        {
            Visibility = Visibility.Collapsed, Background = Brush("#251925"), BorderBrush = Brush("#C95F75"),
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(15),
            Margin = new Thickness(0, 12, 0, 0), Child = body
        };
        AutomationProperties.SetAutomationId(_problemPanel, "Documents.Problem");
        if (_problem is not null) ShowProblem(_problem);
        return _problemPanel;
    }

    private void ShowProblem(UserFacingProblem problem)
    {
        _problem = problem; _problemTitle.Text = $"{problem.Title} ({problem.Code})";
        _problemDetail.Text = problem.Detail; _problemRecovery.Text = $"Next: {problem.RecoveryAction}";
        AutomationProperties.SetName(_problemPanel, $"{problem.Title}. {problem.Detail} Next: {problem.RecoveryAction}");
        _problemPanel.Visibility = Visibility.Visible;
    }

    private static Button ActionButton(string label, string automationId, bool secondary)
    {
        Button button = new()
        {
            Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White,
            BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7),
            Margin = new Thickness(0, 8, 8, 0), MinHeight = 36
        };
        AutomationProperties.SetAutomationId(button, automationId); return button;
    }

    private static Border Metric(string label, string value, string detail)
    {
        StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0"));
        content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0"));
        return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content };
    }
    private static Border Card(string title, string body, string background)
    {
        StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC"));
        return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content };
    }
    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}

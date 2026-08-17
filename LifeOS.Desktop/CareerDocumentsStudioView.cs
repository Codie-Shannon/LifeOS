using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using LifeOS.Core.CareerStudio;
using Microsoft.Win32;

namespace LifeOS.Desktop;

public sealed class CareerDocumentsStudioView : UserControl
{
    private static readonly DateTimeOffset ProofNow =
        new(2026, 7, 26, 15, 0, 0, TimeSpan.FromHours(12));

    private readonly CareerDocumentBuilderService _service = new();
    private readonly CareerDocumentLayoutService _layoutService = new();
    private readonly IReadOnlyList<CareerFact> _facts;
    private readonly List<CvBuilderDocument> _documents;
    private readonly Action _close;
    private readonly bool _portfolioDemo;
    private readonly string _libraryPath;
    private string _activeDocumentId;
    private string _expandedSectionId = "contact";
    private bool _showOptionalSections;
    private ScrollViewer? _editorScrollViewer;
    private double _editorScrollOffset;
    private readonly Stack<CvBuilderDocument> _undoHistory = new();
    private readonly Stack<CvBuilderDocument> _redoHistory = new();
    private double _previewZoom = 0.9;
    private bool _previewOnly;
    private string? _importNotice;
    private bool _hasUnsavedChanges;
    private TextBlock? _saveStatusText;
    private bool _compactLayout;
    private bool _showDesignStudio = true;
    private readonly List<CvVersionSnapshot> _versionHistory = [];
    private readonly List<CvBuilderDocument> _savedVersions = [];
    private readonly List<CvStoredVersion> _storedVersions = [];
    private string? _exportNotice;

    private CvBuilderDocument Active =>
        _documents.Single(document => document.Id == _activeDocumentId);

    public CareerDocumentsStudioView(Action? close = null)
        : this(false, close)
    {
    }

    public CareerDocumentsStudioView(
        bool portfolioDemo,
        Action? close = null,
        string? libraryPath = null)
    {
        _portfolioDemo = portfolioDemo;
        _close = close ?? (() => { });
        _libraryPath = string.IsNullOrWhiteSpace(libraryPath)
            ? CareerDocumentLibraryStore.DefaultFilePath
            : Path.GetFullPath(libraryPath);

        if (portfolioDemo)
        {
            CareerMaterialsProof materials = CareerMaterialsProofData.Build(ProofNow);
            _facts = materials.Facts;
            CvBuilderWorkspace workspace = CareerDocumentBuilderProofData.Build(ProofNow);
            _documents = workspace.Documents.ToList();
            _activeDocumentId = workspace.ActiveDocumentId;
            _versionHistory.Add(_layoutService.CreateSnapshot(Active, "CV foundation"));
            _savedVersions.Add(Active);
        }
        else
        {
            _facts = [];
            CareerDocumentLibrary library = CareerDocumentLibraryStore.Load(_libraryPath);
            _documents = library.Documents.ToList();
            if (_documents.Count == 0)
            {
                _documents.Add(_service.CreateBlank(
                    $"cv-{Guid.NewGuid():N}",
                    "Untitled CV",
                    DateTimeOffset.Now));
            }

            _activeDocumentId = _documents.Any(document => document.Id == library.ActiveDocumentId)
                ? library.ActiveDocumentId
                : _documents[0].Id;
            _storedVersions.AddRange(library.Versions);
            CvStoredVersion[] history = library.Versions
                .Where(version => version.DocumentId == _activeDocumentId)
                .OrderBy(version => version.Snapshot.SavedUtc)
                .ToArray();
            _versionHistory.AddRange(history.Select(version => version.Snapshot));
            _savedVersions.AddRange(history.Select(version => version.Document));
        }
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        PreviewKeyDown += HandleKeyboardShortcut;
        SizeChanged += (_, e) =>
        {
            bool compact = e.NewSize.Width < 1180;
            if (compact == _compactLayout || _hasUnsavedChanges)
                return;

            _compactLayout = compact;
            Render();
        };
        Render();
    }

    private void HandleKeyboardShortcut(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (e.Key == Key.Z)
            {
                Undo();
                e.Handled = true;
            }
            else if (e.Key == Key.Y)
            {
                Redo();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape && _previewOnly)
        {
            _previewOnly = false;
            Render();
            e.Handled = true;
        }
    }

    private void Render()
    {
        if (ActualWidth > 0)
            _compactLayout = ActualWidth < 1180;
        if (_editorScrollViewer is not null)
            _editorScrollOffset = _editorScrollViewer.VerticalOffset;

        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.Children.Add(BuildTopBar());

        Grid workspace = new() { Background = Brush("#0C1220") };
        bool showEditor = !_previewOnly;
        bool showPreview = _previewOnly || !_compactLayout;
        workspace.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = showEditor
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0),
            MinWidth = showEditor ? 340 : 0
        });
        workspace.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = showEditor && showPreview
                ? new GridLength(6)
                : new GridLength(0)
        });
        workspace.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = showPreview
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0),
            MinWidth = showPreview ? 340 : 0
        });

        UIElement editor = BuildContinuousEditor();
        workspace.Children.Add(editor);

        GridSplitter splitter = new()
        {
            Width = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Brush("#27334A"),
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            Cursor = Cursors.SizeWE
        };
        Grid.SetColumn(splitter, 1);
        workspace.Children.Add(splitter);

        UIElement preview = BuildPreviewPane();
        Grid.SetColumn(preview, 2);
        workspace.Children.Add(preview);

        Grid.SetRow(workspace, 1);
        root.Children.Add(workspace);
        Content = root;
    }

    private UIElement BuildTopBar()
    {
        Grid bar = new()
        {
            Height = 66,
            Background = Brush("#101522")
        };
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Button back = SymbolTextButton("\uE72B", "CVs", TryClose);
        back.Margin = new Thickness(14, 12, 0, 12);
        bar.Children.Add(back);

        TextBox documentName = new()
        {
            Text = Active.Name,
            Width = 290,
            Height = 38,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brush("#28272C"),
            Foreground = Brushes.White,
            BorderBrush = Brush("#44424A"),
            Padding = new Thickness(9),
            TextAlignment = TextAlignment.Center
        };
        documentName.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(documentName.Text))
            {
                ReplaceActive(
                    _service.RenameDocument(
                        Active,
                        documentName.Text,
                        DateTimeOffset.Now),
                    isExplicitSave: true);
                e.Handled = true;
            }
        };
        documentName.TextChanged += (_, _) => MarkEditing();
        documentName.ToolTip = "Document name. Press Enter to save.";
        Grid.SetColumn(documentName, 1);
        bar.Children.Add(documentName);

        StackPanel actions = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 11, 14, 11)
        };
        actions.Children.Add(BuildStatusPill());
        Button undo = SymbolButton("\uE7A7", "Undo last change", Undo);
        undo.IsEnabled = _undoHistory.Count > 0;
        actions.Children.Add(undo);
        Button redo = SymbolButton("\uE7A6", "Redo last change", Redo);
        redo.IsEnabled = _redoHistory.Count > 0;
        actions.Children.Add(redo);
        actions.Children.Add(SymbolTextButton(
            _previewOnly ? "\uE8A7" : "\uE740",
            _previewOnly ? "Edit" : "Preview",
            () => RunWhenSaved(() =>
            {
                _previewOnly = !_previewOnly;
                Render();
            })));
        Grid.SetColumn(actions, 2);
        bar.Children.Add(actions);

        return bar;
    }

    private void TryClose()
    {
        if (_hasUnsavedChanges)
        {
            MessageBoxResult result = MessageBox.Show(
                "This CV contains edits that have not been saved. Leave the builder and discard them?",
                "Unsaved CV changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        _close();
    }

    private void MarkEditing()
    {
        _hasUnsavedChanges = true;
        if (_saveStatusText is not null)
            _saveStatusText.Text = "Editing…";
    }

    private UIElement BuildContinuousEditor()
    {
        StackPanel form = new() { Margin = new Thickness(30, 24, 24, 40) };
        form.Children.Add(BuildDesignStudio());

        Grid import = new();
        import.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        import.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        import.Children.Add(ImportTile(
            "\uE898",
            "Upload existing CV",
            "Import is preview-only until reviewed.",
            SelectExistingCv));
        Button profileImport = ImportTile(
            "\uE77B",
            "Import trusted LifeOS profile",
            $"{_facts.Count(fact => fact.IsTrusted)} accepted facts available",
            () => RunWhenSaved(() =>
            {
                _importNotice =
                    $"{_facts.Count(fact => fact.IsTrusted)} trusted career facts are linked to this CV.";
                Render();
            }));
        Grid.SetColumn(profileImport, 1);
        import.Children.Add(profileImport);
        form.Children.Add(import);
        if (!string.IsNullOrWhiteSpace(_importNotice))
        {
            form.Children.Add(new Border
            {
                Background = Brush("#172941"),
                BorderBrush = Brush("#365A82"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 14),
                Child = Body(_importNotice, "#BFD7F2", FontWeights.SemiBold)
            });
        }

        foreach (CvBuilderSection section in Active.Sections
                     .Where(section => section.IsEnabled)
                     .OrderBy(section => section.Order))
        {
            form.Children.Add(BuildSectionAccordion(section));
        }

        Button optional = OutlineButton(
            _showOptionalSections ? "Hide optional sections" : "+ Add optional section",
            () => RunWhenSaved(() =>
            {
                _showOptionalSections = !_showOptionalSections;
                Render();
            }));
        optional.Margin = new Thickness(0, 8, 0, 14);
        form.Children.Add(optional);

        if (_showOptionalSections)
        {
            WrapPanel options = new() { Margin = new Thickness(0, 0, 0, 18) };
            foreach (CvBuilderSection section in Active.Sections.Where(section => !section.IsEnabled))
            {
                options.Children.Add(Chip($"+ {section.Heading}", () =>
                    ReplaceActive(_service.SetSectionEnabled(
                        Active,
                        section.Id,
                        true,
                        DateTimeOffset.Now),
                        section.Id)));
            }

            foreach (string label in new[]
                     {
                         "Languages", "Courses", "Internships", "References",
                         "Qualities", "Achievements"
                     })
            {
                options.Children.Add(Chip($"+ {label}", () =>
                {
                    CvBuilderDocument updated = _service.AddCustomSection(
                        Active,
                        label,
                        DateTimeOffset.Now);
                    CvBuilderSection added = updated.Sections
                        .OrderByDescending(section => section.Order)
                        .First();
                    ReplaceActive(updated, added.Id);
                }));
            }
            options.Children.Add(Chip("+ Custom section", () =>
            {
                CvBuilderDocument updated = _service.AddCustomSection(
                    Active,
                    "Custom section",
                    DateTimeOffset.Now);
                CvBuilderSection added = updated.Sections
                    .OrderByDescending(section => section.Order)
                    .First();
                ReplaceActive(updated, added.Id);
            }));
            form.Children.Add(options);
        }

        CvBuilderReview review = _service.Review(Active, _facts);
        form.Children.Add(new Border
        {
            Background = Brush(review.CanExport ? "#E4F5ED" : "#FFF1DF"),
            BorderBrush = Brush(review.CanExport ? "#82C9A9" : "#E0A85F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = Body(
                review.CanExport
                    ? $"Ready for layout · {review.TrustedFactCount}/{review.TotalSourceFactCount} trusted sources"
                    : $"{review.Issues.Count} blocking item(s) must be resolved",
                review.CanExport ? "#25664E" : "#8A5522",
                FontWeights.SemiBold)
        });

        ScrollViewer scrollViewer = new()
        {
            Content = form,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        scrollViewer.Loaded += (_, _) =>
            scrollViewer.ScrollToVerticalOffset(_editorScrollOffset);
        _editorScrollViewer = scrollViewer;

        return new Border
        {
            Background = Brush("#0F1626"),
            BorderBrush = Brush("#28354D"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = scrollViewer
        };
    }

    private UIElement BuildDesignStudio()
    {
        StackPanel panel = new();
        Grid heading = new();
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        StackPanel copy = new();
        copy.Children.Add(Title("Template, layout & export", 20));
        copy.Children.Add(Body(
            "SG-82 · A4 preview, ATS/readability review and safe versioned derivatives",
            "#9FB0CB",
            FontWeights.Normal));
        heading.Children.Add(copy);
        Button toggle = SymbolButton(
            _showDesignStudio ? "\uE70E" : "\uE70D",
            _showDesignStudio ? "Collapse design studio" : "Expand design studio",
            () =>
            {
                _showDesignStudio = !_showDesignStudio;
                Render();
            });
        Grid.SetColumn(toggle, 1);
        heading.Children.Add(toggle);
        panel.Children.Add(heading);

        if (_showDesignStudio)
        {
            panel.Children.Add(SectionLabel("Professional templates"));
            UniformGrid gallery = new() { Columns = 2, Margin = new Thickness(0, 6, 0, 14) };
            foreach (CvTemplateDefinition template in _layoutService.GetTemplates())
            {
                bool selected = template.Id == Active.TemplateId;
                StackPanel tileContent = new();
                tileContent.Children.Add(new Border
                {
                    Width = 36,
                    Height = 5,
                    Background = Brush(template.Layout.AccentHex),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                tileContent.Children.Add(Title(
                    selected ? $"{template.Name}  ✓" : template.Name,
                    14));
                tileContent.Children.Add(Body(
                    template.Description,
                    "#9FB0CB",
                    FontWeights.Normal));
                Button tile = ButtonBase(string.Empty, selected ? "#20304B" : "#141D30", "#FFFFFF");
                tile.Content = tileContent;
                tile.BorderBrush = Brush(selected ? template.Layout.AccentHex : "#34445F");
                tile.BorderThickness = new Thickness(selected ? 2 : 1);
                tile.Padding = new Thickness(12);
                tile.Margin = new Thickness(0, 0, 10, 10);
                AutomationProperties.SetName(tile, $"Use {template.Name} template");
                tile.Click += (_, _) => RunWhenSaved(() =>
                    ReplaceActive(
                        _layoutService.ApplyTemplate(Active, template.Id, DateTimeOffset.Now),
                        isExplicitSave: true));
                gallery.Children.Add(tile);
            }
            panel.Children.Add(gallery);

            CvDocumentLayout layout = Active.EffectiveLayout;
            Grid controls = new();
            controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            controls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            ComboBox density = Select(
                Enum.GetValues<CvPageDensity>().Select(value => value.ToString()),
                layout.Density.ToString());
            density.SelectionChanged += (_, _) =>
            {
                if (density.SelectedItem is not string selected ||
                    !Enum.TryParse(selected, out CvPageDensity value))
                    return;
                ApplyLayout(layout with { Density = value });
            };
            controls.Children.Add(Field("Page density", density));

            ComboBox typography = Select(
                new[] { "Aptos", "Arial", "Segoe UI", "Calibri" },
                layout.FontFamily);
            typography.SelectionChanged += (_, _) =>
            {
                if (typography.SelectedItem is string selected)
                    ApplyLayout(layout with { FontFamily = selected });
            };
            Border typographyField = Field("Typography", typography);
            Grid.SetColumn(typographyField, 1);
            controls.Children.Add(typographyField);

            ComboBox margin = Select(
                new[] { "15 mm", "18 mm", "20 mm", "22 mm" },
                $"{layout.PageMarginMillimetres:0} mm");
            margin.SelectionChanged += (_, _) =>
            {
                if (margin.SelectedItem is string selected &&
                    double.TryParse(selected.Split(' ')[0], out double value))
                {
                    ApplyLayout(layout with { PageMarginMillimetres = value });
                }
            };
            Border marginField = Field("A4 margins", margin);
            Grid.SetColumn(marginField, 2);
            controls.Children.Add(marginField);
            panel.Children.Add(controls);

            WrapPanel accents = new() { Margin = new Thickness(0, 2, 0, 14) };
            accents.Children.Add(Body("Accent", "#C8D3E6", FontWeights.SemiBold));
            foreach (string accent in new[] { "#315E91", "#6C4EE3", "#176B65", "#8B3D5C" })
            {
                Button swatch = new()
                {
                    Width = 32,
                    Height = 32,
                    Background = Brush(accent),
                    BorderBrush = Brush(accent == layout.AccentHex ? "#FFFFFF" : "#56657D"),
                    BorderThickness = new Thickness(accent == layout.AccentHex ? 3 : 1),
                    Margin = new Thickness(10, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    ToolTip = $"Use accent {accent}"
                };
                swatch.Click += (_, _) => ApplyLayout(layout with { AccentHex = accent });
                accents.Children.Add(swatch);
            }
            panel.Children.Add(accents);

            CvReadabilityReview readability = _layoutService.Review(Active);
            Border review = new()
            {
                Background = Brush(readability.CanExport ? "#132C29" : "#342419"),
                BorderBrush = Brush(readability.CanExport ? "#2C796D" : "#A56833"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
            };
            StackPanel reviewContent = new();
            reviewContent.Children.Add(Title(
                $"ATS & readability · {readability.Score}/100",
                16));
            reviewContent.Children.Add(Body(
                $"{readability.Checks.Count(check => check.Passed)}/{readability.Checks.Count} checks passed · estimated {readability.EstimatedPages} A4 page(s)",
                readability.CanExport ? "#8ED8C3" : "#F2B77F",
                FontWeights.SemiBold));
            WrapPanel checks = new() { Margin = new Thickness(0, 8, 0, 0) };
            foreach (CvReadabilityCheck check in readability.Checks)
            {
                checks.Children.Add(new Border
                {
                    Background = Brush(check.Passed ? "#1B4A42" : "#4A2D1B"),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(9, 4, 9, 4),
                    Margin = new Thickness(0, 0, 6, 6),
                    ToolTip = check.Detail,
                    Child = Body(
                        $"{(check.Passed ? "✓" : "!")} {check.Label}",
                        check.Passed ? "#B8F0DE" : "#FFD1A8",
                        FontWeights.SemiBold)
                });
            }
            reviewContent.Children.Add(checks);
            review.Child = reviewContent;
            panel.Children.Add(review);

            Grid footer = new();
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel history = new();
            history.Children.Add(Title("Version history", 14));
            history.Children.Add(Body(
                _versionHistory.Count == 0
                    ? "No saved layout versions"
                    : string.Join(
                        "  ·  ",
                        _versionHistory.TakeLast(3).Select(snapshot =>
                            $"v{snapshot.Version} {snapshot.Label}")),
                "#9FB0CB",
                FontWeights.Normal));
            footer.Children.Add(history);
            WrapPanel exportActions = new();
            exportActions.Children.Add(OutlineButton("Save version", SaveVersionSnapshot));
            if (_savedVersions.Count > 1)
            {
                Button restore = OutlineButton("Restore previous", RestorePreviousVersion);
                restore.Margin = new Thickness(8, 0, 0, 0);
                exportActions.Children.Add(restore);
            }
            Button pdf = PrimaryButton("Export PDF", () => ExportDocument(CvExportFormat.Pdf));
            pdf.Margin = new Thickness(8, 0, 0, 0);
            exportActions.Children.Add(pdf);
            Button docx = OutlineButton("Export DOCX", () => ExportDocument(CvExportFormat.Docx));
            docx.Margin = new Thickness(8, 0, 0, 0);
            exportActions.Children.Add(docx);
            Grid.SetColumn(exportActions, 1);
            footer.Children.Add(exportActions);
            panel.Children.Add(footer);

            if (!string.IsNullOrWhiteSpace(_exportNotice))
            {
                panel.Children.Add(new Border
                {
                    Background = Brush("#172941"),
                    BorderBrush = Brush("#365A82"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 12, 0, 0),
                    Child = Body(_exportNotice, "#BFD7F2", FontWeights.SemiBold)
                });
            }
        }

        return new Border
        {
            Background = Brush("#111C30"),
            BorderBrush = Brush("#405276"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 18),
            Child = panel
        };
    }

    private void ApplyLayout(CvDocumentLayout layout)
    {
        if (!EnsureSavedBeforeStructureChange())
            return;
        ReplaceActive(
            _layoutService.UpdateLayout(Active, layout, DateTimeOffset.Now),
            isExplicitSave: true);
    }

    private void SaveVersionSnapshot()
    {
        if (!EnsureSavedBeforeStructureChange())
            return;
        _versionHistory.Add(_layoutService.CreateSnapshot(
            Active,
            _layoutService.GetTemplate(Active.TemplateId).Name));
        _savedVersions.Add(Active);
        PersistLibrary();
        _exportNotice = $"Version v{Active.Version} added to local history.";
        Render();
    }

    private void RestorePreviousVersion()
    {
        if (!EnsureSavedBeforeStructureChange() || _savedVersions.Count < 2)
            return;

        CvBuilderDocument previous = _savedVersions[^2] with
        {
            Version = Active.Version + 1,
            UpdatedUtc = DateTimeOffset.Now,
            IsAutosaved = true
        };
        _savedVersions.Add(previous);
        _versionHistory.Add(_layoutService.CreateSnapshot(previous, "Restored version"));
        _exportNotice =
            $"Restored the prior saved layout as new version v{previous.Version}; history was preserved.";
        ReplaceActive(previous, isExplicitSave: true);
    }

    private void ExportDocument(CvExportFormat format)
    {
        if (!EnsureSavedBeforeStructureChange())
            return;

        try
        {
            CvBuilderReview sourceReview = _service.Review(Active, _facts);
            CvExportArtifact artifact = _layoutService.Export(
                Active,
                sourceReview,
                format,
                DateTimeOffset.Now);
            SaveFileDialog dialog = new()
            {
                Title = $"Export reviewed CV as {format.ToString().ToUpperInvariant()}",
                FileName = artifact.SuggestedFileName,
                Filter = format == CvExportFormat.Pdf
                    ? "PDF document (*.pdf)|*.pdf"
                    : "Word document (*.docx)|*.docx",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
                return;

            File.WriteAllBytes(dialog.FileName, artifact.Content);
            _versionHistory.Add(_layoutService.CreateSnapshot(Active, $"{format} export"));
            _savedVersions.Add(Active);
            PersistLibrary();
            _exportNotice =
                $"{format.ToString().ToUpperInvariant()} derivative saved from v{artifact.SourceVersion}. Authoritative Career records were not changed.";
            Render();
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                exception.Message,
                "Export blocked",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private static TextBlock SectionLabel(string text) =>
        Body(text, "#D7E2F4", FontWeights.Bold);

    private UIElement BuildSectionAccordion(CvBuilderSection section)
    {
        bool expanded = section.Id == _expandedSectionId;
        StackPanel card = new();

        Grid heading = new();
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        StackPanel headingText = new();
        headingText.Children.Add(Title(section.Heading, 18));
        headingText.Children.Add(Body(
            string.IsNullOrWhiteSpace(section.Content)
                ? "Needs content"
                : section.SourceFactIds.Count > 0
                    ? "Source-backed"
                    : "Ready for review",
            string.IsNullOrWhiteSpace(section.Content) ? "#E8B36E" : "#8FB7A5",
            FontWeights.SemiBold));
        heading.Children.Add(headingText);

        StackPanel tools = new() { Orientation = Orientation.Horizontal };
        tools.Children.Add(SymbolButton(expanded ? "\uE70E" : "\uE70D", expanded ? "Collapse section" : "Expand section", () =>
        {
            RunWhenSaved(() =>
            {
                _expandedSectionId = expanded ? string.Empty : section.Id;
                Render();
            });
        }));
        if (section.Kind is CvSectionKind.Custom or CvSectionKind.Education or CvSectionKind.Certifications)
        {
            tools.Children.Add(SymbolButton("\uE74D", "Remove section", () =>
            {
                _expandedSectionId = string.Empty;
                ReplaceActive(_service.RemoveSection(
                    Active,
                    section.Id,
                DateTimeOffset.Now));
            }));
        }
        Grid.SetColumn(tools, 2);
        heading.Children.Add(tools);

        Border dragHandle = new()
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(12, 8, 12, 8),
            Cursor = Cursors.SizeAll,
            ToolTip = "Drag to reorder section",
            Child = new TextBlock
            {
                Text = "\uE700",
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 16,
                Foreground = Brush("#9FB0CC"),
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        AutomationProperties.SetName(dragHandle, $"Reorder {section.Heading}");
        dragHandle.PreviewMouseMove += (_, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_hasUnsavedChanges)
                DragDrop.DoDragDrop(dragHandle, section.Id, DragDropEffects.Move);
        };
        Grid.SetColumn(dragHandle, 1);
        heading.Children.Add(dragHandle);
        card.Children.Add(heading);

        if (expanded)
        {
            if (section.Kind == CvSectionKind.Contact)
                BuildPersonalDetails(card, section);
            else
                BuildSectionForm(card, section);
        }

        Border cardBorder = new()
        {
            Background = Brush(expanded ? "#121C30" : "#0F1626"),
            BorderBrush = Brush("#2A3852"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(14),
            Child = card,
            Tag = section.Id
        };
        cardBorder.AllowDrop = true;
        cardBorder.DragOver += (_, e) =>
        {
            if (e.Data.GetData(DataFormats.StringFormat) is not string sourceSectionId ||
                sourceSectionId == section.Id)
            {
                return;
            }

            bool insertAfter = ShouldInsertAfter(sourceSectionId, section.Id);
            cardBorder.BorderBrush = Brush("#8A6CFF");
            cardBorder.BorderThickness = insertAfter
                ? new Thickness(1, 1, 1, 4)
                : new Thickness(1, 4, 1, 1);

            AutoScrollDuringDrag(e);
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        };
        cardBorder.DragLeave += (_, _) =>
        {
            cardBorder.BorderBrush = Brush("#2A3852");
            cardBorder.BorderThickness = new Thickness(1);
        };
        cardBorder.Drop += (_, e) =>
        {
            if (e.Data.GetData(DataFormats.StringFormat) is not string sourceSectionId ||
                sourceSectionId == section.Id)
            {
                return;
            }

            bool insertAfter = ShouldInsertAfter(sourceSectionId, section.Id);
            CvBuilderDocument reordered = insertAfter
                ? _service.MoveSectionAfter(
                    Active,
                    sourceSectionId,
                    section.Id,
                    DateTimeOffset.Now)
                : _service.MoveSectionBefore(
                    Active,
                    sourceSectionId,
                    section.Id,
                    DateTimeOffset.Now);
            ReplaceActive(reordered);
            e.Handled = true;
        };
        return cardBorder;
    }

    private bool ShouldInsertAfter(string sourceSectionId, string targetSectionId)
    {
        CvBuilderSection source = Active.Sections.Single(section =>
            section.Id == sourceSectionId);
        CvBuilderSection target = Active.Sections.Single(section =>
            section.Id == targetSectionId);
        return source.Order < target.Order;
    }

    private void AutoScrollDuringDrag(DragEventArgs e)
    {
        if (_editorScrollViewer is null)
            return;

        double y = e.GetPosition(_editorScrollViewer).Y;
        const double edge = 72;
        const double step = 28;
        if (y < edge)
        {
            _editorScrollViewer.ScrollToVerticalOffset(
                Math.Max(0, _editorScrollViewer.VerticalOffset - step));
        }
        else if (y > _editorScrollViewer.ViewportHeight - edge)
        {
            _editorScrollViewer.ScrollToVerticalOffset(
                _editorScrollViewer.VerticalOffset + step);
        }
    }

    private void BuildPersonalDetails(StackPanel panel, CvBuilderSection section)
    {
        Grid names = TwoColumns();
        names.Children.Add(Field("Given name", Input("Codie")));
        Border family = Field("Family name", Input("Shannon"));
        Grid.SetColumn(family, 1);
        names.Children.Add(family);
        panel.Children.Add(names);

        TextBox role = Input(Active.TargetRole);
        panel.Children.Add(Field("Desired job position", role));

        Grid contacts = TwoColumns();
        contacts.Children.Add(Field("Email address", Input("Private until export")));
        Border phone = Field("Phone number", Input("Private until export"));
        Grid.SetColumn(phone, 1);
        contacts.Children.Add(phone);
        panel.Children.Add(contacts);

        panel.Children.Add(Body(
            "Email, phone and additional links remain private until explicitly included for export.",
            "#93A4C3",
            FontWeights.Normal));

        Button save = PrimaryButton("Save personal details", () =>
        {
            CvBuilderDocument updated = _service.SetTargetRole(Active, role.Text, DateTimeOffset.Now);
            updated = _service.UpdateSection(
                updated,
                section.Id,
                section.Heading,
                section.Content,
                DateTimeOffset.Now);
            ReplaceActive(updated, section.Id, isExplicitSave: true);
        });
        panel.Children.Add(save);
    }

    private void BuildSectionForm(StackPanel panel, CvBuilderSection section)
    {
        TextBox heading = Input(section.Heading);
        panel.Children.Add(Field("Section title", heading));

        if (section.Kind is CvSectionKind.Employment or CvSectionKind.Education)
        {
            foreach (CvBuilderEntry entry in section.Entries ?? [])
                panel.Children.Add(BuildStructuredEntry(section, entry, heading));

            string entryLabel = section.Kind == CvSectionKind.Employment
                ? "employment"
                : "education";
            panel.Children.Add(OutlineButton(
                $"+ Add {entryLabel}",
                () =>
                {
                    CvBuilderDocument titled = _service.UpdateSection(
                        Active,
                        section.Id,
                        heading.Text,
                        section.Content,
                        DateTimeOffset.Now);
                    ReplaceActive(
                        _service.AddEntry(
                            titled,
                            section.Id,
                            DateTimeOffset.Now),
                        section.Id,
                        isExplicitSave: true);
                }));
            return;
        }

        TextBox? subtitle = null;
        LifeOsDateSelector? startDate = null;
        LifeOsDateSelector? endDate = null;
        UIElement? customModules = null;
        UIElement? customDates = null;
        UIElement? customSubtitle = null;

        if (section.Kind == CvSectionKind.Custom)
        {
            if (section.ShowSubtitle)
            {
                subtitle = Input(section.Subtitle);
                subtitle.ToolTip = "Optional line displayed below the section heading.";
                customSubtitle = Field("Subtitle", subtitle);
            }

            WrapPanel modules = new() { Margin = new Thickness(0, 0, 0, 10) };
            modules.Children.Add(Chip(
                section.ShowSubtitle ? "− Subtitle" : "+ Subtitle",
                () => ReplaceActive(
                    _service.SetSectionModules(
                        Active,
                        section.Id,
                        !section.ShowSubtitle,
                        section.ShowDateRange,
                        DateTimeOffset.Now),
                    section.Id)));
            modules.Children.Add(Chip(
                section.ShowDateRange ? "− Date range" : "+ Date range",
                () => ReplaceActive(
                    _service.SetSectionModules(
                        Active,
                        section.Id,
                        section.ShowSubtitle,
                        !section.ShowDateRange,
                        DateTimeOffset.Now),
                    section.Id)));
            customModules = modules;

            if (section.ShowDateRange)
            {
                Grid dateGrid = TwoColumns();
                startDate = CalendarInput(section.StartDate);
                endDate = CalendarInput(section.EndDate);
                dateGrid.Children.Add(Field("Start date", startDate));
                Border customEnd = Field("End date", endDate);
                Grid.SetColumn(customEnd, 1);
                dateGrid.Children.Add(customEnd);
                customDates = dateGrid;
            }
        }

        if (customSubtitle is not null)
            panel.Children.Add(customSubtitle);
        if (customDates is not null)
            panel.Children.Add(customDates);

        RichTextBox content = RichDescription(section.Content, section.RichContent);
        panel.Children.Add(Field("Description", content));
        panel.Children.Add(BuildFormattingToolbar(content));
        if (customModules is not null)
            panel.Children.Add(customModules);
        panel.Children.Add(SourceNotice(section));

        StackPanel actions = new() { Orientation = Orientation.Horizontal };
        actions.Children.Add(PrimaryButton("Done", () =>
            ReplaceActive(_service.UpdateSectionDetails(
                Active,
                section.Id,
                heading.Text,
                ReadRichText(content),
                subtitle?.Text ?? section.Subtitle,
                startDate?.SelectedDate,
                endDate?.SelectedDate,
                section.ShowDateRange ||
                    section.Kind is CvSectionKind.Employment or CvSectionKind.Education,
                WriteRichText(content),
                section.ShowSubtitle,
                DateTimeOffset.Now),
                section.Id,
                isExplicitSave: true)));
        actions.Children.Add(OutlineButton("+ Add detail", () =>
        {
            Paragraph paragraph = new(new Run(string.Empty));
            content.Document.Blocks.Add(paragraph);
            content.CaretPosition = paragraph.ContentStart;
            content.Focus();
        }));
        panel.Children.Add(actions);
    }

    private UIElement BuildStructuredEntry(
        CvBuilderSection section,
        CvBuilderEntry entry,
        TextBox sectionHeading)
    {
        StackPanel form = new();
        Grid titleRow = TwoColumns();
        TextBox title = Input(entry.Title);
        TextBox organization = Input(entry.Organization);
        titleRow.Children.Add(Field(
            section.Kind == CvSectionKind.Employment ? "Job title" : "Education",
            title));
        Border organizationField = Field(
            section.Kind == CvSectionKind.Employment ? "Employer" : "Institution",
            organization);
        Grid.SetColumn(organizationField, 1);
        titleRow.Children.Add(organizationField);
        form.Children.Add(titleRow);

        TextBox city = Input(entry.City);
        form.Children.Add(Field("City", city));

        LifeOsDateSelector start = CalendarInput(entry.StartDate);
        LifeOsDateSelector end = CalendarInput(entry.EndDate);
        Grid dates = TwoColumns();
        dates.Children.Add(Field("Start date", start));
        Border endField = Field("End date", end);
        Grid.SetColumn(endField, 1);
        dates.Children.Add(endField);
        form.Children.Add(dates);

        CheckBox current = new()
        {
            Content = "Current position",
            IsChecked = entry.IsCurrent,
            Foreground = Brush("#E4EAF4"),
            Margin = new Thickness(0, 0, 0, 10)
        };
        current.Checked += (_, _) => end.Visibility = Visibility.Collapsed;
        current.Unchecked += (_, _) => end.Visibility = Visibility.Visible;
        current.Click += (_, _) => MarkEditing();
        end.Visibility = entry.IsCurrent ? Visibility.Collapsed : Visibility.Visible;
        form.Children.Add(current);

        RichTextBox description = RichDescription(entry.Description, entry.RichContent);
        form.Children.Add(Field("Description", description));
        form.Children.Add(BuildFormattingToolbar(description));

        StackPanel actions = new() { Orientation = Orientation.Horizontal };
        actions.Children.Add(PrimaryButton("Save entry", () =>
        {
            CvBuilderDocument updated = _service.UpdateEntry(
                    Active,
                    section.Id,
                    entry with
                    {
                        Title = title.Text.Trim(),
                        Organization = organization.Text.Trim(),
                        City = city.Text.Trim(),
                        StartDate = start.SelectedDate,
                        EndDate = current.IsChecked == true ? null : end.SelectedDate,
                        IsCurrent = current.IsChecked == true,
                        Description = ReadRichText(description),
                        RichContent = WriteRichText(description)
                    },
                    DateTimeOffset.Now);
            CvBuilderSection updatedSection = updated.Sections.Single(candidate =>
                candidate.Id == section.Id);
            updated = _service.UpdateSection(
                updated,
                section.Id,
                sectionHeading.Text,
                updatedSection.Content,
                DateTimeOffset.Now);
            ReplaceActive(
                updated,
                section.Id,
                isExplicitSave: true);
        }));
        actions.Children.Add(OutlineButton("Remove entry", () =>
            ReplaceActive(
                _service.RemoveEntry(
                    Active,
                    section.Id,
                    entry.Id,
                    DateTimeOffset.Now),
                section.Id)));
        form.Children.Add(actions);

        return new Border
        {
            Background = Brush("#0E1728"),
            BorderBrush = Brush("#34445F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 10, 0, 12),
            Child = form
        };
    }

    private UIElement BuildPreviewPane()
    {
        CvDocumentLayout layout = Active.EffectiveLayout;
        double pagePadding = layout.PageMarginMillimetres * 2.2;
        Grid pane = new() { Background = Brush("#EEF0F4") };
        pane.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        pane.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Border page = new()
        {
            Width = 560,
            MinHeight = 792,
            Background = Brushes.White,
            Margin = new Thickness(28, 18, 28, 16),
            Padding = new Thickness(pagePadding),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 18,
                ShadowDepth = 2,
                Opacity = 0.18
            }
        };
        page.LayoutTransform = new ScaleTransform(_previewZoom, _previewZoom);

        StackPanel cv = new();
        TextElement.SetFontFamily(cv, new FontFamily(layout.FontFamily));
        Border banner = new()
        {
            Background = Brush(layout.AccentHex),
            Margin = new Thickness(-pagePadding, -pagePadding, -pagePadding, 22),
            Padding = new Thickness(pagePadding, 26, pagePadding, 26),
            Child = new StackPanel
            {
                Children =
                {
                    PreviewText("CODIE SHANNON", 24, "#FFFFFF", FontWeights.Bold),
                    PreviewText(Active.TargetRole, 12, "#E8F1FA", FontWeights.SemiBold)
                }
            }
        };
        cv.Children.Add(banner);

        foreach (CvBuilderSection section in Active.VisibleSections
                     .Where(section => !string.IsNullOrWhiteSpace(section.Content)))
        {
            cv.Children.Add(PreviewText(
                section.Heading.ToUpperInvariant(),
                11 * layout.FontScale,
                layout.AccentHex,
                FontWeights.Bold,
                new Thickness(0, 10, 0, 4)));
            if (layout.ShowSectionRules)
            {
                cv.Children.Add(new Border
                {
                    Height = 1,
                    Background = Brush("#D7DCE3"),
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }
            if (!string.IsNullOrWhiteSpace(section.Subtitle))
            {
                cv.Children.Add(PreviewText(
                    section.Subtitle,
                    10.5 * layout.FontScale,
                    "#252B36",
                    FontWeights.SemiBold,
                    new Thickness(0, 0, 0, 3)));
            }
            if (section.ShowDateRange && section.StartDate.HasValue)
            {
                string range = section.EndDate.HasValue
                    ? $"{section.StartDate:MMM yyyy} – {section.EndDate:MMM yyyy}"
                    : $"{section.StartDate:MMM yyyy} – Present";
                cv.Children.Add(PreviewText(
                    range,
                    9.5 * layout.FontScale,
                    "#687181",
                    FontWeights.Normal,
                    new Thickness(0, 0, 0, 3)));
            }
            if (section.Entries is { Count: > 0 })
            {
                foreach (CvBuilderEntry entry in section.Entries)
                {
                    string heading = string.Join(
                        " · ",
                        new[] { entry.Title, entry.Organization }
                            .Where(value => !string.IsNullOrWhiteSpace(value)));
                    if (!string.IsNullOrWhiteSpace(heading))
                    {
                        cv.Children.Add(PreviewText(
                            heading,
                            10.5 * layout.FontScale,
                            "#252B36",
                            FontWeights.SemiBold,
                            new Thickness(0, 2, 0, 2)));
                    }

                    if (entry.StartDate.HasValue)
                    {
                        string range = entry.IsCurrent || !entry.EndDate.HasValue
                            ? $"{entry.StartDate:MMM yyyy} – Present"
                            : $"{entry.StartDate:MMM yyyy} – {entry.EndDate:MMM yyyy}";
                        cv.Children.Add(PreviewText(
                            range,
                            9.5 * layout.FontScale,
                            "#687181",
                            FontWeights.Normal));
                    }

                    if (!string.IsNullOrWhiteSpace(entry.Description))
                    {
                        cv.Children.Add(PreviewText(
                            entry.Description,
                            10.5 * layout.FontScale,
                            "#333944",
                            FontWeights.Normal,
                            new Thickness(0, 2, 0, 6)));
                    }
                }
            }
            else
            {
                cv.Children.Add(PreviewText(
                    section.Content,
                    10.5 * layout.FontScale,
                    "#333944",
                    FontWeights.Normal));
            }
        }
        page.Child = cv;

        ScrollViewer previewScroll = new()
        {
            Content = page,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        pane.Children.Add(previewScroll);

        WrapPanel toolbar = new()
        {
            Background = Brushes.White,
            Height = 64,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        toolbar.Children.Add(ToolbarItem("\uE71F", "Zoom out", () => RunWhenSaved(() =>
        {
            _previewZoom = Math.Max(0.65, _previewZoom - 0.1);
            Render();
        })));
        toolbar.Children.Add(new TextBlock
        {
            Text = $"{_previewZoom:P0}",
            Foreground = Brush("#3D4658"),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 4, 0)
        });
        CvReadabilityReview pagination = _layoutService.Review(Active);
        toolbar.Children.Add(new TextBlock
        {
            Text = $"A4 · Page 1 of {pagination.EstimatedPages}",
            Foreground = Brush("#687181"),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(18, 0, 4, 0)
        });
        toolbar.Children.Add(ToolbarItem("\uE710", "Zoom in", () => RunWhenSaved(() =>
        {
            _previewZoom = Math.Min(1.25, _previewZoom + 0.1);
            Render();
        })));
        toolbar.Children.Add(ToolbarItem(
            _previewOnly ? "\uE8A7" : "\uE740",
            _previewOnly ? "Return to editor" : "Fullscreen preview",
            () => RunWhenSaved(() =>
            {
                _previewOnly = !_previewOnly;
                Render();
            })));
        Grid.SetRow(toolbar, 1);
        pane.Children.Add(toolbar);
        return pane;
    }

    private void ReplaceActive(
        CvBuilderDocument replacement,
        string? expanded = null,
        bool isExplicitSave = false)
    {
        if (_hasUnsavedChanges && !isExplicitSave)
        {
            MessageBox.Show(
                "Save or discard the current field edits before changing the document structure.",
                "Unsaved CV changes",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (_saveStatusText is not null)
            _saveStatusText.Text = "Saving…";
        int index = _documents.FindIndex(document => document.Id == replacement.Id);
        CvBuilderDocument current = _documents[index];
        if (current != replacement)
        {
            _undoHistory.Push(current);
            _redoHistory.Clear();
        }
        _documents[index] = replacement;
        _hasUnsavedChanges = false;
        if (expanded is not null)
            _expandedSectionId = expanded;
        PersistLibrary();
        Render();
    }

    private void Undo()
    {
        if (!EnsureSavedBeforeStructureChange())
            return;
        if (_undoHistory.Count == 0)
            return;

        int index = _documents.FindIndex(document => document.Id == _activeDocumentId);
        _redoHistory.Push(_documents[index]);
        _documents[index] = _undoHistory.Pop();
        _hasUnsavedChanges = false;
        PersistLibrary();
        Render();
    }

    private void Redo()
    {
        if (!EnsureSavedBeforeStructureChange())
            return;
        if (_redoHistory.Count == 0)
            return;

        int index = _documents.FindIndex(document => document.Id == _activeDocumentId);
        _undoHistory.Push(_documents[index]);
        _documents[index] = _redoHistory.Pop();
        _hasUnsavedChanges = false;
        PersistLibrary();
        Render();
    }

    private void PersistLibrary()
    {
        if (_portfolioDemo)
            return;

        int historyCount = Math.Min(_versionHistory.Count, _savedVersions.Count);
        CvStoredVersion[] versions = Enumerable.Range(0, historyCount)
            .Select(index => new CvStoredVersion(
                _activeDocumentId,
                _versionHistory[index],
                _savedVersions[index]))
            .ToArray();
        _storedVersions.RemoveAll(version => version.DocumentId == _activeDocumentId);
        _storedVersions.AddRange(versions);
        CareerDocumentLibraryStore.Save(
            new CareerDocumentLibrary(
                CareerDocumentLibrary.CurrentSchemaVersion,
                _documents.ToArray(),
                _activeDocumentId,
                _storedVersions.ToArray()),
            _libraryPath);
    }

    private void SelectExistingCv()
    {
        if (!EnsureSavedBeforeStructureChange())
            return;

        OpenFileDialog dialog = new()
        {
            Title = "Choose an existing CV for review",
            Filter = "CV documents (*.pdf;*.docx)|*.pdf;*.docx|All files (*.*)|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() != true)
            return;

        _importNotice =
            $"{System.IO.Path.GetFileName(dialog.FileName)} is queued for preview-only field review. The original file was not changed.";
        Render();
    }

    private void RunWhenSaved(Action action)
    {
        if (EnsureSavedBeforeStructureChange())
            action();
    }

    private bool EnsureSavedBeforeStructureChange()
    {
        if (!_hasUnsavedChanges)
            return true;

        MessageBox.Show(
            "Save the current field edits before changing the builder layout or history.",
            "Unsaved CV changes",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return false;
    }

    private static Button ImportTile(
        string icon,
        string title,
        string subtitle,
        Action action)
    {
        StackPanel content = new() { HorizontalAlignment = HorizontalAlignment.Center };
        content.Children.Add(new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe Fluent Icons"),
            FontSize = 22,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        content.Children.Add(Title(title, 14));
        content.Children.Add(Body(subtitle, "#93A4C3", FontWeights.Normal));
        Button button = ButtonBase(string.Empty, "#141D30", "#FFFFFF");
        button.Content = content;
        button.BorderBrush = Brush("#3A4964");
        button.BorderThickness = new Thickness(1);
        button.Padding = new Thickness(14);
        button.Margin = new Thickness(0, 0, 10, 18);
        AutomationProperties.SetName(button, title);
        button.Click += (_, _) => action();
        return button;
    }

    private static Border SourceNotice(CvBuilderSection section) => new()
    {
        Background = Brush(section.SourceFactIds.Count == 0 ? "#FFF4E6" : "#EAF7F1"),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(10),
        Margin = new Thickness(0, 0, 0, 12),
        Child = Body(
            section.SourceFactIds.Count == 0
                ? "Review this section before including it in an application."
                : $"Verified from {section.SourceFactIds.Count} trusted LifeOS source(s).",
            section.SourceFactIds.Count == 0 ? "#8C5D26" : "#25664E",
            FontWeights.SemiBold)
    };

    private Border Field(string label, UIElement input)
    {
        AutomationProperties.SetName(input, label);
        if (input is TextBoxBase textEditor)
            textEditor.TextChanged += (_, _) => MarkEditing();
        if (input is LifeOsDateSelector dateSelector)
            dateSelector.SelectedDateChanged += (_, _) => MarkEditing();
        StackPanel panel = new();
        panel.Children.Add(Body(label, "#C8D3E6", FontWeights.SemiBold));
        panel.Children.Add(input);
        return new Border { Margin = new Thickness(0, 0, 12, 12), Child = panel };
    }

    private static Grid TwoColumns()
    {
        Grid grid = new();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        return grid;
    }

    private static TextBox Input(string value) => new()
    {
        Text = value,
        Height = 40,
        Background = Brush("#171F32"),
        Foreground = Brushes.White,
        BorderBrush = Brush("#35445F"),
        Padding = new Thickness(10),
        FontSize = 13
    };

    private static ComboBox Select(IEnumerable<string> values, string selected)
    {
        ComboBox box = new()
        {
            Height = 40,
            Background = Brush("#171F32"),
            Foreground = Brushes.White,
            BorderBrush = Brush("#35445F"),
            Padding = new Thickness(8),
            FontSize = 13
        };
        foreach (string value in values)
            box.Items.Add(value);
        box.SelectedItem = box.Items.Cast<string>()
            .FirstOrDefault(value => string.Equals(value, selected, StringComparison.Ordinal))
            ?? box.Items.Cast<string>().FirstOrDefault();
        return box;
    }

    private static LifeOsDateSelector CalendarInput(DateTime? value) =>
        new(value);

    private static RichTextBox RichDescription(string value, string richContent)
    {
        RichTextBox editor = new()
        {
            MinHeight = 120,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brush("#171F32"),
            Foreground = Brushes.White,
            BorderBrush = Brush("#35445F"),
            Padding = new Thickness(10),
            FontSize = 13,
            AcceptsReturn = true
        };
        if (!string.IsNullOrWhiteSpace(richContent) &&
            XamlReader.Parse(richContent) is FlowDocument savedDocument)
        {
            editor.Document = savedDocument;
        }
        else
        {
            editor.Document.Blocks.Clear();
            editor.Document.Blocks.Add(new Paragraph(new Run(value)));
        }
        return editor;
    }

    private static UIElement BuildFormattingToolbar(RichTextBox editor)
    {
        StackPanel toolbar = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 6)
        };
        toolbar.Children.Add(SymbolButton("\uE8DD", "Bold", () =>
            EditingCommands.ToggleBold.Execute(null, editor)));
        toolbar.Children.Add(SymbolButton("\uE8DB", "Italic", () =>
            EditingCommands.ToggleItalic.Execute(null, editor)));
        toolbar.Children.Add(SymbolButton("\uE8FD", "Bulleted list", () =>
            EditingCommands.ToggleBullets.Execute(null, editor)));
        return toolbar;
    }

    private static string ReadRichText(RichTextBox editor) =>
        new TextRange(
            editor.Document.ContentStart,
            editor.Document.ContentEnd).Text.Trim();

    private static string WriteRichText(RichTextBox editor) =>
        XamlWriter.Save(editor.Document);

    private static Button SymbolTextButton(
        string glyph,
        string label,
        Action action)
    {
        StackPanel content = new()
        {
            Orientation = Orientation.Horizontal
        };
        content.Children.Add(new TextBlock
        {
            Text = glyph,
            FontFamily = new FontFamily("Segoe Fluent Icons"),
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        content.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center
        });

        Button button = ButtonBase(string.Empty, "#202A40", "#FFFFFF");
        button.Content = content;
        AutomationProperties.SetName(button, label);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button SymbolButton(
        string glyph,
        string tooltip,
        Action action)
    {
        Button button = ButtonBase(glyph, "#263754", "#FFFFFF");
        button.FontFamily = new FontFamily("Segoe Fluent Icons");
        button.FontSize = 14;
        button.Padding = new Thickness(10, 7, 10, 7);
        button.Margin = new Thickness(6, 0, 0, 0);
        button.ToolTip = tooltip;
        AutomationProperties.SetName(button, tooltip);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button PrimaryButton(string label, Action action)
    {
        Button button = ButtonBase(label, "#7253E8", "#FFFFFF");
        button.Margin = new Thickness(0, 0, 10, 0);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button OutlineButton(string label, Action action)
    {
        Button button = ButtonBase(label, "#182238", "#E4EAF4");
        button.BorderBrush = Brush("#3A4964");
        button.BorderThickness = new Thickness(1);
        button.Margin = new Thickness(0, 0, 10, 0);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button Chip(string label, Action action)
    {
        Button button = ButtonBase(label, "#182238", "#E4EAF4");
        button.BorderBrush = Brush("#3A4964");
        button.BorderThickness = new Thickness(1);
        button.Padding = new Thickness(11, 7, 11, 7);
        button.Margin = new Thickness(0, 0, 8, 8);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button ToolbarItem(string icon, string tooltip, Action action)
    {
        Button button = ButtonBase(icon, "#FFFFFF", "#3D3F49");
        button.FontFamily = new FontFamily("Segoe Fluent Icons");
        button.FontSize = 16;
        button.ToolTip = tooltip;
        button.Margin = new Thickness(14, 10, 0, 10);
        button.MinWidth = 58;
        button.Click += (_, _) => action();
        return button;
    }

    private static Button ButtonBase(string label, string background, string foreground)
    {
        Button button = new()
        {
            Content = label,
            Foreground = Brush(foreground),
            BorderBrush = Brushes.Transparent,
            Padding = new Thickness(13, 8, 13, 8),
            FontSize = 13,
            Cursor = Cursors.Hand
        };
        button.Style = ButtonStyle(background, foreground);
        return button;
    }

    private static Style ButtonStyle(string background, string foreground)
    {
        Style style = new(typeof(Button));
        style.Setters.Add(new Setter(BackgroundProperty, Brush(background)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(foreground)));

        FrameworkElementFactory border = new(typeof(Border));
        border.Name = "buttonBorder";
        border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
        border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(BorderBrushProperty));
        border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(BorderThicknessProperty));
        border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(PaddingProperty));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));

        FrameworkElementFactory presenter = new(typeof(ContentPresenter));
        presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
        presenter.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
        border.AppendChild(presenter);

        ControlTemplate template = new(typeof(Button)) { VisualTree = border };

        Trigger hover = new()
        {
            Property = IsMouseOverProperty,
            Value = true
        };
        hover.Setters.Add(new Setter(
            Border.BackgroundProperty,
            Brush(background == "#FFFFFF" ? "#D8DFEA" : "#33466A"),
            "buttonBorder"));
        hover.Setters.Add(new Setter(
            Control.ForegroundProperty,
            Brush(background == "#FFFFFF" ? "#20283A" : "#FFFFFF")));
        template.Triggers.Add(hover);
        Trigger disabled = new()
        {
            Property = IsEnabledProperty,
            Value = false
        };
        disabled.Setters.Add(new Setter(OpacityProperty, 0.38));
        disabled.Setters.Add(new Setter(CursorProperty, Cursors.Arrow));
        template.Triggers.Add(disabled);
        style.Setters.Add(new Setter(TemplateProperty, template));
        return style;
    }

    private Border BuildStatusPill()
    {
        _saveStatusText = Body(
            _hasUnsavedChanges ? "Editing…" : $"Saved · v{Active.Version}",
            _hasUnsavedChanges ? "#FFD18A" : "#8BE0B7",
            FontWeights.SemiBold);
        return new Border
        {
            Background = Brush(_hasUnsavedChanges ? "#3A3022" : "#24332D"),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(11, 6, 11, 6),
            Margin = new Thickness(0, 5, 8, 5),
            Child = _saveStatusText
        };
    }

    private static TextBlock Title(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brushes.White,
        TextWrapping = TextWrapping.Wrap
    };

    private static TextBlock Body(string text, string color, FontWeight weight) => new()
    {
        Text = text,
        FontSize = 12,
        FontWeight = weight,
        Foreground = Brush(color),
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 4, 0, 0)
    };

    private static TextBlock PreviewText(
        string text,
        double size,
        string color,
        FontWeight weight,
        Thickness? margin = null) => new()
    {
        Text = text,
        FontSize = size,
        Foreground = Brush(color),
        FontWeight = weight,
        TextWrapping = TextWrapping.Wrap,
        Margin = margin ?? new Thickness()
    };

    private static Brush Brush(string value) =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));

    private sealed class LifeOsDateSelector : Grid
    {
        private readonly Button _display;
        private readonly Button _clear;
        private readonly Popup _popup;
        private DateTime? _selectedDate;
        public event EventHandler? SelectedDateChanged;

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            private set
            {
                _selectedDate = value;
                _display.Content = value.HasValue
                    ? value.Value.ToString("d MMMM yyyy")
                    : "Select a date";
                _clear.Visibility = value.HasValue
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                SelectedDateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public LifeOsDateSelector(DateTime? value)
        {
            Height = 42;
            ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _popup = new Popup
            {
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true
            };

            _display = SymbolTextButton(
                "\uE787",
                value.HasValue ? value.Value.ToString("d MMMM yyyy") : "Select a date",
                () => _popup.IsOpen = true);
            _display.HorizontalContentAlignment = HorizontalAlignment.Left;
            _display.HorizontalAlignment = HorizontalAlignment.Stretch;
            _display.Background = Brush("#171F32");
            _display.BorderBrush = Brush("#35445F");
            _display.BorderThickness = new Thickness(1);
            Children.Add(_display);

            Calendar calendar = new()
            {
                SelectedDate = value,
                DisplayDate = value ?? DateTime.Today,
                Background = Brush("#111A2C"),
                Foreground = Brushes.White,
                BorderBrush = Brush("#46597A"),
                BorderThickness = new Thickness(1),
                SelectionMode = CalendarSelectionMode.SingleDate
            };
            calendar.Resources[typeof(CalendarDayButton)] = CalendarDayStyle();
            calendar.Resources[typeof(CalendarButton)] = CalendarMonthStyle();
            calendar.SelectedDatesChanged += (_, _) =>
            {
                SelectedDate = calendar.SelectedDate;
                _popup.IsOpen = false;
            };

            _clear = SymbolButton("\uE711", "Clear date", () =>
            {
                calendar.SelectedDate = null;
                SelectedDate = null;
            });
            _clear.Margin = new Thickness(6, 0, 0, 0);
            _clear.MinWidth = 40;
            Grid.SetColumn(_clear, 1);
            Children.Add(_clear);

            Border popupSurface = new()
            {
                Background = Brush("#111A2C"),
                BorderBrush = Brush("#46597A"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Child = calendar
            };
            _popup.Child = popupSurface;
            _popup.PlacementTarget = _display;

            SelectedDate = value;
        }

        private static Style CalendarDayStyle()
        {
            Style style = new(typeof(CalendarDayButton));
            style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            Trigger hover = new()
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            hover.Setters.Add(new Setter(BackgroundProperty, Brush("#33466A")));
            style.Triggers.Add(hover);

            Trigger selected = new()
            {
                Property = CalendarDayButton.IsSelectedProperty,
                Value = true
            };
            selected.Setters.Add(new Setter(BackgroundProperty, Brush("#7253E8")));
            selected.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Triggers.Add(selected);
            return style;
        }

        private static Style CalendarMonthStyle()
        {
            Style style = new(typeof(CalendarButton));
            style.Setters.Add(new Setter(BackgroundProperty, Brush("#17233A")));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            Trigger hover = new()
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            hover.Setters.Add(new Setter(BackgroundProperty, Brush("#33466A")));
            style.Triggers.Add(hover);
            return style;
        }
    }
}

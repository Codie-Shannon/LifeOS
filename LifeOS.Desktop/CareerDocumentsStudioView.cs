using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.CareerStudio;

namespace LifeOS.Desktop;

public sealed class CareerDocumentsStudioView : UserControl
{
    private static readonly DateTimeOffset ProofNow =
        new(2026, 7, 26, 15, 0, 0, TimeSpan.FromHours(12));

    private readonly CareerDocumentBuilderService _service = new();
    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(ProofNow);
    private readonly List<CvBuilderDocument> _documents;
    private string _activeDocumentId;
    private string _expandedSectionId = "contact";
    private bool _showOptionalSections;

    private CvBuilderDocument Active =>
        _documents.Single(document => document.Id == _activeDocumentId);

    public CareerDocumentsStudioView()
    {
        CvBuilderWorkspace workspace = CareerDocumentBuilderProofData.Build(ProofNow);
        _documents = workspace.Documents.ToList();
        _activeDocumentId = workspace.ActiveDocumentId;
        Background = Brush("#ECEEF2");
        Foreground = Brush("#20212A");
        FontFamily = new FontFamily("Segoe UI");
        Render();
    }

    private void Render()
    {
        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.Children.Add(BuildTopBar());

        Grid workspace = new() { Background = Brush("#ECEEF2") };
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        UIElement editor = BuildContinuousEditor();
        workspace.Children.Add(editor);

        UIElement preview = BuildPreviewPane();
        Grid.SetColumn(preview, 1);
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
            Background = Brush("#1D1C20")
        };
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Button back = TopButton("←  CVs", () => { });
        back.Margin = new Thickness(14, 12, 0, 12);
        bar.Children.Add(back);

        ComboBox documents = new()
        {
            ItemsSource = _documents,
            DisplayMemberPath = nameof(CvBuilderDocument.Name),
            SelectedItem = Active,
            Width = 290,
            Height = 38,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brush("#28272C"),
            Foreground = Brushes.White,
            BorderBrush = Brush("#44424A"),
            Padding = new Thickness(9)
        };
        documents.SelectionChanged += (_, _) =>
        {
            if (documents.SelectedItem is CvBuilderDocument document &&
                document.Id != _activeDocumentId)
            {
                _activeDocumentId = document.Id;
                _expandedSectionId = "contact";
                Render();
            }
        };
        Grid.SetColumn(documents, 1);
        bar.Children.Add(documents);

        StackPanel actions = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 11, 14, 11)
        };
        actions.Children.Add(StatusPill($"☁  Saved · v{Active.Version}"));
        actions.Children.Add(TopButton("↶", () => { }));
        actions.Children.Add(TopButton("↷", () => { }));
        actions.Children.Add(TopButton("EN-NZ", () => { }));
        Button download = TopButton("Download in SG-82", () => { });
        download.Background = Brush("#7253E8");
        download.ToolTip = "PDF and DOCX export is intentionally scheduled for SG-82.";
        actions.Children.Add(download);
        Grid.SetColumn(actions, 2);
        bar.Children.Add(actions);

        return bar;
    }

    private UIElement BuildContinuousEditor()
    {
        StackPanel form = new() { Margin = new Thickness(30, 24, 24, 40) };

        Grid import = new();
        import.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        import.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        import.Children.Add(ImportTile(
            "↥",
            "Upload existing CV",
            "Import is preview-only until reviewed."));
        Border profileImport = ImportTile(
            "L",
            "Import trusted LifeOS profile",
            $"{_materials.Facts.Count(fact => fact.IsTrusted)} accepted facts available");
        Grid.SetColumn(profileImport, 1);
        import.Children.Add(profileImport);
        form.Children.Add(import);

        foreach (CvBuilderSection section in Active.Sections
                     .Where(section => section.IsEnabled)
                     .OrderBy(section => section.Order))
        {
            form.Children.Add(BuildSectionAccordion(section));
        }

        Button optional = OutlineButton(
            _showOptionalSections ? "Hide optional sections" : "+ Add optional section",
            () =>
            {
                _showOptionalSections = !_showOptionalSections;
                Render();
            });
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
                         "Qualities", "Achievements", "Custom section"
                     })
            {
                options.Children.Add(DisabledChip($"+ {label}"));
            }
            form.Children.Add(options);
        }

        CvBuilderReview review = _service.Review(Active, _materials.Facts);
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

        return new Border
        {
            Background = Brushes.White,
            BorderBrush = Brush("#D1D4DA"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = new ScrollViewer
            {
                Content = form,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }
        };
    }

    private UIElement BuildSectionAccordion(CvBuilderSection section)
    {
        bool expanded = section.Id == _expandedSectionId;
        StackPanel card = new();

        Grid heading = new();
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.Children.Add(Title(section.Heading, 18));

        StackPanel tools = new() { Orientation = Orientation.Horizontal };
        tools.Children.Add(IconButton("↑", () =>
            ReplaceActive(_service.MoveSection(Active, section.Id, -1, DateTimeOffset.Now), section.Id)));
        tools.Children.Add(IconButton("↓", () =>
            ReplaceActive(_service.MoveSection(Active, section.Id, 1, DateTimeOffset.Now), section.Id)));
        tools.Children.Add(IconButton(expanded ? "⌃" : "⌄", () =>
        {
            _expandedSectionId = expanded ? string.Empty : section.Id;
            Render();
        }));
        Grid.SetColumn(tools, 1);
        heading.Children.Add(tools);
        card.Children.Add(heading);

        if (expanded)
        {
            if (section.Kind == CvSectionKind.Contact)
                BuildPersonalDetails(card, section);
            else
                BuildSectionForm(card, section);
        }

        return new Border
        {
            Background = Brushes.White,
            BorderBrush = Brush("#D8DAE0"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 17, 0, 17),
            Child = card
        };
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

        WrapPanel optional = new() { Margin = new Thickness(0, 4, 0, 8) };
        foreach (string field in new[] { "Website", "LinkedIn", "Driving licence", "Nationality", "Custom field" })
            optional.Children.Add(DisabledChip($"+ {field}"));
        panel.Children.Add(optional);

        Button save = PrimaryButton("Save personal details", () =>
        {
            CvBuilderDocument updated = _service.SetTargetRole(Active, role.Text, DateTimeOffset.Now);
            updated = _service.UpdateSection(
                updated,
                section.Id,
                section.Heading,
                section.Content,
                DateTimeOffset.Now);
            ReplaceActive(updated, section.Id);
        });
        panel.Children.Add(save);
    }

    private void BuildSectionForm(StackPanel panel, CvBuilderSection section)
    {
        TextBox heading = Input(section.Heading);
        panel.Children.Add(Field("Section title", heading));

        if (section.Kind is CvSectionKind.Employment or CvSectionKind.Education)
        {
            Grid entry = TwoColumns();
            entry.Children.Add(Field(
                section.Kind == CvSectionKind.Employment ? "Job title" : "Education",
                Input(section.Kind == CvSectionKind.Employment ? Active.TargetRole : string.Empty)));
            Border organization = Field(
                section.Kind == CvSectionKind.Employment ? "Employer" : "Institution",
                Input(section.Kind == CvSectionKind.Employment ? "Self-directed and client projects" : string.Empty));
            Grid.SetColumn(organization, 1);
            entry.Children.Add(organization);
            panel.Children.Add(entry);

            Grid dates = TwoColumns();
            dates.Children.Add(Field("Start date", Input("2025")));
            Border end = Field("End date", Input("Present"));
            Grid.SetColumn(end, 1);
            dates.Children.Add(end);
            panel.Children.Add(dates);
        }

        TextBox content = Multiline(section.Content);
        panel.Children.Add(Field("Description", content));
        panel.Children.Add(SourceNotice(section));

        StackPanel actions = new() { Orientation = Orientation.Horizontal };
        actions.Children.Add(PrimaryButton("Done", () =>
            ReplaceActive(_service.UpdateSection(
                Active,
                section.Id,
                heading.Text,
                content.Text,
                DateTimeOffset.Now),
                section.Id)));
        actions.Children.Add(OutlineButton(
            section.Kind is CvSectionKind.Employment or CvSectionKind.Education
                ? $"+ Add {section.Kind.ToString().ToLowerInvariant()}"
                : "Add another entry",
            () => { }));
        panel.Children.Add(actions);
    }

    private UIElement BuildPreviewPane()
    {
        Grid pane = new() { Background = Brush("#EEF0F4") };
        pane.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        pane.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Border page = new()
        {
            Width = 560,
            MinHeight = 735,
            Background = Brushes.White,
            Margin = new Thickness(28, 18, 28, 16),
            Padding = new Thickness(42),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 18,
                ShadowDepth = 2,
                Opacity = 0.18
            }
        };

        StackPanel cv = new();
        Border banner = new()
        {
            Background = Brush("#315E91"),
            Margin = new Thickness(-42, -42, -42, 22),
            Padding = new Thickness(42, 26, 42, 26),
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

        foreach (CvBuilderSection section in Active.VisibleSections)
        {
            cv.Children.Add(PreviewText(
                section.Heading.ToUpperInvariant(),
                11,
                "#315E91",
                FontWeights.Bold,
                new Thickness(0, 10, 0, 4)));
            cv.Children.Add(new Border
            {
                Height = 1,
                Background = Brush("#D7DCE3"),
                Margin = new Thickness(0, 0, 0, 6)
            });
            cv.Children.Add(PreviewText(
                string.IsNullOrWhiteSpace(section.Content) ? "Add content in the editor." : section.Content,
                10.5,
                "#333944",
                FontWeights.Normal));
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
        toolbar.Children.Add(ToolbarItem("▦", "Templates · SG-82"));
        toolbar.Children.Add(ToolbarItem("Aa", "Typography · SG-82"));
        toolbar.Children.Add(ToolbarItem("↕", "Spacing · SG-82"));
        toolbar.Children.Add(ToolbarItem("◒", "Colours · SG-82"));
        toolbar.Children.Add(ToolbarItem("⛶", "Fullscreen preview"));
        Grid.SetRow(toolbar, 1);
        pane.Children.Add(toolbar);
        return pane;
    }

    private void ReplaceActive(CvBuilderDocument replacement, string? expanded = null)
    {
        int index = _documents.FindIndex(document => document.Id == replacement.Id);
        _documents[index] = replacement;
        if (expanded is not null)
            _expandedSectionId = expanded;
        Render();
    }

    private static Border ImportTile(string icon, string title, string subtitle)
    {
        StackPanel content = new() { HorizontalAlignment = HorizontalAlignment.Center };
        content.Children.Add(Title(icon, 22));
        content.Children.Add(Title(title, 14));
        content.Children.Add(Body(subtitle, "#777B85", FontWeights.Normal));
        return new Border
        {
            BorderBrush = Brush("#D8DAE0"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 10, 18),
            Child = content
        };
    }

    private static Border SourceNotice(CvBuilderSection section) => new()
    {
        Background = Brush(section.SourceFactIds.Count == 0 ? "#FFF4E6" : "#EAF7F1"),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(10),
        Margin = new Thickness(0, 0, 0, 12),
        Child = Body(
            section.SourceFactIds.Count == 0
                ? "Manual content · verify before export"
                : $"LifeOS sources: {string.Join(", ", section.SourceFactIds)}",
            section.SourceFactIds.Count == 0 ? "#8C5D26" : "#25664E",
            FontWeights.SemiBold)
    };

    private static Border Field(string label, UIElement input)
    {
        StackPanel panel = new();
        panel.Children.Add(Body(label, "#454853", FontWeights.SemiBold));
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
        Background = Brush("#F5F5F7"),
        Foreground = Brush("#20212A"),
        BorderBrush = Brush("#D7D8DD"),
        Padding = new Thickness(10),
        FontSize = 13
    };

    private static TextBox Multiline(string value) => new()
    {
        Text = value,
        MinHeight = 110,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        Background = Brush("#F5F5F7"),
        Foreground = Brush("#20212A"),
        BorderBrush = Brush("#D7D8DD"),
        Padding = new Thickness(10),
        FontSize = 13
    };

    private static Button TopButton(string label, Action action)
    {
        Button button = ButtonBase(label, "#29282D", "#FFFFFF");
        button.Margin = new Thickness(6, 0, 0, 0);
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
        Button button = ButtonBase(label, "#FFFFFF", "#3D3F49");
        button.BorderBrush = Brush("#C8CBD2");
        button.BorderThickness = new Thickness(1);
        button.Margin = new Thickness(0, 0, 10, 0);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button IconButton(string label, Action action)
    {
        Button button = ButtonBase(label, "#FFFFFF", "#595C66");
        button.BorderBrush = Brush("#D5D7DC");
        button.BorderThickness = new Thickness(1);
        button.Padding = new Thickness(9, 5, 9, 5);
        button.Margin = new Thickness(5, 0, 0, 0);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button Chip(string label, Action action)
    {
        Button button = ButtonBase(label, "#FFFFFF", "#454853");
        button.BorderBrush = Brush("#C9CCD2");
        button.BorderThickness = new Thickness(1);
        button.Padding = new Thickness(11, 7, 11, 7);
        button.Margin = new Thickness(0, 0, 8, 8);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button DisabledChip(string label)
    {
        Button button = Chip(label, () => { });
        button.ToolTip = "Planned for the complete Career Documents Studio.";
        return button;
    }

    private static Button ToolbarItem(string icon, string tooltip)
    {
        Button button = ButtonBase(icon, "#FFFFFF", "#3D3F49");
        button.ToolTip = tooltip;
        button.Margin = new Thickness(14, 10, 0, 10);
        button.MinWidth = 58;
        return button;
    }

    private static Button ButtonBase(string label, string background, string foreground) => new()
    {
        Content = label,
        Background = Brush(background),
        Foreground = Brush(foreground),
        BorderBrush = Brushes.Transparent,
        Padding = new Thickness(13, 8, 13, 8),
        FontSize = 13,
        Cursor = System.Windows.Input.Cursors.Hand
    };

    private static Border StatusPill(string text) => new()
    {
        Background = Brush("#24332D"),
        CornerRadius = new CornerRadius(14),
        Padding = new Thickness(11, 6, 11, 6),
        Margin = new Thickness(0, 5, 8, 5),
        Child = Body(text, "#8BE0B7", FontWeights.SemiBold)
    };

    private static TextBlock Title(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brush("#25262D"),
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
}

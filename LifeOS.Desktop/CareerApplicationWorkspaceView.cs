using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.CareerStudio;
using Microsoft.Win32;

namespace LifeOS.Desktop;

public sealed class CareerApplicationWorkspaceView : UserControl
{
    private readonly CareerApplicationWorkspaceService _service = new();
    private readonly CareerApplicationService _applicationService = new();
    private readonly CareerDocumentBuilderService _cvService = new();
    private readonly string _workspacePath;
    private readonly string _cvLibraryPath;
    private readonly bool _portfolioDemo;
    private readonly List<CareerOpportunity> _opportunities;
    private readonly List<CoverLetterDocument> _letters;
    private readonly List<CareerApplication> _applications;
    private readonly List<CareerApplicationPack> _packs;
    private readonly List<CareerFact> _facts;
    private string _activeOpportunityId;
    private string _activeLetterId;
    private string? _notice;

    private CareerOpportunity? ActiveOpportunity => _opportunities
        .FirstOrDefault(opportunity => opportunity.Id == _activeOpportunityId);

    private CoverLetterDocument? ActiveLetter => _letters
        .FirstOrDefault(letter => letter.Id == _activeLetterId) ??
        _letters.FirstOrDefault(letter => letter.OpportunityId == _activeOpportunityId);

    public CareerApplicationWorkspaceView(
        bool portfolioDemo = false,
        string? workspacePath = null,
        string? cvLibraryPath = null)
    {
        _portfolioDemo = portfolioDemo;
        _workspacePath = string.IsNullOrWhiteSpace(workspacePath)
            ? CareerApplicationWorkspaceStore.DefaultFilePath
            : Path.GetFullPath(workspacePath);
        _cvLibraryPath = string.IsNullOrWhiteSpace(cvLibraryPath)
            ? CareerDocumentLibraryStore.DefaultFilePath
            : Path.GetFullPath(cvLibraryPath);

        CareerApplicationWorkspace workspace = portfolioDemo
            ? CareerApplicationWorkspaceProofData.Build(
                new DateTimeOffset(2026, 8, 18, 15, 0, 0, TimeSpan.FromHours(12)))
            : CareerApplicationWorkspaceStore.Load(_workspacePath);
        _opportunities = workspace.Opportunities.ToList();
        _letters = workspace.CoverLetters.ToList();
        _applications = workspace.Applications.ToList();
        _packs = workspace.Packs.ToList();
        _facts = workspace.Facts.ToList();
        _activeOpportunityId = workspace.ActiveOpportunityId;
        _activeLetterId = workspace.ActiveCoverLetterId;

        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        Render();
    }

    private void Render()
    {
        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        Border header = new()
        {
            Background = Brush("#101827"),
            BorderBrush = Brush("#26354C"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 18, 24, 16)
        };
        StackPanel headerText = new();
        headerText.Children.Add(Label("Cover letters & application packs", 25, FontWeights.SemiBold));
        headerText.Children.Add(Label(
            "Opportunity-linked writing, explicit evidence acceptance, current-document checks and no automatic submission.",
            13,
            FontWeights.Normal,
            "#A9B6CA"));
        if (!string.IsNullOrWhiteSpace(_notice))
            headerText.Children.Add(Label(_notice, 12, FontWeights.SemiBold, "#8FD3B5"));
        header.Child = headerText;
        root.Children.Add(header);

        Grid body = new() { Margin = new Thickness(18) };
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(330) });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        ScrollViewer listScroll = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = BuildOpportunityColumn()
        };
        body.Children.Add(listScroll);

        ScrollViewer detailScroll = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = BuildDetailColumn()
        };
        Grid.SetColumn(detailScroll, 2);
        body.Children.Add(detailScroll);

        Grid.SetRow(body, 1);
        root.Children.Add(body);
        Content = root;
    }

    private UIElement BuildOpportunityColumn()
    {
        StackPanel panel = new();
        panel.Children.Add(SectionHeading("Opportunities"));
        panel.Children.Add(Label(
            _portfolioDemo
                ? "Portfolio Demo uses synthetic records."
                : "Ordinary mode starts empty and stores only records you add.",
            12,
            FontWeights.Normal,
            "#91A0B7"));

        foreach (CareerOpportunity opportunity in _opportunities)
        {
            Button button = new()
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        Label(opportunity.Title, 14, FontWeights.SemiBold),
                        Label(opportunity.Employer.Name, 12, FontWeights.Normal, "#A9B6CA"),
                        Label(opportunity.Stage.ToString(), 11, FontWeights.SemiBold, "#88B8FF")
                    }
                },
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(14),
                Margin = new Thickness(0, 8, 0, 0),
                Background = Brush(opportunity.Id == _activeOpportunityId ? "#203B62" : "#151F30"),
                Foreground = Brushes.White,
                BorderBrush = Brush("#31445F")
            };
            button.Click += (_, _) =>
            {
                _activeOpportunityId = opportunity.Id;
                _activeLetterId = _letters.FirstOrDefault(letter =>
                    letter.OpportunityId == opportunity.Id)?.Id ?? string.Empty;
                Persist();
                Render();
            };
            panel.Children.Add(button);
        }

        if (_opportunities.Count == 0)
            panel.Children.Add(Card("No opportunities yet", "Add a real opportunity below. Nothing is seeded in ordinary mode."));

        if (_portfolioDemo)
            return panel;

        panel.Children.Add(SectionHeading("Add manually", new Thickness(0, 22, 0, 8)));
        TextBox title = Input("Role title");
        TextBox employer = Input("Employer");
        TextBox summary = Input("Role summary");
        summary.AcceptsReturn = true;
        summary.Height = 74;
        panel.Children.Add(FieldLabel("Role title"));
        panel.Children.Add(title);
        panel.Children.Add(FieldLabel("Employer"));
        panel.Children.Add(employer);
        panel.Children.Add(FieldLabel("Role summary"));
        panel.Children.Add(summary);
        panel.Children.Add(ActionButton("Add opportunity", () =>
        {
            if (string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(employer.Text))
            {
                _notice = "Role title and employer are required.";
                Render();
                return;
            }

            CareerOpportunity opportunity = _service.CreateManualOpportunity(
                $"opp-{Guid.NewGuid():N}",
                title.Text,
                employer.Text,
                summary.Text,
                DateTimeOffset.Now);
            _opportunities.Add(opportunity);
            _activeOpportunityId = opportunity.Id;
            _activeLetterId = string.Empty;
            _notice = "Opportunity saved locally.";
            Persist();
            Render();
        }));
        return panel;
    }

    private UIElement BuildDetailColumn()
    {
        StackPanel panel = new();
        CareerOpportunity? opportunity = ActiveOpportunity;
        if (opportunity is null)
        {
            panel.Children.Add(Card(
                "Start with an opportunity",
                "Add a role on the left. Then link a current CV, build the cover letter, review suggestions and assemble the application pack."));
            return panel;
        }

        panel.Children.Add(SectionHeading(opportunity.Title));
        panel.Children.Add(Label(
            $"{opportunity.Employer.Name}  •  {opportunity.Stage}  •  {opportunity.Source.DisplayName}",
            13,
            FontWeights.Normal,
            "#A9B6CA"));
        if (!string.IsNullOrWhiteSpace(opportunity.RoleSummary))
            panel.Children.Add(Card("Role context", opportunity.RoleSummary));

        BuildEvidenceEditor(panel);
        BuildLetterEditor(panel, opportunity);
        BuildApplicationPack(panel, opportunity);
        return panel;
    }

    private void BuildEvidenceEditor(StackPanel panel)
    {
        panel.Children.Add(SectionHeading("Trusted career evidence", new Thickness(0, 22, 0, 8)));
        if (_facts.Count == 0)
            panel.Children.Add(Card("No evidence linked", "Add a factual statement and its source. Suggestions never invent missing claims."));
        foreach (CareerFact fact in _facts.TakeLast(6))
            panel.Children.Add(Card(fact.Category, $"{fact.FactualValue}\nSource: {fact.SourceId}  •  {fact.TrustState}"));

        if (_portfolioDemo)
            return;

        TextBox category = Input("Category, for example Experience");
        TextBox factText = Input("Factual statement");
        factText.AcceptsReturn = true;
        factText.Height = 66;
        TextBox source = Input("Source or evidence reference");
        panel.Children.Add(FieldLabel("Category"));
        panel.Children.Add(category);
        panel.Children.Add(FieldLabel("Factual statement"));
        panel.Children.Add(factText);
        panel.Children.Add(FieldLabel("Source or evidence reference"));
        panel.Children.Add(source);
        panel.Children.Add(ActionButton("Add reviewed fact", () =>
        {
            if (string.IsNullOrWhiteSpace(factText.Text) || string.IsNullOrWhiteSpace(source.Text))
            {
                _notice = "A factual statement and source reference are required.";
                Render();
                return;
            }
            _facts.Add(new CareerFact(
                $"fact-{Guid.NewGuid():N}",
                string.IsNullOrWhiteSpace(category.Text) ? "Career evidence" : category.Text.Trim(),
                factText.Text.Trim(),
                source.Text.Trim(),
                CareerTrustState.UserAccepted,
                CareerOwnerReviewState.Accepted));
            _notice = "Reviewed career fact saved. Existing letters are unchanged until you deliberately refresh or recreate them.";
            Persist();
            Render();
        }, secondary: true));
    }

    private void BuildLetterEditor(StackPanel panel, CareerOpportunity opportunity)
    {
        panel.Children.Add(SectionHeading("Cover letter", new Thickness(0, 22, 0, 8)));
        CareerDocumentLibrary library = LoadCvLibrary();
        CoverLetterDocument? letter = ActiveLetter;
        CvBuilderDocument? cv = letter is null
            ? library.Documents.FirstOrDefault(document =>
                document.Id == library.ActiveDocumentId)
            : library.Documents.FirstOrDefault(document =>
                document.Id == letter.CvDocumentId);

        if (cv is null)
        {
            panel.Children.Add(Card(
                "CV required",
                "Create and save a CV in CV & Cover Letter Studio first. The application pack will never invent or silently select one."));
            return;
        }

        if (letter is null)
        {
            panel.Children.Add(Card("Linked CV", $"{cv.Name}  •  version {cv.Version}"));
            panel.Children.Add(ActionButton("Create evidence-backed draft", () =>
            {
                CoverLetterDocument created = _service.CreateDraft(
                    $"letter-{Guid.NewGuid():N}",
                    $"{opportunity.Title} cover letter",
                    opportunity,
                    cv,
                    _facts,
                    DateTimeOffset.Now);
                _letters.Add(created);
                _activeLetterId = created.Id;
                _notice = _facts.Count == 0
                    ? "Draft created without evidence claims. Add trusted evidence before export."
                    : "Draft created. Generated sections still require explicit review.";
                Persist();
                Render();
            }));
            return;
        }

        panel.Children.Add(Card(
            "Document links",
            $"Opportunity: {opportunity.Title}\nCV: {cv.Name} v{cv.Version}\nLetter: {letter.Name} v{letter.Version}"));

        foreach (CoverLetterSection section in letter.Sections)
        {
            Border sectionCard = new()
            {
                Background = Brush("#151F30"),
                BorderBrush = Brush("#31445F"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 8, 0, 0)
            };
            StackPanel sectionPanel = new();
            sectionPanel.Children.Add(Label(
                $"{section.Heading}  •  {section.State}",
                14,
                FontWeights.SemiBold));
            TextBox editor = Input(section.Heading);
            editor.Text = section.Text;
            editor.AcceptsReturn = true;
            editor.TextWrapping = TextWrapping.Wrap;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            editor.Height = 92;
            sectionPanel.Children.Add(editor);
            WrapPanel actions = new() { Margin = new Thickness(0, 8, 0, 0) };
            actions.Children.Add(ActionButton("Save edit", () =>
            {
                ReplaceLetter(_service.UpdateSection(
                    letter,
                    section.Id,
                    editor.Text,
                    DateTimeOffset.Now));
                _notice = $"{section.Heading} saved as user-authored text.";
                Persist();
                Render();
            }, compact: true));
            if (section.State == DraftSectionState.Generated)
            {
                actions.Children.Add(ActionButton("Accept suggestion", () =>
                {
                    ReplaceLetter(_service.SetSuggestionState(
                        letter,
                        section.Id,
                        DraftSectionState.Accepted,
                        DateTimeOffset.Now));
                    _notice = $"{section.Heading} explicitly accepted.";
                    Persist();
                    Render();
                }, secondary: true, compact: true));
                actions.Children.Add(ActionButton("Reject", () =>
                {
                    ReplaceLetter(_service.SetSuggestionState(
                        letter,
                        section.Id,
                        DraftSectionState.Rejected,
                        DateTimeOffset.Now));
                    _notice = $"{section.Heading} rejected; export remains blocked until replaced.";
                    Persist();
                    Render();
                }, secondary: true, compact: true));
            }
            sectionPanel.Children.Add(actions);
            sectionCard.Child = sectionPanel;
            panel.Children.Add(sectionCard);
        }

        CheckBox includeContact = new()
        {
            Content = "Include contact details in this document",
            IsChecked = letter.IncludeContactDetails,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 14, 0, 0)
        };
        CheckBox confirmContact = new()
        {
            Content = "I reviewed and confirm the contact details before export",
            IsChecked = letter.ContactDetailsConfirmed,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 6, 0, 0)
        };
        panel.Children.Add(includeContact);
        panel.Children.Add(confirmContact);
        panel.Children.Add(ActionButton("Save contact choice", () =>
        {
            ReplaceLetter(_service.SetContactDetails(
                letter,
                includeContact.IsChecked == true,
                confirmContact.IsChecked == true,
                DateTimeOffset.Now));
            _notice = "Per-document contact-detail choice saved.";
            Persist();
            Render();
        }, secondary: true));

        CoverLetterReview review = _service.Review(letter, opportunity, cv, _facts);
        panel.Children.Add(ReviewCard(review));
        WrapPanel exportActions = new() { Margin = new Thickness(0, 8, 0, 0) };
        exportActions.Children.Add(ActionButton("Export PDF", () => Export(letter, opportunity, cv, CvExportFormat.Pdf), compact: true));
        exportActions.Children.Add(ActionButton("Export DOCX", () => Export(letter, opportunity, cv, CvExportFormat.Docx), secondary: true, compact: true));
        panel.Children.Add(exportActions);
    }

    private void BuildApplicationPack(StackPanel panel, CareerOpportunity opportunity)
    {
        panel.Children.Add(SectionHeading("Application pack", new Thickness(0, 22, 0, 8)));
        CoverLetterDocument? letter = ActiveLetter;
        CareerDocumentLibrary library = LoadCvLibrary();
        CvBuilderDocument? cv = letter is null
            ? null
            : library.Documents.FirstOrDefault(document => document.Id == letter.CvDocumentId);
        CareerApplication? application = _applications.FirstOrDefault(item =>
            item.OpportunityId == opportunity.Id);
        CareerApplicationPack? pack = _packs.FirstOrDefault(item =>
            item.OpportunityId == opportunity.Id);

        if (application is null)
        {
            panel.Children.Add(Card(
                "No application preparation yet",
                "Creating preparation does not submit an application or contact the employer."));
            panel.Children.Add(ActionButton("Approve application preparation", () =>
            {
                int opportunityIndex = _opportunities.FindIndex(item => item.Id == opportunity.Id);
                CareerOpportunity approved = opportunity with
                {
                    Stage = OpportunityStage.Interested,
                    History = opportunity.History.Concat([
                        new OpportunityHistory(
                            DateTimeOffset.Now,
                            "Preparation approved",
                            opportunity.Stage,
                            OpportunityStage.Interested,
                            "Application preparation approved explicitly; nothing was submitted.")
                    ]).ToArray()
                };
                ApplicationActionResult result = _applicationService.CreateFromApprovedOpportunity(
                    approved,
                    explicitlyApproved: true,
                    DateTimeOffset.Now);
                if (!result.Applied)
                {
                    _notice = result.Message;
                    Render();
                    return;
                }
                _opportunities[opportunityIndex] = approved;
                _applications.Add(result.Application);
                _notice = result.Message;
                Persist();
                Render();
            }));
            return;
        }

        panel.Children.Add(Card(
            "Application preparation",
            $"State: {application.State}\nSubmission: {application.SubmissionChannel}\nLifeOS has not submitted anything."));

        if (pack is null)
        {
            panel.Children.Add(ActionButton("Assemble linked pack", () =>
            {
                if (cv is null || letter is null)
                {
                    _notice = "A linked CV and cover letter are required.";
                    Render();
                    return;
                }
                try
                {
                    _packs.Add(_service.CreatePack(
                        $"pack-{Guid.NewGuid():N}",
                        opportunity,
                        application,
                        cv,
                        letter,
                        DateTimeOffset.Now));
                    _notice = "Application pack assembled. Explicit review is still required.";
                    Persist();
                    Render();
                }
                catch (InvalidOperationException exception)
                {
                    _notice = exception.Message;
                    Render();
                }
            }));
            return;
        }

        CareerApplicationPack currentPack = _service.RefreshFreshness(
            pack,
            cv,
            letter,
            DateTimeOffset.Now);
        ReplacePack(currentPack);
        panel.Children.Add(Card(
            "Pack readiness",
            $"Reviewed: {currentPack.Reviewed}\nReady: {currentPack.IsReady}\n" +
            string.Join("\n", currentPack.Documents.Select(link =>
                $"{link.Kind}: v{link.SourceVersion} • {link.Freshness}"))));
        panel.Children.Add(ActionButton("Review and approve current pack", () =>
        {
            if (cv is null || letter is null)
            {
                _notice = "A linked CV and cover letter are required.";
                Render();
                return;
            }
            try
            {
                CoverLetterReview letterReview = _service.Review(letter, opportunity, cv, _facts);
                CvBuilderReview cvReview = _cvService.Review(cv, _facts);
                ReplacePack(_service.ReviewPack(
                    currentPack,
                    cv,
                    cvReview,
                    letter,
                    letterReview,
                    DateTimeOffset.Now));
                _notice = "Current CV and cover letter versions explicitly approved for this pack.";
                Persist();
                Render();
            }
            catch (InvalidOperationException exception)
            {
                _notice = exception.Message;
                Render();
            }
        }));
    }

    private UIElement ReviewCard(CoverLetterReview review)
    {
        string summary = review.Issues.Count == 0
            ? "All source, content and contact-detail checks pass."
            : string.Join("\n", review.Issues.Select(issue => $"• {issue.Message}"));
        return Card(
            review.CanExport ? "Ready to export" : "Review required",
            $"Accepted suggestions: {review.AcceptedSuggestionCount}\nLinked facts: {review.SourceFactCount}\n{summary}",
            review.CanExport ? "#143D32" : "#3B2B22");
    }

    private void Export(
        CoverLetterDocument letter,
        CareerOpportunity opportunity,
        CvBuilderDocument cv,
        CvExportFormat format)
    {
        try
        {
            CoverLetterReview review = _service.Review(letter, opportunity, cv, _facts);
            CvExportArtifact artifact = _service.Export(
                letter,
                review,
                opportunity,
                format,
                DateTimeOffset.Now);
            SaveFileDialog dialog = new()
            {
                FileName = artifact.SuggestedFileName,
                Filter = format == CvExportFormat.Pdf
                    ? "PDF document (*.pdf)|*.pdf"
                    : "Word document (*.docx)|*.docx"
            };
            if (dialog.ShowDialog() != true)
            {
                _notice = "Export cancelled; no file was written.";
                Render();
                return;
            }
            File.WriteAllBytes(dialog.FileName, artifact.Content);
            _notice = $"Derivative exported: {Path.GetFileName(dialog.FileName)}";
            Render();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            _notice = exception.Message;
            Render();
        }
    }

    private void ReplaceLetter(CoverLetterDocument updated)
    {
        int index = _letters.FindIndex(letter => letter.Id == updated.Id);
        if (index >= 0)
            _letters[index] = updated;
        else
            _letters.Add(updated);
        _activeLetterId = updated.Id;
    }

    private void ReplacePack(CareerApplicationPack updated)
    {
        int index = _packs.FindIndex(pack => pack.Id == updated.Id);
        if (index >= 0)
            _packs[index] = updated;
        else
            _packs.Add(updated);
    }

    private void Persist()
    {
        if (_portfolioDemo)
            return;
        CareerApplicationWorkspaceStore.Save(
            new CareerApplicationWorkspace(
                CareerApplicationWorkspace.CurrentSchemaVersion,
                _opportunities,
                _letters,
                _applications,
                _packs,
                _facts,
                _activeOpportunityId,
                _activeLetterId),
            _workspacePath);
    }

    private CareerDocumentLibrary LoadCvLibrary()
    {
        if (!_portfolioDemo)
            return CareerDocumentLibraryStore.Load(_cvLibraryPath);

        CvBuilderWorkspace workspace = CareerDocumentBuilderProofData.Build(
            new DateTimeOffset(2026, 8, 18, 15, 0, 0, TimeSpan.FromHours(12)));
        return new CareerDocumentLibrary(
            CareerDocumentLibrary.CurrentSchemaVersion,
            workspace.Documents,
            workspace.ActiveDocumentId,
            []);
    }

    private static TextBlock SectionHeading(string text, Thickness? margin = null) =>
        new()
        {
            Text = text,
            FontSize = 19,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            Margin = margin ?? new Thickness(0, 0, 0, 8)
        };

    private static TextBlock Label(
        string text,
        double size,
        FontWeight weight,
        string color = "#F5F8FC") => new()
        {
            Text = text,
            FontSize = size,
            FontWeight = weight,
            Foreground = Brush(color),
            TextWrapping = TextWrapping.Wrap
        };

    private static Border Card(string title, string body, string background = "#151F30")
    {
        StackPanel panel = new();
        panel.Children.Add(Label(title, 14, FontWeights.SemiBold));
        panel.Children.Add(Label(body, 12, FontWeights.Normal, "#B4C0D2"));
        return new Border
        {
            Background = Brush(background),
            BorderBrush = Brush("#31445F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 8, 0, 0),
            Child = panel
        };
    }

    private static TextBox Input(string accessibleName) => new()
    {
        ToolTip = accessibleName,
        Margin = new Thickness(0, 8, 0, 0),
        Padding = new Thickness(10, 7, 10, 7),
        Background = Brush("#0E1725"),
        Foreground = Brushes.White,
        BorderBrush = Brush("#3A4E6B"),
        TextWrapping = TextWrapping.Wrap
    };

    private static TextBlock FieldLabel(string text) => new()
    {
        Text = text,
        FontSize = 12,
        FontWeight = FontWeights.SemiBold,
        Foreground = Brush("#B4C0D2"),
        Margin = new Thickness(0, 10, 0, -4)
    };

    private static Button ActionButton(
        string text,
        Action action,
        bool secondary = false,
        bool compact = false)
    {
        Button button = new()
        {
            Content = text,
            Background = Brush(secondary ? "#25334A" : "#315E91"),
            Foreground = Brushes.White,
            BorderBrush = Brush(secondary ? "#405472" : "#477DB4"),
            Padding = compact ? new Thickness(10, 5, 10, 5) : new Thickness(14, 8, 14, 8),
            Margin = new Thickness(0, 9, 8, 0),
            MinHeight = 34
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

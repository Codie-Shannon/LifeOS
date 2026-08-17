using LifeOS.Core.CareerStudio;
using LifeOS.Mobile.Core.Foundation;
using Microsoft.Maui.Controls.Shapes;

namespace LifeOS.Mobile.Views;

public sealed class CareerApplicationHubPage : ContentPage
{
    private readonly CareerApplicationWorkspaceService _service = new();
    private readonly MobileExperienceMode _experienceMode;
    private CareerApplicationWorkspace _workspace;
    private readonly VerticalStackLayout _content = new();

    public CareerApplicationHubPage(MobileExperienceMode experienceMode)
    {
        _experienceMode = experienceMode;
        _workspace = experienceMode == MobileExperienceMode.PortfolioDemo
            ? CareerApplicationWorkspaceProofData.Build(
                new DateTimeOffset(2026, 8, 18, 15, 0, 0, TimeSpan.FromHours(12)))
            : CareerApplicationWorkspaceStore.Load();
        Title = "Career";
        BackgroundColor = Color.FromArgb("#101018");
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_experienceMode == MobileExperienceMode.Ordinary)
            _workspace = CareerApplicationWorkspaceStore.Load();
        Render();
    }

    private void Render()
    {
        _content.Children.Clear();
        _content.Padding = new Thickness(16, 16, 16, 32);
        _content.Spacing = 12;
        _content.Children.Add(Heading("Career applications", 30));
        _content.Children.Add(Text(
            "Review opportunities, cover-letter evidence and application packs. LifeOS never submits or contacts an employer.",
            14,
            "#B6B3C8"));
        _content.Children.Add(Badge(
            _experienceMode == MobileExperienceMode.PortfolioDemo
                ? "PORTFOLIO DEMO • SYNTHETIC"
                : "ORDINARY MODE • LOCAL DATA"));

        if (_workspace.Opportunities.Count == 0)
        {
            _content.Children.Add(Card(
                "No career records yet",
                "Add an opportunity here or create CV and application-pack documents on Desktop. Ordinary mode contains no fictional records.",
                "EMPTY STATE"));
            AddOpportunityForm();
            return;
        }

        foreach (CareerOpportunity opportunity in _workspace.Opportunities)
        {
            CoverLetterDocument? letter = _workspace.CoverLetters.FirstOrDefault(item =>
                item.OpportunityId == opportunity.Id);
            CareerApplication? application = _workspace.Applications.FirstOrDefault(item =>
                item.OpportunityId == opportunity.Id);
            CareerApplicationPack? pack = _workspace.Packs.FirstOrDefault(item =>
                item.OpportunityId == opportunity.Id);

            _content.Children.Add(Card(
                opportunity.Title,
                $"{opportunity.Employer.Name}\n{opportunity.Stage} • {opportunity.Source.DisplayName}\n" +
                $"Cover letter: {(letter is null ? "Not linked" : $"v{letter.Version}")}\n" +
                $"Application: {application?.State.ToString() ?? "Not started"}\n" +
                $"Pack: {(pack?.IsReady == true ? "Reviewed and current" : "Review required")}",
                opportunity.Priority.ToString().ToUpperInvariant()));

            if (letter is not null)
                AddLetterReview(opportunity, letter, pack);
        }

        if (_experienceMode == MobileExperienceMode.Ordinary)
            AddOpportunityForm();
    }

    private void AddOpportunityForm()
    {
        _content.Children.Add(Heading("Add opportunity", 20));
        Entry title = Input("Role title");
        Entry employer = Input("Employer");
        Editor summary = new()
        {
            Placeholder = "Role summary",
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 88,
            BackgroundColor = Color.FromArgb("#1B1A25"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#8E899E")
        };
        _content.Children.Add(title);
        _content.Children.Add(employer);
        _content.Children.Add(summary);
        Button add = Button("Add locally", "#4057D6");
        add.Clicked += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(employer.Text))
            {
                await DisplayAlertAsync("Missing details", "Role title and employer are required.", "OK");
                return;
            }
            CareerOpportunity opportunity = _service.CreateManualOpportunity(
                $"opp-{Guid.NewGuid():N}",
                title.Text,
                employer.Text,
                summary.Text ?? string.Empty,
                DateTimeOffset.Now);
            _workspace = _workspace with
            {
                Opportunities = _workspace.Opportunities.Concat([opportunity]).ToArray(),
                ActiveOpportunityId = opportunity.Id
            };
            Save();
            Render();
        };
        _content.Children.Add(add);
    }

    private void AddLetterReview(
        CareerOpportunity opportunity,
        CoverLetterDocument letter,
        CareerApplicationPack? pack)
    {
        string? linkedCvId = pack?.Documents.FirstOrDefault(link =>
            link.Kind == CareerDocumentKind.Cv &&
            link.Freshness == MaterialFreshnessState.Current)?.DocumentId;
        CoverLetterReview review = _service.Review(
            letter,
            opportunity,
            linkedCvId,
            _workspace.Facts);
        _content.Children.Add(Heading("Cover-letter review", 20));
        foreach (CoverLetterSection section in letter.Sections)
        {
            _content.Children.Add(Card(
                section.Heading,
                $"{section.Text}\nSources: {(section.SourceFactIds.Count == 0 ? "User-authored or contextual" : string.Join(", ", section.SourceFactIds))}",
                section.State.ToString().ToUpperInvariant()));
            if (section.State == DraftSectionState.Generated)
            {
                HorizontalStackLayout actions = new() { Spacing = 8 };
                Button accept = Button("Accept", "#315E91");
                accept.Clicked += (_, _) => SetSuggestion(letter, section.Id, DraftSectionState.Accepted);
                Button reject = Button("Reject", "#363244");
                reject.Clicked += (_, _) => SetSuggestion(letter, section.Id, DraftSectionState.Rejected);
                actions.Children.Add(accept);
                actions.Children.Add(reject);
                _content.Children.Add(actions);
            }
        }
        _content.Children.Add(Card(
            "Safety review",
            review.Issues.Count == 0
                ? "Evidence, content, CV link and contact-detail checks pass."
                : string.Join("\n", review.Issues.Select(issue => $"• {issue.Message}")),
            review.CanExport ? "READY" : "ACTION REQUIRED"));

        if (review.CanExport)
        {
            HorizontalStackLayout exports = new() { Spacing = 8 };
            Button pdf = Button("Share PDF", "#4057D6");
            pdf.Clicked += async (_, _) => await ShareExport(
                opportunity,
                letter,
                review,
                CvExportFormat.Pdf);
            Button docx = Button("Share DOCX", "#363244");
            docx.Clicked += async (_, _) => await ShareExport(
                opportunity,
                letter,
                review,
                CvExportFormat.Docx);
            exports.Children.Add(pdf);
            exports.Children.Add(docx);
            _content.Children.Add(exports);
        }
    }

    private void SetSuggestion(
        CoverLetterDocument letter,
        string sectionId,
        DraftSectionState state)
    {
        CoverLetterDocument updated = _service.SetSuggestionState(
            letter,
            sectionId,
            state,
            DateTimeOffset.Now);
        _workspace = _workspace with
        {
            CoverLetters = _workspace.CoverLetters.Select(item =>
                item.Id == updated.Id ? updated : item).ToArray(),
            Packs = _workspace.Packs.Select(pack =>
                pack.OpportunityId == updated.OpportunityId
                    ? _service.RefreshFreshness(pack, null, updated, DateTimeOffset.Now)
                    : pack).ToArray()
        };
        Save();
        Render();
    }

    private async Task ShareExport(
        CareerOpportunity opportunity,
        CoverLetterDocument letter,
        CoverLetterReview review,
        CvExportFormat format)
    {
        try
        {
            CvExportArtifact artifact = _service.Export(
                letter,
                review,
                opportunity,
                format,
                DateTimeOffset.Now);
            string path = System.IO.Path.Combine(FileSystem.CacheDirectory, artifact.SuggestedFileName);
            await File.WriteAllBytesAsync(path, artifact.Content);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Share reviewed cover-letter derivative",
                File = new ShareFile(path, artifact.MediaType)
            });
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            await DisplayAlertAsync("Export blocked", exception.Message, "OK");
        }
    }

    private void Save()
    {
        if (_experienceMode == MobileExperienceMode.Ordinary)
            CareerApplicationWorkspaceStore.Save(_workspace);
    }

    private static Label Heading(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White
    };

    private static Label Text(string text, double size, string color) => new()
    {
        Text = text,
        FontSize = size,
        TextColor = Color.FromArgb(color)
    };

    private static Label Badge(string text) => new()
    {
        Text = text,
        FontSize = 11,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb("#AFA4FF")
    };

    private static Entry Input(string placeholder) => new()
    {
        Placeholder = placeholder,
        BackgroundColor = Color.FromArgb("#1B1A25"),
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#8E899E")
    };

    private static Button Button(string text, string background) => new()
    {
        Text = text,
        BackgroundColor = Color.FromArgb(background),
        TextColor = Colors.White,
        CornerRadius = 12,
        MinimumHeightRequest = 46
    };

    private static Border Card(string title, string body, string badge)
    {
        VerticalStackLayout layout = new() { Spacing = 7 };
        layout.Children.Add(Heading(title, 17));
        layout.Children.Add(Text(body, 13, "#D6D3E6"));
        layout.Children.Add(Text(badge, 11, "#9A7CFF"));
        return new Border
        {
            BackgroundColor = Color.FromArgb("#1B1A25"),
            Stroke = Color.FromArgb("#343143"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = 14,
            Content = layout
        };
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.CareerStudio;

namespace LifeOS.Desktop;

public sealed class CareerStudioView : UserControl
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 14, 0, 0, TimeSpan.FromHours(12));
    private readonly CareerStudioProof _proof = CareerProofData.Build(ProofNow);
    private readonly ContentControl _content = new();
    private readonly TextBlock _title = new();
    private readonly TextBlock _subtitle = new();
    private readonly Dictionary<string, Button> _buttons = new(StringComparer.Ordinal);

    public CareerStudioView()
    {
        Background = Brush("#0B1020");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        Content = Build();
        Show("Pipeline");
    }

    private UIElement Build()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var nav = new StackPanel();
        nav.Children.Add(Label("LIFEOS v12", 12, "#9AA9C7", FontWeights.SemiBold));
        nav.Children.Add(Label("Career Studio", 29, "#FFFFFF", FontWeights.Bold, new Thickness(0, 5, 0, 2)));
        nav.Children.Add(Label("Opportunity and application pipeline", 13, "#9AA9C7", FontWeights.Normal, new Thickness(0, 0, 0, 20)));

        foreach (var page in new[] { "Pipeline", "Opportunity detail", "Candidate review", "Application checklist", "Application timeline" })
        {
            var button = new Button
            {
                Content = page, Tag = page, HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(13, 10, 13, 10), Margin = new Thickness(0, 0, 0, 5),
                Background = Brushes.Transparent, Foreground = Brush("#D9E2F3"),
                BorderBrush = Brushes.Transparent, BorderThickness = new Thickness(1)
            };
            button.Click += (_, _) => Show((string)button.Tag);
            _buttons[page] = button;
            nav.Children.Add(button);
        }

        nav.Children.Add(Card("Boundary",
            "No autonomous applications\nNo recruiter messaging\nNo invented qualifications\nImported leads remain review-first",
            "REVIEW-FIRST"));

        var rail = new Border
        {
            Background = Brush("#11182B"), BorderBrush = Brush("#27324A"),
            BorderThickness = new Thickness(0, 0, 1, 0), Padding = new Thickness(18, 22, 18, 18),
            Child = new ScrollViewer { Content = nav, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
        };
        root.Children.Add(rail);

        var main = new Grid { Margin = new Thickness(28, 22, 28, 24) };
        main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _title.FontSize = 31; _title.FontWeight = FontWeights.Bold; _title.Foreground = Brushes.White;
        _subtitle.FontSize = 14; _subtitle.Foreground = Brush("#A9B8D5"); _subtitle.Margin = new Thickness(0, 5, 0, 20);
        var heading = new StackPanel(); heading.Children.Add(_title); heading.Children.Add(_subtitle);
        main.Children.Add(heading);
        Grid.SetRow(_content, 1); main.Children.Add(_content);
        Grid.SetColumn(main, 1); root.Children.Add(main);
        return root;
    }

    private void Show(string page)
    {
        foreach (var pair in _buttons)
        {
            pair.Value.Background = pair.Key == page ? Brush("#26365F") : Brushes.Transparent;
            pair.Value.BorderBrush = pair.Key == page ? Brush("#5577D8") : Brushes.Transparent;
        }

        (_title.Text, _subtitle.Text, _content.Content) = page switch
        {
            "Opportunity detail" => ("Opportunity detail", "Requirements, fit, source and linked evidence remain explicit and editable.", OpportunityDetail()),
            "Candidate review" => ("Imported and duplicate candidates", "Candidates await review; no silent trust promotion or merge occurs.", CandidateReview()),
            "Application checklist" => ("Application preparation", "Checklist completion and readiness remain explicit.", Placeholder("Application workflow is completed in Commit B.")),
            "Application timeline" => ("Application timeline", "Status, follow-up and interview context remain auditable.", Placeholder("Application workflow is completed in Commit B.")),
            _ => ("Career opportunity pipeline", "One authoritative pipeline with board filters, closing dates and review-first status.", Pipeline())
        };
    }

    private UIElement Pipeline()
    {
        var panel = Vertical();
        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 14) };
        foreach (var text in new[] { "All stages", "All employers", "All sources", "Closing date", "All work modes", "All priorities" })
            filters.Children.Add(Filter(text));
        panel.Children.Add(filters);
        foreach (var opportunity in _proof.Opportunities)
        {
            panel.Children.Add(Card(
                opportunity.Title,
                $"{opportunity.Employer.Name}\n{opportunity.Stage} • {opportunity.WorkMode} • {opportunity.Location}\nClosing: {(opportunity.ClosingUtc?.ToString("dd MMM yyyy") ?? "Not supplied")}\nSource: {opportunity.Source.Type} • Fresh {opportunity.FreshnessUtc:dd MMM HH:mm}",
                $"{opportunity.Priority} • {opportunity.Stage}".ToUpperInvariant()));
        }
        return Scroll(panel);
    }

    private UIElement OpportunityDetail()
    {
        var opportunity = _proof.Opportunities[0];
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var left = Vertical();
        left.Children.Add(Card(opportunity.Title,
            $"{opportunity.Employer.Name}\n{opportunity.RoleSummary}\n{opportunity.WorkMode} • {opportunity.EmploymentType}\n{opportunity.SalaryOrRateContext}\nSource: {opportunity.Source.Reference}",
            "AUTHORITATIVE LOCAL"));
        foreach (var req in opportunity.Requirements)
            left.Children.Add(Card(req.Type.ToString(), req.Description, req.IsRequired ? "REQUIRED" : "OPTIONAL"));

        var right = Vertical();
        var fit = opportunity.Fit!;
        right.Children.Add(Card("User-reviewed fit",
            $"Strengths: {string.Join("; ", fit.Strengths)}\nGaps: {string.Join("; ", fit.Gaps)}\nBlockers: {(fit.Blockers.Count == 0 ? "None recorded" : string.Join("; ", fit.Blockers))}\n{fit.Explanation}",
            "EDITABLE"));
        right.Children.Add(Card("Linked evidence",
            $"People: {string.Join(", ", opportunity.PeopleLinks)}\nProjects: {string.Join(", ", opportunity.ProjectLinks)}\nDocuments: {string.Join(", ", opportunity.DocumentLinks)}\nPortfolio: {string.Join(", ", opportunity.PortfolioEvidenceLinks)}",
            "PRESERVED LINKS"));
        right.Children.Add(Card("History", string.Join("\n", opportunity.History.Select(x => $"{x.OccurredUtc:dd MMM HH:mm} • {x.Action} • {x.SafeSummary}")), "AUDITABLE"));

        grid.Children.Add(left); Grid.SetColumn(right, 2); grid.Children.Add(right);
        return Scroll(grid);
    }

    private UIElement CandidateReview()
    {
        var panel = Vertical();
        foreach (var imported in _proof.ImportedCandidates)
            panel.Children.Add(Card($"Imported: {imported.RoleTitle}",
                $"{imported.EmployerName}\nIntegration Inbox: {imported.IntegrationInboxItemId}\nSource: {imported.SourceReference}\n{imported.ReviewReason}",
                imported.ReviewState.ToString().ToUpperInvariant()));
        foreach (var duplicate in _proof.DuplicateCandidates)
            panel.Children.Add(Card("Duplicate opportunity candidate",
                $"{duplicate.ExistingOpportunityId} ↔ {duplicate.CandidateOpportunityId}\nSignals: {string.Join(", ", duplicate.Signals)}\nConfidence: {duplicate.Confidence:P0}\nNo automatic merge, apply, contact or rejection.",
                duplicate.ReviewState.ToString().ToUpperInvariant()));
        return Scroll(panel);
    }

    private static UIElement Placeholder(string text) => Scroll(Vertical(Card("Coming from Commit B", text, "SCOPED")));
    private static StackPanel Vertical(params UIElement[] children) { var p = new StackPanel(); foreach (var c in children) p.Children.Add(c); return p; }
    private static ScrollViewer Scroll(UIElement child) => new() { Content = child, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private static ComboBox Filter(string text) => new() { Width = 170, Margin = new Thickness(0, 0, 10, 8), Padding = new Thickness(8), ItemsSource = new[] { text }, SelectedIndex = 0 };
    private static TextBlock Label(string text, double size, string color, FontWeight weight, Thickness? margin = null) =>
        new() { Text = text, FontSize = size, Foreground = Brush(color), FontWeight = weight, Margin = margin ?? new Thickness(0), TextWrapping = TextWrapping.Wrap };
    private static Border Card(string title, string body, string badge)
    {
        var panel = new StackPanel();
        var top = new Grid();
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.Children.Add(Label(title, 18, "#FFFFFF", FontWeights.SemiBold));
        var tag = new Border { Background = Brush("#233558"), CornerRadius = new CornerRadius(9), Padding = new Thickness(10, 5, 10, 5), Child = Label(badge, 11, "#D7E3FF", FontWeights.Bold) };
        Grid.SetColumn(tag, 1); top.Children.Add(tag);
        panel.Children.Add(top);
        panel.Children.Add(Label(body, 13, "#B8C5DD", FontWeights.Normal, new Thickness(0, 8, 0, 0)));
        return new Border { Background = Brush("#151F35"), BorderBrush = Brush("#2D3A56"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(13), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 12), Child = panel };
    }
    private static Brush Brush(string value) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
}

using LifeOS.Core.CareerStudio;
using Microsoft.Maui.Controls.Shapes;

namespace LifeOS.Mobile.Views;

public sealed class CareerPage : ContentPage
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 14, 0, 0, TimeSpan.FromHours(12));
    private readonly CareerStudioProof _proof = CareerProofData.Build(ProofNow);
    private readonly IReadOnlyList<CareerApplication> _applications;
    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(ProofNow);
    private readonly CareerPreparationProof _preparation;
    private readonly VerticalStackLayout _content = new();

    public CareerPage()
    {
        Title = "Career";
        BackgroundColor = Color.FromArgb("#101018");
        _applications = CareerApplicationProofData.Build(_proof, ProofNow);
        _preparation = CareerPreparationProofData.Build(_materials, _proof, ProofNow);
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        var summary = new CareerApplicationService().BuildDashboard(_proof.Opportunities, _applications, ProofNow);
        _content.Children.Clear();
        _content.Padding = new Thickness(14, 14, 14, 30);
        _content.Spacing = 12;
        _content.Children.Add(Heading("Career Studio", 31));
        _content.Children.Add(Text("Review-first opportunities and applications • no autonomous submission or messaging", 14, "#B6B3C8"));
        _content.Children.Add(Card("Career dashboard",
            $"Active opportunities {summary.ActiveOpportunities}\nClosing soon {summary.ClosingSoon}\nApplications {summary.ActiveApplications}\nInterviews {summary.Interviews}\nOverdue follow-ups {summary.OverdueFollowUps}\nWaiting on {summary.WaitingOn}",
            "EXECUTION VIEW"));
        foreach (var opportunity in _proof.Opportunities.Take(3))
            _content.Children.Add(Card(opportunity.Title,
                $"{opportunity.Employer.Name}\n{opportunity.Stage} • {opportunity.WorkMode}\nClosing {(opportunity.ClosingUtc?.ToString("dd MMM") ?? "not supplied")}",
                opportunity.Priority.ToString().ToUpperInvariant()));

        AddAction("Opportunity detail", () => Navigation.PushAsync(new CareerOpportunityPage(_proof.Opportunities[0])));
        AddAction("Application detail", () => Navigation.PushAsync(new CareerApplicationPage(_applications[0])));
        AddAction("Preparation dashboard", () => Navigation.PushAsync(new CareerPreparationPage(_preparation)));
    }

    private void AddAction(string label, Func<Task> action)
    {
        var button = new Button
        {
            Text = label,
            BackgroundColor = Color.FromArgb("#4057D6"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 48
        };
        button.Clicked += async (_, _) => await action();
        _content.Children.Add(button);
    }

    internal static Label Heading(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White
    };

    internal static Label Text(string text, double size = 13, string color = "#D6D3E6") => new()
    {
        Text = text,
        FontSize = size,
        TextColor = Color.FromArgb(color)
    };

    internal static Border Card(string title, string body, string badge)
    {
        var layout = new VerticalStackLayout { Spacing = 7 };
        layout.Children.Add(Heading(title, 17));
        layout.Children.Add(Text(body));
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

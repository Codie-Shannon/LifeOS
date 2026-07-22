using LifeOS.Core.CareerStudio;

namespace LifeOS.Mobile.Views;

public sealed class CareerOpportunityPage : ContentPage
{
    public CareerOpportunityPage(CareerOpportunity opportunity)
    {
        Title = "Opportunity";
        BackgroundColor = Color.FromArgb("#101018");
        var stack = new VerticalStackLayout { Padding = 14, Spacing = 12 };
        stack.Children.Add(CareerPage.Heading(opportunity.Title, 28));
        stack.Children.Add(CareerPage.Text("Explicit capture, stage review, defer and note actions remain local and review-first.", 14, "#B6B3C8"));
        stack.Children.Add(CareerPage.Card("Role and source",
            $"{opportunity.Employer.Name}\n{opportunity.RoleSummary}\n{opportunity.WorkMode} • {opportunity.EmploymentType}\nSource: {opportunity.Source.Reference}",
            opportunity.Stage.ToString().ToUpperInvariant()));
        stack.Children.Add(CareerPage.Card("Fit assessment",
            $"Strengths: {string.Join("; ", opportunity.Fit!.Strengths)}\nGaps: {string.Join("; ", opportunity.Fit.Gaps)}\n{opportunity.Fit.Explanation}",
            "USER-REVIEWED"));
        foreach (var req in opportunity.Requirements)
            stack.Children.Add(CareerPage.Card(req.Type.ToString(), req.Description, req.IsRequired ? "REQUIRED" : "OPTIONAL"));
        var defer = new Button { Text = "Defer review", BackgroundColor = Color.FromArgb("#2C3140"), TextColor = Colors.White, CornerRadius = 12 };
        var note = new Button { Text = "Queue offline note", BackgroundColor = Color.FromArgb("#4057D6"), TextColor = Colors.White, CornerRadius = 12 };
        stack.Children.Add(defer);
        stack.Children.Add(note);
        stack.Children.Add(CareerPage.Text("Offline changes use the existing queue and return through conflict-safe review.", 12, "#A9A5B9"));
        Content = new ScrollView { Content = stack };
    }
}

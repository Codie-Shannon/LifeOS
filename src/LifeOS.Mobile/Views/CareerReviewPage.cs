using LifeOS.Core.CareerStudio;

namespace LifeOS.Mobile.Views;

public sealed class CareerReviewPage : ContentPage
{
    public CareerReviewPage(CareerClosureProof proof)
    {
        Title = "Career review";
        BackgroundColor = Color.FromArgb("#101018");
        var content = new VerticalStackLayout { Padding = new Thickness(14,14,14,30), Spacing = 12 };
        content.Children.Add(CareerPage.Heading("Career Review", 29));
        content.Children.Add(CareerPage.Text("Compact LifeOS-record summary â€¢ no hiring probability", 14, "#B6B3C8"));
        content.Children.Add(CareerPage.Card("Current pipeline", $"Discovered {proof.Review.Pipeline.OpportunitiesDiscovered}\nPrepared {proof.Review.Pipeline.ApplicationsPrepared}\nSubmitted {proof.Review.Pipeline.ApplicationsSubmitted}\nInterviews {proof.Review.Pipeline.Interviews}", "BOUNDED COUNTS"));
        foreach (var follow in proof.FollowUps.Where(x => x.Status is CareerFollowUpStatus.Due or CareerFollowUpStatus.Overdue or CareerFollowUpStatus.Waiting).Take(3))
            content.Children.Add(CareerPage.Card(follow.Title, $"Due {follow.DueUtc:dd MMM}\n{follow.DraftNote}", follow.Status.ToString().ToUpperInvariant()));
        content.Children.Add(CareerPage.Card("Readiness gaps", $"References ready {proof.Review.Coverage.ReadyReferences}/{proof.Review.Coverage.TotalReferences}\nPacks ready {proof.Review.Coverage.ReadyPacks}/{proof.Review.Coverage.TotalPacks}", "REVIEW"));
        content.Children.Add(CareerPage.Card("Recent outcomes", "1 closed outcome recorded\nNo offer probability is inferred", "LIFEOS RECORDS"));
        Content = new ScrollView { Content = content };
    }
}

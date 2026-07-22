using LifeOS.Core.CareerStudio;

namespace LifeOS.Mobile.Views;

public sealed class CareerPreparationPage : ContentPage
{
    public CareerPreparationPage(CareerPreparationProof proof)
    {
        Title = "Preparation";
        BackgroundColor = Color.FromArgb("#101018");
        var content = new VerticalStackLayout { Padding = new Thickness(14,14,14,30), Spacing = 12 };
        content.Children.Add(CareerPage.Heading("Career preparation", 29));
        content.Children.Add(CareerPage.Text("Quick review and offline-safe actions • private material stays hidden", 14, "#B6B3C8"));
        foreach (var card in proof.MobileCards) content.Children.Add(CareerPage.Card(card.Title, card.Summary, card.Badge));
        content.Children.Add(CareerPage.Card("Next interview", $"{proof.Interview.StartsUtc:ddd dd MMM, h:mm tt}\n{proof.Interview.Format}\n{proof.Interview.Checks.Count(x => x.State == PreparationCheckState.Complete)}/{proof.Interview.Checks.Count} checks complete", "PRIVATE-SAFE SUMMARY"));
        foreach (var check in proof.Interview.Checks) content.Children.Add(CareerPage.Card(check.Label, "Tap to update through the existing offline queue/conflict boundary.", check.State.ToString().ToUpperInvariant()));
        Content = new ScrollView { Content = content };
    }
}

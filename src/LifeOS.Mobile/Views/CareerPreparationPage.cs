using LifeOS.Core.CareerStudio;

namespace LifeOS.Mobile.Views;

public sealed class CareerPreparationPage : ContentPage
{
    public CareerPreparationPage(CareerPreparationProof proof, CareerClosureProof closure)
    {
        Title = "Preparation";
        BackgroundColor = Color.FromArgb("#101018");
        var content = new VerticalStackLayout { Padding = new Thickness(14,14,14,30), Spacing = 12 };
        content.Children.Add(CareerPage.Heading("Preparation dashboard", 29));
        content.Children.Add(CareerPage.Text("Dedicated private-safe preparation view â€¢ offline-safe actions", 14, "#B6B3C8"));
        var countdown = Math.Max(0, (int)Math.Ceiling((proof.Interview.StartsUtc - DateTimeOffset.Now).TotalDays));
        content.Children.Add(CareerPage.Card("Interview countdown", $"{countdown} days\n{proof.Interview.StartsUtc:ddd dd MMM, h:mm tt}\n{proof.Interview.Format}", "READ-ONLY CONTEXT"));
        content.Children.Add(CareerPage.Card("Application pack readiness", $"{proof.Pack.Items.Count(x => x.Freshness == MaterialFreshnessState.Current)}/{proof.Pack.Items.Count} materials current\n{proof.Pack.Items.Count(x => x.Freshness == MaterialFreshnessState.Missing)} missing\n{proof.Pack.Items.Count(x => x.Freshness == MaterialFreshnessState.Stale)} stale", proof.Pack.IsReady ? "READY FOR REVIEW" : "GAPS REMAIN"));
        foreach (var question in closure.QuestionsToAsk.Questions.Take(3))
            content.Children.Add(CareerPage.Card("Question to ask", question.Prompt, question.Answered ? "ANSWERED" : "TO ASK"));
        content.Children.Add(CareerPage.Card("Key facts", "C#/.NET evidence backed\nReview-first delivery\nNo fabricated claims", "PRIVATE-SAFE"));
        content.Children.Add(CareerPage.Card("Preparation notes", closure.QuestionsToAsk.Notes, "USER-OWNED"));
        foreach (var check in proof.Interview.Checks)
            content.Children.Add(CareerPage.Card(check.Label, "Tap to update through the existing offline queue/conflict boundary.", check.State.ToString().ToUpperInvariant()));
        Content = new ScrollView { Content = content };
    }
}

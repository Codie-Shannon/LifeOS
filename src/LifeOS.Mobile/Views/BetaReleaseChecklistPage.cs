namespace LifeOS.Mobile.Views;

public sealed class BetaReleaseChecklistPage : ContentPage
{
    public BetaReleaseChecklistPage(
        IReadOnlyList<string> checks)
    {
        Title = "Release checklist";
        BackgroundColor = BetaVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Full Mobile beta checklist",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text =
                        "Review-based beta closure. Evidence must be complete before release.",
                    TextColor = BetaVisuals.Secondary
                }
            }
        };

        foreach (var check in checks)
        {
            content.Children.Add(
                BetaVisuals.CardView(
                    check,
                    "Verified local proof",
                    "Ready"));
        }

        content.Children.Add(
            BetaVisuals.CardView(
                "Release status",
                "Ready for final Pack 2 evidence review",
                "Candidate"));

        content.Children.Add(
            new Label
            {
                Text =
                    "This is a beta candidate, not an automatic production approval.",
                TextColor = BetaVisuals.Secondary
            });

        Content = new ScrollView
        {
            Content = content
        };
    }
}

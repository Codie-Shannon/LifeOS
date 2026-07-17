namespace LifeOS.Mobile.Views;

public sealed class IntegrationInboxPage : ContentPage
{
    public IntegrationInboxPage(int candidateCount)
    {
        Title = "Integration Inbox";
        BackgroundColor = HomeVisuals.Background;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = "Integration Inbox",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    HomeVisuals.CardView(
                        $"{candidateCount} candidates need review",
                        "Provider/source details stay minimized until drill-through."),
                    HomeVisuals.CardView(
                        "Fictional client follow-up",
                        "Gmail candidate • fresh • not yet accepted"),
                    HomeVisuals.CardView(
                        "Tomorrow focus block",
                        "Calendar candidate • fresh • not yet linked"),
                    HomeVisuals.CardView(
                        "Project evidence note",
                        "Drive metadata candidate • fresh • body not downloaded"),
                    new Label
                    {
                        Text =
                            "No candidate silently creates, replaces or " +
                            "reprioritizes a trusted LifeOS record.",
                        TextColor = HomeVisuals.Secondary
                    }
                }
            }
        };
    }
}

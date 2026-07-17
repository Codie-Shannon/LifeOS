using LifeOS.Mobile.Core.Home;

namespace LifeOS.Mobile.Views;

public sealed class AccessibleHomePage : ContentPage
{
    public AccessibleHomePage(HomeDailyOverview overview)
    {
        Title = "Large-text Home";
        BackgroundColor = HomeVisuals.Background;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 18,
                Children =
                {
                    new Label
                    {
                        Text = "Today",
                        FontSize = 42,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = overview.CurrentFocus,
                        FontSize = 24,
                        TextColor = Colors.White,
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    HomeVisuals.CardView(
                        "Priority one",
                        overview.Priorities[0].Title),
                    HomeVisuals.CardView(
                        "Upcoming",
                        $"{overview.Upcoming[0].Title} at " +
                        $"{overview.Upcoming[0].StartsUtc.ToLocalTime():h:mm tt}"),
                    HomeVisuals.CardView(
                        "Needs review",
                        $"{overview.Waiting.NeedsReview} items"),
                    new Label
                    {
                        Text =
                            "Reading order is top to bottom. Controls retain " +
                            "minimum 48dp touch targets.",
                        FontSize = 20,
                        TextColor = HomeVisuals.Secondary
                    }
                }
            }
        };
    }
}

using LifeOS.Mobile.Core.Home;

namespace LifeOS.Mobile.Views;

public sealed class TodayAgendaPage : ContentPage
{
    public TodayAgendaPage(HomeDailyOverview overview)
    {
        Title = "Today agenda";
        BackgroundColor = HomeVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Today agenda",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text =
                        "Compact mobile timeline • times shown in local time",
                    TextColor = HomeVisuals.Secondary
                }
            }
        };

        foreach (var item in overview.Upcoming)
        {
            content.Children.Add(
                HomeVisuals.CardView(
                    item.StartsUtc.ToLocalTime().ToString("h:mm tt"),
                    $"{item.Title} • {item.Kind}"));
        }

        content.Children.Add(
            HomeVisuals.CardView(
                "Review window",
                "Three Integration Inbox candidates require explicit review."));

        Content = new ScrollView
        {
            Content = content
        };
    }
}

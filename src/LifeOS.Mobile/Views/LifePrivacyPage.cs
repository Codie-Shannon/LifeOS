namespace LifeOS.Mobile.Views;

public sealed class LifePrivacyPage : ContentPage
{
    public LifePrivacyPage()
    {
        Title = "Minimized detail";
        BackgroundColor = Colors.Black;

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 18,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Private life details minimized",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text =
                        "Names, addresses, health details and family context stay hidden in mobile proof views.",
                    FontSize = 19,
                    TextColor = LifeProjectVisuals.Secondary,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                LifeProjectVisuals.CardView(
                    "Notifications",
                    "Sensitive content hidden by default"),
                LifeProjectVisuals.CardView(
                    "Recent-app preview",
                    "Private context obscured where supported"),
                LifeProjectVisuals.CardView(
                    "Health boundary",
                    "No diagnosis or treatment recommendation")
            }
        };
    }
}

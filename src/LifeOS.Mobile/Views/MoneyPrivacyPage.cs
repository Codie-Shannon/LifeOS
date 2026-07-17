namespace LifeOS.Mobile.Views;

public sealed class MoneyPrivacyPage : ContentPage
{
    public MoneyPrivacyPage()
    {
        Title = "Privacy mode";
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
                    Text = "Sensitive screen hidden",
                    FontSize = 32,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text =
                        "Amounts, client details and evidence previews are obscured in privacy mode.",
                    FontSize = 20,
                    TextColor = Color.FromArgb("#C7C9D3"),
                    HorizontalTextAlignment = TextAlignment.Center
                },
                MoneyVisuals.CardView(
                    "Recent-app preview",
                    "Hidden where platform support permits"),
                MoneyVisuals.CardView(
                    "Notifications",
                    "Sensitive text hidden by default"),
                new Label
                {
                    Text =
                        "LifeOS does not provide accounting approval or financial advice.",
                    TextColor = MoneyVisuals.Secondary,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };
    }
}

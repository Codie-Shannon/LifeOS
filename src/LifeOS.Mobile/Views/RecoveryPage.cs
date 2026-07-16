namespace LifeOS.Mobile.Views;

public sealed class RecoveryPage : ContentPage
{
    public RecoveryPage(string safeMessage)
    {
        Title = "Recovery"; Content = new VerticalStackLayout { Padding = 22, Spacing = 14, VerticalOptions = LayoutOptions.Center, Children =
        {
            new Label { Text = "LifeOS Full Mobile could not safely open local state", FontSize = 24, FontAttributes = FontAttributes.Bold },
            new Label { Text = safeMessage },
            new Label { Text = "No local data was silently deleted. Capture this redacted message before recovery." }
        }};
    }
}

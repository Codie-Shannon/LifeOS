namespace LifeOS.Companion.Views;

public sealed class RecoveryPage : ContentPage
{
    public RecoveryPage(string message)
    {
        Title = "Local data recovery";
        Content = new ScrollView { Content = new VerticalStackLayout
        {
            Padding = 20, Spacing = 14,
            Children =
            {
                new Label { Text = "LifeOS Companion could not safely open local data", FontSize = 24, FontAttributes = FontAttributes.Bold },
                new Label { Text = message },
                new Label { Text = "No capture data was silently erased. Record this message before taking recovery action." }
            }
        }};
    }
}

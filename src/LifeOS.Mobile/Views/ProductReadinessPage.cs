namespace LifeOS.Mobile.Views;

public sealed class ProductReadinessPage : ContentPage
{
    public ProductReadinessPage()
    {
        Title = "Private beta readiness";
        BackgroundColor = Color.FromArgb("#11131A");

        VerticalStackLayout layout = new()
        {
            Padding = new Thickness(20, 20, 20, 32),
            Spacing = 14
        };
        layout.Children.Add(new Label
        {
            Text = "Private beta setup",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        layout.Children.Add(new Label
        {
            Text = "Core modules work locally. Optional providers can be configured now, later, or declined and changed from Settings.",
            TextColor = Color.FromArgb("#C7C9D3"),
            FontSize = 15
        });

        AddCard(layout, "Local LifeOS data", "Ready", "Core local storage and review queues are available.");
        AddCard(layout, "Google and Microsoft", "Set up later", "Read-only permissions remain optional.");
        AddCard(layout, "External AI", "Declined", "Native LifeOS intelligence remains available without paid AI.");
        AddCard(layout, "Crash reports", "Ask first", "Sanitized reports are sent only after explicit opt-in.");

        layout.Children.Add(new Label
        {
            Text = "Nothing is sent, imported or changed merely because this screen is opened.",
            TextColor = Color.FromArgb("#93C5FD"),
            FontSize = 14
        });

        Content = new ScrollView { Content = layout };
    }

    private static void AddCard(
        VerticalStackLayout layout,
        string title,
        string state,
        string detail)
    {
        layout.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#1C1D27"),
            Stroke = Color.FromArgb("#30313D"),
            StrokeThickness = 1,
            Padding = 16,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label { Text = title, TextColor = Colors.White, FontSize = 17, FontAttributes = FontAttributes.Bold },
                    new Label { Text = state, TextColor = Color.FromArgb("#A78BFA"), FontSize = 14 },
                    new Label { Text = detail, TextColor = Color.FromArgb("#C7C9D3"), FontSize = 13 }
                }
            }
        });
    }
}

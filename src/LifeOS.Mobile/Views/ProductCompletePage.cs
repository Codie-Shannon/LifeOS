namespace LifeOS.Mobile.Views;

public sealed class ProductCompletePage : ContentPage
{
    public ProductCompletePage()
    {
        Title = "Product-complete candidate";
        BackgroundColor = Color.FromArgb("#11131A");

        VerticalStackLayout layout = new()
        {
            Padding = new Thickness(20, 20, 20, 32),
            Spacing = 14
        };
        layout.Children.Add(new Label
        {
            Text = "End-to-end mobile pass",
            TextColor = Colors.White,
            FontSize = 28,
            FontAttributes = FontAttributes.Bold
        });
        layout.Children.Add(new Label
        {
            Text = "Household, Work, Money, communications, settings and offline/sync proof are included in the release-candidate review.",
            TextColor = Color.FromArgb("#C7C9D3"),
            FontSize = 15
        });

        AddCheck(layout, "Household and grocery", "Ready", "Planning and price candidates remain review-first.");
        AddCheck(layout, "Work and time", "Ready", "Billable records retain evidence and export state.");
        AddCheck(layout, "Money", "Ready", "Pay-later deductions require confirmation; no payment initiation.");
        AddCheck(layout, "Communications", "Ready", "Approval, quiet hours and Emergency Stop enforced.");
        AddCheck(layout, "Offline and sync", "Ready", "Local state works without external providers.");
        AddCheck(layout, "Release tag", "Waiting", "Human approval is required after screenshot evidence.");

        Content = new ScrollView { Content = layout };
    }

    private static void AddCheck(VerticalStackLayout layout, string title, string state, string detail)
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

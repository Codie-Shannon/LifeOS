namespace LifeOS.Mobile.Views;

internal static class BetaVisuals
{
    public static readonly Color Background = Color.FromArgb("#11131A");
    public static readonly Color Card = Color.FromArgb("#1B1E28");
    public static readonly Color Stroke = Color.FromArgb("#303441");
    public static readonly Color Secondary = Color.FromArgb("#C7C9D3");
    public static readonly Color Accent = Color.FromArgb("#8B72FF");
    public static readonly Color Success = Color.FromArgb("#8FE3B0");
    public static readonly Color Warning = Color.FromArgb("#FFCA6A");

    public static Label Section(string text) =>
        new()
        {
            Text = text,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Margin = new Thickness(0, 6, 0, 0)
        };

    public static Button Action(string text, Color? background = null) =>
        new()
        {
            Text = text,
            HeightRequest = 48,
            CornerRadius = 14,
            BackgroundColor = background ?? Accent,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };

    public static Border CardView(
        string heading,
        string body,
        string? badge = null)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        grid.Children.Add(
            new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label
                    {
                        Text = heading,
                        FontSize = 17,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = body,
                        FontSize = 14,
                        TextColor = Secondary,
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            });

        if (!string.IsNullOrWhiteSpace(badge))
        {
            var badgeLabel = new Label
            {
                Text = badge,
                TextColor = Accent,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(badgeLabel, 1);
            grid.Children.Add(badgeLabel);
        }

        return new Border
        {
            Padding = 15,
            BackgroundColor = Card,
            Stroke = Stroke,
            StrokeThickness = 1,
            StrokeShape =
                new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 16
                },
            Content = grid
        };
    }
}

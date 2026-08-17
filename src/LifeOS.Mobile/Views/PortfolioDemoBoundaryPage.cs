namespace LifeOS.Mobile.Views;

public sealed class PortfolioDemoBoundaryPage : ContentPage
{
    public PortfolioDemoBoundaryPage(string workspace)
    {
        Title = workspace;
        BackgroundColor = Color.FromArgb("#11131A");
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 34),
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = workspace,
                        FontSize = 34,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Border
                    {
                        Padding = 20,
                        BackgroundColor = Color.FromArgb("#1C1D27"),
                        Stroke = Color.FromArgb("#343746"),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                        {
                            CornerRadius = 18
                        },
                        Content = new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label
                                {
                                    Text = "Nothing here yet",
                                    FontSize = 22,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Colors.White
                                },
                                new Label
                                {
                                    Text = "This workspace has no local records. Portfolio examples stay hidden in ordinary mode and can be enabled explicitly in Settings.",
                                    FontSize = 16,
                                    TextColor = Color.FromArgb("#C7C9D3")
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}

using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Views;

public sealed class SettingsPage : ContentPage
{
    public SettingsPage(MobileFoundationService foundation)
    {
        Title = "Settings";
        BackgroundColor = Color.FromArgb("#11131A");

        var theme = CreatePicker(
            new[] { "System", "Light", "Dark", "High contrast" },
            selectedIndex: 0);

        var accent = CreatePicker(
            new[] { "Violet", "Teal", "Blue", "Amber" },
            selectedIndex: 0);

        var density = CreatePicker(
            new[] { "Comfortable", "Compact" },
            selectedIndex: 0);

        var clearStop = new Button
        {
            Text = "Clear Emergency Stop",
            HeightRequest = 52,
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#304860"),
            TextColor = Colors.White
        };

        clearStop.Clicked += async (_, _) =>
        {
            await foundation.ClearEmergencyStopAsync();

            await DisplayAlertAsync(
                "Emergency Stop",
                "Network and sync may resume only after explicit user action.",
                "OK");
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20, 20, 20, 30),
                Spacing = 18,
                Children =
                {
                    new Label
                    {
                        Text = "Appearance and safety",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },

                    CreateSettingBlock("Theme", theme),
                    CreateSettingBlock("Accent", accent),
                    CreateSettingBlock("Density", density),

                    CreateToggleRow(
                        "Sensitive previews hidden",
                        "Hide private content from screenshots, notifications and recent-app previews.",
                        true),

                    CreateToggleRow(
                        "Require app lock",
                        "Require local authentication before opening LifeOS Full Mobile.",
                        true),

                    clearStop,

                    new Label
                    {
                        Text = "Touch targets are at least 48dp. Text scaling and screen-reader labels are supported.",
                        TextColor = Color.FromArgb("#C7C9D3"),
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            }
        };
    }

    private static Picker CreatePicker(
        IEnumerable<string> items,
        int selectedIndex)
    {
        return new Picker
        {
            ItemsSource = items.ToArray(),
            SelectedIndex = selectedIndex,
            TextColor = Colors.White,
            TitleColor = Color.FromArgb("#C7C9D3"),
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            HeightRequest = 52
        };
    }

    private static View CreateSettingBlock(
        string label,
        Picker picker)
    {
        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = label,
                    TextColor = Colors.White,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold
                },

                new Border
                {
                    BackgroundColor = Color.FromArgb("#1C1D27"),
                    Stroke = Color.FromArgb("#30313D"),
                    StrokeThickness = 1,
                    Padding = new Thickness(14, 0),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    {
                        CornerRadius = 14
                    },
                    Content = picker
                }
            }
        };
    }

    private static View CreateToggleRow(
        string title,
        string description,
        bool isToggled)
    {
        var textBlock = new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label
                {
                    Text = title,
                    TextColor = Colors.White,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold
                },
                new Label
                {
                    Text = description,
                    TextColor = Color.FromArgb("#C7C9D3"),
                    FontSize = 13,
                    LineBreakMode = LineBreakMode.WordWrap
                }
            }
        };

        var toggle = new Switch
        {
            IsToggled = isToggled,
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(toggle, 1);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        grid.Children.Add(textBlock);
        grid.Children.Add(toggle);

        return new Border
        {
            BackgroundColor = Color.FromArgb("#1C1D27"),
            Stroke = Color.FromArgb("#30313D"),
            StrokeThickness = 1,
            Padding = 16,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 14
            },
            Content = grid
        };
    }
}

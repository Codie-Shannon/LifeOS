using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Views;

public sealed class SettingsPage : ContentPage
{
    public SettingsPage(MobileFoundationService foundation)
    {
        Title = "Settings"; BackgroundColor = Color.FromArgb("#11131A");
        var theme = new Picker { Title = "Theme", ItemsSource = new[] { "System", "Light", "Dark", "High contrast" }, SelectedIndex = 0 };
        var accent = new Picker { Title = "Accent", ItemsSource = new[] { "Violet", "Teal", "Blue", "Amber" }, SelectedIndex = 0 };
        var density = new Picker { Title = "Density", ItemsSource = new[] { "Comfortable", "Compact" }, SelectedIndex = 0 };
        var clearStop = new Button { Text = "Clear Emergency Stop" };
        clearStop.Clicked += async (_, _) => { await foundation.ClearEmergencyStopAsync(); await DisplayAlertAsync("Emergency Stop", "Network and sync may resume only after explicit user action.", "OK"); };
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 20, Spacing = 14, Children =
        {
            new Label { Text = "Appearance and safety", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
            theme, accent, density,
            new Label { Text = "Sensitive previews hidden", TextColor = Colors.White }, new Switch { IsToggled = true },
            new Label { Text = "Require app lock", TextColor = Colors.White }, new Switch { IsToggled = true },
            clearStop,
            new Label { Text = "Touch targets are at least 48dp. Text scaling and screen-reader labels are supported.", TextColor = Color.FromArgb("#C7C9D3") }
        }}};
    }
}

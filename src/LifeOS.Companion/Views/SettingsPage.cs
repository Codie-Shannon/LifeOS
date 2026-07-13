using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Views;

public sealed class SettingsPage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly Entry _deviceLabel = new() { MaxLength = 80 };
    public SettingsPage(ICompanionStore store)
    {
        _store = store;
        Title = "Settings";
        var save = new Button { Text = "Save device label" };
        save.Clicked += async (_, _) =>
        {
            try
            {
                await _store.UpdateDeviceLabelAsync(_deviceLabel.Text ?? string.Empty);
                await DisplayAlert("Saved", "Device label updated locally.", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Not saved", ex.Message, "OK"); }
        };
        Content = new VerticalStackLayout
        {
            Padding = 16, Spacing = 12,
            Children =
            {
                new Label { Text = "Settings", FontSize = 24, FontAttributes = FontAttributes.Bold },
                new Label { Text = "Human-readable device label" }, _deviceLabel, save,
                new Label { Text = "System theme is used. No analytics, OAuth, cloud login, background sync, or automatic retry." }
            }
        };
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _deviceLabel.Text = (await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion")).DeviceLabel;
    }
}

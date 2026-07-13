using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Views;

public sealed class DeviceStatusPage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly Label _label = new();
    public DeviceStatusPage(ICompanionStore store)
    {
        _store = store;
        Title = "Device";
        Content = new ScrollView { Content = new VerticalStackLayout
        {
            Padding = 16, Spacing = 12,
            Children =
            {
                new Label { Text = "Device / Status", FontSize = 24, FontAttributes = FontAttributes.Bold },
                _label,
                new Label { Text = "Connection: NotPaired" },
                new Label { Text = "Cloud account: None" },
                new Label { Text = "Desktop transfer: Not enabled in Group 32" },
                new Label { Text = "Local storage: SQLite with AES-GCM protected capture fields" },
                new Label { Text = "Key protection: Android SecureStorage" }
            }
        }};
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var device = await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        _label.Text = $"Label: {device.DeviceLabel}\nLocal ID: {device.DeviceId[..8]}…\nSchema: {device.SchemaVersion}\nVersion: v0.1.0-alpha.1";
    }
}

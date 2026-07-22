using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
using LifeOS.Companion.Services;

namespace LifeOS.Companion.Views;

public sealed class SettingsPage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly PairingCredentialStore _credentials;
    private readonly CompanionNotificationService _notifications;

    private readonly Entry _deviceLabel = new() { MaxLength = 80 };
    private readonly Switch _enabled = new();
    private readonly Switch _privacy = new() { IsToggled = true };
    private readonly Label _summary = new();

    public SettingsPage(
        ICompanionStore store,
        PairingCredentialStore credentials,
        CompanionNotificationService notifications)
    {
        _store = store;
        _credentials = credentials;
        _notifications = notifications;
        Title = "Settings";

        var save = new Button { Text = "Save settings" };
        save.Clicked += async (_, _) =>
        {
            await _store.UpdateDeviceLabelAsync(_deviceLabel.Text ?? string.Empty);

            if (_enabled.IsToggled && !await _notifications.RequestPermissionAsync())
            {
                _enabled.IsToggled = false;
                await DisplayAlertAsync(
                    "Permission denied",
                    "Notifications remain disabled. Transfers and sync were not started.",
                    "OK");
            }

            await _store.SetStateValueAsync(
                "notification_enabled",
                _enabled.IsToggled.ToString().ToLowerInvariant());
            await _store.SetStateValueAsync(
                "notification_private",
                _privacy.IsToggled.ToString().ToLowerInvariant());
        };

        var test = new Button { Text = "Show privacy-safe test notification" };
        test.Clicked += async (_, _) =>
        {
            if (_enabled.IsToggled)
            {
                await _notifications.ShowAsync(
                    NotificationCategory.ReviewAvailable,
                    _privacy.IsToggled);
            }
        };

        var clear = new Button { Text = "Clear glance cache only" };
        clear.Clicked += async (_, _) =>
        {
            await _store.ClearGlanceCacheAsync();
            await RefreshAsync();
        };

        var reset = new Button { Text = "Reset pairing (keep Pending captures)" };
        reset.Clicked += async (_, _) =>
        {
            await _credentials.ResetAsync();
            await RefreshAsync();
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Privacy, storage and accessibility",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label { Text = "Device label" },
                    _deviceLabel,
                    new Label { Text = "Notifications (disabled until explicit opt-in)" },
                    _enabled,
                    new Label { Text = "Privacy-safe lock-screen text" },
                    _privacy,
                    save,
                    test,
                    _summary,
                    clear,
                    reset,
                    new Label
                    {
                        Text = "Pairing reset, glance-cache clear and local capture deletion are separate actions. No cloud account or background sync."
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _deviceLabel.Text = (await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion")).DeviceLabel;
        _enabled.IsToggled = await _store.GetStateValueAsync("notification_enabled") == "true";
        _privacy.IsToggled = await _store.GetStateValueAsync("notification_private") != "false";

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        var summary = await _store.GetLocalDataSummaryAsync(
            await _credentials.LoadAsync() is not null);

        _summary.Text = $"Local data: {summary.Captures} captures | {summary.Pending} Pending | {summary.CachedGlanceSnapshots} glance caches | paired: {summary.IsPaired}";
    }
}

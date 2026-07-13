using LifeOS.Companion.Core;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;

namespace LifeOS.Companion.Views;

public sealed class BetaReadinessPage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly PairingCredentialStore _credentials;
    private readonly Label _body = new() { FontSize = 16 };

    public BetaReadinessPage(ICompanionStore store, PairingCredentialStore credentials)
    {
        _store = store;
        _credentials = credentials;
        Title = "Beta";
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
                        Text = "Companion beta readiness",
                        FontSize = 26,
                        FontAttributes = FontAttributes.Bold
                    },
                    _body
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var credential = await _credentials.LoadAsync();
        var device = await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        var summary = await _store.GetLocalDataSummaryAsync(credential is not null);
        var glance = await _store.GetGlanceAsync();
        var notificationsEnabled = await _store.GetStateValueAsync("notification_enabled");
        var lastGlanceUpdate = glance.Count == 0
            ? "Never"
            : glance.Max(x => x.UpdatedAtUtc).LocalDateTime.ToString("g");

        _body.Text = string.Join(Environment.NewLine, new[]
        {
            $"Version: {ProductIdentity.Version}",
            $"Device: {device.DeviceLabel}",
            $"Pairing: {(credential is null ? "Not paired" : "Paired")}; last Desktop acknowledgement is recorded only after verified success",
            $"Pending: {summary.Pending} | Failed: {summary.Failed} | Delivered: {summary.Delivered}",
            $"Last glance update: {lastGlanceUpdate}",
            $"Notifications: {(notificationsEnabled == "true" ? "Opted in" : "Disabled by default")} | private lock-screen wording",
            $"Conflicts: {summary.Conflicts}",
            $"Local schema: {device.SchemaVersion} | SQLite + AES-GCM protected fields + SecureStorage credentials",
            "Cloud account: NO",
            "Background sync/sender/retry: NO",
            "Status does not rely on colour alone."
        });
    }
}

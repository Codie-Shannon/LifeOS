using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class HomePage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly Label _sync = new();

    public HomePage(MobileFoundationService foundation)
    {
        _foundation = foundation;

        Title = "Home";
        BackgroundColor = Color.FromArgb("#11131A");

        var stop = new Button
        {
            Text = "Emergency Stop",
            HeightRequest = 52,
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#C52A20"),
            TextColor = Colors.White
        };

        SemanticProperties.SetDescription(
            stop,
            "Immediately stop network and synchronization activity");

        stop.Clicked += async (_, _) =>
        {
            await _foundation.ActivateEmergencyStopAsync();
            await RefreshAsync();
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = "LifeOS",
                        FontSize = 34,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },

                    new Label
                    {
                        Text = "Full Mobile foundation",
                        FontSize = 18,
                        TextColor = Color.FromArgb("#C7C9D3")
                    },

                    Card(
                        "Ready",
                        "Local-first demo mode • fictional data only"),

                    Card(
                        "Eight workspaces",
                        string.Join(" • ", MobileWorkspaceCatalog.Permanent)),

                    Card(
                        "Freshness",
                        "Last successful sync 4 minutes ago"),

                    _sync,
                    stop,

                    new Label
                    {
                        Text = "No provider writes. No silent merges. Pending work remains reviewable.",
                        TextColor = Color.FromArgb("#C7C9D3")
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

        await RefreshAsync();
    }

    protected override void OnDisappearing()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        base.OnDisappearing();
    }

    private void OnConnectivityChanged(
        object? sender,
        ConnectivityChangedEventArgs e)
    {
        Dispatcher.Dispatch(async () => await RefreshAsync());
    }

    private async Task RefreshAsync()
    {
        var diagnostics = await _foundation.GetSafeDiagnosticsAsync();

        var storedState = diagnostics
            .First(x => x.Name == "Sync state")
            .Value;

        var pendingQueue = diagnostics
            .First(x => x.Name == "Pending queue")
            .Value;

        var hasInternet =
            Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        var effectiveState = storedState.Equals(
            MobileSyncState.Stopped.ToString(),
            StringComparison.OrdinalIgnoreCase)
                ? MobileSyncState.Stopped.ToString()
                : hasInternet
                    ? storedState
                    : MobileSyncState.Offline.ToString();

        _sync.Text =
            $"Sync state: {effectiveState}  •  Pending queue: {pendingQueue}";

        _sync.TextColor = effectiveState == MobileSyncState.Offline.ToString()
            ? Color.FromArgb("#FFCA6A")
            : Colors.White;

        _sync.FontAttributes = FontAttributes.Bold;
    }

    private static Border Card(string heading, string body)
    {
        return new Border
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#1B1E28"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label
                    {
                        Text = heading,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = body,
                        TextColor = Color.FromArgb("#C7C9D3")
                    }
                }
            }
        };
    }
}

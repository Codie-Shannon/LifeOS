using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Home;
using LifeOS.Mobile.Core.Services;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class HomePage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly HomeDailyService _home = new();
    private readonly Label _sync = new();
    private readonly Label _queue = new();
    private readonly VerticalStackLayout _content = new();

    public HomePage(MobileFoundationService foundation)
    {
        _foundation = foundation;

        Title = "Home";
        BackgroundColor = HomeVisuals.Background;

        Content = new ScrollView
        {
            Content = _content
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Connectivity.Current.ConnectivityChanged -=
            OnConnectivityChanged;

        Connectivity.Current.ConnectivityChanged +=
            OnConnectivityChanged;

        await RenderAsync();
    }

    protected override void OnDisappearing()
    {
        Connectivity.Current.ConnectivityChanged -=
            OnConnectivityChanged;

        base.OnDisappearing();
    }

    private void OnConnectivityChanged(
        object? sender,
        ConnectivityChangedEventArgs args)
    {
        Dispatcher.Dispatch(
            async () => await RefreshSyncAsync());
    }

    private async Task RenderAsync()
    {
        var now = DateTimeOffset.Now;
        var overview = _home.BuildOverview(now);

        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;

        _content.Children.Add(
            new Label
            {
                Text = "Today",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text = now.ToString("dddd, d MMMM"),
                FontSize = 16,
                TextColor = HomeVisuals.Secondary
            });

        _content.Children.Add(
            HomeVisuals.CardView(
                "Current focus",
                overview.CurrentFocus));

        var capture = HomeVisuals.Action("Quick capture");
        SemanticProperties.SetDescription(
            capture,
            "Create a local draft before explicitly queueing it");

        capture.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new QuickCapturePage(_foundation));

        var agenda = HomeVisuals.Action(
            "Open agenda",
            Color.FromArgb("#2C3140"));

        agenda.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new TodayAgendaPage(overview));

        var actions = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };

        actions.Children.Add(capture);
        Grid.SetColumn(agenda, 1);
        actions.Children.Add(agenda);
        _content.Children.Add(actions);

        _content.Children.Add(HomeVisuals.Section("Top priorities"));

        foreach (var priority in overview.Priorities.Take(4))
        {
            var badge = new Label
            {
                Text = priority.Workspace,
                TextColor = HomeVisuals.Accent,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };

            _content.Children.Add(
                HomeVisuals.CardView(
                    priority.Title,
                    $"{priority.Explanation} • Rank {priority.Rank}",
                    badge));
        }

        _content.Children.Add(HomeVisuals.Section("Upcoming"));

        foreach (var item in overview.Upcoming)
        {
            _content.Children.Add(
                HomeVisuals.CardView(
                    item.Title,
                    $"{item.Kind} • {item.StartsUtc.ToLocalTime():h:mm tt}"));
        }

        _content.Children.Add(HomeVisuals.Section("Needs attention"));

        _content.Children.Add(
            HomeVisuals.CardView(
                "Waiting, blocked and overdue",
                $"Waiting on {overview.Waiting.WaitingOn} • " +
                $"Blocked {overview.Waiting.Blocked} • " +
                $"Overdue {overview.Waiting.Overdue} • " +
                $"Needs review {overview.Waiting.NeedsReview}"));

        var inbox = HomeVisuals.Action(
            $"Integration Inbox ({overview.Review.IntegrationInboxCount})",
            Color.FromArgb("#2C3140"));

        inbox.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new IntegrationInboxPage(
                    overview.Review.IntegrationInboxCount));

        _content.Children.Add(inbox);

        _content.Children.Add(HomeVisuals.Section("Freshness and sync"));

        _sync.TextColor = Colors.White;
        _sync.FontAttributes = FontAttributes.Bold;
        _queue.TextColor = HomeVisuals.Secondary;

        _content.Children.Add(
            HomeVisuals.CardView(
                "Trusted status",
                "Imported provider candidates remain review-first."));

        _content.Children.Add(_sync);
        _content.Children.Add(_queue);

        var accessible = HomeVisuals.Action(
            "Open large-text proof",
            Color.FromArgb("#2C3140"));

        accessible.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new AccessibleHomePage(overview));

        _content.Children.Add(accessible);

        var stop = HomeVisuals.Action(
            "Emergency Stop",
            Color.FromArgb("#C52A20"));

        SemanticProperties.SetDescription(
            stop,
            "Immediately stop network and synchronization activity");

        stop.Clicked += async (_, _) =>
        {
            await _foundation.ActivateEmergencyStopAsync();
            await RefreshSyncAsync();
        };

        _content.Children.Add(stop);

        _content.Children.Add(
            new Label
            {
                Text =
                    "Fictional proof data only • no provider writes • " +
                    "no silent task generation or reprioritization",
                TextColor = HomeVisuals.Secondary,
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center
            });

        await RefreshSyncAsync();
    }

    private async Task RefreshSyncAsync()
    {
        var sync = await _foundation.GetSyncSnapshotAsync();
        var hasInternet =
            Connectivity.Current.NetworkAccess ==
            NetworkAccess.Internet;

        var state = sync.State == MobileSyncState.Stopped
            ? MobileSyncState.Stopped
            : hasInternet
                ? sync.State
                : MobileSyncState.Offline;

        _sync.Text = $"Sync state: {state}";
        _sync.TextColor =
            state is MobileSyncState.Offline or MobileSyncState.Stopped
                ? HomeVisuals.Warning
                : Colors.White;

        _queue.Text =
            $"Pending queue: {sync.PendingCount} • " +
            $"Last successful sync: " +
            $"{sync.LastSuccessfulSyncUtc?.ToLocalTime():h:mm tt}";
    }
}

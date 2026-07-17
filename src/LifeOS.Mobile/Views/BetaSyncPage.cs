using LifeOS.Mobile.Core.Beta;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class BetaSyncPage : ContentPage
{
    private readonly BetaClosureService _service;
    private SyncStatus _status;
    private readonly Label _state;

    public BetaSyncPage(
        BetaClosureService service,
        SyncStatus status)
    {
        _service = service;
        _status = status;

        Title = "Sync";
        BackgroundColor = BetaVisuals.Background;

        _state = new Label
        {
            Text = BuildStateText(),
            TextColor = BetaVisuals.Warning,
            FontAttributes = FontAttributes.Bold
        };

        var queue = BetaVisuals.Action("Queue offline update");
        queue.Clicked += (_, _) =>
        {
            _status = _service.QueueOffline(_status);
            _state.Text = BuildStateText();
            _state.TextColor = BetaVisuals.Success;
        };

        var stop = BetaVisuals.Action(
            "Emergency stop sync",
            Color.FromArgb("#C72B20"));

        stop.Clicked += (_, _) =>
        {
            _status = _service.StopSync(_status);
            _state.Text = BuildStateText();
            _state.TextColor = BetaVisuals.Warning;
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = "Sync and offline safety",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    _state,
                    BetaVisuals.CardView(
                        "Connectivity",
                        Connectivity.Current.NetworkAccess.ToString()),
                    BetaVisuals.CardView(
                        "Freshness",
                        _status.Freshness),
                    BetaVisuals.CardView(
                        "Provider writes",
                        _status.ProviderWritesEnabled
                            ? "Enabled"
                            : "Disabled"),
                    queue,
                    stop,
                    new Label
                    {
                        Text =
                            "Pending work remains reviewable. No silent merges or external writes.",
                        TextColor = BetaVisuals.Secondary
                    }
                }
            }
        };
    }

    private string BuildStateText() =>
        $"Sync state: {_status.Health} • Pending queue: {_status.PendingQueue}";
}

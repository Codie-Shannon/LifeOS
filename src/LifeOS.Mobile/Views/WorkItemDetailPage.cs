using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Work;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class WorkItemDetailPage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly WorkService _service;
    private MobileWorkItem _item;
    private readonly Label _status;

    public WorkItemDetailPage(
        MobileFoundationService foundation,
        WorkService service,
        MobileWorkItem item,
        WorkDashboardSnapshot snapshot)
    {
        _foundation = foundation;
        _service = service;
        _item = item;

        Title = "Work item";
        BackgroundColor = WorkVisuals.Background;

        _status = new Label
        {
            Text = $"Status: {_item.Status}",
            TextColor = WorkVisuals.Warning,
            FontAttributes = FontAttributes.Bold
        };

        var followUp = WorkVisuals.Action("Capture follow-up");

        followUp.Clicked += async (_, _) =>
        {
            _item = _service.CaptureFollowUp(
                _item,
                "Wait for explicit fictional client review");

            _status.Text =
                "Status: Waiting • follow-up captured locally";

            if (Connectivity.Current.NetworkAccess !=
                NetworkAccess.Internet)
            {
                _status.Text += " • queued safely offline";
            }

            await DisplayAlertAsync(
                "Follow-up captured",
                "The update remains local and reviewable before sync.",
                "OK");
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
                        Text = _item.Client,
                        FontSize = 16,
                        TextColor = WorkVisuals.Accent
                    },
                    new Label
                    {
                        Text = _item.Title,
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    _status,
                    WorkVisuals.CardView(
                        "Next action",
                        _item.NextAction),
                    WorkVisuals.CardView(
                        "People",
                        string.Join(" • ", _item.People)),
                    WorkVisuals.CardView(
                        "Files",
                        _item.LinkedFiles.Count == 0
                            ? "No linked file metadata"
                            : string.Join(" • ", _item.LinkedFiles)),
                    WorkVisuals.CardView(
                        "Evidence",
                        $"{_item.EvidenceComplete} of " +
                        $"{_item.EvidenceTotal} complete"),
                    WorkVisuals.CardView(
                        "Due",
                        _item.DueUtc.ToLocalTime()
                            .ToString("ddd d MMM, h:mm tt")),
                    followUp,
                    new Label
                    {
                        Text =
                            "Imported communication cannot overwrite this " +
                            "trusted work item silently.",
                        TextColor = WorkVisuals.Secondary
                    }
                }
            }
        };
    }
}

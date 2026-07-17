using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Work;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class WorkPage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly WorkService _service = new();
    private readonly VerticalStackLayout _content = new();

    public WorkPage(MobileFoundationService foundation)
    {
        _foundation = foundation;
        Title = "Work";
        BackgroundColor = WorkVisuals.Background;

        Content = new ScrollView
        {
            Content = _content
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        var snapshot = _service.BuildSnapshot(DateTimeOffset.Now);

        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;

        _content.Children.Add(
            new Label
            {
                Text = "Work",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Active jobs, communication review, meetings, " +
                    "follow-ups and evidence",
                FontSize = 15,
                TextColor = WorkVisuals.Secondary
            });

        var summary = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        AddSummary(
            summary,
            0,
            0,
            "Active",
            snapshot.Items.Count(x => x.Status == MobileWorkStatus.Active));

        AddSummary(
            summary,
            1,
            0,
            "Waiting",
            snapshot.Items.Count(x => x.Status == MobileWorkStatus.Waiting));

        AddSummary(
            summary,
            0,
            1,
            "Blocked",
            snapshot.Items.Count(x => x.Status == MobileWorkStatus.Blocked));

        AddSummary(
            summary,
            1,
            1,
            "Due soon",
            snapshot.Items.Count(x => x.Status == MobileWorkStatus.DueSoon));

        _content.Children.Add(summary);

        _content.Children.Add(WorkVisuals.Section("Active work"));

        foreach (var item in snapshot.Items)
        {
            var button = WorkVisuals.Action(
                $"{item.Client} • {item.Status}",
                Color.FromArgb("#2C3140"));

            var captured = item;
            button.Clicked += async (_, _) =>
                await Navigation.PushAsync(
                    new WorkItemDetailPage(
                        _foundation,
                        _service,
                        captured,
                        snapshot));

            _content.Children.Add(
                WorkVisuals.CardView(
                    item.Title,
                    $"Next: {item.NextAction} • " +
                    $"Evidence {item.EvidenceComplete}/{item.EvidenceTotal}",
                    item.Status.ToString()));

            _content.Children.Add(button);
        }

        _content.Children.Add(
            WorkVisuals.Section("Communication review"));

        var communications = WorkVisuals.Action(
            $"Review communications ({snapshot.Communications.Count})");

        communications.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new CommunicationReviewPage(_service, snapshot));

        _content.Children.Add(communications);

        var meeting = WorkVisuals.Action(
            "Open meeting context",
            Color.FromArgb("#2C3140"));

        meeting.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new MeetingContextPage(snapshot.Meetings[0]));

        _content.Children.Add(meeting);

        var evidence = WorkVisuals.Action(
            "Open evidence checklist",
            Color.FromArgb("#2C3140"));

        evidence.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new WorkEvidencePage(snapshot.Evidence));

        _content.Children.Add(evidence);

        var networkState =
            Connectivity.Current.NetworkAccess ==
            NetworkAccess.Internet
                ? "Online"
                : "Offline";

        _content.Children.Add(
            WorkVisuals.CardView(
                "Safety boundary",
                $"{networkState} • local drafts and queued updates only • " +
                "no email send, Teams post or provider-side mutation"));
    }

    private static void AddSummary(
        Grid grid,
        int column,
        int row,
        string label,
        int count)
    {
        var card = WorkVisuals.CardView(
            label,
            count.ToString());

        Grid.SetColumn(card, column);
        Grid.SetRow(card, row);
        grid.Children.Add(card);
    }
}

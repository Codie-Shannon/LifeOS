using LifeOS.Mobile.Core.Work;

namespace LifeOS.Mobile.Views;

public sealed class CommunicationReviewPage : ContentPage
{
    private readonly WorkService _service;
    private readonly WorkDashboardSnapshot _snapshot;
    private readonly VerticalStackLayout _content = new();

    public CommunicationReviewPage(
        WorkService service,
        WorkDashboardSnapshot snapshot)
    {
        _service = service;
        _snapshot = snapshot;

        Title = "Communication review";
        BackgroundColor = WorkVisuals.Background;

        Content = new ScrollView
        {
            Content = _content
        };

        Render();
    }

    private void Render()
    {
        _content.Children.Clear();
        _content.Padding = 20;
        _content.Spacing = 14;

        _content.Children.Add(
            new Label
            {
                Text = "Communication review",
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Normalized candidates only • raw provider payloads " +
                    "are never displayed",
                TextColor = WorkVisuals.Secondary
            });

        foreach (var candidate in _snapshot.Communications)
        {
            var status = new Label
            {
                Text = $"Review state: {candidate.ReviewState}",
                TextColor =
                    candidate.ReviewState ==
                    CommunicationReviewState.Conflict
                        ? WorkVisuals.Warning
                        : WorkVisuals.Secondary
            };

            var detail = WorkVisuals.Action(
                "Provenance and freshness",
                Color.FromArgb("#2C3140"));

            var captured = candidate;

            detail.Clicked += async (_, _) =>
                await Navigation.PushAsync(
                    new CommunicationProvenancePage(captured));

            var accept = WorkVisuals.Action("Accept");
            accept.Clicked += (_, _) =>
            {
                var result = _service.ReviewCommunication(
                    captured.Id,
                    CommunicationReviewState.Accepted,
                    DateTimeOffset.UtcNow);

                status.Text = $"Review state: {result.State}";
                status.TextColor = Color.FromArgb("#8FE3B0");
            };

            var defer = WorkVisuals.Action(
                "Defer",
                Color.FromArgb("#2C3140"));

            defer.Clicked += (_, _) =>
            {
                var result = _service.ReviewCommunication(
                    captured.Id,
                    CommunicationReviewState.Deferred,
                    DateTimeOffset.UtcNow);

                status.Text = $"Review state: {result.State}";
                status.TextColor = WorkVisuals.Warning;
            };

            _content.Children.Add(
                WorkVisuals.CardView(
                    candidate.Subject,
                    $"{candidate.Provider} • {candidate.SourceKind} • " +
                    candidate.SafePreview,
                    candidate.Freshness));

            _content.Children.Add(status);
            _content.Children.Add(detail);

            var actions = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 10
            };

            actions.Children.Add(accept);
            Grid.SetColumn(defer, 1);
            actions.Children.Add(defer);
            _content.Children.Add(actions);
        }

        _content.Children.Add(
            new Label
            {
                Text =
                    "No email send, Teams posting or external mutation " +
                    "is available.",
                TextColor = WorkVisuals.Secondary
            });
    }
}

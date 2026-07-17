using LifeOS.Mobile.Core.Work;

namespace LifeOS.Mobile.Views;

public sealed class CommunicationProvenancePage : ContentPage
{
    public CommunicationProvenancePage(
        CommunicationCandidate candidate)
    {
        Title = "Provenance";
        BackgroundColor = WorkVisuals.Background;

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
                        Text = "Provenance and freshness",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    WorkVisuals.CardView(
                        "Provider",
                        candidate.Provider),
                    WorkVisuals.CardView(
                        "Source kind",
                        candidate.SourceKind),
                    WorkVisuals.CardView(
                        "Freshness",
                        candidate.Freshness),
                    WorkVisuals.CardView(
                        "Received",
                        candidate.ReceivedUtc.ToLocalTime()
                            .ToString("ddd d MMM, h:mm tt")),
                    WorkVisuals.CardView(
                        "Review state",
                        candidate.ReviewState.ToString()),
                    WorkVisuals.CardView(
                        "Source boundary",
                        candidate.Provenance),
                    new Label
                    {
                        Text =
                            "Raw message bodies, provider identifiers and " +
                            "private payloads are excluded.",
                        TextColor = WorkVisuals.Secondary
                    }
                }
            }
        };
    }
}
